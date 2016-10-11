using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.AddIn.Utils
{
    public static class RevitUtil
    {
        public static string GetCentralFilePath(Document doc)
        {
            string centralPath = doc.PathName;
            try
            {
                if (doc.IsWorkshared)
                {
                    if (doc.IsDetached) { return doc.Title; }
                    else
                    {
                        ModelPath centralModelPath = doc.GetWorksharingCentralModelPath();
                        if (null != centralModelPath)
                        {
                            string userVisibleCentralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(centralModelPath);
                            if (!string.IsNullOrEmpty(userVisibleCentralPath))
                            {
                                centralPath = userVisibleCentralPath;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return centralPath;
        }
    }
}
