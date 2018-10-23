using System;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Utils
{
    public static class FileInfoUtil
    {
        /// <summary>
        /// Returns model's Central File path.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <returns>Central File Path.</returns>
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return centralPath.ToLower();
        }

        /// <summary>
        /// Retrieves office name from machine name ex. NY
        /// </summary>
        /// <returns>Office name.</returns>
        public static string GetOffice()
        {
            try
            {
                var machineName = Environment.MachineName;
                var splits = machineName.Split('-');
                if (!splits.Any()) return string.Empty;

                var s = splits.FirstOrDefault();
                if (s == null) return string.Empty;

                var office = s.ToUpper();
                return office;
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                return string.Empty;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="centralPath"></param>
        ///// <returns></returns>
        //public static Project GetProjectInfo(string centralPath)
        //{
        //    var projectInfo = new Project();
        //    try
        //    {
        //        const string regPattern = @"\\([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\\";
        //        var regex = new Regex(regPattern, RegexOptions.IgnoreCase);
        //        var match = regex.Match(centralPath);
        //        if (match.Success)
        //        {
        //            projectInfo.Number = match.Groups[1].Value;
        //            projectInfo.Name = match.Groups[2].Value;
        //        }

        //        if (string.IsNullOrEmpty(projectInfo.Number))
        //        {
        //            projectInfo.Number = "00.00000.00";
        //            projectInfo.Name = GetProjectName(centralPath);
        //        }

        //        projectInfo.Office = GetFileLocation(centralPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
        //    }
        //    return projectInfo;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //private static string GetProjectName(string path)
        //{
        //    var prefixes = new[] { "E-CAD", "E-BIM", "REVIT" };
        //    const string name = "UNKNOWN";
        //    try
        //    {
        //        var paths = path.Split('\\');

        //        //Find E-BIM or E-CAD and get preceding values
        //        for (var i = 0; i < paths.Length; i++)
        //        {
        //            if (prefixes.Contains(paths[i]))
        //            {
        //                return paths[i - 1];
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
        //        return "";
        //    }
        //    return name;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //private static string GetFileLocation(string path)
        //{
        //    var fileLocation = "";
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(path))
        //        {
        //            var regServer = new Regex(@"^\\\\group\\hok\\(.+?(?=\\))|^\\\\(.{2,3})-\d{2}svr(\.group\.hok\.com)?\\", RegexOptions.IgnoreCase);
        //            var regMatch = regServer.Match(path);
        //            if (regMatch.Success)
        //            {
        //                fileLocation = string.IsNullOrEmpty(regMatch.Groups[1].Value) ? regMatch.Groups[2].Value : regMatch.Groups[1].Value;
        //            }
        //        }
        //    }
        //    catch(Exception ex) 
        //    {
        //        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
        //    }
        //    return fileLocation;
        //}
    }
}
