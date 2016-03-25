using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace HOK.SheetDataManager
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

        public LinkedProject(Guid projectId)
        {
            id = projectId;
        }
    }

    public class Discipline
    {
        private Guid id = Guid.Empty;
        private string name = "";

        public Guid Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }

        public Discipline()
        {
        }
    }

    public class RevitSheet :INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string number = "";
        private string name = "";
        private string volumeNumber = "";
        private Discipline disciplineObj = new Discipline();
        private string drawingType = "";
        private string sortedDiscipline = "";
        //extension
        private bool isSelected = false;
        private bool linked = false;
        private string currentLinkedId = "";
        private bool modified = false;
        private string toolTip = "Not Linked.";

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Number { get { return number; } set { number = value; NotifyPropertyChanged("Number"); } }
        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }
        public string VolumeNumber { get { return volumeNumber; } set { volumeNumber = value; NotifyPropertyChanged("VolumeNumber"); } }
        public Discipline DisciplineObj { get { return disciplineObj; } set { disciplineObj = value; NotifyPropertyChanged("DisciplineObj"); } }
        public string DrawingType { get { return drawingType; } set { drawingType = value; NotifyPropertyChanged("DrawingType"); } }
        public string SortedDiscipline { get { return sortedDiscipline; } set { sortedDiscipline = value; NotifyPropertyChanged("SortedDiscipline"); } }
        //extension
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }
        public bool Linked { get { return linked; } set { linked = value; NotifyPropertyChanged("Linked"); } }
        public string CurrentLinkedId { get { return currentLinkedId; } set { currentLinkedId = value; NotifyPropertyChanged("CurrentLinkedId"); } }
        public bool Modified { get { return modified; } set { modified = value; NotifyPropertyChanged("Modified"); } }
        public string ToolTip { get { return toolTip; } set { toolTip = value; NotifyPropertyChanged("ToolTip"); } }

        public RevitSheet()
        {
        }

        public RevitSheet(Guid guid)
        {
            id = guid;
        }

        public void CopySheetInfo(SheetInfo info)
        {
            number = info.SheetNumber;
            name = info.SheetName;
            volumeNumber = info.VolumeNumber;
            drawingType = info.DrawingType;
            sortedDiscipline = info.SortedDiscipline;
            linked = true;
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

    public class LinkedSheet
    {
        private Guid id = Guid.Empty;
        private Guid sheetId = Guid.Empty;
        private LinkedProject linkProject = new LinkedProject();
        private string linkedElementId = "";//uniqueId
        private bool isSource = false;

        public Guid Id { get { return id; } set { id = value; } }
        public Guid SheetId { get { return sheetId; } set { sheetId = value; } }
        public LinkedProject LinkProject { get { return linkProject; } set { linkProject = value; } }
        public string LinkedElementId { get { return linkedElementId; } set { linkedElementId = value; } }
        public bool IsSource { get { return isSource; } set { isSource = value; } }

        public LinkedSheet()
        {
        }

        public LinkedSheet(Guid guid, Guid sheetGuid, LinkedProject project, string uniqueId, bool source)
        {
            id = guid;
            sheetId = sheetGuid;
            linkProject = project;
            linkedElementId = uniqueId;
            isSource = source;
        }

        public LinkedSheet(Guid guid, Guid sheetGuid, Guid projectId, string uniqueId, bool source)
        {
            id = guid;
            sheetId = sheetGuid;
            linkProject = new LinkedProject(projectId);
            linkedElementId = uniqueId;
            isSource = source;
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
    }

    public class RevitView :INotifyPropertyChanged
    {
        private Guid id =Guid.Empty;
        private string name = "";
        private RevitSheet sheet = null;
        private RevitViewType viewType = null;
        private double locationU = 0;
        private double locationV = 0;
        //extension
        private bool linked = false;
        private bool unPlaced = false;
        private string linkedUniqueId = "";
        private string toolTip = "Not Linked.";

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }
        public RevitSheet Sheet { get { return sheet; } set { sheet = value; NotifyPropertyChanged("Sheet"); } }
        public RevitViewType ViewType { get { return viewType; } set { viewType = value; NotifyPropertyChanged("ViewType"); } }
        public double LocationU { get { return locationU; } set { locationU = value; NotifyPropertyChanged("LocationU"); } }
        public double LocationV { get { return locationV; } set { locationV = value; NotifyPropertyChanged("LocationV"); } }
        public bool Linked { get { return linked; } set { linked = value; NotifyPropertyChanged("Linked"); } }
        public bool UnPlaced { get { return unPlaced; } set { unPlaced = value; NotifyPropertyChanged("UnPlaced"); } }
        public string LinkedUniqueId { get { return linkedUniqueId; } set { linkedUniqueId = value; NotifyPropertyChanged("LinkedUniqueId"); } }
        public string ToolTip { get { return toolTip; } set { toolTip = value; NotifyPropertyChanged("ToolTip"); } }

        public RevitView()
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

    public class RevitRevision : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string description = "";
        private string issuedBy = "";
        private string issuedTo = "";
        private string date = "";
        private RevisionDocument document = new RevisionDocument();
        //extension
        private bool isSelected = false;
        private bool linked = false;
        private bool notIncluded = false; //linked but not included
        private string currentLinkedId = "";
        private bool modified = false;
        private string tooltip = "Not Linked.";

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Description { get { return description; } set { description = value; NotifyPropertyChanged("Description"); } }
        public string IssuedBy { get { return issuedBy; } set { issuedBy = value; NotifyPropertyChanged("IssuedBy"); } }
        public string IssuedTo { get { return issuedTo; } set { issuedTo = value; NotifyPropertyChanged("IssuedTo"); } }
        public string Date { get { return date; } set { date = value; NotifyPropertyChanged("Date"); } }
        public RevisionDocument Document { get { return document; } set { document = value; NotifyPropertyChanged("Document"); } }
        //extension
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }
        public bool Linked { get { return linked; } set { linked = value; NotifyPropertyChanged("Linked"); } }
        public bool NotIncluded { get { return notIncluded; } set { notIncluded = value; NotifyPropertyChanged("NotIncluded"); } }
        public string CurrentLinkedId { get { return currentLinkedId; } set { currentLinkedId = value; NotifyPropertyChanged("CurrentLinkedId"); } }
        public bool Modified { get { return modified; } set { modified = value; NotifyPropertyChanged("Modified"); } }
        public string ToolTip { get { return tooltip; } set { tooltip = value; NotifyPropertyChanged("ToolTip"); } }

        public RevitRevision()
        {
        }

        public RevitRevision(Guid guid)
        {
            id = guid;
        }

        public void CopyRevisionInfo(RevisionInfo info)
        {
            description = info.RevisionDescription;
            issuedBy = info.IssuedBy;
            issuedTo = info.IssuedTo;
            date = info.Date;
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

    public class LinkedRevision 
    {
        private Guid id = Guid.Empty;
        private Guid revisionId = Guid.Empty;
        private int sequence = -1;
        private string number = "";
        private RevisionNumberType numberType = RevisionNumberType.None;
        private LinkedProject linkProject = new LinkedProject();
        private string linkedElementId = "";
        private bool isSource = false;

        public Guid Id { get { return id; } set { id = value; } }
        public Guid RevisionId { get { return revisionId; } set { revisionId = value; } }
        public int Sequence { get { return sequence; } set { sequence = value; } }
        public string Number { get { return number; } set { number = value; } }
        public RevisionNumberType NumberType { get { return numberType; } set { numberType = value; } }
        public LinkedProject LinkProject { get { return linkProject; } set { linkProject = value; } }
        public string LinkedElementId { get { return linkedElementId; } set { linkedElementId = value; } }
        public bool IsSource { get { return isSource; } set { isSource = value; } }

        public LinkedRevision()
        {
        }

        public LinkedRevision(Guid guid, Guid revGuid, int seq, string num, RevisionNumberType numType, Guid projectId, string uniqueId, bool source)
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

        public LinkedRevision(Guid guid, Guid revGuid, int seq, string num, RevisionNumberType numType, LinkedProject project, string uniqueId, bool source)
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
        public RevisionDocument(string filePath)
        {
            if (File.Exists(filePath))
            {
                contents = File.ReadAllBytes(filePath);
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

    public class ReplaceItem
    {
        private Guid itemId = Guid.Empty;
        private ReplaceType itemType = ReplaceType.None;
        private string parameterName = "";
        private Guid sourceId = Guid.Empty;
        private string sourceValue = "";
        private string targetValue = "";

        public Guid ItemId { get { return itemId; } set { itemId = value; } }
        public ReplaceType ItemType { get { return itemType; } set { itemType = value; } }
        public string ParameterName { get { return parameterName; } set { parameterName = value; } }
        public Guid SourceId { get { return sourceId; } set { sourceId = value; } }
        public string SourceValue { get { return sourceValue; } set { sourceValue = value; } }
        public string TargetValue { get { return targetValue; } set { targetValue = value; } }

        public ReplaceItem()
        {
        }

        public ReplaceItem(Guid id, ReplaceType type, string param, Guid s_Id, string sVal, string tVal)
        {
            itemId = id;
            itemType = type;
            parameterName = param;
            sourceId = s_Id;
            sourceValue = sVal;
            targetValue = tVal;
        }
    }

}
