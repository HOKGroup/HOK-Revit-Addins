using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Classes
{
    public class RevitSheet:INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string number = "";
        private string name = "";
        private Discipline disciplineObj = new Discipline();
        private ObservableCollection<LinkedSheet> linkedSheets = new ObservableCollection<LinkedSheet>();
        private Dictionary<Guid/*paramId*/, SheetParameterValue> sheetParameters = new Dictionary<Guid, SheetParameterValue>();
        private Dictionary<Guid/*revisionId*/, RevisionOnSheet> sheetRevisions = new Dictionary<Guid, RevisionOnSheet>();

        private RevitLinkStatus linkStatus = new RevitLinkStatus();

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Number { get { return number; } set { number = value; NotifyPropertyChanged("Number"); } }
        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }
        public Discipline DisciplineObj { get { return disciplineObj; } set { disciplineObj = value; NotifyPropertyChanged("DisciplineObj"); } }
        public ObservableCollection<LinkedSheet> LinkedSheets { get { return linkedSheets; } set { linkedSheets = value; NotifyPropertyChanged("LinkedSheets"); } }
        public Dictionary<Guid, SheetParameterValue> SheetParameters { get { return sheetParameters; } set { sheetParameters = value; NotifyPropertyChanged("SheetParameters"); } }
        public Dictionary<Guid, RevisionOnSheet> SheetRevisions { get { return sheetRevisions; } set { sheetRevisions = value; NotifyPropertyChanged("SheetRevisions"); } }

        public RevitLinkStatus LinkStatus { get { return linkStatus; } set { linkStatus = value; NotifyPropertyChanged("LinkStatus"); } }

        public RevitSheet()
        {

        }

        public RevitSheet(Guid sheetId, string sheetNumber, string sheetName)
        {
            id = sheetId;
            number = sheetNumber;
            name = sheetName;
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

    public class Discipline : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string name = "Undefined";

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }

        public Discipline()
        {
        }

        public Discipline(Guid guid, string disciplineName)
        {
            id = guid;
            name = disciplineName;
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

    public class LinkedSheet : INotifyPropertyChanged
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

        public LinkedSheet(Guid linkId, Guid sId, LinkedProject project, string uniqueId, bool source)
        {
            id = linkId;
            sheetId = sId;
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

    public class SheetParameter : INotifyPropertyChanged
    {
        private Guid parameterId = Guid.Empty;
        private string parameterName = "";
        private string parameterType = "";

        public Guid ParameterId { get { return parameterId; } set { parameterId = value; NotifyPropertyChanged("ParameterId"); } }
        public string ParameterName { get { return parameterName; } set { parameterName = value; NotifyPropertyChanged("ParameterName"); } }
        public string ParameterType { get { return parameterType; } set { parameterType = value; NotifyPropertyChanged("ParameterType"); } }

        public SheetParameter() { }

        public SheetParameter(Guid pId, string name, string type)
        {
            parameterId = pId;
            parameterName = name;
            parameterType = type;
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

    public class SheetParameterValue : INotifyPropertyChanged
    {
        private Guid parameterValueId = Guid.Empty;
        private Guid sheetId = Guid.Empty;
        private SheetParameter parameter = new SheetParameter();
        private string parameterValue = "";

        public Guid ParameterValueId { get { return parameterValueId; } set { parameterValueId = value; NotifyPropertyChanged("ParameterValueId"); } }
        public Guid SheetId { get { return sheetId; } set { sheetId = value; NotifyPropertyChanged("SheetId"); } }
        public SheetParameter Parameter { get { return parameter; } set { parameter = value; NotifyPropertyChanged("Parameter"); } }
        public string ParameterValue { get { return parameterValue; } set { parameterValue = value; NotifyPropertyChanged("ParameterValue"); } }

        public SheetParameterValue() { }

        public SheetParameterValue(Guid pvId, Guid sId, SheetParameter param, string value)
        {
            parameterValueId = pvId;
            sheetId = sId;
            parameter = param;
            parameterValue = value;
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
