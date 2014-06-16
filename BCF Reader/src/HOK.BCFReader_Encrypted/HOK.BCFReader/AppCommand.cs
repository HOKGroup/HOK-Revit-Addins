using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace HOK.BCFReader
{
    public class AppCommand:IExternalApplication
    {
        private string tabName="";

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            tabName = " HOK Tools ";
            try { application.CreateRibbonTab(tabName); }
            catch { }

            RibbonPanel bcfPanel = application.CreateRibbonPanel(tabName, "HOK BCF");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            PushButton pushBttn = bcfPanel.AddItem(new PushButtonData("BCF Reader", "BCF Reader", currentAssembly, "HOK.BCFReader.Command")) as PushButton;
            Uri uriImage = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/comment.ico");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushBttn.LargeImage = largeImage;
            return Result.Succeeded;
        }
    }
}
