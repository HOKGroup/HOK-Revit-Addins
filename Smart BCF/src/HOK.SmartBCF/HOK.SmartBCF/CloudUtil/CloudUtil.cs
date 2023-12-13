using HOK.SmartBCF.Schemas;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.CloudUtil
{
    public static class CloudUtil
    {
        public static bool useLocalServer = true;

        const string base_url_local = "http://127.0.0.1:80";
        const string base_url_global = "";
        const string api_version = "";

        public static string RestApiBaseUrl
        {
            get
            {
                return useLocalServer ? base_url_local : base_url_global;
            }
        }

        public static string RestApiUri
        {
            get
            {
                return RestApiBaseUrl + "/" + api_version;
            }
        }

        public static string Put(string collectionName, Component component)
        {
            string content = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(collectionName, Method.PUT);

                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(component);

                IRestResponse response = client.Execute(request);
                content = response.Content;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return content;
        }

        public static string Post(string collectionName, Component component)
        {
            string content = "";
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(collectionName, Method.POST);

                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(component);

                IRestResponse response = client.Execute(request);
                content = response.Content;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return content;
        }

        public static List<Component> Get(string collectionName)
        {
            var client = new RestClient(RestApiBaseUrl);

            var request = new RestRequest(collectionName, Method.GET);

            IRestResponse<List<Component>> response
              = client.Execute<List<Component>>(request);

            return response.Data;
        }



    }
}
