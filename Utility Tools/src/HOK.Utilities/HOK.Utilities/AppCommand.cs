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
            string directoryName = Path.GetDirectoryName(currentAssembly);


            ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\HOK Utilities_Instruction.pdf");

            SplitButtonData splitButtonData = new SplitButtonData("HOKUtilities", "HOK Utilities");
            SplitButton splitButton = rp.AddItem(splitButtonData) as SplitButton;

            string finishPath = Path.Combine(directoryName, "HOK.FinishCreator.dll");
            PushButton finishButton = splitButton.AddPushButton(new PushButtonData("FinishCreator", "Finish Creator", finishPath, "HOK.FinishCreator.FinishCommand")) as PushButton;
            finishButton.LargeImage = LoadBitmapImage(assembly, "finish.png");
            finishButton.ToolTip = "Create floor finishes from the selected rooms.";
            finishButton.SetContextualHelp(contextualHelp);

            string ceilingPath = Path.Combine(directoryName, "HOK.CeilingHeight.dll");
            PushButton ceilingButton = splitButton.AddPushButton(new PushButtonData("CeilingHeight", "Ceiling Heights", ceilingPath, "HOK.CeilingHeight.CeilingCommand")) as PushButton;
            ceilingButton.LargeImage = LoadBitmapImage(assembly, "height.png");
            ceilingButton.ToolTip = "Select rooms to measure the height from floors to ceilings.";
            ceilingButton.SetContextualHelp(contextualHelp);

            string levelPath = Path.Combine(directoryName, "HOK.LevelManager.dll");
            PushButton levelButton = splitButton.AddPushButton(new PushButtonData("LevelManager", "Level Manager", levelPath, "HOK.LevelManager.LevelCommand")) as PushButton;
            levelButton.LargeImage = LoadBitmapImage(assembly, "level.png");
            levelButton.ToolTip = "Rehost elements from one level to anather. ";
            levelButton.SetContextualHelp(contextualHelp);

#if RELEASE2013
#else
            string viewPath = Path.Combine(directoryName, "HOK.ViewDepth.dll");
            PushButton viewButton = splitButton.AddPushButton(new PushButtonData("ViewDepth", "View Depth", viewPath, "HOK.ViewDepth.ViewCommand")) as PushButton;
            viewButton.LargeImage = LoadBitmapImage(assembly, "camera.ico");
            viewButton.ToolTip = "Override the graphics of the element based on the distance";
            viewButton.SetContextualHelp(contextualHelp);
#endif

            string arrowPath = Path.Combine(directoryName, "HOK.Arrowhead.dll");
            PushButton arrowButton = splitButton.AddPushButton(new PushButtonData("LeaderArrowhead", "Leader Arrowhead", arrowPath, "HOK.Arrowhead.ArrowCommand")) as PushButton;
            arrowButton.LargeImage = LoadBitmapImage(assembly, "arrowhead.png");
            arrowButton.ToolTip = "Assign a leader arrowhead style to all tag types.";
            arrowButton.SetContextualHelp(contextualHelp);

            string worksetPath = Path.Combine(directoryName, "HOK.WorksetView.dll");
            PushButton worksetButton = splitButton.AddPushButton(new PushButtonData("View Creator", "View Creator", worksetPath, "HOK.WorksetView.WorksetCommand")) as PushButton;
            worksetButton.LargeImage = LoadBitmapImage(assembly, "workset.png");
            worksetButton.ToolTip = "Create 3D Views for each workset.";
            worksetButton.SetContextualHelp(contextualHelp);

            string doorPath = Path.Combine(directoryName, "HOK.DoorRoom.dll");
            PushButton doorButton = splitButton.AddPushButton(new PushButtonData("Door Link", "Door Link", doorPath, "HOK.DoorRoom.DoorCommand")) as PushButton;
            doorButton.LargeImage = LoadBitmapImage(assembly, "door.png");
            doorButton.ToolTip = "Assign To-Room and From-Room values for all door elements across linked models.";
            doorButton.SetContextualHelp(contextualHelp);

            string roomPath = Path.Combine(directoryName, "HOK.RoomUpdater.dll");
            PushButton roomButton = splitButton.AddPushButton(new PushButtonData("Room Updater", "Room Updater", roomPath, "HOK.RoomUpdater.RoomCommand")) as PushButton;
            roomButton.LargeImage = LoadBitmapImage(assembly, "cube.png");
            roomButton.ToolTip = "Assign room data into enclosed elements.";
            roomButton.SetContextualHelp(contextualHelp);

            string elevationPath = Path.Combine(directoryName, "HOK.RoomElevation.dll");
            PushButton elevationButton = splitButton.AddPushButton(new PushButtonData("Elevation Creator", "Elevation Creator", elevationPath, "HOK.RoomElevation.ElevationCommand")) as PushButton;
            elevationButton.LargeImage = LoadBitmapImage(assembly, "elevation.png");
            elevationButton.ToolTip = "Place an elevation marker within a room and rotate the marker to be perpendicular to a selected wall.";
            elevationButton.SetContextualHelp(contextualHelp);

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
                MessageBox.Show("Failed to load the embedded resource image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
        }
    }
}
