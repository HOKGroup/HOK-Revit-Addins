#region References

using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.MissionControl.GroupsManager.Utilities;

#endregion

namespace HOK.MissionControl.GroupsManager
{
    public class GroupsManagerModel
    {
        public Document Doc { get; set; }

        public GroupsManagerModel(Document doc)
        {
            Doc = doc;
        }

        /// <summary>
        /// Handles processing of Groups being added or removed. This can be done here instead of a External Command Handler
        /// because the only thing that we are updating is the UI so we don't need to be in Revit context.
        /// </summary>
        /// <param name="msg">Document Changed Message object.</param>
        /// <param name="groups">Groups stored on the UI.</param>
        public void ProcessDocumentChanged(DocumentChanged msg, ObservableCollection<GroupTypeWrapper> groups)
        {
            if (msg.Deleted.Any())
            {
                // (Konrad) Handle GroupType being deleted
                groups.RemoveAll(x => msg.Deleted.Contains(x.Id));

                // (Konrad) Handle instances of groups being deleted
                foreach (var gtw in groups)
                {
                    gtw.Instances.RemoveAll(x => msg.Deleted.Contains(x));
                }
            }

            // (Konrad) Handle new instances of GroupType being added
            var addedTypes = msg.Added.Intersect(new FilteredElementCollector(Doc).OfClass(typeof(GroupType)).ToElementIds()).ToList();
            if (addedTypes.Any())
            {
                foreach (var id in addedTypes)
                {
                    if (!(Doc.GetElement(id) is GroupType gt)) continue;

                    var gtw = new GroupTypeWrapper(gt);
                    groups.Add(gtw);
                }
            }

            // (Konrad) Handle new instances of groups being added
            var added = msg.Added.Intersect(new FilteredElementCollector(Doc).OfClass(typeof(Group)).ToElementIds()).ToList();
            if (!added.Any()) return;

            foreach (var id in added)
            {
                if (!(Doc.GetElement(id) is Group g)) continue;

                var existing = groups.FirstOrDefault(x => x.Id == g.GroupType.Id);
                existing?.Instances.Add(id);
            }
        }

        /// <summary>
        /// Retrieves all Group Types from the project.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<GroupTypeWrapper> CollectGroups()
        {
            var gTypes = new FilteredElementCollector(Doc)
                .OfClass(typeof(GroupType))
                .Cast<GroupType>()
                .ToDictionary(x => x.Id, x => new GroupTypeWrapper(x));

            foreach (var gi in new FilteredElementCollector(Doc).OfClass(typeof(Group)).Cast<Group>())
            {
                var gTypeId = gi.GroupType.Id;
                if (!gTypes.ContainsKey(gTypeId)) continue;

                var gType = gTypes[gTypeId];
                gType.MemberCount = gi.GetMemberIds().Count;
                gType.Instances.Add(gi.Id);
            }

            return new ObservableCollection<GroupTypeWrapper>(gTypes.Values.ToList().OrderBy(x => x.Name));
        }
    }
}
