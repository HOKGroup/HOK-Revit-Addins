using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class TaskControlModel
    {
        public FamilyItem Family { get; set; }
        public FamilyTask Task { get; set; }

        public TaskControlModel(FamilyItem family, FamilyTask task)
        {
            Family = family;
            Task = task;
        }

        public void LaunchTaskAssistant()
        {
            var model = new TaskAssistantModel(Family, Task);
            var viewModel = new TaskAssistantViewModel(model);
            var view = new TaskAssistantView
            {
                DataContext = viewModel
            };

            view.Show();
        }
    }
}
