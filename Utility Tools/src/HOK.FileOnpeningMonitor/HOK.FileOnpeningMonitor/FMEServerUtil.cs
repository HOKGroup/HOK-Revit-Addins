using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using RestSharp;
using HOK.Core.Utilities;
using System.Net;

namespace HOK.FileOnpeningMonitor
{
    public static class FMEServerUtil
    {
        private static string userId = "";
        private static string password = "";
        private static string apiToken = "";
        private static string host = "";
        private static int port;
        private static string clientId = "";

        public static bool RunFMEWorkspaceHTTP(CentralFileInfo info, string repository, string workspace, out Dictionary<string, string> properties)
        {
            var result = false;
            properties = new Dictionary<string, string>();
            try
            {
                var settingsString = Resources.StreamEmbeddedResource("HOK.Core.Resources.Settings.json");
                var settings = Json.Deserialize<Settings>(settingsString);

                apiToken = settings.FileOnOpeningFmeApiToken;
                host = settings.FileOnOpeningFmeHost;
                var baseUrl = $"http://{host}/fmerest/v3/";
                var resource = $"transformations/submit/{repository}/{workspace}";
                var body = new Transformation
                {
                    publishedParameters = new List<PublishedParameter>
                    {
                        new PublishedParameter { name = "FileName_pp", value = info.DocCentralPath },
                        new PublishedParameter { name = "Username_pp", value = info.UserName },
                        new PublishedParameter { name = "Office_pp", value = info.UserLocation }
                    }
                };

                var client = new RestClient(baseUrl);
                var request = new RestRequest(resource, Method.POST);
                request.AddHeader("Authorization", $"fmetoken token={apiToken}");
                request.AddJsonBody(body);
                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to run FME workspace from the FME Server.\n" + ex.Message, "Run FME Workspace", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

    }
}
