using HOK.ElementWatcher.Classes;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.ElementWatcher.Utils
{
    public enum ControllerType
    {
        categorytriggers, projectfiles, projects, projectupdaters, requestqueues
    }

    public static class ServerUtil
    {
        public static bool UseLocalServer = false;

        public const string baseUrlLocal = "";
        public const string baseUrlGlobal = "http://dtmtool.herokuapp.com";
        public const string apiVersion = "api/v1";

        public static string RestApiBaseUrl { get { return UseLocalServer ? baseUrlLocal : baseUrlGlobal; } }
        public static string RestApiUri { get { return RestApiBaseUrl + "/" + apiVersion; } }

        #region Get
        public static List<Project> GetProjects(string query)
        {
            List<Project> items = new List<Project>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projects.ToString() + "/" + query, Method.GET);
                IRestResponse<List<Project>> response = client.Execute<List<Project>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to get items from projects/" + query + "\n" + ex.Message, "Server Util: Get Projects", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return items;
        }

        public static List<ProjectFile> GetProjectFiles(string query)
        {
            List<ProjectFile> items = new List<ProjectFile>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projectfiles.ToString() + "/" + query, Method.GET);
                IRestResponse<List<ProjectFile>> response = client.Execute<List<ProjectFile>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to get items from projectfiless/" + query + "\n" + ex.Message, "Server Util: Get Project Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return items;
        }

        public static List<ProjectUpdater> GetProjectUpdater(string query)
        {
            List<ProjectUpdater> items = new List<ProjectUpdater>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projectupdaters.ToString() + "/" + query, Method.GET);
                IRestResponse<List<ProjectUpdater>> response = client.Execute<List<ProjectUpdater>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to get items from projectupdaters/" + query + "\n" + ex.Message, "Server Util: Get Project Updaters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return items;
        }

        public static List<CategoryTrigger> GetCategoryTriggers(string query)
        {
            List<CategoryTrigger> items = new List<CategoryTrigger>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.categorytriggers.ToString() + "/" + query, Method.GET);
                IRestResponse<List<CategoryTrigger>> response = client.Execute<List<CategoryTrigger>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to get items from categorytriggers/" + query + "\n" + ex.Message, "Server Util: Get Category Triggers", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return items;
        }

        public static List<RequestQueue> GetRequestQueues(string query)
        {
            List<RequestQueue> items = new List<RequestQueue>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.requestqueues.ToString() + "/" + query, Method.GET);
                IRestResponse<List<RequestQueue>> response = client.Execute<List<RequestQueue>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to get items from requestqueues/" + query + "\n" + ex.Message, "Server Util: Get Request Queues", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return items;
        }
        #endregion

        #region Post
        public static HttpStatusCode PostProject(out string content, out string errorMessage, Project project)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projects.ToString(), Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(project);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to post items projects\n" + ex.Message, "Server Util: Post Projects", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static HttpStatusCode PostProjectFile(out string content, out string errorMessage, ProjectFile projectFile)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projectfiles.ToString(), Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(projectFile);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to post items projectfiles\n" + ex.Message, "Server Util: Post Project Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static HttpStatusCode PostProjectUpdater(out string content, out string errorMessage, ProjectUpdater updater)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projectupdaters.ToString(), Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(updater);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to post items projectupdaters\n" + ex.Message, "Server Util: Post Project Updaters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static HttpStatusCode PostCategoryTrigger(out string content, out string errorMessage, CategoryTrigger trigger)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.categorytriggers.ToString(), Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(trigger);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to post items categorytriggers\n" + ex.Message, "Server Util: Post Category Triggers", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static HttpStatusCode PostRequestQueues(out string content, out string errorMessage, RequestQueue queue)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.requestqueues.ToString(), Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(queue);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to post items requestqueues\n" + ex.Message, "Server Util: Post Request Queues", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }
        #endregion


        #region Update
        public static HttpStatusCode UpdateProject(out string content, out string errorMessage, Project project, string id)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projects.ToString() + "/" + id, Method.PUT);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(project);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to update items in projects:id=" + id + "\n" + ex.Message, "Server Util: Update Project", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }


        public static HttpStatusCode UpdateProjectFile(out string content, out string errorMessage, ProjectFile projectFile, string id)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projectfiles.ToString() + "/" + id, Method.PUT);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(projectFile);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to update items in projectfiles:id=" + id + "\n" + ex.Message, "Server Util: Update Project File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static HttpStatusCode UpdateProjectUpdater(out string content, out string errorMessage, ProjectUpdater updater, string id)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.projectupdaters.ToString() + "/" + id, Method.PUT);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(updater);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to update items in projectupdaters:id=" + id + "\n" + ex.Message, "Server Util: Update Project Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static HttpStatusCode UpdateCategoryTrigger(out string content, out string errorMessage, CategoryTrigger trigger, string id)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.categorytriggers.ToString() + "/" + id, Method.PUT);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(trigger);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to update items in categorytriggers:id=" + id + "\n" + ex.Message, "Server Util: Update Category Triggers", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static HttpStatusCode UpdateRequestQueue(out string content, out string errorMessage, RequestQueue queue, string id)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + ControllerType.requestqueues.ToString() + "/" + id, Method.PUT);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(queue);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to update items in requestqueues:id=" + id + "\n" + ex.Message, "Server Util: Update Request Queue", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }
        #endregion
        

        public static HttpStatusCode DeleteItem(out string content, out string errorMessage, ControllerType clType, string query)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/"+clType.ToString()+"/" + query, Method.DELETE);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to delete items " + clType.ToString() + "/" + query + "\n" + ex.Message, "Server Util: Delete Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static HttpStatusCode PostConfiguration(out string content, out string errorMessage, DTMConfigurations config)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                status = PostProjectFile(out content, out errorMessage, config.ProjectFileInfo);
                status = UpdateProject(out content, out errorMessage, config.ProjectFileInfo.ProjectInfo, config.ProjectFileInfo.ProjectInfo._id);
                foreach (ProjectUpdater updater in config.ProjectUpdaters)
                {
                    status = PostProjectUpdater(out content, out errorMessage, updater);
                    foreach (CategoryTrigger trigger in updater.CategoryTriggers)
                    {
                        status = PostCategoryTrigger(out content, out errorMessage, trigger);
                        foreach (RequestQueue queue in trigger.Requests)
                        {
                            status = PostRequestQueues(out content, out errorMessage, queue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to post configuration. \n" + ex.Message, "Server Util: Post Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

        public static bool GetConfiguration(string docGuid, out DTMConfigurations config)
        {
            bool found = false;
            config = new DTMConfigurations();
            try
            {
                List<ProjectFile> projectFiles = GetProjectFiles(docGuid.ToString());
                if (projectFiles.Count > 0)
                {
                    ProjectFile projectFile = projectFiles.First();
                    config.ProjectFileInfo = projectFile;
                    string projectId = config.ProjectFileInfo.Project_Id;
                    List<Project> projects = GetProjects(projectId);
                    if (projects.Count > 0)
                    {
                        config.ProjectFileInfo.ProjectInfo = projects.First();
                    }

                    List<ProjectUpdater> updaters = GetProjectUpdater("projectfileid/" + projectFile._id);
                    if (updaters.Count > 0)
                    {
                        for (int i = 0; i < updaters.Count; i++)
                        {
                            ProjectUpdater updater = updaters[i];
                            List<CategoryTrigger> triggers = GetCategoryTriggers("updaterid/" + updater._id);
                            for (int j = 0; j < triggers.Count; j++)
                            {
                                CategoryTrigger trigger = triggers[j];
                                List<RequestQueue> queues = GetRequestQueues("triggerid/"+trigger._id);
                                for (int k = 0; k < queues.Count; k++)
                                {
                                    RequestQueue queue = queues[k];
                                    trigger.Requests.Add(queue);
                                }
                                updater.CategoryTriggers.Add(trigger);
                            }

                            config.ProjectUpdaters.Add(updater);
                        }
                    }
                }
               
                found = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to get configuration. \n" + ex.Message, "Server Util: Get Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return found;
        }

        public static HttpStatusCode UpdateConfiguration(out string content, out string errorMessage, DTMConfigurations config)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                foreach (ProjectUpdater updater in config.ProjectUpdaters)
                {
                    status = UpdateProjectUpdater(out content, out errorMessage, updater, updater._id.ToString());
                    foreach (CategoryTrigger trigger in updater.CategoryTriggers)
                    {
                        status = UpdateCategoryTrigger(out content, out errorMessage, trigger, trigger._id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to update configuration. \n" + ex.Message, "Server Util: Update Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return status;
        }

    }
}
