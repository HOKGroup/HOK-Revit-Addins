using Autodesk.Revit.DB;

namespace HOK.ViewAnalysis
{
    public class LinkedInstanceData
    {
        public RevitLinkInstance Instance { get; set; }
        public int InstanceId { get; set; }
        public Document LinkedDocument { get; set; }
        public string DocumentTitle { get; set; }
        public Transform TransformValue { get; set; }

        public LinkedInstanceData(RevitLinkInstance instance)
        {
            Instance = instance;
            InstanceId = instance.Id.IntegerValue;
            LinkedDocument = instance.GetLinkDocument();
            DocumentTitle = LinkedDocument.Title;
            TransformValue = instance.GetTotalTransform();
        }
    }
}
