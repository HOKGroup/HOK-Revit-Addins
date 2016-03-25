using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Classes
{
    public class RevitRevision:INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string description = "";
        private string issuedBy = "";
        private string issuedTo = "";
        private string date = "";
        private RevisionDocument document = new RevisionDocument();
        
        //linked items
        private ObservableCollection<LinkedRevision> linkedRevisions = new ObservableCollection<LinkedRevision>();
        private RevitLinkStatus linkStatus = new RevitLinkStatus();

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Description { get { return description; } set { description = value; NotifyPropertyChanged("Description"); } }
        public string IssuedBy { get { return issuedBy; } set { issuedBy = value; NotifyPropertyChanged("IssuedBy"); } }
        public string IssuedTo { get { return issuedTo; } set { issuedTo = value; NotifyPropertyChanged("IssuedTo"); } }
        public string Date { get { return date; } set { date = value; NotifyPropertyChanged("Date"); } }
        public RevisionDocument Document { get { return document; } set { document = value; NotifyPropertyChanged("Document"); } }
        
        //linked items
        public ObservableCollection<LinkedRevision> LinkedRevisions { get { return linkedRevisions; } set { linkedRevisions = value; NotifyPropertyChanged("LinkedRevisions"); } }
        public RevitLinkStatus LinkStatus { get { return linkStatus; } set { linkStatus = value; NotifyPropertyChanged("LinkStatus"); } }

        public RevitRevision()
        {
        }

        public RevitRevision(Guid revId, string revDescription, string revBy, string revTo, string revDate)
        {
            id = revId;
            description = revDescription;
            issuedBy = revBy;
            issuedTo = revTo;
            date = revDate;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
    }


    public class RevisionDocument:INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string title = "";
        private string path = "";
        private byte[] contents;

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Title { get { return title; } set { title = value; NotifyPropertyChanged("Title"); } }
        public string Path { get { return path; } set { path = value; NotifyPropertyChanged("Path"); } }
        public byte[] Contents { get { return contents; } set { contents = value; NotifyPropertyChanged("Contents"); } }

        public RevisionDocument()
        {
        }

        public RevisionDocument(Guid docId, string filePath)
        {
            id = docId;
            path = filePath;
            title = System.IO.Path.GetFileName(filePath);
        }

        public RevisionDocument(string filePath)
        {
            if (File.Exists(filePath))
            {
                contents = File.ReadAllBytes(filePath);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
    }


    public class LinkedRevision : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private Guid revisionId = Guid.Empty;
        private int sequence = -1;
        private string number = "";
        private NumberType numberType = NumberType.None;
        private LinkedProject linkProject = new LinkedProject();
        private string linkedElementId = "";
        private bool isSource = false;

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public Guid RevisionId { get { return revisionId; } set { revisionId = value; NotifyPropertyChanged("RevisionId"); } }
        public int Sequence { get { return sequence; } set { sequence = value; NotifyPropertyChanged("Sequence"); } }
        public string Number { get { return number; } set { number = value; NotifyPropertyChanged("Number"); } }
        public NumberType NumberType { get { return numberType; } set { numberType = value; NotifyPropertyChanged("NumberType"); } }
        public LinkedProject LinkProject { get { return linkProject; } set { linkProject = value; NotifyPropertyChanged("LinkProject"); } }
        public string LinkedElementId { get { return linkedElementId; } set { linkedElementId = value; NotifyPropertyChanged("LinkedElementId"); } }
        public bool IsSource { get { return isSource; } set { isSource = value; NotifyPropertyChanged("IsSource"); } }

        public LinkedRevision()
        {
        }

        public LinkedRevision(Guid guid, Guid revGuid, int seq, string num, NumberType numType, Guid projectId, string uniqueId, bool source)
        {
            id = guid;
            revisionId = revGuid;
            sequence = seq;
            number = num;
            numberType = numType;
            linkProject = new LinkedProject(projectId);
            linkedElementId = uniqueId;
            isSource = source;
        }

        public LinkedRevision(Guid guid, Guid revGuid, int seq, string num, NumberType numType, LinkedProject project, string uniqueId, bool source)
        {
            id = guid;
            revisionId = revGuid;
            sequence = seq;
            number = num;
            numberType = numType;
            linkProject = project;
            linkedElementId = uniqueId;
            isSource = source;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
    }

    public enum NumberType
    {
        Numeric, None, Alphanumeric
    }

    public class RevisionOnSheet : INotifyPropertyChanged
    {
        private Guid mapId = Guid.Empty;
        private Guid sheetId = Guid.Empty;
        private RevitRevision rvtRevision = new RevitRevision();
        private bool include = false;

        private RevitLinkStatus linkStatus = new RevitLinkStatus();

        public Guid MapId { get { return mapId; } set { mapId = value; NotifyPropertyChanged("MapId"); } }
        public Guid SheetId { get { return sheetId; } set { sheetId = value; NotifyPropertyChanged("SheetId"); } }
        public RevitRevision RvtRevision { get { return rvtRevision; } set { rvtRevision = value; NotifyPropertyChanged("RvtRevision"); } }
        public bool Include { get { return include; } set { include = value; NotifyPropertyChanged("Include"); } }

        public RevitLinkStatus LinkStatus { get { return linkStatus; } set { linkStatus = value; NotifyPropertyChanged("LinkStatus"); } }

        public RevisionOnSheet() { }

        public RevisionOnSheet(Guid map_id, Guid sheet_id, RevitRevision revision, bool included)
        {
            mapId = map_id;
            sheetId = sheet_id;
            rvtRevision = revision;
            include = included;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
    }
}
