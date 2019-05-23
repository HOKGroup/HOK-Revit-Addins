#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Properties;

#endregion

namespace HOK.MissionControl.Tools.Communicator.Tasks.FamilyTaskAssistant
{
    public class FamilyTaskAssistantModel
    {
        public string CentralPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="family"></param>
        /// <returns></returns>
        public ObservableCollection<CheckWrapper> CollectChecks(FamilyItem family)
        {
            var config = MissionControlSetup.Configurations.ContainsKey(CentralPath)
                ? MissionControlSetup.Configurations[CentralPath]
                : null;

            var familyNameCheck = new List<string> { "HOK_I", "HOK_M" }; //defaults
            if (config != null)
            {
                familyNameCheck = config.Updaters.First(x => string.Equals(x.UpdaterId,
                    Resources.HealthReportTrackerGuid, StringComparison.OrdinalIgnoreCase)).UserOverrides.FamilyNameCheck.Values;
            }

            var output = new ObservableCollection<CheckWrapper>
            {
                new CheckWrapper
                {
                    CheckName = $"Name: {family.Name}",
                    IsCheckPassing = familyNameCheck.Any(family.Name.Contains),
                    ToolTipText = "Check will fail if Family name does not contain " + string.Join(", ", familyNameCheck) + "."
                },
                new CheckWrapper
                {
                    CheckName = $"Size: {family.Size}",
                    IsCheckPassing = family.SizeValue < 1000000,
                    ToolTipText = "Check will fail if file size exceeds 1MB."
                },
                new CheckWrapper
                {
                    CheckName = $"Instances: {family.Instances}",
                    IsCheckPassing = family.Instances > 0,
                    ToolTipText = "Check will fail if Family has no placed instances."
                },
                new CheckWrapper
                {
                    CheckName = $"Voids: {family.VoidCount}",
                    IsCheckPassing = family.VoidCount < 5,
                    ToolTipText = "Check will fail if Family contains more than 5 Void Cuts."
                },
                new CheckWrapper
                {
                    CheckName = $"Arrays: {family.ArrayCount}",
                    IsCheckPassing = family.ArrayCount < 5,
                    ToolTipText = "Check will fail if Family contains more than 5 Arrays."
                },
                new CheckWrapper
                {
                    CheckName = $"Nested Families: {family.NestedFamilyCount}",
                    IsCheckPassing = family.NestedFamilyCount < 5,
                    ToolTipText = "Check will fail if Family contains more than 5 Nested Families."
                }
            };

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="family"></param>
        public void EditFamily(FamilyItem family)
        {
            AppCommand.CommunicatorHandler.FamilyItem = family;
            AppCommand.CommunicatorHandler.Request.Make(RequestId.EditFamily);
            AppCommand.CommunicatorEvent.Raise();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wrapper"></param>
        public void Submit(FamilyTaskWrapper wrapper)
        {
            var t = (FamilyTask)wrapper.Task;
            var e = (FamilyItem)wrapper.Element;

            AppCommand.CommunicatorHandler.FamilyItem = e;
            AppCommand.CommunicatorHandler.FamilyTask = t;
            AppCommand.CommunicatorHandler.Request.Make(RequestId.SubmitFamily);
            AppCommand.CommunicatorEvent.Raise();
        }
    }

    public class CheckWrapper
    {
        public string CheckName { get; set; }
        public bool IsCheckPassing { get; set; }
        public string ToolTipText { get; set; }
    }
}
