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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using MessageBox = System.Windows.MessageBox;
using System.Reflection;
using System.IO;
using Outlook = Microsoft.Office.Interop.Outlook;
using HOK.BatcUpgrader;
using System.Collections;

namespace HOK.BatchExporter
{
    public enum RevitVersion
    {
        Revit2014,
        Revit2015
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string/*unitconfigFile*/, ProjectSettings> projectSettings = new Dictionary<string, ProjectSettings>();
        private ProjectHeader selectedProjectHeader = null;
        private ProjectSettings selectedProject = null;
        private UpgradeOptions defaultUpgradeOptions = new UpgradeOptions();
        private BatchSettings batchSettings = new BatchSettings();

        private TaskSettings taskSettings = new TaskSettings();
        
        private Dictionary<int/*index*/, string/*worksetConfig*/> openWorksets = new Dictionary<int, string>();

        private string revit2014Exe = "";
        private string revit2015Exe = "";

       
        public MainWindow()
        {
            revit2014Exe = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Autodesk\\Revit 2014\\Revit.exe";
            revit2015Exe = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Autodesk\\Revit 2015\\Revit.exe";
            InstallerSettings installerSettings = new InstallerSettings("2014");
            if (!installerSettings.CheckInstallation())
            {
                installerSettings.InstallFiles();
            }
            installerSettings = new InstallerSettings("2015");
            if (!installerSettings.CheckInstallation())
            {
                installerSettings.InstallFiles();
            }

            InitializeComponent();
            this.Title = "HOK Batch Upgrader v."+Assembly.GetExecutingAssembly().GetName().Version.ToString();
           
            if (batchSettings.InitializeSettings())
            {
                projectSettings = batchSettings.ProjectSettingsDictionary;
                FillProjectLists();

                defaultUpgradeOptions = GetDefaultUpgradeOptions();
            }

            comboBoxWorkset.Items.Add("Editable"); openWorksets.Add(0, "AllEditable");
            comboBoxWorkset.Items.Add("All"); openWorksets.Add(1, "AllWorksets");
            comboBoxWorkset.Items.Add("Specify..."); openWorksets.Add(2, "AskUserToSpecify");
            comboBoxWorkset.Items.Add("Last Viewed"); openWorksets.Add(3, "LastViewed");
        }

        private UpgradeOptions GetDefaultUpgradeOptions()
        {
            UpgradeOptions upgradeOptions = new UpgradeOptions();
            upgradeOptions.UpgradeVersion = "2014";
            upgradeOptions.IsFinalUpgrade = false;

            OpenOptions openOptions = new OpenOptions();
            openOptions.Audit = true;
            openOptions.DetachAndPreserveWorksets = true;
            openOptions.OpenAllWorkset = false;

            upgradeOptions.UpgradeVersionOpenOptions = openOptions;

            SaveAsOptions saveAsOptions = new SaveAsOptions();
            saveAsOptions.NumOfBackups = 5;
            saveAsOptions.MakeCentral = true;
            saveAsOptions.Relinquish = false;
            saveAsOptions.WorksetConfiguration = "AskUserToSpecify";
            saveAsOptions.ReviewLocation = FindReviewDirectory();
            saveAsOptions.LogLocation = @"V:\HOK-Tools\BatchUpgrader\Logs";

            upgradeOptions.UpgradeVersionSaveAsOptions = saveAsOptions;
            return upgradeOptions;
        }

        private string FindReviewDirectory()
        {
            string reviewDirectoryName = "";
            try
            {
                if (null != selectedProject)
                {
                    if (selectedProject.FileItems.Count > 0)
                    {
                        FileItem fileItem = selectedProject.FileItems[0];
                        string revitFileName = fileItem.RevitFile;

                        string curDirectory = System.IO.Path.GetDirectoryName(revitFileName);
                        if (revitFileName.Contains("Software") && revitFileName.Contains("Revit"))
                        {
                            DirectoryInfo softwareDirectory = Directory.GetParent(curDirectory);
                            while (softwareDirectory.Name != "Software")
                            {
                                softwareDirectory = Directory.GetParent(softwareDirectory.FullName);
                            }

                            DirectoryInfo bimDirectory = Directory.GetParent(softwareDirectory.FullName);
                            DirectoryInfo reviewDirectory = null;
                            foreach (DirectoryInfo directoryInfo in bimDirectory.GetDirectories())
                            {
                                if (directoryInfo.Name == "Review")
                                {
                                    reviewDirectory = directoryInfo;
                                    break;
                                }
                            }
                            if (null != reviewDirectory)
                            {
                                string upgradeTest = System.IO.Path.Combine(reviewDirectory.FullName, "UpgradeTest");
                                reviewDirectoryName = upgradeTest;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find the Review directory.\n" + ex.Message, "Find Review Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return reviewDirectoryName;
        }

        public void DisplayUpgradeVersion(UpgradeOptions upgradeOption)
        {
            if (upgradeOption.UpgradeVersion == "2014") { radioButton2014.IsChecked = true; }
            else if (upgradeOption.UpgradeVersion == "2015") { radioButton2015.IsChecked = true; }

            timePickerUpgrade.Value = upgradeOption.TaskScheduleSettings.StartTime;
            datePickerUpgrade.SelectedDate = upgradeOption.TaskScheduleSettings.StartTime;

            checkBoxFinal.IsChecked = upgradeOption.IsFinalUpgrade;
            checkBoxAudit.IsChecked = upgradeOption.UpgradeVersionOpenOptions.Audit;
            checkBoxDetach.IsChecked = upgradeOption.UpgradeVersionOpenOptions.DetachAndPreserveWorksets;
            checkBoxOpen.IsChecked = upgradeOption.UpgradeVersionOpenOptions.OpenAllWorkset;

            textBoxBackups.Text = upgradeOption.UpgradeVersionSaveAsOptions.NumOfBackups.ToString();
            checkBoxCentral.IsChecked = upgradeOption.UpgradeVersionSaveAsOptions.MakeCentral;
            checkBoxRelinquish.IsChecked = upgradeOption.UpgradeVersionSaveAsOptions.Relinquish;

            switch (upgradeOption.UpgradeVersionSaveAsOptions.WorksetConfiguration)
            {
                case "AllEditable":
                    comboBoxWorkset.SelectedIndex = 0;
                    break;
                case "AllWorksets":
                    comboBoxWorkset.SelectedIndex = 1;
                    break;
                case "AskUserToSpecify":
                    comboBoxWorkset.SelectedIndex = 2;
                    break;
                case "LastViewed":
                    comboBoxWorkset.SelectedIndex = 3;
                    break;
            }

            if (string.IsNullOrEmpty(upgradeOption.UpgradeVersionSaveAsOptions.ReviewLocation)) { upgradeOption.UpgradeVersionSaveAsOptions.ReviewLocation = FindReviewDirectory(); }
            textBoxBackupLocation.Text = upgradeOption.UpgradeVersionSaveAsOptions.ReviewLocation;
            textBoxBackupLocation.ToolTip = textBoxBackupLocation.Text;
            textBoxLog.Text = upgradeOption.UpgradeVersionSaveAsOptions.LogLocation;
            textBoxLog.ToolTip = textBoxLog.Text;
        }

        private void tabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControlMain.SelectedIndex == 0)
            {
                dockPanelProject.Visibility = Visibility.Visible;
                dockPanelSelected.Visibility = Visibility.Hidden;
            }
            else
            {
                if (null != selectedProject)
                {
                    dockPanelSelected.Visibility = Visibility.Visible;
                    dockPanelProject.Visibility = Visibility.Hidden;
                }
                else
                {
                    MessageBox.Show("Please select an item for the batch upgrader to display settings.", "Empty Project", MessageBoxButton.OK, MessageBoxImage.Information);
                    tabControlMain.SelectedIndex = 0;
                }
            }
        }

        private void FillProjectLists()
        {
            try
            {
                dataGridProject.ItemsSource = null;
                ObservableCollection<ProjectHeader> projectHeaderCollection = new ObservableCollection<ProjectHeader>();
               
                foreach (string configFile in projectSettings.Keys)
                {
                    ProjectHeader projectHeader = new ProjectHeader();
                    ProjectSettings projectSetting = projectSettings[configFile];
                    projectHeader.Office = projectSetting.Office;
                    projectHeader.ProjectName = projectSetting.ProjectName;
                    projectHeaderCollection.Add(projectHeader);
                }
                
                ListCollectionView collection = new ListCollectionView(projectHeaderCollection);
                collection.GroupDescriptions.Add(new PropertyGroupDescription("Office"));
                dataGridProject.ItemsSource = collection;

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to bind data to data grid for projects.\n"+ex.Message, "FillProjectLists", MessageBoxButton.OK);
            }
        }

        #region Main Project Panel
        private void dataGridProject_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Office")
            {
                e.Column.Visibility = Visibility.Hidden;
            }
            else
            {
                e.Column.MinWidth = 215;
            }
        }

        private void dataGridProject_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (null != dataGridProject.SelectedItem)
                {
                    if (null != selectedProject ) { SaveChanges(selectedProject); }
                    selectedProjectHeader = dataGridProject.SelectedItem as ProjectHeader;
                    if (null != selectedProjectHeader)
                    {
                        foreach (string configFile in projectSettings.Keys)
                        {
                            ProjectSettings projectSetting = projectSettings[configFile];
                            if (projectSetting.ProjectName == selectedProjectHeader.ProjectName && projectSetting.Office == selectedProjectHeader.Office)
                            {
                                selectedProject = projectSetting;
                                break;
                            }
                        }
                    }

                    UpdateProjectInfo(selectedProject);
                    DisplayUpgradeVersion(selectedProject.UpgradeOptions);

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot update the selected project information.\n" + ex.Message, "dataGridProject_SelectedCellsChanged", MessageBoxButton.OK);
            }
        }

        private void SaveChanges(ProjectSettings ps)
        {
            try
            {
                if ((bool)radioButton2014.IsChecked) { ps.UpgradeOptions.UpgradeVersion = "2014"; }
                else if ((bool)radioButton2015.IsChecked) { ps.UpgradeOptions.UpgradeVersion = "2015"; }

                if (null != datePickerUpgrade.DisplayDate && null != timePickerUpgrade.Value)
                {
                    DateTime date = (DateTime)datePickerUpgrade.SelectedDate;
                    DateTime time = (DateTime)timePickerUpgrade.Value;
                    DateTime upgradeDateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
                    ps.UpgradeOptions.TaskScheduleSettings.StartTime = upgradeDateTime;
                }

                ps.UpgradeOptions.IsFinalUpgrade = (bool)checkBoxFinal.IsChecked;
                ps.UpgradeOptions.UpgradeVersionOpenOptions.Audit = (bool)checkBoxAudit.IsChecked;
                ps.UpgradeOptions.UpgradeVersionOpenOptions.DetachAndPreserveWorksets = (bool)checkBoxDetach.IsChecked;
                ps.UpgradeOptions.UpgradeVersionOpenOptions.OpenAllWorkset = (bool)checkBoxOpen.IsChecked;
                

                ps.UpgradeOptions.UpgradeVersionSaveAsOptions.NumOfBackups = int.Parse(textBoxBackups.Text);
                ps.UpgradeOptions.UpgradeVersionSaveAsOptions.MakeCentral = (bool)checkBoxCentral.IsChecked;
                ps.UpgradeOptions.UpgradeVersionSaveAsOptions.Relinquish = (bool)checkBoxRelinquish.IsChecked;
                ps.UpgradeOptions.UpgradeVersionSaveAsOptions.WorksetConfiguration = openWorksets[comboBoxWorkset.SelectedIndex];
                ps.UpgradeOptions.UpgradeVersionSaveAsOptions.ReviewLocation = textBoxBackupLocation.Text;
                ps.UpgradeOptions.UpgradeVersionSaveAsOptions.LogLocation = textBoxLog.Text;


                if (projectSettings.ContainsKey(ps.ConfigFileName))
                {
                    projectSettings.Remove(ps.ConfigFileName);
                }
                projectSettings.Add(ps.ConfigFileName, ps);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save changes on UI components.\n" + ex.Message, "SaveChanges", MessageBoxButton.OK);
            }
        }

        private void UpdateProjectInfo(ProjectSettings ps)
        {
            try
            {
                textBoxProjectName.Text = "";
                listViewFiles.ItemsSource = null;
                listViewRevitFile.ItemsSource = null;

                if (null != ps)
                {
                    ObservableCollection<RevitFileHeader> revitFiles = new ObservableCollection<RevitFileHeader>();
                    textBoxProjectName.Text = ps.ProjectName;

                    List<string> fileNames = new List<string>();
                    string[] splitter = new string[] { "\\" };
                    foreach (FileItem item in ps.FileItems)
                    {
                        RevitFileHeader revitFileHeader = new RevitFileHeader();
                        revitFileHeader.RevitFile = item.RevitFile;
                        revitFileHeader.OutputFolder = item.OutputFolder;

                        string[] strSplit = item.RevitFile.Split(splitter, StringSplitOptions.None);
                        fileNames.Add(strSplit[strSplit.Length - 1]);
                        revitFiles.Add(revitFileHeader);
                    }

                    listViewFiles.ItemsSource = fileNames;
                    listViewRevitFile.ItemsSource = revitFiles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot update the project information.\n" + ex.Message, "UpdateProjectInfo", MessageBoxButton.OK);
            }
        }


        private string ConvertBoolToString(bool value)
        {
            if (value) { return "Yes"; }
            else { return "No"; }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProjectWindow projectWindow = new ProjectWindow(false);
                projectWindow.Owner = Application.Current.MainWindow;

                Nullable<bool> result = projectWindow.ShowDialog();
                if (result == true)
                {
                    ProjectSettings projectSetting = projectWindow.CollectedProjectSettings;
                    string configFile = projectSetting.ConfigFileName;

                    if (projectSettings.ContainsKey(configFile))
                    {
                        System.Windows.MessageBox.Show(configFile + " is already loaded in this project.", "File Conflicted", MessageBoxButton.OK);
                    }
                    else
                    {
                        if (projectWindow.IsLoadedConfig)
                        {
                            projectSettings.Add(configFile, projectSetting);
                            FillProjectLists();
                        }
                        else
                        {
                            if (batchSettings.WriteProjectSettings(configFile, projectSetting))
                            {
                                projectSettings.Add(configFile, projectSetting);
                                FillProjectLists();
                            }
                        }

                        if (null != selectedProject) { SaveChanges(selectedProject); }
                        selectedProject = projectSetting;
                        DisplayUpgradeVersion(selectedProject.UpgradeOptions);
                        for (int i = 0; i < dataGridProject.Items.Count; i++)
                        {
                            ProjectHeader header = dataGridProject.Items[i] as ProjectHeader;
                            if (header.Office == projectSetting.Office && header.ProjectName == projectSetting.ProjectName)
                            {
                                dataGridProject.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    projectWindow.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot add a new project.\n" + ex.Message, "buttonAdd_Click", MessageBoxButton.OK);
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedProjectHeader)
                {
                    string keyString = "";
                    ProjectSettings projectToDelete = null;
                    foreach (string configFile in projectSettings.Keys)
                    {
                        ProjectSettings projectSetting = projectSettings[configFile];
                        if (selectedProjectHeader.ProjectName == projectSetting.ProjectName && selectedProjectHeader.Office == projectSetting.Office)
                        {
                            keyString = configFile; projectToDelete = projectSetting; break;
                        }
                    }
                    if (!string.IsNullOrEmpty(keyString) && null != projectToDelete)
                    {
                        projectSettings.Remove(keyString);

                        FillProjectLists();
                        selectedProject = null;
                        listViewFiles.ItemsSource = null;
                        listViewRevitFile.ItemsSource = null;
                        textBoxProjectName.Text = "";
                        ClearTaskSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot delet the selected project.\n" + ex.Message, "buttonDelete_Click", MessageBoxButton.OK);
            }
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedProjectHeader && null!=selectedProject)
                {
                    List<string> configFiles = projectSettings.Keys.ToList();
                    foreach (string configFile in configFiles)
                    {
                        ProjectSettings projectSetting = projectSettings[configFile];
                        if (selectedProjectHeader.ProjectName == projectSetting.ProjectName && selectedProjectHeader.Office == projectSetting.Office)
                        {
                            ProjectWindow projectWindow = new ProjectWindow(projectSetting, true);
                            projectWindow.Owner = Application.Current.MainWindow;

                            Nullable<bool> result = projectWindow.ShowDialog();
                            if (result == true)
                            {
                                projectSetting = projectWindow.CollectedProjectSettings;
                                projectSettings.Remove(configFile);
                                projectSettings.Add(projectSetting.ConfigFileName, projectSetting);
                                FillProjectLists();
                            }


                            for (int i = 0; i < dataGridProject.Items.Count; i++)
                            {
                                ProjectHeader header = dataGridProject.Items[i] as ProjectHeader;
                                if (header.Office == projectSetting.Office && header.ProjectName == projectSetting.ProjectName)
                                {
                                    dataGridProject.SelectedIndex = i;
                                    break;
                                }
                            }
                            projectWindow.Close();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot edit project settings.\n" + ex.Message, "Edit Project Settings", MessageBoxButton.OK);
            }
            
        }

        private void ClearTaskSettings()
        {
            try
            {
                timePickerUpgrade.Value = DateTime.Now;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot clear task settings.\n" + ex.Message, "Clear Task Settings", MessageBoxButton.OK);
            }
        }
        #endregion

        #region Manage Files
        private void buttonAddFile_Click(object sender, RoutedEventArgs e)
        {
            if (null != selectedProject)
            {
                FilesWindow filesWindow = new FilesWindow();
                filesWindow.Owner = Application.Current.MainWindow;
                Nullable<bool> result = filesWindow.ShowDialog();
                if (result == true)
                {
                    AddFileToList(filesWindow.RvtFileName, filesWindow.OutputFolder);
                    if (string.IsNullOrEmpty(selectedProject.UpgradeOptions.UpgradeVersionSaveAsOptions.ReviewLocation))
                    {
                        selectedProject.UpgradeOptions.UpgradeVersionSaveAsOptions.ReviewLocation = FindReviewDirectory();
                        textBoxBackupLocation.Text = selectedProject.UpgradeOptions.UpgradeVersionSaveAsOptions.ReviewLocation;
                    }
                }

            }
            else
            {
                MessageBox.Show("Please select a project name to add files.", "Select a Project", MessageBoxButton.OK);
            }
        }

        private void buttonAddFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedProject)
                {
                    MultiFilesWindow filesWindow = new MultiFilesWindow();
                    filesWindow.Owner = Application.Current.MainWindow;
                    if (filesWindow.ShowDialog() == true)
                    {
                        foreach (string rvtFileName in filesWindow.RevitFileNames)
                        {
                            AddFileToList(rvtFileName, filesWindow.OutputFolder);
                        }

                        if (string.IsNullOrEmpty(selectedProject.UpgradeOptions.UpgradeVersionSaveAsOptions.ReviewLocation))
                        {
                            selectedProject.UpgradeOptions.UpgradeVersionSaveAsOptions.ReviewLocation = FindReviewDirectory();
                            textBoxBackupLocation.Text = selectedProject.UpgradeOptions.UpgradeVersionSaveAsOptions.ReviewLocation;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Cannot add files from folder.\n"+ex.Message, "Add Files from Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void MenuItem_AddFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedProject)
                {
                    FilesWindow filesWindow = new FilesWindow();
                    filesWindow.Owner = Application.Current.MainWindow;

                    Nullable<bool> result = filesWindow.ShowDialog();
                    if (result == true)
                    {
                        AddFileToList(filesWindow.RvtFileName, filesWindow.OutputFolder);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a project name to add files.", "Select a Project", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot add file in the list.\n" + ex.Message, "Add Files", MessageBoxButton.OK);
            }
        }

        private void AddFileToList(string revitFile, string outputFolder)
        {
            try
            {
                FileItem fileItem = new FileItem();
                fileItem.RevitFile = revitFile;
                fileItem.OutputFolder = outputFolder;
                selectedProject.FileItems.Add(fileItem);

                List<string> configFiles = projectSettings.Keys.ToList();
                foreach (string configFile in configFiles)
                {
                    ProjectSettings projectSetting = projectSettings[configFile];
                    if (projectSetting.ProjectName == selectedProject.ProjectName && projectSetting.Office == selectedProject.Office)
                    {
                        projectSettings.Remove(configFile);
                        projectSettings.Add(configFile, selectedProject);
                        break;
                    }
                }

                UpdateProjectInfo(selectedProject);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot add files in the list.\n" + ex.Message, "Add File", MessageBoxButton.OK);
            }
        }

        private void InsertFileToList(string revitFile, string outputFolder, int index)
        {
            try
            {
                FileItem fileItem = new FileItem();
                fileItem.RevitFile = revitFile;
                fileItem.OutputFolder = outputFolder;
                selectedProject.FileItems.Insert(index, fileItem);

                List<string> configFiles = projectSettings.Keys.ToList();
                foreach (string configFile in configFiles)
                {
                    ProjectSettings projectSetting = projectSettings[configFile];
                    if (projectSetting.ProjectName == selectedProject.ProjectName && projectSetting.Office == selectedProject.Office)
                    {
                        projectSettings.Remove(configFile);
                        projectSettings.Add(configFile, selectedProject);
                        break;
                    }
                }

                UpdateProjectInfo(selectedProject);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot add files in the list.\n" + ex.Message, "Add File", MessageBoxButton.OK);
            }
        }

        private void buttonDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewRevitFile.SelectedItems.Count > 0)
                {
                    foreach (object item in listViewRevitFile.SelectedItems)
                    {
                        RevitFileHeader selectedItem = item as RevitFileHeader;

                        for (int i = 0; i < selectedProject.FileItems.Count; i++)
                        {
                            if (selectedProject.FileItems[i].RevitFile == selectedItem.RevitFile)
                            {
                                selectedProject.FileItems.RemoveAt(i);
                            }
                        }
                    }

                    List<string> configFiles = projectSettings.Keys.ToList();
                    foreach (string configFile in configFiles)
                    {
                        ProjectSettings projectSetting = projectSettings[configFile];
                        if (projectSetting.ProjectName == selectedProject.ProjectName && projectSetting.Office == selectedProject.Office)
                        {
                            projectSettings.Remove(configFile);
                            projectSettings.Add(configFile, selectedProject);
                            break;
                        }
                    }

                    UpdateProjectInfo(selectedProject);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot delete files from the list.\n" + ex.Message, "Delete File", MessageBoxButton.OK);
            }
        }

        private void MenuItem_DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewRevitFile.SelectedItems.Count > 0)
                {
                    foreach (object item in listViewRevitFile.SelectedItems)
                    {
                        RevitFileHeader selectedItem = item as RevitFileHeader;

                        for (int i = 0; i < selectedProject.FileItems.Count; i++)
                        {
                            if (selectedProject.FileItems[i].RevitFile == selectedItem.RevitFile)
                            {
                                selectedProject.FileItems.RemoveAt(i);
                            }
                        }
                    }

                    List<string> configFiles = projectSettings.Keys.ToList();
                    foreach (string configFile in configFiles)
                    {
                        ProjectSettings projectSetting = projectSettings[configFile];
                        if (projectSetting.ProjectName == selectedProject.ProjectName && projectSetting.Office == selectedProject.Office)
                        {
                            projectSettings.Remove(configFile);
                            projectSettings.Add(configFile, selectedProject);
                            break;
                        }
                    }

                    UpdateProjectInfo(selectedProject);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot delete files from the list.\n" + ex.Message, "Delete File", MessageBoxButton.OK);
            }
        }

        private void buttonEditPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewRevitFile.SelectedItems.Count > 0)
                {
                    RevitFileHeader selectedItem = listViewRevitFile.SelectedItem as RevitFileHeader;
                    FilesWindow filesWindow = new FilesWindow();
                    filesWindow.RvtFileName = selectedItem.RevitFile;
                    filesWindow.OutputFolder = selectedItem.OutputFolder;
                    filesWindow.DisplayPredefined();

                    Nullable<bool> result = filesWindow.ShowDialog();
                    if (result == true)
                    {
                        for (int i = 0; i < selectedProject.FileItems.Count; i++)
                        {
                            if (selectedProject.FileItems[i].RevitFile == selectedItem.RevitFile)
                            {
                                selectedProject.FileItems.RemoveAt(i);
                            }
                        }
                        AddFileToList(filesWindow.RvtFileName, filesWindow.OutputFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot edit the file paths.\n" + ex.Message, "Edit Path", MessageBoxButton.OK);
            }
        }

        private void MenuItem_EditPaths_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewRevitFile.SelectedItems.Count > 0)
                {
                    RevitFileHeader selectedItem = listViewRevitFile.SelectedItem as RevitFileHeader;
                    FilesWindow filesWindow = new FilesWindow();
                    filesWindow.RvtFileName = selectedItem.RevitFile;
                    filesWindow.OutputFolder = selectedItem.OutputFolder;
                    filesWindow.DisplayPredefined();

                    Nullable<bool> result = filesWindow.ShowDialog();
                    if (result == true)
                    {
                        for (int i = 0; i < selectedProject.FileItems.Count; i++)
                        {
                            if (selectedProject.FileItems[i].RevitFile == selectedItem.RevitFile)
                            {
                                selectedProject.FileItems.RemoveAt(i);
                            }
                        }
                        AddFileToList(filesWindow.RvtFileName, filesWindow.OutputFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot edit the file paths.\n" + ex.Message, "Edit Path", MessageBoxButton.OK);
            }
        }

        private void buttonDuplicate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewRevitFile.SelectedItems.Count > 0)
                {
                    RevitFileHeader selectedItem = listViewRevitFile.SelectedItem as RevitFileHeader;
                    FilesWindow filesWindow = new FilesWindow();
                    filesWindow.RvtFileName = selectedItem.RevitFile.Replace(".rvt", "-Copy.rvt");
                    filesWindow.OutputFolder = selectedItem.OutputFolder;
                    filesWindow.DisplayPredefined();

                    Nullable<bool> result = filesWindow.ShowDialog();
                    if (result == true)
                    {
                        AddFileToList(filesWindow.RvtFileName, filesWindow.OutputFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot duplicate the file paths.\n" + ex.Message, "Duplicate Path", MessageBoxButton.OK);
            }
        }

        private void MenuItem_DuplicateSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewRevitFile.SelectedItems.Count > 0)
                {
                    RevitFileHeader selectedItem = listViewRevitFile.SelectedItem as RevitFileHeader;
                    FilesWindow filesWindow = new FilesWindow();
                    filesWindow.RvtFileName = selectedItem.RevitFile.Replace(".rvt", "-Copy.rvt");
                    filesWindow.OutputFolder = selectedItem.OutputFolder;
                    filesWindow.DisplayPredefined();

                    Nullable<bool> result = filesWindow.ShowDialog();
                    if (result == true)
                    {
                        AddFileToList(filesWindow.RvtFileName, filesWindow.OutputFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot duplicate the file paths.\n" + ex.Message, "Duplicate Path", MessageBoxButton.OK);
            }
        }
        #endregion

        #region Final Commands
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("Would you like to save configuration settings before closing the window?", "Close Window", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question);
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                if (SaveProject())
                {
                    this.Close();
                }
            }
            else if (dr == System.Windows.Forms.DialogResult.No)
            {
                this.Close();
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (SaveProject())
            {
                MessageBox.Show("The configuration settings for the batch upgrader was successfully saved.", "Configuration Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveProject())
            {
                 MessageBox.Show("The configuration settings for the batch upgrader was successfully saved.", "Configuration Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool SaveProject()
        {
            bool result = false;
            try
            {
                if (ValidateSettings())
                {
                    if (null != selectedProject) { SaveChanges(selectedProject); }

                    batchSettings.ProjectSettingsDictionary = projectSettings;
                    if (batchSettings.WriteMasterConfiguration("2014") && batchSettings.WriteMasterConfiguration("2015"))
                    {
                        if (string.IsNullOrEmpty(taskSettings.PassWord))
                        {
                            if (taskSettings.InitializeTaskScheduler())
                            {
                                taskSettings.ClearScheduledTask();
                                foreach (string configFile in projectSettings.Keys)
                                {
                                    ProjectSettings ps = projectSettings[configFile];
                                    batchSettings.WriteProjectSettings(configFile, ps);
                                    taskSettings.CreateScheduluedTask(ps);
                                }

                                result = true;
                            }
                        }
                        else
                        {
                            taskSettings.ClearScheduledTask();
                            foreach (string configFile in projectSettings.Keys)
                            {
                                ProjectSettings ps = projectSettings[configFile];
                                batchSettings.WriteProjectSettings(configFile, ps);
                                taskSettings.CreateScheduluedTask(ps);
                            }
                            
                            result = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save the configuration settings for the batch upgrader.\n" + ex.Message, "Failed to save", MessageBoxButton.OK);
            }
            return result;
        }

        private bool ValidateSettings()
        {
            bool validSettigns = false;
            try
            {
                int backupNum = 0;
                if (!int.TryParse(textBoxBackups.Text, out backupNum))
                {
                    MessageBox.Show("Please enter a number for the maximum Backups.", "Invalid Backup Number", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                if (null == comboBoxWorkset.SelectedItem)
                {
                    MessageBox.Show("Please select a default behavior of opening worksets.", "Empty Selection for Workset", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                if (string.IsNullOrEmpty(textBoxBackupLocation.Text))
                {
                    MessageBox.Show("Please input a valid path for Review Files under the Review Section.", "Review Files", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }


                if (string.IsNullOrEmpty(textBoxLog.Text))
                {
                    MessageBox.Show("Please input a valid path for log files under the Review Section", "Log Location", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                else if (!Directory.Exists(textBoxLog.Text))
                {
                    MessageBox.Show("Please input a valid path for log files under the Review Section", "Log Location", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                validSettigns = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to validate settings.\n"+ex.Message, "Validate Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return validSettigns;
        }

        #endregion

        #region Upgrade
      
        


        private void buttonUpgrade_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedProject)
                {
                    bool saved = SaveProject();
                    if (saved)
                    {
                        bool runTask = taskSettings.RunTask(selectedProject.UpgradeOptions.TaskScheduleSettings.TaskName);
                        if (runTask)
                        {
                            buttonEnd.IsEnabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Please save the configuration settings of the project first.", "Save Project", MessageBoxButton.OK);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot upgrade version.\n" + ex.Message, "Upgrade Version Now", MessageBoxButton.OK);
            }
        }

        private void checkBoxFinal_Checked(object sender, RoutedEventArgs e)
        {
            if (null != selectedProject)
            {
                selectedProject.UpgradeOptions.IsFinalUpgrade = true;
            }
        }

        private void checkBoxFinal_Unchecked(object sender, RoutedEventArgs e)
        {
            if (null != selectedProject)
            {
                selectedProject.UpgradeOptions.IsFinalUpgrade = false;
            }
        }
        #endregion

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string helpFile=@"V:\RVT-Data\HOK Program\Documentation\Batch Processor_Instruction.pdf";
            if (File.Exists(helpFile))
            {
                System.Diagnostics.Process.Start(helpFile);
            }
        }

        private void emailLink_Click(object sender, RoutedEventArgs e)
        {
            Outlook.Application outlookApplication = new Outlook.Application();
            Outlook.NameSpace nameSpace = outlookApplication.GetNamespace("MAPI");
            Outlook.Folder folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            Outlook.MailItem mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = "Batch Processor Problem Report";
            mailItem.Body = "**** This email will go to the developer. Please attach a log file created in V:\\HOK-Tools\\BatchUpgrader\\Logs****\n" + "What office are you in? \n" + "What project are you working on? \n" + "Describe the problem:";

            mailItem.Recipients.Add("jinsol.kim@hok.com");
            mailItem.Display(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                /*
                MessageBoxResult result = MessageBox.Show("Would you like to save the project settings before closing this window?", "Closing Window", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveProject(false);
                }
                 */
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the project. \n" + ex.Message, "Closing Window", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void buttonEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit " + selectedProject.UpgradeOptions.UpgradeVersion + "\\BatchUpgrader";

                taskSettings.RevitKill();
                RegistryUtil.keyAddress = keyAddress;
                RegistryUtil.SetRegistryKey("ActivateAddIn", false);
                buttonEnd.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to end process.\n"+ex.Message, "End Process", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDefault_Click(object sender, RoutedEventArgs e)
        {
            DisplayUpgradeVersion(defaultUpgradeOptions);
        }

        private void buttonChangeBackUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileFolderDialog fileFolderDialog = new FileFolderDialog();
                fileFolderDialog.Dialog.Title = "Select the directory that you want to store files.";
                System.Windows.Forms.DialogResult result = fileFolderDialog.ShowDialog();
 
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textBoxBackupLocation.Text = fileFolderDialog.SelectedPath;
                    textBoxBackupLocation.ToolTip = textBoxBackupLocation.Text;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change backup location.\n"+ex.Message, "Backup Location", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonChangeLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileFolderDialog fileFolderDialog = new FileFolderDialog();
                fileFolderDialog.Dialog.Title = "Select the directory for the log file";
                System.Windows.Forms.DialogResult result = fileFolderDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textBoxLog.Text = fileFolderDialog.SelectedPath;
                    textBoxLog.ToolTip = textBoxLog.Text;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change backup location.\n" + ex.Message, "Backup Location", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonMoveUp_Click(object sender, RoutedEventArgs e)
        {
            //move the file item up
            try
            {
                if (listViewRevitFile.SelectedItems.Count > 0)
                {
                    RevitFileHeader selectedItem = listViewRevitFile.SelectedItem as RevitFileHeader;
                    int fileIndex = 0;
                    for (int i = 0; i < selectedProject.FileItems.Count; i++)
                    {
                        if (selectedProject.FileItems[i].RevitFile == selectedItem.RevitFile)
                        {
                            fileIndex = i;
                        }
                    }

                    if (fileIndex != 0)
                    {
                        selectedProject.FileItems.RemoveAt(fileIndex);
                        InsertFileToList(selectedItem.RevitFile, selectedItem.OutputFolder, fileIndex-1);
                        listViewRevitFile.SelectedIndex = fileIndex - 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to move the selected file up.\n"+ex.Message, "Move Up", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonMoveDown_Click(object sender, RoutedEventArgs e)
        {
            //move the file item down
            try
            {
                if (listViewRevitFile.SelectedItems.Count > 0)
                {
                    RevitFileHeader selectedItem = listViewRevitFile.SelectedItem as RevitFileHeader;
                    int fileIndex = 0;
                    for (int i = 0; i < selectedProject.FileItems.Count; i++)
                    {
                        if (selectedProject.FileItems[i].RevitFile == selectedItem.RevitFile)
                        {
                            fileIndex = i;
                        }
                    }

                    if (fileIndex != selectedProject.FileItems.Count-1)
                    {
                        selectedProject.FileItems.RemoveAt(fileIndex);
                        InsertFileToList(selectedItem.RevitFile, selectedItem.OutputFolder, fileIndex + 1);
                        listViewRevitFile.SelectedIndex = fileIndex + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to move the selected file down.\n" + ex.Message, "Move Down", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }

}
