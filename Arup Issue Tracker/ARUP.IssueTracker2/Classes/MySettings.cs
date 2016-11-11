using System;
using System.Configuration;
using System.Windows;

namespace ARUP.IssueTracker.Classes
{
    public static class MySettings
    {
        //http://jira.arup.com
        //https://casedesigninc.atlassian.net
        //https://testingjiravdc.atlassian.net

        //private const string _jiraservercase = "https://casedesigninc.atlassian.net";
        //private const string _jiraserverarup = "http://jira.arup.com";
        //private const string _jiraserverhok = "https://testingjiravdc.atlassian.net";
        private const string _jiraserverskanska = "https://jira.skanska.com";

        public  static string Get(string key)
        {
            try
            {
                //used to switch the hardcoded server to CASE's or ARUP based on the existence of a file on disk or not
                if (key == "jiraserver")
                {
                    string serverfile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CASE", "ARUP Issue Tracker", "usecaseserver");
                    if (System.IO.File.Exists(serverfile))
                        return System.IO.File.ReadAllText(serverfile).Replace(" ","");
                    else
                        return _jiraserverskanska;

                }
                if (key == "guidfield")
                {
                   string guidfile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CASE", "ARUP Issue Tracker", "guidfieldid");
                   if (System.IO.File.Exists(guidfile))
                       return System.IO.File.ReadAllText(guidfile).Replace(" ", "");
                   else
                       return "customfield_10104";
                        //return "customfield_10900";

                }

                Configuration config = GetConfig();

                if (config == null)
                    return string.Empty;


                KeyValueConfigurationElement element = config.AppSettings.Settings[key];
                if (element != null)
                {
                    string value = element.Value;
                    if (!string.IsNullOrEmpty(value))
                        return value;
                }
                else
                {
                    config.AppSettings.Settings.Add(key, "");
                    config.Save(ConfigurationSaveMode.Modified);
                }
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
            return string.Empty;
        }
        public static void Set(string key, string value)
        {
            try
            {
                Configuration config = GetConfig();
                if (config == null)
                    return;

                KeyValueConfigurationElement element = config.AppSettings.Settings[key];
                if (element != null)
                    element.Value = value;
                else
                    config.AppSettings.Settings.Add(key, value);

                config.Save(ConfigurationSaveMode.Modified);

            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        private static Configuration GetConfig()
        {

            string _issuetracker = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CASE", "ARUP Issue Tracker", "ARUPIssueTracker.config");

            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = _issuetracker;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            
            if (config == null)
                    MessageBox.Show("Error loading the Configuration file.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return config;
        }
    }
}
