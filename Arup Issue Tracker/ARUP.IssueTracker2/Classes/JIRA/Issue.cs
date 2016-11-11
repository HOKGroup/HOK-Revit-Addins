using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;


namespace ARUP.IssueTracker.Classes
{
    public class Issue
    {
        public string id { get; set; }
        public string self { get; set; }
        public string key { get; set; }

        public Fields fields { get; set; }

        public string formattedsubject
        {
            get
            {
                return "[" + key + "] " + fields.summary;
            }

        }
        public string formatteddate
        {
            get
            {
                string text = "Updated ";
                if (fields.updated == fields.created)
                    text = "Created ";
                RelativeDate rd = new RelativeDate();
                return text + rd.ToRelative(fields.updated);
            }

        }
        public string formatteddate2
        {
            get
            {
                return Convert.ToDateTime(fields.updated).ToShortDateString() + " at " + Convert.ToDateTime(fields.updated).ToShortTimeString();
            }

        }
        public string formattedstatus
        {
            get
            {
                if (null != fields.resolution && fields.resolution.name.Any())
                    return fields.resolution.name;
                else
                    return "";

            }

        }
        public string formattedguid
        {
            get
            {
                return (string.IsNullOrWhiteSpace(fields.guid)) ? "" : "GUID: " + fields.guid.ToUpper();

            }

        }
        //public string formattedusers
        //{
        //    get
        //    {
        //        string components = "none";
        //        List<User> list = fields.customfield_11400;
        //        if (list != null && list.Any())
        //        {
        //            components = "";
        //            foreach (var c in list)
        //                components += c.name + ", ";
        //            components = components.Remove(components.Count() - 2);
        //        }
        //        return components;
        //    }
        //}
        //public string formattedcomponents
        //{
        //    get
        //    {
        //        string components = "none";
        //        List<Component> list = fields.components;
        //        if (list != null && list.Any())
        //        {
        //            components = "";
        //            foreach (var c in list)
        //                components += c.name + ", ";
        //            components = components.Remove(components.Count() - 2);
        //        }
        //        return components;
        //    }
        //}
        //public string formattedmeetdate
        //{
        //    get
        //    {
        //        return (fields.customfield_11401 == null || fields.customfield_11401 == "") ? "none" : fields.customfield_11401;
        //    }
        //}
        public TextDecorationCollection formattedresolution
        {
            get
            {//fields.status.name.ToUpper() + " - " + fields.priority.name.ToUpper() + " - " +
                if (fields.resolution != null || fields.status.name.ToLower() == "resolved" || fields.status.name.ToLower() == "closed" || fields.status.name.ToLower() == "done")
                    return TextDecorations.Strikethrough;
                else
                    return null;

            }

        }
        public string formattedattach
        {
            get
            {
                string r = "";
                if (fields.attachment.Any())
                {

                    foreach (var attach in fields.attachment)
                    {
                        r += attach.filename + " ";
                    }
                }
                return r;


            }

        }
        public string formattedattachnumb
        {
            get
            {
                return "Attachments (" + fields.attachment.Count.ToString() + ")";
            }

        }
        //public string firstimage
        //{
        //    get
        //    {
        //        if (fields.attachment.Any())
        //        {

        //            foreach (var attach in fields.attachment)
        //            {
        //                if (attach.filename.Contains("snapshot.png"))
        //                    return attach.content;
        //            }
        //        }
        //        return "";
        //    }

        //}
        public string formattedcommcount
        {
            get
            {
                string c = "";
                if (fields.comment.comments.Count == 1)
                    c = fields.comment.comments.Count.ToString() + " Comment";
                else if (fields.comment.comments.Count == 0)
                    c = "No Comments";
                else
                    c = fields.comment.comments.Count.ToString() + " Comments";
                return c;
            }
        }
        public string snapshot
        {
            get
            {
                string url = "";
                if (fields.attachment != null && fields.attachment.Any() && fields.attachment.Any(o => o.filename == "snapshot.png"))
                    url = fields.attachment.First(o => o.filename == "snapshot.png").thumbnail;
                return url;
            }

        }
        public string snapshotFull
        {
            get
            {
                string url = "";
                if (fields.attachment != null && fields.attachment.Any() && fields.attachment.Any(o => o.filename == "snapshot.png"))
                    url = fields.attachment.First(o => o.filename == "snapshot.png").content;
                return url;
            }

        }
        public string viewpoint
        {
            get
            {
                string url = "";
                if (fields.attachment!=null && fields.attachment.Any() && fields.attachment.Any(o => o.filename == "viewpoint.bcfv"))
                    url = fields.attachment.First(o => o.filename == "viewpoint.bcfv").content;
                return url;
            }

        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
