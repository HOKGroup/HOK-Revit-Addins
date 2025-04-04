using System;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Groups;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class GroupMonitor
    {
        /// <summary>
        /// Publishes information about groups in the model.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="groupsId">Id of the Groups Document in MongoDB.</param>
        public void PublishData(Document doc, string groupsId)
        {
            try
            {
                var gTypes = new FilteredElementCollector(doc)
                    .OfClass(typeof(GroupType))
                    .Cast<GroupType>()
                    .ToDictionary(
                        x => x.Id,
                        x =>
                            new GroupItem
                            {
                                Name = x.Name,
                                // (Konrad) If there is a Detail Group attached to Model Group
                                // it will have the same name as Model Group but different Category.
                                Type =
                                    x.Category.Name == "Attached Detail Groups"
                                        ? "Attached Detail Group"
                                        : x.FamilyName
                            }
                    );

                foreach (
                    var gi in new FilteredElementCollector(doc).OfClass(typeof(Group)).Cast<Group>()
                )
                {
                    var gTypeId = gi.GroupType.Id;
                    if (!gTypes.ContainsKey(gTypeId))
                        continue;

                    var gType = gTypes[gTypeId];
                    gType.Instances.Add(new GroupInstanceItem(gi));
                    gType.MemberCount = gi.GetMemberIds().Count;
                }

                var groupStats = new GroupDataItem { Groups = gTypes.Values.ToList() };

                if (
                    !ServerUtilities.Post(
                        groupStats,
                        "groups/" + groupsId + "/groupstats",
                        out object unused
                    )
                )
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Groups Data.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
