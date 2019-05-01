#region References

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.WpfUtilities;
using HOK.Feedback;
using HOK.MissionControl.StylesManager.Tabs;
using HOK.MissionControl.StylesManager.Utilities;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

#endregion

namespace HOK.MissionControl.StylesManager.DimensionsTab
{
    public class DimensionsViewModel : ViewModelBase
    {
        #region Properties

        public DimensionsModel Model { get; set; }
        public RelayCommand Replace { get; set; }
        public RelayCommand<UserControl> Close { get; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand<bool> CheckDimension { get; set; }
        public RelayCommand<bool> CheckReplacementDimension { get; set; }
        public RelayCommand<UserControl> ControlClosed { get; set; }

        private ObservableCollection<DimensionTypeWrapper> _dimensions;
        public ObservableCollection<DimensionTypeWrapper> Dimensions
        {
            get { return _dimensions; }
            set { _dimensions = value; RaisePropertyChanged(() => Dimensions); }
        }

        private ObservableCollection<DimensionTypeWrapper> _replacementDimensions;
        public ObservableCollection<DimensionTypeWrapper> ReplacementDimensions
        {
            get { return _replacementDimensions; }
            set { _replacementDimensions = value; RaisePropertyChanged(() => ReplacementDimensions); }
        }

        #endregion

        public DimensionsViewModel(DimensionsModel model)
        {
            Model = model;
            Dimensions = Model.CollectDimensions();
            ReplacementDimensions = Model.CollectDimensions();

            Replace = new RelayCommand(OnReplace);
            Close = new RelayCommand<UserControl>(OnClose);
            SubmitComment = new RelayCommand(OnSubmitComment);
            CheckDimension = new RelayCommand<bool>(OnCheckDimension);
            CheckReplacementDimension = new RelayCommand<bool>(OnCheckReplacementDimension);
            ControlClosed = new RelayCommand<UserControl>(OnControlClosed);

            Messenger.Default.Register<DimensionsDeleted>(this, OnDimensionsDeleted);
        }

        #region Message Handlers

        private void OnDimensionsDeleted(DimensionsDeleted msg)
        {
            Dimensions = Model.CollectDimensions();
            ReplacementDimensions = Model.CollectDimensions();

            StatusBarManager.StatusLabel.Text = "Successfully replaced (" + msg.Dimensions.Count + ") Dimension Types.";
        }

        #endregion

        #region Command Handlers

        private void OnReplace()
        {
            var dims = Dimensions.Where(x => x.IsSelected).ToList();
            var replacement = ReplacementDimensions.FirstOrDefault(x => x.IsSelected);
            if (!dims.Any() || replacement == null)
            {
                StatusBarManager.StatusLabel.Text = "Please select at least one (1) Dimension Type and Replacement Dimension Type.";
                return;
            }

            AppCommand.StylesManagerHandler.Arg1 = dims;
            AppCommand.StylesManagerHandler.Arg2 = replacement;
            AppCommand.StylesManagerHandler.Request.Make(StylesRequestType.ReplaceDimensionTypes);
            AppCommand.StylesManagerEvent.Raise();
        }

        private void OnCheckDimension(bool isChecked)
        {
            var selectedType = Dimensions.FirstOrDefault(x => x.IsSelected)?.Type;
            foreach (var d in Dimensions)
            {
                // nothing selected let's enable everything
                if (string.IsNullOrEmpty(selectedType))
                {
                    d.IsEnabled = true;
                    continue;
                }

                // only dimensions that match the type selected
                // should be enabled here
                d.IsEnabled = d.Type == selectedType;
            }

            var replacementSelected = ReplacementDimensions.Count(x => x.IsSelected) > 0;
            foreach (var d in ReplacementDimensions)
            {
                if (string.IsNullOrEmpty(selectedType))
                {
                    d.IsEnabled = true;
                    continue;
                }

                if (replacementSelected)
                {
                    d.IsEnabled = d.IsSelected;
                    continue;
                }

                d.IsEnabled = d.Type == selectedType &&
                              !Dimensions.Where(x => x.IsSelected).Contains(d);
            }

            // if none of the types are selected let's clean selection
            // in replacement table as well
            if (Dimensions.Any(x => x.IsSelected)) return;
            foreach (var d in ReplacementDimensions)
            {
                d.IsSelected = false;
            }
        }

        private void OnCheckReplacementDimension(bool isChecked)
        {
            var selectedType = Dimensions.FirstOrDefault(x => x.IsSelected)?.Type;
            foreach (var d in ReplacementDimensions)
            {
                if (string.IsNullOrEmpty(selectedType))
                {
                    d.IsEnabled = true;
                    continue;
                }

                if (isChecked)
                {
                    d.IsEnabled = d.IsSelected;
                }
                else
                {
                    d.IsEnabled = d.Type == selectedType && 
                                  !Dimensions.Where(x => x.IsSelected).Contains(d);
                }
            }
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

        private static void OnClose(UserControl control)
        {
            var win = Window.GetWindow(control);
            win?.Close();
        }

        private void OnControlClosed(UserControl control)
        {
            // (Konrad) Unregistered any Messenger handlers.
            Cleanup();
        }

        #endregion

    }
}
