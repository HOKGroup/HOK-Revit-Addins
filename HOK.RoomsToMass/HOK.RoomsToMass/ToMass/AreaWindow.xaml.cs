#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

#endregion

namespace HOK.RoomsToMass.ToMass
{
    /// <summary>
    /// Interaction logic for AreaWindow.xaml
    /// </summary>
    public partial class AreaWindow
    {
        private readonly UIApplication app;
        private readonly Document doc;
        private RevitDocumentProperties selectedModel;
        private MassConfiguration massConfig;
        public Dictionary<string, RevitDocumentProperties> modelDictionary;
        public Dictionary<string, AreaProperties> AreaDictionary { get; set; }
        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, Object value);

        public AreaWindow(UIApplication uiapp, Dictionary<string, RevitDocumentProperties> models, Dictionary<string, AreaProperties> dictionary)
        {
            app = uiapp;
            doc = app.ActiveUIDocument.Document;
            modelDictionary = models;
            AreaDictionary = dictionary;

            InitializeComponent();

            Title = "Create Mass v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var documents = modelDictionary.Values.OrderBy(o => o.DocumentTitle).ToList();
            comboBoxModel.ItemsSource = documents;

            //area elements only in host doc
            var index = documents.FindIndex(o => o.IsLinked == false); 
            if (index > -1)
            {
                comboBoxModel.SelectedIndex = index;
            }

            massConfig = MassConfigDataStorageUtil.GetMassConfiguration(doc, SourceType.Areas);
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
            var selectedAreas = AreaDictionary.Values.Where(x => x.AreaDocument == selectedModel.DocumentTitle).ToList();
            if (!selectedAreas.Any()) return;

            var areaList = selectedAreas.OrderBy(o => o.AreaNumber).ToList();
            dataGridArea.ItemsSource = null;
            dataGridArea.ItemsSource = areaList;
        }

        private void comboBoxModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null == comboBoxModel.SelectedItem) return;

            selectedModel = (RevitDocumentProperties)comboBoxModel.SelectedItem;
            DisplayAreaInfo();
        }

        private void menuItemCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var selectedItem in dataGridArea.SelectedItems)
                {
                    var selectedArea = (AreaProperties)selectedItem;
                    if (AreaDictionary.ContainsKey(selectedArea.AreaUniqueId))
                    {
                        var ap = AreaDictionary[selectedArea.AreaUniqueId];
                        ap.IsSelected = true;
                        AreaDictionary.Remove(ap.AreaUniqueId);
                        AreaDictionary.Add(ap.AreaUniqueId, ap);
                    }
                }
                DisplayAreaInfo();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to check selected items.\n" + ex.Message, "Check Selected Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemUncheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var selectedItem in dataGridArea.SelectedItems)
                {
                    var selectedArea = (AreaProperties)selectedItem;
                    if (!AreaDictionary.ContainsKey(selectedArea.AreaUniqueId)) continue;

                    var ap = AreaDictionary[selectedArea.AreaUniqueId];
                    ap.IsSelected = false;
                    AreaDictionary.Remove(ap.AreaUniqueId);
                    AreaDictionary.Add(ap.AreaUniqueId, ap);
                }
                DisplayAreaInfo();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to check selected items.\n" + ex.Message, "Check Selected Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIds = new List<ElementId>();
                foreach (var selectedItem in dataGridArea.SelectedItems)
                {
                    var selectedArea = (AreaProperties)selectedItem;
                    selectedIds.Add(selectedArea.AreaElementId);
                }

                if (!selectedIds.Any()) return;

                using (var trans = new Transaction(doc))
                {
                    trans.Start("Select Elements");
                    try
                    {
                        var uidoc = app.ActiveUIDocument;
                        var selection = uidoc.Selection;
                        uidoc.ShowElements(selectedIds);
                        selection.SetElementIds(selectedIds);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                        trans.RollBack();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to view elements in the background.\n" + ex.Message, "View Selected Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItemMassView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIds = new List<ElementId>();
                foreach (var selectedItem in dataGridArea.SelectedItems)
                {
                    var selectedArea = (AreaProperties)selectedItem;
                    if (null != selectedArea.Linked3dMass)
                    {
                        selectedIds.Add(selectedArea.Linked3dMass.MassElement.Id);
                    }
                }

                if (!selectedIds.Any()) return;

                using (var trans = new Transaction(doc))
                {
                    trans.Start("Select Elements");
                    try
                    {
                        var uidoc = app.ActiveUIDocument;
                        var selection = uidoc.Selection;
                        uidoc.ShowElements(selectedIds);
                        selection.SetElementIds(selectedIds);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                        trans.RollBack();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to view 3d mass elements in the background.\n" + ex.Message, "View Selected Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuItem2dMassView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIds = new List<ElementId>();
                foreach (var selectedItem in dataGridArea.SelectedItems)
                {
                    var selectedArea = (AreaProperties)selectedItem;
                    if (null != selectedArea.Linked2dMass)
                    {
                        selectedIds.Add(selectedArea.Linked2dMass.MassElement.Id);
                    }
                }

                if (!selectedIds.Any()) return;

                using (var trans = new Transaction(doc))
                {
                    trans.Start("Select Elements");
                    try
                    {
                        var uidoc = app.ActiveUIDocument;
                        var selection = uidoc.Selection;
                        uidoc.ShowElements(selectedIds);
                        selection.SetElementIds(selectedIds);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                        trans.RollBack();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to view 2d mass elements in the background.\n" + ex.Message, "View Selected Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                var updatedList = new List<AreaProperties>();
                foreach (var ap in areaList)
                {
                    ap.IsSelected = true;
                    updatedList.Add(ap);
                }

                dataGridArea.ItemsSource = null;
                dataGridArea.ItemsSource = updatedList;
            }
            catch(Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                var updatedList = new List<AreaProperties>();
                foreach (var ap in areaList)
                {
                    ap.IsSelected = false;
                    updatedList.Add(ap);
                }

                dataGridArea.ItemsSource = null;
                dataGridArea.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void buttonCheckDiscrepancy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                var updatedList = new List<AreaProperties>();
                foreach (var ap in areaList)
                {
                    ap.IsSelected = ap.ModifiedHost;
                    updatedList.Add(ap);
                }

                dataGridArea.ItemsSource = null;
                dataGridArea.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void expanderParameter_Collapsed(object sender, RoutedEventArgs e)
        {
            var collapsedLength = new GridLength(30);
            rowDefinitionExpander.Height = collapsedLength;
        }

        private void expanderParameter_Expanded(object sender, RoutedEventArgs e)
        {
            var expandedLength = new GridLength(0.5, GridUnitType.Star);
            rowDefinitionExpander.Height = expandedLength;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var useDefaultHeight = checkBoxDefaultHeight.IsChecked != null && (bool)checkBoxDefaultHeight.IsChecked;
                double defaultHeight = 0;
                if (useDefaultHeight)
                {
                    if (!ValidateInput(out defaultHeight)) return;
                }

                var areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                var selectedAreas = areaList.Where(x => x.IsSelected).ToList();

                if (selectedAreas.Any())
                {
                    statusLable.Text = "Creating 3D Masses ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedAreas.Count();

                    var createdParamMaps = massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0;

                    UpdateProgressBarDelegate updatePbDelegate = progressBar.SetValue;
                    using (var tg = new TransactionGroup(doc))
                    {
                        tg.Start("Create 3D Masses");
                        try
                        {
                            double count = 0;
                            foreach (var ap in selectedAreas)
                            {
                                if (useDefaultHeight)
                                {
                                    ap.UserHeight = defaultHeight;
                                }
                                using (var trans = new Transaction(doc))
                                {
                                    trans.Start("Create 3D Mass");
                                    var options = trans.GetFailureHandlingOptions();
                                    options.SetFailuresPreprocessor(new DuplicateWarningSwallower());
                                    trans.SetFailureHandlingOptions(options);

                                    try
                                    {
                                        if (MassCreator.CreateAreaSolid(doc, ap, out var createdMass))
                                        {
                                            if (AreaDictionary.ContainsKey(ap.AreaUniqueId))
                                            {
                                                var updatedArea = new AreaProperties(ap)
                                                {
                                                    IsSelected = false,
                                                    Linked3d = true,
                                                    Linked3dMass = createdMass,
                                                    ModifiedHost = false,
                                                    ToolTip = "Mass 3D Id: " + createdMass.MassId
                                                };

                                                if (updatedArea.Linked2d && null != updatedArea.Linked2dMass)
                                                {
                                                    updatedArea.ToolTip += "\nMass 2D Id: " + updatedArea.Linked2dMass.MassId;
                                                }

                                                if (createdParamMaps)
                                                {
                                                    var unused = UpdateParameter(ap.AreaElement, createdMass.MassElement);
                                                }
                                                AreaDictionary.Remove(ap.AreaUniqueId);
                                                AreaDictionary.Add(ap.AreaUniqueId, updatedArea);
                                            }
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                                        trans.RollBack();
                                    }
                                }

                                count++;
                                Dispatcher.Invoke(updatePbDelegate,
                                    System.Windows.Threading.DispatcherPriority.Background, ProgressBar.ValueProperty, count);
                            }

                            DisplayAreaInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                            tg.RollBack();
                            MessageBox.Show("Failed to create masses from the selected areas\n" + ex.Message,
                                "Create 3D Masses from Areas", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show("Please select areas to update masses.", "Select Areas", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to create 3d masses from the selected areas.\n" + ex.Message,
                    "Create 3D Masses from Areas", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void button2DCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                var selectedAreas = areaList.Where(x => x.IsSelected).ToList();

                if (selectedAreas.Any())
                {
                    statusLable.Text = "Creating 2D Masses ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedAreas.Count();

                    var createdParamMaps = massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0;

                    var updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (var tg = new TransactionGroup(doc))
                    {
                        tg.Start("Create 2D Masses");
                        try
                        {
                            double count = 0;
                            foreach (var ap in selectedAreas)
                            {
                                using (var trans = new Transaction(doc))
                                {
                                    trans.Start("Create 2D Mass");
                                    var options = trans.GetFailureHandlingOptions();
                                    options.SetFailuresPreprocessor(new DuplicateWarningSwallower());
                                    trans.SetFailureHandlingOptions(options);

                                    try
                                    {
                                        if (MassCreator.CreateAreaFace(doc, ap, out var createdMass))
                                        {
                                            if (AreaDictionary.ContainsKey(ap.AreaUniqueId))
                                            {
                                                var updatedArea = new AreaProperties(ap)
                                                {
                                                    IsSelected = false,
                                                    Linked2d = true,
                                                    Linked2dMass = createdMass,
                                                    ModifiedHost = false
                                                };

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
                                                    var unused = UpdateParameter(ap.AreaElement, createdMass.MassElement);
                                                }
                                                AreaDictionary.Remove(ap.AreaUniqueId);
                                                AreaDictionary.Add(ap.AreaUniqueId, updatedArea);
                                            }
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                                        trans.RollBack();
                                    }
                                }

                                count++;
                                Dispatcher.Invoke(updatePbDelegate,
                                    System.Windows.Threading.DispatcherPriority.Background, ProgressBar.ValueProperty,
                                    count);
                            }

                            DisplayAreaInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                            tg.RollBack();
                            MessageBox.Show("Failed to create 2d masses from the selected areas\n" + ex.Message,
                                "Create 2D Masses from Areas", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show("Please select areas to update masses.", "Select Areas", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to create 2d masses from the selected areas.\n" + ex.Message,
                    "Create 2D Masses from Areas", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonParametersSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var paramWindow = new MassParameterWindow(app, massConfig);
                if (!(bool) paramWindow.ShowDialog()) return;

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
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to set linked parameters.\n" + ex.Message, "Linked Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUpdateParameters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (massConfig.UpdateType != ParameterUpdateType.None && massConfig.MassParameters.Count > 0)
                {
                    var areaList = (List<AreaProperties>)dataGridArea.ItemsSource;
                    var selectedAreas = areaList.Where(x => x.IsSelected).ToList();

                    if (!selectedAreas.Any()) return;

                    statusLable.Text = "Updating Parameters ...";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedAreas.Count();

                    var updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (var tg = new TransactionGroup(doc))
                    {
                        tg.Start("Update Parameters");
                        try
                        {
                            double count = 0;
                            foreach (var ap in selectedAreas)
                            {
                                using (var trans = new Transaction(doc))
                                {
                                    trans.Start("Update Parameter");
                                    var options = trans.GetFailureHandlingOptions();
                                    options.SetFailuresPreprocessor(new DuplicateWarningSwallower());
                                    trans.SetFailureHandlingOptions(options);

                                    try
                                    {
                                        var updated = false;
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
                                            var updatedArea = new AreaProperties(ap)
                                            {
                                                IsSelected = false
                                            };
                                            AreaDictionary.Remove(ap.AreaUniqueId);
                                            AreaDictionary.Add(ap.AreaUniqueId, updatedArea);
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                                        trans.RollBack();
                                    }
                                }

                                count++;
                                Dispatcher.Invoke(updatePbDelegate,
                                    System.Windows.Threading.DispatcherPriority.Background, ProgressBar.ValueProperty,
                                    count);
                            }
                            DisplayAreaInfo();
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                            tg.RollBack();
                            MessageBox.Show("Failed to update parameters of linked masses.\n" + ex.Message,
                                "Update Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
                else
                {
                    MessageBox.Show(
                        "Please set the configuration for the parameter mappings.\nGo to the Parameters Settings button for more detail.",
                        "Parameters Settings Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to update parameter values.\n" + ex.Message, "Update Parameters",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region Utilities

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heightValue"></param>
        /// <returns></returns>
        private bool ValidateInput(out double heightValue)
        {
            var valid = false;
            heightValue = 0;
            try
            {
                var heightText = textBoxHeight.Text;
                if (double.TryParse(heightText, out heightValue))
                {
                    if (heightValue > 0)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                {
                    MessageBox.Show("Please enter a valid value for the height of masses to be created.",
                        "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to validate user inputs.\n" + ex.Message, "Validate Input", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            return valid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostElement"></param>
        /// <param name="massElement"></param>
        /// <returns></returns>
        private bool UpdateParameter(Element hostElement, Element massElement)
        {
            var updated = true;
            try
            {
                var sourceElement = hostElement;
                var recipientElement = massElement;
                var reversed = massConfig.UpdateType == ParameterUpdateType.FromMassToHost;

                if (reversed)
                {
                    sourceElement = massElement;
                    recipientElement = hostElement;
                }

                foreach (var mapInfo in massConfig.MassParameters)
                {
                    try
                    {
                        var sourceInfo = mapInfo.HostParamInfo;
                        var recipientInfo = mapInfo.MassParamInfo;
                        if (reversed)
                        {
                            sourceInfo = mapInfo.MassParamInfo;
                            recipientInfo = mapInfo.HostParamInfo;
                        }

                        var sourceParam = sourceElement.LookupParameter(sourceInfo.ParameterName);
                        var recipientParam = recipientElement.LookupParameter(recipientInfo.ParameterName);
                        if (null == sourceParam || null == recipientParam) continue;

                        if (sourceParam.StorageType != recipientParam.StorageType || recipientParam.IsReadOnly)
                            continue;

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
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                        updated = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to update parameter.\n" + ex.Message, "Update Parameter", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                updated = false;
            }
            return updated;
        }

        #endregion
    }

    /// <summary>
    /// Class that helps swallow warnings about duplicate Mark values.
    /// </summary>
    public class DuplicateWarningSwallower : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor fa)
        {
            var failures = fa.GetFailureMessages();
            foreach (var f in failures)
            {
                var failId = f.GetFailureDefinitionId();
                if (failId == BuiltInFailures.OverlapFailures.DuplicateInstances ||
                    failId == BuiltInFailures.GeneralFailures.DuplicateValue)
                {
                    fa.DeleteWarning(f);
                }
            }
            return FailureProcessingResult.Continue;
        }
    }
}
