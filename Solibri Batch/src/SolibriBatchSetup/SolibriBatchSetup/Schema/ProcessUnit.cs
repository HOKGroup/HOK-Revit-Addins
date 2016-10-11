using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolibriBatchSetup.Schema
{

    public class ProcessUnit : INotifyPropertyChanged
    {
        //Private Fields
        private Guid unitId = Guid.Empty;
        private string taskName = "";
        private string taskDirectory = "";
        private OpenModel openSolibri = new OpenModel();
        private ObservableCollection<InputModel> models = new ObservableCollection<InputModel>(); //OpenModel or UpdateModel
        private ObservableCollection<OpenClassification> classifications = new ObservableCollection<OpenClassification>();
        private ObservableCollection<OpenRuleset> rulesets = new ObservableCollection<OpenRuleset>();

        private bool checkEnabled = false;
        private Check checkTask = new Check();
        private AutoComment commentTask = new AutoComment();
        private WriterReport checkingReport = new WriterReport();

        private CreatePresentation presentationCreate = new CreatePresentation();
        private UpdatePresentation presentationUpdate = new UpdatePresentation();

        private GeneralReport presentationReport = new GeneralReport();
        private BCFReport bcfReport = new BCFReport();
        private CoordinationReport coordReport = new CoordinationReport();

        private SaveModel saveSolibri = new SaveModel();
        private string[] disciplines = new string[]
        {
            "Architectural", "Air Conditioning", "Building Services", "Electrical", "Heat", "Structural", "Ventilation", "Plumbing", "Sprinkler", "Inventory", "Facility Management",
            "Landscape", "Prefab Concrete", "Steel Structure", "Site Operations", "Cooling", "Special Piping", "Process", "HVAC"
        };

        //Public Fields
        public Guid UnitId { get { return unitId; } set { unitId = value; NotifyPropertyChanged("UnitId"); } }
        public string TaskName { get { return taskName; } set { taskName = value; NotifyPropertyChanged("TaskName"); } }
        public string TaskDirectory { get { return taskDirectory; } set { taskDirectory = value; NotifyPropertyChanged("TaskDirectory"); } }
        public OpenModel OpenSolibri { get { return openSolibri; } set { openSolibri = value; NotifyPropertyChanged("OpenSolibri"); } }
        public ObservableCollection<InputModel> Models { get { return models; } set { models = value; NotifyPropertyChanged("IfcFiles"); } }
        public ObservableCollection<OpenClassification> Classifications { get { return classifications; } set { classifications = value; NotifyPropertyChanged("Classifications"); } }
        public ObservableCollection<OpenRuleset> Rulesets { get { return rulesets; } set { rulesets = value; NotifyPropertyChanged("Rulesets"); } }

        public bool CheckEnabled { get { return checkEnabled; } set { checkEnabled = value; NotifyPropertyChanged("CheckEnabled"); } }
        public Check CheckTask { get { return checkTask; } set { checkTask = value; NotifyPropertyChanged("CheckTask"); checkEnabled = checkTask.IsSpecified; } }
        public AutoComment CommentTask { get { return commentTask; } set { commentTask = value; NotifyPropertyChanged("CommentTask"); } }
        public WriterReport CheckingReport { get { return checkingReport; } set { checkingReport = value; NotifyPropertyChanged("CheckingReport"); } }

        public CreatePresentation PresentationCreate { get { return presentationCreate; } set { presentationCreate = value; NotifyPropertyChanged("PresentationCreate"); } }
        public UpdatePresentation PresentationUpdate { get { return presentationUpdate; } set { presentationUpdate = value; NotifyPropertyChanged("PresentationUpdate"); } }

        public GeneralReport PresentationReport { get { return presentationReport; } set { presentationReport = value; NotifyPropertyChanged("PresentationReport"); } }
        public BCFReport BCFReport { get { return bcfReport; } set { bcfReport = value; NotifyPropertyChanged("BCFReport"); } }
        public CoordinationReport CoordReport { get { return coordReport; } set { coordReport = value; NotifyPropertyChanged("CoordReport"); } }

        public SaveModel SaveSolibri { get { return saveSolibri; } set { saveSolibri = value; NotifyPropertyChanged("SaveSolibri"); } }
        public string[] Disciplines { get { return disciplines; } set { disciplines = value; NotifyPropertyChanged("Disciplines"); } }

        public ProcessUnit()
        {
            disciplines = disciplines.OrderBy(o => o).ToArray();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
