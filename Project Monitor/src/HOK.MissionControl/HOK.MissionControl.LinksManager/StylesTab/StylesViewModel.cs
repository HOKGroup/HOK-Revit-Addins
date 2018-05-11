#region References
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.ElementWrapers;
using HOK.Core.WpfUtilities.FeedbackUI;
#endregion

namespace HOK.MissionControl.LinksManager.StylesTab
{
    public class StylesViewModel : ViewModelBase
    {
        public IList SelectedRows { get; set; }
        public StylesModel Model { get; set; }
        public ListCollectionView Styles { get; set; }
        public RelayCommand Delete { get; }
        public RelayCommand<UserControl> Close { get; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand SelectAll { get; set; }
        public RelayCommand SelectNone { get; set; }
        public RelayCommand<bool> Check { get; set; }

        private bool _expanded;
        public bool Expanded
        {
            get { return _expanded; }
            set { _expanded = value; RaisePropertyChanged(() => Expanded); }
        }

        public StylesViewModel(StylesModel model)
        {
            Model = model;
            Styles = new ListCollectionView(Model.Styles);
            Styles.GroupDescriptions.Add(new PropertyGroupDescription("ParentName"));
            Expanded = true;

            Delete = new RelayCommand(OnDelete);
            Close = new RelayCommand<UserControl>(OnClose);
            SubmitComment = new RelayCommand(OnSubmitComment);
            SelectAll = new RelayCommand(OnSelectAll);
            SelectNone = new RelayCommand(OnSelectNone);
            Check = new RelayCommand<bool>(OnCheck);
        }

        private void OnSelectNone()
        {
            foreach (var wrapper in Styles.SourceCollection)
            {
                ((CategoryWrapper)wrapper).IsSelected = false;
            }
        }

        private void OnSelectAll()
        {
            foreach (var wrapper in Styles)
            {
                ((CategoryWrapper)wrapper).IsSelected = true;
            }
        }

        private void OnCheck(bool isChecked)
        {
            var selected = SelectedRows.Cast<CategoryWrapper>().ToList();
            if (!selected.Any()) return;

            foreach (var wrapper in selected)
            {
                wrapper.IsSelected = isChecked;
            }
        }

        private static void OnSubmitComment()
        {
            var title = "Mission Control - Links Manager v." + Assembly.GetExecutingAssembly().GetName().Version;
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

        private void OnDelete()
        {
            var selected = (from CategoryWrapper s in Styles.SourceCollection where s.IsSelected select s).ToList();
            var deleted = Model.Delete(selected);
            foreach (var i in deleted)
            {
                Styles.Remove(i);
            }
        }

        private static void OnClose(UserControl control)
        {
            var win = Window.GetWindow(control);
            win?.Close();
        }
    }
}
