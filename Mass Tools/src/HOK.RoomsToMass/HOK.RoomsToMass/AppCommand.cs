using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;

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
            Uri uriImage = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/cube.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushButton.LargeImage = largeImage;

            pushButton.ToolTip = "Creates 3D Mass from rooms, areas and floors.";
            uriImage = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/tooltip.png");
            largeImage = new BitmapImage(uriImage);
            pushButton.ToolTipImage = largeImage;

            PushButton pushButton2 = splitButton.AddPushButton(new PushButtonData("UpdateData", "Update Data", currentAssembly, "HOK.RoomsToMass.DataCommand"));
            Uri uriImage2 = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/refresh.png");
            BitmapImage largeImage2 = new BitmapImage(uriImage2);
            pushButton2.LargeImage = largeImage2;
            pushButton2.ToolTip = "This will transfer parameters values between source objects and created masses.";

            PushButton pushButton3 = splitButton.AddPushButton(new PushButtonData("MassCommands", "Mass Commands", currentAssembly, "HOK.RoomsToMass.AssignerCommand"));
            Uri uriImage3 = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/shape.png");
            BitmapImage largeImage3 = new BitmapImage(uriImage3);
            pushButton3.LargeImage = largeImage3;
            pushButton3.ToolTip = "This will assign worksets or other parameters values in elements depending on what mass objects surrounding the elements.";
            #endregion

            return Result.Succeeded;
        }
    }
}
