using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Interop;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Tasks.TaskAssistant;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class FamilyTaskWrapper: FamilyTask, INotifyPropertyChanged
    {
        private FamilyItem _family;
        public FamilyItem Family
        {
            get { return _family; }
            set { _family = value; RaisePropertyChanged("Family"); }
        }

        private FamilyTask _task;
        public FamilyTask Task {
            get { return _task; }
            set { _task = value; RaisePropertyChanged("Task"); }
        }

        public FamilyTaskWrapper(FamilyItem fam, FamilyTask task)
        {
            Family = fam;
            Task = task;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }

    public class CommunicatorTasksModel
    {
        private FamilyStat FamilyStats { get; }
        public TaskAssistantView TaskAssistantView { get; set; }

        public CommunicatorTasksModel(FamilyStat famStat)
        {
            FamilyStats = famStat;
        }

        public void LaunchTaskAssistant(FamilyTaskWrapper wrapper)
        {
            // (Konrad) In case that someone deletes the task using the web interface
            // this will trigger with a null value. If the window is open let's close it and reset.
            if (wrapper == null)
            {
                if (TaskAssistantView == null) return;

                TaskAssistantView.Close();
                TaskAssistantView = null;
                return;
            }

            var viewModel = new TaskAssistantViewModel(wrapper);
            if (TaskAssistantView != null && TaskAssistantView.IsVisible)
            {
                TaskAssistantView.DataContext = viewModel;
                TaskAssistantView.Activate();
            }
            else
            {
                TaskAssistantView = new TaskAssistantView
                {
                    DataContext = viewModel
                };
                var unused = new WindowInteropHelper(TaskAssistantView)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };

                TaskAssistantView.Show();
            }
        }

        public ObservableCollection<FamilyTaskWrapper> GetTasks()
        {
            if (FamilyStats == null) return new ObservableCollection<FamilyTaskWrapper>();

            var familiesToWatch = new Dictionary<string, FamilyItem>();
            var output = new List<FamilyTaskWrapper>();
            foreach (var family in FamilyStats.families)
            {
                // (Konrad) We are storing this Family Item for later so that if user makes any changes to this family
                // and reloads it back into the model, we can capture that and post to MongoDB at the end of the session.
                if (!familiesToWatch.ContainsKey(family.name)) familiesToWatch.Add(family.name, family);

                if (!family.tasks.Any()) continue;
                foreach (var task in family.tasks)
                {
                    if (!string.IsNullOrEmpty(task.completedBy)) continue;
                    if (!string.Equals(task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase)) continue;

                    output.Add(new FamilyTaskWrapper(family, task));
                }
            }

            AppCommand.FamiliesToWatch = familiesToWatch;
            return new ObservableCollection<FamilyTaskWrapper>(output);

        }
    }
}
