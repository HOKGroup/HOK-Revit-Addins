using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.FileOnpeningMonitor
{
    public class CentralFileInfo
    {
        private readonly List<string> prefixes = new List<string>();
        public Document Doc { get; set; }
        public string DocCentralPath { get; set; }
        public string UserName { get; set; } = "";
        public string UserLocation { get; set; } = "";
        public string IPAddress { get; set; } = "";
        public string FileLocation { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public string ProjectNumber { get; set; } = "";
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;

        public CentralFileInfo(Document doc)
        {
            Doc = doc;
            DocCentralPath = doc.PathName;
            prefixes.Add("E-CAD");
            prefixes.Add("E-BIM");
            prefixes.Add("REVIT");

            GetProjectInfo();
            GetUserInfo();
        }

        private static string GetReportingIp()
        {
            try
            {
                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || x.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                    .Where(ni => (ni.Description.StartsWith("PANGP") || !ni.Description.Contains("Virtual")) &&  !ni.Description.Contains("VMware"))
                    .ToArray();

                foreach (NetworkInterface ni in nis)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return "UNKNOWN";
        }

        private void GetProjectInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(DocCentralPath)) return;

                try
                {
                    const string regPattern = @"\\([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\\";
                    var regex = new Regex(regPattern, RegexOptions.IgnoreCase);
                    var match = regex.Match(DocCentralPath);
                    if (match.Success)
                    {
                        ProjectNumber = match.Groups[1].Value;
                        ProjectName = match.Groups[2].Value;
                    }
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }

                if (string.IsNullOrEmpty(ProjectNumber))
                {
                    var paths = DocCentralPath.Split('\\');

                    //Find E-BIM or E-CAD and get preceding values
                    for (var i = 0; i < paths.Length; i++)
                    {
                        if (!prefixes.Contains(paths[i])) continue;

                        ProjectName = paths[i - 1]; break;
                    }
                    ProjectNumber = "00.00000.00";
                }

                var regServer = new Regex(@"^\\\\group\\hok\\(.+?(?=\\))|^\\\\(.{2,3})-\d{2}svr(\.group\.hok\.com)?\\", RegexOptions.IgnoreCase);
                var regMatch = regServer.Match(DocCentralPath);
                if (regMatch.Success)
                {
                    FileLocation = string.IsNullOrEmpty(regMatch.Groups[1].Value) ? regMatch.Groups[2].Value : regMatch.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void GetUserInfo()
        {
            try
            {
                try
                {
                    UserName = Environment.UserName;
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                    UserName = "";
                }

                var systemInfo = new ActiveDs.ADSystemInfo();
                var siteName = systemInfo.SiteName;

                UserLocation = !string.IsNullOrEmpty(siteName) ? siteName : "UNKNOWN";
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                UserLocation = "UNKNOWN";
            }

            IPAddress = GetReportingIp();
        }
    }
}
