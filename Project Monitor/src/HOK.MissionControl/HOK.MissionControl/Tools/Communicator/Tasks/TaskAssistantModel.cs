using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Tasks
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

        public void EditFamily()
        {
            AppCommand.CommunicatorHandler.FamilyItem = Family;
            AppCommand.CommunicatorHandler.FamilyTask = Task;
            AppCommand.CommunicatorHandler.Request.Make(RequestId.EditFamily);
            AppCommand.CommunicatorEvent.Raise();
        }
    }
}
