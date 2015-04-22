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
using Google.Apis.Util.Store;
using Google.GData.Spreadsheets;

namespace HOK.ModelManager.GoogleDocs
{
    public static class GoogleDriveUtil
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
                string jsonPath = System.IO.Path.Combine(currentDirectory, "client_secrets.json");

                using (var filestream = new System.IO.FileStream(jsonPath,
                    System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(filestream).Secrets,
                        new[] { DriveService.Scope.Drive },
                        "user",
                        CancellationToken.None).Result;
                        
                }


                // Create the service.
                driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "HOK Project Replicator",
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get user credential.\n" + ex.Message, "Get User Credential", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return driveService;
        }

        public static string GetGoogleSheetId(ModelInfo modelInfo)
        {
            string sheetId = "";
            try
            {
                ProjectReplicatorSettings settings = DataStorageUtil.ReadSettings(modelInfo.Doc);
                sheetId = settings.GoogleSheetId;
                if (!string.IsNullOrEmpty(sheetId))
                {
                    FilesResource.GetRequest getRequest = service.Files.Get(sheetId);
                    File sheetFile = getRequest.Execute();
                    if (null != sheetFile)
                    {
                        if (null == sheetFile.ExplicitlyTrashed)
                        {
                            return sheetFile.Id;
                        }
                    }
                }

                sheetId = FindGoogleSheetId(modelInfo);
                settings.GoogleSheetId = sheetId;
                bool updated = DataStorageUtil.UpdateSettings(modelInfo.Doc, settings);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get google sheet Id\n"+ex.Message, "Google Drive : File ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sheetId;
        }

        private static string FindGoogleSheetId(ModelInfo modelInfo)
        {
            string fileId = "";
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    File projectFolder = GetProjectFolder(modelInfo);
                    if (null != projectFolder)
                    {
                        FilesResource.ListRequest request = service.Files.List();
                        request.Q = "title = \'" + modelInfo.DocTitle + "\' and \'" + projectFolder.Id + "\' in parents and trashed = false";
                        FileList files = request.Execute();

                        
                        if (files.Items.Count > 0)
                        {
                            fileId = files.Items.First().Id;
                        }
                        else
                        {
                            File createdSheet = CreateGoogleSheet(modelInfo, projectFolder.Id);
                            if (null != createdSheet)
                            {
                                fileId = createdSheet.Id;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find google sheet.\n"+ex.Message, "Find Google Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return fileId;
        }

        private static File GetProjectFolder(ModelInfo modelInfo)
        {
            File projectFolder = null;
            try
            {
                FilesResource.ListRequest request = service.Files.List();
                request.Q = "title = \'Project Replication\'";
                FileList files = request.Execute();

                if (files.Items.Count > 0)
                {
                    string projectReplicationFolderId = files.Items.First().Id;

                    if (modelInfo.HOKStandard)
                    {
                        request = service.Files.List();
                        request.Q = "title = \'HOK Offices\' and \'" + projectReplicationFolderId + "\' in parents";
                        files = request.Execute();

                        if (files.Items.Count > 0)
                        {
                            string hokFolderId = files.Items.First().Id;
                            string location = (!string.IsNullOrEmpty(modelInfo.FileLocation)) ? modelInfo.FileLocation : modelInfo.UserLocation;

                            request = service.Files.List();
                            request.Q = "title = \'" + location + "\' and \'" + hokFolderId + "\' in parents";
                            files = request.Execute();

                            if (files.Items.Count > 0)
                            {
                                string locationFolderId = files.Items.First().Id;
                                string projectFolderName = modelInfo.ProjectNumber + " " + modelInfo.ProjectName;

                                request = service.Files.List();
                                request.Q = "title = \'" + projectFolderName + "\' and \'" + locationFolderId + "\' in parents";
                                files = request.Execute();

                                if (files.Items.Count > 0)
                                {
                                    projectFolder = files.Items.First();
                                }
                                else
                                {
                                    //create project folder
                                    File body = new File();
                                    body.Title = projectFolderName;
                                    body.Description = "Project Number: " + modelInfo.ProjectNumber + ", Project Name: " + modelInfo.ProjectName;
                                    body.MimeType = "application/vnd.google-apps.folder";
                                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = locationFolderId } };

                                    FilesResource.InsertRequest insertRequest = service.Files.Insert(body);
                                    projectFolder = insertRequest.Execute();
                                }
                            }
                        }
                    }
                    else
                    {
                        //External Users
                        request = service.Files.List();
                        request.Q = "title = \'External Users\' and \'" + projectReplicationFolderId + "\' in parents";
                        files = request.Execute();

                        if (files.Items.Count > 0)
                        {
                            string externalFolderId = files.Items.First().Id;
                            string companyName = modelInfo.CompanyName;

                            request = service.Files.List();
                            request.Q = "title = \'" + companyName + "\' and \'" + externalFolderId + "\' in parents";
                            files = request.Execute();

                            if (files.Items.Count > 0)
                            {
                                projectFolder = files.Items.First();
                            }
                            else
                            {
                                request = service.Files.List();
                                request.Q = "title = \'Unknown\' and \'" + externalFolderId + "\' in parents";
                                files = request.Execute();

                                if (files.Items.Count > 0)
                                {
                                    projectFolder = files.Items.First();
                                }
                            }
                        }
                    }
                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get project folder.\n"+ex.Message, "Find Project Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return projectFolder; 
        }

        private static File CreateGoogleSheet(ModelInfo modelInfo, string projectFolderId)
        {
            File sheetFile = null;
            try
            {
                if (null == service)
                {
                    service = GetUserCredential();
                }

                if (null != service)
                {
                    File body = new File();
                    body.Title = modelInfo.DocTitle;
                    body.Description = "Created by Project Replicator";
                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = projectFolderId } };
                    body.MimeType = "application/vnd.google-apps.spreadsheet";

                    FilesResource.InsertRequest request = service.Files.Insert(body);
                    sheetFile = request.Execute();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create google sheet.\n"+ex.Message, "Create Google Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sheetFile;
        }
    }
}
