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
using Google.Apis.Drive.v2.Data;
using HOK.SmartBCF.GoogleUtils;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for OnlineBCFWindow.xaml
    /// </summary>
    public partial class OnlineBCFWindow : Window
    {
        private string projectId = "";
        private FolderHolders googleFolders = null;

        private List<OnlineBCFInfo> onlineBCFs = new List<OnlineBCFInfo>();
       
        public List<OnlineBCFInfo> OnlineBCFs { get { return onlineBCFs; } set { onlineBCFs = value; } }
       
        public OnlineBCFWindow(FolderHolders folders)
        {
            googleFolders = folders;
            projectId = googleFolders.RootId;
            InitializeComponent();
            labelProjectId.Content = projectId;
            DisplaySheetList();
        }

        private void DisplaySheetList()
        {
            try
            {
                if (null!=googleFolders && !string.IsNullOrEmpty(projectId))
                {
                    onlineBCFs = FileManager.GetOnlineBCFs(googleFolders);
                    onlineBCFs = onlineBCFs.OrderBy(o => o.SheetTitle).ToList();
                    dataGridFiles.ItemsSource = null;
                    dataGridFiles.ItemsSource = onlineBCFs;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display lists of BCF Google Spreadsheet.\n"+ex.Message, "Display BCF Lists", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
   

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private delegate void UpdateLableDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateProgressDelegate(System.Windows.DependencyProperty dp, Object value);

        private OnlineBCFInfo UploadOnlineBCF(FolderHolders folders, OnlineBCFInfo onlineBCF)
        {
            OnlineBCFInfo copiedBCFInfo = null;
            try
            {
                UpdateLableDelegate updateLabelDelegate = new UpdateLableDelegate(statusLable.SetValue);
                UpdateProgressDelegate updateProgressDelegate = new UpdateProgressDelegate(progressBar.SetValue);

                double progressValue = 0;
                progressBar.Maximum = 3 + onlineBCF.ImageFiles.Count;
                progressBar.Value = progressValue;
                progressBar.Visibility = Visibility.Visible;

                if (null != folders.ActiveBCFFolder)
                {
                    progressValue += 1;
                    string labelText = "Uploading online BCF : " + onlineBCF.SheetTitle;
                    Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, labelText });
                    Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                    File copiedFile = FileManager.CopyFile(onlineBCF.SpreadsheetFile, folders.ActiveBCFFolder.Id);
                    if (null != copiedFile)
                    {
                        copiedBCFInfo = new OnlineBCFInfo(folders.RootId, copiedFile);
                    }
                }

                if (null != copiedBCFInfo)
                {
                    if (null != folders.ArchiveBCFFolder)
                    {
                        progressValue += 1;
                        Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });
                        
                        File copiedZip = FileManager.CopyFile(onlineBCF.ArchiveFile, folders.ArchiveBCFFolder.Id);
                        if (null != copiedZip && null != copiedBCFInfo)
                        {
                            copiedBCFInfo.ArchiveFile = copiedZip;
                        }
                    }

                    if (null != folders.ActiveImgFolder)
                    {
                        List<File> copiedFiles = new List<File>();
                        foreach (File imgFile in onlineBCF.ImageFiles)
                        {
                            progressValue += 1;
                            Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                            File copiedImage = FileManager.CopyFile(imgFile, folders.ActiveImgFolder.Id);
                            if (null != copiedImage)
                            {
                                copiedFiles.Add(copiedImage);
                            }
                        }
                        if (null != copiedBCFInfo)
                        {
                            copiedBCFInfo.ImageFiles = copiedFiles;
                        }
                    }

                    //update property for cross referencing
                    if (null != copiedBCFInfo.SpreadsheetFile && null != copiedBCFInfo.ArchiveFile && copiedBCFInfo.ImageFiles.Count > 0)
                    {
                        progressValue += 1;
                        Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                        Dictionary<string/*key*/, string/*value*/> propertyDictionary = new Dictionary<string, string>();
                        propertyDictionary.Add("SheetId", copiedBCFInfo.SpreadsheetId);
                        propertyDictionary.Add("ArchiveZipId", copiedBCFInfo.ArchiveFile.Id);

                        copiedBCFInfo.SpreadsheetFile = FileManager.AddProperties(copiedBCFInfo.SpreadsheetFile.Id, propertyDictionary);
                        copiedBCFInfo.ArchiveFile = FileManager.AddProperties(copiedBCFInfo.ArchiveFile.Id, propertyDictionary);

                        for (int i = 0; i < copiedBCFInfo.ImageFiles.Count; i++)
                        {
                            copiedBCFInfo.ImageFiles[i] = FileManager.AddProperties(copiedBCFInfo.ImageFiles[i].Id, propertyDictionary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to upload online BCF.\n" + ex.Message, "Upload Online BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return copiedBCFInfo;
        }


        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

    public class OnlineBCFInfo
    {
        private string bcfProjectId = "";
        private string spreadsheetId = "";
        private string sheetTitle = "";
        private string sheetOwner = "";
        private string sheetModified = "";
        private File spreadsheetFile = null;
        private List<File> imageFiles = new List<File>();
        private File archiveFile = null;
        private bool isSelected = false;

        public string BCFProjectId { get { return bcfProjectId; } set { bcfProjectId = value; } }
        public string SpreadsheetId { get { return spreadsheetId; } set { spreadsheetId = value; } }
        public string SheetTitle { get { return sheetTitle; } set { sheetTitle = value; } }
        public string SheetOwner { get { return sheetOwner; } set { sheetOwner = value; } }
        public string SheetModified { get { return sheetModified; } set { sheetModified = value; } }
        public File SpreadsheetFile { get { return spreadsheetFile; } set { spreadsheetFile = value; } }
        public List<File> ImageFiles { get { return imageFiles; } set { imageFiles = value; } }
        public File ArchiveFile { get { return archiveFile; } set { archiveFile = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public OnlineBCFInfo(string rootId, File file)
        {
            bcfProjectId = rootId;
            spreadsheetId = file.Id;
            sheetTitle = file.Title;
            foreach (string owner in file.OwnerNames)
            {
                sheetOwner = owner; break;
            }
            sheetModified = file.ModifiedDate.ToString();
            spreadsheetFile = file;
        }
    }
}
