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
using SolibriBatchSetup.Schema;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private AutorunSettings settings = new AutorunSettings();
        private List<SolibriProperties> solibries = new List<SolibriProperties>();
        private List<RemoteMachine> remoteComputers = new List<RemoteMachine>();

        public AutorunSettings Settings { get { return settings; } set { settings = value; } }

        public SettingWindow(AutorunSettings autoSetting)
        {
            settings = autoSetting;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = settings;
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
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "CLASSIFICATION File (*.classification)|*.classification";
                openDialog.Title = "Add Classification Files";
                openDialog.Multiselect = true;
                if (openDialog.ShowDialog() == true)
                {
                    foreach (string fileName in openDialog.FileNames)
                    {
                       settings.Classifications.Add(new OpenClassification(fileName));
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
                for (int i = dataGridClassification.SelectedItems.Count - 1; i > -1; i--)
                {
                    OpenClassification classification = dataGridClassification.SelectedItems[i] as OpenClassification;
                    settings.Classifications.Remove(classification);
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
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "CSET File (*.cset)|*.cset";
                openDialog.Title = "Add Ruleset Files";
                openDialog.Multiselect = true;
                if (openDialog.ShowDialog() == true)
                {
                    foreach (string fileName in openDialog.FileNames)
                    {
                        settings.Rulesets.Add(new OpenRuleset(fileName));
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
                for (int i = dataGridRuleset.SelectedItems.Count - 1; i > -1; i--)
                {
                    OpenRuleset ruleset = dataGridRuleset.SelectedItems[i] as OpenRuleset;
                    settings.Rulesets.Remove(ruleset);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonOpenReportFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileFolderDialog folderDialog = new FileFolderDialog();
                folderDialog.Dialog.Title = "Select an Output Folder for BCF Reports";
                System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string folderName = folderDialog.SelectedPath;
                    settings.ReportSettings.OutputFolder = folderName;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonOpenTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Microsoft Excel 97-2003 Worksheet (*.xls)|*.xls";
                openDialog.Title = "Open a Template File";
                if (openDialog.ShowDialog() == true)
                {
                    settings.ReportSettings.CoordinationTemplate = openDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
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
                    settings.SaveSolibriSettings.OutputFolder = folderName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set an output folder location.\n" + ex.Message, "Solibri Output Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void buttonDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                comboBoxSolibri.SelectedIndex = solibries.FindIndex(o => o.VersionNumber == "Solibri Model Checker v9.7");
                comboBoxComputer.SelectedIndex = remoteComputers.FindIndex(o => o.ComputerName == "NY-BAT-D001");
                settings.Classifications = new ObservableCollection<OpenClassification>();
                settings.Rulesets = new ObservableCollection<OpenRuleset>();
                
                settings.ReportSettings.IsCheckingSelected = false;
                settings.ReportSettings.IsPresentationSelected = false;
                settings.ReportSettings.IsBCFSelected = false;
                settings.ReportSettings.IsCoordinationSelected = false;
                settings.ReportSettings.SaveInPlace = true;
                settings.ReportSettings.OutputFolder = "";
                settings.ReportSettings.AppendDate = true;

                settings.SaveSolibriSettings.SaveInPlace = true;
                settings.SaveSolibriSettings.OutputFolder = "";
                settings.SaveSolibriSettings.AppendDate = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxSolibri.SelectedItem && null != comboBoxComputer.SelectedItem)
                {
                    if (!settings.ReportSettings.SaveInPlace && string.IsNullOrEmpty(settings.ReportSettings.OutputFolder))
                    {
                        MessageBox.Show("Please enter a valid directory for reports.", "Reports - Save In Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (!settings.ReportSettings.SaveInPlace && string.IsNullOrEmpty(settings.ReportSettings.OutputFolder))
                    {
                        MessageBox.Show("Please enter a valid directory for solibri.", "Saving Solibri Model - Save In Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        SaveUserSettings();
                        this.DialogResult = true;
                    }
                    
                }
                else
                {
                    MessageBox.Show("Please select a valid batch option.", "Batch Option Missing", MessageBoxButton.OK, MessageBoxImage.Information);
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

                Properties.Settings.Default.Classifications = new StringCollection();
                var classificaitonFiles = from classificaiton in settings.Classifications select classificaiton.File;
                if (classificaitonFiles.Count() > 0)
                {
                    string[]  cFiles = classificaitonFiles.ToArray();
                    Properties.Settings.Default.Classifications.AddRange(cFiles);
                }

                Properties.Settings.Default.Rulesets = new StringCollection();
                var rulesetFiles = from ruleset in settings.Rulesets select ruleset.File;
                if (rulesetFiles.Count() > 0)
                {
                    string[] rFiles = rulesetFiles.ToArray();
                    Properties.Settings.Default.Rulesets.AddRange(rFiles);
                }

                Properties.Settings.Default.ReportCheckingSelected = settings.ReportSettings.IsCheckingSelected;
                Properties.Settings.Default.ReportPresentationSelected = settings.ReportSettings.IsPresentationSelected;
                Properties.Settings.Default.ReportBCFSelected = settings.ReportSettings.IsBCFSelected;
                Properties.Settings.Default.ReportCoordinationSelected = settings.ReportSettings.IsCoordinationSelected;
                Properties.Settings.Default.ReportSaveInPlace = settings.ReportSettings.SaveInPlace;
                Properties.Settings.Default.ReportOutputFolder = settings.ReportSettings.OutputFolder;
                Properties.Settings.Default.ReportAppendDate = settings.ReportSettings.AppendDate;


                Properties.Settings.Default.SolibriSaveInPlace = settings.SaveSolibriSettings.SaveInPlace;
                Properties.Settings.Default.SolibriOutputFolder = settings.SaveSolibriSettings.OutputFolder;
                Properties.Settings.Default.SolibriAppendDate = settings.SaveSolibriSettings.AppendDate;

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save user settings.\n" + ex.Message, "Save User Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        
    }

  
}
