using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using ARUP.IssueTracker.Windows;
using System.Windows;
using Arup.RestSharp;
using ARUP.IssueTracker.Classes.BCF2;
using System.Text;

namespace ARUP.IssueTracker.Classes
{
    class JiraUploader
    {
        private string projectKey = "";
        private List<Issue> issuesJira = new List<Issue>();
        private List<Markup> issues = new List<Markup>();
        private string path = "";
        private bool delAfter;
        ProgressWin progressWin = new ProgressWin();
        int projIndex = 0;

        //EVENTS
        public event EventHandler<IntArg> uploadComplete;

        private int uploadErrors = 0;

        BackgroundWorker worker = new BackgroundWorker();


        public JiraUploader(string pk, List<Issue> _issuesJira, List<Markup> i, string p, int pi, bool _delAfter)
        {
            //initialize
            projectKey = pk;
            issuesJira = _issuesJira;
            issues = i;
            path = p;
            projIndex = pi;
            delAfter = _delAfter;

            progressWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            progressWin.Show();
            progressWin.SetProgress(0, getProgressString(0));
            progressWin.killWorker += new EventHandler(worker_Kill);
            //bg worker


            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerAsync();
            worker.Dispose();


        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            XmlSerializer serializerV = new XmlSerializer(typeof(VisualizationInfo));

            int newIssueCounter = 0;
            int updateIssueCounter = 0;
            int unchangedIssueCounter = 0;

            for (int i = 0; i < issues.Count(); i++)
            {
                try
                {
                    Markup issue = issues[i];
                    worker.ReportProgress((100 * i + 1) / issues.Count(), getProgressString(i + 1));// HAS TO BE OUT OF THE DISPATCHER!
                    // check status on each step
                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        return; // abort work, if it's cancelled
                    }

                    //CHECK IF ALREADY EXISTING
                    // could use the expression: cf[11600] ~ "aaaa"
                    // = operator not supported
                    string fields = " AND  GUID~" + issue.Topic.Guid + "&fields=key,comment";
                    string query = "search?jql=project=" + projectKey + fields;

                    var request4 = new RestRequest(query, Method.GET);
                    request4.AddHeader("Content-Type", "application/json");
                    request4.RequestFormat = Arup.RestSharp.DataFormat.Json;
                    var response4 = JiraClient.Client.Execute<Issues>(request4);

                    if (!RestCallback.Check(response4))
                        break;

                    //DOESN'T exist already
                    if (!response4.Data.issues.Any())
                    {
                        //files to be uploaded
                        List<string> filesToBeUploaded = new List<string>();
                        if (File.Exists(Path.Combine(path, issue.Topic.Guid, "markup.bcf")))
                            filesToBeUploaded.Add(Path.Combine(path, issue.Topic.Guid, "markup.bcf"));
                        issue.Viewpoints.ToList().ForEach(vp => {
                            if (!string.IsNullOrWhiteSpace(vp.Snapshot) && File.Exists(Path.Combine(path, issue.Topic.Guid, vp.Snapshot)))
                                filesToBeUploaded.Add( Path.Combine(path, issue.Topic.Guid, vp.Snapshot) );
                            if (!string.IsNullOrWhiteSpace(vp.Viewpoint) && File.Exists(Path.Combine(path, issue.Topic.Guid, vp.Viewpoint)))
                                filesToBeUploaded.Add( Path.Combine(path, issue.Topic.Guid, vp.Viewpoint) );
                        });
                        string key = "";

                        //update view - it might be a new issue (for direct Jira upload)
                        // Serialize the object, and close the TextWriter
                        string viewpoint = Path.Combine(path, issue.Topic.Guid, "viewpoint.bcfv");
                        if (!File.Exists(viewpoint))
                        {
                            Stream writerV = new FileStream(viewpoint, FileMode.Create);
                            serializerV.Serialize(writerV, issue.Viewpoints[0].VisInfo);
                            writerV.Close();
                        }
                        if (filesToBeUploaded.Find(file => file == viewpoint) == null)
                            filesToBeUploaded.Add(viewpoint);

                        var request = new RestRequest("issue", Method.POST);
                        request.AddHeader("Content-Type", "application/json");
                        request.RequestFormat = Arup.RestSharp.DataFormat.Json;

                        newIssueCounter++;
                        var newissue =
                            new
                            {

                                fields = new Dictionary<string, object>()
    
                            };
                        newissue.fields.Add("project", new { key = projectKey });
                        //newissue.fields.Add("parent", new { key = "BCF-20" });
                        if (!string.IsNullOrWhiteSpace(issuesJira[i].fields.description)) 
                        { 
                            newissue.fields.Add("description", issuesJira[i].fields.description);
                        }   
                        newissue.fields.Add("summary", (string.IsNullOrWhiteSpace(issue.Topic.Title)) ? "no title" : issue.Topic.Title);
                        newissue.fields.Add("issuetype", new { id = issuesJira[i].fields.issuetype.id });
                        newissue.fields.Add(MySettings.Get("guidfield"), issue.Topic.Guid);

                        // validate assignee name if present
                        if (issuesJira[i].fields.assignee != null) 
                        {
                            if (!string.IsNullOrWhiteSpace(issuesJira[i].fields.assignee.name))
                            {
                                if (isAssigneeAssignable(issuesJira[i].fields.assignee.name, projectKey))
                                {
                                    newissue.fields.Add("assignee", new { name = issuesJira[i].fields.assignee.name });
                                }
                                else
                                {
                                    newissue.fields.Add("assignee", new { name = issuesJira[i].fields.creator.name });
                                }
                            }
                        }                                              
                            

                        if (issuesJira[i].fields.priority != null)
                            newissue.fields.Add("priority", new { id = issuesJira[i].fields.priority.id });

                        if (issuesJira[i].fields.components != null && issuesJira[i].fields.components.Any())
                            newissue.fields.Add("components",  issuesJira[i].fields.components );

                        if (issuesJira[i].fields.labels != null && issuesJira[i].fields.labels.Any())
                            newissue.fields.Add("labels", issuesJira[i].fields.labels);

                        request.AddBody(newissue);

                        var response = JiraClient.Client.Execute(request);

                        var responseIssue = new Issue();
                        if (RestCallback.Check(response))
                        {
                            responseIssue = Arup.RestSharp.SimpleJson.DeserializeObject<Issue>(response.Content);
                            key = responseIssue.key;//attach and comment sent to the new issue
                        }
                        else
                        {
                            uploadErrors++;
                            break;
                        }
                        
                        //upload all viewpoints and snapshots
                        var request2 = new RestRequest("issue/" + key + "/attachments", Method.POST);
                        request2.AddHeader("X-Atlassian-Token", "nocheck");
                        request2.RequestFormat = Arup.RestSharp.DataFormat.Json;
                        filesToBeUploaded.ForEach(file => request2.AddFile("file", File.ReadAllBytes(file), Path.GetFileName(file)));
                        var response2 = JiraClient.Client.Execute(request2);
                        RestCallback.Check(response2);
                        
                        //ADD COMMENTS
                        if (issue.Comment.Any())
                        {
                            issue.Comment = new System.Collections.ObjectModel.ObservableCollection<BCF2.Comment>(issue.Comment.Reverse());
                            foreach (var c in issue.Comment)
                            {
                                if (string.IsNullOrEmpty(c.Comment1)) { continue; }
                                var request3 = new RestRequest("issue/" + key + "/comment", Method.POST);
                                request3.AddHeader("Content-Type", "application/json");
                                request3.RequestFormat = Arup.RestSharp.DataFormat.Json;
                                var newcomment = new { body = c.Comment1 };
                                request3.AddBody(newcomment);
                                var response3 = JiraClient.Client.Execute<Comment2>(request3);
                                if(!RestCallback.Check(response3))
                                    break;
                            }
                        }
                        
                        if (i == issues.Count() - 1)
                        {
                            worker.ReportProgress(100, getProgressString(i + 1));
                        }
                    }
                    else //UPDATE ISSUE
                    {
                        var oldIssue = response4.Data.issues.First();
                        if (issue.Comment.Any())
                        {
                            issue.Comment = new System.Collections.ObjectModel.ObservableCollection<BCF2.Comment>(issue.Comment.Reverse());
                            int unmodifiedCommentNumber = 0;
                            foreach (var c in issue.Comment)
                            {
                                //clean all metadata annotations
                                string newComment = cleanAnnotationInComment(c.Comment1);
                                string normalized1 = Regex.Replace(newComment, @"\s", "");
                                if (oldIssue.fields.comment.comments.Any(o => Regex.Replace(cleanAnnotationInComment(o.body), @"\s", "").Equals(normalized1, StringComparison.OrdinalIgnoreCase)))
                                {
                                    unmodifiedCommentNumber++;
                                    continue;
                                }

                                var request3 = new RestRequest("issue/" + oldIssue.key + "/comment", Method.POST);
                                request3.AddHeader("Content-Type", "application/json");
                                request3.RequestFormat = Arup.RestSharp.DataFormat.Json;
                                var newcomment = new { body = c.Comment1 };
                                request3.AddBody(newcomment);
                                var response3 = JiraClient.Client.Execute<Comment2>(request3);

                                //upload viewpoint and snapshot
                                var request5 = new RestRequest("issue/" + oldIssue.key + "/attachments", Method.POST);
                                request5.AddHeader("X-Atlassian-Token", "nocheck");
                                request5.RequestFormat = Arup.RestSharp.DataFormat.Json;
                                issue.Viewpoints.ToList().ForEach(vp => {
                                    if (c.Viewpoint != null)
                                    {
                                        if(vp.Guid == c.Viewpoint.Guid)
                                        {
                                            if (File.Exists(Path.Combine(path, issue.Topic.Guid, vp.Snapshot)))
                                                request5.AddFile("file", File.ReadAllBytes(Path.Combine(path, issue.Topic.Guid, vp.Snapshot)), vp.Snapshot);
                                            if (File.Exists(Path.Combine(path, issue.Topic.Guid, vp.Viewpoint)))
                                                request5.AddFile("file", File.ReadAllBytes(Path.Combine(path, issue.Topic.Guid, vp.Viewpoint)), vp.Viewpoint);
                                        }
                                    }                                    
                                });
                                if (request5.Files.Count > 0) 
                                {
                                    var response5 = JiraClient.Client.Execute(request5);
                                    RestCallback.Check(response5);
                                }
                                

                                if (!RestCallback.Check(response3)) 
                                {
                                    break;
                                }
                                    
                            }

                            if (unmodifiedCommentNumber == issue.Comment.Count)
                            {
                                unchangedIssueCounter++;
                            }
                            else 
                            {
                                updateIssueCounter++;
                            }
                        }
                        else 
                        {
                            unchangedIssueCounter++;
                        }
                    }

                } // END TRY
                catch (System.Exception ex1)
                {
                    MessageBox.Show("exception: " + ex1);
                }


            }// END LOOP

            string msg = string.Format("{0} new issue(s) added, {1} issue(s) updated, and {2} issue(s) unchanged.", newIssueCounter, updateIssueCounter, unchangedIssueCounter);
            MessageBox.Show(msg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private string getProgressString(int i)
        {
            string s = (issues.Count > 1) ? "s" : "";
            string desc = string.Format("Uploading {0}/{1} Issue{2} to Jira...",
                                                    i,
                                                    issues.Count,
                                                    s);
            return desc;
        }
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            progressWin.SetProgress(Math.Min(e.ProgressPercentage, 100), e.UserState.ToString());
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressWin.Close();
            if (uploadErrors > 0)
            {
                string s = (uploadErrors > 1) ? "s" : "";
                MessageBox.Show(string.Format("{0} Issue{1} had errors!", uploadErrors, s), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (e.Cancelled)
            {
                MessageBox.Show("Upload Canceled!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (uploadComplete != null)
            {

                uploadComplete(this, new IntArg(projIndex));
            }
            if (delAfter)
                DeleteDirectory(path);
        }
        void worker_Kill(object sender, EventArgs e)
        {

            if (worker.IsBusy)
            {
                MessageBox.Show("exception: ");
                worker.CancelAsync();
            }

        }

        private void DeleteDirectory(string target_dir)
        {
            try
            {
                if (Directory.Exists(target_dir))
                {
                    string[] files = Directory.GetFiles(target_dir);
                    string[] dirs = Directory.GetDirectories(target_dir);
                    foreach (string file in files)
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }

                    foreach (string dir in dirs)
                    {
                        DeleteDirectory(dir);
                    }
                    Directory.Delete(target_dir, false);
                }
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }

        private string cleanAnnotationInComment(string originalComment) 
        {
            string[] lines = originalComment.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            StringBuilder commentBody = new StringBuilder();
            foreach (string line in lines)
            {
                if (!line.Contains("<Viewpoint>") && !line.Contains("<Snapshot>") && !line.Contains("<Attachment>") && !line.Contains("|width=200!"))
                {
                    commentBody.AppendLine(line);
                }
            }
            return commentBody.ToString().Trim();
        }

        /// <summary>
        /// Check if the presented assignee is valid in a project.
        /// </summary>
        /// <param name="assigneeName">The (Arup/Jira) account name of the assignee</param>
        /// <returns></returns>
        private bool isAssigneeAssignable(string assigneeName, string projectKey)    
        {
            try 
            {
                List<User> userlist = new List<User>();
                var maxresults = 1000;
                for (var i = 0; i < 100; i++)
                {
                    var apicall = "user/assignable/search?project=" +
                              projectKey + "&maxResults=" + maxresults + "&startAt=" + (i * maxresults);
                    var request = new RestRequest(apicall, Method.GET);
                    request.AddHeader("Content-Type", "application/json");
                    request.RequestFormat = Arup.RestSharp.DataFormat.Json;
                    request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                    var response = JiraClient.Client.Execute<List<User>>(request);
                    if (!RestCallback.Check(response) || !response.Data.Any())
                        break;

                    userlist.AddRange(response.Data);
                    if (response.Data.Count < maxresults)
                        break;
                }

                foreach (User user in userlist)
                {
                    if (user.name == assigneeName)
                    {
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return false;            
        }
    }
}
