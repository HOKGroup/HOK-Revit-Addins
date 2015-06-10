using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace HOK.FileOnpeningMonitor
{
    public class CentralFileInfo
    {
        private Document m_doc = null;
        private string docCentralPath = "";
        private string userName = "";
        private string userLocation = "";
        private string ipAddress = "";
        private string fileLocation = "";
        private string projectName = "";
        private string projectNumber = "";
        private DateTime startTime = DateTime.Now;
        private DateTime endTime = DateTime.Now;

        private List<string> prefixes = new List<string>();

        public Document Doc { get { return m_doc; } set { m_doc = value; } }
        public string DocCentralPath { get { return docCentralPath; } set { docCentralPath = value; } }
        public string UserName { get { return userName; } set { userName = value; } }
        public string UserLocation { get { return userLocation; } set { userLocation = value; } }
        public string IPAddress { get { return ipAddress; } set { ipAddress = value; } }
        public string FileLocation { get { return fileLocation; } set { fileLocation = value; } }
        public string ProjectName { get { return projectName; } set { projectName = value; } }
        public string ProjectNumber { get { return projectNumber; } set { projectNumber = value; } }
        public DateTime StartTime { get { return startTime; } set { startTime = value; } }
        public DateTime EndTime { get { return endTime; } set { endTime = value; } }

        public CentralFileInfo(Document doc)
        {
            m_doc = doc;
            docCentralPath = doc.PathName;
            prefixes.Add("E-CAD");
            prefixes.Add("E-BIM");
            prefixes.Add("REVIT");

            GetProjectInfo();
            GetUserInfo();
        }

        private void GetProjectInfo()
        {
            try
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
                    catch
                    {
                    }

                    if (string.IsNullOrEmpty(projectNumber))
                    {
                        string[] paths = docCentralPath.Split('\\');

                        //Find E-BIM or E-CAD and get preceding values
                        for (int i = 0; i < paths.Length; i++)
                        {
                            if (prefixes.Contains(paths[i]))
                            {
                                projectName = paths[i - 1]; break;
                            }
                        }
                        projectNumber = "00.00000.00";
                    }

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

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void GetUserInfo()
        {
            try
            {
                try
                {
                    userName = System.Environment.UserName;
                }
                catch
                {
                    userName = "";
                }

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
            catch (Exception ex)
            {
                string messag = ex.Message;
                userLocation = "UNKNOWN";
            }

            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = hostEntry.AddressList[1].ToString();
            }
            catch
            {
                ipAddress = "UNKNOWN";
            }
        }
    }
}
