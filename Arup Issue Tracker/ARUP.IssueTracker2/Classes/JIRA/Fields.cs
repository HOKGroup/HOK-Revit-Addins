using System.Collections.Generic;

namespace ARUP.IssueTracker.Classes
{
    public class Fields
    {
        public Resolution resolution { get; set; }
        public Priority priority { get; set; }
        public User creator { get; set; }
        public Project project { get; set; }
        public string summary { get; set; }
        public Status status { get; set; }
        public string created { get; set; }
        public string updated { get; set; }
        public string description { get; set; }
        public User assignee { get; set; }
        public Issuetype issuetype { get; set; }
        public List<Attachment> attachment { get; set; }
        public Comment comment { get; set; }
        public List<Component> components { get; set; }
        public string customfield_10104 { get; set; }
        public string customfield_10105 { get; set; }
        public List<string> labels { get; set; }

        // for filtering attachments without viewpoints/snapshots
        public List<Attachment> filteredAttachments 
        {
            get 
            {
                List<Attachment> filtered = new List<Attachment>();
                attachment.ForEach(o => {
                    if (o.filename != "markup.bcf" && o.filename != "viewpoint.bcfv" && o.filename != "snapshot.png" && !comment.comments.Exists(c => c.snapshotFileName == o.filename) && !comment.comments.Exists(c => c.viewpointFileName == o.filename))
                    {
                        filtered.Add(o);
                    }
                });

                return filtered;
            } 
        }

        public string guid
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(customfield_10104))
                    return customfield_10104;

                if (!string.IsNullOrWhiteSpace(customfield_10105))
                    return customfield_10105;
                 
                return string.Empty;
            }
        }
        //public List<User> customfield_11400 { get; set; }
        //public List<Component> components { get; set; }
        
    }


}
