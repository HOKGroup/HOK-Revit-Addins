using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.Navisworks.Api.Plugins;

namespace ARUP.IssueTracker.Navisworks
{
  [Plugin("ARUP.IssueTracker.Navisworks.Plugin", "CASE", DisplayName = "Arup Issue Tracker by CASE", ToolTip = "Arup Issue Tracker by CASE")]
  [DockPanePlugin(800, 500, FixedSize = false)]
  class Plugin : DockPanePlugin
  {
    NavisWindow ns;

    public override Control CreateControlPane()
    {
      //create an ElementHost
      ElementHost eh = new ElementHost();

      

      //assign the control
      //eh.Anchor = AnchorStyles.Top;
      eh.Dock = DockStyle.Top;
      eh.Anchor = AnchorStyles.Top;
      ns = new NavisWindow();
      eh.Child = ns;
      eh.HandleCreated += eh_HandleCreated;
      eh.CreateControl();

      //return the ElementHost
      return eh;
    }

    void eh_HandleCreated(object sender, EventArgs e)
    {
      ElementHost eh = sender as ElementHost;
      eh.Dock = DockStyle.Top;
      eh.Anchor = AnchorStyles.Top;

    }

    public override void DestroyControlPane(Control pane)
    {
      ns.mainPan.onClosing(null);
      pane.Dispose();
    }
  }
}