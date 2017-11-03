using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Tools.Communicator.Tasks.FamilyTaskAssistant;
using HOK.MissionControl.Tools.Communicator.Tasks.SheetTaskAssistant;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class CommunicatorTasksModel
    {
        private FamilyData FamiliesData { get; }
        private SheetData SheetsData { get; }
        public object TaskView { get; set; }

        public CommunicatorTasksModel(FamilyData famStat = null, SheetData sheetData = null)
        {
            FamiliesData = famStat;
            SheetsData = sheetData;
        }

        /// <summary>
        /// Launches the Task Assistant for the appropriate Task.
        /// </summary>
        /// <param name="wrapper">Task Wrapper object.</param>
        public void LaunchTaskAssistant(TaskWrapper wrapper)
        {
            // (Konrad) In case that someone deletes the task using the web interface
            // this will trigger with a null value. If the window is open let's close it and reset.
            if (wrapper == null)
            {
                if (TaskView == null) return;

                ((Window)TaskView).Close();
                TaskView = null;
                return;
            }

            // (Konrad) Sort out which task has shown up
            if (wrapper.GetType() == typeof(FamilyTaskWrapper))
            {
                var familyTaskWrapper = (FamilyTaskWrapper) wrapper;
                var viewModel = new FamilyTaskAssistantViewModel(familyTaskWrapper);
                if (TaskView != null)
                {
                    var view = TaskView as FamilyTaskAssistantView;
                    if (view != null)
                    {
                        view.DataContext = viewModel;
                        view.Activate();
                    }
                    else
                    {
                        // (Konrad) Currently open Window is not of this type. Let's close it.
                        ((Window)TaskView).Close();

                        // (Konrad) Let's open a new one
                        TaskView = new FamilyTaskAssistantView
                        {
                            DataContext = viewModel
                        };
                        var unused = new WindowInteropHelper((FamilyTaskAssistantView)TaskView)
                        {
                            Owner = Process.GetCurrentProcess().MainWindowHandle
                        };

                        ((FamilyTaskAssistantView)TaskView).Show();
                    }
                }
                else
                {
                    TaskView = new FamilyTaskAssistantView
                    {
                        DataContext = viewModel
                    };
                    var unused = new WindowInteropHelper((FamilyTaskAssistantView)TaskView)
                    {
                        Owner = Process.GetCurrentProcess().MainWindowHandle
                    };

                    ((FamilyTaskAssistantView)TaskView).Show();
                }
            }
            else if (wrapper.GetType() == typeof(SheetTaskWrapper))
            {
                var sheetTaskWrapper = (SheetTaskWrapper) wrapper;
                var viewModel = new SheetTaskAssistantViewModel(sheetTaskWrapper);
                if (TaskView != null)
                {
                    var view = TaskView as SheetTaskAssistantView;
                    if (view != null)
                    {
                        view.DataContext = viewModel;
                        view.Activate();
                    }
                    else
                    {
                        // (Konrad) Currently open Window is not of this type. Let's close it.
                        ((Window)TaskView).Close();

                        // (Konrad) Let's open a new one
                        TaskView = new SheetTaskAssistantView
                        {
                            DataContext = viewModel
                        };
                        var unused = new WindowInteropHelper((SheetTaskAssistantView)TaskView)
                        {
                            Owner = Process.GetCurrentProcess().MainWindowHandle
                        };

                        ((SheetTaskAssistantView)TaskView).Show();
                    }
                }
                else
                {
                    // (Konrad) Let's open a new one
                    TaskView = new SheetTaskAssistantView
                    {
                        DataContext = viewModel
                    };
                    var unused = new WindowInteropHelper((SheetTaskAssistantView)TaskView)
                    {
                        Owner = Process.GetCurrentProcess().MainWindowHandle
                    };

                    ((SheetTaskAssistantView)TaskView).Show();
                }
            }
        }

        /// <summary>
        /// Collects all tasks (Sheets, Families) to be displayed.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<TaskWrapper> GetTasks()
        {
            var tasks = new ObservableCollection<TaskWrapper>();

            if (FamiliesData != null)
            {
                var familiesToWatch = new Dictionary<string, FamilyItem>();
                //var familyTasks = new List<FamilyTaskWrapper>();
                foreach (var family in FamiliesData.families)
                {
                    // (Konrad) We are storing this Family Item for later so that if user makes any changes to this family
                    // and reloads it back into the model, we can capture that and post to MongoDB at the end of the session.
                    if (!familiesToWatch.ContainsKey(family.name)) familiesToWatch.Add(family.name, family);

                    if (!family.tasks.Any()) continue;
                    foreach (var task in family.tasks)
                    {
                        if (!string.IsNullOrEmpty(task.completedBy)) continue;
                        if (!string.Equals(task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase)) continue;

                        tasks.Add(new FamilyTaskWrapper(family, task));
                    }
                }

                AppCommand.FamiliesToWatch = familiesToWatch;
            }

            if (SheetsData != null)
            {
                var sheetsToEdit = SheetsData.sheetsChanges;
                if (!sheetsToEdit.Any()) return tasks;

                foreach (var sheetTask in sheetsToEdit)
                {
                    if (!string.Equals(sheetTask.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase)) continue;

                    // (Konrad) Let's match proposed sheet to a sheet existing in the model
                    var sheetItem = AppCommand.SheetsData.sheets.FirstOrDefault(x => x.uniqueId == sheetTask.uniqueId);

                    tasks.Add(new SheetTaskWrapper(sheetTask, sheetItem));
                }
            }

            return tasks;
        }
    }
}
