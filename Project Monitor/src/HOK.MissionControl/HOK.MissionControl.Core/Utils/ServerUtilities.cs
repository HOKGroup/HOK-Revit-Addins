using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HOK.MissionControl.Core.Schemas;
using RestSharp;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Core.Utils
{
    /// <summary>
    /// Different states of the Revit model.
    /// </summary>
    public enum WorksetMonitorState
    {
        onopened,
        onsynched
    }

    public static class ServerUtilities
    {
        public static bool UseLocalServer = true;
        //public const string baseUrlLocal = "http://hok-184vs/";
        public const string BaseUrlLocal = "http://localhost:8080/";
        public const string BaseUrlGlobal = "http://hokmissioncontrol.herokuapp.com/";
        public const string ApiVersion = "api/v1";
        public static string RestApiBaseUrl => UseLocalServer ? BaseUrlLocal : BaseUrlGlobal;
        public static string RestApiUri => RestApiBaseUrl + "/" + ApiVersion;

        #region GET

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

                    Log.AppendLog("Project Found: " + projectFound.Id);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return projectFound;
        }

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
                if (response.Data != null)
                {
                    var items = response.Data;
                    foreach (var config in items)
                    {
                        foreach (var file in config.files)
                        {
                            if (!string.Equals(file.centralPath.ToLower(), centralPath.ToLower(),
                                StringComparison.Ordinal)) continue;
                            configFound = config;

                            Log.AppendLog("Configuration Found: " + configFound.Id);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return configFound;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="worksetDocumentId"></param>
        ///// <param name="route"></param>
        ///// <returns></returns>
        //public static FamilyResponse GetFamilyStats(string worksetDocumentId, string route)
        //{
        //    var items = new FamilyResponse();
        //    try
        //    {
        //        var client = new RestClient(RestApiBaseUrl);
        //        var request = new RestRequest(ApiVersion + "/worksets/" + worksetDocumentId + "/" + route, Method.GET);
        //        var response = client.Execute<FamilyResponse>(request);
        //        if (null != response.Data)
        //        {
        //            items = response.Data;
        //        }
        //    }
        //    catch
        //    {
        //        //ignored
        //    }
        //    return items;
        //}

        /// <summary>
        /// Retrieves a Collection from MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of response class.</typeparam>
        /// <param name="responseType">Response object type.</param>
        /// <param name="route">Route to post request to.</param>
        /// <returns></returns>
        public static List<T> GetCollection<T>(T responseType, string route) where T : new()
        {
            var items = new List<T>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + route, Method.GET);
                var response = client.Execute<List<T>>(request);
                if (response.Data != null)
                {
                    items = response.Data;

                    Log.AppendLog(response.ResponseStatus + "-" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return items;
        }

        #endregion

        #region POST

        /// <summary>
        /// Posts Trigger Records to MongoDB. Trigger records are created when users override DTM Tools.
        /// </summary>
        /// <param name="record">Record to post.</param>
        public static HttpStatusCode PostTriggerRecords(TriggerRecord record)
        {
            var status = HttpStatusCode.Unused;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request =
                    new RestRequest(ApiVersion + "/triggerrecords", Method.POST) { RequestFormat = DataFormat.Json };
                request.AddBody(record);

                var response = client.Execute<TriggerRecord>(request);
                status = response.StatusCode;

                Log.AppendLog(response.ResponseStatus + "-triggerrecords");
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
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

                Log.AppendLog(response.ResponseStatus + "-addworkset");
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return status;
        }

        /// <summary>
        /// POSTs any new data Schema. Creates new Collection in MongoDB.
        /// </summary>
        /// <returns>Newly created Collection Schema with MongoDB assigned Id.</returns>
        public static T PostDataScheme<T>(T dataSchema, string route) where T : new()
        {
            var resresponse = new T();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/"+ route, Method.POST);
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.AddBody(dataSchema);

                var response = client.Execute<T>(request);
                resresponse = response.Data;

                Log.AppendLog(response.ResponseStatus + "-" + route);
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return resresponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worksetDocumentId"></param>
        /// <param name="objectId"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HttpStatusCode UpdateSessionInfo(string worksetDocumentId, string objectId, string action)
        {
            var httpStatusCode = HttpStatusCode.Unused;
            try
            {
                var restClient = new RestClient(RestApiBaseUrl);
                var restRequest = new RestRequest(ApiVersion + "/worksets/" + worksetDocumentId + "/sessioninfo/" + objectId, Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };
                restRequest.AddQueryParameter("action", action);
                httpStatusCode = restClient.Execute(restRequest).StatusCode;

                Log.AppendLog(httpStatusCode + "-sessioninfo-" + action);
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return httpStatusCode;
        }

        /// <summary>
        /// POST Data object/Schema to MongoDB.
        /// </summary>
        /// <typeparam name="T">Data Scheme Type.</typeparam>
        /// <param name="dataSchema">Data Schema object to post.</param>
        /// <param name="collectionName">Main route name.</param>
        /// <param name="collectionId">Id of the main collection.</param>
        /// <param name="route">Action/route to execute.</param>
        /// <returns>Data Schema object returned from database.</returns>
        public static T PostToMongoDB<T>(T dataSchema, string collectionName, string collectionId, string route) where T : new()
        {
            var output = new T();
            try
            {
                var restClient = new RestClient(RestApiBaseUrl);
                var restRequest = new RestRequest(ApiVersion + "/" + collectionName+ "/" + collectionId + "/" + route, Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                restRequest.AddBody(dataSchema);
                var restResponse = restClient.Execute<T>(restRequest);
                if (restResponse.Data != null)
                {
                    output = restResponse.Data;

                    Log.AppendLog(collectionName + "/" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return output;
        }

        #endregion

        #region UPDATE

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