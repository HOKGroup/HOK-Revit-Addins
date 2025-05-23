#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;
using HOK.Core.Utilities;
// ReSharper disable InconsistentNaming

#endregion

namespace HOK.MissionControl.Core.Utils
{
    public static class ServerUtilities
    {
        public static string RestApiBaseUrl { get; set; }
        public const string ApiVersion = "api/v2";

        private static HOK.Core.Utilities.Settings Settings { get; set; }

        static ServerUtilities()
        {
            var settingsString = Resources.StreamEmbeddedResource(
                "HOK.Core.Resources.Settings.json"
            );
            RestApiBaseUrl = Json.Deserialize<HOK.Core.Utilities.Settings>(
                settingsString
            )?.HttpAddress; //production
            //RestApiBaseUrl = Json.Deserialize<Settings>(settingsString)?.HttpAddressDebug; //debug
            Settings = Json.Deserialize<HOK.Core.Utilities.Settings>(settingsString);
        }

        #region GET

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetOne<T>(string path, out T result)
            where T : new()
        {
            result = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path, Method.Get);
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var data = Json.Deserialize<T>(response.Content);
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }

                    Log.AppendLog(
                        LogMessageType.ERROR,
                        "Could not find a document matching the query."
                    );
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
        public static bool Get<T>(string path, out T result)
            where T : new()
        {
            result = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path, Method.Get);
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var data = Json.Deserialize<List<T>>(response.Content).FirstOrDefault();
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }

                    Log.AppendLog(
                        LogMessageType.ERROR,
                        "Could not find a document matching the query."
                    );
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
        public static bool GetByCentralPath<T>(string centralPath, string path, out T result)
            where T : new()
        {
            result = new T();
            try
            {
                // (Konrad) Since we cannot pass file path with "\" they were replaced with illegal pipe char "|".
                // Since pipe cannot be used in a legal file path, it's a good placeholder to use.
                // File path can also contain forward slashes for RSN and BIM 360 paths ex: RSN:// and BIM 360://
                string filePath;
                if (centralPath.Contains(@"\"))
                    filePath = centralPath.Replace(@"\", "|");
                else if (centralPath.Contains(@"/"))
                    filePath = centralPath.Replace(@"/", "|");
                else
                {
                    Log.AppendLog(
                        LogMessageType.ERROR,
                        "Could not replace \\ or / with | in the file path. Exiting."
                    );
                    return false;
                }

                var client = new RestClient(RestApiBaseUrl);

                var request = new RestRequest(ApiVersion + "/" + path + "/" + filePath, Method.Get);
                request.RequestFormat = DataFormat.Json;

                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                    return false;
                if (!string.IsNullOrEmpty(response.Content))
                {
                    var data = Json.Deserialize<List<T>>(response.Content).FirstOrDefault();
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }

                    Log.AppendLog(
                        LogMessageType.ERROR,
                        "Could not find a document with matching central path."
                    );
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

        #region POST

        /// <summary>
        /// PUTs body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="route"></param>
        public static bool Put<T>(T body, string route)
            where T : class
        {
            try
            {
                var client = new RestClient(RestApiBaseUrl);

                var request = new RestRequest(ApiVersion + "/" + route, Method.Put);
                request.RequestFormat = DataFormat.Json;
                var jsonBody = Json.Serialize<T>(body);
                request.AddBody(jsonBody, "application/json");

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
        public static async Task<T> PostAsync<T>(object body, string route)
            where T : new()
        {
            var client = new RestClient(RestApiBaseUrl);

            var request = new RestRequest(ApiVersion + "/" + route, Method.Post);
            request.RequestFormat = DataFormat.Json;
            var jsonBody = Json.Serialize(body);
            request.AddBody(jsonBody, "application/json");

            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, response.StatusDescription);
                return new T();
            }

            if (!string.IsNullOrEmpty(response.Content))
            {
                var data = Json.Deserialize<T>(response.Content);
                if (data != null)
                {
                    return data;
                }
            }
            Log.AppendLog(LogMessageType.INFO, response.StatusDescription + "-" + route);
            return new T();
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="route"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool Post<T>(object body, string route, out T result)
            where T : new()
        {
            result = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);

                var request = new RestRequest(ApiVersion + "/" + route, Method.Post);
                request.RequestFormat = DataFormat.Json;
                var jsonBody = Json.Serialize(body);
                request.AddBody(jsonBody, "application/json");

                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.Created)
                    return false;
                if (!string.IsNullOrEmpty(response.Content))
                {
                    var data = Json.Deserialize<T>(response.Content);
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }
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
        /// Retrieves information about the model, including its file size.
        /// </summary>
        /// <param name="clientPath">Base URL to the Revit Server.</param>
        /// <param name="requestPath">Request string.</param>
        /// <returns>File size in bytes.</returns>
        public static T GetFileInfoFromRevitServer<T>(string clientPath, string requestPath)
            where T : new()
        {
            var result = default(T);
            try
            {
                var client = new RestClient(clientPath);
                var request = new RestRequest(requestPath, Method.Get);
                request.AddHeader("User-Name", Environment.UserName);
                request.AddHeader("Operation-GUID", Guid.NewGuid().ToString());
                string[] clarityServers = Settings.ClarityServers;
                if (clarityServers.Any(clientPath.Contains))
                {
                    string clarityId = Settings.ClarityUserId;
                    request.AddHeader("ClarityUserId", clarityId);
                    string securityToken = Settings.ClarityToken;
                    request.AddHeader("SecurityToken", securityToken);
                    string clarityMachine = Settings.ClarityMachine;
                    request.AddHeader("User-Machine-Name", clarityMachine);
                }
                else
                {
                    request.AddHeader("User-Machine-Name", Environment.UserName + "PC");
                }

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
        [JsonPropertyName("n")]
        public int N { get; set; }

        [JsonPropertyName("nModified")]
        public int NModified { get; set; }

        [JsonPropertyName("ok")]
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
