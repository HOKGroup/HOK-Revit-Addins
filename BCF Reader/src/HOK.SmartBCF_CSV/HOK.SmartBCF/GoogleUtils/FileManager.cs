using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.GData.Spreadsheets;
using HOK.SmartBCF.Utils;
using HOK.SmartBCF.Walker;



namespace HOK.SmartBCF.GoogleUtils
{
    public static class FileManager
    {
        private static string keyFile = "HOK smartBCF.p12";
        private static string serviceAccountEmail = "756603983986-lrc8dm2b0nl381cepd60q2o7fo8df3bg@developer.gserviceaccount.com";
        private static string smartBCFVersion = "SmartBCF v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static string[] markupCols = new string[] { "IssueGuid", "IssueTopic", "CommentGuid", "Comment", "Status", "VerbalStatus", "Author", "Date" };
        private static string[] viewpointCols = new string[] { "IssueGuid", "ComponentIfcGuid", "AuthoringToolId", "Action", "Responsible" };
        private static string[] colorschemeCols = new string[] { "ColorSchemeId", "SchemeName", "ParameterName", "ParameterValue", "ColorR", "ColorG", "ColorB" };
        private static string[] categoryCols = new string[] { "CategoryName" };

        public static DriveService service = null;

        private static DriveService GetUserCredential()
        {
            DriveService driveService = null;
            try
            {
                string currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string currentDirectory = System.IO.Path.GetDirectoryName(currentAssembly);
                string keyFilePath = System.IO.Path.Combine(currentDirectory, "Resources\\" + keyFile);

                X509Certificate2 certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer(serviceAccountEmail)
                    {
                        Scopes = new[] { DriveService.Scope.Drive }
                    }.FromCertificate(certificate));

                // Create the service.
                driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "smartBCF",
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get user credential.\n" + ex.Message, "Get User Credential", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return driveService;
        }

        public static bool RootFolderExist(string rootFolderId, out File rootFolder)
        {
            bool exist = false;
            rootFolder = null;
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    rootFolder = service.Files.Get(rootFolderId).Execute();
                    if (null != rootFolder)
                    {
                        PermissionId permissionId = service.Permissions.GetIdForEmail(serviceAccountEmail).Execute();
                        Permission permission = null;
                        try { permission = service.Permissions.Get(rootFolderId, permissionId.Id).Execute();  }
                        catch { }

                        if (null == permission)
                        {
                            Permission newPermission = new Permission();
                            newPermission.Value = serviceAccountEmail;
                            newPermission.Type = "user";
                            newPermission.Role = "writer";
                            permission = service.Permissions.Insert(newPermission, rootFolderId).Execute();
                        }

                        if (null != permission) { exist = true; }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Google Folder cannot be found by Id.\n"+ex.Message, "Google Folder Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return exist;
        }

        public static FolderHolders FindGoogleFolders(string rootFolderId)
        {
            FolderHolders holder = new FolderHolders(rootFolderId);
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service )
                {
                    File rootFolder = null;
                    if(RootFolderExist(rootFolderId, out rootFolder))
                    {
                        holder.RootFolder = rootFolder;
                        holder.RootTitle = rootFolder.Title;

                        string colorSchemeTitle = "ColorSchemes.csv";
                        File colorSheet = FindSubItemByFolderId(colorSchemeTitle, rootFolderId);
                        if (null == colorSheet)
                        {
                            ColorSchemeInfo colorSchemeInfo = BCFParser.CreateDefaultSchemeInfo();
                            System.IO.MemoryStream colorStream = BCFParser.CreateColorSchemeStream(colorSchemeInfo);
                            if (null != colorStream)
                            {
                                colorSheet = FileManager.UploadSpreadsheet(colorStream, colorSchemeTitle, rootFolderId, rootFolderId);// UploadId as folder id
                            }
                        }
                        holder.ColorSheet = colorSheet;

                        string categorySheetTitle = "ElementCategories.csv";
                        File categorySheet = FindSubItemByFolderId(categorySheetTitle, rootFolderId);
                        if (null == categorySheet)
                        {
                            List<string> categoryNames = new List<string>();
                            System.IO.MemoryStream categoryStream = BCFParser.CreateCategoryStream(categoryNames);
                            if (null != categoryStream)
                            {
                                categorySheet = FileManager.UploadSpreadsheet(categoryStream, categorySheetTitle, rootFolderId, rootFolderId);// UploadId as folder id
                            }
                        }
                        holder.CategorySheet = categorySheet;

                        File activeFolder = FindSubItemByFolderId("Active", rootFolderId);
                        if (null == activeFolder)
                        {
                            activeFolder = CreateSubFolder(rootFolderId, "Active", "Interactive BCF Data will be stored in this folder.");
                        }
                        if (null != activeFolder)
                        {
                            File bcfFolder = FindSubItemByFolderId("BCF_Files", activeFolder.Id);
                            if (null == bcfFolder)
                            {
                                bcfFolder = CreateSubFolder(activeFolder.Id, "BCF_Files", "Parsed BCF Data will be stored as Google Spreadsheet.");
                            }

                            File imgFolder = FindSubItemByFolderId("BCF_Images", activeFolder.Id);
                            if (null == imgFolder)
                            {
                                imgFolder = CreateSubFolder(activeFolder.Id, "BCF_Images", "Screen captured images of each issue will be stored in this folder.");
                            }

                            holder.ActiveFolder = activeFolder;
                            holder.ActiveBCFFolder = bcfFolder;
                            holder.ActiveImgFolder = imgFolder;
                        }

                        File archiveFolder = FindSubItemByFolderId("Archive", rootFolderId);
                        if (null == archiveFolder)
                        {
                            archiveFolder = CreateSubFolder(rootFolderId, "Archive", "Archived .bcfzip files will be stored in this folder.");
                        }
                        if (null != archiveFolder)
                        {
                            File bcfFolder = FindSubItemByFolderId("BCF_Files", archiveFolder.Id);
                            if (null == bcfFolder)
                            {
                                bcfFolder = CreateSubFolder(archiveFolder.Id, "BCF_Files",  "Parsed BCF Data will be stored as Google Spreadsheet.");
                            }

                            File imgFolder = FindSubItemByFolderId("BCF_Images", archiveFolder.Id);
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
                body.Description = smartBCFVersion+" - "+ description;
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

        public static File FindSubItemByFolderId(string itemTitle, string parentId)
        {
            File subItem = null;
            try
            {
                FilesResource.ListRequest request = service.Files.List();
                request.Q = "title = \'" + itemTitle + "\' and \'" + parentId + "\' in parents";
                FileList files = request.Execute();

                if (files.Items.Count > 0)
                {
                    foreach (File file in files.Items)
                    {
                        if (null == file.ExplicitlyTrashed)
                        {
                            subItem = file; break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Item Title: " + itemTitle + "  Parent Id: " + parentId + "\nFailed to find a sub item.\n" + ex.Message, "Find Sub Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return subItem;
        }

        public static File FindFileById(string fileId)
        {
            File file = null;
            try
            {
                File fileFound = service.Files.Get(fileId).Execute();
                if (null == fileFound.ExplicitlyTrashed && fileFound.Parents.Count > 0)
                {
                    file = fileFound;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find a file by fileId: "+fileId+"\n"+ex.Message, "FindSubFile", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return file;
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

        public static File UploadBCF(string bcfPath, string parentId, string uploadId)
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
                        body.Description = smartBCFVersion+" - Archived BCF";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                        body.Properties = new List<Property>();
                        Property property = new Property() { Key = "UploadId", Value = uploadId, Visibility = "PUBLIC" };
                        body.Properties.Add(property);
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

        public static File UploadSpreadsheet(System.IO.MemoryStream stream, string fileName, string parentId, string uploadId)
        {
            File uploadedFile = null;
            try
            {
                string mimeType = "text/csv";
                File body = new File();
                body.Title = fileName;
                body.Description = smartBCFVersion;
                body.MimeType = mimeType;
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                body.Properties = new List<Property>();
                Property property = new Property() { Key = "UploadId", Value = uploadId, Visibility = "PUBLIC" };
                body.Properties.Add(property);

                FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, mimeType);
                request.Convert = true;
                request.Upload();

                uploadedFile = request.ResponseBody;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to upload a spreadsheet.\n." + fileName + "\n" + ex.Message, "Upload Spreadsheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return uploadedFile;
        }

        public static File UpdateSpreadsheet(System.IO.MemoryStream stream, string fileId, string parentId)
        {
            File updatedFile = null;
            try
            {
                File file = service.Files.Get(fileId).Execute();
                if (null != file)
                {
                    FilesResource.UpdateMediaUpload request = service.Files.Update(file, fileId, stream, file.MimeType);
                    request.Convert = true;
                    request.Upload();

                    updatedFile = request.ResponseBody;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update spreadsheet.\n"+ex.Message, "Update Spreadsheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedFile;
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
                        body.Description = smartBCFVersion + " - Issue Info";
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

        public static File CreateColorSheet(string parentId)
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
                    File body = new File();
                    body.Title = "Color Schemes";
                    body.Description = smartBCFVersion + " - Color Schemes";
                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                    body.MimeType = "application/vnd.google-apps.spreadsheet";

                    FilesResource.InsertRequest request = service.Files.Insert(body);
                    spreadsheet = request.Execute();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a common color schemes spreadsheet.\n" + ex.Message, "Create Color Schemes Spreadsheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return spreadsheet;
        }

        public static List<File> UploadBCFImages(BCFZIP bcfzip, string parentId, string uploadId, ProgressBar progressBar)
        {
            List<File> uploadedFiles = new List<File>();
            try
            {
                if (bcfzip.BCFComponents.Length > 0)
                {
                    HOK.SmartBCF.Walker.ImportBCFWindow.UpdateProgressDelegate updateProgressDelegate = new HOK.SmartBCF.Walker.ImportBCFWindow.UpdateProgressDelegate(progressBar.SetValue);
                    double progressValue = 0;
                    progressBar.Maximum = bcfzip.BCFComponents.Length;
                    progressBar.Value = progressValue;

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
                                if (AbortFlag.GetAbortFlag()) { return null; }

                                File body = new File();
                                body.Title = bcf.GUID;
                                body.Description = smartBCFVersion + " - Topic: " + bcf.Markup.Topic.Title;
                                body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                                body.MimeType = "image/png";
                                body.Properties = new List<Property>();
                                Property property = new Property() { Key = "UploadId", Value = uploadId, Visibility = "PUBLIC" };
                                body.Properties.Add(property);

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
                                        progressValue++;
                                        Dispatcher.CurrentDispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });
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

                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
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

        public static string GetPropertyValue(IList<Property> properties, string propertyName)
        {
            string propertyValue = "";
            try
            {
                foreach (Property p in properties)
                {
                    if (p.Key == propertyName)
                    {
                        propertyValue = p.Value; break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(propertyName + ": Failed to get a property value.\n" + ex.Message, "File Manager - Get Property Value", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return propertyValue;
        }

        public static BitmapImage DownloadImage(string issueId, string parentId)
        {
            BitmapImage bitmap = new BitmapImage();
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    File imgFile = null;
                    try
                    {
                        FilesResource.ListRequest request = service.Files.List();
                        request.Q = "title contains \'" + issueId + "\' and \'" + parentId + "\' in parents";
                        FileList files = request.Execute();

                        if (files.Items.Count > 0)
                        {
                            imgFile = files.Items.First();
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                    
                    if (null != imgFile)
                    {
                        if (!string.IsNullOrEmpty(imgFile.DownloadUrl))
                        {
                            try
                            {
                                bitmap.BeginInit();
                                System.IO.Stream stream = service.HttpClient.GetStreamAsync(imgFile.DownloadUrl).Result;
                                bitmap.StreamSource = stream;
                                bitmap.EndInit();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Failed to download image from Google Drive.\nIssue Id: " + issueId + "\n" + ex.Message, "Download Image", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to download image from Google Drive.\nIssue Id: " + issueId + "\n" + ex.Message, "Download Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bitmap;
        }

        public static List<OnlineBCFInfo> GetOnlineBCFs(FolderHolders folders)
        {
            List<OnlineBCFInfo> onlineBCFs = new List<OnlineBCFInfo>();
            try
            {
                FilesResource.ListRequest request = service.Files.List();
                request.Q = "mimeType=\'application/vnd.google-apps.spreadsheet\' and \'" + folders.ActiveBCFFolder.Id + "\' in parents";
                FileList files = request.Execute();

                if (files.Items.Count > 0)
                {
                    var markupFiles = from file in files.Items where file.Title.Contains("_Markup.csv") select file;
                    var viewpointFiles = from file in files.Items where file.Title.Contains("_Viewpoint.csv") select file;

                    if (markupFiles.Count() > 0)
                    {
                        foreach (File file in markupFiles)
                        {
                            string bcfName = file.Title.Replace("_Markup.csv", "");
                            var vpFiles = from vpFile in viewpointFiles where vpFile.Title.Contains(bcfName) select vpFile;
                            if (vpFiles.Count() > 0)
                            {
                                File viewpointSheet = null;
                                string markupUploadId = GetPropertyValue(file.Properties, "UploadId");
                                foreach(File vp in vpFiles)
                                {
                                    string vpUploadId = GetPropertyValue(vp.Properties, "UploadId");
                                    if (vpUploadId == markupUploadId)
                                    {
                                        viewpointSheet = vp; break;
                                    }
                                }

                                if (null != viewpointSheet)
                                {
                                    OnlineBCFInfo info = new OnlineBCFInfo(folders.RootId, file, viewpointSheet);
                                    info.ImageFiles = FindFilesByProperty(folders.ActiveImgFolder.Id, "UploadId", markupUploadId);
                                    List<File> archiveFiles = FindFilesByProperty(folders.ArchiveBCFFolder.Id, "UploadId", markupUploadId);
                                    if (archiveFiles.Count > 0)
                                    {
                                        info.ArchiveFile = archiveFiles.First();
                                    }
                                    onlineBCFs.Add(info);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get online BCFs.\n"+ex.Message, "Get Online BCFs", MessageBoxButton.OK, MessageBoxImage.Warning);
            } 
            return onlineBCFs;
        }

        public static OnlineBCFInfo UploadOnlineBCF(FolderHolders folders, OnlineBCFInfo onlineBCF)
        {
            OnlineBCFInfo copiedBCFInfo = null;
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }
                if (null != service)
                {
                    string uploadId = Guid.NewGuid().ToString();
                    Property property = new Property() { Key = "UploadId", Value = uploadId, Visibility = "PUBLIC" };

                    if (null != folders.ActiveBCFFolder)
                    {
                        

                        //copy markup sheet
                        File markupBody = new File();
                        markupBody.Title = onlineBCF.MarkupFile.Title;
                        markupBody.Description = smartBCFVersion + " - Parsed BCF";
                        markupBody.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ActiveBCFFolder.Id } };
                        markupBody.MimeType = "application/vnd.google-apps.spreadsheet";
                        markupBody.Properties = new List<Property>();
                        markupBody.Properties.Add(property);

                        File copiedMarkup = service.Files.Copy(markupBody, onlineBCF.MarkupSheetId).Execute();
                        

                        File viewpointBody = new File();
                        viewpointBody.Title = onlineBCF.ViewpointFile.Title;
                        viewpointBody.Description = smartBCFVersion + " - Parsed BCF";
                        viewpointBody.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ActiveBCFFolder.Id } };
                        viewpointBody.MimeType = "application/vnd.google-apps.spreadsheet";
                        viewpointBody.Properties = new List<Property>();
                        viewpointBody.Properties.Add(property);

                        File copiedViewpoint = service.Files.Copy(viewpointBody, onlineBCF.ViewpointSheetId).Execute();

                        if (null!=copiedMarkup && null!=copiedViewpoint)
                        {
                            copiedBCFInfo = new OnlineBCFInfo(folders.RootId, copiedMarkup, copiedViewpoint);
                        }
                    }

                    if (null != folders.ArchiveBCFFolder)
                    {
                        //upload bcfzip
                        File body = new File();
                        body.Title = onlineBCF.ArchiveFile.Title;
                        body.Description = smartBCFVersion + " - Archived BCF";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ArchiveBCFFolder.Id } };
                        body.Properties = new List<Property>();
                        body.Properties.Add(property);

                        File copiedZip = service.Files.Copy(body, onlineBCF.ArchiveFile.Id).Execute();
                        if (null != copiedZip && null != copiedBCFInfo)
                        {
                            copiedBCFInfo.ArchiveFile = copiedZip;
                        }
                    }

                    if (null != folders.ActiveImgFolder)
                    {
                        List<File> copiedFiles=new List<File>();
                        foreach (File imgFile in onlineBCF.ImageFiles)
                        {
                            File body = new File();
                            body.Title = imgFile.Title;
                            body.Description = imgFile.Description;
                            body.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ActiveImgFolder.Id } };
                            body.MimeType = "image/png";
                            body.Properties = new List<Property>();
                            body.Properties.Add(property);

                            File copiedImage = service.Files.Copy(body, imgFile.Id).Execute();
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

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to upload online BCF.\n"+ex.Message, "Upload Online BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return copiedBCFInfo;
        }

        public static File CopyFile(File sourceFile, string parentId)
        {
            File copiedFile = null;
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }
                if (null != service)
                {
                    File body = new File();
                    body.Title = sourceFile.Title;
                    body.Description = sourceFile.Description;
                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                    body.Properties = sourceFile.Properties;

                    copiedFile = service.Files.Copy(body, sourceFile.Id).Execute();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy file from "+sourceFile.Title+"\n"+ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return copiedFile;
        }

        public static bool CheckExistingFiles(string fileName, FolderHolders folderHolder)
        {
            bool result = false;
            try
            {
                string markupCSV = fileName + "_Markup.csv";
                string viewpointCSV = fileName + "_Viewpoint.csv";

                File markupUploaded = FileManager.FindSubItemByFolderId(markupCSV, folderHolder.ActiveBCFFolder.Id);
                File viewpointUploaded = FileManager.FindSubItemByFolderId(viewpointCSV, folderHolder.ActiveBCFFolder.Id);
                if (null != markupUploaded || null != viewpointUploaded)
                {
                    MessageBoxResult mbr = MessageBox.Show(fileName + " already exists in the shared Google Drive folder.\nWould you like to replace the file in the shared folders?", "File Already Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    switch (mbr)
                    {
                        case MessageBoxResult.Yes:
                            //delete the existing files first

                            string searchKey = "UploadId";
                            string searchValue = GetPropertyValue(markupUploaded.Properties, searchKey);

                            string markupFileId = markupUploaded.Id;
                            List<File> archiveFound = FileManager.FindFilesByProperty(folderHolder.ArchiveBCFFolder.Id, searchKey, searchValue);
                            if (archiveFound.Count > 0)
                            {
                                foreach (File file in archiveFound)
                                {
                                    bool deleted = FileManager.DeleteFile(file.Id);
                                }
                            }
                            List<File> imagesFound = FileManager.FindFilesByProperty(folderHolder.ActiveImgFolder.Id, searchKey, searchValue);
                            if (imagesFound.Count > 0)
                            {
                                foreach (File file in imagesFound)
                                {
                                    bool deleted = FileManager.DeleteFile(file.Id);
                                }
                            }

                            if (null != markupUploaded)
                            {
                                bool bcfDeleted = FileManager.DeleteFile(markupUploaded.Id);
                            }

                            if (null != viewpointUploaded)
                            {
                                bool bcfDeleted = FileManager.DeleteFile(viewpointUploaded.Id);
                            }
                            
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
                MessageBox.Show("Failed to check whether the bcf file already exists in the shared link or not.\n" + ex.Message, "Check Existing Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static string GetSharedLinkAddress(string folderId)
        {
            string sharedLink = "https://drive.google.com/folderview?id=" + folderId + "&usp=sharing";
            return sharedLink;
        }

        public static string GetFolderId(string sharedLink)
        {
            string folderId = "";
            string[] seperator = new string[] { "folderview?id=", "&usp=sharing" };
            string[] matchedStr = sharedLink.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            if (matchedStr.Length > 1)
            {
                folderId = matchedStr[1];
            }
            return folderId;
        }

        public static Dictionary<string, IssueEntry> ReadIssues(LinkedBcfFileInfo bcfFileInfo)
        {
            Dictionary<string/*issueId*/, IssueEntry> issueDictionary = new Dictionary<string, IssueEntry>();
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }
                if (null != service)
                {
                    File markupFile = service.Files.Get(bcfFileInfo.MarkupFileId).Execute();
                    if (null != markupFile)
                    {
                        if (markupFile.ExportLinks.ContainsKey("text/csv"))
                        {
                            string downloadUrl = markupFile.ExportLinks["text/csv"];
                            var x = service.HttpClient.GetByteArrayAsync(downloadUrl);
                            byte[] arrayByte = x.Result;

                            string csvString = System.Text.Encoding.UTF8.GetString(arrayByte);
                            string[] rows = csvString.Split(new string[] { "\n" }, StringSplitOptions.None);
                            if (rows.Length > 1)
                            {
                                for (int i = 1; i < rows.Length; i++)
                                {
                                    string[] cells = rows[i].Split(new char[] { ',' });
                                    if (cells.Length == 8)
                                    {
                                        IssueEntry issueEntry = new IssueEntry();
                                        issueEntry.BCFName = bcfFileInfo.BCFName;

                                        string issueId = cells[0];
                                        string issueTopic = cells[1];
                                        string commentId = cells[2];
                                        string commentStr = cells[3];
                                        string status = cells[4];
                                        string verbalStatus = cells[5];
                                        string author = cells[6];
                                        string date = cells[7];

                                        issueEntry.IssueId = issueId;
                                        issueEntry.IssueTopic = issueTopic;

                                        Comment comment = new Comment();
                                        comment.Topic.Guid = issueId;
                                        comment.Guid = commentId;
                                        comment.Comment1 = commentStr;
                                        comment.Status = status;
                                        comment.VerbalStatus = verbalStatus;
                                        comment.Author = author;
                                        comment.Date = DateTime.Parse(date);

                                        Dictionary<string, CellAddress> cellEntries = new Dictionary<string, CellAddress>();
                                        for (int j = 0; j < markupCols.Length; j++)
                                        {
                                            CellAddress cell = new CellAddress((uint)(i+1), (uint)(j+1));
                                            cellEntries.Add(markupCols[j], cell);
                                        }
                                        comment.CellEntries = cellEntries;

                                        if (!issueDictionary.ContainsKey(issueEntry.IssueId))
                                        {
                                            issueEntry.CommentDictionary.Add(comment.Guid, comment);
                                            issueDictionary.Add(issueEntry.IssueId, issueEntry);
                                        }
                                        else
                                        {
                                            issueDictionary[issueEntry.IssueId].CommentDictionary.Add(comment.Guid, comment);
                                        }
                                    }
                                }
                            }
                        }
                    }


                    if (issueDictionary.Count > 0)
                    {
                        File viewpointFile = service.Files.Get(bcfFileInfo.ViewpointFileId).Execute();
                        if (null != viewpointFile)
                        {
                            if (viewpointFile.ExportLinks.ContainsKey("text/csv"))
                            {
                                string downloadUrl = viewpointFile.ExportLinks["text/csv"];
                                var x = service.HttpClient.GetByteArrayAsync(downloadUrl);
                                byte[] arrayByte = x.Result;

                                string csvString = System.Text.Encoding.UTF8.GetString(arrayByte);
                                string[] rows = csvString.Split(new string[] { "\n" }, StringSplitOptions.None);

                                if (rows.Length > 1)
                                {
                                    for (int i = 1; i < rows.Length; i++)
                                    {
                                        string[] cells = rows[i].Split(new char[] { ',' });
                                        if (cells.Length == 5)
                                        {
                                            string issueId = cells[0];
                                            int elementId = int.Parse(cells[2]);
                                            string action = cells[3];
                                            string responsibleParty = cells[4];

                                            ElementProperties ep = new ElementProperties(elementId);
                                            ep.IssueId = issueId;
                                            ep.Action = action;
                                            ep.ResponsibleParty = responsibleParty;

                                            Dictionary<string, CellAddress> cellEntries = new Dictionary<string, CellAddress>();
                                            for (int j = 0; j < viewpointCols.Length; j++)
                                            {
                                                CellAddress cell = new CellAddress((uint)(i+1), (uint)(j+1));
                                                cellEntries.Add(viewpointCols[j], cell);
                                            }
                                            ep.CellEntries = cellEntries;

                                            if (issueDictionary.ContainsKey(issueId))
                                            {
                                                if (!issueDictionary[issueId].ElementDictionary.ContainsKey(elementId))
                                                {
                                                    issueDictionary[issueId].ElementDictionary.Add(ep.ElementId, ep);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read issues from Google spreadsheet.\n" + ex.Message, "Read Issues", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return issueDictionary;
        }

        public static ColorSchemeInfo ReadColorSchemes(string colorSheetId, string categorySheetId, bool isImported)
        {
            ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    File categoryFile = service.Files.Get(categorySheetId).Execute();
                    List<string> categoryNames = new List<string>();
                    if (null != categoryFile)
                    {
                        if (categoryFile.ExportLinks.ContainsKey("text/csv"))
                        {
                            string downloadUrl = categoryFile.ExportLinks["text/csv"];
                            var x = service.HttpClient.GetByteArrayAsync(downloadUrl);
                            byte[] arrayByte = x.Result;

                            string csvString = System.Text.Encoding.UTF8.GetString(arrayByte);
                            string[] rows = csvString.Split(new string[] { "\n" }, StringSplitOptions.None);

                            if (rows.Length > 1)
                            {
                                for (int i = 1; i < rows.Length; i++)
                                {
                                    if (!categoryNames.Contains(rows[i]))
                                    {
                                        categoryNames.Add(rows[i]);
                                    }
                                }
                            }
                        }
                    }

                    File colorSchemeFile = service.Files.Get(colorSheetId).Execute();
                    if (null != colorSchemeFile)
                    {
                        if (colorSchemeFile.ExportLinks.ContainsKey("text/csv"))
                        {
                            string downloadUrl = colorSchemeFile.ExportLinks["text/csv"];
                            var x = service.HttpClient.GetByteArrayAsync(downloadUrl);
                            byte[] arrayByte = x.Result;

                            string csvString = System.Text.Encoding.UTF8.GetString(arrayByte);
                            string[] rows = csvString.Split(new string[] { "\n" }, StringSplitOptions.None);

                            if (rows.Length > 1)
                            {
                                for (int i = 1; i < rows.Length; i++)
                                {
                                    string[] cells = rows[i].Split(new char[] { ',' });
                                    if (cells.Length == 7)
                                    {
                                        string schemeId = cells[0];
                                        string schemeName = cells[1];
                                        string paramName = cells[2];
                                        string paramValue = cells[3];
                                        byte[] colorBytes = new byte[3];
                                        colorBytes[0] = byte.Parse(cells[4]);
                                        colorBytes[1] = byte.Parse(cells[5]);
                                        colorBytes[2] = byte.Parse(cells[6]);

                                        Dictionary<string, CellAddress> cellEntries = new Dictionary<string, CellAddress>();
                                        for (int j = 0; j < colorschemeCols.Length; j++)
                                        {
                                            CellAddress cell = new CellAddress((uint)(i+1), (uint)(j+1));
                                            cellEntries.Add(colorschemeCols[j], cell);
                                        }
                                       
                                        int colorschemeIndex = schemeInfo.ColorSchemes.FindIndex(o => o.SchemeId == schemeId);
                                        if (colorschemeIndex > -1)
                                        {
                                            ColorDefinition cd = new ColorDefinition();
                                            cd.ParameterValue = paramValue;
                                            cd.Color = colorBytes;

                                            System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
                                            cd.BackgroundColor = new SolidColorBrush(windowColor);
                                            cd.CellEntries = cellEntries;

                                            schemeInfo.ColorSchemes[colorschemeIndex].ColorDefinitions.Add(cd);
                                        }
                                        else
                                        {
                                            ColorScheme scheme = new ColorScheme();
                                            scheme.SchemeId = schemeId;
                                            scheme.SchemeName = schemeName;
                                            scheme.Categories = categoryNames;
                                            scheme.ParameterName = paramName;
                                            scheme.DefinitionBy = DefinitionType.ByValue;

                                            ColorDefinition cd = new ColorDefinition();
                                            cd.ParameterValue = paramValue;
                                            cd.Color = colorBytes;

                                            System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
                                            cd.BackgroundColor = new SolidColorBrush(windowColor);
                                            cd.CellEntries = cellEntries;

                                            scheme.ColorDefinitions.Add(cd);
                                            schemeInfo.ColorSchemes.Add(scheme);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read color schemes.\n" + ex.Message, "Read Color Schemes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schemeInfo;
        }
    }

    public class FolderHolders
    {
        private string rootId = "";
        private string rootTitle = "";
        private File rootFolder = null;
        private File colorSheet = null;
        private File categorySheet = null;
        private File activeFolder = null;
        private File activeBCFFolder = null;
        private File activeImgFolder = null;
        private File archiveFolder = null;
        private File archiveBCFFolder = null;
        private File archiveImgFolder = null;

        public string RootId { get { return rootId; } set { rootId = value; } }
        public string RootTitle { get { return rootTitle; } set { rootTitle = value; } }
        public File RootFolder { get { return rootFolder; } set { rootFolder = value; } }

        public File ColorSheet { get { return colorSheet; } set { colorSheet = value; } }
        public File CategorySheet { get { return categorySheet; } set { categorySheet = value; } }

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
