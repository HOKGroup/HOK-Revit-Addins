using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;

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
        public const string RestApiBaseUrl = "http://hok-184vs/";
        //public const string RestApiBaseUrl = "http://localhost:8080/";
        public const string ApiVersion = "api/v1";

        #region GET

        public static T Get<T>(string path)
        {
            var result = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path, Method.GET);
                var response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.InternalServerError) return result;

                if (!string.IsNullOrEmpty(response.Content))
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(response.Content).FirstOrDefault();
                    if (data != null) result = data;

                    Log.AppendLog(LogMessageType.ERROR, "Could not find a document with matching central path.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Retrieves a document from collection by it's centralPath property.
        /// </summary>
        /// <param name="centralPath">Full file path with file extension.</param>
        /// <param name="path">HTTP request url.</param>
        /// <returns>Document if found matching central path or type.</returns>
        public static T GetByCentralPath<T>(string centralPath, string path)
        {
            var result = default(T);
            try
            {
                // (Konrad) Since we cannot pass file path with "\" they were replaced with illegal pipe char "|".
                // Since pipe cannot be used in a legal file path, it's a good placeholder to use.
                // File path can also contain forward slashes for RSN and A360 paths ex: RSN:// and A360://
                string filePath;
                if (centralPath.Contains(@"\")) filePath = centralPath.Replace(@"\", "|");
                else if (centralPath.Contains(@"/")) filePath = centralPath.Replace(@"/", "|");
                else
                {
                    Log.AppendLog(LogMessageType.ERROR, "Could not replace \\ or / with | in the file path. Exiting.");
                    return result;
                }

                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path + "/" + filePath, Method.GET);
                var response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.InternalServerError) return result;

                if (!string.IsNullOrEmpty(response.Content))
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(response.Content).FirstOrDefault();
                    if (data != null) result = data;
                    else Log.AppendLog(LogMessageType.ERROR, "Could not find a document with matching central path.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Retrieves all objects in a collection by route.
        /// </summary>
        /// <typeparam name="T">Type to be returned.</typeparam>
        /// <param name="route">Route to make the request to.</param>
        /// <returns>A List of Type objects if any were found.</returns>
        public static async Task<List<T>> FindAll<T>(string route) where T : new()
        {
            var client = new RestClient(RestApiBaseUrl);
            var request = new RestRequest(ApiVersion + "/" + route, Method.GET);
            var response = await client.ExecuteTaskAsync<List<T>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, response.StatusDescription);
                return new List<T>();
            }

            Log.AppendLog(LogMessageType.INFO, response.StatusDescription + "-" + route);
            return response.Data;
        }

        /// <summary>
        /// Retrieves a Collection from MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of response class.</typeparam>
        /// <param name="responseType">Response object type.</param>
        /// <param name="route">Route to post request to.</param>
        /// <returns></returns>
        [Obsolete]
        public static List<T> FindAll<T>(T responseType, string route) where T : new()
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

                    Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return items;
        }

        /// <summary>
        /// Retrieves a Collection from MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of response class.</typeparam>
        /// <param name="route">Route to post request to.</param>
        /// <returns></returns>
        public static T FindOne<T>(string route) where T : new()
        {
            var items = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + route, Method.GET);
                var response = client.Execute<T>(request);
                if (response.Data != null)
                {
                    items = response.Data;

                    Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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

                Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-triggerrecords");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return status;
        }

        /// <summary>
        /// PUTs created Health Record Id into Project's healthrecords array.
        /// </summary>
        /// <param name="project">Project class.</param>
        /// <param name="id"></param>
        public static void AddHealthRecordToProject(Project project, string id)
        {
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(
                    ApiVersion + "/projects/" + project.Id + "/addhealthrecord/" + id, Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddBody(project);
                var response = client.Execute(request);
                Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-addhealthrecord");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// PUTs body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="route"></param>
        public static void Put<T>(T body, string route)
        {
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(
                    ApiVersion + "/" + route, Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddBody(body);
                var response = client.Execute(request);
                Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + route);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// POSTs any new data Schema. Creates new Collection in MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of return data.</typeparam>
        /// <param name="body">Body of rest call.</param>
        /// <param name="route">Route to be called.</param>
        /// <returns>Newly created Collection Schema with MongoDB assigned Id.</returns>
        public static async Task<T> POST<T>(object body, string route) where T : new()
        {
            var client = new RestClient(RestApiBaseUrl);
            client.ClearHandlers();
            client.AddHandler("application/json", new NewtonsoftJsonSerializer());
            client.AddHandler("text/json", new NewtonsoftJsonSerializer());
            client.AddHandler("text/x-json", new NewtonsoftJsonSerializer());
            client.AddHandler("text/javascript", new NewtonsoftJsonSerializer());
            client.AddHandler("*+json", new NewtonsoftJsonSerializer());

            var request = new RestRequest(ApiVersion + "/" + route, Method.POST);
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddHeader("Content-type", "application/json");
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = new NewtonsoftJsonSerializer();
            request.AddBody(body);

            var response = await client.ExecuteTaskAsync<T>(request);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, response.StatusDescription);
                return new T();
            }

            Log.AppendLog(LogMessageType.INFO, response.StatusDescription + "-" + route);
            return response.Data;
        }

        /// <summary>
        /// POSTs any new data Schema. Creates new Collection in MongoDB.
        /// </summary>
        /// <returns>Newly created Collection Schema with MongoDB assigned Id.</returns>
        [Obsolete]
        public static T Post<T>(object body, string route) where T : new()
        {
            var resresponse = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                client.ClearHandlers();
                client.AddHandler("application/json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/x-json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/javascript", new NewtonsoftJsonSerializer());
                client.AddHandler("*+json", new NewtonsoftJsonSerializer());

                var request = new RestRequest(ApiVersion + "/"+ route, Method.POST);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.JsonSerializer = new NewtonsoftJsonSerializer();
                request.AddBody(body);

                var response = client.Execute<T>(request);
                if (response.Data != null)
                {
                    resresponse = response.Data;
                    Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-" + route);
                }
                else
                {
                    resresponse = default(T);
                    Log.AppendLog(LogMessageType.ERROR, response.ResponseStatus + "-" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
                var restRequest = new RestRequest(ApiVersion + "/healthrecords/" + worksetDocumentId + "/sessioninfo/" + objectId, Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };
                restRequest.AddQueryParameter("action", action);
                httpStatusCode = restClient.Execute(restRequest).StatusCode;

                Log.AppendLog(LogMessageType.INFO, httpStatusCode + "-sessioninfo-" + action);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return httpStatusCode;
        }

        #endregion

        #region Revit Server REST API

        /// <summary>
        /// Retrieves information about the model, inclusing its file size.
        /// </summary>
        /// <param name="clientPath">Base URL to the Revit Server.</param>
        /// <param name="requestPath">Request string.</param>
        /// <returns>File size in bytes.</returns>
        public static int GetFileInfoFromRevitServer(string clientPath, string requestPath)
        {
            var size = 0;
            try
            {
                var client = new RestClient(clientPath);
                var request = new RestRequest(requestPath, Method.GET);
                request.AddHeader("User-Name", Environment.UserName);
                request.AddHeader("User-Machine-Name", Environment.UserName + "PC");
                request.AddHeader("Operation-GUID", Guid.NewGuid().ToString());

                var response = client.Execute<RsFileInfo>(request);
                if (response.Data != null)
                {
                    size = response.Data.ModelSize;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return size;
        }

        #endregion
    }
}