using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using ARUP.IssueTracker.Classes;
using ARUP.IssueTracker.UserControls;
using System.Windows;
using Arup.RestSharp;

namespace ARUP.IssueTracker.Classes
{

    class BackgroundJira
    {
        public event EventHandler<ResponseArg> WorkerComplete;
        private List<RestRequest> requests;
        private List<IRestResponse> responses = new List<IRestResponse>();
        private bool usebaseurl;
        private bool divide;
        //private IRestResponse response;

        public void Start<T>(List<RestRequest> _request, bool showmultiple = true, bool _usebaseurl = true, bool _divide = false) where T : new()
        {
            try
            {
                requests = _request;
                usebaseurl = _usebaseurl;
                divide = _divide;

                var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                worker.DoWork += new DoWorkEventHandler(worker_DoWork<T>);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

                if (null != JiraClient.Waiter)
                {
                    //add the current process to the count
                    JiraClient.Waiter.processes++;
                    //I don't want this to fire if the waiter is null
                    worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

                    if (requests.Count > 1 && showmultiple)
                    {
                        JiraClient.Waiter.progress.IsIndeterminate = false;
                        JiraClient.Waiter.progressText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        JiraClient.Waiter.progressText.Content = "";
                        JiraClient.Waiter.progressText.Visibility = Visibility.Collapsed;
                        JiraClient.Waiter.progress.IsIndeterminate = true;
                    }

                    JiraClient.Waiter.Visibility = System.Windows.Visibility.Visible;
                }

                worker.RunWorkerAsync();
                worker.Dispose();
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        void worker_DoWork<T>(object sender, DoWorkEventArgs e) where T : new()
        {
            try
            {
                var client = JiraClient.Client;
                if (!usebaseurl)
                {
                    client = new RestClient();
                    client.CookieContainer = JiraClient.Client.CookieContainer;
                }
                BackgroundWorker worker = (BackgroundWorker)sender;
                if (requests.Count > 1)
                {
                    for (var i = 0; i < requests.Count; i++)
                    {
                        worker.ReportProgress((100 * i + 1) / requests.Count(), getProgressString(i + 1));
                        // HAS TO BE OUT OF THE DISPATCHER!

                        var response = client.Execute<T>(requests[i]);
                        responses.Add(response);
                        if (!RestCallback.Check(response))
                            break;

                        if (i == requests.Count() - 1)
                        {
                            worker.ReportProgress(100, getProgressString(i + 1));
                        }
                    }
                }
                else
                {

                    var response = client.Execute<T>(requests[0]);
                    responses.Add(response);
                    RestCallback.Check(response);
                }
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }

        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (WorkerComplete != null)
            {
                WorkerComplete(this, new ResponseArg(responses));
            }
            if (null != JiraClient.Waiter)
            {
                JiraClient.Waiter.processes--;
                if (JiraClient.Waiter.processes == 0)
                    JiraClient.Waiter.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (JiraClient.Waiter != null)
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                    () =>
                    {
                        JiraClient.Waiter.progress.Value = Math.Min(e.ProgressPercentage, 100);
                        JiraClient.Waiter.progressText.Content = e.UserState.ToString();
                    }));
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }


        }
        private string getProgressString(int i)
        {
            int val = i;
            int count = requests.Count;
            if (divide)
            {
                val = val / 2;
                count = count / 2;
            }

            string desc = string.Format("Processing {0}/{1}...",
                                                    val,
                                                    count);
            return desc;
        }
    }
}
