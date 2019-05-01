using Autodesk.Revit.UI;

namespace HOK.MissionControl.Tools.Communicator
{
    /// <summary>
    /// Interaction logic for CommunicatorView.xaml
    /// </summary>
    public partial class CommunicatorView : IDockablePaneProvider
    {
        public CommunicatorView()
        {
            InitializeComponent();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Tabbed,
                TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser
            };
        }
    }
}
