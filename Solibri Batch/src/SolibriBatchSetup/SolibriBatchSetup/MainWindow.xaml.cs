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
                dataGridTasks.ItemsSource = null;
                dataGridTasks.ItemsSource = processUnits;

                statusLable.Text = System.IO.Path.GetFileNameWithoutExtension(batch.FilePath);

                if (processUnits.Count > 0)
                {
                    dataGridTasks.SelectedIndex = 0;
                }
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
                settings.SolibriSetup = settings.SolibriOptions.Find(o => o.VersionNumber == Properties.Settings.Default.SolibriVersionNumber);
                settings.RemoteSetup = settings.RemoteOptions.Find(o => o.ComputerName == Properties.Settings.Default.ComputerName);
                foreach (string classification in Properties.Settings.Default.Classifications)
                {
                    settings.Classifications.Add(new OpenClassification(classification));
                }
                foreach (string ruleset in Properties.Settings.Default.Rulesets)
                {
                    settings.Rulesets.Add(new OpenRuleset(ruleset));
                }
                settings.ReportSettings = new ReportSettings()
                {
                    SaveInPlace = Properties.Settings.Default.ReportSaveInPlace,
                    OutputFolder = Properties.Settings.Default.ReportOutputFolder,
                    AppendDate = Properties.Settings.Default.ReportAppendDate,
                    IsCheckingSelected = Properties.Settings.Default.ReportCheckingSelected,
                    IsPresentationSelected = Properties.Settings.Default.ReportPresentationSelected,
                    IsBCFSelected = Properties.Settings.Default.ReportBCFSelected,
                    IsCoordinationSelected = Properties.Settings.Default.ReportCoordinationSelected,
                    CoordinationTemplate = Properties.Settings.Default.ReportTemplate
                };
                settings.SaveSolibriSettings = new SaveModelSettings()
                {
                    SaveInPlace = Properties.Settings.Default.SolibriSaveInPlace,
                    OutputFolder = Properties.Settings.Default.SolibriOutputFolder,
                    AppendDate = Properties.Settings.Default.SolibriAppendDate
                };
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
                        DisplayConfig();
                        statusLable.Text = "Ready";
                        isEditing = false;
                    }
                }
                else
                {
                    batch = new Batch();
                    processUnits = BatchSetupUtils.ExtractProcessUnits(batch);
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
                        DisplayConfig();
                        isEditing = false;
                    }
                }

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
                if (!IsValidSettings()) { return; }

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
                    batch.Target.Elements = BatchSetupUtils.ConvertToGenericElements(processUnits);
                    bool savedConfig = BatchSetupUtils.SaveBatchConfig(batch);
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

        private bool IsValidSettings()
        {
            bool valid = false;
            try
            {
                foreach (ProcessUnit unit in processUnits)
                {
                    //Open Solibri
                    if (string.IsNullOrEmpty(unit.OpenSolibri.File) && unit.Models.Count == 0)
                    {
                        MessageBox.Show("Task Name: " + unit.TaskName + "/nEnter valid input models.", "Input Models Missing", MessageBoxButton.OK, MessageBoxImage.Information); 
                        return valid;
                    }
                    //Reporting
                    
                    //Saving Solibri
                }
                
                valid = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return valid;
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
                    batch.Target.Elements = BatchSetupUtils.ConvertToGenericElements(processUnits);
                    bool savedConfig = BatchSetupUtils.SaveBatchConfig(batch);
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
                        bool applied = ApplySettings(i);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        #region Add or Remove Items
        private void buttonAddTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateTaskWindow taskWindow = new CreateTaskWindow();
                taskWindow.Owner = this;
                if ((bool)taskWindow.ShowDialog())
                {
                    ProcessUnit unit = taskWindow.Task;
                    processUnits.Add(unit);
                    dataGridTasks.SelectedIndex = processUnits.IndexOf(unit);
                    ApplySettings(dataGridTasks.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedItems.Count > 0)
                {
                    for (int i = dataGridTasks.SelectedItems.Count - 1; i > -1; i--)
                    {
                        ProcessUnit unit = dataGridTasks.SelectedItems[i] as ProcessUnit;
                        processUnits.Remove(unit);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonOpenSolibri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "Solibri Model Checker Files (*.smc)|*.smc";
                    openDialog.Title = "Open a Solibri Model";
                    openDialog.Multiselect = false;
                    if (openDialog.ShowDialog() == true)
                    {
                        string fileName = openDialog.FileName;
                        OpenModel openModel = new OpenModel(fileName);
                        processUnits[dataGridTasks.SelectedIndex].OpenSolibri = openModel;
                        ApplySettings(dataGridTasks.SelectedIndex);
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open solibri files.\n" + ex.Message, "Open Solibri Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int taskIndex = dataGridTasks.SelectedIndex;
                    for (int i = 0; i < processUnits[taskIndex].Models.Count; i++)
                    {
                        processUnits[taskIndex].Models[i].IsUpdate = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int taskIndex = dataGridTasks.SelectedIndex;
                    for (int i = 0; i < processUnits[taskIndex].Models.Count; i++)
                    {
                        processUnits[taskIndex].Models[i].IsUpdate = false;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonAddIFC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "Model (*.ifc, *.zip, *.dwg, *.ifczip)|*.ifc;*.zip;*.dwg;*.ifczip";
                    openDialog.Title = "Add Models to Open";
                    openDialog.Multiselect = true;
                    if (openDialog.ShowDialog() == true)
                    {
                        foreach (string fileName in openDialog.FileNames)
                        {
                            processUnits[index].Models.Add(new InputModel(fileName));
                        }
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
                if (dataGridTasks.SelectedIndex > -1 && dataGridIfc.SelectedItems.Count > 0)
                {
                    int taskIndex = dataGridTasks.SelectedIndex;
                    for (int i = dataGridIfc.SelectedItems.Count - 1; i > -1; i--)
                    {
                        InputModel model = dataGridIfc.SelectedItems[i] as InputModel;
                        processUnits[taskIndex].Models.Remove(model);
                    }
                    isEditing = true;

                    var updateItems = from item in processUnits[taskIndex].Models where !string.IsNullOrEmpty(item.With) select item;
                    if (updateItems.Count() > 0)
                    {
                        checkBoxUpdate.IsEnabled = true;
                    }
                    else
                    {
                        checkBoxUpdate.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete IFC items.\n"+ex.Message, "Delete IFC Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonWith_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridRow row = DataGridUtils.FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);
                if (null != row)
                {
                    InputModel model = row.Item as InputModel;
                    if (null != model)
                    {
                        OpenFileDialog openDialog = new OpenFileDialog();
                        openDialog.Title = "Open a Model to Update with";
                        openDialog.Filter = "Model (*.ifc, *.zip, *.dwg, *.ifczip)|*.ifc;*.zip;*.dwg;*.ifczip";
                        openDialog.Multiselect = false;
                        if (openDialog.ShowDialog() == true)
                        {
                            string fileName = openDialog.FileName;

                            int taskIndex = dataGridTasks.SelectedIndex;
                            int fileIndex = processUnits[taskIndex].Models.IndexOf(model);

                            processUnits[taskIndex].Models[fileIndex].IsUpdate = true;
                            processUnits[taskIndex].Models[fileIndex].With = fileName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonAddClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "CLASSIFICATION File (*.classification)|*.classification";
                    openDialog.Title = "Add Classification Files";
                    openDialog.Multiselect = true;
                    if (openDialog.ShowDialog() == true)
                    {
                        foreach (string fileName in openDialog.FileNames)
                        {
                            processUnits[index].Classifications.Add(new OpenClassification(fileName));
                        }
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDeleteClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1 && dataGridClassification.SelectedItems.Count > 0)
                {
                    int taskIndex = dataGridTasks.SelectedIndex;
                    for (int i = dataGridClassification.SelectedItems.Count - 1; i > -1; i--)
                    {
                        OpenClassification classification = dataGridClassification.SelectedItems[i] as OpenClassification;
                        processUnits[taskIndex].Classifications.Remove(classification);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonImportClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (settings.Classifications.Count > 0)
                {
                    int index = dataGridTasks.SelectedIndex;
                    processUnits[index].Classifications.Clear();
                    processUnits[index].Classifications = settings.Classifications;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonAddRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "CSET File (*.cset)|*.cset";
                    openDialog.Title = "Add Ruleset Files";
                    openDialog.Multiselect = true;
                    if (openDialog.ShowDialog() == true)
                    {
                        foreach (string fileName in openDialog.FileNames)
                        {
                            processUnits[index].Rulesets.Add(new OpenRuleset(fileName));
                        }
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDeleteRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (dataGridTasks.SelectedIndex > -1 && dataGridRuleset.SelectedItems.Count > 0)
                {
                    int taskIndex = dataGridTasks.SelectedIndex;
                    for (int i = dataGridRuleset.SelectedItems.Count - 1; i > -1; i--)
                    {
                        OpenRuleset ruleset = dataGridRuleset.SelectedItems[i] as OpenRuleset;
                        processUnits[taskIndex].Rulesets.Remove(ruleset);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonImportRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (settings.Rulesets.Count > 0)
                {
                    int index = dataGridTasks.SelectedIndex;
                    processUnits[index].Rulesets.Clear();
                    processUnits[index].Rulesets = settings.Rulesets;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void checkBoxCreate_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)checkBoxCreate.IsChecked)
                {
                    checkBoxUpdate.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void checkBoxUpdate_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)checkBoxUpdate.IsChecked)
                {
                    int index = dataGridTasks.SelectedIndex;
                    if (index > -1)
                    {
                        var updateModels = from model in processUnits[index].Models where model.IsUpdate select model;
                        if (updateModels.Count() == 0)
                        {
                            checkBoxUpdate.IsChecked = false;
                            MessageBox.Show("Update models should be defined to run this option.", "Invalid Option Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            checkBoxCreate.IsChecked = false;
                        }
                    }
                    else
                    {
                        checkBoxUpdate.IsChecked = false;
                    }
                }
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckingPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "PDF File (*.pdf)|*.pdf";
                    saveDialog.Title = "Save a PDF File";
                    if (saveDialog.ShowDialog() == true)
                    {
                        processUnits[index].CheckingReport.PdfFile = saveDialog.FileName;
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckingRtf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "Rich Text Format (*.rtf)|*.rtf";
                    saveDialog.Title = "Save a Rich Text Format";
                    if (saveDialog.ShowDialog() == true)
                    {
                        processUnits[index].CheckingReport.RtfFile = saveDialog.FileName;
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "PDF File (*.pdf)|*.pdf";
                    saveDialog.Title = "Save a PDF File";
                    if (saveDialog.ShowDialog() == true)
                    {
                        processUnits[index].PresentationReport.PdfFile = saveDialog.FileName;
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonRtf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "Rich Text Format (*.rtf)|*.rtf";
                    saveDialog.Title = "Save a Rich Text Format";
                    if (saveDialog.ShowDialog() == true)
                    {
                        processUnits[index].PresentationReport.RtfFile = saveDialog.FileName;
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonBcf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "BCF File (*.bfczip)|*.bcfzip";
                    saveDialog.Title = "Save a BCF File";
                    if (saveDialog.ShowDialog() == true)
                    {
                        processUnits[index].BCFReport.File = saveDialog.FileName;
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "Microsoft Excel 97-2003 Worksheet (*.xls)|*.xls";
                    saveDialog.Title = "Save an Coordination Report";
                    if (saveDialog.ShowDialog() == true)
                    {
                        processUnits[index].CoordReport.File = saveDialog.FileName;
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "Microsoft Excel 97-2003 Worksheet (*.xls)|*.xls";
                    openDialog.Title = "Open a Template File";
                    if (openDialog.ShowDialog() == true)
                    {
                        processUnits[index].CoordReport.TemplateFile = openDialog.FileName;
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonSaveSolibri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridTasks.SelectedIndex > -1)
                {
                    int index = dataGridTasks.SelectedIndex;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "Solibri Model Checker File (*.smc)|*.smc";
                    saveDialog.Title = "Save a Solibri Model Checker File";
                    if (saveDialog.ShowDialog() == true)
                    {
                        string fileName = saveDialog.FileName;
                        processUnits[index].SaveSolibri.File = saveDialog.FileName;
                        processUnits[index].TaskName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                        isEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        #endregion

        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    int unitIndex = dataGridTasks.SelectedIndex;
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    string xmlFile = "";
                    List<string> solibriFiles =new List<string>();
                    List<string> ifcFiles = new List<string>();
                    List<string> rulesetFiles = new List<string>();
                    
                    foreach (string file in files)
                    {
                        string extension = System.IO.Path.GetExtension(file).ToLower();
                        switch (extension)
                        {
                            case ".smc":
                                solibriFiles.Add(file);
                                break;
                            case ".xml":
                                xmlFile = file;
                                break;
                            case ".ifc":
                                ifcFiles.Add(file);
                                break;
                            case ".zip":
                                ifcFiles.Add(file);
                                break;
                            case ".dwg":
                                ifcFiles.Add(file);
                                break;
                            case ".ifczip":
                                ifcFiles.Add(file);
                                break;
                            case ".cset":
                                rulesetFiles.Add(file);
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(xmlFile))
                    {
                        bool read = BatchSetupUtils.ReadBatchConfig(xmlFile, out batch);
                        if (read)
                        {
                            processUnits = BatchSetupUtils.ExtractProcessUnits(batch);
                            DisplayConfig();
                        }
                    }
                    else if(unitIndex > -1)
                    {
                        if (solibriFiles.Count > 0)
                        {
                            processUnits[unitIndex].OpenSolibri = new OpenModel(solibriFiles.First());
                            isEditing = true;
                        }
                        
                        if (rulesetFiles.Count > 0 )
                        {
                            foreach (string ruleset in rulesetFiles)
                            {
                                processUnits[unitIndex].Rulesets.Add(new OpenRuleset(ruleset));
                            }
                            isEditing = true;
                        }
                        if (ifcFiles.Count > 0)
                        {
                            foreach (string ifcFile in ifcFiles)
                            {
                                processUnits[unitIndex].Models.Add(new InputModel(ifcFile));
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

        private bool ApplySettings(int selectedIndex)
        {
            bool applied = false;
            try
            {

                string taskName = processUnits[selectedIndex].TaskName;
                string taskDirectory = processUnits[selectedIndex].TaskDirectory;
                string dateSuffix = DateTime.Now.ToString("yyyy-MM-dd");

                if (!Directory.Exists(taskDirectory) || string.IsNullOrEmpty(taskName)) { return applied; }


                //Reports
                ReportSettings rSetting = settings.ReportSettings;
                string rDirectory = (rSetting.SaveInPlace) ? taskDirectory : rSetting.OutputFolder;
                if (Directory.Exists(rDirectory))
                {
                    if (rSetting.IsCheckingSelected)
                    {
                        string checkingFileName = (rSetting.AppendDate) ? (taskName + "-checking-" + dateSuffix) : (taskName + "-checking");
                        processUnits[selectedIndex].CheckingReport.PdfFile = System.IO.Path.Combine(rDirectory, checkingFileName + ".pdf");
                        processUnits[selectedIndex].CheckingReport.RtfFile = System.IO.Path.Combine(rDirectory, checkingFileName + ".rtf");
                    }
                    if (rSetting.IsPresentationSelected)
                    {
                        string presentationFileName = (rSetting.AppendDate) ? (taskName + "-presentation-" + dateSuffix) : (taskName + "-presentation");
                        processUnits[selectedIndex].PresentationReport.PdfFile = System.IO.Path.Combine(rDirectory, presentationFileName + ".pdf");
                        processUnits[selectedIndex].PresentationReport.RtfFile = System.IO.Path.Combine(rDirectory, presentationFileName + ".rtf");
                    }
                    if (rSetting.IsBCFSelected)
                    {
                        string bcfFileName = (rSetting.AppendDate) ? (taskName + "-" + dateSuffix + ".bcfzip") : (taskName + ".bcfzip");
                        processUnits[selectedIndex].BCFReport.File = System.IO.Path.Combine(rDirectory, bcfFileName);
                    }
                    if (rSetting.IsCoordinationSelected)
                    {
                        string coordFileName = (rSetting.AppendDate) ? (taskName + "-" + dateSuffix + ".xls") : (taskName + ".xls");
                        processUnits[selectedIndex].CoordReport.File = System.IO.Path.Combine(rDirectory, coordFileName);

                        if (File.Exists(rSetting.CoordinationTemplate))
                        {
                            processUnits[selectedIndex].CoordReport.TemplateFile = rSetting.CoordinationTemplate;
                        }
                    }
                }
               

                //Saving Solibri
                SaveModelSettings sSetting = settings.SaveSolibriSettings;
                string sDirectory = (sSetting.SaveInPlace) ? taskDirectory : sSetting.OutputFolder;
                if (Directory.Exists(sDirectory))
                {
                    string saveFileName = (sSetting.AppendDate) ? (taskName + "-" + dateSuffix + ".smc") : (taskName + ".smc");
                    processUnits[selectedIndex].SaveSolibri.File = System.IO.Path.Combine(sDirectory, saveFileName);
                }

                applied = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply settings.\n" + ex.Message, "Apply Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return applied;
        }

    }
}
