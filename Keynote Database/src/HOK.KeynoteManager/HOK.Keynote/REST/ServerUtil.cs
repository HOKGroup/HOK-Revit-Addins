using HOK.Keynote.ClassModels;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HOK.Keynote.REST
{
    public static class ServerUtil
    {
        public static bool UseLocalServer = false;

        public const string baseUrlLocal = "http://172.22.34.155:80";
        public const string baseUrlGlobal = "http://keynotemanager.herokuapp.com";
        public const string apiVersion = "api/v1";

        public static string RestApiBaseUrl
        {
            get
            {
                return UseLocalServer ? baseUrlLocal : baseUrlGlobal;
            }
        }

        public static string RestApiUri
        {
            get
            {
                return RestApiBaseUrl + "/" + apiVersion;
            }
        }

        public static List<KeynoteProjectInfo> GetProjects(string query)
        {
            List<KeynoteProjectInfo> projectInfoList = new List<KeynoteProjectInfo>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + query, Method.GET);
                IRestResponse<List<KeynoteProjectInfo>> response = client.Execute<List<KeynoteProjectInfo>>(request);
                projectInfoList = response.Data;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return projectInfoList;
        }

        public static List<KeynoteInfo> GetKeynotes(string query)
        {
            List<KeynoteInfo> keynotes = new List<KeynoteInfo>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + query, Method.GET);
                IRestResponse<List<KeynoteInfo>> response = client.Execute<List<KeynoteInfo>>(request);
                keynotes = response.Data;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return keynotes;
        }

        public static HttpStatusCode UpdateKeynotes(out string content, out string errorMessage, string query, KeynoteInfo keynoteInfo)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            var client = new RestClient(RestApiBaseUrl);
            var request = new RestRequest(apiVersion + "/" + query, Method.PUT);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(keynoteInfo);

            IRestResponse response = client.Execute(request);

            content = response.Content;
            errorMessage = response.ErrorMessage;

            return status;
        }

        public static string DeleteKeynote(string query)
        {
            var client = new RestClient(RestApiBaseUrl);
            var request = new RestRequest(apiVersion + "/" + query, Method.DELETE);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }

        public static HttpStatusCode PostBatch(out string content, out string errorMessage, string query, List<KeynoteInfo> keynoteList)
        {
            var client = new RestClient(RestApiBaseUrl);
            var request = new RestRequest(apiVersion + "/" + query, Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(keynoteList);

            IRestResponse response = client.Execute(request);
            content = response.Content;
            errorMessage = response.ErrorMessage;

            return response.StatusCode;
        }

    }
}
