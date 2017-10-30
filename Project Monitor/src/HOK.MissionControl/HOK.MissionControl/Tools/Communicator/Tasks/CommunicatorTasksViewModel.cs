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
        public RelayCommand<TaskWrapper> LaunchTaskAssistant { get; set; }
        public RelayCommand<DataGridExtension> MouseEnter { get; set; }
        private readonly object _lock = new object();

        public CommunicatorTasksViewModel(CommunicatorTasksModel model)
        {
            Model = model;
            Tasks = Model.GetTasks();
            BindingOperations.EnableCollectionSynchronization(_tasks, _lock);

            LaunchTaskAssistant = new RelayCommand<TaskWrapper>(OnLaunchTaskAssistant);
            MouseEnter = new RelayCommand<DataGridExtension>(OnMouseEnter);

            Messenger.Default.Register<FamilyTaskDeletedMessage>(this, OnFamilyTaskDeleted);
            Messenger.Default.Register<FamilyTaskAddedMessage>(this, OnFamilyTaskAdded);
            Messenger.Default.Register<FamilyTaskUpdatedMessage>(this, OnFamilyTaskUpdated);
            Messenger.Default.Register<FamilyTaskAssistantClosedMessage>(this, OnFamilyTaskAssistantClosed);
            Messenger.Default.Register<SheetsTaskUpdateMessage>(this, OnSheetTaskUpdated);
        }

        private void OnSheetTaskUpdated(SheetsTaskUpdateMessage msg)
        {
            var existingIndex = AppCommand.SheetsData.sheetsChanges.IndexOf(msg.Task);
            if (existingIndex == -1)
            {
                // Add new one
                AppCommand.SheetsData.sheetsChanges.Add(msg.Task);

                if (msg.Task.assignedTo == Environment.UserName.ToLower())
                {
                    lock (_lock)
                    {
                        Tasks.Add(new SheetTaskWrapper(msg.Task));
                    }
                }
            }
            else
            {
                // Update existing
                AppCommand.SheetsData.sheetsChanges[existingIndex] = msg.Task;
                var storedTask = Tasks.First(x => x.GetType() == typeof(SheetTaskWrapper) &&
                                                           ((SheetTask) x.Task).identifier == msg.Task.identifier);

                if (msg.Task.assignedTo == Environment.UserName.ToLower() && string.IsNullOrEmpty(msg.Task.completedBy))
                {
                    ((SheetTask)storedTask.Task).name = msg.Task.name; //TODO: must be class with propertychanged prop
                }
                else
                {
                    // (Konrad) Someone reassigned the task away from us.
                    lock (_lock)
                    {
                        Tasks.Remove(storedTask);
                    }
                }
            }
        }

        private static void OnMouseEnter(DataGridExtension dg)
        {
            dg.Cursor = Cursors.Hand;
        }

        private void OnLaunchTaskAssistant(TaskWrapper task)
        {
            if (SelectedTask == null) return;
            Model.LaunchTaskAssistant(task);
        }

        /// <summary>
        /// Handles a message emmited by Closing a Family Task.
        /// </summary>
        /// <param name="msg"></param>
        private void OnFamilyTaskAssistantClosed(FamilyTaskAssistantClosedMessage msg)
        {
            SelectedTask = null;
            Model.TaskView = null;
        }
        
        /// <summary>
        /// Handles a message emitted by Updating a Family Task.
        /// </summary>
        /// <param name="msg"></param>
        private void OnFamilyTaskUpdated(FamilyTaskUpdatedMessage msg)
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
                Log.AppendLog(LogMessageType.ERROR, "FamilyTaskAssistant: Attempted to add task to non-existing family.");
                return;
            }

            var existingTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(FamilyTask) && ((FamilyTask)x.Task).Id == msg.OldTaskId);
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

        private void OnFamilyTaskAdded(FamilyTaskAddedMessage msg)
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
                Log.AppendLog(LogMessageType.ERROR, "FamilyTaskAssistant: Attemted to add task to non-existing family.");
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

        private void OnFamilyTaskDeleted(FamilyTaskDeletedMessage msg)
        {
            var currentFamilyTasks = Tasks
                .Where(x => x.GetType() == typeof(FamilyTask))
                .ToDictionary(x => ((FamilyTask)x.Task).Id, x => x);

            foreach (var id in msg.DeletedIds)
            {
                if (currentFamilyTasks.ContainsKey(id))
                {
                    // (Konrad) This message was spawned on another thread that Socket App runs
                    // In order to manipulate a collection on UI thread we need to lock it first.
                    lock (_lock)
                    {
                        Tasks.Remove(currentFamilyTasks[id]);
                    }
                }
            }
        }

        private ObservableCollection<TaskWrapper> _tasks;
        public ObservableCollection<TaskWrapper> Tasks
        {
            get { return _tasks; }
            set { _tasks = value; RaisePropertyChanged(() => Tasks); }
        }

        private TaskWrapper _selectedTask;
        public TaskWrapper SelectedTask
        {
            get { return _selectedTask; }
            set { _selectedTask = value; RaisePropertyChanged(() => SelectedTask); }
        }
    }
}
