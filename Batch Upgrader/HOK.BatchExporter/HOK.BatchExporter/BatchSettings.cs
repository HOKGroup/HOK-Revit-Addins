using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Xml;
using Microsoft.Win32.TaskScheduler;
using System.Xml.Serialization;
using System.Collections.Specialized;

namespace HOK.BatchExporter
{
    public class BatchSettings
    {
        private string masterConfigPath2014= "";
        private string masterConfigPath2015 = "";

        private string logDirectory = @"V:\HOK-Tools\BatchUpgrader\Logs";
        private Dictionary<string/*unitConfigPath*/, ProjectSettings> projectSettingsDictionary = new Dictionary<string, ProjectSettings>();
        private StringBuilder xmlVersionMismached = new StringBuilder();

        public Dictionary<string, ProjectSettings> ProjectSettingsDictionary { get { return projectSettingsDictionary; } set { projectSettingsDictionary = value; } }

        public BatchSettings()
        {
            
        }

        public bool InitializeSettings()
        {
            bool result = false;
            try
            {
                bool createdDirectory = CreateLogDirectory();
                masterConfigPath2014 = FindMasterConfigFile("2014");
                masterConfigPath2015 = FindMasterConfigFile("2015");

                if (!string.IsNullOrEmpty(masterConfigPath2014))
                {
                    if (File.Exists(masterConfigPath2014))
                    {
                        FindUnitConfigFile(masterConfigPath2014);
                    }
                }
                
                if (!string.IsNullOrEmpty(masterConfigPath2015))
                {
                    if (File.Exists(masterConfigPath2015))
                    {
                        FindUnitConfigFile(masterConfigPath2015);
                    }
                }
                
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
                        else
                        {
                            xmlVersionMismached.AppendLine(filePath);
                        }
                    }
                    if (xmlVersionMismached.Length > 0)
                    {
                        MessageBox.Show("This tool cannot read xml configuration files created from previous versions.\n Please create a new project using the current version.\n\n" + xmlVersionMismached.ToString(),
                            "Version Mismatch", MessageBoxButton.OK);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot initialize the settings.\n" + ex.Message, "InitializeSettings", MessageBoxButton.OK);
            }
            return result;
        }

        private bool CreateLogDirectory()
        {
            bool result = false;
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create a default directory for log files in "+logDirectory+".\n"+ex.Message, "Create Log Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private string FindMasterConfigFile(string versionNumber)
        {
            string masterConfigFilePath = "";
            try
            {
                string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit "+versionNumber+"\\BatchUpgrader";
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                if (null == registryKey)
                {
                    registryKey = Registry.CurrentUser.CreateSubKey(keyAddress);
                }

                if (null == registryKey.GetValue("BatchConfiguration"))
                {
                    string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\"+versionNumber;
                    string defaultFileDirectory = appDataDirectory + @"\HOK-BatchProcessor.bundle\Contents\Resources\BatchUpgradeConfiguration.xml";
                    registryKey.SetValue("BatchConfiguration", defaultFileDirectory);
                }
                masterConfigFilePath = registryKey.GetValue("BatchConfiguration").ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot find the master configuration file.\n" + ex.Message, "FindMasterConfigFile", MessageBoxButton.OK);
                masterConfigFilePath = "";
            }
            return masterConfigFilePath;
        }

        private bool FindUnitConfigFile(string masterConfigPath)
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
                MessageBox.Show("Cannot find the unit configuration file.\n" + ex.Message, "FindUnitConfigFile", MessageBoxButton.OK);
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
                MessageBox.Show("Cannot read the project settings from \n" + configFilePath + "\n\n" + ex.Message, "ReadProjectSettings", MessageBoxButton.OK);
                return null;
            }
            return projectSettings;
        }

        public bool WriteProjectSettings(string configFile, ProjectSettings projectSetting)
        {
            bool result = false;
            try
            {
                if (File.Exists(configFile))
                {
                    File.Delete(configFile);
                }

                XmlSerializer m_serializer = new XmlSerializer(typeof(ProjectSettings));
                StreamWriter writer = new StreamWriter(configFile);
                m_serializer.Serialize(writer, projectSetting);
                writer.Close();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Project Name:" + projectSetting.ProjectName + "\nCannot write the project setting.\n" + ex.Message, "WriteProjectSettings", MessageBoxButton.OK);
            }
            return result;
        }

        public bool WriteMasterConfiguration(string versionNumber)
        {
            bool result = false;
            try
            {
                string masterConfigPath = (versionNumber == "2014") ? masterConfigPath2014 : masterConfigPath2015;
                if (File.Exists(masterConfigPath))
                {
                    File.Delete(masterConfigPath);
                }
                if (projectSettingsDictionary.Count > 0)
                {
                    using (XmlWriter writer = XmlWriter.Create(masterConfigPath))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("ConfigurationFiles"); writer.WriteWhitespace("\n");

                        var settings = from setting in projectSettingsDictionary.Values where setting.UpgradeOptions.UpgradeVersion == versionNumber select setting;

                        foreach (ProjectSettings setting in settings)
                        {
                            writer.WriteElementString("File", setting.ConfigFileName); writer.WriteWhitespace("\n");
                        }

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    result = true;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot write the master configuration file.\n"+ex.Message, "WriteMasterConfigFile", MessageBoxButton.OK);
            }
            return result;
        }

    }
}
