using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;



namespace HOK.ModelReporting
{
    public class EventSettings
    {
        private Document m_doc;
        private Dictionary<string, string> prefixes = new Dictionary<string, string>();
        private string docCentralPath = "";
        private string docLocalPath = "";
        private string projectName = "";
        private string projectNumber = "";
        private double projectLatitude = 0;
        private double projectLongititude = 0;
        private string fileLocation = "";
        private string localFileLocation = "";
        private string version = "";
        private string versionNumber = "";
        private bool validCentral = false;
        private bool openDetached = false;
        private bool openCentral = false;
        private long sizeStart = 0;
        private long sizeEnd = 0;
        private DateTime startTime = DateTime.Now;
        private DateTime endTime = DateTime.Now;
        private string userLocation = "";
        private string ipAddress = "";
        private bool isRecordable = true;

        public Document Doc { get { return m_doc; } set { m_doc = value; } }
        public string DocCentralPath { get { return docCentralPath; } set { docCentralPath = value; } }
        public string DocLocalPath { get { return docLocalPath; } set { docLocalPath = value; } }
        public string ProjectName { get { return projectName; } set { projectName = value; } }
        public string ProjectNumber { get { return projectNumber; } set { projectNumber = value; } }
        public double ProjectLatitude { get { return projectLatitude; } set { projectLatitude = value; } }
        public double ProjectLongitude { get { return projectLongititude; } set { projectLongititude = value; } }
        public string FileLocation { get { return fileLocation; } set { fileLocation = value; } }
        public string LocalFileLocation { get { return localFileLocation; } set { localFileLocation = value; } }
        public string Version { get { return version; } set { version = value; } }
        public string VersionNumber { get { return versionNumber; } set { versionNumber = value; } }
        public bool ValidCentral { get { return validCentral; } set { validCentral = value; } }
        public bool OpenDetached { get { return openDetached; } set { openDetached = value; } }
        public bool OpenCentral { get { return openCentral; } set { openCentral = value; } }
        public long SizeStart { get { return sizeStart; } set { sizeStart = value; } }
        public long SizeEnd { get { return sizeEnd; } set { sizeEnd = value; } }
        public DateTime StartTime { get { return startTime; } set { startTime = value; } }
        public DateTime EndTime { get { return endTime; } set { endTime = value; } }
        public string UserLocation { get { return userLocation; } set { userLocation = value; } }
        public string IPAddress { get { return ipAddress; } set { ipAddress = value; } }
        public bool IsRecordable { get { return isRecordable; } set { isRecordable = value; } }

        public EventSettings(Document doc)
        {
            m_doc = doc;
            GetCentralPath();
            DoSetup();
            GetIPAddress();
            GetUserLocation();
        }

        private void GetCentralPath()
        {
            try
            {
                if (m_doc.IsWorkshared)
                {
                    ModelPath modelPath = m_doc.GetWorksharingCentralModelPath();
                    string centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        validCentral = true;
                        openDetached = false;
                        docCentralPath = centralPath;
                    }
                    else
                    {
                        //detached
                        validCentral = false;
                        openDetached = true;
                        docCentralPath = m_doc.PathName;
                    }
                }
                else
                {
                    validCentral = false;
                    openDetached = false;
                    docCentralPath = m_doc.PathName;
                }
                docLocalPath = m_doc.PathName;

            }
            catch (Exception ex)
            {
                string message = ex.Message;
                validCentral = false;
                docCentralPath = m_doc.PathName;
                docLocalPath = m_doc.PathName;
            }
            finally
            {
                if (validCentral && (docCentralPath == docLocalPath))
                {
                    openCentral = true;
                }
            }
        }

        private void DoSetup()
        {
            prefixes.Add("E-CAD", "E-CAD");
            prefixes.Add("E-BIM", "E-BIM");
            prefixes.Add("REVIT", "REVIT");

            //Get ProjectName and ProjectNumber
            if (!string.IsNullOrEmpty(docCentralPath))
            {
                try
                {
                    //string regPattern = @"\\([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\\";
                    string regPattern = @"[\\|/](\d{2}\.\d{5}\.\d{2})\s+(.+?(?=[\\|/]))";
                    Regex regex = new Regex(regPattern, RegexOptions.IgnoreCase);
                    Match match = regex.Match(docCentralPath);
                    if (match.Success)
                    {
                        projectNumber = match.Groups[1].Value;
                        projectName = match.Groups[2].Value;
                    }
                }
                catch { }

                if(string.IsNullOrEmpty(projectNumber))
                {
                    projectName=GetProjectName(docCentralPath);
                    projectNumber = "00.00000.00";

                    if (docCentralPath.StartsWith(@"\\GROUP\HOK") || docCentralPath.StartsWith("RSN:") || docCentralPath.StartsWith("A360:"))
                    {
                        isRecordable = true;
                    }
                    else
                    {
                        isRecordable = false;
                    }
                }
            }

            //Get Project Location: Longitude, Latitude
            try
            {
                ProjectLocation projectLocation = m_doc.ActiveProjectLocation;
                SiteLocation site = projectLocation.SiteLocation;
                const double angleRatio = Math.PI / 180;

                projectLatitude = site.Latitude / angleRatio;
                projectLongititude = site.Longitude / angleRatio;
            }
            catch { }

            //Get File Location
            try
            {
                fileLocation = GetFileLocation(docCentralPath);
                localFileLocation = GetFileLocation(docLocalPath);
            }
            catch { }

            //Get Version Number
            try
            {
                versionNumber = m_doc.Application.VersionNumber;
            }
            catch { }
           
        }

        private string GetFileLocation(string path)
        {
            string fileLocation = "";
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    //Regex regServer = new Regex(@"^\\\\group\\hok\\(.+?(?=\\))|^\\\\(.{2,3})-\d{2}svr(\.group\.hok\.com)?\\", RegexOptions.IgnoreCase);
                    Regex regServer = new Regex(@"^\\\\group\\hok\\(.+?(?=\\))|[\\|/](\w{2,3})-\d{2}svr(\.group\.hok\.com)?[\\|/]|^[rR]:\\(\w{2,3})\\", RegexOptions.IgnoreCase);
                    Match regMatch = regServer.Match(path);
                    if (regMatch.Success)
                    {
                        if(!string.IsNullOrEmpty(regMatch.Groups[4].ToString()))
                        {
                            fileLocation = regMatch.Groups[4].Value;
                        }
                        else if (!string.IsNullOrEmpty(regMatch.Groups[2].ToString()))
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
            catch { }
            return fileLocation;
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
                    if(prefixes.ContainsKey(paths[i]))
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

        private void GetProjectInfoOnRevitServer(string path, out string pNumber, out string pName)
        {
            pNumber = "00.00000.00";
            pName = "";
            try
            {

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public long GetFileSize()
        {
            long fileSize = 0;
            try
            {
                if (docCentralPath.StartsWith("C:")) { return fileSize; }
                if (!string.IsNullOrEmpty(docCentralPath))
                {
                    FileInfo fileInfo = new FileInfo(docCentralPath);
                    fileSize = fileInfo.Length / 1024;
                }
                
            }
            catch
            {
                fileSize = 0;
            }
            return fileSize;
        }

        private void GetIPAddress()
        {
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
