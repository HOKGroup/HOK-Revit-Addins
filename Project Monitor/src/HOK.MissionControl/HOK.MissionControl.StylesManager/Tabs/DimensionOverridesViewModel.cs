#region References

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.WpfUtilities;
using HOK.Core.WpfUtilities.FeedbackUI;
using HOK.MissionControl.StylesManager.Utilities;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

#endregion

namespace HOK.MissionControl.StylesManager.Tabs
{
    public class DimensionOverridesViewModel : ViewModelBase
    {
        #region Properties

        public DimensionOverridesModel Model { get; set; }
        private readonly object _lock = new object();
        public IList SelectedRows { get; set; }
        public RelayCommand SelectAll { get; set; }
        public RelayCommand SelectNone { get; set; }
        public RelayCommand ClearOverride { get; set; }
        public RelayCommand<UserControl> Close { get; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand<bool> Check { get; set; }
        public RelayCommand<bool> ShowFiltered { get; set; }
        public RelayCommand<UserControl> ControlClosed { get; set; }
        public RelayCommand<DimensionWrapper> FindDimension { get; set; }

        private ObservableCollection<DimensionWrapper> _dimensionOverrides;
        public ObservableCollection<DimensionWrapper> DimensionOverrides
        {
            get { return _dimensionOverrides; }
            set { _dimensionOverrides = value; RaisePropertyChanged(() => DimensionOverrides); }
        }

        #endregion

        public DimensionOverridesViewModel(DimensionOverridesModel model)
        {
            Model = model;
            DimensionOverrides = Model.CollectDimensionOverrides();
            BindingOperations.EnableCollectionSynchronization(_dimensionOverrides, _lock);

            SelectAll = new RelayCommand(OnSelectAll);
            SelectNone = new RelayCommand(OnSelectNone);
            ClearOverride = new RelayCommand(OnClearOverride);
            Close = new RelayCommand<UserControl>(OnClose);
            SubmitComment = new RelayCommand(OnSubmitComment);
            Check = new RelayCommand<bool>(OnCheck);
            ShowFiltered = new RelayCommand<bool>(OnShowFiltered);
            FindDimension = new RelayCommand<DimensionWrapper>(OnFindDimension);
            ControlClosed = new RelayCommand<UserControl>(OnControlClosed);

            Messenger.Default.Register<OverridesCleared>(this, OnOverridesCleared);
        }

        #region Message Handlers

        private void OnOverridesCleared(OverridesCleared msg)
        {
            lock (_lock)
            {
                foreach (var i in msg.Dimensions)
                {
                    DimensionOverrides.Remove(i);
                }
            }
        }

        #endregion

        #region Command Handlers

        private static void OnFindDimension(DimensionWrapper dw)
        {
            if (dw == null) return;

            AppCommand.StylesManagerHandler.Arg1 = dw;
            AppCommand.StylesManagerHandler.Request.Make(StylesRequestType.FindView);
            AppCommand.StylesManagerEvent.Raise();
        }

        private void OnClearOverride()
        {
            var selected = (from DimensionWrapper s in DimensionOverrides where s.IsSelected select s).ToList();
            if (!selected.Any())
            {
                StatusBarManager.StatusLabel.Text = "Please select at least one (1) Dimension.";
                return;
            }

            AppCommand.StylesManagerHandler.Arg1 = selected;
            AppCommand.StylesManagerHandler.Request.Make(StylesRequestType.ClearOverrides);
            AppCommand.StylesManagerEvent.Raise();
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

        private void OnShowFiltered(bool isChecked)
        {
            foreach (var wrapper in DimensionOverrides)
            {
                if(!wrapper.IsFiltered) continue;
                
                wrapper.IsVisible = isChecked;
            }
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

        private static void OnClose(UserControl control)
        {
            var win = Window.GetWindow(control);
            win?.Close();
        }

        private void OnControlClosed(UserControl control)
        {
            // (Konrad) Unregisters any Messanger handlers.
            Cleanup();
        }

        #endregion
    }
}
