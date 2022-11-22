using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HOK.Core.Utilities;
using RestSharp;

namespace HOK.Feedback
{
    public class FeedbackModel
    {
        private Settings Settings { get; set; }
        private const string baseUrl = "https://api.github.com";

        public FeedbackModel()
        {
            var settingsString = Resources.StreamEmbeddedResource("HOK.Core.Resources.Settings.json");
            Settings = Json.Deserialize<Settings>(settingsString);
        }

        /// <summary>
        /// Async call to GitHub that removes an image.
        /// </summary>
        /// <typeparam name="T">Type of response object.</typeparam>
        /// <param name="att">Attachment object to be removed.</param>
        /// <returns>Response object.</returns>
        public async Task<T> RemoveImage<T>(AttachmentViewModel att) where T : new()
        {
            // (Konrad) Apparently it's possible that new Windows updates change the standard 
            // SSL protocol to SSL3. RestSharp uses whatever current one is while GitHub server 
            // is not ready for it yet, so we have to use TLS1.2 explicitly.
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var client = new RestClient(baseUrl);
            var request = new RestRequest(Settings.FeedbackPath + "contents/" + att.UploadImageContent.path, Method.DELETE)
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };
            request.AddHeader("Content-type", "application/json");
            request.AddHeader("Authorization", "Token " + Settings.FeedbackToken);
            request.RequestFormat = DataFormat.Json;

            request.AddJsonBody(new DeleteObject
            {
                path = att.UploadImageContent.path,
                message = "removing an image",
                sha = att.UploadImageContent.sha,
                branch = "master"
            });

            var response = await client.ExecuteAsync<T>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, response.StatusDescription);
                return new T();
            }

            return response.Data;
        }

        /// <summary>
        /// Async call to GitHub that adds an image.
        /// </summary>
        /// <typeparam name="T">Type of response object.</typeparam>
        /// <param name="att">Attachment object to be added.</param>
        /// <param name="createTemp"></param>
        /// <returns>Response object.</returns>
        public async Task<T> PostImage<T>(AttachmentViewModel att, bool createTemp) where T: new()
        {
            string tempFile;
            if (createTemp)
            {
                if (!File.Exists(att.FilePath)) return new T();

                try
                {
                    tempFile = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddTHHmmss") + "_" + Path.GetFileName(att.FilePath));
                    File.Copy(att.FilePath, tempFile);
                }
                catch (Exception e)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    return new T();
                }
            }
            else
            {
                tempFile = att.FilePath;
            }

            var bytes = File.ReadAllBytes(tempFile);
            var body = new UploadObject
            {
                path = Path.Combine("images", Path.GetFileName(tempFile)),
                message = "uploading an image",
                content = Convert.ToBase64String(bytes),
                branch = "master"
            };

            // (Konrad) Apparently it's possible that new Windows updates change the standard 
            // SSL protocol to SSL3. RestSharp uses whatever current one is while GitHub server 
            // is not ready for it yet, so we have to use TLS1.2 explicitly.
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var client = new RestClient(baseUrl);
            var request = new RestRequest(Settings.FeedbackPath + "contents/" + body.path, Method.PUT)
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };
            request.AddHeader("Content-type", "application/json");
            request.AddHeader("Authorization", "Token " + Settings.FeedbackToken);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(body);

            try
            {
                File.Delete(tempFile);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            var response = await client.ExecuteAsync<T>(request);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, response.StatusDescription);
                return new T();
            }

            return response.Data;
        }

        /// <summary>
        /// Submits a feedback from user to GitHub account via Issues page.
        /// </summary>
        /// <param name="name">User Name</param>
        /// <param name="email">User Email</param>
        /// <param name="feedback">Feedback/Comments</param>
        /// <param name="toolname">Name and version of tool used to submit.</param>
        /// <returns>Message</returns>
        public async Task<T> Submit<T>(string name, string email, string feedback, string toolname) where T: new()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("From: " + name);
            stringBuilder.AppendLine("Email: " + email);
            stringBuilder.AppendLine("User: " + Environment.UserName);
            stringBuilder.AppendLine("Machine: " + Environment.MachineName);
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

            // (Konrad) Apparently it's possible that new Windows updates change the standard 
            // SSL protocol to SSL3. RestSharp uses whatever current one is while GitHub server 
            // is not ready for it yet, so we have to use TLS1.2 explicitly.
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var client = new RestClient(baseUrl);
            var request = new RestRequest(Settings.FeedbackPath + "issues", Method.POST)
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };
            request.AddHeader("Content-type", "application/json");
            request.AddHeader("Authorization", "Token " + Settings.FeedbackToken);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(body);

            var response = await client.ExecuteAsync<T>(request);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, response.StatusDescription);
                return new T();
            }

            return response.Data;
        }
    }
}
