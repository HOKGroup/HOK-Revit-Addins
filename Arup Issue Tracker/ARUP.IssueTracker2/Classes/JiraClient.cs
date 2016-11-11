using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARUP.IssueTracker.UserControls;
using Arup.RestSharp;

namespace ARUP.IssueTracker.Classes
{
    public static class JiraClient
    {
        private static RestClient client;
        public static RestClient Client
        {
            get
            {
                return client;
            }

            set
            {
                client = value;
            }
        }

        public static Waiter Waiter;
    }
}
