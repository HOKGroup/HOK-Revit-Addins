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

#endregion

namespace HOK.MissionControl.StylesManager.Tabs
{
    public class DimensionOverridesViewModel : ViewModelBase
    {
        #region Properties

        public DimensionOverridesModel Model { get; set; }
        public ObservableCollection<DimensionWrapper> DimensionOverrides { get; set; }
        public IList SelectedRows { get; set; }
        public RelayCommand SelectAll { get; set; }
        public RelayCommand SelectNone { get; set; }
        public RelayCommand Delete { get; }
        public RelayCommand<UserControl> Close { get; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand<bool> Check { get; set; }
        public RelayCommand<DimensionWrapper> FindDimension { get; set; }

        #endregion

        public DimensionOverridesViewModel(DimensionOverridesModel model)
        {
            Model = model;
            SelectAll = new RelayCommand(OnSelectAll);
            SelectNone = new RelayCommand(OnSelectNone);
            Delete = new RelayCommand(OnDelete);
            Close = new RelayCommand<UserControl>(OnClose);
            SubmitComment = new RelayCommand(OnSubmitComment);
            Check = new RelayCommand<bool>(OnCheck);
            FindDimension = new RelayCommand<DimensionWrapper>(OnFindDimension);

            DimensionOverrides = Model.CollectDimensionOverrides();
        }

        private static void OnFindDimension(DimensionWrapper dw)
        {
            if (dw == null) return;

            AppCommand.StylesManagerHandler.Arg1 = dw;
            AppCommand.StylesManagerHandler.Request.Make(StylesRequestType.FindView);
            AppCommand.StylesManagerEvent.Raise();
        }

        private void OnDelete()
        {
            var selected = (from DimensionWrapper s in DimensionOverrides where s.IsSelected select s).ToList();
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
            var selected = SelectedRows.Cast<DimensionWrapper>().ToList();
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
            foreach (var wrapper in DimensionOverrides)
            {
                wrapper.IsSelected = false;
            }
        }

        private void OnSelectAll()
        {
            foreach (var wrapper in DimensionOverrides)
            {
                wrapper.IsSelected = true;
            }
        }
    }
}
