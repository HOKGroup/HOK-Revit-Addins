using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using Autodesk.Revit.DB;
using Microsoft.Win32.TaskScheduler;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using System.Xml.Serialization;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, All = "HKEY_CURRENT_USER")]

namespace HOK.BatchExporterAddIn
{
    public class DataReader
    {
#if RELEASE2014
        private string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2014\\BatchUpgrader";
        private string revitVersion = "2014";
#elif RELEASE2015
        private string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2015\\BatchUpgrader";
        private string revitVersion="2015";
#endif
        private string masterConfigPath = "";
        private Dictionary<string, ProjectSettings> projectSettingsDictionary = new Dictionary<string, ProjectSettings>();

        public Dictionary<string, ProjectSettings> ProjectSettingsDictionary { get { return projectSettingsDictionary; } set { projectSettingsDictionary = value; } }

        public DataReader()
        {
            InitializeSettings();
        }

        public bool InitializeSettings()
        {
            bool result = false;
            try
            {
                if (FindMasterConfigFile())
                {
                    if (FindUnitConfigFile())
                    {
                        if (projectSettingsDictionary.Count > 0)
                        {
                            List<string> configFilePaths = projectSettingsDictionary.Keys.ToList();
                            foreach (string filePath in configFilePaths)
                            {
                                bool isReadable = false;
                                ProjectSettings projectSettings = ReadProjectSettings(filePath, out isReadable);
                                projectSettings.ConfigFileName = filePath;
                                projectSettingsDictionary.Remove(filePath);
                                if (isReadable)
                                {
                                    projectSettingsDictionary.Add(filePath, projectSettings);
                                }
                            }
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string exceptionMsg = ex.Message;
            }
            return result;
        }

        private bool FindMasterConfigFile()
        {
            try
            {
                bool existConfig = false;
               
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                if (null == registryKey)
                {
                    registryKey = Registry.CurrentUser.CreateSubKey(keyAddress);
                }

                if (null == registryKey.GetValue("BatchConfiguration"))
                {
                    string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\" + revitVersion;
                    string defaultFileDirectory = appDataDirectory + @"\HOK-BatchProcessor.bundle\Contents\Resources\BatchUpgradeConfiguration.xml";
                    registryKey.SetValue("BatchConfiguration", defaultFileDirectory);
                }

                masterConfigPath = registryKey.GetValue("BatchConfiguration").ToString();
                if (File.Exists(masterConfigPath))
                {
                    existConfig = true;
                }
                else
                {
                    existConfig = false;
                }
                return existConfig;
            }
            catch (Exception ex)
            {
                string exceptionMsg = ex.Message;
                return false;
            }
        }

        private bool FindUnitConfigFile()
        {
            try
            {
                FileStream fs = new FileStream(masterConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                XmlReader reader = XmlReader.Create(fs);
                while (reader.Read())
                {
                    if (reader.IsStartElement() && reader.Name == "File")
                    {
                        reader.ReadStartElement("File");
                        string filePath = reader.ReadString();
                        if (File.Exists(filePath) && !projectSettingsDictionary.ContainsKey(filePath))
                        {
                            ProjectSettings projectSettings = new ProjectSettings();
                            projectSettingsDictionary.Add(filePath, projectSettings);
                        }
                    }
                }
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                string exceptionMsg = ex.Message;
                return false;
            }
        }

        public ProjectSettings ReadProjectSettings(string configFilePath, out bool isReadable)
        {
            ProjectSettings projectSettings = new ProjectSettings();
            isReadable = false;
            try
            {
                XmlSerializer m_serializer = new XmlSerializer(typeof(ProjectSettings));
                FileStream fs = new FileStream(configFilePath, FileMode.Open);
                XmlReader reader = XmlReader.Create(fs);
                if (m_serializer.CanDeserialize(reader))
                {
                    projectSettings = (ProjectSettings)m_serializer.Deserialize(reader);
                    projectSettings.ConfigFileName = configFilePath;
                    isReadable = true;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot read the project settings from \n" + configFilePath + "\n\n" + ex.Message, "ReadProjectSettings");
                return null;
            }
            return projectSettings;
        }
    }

    public class TaskScheduleSettings
    {
        private string taskName = "";
        public string TaskName { get { return taskName; } set { taskName = value; } }

        public TaskDefinition TaskDefinitionObject { get; set; }

        private DateTime startTime = DateTime.Now;
        public DateTime StartTime { get { return startTime; } set { startTime = value; } }

        private DaysOfTheWeek daysofWeek = DaysOfTheWeek.Monday;
        public DaysOfTheWeek DaysOfWeek { get { return daysofWeek; } set { daysofWeek = value; } }
    }
}
