using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;
using System.ComponentModel;

namespace HOK.BatchExporter
{
    public class ProjectSettings
    {
        private string configFileName = "";
        private string office = "";
        private string projectName = "";

        private List<FileItem> fileItems = new List<FileItem>();
        private UpgradeOptions upgradeOptions = new UpgradeOptions();

        [XmlIgnore]
        public string ConfigFileName { get { return configFileName; } set { configFileName = value; } }
        [XmlAttribute]
        public string Office { get { return office; } set { office = value; } }
        [XmlAttribute]
        public string ProjectName { get { return projectName; } set { projectName = value; } }

        public List<FileItem> FileItems { get { return fileItems; } set { fileItems = value; } }

        public UpgradeOptions UpgradeOptions { get { return upgradeOptions; } set { upgradeOptions = value; } }

        public ProjectSettings() { }

    }

    public class FileItem
    {
        private string revitFile = "";
        private string outputFolder = "";

        public string RevitFile { get { return revitFile; } set { revitFile = value; } }
        //automatically create IFC, DWG, PDF
        public string OutputFolder { get { return outputFolder; } set { outputFolder = value; } }

        public FileItem()
        {
        }

    }

    public class ScheduleSettings
    {
        private string taskName = "";
        private DateTime startTime = DateTime.Now;
        private bool isWeeklyTrigger = false;
        private int daysOfWeek = 1;

        public string TaskName { get { return taskName; } set { taskName = value; } }
        [XmlElement(DataType = "dateTime")]
        public DateTime StartTime { get { return startTime; } set { startTime = value; } }
        public bool IsWeeklyTrigger { get { return isWeeklyTrigger; } set { isWeeklyTrigger = value; } }
        public int DaysOfWeek { get { return daysOfWeek; } set { daysOfWeek = value; } }

        public ScheduleSettings() { }
    }
    
    public class UpgradeOptions
    {
        private string configurationName = "<In-Session Setup>";
        private string upgradeVersion = "2014";
        private ScheduleSettings taskScheduleSettings = new ScheduleSettings();
        private bool isFinalUpgrade = false;
        private OpenOptions openOptions = new OpenOptions();
        private SaveAsOptions saveAsOptions = new SaveAsOptions();

        [XmlAttribute]
        public string ConfigurationName { get { return configurationName; } set { configurationName = value; } }

        public string UpgradeVersion { get { return upgradeVersion; } set { upgradeVersion = value; } }

        public ScheduleSettings TaskScheduleSettings { get { return taskScheduleSettings; } set { taskScheduleSettings = value; } }

        public bool IsFinalUpgrade { get { return isFinalUpgrade; } set { isFinalUpgrade = value; } }

        public OpenOptions UpgradeVersionOpenOptions { get { return openOptions; } set { openOptions = value; } }
        public SaveAsOptions UpgradeVersionSaveAsOptions { get { return saveAsOptions; } set { saveAsOptions = value; } }

        public UpgradeOptions() { }
    }

    public class OpenOptions
    {
        private bool audit = true;
        private bool detachAndPreserveWorksets = true;
        private bool openAllWorkset = false;

        public bool Audit { get { return audit; } set { audit = value; } }
        public bool DetachAndPreserveWorksets { get { return detachAndPreserveWorksets; } set { detachAndPreserveWorksets = value; } }
        public bool OpenAllWorkset { get { return openAllWorkset; } set { openAllWorkset = value; } }

        public OpenOptions()
        {
        }
    }

    public class SaveAsOptions
    {
        private int numOfBackups = 5;
        private bool makeCentral = true;
        private bool relinquish = false;
        private string worksetConfiguration = "AskUserToSpecify";
        private string logLocation = @"V:\HOK-Tools\BatchUpgrader\Logs";
        private string reviewLocation = "";

        public int NumOfBackups { get { return numOfBackups; } set { numOfBackups = value; } }
        public bool MakeCentral { get { return makeCentral; } set { makeCentral = value; } }

        [XmlElement, DefaultValue(false)]
        public bool Relinquish { get { return relinquish; } set { relinquish = value; } }
        public string WorksetConfiguration { get { return worksetConfiguration; } set { worksetConfiguration = value; } }
        public string LogLocation { get { return logLocation; } set { logLocation = value; } }
        public string ReviewLocation { get { return reviewLocation; } set { reviewLocation = value; } }

        public SaveAsOptions()
        { }
    }
}
