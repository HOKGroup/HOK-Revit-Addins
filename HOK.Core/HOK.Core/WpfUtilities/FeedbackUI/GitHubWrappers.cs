using System.Collections.Generic;

namespace HOK.Core.WpfUtilities.FeedbackUI
{
    public class Response
    {
        public UploadImageContent content { get; set; }
        public UploadCommit commit { get; set; }
    }

    public class UploadCommit
    {
        public string sha { get; set; }
    }

    public class UploadImageContent
    {
        public string path { get; set; }
        public string sha { get; set; }
        public string name { get; set; }
        public string html_url { get; set; }
    }

    public class DeleteObject
    {
        public string path { get; set; }
        public string message { get; set; }
        public string sha { get; set; }
        public string branch { get; set; }
    }

    public class UploadObject
    {
        public string path { get; set; }
        public string message { get; set; }
        public string content { get; set; }
        public string branch { get; set; }
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
