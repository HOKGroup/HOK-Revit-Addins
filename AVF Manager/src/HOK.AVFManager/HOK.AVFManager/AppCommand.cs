using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;

namespace HOK.AVFManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class AppCommand:IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("HOK Analysis");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            PushButton pb1 = rp.AddItem(new PushButtonData("  Analysis  ", "  Analysis  ", currentAssembly, "HOK.AVFManager.Command")) as PushButton;
            Uri uriImage = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/chart.ico");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pb1.LargeImage = largeImage;
            return Result.Succeeded;
        }
    }
}
