using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Text.RegularExpressions;
using HOK.ModelManager.RegistryManager;

namespace HOK.ModelManager.GoogleDocs
{
    public class ModelInfo
    {
        private Document m_doc;
        private string docCentralPath = "";
        private string docTitle = "";
        private bool validCentral = false;
        private Dictionary<string, string> prefixes = new Dictionary<string, string>();
        private string projectName = "";
        private string projectNumber = "";
        private string fileLocation = "";
        private string userLocation = "";
        private bool hokStandard = false;
        private string companyName = "Unknown";
        private string fileIdentifier = "";

        public Document Doc { get { return m_doc; } set { m_doc = value; } }
        public string DocCentralPath { get { return docCentralPath; } set { docCentralPath = value; } }
        public string DocTitle { get { return docTitle; } set { docTitle = value; } }
        public bool ValidCentral { get { return validCentral; } set { validCentral = value; } }
        public string ProjectName { get { return projectName; } set { projectName = value; } }
        public string ProjectNumber { get { return projectNumber; } set { projectNumber = value; } }
        public string FileLocation { get { return fileLocation; } set { fileLocation = value; } }
        public string UserLocation { get { return userLocation; } set { userLocation = value; } }
        public bool HOKStandard { get { return hokStandard; } set { hokStandard = value; } }
        public string CompanyName { get { return companyName; } set { companyName = value; } }
        public string FileIdentifier { get { return fileIdentifier; } set { fileIdentifier = value; } }

        public ModelInfo(Document doc)
        {
            m_doc = doc;
            docTitle = doc.Title;
            prefixes.Add("E-CAD", "E-CAD");
            prefixes.Add("E-BIM", "E-BIM");
            prefixes.Add("REVIT", "REVIT");

            docCentralPath = GetCentralPath();
            GetProjectInfo();
            GetFileLocation();
            GetUserLocation();

            if (string.IsNullOrEmpty(fileLocation) && userLocation=="UNKNOWN")
            {
                hokStandard = false;
                string regValue = RegistryUtil.GetRegistryKey("CompanyName");
                if (!string.IsNullOrEmpty(regValue)) { companyName = regValue; }
                fileIdentifier = FileIdentifierUtil.GetIdentifier(docCentralPath);
            }
            else
            {
                hokStandard = true;
            }
        }

        private string GetCentralPath()
        {
            string centralPath = "";
            try
            {
                if (m_doc.IsWorkshared)
                {
                    ModelPath modelPath = m_doc.GetWorksharingCentralModelPath();
                    centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        validCentral = true;
                    }
                    else
                    {
                        //detached
                        validCentral = false;
                        centralPath = m_doc.PathName;
                    }
                }
                else
                {
                    validCentral = false;
                    centralPath = m_doc.PathName;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return centralPath;
        }

        private void GetProjectInfo()
        {
            if (!string.IsNullOrEmpty(docCentralPath))
            {
                try
                {
                    string regPattern = @"\\([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\\";
                    Regex regex = new Regex(regPattern, RegexOptions.IgnoreCase);
                    Match match = regex.Match(docCentralPath);
                    if (match.Success)
                    {
                        projectNumber = match.Groups[1].Value;
                        projectName = match.Groups[2].Value;
                    }
                }
                catch { }
                if (!string.IsNullOrEmpty(projectNumber) && !string.IsNullOrEmpty(projectName))
                {
                    hokStandard = true;
                }
                else if(string.IsNullOrEmpty(projectNumber))
                {
                    projectName = GetProjectName(docCentralPath);
                    projectNumber = "00.00000.00";
                    hokStandard = false;
                }
            }
        }

        private string GetProjectName(string path)
        {
            string name = "";
            try
            {
                string[] paths = path.Split('\\');

                //Find E-BIM or E-CAD and get preceding values
                for (int i = 0; i < paths.Length; i++)
                {
                    if (prefixes.ContainsKey(paths[i]))
                    {
                        return paths[i - 1];
                    }
                }
            }
            catch
            {
                return "";
            }
            return name;
        }

        private void GetFileLocation()
        {
            try
            {
                Regex regServer = new Regex(@"^\\\\group\\hok\\(.+?(?=\\))|^\\\\(.{2,3})-\d{2}svr(\.group\.hok\.com)?\\", RegexOptions.IgnoreCase);
                Match regMatch = regServer.Match(docCentralPath);
                if (regMatch.Success)
                {
                    if (string.IsNullOrEmpty(regMatch.Groups[1].Value))
                    {
                        fileLocation = regMatch.Groups[2].Value;
                    }
                    else
                    {
                        fileLocation = regMatch.Groups[1].Value;
                    }
                }
            }
            catch
            {
            }
        }

        private void GetUserLocation()
        {
            try
            {
                ActiveDs.ADSystemInfo systemInfo = new ActiveDs.ADSystemInfo();
                string siteName = systemInfo.SiteName;

                if (!string.IsNullOrEmpty(siteName))
                {
                    userLocation = siteName;
                }
                else
                {
                    userLocation = "UNKNOWN";
                }
            }
            catch
            {
                userLocation = "UNKNOWN";
            }
        }

    }
    
}
