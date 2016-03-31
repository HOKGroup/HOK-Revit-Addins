using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HOK.ElementFlatter
{
    public class AppCommand:IExternalApplication
    {
        private string tabName = "  HOK - Beta";

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

                RibbonPanel panel = application.CreateRibbonPanel(tabName, "Element Flatter");
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

                BitmapSource sheetImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.create.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                PushButton sheetButton = panel.AddItem(new PushButtonData("Flatten", "Flatten", currentAssembly, "HOK.ElementFlatter.Command")) as PushButton;
                sheetButton.LargeImage = sheetImage;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return Result.Succeeded;
        }
    }
}
