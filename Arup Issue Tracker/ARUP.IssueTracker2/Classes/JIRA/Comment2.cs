using System;

namespace ARUP.IssueTracker.Classes
{
    public class Comment2
    {
        public string self { get; set; }
        public string id { get; set; }
        public Author author { get; set; }
        public string body { get; set; }
        public UpdateAuthor updateAuthor { get; set; }
        public string created { get; set; }
        public string updated { get; set; }

        //for multiple snapshots/viewpoints
        public string viewpointFileName { get; set; }
        public string viewpointFileUrl { get; set; }
        public string snapshotFileName { get; set; }
        public string snapshotThumbnailUrl { get; set; }
        public string snapshotFullUrl { get; set; }

        public string formatteddate
        {
            get
            {
                RelativeDate rd = new RelativeDate();
                return rd.ToRelative(created);
            }

        }
        public string formatteddate2
        {
            get
            {
                return Convert.ToDateTime(created).ToShortDateString() + " - " + Convert.ToDateTime(created).ToShortTimeString();
            }

        }
        public string expTitle
        {
            get
            {
                string c = "";
                if (null != body)
                {
                    c = body;
                    if (c.Length > 100)
                        c = c.Substring(0, 100) + "...";
                    c = c.Replace(System.Environment.NewLine, " ");
                }
                return c;
            }

        }
    }
}
