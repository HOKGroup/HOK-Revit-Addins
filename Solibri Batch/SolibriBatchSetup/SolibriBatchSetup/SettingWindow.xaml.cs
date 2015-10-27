using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private AutorunSettings settings = new AutorunSettings();
        private List<SolibriProperties> solibries = new List<SolibriProperties>();
        private List<RempoteMachine> remoteComputers = new List<RempoteMachine>();

        public AutorunSettings Settings { get { return settings; } set { settings = value; } }

        public SettingWindow(AutorunSettings autoSetting)
        {
            settings = autoSetting;
            InitializeComponent();
            CollectDefaultSettings();
            DisplaySetting();
        }

        private void CollectDefaultSettings()
        {
            try
            {
                //solibri versions
                SolibriProperties sp = new SolibriProperties("Solibri Model Checker v9.5", @"C:\Program Files\Solibri\SMCv9.5\Solibri Model Checker v9.5.exe");
                solibries.Add(sp);
                sp = new SolibriProperties("Solibri Model Checker v9.6", @"C:\Program Files\Solibri\SMCv9.6\Solibri Model Checker v9.6.exe");
                solibries.Add(sp);
                solibries = solibries.OrderBy(o => o.VersionNumber).ToList();

                comboBoxSolibri.ItemsSource = null;
                comboBoxSolibri.ItemsSource = solibries;

                comboBoxSolibri.DisplayMemberPath = "VersionNumber";

                //remote machines
                RempoteMachine rm = new RempoteMachine("NY", "NY-BAT-D001", @"\\NY-BAT-D001\SolibriBatch");
                remoteComputers.Add(rm);
                remoteComputers = remoteComputers.OrderBy(o => o.ComputerName).ToList();

                comboBoxComputer.ItemsSource = null;
                comboBoxComputer.ItemsSource = remoteComputers;
                comboBoxComputer.DisplayMemberPath = "DirectoryName";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect default settings.\n"+ex.Message, "Collect Default Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplaySetting()
        {
            try
            {
                int solibriIndex = solibries.FindIndex(o => o.VersionNumber == settings.SolibriSetup.VersionNumber);
                if (solibriIndex > -1)
                {
                    comboBoxSolibri.SelectedIndex = solibriIndex;
                }

                int remoteIndex = remoteComputers.FindIndex(o => o.DirectoryName == settings.RemoteSetup.DirectoryName);
                if (remoteIndex > -1)
                {
                    comboBoxComputer.SelectedIndex = remoteIndex;
                }

                if (settings.SaveSolibriSettings.SaveInPlace)
                {
                    radioButtonInPlaceSolibri.IsChecked = true;
                }
                else
                {
                    radioButtonFolderSolibri.IsChecked = true;
                }
                textBoxSolibriFolder.Text = settings.SaveSolibriSettings.OutputFolder;
                checkBoxAppendSolibri.IsChecked = settings.SaveSolibriSettings.AppendDate;

                if (settings.SaveBCFSettings.SaveInPlace)
                {
                    radioButtonInPlaceBCF.IsChecked = true;
                }
                else
                {
                    radioButtonFolderBCF.IsChecked = true;
                }
                textBoxBCFFolder.Text = settings.SaveBCFSettings.OutputFolder;
                checkBoxAppendBCF.IsChecked = settings.SaveBCFSettings.AppendDate;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display settings.\n" + ex.Message, "Display Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOpenSolibriFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileFolderDialog folderDialog = new FileFolderDialog();
                folderDialog.Dialog.Title = "Select an Output Folder for Solibri Files";
                System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string folderName = folderDialog.SelectedPath;
                    textBoxSolibriFolder.Text = folderName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set an output folder location.\n" + ex.Message, "Solibri Output Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonOpenBCFFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileFolderDialog folderDialog = new FileFolderDialog();
                folderDialog.Dialog.Title = "Select an Output Folder for BCF Reports";
                System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string folderName = folderDialog.SelectedPath;
                    textBoxBCFFolder.Text = folderName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set an output folder location.\n" + ex.Message, "Solibri Output Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxSolibri.SelectedItem && null != comboBoxComputer.SelectedItem)
                {
                    settings.SolibriSetup = comboBoxSolibri.SelectedItem as SolibriProperties;
                    settings.RemoteSetup = comboBoxComputer.SelectedItem as RempoteMachine;

                    settings.SaveSolibriSettings.SaveInPlace = (bool)radioButtonInPlaceSolibri.IsChecked;
                    settings.SaveSolibriSettings.OutputFolder = textBoxSolibriFolder.Text;
                    settings.SaveSolibriSettings.AppendDate = (bool)checkBoxAppendSolibri.IsChecked;

                    settings.SaveBCFSettings.SaveInPlace = (bool)radioButtonInPlaceBCF.IsChecked;
                    settings.SaveBCFSettings.OutputFolder = textBoxBCFFolder.Text;
                    settings.SaveBCFSettings.AppendDate = (bool)checkBoxAppendBCF.IsChecked;

                    SaveUserSettings();
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply settings.\n" + ex.Message, "Apply Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveUserSettings()
        {
            try
            {
                Properties.Settings.Default.SolibriVersionNumber = settings.SolibriSetup.VersionNumber;
                Properties.Settings.Default.SolibriExe = settings.SolibriSetup.ExeFile;

                Properties.Settings.Default.ComputerLocation = settings.RemoteSetup.Location;
                Properties.Settings.Default.ComputerName = settings.RemoteSetup.ComputerName;
                Properties.Settings.Default.DirectoryName = settings.RemoteSetup.DirectoryName;

                Properties.Settings.Default.SolibriSaveInPlace = settings.SaveSolibriSettings.SaveInPlace;
                Properties.Settings.Default.SolibriOutputFolder = settings.SaveSolibriSettings.OutputFolder;
                Properties.Settings.Default.SolibriAppendDate = settings.SaveSolibriSettings.AppendDate;

                Properties.Settings.Default.BCFSaveInPlace = settings.SaveBCFSettings.SaveInPlace;
                Properties.Settings.Default.BCFOutputFolder = settings.SaveBCFSettings.OutputFolder;
                Properties.Settings.Default.BCFAppendDate = settings.SaveBCFSettings.AppendDate;

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save user settings.\n" + ex.Message, "Save User Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }

    public class AutorunSettings
    {
        private SolibriProperties solibriSetup = new SolibriProperties();
        private RempoteMachine remoteSetup = new RempoteMachine();
        private SaveModelSettings saveSolibriSettings = new SaveModelSettings(BatchFileType.Solibri);
        private SaveModelSettings saveBCFSettings = new SaveModelSettings(BatchFileType.BCF);

        public SolibriProperties SolibriSetup { get { return solibriSetup; } set { solibriSetup = value; } }
        public RempoteMachine RemoteSetup { get { return remoteSetup; } set { remoteSetup = value; } }
        public SaveModelSettings SaveSolibriSettings { get { return saveSolibriSettings; } set { saveSolibriSettings = value; } }
        public SaveModelSettings SaveBCFSettings { get { return saveBCFSettings; } set { saveBCFSettings = value; } }

        public AutorunSettings()
        {
            solibriSetup = new SolibriProperties("Solibri Model Checker v9.6", @"C:\Program Files\Solibri\SMCv9.6\Solibri Model Checker v9.6.exe");
            remoteSetup = new RempoteMachine("NY", "NY-BAT-D001", @"\\NY-BAT-D001\SolibriBatch");
        }

        public AutorunSettings(SolibriProperties solibri, RempoteMachine remote)
        {
            solibriSetup = solibri;
            remoteSetup = remote;
        }
    }

    public class SolibriProperties
    {
        private string versionNumber = "";
        private string exeFile = "";

        public string VersionNumber { get { return versionNumber; } set { versionNumber = value; } }
        public string ExeFile { get { return exeFile; } set { exeFile = value; } }

        public SolibriProperties()
        {
        }

        public SolibriProperties(string version, string exe)
        {
            versionNumber = version;
            exeFile = exe;
        }
    }

    public class RempoteMachine
    {
        private string location = "";
        private string computerName = "";
        private string directoryName = "";

        public string Location { get { return location; } set { location = value; } }
        public string ComputerName { get { return computerName; } set { computerName = value; } }
        public string DirectoryName { get { return directoryName; } set { directoryName = value; } }

        public RempoteMachine()
        {
        }

        public RempoteMachine(string officeLocation, string comName, string directory)
        {
            location = officeLocation;
            computerName = comName;
            directoryName = directory;
        }
    }

    public class SaveModelSettings
    {
        private BatchFileType fileType = BatchFileType.None;
        private bool saveInPlace = true;
        private string outputFolder = "";
        private bool appendDate = true;

        public BatchFileType FileType { get { return fileType; } set { fileType = value; } }
        public bool SaveInPlace { get { return saveInPlace; } set { saveInPlace = value; } }
        public string OutputFolder { get { return outputFolder; } set { outputFolder = value; } }
        public bool AppendDate { get { return appendDate; } set { appendDate = value; } }

        public SaveModelSettings(BatchFileType type)
        {
            fileType = type;
        }

        public SaveModelSettings(BatchFileType type, bool inPlace, string folderName, bool append)
        {
            fileType = type;
            saveInPlace = inPlace;
            outputFolder = folderName;
            appendDate = append;
        }
    }

    public enum BatchFileType
    {
        None, Solibri, IFC, BCF, Ruleset, PDF
    }
}
