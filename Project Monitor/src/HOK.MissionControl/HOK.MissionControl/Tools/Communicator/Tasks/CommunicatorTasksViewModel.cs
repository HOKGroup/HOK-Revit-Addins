using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Tools.Communicator.Messaging;
using HOK.MissionControl.Core.Schemas.Sheets;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class CommunicatorTasksViewModel : ViewModelBase
    {
        public CommunicatorTasksModel Model { get; set; }
        public RelayCommand<TaskWrapper> LaunchTaskAssistant { get; set; }
        public RelayCommand<DataGridExtension> MouseEnter { get; set; }
        public RelayCommand<UserControl> WindowLoaded { get; }
        public DataGridExtension Control { get; set; }
        private readonly object _lock = new object();

        public CommunicatorTasksViewModel(CommunicatorTasksModel model)
        {
            Model = model;
            Tasks = Model.GetTasks();
            BindingOperations.EnableCollectionSynchronization(_tasks, _lock);

            LaunchTaskAssistant = new RelayCommand<TaskWrapper>(OnLaunchTaskAssistant);
            MouseEnter = new RelayCommand<DataGridExtension>(OnMouseEnter);
            WindowLoaded = new RelayCommand<UserControl>(OnWindowLoaded);

            Messenger.Default.Register<FamilyTaskDeletedMessage>(this, OnFamilyTaskDeleted);
            Messenger.Default.Register<FamilyTaskAddedMessage>(this, OnFamilyTaskAdded);
            Messenger.Default.Register<FamilyTaskUpdatedMessage>(this, OnFamilyTaskUpdated);
            Messenger.Default.Register<TaskAssistantClosedMessage>(this, OnFamilyTaskAssistantClosed);
            Messenger.Default.Register<SheetsTaskUpdateMessage>(this, OnSheetTaskUpdated);
            Messenger.Default.Register<SheetsTaskApprovedMessage>(this, OnSheetTaskApproved);
            Messenger.Default.Register<SheetsTaskDeletedMessage>(this, OnSheetsTaskDeleted);
            Messenger.Default.Register<SheetsTaskSheetAddedMessage>(this, OnSheetsTaskSheetAdded);
        }

        private void OnWindowLoaded(UserControl win)
        {
            //(Konrad) We store a control for use later with Messenger
            Control = ((CommunicatorTasksView) win).DataGridTasks;
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

        #region Message Handlers

        /// <summary>
        /// Handles sheet changes being deleted in Mission Control
        /// </summary>
        /// <param name="msg">Message from Mission Control</param>
        private void OnSheetsTaskDeleted(SheetsTaskDeletedMessage msg)
        {
            DeleteTask(msg.Identifier);
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
        /// Handles sheet change being approved by user and updated in DB.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnSheetTaskApproved(SheetsTaskApprovedMessage msg)
        {
            // (Konrad) Update AppCommand sheets
            var currentSheet = AppCommand.SheetsData.sheets.FirstOrDefault(x => x.identifier == msg.Identifier);
            if (currentSheet != null)
            {
                currentSheet.name = msg.Sheet.name;
                currentSheet.number = msg.Sheet.number;
            }

            DeleteTask(msg.Identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void OnSheetsTaskSheetAdded(SheetsTaskSheetAddedMessage msg)
        {
            AppCommand.SheetsData.sheetsChanges.AddRange(msg.NewSheets);

            lock (_lock)
            {
                foreach (var task in msg.NewSheets)
                {
                    Tasks.Add(new SheetTaskWrapper(task, null));
                }
            }
        }

        /// <summary>
        /// Handles Sheets being updated in Mission Control.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnSheetTaskUpdated(SheetsTaskUpdateMessage msg)
        {
            var existingIndex = AppCommand.SheetsData.sheetsChanges.IndexOf(msg.Task);
            if (existingIndex == -1)
            {
                // Add new one
                AppCommand.SheetsData.sheetsChanges.Add(msg.Task);
                var sheetItem = AppCommand.SheetsData.sheets.FirstOrDefault(x => x.uniqueId == msg.Task.uniqueId);

                if (msg.Task.assignedTo == Environment.UserName.ToLower())
                {
                    lock (_lock)
                    {
                        Tasks.Add(new SheetTaskWrapper(msg.Task, sheetItem));
                    }
                }
            }
            else
            {
                // Update existing
                AppCommand.SheetsData.sheetsChanges[existingIndex] = msg.Task;
                var storedTask = Tasks.First(x => x.GetType() == typeof(SheetTaskWrapper) && ((SheetItem)x.Task).identifier == msg.Task.identifier);
                if (msg.Task.assignedTo == Environment.UserName.ToLower())
                {
                    // (Konrad) In order to trigger changes to UI, we need to update properties individually
                    // Only then do they fire proper events and update UI.
                    var sheetTask = (SheetItem)storedTask.Task;
                    sheetTask.name = msg.Task.name;
                    sheetTask.number = msg.Task.number;
                    sheetTask.message = msg.Task.message;
                    sheetTask.assignedTo = msg.Task.assignedTo;
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

        /// <summary>
        /// Handles FamilyTask updates in Mission Control.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
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

            var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(FamilyTaskWrapper) && ((FamilyTask)x.Task).Id == msg.OldTaskId);
            if (storedTask != null)
            {
                if (task.assignedTo == Environment.UserName.ToLower() && string.IsNullOrEmpty(task.completedBy))
                {
                    // (Konrad) In order to trigger changes to UI, we need to update properties individually
                    // Only then do they fire proper events and update UI.
                    var familyTask = (FamilyTask)storedTask.Task;
                    familyTask.name = task.name;
                    familyTask.assignedTo = task.assignedTo;
                    familyTask.comments = task.comments;
                    familyTask.message = task.message;
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

        /// <summary>
        /// Handles FamilyTask being added in Mission Control.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
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

        /// <summary>
        /// Handles an event when FamilyTask has been deleted in MissionControl.
        /// </summary>
        /// <param name="msg">Message from MissionControl.</param>
        private void OnFamilyTaskDeleted(FamilyTaskDeletedMessage msg)
        {
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
                        // (Konrad) This message was spawned on another thread that Socket App runs
                        // In order to manipulate a collection on UI thread we need to lock it first.
                        lock (_lock)
                        {
                            Tasks.Remove(storedTask);
                        }

                        // (Konrad) Similarly to above. We are on a different thread. If we are deleting a task
                        // that has the window currently open, we want to close the window. To do that we need
                        // to get back on UI thread. Every Control has a dispatcher that can do that for us.
                        if (SelectedTask == storedTask)
                        {
                            Control.Dispatcher.Invoke(() =>
                            {
                                Model.LaunchTaskAssistant(null);

                            }, DispatcherPriority.Normal);
                        }
                    }
                }
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        private void DeleteTask(string identifier)
        {
            // (Konrad) Update AppCommand sheetsChanges
            var approvedItem = AppCommand.SheetsData.sheetsChanges.FirstOrDefault(x => x.identifier == identifier);
            if (approvedItem != null)
            {
                var index = AppCommand.SheetsData.sheetsChanges.IndexOf(approvedItem);
                if (index != -1) AppCommand.SheetsData.sheetsChanges.RemoveAt(index);
            }

            // (Konrad) Update Tasks UI
            var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(SheetTaskWrapper) && ((SheetItem)x.Task).identifier == identifier);
            if (storedTask != null)
            {
                var taskIndex = Tasks.IndexOf(storedTask);
                lock (_lock)
                {
                    Tasks.RemoveAt(taskIndex);
                }

                // (Konrad) If task is open let's close it 
                if (SelectedTask == storedTask)
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
