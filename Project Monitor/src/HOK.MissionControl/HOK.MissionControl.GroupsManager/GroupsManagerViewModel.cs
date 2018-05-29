#region References

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.WpfUtilities.FeedbackUI;
using HOK.MissionControl.GroupsManager.Utilities;

#endregion

namespace HOK.MissionControl.GroupsManager
{
    public class GroupsManagerViewModel : ViewModelBase
    {
        #region Properties

        private GroupsManagerModel Model { get; set; }
        public string Title { get; set; }
        public ObservableCollection<GroupTypeWrapper> Groups { get; set; }
        public IList SelectedRows { get; set; }
        public RelayCommand<Window> WindowLoaded { get; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand<Window> Close { get; set; }
        public RelayCommand Delete { get; set; }
        public RelayCommand SelectAll { get; set; }
        public RelayCommand SelectNone { get; set; }
        public RelayCommand<bool> Check { get; set; }

        #endregion

        public GroupsManagerViewModel(GroupsManagerModel model)
        {
            Model = model;
            Groups = Model.CollectGroups();
            Title = "Mission Control - Groups Manager v." + Assembly.GetExecutingAssembly().GetName().Version;

            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            SubmitComment = new RelayCommand(OnSubmitComment);
            Close = new RelayCommand<Window>(OnClose);
            Delete = new RelayCommand(OnDelete);
            SelectAll = new RelayCommand(OnSelectAll);
            SelectNone = new RelayCommand(OnSelectNone);
            Check = new RelayCommand<bool>(OnCheck);
        }

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

        private static void OnDelete()
        {
            throw new System.NotImplementedException();
        }

        private static void OnWindowLoaded(Window win)
        {
            HOK.Core.WpfUtilities.StatusBarManager.ProgressBar = ((GroupsManagerView)win).progressBar;
            HOK.Core.WpfUtilities.StatusBarManager.StatusLabel = ((GroupsManagerView)win).statusLabel;
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
