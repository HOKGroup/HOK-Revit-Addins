using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace HOK.ColorSchemeEditor
{
    public class AppCommand:IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("HOK Color Editor");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            Assembly assembly = Assembly.GetExecutingAssembly();
            PushButton pushButton = rp.AddItem(new PushButtonData("Color Editor", "Color Editor", currentAssembly, "HOK.ColorSchemeEditor.Command")) as PushButton;
            pushButton.LargeImage = LoadBitmapImage(assembly, "color32.png"); ;
            
            return Result.Succeeded;
        }

        private BitmapImage LoadBitmapImage(Assembly assembly, string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                string prefix = typeof(AppCommand).Namespace + ".Resources.";
                Stream stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return image;
        }
    }
}
