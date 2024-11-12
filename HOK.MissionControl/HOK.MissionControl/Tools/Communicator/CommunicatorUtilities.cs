using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Tools.Communicator
{
    public static class CommunicatorUtilities
    {
        /// <summary>
        /// Registers Communicator Dockable Panel.
        /// </summary>
        /// <param name="application">UIControlledApp</param>
        public static void RegisterCommunicator(UIControlledApplication application)
        {
            var view = new CommunicatorView();
            AppCommand.CommunicatorWindow = view;

            var unused = new DockablePaneProviderData
            {
                FrameworkElement = AppCommand.CommunicatorWindow,
                InitialState = new DockablePaneState
                {
                    DockPosition = DockPosition.Tabbed,
                    TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser
                }
            };

            var dpid = new DockablePaneId(new Guid(Properties.Resources.CommunicatorGuid));
            try
            {
                // (Konrad) It's possible that a dockable panel with the same id already exists
                // This ensures that we don't get an exception here. 
                application.RegisterDockablePane(dpid, "Mission Control", AppCommand.CommunicatorWindow);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        /// <summary>
        /// Due to all asynch stuff some data might not be available right away so we use this callback to instantiate the Communicator.
        /// </summary>
        public static void LaunchCommunicator()
        {
            AppCommand.CommunicatorWindow.MainControl.Dispatcher.Invoke(() =>
            {
                // (Konrad) We have to make sure that we unregister from all Messaging before reloading UI.
                if (AppCommand.CommunicatorWindow.DataContext != null)
                {
                    var tabItems = AppCommand.CommunicatorWindow.MainControl.Items.SourceCollection;
                    foreach (var ti in tabItems)
                    {
                        var content = ((UserControl)((TabItem)ti).Content).DataContext as ObservableRecipient;
                    }
                }

                // (Konrad) Now we can reset the ViewModel
                AppCommand.CommunicatorWindow.DataContext = new CommunicatorViewModel();
                if (AppCommand.CommunicatorWindow.MainControl.Items.Count > 0)
                {
                    AppCommand.CommunicatorWindow.MainControl.SelectedIndex = 0;
                }
            }, DispatcherPriority.Normal);
        }

        /// <summary>
        /// Communicator Image can only be set when we are done loading the app.
        /// </summary>
        public static void SetCommunicatorImage()
        {
            // (Konrad) This needs to run after the doc is opened, because UI elements don't get created until then.
            AppCommand.EnqueueTask(app =>
            {
                var dpid = new DockablePaneId(new Guid(Properties.Resources.CommunicatorGuid));
                var dp = app.GetDockablePane(dpid);
                var assembly = Assembly.GetExecutingAssembly();
                if (dp != null)
                {
                    AppCommand.Instance.CommunicatorButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", dp.IsShown()
                        ? "communicatorOn_32x32.png"
                        : "communicatorOff_32x32.png");
                    AppCommand.Instance.CommunicatorButton.ItemText = dp.IsShown()
                        ? "Hide" + Environment.NewLine + "Communicator"
                        : "Show" + Environment.NewLine + "Communicator";
                }
            });
        }
    }
}
