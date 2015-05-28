using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Newtonsoft.Json;

namespace HOK.SmartBCF.GoogleUtils
{
    public static class ConfigurationManager
    {
        public static DriveService service = null;
        private static string configFileName = "config.json";
        private static string projectInfoFileName = "projectInfo.json";

        private static SmartBCFConfiguration sbConfiguration = null;
        private static BCFProjectInfo bcfProjectInfo = null;

        public static SmartBCFConfiguration SBConfiguration { get { return sbConfiguration; } set { sbConfiguration = value; } }
        public static BCFProjectInfo BfcProjectInfo { get { return bcfProjectInfo; } set { bcfProjectInfo = value; } }

        private static DriveService GetUserCredential()
        {
            DriveService driveService = null;
            try
            {
                UserCredential credential;
                string currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string currentDirectory = System.IO.Path.GetDirectoryName(currentAssembly);
                string jsonPath = System.IO.Path.Combine(currentDirectory, "client_secrets_samrtBCF.json");

                using (var filestream = new System.IO.FileStream(jsonPath,
                    System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(filestream).Secrets,
                        new[] { DriveService.Scope.Drive, DriveService.Scope.Drive},
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

        public static SmartBCFConfiguration ReadConfiguration()
        {
            SmartBCFConfiguration sbConfiguration = null;
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }
                if (null != service)
                {
                    List<File> result = new List<File>();
                    FilesResource.ListRequest request = service.Files.List();
                    request.Q = "'appfolder' in parents and title = \'"+configFileName+"\'";
                    FileList fileList = request.Execute();
                    File configFile = null;
                    if (fileList.Items.Count > 0)
                    {
                        configFile = fileList.Items.First();
                    }

                    if (null != configFile)
                    {
                        var x = service.HttpClient.GetByteArrayAsync(configFile.DownloadUrl);
                        byte[] arrBytes = x.Result;

                        if (arrBytes.Length > 0)
                        {
                            string jsonString = Encoding.ASCII.GetString(arrBytes);
                            sbConfiguration = JsonConvert.DeserializeObject<SmartBCFConfiguration>(jsonString);
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read configuration for folder settings.\n"+ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sbConfiguration;
        }

        public static bool UpdateConfiguration(SmartBCFConfiguration config)
        {
            bool updated = false;
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }
                if (null != service)
                {
                    List<File> result = new List<File>();
                    FilesResource.ListRequest request = service.Files.List();
                    request.Q = "'appfolder' in parents and title = \'" + configFileName + "\'";
                    FileList fileList = request.Execute();
                    File configFile = null;
                    if (fileList.Items.Count > 0)
                    {
                        configFile = fileList.Items.First();
                    }

                    if (null != configFile)
                    {
                        string jsonString = JsonConvert.SerializeObject(config);
                        byte[] arrBytes = Encoding.ASCII.GetBytes(jsonString);
                        System.IO.MemoryStream stream = new System.IO.MemoryStream(arrBytes);

                        FilesResource.UpdateMediaUpload uploadRequest = service.Files.Update(configFile, configFile.Id, stream, "");
                        uploadRequest.Upload();

                        File fileUpdated = uploadRequest.ResponseBody;
                        if (null != fileUpdated)
                        {
                            updated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update configuration settings in app folder.\n"+ex.Message, "Update Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        public static BCFFolderStructure ReadProjectInfo(string projectName, SmartBCFConfiguration config)
        {
            BCFFolderStructure folderStructure = null;
            try
            {
                List<GoogleFolderInfo> projectFolders = config.ProjectFolders;
                var folderFound = from folder in projectFolders where folder.FolderName == projectName select folder;
                if (folderFound.Count() > 0)
                {
                    string projectId = folderFound.First().FolderId;

                    if (null == service)
                    {
                        service = GetUserCredential();
                    }
                    if (null != service)
                    {
                        List<File> result = new List<File>();
                        FilesResource.ListRequest request = service.Files.List();
                        request.Q = "'appfolder' in parents and title = \'" + projectInfoFileName + "\'";
                        FileList fileList = request.Execute();
                        File projectInfoFile = null;
                        if (fileList.Items.Count > 0)
                        {
                            projectInfoFile = fileList.Items.First();
                        }

                        if (null != projectInfoFile)
                        {
                            var x = service.HttpClient.GetByteArrayAsync(projectInfoFile.DownloadUrl);
                            byte[] arrBytes = x.Result;

                            if (arrBytes.Length > 0)
                            {
                                string jsonString = Encoding.ASCII.GetString(arrBytes);
                                bcfProjectInfo = JsonConvert.DeserializeObject<BCFProjectInfo>(jsonString);
                                if (null != bcfProjectInfo)
                                {
                                    var projectFound = from project in bcfProjectInfo.BcfProjects where project.ProjectId == projectId select project;
                                    if (projectFound.Count() > 0)
                                    {
                                        folderStructure = projectFound.First();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read Project Info.\n"+ex.Message, "Read Project Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return folderStructure;
        }

        public static bool UpdateProjectInfo(BCFFolderStructure folderStructure)
        {
            bool updated = false;
            try
            {
                if (null != bcfProjectInfo)
                {
                    List<BCFFolderStructure> projects = bcfProjectInfo.BcfProjects;
                    int index = projects.FindIndex(o => o.ProjectId == folderStructure.ProjectId);
                    if (index > -1)
                    {
                        projects.RemoveAt(index);
                    }
                    projects.Add(folderStructure);
                    bcfProjectInfo.BcfProjects.Clear();
                    bcfProjectInfo.BcfProjects.AddRange(projects);

                    List<File> result = new List<File>();
                    FilesResource.ListRequest request = service.Files.List();
                    request.Q = "'appfolder' in parents and title = \'" + projectInfoFileName + "\'";
                    FileList fileList = request.Execute();
                    File projectInfoFile = null;
                    if (fileList.Items.Count > 0)
                    {
                        projectInfoFile = fileList.Items.First();

                        if (null != projectInfoFile)
                        {
                            string jsonString = JsonConvert.SerializeObject(bcfProjectInfo);
                            byte[] arrBytes = Encoding.ASCII.GetBytes(jsonString);
                            System.IO.MemoryStream stream = new System.IO.MemoryStream(arrBytes);

                            FilesResource.UpdateMediaUpload uploadRequest = service.Files.Update(projectInfoFile, projectInfoFile.Id, stream, "");
                            uploadRequest.Upload();

                            File fileUpdated = uploadRequest.ResponseBody;
                            if (null != fileUpdated)
                            {
                                updated = true;
                            }
                        }
                    }
                    else
                    {
                        File body = new File();
                        body.Title = projectInfoFileName;
                        body.Description = "The list of the information of bcf project folders will be stored.";
                        body.Parents = new List<ParentReference>() { new ParentReference() { Id = "appfolder" } };

                        string jsonString = JsonConvert.SerializeObject(bcfProjectInfo);
                        byte[] arrBytes = Encoding.ASCII.GetBytes(jsonString);
                        System.IO.MemoryStream stream = new System.IO.MemoryStream(arrBytes);

                        FilesResource.InsertMediaUpload uploadRequest = service.Files.Insert(body, stream, "");
                        uploadRequest.Upload();

                        File fileUpdated = uploadRequest.ResponseBody;
                        if (null != fileUpdated)
                        {
                            updated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update Project Info.\n"+ex.Message, "Update Project Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                updated = false;
            }
            return updated;
        }
    }

    public class SmartBCFConfiguration
    {
        private List<GoogleFolderInfo> systemFolders = new List<GoogleFolderInfo>(); // generated by users in Google Drive
        private List<GoogleFolderInfo> projectFolders = new List<GoogleFolderInfo>(); //root project folder generated by smartBCF

        public List<GoogleFolderInfo> SystemFolders { get { return systemFolders; } set { systemFolders = value; } }
        public List<GoogleFolderInfo> ProjectFolders { get { return projectFolders; } set { projectFolders = value; } }
    }

    public class BCFProjectInfo
    {
        private List<BCFFolderStructure> bcfProjects = new List<BCFFolderStructure>();

        public List<BCFFolderStructure> BcfProjects { get { return bcfProjects; } set { bcfProjects = value; } }
    }

    public class BCFFolderStructure
    {
        private string projectId = "";
        private string projectName = "";
        private string projectLink = "";

        private GoogleFolderInfo archiveFolder = new GoogleFolderInfo();
        private GoogleFolderInfo archiveBCFFolder = new GoogleFolderInfo();
        private GoogleFolderInfo archiveImageFolder = new GoogleFolderInfo();
        private List<GoogleFileInfo> archiveBCFFiles = new List<GoogleFileInfo>();
        private List<GoogleFileInfo> archiveImageFiles = new List<GoogleFileInfo>();

        private GoogleFolderInfo activeFolder = new GoogleFolderInfo();
        private GoogleFolderInfo activeBCFFolder = new GoogleFolderInfo();
        private GoogleFolderInfo activeImageFolder = new GoogleFolderInfo();
        private List<GoogleFileInfo> activeBCFFiles = new List<GoogleFileInfo>();
        private List<GoogleFileInfo> activeImageFiles = new List<GoogleFileInfo>();

        private GoogleFileInfo colorSheet = new GoogleFileInfo();

        public string ProjectId { get { return projectId; } set { projectId = value; } }
        public string ProjectName { get { return projectName; } set { projectName = value; } }
        public string ProjectLink { get { return projectLink; } set { projectLink = value; } }

        public GoogleFolderInfo ArchiveFolder { get { return archiveFolder; } set { archiveFolder = value; } }
        public GoogleFolderInfo ArchiveBCFFolder { get { return archiveBCFFolder; } set { archiveBCFFolder = value; } }
        public GoogleFolderInfo ArchiveImageFolder { get { return archiveImageFolder; } set { archiveImageFolder = value; } }
        public List<GoogleFileInfo> ArchiveBCFFiles { get { return archiveBCFFiles; } set { archiveBCFFiles = value; } }
        public List<GoogleFileInfo> ArchiveImageFiles { get { return archiveImageFiles; } set { archiveImageFiles = value; } }

        public GoogleFolderInfo ActiveFolder { get { return activeFolder; } set { activeFolder = value; } }
        public GoogleFolderInfo ActiveBCFFolder { get { return activeBCFFolder; } set { activeBCFFolder = value; } }
        public GoogleFolderInfo ActiveImageFolder { get { return activeImageFolder; } set { activeImageFolder = value; } }
        public List<GoogleFileInfo> ActiveBCFFiles { get { return activeBCFFiles; } set { activeBCFFiles = value; } }
        public List<GoogleFileInfo> ActiveImageFiles { get { return activeImageFiles; } set { activeImageFiles = value; } }

        public GoogleFileInfo ColorSheet { get { return colorSheet; } set { colorSheet = value; } }
    }

    public class GoogleFolderInfo
    {
        private string folderName = "";
        private string folderId = "";

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FolderName { get { return folderName; } set { folderName = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FolderId { get { return folderId; } set { folderId = value; } }

        public GoogleFolderInfo(string name, string id)
        {
            folderName = name;
            folderId = id;
        }

        public GoogleFolderInfo() { }
    }

    public class GoogleFileInfo
    {
        private string fileName = "";
        private string fileId = "";
        private BCFFileType fileType = BCFFileType.None;
        private string referenceFileId = "";

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FileName { get { return fileName; } set { fileName = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FileId { get { return fileId; } set { fileId = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public BCFFileType FileType { get { return fileType; } set { fileType = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReferenceFileId { get { return referenceFileId; } set { referenceFileId = value; } }

        public GoogleFileInfo(string name, string id, BCFFileType bcfFileType)
        {
            fileName = name;
            fileId = id;
            fileType = bcfFileType;
        }

        public GoogleFileInfo(File file, BCFFileType bcfFileType)
        {
            fileName = file.Title;
            fileId = file.Id;
            fileType = bcfFileType;
        }

        public GoogleFileInfo() { }
    }

    public enum BCFFileType
    {
        None, Bcfzip, Image, Spreadsheet
    }
}
