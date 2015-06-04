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
using HOK.SmartBCF.Walker;



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
                UserCredential credential;
                string currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string currentDirectory = System.IO.Path.GetDirectoryName(currentAssembly);
                string jsonPath = System.IO.Path.Combine(currentDirectory, "Resources\\client_secrets_samrtBCF.json");

                using (var filestream = new System.IO.FileStream(jsonPath,
                    System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(filestream).Secrets,
                        new[] { DriveService.Scope.Drive, },
                        "user",
                        CancellationToken.None).Result;

                }

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
                        exist = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Google Folder cannot be found by Id.\n"+ex.Message, "Google Folder Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return exist;
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

                if (null != service )
                {
                    File rootFolder = null;
                    if(RootFolderExist(rootFolderId, out rootFolder))
                    {
                        holder.RootFolder = rootFolder;
                        holder.RootTitle = rootFolder.Title;

                        File colorSheet = FindSubFolder("Color Schemes", rootFolderId);
                        if (null == colorSheet)
                        {
                            colorSheet = CreateColorSheet(rootFolderId);
                            if (null != colorSheet)
                            {
                                ColorSchemeInfo colorSchemeInfo = BCFParser.CreateDefaultSchemeInfo();
                                bool updatedColors = BCFParser.WriteColorSheet(colorSchemeInfo, colorSheet.Id);
                            }
                        }
                        holder.ColorSheet = colorSheet;

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
                ChildrenResource.ListRequest request = service.Children.List(parentId);
                request.Q = "title contains \'" + folderTitle + "\'";
                ChildList childList = request.Execute();
                if (childList.Items.Count > 0)
                {
                    string fileId = childList.Items.First().Id;
                    subFolder = service.Files.Get(fileId).Execute();
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
                    body.Description = "List of color coded status.";
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
                    foreach (File file in files.Items)
                    {
                        OnlineBCFInfo info = new OnlineBCFInfo(folders.RootId, file);
                        info.ImageFiles = FindImageFiles(file.Id, folders.ActiveImgFolder.Id);
                        info.ArchiveFile = FindArchiveZipFile(file.Id, folders.ArchiveBCFFolder.Id);
                        onlineBCFs.Add(info);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get online BCFs.\n"+ex.Message, "Get Online BCFs", MessageBoxButton.OK, MessageBoxImage.Warning);
            } 
            return onlineBCFs;
        }

        public static List<File> FindImageFiles(string sheetId, string parentId)
        {
            List<File> imageFiles = new List<File>();
            try
            {
                FilesResource.ListRequest request = service.Files.List();
                request.Q = "mimeType=\'image/png\' and \'" + parentId + "\' in parents and properties has {key=\'SheetId\' and value=\'" + sheetId + "\' and visibility=\'PUBLIC\'}";
                FileList files = request.Execute();
                if (files.Items.Count > 0)
                {
                    imageFiles = files.Items.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find image files.\n"+ex.Message, "Find Image Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return imageFiles;
        }

        public static File FindArchiveZipFile(string sheetId, string parentId)
        {
            File archiveZip = null;
            try
            {
                FilesResource.ListRequest request = service.Files.List();
                request.Q = "\'" + parentId + "\' in parents and properties has {key=\'SheetId\' and value=\'" + sheetId + "\' and visibility=\'PUBLIC\'}";
                FileList files = request.Execute();
                if (files.Items.Count > 0)
                {
                    archiveZip = files.Items.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find archive bcfzip.\n"+ex.Message, "Find Archive Zip", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return archiveZip;
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
                    if (null != folders.ActiveBCFFolder)
                    {
                        //copy spreadsheet
                        File body = new File();
                        body.Title = onlineBCF.SheetTitle;
                        body.Description = "Parsed BCF";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ActiveBCFFolder.Id } };
                        body.MimeType = "application/vnd.google-apps.spreadsheet";
                        body.Properties = onlineBCF.SpreadsheetFile.Properties;
                        File copiedFile = service.Files.Copy(body, onlineBCF.SpreadsheetId).Execute();
                        if (null != copiedFile)
                        {
                            copiedBCFInfo = new OnlineBCFInfo(folders.RootId, copiedFile);
                        }
                    }

                    if (null != folders.ArchiveBCFFolder)
                    {
                        //upload bcfzip
                        File body = new File();
                        body.Title = onlineBCF.ArchiveFile.Title;
                        body.Description = "Archived BCF";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ArchiveBCFFolder.Id } };
                        body.Properties = onlineBCF.ArchiveFile.Properties;
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
                            body.Properties = imgFile.Properties;

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

                    if (null != copiedBCFInfo)
                    {
                        //update property for cross referencing
                        if (null != copiedBCFInfo.SpreadsheetFile && null != copiedBCFInfo.ArchiveFile && copiedBCFInfo.ImageFiles.Count > 0)
                        {
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
                File fileUploaded = FileManager.FindSubFolder(fileName, folderHolder.ActiveBCFFolder.Id);
                if (null != fileUploaded)
                {
                    MessageBoxResult mbr = MessageBox.Show(fileName + " already exists in the shared Google Drive folder.\nWould you like to replace the file in the shared folders?", "File Already Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    switch (mbr)
                    {
                        case MessageBoxResult.Yes:
                            //delete the existing files first
                            string bcfFileId = fileUploaded.Id;
                            List<File> archiveFound = FileManager.FindFilesByProperty(folderHolder.ArchiveBCFFolder.Id, "SheetId", bcfFileId);
                            if (archiveFound.Count > 0)
                            {
                                foreach (File file in archiveFound)
                                {
                                    bool deleted = FileManager.DeleteFile(file.Id);
                                }
                            }
                            List<File> imagesFound = FileManager.FindFilesByProperty(folderHolder.ActiveImgFolder.Id, "SheetId", bcfFileId);
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

    }

    public class FolderHolders
    {
        private string rootId = "";
        private string rootTitle = "";
        private File rootFolder = null;
        private File colorSheet = null;
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
