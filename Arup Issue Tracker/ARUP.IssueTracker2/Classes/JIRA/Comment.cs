namespace ARUP.IssueTracker.Classes
{
    public partial class Comment
    {
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public System.Collections.Generic.List<Comment2> comments { get; set; }
    }
}
