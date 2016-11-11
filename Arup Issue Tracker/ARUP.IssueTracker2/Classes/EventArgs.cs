using Arup.RestSharp;
using System;
using System.Collections.Generic;

namespace ARUP.IssueTracker.Classes
{
    public class StringArg : EventArgs
    {
        private readonly string mystring;

        public StringArg(string mystring)
    {
        this.mystring = mystring;
    }

        public string Mystring
    {
        get { return this.mystring; }
    }
    }
    public class IntArg : EventArgs
    {
        private readonly int myint;

        public IntArg(int myint)
        {
            this.myint = myint;
        }

        public int Myint
        {
            get { return this.myint; }
        }
    }

    public class ResponseArg : EventArgs
    {
        private readonly List<IRestResponse> responses;

        public ResponseArg(List<IRestResponse> responses)
        {
            this.responses = responses;
        }

        public List<IRestResponse> Responses
        {
            get { return this.responses; }
        }
    }
}
