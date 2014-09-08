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
        private string folderId = "";
        private bool rememberPath = false;
        private BCFZIP bcfzip = new BCFZIP();
        private FileHolders fileHolders = null;
        private LinkedBcfFileInfo bcfFileInfo = null;

        public string BCFPath { get { return bcfPath; } set { bcfPath = value; } }
        public string SharedLink { get { return sharedLink; } set { sharedLink = value; } }
        public string FolderId { get { return folderId; } set { folderId = value; } }
        public bool RememberPath { get { return rememberPath; } set { rememberPath = value; } }
        public FileHolders FilesInfo { get { return fileHolders; } set { fileHolders = value; } }
        public LinkedBcfFileInfo BCFFileInfo { get { return bcfFileInfo; } set { bcfFileInfo = value; } }

        public ImportBCFWindow()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private delegate void UpdateLableDelegate(System.Windows.DependencyProperty dp, Object value);

        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            UpdateLableDelegate updateLabelDelegate = new UpdateLableDelegate(statusLable.SetValue);
            try
            {
                bcfPath = textBoxBCF.Text;
                sharedLink = textBoxFolder.Text;
                fileHolders = new FileHolders(bcfPath, sharedLink);
                File uploadedBCF = null;
                File createdSheet = null;
                List<File> uploadedImages = null;


                if (!string.IsNullOrEmpty(bcfPath) && !string.IsNullOrEmpty(sharedLink) && sharedLink.Contains("id="))
                {
                    folderId = GetFolderId(sharedLink);
                    if (!string.IsNullOrEmpty(folderId))
                    {
                        Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Searching default folders..." });
                        FolderHolders folderHolder = FileManager.CreateDefaultFolders(folderId);

                        string bcfzipName = System.IO.Path.GetFileName(bcfPath);
                        if (CheckExistingFiles(bcfzipName, folderHolder))
                        {
                            if (null != folderHolder.ArchiveBCFFolder)
                            {
                                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Uploading bcfzip to an archive folder..." });
                                string parentId = folderHolder.ArchiveBCFFolder.Id;
                                uploadedBCF = FileManager.UploadBCF(bcfPath, parentId);
                            }
                            if (null != folderHolder.ActiveBCFFolder && null != folderHolder.ArchiveImgFolder)
                            {
                                BCFUtil bcfUtil = new BCFUtil();
                                bcfzip = bcfUtil.ReadBCF(bcfPath);
                                fileHolders.BcfZip = bcfzip;

                                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Creating Google spreadsheet..." });

                                string parentId = folderHolder.ActiveBCFFolder.Id;
                                createdSheet = FileManager.CreateSpreadsheet(bcfPath, parentId);
                                if (null != createdSheet)
                                {
                                    bool sheetCreated = BCFParser.ConverToGoogleDoc(bcfzip, createdSheet.Id);
                                }

                                if (null != bcfzip)
                                {
                                    Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Uploading BCF images..." });
                                    parentId = folderHolder.ActiveImgFolder.Id;
                                    uploadedImages = FileManager.UploadBCFImages(bcfzip, parentId);
                                }
                            }

                            if (null != uploadedBCF && null != createdSheet && null != uploadedImages)
                            {
                                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Updating properties of files..." });
                                Dictionary<string/*key*/, string/*value*/> propertyDictionary = new Dictionary<string, string>();
                                propertyDictionary.Add("SheetId", createdSheet.Id);
                                propertyDictionary.Add("BcfPath", bcfPath);
                                propertyDictionary.Add("ArchiveZipId", uploadedBCF.Id);

                                uploadedBCF = FileManager.AddProperties(uploadedBCF.Id, propertyDictionary);
                                createdSheet = FileManager.AddProperties(createdSheet.Id, propertyDictionary);
                               
                                for (int i = 0; i < uploadedImages.Count; i++)
                                {
                                    uploadedImages[i] = FileManager.AddProperties(uploadedImages[i].Id, propertyDictionary);
                                }

                                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Completed." });
                                fileHolders.ArchivedBCF = uploadedBCF;
                                fileHolders.ActiveBCF = createdSheet;
                                fileHolders.BCFImages = uploadedImages;

                                bcfFileInfo = new LinkedBcfFileInfo(createdSheet.Title, createdSheet.Id, sharedLink, folderId);

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
        }

        private bool CheckExistingFiles(string fileName, FolderHolders folderHolder)
        {
            bool result = false;
            try
            {
                File fileUploaded = FileManager.FindSubFolder(fileName, folderHolder.ArchiveBCFFolder.Id);
                if (null != fileUploaded)
                {
                    MessageBoxResult mbr = MessageBox.Show(fileName+" already exists in the shared Google Drive folder.\nWould you like to replace the file in the shared folders?", "File Already Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    switch (mbr)
                    {
                        case MessageBoxResult.Yes:
                            //delete the existing files first
                            string bcfFileId = fileUploaded.Id;
                            List<File> sheetsFound = FileManager.FindFilesByProperty(folderHolder.ActiveBCFFolder.Id, "ArchiveZipId", bcfFileId);
                            if (sheetsFound.Count > 0)
                            {
                                foreach (File file in sheetsFound)
                                {
                                    bool deleted = FileManager.DeleteFile(file.Id);
                                }
                            }
                            List<File> imagesFound = FileManager.FindFilesByProperty(folderHolder.ActiveImgFolder.Id, "ArchiveZipId", bcfFileId);
                            if (imagesFound.Count > 0)
                            {
                                foreach (File file in imagesFound)
                                {
                                    bool deleted = FileManager.DeleteFile(file.Id);
                                }
                            }

                            bool bcfDeleted = FileManager.DeleteFile(bcfFileId);

                            result = true;
                            break;
                        case MessageBoxResult.No:
                            result = true;
                            break;
                        case MessageBoxResult.Cancel:
                            result = false;
                            break;
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check whether the bcf file already exists in the shared link or not.\n"+ex.Message, "Check Existing Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private string GetFolderId(string linkURL)
        {
            string strFolderId = "";
            try
            {
                string[] seperator = new string[] { "folderview?id=", "&usp=sharing" };
                string[] matchedStr = linkURL.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                if (matchedStr.Length > 1)
                {
                    strFolderId = matchedStr[1];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(linkURL+"\nFailed to extract folder Id from the URL.\n"+ex.Message, "Get Folder Id", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return strFolderId;
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
    }
}
