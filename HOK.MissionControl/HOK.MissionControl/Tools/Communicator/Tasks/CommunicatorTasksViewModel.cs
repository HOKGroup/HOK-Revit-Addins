#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Tools.Communicator.Messaging;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Utils;
#endregion

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class CommunicatorTasksViewModel : ObservableRecipient
    {
        public CommunicatorTasksModel Model { get; set; }
        public RelayCommand<TaskWrapper> LaunchTaskAssistant { get; set; }
        public RelayCommand<DataGridExtension> MouseEnter { get; set; }
        public RelayCommand<UserControl> WindowLoaded { get; }
        public RelayCommand<UserControl> WindowClosed { get; set; }
        public DataGridExtension Control { get; set; }
        private readonly object _lock = new object();

        public CommunicatorTasksViewModel(CommunicatorTasksModel model)
        {
            Model = model;
            BindingOperations.EnableCollectionSynchronization(_tasks, _lock);

            LaunchTaskAssistant = new RelayCommand<TaskWrapper>(OnLaunchTaskAssistant);
            MouseEnter = new RelayCommand<DataGridExtension>(OnMouseEnter);
            WindowLoaded = new RelayCommand<UserControl>(OnWindowLoaded);
            WindowClosed = new RelayCommand<UserControl>(OnWindowClosed);

            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, CommunicatorDataDownloaded>(this, static (r, m) => r.OnCommunicatorDataDownloaded(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, FamilyTaskDeletedMessage>(this, static (r, m) => r.OnFamilyTaskDeleted(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, FamilyTaskAddedMessage>(this, static (r, m) => r.OnFamilyTaskAdded(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, FamilyTaskUpdatedMessage>(this, static (r, m) => r.OnFamilyTaskUpdated(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, TaskAssistantClosedMessage>(this, static (r, m) => r.OnFamilyTaskAssistantClosed(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, SheetsTaskAddedMessage>(this, static (r, m) => r.OnSheetTaskAdded(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, SheetsTaskUpdatedMessage>(this, static (r, m) => r.OnSheetTaskUpdated(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, SheetsTaskDeletedMessage>(this, static (r, m) => r.OnSheetsTaskDeleted(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, SheetTaskSheetsCreatedMessage>(this, static (r, m) => r.OnSheetTaskSheetsCreated(m));
            WeakReferenceMessenger.Default.Register<CommunicatorTasksViewModel, SheetTaskSheetDeletedMessage>(this, static (r, m) => r.OnSheetTaskSheetDeleted(m));
        }

        private void OnWindowLoaded(UserControl win)
        {
            //(Konrad) We store a control for use later with Messenger
            Control = ((CommunicatorTasksView) win).DataGridTasks;
        }

        private void OnWindowClosed(UserControl win)
        {
            // (Konrad) We need to unregister the event handler when window is closed, otherwise it will add another one next time.
            OnDeactivated();
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

        private ObservableCollection<TaskWrapper> _tasks = new ObservableCollection<TaskWrapper>();
        public ObservableCollection<TaskWrapper> Tasks
        {
            get { return _tasks; }
            set { _tasks = value; OnPropertyChanged(nameof(Tasks)); }
        }

        private TaskWrapper _selectedTask;
        public TaskWrapper SelectedTask
        {
            get { return _selectedTask; }
            set { _selectedTask = value; OnPropertyChanged(nameof(SelectedTask)); }
        }

        #region Message Handlers

        /// <summary>
        /// Handles a message emitted when stats are done downloading.
        /// </summary>
        /// <param name="obj"></param>
        private void OnCommunicatorDataDownloaded(CommunicatorDataDownloaded obj)
        {
            List<TaskWrapper> tasks;
            switch (obj.Type)
            {
                case DataType.Sheets:
                    Model.SheetsData = MissionControlSetup.SheetsData[obj.CentralPath];
                    tasks = Model.ProcessSheetsData();
                    break;
                case DataType.Families:
                    Model.FamiliesData = MissionControlSetup.FamilyData[obj.CentralPath];
                    tasks = Model.ProcessFamiliesData();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (tasks == null) return;

            lock (_lock)
            {
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }
            }
        }

        /// <summary>
        /// Handles a message emmited by Closing a Family Task.
        /// </summary>
        /// <param name="msg"></param>
        private void OnFamilyTaskAssistantClosed(TaskAssistantClosedMessage msg)
        {
            SelectedTask = null;
            Model.TaskView = null;
        }

        /// <summary>
        /// Handles deleting of a new sheet. 
        /// </summary>
        /// <param name="msg"></param>
        private void OnSheetTaskSheetDeleted(SheetTaskSheetDeletedMessage msg)
        {
            var sheetsData = MissionControlSetup.SheetsData[msg.CentralPath];
            var existingIndex = sheetsData.Sheets.FindIndex(x => string.Equals(x.Id, msg.SheetId, StringComparison.Ordinal));
            if (existingIndex == -1) return;

            // (Konrad) Update SheetsData stored in AppCommand
            sheetsData.Sheets.RemoveAt(existingIndex);

            foreach (var id in msg.DeletedIds)
            {
                // (Konrad) Update Tasks collection in UI
                var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(SheetTaskWrapper) && string.Equals(((SheetTask)x.Task).Id, id, StringComparison.Ordinal));
                if (storedTask == null) continue;

                LockRemoveClose(storedTask);
            }
        }

        /// <summary>
        /// Handles creating a new sheet. New sheet has to have a matching centralPath and assignedTo to pass.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnSheetTaskSheetsCreated(SheetTaskSheetsCreatedMessage msg)
        {
            var sheetsData = MissionControlSetup.SheetsData[msg.CentralPath];
            var assignedTo = msg.Sheets.FirstOrDefault()?.Tasks.FirstOrDefault()?.AssignedTo; // all tasks will be assigned to one user only
            if (!string.IsNullOrEmpty(assignedTo) && !string.Equals(assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.Ordinal)) return;

            sheetsData.Sheets.AddRange(msg.Sheets); // update stored sheets
            lock (_lock)
            {
                foreach (var sheetItem in msg.Sheets)
                {
                    Tasks.Add(new SheetTaskWrapper(sheetItem, sheetItem.Tasks.First()));
                }
            }
        }

        /// <summary>
        /// Handles sheet changes being deleted in Mission Control
        /// </summary>
        /// <param name="msg">Message from Mission Control</param>
        private void OnSheetsTaskDeleted(SheetsTaskDeletedMessage msg)
        {
            var sheetsData = MissionControlSetup.SheetsData[msg.CentralPath];
            var existingIndex = sheetsData.Sheets.FindIndex(x => string.Equals(x.Id, msg.SheetId, StringComparison.Ordinal));
            if (existingIndex == -1) return;

            foreach (var id in msg.DeletedIds)
            {
                // (Konrad) Update SheetsData stored in AppCommand
                var taskIndex = sheetsData.Sheets[existingIndex].Tasks.FindIndex(x => string.Equals(x.Id, id, StringComparison.Ordinal));
                if(taskIndex == -1) continue;

                sheetsData.Sheets[existingIndex].Tasks.RemoveAt(taskIndex);

                // (Konrad) Update Tasks collection in UI
                var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(SheetTaskWrapper) && string.Equals(((SheetTask)x.Task).Id, id, StringComparison.Ordinal));
                if (storedTask == null) continue;

                LockRemoveClose(storedTask);
            }
        }

        /// <summary>
        /// Handles Sheet Task being updated.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnSheetTaskUpdated(SheetsTaskUpdatedMessage msg)
        {
            var sheetsData = MissionControlSetup.SheetsData[msg.CentralPath];
            var existingIndex = sheetsData.Sheets.IndexOf(msg.Sheet);
            if (existingIndex == -1) return;
            var existingTaskIndex = sheetsData.Sheets[existingIndex].Tasks.FindIndex(x => x.Id == msg.Task.Id);
            if (existingTaskIndex == -1)
            {
                if (string.Equals(msg.Task.AssignedTo.ToLower(), Environment.UserName.ToLower(),StringComparison.CurrentCultureIgnoreCase))
                {
                    lock (_lock)
                    {
                        Tasks.Add(new SheetTaskWrapper(msg.Sheet, msg.Task)); // task wasn't ours but was re-assigned to us
                    }
                    sheetsData.Sheets[existingIndex].Tasks.Add(msg.Task);
                }
                return;
            }

            var existingTask = sheetsData.Sheets[existingIndex].Tasks[existingTaskIndex];
            if (!string.Equals(existingTask.AssignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase))
            {
                if (!string.Equals(msg.Task.AssignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase)) return; // it still doesn't belong to us.
                lock (_lock)
                {
                    Tasks.Add(new SheetTaskWrapper(msg.Sheet, msg.Task)); // old task was re-assigned to us, so we can add it
                }
            }
            else
            {
                var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(SheetTaskWrapper) && ((SheetTask)x.Task).Id == msg.Task.Id);
                if (storedTask == null)
                {
                    lock (_lock)
                    {
                        Tasks.Add(new SheetTaskWrapper(msg.Sheet, msg.Task)); // task might have been closed and someone is re-opening it
                    }

                    sheetsData.Sheets[existingIndex].Tasks[existingTaskIndex] = msg.Task;
                    return;
                }

                if (!string.Equals(msg.Task.AssignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase))
                {
                    LockRemoveClose(storedTask);
                }
                else
                {
                    var task = storedTask.Task as SheetTask;
                    if (task == null) return;

                    task.CopyProperties(msg.Task); // it's still ours, update it

                    // (Konrad) We only get here when user hits "Approve"
                    if (!string.IsNullOrEmpty(task.CompletedBy))
                    {
                        LockRemoveClose(storedTask);
                    }
                }
            }

            // replace existing task
            sheetsData.Sheets[existingIndex].Tasks[existingTaskIndex] = msg.Task;
        }

        /// <summary>
        /// Handles Sheet Task being added.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnSheetTaskAdded(SheetsTaskAddedMessage msg)
        {
            var sheetsData = MissionControlSetup.SheetsData[msg.CentralPath];
            var existingIndex = sheetsData.Sheets.IndexOf(msg.Sheet);
            if (existingIndex != -1)
            {
                // Update existing sheet
                sheetsData.Sheets[existingIndex].Tasks.Add(msg.Task);

                lock (_lock)
                {
                    Tasks.Add(new SheetTaskWrapper(msg.Sheet, msg.Task));
                }
            }
            else
            {
                Log.AppendLog(LogMessageType.WARNING, "Cannot add Sheet Task because no matching Sheet was found.");
            }
        }

        /// <summary>
        /// Handles FamilyTask updates in Mission Control.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnFamilyTaskUpdated(FamilyTaskUpdatedMessage msg)
        {
            FamilyItem family;

            // (Konrad) Updated globally stored families.
            if (AppCommand.FamiliesToWatch.ContainsKey(msg.FamilyName))
            {
                var oldFamily = AppCommand.FamiliesToWatch[msg.FamilyName];
                var taskIndex = oldFamily.Tasks.FindIndex(x => x.Id == msg.Task.Id);
                if (taskIndex == -1) return; 

                oldFamily.Tasks.RemoveAt(taskIndex);
                oldFamily.Tasks.Add(msg.Task);

                family = oldFamily;
            }
            else
            {
                Log.AppendLog(LogMessageType.ERROR, "FamilyTaskAssistant: Attempted to add task to non-existing family.");
                return;
            }

            var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(FamilyTaskWrapper) && ((FamilyTask)x.Task).Id == msg.Task.Id);
            if (storedTask != null)
            {
                if (string.Equals(msg.Task.AssignedTo.ToLower(), Environment.UserName.ToLower(),
                        StringComparison.Ordinal) && string.IsNullOrEmpty(msg.Task.CompletedBy))
                {
                    // (Konrad) In order to trigger changes to UI, we need to update properties individually
                    // Only then do they fire proper events and update UI.
                    var familyTask = storedTask.Task as FamilyTask;
                    if (familyTask == null) return;

                    familyTask.CopyProperties(msg.Task);
                }
                else
                {
                    // (Konrad) Someone reassigned the task away from us.
                    lock (_lock)
                    {
                        LockRemoveClose(storedTask);
                    }
                }
            }
            else
            {
                if (string.Equals(msg.Task.AssignedTo.ToLower(), Environment.UserName.ToLower(),
                    StringComparison.Ordinal))
                {
                    // (Konrad) Someone reassigned a task to us.
                    lock (_lock)
                    {
                        Tasks.Add(new FamilyTaskWrapper(family, msg.Task));
                    }
                }
            }
        }

        /// <summary>
        /// Handles FamilyTask being added in Mission Control.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnFamilyTaskAdded(FamilyTaskAddedMessage msg)
        {
            FamilyItem family;

            // (Konrad) Updated globally stored families.
            if (AppCommand.FamiliesToWatch.ContainsKey(msg.FamilyName))
            {
                var oldFamily = AppCommand.FamiliesToWatch[msg.FamilyName];
                oldFamily.Tasks.Add(msg.Task);

                AppCommand.FamiliesToWatch.Remove(msg.FamilyName);
                AppCommand.FamiliesToWatch.Add(msg.FamilyName, oldFamily);

                family = oldFamily;
            }
            else
            {
                Log.AppendLog(LogMessageType.ERROR, "FamilyTaskAssistant: Attemted to add task to non-existing family.");
                return;
            }

            // (Konrad) This message was spawned on another thread that Socket App runs
            // In order to manipulate a collection on UI thread we need to lock it first.
            lock (_lock)
            {
                Tasks.Add(new FamilyTaskWrapper(family, msg.Task));
            }
        }

        /// <summary>
        /// Handles an event when FamilyTask has been deleted in MissionControl.
        /// </summary>
        /// <param name="msg">Message from MissionControl.</param>
        private void OnFamilyTaskDeleted(FamilyTaskDeletedMessage msg)
        {
            // (Konrad) Remove tasks from FamiliesToWatch
            if (AppCommand.FamiliesToWatch.ContainsKey(msg.FamilyName))
            {
                var oldFamily = AppCommand.FamiliesToWatch[msg.FamilyName];
                foreach (var id in msg.DeletedIds)
                {
                    var index = oldFamily.Tasks.FindIndex(x => x.Id == id);
                    if(index != -1) oldFamily.Tasks.RemoveAt(index);
                }
            }

            var currentFamilyTasks = Tasks
                .Where(x => x.GetType() == typeof(FamilyTaskWrapper))
                .ToDictionary(x => ((FamilyTask)x.Task).Id, x => (FamilyTaskWrapper)x);

            foreach (var id in msg.DeletedIds)
            {
                if (currentFamilyTasks.ContainsKey(id))
                {
                    var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(FamilyTaskWrapper) && ((FamilyTask)x.Task).Id == id);
                    if (storedTask != null)
                    {
                        LockRemoveClose(storedTask);
                    }
                }
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Locks the Tasks collection, Removes specified task and attempts to close the task window if open.
        /// </summary>
        /// <param name="task">Task object to be removed.</param>
        private void LockRemoveClose(TaskWrapper task)
        {
            lock (_lock)
            {
                Tasks.Remove(task);

                // (Konrad) Similarly to above. We are on a different thread. If we are deleting a task
                // that has the window currently open, we want to close the window. To do that we need
                // to get back on UI thread. Every Control has a dispatcher that can do that for us.
                if (SelectedTask == task)
                {
                    Control.Dispatcher.Invoke(() =>
                    {
                        Model.LaunchTaskAssistant(null);

                    }, DispatcherPriority.Normal);
                }
            }
        }

        #endregion
    }
}
