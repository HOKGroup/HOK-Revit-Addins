#region References

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.WpfUtilities.FeedbackUI;
using HOK.MissionControl.StylesManager.Tabs;

#endregion

namespace HOK.MissionControl.StylesManager.DimensionsTab
{
    public class DimensionsViewModel : ViewModelBase
    {
        public DimensionsModel Model { get; set; }
        public ObservableCollection<DimensionTypeWrapper> Dimensions { get; set; }
        public IList SelectedRows { get; set; }
        public RelayCommand SelectAll { get; set; }
        public RelayCommand SelectNone { get; set; }
        public RelayCommand Delete { get; }
        public RelayCommand<UserControl> Close { get; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand<bool> Check { get; set; }

        public DimensionsViewModel(DimensionsModel model)
        {
            Model = model;
            SelectAll = new RelayCommand(OnSelectAll);
            SelectNone = new RelayCommand(OnSelectNone);
            Delete = new RelayCommand(OnDelete);
            Close = new RelayCommand<UserControl>(OnClose);
            SubmitComment = new RelayCommand(OnSubmitComment);
            Check = new RelayCommand<bool>(OnCheck);

            Dimensions = Model.CollectDimensions();
        }

        private void OnDelete()
        {
            var selected = (from DimensionTypeWrapper s in Dimensions where s.IsSelected select s).ToList();
            //var deleted = Model.Delete(selected);

            //foreach (var i in deleted)
            //{
            //    Images.Remove(i);
            //}
        }

        private static void OnSubmitComment()
        {
            var title = "Mission Control - Styles Manager v." + Assembly.GetExecutingAssembly().GetName().Version;
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

        private void OnCheck(bool isChecked)
        {
            var selected = SelectedRows.Cast<DimensionTypeWrapper>().ToList();
            if (!selected.Any()) return;

            foreach (var wrapper in selected)
            {
                wrapper.IsSelected = isChecked;
            }
        }

        private static void OnClose(UserControl control)
        {
            var win = Window.GetWindow(control);
            win?.Close();
        }

        private void OnSelectNone()
        {
            foreach (var wrapper in Dimensions)
            {
                wrapper.IsSelected = false;
            }
        }

        private void OnSelectAll()
        {
            foreach (var wrapper in Dimensions)
            {
                wrapper.IsSelected = true;
            }
        }
    }
}
