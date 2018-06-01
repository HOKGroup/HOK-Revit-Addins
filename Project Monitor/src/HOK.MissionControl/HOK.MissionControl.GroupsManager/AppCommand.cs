using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.MissionControl.GroupsManager
{
    [Name(nameof(Properties.Resources.GroupsManager_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.GroupsManager_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.GroupsManager_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.GroupsManager_PanelName), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.GroupsManager_Namespace), typeof(Properties.Resources))]
    [AdditionalButtonNames(nameof(Properties.Resources.GroupsManager_AdditionalButtonNames), typeof(Properties.Resources))]
    public class AppCommand : IExternalApplication
    {
        private const string tabName = "  HOK - Beta";
        public static GroupManagerRequestHandler GroupManagerHandler { get; set; }
        public static ExternalEvent GroupManagerEvent { get; set; }

        public Result OnStartup(UIControlledApplication application)
        {
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
