#region References

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.WpfUtilities.FeedbackUI;
using HOK.MissionControl.GroupsManager.Utilities;

#endregion

namespace HOK.MissionControl.GroupsManager
{
    public class GroupsManagerViewModel : ViewModelBase
    {
        #region Properties

        private GroupsManagerModel Model { get; }
        private readonly object _lock = new object();
        public string Title { get; set; }
        public IList SelectedRows { get; set; }
        public RelayCommand<Window> WindowLoaded { get; }
        public RelayCommand<Window> WindowClosed { get; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand<Window> Close { get; set; }
        public RelayCommand Delete { get; set; }
        public RelayCommand Ungroup { get; set; }
        public RelayCommand SelectAll { get; set; }
        public RelayCommand SelectNone { get; set; }
        public RelayCommand<bool> Check { get; set; }
        public RelayCommand<GroupTypeWrapper> FindGroup { get; set; }

        private ObservableCollection<GroupTypeWrapper> _groups;
        public ObservableCollection<GroupTypeWrapper> Groups
        {
            get { return _groups;}
            set { _groups = value; RaisePropertyChanged(() => Groups); }
        }

        #endregion

        public GroupsManagerViewModel(GroupsManagerModel model)
        {
            Model = model;
            Groups = Model.CollectGroups();
            Title = "Mission Control - Groups Manager v." + Assembly.GetExecutingAssembly().GetName().Version;
            BindingOperations.EnableCollectionSynchronization(_groups, _lock);

            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            WindowClosed = new RelayCommand<Window>(OnWindowClosed);
            SubmitComment = new RelayCommand(OnSubmitComment);
            Close = new RelayCommand<Window>(OnClose);
            Delete = new RelayCommand(OnDelete);
            Ungroup = new RelayCommand(OnUngroup);
            SelectAll = new RelayCommand(OnSelectAll);
            SelectNone = new RelayCommand(OnSelectNone);
            Check = new RelayCommand<bool>(OnCheck);
            FindGroup = new RelayCommand<GroupTypeWrapper>(OnFindGroup);

            Messenger.Default.Register<GroupsDeleted>(this, OnGroupsDeleted);
        }

        #region Message Handlers

        private void OnGroupsDeleted(GroupsDeleted msg)
        {
            lock (_lock)
            {
                foreach (var i in msg.Groups)
                {
                    Groups.Remove(i);
                }
            }
        }

        #endregion

        #region Command Handlers

        private void OnSelectNone()
        {
            foreach (var wrapper in Groups)
            {
                wrapper.IsSelected = false;
            }
        }

        private void OnSelectAll()
        {
            foreach (var wrapper in Groups)
            {
                wrapper.IsSelected = true;
            }
        }

        private void OnCheck(bool isChecked)
        {
            var selected = SelectedRows.Cast<GroupTypeWrapper>().ToList();
            if (!selected.Any()) return;

            foreach (var wrapper in selected)
            {
                wrapper.IsSelected = isChecked;
            }
        }

        private void OnDelete()
        {
            var selected = (from GroupTypeWrapper s in Groups where s.IsSelected select s).ToList();

            AppCommand.GroupManagerHandler.Arg1 = selected;
            AppCommand.GroupManagerHandler.Request.Make(GroupRequestType.Delete);
            AppCommand.GroupManagerEvent.Raise();
        }

        private void OnUngroup()
        {
            var selected = (from GroupTypeWrapper s in Groups where s.IsSelected select s).ToList();

            AppCommand.GroupManagerHandler.Arg1 = selected;
            AppCommand.GroupManagerHandler.Request.Make(GroupRequestType.Ungroup);
            AppCommand.GroupManagerEvent.Raise();
        }

        private static void OnFindGroup(GroupTypeWrapper gt)
        {
            if (gt == null) return;

            AppCommand.GroupManagerHandler.Arg1 = gt;
            AppCommand.GroupManagerHandler.Request.Make(GroupRequestType.FindView);
            AppCommand.GroupManagerEvent.Raise();
        }

        private static void OnWindowLoaded(Window win)
        {
            HOK.Core.WpfUtilities.StatusBarManager.ProgressBar = ((GroupsManagerView)win).progressBar;
            HOK.Core.WpfUtilities.StatusBarManager.StatusLabel = ((GroupsManagerView)win).statusLabel;
        }

        private void OnWindowClosed(Window win)
        {
            // (Konrad) Unregisters any Messanger handlers.
            Cleanup();
        }

        private static void OnSubmitComment()
        {
            var title = "Mission Control - Groups Manager v." + Assembly.GetExecutingAssembly().GetName().Version;
            var model = new FeedbackModel();
            var viewModel = new FeedbackViewModel(model, title);
            var view = new FeedbackView
            {
                DataContext = viewModel
            };

            var unused = new WindowInteropHelper(view)
            {
                Owner = Process.GetCurrentProcess().MainWindowHandle
            };

            view.ShowDialog();
        }

        private static void OnClose(Window win)
        {
            win?.Close();
        }

        #endregion
    }
}
