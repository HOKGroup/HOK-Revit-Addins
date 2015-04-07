using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace HOK.BatchExporter
{
    /// <summary>
    /// Interaction logic for ProjectWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        //private string configFilePath = "";
        private ProjectSettings projectSettings = new ProjectSettings();
        private bool isLoadedConfig = false;
        private string batchDirectory = @"V:\HOK-Tools\BatchUpgrader\Configuration";
        private bool isEdit = false;

        public ProjectSettings CollectedProjectSettings { get { return projectSettings; } set { value = projectSettings; } }
        public bool IsLoadedConfig { get { return isLoadedConfig; } set { value = isLoadedConfig; } }
        public bool IsEdit { get { return isEdit; } set { value = isEdit; } }

        public ProjectWindow(bool editMode)
        {
            isEdit = editMode;
            if (!Directory.Exists(batchDirectory))
            {
                try
                {
                    Directory.CreateDirectory(batchDirectory);
                }
                catch { }
            }
            InitializeComponent();
            FillOfficeLocation();
        }

        public ProjectWindow(ProjectSettings ps, bool editMode)
        {
            projectSettings = ps;
            isEdit = editMode;
            InitializeComponent();
            FillOfficeLocation();
            if (isEdit)
            {
                textBoxProjectName.Text = projectSettings.ProjectName;
                this.Title = "Edit Project";
                buttonAdd.Content = "Edit";
            }
        }

        private void FillOfficeLocation()
        {
            string[] offices = Enum.GetNames(typeof(OfficeLocation));
            foreach (string officeName in offices)
            {
                comboBoxOffice.Items.Add(officeName);
            }

            if (isEdit && offices.Contains(projectSettings.Office))
            {
                for (int index = 0; index < comboBoxOffice.Items.Count; index++)
                {
                    if (offices[index] == projectSettings.Office)
                    {
                        comboBoxOffice.SelectedIndex = index;
                        break;
                    }
                }
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isLoadedConfig = false;
                if (!string.IsNullOrEmpty(textBoxProjectName.Text) && null != comboBoxOffice.SelectedItem)
                {
                    projectSettings.ProjectName = textBoxProjectName.Text;
                    projectSettings.Office = comboBoxOffice.SelectedItem.ToString();

                    if (isEdit)
                    {
                        if (File.Exists(projectSettings.ConfigFileName))
                        {
                            string directoryName = System.IO.Path.GetDirectoryName(projectSettings.ConfigFileName);
                            string fileName = "BatchUpgrader_" + projectSettings.Office + "_" + projectSettings.ProjectName+".xml";
                            string path=System.IO.Path.Combine(directoryName, fileName);
                            File.Move(projectSettings.ConfigFileName, path);
                            projectSettings.ConfigFileName = path;
                            this.DialogResult = true;
                        }
                    }
                    else
                    {
                        projectSettings = SetDefaultSettings(projectSettings);
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        if (Directory.Exists(batchDirectory))
                        {
                            saveFileDialog.InitialDirectory = batchDirectory;
                        }
                        saveFileDialog.FileName = "BatchUpgrader_" + projectSettings.Office + "_" + projectSettings.ProjectName;
                        saveFileDialog.DefaultExt = ".xml";
                        saveFileDialog.Filter = "Configuration File (.xml)|*.xml";
                        Nullable<bool> result = saveFileDialog.ShowDialog();
                        if (result == true)
                        {
                            projectSettings.ConfigFileName = saveFileDialog.FileName;
                            this.DialogResult = true;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a project name and select an office.", "Missing Information", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot add the project.\n" + ex.Message, "buttonAdd_Click", MessageBoxButton.OK);
            }
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isLoadedConfig = true;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = batchDirectory;
                openFileDialog.DefaultExt = ".xml";
                openFileDialog.Filter = "Configuration File (.xml)|*.xml";

                Nullable<bool> result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    string configFile = openFileDialog.FileName;
                    if (File.Exists(configFile))
                    {
                        BatchSettings batchSettings = new BatchSettings();
                        bool isReadable = false;
                        projectSettings = batchSettings.ReadProjectSettings(configFile, out isReadable);
                        if (isReadable)
                        {
                            projectSettings.UpgradeOptions.TaskScheduleSettings.TaskName = Guid.NewGuid().ToString();
                            projectSettings.ConfigFileName = configFile;
                            this.DialogResult = true;
                        }
                        else
                        {
                            MessageBox.Show("This tool cannot read the configuration file created from previous versions.\n Please create a new project using the current version.",
                               "Version Mismatch", MessageBoxButton.OK);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load the exisitng project file." + ex.Message, "buttonLoad_Click", MessageBoxButton.OK);
            }
        }

        private ProjectSettings SetDefaultSettings(ProjectSettings pSettings)
        {
            ProjectSettings ps = pSettings;
            try
            {
                #region Upgrade
                UpgradeOptions upgradeOptions = new UpgradeOptions();
                upgradeOptions.UpgradeVersion = "2014";
                upgradeOptions.IsFinalUpgrade = false;
                upgradeOptions.ConfigurationName = "<In-Session Setup>";

                ScheduleSettings upgradeScheduleSettings = new ScheduleSettings();
                upgradeScheduleSettings.TaskName = Guid.NewGuid().ToString();
                upgradeScheduleSettings.StartTime = DateTime.Now;
                upgradeScheduleSettings.DaysOfWeek = 1;
                upgradeScheduleSettings.IsWeeklyTrigger = false;
                upgradeOptions.TaskScheduleSettings = upgradeScheduleSettings;
                
                OpenOptions openOptions = new OpenOptions();
                openOptions.Audit = true;
                openOptions.DetachAndPreserveWorksets = true;
                openOptions.OpenAllWorkset = false;

                upgradeOptions.UpgradeVersionOpenOptions = openOptions;

                SaveAsOptions saveOptions = new SaveAsOptions();
                saveOptions.NumOfBackups = 5;
                saveOptions.MakeCentral = true;
                saveOptions.Relinquish = false;
                saveOptions.WorksetConfiguration = "AskUserToSpecify";
                saveOptions.ReviewLocation = "";
                saveOptions.LogLocation = @"V:\HOK-Tools\BatchUpgrader\Logs";

                upgradeOptions.UpgradeVersionSaveAsOptions = saveOptions;

                ps.UpgradeOptions = upgradeOptions;
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot set values with default settings.\n" + ex.Message, "Set Default Settings", MessageBoxButton.OK);
            }
            return ps;
        }

    }

    public enum OfficeLocation
    {
        ATL,
        BEI,
        CAL,
        CHI,
        DAL,
        DEN,
        DOH,
        DUB,
        HK,
        HOU,
        LA,
        LON,
        MIA,
        NY,
        OTT,
        SEA,
        SF,
        SH,
        STL,
        TOR,
        TPA,
        VAN,
        WDC,
        WPP
    }
}
