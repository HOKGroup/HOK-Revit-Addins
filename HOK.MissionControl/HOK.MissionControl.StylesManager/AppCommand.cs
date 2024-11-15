using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using Nice3point.Revit.Toolkit.External;

namespace HOK.MissionControl.StylesManager
{
    public class AppCommand : ExternalApplication
    {
        private const string tabName = "   HOK   ";
        public static StylesManagerRequestHandler StylesManagerHandler { get; set; }
        public static ExternalEvent StylesManagerEvent { get; set; }

        public override void OnStartup()
        {
            try
            {
                Application.CreateRibbonTab(tabName);
            }
            catch
            {
                Log.AppendLog(LogMessageType.INFO, "Ribbon tab was not created because it already exists: " + tabName);
            }

            var assembly = Assembly.GetAssembly(GetType());
            var panel = Application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Mission Control")
                        ?? Application.CreateRibbonPanel(tabName, "Mission Control");
            var unused = (PushButton)panel.AddItem(new PushButtonData("StylesManager_Command", "  Styles  " + Environment.NewLine + "Manager",
                assembly.Location, "HOK.MissionControl.StylesManager.StylesManagerCommand")
            {
                LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl.StylesManager", Properties.Resources.StylesManager_ImageName),
                ToolTip = Properties.Resources.StylesManager_Description
            });

            StylesManagerHandler = new StylesManagerRequestHandler();
            StylesManagerEvent = ExternalEvent.Create(StylesManagerHandler);
        }

    }
}
