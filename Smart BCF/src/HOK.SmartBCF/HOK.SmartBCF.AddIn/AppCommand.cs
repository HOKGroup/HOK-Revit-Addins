using System;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using HOK.SmartBCF.AddIn.Properties;

namespace HOK.SmartBCF.AddIn
{
    public class AppCommand : IExternalApplication
    {
        internal static AppCommand thisApp = null;
        private MainWindow mainWindow = null;
        private string tabName = "  HOK - Beta";

        public Result OnShutdown(UIControlledApplication application)
        {
            if (mainWindow != null && mainWindow.IsVisible)
            {
                mainWindow.Close();
            }
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                thisApp = this;
                mainWindow = null;

                try { application.CreateRibbonTab(tabName); }
                catch { }

                RibbonPanel panel = application.CreateRibbonPanel(tabName, "BCF");
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

                BitmapSource sheetImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Resources.walker.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                PushButton sheetButton = panel.AddItem(new PushButtonData("Smart BCF", "Smart BCF", currentAssembly, "HOK.SmartBCF.AddIn.Command")) as PushButton;
                sheetButton.LargeImage = sheetImage;
                sheetButton.AvailabilityClassName = "HOK.SmartBCF.AddIn.Availability";

                string instructionFile = @"V:\RVT-Data\HOK Program\Documentation\SmartBCF_Instruction.pdf";
                if (File.Exists(instructionFile))
                {
                    ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, instructionFile);
                    sheetButton.SetContextualHelp(contextualHelp);
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return Result.Succeeded;
        }

        public void ShowWindow(UIApplication uiapp)
        {
            if (mainWindow == null)
            {
                BCFHandler handler = new BCFHandler(uiapp);
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                mainWindow = new MainWindow(exEvent, handler);
                mainWindow.Closed += WindowClosed;
                mainWindow.Show();
            }
        }

        public void WakeWindow()
        {
            if (mainWindow != null)
            {

            }
        }

        public void WindowClosed(object sender, System.EventArgs e)
        {
            mainWindow = null;
            bool closed = BCFDBWriter.BCFDBWriter.CloseConnection();
        }
    }

    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }
}
