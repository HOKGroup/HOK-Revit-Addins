using Autodesk.Revit.DB;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.ViewAnalysis
{
    public class LinkedInstanceData
    {
        public RevitLinkInstance Instance { get; set; }
        public long InstanceId { get; set; }
        public Document LinkedDocument { get; set; }
        public string DocumentTitle { get; set; }
        public Transform TransformValue { get; set; }

        public LinkedInstanceData(RevitLinkInstance instance)
        {
            Instance = instance;
            InstanceId = GetElementIdValue(instance.Id);
            LinkedDocument = instance.GetLinkDocument();
            DocumentTitle = LinkedDocument.Title;
            TransformValue = instance.GetTotalTransform();
        }
    }
}
