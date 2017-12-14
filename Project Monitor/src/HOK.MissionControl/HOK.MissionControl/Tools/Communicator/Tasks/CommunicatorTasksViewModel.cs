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
        public RelayCommand<UserControl> WindowClosed { get; set; }
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
            WindowClosed = new RelayCommand<UserControl>(OnWindowClosed);

            Messenger.Default.Register<FamilyTaskDeletedMessage>(this, OnFamilyTaskDeleted);
            Messenger.Default.Register<FamilyTaskAddedMessage>(this, OnFamilyTaskAdded);
            Messenger.Default.Register<FamilyTaskUpdatedMessage>(this, OnFamilyTaskUpdated);
            Messenger.Default.Register<TaskAssistantClosedMessage>(this, OnFamilyTaskAssistantClosed);
            Messenger.Default.Register<SheetsTaskAddedMessage>(this, OnSheetTaskAdded);
            Messenger.Default.Register<SheetsTaskUpdatedMessage>(this, OnSheetTaskUpdated);
            Messenger.Default.Register<SheetsTaskDeletedMessage>(this, OnSheetsTaskDeleted);
            Messenger.Default.Register<SheetTaskSheetsCreatedMessage>(this, OnSheetTaskSheetsCreated);
            //Messenger.Default.Register<SheetsTaskApprovedMessage>(this, OnSheetTaskApproved);
            //Messenger.Default.Register<SheetsTaskSheetAddedMessage>(this, OnSheetsTaskSheetAdded);
        }

        private void OnWindowLoaded(UserControl win)
        {
            //(Konrad) We store a control for use later with Messenger
            Control = ((CommunicatorTasksView) win).DataGridTasks;
        }

        private void OnWindowClosed(UserControl win)
        {
            // (Konrad) We need to unregister the event handler when window is closed, otherwise it will add another one next time.
            Cleanup();
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
        /// Handles a message emmited by Closing a Family Task.
        /// </summary>
        /// <param name="msg"></param>
        private void OnFamilyTaskAssistantClosed(TaskAssistantClosedMessage msg)
        {
            SelectedTask = null;
            Model.TaskView = null;
        }

        ///// <summary>
        ///// Handles sheet change being approved by user and updated in DB.
        ///// </summary>
        ///// <param name="msg">Message from Mission Control.</param>
        //private void OnSheetTaskApproved(SheetsTaskApprovedMessage msg)
        //{
        //    // (Konrad) Update AppCommand sheets
        //    var currentSheet = AppCommand.SheetsData.sheets.FirstOrDefault(x => x.identifier == msg.Identifier);
        //    if (currentSheet != null)
        //    {
        //        // (Konrad) Update stored data.
        //        currentSheet.name = msg.Sheet.name;
        //        currentSheet.number = msg.Sheet.number;
        //    }
        //    else
        //    {
        //        // (Konrad) New Sheet would not be present in the storage yet. We can add it.
        //        AppCommand.SheetsData.sheets.Add(msg.Sheet);
        //    }

        //    DeleteTask(msg.Identifier, msg.Sheet);
        //}

        /// <summary>
        /// Handles creating a new sheet. New sheet has to have a matching centralPath and assignedTo to pass.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnSheetTaskSheetsCreated(SheetTaskSheetsCreatedMessage msg)
        {
            var assignedTo = msg.Sheets.FirstOrDefault()?.tasks.FirstOrDefault()?.assignedTo;
            if (!string.IsNullOrEmpty(assignedTo) && !string.Equals(assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.Ordinal)) return;

            AppCommand.SheetsData.sheets.AddRange(msg.Sheets); // update stored sheets
            lock (_lock)
            {
                foreach (var sheetItem in msg.Sheets)
                {
                    Tasks.Add(new SheetTaskWrapper(sheetItem, sheetItem.tasks.First()));
                }
            }
        }

        /// <summary>
        /// Handles sheet changes being deleted in Mission Control
        /// </summary>
        /// <param name="msg">Message from Mission Control</param>
        private void OnSheetsTaskDeleted(SheetsTaskDeletedMessage msg)
        {
            var existingIndex = AppCommand.SheetsData.sheets.FindIndex(x => x.identifier == msg.Identifier);
            if (existingIndex == -1) return;

            foreach (var id in msg.Deleted)
            {
                // (Konrad) Update SheetsData stored in AppCommand
                var taskIndex = AppCommand.SheetsData.sheets[existingIndex].tasks.FindIndex(x => x.Id == id);
                if(taskIndex == -1) continue;

                AppCommand.SheetsData.sheets[existingIndex].tasks.RemoveAt(taskIndex);

                // (Konrad) Update Tasks collection in UI
                var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(SheetTaskWrapper) && ((SheetTask)x.Task).Id == id);
                if (storedTask == null) continue;

                lock (_lock)
                {
                    Tasks.Remove(storedTask);

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

        /// <summary>
        /// Handles Sheet Task being updated.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnSheetTaskUpdated(SheetsTaskUpdatedMessage msg)
        {
            var existingIndex = AppCommand.SheetsData.sheets.IndexOf(msg.Sheet);
            if (existingIndex == -1) return;
            var existingTaskIndex = AppCommand.SheetsData.sheets[existingIndex].tasks.FindIndex(x => x.Id == msg.Task.Id);
            if (existingTaskIndex == -1)
            {
                if (string.Equals(msg.Task.assignedTo.ToLower(), Environment.UserName.ToLower(),StringComparison.CurrentCultureIgnoreCase))
                {
                    lock (_lock)
                    {
                        Tasks.Add(new SheetTaskWrapper(msg.Sheet, msg.Task)); // task wasn't ours but was re-assigned to us
                    }
                    AppCommand.SheetsData.sheets[existingIndex].tasks.Add(msg.Task);
                }
                return;
            }

            var existingTask = AppCommand.SheetsData.sheets[existingIndex].tasks[existingTaskIndex];
            if (!string.Equals(existingTask.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase))
            {
                if (!string.Equals(msg.Task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase)) return; // it still doesn't belong to us.
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

                    AppCommand.SheetsData.sheets[existingIndex].tasks[existingTaskIndex] = msg.Task;
                    return;
                }

                if (!string.Equals(msg.Task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase))
                {
                    lock (_lock)
                    {
                        Tasks.Remove(storedTask); // it was assigned away from us

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
                else
                {
                    lock (_lock)
                    {
                        var task = storedTask.Task as SheetTask;
                        if (task == null) return;

                        task.CopyProperties(msg.Task); // it's still ours, update it

                        // (Konrad) We only get here when user hits "Approve"
                        if (!string.IsNullOrEmpty(task.completedBy))
                        {
                            Tasks.Remove(storedTask);

                            // close task assistant
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

            // replace existing task
            AppCommand.SheetsData.sheets[existingIndex].tasks[existingTaskIndex] = msg.Task;
        }

        /// <summary>
        /// Handles Sheet Task being added.
        /// </summary>
        /// <param name="msg">Message from Mission Control.</param>
        private void OnSheetTaskAdded(SheetsTaskAddedMessage msg)
        {
            var existingIndex = AppCommand.SheetsData.sheets.IndexOf(msg.Sheet);
            if (existingIndex != -1)
            {
                // Update existing sheet
                AppCommand.SheetsData.sheets[existingIndex].tasks.Add(msg.Task);

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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="identifier"></param>
        ///// <param name="sheet"></param>
        //private void DeleteTask(string identifier, SheetItem sheet = null)
        //{
        //    // (Konrad) Update AppCommand sheetsChanges
        //    var approvedItem = AppCommand.SheetsData.sheetsChanges.FirstOrDefault(x => x.identifier == identifier);
        //    if (approvedItem != null)
        //    {
        //        var index = AppCommand.SheetsData.sheetsChanges.IndexOf(approvedItem);
        //        if (index != -1) AppCommand.SheetsData.sheetsChanges.RemoveAt(index);
        //    }
        //    else
        //    {
        //        // (Konrad) In this case the task approved was a new sheet task. It doesn't have identifier. 
        //        // We are also using for loop here because SheetItem class compares two items using the identifier. 
        //        // It would return true for all new sheets since they have no identifier.
        //        var foundIndex = -1;
        //        for (var i = 0; i < AppCommand.SheetsData.sheetsChanges.Count; i++)
        //        {
        //            var item = AppCommand.SheetsData.sheetsChanges[i];
        //            if (item.identifier == "" && item.name == sheet?.name && item.number == sheet?.number)
        //            {
        //                foundIndex = i;
        //                break;
        //            }
        //        }
        //        if (foundIndex != -1)
        //        {
        //            AppCommand.SheetsData.sheetsChanges.RemoveAt(foundIndex);
        //        }
        //    }

        //    // (Konrad) Update Tasks UI
        //    var storedTask = Tasks.FirstOrDefault(x => x.GetType() == typeof(SheetTaskWrapper) && ((SheetItem)x.Task).identifier == identifier);
        //    if (storedTask != null)
        //    {
        //        var taskIndex = Tasks.IndexOf(storedTask);
        //        lock (_lock)
        //        {
        //            Tasks.RemoveAt(taskIndex);
        //        }

        //        // (Konrad) If task is open let's close it 
        //        if (SelectedTask == storedTask)
        //        {
        //            Control.Dispatcher.Invoke(() =>
        //            {
        //                Model.LaunchTaskAssistant(null);

        //            }, DispatcherPriority.Normal);
        //        }
        //    }
        //    else
        //    {
        //        // (Konrad) Same as above. If new sheet task was approved it will not have identifier on it.
        //        var storedIndex = -1;
        //        for (var i = 0; i < Tasks.Count; i++)
        //        {
        //            var task = (SheetItem)Tasks[i].Task;
        //            if (task.identifier == "" && task.name == sheet?.name && task.number == sheet?.number)
        //            {
        //                storedIndex = i;
        //                break;
        //            }
        //        }
        //        if (storedIndex != -1)
        //        {
        //            lock (_lock)
        //            {
        //                Tasks.RemoveAt(storedIndex);
        //            }
        //        }

        //        // (Konrad) If task is open let's close it 
        //        var selected = SelectedTask?.Task as SheetItem;
        //        if (selected?.identifier == "" && selected.name == sheet?.name && selected.number == sheet?.number)
        //        {
        //            Control.Dispatcher.Invoke(() =>
        //            {
        //                Model.LaunchTaskAssistant(null);

        //            }, DispatcherPriority.Normal);
        //        }
        //    }
        //}

        #endregion
    }
}
