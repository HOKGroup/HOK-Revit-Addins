using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolibriBatchSetup.Schema
{
    public class AutorunSettings : INotifyPropertyChanged
    {
        private List<SolibriProperties> solibriOptions = new List<SolibriProperties>();
        private List<RemoteMachine> remoteOptions = new List<RemoteMachine>();

        private SolibriProperties solibriSetup = new SolibriProperties();
        private RemoteMachine remoteSetup = new RemoteMachine();
        private ObservableCollection<OpenClassification> classifications = new ObservableCollection<OpenClassification>();
        private ObservableCollection<OpenRuleset> rulesets = new ObservableCollection<OpenRuleset>();
        private ReportSettings reportSettings = new ReportSettings();
        private SaveModelSettings saveSolibriSettings = new SaveModelSettings();

        public List<SolibriProperties> SolibriOptions { get { return solibriOptions; } set { solibriOptions = value; } }
        public List<RemoteMachine> RemoteOptions { get { return remoteOptions; } set { remoteOptions = value; } }

        public SolibriProperties SolibriSetup { get { return solibriSetup; } set { solibriSetup = value; NotifyPropertyChanged("SolibriSetup"); } }
        public RemoteMachine RemoteSetup { get { return remoteSetup; } set { remoteSetup = value; NotifyPropertyChanged("RemoteSetup"); } }
        public ObservableCollection<OpenClassification> Classifications { get { return classifications; } set { classifications = value; NotifyPropertyChanged("Classifications"); } }
        public ObservableCollection<OpenRuleset> Rulesets { get { return rulesets; } set { rulesets = value; NotifyPropertyChanged("Rulesets"); } }
        public ReportSettings ReportSettings { get { return reportSettings; } set { reportSettings = value; NotifyPropertyChanged("ReportSettings"); } }
        public SaveModelSettings SaveSolibriSettings { get { return saveSolibriSettings; } set { saveSolibriSettings = value; NotifyPropertyChanged("SaveSolibriSettings"); } }
        
        public AutorunSettings()
        {
            SolibriProperties sp = new SolibriProperties("Solibri Model Checker v9.5", @"C:\Program Files\Solibri\SMCv9.5\Solibri Model Checker v9.5.exe");
            solibriOptions.Add(sp);
            sp = new SolibriProperties("Solibri Model Checker v9.6", @"C:\Program Files\Solibri\SMCv9.6\Solibri Model Checker v9.6.exe");
            solibriOptions.Add(sp);
            sp = new SolibriProperties("Solibri Model Checker v9.7", @"C:\Program Files\Solibri\SMCv9.7\Solibri Model Checker v9.7.exe");
            solibriOptions.Add(sp);
            solibriOptions = solibriOptions.OrderBy(o => o.VersionNumber).ToList();

            RemoteMachine rm = new RemoteMachine("NY", "NY-BAT-D001", @"\\NY-BAT-D001\SolibriBatch");
            remoteOptions.Add(rm);
            RemoteMachine rm2 = new RemoteMachine("PHI", "PHI-BAT-D001", @"\\PHI-BAT-D001\SolibriBatch");
            remoteOptions.Add(rm2);
            RemoteMachine rm3 = new RemoteMachine("HOU", "HOU-BAT-D001", @"\\HOU-BAT-D001\SolibriBatch");
            remoteOptions.Add(rm3);
            remoteOptions = remoteOptions.OrderBy(o => o.ComputerName).ToList();

        }

        public AutorunSettings(SolibriProperties solibri, RemoteMachine remote)
        {
            solibriSetup = solibri;
            remoteSetup = remote;
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

    public class SolibriProperties : INotifyPropertyChanged
    {
        private string versionNumber = "";
        private string exeFile = "";

        public string VersionNumber { get { return versionNumber; } set { versionNumber = value; NotifyPropertyChanged("VersionNumber"); } }
        public string ExeFile { get { return exeFile; } set { exeFile = value; NotifyPropertyChanged("ExeFile"); } }

        public SolibriProperties()
        {
        }

        public SolibriProperties(string version, string exe)
        {
            versionNumber = version;
            exeFile = exe;
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

    public class RemoteMachine : INotifyPropertyChanged
    {
        private string location = "";
        private string computerName = "";
        private string directoryName = "";

        public string Location { get { return location; } set { location = value; NotifyPropertyChanged("Location"); } }
        public string ComputerName { get { return computerName; } set { computerName = value; NotifyPropertyChanged("ComputerName"); } }
        public string DirectoryName { get { return directoryName; } set { directoryName = value; NotifyPropertyChanged("DirectoryName"); } }

        public RemoteMachine()
        {
        }

        public RemoteMachine(string officeLocation, string comName, string directory)
        {
            location = officeLocation;
            computerName = comName;
            directoryName = directory;
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

    public class SaveModelSettings : INotifyPropertyChanged
    {
        private bool saveInPlace = true;
        private string outputFolder = "";
        private bool appendDate = true;

        public bool SaveInPlace { get { return saveInPlace; } set { saveInPlace = value; NotifyPropertyChanged("SaveInPlace"); } }
        public string OutputFolder { get { return outputFolder; } set { outputFolder = value; NotifyPropertyChanged("OutputFolder"); } }
        public bool AppendDate { get { return appendDate; } set { appendDate = value; NotifyPropertyChanged("AppendDate"); } }

        public SaveModelSettings()
        {
        }

        public SaveModelSettings(bool inPlace, string folderName, bool append)
        {
            saveInPlace = inPlace;
            outputFolder = folderName;
            appendDate = append;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class ReportSettings :SaveModelSettings
    {
        private bool isCheckingSelected = false;
        private bool isPresentationSelected = false;
        private bool isBCFSelected = false;
        private bool isCoordinationSelected = false;
        private string coordinationTemplate = "";

        public bool IsCheckingSelected { get { return isCheckingSelected; } set { isCheckingSelected = value; NotifyPropertyChanged("IsCheckingSelected"); } }
        public bool IsPresentationSelected { get { return isPresentationSelected; } set { isPresentationSelected = value; NotifyPropertyChanged("IsPresentationSelected"); } }
        public bool IsBCFSelected { get { return isBCFSelected; } set { isBCFSelected = value; NotifyPropertyChanged("IsBCFSelected"); } }
        public bool IsCoordinationSelected { get { return isCoordinationSelected; } set { isCoordinationSelected = value; NotifyPropertyChanged("IsCoordinationSelected"); } }
        public string CoordinationTemplate { get { return coordinationTemplate; } set { coordinationTemplate = value; NotifyPropertyChanged("CoordinationTemplate"); } }

        public ReportSettings() 
        {
            
        }

        public ReportSettings(bool inPlace, string folderName, bool append, bool checkSelected, bool presentationSelected, bool bcfSelected, bool coordSelected)
        {
            this.SaveInPlace = inPlace;
            this.OutputFolder = folderName;
            this.AppendDate = append;
            isCheckingSelected = checkSelected;
            isPresentationSelected = presentationSelected;
            isBCFSelected = bcfSelected;
            isCoordinationSelected = coordSelected;
        }
    }

    public enum BatchFileType
    {
        None, Solibri, IFC, BCF, Ruleset, PDF
    }
}
