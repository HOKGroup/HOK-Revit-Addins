using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HOK.XYZLocator
{
    public class AppCommand : IExternalApplication
    {
        private string tabName = "HOK";

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                try { application.CreateRibbonTab(tabName); }
                catch { }

                RibbonPanel panel = application.CreateRibbonPanel(tabName, "XYZ Locator");
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

                BitmapSource locatorImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.location.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                PushButton locatorButton = panel.AddItem(new PushButtonData("Locate", "Locate", currentAssembly, "HOK.XYZLocator.XYZCommand")) as PushButton;
                locatorButton.LargeImage = locatorImage;

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return Result.Succeeded;
        }
    }
}
