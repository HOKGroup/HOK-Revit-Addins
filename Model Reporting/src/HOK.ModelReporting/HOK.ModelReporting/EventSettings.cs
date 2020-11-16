using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace HOK.ModelReporting
{
    public class EventSettings
    {
        private readonly Dictionary<string, string> _prefixes = new Dictionary<string, string>();
        public Document Doc { get; set; }
        public string DocCentralPath { get; set; } = "";
        public string DocLocalPath { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public string ProjectNumber { get; set; } = "";
        public double ProjectLatitude { get; set; }
        public double ProjectLongitude { get; set; }
        public string FileLocation { get; set; } = "";
        public string LocalFileLocation { get; set; } = "";
        public string Version { get; set; } = "";
        public string VersionNumber { get; set; } = "";
        public bool ValidCentral { get; set; }
        public bool OpenDetached { get; set; }
        public bool OpenCentral { get; set; }
        public long SizeStart { get; set; } = 0;
        public long SizeEnd { get; set; } = 0;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;
        public string UserLocation { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public bool IsRecordable { get; set; } = true;

        public EventSettings(Document doc)
        {
            Doc = doc;
            GetCentralPath();
            DoSetup();
            GetIpAddress();
            GetUserLocation();
        }

        private void GetCentralPath()
        {
            try
            {
                if (Doc.IsWorkshared)
                {
                    var modelPath = Doc.GetWorksharingCentralModelPath();
                    var centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        ValidCentral = true;
                        OpenDetached = false;
                        DocCentralPath = centralPath;
                    }
                    else
                    {
                        //detached
                        ValidCentral = false;
                        OpenDetached = true;
                        DocCentralPath = Doc.PathName;
                    }
                }
                else
                {
                    ValidCentral = false;
                    OpenDetached = false;
                    DocCentralPath = Doc.PathName;
                }
                DocLocalPath = Doc.PathName;

            }
            catch (Exception)
            {
                ValidCentral = false;
                DocCentralPath = Doc.PathName;
                DocLocalPath = Doc.PathName;
            }
            finally
            {
                if (ValidCentral && (DocCentralPath == DocLocalPath))
                {
                    OpenCentral = true;
                }
            }
        }

        private void DoSetup()
        {
            _prefixes.Add("E-CAD", "E-CAD");
            _prefixes.Add("E-BIM", "E-BIM");
            _prefixes.Add("REVIT", "REVIT");
            _prefixes.Add("E-Design", "E-Design");

            // Get ProjectName and ProjectNumber
            if (!string.IsNullOrEmpty(DocCentralPath))
            {
                try
                {
                    const string regPattern = @"[\\|/](\d{2}\.\d{5}\.\d{2})\s+(.+?(?=[\\|/]))";
                    var regex = new Regex(regPattern, RegexOptions.IgnoreCase);
                    var match = regex.Match(DocCentralPath);
                    if (match.Success)
                    {
                        ProjectNumber = match.Groups[1].Value;
                        ProjectName = match.Groups[2].Value;
                    }
                }
                catch
                {
                    // ignored
                }

                if(string.IsNullOrEmpty(ProjectNumber))
                {
                    ProjectName = GetProjectName(DocCentralPath);
                    const string projectNumberRegPattern = @"\d{2}\.\d{5}\.\d{2}";
                    var regex = new Regex(projectNumberRegPattern);
                    var match = regex.Match(Doc.ProjectInformation.Number);
                    if (match.Success)
                    {
                        ProjectNumber = match.Value; 
                    }
                    else
                    {
                        ProjectNumber = "00.00000.00";
                    }

                    if (DocCentralPath.StartsWith(@"\\GROUP\HOK", StringComparison.OrdinalIgnoreCase) ||
                        DocCentralPath.StartsWith("RSN://", StringComparison.OrdinalIgnoreCase) ||
                        DocCentralPath.StartsWith("BIM 360://", StringComparison.OrdinalIgnoreCase))
                    {
                        IsRecordable = true;
                    }
                    else
                    {
                        IsRecordable = false;
                    }
                }
            }

            //Get Project Location: Longitude, Latitude
            try
            {
                var projectLocation = Doc.ActiveProjectLocation;
#if RELEASE2015 || RELEASE2016 || RELEASE2017 || RELEASE2018
                var site = projectLocation.SiteLocation;
#else
                var site = projectLocation.GetSiteLocation();
#endif
                const double angleRatio = Math.PI / 180;
                ProjectLatitude = site.Latitude / angleRatio;
                ProjectLongitude = site.Longitude / angleRatio;
            }
            catch
            {
                // ignored
            }

            //Get File Location
            try
            {
                FileLocation = GetFileLocation(DocCentralPath);
                LocalFileLocation = GetFileLocation(DocLocalPath);
            }
            catch
            {
                // ignored
            }

            //Get Version Number
            try
            {
                VersionNumber = Doc.Application.VersionNumber;
            }
            catch
            {
                // ignored
            }
           
        }

        private static string GetFileLocation(string path)
        {
            var fileLocation = "";
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var regServer =
                        new Regex(
                            @"^\\\\group\\hok\\(.+?(?=\\))|[\\|/](\w{2,3})-\d{2}svr(\.group\.hok\.com)?[\\|/]|^[rR]:\\(\w{2,3})\\",
                            RegexOptions.IgnoreCase);
                    var regMatch = regServer.Match(path);
                    if (regMatch.Success)
                    {
                        if (!string.IsNullOrEmpty(regMatch.Groups[4].ToString()))
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
            catch
            {
                // ignored
            }
            return fileLocation;
        }

        private string GetProjectName(string path)
        {
            const string name = "";
            try
            {
                var paths = path.Split('\\');

                //Find E-BIM or E-CAD and get preceding values
                for (var i = 0; i < paths.Length; i++)
                {
                    if(_prefixes.ContainsKey(paths[i]))
                    {
                        return paths[i - 1];
                    }
                }
            }
            catch
            {
                // ignored
            }
            return name;
        }

        public long GetFileSize()
        {
            long fileSize = 0;
            try
            {
                if (DocCentralPath.StartsWith("C:", StringComparison.OrdinalIgnoreCase)) { return fileSize; }

                if (!string.IsNullOrEmpty(DocLocalPath))
                {
                    var fileInfo = new FileInfo(DocLocalPath);
                    fileSize = fileInfo.Length / 1024;
                }
            }
            catch
            {
                fileSize = 0;
            }
            return fileSize;
        }

        private void GetIpAddress()
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IpAddress = hostEntry.AddressList[1].ToString();
            }
            catch
            {
                IpAddress = "UNKNOWN";
            }
        }

        private void GetUserLocation()
        {
            try
            {
                var systemInfo = new ActiveDs.ADSystemInfo();
                var siteName = systemInfo.SiteName;
                UserLocation = !string.IsNullOrEmpty(siteName) ? siteName : "UNKNOWN";
                if (UserLocation == "VPN")
                {
                    if (IpAddress == "")
                    {
                        GetIpAddress();
                    }
                    if (IpAddress.StartsWith("172.30.56"))
                    {
                        UserLocation = "LON";
                    }
                }
            }
            catch
            {
                UserLocation = "UNKNOWN";
            }
        }
    }
}
