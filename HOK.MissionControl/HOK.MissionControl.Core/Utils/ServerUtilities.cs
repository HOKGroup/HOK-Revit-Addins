using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Core.Utils
{
    public static class ServerUtilities
    {
        public static bool UseLocalServer = true;
        //public const string RestApiBaseUrl = "http://hok-184vs/";
        public const string RestApiBaseUrl = "http://localhost:8080/";
        public const string ApiVersion = "api/v2";

        #region GET

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetOne<T>(string path, out T result) where T : new()
        {
            result = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path, Method.GET);
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK) return false;

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var data = JsonConvert.DeserializeObject<T>(response.Content);
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }

                    Log.AppendLog(LogMessageType.ERROR, "Could not find a document matching the query.");
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Generic GET request that returns true if asset matching the query was successfully retrieved.
        /// </summary>
        /// <typeparam name="T">Type of the Asset that will be returned.</typeparam>
        /// <param name="path">HTTP request url.</param>
        /// <param name="result">Resulting Asset.</param>
        /// <returns>True if success, otherwise false.</returns>
        public static bool Get<T>(string path, out T result) where T : new()
        {
            result = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path, Method.GET);
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK) return false;

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(response.Content).FirstOrDefault();
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }

                    Log.AppendLog(LogMessageType.ERROR, "Could not find a document matching the query.");
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a document from collection by it's centralPath property.
        /// </summary>
        /// <param name="centralPath">Full file path with file extension.</param>
        /// <param name="path">HTTP request url.</param>
        /// <param name="result"></param>
        /// <returns>Document if found matching central path or type.</returns>
        public static bool GetByCentralPath<T>(string centralPath, string path, out T result) where T : new()
        {
            result = new T();
            try
            {
                // (Konrad) Since we cannot pass file path with "\" they were replaced with illegal pipe char "|".
                // Since pipe cannot be used in a legal file path, it's a good placeholder to use.
                // File path can also contain forward slashes for RSN and BIM 360 paths ex: RSN:// and BIM 360://
                string filePath;
                if (centralPath.Contains(@"\")) filePath = centralPath.Replace(@"\", "|");
                else if (centralPath.Contains(@"/")) filePath = centralPath.Replace(@"/", "|");
                else
                {
                    Log.AppendLog(LogMessageType.ERROR, "Could not replace \\ or / with | in the file path. Exiting.");
                    return false;
                }

                var client = new RestClient(RestApiBaseUrl);
                client.ClearHandlers();
                client.AddHandler("application/json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/x-json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/javascript", new NewtonsoftJsonSerializer());
                client.AddHandler("*+json", new NewtonsoftJsonSerializer());

                var request = new RestRequest(ApiVersion + "/" + path + "/" + filePath, Method.GET);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.JsonSerializer = new NewtonsoftJsonSerializer();

                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK) return false;
                if (!string.IsNullOrEmpty(response.Content))
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(response.Content).FirstOrDefault();
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }

                    Log.AppendLog(LogMessageType.ERROR, "Could not find a document with matching central path.");
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
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

        #endregion

        #region POST

        /// <summary>
        /// PUTs body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="route"></param>
        public static bool Put<T>(T body, string route)
        {
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                client.ClearHandlers();
                client.AddHandler("application/json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/x-json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/javascript", new NewtonsoftJsonSerializer());
                client.AddHandler("*+json", new NewtonsoftJsonSerializer());

                var request = new RestRequest(ApiVersion + "/" + route, Method.PUT);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.JsonSerializer = new NewtonsoftJsonSerializer();
                request.AddBody(body);

                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + route);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// POSTs any new data Schema. Creates new Collection in MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of return data.</typeparam>
        /// <param name="body">Body of rest call.</param>
        /// <param name="route">Route to be called.</param>
        /// <returns>Newly created Collection Schema with MongoDB assigned Id.</returns>
        public static async Task<T> PostAsync<T>(object body, string route) where T : new()
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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="route"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool Post<T>(object body, string route, out T result) where T : new()
        {
            result = default(T);
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
                if (response.StatusCode != HttpStatusCode.Created) return false;
                if (response.Data != null)
                {
                    result = response.Data;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        #endregion

        #region Revit Server REST API

        /// <summary>
        /// Retrieves information about the model, inclusing its file size.
        /// </summary>
        /// <param name="clientPath">Base URL to the Revit Server.</param>
        /// <param name="requestPath">Request string.</param>
        /// <returns>File size in bytes.</returns>
        public static T GetFileInfoFromRevitServer<T>(string clientPath, string requestPath) where T : new()
        {
            var result = default(T);
            try
            {
                var client = new RestClient(clientPath);
                var request = new RestRequest(requestPath, Method.GET);
                request.AddHeader("User-Name", Environment.UserName);
                request.AddHeader("User-Machine-Name", Environment.UserName + "PC");
                request.AddHeader("Operation-GUID", Guid.NewGuid().ToString());

                var response = client.Execute<T>(request);
                if (response.Data != null)
                {
                    return response.Data;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return result;
        }

        #endregion
    }

    public class ResponseCreated
    {
        [JsonProperty("n")]
        public int N { get; set; }

        [JsonProperty("nModified")]
        public int NModified { get; set; }

        [JsonProperty("ok")]
        public int Ok { get; set; }
    }

    /// <summary>
    /// Different states of the Revit model.
    /// </summary>
    public enum WorksetMonitorState
    {
        onopened,
        onsynched
    }
}