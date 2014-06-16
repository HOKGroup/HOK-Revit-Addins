using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Reflection;
using System.Windows;

namespace HOK.Utilities
{
    public class AppCommand:IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("HOK Utilities");
            Assembly assembly = Assembly.GetExecutingAssembly();
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\HOK Utilities_Instruction.pdf");

            SplitButtonData splitButtonData = new SplitButtonData("HOKUtilities", "HOK Utilities");
            SplitButton splitButton = rp.AddItem(splitButtonData) as SplitButton;

            PushButton pushButton1 = splitButton.AddPushButton(new PushButtonData("FinishCreator", "Finish Creator", currentAssembly, "HOK.Utilities.FinishCommand")) as PushButton;
            pushButton1.LargeImage = LoadBitmapImage(assembly, "finish.png");
            pushButton1.ToolTip = "Create floor finishes from the selected rooms.";
            pushButton1.SetContextualHelp(contextualHelp);

            PushButton pushButton2 = splitButton.AddPushButton(new PushButtonData("CeilingHeight", "Ceiling Heights", currentAssembly, "HOK.Utilities.CeilingCommand")) as PushButton;
            pushButton2.LargeImage = LoadBitmapImage(assembly, "height.png");
            pushButton2.ToolTip = "Select rooms to measure the height from floors to ceilings.";
            pushButton2.SetContextualHelp(contextualHelp);

            PushButton pushButton3 = splitButton.AddPushButton(new PushButtonData("LevelManager", "Level Manager", currentAssembly, "HOK.Utilities.LevelCommand")) as PushButton;
            pushButton3.LargeImage = LoadBitmapImage(assembly, "level.png");
            pushButton3.ToolTip = "Rehost elements from one level to anather. ";
            pushButton3.SetContextualHelp(contextualHelp);

#if RELEASE2013
#else
            PushButton pushButton4 = splitButton.AddPushButton(new PushButtonData("ViewDepth", "View Depth", currentAssembly, "HOK.Utilities.ViewCommand")) as PushButton;
            pushButton4.LargeImage = LoadBitmapImage(assembly, "camera.ico");
            pushButton4.ToolTip = "Override the graphics of the element based on the distance";
            pushButton4.SetContextualHelp(contextualHelp);
#endif
           
            PushButton pushButton5 = splitButton.AddPushButton(new PushButtonData("LeaderArrowhead", "Leader Arrowhead", currentAssembly, "HOK.Utilities.ArrowCommand")) as PushButton;
            pushButton5.LargeImage = LoadBitmapImage(assembly, "arrowhead.png");
            pushButton5.ToolTip = "Assign a leader arrowhead style to all tag types.";
            pushButton5.SetContextualHelp(contextualHelp);

            PushButton pushButton6 = splitButton.AddPushButton(new PushButtonData("View Creator", "View Creator", currentAssembly, "HOK.Utilities.WorksetCommand")) as PushButton;
            pushButton6.LargeImage = LoadBitmapImage(assembly, "workset.png");
            pushButton6.ToolTip = "Create 3D Views for each workset.";
            pushButton6.SetContextualHelp(contextualHelp);

            PushButton pushButton7 = splitButton.AddPushButton(new PushButtonData("Door Link", "Door Link", currentAssembly, "HOK.Utilities.DoorCommand")) as PushButton;
            pushButton7.LargeImage = LoadBitmapImage(assembly, "door.png");
            pushButton7.ToolTip = "Assign To-Room and From-Room values for all door elements across linked models.";
            pushButton7.SetContextualHelp(contextualHelp);

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
                MessageBox.Show("Failed to load the embedded resource image.\n"+ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
        }
    }
}
