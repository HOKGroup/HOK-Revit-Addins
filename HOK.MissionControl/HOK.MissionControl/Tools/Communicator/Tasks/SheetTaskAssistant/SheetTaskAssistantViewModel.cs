#region References
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Tools.Communicator.Messaging;
#endregion

namespace HOK.MissionControl.Tools.Communicator.Tasks.SheetTaskAssistant
{
    public class SheetTaskAssistantViewModel: ObservableRecipient
    {
        public string Title { get; set; }
        public SheetTaskAssistantModel Model { get; set; }
        public TextBlock Control { get; set; }
        public Window Win { get; set; }
        public RelayCommand OpenView { get; set; }
        public RelayCommand<Window> Close { get; set; }
        public RelayCommand<Window> Approve { get; set; }
        public RelayCommand<Window> WindowLoaded { get; set; }
        public RelayCommand<Window> WindowClosed { get; set; }

        public SheetTaskAssistantViewModel(SheetTaskWrapper wrapper)
        {
            Model = new SheetTaskAssistantModel();
            Wrapper = wrapper;
            Title = "Mission Control - Sheet Task Assistant v." + Assembly.GetExecutingAssembly().GetName().Version;
            OkText = ((SheetItem)wrapper.Element).IsNewSheet ? "Create" : "Approve";

            OpenView = new RelayCommand(OnOpenView);
            Close = new RelayCommand<Window>(OnClose);
            Approve = new RelayCommand<Window>(OnApprove);
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            WindowClosed = new RelayCommand<Window>(OnWindowClosed);

            // (Konrad) Event handlers for when Revit finishes approving a sheet task, or when document is closed.
            WeakReferenceMessenger.Default.Register<SheetTaskAssistantViewModel, SheetTaskCompletedMessage>(this, static (r, m) => r.OnSheetTaskCompleted(m));
            WeakReferenceMessenger.Default.Register<SheetTaskAssistantViewModel, DocumentClosed>(this, static (r, m) => r.OnDocumentClosed(m));
        }

        private void OnWindowClosed(Window win)
        {
            // (Konrad) We notify the Communicator View Model that window has closed so View gets set to null and selection is reset.
            WeakReferenceMessenger.Default.Send(new TaskAssistantClosedMessage { IsClosed = true });

            // (Konrad) We need to unregister Messenger handlers or they get duplicated.
            OnDeactivated();
        }

        private void OnWindowLoaded(Window win)
        {
            Win = win;
            HOK.Core.WpfUtilities.StatusBarManager.StatusLabel = Control = ((SheetTaskAssistantView)win).statusLabel;
        }

        private void OnApprove(Window win)
        {
            Model.SubmitSheetTask(Wrapper);
        }

        private static void OnClose(Window win)
        {
            win.Close();
        }

        private void OnOpenView()
        {
            Model.OpenView((SheetItem)Wrapper.Element);
        }

        private SheetTaskWrapper _wrapper;
        public SheetTaskWrapper Wrapper
        {
            get { return _wrapper; }
            set { _wrapper = value; OnPropertyChanged(nameof(Wrapper)); Broadcast(_wrapper, value, nameof(Wrapper)); }
        }

        private string _okText;
        public string OkText
        {
            get { return _okText; }
            set { _okText = value; OnPropertyChanged(nameof(OkText)); Broadcast(_okText, value, nameof(OkText)); }
        }

        #region Message Handlers

        /// <summary>
        /// Event raised when Revit Document is closed. We need to make sure that task window is also closed.
        /// </summary>
        /// <param name="msg">Message object.</param>
        private void OnDocumentClosed(DocumentClosed msg)
        {
            Win?.Close();
        }

        /// <summary>
        /// Event raised when External Command finishes making proposed changes to the sheet.
        /// </summary>
        /// <param name="msg">Message object.</param>
        private void OnSheetTaskCompleted(SheetTaskCompletedMessage msg)
        {
            if (msg.Completed)
            {
                // (Konrad) Changes were applied successfully.
                if (Control == null) return;

                Control.Dispatcher.Invoke(() =>
                {
                    Model.Approve(Wrapper, msg.CentralPath);
                }, DispatcherPriority.Normal);
            }
            else
            {
                HOK.Core.WpfUtilities.StatusBarManager.StatusLabel.Text = msg.Message;
            }
        }

        #endregion
    }
}
