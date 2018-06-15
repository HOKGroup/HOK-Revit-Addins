using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.MissionControl.StylesManager
{
    [Name(nameof(Properties.Resources.StylesManager_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.StylesManager_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.StylesManager_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.StylesManager_PanelName), typeof(Properties.Resources))]
    [ButtonText(nameof(Properties.Resources.StylesManager_ButtonText), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.StylesManager_Namespace), typeof(Properties.Resources))]
    [AdditionalButtonNames(nameof(Properties.Resources.StylesManager_AdditionalButtonNames), typeof(Properties.Resources))]
    public class AppCommand : IExternalApplication
    {
        private const string tabName = "  HOK - Beta";
        public static StylesManagerRequestHandler StylesManagerHandler { get; set; }
        public static ExternalEvent StylesManagerEvent { get; set; }

        public Result OnStartup(UIControlledApplication application)
        {
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
