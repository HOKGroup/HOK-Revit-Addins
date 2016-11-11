using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ARUP.IssueTracker.Classes.BCF1
{
    public class IssueBCF
    {
        public VisualizationInfo viewpoint { get; set; }
        public Markup markup { get; set; }
        public Guid guid { get; set; }
        public string snapshot { get; set; }

        //for BCF 2.0 cache
        public ARUP.IssueTracker.Classes.BCF2.Markup bcf2Markup { get; set; }
        public ARUP.IssueTracker.Classes.BCF2.VisualizationInfo bcf2Viewpoint { get; set; }

        public IssueBCF()
        {
            guid = Guid.NewGuid();
            markup = new Markup();
            markup.Topic = new Topic();
            markup.Topic.Guid = guid.ToString();
            markup.Comment = new ObservableCollection<CommentBCF>();
            markup.Header = new HeaderFile[1];
            markup.Header[0] = new HeaderFile();
            markup.Header[0].Date = new DateTime();
            markup.Header[0].DateSpecified = true; 
            



            viewpoint = new VisualizationInfo();
            
        }
        public string formattedguid
        {
            get
            {
                return (string.IsNullOrWhiteSpace(guid.ToString())) ? "" : "GUID: " + guid.ToString().ToUpper();

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
