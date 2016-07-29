using Autodesk.Revit.DB;
using HOK.ElementWatcher.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HOK.ElementWatcher.Utils
{
    public static class FileInfoUtil
    {
        public static string GetCentralFilePath(Document doc)
        {
            string centralPath = "";
            try
            {
                if (!doc.IsWorkshared) { centralPath = doc.PathName; return centralPath; }
                ModelPath centralModelPath = doc.GetWorksharingCentralModelPath();
                if (null != centralModelPath)
                {
                    string userVisiblePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(centralModelPath);
                    if (!string.IsNullOrEmpty(userVisiblePath))
                    {
                        centralPath = userVisiblePath;
                    }
                    else
                    {
                        centralPath = doc.PathName;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return centralPath;
        }

        public static Project GetProjectInfo(string centralPath)
        {
            Project projectInfo = new Project();
            try
            {
                projectInfo._id = Guid.NewGuid().ToString();

                string regPattern = @"\\([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\\";
                Regex regex = new Regex(regPattern, RegexOptions.IgnoreCase);
                Match match = regex.Match(centralPath);
                if (match.Success)
                {
                    projectInfo.ProjectNumber = match.Groups[1].Value;
                    projectInfo.ProjectName = match.Groups[2].Value;
                }

                if (string.IsNullOrEmpty(projectInfo.ProjectNumber))
                {
                    projectInfo.ProjectNumber = "00.00000.00";
                    projectInfo.ProjectName = GetProjectName(centralPath);
                }

                projectInfo.Office = GetFileLocation(centralPath);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return projectInfo;
        }

        private static string GetProjectName(string path)
        {
            string[] prefixes = new string[] { "E-CAD", "E-BIM", "REVIT" };
            string name = "UNKNOWN";
            try
            {
                string[] paths = path.Split('\\');
               
                //Find E-BIM or E-CAD and get preceding values
                for (int i = 0; i < paths.Length; i++)
                {
                    if (prefixes.Contains(paths[i]))
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

        private static string GetFileLocation(string path)
        {
            string fileLocation = "";
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Regex regServer = new Regex(@"^\\\\group\\hok\\(.+?(?=\\))|^\\\\(.{2,3})-\d{2}svr(\.group\.hok\.com)?\\", RegexOptions.IgnoreCase);
                    Match regMatch = regServer.Match(path);
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
            catch { }
            return fileLocation;
        }
    }
}
