using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using SolibriBatchSetup.Schema;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Batch batch= new Batch();
        private ObservableCollection<ProcessUnit> processUnits = new ObservableCollection<ProcessUnit>();
        private ObservableCollection<OpenRuleset> commonRuleSets = new ObservableCollection<OpenRuleset>();

        private AutorunSettings settings = new AutorunSettings();
        private bool isEditing = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Solibri Batch Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            DisplayConfig();
            LoadUserSettings();
        }

        private void DisplayConfig()
        {
            try
            {
                dataGridOpenSolibri.ItemsSource = null;
                dataGridOpenSolibri.ItemsSource = processUnits;
                if (dataGridOpenSolibri.Items.Count > 0)
                {
                    dataGridOpenSolibri.SelectedIndex = 0;
                }

                dataGridRuleset.ItemsSource = null;
                dataGridRuleset.ItemsSource = commonRuleSets;

                dataGridBCF.ItemsSource = null;
                dataGridBCF.ItemsSource = processUnits;

                dataGridSaveSolibri.ItemsSource = null;
                dataGridSaveSolibri.ItemsSource = processUnits;

                statusLable.Text = System.IO.Path.GetFileNameWithoutExtension(batch.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display configuration file.\n" + ex.Message, "Display Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadUserSettings()
        {
            try
            {
                settings.SolibriSetup = new SolibriProperties(Properties.Settings.Default.SolibriVersionNumber, Properties.Settings.Default.SolibriExe);
                settings.RemoteSetup = new RempoteMachine(Properties.Settings.Default.ComputerLocation, Properties.Settings.Default.ComputerName, Properties.Settings.Default.DirectoryName);
                settings.SaveSolibriSettings = new SaveModelSettings(BatchFileType.Solibri, Properties.Settings.Default.SolibriSaveInPlace, Properties.Settings.Default.SolibriOutputFolder, Properties.Settings.Default.SolibriAppendDate);
                settings.SaveBCFSettings = new SaveModelSettings(BatchFileType.BCF, Properties.Settings.Default.BCFSaveInPlace, Properties.Settings.Default.BCFOutputFolder, Properties.Settings.Default.BCFAppendDate);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
       
        #region Menu Items
        private void NewConfig(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (isEditing)
                {
                    MessageBoxResult result = MessageBox.Show("Would you like to save the curent configuration file?", "Save File", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
                    {
                        if (result == MessageBoxResult.Yes)
                        {
                            SaveConfig();
                        }
                        batch = new Batch();
                        processUnits = BatchSetupUtils.ExtractProcessUnits(batch);
                        commonRuleSets = BatchSetupUtils.ExtractRulesets(batch);
                        DisplayConfig();
                        statusLable.Text = "Ready";
                        isEditing = false;
                    }
                }
                else
                {
                    batch = new Batch();
                    processUnits = BatchSetupUtils.ExtractProcessUnits(batch);
                    commonRuleSets = BatchSetupUtils.ExtractRulesets(batch);
                    DisplayConfig();
                    statusLable.Text = "Ready";
                    isEditing = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void OpenConfig(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Batch Configuration File (*.xml)|*.xml";
                openDialog.Title = "Open Batch Configuration File";
                openDialog.InitialDirectory = settings.RemoteSetup.DirectoryName;
                openDialog.Multiselect = false;
                if (openDialog.ShowDialog() == true)
                {
                    string xmlFile = openDialog.FileName;
                    bool read = BatchSetupUtils.ReadBatchConfig(xmlFile, out batch);
                    if (read)
                    {
                        processUnits = BatchSetupUtils.ExtractProcessUnits(batch);
                        commonRuleSets = BatchSetupUtils.ExtractRulesets(batch);
                        DisplayConfig();
                        isEditing = false;
                    }
                }

                /*
                OpenFileWindow openWindow = new OpenFileWindow(settings.RemoteSetup.DirectoryName);
                openWindow.Owner = this;
                if (openWindow.ShowDialog() == true)
                {
                    string xmlFile = openWindow.ConfigFileName;
                    bool read = BatchSetupUtils.ReadBatchConfig(xmlFile, out batch);
                    if (read)
                    {
                        processUnits = BatchSetupUtils.ExtractProcessUnits(batch);
                        commonRuleSets = BatchSetupUtils.ExtractRulesets(batch);
                        DisplayConfig();
                        isEditing = false;
                    }
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open configuration file.\n" + ex.Message, "Open Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(batch.FilePath))
                {
                    SaveFileWindow saveWindow = new SaveFileWindow(settings.RemoteSetup.DirectoryName);
                    saveWindow.Owner = this;
                    if (saveWindow.ShowDialog() == true)
                    {
                        batch.FilePath = saveWindow.ConfigFileName;
                    }
                }

                if (!string.IsNullOrEmpty(batch.FilePath))
                {
                    batch.Target.Elements = BatchSetupUtils.ConvertToGenericElements(processUnits, commonRuleSets);
                    bool savedConfig = BatchSetupUtils.SaveBatchConfig(batch.FilePath, batch);
                    bool savedBatch = BatchSetupUtils.CreateBatchFile(batch.FilePath, settings);
                    if (savedConfig && savedBatch)
                    {
                        statusLable.Text = System.IO.Path.GetFileNameWithoutExtension(batch.FilePath);
                        isEditing = false;
                        MessageBox.Show("Batch configuration files are successfully saved!!", "Batch Configuration Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the configuration file.\n" + ex.Message, "Save Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveConfig(object sender, ExecutedRoutedEventArgs e)
        {
            SaveConfig();
        }

        private void SaveAsConfig(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveFileWindow saveWindow = new SaveFileWindow(settings.RemoteSetup.DirectoryName);
                saveWindow.Owner = this;
                if (saveWindow.ShowDialog() == true)
                {
                    batch.FilePath = saveWindow.ConfigFileName;
                    batch.Target.Elements = BatchSetupUtils.ConvertToGenericElements(processUnits, commonRuleSets);
                    bool savedConfig = BatchSetupUtils.SaveBatchConfig(batch.FilePath, batch);
                    bool savedBatch = BatchSetupUtils.CreateBatchFile(batch.FilePath, settings);
                    if (savedConfig && savedBatch)
                    {
                        statusLable.Text = System.IO.Path.GetFileNameWithoutExtension(batch.FilePath);
                        isEditing = false;
                        MessageBox.Show("Batch configuration files are successfully saved!!", "Batch Configuration Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the configuration file in a different location.\n" + ex.Message, "Save As Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void menuOptions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingWindow settingWindow = new SettingWindow(settings);
                settingWindow.Owner = this;
                if (settingWindow.ShowDialog() == true)
                {
                    settings = settingWindow.Settings;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuBug_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Outlook.Application outlookApp = new Outlook.Application();
                Outlook.NameSpace nameSpace = outlookApp.GetNamespace("MAPI");
                Outlook.Folder folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
                Outlook.MailItem mailItem = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

                mailItem.Subject = "Solibri Batch Manager Problem Report";
                mailItem.Body = "**** This email will go to the developer of the application. ****\nPlease attach the configuration file generated or screen captured images of the application.\n";

                mailItem.Recipients.Add("jinsol.kim@hok.com");
                mailItem.Display(false);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                MessageBoxResult result = MessageBox.Show("Would you like to save the curent configuration file?", "Save File", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
                {
                    if (result == MessageBoxResult.Yes)
                    {
                        SaveConfig();
                    }
                }
            }
            this.Close();
        }
        #endregion

        private void expanderIFC_Collapsed(object sender, RoutedEventArgs e)
        {
            expanderIFC.Header = "Show IFC Files";
             GridLength collapsedHeight = new GridLength(40, GridUnitType.Pixel);
             expanderRowDefinition.Height = collapsedHeight;
        }

        private void expanderIFC_Expanded(object sender, RoutedEventArgs e)
        {
            expanderIFC.Header = "Hide IFC Files";
            GridLength expandedHeight = new GridLength(1, GridUnitType.Star);
            expanderRowDefinition.Height = expandedHeight;
        }

        private void dataGridOpenSolibri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dataGridIfc.ItemsSource = null;
                if (null != dataGridOpenSolibri.SelectedItem)
                {
                    ProcessUnit selectedUnit = dataGridOpenSolibri.SelectedItem as ProcessUnit;
                    dataGridIfc.ItemsSource = selectedUnit.IfcFiles;

                    int index = dataGridOpenSolibri.SelectedIndex;
                    dataGridBCF.SelectedIndex = index;
                    dataGridSaveSolibri.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridBCF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int index = dataGridBCF.SelectedIndex;
                dataGridOpenSolibri.SelectedIndex = index;
                dataGridSaveSolibri.SelectedIndex = index;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridSaveSolibri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int index = dataGridSaveSolibri.SelectedIndex;
                dataGridOpenSolibri.SelectedIndex = index;
                dataGridBCF.SelectedIndex = index;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (isEditing)
                {
                    MessageBoxResult result = MessageBox.Show("Would you like to save the curent configuration file?", "Save File", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
                    {
                        if (result == MessageBoxResult.Yes)
                        {
                            SaveConfig();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingWindow settingWindow = new SettingWindow(settings);
                settingWindow.Owner = this;
                if (settingWindow.ShowDialog() == true)
                {
                    settings = settingWindow.Settings;
                    for (int i = processUnits.Count-1; i >-1; i--)
                    {
                        ProcessUnit unit = processUnits[i];
                        unit = ApplySettings(unit);
                        processUnits.RemoveAt(i);
                        processUnits.Insert(i, unit);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        #region Add or Remove Items
        private void buttonAddSolibri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Solibri Model Checker Files (*.smc)|*.smc";
                openDialog.Title = "Open Solibri Models";
                openDialog.Multiselect = true;
                if (openDialog.ShowDialog() == true)
                {
                    string fileName = openDialog.FileName;
                    ProcessUnit unit = new ProcessUnit();
                    unit.OpenSolibri = new OpenModel(fileName);
                    unit = ApplySettings(unit);
                    processUnits.Add(unit);
                    isEditing = true;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add solibri files.\n"+ex.Message, "Add Solibri Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCombineIFC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "IFC files (*.ifc)|*.ifc";
                openFileDialog.RestoreDirectory = false;
                openFileDialog.Title = "Open IFC Files";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == true)
                {
                    if (openFileDialog.FileNames.Length > 0)
                    {
                        ProcessUnit unit = new ProcessUnit();
                        foreach (string fileName in openFileDialog.FileNames)
                        {
                            unit.IfcFiles.Add(new OpenModel(fileName));
                        }

                        IFCWindow ifcWindow = new IFCWindow(unit);
                        if (ifcWindow.ShowDialog() == true)
                        {
                            unit = ifcWindow.Unit;
                            if (commonRuleSets.Count > 0)
                            {
                                //autorunSetting
                                unit = ApplySettings(unit);
                            }
                            processUnits.Add(unit);
                        }
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to combine IFC files.\n"+ex.Message, "Combine IFC files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
       
        private void buttonDeleteSolibri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridOpenSolibri.SelectedItems.Count > 0)
                {
                    for (int i = dataGridOpenSolibri.SelectedItems.Count - 1; i > -1; i--)
                    {
                        ProcessUnit unit = dataGridOpenSolibri.SelectedItems[i] as ProcessUnit;
                        if (null != unit)
                        {
                            processUnits.Remove(unit);
                        }
                    }

                    isEditing = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete Solibri items.\n"+ex.Message, "Delete Solibri Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddIFC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridOpenSolibri.SelectedItem)
                {
                    int index = dataGridOpenSolibri.SelectedIndex;
                    ProcessUnit unit = processUnits[index];

                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "IFC files (*.ifc)|*.ifc";
                    openDialog.Title = "Add IFC Files to be Combined";
                    openDialog.Multiselect = true;
                    if (openDialog.ShowDialog() == true)
                    {
                        foreach (string fileName in openDialog.FileNames)
                        {
                            unit.IfcFiles.Add(new OpenModel(fileName));
                        }

                        processUnits.RemoveAt(index);
                        processUnits.Insert(index, unit);
                        dataGridOpenSolibri.SelectedIndex = index;

                        isEditing = true;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add IFC items.\n"+ex.Message, "Add IFC Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDeleteIFC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridIfc.SelectedItems.Count > 0)
                {
                    int index = dataGridOpenSolibri.SelectedIndex;
                    ProcessUnit unit = processUnits[index];

                    for (int i = dataGridIfc.SelectedItems.Count - 1; i > -1; i--)
                    {
                        OpenModel model = dataGridIfc.SelectedItems[i] as OpenModel;
                        unit.IfcFiles.Remove(model);
                    }

                    processUnits.RemoveAt(index);
                    processUnits.Insert(index, unit);
                    dataGridOpenSolibri.SelectedIndex = index;
                    isEditing = true;
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete IFC items.\n"+ex.Message, "Delete IFC Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Ruleset Files (*.cset)|*.cset";
                openFileDialog.RestoreDirectory = false;
                openFileDialog.Title = "Open Ruleset Files";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    bool updateBCF = false;
                    if (commonRuleSets.Count == 0) { updateBCF = true; }
                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        var existingFound = from rule in commonRuleSets where rule.File == fileName select rule;
                        if (existingFound.Count() > 0) { continue; }
                        commonRuleSets.Add(new OpenRuleset(fileName));
                    }

                    if (updateBCF)
                    {
                        //update bcf path
                        UpdateSettings();
                    }

                    isEditing = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add ruleset files.\n"+ex.Message, "Add Ruleset Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDeleteRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = dataGridRuleset.SelectedItems.Count-1; i >-1; i--)
                {
                    OpenRuleset ruleSet = dataGridRuleset.SelectedItems[i] as OpenRuleset;
                    commonRuleSets.Remove(ruleSet);
                }

                if (commonRuleSets.Count == 0)
                {
                    //remove bcf path
                    UpdateSettings();
                }
                
                isEditing = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete ruleset items.\n"+ex.Message, "Delete Ruleset Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    string xmlFile = "";
                    List<string> solibriFiles =new List<string>();
                    List<string> ifcFiles = new List<string>();
                    List<string> rulesetFiles = new List<string>();
                    foreach (string file in files)
                    {
                        if (System.IO.Path.GetExtension(file).Contains("smc"))
                        {
                            solibriFiles.Add(file);
                        }
                        if (System.IO.Path.GetExtension(file).Contains("xml"))
                        {
                            xmlFile = file; break;
                        }
                        if (System.IO.Path.GetExtension(file).Contains("ifc"))
                        {
                            ifcFiles.Add(file);
                        }
                        if (System.IO.Path.GetExtension(file).Contains("cset"))
                        {
                            rulesetFiles.Add(file);
                        }
                    }

                    if (!string.IsNullOrEmpty(xmlFile))
                    {
                        bool read = BatchSetupUtils.ReadBatchConfig(xmlFile, out batch);
                        if (read)
                        {
                            processUnits = BatchSetupUtils.ExtractProcessUnits(batch);
                            commonRuleSets = BatchSetupUtils.ExtractRulesets(batch);
                            DisplayConfig();
                        }
                    }
                    else
                    {
                        if (solibriFiles.Count > 0)
                        {
                            foreach (string sfile in solibriFiles)
                            {
                                ProcessUnit pUnit = new ProcessUnit();
                                pUnit.OpenSolibri = new OpenModel(sfile);
                                pUnit = ApplySettings(pUnit);
                                processUnits.Add(pUnit);
                            }
                            dataGridOpenSolibri.SelectedIndex = processUnits.Count - 1;
                            isEditing = true;
                        }
                        
                        if (rulesetFiles.Count > 0)
                        {
                            bool updateBCF = false;
                            if (commonRuleSets.Count == 0) { updateBCF = true; }

                            foreach (string ruleset in rulesetFiles)
                            {
                                commonRuleSets.Add(new OpenRuleset(ruleset));
                            }
                            if (updateBCF) { UpdateSettings(); }
                            isEditing = true;
                        }
                        if (ifcFiles.Count > 0)
                        {
                            ProcessUnit pUnit = new ProcessUnit();
                            foreach (string ifcFile in ifcFiles)
                            {
                                pUnit.IfcFiles.Add(new OpenModel(ifcFile));
                            }
                            IFCWindow ifcWindow = new IFCWindow(pUnit);
                            if (ifcWindow.ShowDialog() == true)
                            {
                                pUnit = ifcWindow.Unit;
                                pUnit = ApplySettings(pUnit);
                                processUnits.Add(pUnit);
                                dataGridOpenSolibri.SelectedIndex = processUnits.Count - 1;
                                isEditing = true;
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to drop files.\n" + ex.Message, "File Drop", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private ProcessUnit ApplySettings(ProcessUnit unit)
        {
            ProcessUnit pUnit = unit;
            try
            {
                string solibriFileName = unit.OpenSolibri.File;
                string dateSuffix = DateTime.Now.ToString("yyyy-MM-dd");
                string fileNameOnly = System.IO.Path.GetFileNameWithoutExtension(solibriFileName);
                string smcFileName = System.IO.Path.GetFileName(solibriFileName);
                if (settings.SaveSolibriSettings.SaveInPlace)
                {
                    if (settings.SaveSolibriSettings.AppendDate)
                    {
                        solibriFileName = solibriFileName.Replace(smcFileName, fileNameOnly + "-" + dateSuffix + ".smc");
                    }
                }
                else
                {
                    string folder = settings.SaveSolibriSettings.OutputFolder;
                    if (settings.SaveSolibriSettings.AppendDate)
                    {
                        solibriFileName = System.IO.Path.Combine(folder, fileNameOnly + "-" + dateSuffix + ".smc");
                    }
                    else
                    {
                        solibriFileName = System.IO.Path.Combine(folder, smcFileName);
                    }
                }
                unit.SaveSolibri = new SaveModel(solibriFileName);

                string bcfFileName = "";
                if (commonRuleSets.Count > 0)
                {
                    if (settings.SaveBCFSettings.SaveInPlace)
                    {
                        string directory = System.IO.Path.GetDirectoryName(unit.OpenSolibri.File);
                        if (settings.SaveBCFSettings.AppendDate)
                        {
                            bcfFileName = System.IO.Path.Combine(directory, fileNameOnly + "-" + dateSuffix + ".bcfzip");
                        }
                        else
                        {
                            bcfFileName = System.IO.Path.Combine(directory, fileNameOnly + ".bcfzip");
                        }
                    }
                    else
                    {
                        string folder = settings.SaveBCFSettings.OutputFolder;
                        if (settings.SaveBCFSettings.AppendDate)
                        {
                            bcfFileName = System.IO.Path.Combine(folder, fileNameOnly + "-" + dateSuffix + ".bcfzip");
                        }
                        else
                        {
                            bcfFileName = System.IO.Path.Combine(folder, fileNameOnly + ".bcfzip");
                        }
                    }
                }

                unit.BCFReport = new BCFReport(bcfFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply settings.\n" + ex.Message, "Apply Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return pUnit;
        }

        private void UpdateSettings()
        {
            try
            {
                for (int i = processUnits.Count - 1; i > -1; i--)
                {
                    ProcessUnit unit = processUnits[i];
                    unit = ApplySettings(unit);
                    processUnits.RemoveAt(i);
                    processUnits.Insert(i, unit);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }
}
