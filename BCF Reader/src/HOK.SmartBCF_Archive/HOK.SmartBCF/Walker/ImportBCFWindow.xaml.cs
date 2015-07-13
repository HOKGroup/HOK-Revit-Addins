using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Microsoft.Win32;

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for ImportBCFWindow.xaml
    /// </summary>
    public partial class ImportBCFWindow : Window
    {
        private string bcfPath = "";
        private string sharedLink = "";
        private bool rememberPath = false;
        private BCFZIP bcfzip = new BCFZIP();
        private FileHolders fileHolders = null;
        private FolderHolders googleFolders = null;
        private LinkedBcfFileInfo bcfFileInfo = null;
        private string bcfProjectId = "";
        private string bcfColorSchemeId = "";
        private bool rememberProjectId = false;

        public string BCFPath { get { return bcfPath; } set { bcfPath = value; } }
        public string SharedLink { get { return sharedLink; } set { sharedLink = value; } }
        public bool RememberPath { get { return rememberPath; } set { rememberPath = value; } }
        public FileHolders FilesInfo { get { return fileHolders; } set { fileHolders = value; } }
        public FolderHolders GoogleFolders { get { return googleFolders; } set { googleFolders = value; } }
        public LinkedBcfFileInfo BCFFileInfo { get { return bcfFileInfo; } set { bcfFileInfo = value; } }
        public string BCFProjectId { get { return bcfProjectId; } set { bcfProjectId = value; } }
        public string BCFColorSchemeId { get { return bcfColorSchemeId; } set { bcfColorSchemeId = value; } }
        public bool RememberProjectId { get { return rememberProjectId; } set { rememberProjectId = value; } }

        public ImportBCFWindow(FolderHolders folders)
        {
            googleFolders = folders;
            if (null != googleFolders)
            {
                bcfProjectId = googleFolders.RootId;
            }
            
            InitializeComponent();
            if (!string.IsNullOrEmpty(bcfProjectId))
            {
                sharedLink = FileManager.GetSharedLinkAddress(bcfProjectId);
                textBoxFolder.Text = sharedLink;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private delegate void UpdateLableDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateProgressDelegate(System.Windows.DependencyProperty dp, Object value);

        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            UpdateLableDelegate updateLabelDelegate = new UpdateLableDelegate(statusLable.SetValue);
            UpdateProgressDelegate updateProgressDelegate = new UpdateProgressDelegate(progressBar.SetValue);

            try
            {
                double progressValue =0;
                progressBar.Maximum = 4;
                progressBar.Value = progressValue;
                progressBar.Visibility = Visibility.Visible;
                bcfPath = textBoxBCF.Text;
                sharedLink = textBoxFolder.Text;
                string folderId = "";
                if (sharedLink.Contains("id="))
                {
                    folderId = FileManager.GetFolderId(sharedLink);
                }

                if (!string.IsNullOrEmpty(bcfProjectId))
                {
                    if (folderId != bcfProjectId)
                    {
                        bcfProjectId = folderId;
                        googleFolders = FileManager.CreateDefaultFolders(bcfProjectId);
                    }
                }
                else
                {
                    bcfProjectId = folderId;
                    googleFolders = FileManager.CreateDefaultFolders(bcfProjectId);
                }
                
                fileHolders = new FileHolders(bcfPath, sharedLink);
                File colorSheet = null;
                File uploadedBCF = null;
                File createdSheet = null;
                List<File> uploadedImages = null;

                if (!string.IsNullOrEmpty(bcfPath) && null!=googleFolders)
                {
                    if (null!=googleFolders)
                    {
                        progressValue += 1;
                        Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Searching default folders..." });
                        Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue});

                        colorSheet = googleFolders.ColorSheet;
                        bcfColorSchemeId = colorSheet.Id;
                       
                        string bcfName = System.IO.Path.GetFileNameWithoutExtension(bcfPath);
                        if (FileManager.CheckExistingFiles(bcfName, googleFolders))
                        {
                            if (null != googleFolders.ArchiveBCFFolder)
                            {
                                progressValue += 1;
                                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Uploading bcfzip to an archive folder..." });
                                Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                                string parentId = googleFolders.ArchiveBCFFolder.Id;
                                uploadedBCF = FileManager.UploadBCF(bcfPath, parentId);
                            }
                            if (null != googleFolders.ActiveBCFFolder && null != googleFolders.ArchiveImgFolder)
                            {
                                BCFUtil bcfUtil = new BCFUtil();
                                bcfzip = bcfUtil.ReadBCF(bcfPath);
                                fileHolders.BcfZip = bcfzip;

                                progressValue += 1;
                                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Creating Google spreadsheet..." });
                                Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                                string parentId = googleFolders.ActiveBCFFolder.Id;
                                createdSheet = FileManager.CreateSpreadsheet(bcfPath, parentId);
                                if (null != createdSheet && null != colorSheet)
                                {
                                    bool sheetCreated = BCFParser.ConverToGoogleDoc(bcfzip, createdSheet.Id, colorSheet.Id);
                                }

                                if (null != bcfzip)
                                {
                                    Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Uploading BCF images..." });
                                    Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });
                                    parentId = googleFolders.ActiveImgFolder.Id;
                                    uploadedImages = FileManager.UploadBCFImages(bcfzip, parentId);
                                }

                            }

                            if (null != uploadedBCF && null != createdSheet && null != uploadedImages)
                            {
                                progressValue =0;
                                progressBar.Value = progressValue;
                                progressBar.Maximum = uploadedImages.Count + 2;
                                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Updating properties of files..." });
                               
                                Dictionary<string/*key*/, string/*value*/> propertyDictionary = new Dictionary<string, string>();
                                propertyDictionary.Add("SheetId", createdSheet.Id);
                                propertyDictionary.Add("BcfPath", bcfPath);
                                propertyDictionary.Add("ArchiveZipId", uploadedBCF.Id);

                                progressValue += 1;
                                uploadedBCF = FileManager.AddProperties(uploadedBCF.Id, propertyDictionary);
                                Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                                progressValue += 1;
                                createdSheet = FileManager.AddProperties(createdSheet.Id, propertyDictionary);
                                Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                                for (int i = 0; i < uploadedImages.Count; i++)
                                {
                                    progressValue += 1;
                                    uploadedImages[i] = FileManager.AddProperties(uploadedImages[i].Id, propertyDictionary);
                                    Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });
                                }

                                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Completed." });
                                progressBar.Visibility = Visibility.Hidden;

                                fileHolders.ArchivedBCF = uploadedBCF;
                                fileHolders.ActiveBCF = createdSheet;
                                fileHolders.BCFImages = uploadedImages;

                                bcfFileInfo = new LinkedBcfFileInfo(createdSheet.Title, createdSheet.Id, sharedLink, bcfProjectId, googleFolders.RootTitle, bcfProjectId);

                                this.DialogResult = true;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Folder Id cannot be identified.\n Please enter a valid shared link.\n", "Invalid Shared Link", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a correct form of the file path of bcf or the address of shared link.\n", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import BCF.\n"+ex.Message, "Import BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Ready." });
            }
            progressBar.Visibility = Visibility.Hidden;
        }

        private bool CheckSubFolders()
        {
            bool valid = false;
            return valid;
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ".bcfzip";
                openFileDialog.Filter = "bcf files (*.bcfzip)|*.bcfzip";
                openFileDialog.Multiselect = false;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    string fileName = openFileDialog.FileName;
                    textBoxBCF.Text = fileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open BCF file.\n"+ex.Message, "Open BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxRemember_Checked(object sender, RoutedEventArgs e)
        {
            rememberProjectId = (bool)checkBoxRemember.IsChecked;
        }

        private void checkBoxRemember_Unchecked(object sender, RoutedEventArgs e)
        {
            rememberProjectId = (bool)checkBoxRemember.IsChecked;
        }
    }
}
