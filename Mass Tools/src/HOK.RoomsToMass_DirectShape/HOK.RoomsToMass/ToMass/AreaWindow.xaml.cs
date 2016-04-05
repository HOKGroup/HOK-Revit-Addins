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
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.RoomsToMass.ToMass
{
    /// <summary>
    /// Interaction logic for AreaWindow.xaml
    /// </summary>
    public partial class AreaWindow : Window
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private RevitDocumentProperties selectedModel = null;
        private Dictionary<string, RevitDocumentProperties> modelDictionary = new Dictionary<string, RevitDocumentProperties>();
        private Dictionary<string/*areaUniqueId*/, AreaProperties> areaDictionary = new Dictionary<string, AreaProperties>();
        private MassConfiguration massConfig = null;

        public Dictionary<string, AreaProperties> AreaDictionary { get { return areaDictionary; } set { areaDictionary = value; } }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public AreaWindow(UIApplication uiapp, Dictionary<string, RevitDocumentProperties> models, Dictionary<string, AreaProperties> dictionary)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            modelDictionary = models;
            areaDictionary = dictionary;

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

            massConfig = MassConfigDataStorageUtil.GetMassConfiguration(m_doc, SourceType.Areas);
            massConfig.MassSourceType = SourceType.Areas;
            massConfig.HostCategory = "Areas";
            massConfig.MassCategory = "Mass";
            if (massConfig.UpdateType == ParameterUpdateType.FromHostToMass)
            {
                expanderParameter.Header = "Linked Parameters - (Areas to Masses)";
            }
            else if (massConfig.UpdateType == ParameterUpdateType.FromMassToHost)
            {
                expanderParameter.Header = "Linked Parameters - (Masses to Areas)";
            }
            dataGridParameters.ItemsSource = null;
            dataGridParameters.ItemsSource = massConfig.MassParameters;
        }

        private void DisplayAreaInfo()
        {
            var selectedAreas = from area in areaDictionary.Values where area.AreaDocument == selectedModel.DocumentTitle select area;
            if (selectedAreas.Count() > 0)
            {
                List<AreaProperties> areaList = selectedAreas.OrderBy(o => o.AreaNumber).ToList();
                dataGridArea.ItemsSource = null;
                dataGridArea.ItemsSource = areaList;
            }
        }

        private void comboBoxModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBoxModel.SelectedItem)
            {
                selectedModel = (RevitDocumentProperties)comboBoxModel.SelectedItem;
                DisplayAreaInfo();
            }
        }

        private void menuItemCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridArea.SelectedItems)
                {
                    foreach (var selectedItem in dataGridArea.SelectedItems)
                    {
                        AreaProperties selectedArea = (AreaProperties)selectedItem;
                        if (areaDictionary.ContainsKey(selectedArea.AreaUniqueId))
                        {
                            AreaProperties ap = areaDictionary[selectedArea.AreaUniqueId];
                            ap.IsSelected = true;
                            areaDictionary.Remove(ap.AreaUniqueId);
                            areaDictionary.Add(ap.AreaUniqueId, ap);
                        }
                    }
                    DisplayAreaInfo();
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
                if (null != dataGridArea.SelectedItems)
                {
                    foreach (var selectedItem in dataGridArea.SelectedItems)
                    {
                        AreaProperties selectedArea = (AreaProperties)selectedItem;
                        if (areaDictionary.ContainsKey(selectedArea.AreaUniqueId))
                        {
                            AreaProperties ap = areaDictionary[selectedArea.AreaUniqueId];
                            ap.IsSelected = false;
                            areaDictionary.Remove(ap.AreaUniqueId);
                            areaDictionary.Add(ap.AreaUniqueId, ap);
                        }
                    }
                    DisplayAreaInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check selected items.\n" + ex.Message, "Check Selected Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridArea.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridArea.SelectedItems)
                    {
                        AreaProperties selectedArea = (AreaProperties)selectedItem;
                        selectedIds.Add(selectedArea.AreaElementId);
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
                if (null != dataGridArea.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridArea.SelectedItems)
                    {
                        AreaProperties selectedArea = (AreaProperties)selectedItem;
                        if (null!=selectedArea.Linked3dMass)
                        {
                            selectedIds.Add(selectedArea.Linked3dMass.MassElement.Id);
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
                if (null != dataGridArea.SelectedItems)
                {
                    List<ElementId> selectedIds = new List<ElementId>();
                    foreach (var selectedItem in dataGridArea.SelectedItems)
                    {
                        AreaProperties selectedArea = (AreaProperties)selectedItem;
                        if (null != selectedArea.Linked2dMass)
                        {
                            selectedIds.Add(selectedArea.Linked2dMass.MassElement.Id);
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
                List<AreaProperties> areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                List<AreaProperties> updatedList = new List<AreaProperties>();
                foreach (AreaProperties ap in areaList)
                {
                    ap.IsSelected = true;
                    updatedList.Add(ap);
                }

                dataGridArea.ItemsSource = null;
                dataGridArea.ItemsSource = updatedList;
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<AreaProperties> areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                List<AreaProperties> updatedList = new List<AreaProperties>();
                foreach (AreaProperties ap in areaList)
                {
                    ap.IsSelected = false;
                    updatedList.Add(ap);
                }

                dataGridArea.ItemsSource = null;
                dataGridArea.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckDiscrepancy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<AreaProperties> areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                List<AreaProperties> updatedList = new List<AreaProperties>();
                foreach (AreaProperties ap in areaList)
                {
                    ap.IsSelected = ap.ModifiedHost;
                    updatedList.Add(ap);
                }

                dataGridArea.ItemsSource = null;
                dataGridArea.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
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

                List<AreaProperties> areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                var selectedAreas = from area in areaList where area.IsSelected select area;

                if (selectedAreas.Count() > 0)
                {
                    statusLable.Text = "Creating 3D Masses ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedAreas.Count();

                    bool createdParamMaps = (massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0) ? true : false;

                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (TransactionGroup tg = new TransactionGroup(m_doc))
                    {
                        tg.Start("Create 3D Masses");
                        try
                        {
                            double count = 0;
                            foreach (AreaProperties ap in selectedAreas)
                            {
                                if (useDefaultHeight)
                                {
                                    ap.UserHeight = defaultHeight;
                                }
                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Create 3D Mass");
                                    try
                                    {
                                        MassProperties createdMass = null;
                                        if (MassCreator.CreateAreaSolid(m_doc, ap, out createdMass))
                                        {
                                            if (areaDictionary.ContainsKey(ap.AreaUniqueId))
                                            {
                                                AreaProperties updatedArea = new AreaProperties(ap);
                                                updatedArea.IsSelected = false;
                                                updatedArea.Linked3d = true;
                                                updatedArea.Linked3dMass = createdMass;
                                                updatedArea.ModifiedHost = false;
                                                updatedArea.ToolTip = "Mass 3D Id: " + createdMass.MassId;
                                                if (updatedArea.Linked2d && null != updatedArea.Linked2dMass)
                                                {
                                                    updatedArea.ToolTip += "\nMass 2D Id: " + updatedArea.Linked2dMass.MassId;
                                                }

                                                if (createdParamMaps)
                                                {
                                                    bool parameterUpdated = UpdateParameter(ap.AreaElement, createdMass.MassElement);
                                                }
                                                areaDictionary.Remove(ap.AreaUniqueId);
                                                areaDictionary.Add(ap.AreaUniqueId, updatedArea);
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

                            DisplayAreaInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to create masses from the selected areas\n" + ex.Message, "Create 3D Masses from Areas", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show("Please select areas to update masses.", "Select Areas", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d masses from the selected areas.\n" + ex.Message, "Create 3D Masses from Areas", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void button2DCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<AreaProperties> areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                var selectedAreas = from area in areaList where area.IsSelected select area;

                if (selectedAreas.Count() > 0)
                {
                    statusLable.Text = "Creating 2D Masses ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedAreas.Count();

                    bool createdParamMaps = (massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0) ? true : false;

                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (TransactionGroup tg = new TransactionGroup(m_doc))
                    {
                        tg.Start("Create 2D Masses");
                        try
                        {
                            double count = 0;
                            foreach (AreaProperties ap in selectedAreas)
                            {
                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Create 2D Mass");
                                    try
                                    {
                                        MassProperties createdMass = null;
                                        if (MassCreator.CreateAreaFace(m_doc, ap, out createdMass))
                                        {
                                            if (areaDictionary.ContainsKey(ap.AreaUniqueId))
                                            {
                                                AreaProperties updatedArea = new AreaProperties(ap);
                                                updatedArea.IsSelected = false;
                                                updatedArea.Linked2d = true;
                                                updatedArea.Linked2dMass = createdMass;
                                                updatedArea.ModifiedHost = false;
                                                if (updatedArea.Linked3d && null != updatedArea.Linked3dMass)
                                                {
                                                    updatedArea.ToolTip = "Mass 3D Id: " + updatedArea.Linked3dMass.MassId;
                                                    updatedArea.ToolTip += "\nMass 2D Id: " + updatedArea.Linked2dMass.MassId;
                                                }
                                                else
                                                {
                                                    updatedArea.ToolTip = "Mass 2D Id: " + updatedArea.Linked2dMass.MassId;
                                                }

                                                if (createdParamMaps)
                                                {
                                                    bool parameterUpdated = UpdateParameter(ap.AreaElement, createdMass.MassElement);
                                                }
                                                areaDictionary.Remove(ap.AreaUniqueId);
                                                areaDictionary.Add(ap.AreaUniqueId, updatedArea);
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

                            DisplayAreaInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to create 2d masses from the selected areas\n" + ex.Message, "Create 2D Masses from Areas", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show("Please select areas to update masses.", "Select Areas", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 2d masses from the selected areas.\n" + ex.Message, "Create 2D Masses from Areas", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        expanderParameter.Header = "Linked Parameters - (Areas to Masses)";
                    }
                    else if (massConfig.UpdateType == ParameterUpdateType.FromMassToHost)
                    {
                        expanderParameter.Header = "Linked Parameters - (Masses to Areas)";
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
                    List<AreaProperties> areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                    var selectedAreas = from area in areaList where area.IsSelected select area;

                    if (selectedAreas.Count() > 0)
                    {
                        statusLable.Text = "Updating Parameters ...";
                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        progressBar.Value = 0;
                        progressBar.Maximum = selectedAreas.Count();

                        UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                        using (TransactionGroup tg = new TransactionGroup(m_doc))
                        {
                            tg.Start("Update Parameters");
                            try
                            {
                                double count = 0;
                                foreach (AreaProperties ap in selectedAreas)
                                {
                                    using (Transaction trans = new Transaction(m_doc))
                                    {
                                        trans.Start("Update Parameter");
                                        try
                                        {
                                            bool updated = false;
                                            if (null != ap.Linked2dMass)
                                            {
                                                updated = UpdateParameter(ap.AreaElement, ap.Linked2dMass.MassElement);
                                            }
                                            if (null != ap.Linked3dMass)
                                            {
                                                updated = UpdateParameter(ap.AreaElement, ap.Linked3dMass.MassElement);
                                            }
                                            
                                            if (updated)
                                            {
                                                AreaProperties updatedArea = new AreaProperties(ap);
                                                updatedArea.IsSelected = false;
                                                areaDictionary.Remove(ap.AreaUniqueId);
                                                areaDictionary.Add(ap.AreaUniqueId, updatedArea);
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
                                DisplayAreaInfo();
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

       

        

        
    }
}
