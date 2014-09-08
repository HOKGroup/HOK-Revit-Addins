using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.GData.Spreadsheets;
using HOK.SmartBCF.Utils;



namespace HOK.SmartBCF.GoogleUtils
{
    public static class FileManager
    {
        public static DriveService service = null;

        private static DriveService GetUserCredential()
        {
            DriveService driveService = null;
            try
            {
               
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
               new ClientSecrets
               {
                   ClientId = "756603983986-ht1lgljr5m3tn8b429fen871lfutet7d.apps.googleusercontent.com",
                   ClientSecret = "TTtpuUjaJg7SQ6Wuew3G6YH7",
               },
               new[] { DriveService.Scope.Drive, SpreadsheetsService.GSpreadsheetsService },
              "user",
               CancellationToken.None).Result;


                // Create the service.
                driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "HOK smartBCF",
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get user credential.\n" + ex.Message, "Get User Credential", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return driveService;
        }

        public static FolderHolders CreateDefaultFolders(string rootFolderId)
        {
            FolderHolders holder = new FolderHolders(rootFolderId);
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    File activeFolder = FindSubFolder("Active", rootFolderId);
                    if (null == activeFolder)
                    {
                        activeFolder = CreateSubFolder(rootFolderId, "Active", "Interactive BCF Data will be stored in this folder.");
                    }
                    if (null != activeFolder)
                    {
                        File bcfFolder = FindSubFolder("BCF_Files", activeFolder.Id);
                        if (null == bcfFolder)
                        {
                            bcfFolder = CreateSubFolder(activeFolder.Id, "BCF_Files", "Parsed BCF Data will be stored as Google Spreadsheet.");
                        }

                        File imgFolder = FindSubFolder("BCF_Images", activeFolder.Id);
                        if (null == imgFolder)
                        {
                            imgFolder = CreateSubFolder(activeFolder.Id, "BCF_Images", "Screen captured images of each issue will be stored in this folder.");
                        }

                        holder.ActiveFolder = activeFolder;
                        holder.ActiveBCFFolder = bcfFolder;
                        holder.ActiveImgFolder = imgFolder;
                    }

                    File archiveFolder = FindSubFolder("Archive", rootFolderId);
                    if (null == archiveFolder)
                    {
                        archiveFolder = CreateSubFolder(rootFolderId, "Archive", "Archived .bcfzip files will be stored in this folder.");
                    }
                    if (null != archiveFolder)
                    {
                        File bcfFolder = FindSubFolder("BCF_Files", archiveFolder.Id);
                        if (null == bcfFolder)
                        {
                            bcfFolder = CreateSubFolder(archiveFolder.Id, "BCF_Files", "Parsed BCF Data will be stored as Google Spreadsheet.");
                        }

                        File imgFolder = FindSubFolder("BCF_Images", archiveFolder.Id);
                        if (null == imgFolder)
                        {
                            imgFolder = CreateSubFolder(archiveFolder.Id, "BCF_Images", "Screen captured images of each issue will be stored in this folder.");
                        }

                        holder.ArchiveFolder = archiveFolder;
                        holder.ArchiveBCFFolder = bcfFolder;
                        holder.ArchiveImgFolder = imgFolder;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create default folders.\n"+ex.Message, "Create Default Folders", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return holder;
        }

        private static File CreateSubFolder(string parentId, string folderTitle, string description)
        {
            File subFolder = null;
            try
            {
                File body = new File();
                body.Title = folderTitle;
                body.Description = description;
                body.MimeType = "application/vnd.google-apps.folder";
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                
                FilesResource.InsertRequest request = service.Files.Insert(body);
                subFolder = request.Execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a sub folder.\n"+ex.Message, "Create a sub folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return subFolder;
        }

        public static File FindSubFolder(string folderTitle, string parentId)
        {
            File subFolder = null;
            try
            {
                FilesResource.ListRequest request = service.Files.List();
                request.Q = "title contains \'"+folderTitle+"\' and \'"+parentId+"\' in parents";
                FileList files = request.Execute();

                if (files.Items.Count > 0)
                {
                    subFolder = files.Items.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Folder Title: " + folderTitle + "  Parent Id: " + parentId + "\nFailed to find a sub folder.\n" + ex.Message, "Find Sub Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return subFolder;
        }

        public static List<File> FindFilesByProperty(string parentId, string propertyKey, string propertyValue)
        {
            List<File> filesFound = new List<File>();
            try
            {
                FilesResource.ListRequest request = service.Files.List();
                request.Q = "properties has { key= \'" + propertyKey + "\' and value=\'" + propertyValue + "\' and visibility=\'PUBLIC\' } and \'"+ parentId +"\' in parents";
                FileList files = request.Execute();

                filesFound = files.Items.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find files by property values.\n"+ex.Message, "Find Files By Property", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return filesFound;
        }

        public static bool DeleteFile(string fileId)
        {
            bool deleted = false;
            try
            {
                FilesResource.DeleteRequest request = service.Files.Delete(fileId);
                request.Execute();
                deleted = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete a file from the Google Drive.\n" + ex.Message, "Delete File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deleted;
        }

        public static File UploadBCF(string bcfPath, string parentId)
        {
            File uploadedBCF = null;
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    if (System.IO.File.Exists(bcfPath))
                    {
                        string title = System.IO.Path.GetFileName(bcfPath);
                        File body = new File();
                        body.Title = title;
                        body.Description = "Archived BCF";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                        //body.Thumbnail = GetThumbnail("bcficon64.png");

                        byte[] byteArray = System.IO.File.ReadAllBytes(bcfPath);
                        System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                     
                        FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "");
                        request.Upload();
                        IUploadProgress progress = request.GetProgress();
                        
                        while (progress.Status != UploadStatus.Completed && progress.Status!=UploadStatus.Failed)
                        {
                            System.Threading.Thread.Sleep(500);
                        }
                        if (progress.Status == UploadStatus.Completed)
                        {
                            uploadedBCF = request.ResponseBody;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(bcfPath+"\nFailed to upload a bcfzip.\n"+ex.Message, "Upload BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return uploadedBCF;
        }

        public static File CreateSpreadsheet(string bcfPath, string parentId)
        {
            File spreadsheet = null;
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    if (System.IO.File.Exists(bcfPath))
                    {
                        string title = System.IO.Path.GetFileNameWithoutExtension(bcfPath);
                        File body = new File();
                        body.Title = title;
                        body.Description = "Parsed BCF";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                        body.MimeType = "application/vnd.google-apps.spreadsheet";
                        
                        FilesResource.InsertRequest request = service.Files.Insert(body);
                        spreadsheet = request.Execute();
                       
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a spreadsheet from BCF.\n"+ex.Message, "Create Spreadsheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return spreadsheet;
        }

        public static List<File> UploadBCFImages(BCFZIP bcfzip, string parentId)
        {
            List<File> uploadedFiles = new List<File>();
            try
            {
                if (bcfzip.BCFComponents.Length > 0)
                {
                    if (null == service)
                    {
                        service = GetUserCredential();
                    }

                    if (null != service)
                    {
                        foreach (BCFComponent bcf in bcfzip.BCFComponents)
                        {
                            string imagePath = bcf.Snapshot.FilePath;
                            if (System.IO.File.Exists(imagePath))
                            {
                                File body = new File();
                                body.Title = bcf.GUID;
                                body.Description = "Topic: " + bcf.Markup.Topic.Title;
                                body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                                body.MimeType = "image/png";

                                byte[] byteArray = System.IO.File.ReadAllBytes(imagePath);
                                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

                                FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "image/png");
                                request.Upload();
                                IUploadProgress progress = request.GetProgress();
                                while (progress.Status != UploadStatus.Completed && progress.Status != UploadStatus.Failed)
                                {
                                    System.Threading.Thread.Sleep(500);
                                }

                                if (progress.Status == UploadStatus.Completed)
                                {
                                    File uploadedImage = request.ResponseBody;
                                    if (null != uploadedImage)
                                    {
                                        uploadedFiles.Add(uploadedImage);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to upload BCF Images.\n"+ex.Message, "Upload BCF Images", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return uploadedFiles;
        }

        private static File.ThumbnailData GetThumbnail(string imageName)
        {
            File.ThumbnailData thumbnail = new File.ThumbnailData();
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string prefix = typeof(AppCommand).Namespace + ".Resources.";
                System.IO.Stream stream = assembly.GetManifestResourceStream(prefix + imageName);

                Image image = Image.FromStream(stream);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    image.Save(memoryStream, ImageFormat.Png);
                    byte[] imageBytes = memoryStream.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);

                    thumbnail.Image = base64String;
                    thumbnail.MimeType = "image/png";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get thumbnail image.\n"+ex.Message, "Get Thumbnail", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return thumbnail;
        }

        public static File AddProperty(File file, string key, string value)
        {
            File updatedFile = null;
            try
            {
                bool pFound = false;
                if (null != file.Properties)
                {
                    for (int i = 0; i < file.Properties.Count; i++)
                    {
                        if (file.Properties[i].Key == key)
                        {
                            pFound = true;
                            file.Properties[i].Value = value;
                            break;
                        }
                    }
                    if (!pFound)
                    {
                        Property property = new Property() { Key = key, Value = value, Visibility="PUBLIC" };
                        file.Properties.Add(property);
                    }
                }
                else
                {
                    List<Property> properties = new List<Property>();
                    Property property = new Property() { Key = key, Value = value, Visibility="PUBLIC" };
                    properties.Add(property);

                    file.Properties = properties;
                }
                
                updatedFile = service.Files.Update(file, file.Id).Execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add property.\n"+ex.Message, "Add Properties", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedFile;
        }

        public static File AddProperties(string fileId, Dictionary<string, string> dictionary)
        {
            File updatedFile = null;
            try
            {
                File file = service.Files.Get(fileId).Execute();
                updatedFile = file;
                if (null != file)
                {
                    if (null == file.Properties)
                    {
                        file.Properties = new List<Property>();
                    }

                    foreach (string key in dictionary.Keys)
                    {
                        bool pFound = false;
                        for (int i = 0; i < file.Properties.Count; i++)
                        {
                            if (file.Properties[i].Key == key)
                            {
                                pFound = true;
                                file.Properties[i].Value = dictionary[key];
                                break;
                            }
                        }
                        if (!pFound)
                        {
                            Property property = new Property() { Key = key, Value = dictionary[key], Visibility = "PUBLIC" };
                            file.Properties.Add(property);
                        }
                    }
                    updatedFile = service.Files.Update(file, file.Id).Execute();
                }
                
                
            }
            catch (Exception ex) { string message = ex.Message; }
            return updatedFile;
        }

    }

    public class FolderHolders
    {
        private string rootId = "";
        private File activeFolder = null;
        private File activeBCFFolder = null;
        private File activeImgFolder = null;
        private File archiveFolder = null;
        private File archiveBCFFolder = null;
        private File archiveImgFolder = null;

        public string RootId { get { return rootId; } set { rootId = value; } }

        public File ActiveFolder { get { return activeFolder; } set { activeFolder = value; } }
        public File ActiveBCFFolder { get { return activeBCFFolder; } set { activeBCFFolder = value; } }
        public File ActiveImgFolder { get { return activeImgFolder; } set { activeImgFolder = value; } }

        public File ArchiveFolder { get { return archiveFolder; } set { archiveFolder = value; } }
        public File ArchiveBCFFolder { get { return archiveBCFFolder; } set { archiveBCFFolder = value; } }
        public File ArchiveImgFolder { get { return archiveImgFolder; } set { archiveImgFolder = value; } }

        public FolderHolders(string folderId)
        {
            rootId = folderId;
        }
    }

    public class FileHolders
    {
        private string bcfPath = "";
        private string sharedLink = "";
        private FolderHolders folderHolders = null;
        private BCFZIP bcfzip = null;
        private File archivedBCF = null;
        private File activeBCF = null;
        private List<File> bcfImages = null;

        public string BCFPath { get { return bcfPath; } set { bcfPath = value; } }
        public string SharedLink { get { return sharedLink; } set { sharedLink = value; } }
        public FolderHolders FolderInfo { get { return folderHolders; } set { folderHolders = value; } }
        public BCFZIP BcfZip { get { return bcfzip; } set { bcfzip = value; } }
        public File ArchivedBCF { get { return archivedBCF; } set { archivedBCF = value; } }
        public File ActiveBCF { get { return activeBCF; } set { activeBCF = value; } }
        public List<File> BCFImages { get { return bcfImages; } set { bcfImages = value; } }

        public FileHolders(string path, string link)
        {
            bcfPath = path;
            sharedLink = link;
        }
      
    }
}
