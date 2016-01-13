using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace HOK.RoomsToMass
{
    public class AppCommand:IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("HOK Mass Tools");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            #region Create a SplitButton for user to create Mass or Transfer Data
            SplitButtonData splitButtonData = new SplitButtonData("MassTool", "3D Mass");
            SplitButton splitButton = rp.AddItem(splitButtonData) as SplitButton;
            
            PushButton pushButton = splitButton.AddPushButton(new PushButtonData("CreateMass", "Create Mass", currentAssembly, "HOK.RoomsToMass.Command"));
            pushButton.LargeImage = LoadBitmapImage("cube.png");

            pushButton.ToolTip = "Creates 3D Mass from rooms, areas and floors.";
            pushButton.ToolTipImage = LoadBitmapImage("tooltip.png");

            PushButton pushButton2 = splitButton.AddPushButton(new PushButtonData("UpdateData", "Update Data", currentAssembly, "HOK.RoomsToMass.DataCommand"));
            pushButton2.LargeImage = LoadBitmapImage("refresh.png");
            pushButton2.ToolTip = "This will transfer parameters values between source objects and created masses.";

            PushButton pushButton3 = splitButton.AddPushButton(new PushButtonData("MassCommands", "Mass Commands", currentAssembly, "HOK.RoomsToMass.AssignerCommand"));
            pushButton3.LargeImage = LoadBitmapImage("shape.png");
            pushButton3.ToolTip = "This will assign worksets or other parameters values in elements depending on what mass objects surrounding the elements.";
            #endregion

            return Result.Succeeded;
        }

        private BitmapImage LoadBitmapImage(string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
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
