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
        private Document Doc { get; }

        public GroupsManagerModel(Document doc)
        {
            Doc = doc;
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
