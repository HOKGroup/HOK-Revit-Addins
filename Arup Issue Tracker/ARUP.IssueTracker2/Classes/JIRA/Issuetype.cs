namespace ARUP.IssueTracker.Classes
{
    public class Issuetype
    {
        public string self { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public bool subtask { get; set; }
        public Fields fields { get; set; }
    }
}
