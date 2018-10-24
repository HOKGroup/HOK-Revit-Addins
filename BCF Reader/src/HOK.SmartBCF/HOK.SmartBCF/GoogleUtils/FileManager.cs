#region References

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using HOK.Core.Utilities;
using HOK.SmartBCF.Utils;
using HOK.SmartBCF.Walker;
using File = Google.Apis.Drive.v2.Data.File;

#endregion

namespace HOK.SmartBCF.GoogleUtils
{
    public static class FileManager
    {
        private const string keyFile = "HOK smartBCF.p12";
        private const string serviceAccountEmail = "756603983986-lrc8dm2b0nl381cepd60q2o7fo8df3bg@developer.gserviceaccount.com";
        private static readonly string smartBCFVersion = "SmartBCF v." + Assembly.GetExecutingAssembly().GetName().Version;

        private static string[] markupCols =
        {
            "IssueGuid",
            "IssueTopic",
            "CommentGuid",
            "Comment",
            "Status",
            "VerbalStatus",
            "Author",
            "Date"
        };
        private static string[] viewpointCols =
        {
            "IssueGuid",
            "ComponentIfcGuid",
            "AuthoringToolId",
            "Action",
            "Responsible"
        };
        private static string[] colorschemeCols =
        {
            "ColorSchemeId",
            "SchemeName",
            "ParameterName",
            "ParameterValue",
            "ColorR",
            "ColorG",
            "ColorB"
        };
        private static string[] categoryCols =
        {
            "CategoryName"
        };

        public static DriveService service;

        /// <summary>
        /// Retrieves google user credentials embedded in the Resources.
        /// </summary>
        /// <returns>Drive Service or null if failed to validate credentials.</returns>
        private static DriveService GetUserCredential()
        {
            DriveService driveService = null;
            try
            {
                var currentAssembly = Assembly.GetExecutingAssembly().Location;
                var currentDirectory = Path.GetDirectoryName(currentAssembly);
                var keyFilePath = Path.Combine(currentDirectory, "Resources\\" + keyFile);
                var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);
                var credential = new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer(serviceAccountEmail)
                    {
                        Scopes = new[] { DriveService.Scope.Drive }
                    }.FromCertificate(certificate));

                // (Jinsol) Create the service.
                driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "smartBCF",
                });
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to get user credential.\n" + ex.Message, "Get User Credential",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return driveService;
        }

        public static bool RootFolderExist(string rootFolderId, out File rootFolder)
        {
            var exist = false;
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
                        var permissionId = service.Permissions.GetIdForEmail(serviceAccountEmail).Execute();
                        Permission permission = null;
                        try
                        {
                            permission = service.Permissions.Get(rootFolderId, permissionId.Id).Execute();
                        }
                        catch
                        {
                            // ignored
                        }

                        if (null == permission)
                        {
                            var newPermission = new Permission
                            {
                                Value = serviceAccountEmail,
                                Type = "user",
                                Role = "writer"
                            };
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootFolderId"></param>
        /// <returns></returns>
        public static FolderHolders FindGoogleFolders(string rootFolderId)
        {
            var holder = new FolderHolders(rootFolderId);
            try
            {
                if (service == null) service = GetUserCredential();
                File rootFolder;
                if (service != null)
                {
                    if(RootFolderExist(rootFolderId, out rootFolder))
                    {
                        holder.RootFolder = rootFolder;
                        holder.RootTitle = rootFolder.Title;

                        const string colorSchemeTitle = "ColorSchemes.csv";
                        var colorSheet = FindSubItemByFolderId(colorSchemeTitle, rootFolderId);
                        if (colorSheet == null)
                        {
                            var colorSchemeInfo = BCFParser.CreateDefaultSchemeInfo();
                            var colorStream = BCFParser.CreateColorSchemeStream(colorSchemeInfo);
                            if (colorStream != null)
                            {
                                colorSheet = UploadSpreadsheet(colorStream, colorSchemeTitle, rootFolderId, rootFolderId);// UploadId as folder id
                            }
                        }
                        holder.ColorSheet = colorSheet;

                        var categorySheetTitle = "ElementCategories.csv";
                        var categorySheet = FindSubItemByFolderId(categorySheetTitle, rootFolderId);
                        if (null == categorySheet)
                        {
                            var categoryNames = new List<string>();
                            var categoryStream = BCFParser.CreateCategoryStream(categoryNames);
                            if (null != categoryStream)
                            {
                                categorySheet = UploadSpreadsheet(categoryStream, categorySheetTitle, rootFolderId, rootFolderId);// UploadId as folder id
                            }
                        }
                        holder.CategorySheet = categorySheet;

                        var activeFolder = FindSubItemByFolderId("Active", rootFolderId);
                        if (null == activeFolder)
                        {
                            activeFolder = CreateSubFolder(rootFolderId, "Active", "Interactive BCF Data will be stored in this folder.");
                        }
                        if (null != activeFolder)
                        {
                            var bcfFolder = FindSubItemByFolderId("BCF_Files", activeFolder.Id);
                            if (null == bcfFolder)
                            {
                                bcfFolder = CreateSubFolder(activeFolder.Id, "BCF_Files", "Parsed BCF Data will be stored as Google Spreadsheet.");
                            }

                            var imgFolder = FindSubItemByFolderId("BCF_Images", activeFolder.Id);
                            if (null == imgFolder)
                            {
                                imgFolder = CreateSubFolder(activeFolder.Id, "BCF_Images", "Screen captured images of each issue will be stored in this folder.");
                            }

                            holder.ActiveFolder = activeFolder;
                            holder.ActiveBCFFolder = bcfFolder;
                            holder.ActiveImgFolder = imgFolder;
                        }

                        var archiveFolder = FindSubItemByFolderId("Archive", rootFolderId);
                        if (null == archiveFolder)
                        {
                            archiveFolder = CreateSubFolder(rootFolderId, "Archive", "Archived .bcfzip files will be stored in this folder.");
                        }
                        if (null != archiveFolder)
                        {
                            var bcfFolder = FindSubItemByFolderId("BCF_Files", archiveFolder.Id);
                            if (null == bcfFolder)
                            {
                                bcfFolder = CreateSubFolder(archiveFolder.Id, "BCF_Files",  "Parsed BCF Data will be stored as Google Spreadsheet.");
                            }

                            var imgFolder = FindSubItemByFolderId("BCF_Images", archiveFolder.Id);
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
                var body = new File();
                body.Title = folderTitle;
                body.Description = smartBCFVersion+" - "+ description;
                body.MimeType = "application/vnd.google-apps.folder";
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                
                var request = service.Files.Insert(body);
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
                var request = service.Files.List();
                request.Q = "title = \'" + itemTitle + "\' and \'" + parentId + "\' in parents";
                var files = request.Execute();

                if (files.Items.Count > 0)
                {
                    foreach (var file in files.Items)
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
                var fileFound = service.Files.Get(fileId).Execute();
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
            var filesFound = new List<File>();
            try
            {
                var request = service.Files.List();
                request.Q = "properties has { key= \'" + propertyKey + "\' and value=\'" + propertyValue + "\' and visibility=\'PUBLIC\' } and \'"+ parentId +"\' in parents";
                var files = request.Execute();

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
            var deleted = false;
            try
            {
                var request = service.Files.Delete(fileId);
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
                        var title = System.IO.Path.GetFileName(bcfPath);
                        var body = new File();
                        body.Title = title;
                        body.Description = smartBCFVersion+" - Archived BCF";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                        body.Properties = new List<Property>();
                        var property = new Property() { Key = "UploadId", Value = uploadId, Visibility = "PUBLIC" };
                        body.Properties.Add(property);
                        //body.Thumbnail = GetThumbnail("bcficon64.png");

                        var byteArray = System.IO.File.ReadAllBytes(bcfPath);
                        var stream = new System.IO.MemoryStream(byteArray);
                     
                        var request = service.Files.Insert(body, stream, "");
                        request.Upload();
                        var progress = request.GetProgress();
                        
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

        /// <summary>
        /// Uploads Spreadsheet file to Google Drive.
        /// </summary>
        /// <param name="stream">Memory Stream of the resource to be uploaded.</param>
        /// <param name="fileName">File Name/Title for the Spreadsheet.</param>
        /// <param name="parentId"></param>
        /// <param name="uploadId"></param>
        /// <returns>Google Drive File response if uploaded, otherwise null.</returns>
        public static File UploadSpreadsheet(MemoryStream stream, string fileName, string parentId, string uploadId)
        {
            File uploadedFile = null;
            try
            {
                const string mimeType = "text/csv";
                var body = new File
                {
                    Title = fileName,
                    Description = smartBCFVersion,
                    MimeType = mimeType,
                    Parents = new List<ParentReference> { new ParentReference { Id = parentId } },
                    Properties = new List<Property> { new Property { Key = "UploadId", Value = uploadId, Visibility = "PUBLIC" } }
                };

                var request = service.Files.Insert(body, stream, mimeType);
                request.Convert = true;
                request.Upload();

                uploadedFile = request.ResponseBody;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to upload a spreadsheet.\n." + fileName + "\n" + ex.Message,
                    "Upload Spreadsheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return uploadedFile;
        }

        public static File UpdateSpreadsheet(System.IO.MemoryStream stream, string fileId, string parentId)
        {
            File updatedFile = null;
            try
            {
                var file = service.Files.Get(fileId).Execute();
                if (null != file)
                {
                    var request = service.Files.Update(file, fileId, stream, file.MimeType);
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
                        var title = System.IO.Path.GetFileNameWithoutExtension(bcfPath);
                        var body = new File();
                        body.Title = title;
                        body.Description = smartBCFVersion + " - Issue Info";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                        body.MimeType = "application/vnd.google-apps.spreadsheet";

                        var request = service.Files.Insert(body);
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
                    var body = new File();
                    body.Title = "Color Schemes";
                    body.Description = smartBCFVersion + " - Color Schemes";
                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                    body.MimeType = "application/vnd.google-apps.spreadsheet";

                    var request = service.Files.Insert(body);
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
            var uploadedFiles = new List<File>();
            try
            {
                if (bcfzip.BCFComponents.Length > 0)
                {
                    var updateProgressDelegate = new ImportBCFWindow.UpdateProgressDelegate(progressBar.SetValue);
                    double progressValue = 0;
                    progressBar.Maximum = bcfzip.BCFComponents.Length;
                    progressBar.Value = progressValue;

                    if (null == service)
                    {
                        service = GetUserCredential();
                    }

                    if (null != service)
                    {
                        foreach (var bcf in bcfzip.BCFComponents)
                        {
                            var imagePath = bcf.Snapshot.FilePath;
                            if (System.IO.File.Exists(imagePath))
                            {
                                if (AbortFlag.GetAbortFlag()) { return null; }

                                var body = new File();
                                body.Title = bcf.GUID;
                                body.Description = smartBCFVersion + " - Topic: " + bcf.Markup.Topic.Title;
                                body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                                body.MimeType = "image/png";
                                body.Properties = new List<Property>();
                                var property = new Property() { Key = "UploadId", Value = uploadId, Visibility = "PUBLIC" };
                                body.Properties.Add(property);

                                var byteArray = System.IO.File.ReadAllBytes(imagePath);
                                var stream = new System.IO.MemoryStream(byteArray);

                                var request = service.Files.Insert(body, stream, "image/png");
                                request.Upload();
                                var progress = request.GetProgress();
                                while (progress.Status != UploadStatus.Completed && progress.Status != UploadStatus.Failed)
                                {
                                    System.Threading.Thread.Sleep(500);
                                }

                                if (progress.Status == UploadStatus.Completed)
                                {
                                    var uploadedImage = request.ResponseBody;
                                    if (null != uploadedImage)
                                    {
                                        uploadedFiles.Add(uploadedImage);
                                        progressValue++;
                                        Dispatcher.CurrentDispatcher.Invoke(updateProgressDelegate, DispatcherPriority.Background, new object[] { RangeBase.ValueProperty, progressValue });
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
            var thumbnail = new File.ThumbnailData();
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var prefix = typeof(AppCommand).Namespace + ".Resources.";
                var stream = assembly.GetManifestResourceStream(prefix + imageName);

                var image = System.Drawing.Image.FromStream(stream);
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    image.Save(memoryStream, ImageFormat.Png);
                    var imageBytes = memoryStream.ToArray();
                    var base64String = Convert.ToBase64String(imageBytes);

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
                var pFound = false;
                if (null != file.Properties)
                {
                    for (var i = 0; i < file.Properties.Count; i++)
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
                        var property = new Property() { Key = key, Value = value, Visibility="PUBLIC" };
                        file.Properties.Add(property);
                    }
                }
                else
                {
                    var properties = new List<Property>();
                    var property = new Property() { Key = key, Value = value, Visibility="PUBLIC" };
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
                var file = service.Files.Get(fileId).Execute();
                updatedFile = file;
                if (null != file)
                {
                    if (null == file.Properties)
                    {
                        file.Properties = new List<Property>();
                    }

                    foreach (var key in dictionary.Keys)
                    {
                        var pFound = false;
                        for (var i = 0; i < file.Properties.Count; i++)
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
                            var property = new Property() { Key = key, Value = dictionary[key], Visibility = "PUBLIC" };
                            file.Properties.Add(property);
                        }
                    }
                    updatedFile = service.Files.Update(file, file.Id).Execute();
                }
            }
            catch (Exception ex) { var message = ex.Message; }
            return updatedFile;
        }

        public static string GetPropertyValue(IList<Property> properties, string propertyName)
        {
            var propertyValue = "";
            try
            {
                foreach (var p in properties)
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
            var bitmap = new BitmapImage();
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
                        var request = service.Files.List();
                        request.Q = "title contains \'" + issueId + "\' and \'" + parentId + "\' in parents";
                        var files = request.Execute();

                        if (files.Items.Count > 0)
                        {
                            imgFile = files.Items.First();
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = ex.Message;
                    }
                    
                    if (null != imgFile)
                    {
                        if (!string.IsNullOrEmpty(imgFile.DownloadUrl))
                        {
                            try
                            {
                                bitmap.BeginInit();
                                var stream = service.HttpClient.GetStreamAsync(imgFile.DownloadUrl).Result;
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
            var onlineBCFs = new List<OnlineBCFInfo>();
            try
            {
                var request = service.Files.List();
                request.Q = "mimeType=\'application/vnd.google-apps.spreadsheet\' and \'" + folders.ActiveBCFFolder.Id + "\' in parents";
                var files = request.Execute();

                if (files.Items.Count > 0)
                {
                    var markupFiles = from file in files.Items where file.Title.Contains("_Markup.csv") select file;
                    var viewpointFiles = from file in files.Items where file.Title.Contains("_Viewpoint.csv") select file;

                    if (markupFiles.Count() > 0)
                    {
                        foreach (var file in markupFiles)
                        {
                            var bcfName = file.Title.Replace("_Markup.csv", "");
                            var vpFiles = from vpFile in viewpointFiles where vpFile.Title.Contains(bcfName) select vpFile;
                            if (vpFiles.Count() > 0)
                            {
                                File viewpointSheet = null;
                                var markupUploadId = GetPropertyValue(file.Properties, "UploadId");
                                foreach(var vp in vpFiles)
                                {
                                    var vpUploadId = GetPropertyValue(vp.Properties, "UploadId");
                                    if (vpUploadId == markupUploadId)
                                    {
                                        viewpointSheet = vp; break;
                                    }
                                }

                                if (null != viewpointSheet)
                                {
                                    var info = new OnlineBCFInfo(folders.RootId, file, viewpointSheet);
                                    info.ImageFiles = FindFilesByProperty(folders.ActiveImgFolder.Id, "UploadId", markupUploadId);
                                    var archiveFiles = FindFilesByProperty(folders.ArchiveBCFFolder.Id, "UploadId", markupUploadId);
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
                    var uploadId = Guid.NewGuid().ToString();
                    var property = new Property() { Key = "UploadId", Value = uploadId, Visibility = "PUBLIC" };

                    if (null != folders.ActiveBCFFolder)
                    {
                        

                        //copy markup sheet
                        var markupBody = new File();
                        markupBody.Title = onlineBCF.MarkupFile.Title;
                        markupBody.Description = smartBCFVersion + " - Parsed BCF";
                        markupBody.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ActiveBCFFolder.Id } };
                        markupBody.MimeType = "application/vnd.google-apps.spreadsheet";
                        markupBody.Properties = new List<Property>();
                        markupBody.Properties.Add(property);

                        var copiedMarkup = service.Files.Copy(markupBody, onlineBCF.MarkupSheetId).Execute();
                        

                        var viewpointBody = new File();
                        viewpointBody.Title = onlineBCF.ViewpointFile.Title;
                        viewpointBody.Description = smartBCFVersion + " - Parsed BCF";
                        viewpointBody.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ActiveBCFFolder.Id } };
                        viewpointBody.MimeType = "application/vnd.google-apps.spreadsheet";
                        viewpointBody.Properties = new List<Property>();
                        viewpointBody.Properties.Add(property);

                        var copiedViewpoint = service.Files.Copy(viewpointBody, onlineBCF.ViewpointSheetId).Execute();

                        if (null!=copiedMarkup && null!=copiedViewpoint)
                        {
                            copiedBCFInfo = new OnlineBCFInfo(folders.RootId, copiedMarkup, copiedViewpoint);
                        }
                    }

                    if (null != folders.ArchiveBCFFolder)
                    {
                        //upload bcfzip
                        var body = new File();
                        body.Title = onlineBCF.ArchiveFile.Title;
                        body.Description = smartBCFVersion + " - Archived BCF";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ArchiveBCFFolder.Id } };
                        body.Properties = new List<Property>();
                        body.Properties.Add(property);

                        var copiedZip = service.Files.Copy(body, onlineBCF.ArchiveFile.Id).Execute();
                        if (null != copiedZip && null != copiedBCFInfo)
                        {
                            copiedBCFInfo.ArchiveFile = copiedZip;
                        }
                    }

                    if (null != folders.ActiveImgFolder)
                    {
                        var copiedFiles=new List<File>();
                        foreach (var imgFile in onlineBCF.ImageFiles)
                        {
                            var body = new File();
                            body.Title = imgFile.Title;
                            body.Description = imgFile.Description;
                            body.Parents = new List<ParentReference>() { new ParentReference() { Id = folders.ActiveImgFolder.Id } };
                            body.MimeType = "image/png";
                            body.Properties = new List<Property>();
                            body.Properties.Add(property);

                            var copiedImage = service.Files.Copy(body, imgFile.Id).Execute();
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
                    var body = new File();
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
            var result = false;
            try
            {
                var markupCSV = fileName + "_Markup.csv";
                var viewpointCSV = fileName + "_Viewpoint.csv";

                var markupUploaded = FindSubItemByFolderId(markupCSV, folderHolder.ActiveBCFFolder.Id);
                var viewpointUploaded = FindSubItemByFolderId(viewpointCSV, folderHolder.ActiveBCFFolder.Id);
                if (null != markupUploaded || null != viewpointUploaded)
                {
                    var mbr = MessageBox.Show(fileName + " already exists in the shared Google Drive folder.\nWould you like to replace the file in the shared folders?", "File Already Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    switch (mbr)
                    {
                        case MessageBoxResult.Yes:
                            //delete the existing files first

                            var searchKey = "UploadId";
                            var searchValue = GetPropertyValue(markupUploaded.Properties, searchKey);

                            var markupFileId = markupUploaded.Id;
                            var archiveFound = FindFilesByProperty(folderHolder.ArchiveBCFFolder.Id, searchKey, searchValue);
                            if (archiveFound.Count > 0)
                            {
                                foreach (var file in archiveFound)
                                {
                                    var deleted = DeleteFile(file.Id);
                                }
                            }
                            var imagesFound = FindFilesByProperty(folderHolder.ActiveImgFolder.Id, searchKey, searchValue);
                            if (imagesFound.Count > 0)
                            {
                                foreach (var file in imagesFound)
                                {
                                    var deleted = DeleteFile(file.Id);
                                }
                            }

                            if (null != markupUploaded)
                            {
                                var bcfDeleted = DeleteFile(markupUploaded.Id);
                            }

                            if (null != viewpointUploaded)
                            {
                                var bcfDeleted = DeleteFile(viewpointUploaded.Id);
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
            var sharedLink = "https://drive.google.com/folderview?id=" + folderId + "&usp=sharing";
            return sharedLink;
        }

        public static string GetFolderId(string sharedLink)
        {
            var folderId = "";
            string[] seperator = { "folderview?id=", "&usp=sharing" };
            var matchedStr = sharedLink.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            if (matchedStr.Length > 1)
            {
                folderId = matchedStr[1];
            }
            return folderId;
        }

        public static Dictionary<string, IssueEntry> ReadIssues(LinkedBcfFileInfo bcfFileInfo)
        {
            var issueDictionary = new Dictionary<string, IssueEntry>();
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }
                if (null != service)
                {
                    var markupFile = service.Files.Get(bcfFileInfo.MarkupFileId).Execute();
                    if (null != markupFile)
                    {
                        if (markupFile.ExportLinks.ContainsKey("text/csv"))
                        {
                            var downloadUrl = markupFile.ExportLinks["text/csv"];
                            var x = service.HttpClient.GetByteArrayAsync(downloadUrl);
                            var arrayByte = x.Result;

                            var csvString = System.Text.Encoding.UTF8.GetString(arrayByte);
                            var rows = csvString.Split(new string[] { "\n" }, StringSplitOptions.None);
                            if (rows.Length > 1)
                            {
                                for (var i = 1; i < rows.Length; i++)
                                {
                                    var cells = rows[i].Split(new char[] { ',' });
                                    if (cells.Length == 8)
                                    {
                                        var issueEntry = new IssueEntry();
                                        issueEntry.BCFName = bcfFileInfo.BCFName;

                                        var issueId = cells[0];
                                        var issueTopic = cells[1];
                                        var commentId = cells[2];
                                        var commentStr = cells[3];
                                        var status = cells[4];
                                        var verbalStatus = cells[5];
                                        var author = cells[6];
                                        var date = cells[7];

                                        issueEntry.IssueId = issueId;
                                        issueEntry.IssueTopic = issueTopic;

                                        var comment = new Comment();
                                        comment.Topic.Guid = issueId;
                                        comment.Guid = commentId;
                                        comment.Comment1 = commentStr;
                                        comment.Status = status;
                                        comment.VerbalStatus = verbalStatus;
                                        comment.Author = author;
                                        comment.Date = DateTime.Parse(date);

                                        var cellEntries = new Dictionary<string, CellAddress>();
                                        for (var j = 0; j < markupCols.Length; j++)
                                        {
                                            var cell = new CellAddress((uint)(i+1), (uint)(j+1));
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
                        var viewpointFile = service.Files.Get(bcfFileInfo.ViewpointFileId).Execute();
                        if (null != viewpointFile)
                        {
                            if (viewpointFile.ExportLinks.ContainsKey("text/csv"))
                            {
                                var downloadUrl = viewpointFile.ExportLinks["text/csv"];
                                var x = service.HttpClient.GetByteArrayAsync(downloadUrl);
                                var arrayByte = x.Result;

                                var csvString = System.Text.Encoding.UTF8.GetString(arrayByte);
                                var rows = csvString.Split(new string[] { "\n" }, StringSplitOptions.None);

                                if (rows.Length > 1)
                                {
                                    for (var i = 1; i < rows.Length; i++)
                                    {
                                        var cells = rows[i].Split(new char[] { ',' });
                                        if (cells.Length == 5)
                                        {
                                            var issueId = cells[0];
                                            var elementId = int.Parse(cells[2]);
                                            var action = cells[3];
                                            var responsibleParty = cells[4];

                                            var ep = new ElementProperties(elementId);
                                            ep.IssueId = issueId;
                                            ep.Action = action;
                                            ep.ResponsibleParty = responsibleParty;

                                            var cellEntries = new Dictionary<string, CellAddress>();
                                            for (var j = 0; j < viewpointCols.Length; j++)
                                            {
                                                var cell = new CellAddress((uint)(i+1), (uint)(j+1));
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
            var schemeInfo = new ColorSchemeInfo();
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    var categoryFile = service.Files.Get(categorySheetId).Execute();
                    var categoryNames = new List<string>();
                    if (null != categoryFile)
                    {
                        if (categoryFile.ExportLinks.ContainsKey("text/csv"))
                        {
                            var downloadUrl = categoryFile.ExportLinks["text/csv"];
                            var x = service.HttpClient.GetByteArrayAsync(downloadUrl);
                            var arrayByte = x.Result;

                            var csvString = System.Text.Encoding.UTF8.GetString(arrayByte);
                            var rows = csvString.Split(new string[] { "\n" }, StringSplitOptions.None);

                            if (rows.Length > 1)
                            {
                                for (var i = 1; i < rows.Length; i++)
                                {
                                    if (!categoryNames.Contains(rows[i]))
                                    {
                                        categoryNames.Add(rows[i]);
                                    }
                                }
                            }
                        }
                    }

                    var colorSchemeFile = service.Files.Get(colorSheetId).Execute();
                    if (null != colorSchemeFile)
                    {
                        if (colorSchemeFile.ExportLinks.ContainsKey("text/csv"))
                        {
                            var downloadUrl = colorSchemeFile.ExportLinks["text/csv"];
                            var x = service.HttpClient.GetByteArrayAsync(downloadUrl);
                            var arrayByte = x.Result;

                            var csvString = System.Text.Encoding.UTF8.GetString(arrayByte);
                            var rows = csvString.Split(new string[] { "\n" }, StringSplitOptions.None);

                            if (rows.Length > 1)
                            {
                                for (var i = 1; i < rows.Length; i++)
                                {
                                    var cells = rows[i].Split(new char[] { ',' });
                                    if (cells.Length == 7)
                                    {
                                        var schemeId = cells[0];
                                        var schemeName = cells[1];
                                        var paramName = cells[2];
                                        var paramValue = cells[3];
                                        var colorBytes = new byte[3];
                                        colorBytes[0] = byte.Parse(cells[4]);
                                        colorBytes[1] = byte.Parse(cells[5]);
                                        colorBytes[2] = byte.Parse(cells[6]);

                                        var cellEntries = new Dictionary<string, CellAddress>();
                                        for (var j = 0; j < colorschemeCols.Length; j++)
                                        {
                                            var cell = new CellAddress((uint)(i+1), (uint)(j+1));
                                            cellEntries.Add(colorschemeCols[j], cell);
                                        }
                                       
                                        var colorschemeIndex = schemeInfo.ColorSchemes.FindIndex(o => o.SchemeId == schemeId);
                                        if (colorschemeIndex > -1)
                                        {
                                            var cd = new ColorDefinition();
                                            cd.ParameterValue = paramValue;
                                            cd.Color = colorBytes;

                                            var windowColor = Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
                                            cd.BackgroundColor = new SolidColorBrush(windowColor);
                                            cd.CellEntries = cellEntries;

                                            schemeInfo.ColorSchemes[colorschemeIndex].ColorDefinitions.Add(cd);
                                        }
                                        else
                                        {
                                            var scheme = new ColorScheme();
                                            scheme.SchemeId = schemeId;
                                            scheme.SchemeName = schemeName;
                                            scheme.Categories = categoryNames;
                                            scheme.ParameterName = paramName;
                                            scheme.DefinitionBy = DefinitionType.ByValue;

                                            var cd = new ColorDefinition();
                                            cd.ParameterValue = paramValue;
                                            cd.Color = colorBytes;

                                            var windowColor = Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
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
        public string RootId { get; set; }
        public string RootTitle { get; set; } = "";
        public File RootFolder { get; set; }

        public File ColorSheet { get; set; }
        public File CategorySheet { get; set; }

        public File ActiveFolder { get; set; }
        public File ActiveBCFFolder { get; set; }
        public File ActiveImgFolder { get; set; }


        public File ArchiveFolder { get; set; }
        public File ArchiveBCFFolder { get; set; }
        public File ArchiveImgFolder { get; set; }

        public FolderHolders(string folderId)
        {
            RootId = folderId;
        }
    }

    public class FileHolders
    {
        public string BCFPath { get; set; }
        public string SharedLink { get; set; }
        public FolderHolders FolderInfo { get; set; } = null;
        public BCFZIP BcfZip { get; set; } = null;
        public File ArchivedBCF { get; set; } = null;
        public File ActiveBCF { get; set; } = null;
        public List<File> BCFImages { get; set; } = null;

        public FileHolders(string path, string link)
        {
            BCFPath = path;
            SharedLink = link;
        }
    }
}
