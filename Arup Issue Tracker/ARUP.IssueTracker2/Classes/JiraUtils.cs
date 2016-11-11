using Arup.RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARUP.IssueTracker.Classes
{
    public class JiraUtils
    {

        /// <summary>
        /// Get the list of possible Status Transitions for an Issue
        /// </summary>
        /// <param name="IssueKey">Jira Key of the Issue</param>
        /// <returns></returns>
        public static List<Transition> GeTransitions(string IssueKey)
        {
            var request = new RestRequest("issue/" + IssueKey + "/transitions", Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = Arup.RestSharp.DataFormat.Json;

            var response2 = JiraClient.Client.Execute<IssueTransitions>(request);

            if (RestCallback.Check(response2) && response2.Data.transitions.Any())
                return response2.Data.transitions;
            return null;
        }
        /// <summary>
        /// Changes status to an issue on a given Transition ID
        /// </summary>
        /// <param name="IssueKey">Jira Key of the Issue</param>
        /// <param name="TransitionId">Jira Transition ID</param>
        /// <param name="waiter">Background waiter animation</param>
        /// <param name="eventHandler">Event handler to fire after the action is complete</param>
        public static void SetTransition(string IssueKey, string TransitionId,
            EventHandler<ResponseArg> eventHandler)
        {
            var request =
                new RestRequest(
                    "issue/" + IssueKey + "/transitions",
                    Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = Arup.RestSharp.DataFormat.Json;
            var newcomment = new { transition = new { id = TransitionId } };
            request.AddBody(newcomment);
            List<RestRequest> requests = new List<RestRequest>();
            requests.Add(request);
            BackgroundJira bj = new BackgroundJira();
            bj.WorkerComplete += eventHandler;
            bj.Start<Status>(requests);
        }
        /// <summary>
        /// Changes the priority of a list of issues on a given priority ID
        /// </summary>
        /// <param name="IssueKeys"></param>
        /// <param name="PriorityId"></param>
        /// <param name="waiter"></param>
        /// <param name="eventHandler"></param>
        public static void SetPriorities(List<string> IssueKeys, string PriorityId,
            EventHandler<ResponseArg> eventHandler)
        {
            List<RestRequest> requests = new List<RestRequest>();
            foreach (var issueKey in IssueKeys)
            {
                var request = new RestRequest("issue/" + issueKey, Method.PUT);
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = Arup.RestSharp.DataFormat.Json;
                var newissue =
                    new
                    {
                        fields = new
                        {
                            priority = new
                            {
                                id = PriorityId
                            }
                        }
                    };
                request.AddBody(newissue);
                requests.Add(request);
            }
            BackgroundJira bj = new BackgroundJira();
            bj.WorkerComplete += eventHandler;
            bj.Start<Priority>(requests);
        }


    }
}
