using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.MissionControl.StylesManager
{
    public class AppCommand : IExternalApplication
    {
        private const string tabName = "   HOK   ";
        public static StylesManagerRequestHandler StylesManagerHandler { get; set; }
        public static ExternalEvent StylesManagerEvent { get; set; }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch
            {
                Log.AppendLog(LogMessageType.INFO, "Ribbon tab was not created because it already exists: " + tabName);
            }

            var assembly = Assembly.GetAssembly(GetType());
            var panel = application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Mission Control")
                        ?? application.CreateRibbonPanel(tabName, "Mission Control");
            var unused = (PushButton)panel.AddItem(new PushButtonData("StylesManager_Command", "  Styles  " + Environment.NewLine + "Manager",
                assembly.Location, "HOK.MissionControl.StylesManager.StylesManagerCommand")
            {
                LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl.StylesManager", Properties.Resources.StylesManager_ImageName),
                ToolTip = Properties.Resources.StylesManager_Description
            });

            StylesManagerHandler = new StylesManagerRequestHandler();
            StylesManagerEvent = ExternalEvent.Create(StylesManagerHandler);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
