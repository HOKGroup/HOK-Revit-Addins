using Autodesk.Revit.DB;

namespace HOK.MissionControl.LinksManager
{
    public class LinksManagerModel
    {
        public readonly Document _doc;

        public LinksManagerModel(Document doc)
        {
            _doc = doc;
        }
    }
}
