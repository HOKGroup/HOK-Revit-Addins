using Autodesk.Revit.DB;

namespace HOK.Core.Utilities
{
    public static class RevitDocument
    {
        public static string GetCentralPath(Document doc)
        {
            string docCentralPath;
            try
            {
                if (doc.IsWorkshared)
                {
                    var modelPath = doc.GetWorksharingCentralModelPath();
                    var centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    docCentralPath = !string.IsNullOrEmpty(centralPath) ? centralPath : doc.PathName;
                }
                else
                {
                    docCentralPath = doc.PathName;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                docCentralPath = doc.PathName;
            }

            return docCentralPath;
        }
    }
}
