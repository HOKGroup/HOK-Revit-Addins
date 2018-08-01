using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.MissionControl.GroupsManager
{
    public class AppCommand : IExternalApplication
    {
        private const string tabName = "   HOK   ";
        public static GroupManagerRequestHandler GroupManagerHandler { get; set; }
        public static ExternalEvent GroupManagerEvent { get; set; }

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
            var unused = (PushButton)panel.AddItem(new PushButtonData("GroupManager_Command", "  Groups  " + Environment.NewLine + "Manager",
                assembly.Location, "HOK.MissionControl.GroupsManager.GroupsManagerCommand")
            {
                LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl.GroupsManager", Properties.Resources.GroupsManager_ImageName),
                ToolTip = Properties.Resources.GroupsManager_Description
            });

            GroupManagerHandler = new GroupManagerRequestHandler();
            GroupManagerEvent = ExternalEvent.Create(GroupManagerHandler);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
