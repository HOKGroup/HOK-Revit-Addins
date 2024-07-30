﻿using System;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;
using HOK.Core.Utilities;

namespace HOK.Navigator
{
    public class AppCommand : IExternalApplication
    {
        private UIControlledApplication m_app;
        private string tabName = "";
        private string currentAssembly = "";
        public PushButton helpButton;

        public Result OnStartup(UIControlledApplication application)
        {
            m_app = application;
            tabName = "   HOK   ";
            currentAssembly = Assembly.GetAssembly(GetType()).Location;

            application.ControlledApplication.ApplicationInitialized += EventAppInitialize;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized -= EventAppInitialize;

            return Result.Succeeded;
        }

        private void EventAppInitialize(object sender, ApplicationInitializedEventArgs arg)
        {
            CreatePushButton();
        }

        /// <summary>
        /// Creates push button for Navigator.
        /// </summary>
        private void CreatePushButton()
        {
            try
            {
                try
                {
                    m_app.CreateRibbonTab(tabName);
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }

                var created = m_app.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Help");
                var helpPanel = created ?? m_app.CreateRibbonPanel(tabName, "Help");
                var assembly = Assembly.GetExecutingAssembly();
                var hokImage = LoadBitmapImage(assembly, "hok.png");

                var buttonData = new PushButtonData("HOK Navigator", "HOK" + Environment.NewLine + " Navigator ", currentAssembly, "HOK.Navigator.HelpCommand")
                {
                    AvailabilityClassName = "HOK.Navigator.Availability",
                    LargeImage = hokImage,
                    Image = hokImage
                };
                helpButton = helpPanel.AddItem(buttonData) as PushButton;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private static BitmapImage LoadBitmapImage(Assembly assembly, string imageName)
        {
            var image = new BitmapImage();
            try
            {
                var prefix = typeof(AppCommand).Namespace + ".Resources.";
                var stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return image;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }
}
