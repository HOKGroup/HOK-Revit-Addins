using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.MissionControl.FamilyPublish
{
    public class AppCommand : IExternalApplication
    {
        private UIControlledApplication m_app;
        private string tabName = "";
        private string currentDirectory = "";
        private string currentAssembly = "";

        public Result OnStartup(UIControlledApplication application)
        {
            m_app = application;
            tabName = "  HOK - Beta";

            try
            {
                m_app.CreateRibbonTab(tabName);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            currentAssembly = Assembly.GetAssembly(GetType()).Location;
            currentDirectory = Path.GetDirectoryName(currentAssembly);

            CreateMissionControlPushButtons();

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateMissionControlPushButtons()
        {
            try
            {
                var missionControlPanel = m_app.CreateRibbonPanel(tabName, "Mission Control");
                var assemblyPath = currentDirectory + "/HOK.MissionControl.FamilyPublish.dll";

                var pb1 = new PushButtonData(
                    "PublishFamilyDataCommand",
                    "Publish Family" + Environment.NewLine + "Data",
                    assemblyPath,
                    "HOK.MissionControl.FamilyPublish.FamilyPublishCommand")
                {
                    ToolTip = "Mission Control Family Export Tool."
                };
                var fpAssembly = Assembly.GetExecutingAssembly();
                pb1.LargeImage = ButtonUtil.LoadBitmapImage(fpAssembly, "HOK.MissionControl.FamilyPublish", "publishFamily_32x32.png");

                missionControlPanel.AddItem(pb1);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
