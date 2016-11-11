using System.Collections.Generic;

namespace ARUP.IssueTracker.Classes
{
    public class Lead
    {
        public string self { get; set; }
        public string displayName { get; set; }
        public bool active { get; set; }
        public string name { get; set; }
        public AvatarUrls avatarUrls { get; set; }
        
    }
}
