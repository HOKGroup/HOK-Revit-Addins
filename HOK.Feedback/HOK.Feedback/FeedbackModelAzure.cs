using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HOK.Core.Utilities;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;


namespace HOK.Feedback
{
    public class FeedbackModelAzure
    {
        private Settings Settings { get; set; }

        public FeedbackModelAzure()
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Submits a feedback from user to GitHub account via Issues page.
        /// </summary>
        /// <param name="name">User Name</param>
        /// <param name="email">User Email</param>
        /// <param name="feedback">Feedback/Comments</param>
        /// <param name="toolname">Name and version of tool used to submit.</param>
        /// <returns>Message</returns>
        public async Task<WorkItem> Submit(string name, string email, string feedback, string toolname)
        {
            //Prompt user for credential
            VssConnection connection = new VssConnection(new Uri(Settings.FeedbackOrgUrl_AzureDevOps), new VssBasicCredential(string.Empty, Settings.FeedbackToken_AzureDevOps));

            //create http client and query for resutls
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            JsonPatchDocument patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = $"Test - {toolname}"
                },
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Description",
                    Value = feedback
                },
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.CreatedBy",
                    //Value =  $"${name} <${email}>"
                    Value =  $"Greg Schleusner <greg.schleusner@hok.com>"
                },
            };


            WorkItem t = await witClient.CreateWorkItemAsync(patchDocument, "HOK-Revit-Addins", "Issue");
            return t;
        }
    }
}
