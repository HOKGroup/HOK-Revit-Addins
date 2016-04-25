using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HOK.KeynoteUploader
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

        #region get
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

        public static List<KeynoteSetInfo> GetKeynoteSets(string query)
        {
            List<KeynoteSetInfo> keynoteSetList = new List<KeynoteSetInfo>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/" + query, Method.GET);
                IRestResponse<List<KeynoteSetInfo>> response = client.Execute<List<KeynoteSetInfo>>(request);
                keynoteSetList = response.Data;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return keynoteSetList;
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

        #endregion

        #region post
        public static HttpStatusCode PostProject(out string content, out string errorMessage, KeynoteProjectInfo projectInfo)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/projects", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(projectInfo);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }

        public static HttpStatusCode PostKeynoteSet(out string content, out string errorMessage, KeynoteSetInfo keynoteSetInfo)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/keynotesets", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(keynoteSetInfo);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }

        public static HttpStatusCode PostKeynote(out string content, out string errorMessage, List<KeynoteInfo> keynoteList)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/keynotes", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(keynoteList);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }

        public static HttpStatusCode PostKeynote(out string content, out string errorMessage, KeynoteInfo keynote)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/keynotes", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(keynote);

                IRestResponse response = client.Execute(request);
                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return status;
        }

        #endregion

        #region update
        public static HttpStatusCode UpdateProject(out string content, out string errorMessage, KeynoteProjectInfo projectInfo)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/projects/" + projectInfo._id, Method.PUT);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(projectInfo);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }

        public static HttpStatusCode UpdateKeynoteSet(out string content, out string errorMessage, KeynoteSetInfo setInfo)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/keynotesets/" + setInfo._id, Method.PUT);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(setInfo);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }

        public static HttpStatusCode UpdateKeynote(out string content, out string errorMessage, KeynoteInfo keynoteInfo)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/keynotes/" + keynoteInfo._id, Method.PUT);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(keynoteInfo);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }
        #endregion

        #region delete
        public static HttpStatusCode DeleteProject(out string content, out string errorMessage, string id)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/projects/" + id, Method.DELETE);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }

        public static HttpStatusCode DeleteKeynoteSet(out string content, out string errorMessage, string id)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/keynotesets/" + id, Method.DELETE);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }

        public static HttpStatusCode DeleteKeynote(out string content, out string errorMessage, string id)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/keynotes/" + id, Method.DELETE);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return status;
        }

        public static HttpStatusCode DeleteKeynoteBySetId(out string content, out string errorMessage, string setId)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            content = "";
            errorMessage = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(apiVersion + "/keynotes/setid/" + setId, Method.DELETE);

                IRestResponse response = client.Execute(request);

                content = response.Content;
                errorMessage = response.ErrorMessage;
                status = response.StatusCode;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return status;
        }
        #endregion

    }
}
