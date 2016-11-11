namespace ARUP.IssueTracker.Classes
{
   public class Issues
    {
        public string expand { get; set; }
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public System.Collections.Generic.List<Issue> issues { get; set; }
    }
}
