using HOK.MissionControl.Core.Classes;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Core.Utils
{
    public static class ServerUtil
    {
        public static bool UseLocalServer = true;
        public const string baseUrlLocal = "http://hok-184vs/";
        //public const string baseUrlLocal = "http://localhost:80/";
        public const string baseUrlGlobal = "http://hokmissioncontrol.herokuapp.com/";
        public const string apiVersion = "api/v1";

        public static string RestApiBaseUrl { get { return UseLocalServer ? baseUrlLocal : baseUrlGlobal; } }
        public static string RestApiUri { get { return RestApiBaseUrl + "/" + apiVersion; } }

        #region GET
        public static List<Project> GetProjects(string query)
        {
            List<Project> items = new List<Project>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/projects/" + query, Method.GET);
                IRestResponse<List<Project>> response = client.Execute<List<Project>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-GetProjects:" + ex.Message);
            }
            return items;
        }

        public static Project GetProjectByConfigurationId(string configId)
        {
            Project projectFound = null;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/projects/configid/{configid}", Method.GET);
                request.AddUrlSegment("configid", configId);

                IRestResponse<List<Project>> response = client.Execute<List<Project>>(request);
                if (null != response.Data)
                {
                    List<Project> items = response.Data;
                    projectFound = items.First();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-GetProjects:" + ex.Message);
            }
            return projectFound;
        }

        public static List<Configuration> GetConfigurations(string query)
        {
            List<Configuration> items = new List<Configuration>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/configurations/" + query, Method.GET);

                IRestResponse tempResponse = client.Execute(request);
                IRestResponse<List<Configuration>> response = client.Execute<List<Configuration>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-GetConfigurations:" + ex.Message);
            }
            return items;
        }

        public static List<Configuration> GetConfigurationByUri(string uri)
        {
            List<Configuration> items = new List<Configuration>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/configurations/uri/{uri}", Method.GET);
                request.AddUrlSegment("uri", uri);

                IRestResponse<List<Configuration>> response = client.Execute<List<Configuration>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-GetConfigurations:" + ex.Message);
            }
            return items;
        }

        public static Configuration GetConfigurationByCentralPath(string centralPath)
        {
            Configuration configFound = null;
            try
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(centralPath);

                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/configurations/uri/{uri}", Method.GET);
                request.AddUrlSegment("uri", fileName);

                IRestResponse<List<Configuration>> response = client.Execute<List<Configuration>>(request);
                if (null != response.Data)
                {
                    List<Configuration> items = response.Data;
                    foreach (Configuration config in items)
                    {
                        foreach (RvtFile file in config.files)
                        {
                            if (file.centralPath.ToLower() == centralPath.ToLower())
                            {
                                configFound = config;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-GetConfigurations:" + ex.Message);
            }
            return configFound;
        }


        public static List<TriggerRecord> GetTriggerRecords(string query)
        {
            List<TriggerRecord> items = new List<TriggerRecord>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/triggerrecords/" + query, Method.GET);
                IRestResponse<List<TriggerRecord>> response = client.Execute<List<TriggerRecord>>(request);
                if (null != response.Data)
                {
                    items = response.Data;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-GetTriggerRecords:" + ex.Message);
            }
            return items;
        }
        #endregion

        #region POST
        public static HttpStatusCode PostProject(out string content, out string errorMessage, Project project)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/projects", Method.POST);
                request.AddHeader("Content-type", "application/json");
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
                LogUtil.AppendLog("ServerUtil-PostProject:" + ex.Message);
            }
            return status;
        }

        public static HttpStatusCode PostConfiguration(out string content, out string errorMessage, Configuration config)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/configurations", Method.POST);
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(config);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-PostConfiguration:" + ex.Message);
            }
            return status;
        }

        public static HttpStatusCode PostTriggerRecords(out string content, out string errorMessage, TriggerRecord record)
        {
            HttpStatusCode status = HttpStatusCode.Unused;

            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/triggerrecords", Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(record);

                IRestResponse response = client.Execute<TriggerRecord>(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-PostTriggerRecords:" + ex.Message);
            }
            return status;
        }
        #endregion

        #region UPDATE
        public static HttpStatusCode UpdateProject(out string content, out string errorMessage, Project project)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/projects/" + project._id, Method.PUT);
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
                LogUtil.AppendLog("ServerUtil-UpdateProject:" + ex.Message);
            }
            return status;
        }

        public static HttpStatusCode UpdateConfiguration(out string content, out string errorMessage, Configuration config)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/configurations/" + config._id, Method.PUT);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(config);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-UpdateConfiguration:" + ex.Message);
            }
            return status;
        }


        #endregion

        #region DELETE
        public static HttpStatusCode DeleteProject(out string content, out string errorMessage, string query)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/projects/" + query, Method.DELETE);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-DeleteProject:" + ex.Message);
            }
            return status;
        }

        public static HttpStatusCode DeleteConfiguration(out string content, out string errorMessage, string query)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/configurations/" + query, Method.DELETE);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-DeleteConfiguration:" + ex.Message);
            }
            return status;
        }

        public static HttpStatusCode DeleteTriggerRecord(out string content, out string errorMessage, string query)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/triggerrecords/" + query, Method.DELETE);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("ServerUtil-DeleteTriggerRecord:" + ex.Message);
            }
            return status;
        }
        #endregion
    }
}
