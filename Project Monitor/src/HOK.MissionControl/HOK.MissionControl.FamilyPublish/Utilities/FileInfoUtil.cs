using System;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.MissionControl.FamilyPublish.Utilities
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return centralPath;
        }
    }
}
