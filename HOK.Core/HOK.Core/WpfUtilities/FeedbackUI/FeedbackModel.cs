using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using HOK.Core.Utilities;
using RestSharp;

namespace HOK.Core.WpfUtilities.FeedbackUI
{
    public class FeedbackModel
    {
        /// <summary>
        /// Submits a feedback from user to GitHub account via Issues page.
        /// </summary>
        /// <param name="name">User Name</param>
        /// <param name="email">User Email</param>
        /// <param name="feedback">Feedback/Comments</param>
        /// <param name="toolname">Name and version of tool used to submit.</param>
        /// <returns>Message</returns>
        public string Submit(string name, string email, string feedback, string toolname)
        {
            try
            {
                // (Konrad) This is a token and credentials for the github user we usur here
                // This user doesn't have admin rights so we cant assign labels and assignees.
                // username: hokfeedback
                // password: Password123456
                // token: fc396d894a4f27520b8ce85564c5fc2b2a15b88f

                var baseUrl = "https://api.github.com";
                var client = new RestClient(baseUrl);

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("From: " + name);
                stringBuilder.AppendLine("Email: " + email);
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("Body: ");
                stringBuilder.AppendLine(feedback);

                var body = new Issue
                {
                    title = "hokfeedback - " + toolname,
                    body = stringBuilder.ToString(),
                    assignees = new List<string>(),
                    labels = new List<string>()
                };

                try
                {
                    var request = new RestRequest("/repos/HOKGroup/HOK-Revit-Addins/issues", Method.POST)
                    {
                        OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
                    };
                    request.AddHeader("Content-type", "application/json");
                    request.AddHeader("Authorization", "Token fc396d894a4f27520b8ce85564c5fc2b2a15b88f");
                    request.RequestFormat = DataFormat.Json;
                    request.AddBody(body);

                    var response = client.Execute<Issue>(request);
                    return response.StatusCode == HttpStatusCode.Created ? "Success" : "Failed to create GitHub issue. Try again.";
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                    return ex.Message;
                }
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                return e.Message;
            }
        }
    }

    public class Issue
    {
        public string title { get; set; }
        public string body { get; set; }
        public string milestone { get; set; }
        public List<string> assignees { get; set; }
        public List<string> labels { get; set; }
    }
}
