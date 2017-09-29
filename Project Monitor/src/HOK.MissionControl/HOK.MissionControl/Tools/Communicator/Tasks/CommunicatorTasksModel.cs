using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Tasks.TaskControl;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class CommunicatorTasksModel
    {
        public ObservableCollection<TaskControlViewModel> ProcessData(FamilyStat famStat)
        {
            if (famStat == null) return new ObservableCollection<TaskControlViewModel>();

            var familiesToWatch = new Dictionary<int, FamilyItem>();
            var output = new List<TaskControlViewModel>();
            foreach (var family in famStat.families)
            {
                // (Konrad) We are storing this Family Item for later so that if user makes any changes to this family
                // and reloads it back into the model, we can capture that and post to MongoDB at the end of the session.
                if (!familiesToWatch.ContainsKey(family.elementId)) familiesToWatch.Add(family.elementId, family);

                if (!family.tasks.Any()) continue;
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

            AppCommand.FamiliesToWatch = familiesToWatch;
            return new ObservableCollection<TaskControlViewModel>(output.OrderBy(x => x.TaskComplete));
        }
    }
}
