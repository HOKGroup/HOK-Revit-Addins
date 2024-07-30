using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.RoomsToMass.ToMass
{
    
    /// <summary>
    /// Interaction logic for FloorWindow.xaml
    /// </summary>
    public partial class FloorWindow
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private RevitDocumentProperties selectedModel = null;
        private Dictionary<string, RevitDocumentProperties> modelDictionary = new Dictionary<string, RevitDocumentProperties>();
        private Dictionary<string/*areaUniqueId*/, FloorProperties> floorDictionary = new Dictionary<string, FloorProperties>();
        private MassConfiguration massConfig = null;

        public Dictionary<string, FloorProperties> FloorDictionary { get { return floorDictionary; } set { floorDictionary = value; } }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public FloorWindow(UIApplication uiapp, Dictionary<string, RevitDocumentProperties> models, Dictionary<string, FloorProperties> dictionary)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            modelDictionary = models;
            floorDictionary = dictionary;

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

            massConfig = MassConfigDataStorageUtil.GetMassConfiguration(m_doc, SourceType.Floors);
            massConfig.MassSourceType = SourceType.Floors;
            massConfig.HostCategory = "Floors";
            massConfig.MassCategory = "Mass";
            if (massConfig.UpdateType == ParameterUpdateType.FromHostToMass)
            {
                expanderParameter.Header = "Linked Parameters - (Floors to Masses)";
            }
            else if (massConfig.UpdateType == ParameterUpdateType.FromMassToHost)
            {
                expanderParameter.Header = "Linked Parameters - (Masses to Floors)";
            }
            dataGridParameters.ItemsSource = null;
            dataGridParameters.ItemsSource = massConfig.MassParameters;
        }

        private void DisplayFloorInfo()
        {
            var selectedFloors = from floor in floorDictionary.Values where floor.FloorDocument == selectedModel.DocumentTitle select floor;
            if (selectedFloors.Count() > 0)
            {
                List<FloorProperties> floorList = selectedFloors.OrderBy(o => o.FloorName).ToList();
                dataGridFloor.ItemsSource = null;
                dataGridFloor.ItemsSource = floorList;
            }
        }

        private void comboBoxModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBoxModel.SelectedItem)
            {
                selectedModel = (RevitDocumentProperties)comboBoxModel.SelectedItem;
                DisplayFloorInfo();
            }
        }

        private void menuItemCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridFloor.SelectedItems)
                {
                    foreach (var selectedItem in dataGridFloor.SelectedItems)
                    {
                        FloorProperties selectedFloor = (FloorProperties)selectedItem;
                        if (floorDictionary.ContainsKey(selectedFloor.FloorUniqueId))
                        {
                            FloorProperties fp = floorDictionary[selectedFloor.FloorUniqueId];
                            fp.IsSelected = true;
                            floorDictionary.Remove(fp.FloorUniqueId);
                            floorDictionary.Add(fp.FloorUniqueId, fp);
                        }
                    }
                    DisplayFloorInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check selected items.\n" + ex.Message, "Check Selected Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemUncheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridFloor.SelectedItems)
                {
                    foreach (var selectedItem in dataGridFloor.SelectedItems)
                    {
                        FloorProperties selectedFloor = (FloorProperties)selectedItem;
                        if (floorDictionary.ContainsKey(selectedFloor.FloorUniqueId))
                        {
                            FloorProperties fp = floorDictionary[selectedFloor.FloorUniqueId];
                            fp.IsSelected = false;
                            floorDictionary.Remove(fp.FloorUniqueId);
                            floorDictionary.Add(fp.FloorUniqueId, fp);
                        }
                    }
                    DisplayFloorInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck selected items.\n" + ex.Message, "Uncheck Selected Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridFloor.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridFloor.SelectedItems)
                    {
                        FloorProperties selectedFloor = (FloorProperties)selectedItem;
                        selectedIds.Add(selectedFloor.FloorElementId);
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
                MessageBox.Show("Failed to view elements in the background.\n" + ex.Message, "View Selected Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemMassView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridFloor.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridFloor.SelectedItems)
                    {
                        FloorProperties selectedFloor = (FloorProperties)selectedItem;
                        if (null!=selectedFloor.Linked3dMass)
                        {
                            selectedIds.Add(selectedFloor.Linked3dMass.MassElement.Id);
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
                MessageBox.Show("Failed to view 3d mass elements in the background.\n" + ex.Message, "View Selected Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItem2dMassView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridFloor.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridFloor.SelectedItems)
                    {
                        FloorProperties selectedFloor = (FloorProperties)selectedItem;
                        if (null != selectedFloor.Linked2dMass)
                        {
                            selectedIds.Add(selectedFloor.Linked2dMass.MassElement.Id);
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
                MessageBox.Show("Failed to view 2d mass elements in the background.\n" + ex.Message, "View Selected Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<FloorProperties> floorList = (List<FloorProperties>)dataGridFloor.ItemsSource;
                List<FloorProperties> updatedList = new List<FloorProperties>();
                foreach (FloorProperties fp in floorList)
                {
                    fp.IsSelected = true;
                    updatedList.Add(fp);
                }

                dataGridFloor.ItemsSource = null;
                dataGridFloor.ItemsSource = updatedList;
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
                List<FloorProperties> floorList = (List<FloorProperties>)dataGridFloor.ItemsSource;
                List<FloorProperties> updatedList = new List<FloorProperties>();
                foreach (FloorProperties fp in floorList)
                {
                    fp.IsSelected = false;
                    updatedList.Add(fp);
                }

                dataGridFloor.ItemsSource = null;
                dataGridFloor.ItemsSource = updatedList;
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
                List<FloorProperties> floorList = (List<FloorProperties>)dataGridFloor.ItemsSource;
                List<FloorProperties> updatedList = new List<FloorProperties>();
                foreach (FloorProperties fp in floorList)
                {
                    fp.IsSelected = fp.ModifiedHost;
                    updatedList.Add(fp);
                }

                dataGridFloor.ItemsSource = null;
                dataGridFloor.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
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

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

                List<FloorProperties> floorList = (List<FloorProperties>)dataGridFloor.ItemsSource;
                var selectedFloors = from floor in floorList where floor.IsSelected select floor;

                if (selectedFloors.Count() > 0)
                {
                    statusLable.Text = "Creating 3D Masses ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedFloors.Count();

                    bool createdParamMaps = (massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0) ? true : false;

                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (TransactionGroup tg = new TransactionGroup(m_doc))
                    {
                        tg.Start("Create 3D Masses");
                        try
                        {
                            double count = 0;
                            foreach (FloorProperties fp in selectedFloors)
                            {
                                if (useDefaultHeight)
                                {
                                    fp.UserHeight = defaultHeight;
                                }
                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Create Mass");
                                    var options = trans.GetFailureHandlingOptions();
                                    options.SetFailuresPreprocessor(new DuplicateWarningSwallower());
                                    trans.SetFailureHandlingOptions(options);

                                    try
                                    {
                                        MassProperties createdMass = null;
                                        if (MassCreator.CreateFloorSolid(m_doc, fp, out createdMass))
                                        {
                                            if (floorDictionary.ContainsKey(fp.FloorUniqueId))
                                            {
                                                FloorProperties updatedFloor = new FloorProperties(fp);
                                                updatedFloor.IsSelected = false;
                                                updatedFloor.Linked3d = true;
                                                updatedFloor.Linked3dMass = createdMass;
                                                updatedFloor.ModifiedHost = false;
                                                updatedFloor.ToolTip = "Mass 3D Id: " + createdMass.MassId;
                                                if (updatedFloor.Linked2d && null != updatedFloor.Linked2dMass)
                                                {
                                                    updatedFloor.ToolTip += "\nMass 2D Id: " + updatedFloor.Linked2dMass.MassId;
                                                }

                                                if (createdParamMaps)
                                                {
                                                    bool parameterUpdated = UpdateParameter(fp.FloorElement, createdMass.MassElement);
                                                }
                                                floorDictionary.Remove(fp.FloorUniqueId);
                                                floorDictionary.Add(fp.FloorUniqueId, updatedFloor);
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

                            DisplayFloorInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to create masses from the selected floors\n" + ex.Message, "Create 3D Masses from Floors", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show("Please select floors to update masses.", "Select Floors", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create masses from the selected floors.\n" + ex.Message, "Create 3D Masses from Floors", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void button2DCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<FloorProperties> floorList = (List<FloorProperties>)dataGridFloor.ItemsSource;
                var selectedFloors = from floor in floorList where floor.IsSelected select floor;

                if (selectedFloors.Count() > 0)
                {
                    statusLable.Text = "Creating 2D Masses ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedFloors.Count();

                    bool createdParamMaps = (massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0) ? true : false;

                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (TransactionGroup tg = new TransactionGroup(m_doc))
                    {
                        tg.Start("Create 2D Masses");
                        try
                        {
                            double count = 0;
                            foreach (FloorProperties fp in selectedFloors)
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
                                        if (MassCreator.CreateFloorFace(m_doc, fp, out createdMass))
                                        {
                                            if (floorDictionary.ContainsKey(fp.FloorUniqueId))
                                            {
                                                FloorProperties updatedFloor = new FloorProperties(fp);
                                                updatedFloor.IsSelected = false;
                                                updatedFloor.Linked2d = true;
                                                updatedFloor.Linked2dMass = createdMass;
                                                updatedFloor.ModifiedHost = false;
                                                if (updatedFloor.Linked3d && null != updatedFloor.Linked3dMass)
                                                {
                                                    updatedFloor.ToolTip = "Mass 3D Id: " + updatedFloor.Linked3dMass.MassId;
                                                    updatedFloor.ToolTip += "\nMass 2D Id: " + updatedFloor.Linked2dMass.MassId;
                                                }
                                                else
                                                {
                                                    updatedFloor.ToolTip = "Mass 2D Id: " + updatedFloor.Linked2dMass.MassId;
                                                }
                                                
                                                if (createdParamMaps)
                                                {
                                                    bool parameterUpdated = UpdateParameter(fp.FloorElement, createdMass.MassElement);
                                                }
                                                floorDictionary.Remove(fp.FloorUniqueId);
                                                floorDictionary.Add(fp.FloorUniqueId, updatedFloor);
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

                            DisplayFloorInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to create masses from the selected floors\n" + ex.Message, "Create 3D Masses from Floors", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show("Please select floors to update masses.", "Select Floors", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create masses from the selected floors.\n" + ex.Message, "Create 3D Masses from Floors", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Failed to validate user inputs.\n" + ex.Message, "Validate Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return valid;
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
                        expanderParameter.Header = "Linked Parameters - (Floors to Masses)";
                    }
                    else if (massConfig.UpdateType == ParameterUpdateType.FromMassToHost)
                    {
                        expanderParameter.Header = "Linked Parameters - (Masses to Floors)";
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
                    List<FloorProperties> floorList = (List<FloorProperties>)dataGridFloor.ItemsSource;
                    var selectedFloors = from floor in floorList where floor.IsSelected select floor;

                    if (selectedFloors.Count() > 0)
                    {
                        statusLable.Text = "Updating Parameters ...";
                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        progressBar.Value = 0;
                        progressBar.Maximum = selectedFloors.Count();

                        UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                        using (TransactionGroup tg = new TransactionGroup(m_doc))
                        {
                            tg.Start("Update Parameters");
                            try
                            {
                                double count = 0;
                                foreach (FloorProperties fp in selectedFloors)
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
                                            if (null != fp.Linked2dMass)
                                            {
                                                updated = UpdateParameter(fp.FloorElement, fp.Linked2dMass.MassElement);
                                            }
                                            if (null != fp.Linked3dMass)
                                            {
                                                updated = UpdateParameter(fp.FloorElement, fp.Linked3dMass.MassElement);
                                            }

                                            if (updated)
                                            {
                                                FloorProperties updatedFloor = new FloorProperties(fp);
                                                updatedFloor.IsSelected = false;
                                                floorDictionary.Remove(fp.FloorUniqueId);
                                                floorDictionary.Add(fp.FloorUniqueId, updatedFloor);
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
                                DisplayFloorInfo();
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
                MessageBox.Show("Failed to update parameter values.\n" + ex.Message, "Update Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
