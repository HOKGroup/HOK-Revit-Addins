using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HOK.MissionControl.Core.Classes;
using RestSharp;

namespace HOK.MissionControl.Core.Utils
{
    /// <summary>
    /// Different states of the Revit model.
    /// </summary>
    public enum WorksetMonitorState
    {
        onOpen,
        onSynch
    }

    public static class ServerUtil
    {
        public static bool UseLocalServer = true;
        //public const string baseUrlLocal = "http://hok-184vs/";
        public const string BaseUrlLocal = "http://localhost:8080/";
        public const string BaseUrlGlobal = "http://hokmissioncontrol.herokuapp.com/";
        public const string ApiVersion = "api/v1";
        public static string RestApiBaseUrl => UseLocalServer ? BaseUrlLocal : BaseUrlGlobal;
        public static string RestApiUri => RestApiBaseUrl + "/" + ApiVersion;

        #region GET
        //public static List<Project> GetProjects(string query)
        //{
        //    List<Project> items = new List<Project>();
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/projects/" + query, Method.GET);
        //        IRestResponse<List<Project>> response = client.Execute<List<Project>>(request);
        //        if (null != response.Data)
        //        {
        //            items = response.Data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-GetProjects:" + ex.Message);
        //    }
        //    return items;
        //}

        /// <summary>
        /// Returns Project from a Configuration Id.
        /// </summary>
        /// <param name="configId">Configuration Id.</param>
        /// <returns>Project class.</returns>
        public static Project GetProjectByConfigurationId(string configId)
        {
            Project projectFound = null;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/projects/configid/{configid}", Method.GET);
                request.AddUrlSegment("configid", configId);

                var response = client.Execute<List<Project>>(request);
                if (null != response.Data)
                {
                    var items = response.Data;
                    projectFound = items.First();
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("ServerUtil-GetProjects:" + ex.Message);
            }
            return projectFound;
        }

        //public static List<Configuration> GetConfigurations(string query)
        //{
        //    List<Configuration> items = new List<Configuration>();
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/configurations/" + query, Method.GET);

        //        IRestResponse tempResponse = client.Execute(request);
        //        IRestResponse<List<Configuration>> response = client.Execute<List<Configuration>>(request);
        //        if (null != response.Data)
        //        {
        //            items = response.Data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-GetConfigurations:" + ex.Message);
        //    }
        //    return items;
        //}

        //public static List<Configuration> GetConfigurationByUri(string uri)
        //{
        //    List<Configuration> items = new List<Configuration>();
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/configurations/uri/{uri}", Method.GET);
        //        request.AddUrlSegment("uri", uri);

        //        IRestResponse<List<Configuration>> response = client.Execute<List<Configuration>>(request);
        //        if (null != response.Data)
        //        {
        //            items = response.Data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-GetConfigurations:" + ex.Message);
        //    }
        //    return items;
        //}

        /// <summary>
        /// Retrieves a Mission Control Configuration that matches given Central File path.
        /// </summary>
        /// <param name="centralPath">Central File Path.</param>
        /// <returns>Mission Control Configuration</returns>
        public static Configuration GetConfigurationByCentralPath(string centralPath)
        {
            Configuration configFound = null;
            try
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(centralPath);
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/configurations/uri/{uri}", Method.GET);
                request.AddUrlSegment("uri", fileName);

                var response = client.Execute<List<Configuration>>(request);
                if (null != response.Data)
                {
                    var items = response.Data;
                    foreach (var config in items)
                    {
                        foreach (var file in config.files)
                        {
                            if (!string.Equals(file.centralPath.ToLower(), centralPath.ToLower(),
                                StringComparison.Ordinal)) continue;
                            configFound = config;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("ServerUtil-GetConfigurations:" + ex.Message);
            }
            return configFound;
        }

        //public static List<TriggerRecord> GetTriggerRecords(string query)
        //{
        //    List<TriggerRecord> items = new List<TriggerRecord>();
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/triggerrecords/" + query, Method.GET);
        //        IRestResponse<List<TriggerRecord>> response = client.Execute<List<TriggerRecord>>(request);
        //        if (null != response.Data)
        //        {
        //            items = response.Data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-GetTriggerRecords:" + ex.Message);
        //    }
        //    return items;
        //}

        #endregion

        #region POST

        //public static HttpStatusCode PostProject(out string content, out string errorMessage, Project project)
        //{
        //    HttpStatusCode status = HttpStatusCode.Unused;

        //    content = "";
        //    errorMessage = "";
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/projects", Method.POST);
        //        request.AddHeader("Content-type", "application/json");
        //        request.RequestFormat = RestSharp.DataFormat.Json;
        //        request.AddBody(project);

        //        IRestResponse response = client.Execute(request);
        //        content = response.Content;
        //        errorMessage = response.ErrorMessage;
        //        status = response.StatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogUtil.AppendLog("ServerUtil-PostProject:" + ex.Message);
        //    }
        //    return status;
        //}

        //public static HttpStatusCode PostConfiguration(out string content, out string errorMessage, Configuration config)
        //{
        //    HttpStatusCode status = HttpStatusCode.Unused;

        //    content = "";
        //    errorMessage = "";
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/configurations", Method.POST);
        //        request.AddHeader("Content-type", "application/json");
        //        request.RequestFormat = RestSharp.DataFormat.Json;
        //        request.AddBody(config);

        //        IRestResponse response = client.Execute(request);
        //        content = response.Content;
        //        errorMessage = response.ErrorMessage;
        //        status = response.StatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-PostConfiguration:" + ex.Message);
        //    }
        //    return status;
        //}

        /// <summary>
        /// POSTs Trigger records when users override DTM Tool.
        /// </summary>
        /// <param name="record">Trigger Record to post.</param>
        /// <returns>Status</returns>
        public static HttpStatusCode PostTriggerRecords(TriggerRecord record)
        {
            var status = HttpStatusCode.Unused;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request =
                    new RestRequest(ApiVersion + "/triggerrecords", Method.POST) {RequestFormat = DataFormat.Json};
                request.AddBody(record);

                var response = client.Execute<TriggerRecord>(request);
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("ServerUtil-PostTriggerRecords:" + ex.Message);
            }
            return status;
        }

        /// <summary>
        /// PUTs created Workset id into Project's Worksets array.
        /// </summary>
        /// <param name="project">Project class.</param>
        /// <param name="worksetDocumentId">Id of the Workset to add.</param>
        /// <returns>Status</returns>
        public static HttpStatusCode AddWorksetToProject(Project project, string worksetDocumentId)
        {
            var status = HttpStatusCode.Unused;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(
                    ApiVersion + "/projects/" + project.Id + "/addworkset/" + worksetDocumentId, Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddBody(project);

                var response = client.Execute(request);
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("ServerUtil-AddWorksetToProject:" + ex.Message);
            }
            return status;
        }

        /// <summary>
        /// POSTs new Workset to database.
        /// </summary>
        /// <returns>_id of the new Workset</returns>
        public static string PostWorksetData()
        {
            var response = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/worksets", Method.POST);
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.AddBody(new WorksetData());

                var resresponse = client.Execute<WorksetData>(request);
                response = resresponse.Data.Id;
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("ServerUtil-PostWorksetData:" + ex.Message);
            }
            return response;
        }

        /// <summary>
        /// POST Worksets info for onOpened and onSynched events.
        /// </summary>
        /// <param name="worksetInfo">Worksets document class.</param>
        /// <param name="worksetDocumentId">Worksets document _id.</param>
        /// <param name="state">State of the Revit Document ex. onOpened.</param>
        /// <returns>Response status.</returns>
        public static HttpStatusCode PostWorksetInfo(WorksetEvent worksetInfo, string worksetDocumentId, WorksetMonitorState state)
        {
            var status = HttpStatusCode.Unused;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                RestRequest request;
                switch (state)
                {
                    case WorksetMonitorState.onOpen:
                        request = 
                            new RestRequest(ApiVersion + "/worksets/" + worksetDocumentId + "/onopened", Method.POST) { RequestFormat = DataFormat.Json };
                        break;
                    case WorksetMonitorState.onSynch:
                        request = 
                            new RestRequest(ApiVersion + "/worksets/" + worksetDocumentId + "/onsynched", Method.POST) { RequestFormat = DataFormat.Json };
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
                request.AddBody(worksetInfo);

                var response = client.Execute<WorksetEvent>(request);
                status = response.StatusCode;
            }
            catch (Exception e)
            {
                LogUtil.AppendLog("ServerUtil-PostWorksetInfo: " + e.Message);
            }
            return status;
        }

        /// <summary>
        /// POST Workset counts to itemCount document.
        /// </summary>
        /// <param name="worksetInfo">List of Workset items.</param>
        /// <param name="worksetDocumentId">Workset Document _id.</param>
        /// <returns>Response status.</returns>
        public static HttpStatusCode PostWorksetCounts(WorksetItem worksetInfo, string worksetDocumentId)
        {
            var status = HttpStatusCode.Unused;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request =
                    new RestRequest(ApiVersion + "/worksets/" + worksetDocumentId + "/itemcount", Method.POST);
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.AddBody(worksetInfo);

                var response = client.Execute<WorksetItem>(request);
                status = response.StatusCode;
            }
            catch (Exception e)
            {
                LogUtil.AppendLog("ServerUtil-PostWorksetCounts: " + e.Message);
            }
            return status;
        }

        #endregion

        #region UPDATE

        //public static HttpStatusCode UpdateProject(out string content, out string errorMessage, Project project)
        //{
        //    HttpStatusCode status = HttpStatusCode.Unused;
        //    content = "";
        //    errorMessage = "";
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/projects/" + project._id, Method.PUT);
        //        request.RequestFormat = RestSharp.DataFormat.Json;
        //        request.AddBody(project);

        //        IRestResponse response = client.Execute(request);

        //        content = response.Content;
        //        errorMessage = response.ErrorMessage;
        //        status = response.StatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-UpdateProject:" + ex.Message);
        //    }
        //    return status;
        //}


        //public static HttpStatusCode UpdateConfiguration(out string content, out string errorMessage, Configuration config)
        //{
        //    HttpStatusCode status = HttpStatusCode.Unused;
        //    content = "";
        //    errorMessage = "";
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/configurations/" + config._id, Method.PUT);
        //        request.RequestFormat = RestSharp.DataFormat.Json;
        //        request.AddBody(config);

        //        IRestResponse response = client.Execute(request);

        //        content = response.Content;
        //        errorMessage = response.ErrorMessage;
        //        status = response.StatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-UpdateConfiguration:" + ex.Message);
        //    }
        //    return status;
        //}


        #endregion

        #region DELETE

        //public static HttpStatusCode DeleteProject(out string content, out string errorMessage, string query)
        //{
        //    HttpStatusCode status = HttpStatusCode.Unused;
        //    content = "";
        //    errorMessage = "";
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/projects/" + query, Method.DELETE);

        //        IRestResponse response = client.Execute(request);

        //        content = response.Content;
        //        errorMessage = response.ErrorMessage;
        //        status = response.StatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-DeleteProject:" + ex.Message);
        //    }
        //    return status;
        //}

        //public static HttpStatusCode DeleteConfiguration(out string content, out string errorMessage, string query)
        //{
        //    HttpStatusCode status = HttpStatusCode.Unused;
        //    content = "";
        //    errorMessage = "";
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/configurations/" + query, Method.DELETE);

        //        IRestResponse response = client.Execute(request);

        //        content = response.Content;
        //        errorMessage = response.ErrorMessage;
        //        status = response.StatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-DeleteConfiguration:" + ex.Message);
        //    }
        //    return status;
        //}

        //public static HttpStatusCode DeleteTriggerRecord(out string content, out string errorMessage, string query)
        //{
        //    HttpStatusCode status = HttpStatusCode.Unused;
        //    content = "";
        //    errorMessage = "";
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(apiVersion + "/triggerrecords/" + query, Method.DELETE);

        //        IRestResponse response = client.Execute(request);

        //        content = response.Content;
        //        errorMessage = response.ErrorMessage;
        //        status = response.StatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        LogUtil.AppendLog("ServerUtil-DeleteTriggerRecord:" + ex.Message);
        //    }
        //    return status;
        //}
        #endregion
    }
}
