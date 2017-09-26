using System.Diagnostics;
using System.Windows.Interop;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Tasks.TaskAssistant;

namespace HOK.MissionControl.Tools.Communicator.Tasks.TaskControl
{
    public class TaskControlModel
    {
        public FamilyItem Family { get; set; }
        public FamilyTask Task { get; set; }
        public static TaskAssistantView View { get; set; }

        public TaskControlModel(FamilyItem family, FamilyTask task)
        {
            Family = family;
            Task = task;
        }

        public void LaunchTaskAssistant()
        {
            var model = new TaskAssistantModel(Family, Task);
            var viewModel = new TaskAssistantViewModel(model);
            if (View != null && View.IsVisible)
            {
                View.DataContext = viewModel;
                View.Activate();
            }
            else
            {
                View = new TaskAssistantView
                {
                    DataContext = viewModel
                };
                var unused = new WindowInteropHelper(View)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };

                View.Show();
            }
        }
    }
}
