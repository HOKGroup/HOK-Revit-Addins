using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator.Tasks.CheckControl;
using HOK.MissionControl.Tools.Communicator.Tasks.TaskControl;

namespace HOK.MissionControl.Tools.Communicator.Tasks.TaskAssistant
{
    public class TaskAssistantModel
    {
        public FamilyItem Family { get; set; }
        public FamilyTask Task { get; set; }

        public TaskAssistantModel(FamilyItem family, FamilyTask task)
        {
            Family = family;
            Task = task;
        }

        public void SubmitEdits()
        {
            var familyStatsId = AppCommand.HrData.familyStats;
            if (string.IsNullOrEmpty(familyStatsId)) return;

            //var families = AppCommand.FamiliesToWatch;

            //// TODO: This is disgusting! I need to figure out a way to make a single call to MongoDB.
            //foreach (var family in families.Values)
            //{
            //    ServerUtilities.UpdateField(new {key = family}, "families/" + familyStatsId + "/updateone");
            //}

            if (AppCommand.FamiliesToWatch.ContainsKey(Family.elementId))
            {
                ServerUtilities.UpdateField(new { key = AppCommand.FamiliesToWatch[Family.elementId] }, "families/" + familyStatsId + "/updateone");
            }

            Task.completedOn = DateTime.Now;
            Task.completedBy = Environment.UserName.ToLower();

            ServerUtilities.Post<FamilyStat>(Task, "families/" + familyStatsId + "/name/" + Family.name + "/updatetask/" + Task.Id);

            var info = new TaskUpdatedMessage
            {
                Task = Task
            };
            Messenger.Default.Send(info);

            //var updatedFamily = updated.families.FirstOrDefault(x => x.elementId == Family.elementId);
            //if (updatedFamily != null)
            //{
            //    var updatedTask = updatedFamily.tasks.FirstOrDefault(x => x.Id == Task.Id);
            //    var info = new TaskUpdatedMessage
            //    {
            //        Task = updatedTask
            //    };
            //    Messenger.Default.Send(info);
            //}
        }

        public ObservableCollection<FamilyCheckViewModel> ProcessChecks()
        {
            var output = new ObservableCollection<FamilyCheckViewModel>
            {
                new FamilyCheckViewModel(Family)
                {
                    CheckName = $"Name: {Family.name}",
                    IsCheckPassing = Family.name.Contains("_HOK_I") || Family.name.Contains("_HOK_M"),
                    CheckType = FamilyCheckTypes.Name,
                    IsCheckVerified = Family.isNameVerified
                },
                new FamilyCheckViewModel(Family)
                {
                    CheckName = $"Size: {Family.size}",
                    IsCheckPassing = Family.sizeValue < 1000000,
                    CheckType = FamilyCheckTypes.Size,
                    IsCheckVerified = Family.isSizeVerified
                },
                new FamilyCheckViewModel(Family)
                {
                    CheckName = $"Instances: {Family.instances}",
                    IsCheckPassing = Family.instances > 0,
                    CheckType = FamilyCheckTypes.Instances,
                    IsCheckVerified = Family.isInstancesVerified
                },
                new FamilyCheckViewModel(Family)
                {
                    CheckName = $"Voids: {Family.voidCount}",
                    IsCheckPassing = Family.voidCount < 5,
                    CheckType = FamilyCheckTypes.Voids,
                    IsCheckVerified = Family.isVoidCountVerified
                },
                new FamilyCheckViewModel(Family)
                {
                    CheckName = $"Arrays: {Family.arrayCount}",
                    IsCheckPassing = Family.arrayCount < 5,
                    CheckType = FamilyCheckTypes.Arrays,
                    IsCheckVerified = Family.isArrayCountVerified
                },
                new FamilyCheckViewModel(Family)
                {
                    CheckName = $"Nested Families: {Family.nestedFamilyCount}",
                    IsCheckPassing = Family.nestedFamilyCount < 5,
                    CheckType = FamilyCheckTypes.NestedFamilies,
                    IsCheckVerified = Family.isNestedFamilyCountVerified
                }
            };
            return output;
        }

        public void EditFamily()
        {
            AppCommand.CommunicatorHandler.FamilyItem = Family;
            AppCommand.CommunicatorHandler.Request.Make(RequestId.EditFamily);
            AppCommand.CommunicatorEvent.Raise();
        }
    }

    public enum FamilyCheckTypes
    {
        Name,
        Size,
        Instances,
        Voids,
        Arrays,
        NestedFamilies
    }
}
