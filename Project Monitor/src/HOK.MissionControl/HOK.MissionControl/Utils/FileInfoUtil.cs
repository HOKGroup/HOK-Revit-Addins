using System;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Classes;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Utils
{
    public static class FileInfoUtil
    {
        /// <summary>
        /// Returns model's Central File path.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <returns></returns>
        public static string GetCentralFilePath(Document doc)
        {
            var centralPath = "";
            try
            {
                var centralModelPath = doc.GetWorksharingCentralModelPath();
                if (null != centralModelPath)
                {
                    var userVisiblePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(centralModelPath);
                    if (!string.IsNullOrEmpty(userVisiblePath))
                    {
                        centralPath = userVisiblePath;
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("FileInfoUtil-GetCentralFilePath:" + ex.Message);
            }
            return centralPath;
        }

        public static Project GetProjectInfo(string centralPath)
        {
            var projectInfo = new Project();
            try
            {
                var regPattern = @"\\([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\\";
                var regex = new Regex(regPattern, RegexOptions.IgnoreCase);
                var match = regex.Match(centralPath);
                if (match.Success)
                {
                    projectInfo.number = match.Groups[1].Value;
                    projectInfo.name = match.Groups[2].Value;
                }

                if (string.IsNullOrEmpty(projectInfo.number))
                {
                    projectInfo.number = "00.00000.00";
                    projectInfo.name = GetProjectName(centralPath);
                }

                projectInfo.office = GetFileLocation(centralPath);
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("FileInfoUtil-GetProjectInfo:" + ex.Message);
            }
            return projectInfo;
        }

        private static string GetProjectName(string path)
        {
            var prefixes = new string[] { "E-CAD", "E-BIM", "REVIT" };
            var name = "UNKNOWN";
            try
            {
                var paths = path.Split('\\');

                //Find E-BIM or E-CAD and get preceding values
                for (var i = 0; i < paths.Length; i++)
                {
                    if (prefixes.Contains(paths[i]))
                    {
                        return paths[i - 1];
                    }
                }
            }
            catch(Exception ex)
            {
                LogUtil.AppendLog("FileInfoUtil-GetProjectName:" + ex.Message);
                return "";
            }
            return name;
        }

        private static string GetFileLocation(string path)
        {
            var fileLocation = "";
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var regServer = new Regex(@"^\\\\group\\hok\\(.+?(?=\\))|^\\\\(.{2,3})-\d{2}svr(\.group\.hok\.com)?\\", RegexOptions.IgnoreCase);
                    var regMatch = regServer.Match(path);
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
            catch(Exception ex) 
            {
                LogUtil.AppendLog("FileInfoUtil-GetFileLocation:" + ex.Message);
            }
            return fileLocation;
        }
    }
}
