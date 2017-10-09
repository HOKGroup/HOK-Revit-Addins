using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class CommunicatorTasksViewModel : ViewModelBase
    {
        public CommunicatorTasksModel Model { get; set; }
        public RelayCommand<FamilyTaskWrapper> LaunchTaskAssistant { get; set; }
        public RelayCommand<DataGridExtension> MouseEnter { get; set; }
        private readonly object _lock = new object();

        public CommunicatorTasksViewModel(CommunicatorTasksModel model)
        {
            Model = model;
            Tasks = Model.GetTasks();
            BindingOperations.EnableCollectionSynchronization(_tasks, _lock);

            LaunchTaskAssistant = new RelayCommand<FamilyTaskWrapper>(OnLaunchTaskAssistant);
            MouseEnter = new RelayCommand<DataGridExtension>(OnMouseEnter);
            
            Messenger.Default.Register<TaskDeletedMessage>(this, OnTaskDeleted);
            Messenger.Default.Register<TaskAddedMessage>(this, OnTaskAdded);
            Messenger.Default.Register<TaskUpdatedMessage>(this, OnTaskUpdated);
            Messenger.Default.Register<TaskAssistantClosedMessage>(this, OnTaskAssistantClosed);
        }

        private void OnTaskAssistantClosed(TaskAssistantClosedMessage obj)
        {
            SelectedTask = null;
        }

        private static void OnMouseEnter(DataGridExtension dg)
        {
            dg.Cursor = Cursors.Hand;
        }

        private void OnLaunchTaskAssistant(FamilyTaskWrapper task)
        {
            if (SelectedTask == null) return;
            Model.LaunchTaskAssistant(task);
        }

        private void OnTaskUpdated(TaskUpdatedMessage msg)
        {
            FamilyTask task;
            FamilyItem family;

            // (Konrad) Updated globally stored families.
            if (AppCommand.FamiliesToWatch.ContainsKey(msg.FamilyName))
            {
                var oldFamily = AppCommand.FamiliesToWatch[msg.FamilyName];
                var newFamily = msg.FamilyStat.families.FirstOrDefault(x => x.name == msg.FamilyName);
                if (newFamily == null) return;

                task = newFamily.tasks[newFamily.tasks.Count - 1]; // latest task will be last on a list
                oldFamily.tasks.Add(task);

                AppCommand.FamiliesToWatch.Remove(msg.FamilyName);
                AppCommand.FamiliesToWatch.Add(msg.FamilyName, oldFamily);

                family = oldFamily;
            }
            else
            {
                Log.AppendLog(LogMessageType.ERROR, "TaskAssistant: Attempted to add task to non-existing family.");
                return;
            }

            var existingTask = Tasks.FirstOrDefault(x => x.Task.Id == msg.OldTaskId);
            if (existingTask != null)
            {
                if (task.assignedTo == Environment.UserName.ToLower() && string.IsNullOrEmpty(task.completedBy))
                {
                    // (Konrad) Task still belongs to us. Let's update it.
                    existingTask.Task = task;
                }
                else
                {
                    // (Konrad) Someone reassigned the task away from us.
                    lock (_lock)
                    {
                        Tasks.Remove(existingTask);
                    }
                }
            }
            else
            {
                if (task.assignedTo == Environment.UserName.ToLower())
                {
                    // (Konrad) Someone reassigned a task to us.
                    lock (_lock)
                    {
                        Tasks.Add(new FamilyTaskWrapper(family, task));
                    }
                }
            }
        }

        private void OnTaskAdded(TaskAddedMessage msg)
        {
            FamilyTask task;
            FamilyItem family;

            // (Konrad) Updated globally stored families.
            if (AppCommand.FamiliesToWatch.ContainsKey(msg.FamilyName))
            {
                var oldFamily = AppCommand.FamiliesToWatch[msg.FamilyName];
                var newFamily = msg.FamilyStat.families.FirstOrDefault(x => x.name == msg.FamilyName);
                if (newFamily == null) return;

                task = newFamily.tasks[newFamily.tasks.Count - 1]; // latest task will be last on a list
                oldFamily.tasks.Add(task);

                AppCommand.FamiliesToWatch.Remove(msg.FamilyName);
                AppCommand.FamiliesToWatch.Add(msg.FamilyName, oldFamily);

                family = oldFamily;
            }
            else
            {
                Log.AppendLog(LogMessageType.ERROR, "TaskAssistant: Attemted to add task to non-existing family.");
                return;
            }

            // (Konrad) Update Task UI
            if (!string.Equals(task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase)) return;

            // (Konrad) This message was spawned on another thread that Socket App runs
            // In order to manipulate a collection on UI thread we need to lock it first.
            lock (_lock)
            {
                Tasks.Add(new FamilyTaskWrapper(family, task));
            }
        }

        private void OnTaskDeleted(TaskDeletedMessage msg)
        {
            var currentTasks = Tasks.ToDictionary(x => x.Task.Id, x => x);
            foreach (var id in msg.DeletedIds)
            {
                if (currentTasks.ContainsKey(id))
                {
                    // (Konrad) This message was spawned on another thread that Socket App runs
                    // In order to manipulate a collection on UI thread we need to lock it first.
                    lock (_lock)
                    {
                        Tasks.Remove(currentTasks[id]);
                    }
                }
            }
        }

        private ObservableCollection<FamilyTaskWrapper> _tasks;
        public ObservableCollection<FamilyTaskWrapper> Tasks
        {
            get { return _tasks; }
            set { _tasks = value; RaisePropertyChanged(() => Tasks); }
        }

        private FamilyTaskWrapper _selectedTask;
        public FamilyTaskWrapper SelectedTask
        {
            get { return _selectedTask; }
            set { _selectedTask = value; RaisePropertyChanged(() => SelectedTask); }
        }
    }
}
