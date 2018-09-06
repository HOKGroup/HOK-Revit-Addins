using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.GroupsManager.Utilities
{
    public class DocumentChanged
    {
        public ICollection<ElementId> Deleted { get; set; }
        public IEnumerable<ElementId> Added { get; set; }
        public Document Document { get; set; }

        public DocumentChanged()
        {
        }

        public DocumentChanged(ICollection<ElementId> deleted, IEnumerable<ElementId> added, Document doc)
        {
            Deleted = deleted;
            Added = added;
            Document = doc;
        }
    }

    public class GroupsDeleted
    {
        public List<GroupTypeWrapper> Groups { get; set; }
    }
}
