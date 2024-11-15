﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.RoomsToMass.ToMass
{
    /// <summary>
    /// Interaction logic for RoomWindow.xaml
    /// </summary>
    public partial class RoomWindow : Window
    {
        private UIApplication m_app = null;
        private Document m_doc= null;
        private RevitDocumentProperties selectedModel = null;
        private SpatialElementBoundaryLocation selectedRoomBoundary = SpatialElementBoundaryLocation.Center;
        private Dictionary<string, RevitDocumentProperties> modelDictionary = new Dictionary<string, RevitDocumentProperties>();
        private Dictionary<string/*roomUniqueId*/, RoomProperties> roomDictionary = new Dictionary<string, RoomProperties>();
        private MassConfiguration massConfig = null;

        public Dictionary<string, RoomProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public RoomWindow(UIApplication uiapp, Dictionary<string, RevitDocumentProperties> models ,Dictionary<string, RoomProperties> dictionary)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            modelDictionary = models;
            roomDictionary = dictionary;

            InitializeComponent();

            this.Title = "Create Mass v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            List<RevitDocumentProperties> documents = modelDictionary.Values.OrderBy(o => o.DocumentTitle).ToList();
            comboBoxModel.ItemsSource = documents;
            //area elements only in host doc
            int index = documents.FindIndex(o => o.IsLinked == false);
            if (index > -1)
            {
                comboBoxModel.SelectedIndex = index;
            }

            massConfig = MassConfigDataStorageUtil.GetMassConfiguration(m_doc, SourceType.Rooms);
            massConfig.MassSourceType = SourceType.Rooms;
            massConfig.HostCategory = "Rooms";
            massConfig.MassCategory = "Mass";
            if (massConfig.UpdateType == ParameterUpdateType.FromHostToMass)
            {
                expanderParameter.Header = "Linked Parameters - (Rooms to Masses)";
            }
            else if (massConfig.UpdateType == ParameterUpdateType.FromMassToHost)
            {
                expanderParameter.Header = "Linked Parameters - (Masses to Rooms)";
            }
            dataGridParameters.ItemsSource = null;
            dataGridParameters.ItemsSource = massConfig.MassParameters;

            List<KeyValuePair<string, SpatialElementBoundaryLocation>> boundaryLocations = new List<KeyValuePair<string, SpatialElementBoundaryLocation>>()
            {
                new KeyValuePair<string, SpatialElementBoundaryLocation>("At wall finish", SpatialElementBoundaryLocation.Finish),
                new KeyValuePair<string, SpatialElementBoundaryLocation>("At wall center", SpatialElementBoundaryLocation.Center),
            };
            comboBoxRoomBoundary.ItemsSource = boundaryLocations;
            comboBoxRoomBoundary.DisplayMemberPath = "Key";
            comboBoxRoomBoundary.SelectedValuePath = "Value";
            comboBoxRoomBoundary.SelectedIndex = massConfig.RoomBoundaryAtCenterLine ? 1 : 0;
        }

        private void DisplayRoomInfo()
        {
            var selectedRooms = from room in roomDictionary.Values where room.RoomDocument == selectedModel.DocumentTitle select room;
            if (selectedRooms.Count() > 0)
            {
                List<RoomProperties> roomList = selectedRooms.OrderBy(o => o.RoomNumber).ToList();
                dataGridRoom.ItemsSource = null;
                dataGridRoom.ItemsSource = roomList;
            }
        }

        private void expanderParameter_Collapsed(object sender, RoutedEventArgs e)
        {
            GridLength collapsedLength = new GridLength(30);
            rowDefinitionExpander.Height = collapsedLength;
        }

        private void expanderParameter_Expanded(object sender, RoutedEventArgs e)
        {
            GridLength expandedLength = new GridLength(0.5, GridUnitType.Star);
            rowDefinitionExpander.Height = expandedLength;
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<RoomProperties> roomList = (List<RoomProperties>)dataGridRoom.ItemsSource;
                List<RoomProperties> updatedList = new List<RoomProperties>();
                foreach (RoomProperties rp in roomList)
                {
                    rp.IsSelected = true;
                    updatedList.Add(rp);
                }

                dataGridRoom.ItemsSource = null;
                dataGridRoom.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<RoomProperties> roomList = (List<RoomProperties>)dataGridRoom.ItemsSource;
                List<RoomProperties> updatedList = new List<RoomProperties>();
                foreach (RoomProperties rp in roomList)
                {
                    rp.IsSelected = false;
                    updatedList.Add(rp);
                }

                dataGridRoom.ItemsSource = null;
                dataGridRoom.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void buttonCheckDiscrepancy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<RoomProperties> roomList = (List<RoomProperties>)dataGridRoom.ItemsSource;
                List<RoomProperties> updatedList = new List<RoomProperties>();
                foreach (RoomProperties rp in roomList)
                {
                    rp.IsSelected = rp.ModifiedHost;
                    updatedList.Add(rp);
                }

                dataGridRoom.ItemsSource = null;
                dataGridRoom.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool useDefaultHeight = (bool)checkBoxDefaultHeight.IsChecked;
                double defaultHeight = 0;
                if (useDefaultHeight)
                {
                    if (!ValidateInput(out defaultHeight)) { return; }
                }

                List<RoomProperties> roomList = (List<RoomProperties>)dataGridRoom.ItemsSource;
                var roomsFound = from room in roomList where room.IsSelected select room;
                List<RoomProperties> selectedRooms = roomsFound.ToList();
                if (selectedRooms.Count > 0)
                {
                    statusLable.Text = "Creating 3D Masses ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedRooms.Count;

                    bool createdParamMaps = (massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0) ? true : false;

                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (TransactionGroup tg = new TransactionGroup(m_doc))
                    {
                        tg.Start("Create 3D Masses");
                        try
                        {
                            double count = 0;
                            for (int i = 0; i < selectedRooms.Count; i++)
                            {
                                RoomProperties rp = selectedRooms[i];

                                SpatialElementBoundaryOptions spatialOpts = new SpatialElementBoundaryOptions();
                                spatialOpts.SpatialElementBoundaryLocation = selectedRoomBoundary;
                                SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(m_doc, spatialOpts);
                                rp.GetRoomGeometry(rp.RoomTransform, calculator);
                                if (useDefaultHeight)
                                {
                                    rp.UserHeight = defaultHeight;
                                }
                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Create 3D Mass");
                                    var options = trans.GetFailureHandlingOptions();
                                    options.SetFailuresPreprocessor(new DuplicateWarningSwallower());
                                    trans.SetFailureHandlingOptions(options);

                                    try
                                    {
                                        MassProperties createdMass = null;
                                        if (MassCreator.CreateRoomSolid(m_doc, rp, out createdMass))
                                        {
                                            if (roomDictionary.ContainsKey(rp.RoomUniqueId))
                                            {
                                                RoomProperties updatedRoom = new RoomProperties(rp);
                                                updatedRoom.IsSelected = false;
                                                updatedRoom.Linked3d = true;
                                                updatedRoom.Linked3dMass = createdMass;
                                                updatedRoom.ModifiedHost = false;
                                                updatedRoom.ToolTip = "Mass 3D Id: " + createdMass.MassId;
                                                if (updatedRoom.Linked2d && null != updatedRoom.Linked2dMass)
                                                {
                                                    updatedRoom.ToolTip += "\nMass 2D Id: " + updatedRoom.Linked2dMass.MassId;
                                                }

                                                if (createdParamMaps)
                                                {
                                                    bool parameterUpdated = UpdateParameter(rp.RoomElement, createdMass.MassElement);
                                                }
                                                roomDictionary.Remove(rp.RoomUniqueId);
                                                roomDictionary.Add(rp.RoomUniqueId, updatedRoom);
                                            }
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.RollBack();
                                        string message = ex.Message;
                                    }
                                }

                                count++;
                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, count });
                            }

                            DisplayRoomInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to create masses from the selected rooms\n" + ex.Message, "Create 3D Masses from Rooms", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show("Please select rooms to update masses.", "Select Rooms", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d masses from the selected rooms.\n"+ex.Message, "Create 3D Masses from Rooms", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void button2DCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<RoomProperties> roomList = (List<RoomProperties>)dataGridRoom.ItemsSource;
                var selectedRooms = from room in roomList where room.IsSelected select room;

                if (selectedRooms.Count() > 0)
                {
                    statusLable.Text = "Creating 2D Masses ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedRooms.Count();

                    bool createdParamMaps = (massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0) ? true : false;

                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (TransactionGroup tg = new TransactionGroup(m_doc))
                    {
                        tg.Start("Create 2D Masses");
                        try
                        {
                            double count = 0;
                            foreach (RoomProperties rp in selectedRooms)
                            {
                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Create 2D Mass");
                                    var options = trans.GetFailureHandlingOptions();
                                    options.SetFailuresPreprocessor(new DuplicateWarningSwallower());
                                    trans.SetFailureHandlingOptions(options);

                                    try
                                    {
                                        MassProperties createdMass = null;
                                        if (MassCreator.CreateRoomFace(m_doc, rp, out createdMass))
                                        {
                                            if (roomDictionary.ContainsKey(rp.RoomUniqueId))
                                            {
                                                RoomProperties updatedRoom = new RoomProperties(rp);
                                                updatedRoom.IsSelected = false;
                                                updatedRoom.Linked2d = true;
                                                updatedRoom.Linked2dMass = createdMass;
                                                updatedRoom.ModifiedHost = false;
                                                if (updatedRoom.Linked3d && null != updatedRoom.Linked3dMass)
                                                {
                                                    updatedRoom.ToolTip = "Mass 3D Id: " + updatedRoom.Linked3dMass.MassId;
                                                    updatedRoom.ToolTip += "\nMass 2D Id: " + updatedRoom.Linked2dMass.MassId;
                                                }
                                                else
                                                {
                                                    updatedRoom.ToolTip = "Mass 2D Id: " + updatedRoom.Linked2dMass.MassId;
                                                }

                                                if (createdParamMaps)
                                                {
                                                    bool parameterUpdated = UpdateParameter(rp.RoomElement, createdMass.MassElement);
                                                }
                                                roomDictionary.Remove(rp.RoomUniqueId);
                                                roomDictionary.Add(rp.RoomUniqueId, updatedRoom);
                                            }
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.RollBack();
                                        string message = ex.Message;
                                    }
                                }

                                count++;
                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, count });
                            }

                            DisplayRoomInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to create masses from the selected rooms\n" + ex.Message, "Create 3D Masses from Rooms", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show("Please select rooms to update masses.", "Select Rooms", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to create 2d masses from the selected rooms.\n"+ex.Message, "Create 2D Masses from Rooms", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool ValidateInput(out double heightValue)
        {
            bool valid = false;
            heightValue = 0;
            try
            {
                string heightText = textBoxHeight.Text;
                if (double.TryParse(heightText, out heightValue))
                {
                    if (heightValue > 0)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                {
                    MessageBox.Show("Please enter a valid value for the height of masses to be created.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to validate user inputs.\n"+ex.Message, "Validate Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return valid;
        }

        private void menuItemCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRoom.SelectedItems)
                {
                    foreach (var selectedItem in dataGridRoom.SelectedItems)
                    {
                        RoomProperties selectedRoom = (RoomProperties)selectedItem;
                        if (roomDictionary.ContainsKey(selectedRoom.RoomUniqueId))
                        {
                            RoomProperties rp = roomDictionary[selectedRoom.RoomUniqueId];
                            rp.IsSelected = true;
                            roomDictionary.Remove(rp.RoomUniqueId);
                            roomDictionary.Add(rp.RoomUniqueId, rp);
                        }
                    }
                    
                    DisplayRoomInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check selected items.\n"+ex.Message, "Check Selected Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemUncheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRoom.SelectedItems)
                {
                    foreach (var selectedItem in dataGridRoom.SelectedItems)
                    {
                        RoomProperties selectedRoom = (RoomProperties)selectedItem;
                        if (roomDictionary.ContainsKey(selectedRoom.RoomUniqueId))
                        {
                            RoomProperties rp = roomDictionary[selectedRoom.RoomUniqueId];
                            rp.IsSelected = false;
                            roomDictionary.Remove(rp.RoomUniqueId);
                            roomDictionary.Add(rp.RoomUniqueId, rp);
                        }
                    }

                    DisplayRoomInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck selected items.\n"+ex.Message, "Uncheck Selected Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRoom.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridRoom.SelectedItems)
                    {
                        RoomProperties selectedRoom = (RoomProperties)selectedItem;
                        selectedIds.Add(selectedRoom.RoomElementId);
                    }

                    if (selectedIds.Count > 0)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Select Elements");
                            try
                            {
                                UIDocument uidoc = m_app.ActiveUIDocument;
                                Selection selection = uidoc.Selection;
                                uidoc.ShowElements(selectedIds);
                                selection.SetElementIds(selectedIds);

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to view elements in the background.\n"+ex.Message, "View Selected Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemMassView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRoom.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridRoom.SelectedItems)
                    {
                        RoomProperties selectedRoom = (RoomProperties)selectedItem;
                        if (null != selectedRoom.Linked3dMass)
                        {
                            selectedIds.Add(selectedRoom.Linked3dMass.MassElement.Id);
                        }
                    }

                    if (selectedIds.Count > 0)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Select Elements");
                            try
                            {
                                UIDocument uidoc = m_app.ActiveUIDocument;
                                Selection selection = uidoc.Selection;
                                uidoc.ShowElements(selectedIds);
                                selection.SetElementIds(selectedIds);

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to view 3d elements in the background.\n" + ex.Message, "View Selected Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItem2dMassView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRoom.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridRoom.SelectedItems)
                    {
                        RoomProperties selectedRoom = (RoomProperties)selectedItem;
                        if (null != selectedRoom.Linked2dMass)
                        {
                            selectedIds.Add(selectedRoom.Linked2dMass.MassElement.Id);
                        }
                    }

                    if (selectedIds.Count > 0)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Select Elements");
                            try
                            {
                                UIDocument uidoc = m_app.ActiveUIDocument;
                                Selection selection = uidoc.Selection;
                                uidoc.ShowElements(selectedIds);
                                selection.SetElementIds(selectedIds);

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to view 2d mass elements in the background.\n" + ex.Message, "View Selected Eleemnts", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonParametersSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MassParameterWindow paramWindow = new MassParameterWindow(m_app, massConfig);
                if ((bool)paramWindow.ShowDialog())
                {
                    massConfig = paramWindow.MassConfig;
                    if (massConfig.UpdateType == ParameterUpdateType.FromHostToMass)
                    {
                        expanderParameter.Header = "Linked Parameters - (Rooms to Masses)";
                    }
                    else if (massConfig.UpdateType == ParameterUpdateType.FromMassToHost)
                    {
                        expanderParameter.Header = "Linked Parameters - (Masses to Rooms)";
                    }
                    dataGridParameters.ItemsSource = null;
                    dataGridParameters.ItemsSource = massConfig.MassParameters;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set linked parameters.\n" + ex.Message, "Linked Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUpdateParameters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0)
                {
                    List<RoomProperties> roomList = (List<RoomProperties>)dataGridRoom.ItemsSource;
                    var selectedRooms = from room in roomList where room.IsSelected select room;

                    if (selectedRooms.Count() > 0)
                    {
                        statusLable.Text = "Updating Parameters ...";
                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        progressBar.Value = 0;
                        progressBar.Maximum = selectedRooms.Count();

                        UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                        using (TransactionGroup tg = new TransactionGroup(m_doc))
                        {
                            tg.Start("Update Parameters");
                            try
                            {
                                double count = 0;
                                foreach (RoomProperties rp in selectedRooms)
                                {
                                    using (Transaction trans = new Transaction(m_doc))
                                    {
                                        trans.Start("Update Parameter");
                                        var options = trans.GetFailureHandlingOptions();
                                        options.SetFailuresPreprocessor(new DuplicateWarningSwallower());
                                        trans.SetFailureHandlingOptions(options);

                                        try
                                        {
                                            bool updated = false;
                                            if (null != rp.Linked2dMass)
                                            {
                                                updated = UpdateParameter(rp.RoomElement, rp.Linked2dMass.MassElement);
                                            }
                                            if (null != rp.Linked3dMass)
                                            {
                                                updated = UpdateParameter(rp.RoomElement, rp.Linked3dMass.MassElement);
                                            }
                                            
                                            if (updated)
                                            {
                                                RoomProperties updatedRoom = new RoomProperties(rp);
                                                updatedRoom.IsSelected = false;
                                                roomDictionary.Remove(rp.RoomUniqueId);
                                                roomDictionary.Add(rp.RoomUniqueId, updatedRoom);
                                            }
                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.RollBack();
                                            string message = ex.Message;
                                        }
                                    }

                                    count++;
                                    Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, count });
                                }

                                DisplayRoomInfo();
                                tg.Assimilate();
                            }
                            catch (Exception ex)
                            {
                                tg.RollBack();
                                MessageBox.Show("Failed to update parameters of linked masses.\n" + ex.Message, "Update Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }

                        progressBar.Visibility = System.Windows.Visibility.Hidden;
                        statusLable.Text = "Ready";
                    }
                }
                else
                {
                    MessageBox.Show("Please set the configuration for the parameter mappings.\nGo to the Parameters Settings button for more detail.", "Parameters Settings Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update parameter values.\n"+ex.Message, "Update Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool UpdateParameter(Element hostElement, Element massElement)
        {
            bool updated = true;
            try
            {
                Element sourceElement = hostElement;
                Element recipientElement = massElement;
                bool reversed = (massConfig.UpdateType == ParameterUpdateType.FromMassToHost) ? true : false;

                if (reversed)
                {
                    sourceElement = massElement;
                    recipientElement = hostElement;
                }

                foreach (ParameterMapInfo mapInfo in massConfig.MassParameters)
                {
                    try
                    {
                        ParameterInfo sourceInfo = mapInfo.HostParamInfo;
                        ParameterInfo recipientInfo = mapInfo.MassParamInfo;
                        if (reversed)
                        {
                            sourceInfo = mapInfo.MassParamInfo;
                            recipientInfo = mapInfo.HostParamInfo;
                        }

                        Parameter sourceParam = sourceElement.LookupParameter(sourceInfo.ParameterName);
                        Parameter recipientParam = recipientElement.LookupParameter(recipientInfo.ParameterName);
                        if (null != sourceParam && null != recipientParam)
                        {
                            if (sourceParam.StorageType == recipientParam.StorageType && !recipientParam.IsReadOnly)
                            {
                                switch (recipientParam.StorageType)
                                {
                                    case StorageType.ElementId:
                                        recipientParam.Set(sourceParam.AsElementId());
                                        break;
                                    case StorageType.Integer:
                                        recipientParam.Set(sourceParam.AsInteger());
                                        break;
                                    case StorageType.String:
                                        recipientParam.Set(sourceParam.AsString());
                                        break;
                                    case StorageType.Double:
                                        recipientParam.Set(sourceParam.AsDouble());
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        updated = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update parameter.\n" + ex.Message, "Update Parameter", MessageBoxButton.OK, MessageBoxImage.Warning);
                updated = false;
            }
            return updated;
        }

        private void comboBoxModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxModel.SelectedItem)
                {
                    selectedModel = (RevitDocumentProperties)comboBoxModel.SelectedItem;
                    DisplayRoomInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the source model.\n" + ex.Message, "Source Model Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void comboBoxRoomBoundary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBoxRoomBoundary.SelectedItem)
            {
                KeyValuePair<string, SpatialElementBoundaryLocation> selected = (KeyValuePair<string, SpatialElementBoundaryLocation>)comboBoxRoomBoundary.SelectedItem;
                if (massConfig != null)
                {
                    massConfig.RoomBoundaryAtCenterLine = selected.Value == SpatialElementBoundaryLocation.Center;
                    MassConfigDataStorageUtil.StoreMassConfiguration(m_doc, massConfig);
                }
                selectedRoomBoundary = selected.Value;
            }

        }
    }
}
