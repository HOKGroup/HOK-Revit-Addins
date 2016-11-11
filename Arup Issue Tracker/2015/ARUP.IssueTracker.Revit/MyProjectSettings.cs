using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System.Windows;

using System.Configuration;
using Autodesk.Revit.DB.Mechanical;

namespace ARUP.IssueTracker.Revit
{
    public static class MyProjectSettings
    {
//        Example project setting:
/*
<?xml version="1.0" encoding="utf-8"?>
<!--
USE: 
This file should be named as: MyRevitProject.rvt.bcfconfig
and be located in the same folder as MyRevitProject.rvt

includeZ
if set to 1 BCF views will be translated by the Z vector of the project base location,
this will result in a correct visualization in Navisworks but not in Tekla.
Default value = 0

useDefaultZoom
if set to 1 BCF views will not be zoomed of a custom value to render correctly when 
working with Tekla.
Defaul value = 0
-->
<configuration>
    <appSettings>
        <add key="includeZ" value="0" />
		<add key="useDefaultZoom" value="0" />
    </appSettings>
</configuration>
*/

        public  static string Get(string key, string project)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(project))
                    return string.Empty;

                Configuration config = GetConfig(project);

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
        //public static void Set(string key, string value, string project)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(project))
        //            return;
        //        Configuration config = GetConfig(project);
        //        if (config == null)
        //            return;

        //        KeyValueConfigurationElement element = config.AppSettings.Settings[key];
        //        if (element != null)
        //            element.Value = value;
        //        else
        //            config.AppSettings.Settings.Add(key, value);

        //        config.Save(ConfigurationSaveMode.Modified);

        //    }
        //    catch (System.Exception ex1)
        //    {
        //        MessageBox.Show("exception: " + ex1);
        //    }
        //}
        private static Configuration GetConfig(string project)
        {

            string _issuetracker = project + ".bcfconfig";
            if (File.Exists(_issuetracker))
            {


                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
                configMap.ExeConfigFilename = _issuetracker;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap,
                    ConfigurationUserLevel.None);
                if (config == null)
                    MessageBox.Show("Error loading the Configuration file.", "Configuration Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                return config;
            }
        return null;
   
        }
    }
}
