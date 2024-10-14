#region References

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HOK.Core.WpfUtilities;
using HOK.Feedback;
using HOK.MissionControl.StylesManager.Utilities;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

#endregion

namespace HOK.MissionControl.StylesManager.Tabs
{
    public class TextViewModel : ObservableRecipient
    {
        public TextModel Model { get; set; }
        public RelayCommand Replace { get; set; }
        public RelayCommand<UserControl> Close { get; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand<bool> CheckStyle { get; set; }
        public RelayCommand<bool> CheckReplacementStyle { get; set; }
        public RelayCommand<UserControl> ControlClosed { get; set; }

        private ObservableCollection<TextStyleWrapper> _textStyles;
        public ObservableCollection<TextStyleWrapper> TextStyles
        {
            get { return _textStyles; }
            set { _textStyles = value; OnPropertyChanged(nameof(TextStyles)); }
        }

        private ObservableCollection<TextStyleWrapper> _replacementTextStyles;
        public ObservableCollection<TextStyleWrapper> ReplacementTextStyles
        {
            get { return _replacementTextStyles; }
            set { _replacementTextStyles = value; OnPropertyChanged(nameof(ReplacementTextStyles)); }
        }

        public TextViewModel(TextModel model)
        {
            Model = model;
            TextStyles = Model.CollectTextStyles();
            ReplacementTextStyles = Model.CollectTextStyles();

            Replace = new RelayCommand(OnReplace);
            Close = new RelayCommand<UserControl>(OnClose);
            SubmitComment = new RelayCommand(OnSubmitComment);
            CheckStyle = new RelayCommand<bool>(OnCheckStyle);
            CheckReplacementStyle = new RelayCommand<bool>(OnCheckReplacementStyle);
            ControlClosed = new RelayCommand<UserControl>(OnControlClosed);

            WeakReferenceMessenger.Default.Register<TextViewModel, TextStylesDeleted>(this, static (r, m) => r.OnTextStylesDeleted(m));
        }

        #region Message Handlers

        private void OnTextStylesDeleted(TextStylesDeleted msg)
        {
            TextStyles = Model.CollectTextStyles();
            ReplacementTextStyles = Model.CollectTextStyles();

            StatusBarManager.StatusLabel.Text = "Successfully replaced (" + msg.TextStyles.Count + ") Text Styles.";
        }

        #endregion

        #region Command Handlers

        private void OnReplace()
        {
            var styles = TextStyles.Where(x => x.IsSelected).ToList();
            var replacement = ReplacementTextStyles.FirstOrDefault(x => x.IsSelected);
            if (!styles.Any() || replacement == null)
            {
                StatusBarManager.StatusLabel.Text = "Please select at least one (1) Dimension Type and Replacement Dimension Type.";
                return;
            }

            AppCommand.StylesManagerHandler.Arg1 = styles;
            AppCommand.StylesManagerHandler.Arg2 = replacement;
            AppCommand.StylesManagerHandler.Request.Make(StylesRequestType.ReplaceTextStyles);
            AppCommand.StylesManagerEvent.Raise();
        }

        private void OnCheckStyle(bool isChecked)
        {
            var replacementSelected = ReplacementTextStyles.Count(x => x.IsSelected) > 0;
            foreach (var d in ReplacementTextStyles)
            {
                if (replacementSelected)
                {
                    d.IsEnabled = d.IsSelected;
                    continue;
                }

                d.IsEnabled = !TextStyles.Where(x => x.IsSelected).Contains(d);
            }

            // if none of the types are selected let's clean selection
            // in replacement table as well
            if (TextStyles.Any(x => x.IsSelected)) return;
            foreach (var d in ReplacementTextStyles)
            {
                d.IsSelected = false;
            }
        }

        private void OnCheckReplacementStyle(bool isChecked)
        {
            foreach (var d in ReplacementTextStyles)
            {
                if (isChecked)
                {
                    d.IsEnabled = d.IsSelected;
                }
                else
                {
                    d.IsEnabled = !TextStyles.Where(x => x.IsSelected).Contains(d);
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
            // (Konrad) Unregistered any WeakReferenceMessenger handlers.
            OnDeactivated();
        }

        #endregion
    }
}
