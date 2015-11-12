using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetDataEditor
{
    public class RevitSheetData
    {
        private string dbFile = "";

        private Dictionary<Guid, Discipline> disciplines = new Dictionary<Guid, Discipline>();
        private Dictionary<Guid, RevitSheet> sheets = new Dictionary<Guid, RevitSheet>();
        private Dictionary<Guid, LinkedSheet> linkedSheets = new Dictionary<Guid, LinkedSheet>();
        private Dictionary<Guid, RevitRevision> revisions = new Dictionary<Guid, RevitRevision>();
        private Dictionary<Guid, LinkedRevision> linkedRevisions = new Dictionary<Guid, LinkedRevision>();
        private Dictionary<Guid, RevitView> views = new Dictionary<Guid, RevitView>();
        private Dictionary<Guid, RevitViewType> viewTypes = new Dictionary<Guid, RevitViewType>();
        private Dictionary<Guid, RevisionOnSheet> revisionMatrix = new Dictionary<Guid, RevisionOnSheet>();
        private Dictionary<Guid, ReplaceItem> replaceItems = new Dictionary<Guid, ReplaceItem>();
        private Dictionary<Guid, LinkedProject> linkedProjects = new Dictionary<Guid, LinkedProject>();

        public string DatabaseFile { get { return dbFile; } set { dbFile = value; } }
        public Dictionary<Guid, Discipline> Disciplines { get { return disciplines; } set { disciplines = value; } }
        public Dictionary<Guid, RevitSheet> Sheets { get { return sheets; } set { sheets = value; } }
        public Dictionary<Guid, LinkedSheet> LinkedSheets { get { return linkedSheets; } set { linkedSheets = value; } }
        public Dictionary<Guid, RevitRevision> Revisions { get { return revisions; } set { revisions = value; } }
        public Dictionary<Guid, LinkedRevision> LinkedRevisions { get { return linkedRevisions; } set { linkedRevisions = value; } }
        public Dictionary<Guid, RevitView> Views { get { return views; } set { views = value; } }
        public Dictionary<Guid, RevitViewType> ViewTypes { get { return viewTypes; } set { viewTypes = value; } }
        public Dictionary<Guid, RevisionOnSheet> RevisionMatrix { get { return revisionMatrix; } set { revisionMatrix = value; } }
        public Dictionary<Guid, ReplaceItem> ReplaceItems { get { return replaceItems; } set { replaceItems = value; } }
        public Dictionary<Guid, LinkedProject> LinkedProjects { get { return linkedProjects; } set { linkedProjects = value; } }

        public RevitSheetData()
        {
        }

        public RevitSheetData(string filePath)
        {
            dbFile = filePath;
        }
    }

    public class LinkedProject
    {
        private Guid id = Guid.Empty;                                   
        private string projectNumber = "";
        private string projectName = "";
        private string filePath = "";
        private DateTime linkedDate = DateTime.MinValue;
        private string linkedBy = "";

        public Guid Id { get { return id; } set { id = value; } }
        public string ProjectNumber { get { return projectNumber; } set { projectNumber = value; } }
        public string ProjectName { get { return projectName; } set { projectName = value; } }
        public string FilePath { get { return filePath; } set { filePath = value; } }
        public DateTime LinkedDate { get { return linkedDate; } set { linkedDate = value; } }
        public string LinkedBy { get { return linkedBy; } set { linkedBy = value; } }

        public LinkedProject()
        {

        }

        private LinkedProject(Guid guid, string number, string name, string path, DateTime date, string linkBy)
        {
            id = guid;
            projectNumber = number;
            projectName = name;
            filePath = path;
            linkedDate = date;
            linkedBy = linkBy;
        }
    }

    public class Discipline
    {
        private Guid id = Guid.Empty;
        private string name = "Undefined";

        public Guid Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }

        public Discipline()
        {
        }

        public Discipline(Guid guid, string disciplineName)
        {
            id = guid;
            name = disciplineName;
        }
    }

    public class RevitSheet : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string number = "";
        private string name = "";
        private string volumeNumber = "";
        private Discipline disciplineObj = new Discipline();
        private string drawingType = "";
        private string sortedDiscipline = "";

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Number { get { return number; } set { number = value; NotifyPropertyChanged("Number"); } }
        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }
        public string VolumeNumber { get { return volumeNumber; } set { volumeNumber = value; NotifyPropertyChanged("VolumeNumber"); } }
        public Discipline DisciplineObj { get { return disciplineObj; } set { disciplineObj = value; NotifyPropertyChanged("DisciplineObj"); } }
        public string DrawingType { get { return drawingType; } set { drawingType = value; NotifyPropertyChanged("DrawingType"); } }
        public string SortedDiscipline { get { return sortedDiscipline; } set { sortedDiscipline = value; NotifyPropertyChanged("SortedDiscipline"); } }

        public RevitSheet()
        {
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

    public class LinkedSheet: INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private Guid sheetId = Guid.Empty;
        private LinkedProject linkProject = new LinkedProject();
        private string linkedElementId = "";
        private bool isSource = false;

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public Guid SheetId { get { return sheetId; } set { sheetId = value; NotifyPropertyChanged("SheetId"); } }
        public LinkedProject LinkProject { get { return linkProject; } set { linkProject = value; NotifyPropertyChanged("LinkProject"); } }
        public string LinkedElementId { get { return linkedElementId; } set { linkedElementId = value; NotifyPropertyChanged("LinkedElementId"); } }
        public bool IsSource { get { return isSource; } set { isSource = value; NotifyPropertyChanged("IsSource"); } }

        public LinkedSheet()
        {
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
    
    public enum ViewTypeEnum
    {
        Undefined, FloorPlan, CeilingPlan, Elevation, ThreeD, Schedule, DrawingSheet, ProjectBrowser, Report, DraftingView, Legend, SystemBrowser, EngineeringPlan,
        AreaPlan, Section, Detail, CostReport, LoadsReport, PresureLossReport, ColumnSchedule, PanelSchedule, Walkthrough, Rendering, Internal
    }

    public class RevitViewType
    {
        private Guid id = Guid.Empty;
        private string name = "";
        private ViewTypeEnum viewType = ViewTypeEnum.Undefined;

        public Guid Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public ViewTypeEnum ViewType { get { return viewType; } set { viewType = value; } }

        public RevitViewType()
        { }

        public RevitViewType(Guid guid, string viewTypeName, ViewTypeEnum viewTypeEnum)
        {
            id = guid;
            name = viewTypeName;
            viewType = viewTypeEnum;
        }
    
    }

    public class RevitView
    {
        private Guid id = Guid.Empty;
        private string name = "";
        private RevitSheet sheet = null;
        private string sheetNumber = "";
        private RevitViewType viewType = null;
        private string viewTypeName = "";
        private double locationU = 0;
        private double locationV = 0;
        private bool isSelected = false;

        public Guid Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public RevitSheet Sheet { get { return sheet; } set { sheet = value; } }
        public string SheetNumber { get { return sheetNumber; } set { sheetNumber = value; } }
        public RevitViewType ViewType { get { return viewType; } set { viewType = value; } }
        public string ViewTypeName { get { return viewTypeName; } set { viewTypeName = value; } }
        public double LocationU { get { return locationU; } set { locationU = value; } }
        public double LocationV { get { return locationV; } set { locationV = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public RevitView()
        {
        }

        public void SetSheet(RevitSheet rSheet)
        {
            sheet = rSheet;
            sheetNumber = sheet.Number;
        }

        public void SetViewType(RevitViewType rvType)
        {
            viewType = rvType;
            viewTypeName = viewType.Name;
        }
    }

    public enum RevisionNumberType
    {
        Numeric, None, Alphanumeric
    }

    public class RevitRevision : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string description = "";
        private string issuedBy = "";
        private string issuedTo = "";
        private string date = "";
        private RevisionDocument document = new RevisionDocument();
        private bool isSelected = false;

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Description { get { return description; } set { description = value; NotifyPropertyChanged("Description"); } }
        public string IssuedBy { get { return issuedBy; } set { issuedBy = value; NotifyPropertyChanged("IssuedBy"); } }
        public string IssuedTo { get { return issuedTo; } set { issuedTo = value; NotifyPropertyChanged("IssuedTo"); } }
        public string Date { get { return date; } set { date = value; NotifyPropertyChanged("Date"); } }
        public RevisionDocument Document { get { return document; } set { document = value; NotifyPropertyChanged("Document"); } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public RevitRevision()
        {
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
        private RevisionNumberType numberType = RevisionNumberType.None;
        private LinkedProject linkProject = new LinkedProject();
        private string linkedElementId = ""; //uniqueId
        private bool isSource = false;

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public Guid RevisionId { get { return revisionId; } set { revisionId = value; } }
        public int Sequence { get { return sequence; } set { sequence = value; NotifyPropertyChanged("Sequence"); } }
        public string Number { get { return number; } set { number = value; NotifyPropertyChanged("Number"); } }
        public RevisionNumberType NumberType { get { return numberType; } set { numberType = value; NotifyPropertyChanged("NumberType"); } }
        public LinkedProject LinkProject { get { return linkProject; } set { linkProject = value; NotifyPropertyChanged("LinkProject"); } }
        public string LinkedElementId { get { return linkedElementId; } set { linkedElementId = value; NotifyPropertyChanged("LinkedElementId"); } }
        public bool IsSource { get { return isSource; } set { isSource = value; NotifyPropertyChanged("IsSource"); } }

        public LinkedRevision()
        {
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

    public class RevisionDocument
    {
        private Guid id = Guid.Empty;
        private string title = "";
        private string path = "";
        private byte[] contents;

        public Guid Id { get { return id; } set { id = value; } }
        public string Title { get { return title; } set { title = value; } }
        public string Path { get { return path; } set { path = value; } }
        public byte[] Contents { get { return contents; } set { contents = value; } }

        public RevisionDocument()
        {
        }
        public RevisionDocument(Guid guid, string filePath)
        {
            if (File.Exists(filePath))
            {
                id = guid;
                path = filePath;
                title = System.IO.Path.GetFileName(filePath);
                //contents = File.ReadAllBytes(filePath);
            }
        }
    }

    public class RevisionOnSheet
    {
        private Guid mapId = Guid.Empty;
        private Guid sheetId = Guid.Empty;
        private Guid revisionId = Guid.Empty;

        public Guid MapId { get { return mapId; } set { mapId = value; } }
        public Guid SheetId { get { return sheetId; } set { sheetId = value; } }
        public Guid RevisionId { get { return revisionId; } set { revisionId = value; } }

        public RevisionOnSheet(Guid map_id, Guid sheet_id, Guid revision_id)
        {
            mapId = map_id;
            sheetId = sheet_id;
            revisionId = revision_id;
        }
    }

    public enum ReplaceType
    {
        Sheet, View, None
    }

    public class ReplaceItem : INotifyPropertyChanged
    {
        private Guid itemId = Guid.Empty;
        private ReplaceType itemType = ReplaceType.None;
        private string parameterName = "";
        private string sourceValue = "";
        private string targetValue = "";

        public Guid ItemId { get { return itemId; } set { itemId = value; NotifyPropertyChanged("ItemId"); } }
        public ReplaceType ItemType { get { return itemType; } set { itemType = value; NotifyPropertyChanged("ItemType"); } }
        public string ParameterName { get { return parameterName; } set { parameterName = value; NotifyPropertyChanged("ParameterName"); } }
        public string SourceValue { get { return sourceValue; } set { sourceValue = value; NotifyPropertyChanged("SourceValue"); } }
        public string TargetValue { get { return targetValue; } set { targetValue = value; NotifyPropertyChanged("TargetValue"); } }

        public ReplaceItem()
        {
        }

        public ReplaceItem(Guid id, ReplaceType type, string param, string sVal, string tVal)
        {
            itemId = id;
            itemType = type;
            parameterName = param;
            sourceValue = sVal;
            targetValue = tVal;
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
