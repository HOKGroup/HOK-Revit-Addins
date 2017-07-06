using System;
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
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace HOK.RoomElevation
{
    /// <summary>
    /// Interaction logic for ElevationWindow.xaml
    /// </summary>
    /// 
    public partial class ElevationWindow : Window
    {
        private UIApplication m_app;
        private Document m_doc;
        private ViewPlan viewPlan = null;
        private List<ViewFamilyType> viewFamilyTypes = new List<ViewFamilyType>();
        //private ViewFamilyType viewElevationFamilyType = null;
        private ElevationCreatorSettings toolSettings = null;
        private Room sampleRoom = null;
        private Dictionary<int, RoomElevationProperties> roomDictionary = new Dictionary<int, RoomElevationProperties>();
        private Dictionary<int, LinkedInstanceProperties> linkedDocuments = new Dictionary<int, LinkedInstanceProperties>();

        public ElevationCreatorSettings ToolSettings { get { return toolSettings; } set { toolSettings = value; } }
        public Dictionary<int, RoomElevationProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }
        public Dictionary<int, LinkedInstanceProperties> LinkedDocuments { get { return linkedDocuments; } set { linkedDocuments = value; } }

        public ElevationWindow(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            string versionNumber = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Title = "Room Elevation Creator - v." + versionNumber;
        }

        public bool CheckPrerequisites()
        {
            bool result = false;
            try
            {
                //Viewplan
                Autodesk.Revit.DB.View activeView = m_doc.ActiveView;
                viewPlan = activeView as ViewPlan;
                if (null != viewPlan)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<ViewFamilyType> viewTypeList = collector.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().ToList();
                    var viewTypes = from vft in viewTypeList where vft.ViewFamily == ViewFamily.Elevation select vft;
                    if (viewTypes.Count() > 0)
                    {
                        viewFamilyTypes = viewTypes.ToList();
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show("A valid view family type for Elevation has not been loaded in this project.", "View Family Type Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                        result = false;
                    }
                }
                else
                {
                    MessageBox.Show("Please open a plan view that elevation markers are visible.", "Open a Plan View", MessageBoxButton.OK, MessageBoxImage.Information);
                    result = false;
                }

                FilteredElementCollector linkCollector = new FilteredElementCollector(m_doc, viewPlan.Id);
                List<RevitLinkInstance> linkInstances = linkCollector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();
                foreach (RevitLinkInstance instance in linkInstances)
                {
                    LinkedInstanceProperties lip = new LinkedInstanceProperties(instance);
                    if (null != lip.LinkedDocument)
                    {
                        if (!linkedDocuments.ContainsKey(lip.InstanceId))
                        {
                            linkedDocuments.Add(lip.InstanceId, lip);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check prereauisites.\n" + ex.Message, "Check Prerequisites", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public bool DisplayUI()
        {
            bool result = false;
            try
            {
                bool displayedSettings = DisplaySettings();
                bool displayedRooms = DisplayRooms();

                result = displayedRooms && displayedSettings;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display UI components\n"+ex.Message , "Display UI", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }
        
        private bool DisplayRooms()
        {
            bool result = false;
            try
            {
                
                Dictionary<int/*roomId*/, RoomElevationProperties> roomsStored = ElevationCreatorDataStorageUtil.GetRoomElevationProperties(m_doc, linkedDocuments);
                roomDictionary = GetRoomsProperties(roomsStored);

                treeViewRoom.ItemsSource = TreeviewModel.SetTreeView(roomDictionary, toolSettings.IsLinkedRoom );
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display rooms.\n"+ex.Message, "Elevation Creator: DisplayRooms", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private Dictionary<int, RoomElevationProperties> GetRoomsProperties(Dictionary<int, RoomElevationProperties> roomsStored)
        {
            Dictionary<int, RoomElevationProperties> dictionary = new Dictionary<int, RoomElevationProperties>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc, viewPlan.Id);
                List<Room> rooms = collector.OfCategory(BuiltInCategory.OST_Rooms).ToElements().Cast<Room>().ToList();
                foreach (Room room in rooms)
                {
                    int roomId = room.Id.IntegerValue;
                    if (room.Area > 0)
                    {
                        //skip unplaced rooms
                        RoomElevationProperties rep = new RoomElevationProperties(room);
                        if (roomsStored.ContainsKey(roomId))
                        {
                            rep = roomsStored[roomId];
                        }

                        if (!dictionary.ContainsKey(roomId))
                        {
                            dictionary.Add(roomId, rep);
                        }
                    }
                }

                foreach (LinkedInstanceProperties linkedInstance in linkedDocuments.Values)
                {
                    collector = new FilteredElementCollector(linkedInstance.LinkedDocument);
                    List<Room> linkedRooms = collector.OfCategory(BuiltInCategory.OST_Rooms).ToElements().Cast<Room>().ToList();
                    foreach (Room room in linkedRooms)
                    {
                        int roomId = room.Id.IntegerValue;
                        if (room.Area > 0)
                        {
                            RoomElevationProperties rep = new RoomElevationProperties(room, linkedInstance.InstanceId);
                            if (roomsStored.ContainsKey(roomId))
                            {
                                rep = roomsStored[roomId];
                            }

                            if (!dictionary.ContainsKey(roomId))
                            {
                                dictionary.Add(roomId, rep);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get Room and Elevation properties.\n" + ex.Message, "Elevation Creator: GetRoomsProperties", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return dictionary;
        }

        private bool DisplaySettings()
        {
            bool result = false;
            try
            {
                CollectViewFamilies();
                CollectViewTemplates();
                CollectRoomParameters();
                GetToolSettings();
                toolSettings.ActiveViewPlan = viewPlan;
                labelViewName.Content = GetSampleViewName();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display tools settings.\n"+ex.Message, "Elevation Creator: DisplaySettings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private void CollectViewFamilies()
        {
            try
            {
                viewFamilyTypes = viewFamilyTypes.OrderBy(view => view.Name).ToList();
                comboBoxViewFamily.ItemsSource = viewFamilyTypes;
                comboBoxViewFamily.DisplayMemberPath = "Name";

                for (int i = 0; i < comboBoxViewFamily.Items.Count; i++)
                {
                    ViewFamilyType currentType = comboBoxViewFamily.Items[i] as ViewFamilyType;
                    if (currentType.Name.ToLower().Contains("interior"))
                    {
                        comboBoxViewFamily.SelectedIndex = i; break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect view family types.\n"+ex.Message, "Collect View Family Types", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CollectViewTemplates()
        {
            try
            {
                List<ViewTemplateProperties> templates = new List<ViewTemplateProperties>();
                ViewTemplateProperties vtp = new ViewTemplateProperties();
                templates.Add(vtp);

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<ViewSection> viewSections = collector.OfClass(typeof(ViewSection)).ToElements().Cast<ViewSection>().ToList();
                var sections = from section in viewSections where section.IsTemplate select section;
                if (sections.Count() > 0)
                {
                    List<ViewSection> sectionList = sections.OrderBy(view => view.ViewName).ToList();
                    foreach (ViewSection viewSection in sectionList)
                    {
                        vtp = new ViewTemplateProperties(viewSection);
                        templates.Add(vtp);
                    }
                }

                comboBoxViewTemplate.ItemsSource = templates;
                comboBoxViewTemplate.DisplayMemberPath = "TemplateName";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect view templates.\n"+ex.Message, "Elevation Creator: Collect View Templates", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CollectRoomParameters()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Room> rooms = collector.OfCategory(BuiltInCategory.OST_Rooms).ToElements().Cast<Room>().ToList();
                if (rooms.Count > 0)
                {
                    sampleRoom = rooms.First();
                    List<string> roomParameters = new List<string>();
                    foreach (Parameter parameter in sampleRoom.Parameters)
                    {
                        string paramName = parameter.Definition.Name;
                        if (paramName.Contains(".Extensions"))
                        {
                            continue;
                        }

                        if (!roomParameters.Contains(paramName))
                        {
                            roomParameters.Add(paramName);
                        }
                    }

                    roomParameters = roomParameters.OrderBy(paramName => paramName).ToList();
                    comboBoxIntermediate.ItemsSource = roomParameters;
                    comboBoxSuffix.ItemsSource = roomParameters;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect room parameters.\n"+ex.Message, "Elevation Creator: CollectRoomParameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GetSampleViewName()
        {
            string viewName = "";
            try
            {
                string prefix = toolSettings.PrefixText;
                string intermediateText = toolSettings.IntermediateText;
                string suffix = toolSettings.SuffixText;

                if (null != sampleRoom)
                {
                    Parameter parameter = sampleRoom.LookupParameter(toolSettings.IntermediateText);
                    if (null != parameter)
                    {
                        if (parameter.StorageType == StorageType.String)
                        {
                            intermediateText = parameter.AsString();
                        }
                        else
                        {
                            intermediateText = parameter.AsValueString();
                        }
                    }

                    parameter = sampleRoom.LookupParameter(toolSettings.SuffixText);

                    if (null != parameter)
                    {
                        if (parameter.StorageType == StorageType.String)
                        {
                            suffix = parameter.AsString();
                        }
                        else
                        {
                            suffix = parameter.AsValueString();
                        }
                    }

                    if (toolSettings.PrefixSelected)
                    {
                        viewName = prefix;
                    }
                    if (toolSettings.IntermediateSelected)
                    {
                        viewName += intermediateText;
                    }

                    if (toolSettings.ElevationSelected)
                    {
                        if (!string.IsNullOrEmpty(viewName))
                        {
                            viewName += "-Elevation";
                        }
                        else
                        {
                            viewName += "Elevation";
                        }
                    }

                    if (toolSettings.ABCDSelected)
                    {
                        if (!string.IsNullOrEmpty(viewName))
                        {
                            viewName += "-A";
                        }
                        else
                        {
                            viewName +="A";
                        }
                    }

                    if (toolSettings.SuffixSelected)
                    {
                        if (!string.IsNullOrEmpty(viewName))
                        {
                            viewName += " (" + suffix+")";
                        }
                        else
                        {
                            viewName += "("+suffix+")";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the example of the view name.\n"+ex.Message , "Elevation Creator: GetSampleViewName", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewName;
        }

        private void GetToolSettings()
        {
            try
            {
                toolSettings = ElevationCreatorSettingStorageUtil.GetElevationCreatorSettings(m_doc);
                radioBttnRoomLink.IsChecked = toolSettings.IsLinkedRoom;
                radioBttnWallLink.IsChecked = toolSettings.IsLInkedWall;

                for (int i = 0; i < comboBoxViewFamily.Items.Count; i++)
                {
                    ViewFamilyType vft = comboBoxViewFamily.Items[i] as ViewFamilyType;
                    if (vft.Id.IntegerValue == toolSettings.ViewFamilyId)
                    {
                        comboBoxViewFamily.SelectedIndex = i; break;
                    }
                }

                for (int i = 0; i < comboBoxViewTemplate.Items.Count; i++)
                {
                    ViewTemplateProperties vtp = comboBoxViewTemplate.Items[i] as ViewTemplateProperties;
                    if (vtp.TemplateId.IntegerValue == toolSettings.ViewTemplateId)
                    {
                        comboBoxViewTemplate.SelectedIndex = i; break;
                    }
                }

                radioBttnTemplate.IsChecked = toolSettings.ScaleByTemplate;
                textBoxScale.Text = toolSettings.CustomScale.ToString();
                textBoxSpace.Text = toolSettings.SpaceAround.ToString();
                checkBoxA.IsChecked = toolSettings.AIsSelected;
                checkBoxB.IsChecked = toolSettings.BIsSelected;
                checkBoxC.IsChecked = toolSettings.CIsSelected;
                checkBoxD.IsChecked = toolSettings.DIsSelected;
                checkBoxPrefix.IsChecked = toolSettings.PrefixSelected;
                textBoxPrefix.Text = toolSettings.PrefixText;
                checkBoxIntermediate.IsChecked = toolSettings.IntermediateSelected;

                for (int i = 0; i < comboBoxIntermediate.Items.Count;i++)
                {
                    string itemText = comboBoxIntermediate.Items[i].ToString();
                    if (itemText == toolSettings.IntermediateText)
                    {
                        comboBoxIntermediate.SelectedIndex = i;
                    }
                }

                checkBoxElevation.IsChecked = toolSettings.ElevationSelected;
                checkBoxABCD.IsChecked = toolSettings.ABCDSelected;
                checkBoxSuffix.IsChecked = toolSettings.SuffixSelected;

                for (int i = 0; i < comboBoxSuffix.Items.Count; i++)
                {
                    string itemText = comboBoxSuffix.Items[i].ToString();
                    if (itemText == toolSettings.SuffixText)
                    {
                        comboBoxSuffix.SelectedIndex = i;
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get Elevation Creator settings.\n"+ex.Message, "Elevation Creator: GetToolSettings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool SetToolSettings()
        {
            bool result = false;
            try
            {
                toolSettings.IsLinkedRoom = (bool) radioBttnRoomLink.IsChecked;
                toolSettings.IsLInkedWall = (bool)radioBttnWallLink.IsChecked;
                ViewFamilyType vft = (ViewFamilyType)comboBoxViewFamily.SelectedItem;
                if (null != vft)
                {
                    toolSettings.ViewFamilyId = vft.Id.IntegerValue;
                }
                ViewTemplateProperties vtp = (ViewTemplateProperties) comboBoxViewTemplate.SelectedItem;
                if (null != vtp)
                {
                    toolSettings.ViewTemplateId = vtp.TemplateId.IntegerValue;
                }

                toolSettings.ScaleByTemplate = (bool)radioBttnTemplate.IsChecked;

                int scaleVal=0;
                if (!int.TryParse(textBoxScale.Text, out scaleVal))
                {
                    MessageBox.Show("Please enter a valid number for the scale value.", "Elevation Creator : Invalid Scale Value", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                toolSettings.CustomScale = scaleVal;

                int spaceAround = 0;
                if (!int.TryParse(textBoxSpace.Text, out spaceAround))
                {
                    MessageBox.Show("Please enter a valid number for the space around the views.", "Elevation Creator : Invalid Space Value", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                toolSettings.SpaceAround = spaceAround;

                toolSettings.AIsSelected = (bool)checkBoxA.IsChecked;
                toolSettings.BIsSelected = (bool)checkBoxB.IsChecked;
                toolSettings.CIsSelected = (bool)checkBoxC.IsChecked;
                toolSettings.DIsSelected = (bool)checkBoxD.IsChecked;

                toolSettings.PrefixSelected = (bool)checkBoxPrefix.IsChecked;
                toolSettings.PrefixText = textBoxPrefix.Text;
                if (toolSettings.PrefixSelected && string.IsNullOrEmpty(toolSettings.PrefixText))
                {
                    MessageBox.Show("Please enter a vlid prefix text.", "Elevation Creator : Empty Prefix", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                toolSettings.IntermediateSelected = (bool)checkBoxIntermediate.IsChecked;
                toolSettings.IntermediateText = comboBoxIntermediate.Text;

                toolSettings.ElevationSelected = (bool)checkBoxElevation.IsChecked;
                toolSettings.ABCDSelected = (bool)checkBoxABCD.IsChecked;
                toolSettings.SuffixSelected = (bool)checkBoxSuffix.IsChecked;
                toolSettings.SuffixText = comboBoxSuffix.Text;

                result = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the settings.\n"+ex.Message, "Elevation Creator: SetToolSettings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private void buttonCreateByPick_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SetToolSettings())
                {
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start creating elevation views by pick elements.\n"+ex.Message, "Elevation Creator: CreateByPickElements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCreateByList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SetToolSettings())
                {
                    List<RoomElevationProperties> selectedRooms = new List<RoomElevationProperties>();

                    List<TreeviewModel> treeviewModels = treeViewRoom.ItemsSource as List<TreeviewModel>;
                    foreach (TreeviewModel roomNode in treeviewModels)
                    {
                        if (roomNode.IsChecked == true)
                        {
                            if (null != roomNode.RoomProperties)
                            {
                                RoomElevationProperties rep = roomNode.RoomProperties;
                                selectedRooms.Add(rep);
                                
                            }
                        }
                    }

                    if (selectedRooms.Count > 0)
                    {
                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        statusLable.Visibility = System.Windows.Visibility.Visible;
                        statusLable.Text = "Creating Elevation Views . . .";

                        progressBar.Minimum = 0;
                        progressBar.Maximum = selectedRooms.Count;
                        progressBar.Value = 0;

                        double value = 0;
                        UpdateProgressBarDelegate updatePdDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

                        foreach (RoomElevationProperties rep in selectedRooms)
                        {
                            ElevationCreator creator = new ElevationCreator(m_app, rep, toolSettings, linkedDocuments);
                            if (creator.CheckExisting())
                            {
                                if (creator.CreateElevationByList())
                                {
                                    RoomElevationProperties roomProperties = new RoomElevationProperties(creator.RoomProperties);
                                    if (roomDictionary.ContainsKey(roomProperties.RoomId))
                                    {
                                        roomDictionary.Remove(roomProperties.RoomId);
                                    }
                                    roomDictionary.Add(roomProperties.RoomId, roomProperties);
                                }
                            }

                            value += 1;
                            Dispatcher.Invoke(updatePdDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                        }

                        statusLable.Text = "Ready";
                        progressBar.Visibility = System.Windows.Visibility.Hidden;

                        treeViewRoom.ItemsSource = null;
                        treeViewRoom.ItemsSource = TreeviewModel.SetTreeView(roomDictionary, toolSettings.IsLinkedRoom);

                        if (LogMessageBuilder.GetLogMessages().Length > 0)
                        {
                            LogMessageBox logMessageBox = new LogMessageBox();
                            logMessageBox.Show();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start creating elevation views by rooms lists.\n"+ex.Message , "Elevation Creator: CreateByRoomList", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

       
        
        private void buttonCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TreeviewModel> treeviewModels = treeViewRoom.ItemsSource as List<TreeviewModel>;
                foreach (TreeviewModel model in treeviewModels)
                {
                    model.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all rooms in the list.\n" + ex.Message, "Elevation Creator: Check All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUncheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TreeviewModel> treeviewModels = treeViewRoom.ItemsSource as List<TreeviewModel>;
                foreach (TreeviewModel model in treeviewModels)
                {
                    model.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck all rooms in the list.\n"+ex.Message , "Elevation Creator: Uncheck All ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxPrefix_Checked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.PrefixSelected = (bool)checkBoxPrefix.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void checkBoxPrefix_Unchecked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.PrefixSelected = (bool)checkBoxPrefix.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void textBoxPrefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.PrefixText = textBoxPrefix.Text;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void checkBoxIntermediate_Checked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.IntermediateSelected = (bool)checkBoxIntermediate.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void checkBoxIntermediate_Unchecked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.IntermediateSelected = (bool)checkBoxIntermediate.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void comboBoxIntermediate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != toolSettings)
            {
                if (null != comboBoxIntermediate.SelectedItem)
                {
                    toolSettings.IntermediateText = comboBoxIntermediate.SelectedItem.ToString();
                    labelViewName.Content = GetSampleViewName();
                }
            }
        }

        private void checkBoxElevation_Checked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.ElevationSelected = (bool)checkBoxElevation.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void checkBoxElevation_Unchecked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.ElevationSelected = (bool)checkBoxElevation.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void checkBoxABCD_Checked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.ABCDSelected = (bool)checkBoxABCD.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void checkBoxABCD_Unchecked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.ABCDSelected = (bool)checkBoxABCD.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void checkBoxSuffix_Checked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.SuffixSelected = (bool)checkBoxSuffix.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void checkBoxSuffix_Unchecked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                toolSettings.SuffixSelected = (bool)checkBoxSuffix.IsChecked;
                labelViewName.Content = GetSampleViewName();
            }
        }

        private void comboBoxSuffix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != toolSettings)
            {
                if (null != comboBoxSuffix.SelectedItem)
                {
                    toolSettings.SuffixText = comboBoxSuffix.SelectedItem.ToString();
                    labelViewName.Content = GetSampleViewName();
                }
            }
        }

        private void radioBttnRoomLink_Checked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                bool islinkedRoom = (bool)radioBttnRoomLink.IsChecked;
                treeViewRoom.ItemsSource = null;
                treeViewRoom.ItemsSource = TreeviewModel.SetTreeView(roomDictionary, islinkedRoom);
            }
        }

        private void radioBttnRoomHost_Checked(object sender, RoutedEventArgs e)
        {
            if (null != toolSettings)
            {
                bool islinkedRoom = (bool)radioBttnRoomLink.IsChecked;
                treeViewRoom.ItemsSource = null;
                treeViewRoom.ItemsSource = TreeviewModel.SetTreeView(roomDictionary, islinkedRoom);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //save settings
            try
            {
                if (SetToolSettings())
                {
                    if (ElevationCreatorSettingStorageUtil.StoreElevationCreatorSettings(m_doc, toolSettings))
                    {
                        //saved settings.
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings to extensible storage.\n"+ex.Message, "Elevatino Creator: Save Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

    public class ViewTemplateProperties
    {
        private ViewSection templateObj = null;
        private ElementId templateId = ElementId.InvalidElementId;
        private string templateName = "";

        public ViewSection TemplateObj{get{return templateObj;}set{templateObj=value;}}
        public ElementId TemplateId { get { return templateId; } set { templateId = value; } }
        public string TemplateName { get { return templateName; } set { templateName = value; } }

        public ViewTemplateProperties()
        {
            templateName = "<None>";
        }

        public ViewTemplateProperties(ViewSection section)
        {
            templateObj = section;
            templateId = section.Id;
            templateName = section.ViewName;
        }
    }

    //for binding purpose in UI

    public class DeleteViewsPreprocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
            {
                FailureDefinitionId defId = failure.GetFailureDefinitionId();

                //delete warning "Elevation View Delete"
                if (failure.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
            }

            return FailureProcessingResult.Continue;
        }
    }

}
