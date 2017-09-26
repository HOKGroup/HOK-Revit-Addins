using System.Collections.ObjectModel;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Tasks.FamilyChecksControl;

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
        }

        public ObservableCollection<FamilyCheckViewModel> ProcessChecks()
        {
            var output = new ObservableCollection<FamilyCheckViewModel>
            {
                new FamilyCheckViewModel
                {
                    CheckName = $"Name: {Family.name}",
                    IsCheckPassing = Family.name.Contains("_HOK_I") || Family.name.Contains("_HOK_M")
                },
                new FamilyCheckViewModel
                {
                    CheckName = $"Size: {Family.size}",
                    IsCheckPassing = Family.sizeValue < 1000000
                },
                new FamilyCheckViewModel
                {
                    CheckName = $"Instances: {Family.instances}",
                    IsCheckPassing = Family.instances > 0
                },
                new FamilyCheckViewModel
                {
                    CheckName = $"Voids: {Family.voidCount}",
                    IsCheckPassing = Family.voidCount < 5
                },
                new FamilyCheckViewModel
                {
                    CheckName = $"Arrays: {Family.arrayCount}",
                    IsCheckPassing = Family.arrayCount < 5
                },
                new FamilyCheckViewModel
                {
                    CheckName = $"Nested Families: {Family.nestedFamilyCount}",
                    IsCheckPassing = Family.nestedFamilyCount < 5
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
}
