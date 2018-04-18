using System;
using System.Collections.ObjectModel;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.Communicator.Tasks.FamilyTaskAssistant
{
    public class FamilyTaskAssistantModel
    {
        public ObservableCollection<CheckWrapper> CollectChecks(FamilyItem family)
        {
            var output = new ObservableCollection<CheckWrapper>
            {
                new CheckWrapper
                {
                    CheckName = $"Name: {family.name}",
                    //TODO: This needs to implement the extra property stored on the settings file
                    IsCheckPassing = family.name.Contains("_HOK_I") || family.name.Contains("_HOK_M"),
                    ToolTipText = "Check will fail if Family name does not contain \"_HOK_I\" or \"_HOK_M\"."
                },
                new CheckWrapper
                {
                    CheckName = $"Size: {family.size}",
                    IsCheckPassing = family.sizeValue < 1000000,
                    ToolTipText = "Check will fail if file size exceeds 1MB."
                },
                new CheckWrapper
                {
                    CheckName = $"Instances: {family.instances}",
                    IsCheckPassing = family.instances > 0,
                    ToolTipText = "Check will fail if Family has no placed instances."
                },
                new CheckWrapper
                {
                    CheckName = $"Voids: {family.voidCount}",
                    IsCheckPassing = family.voidCount < 5,
                    ToolTipText = "Check will fail if Family contains more than 5 void cuts."
                },
                new CheckWrapper
                {
                    CheckName = $"Arrays: {family.arrayCount}",
                    IsCheckPassing = family.arrayCount < 5,
                    ToolTipText = "Check will fail if Family contains more than 5 arrays."
                },
                new CheckWrapper
                {
                    CheckName = $"Nested Families: {family.nestedFamilyCount}",
                    IsCheckPassing = family.nestedFamilyCount < 5,
                    ToolTipText = "Check will fail if Family contains more than 5 nested families."
                }
            };

            return output;
        }

        public void EditFamily(FamilyItem family)
        {
            AppCommand.CommunicatorHandler.FamilyItem = family;
            AppCommand.CommunicatorHandler.Request.Make(RequestId.EditFamily);
            AppCommand.CommunicatorEvent.Raise();
        }

        public void Submit(FamilyTaskWrapper wrapper)
        {
            var t = (FamilyTask) wrapper.Task;
            var e = (FamilyItem) wrapper.Element;

            var familyStatsId = AppCommand.HrData.familyStats;
            if (string.IsNullOrEmpty(familyStatsId)) return;

            t.completedOn = DateTime.UtcNow;
            t.completedBy = Environment.UserName.ToLower();

            ServerUtilities.Post<FamilyData>(wrapper.Task, "families/" + familyStatsId + "/family/" + e.name + "/updatetask/" + t.Id);
        }
    }

    public class CheckWrapper
    {
        public string CheckName { get; set; }
        public bool IsCheckPassing { get; set; }
        public string ToolTipText { get; set; }
    }
}
