using System;
using System.Collections.ObjectModel;
using System.Linq;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class CommunicatorTasksModel
    {
        public ObservableCollection<TaskControlViewModel> ProcessData(FamilyStat famStat)
        {
            if (famStat == null) return new ObservableCollection<TaskControlViewModel>();

            var output = new ObservableCollection<TaskControlViewModel>();
            foreach (var family in famStat.families)
            {
                if(!family.tasks.Any()) continue;
                foreach (var task in family.tasks)
                {
                    if(!string.Equals(task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase)) continue;

                    output.Add(new TaskControlViewModel
                    {
                        Model = new TaskControlModel(family, task),
                        TaskComplete = !string.IsNullOrEmpty(task.completedBy),
                        TaskName = $"({task.submittedBy}){family.name}: {task.name}"
                    });
                }
            }

            return output;
        }
    }
}
