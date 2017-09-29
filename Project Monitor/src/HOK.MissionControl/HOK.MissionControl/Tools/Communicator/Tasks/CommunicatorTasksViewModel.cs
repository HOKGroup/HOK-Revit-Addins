using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Messaging;
using HOK.MissionControl.Tools.Communicator.Tasks.TaskControl;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class CommunicatorTasksViewModel : ViewModelBase
    {
        public CommunicatorTasksModel Model { get; set; }
        private readonly object _lock = new object();

        public CommunicatorTasksViewModel(FamilyStat famData)
        {
            Model = new CommunicatorTasksModel();
            TaskControls = Model.ProcessData(famData);
            BindingOperations.EnableCollectionSynchronization(_taskControls, _lock);

            Messenger.Default.Register<TaskDeletedMessage>(this, OnTaskDeleted);
            Messenger.Default.Register<TaskAddedMessage>(this, OnTaskAdded);
        }

        private void OnTaskAdded(TaskAddedMessage msg)
        {
            FamilyTask task;
            FamilyItem family;

            // (Konrad) Updated globally stored families.
            if (AppCommand.FamiliesToWatch.ContainsKey(msg.FamilyId))
            {
                var oldFamily = AppCommand.FamiliesToWatch[msg.FamilyId];
                var newFamily = msg.FamilyStat.families.FirstOrDefault(x => x.elementId == msg.FamilyId);
                if (newFamily == null) return;

                task = newFamily.tasks[newFamily.tasks.Count - 1];
                oldFamily.tasks.Add(task);

                AppCommand.FamiliesToWatch.Remove(msg.FamilyId);
                AppCommand.FamiliesToWatch.Add(msg.FamilyId, oldFamily);

                family = oldFamily;
            }
            else
            {
                Log.AppendLog(LogMessageType.ERROR, "TaskAssistant: Attemted to add task to non-existing family.");
                return;
            }

            // (Konrad) Update Task UI
            if (!string.Equals(task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase)) return;
            
            var viewModel = new TaskControlViewModel
            {
                Model = new TaskControlModel(family, task),
                TaskComplete = !string.IsNullOrEmpty(task.completedBy),
                TaskName = $"({task.submittedBy}){family.name}: {task.name}"
            };

            // (Konrad) This message was spawned on another thread that Socket App runs
            // In order to manipulate a collection on UI thread we need to lock it first.
            lock (_lock)
            {
                TaskControls.Add(viewModel);
            }
        }

        /// <summary>
        /// This handles Task Deleted Messages from the Mission Control socket app.
        /// </summary>
        /// <param name="msg">Message object.</param>
        private void OnTaskDeleted(TaskDeletedMessage msg)
        {
            var currentTasks = TaskControls.ToDictionary(x => x.Model.Task.Id, x => x);
            foreach (var id in msg.DeletedIds)
            {
                if (currentTasks.ContainsKey(id))
                {
                    // (Konrad) This message was spawned on another thread that Socket App runs
                    // In order to manipulate a collection on UI thread we need to lock it first.
                    lock (_lock)
                    {
                        TaskControls.Remove(currentTasks[id]);
                    }
                }
            }
        }

        private ObservableCollection<TaskControlViewModel> _taskControls;
        public ObservableCollection<TaskControlViewModel> TaskControls
        {
            get { return _taskControls; }
            set { _taskControls = value; RaisePropertyChanged(() => TaskControls); }
        }
    }
}
