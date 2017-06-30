using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows;
using Autodesk.Revit.UI;

namespace HOK.Utilities
{
    public class AppCommand:IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var rp = application.CreateRibbonPanel("HOK Utilities");
            var assembly = Assembly.GetExecutingAssembly();
            var currentAssembly = Assembly.GetAssembly(GetType()).Location;
            var directoryName = Path.GetDirectoryName(currentAssembly);

            var contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\HOK Utilities_Instruction.pdf");

            var splitButtonData = new SplitButtonData("HOKUtilities", "HOK Utilities");
            var splitButton = (SplitButton)rp.AddItem(splitButtonData);

            if (directoryName == null) return Result.Succeeded;

            var finishPath = Path.Combine(directoryName, "HOK.FinishCreator.dll");
            var finishButton = splitButton.AddPushButton(new PushButtonData("FinishCreator", "Finish Creator", finishPath, "HOK.FinishCreator.FinishCommand"));
            finishButton.LargeImage = LoadBitmapImage(assembly, "finish.png");
            finishButton.ToolTip = "Create floor finishes from the selected rooms.";
            finishButton.SetContextualHelp(contextualHelp);

            var ceilingPath = Path.Combine(directoryName, "HOK.CeilingHeight.dll");
            var ceilingButton = splitButton.AddPushButton(new PushButtonData("CeilingHeight", "Ceiling Heights", ceilingPath, "HOK.CeilingHeight.CeilingCommand"));
            ceilingButton.LargeImage = LoadBitmapImage(assembly, "height.png");
            ceilingButton.ToolTip = "Select rooms to measure the height from floors to ceilings.";
            ceilingButton.SetContextualHelp(contextualHelp);

            var levelPath = Path.Combine(directoryName, "HOK.LevelManager.dll");
            var levelButton = splitButton.AddPushButton(new PushButtonData("LevelManager", "Level Manager", levelPath, "HOK.LevelManager.LevelCommand"));
            levelButton.LargeImage = LoadBitmapImage(assembly, "level.png");
            levelButton.ToolTip = "Rehost elements from one level to anather. ";
            levelButton.SetContextualHelp(contextualHelp);

            var viewPath = Path.Combine(directoryName, "HOK.ViewDepth.dll");
            var viewButton = splitButton.AddPushButton(new PushButtonData("ViewDepth", "View Depth", viewPath, "HOK.ViewDepth.ViewCommand"));
            viewButton.LargeImage = LoadBitmapImage(assembly, "camera.ico");
            viewButton.ToolTip = "Override the graphics of the element based on the distance";
            viewButton.SetContextualHelp(contextualHelp);

            var arrowPath = Path.Combine(directoryName, "HOK.Arrowhead.dll");
            var arrowButton = splitButton.AddPushButton(new PushButtonData("LeaderArrowhead", "Leader Arrowhead", arrowPath, "HOK.Arrowhead.ArrowCommand"));
            arrowButton.LargeImage = LoadBitmapImage(assembly, "arrowhead.png");
            arrowButton.ToolTip = "Assign a leader arrowhead style to all tag types.";
            arrowButton.SetContextualHelp(contextualHelp);

            var worksetPath = Path.Combine(directoryName, "HOK.WorksetView.dll");
            var worksetButton = splitButton.AddPushButton(new PushButtonData("View Creator", "View Creator", worksetPath, "HOK.WorksetView.WorksetCommand"));
            worksetButton.LargeImage = LoadBitmapImage(assembly, "workset.png");
            worksetButton.ToolTip = "Create 3D Views for each workset.";
            worksetButton.SetContextualHelp(contextualHelp);

            var doorPath = Path.Combine(directoryName, "HOK.DoorRoom.dll");
            var doorButton = splitButton.AddPushButton(new PushButtonData("Door Link", "Door Link", doorPath, "HOK.DoorRoom.DoorCommand"));
            doorButton.LargeImage = LoadBitmapImage(assembly, "door.png");
            doorButton.ToolTip = "Assign To-Room and From-Room values for all door elements across linked models.";
            doorButton.SetContextualHelp(contextualHelp);

            var roomPath = Path.Combine(directoryName, "HOK.RoomUpdater.dll");
            var roomButton = splitButton.AddPushButton(new PushButtonData("Room Updater", "Room Updater", roomPath, "HOK.RoomUpdater.RoomCommand"));
            roomButton.LargeImage = LoadBitmapImage(assembly, "cube.png");
            roomButton.ToolTip = "Assign room data into enclosed elements.";
            roomButton.SetContextualHelp(contextualHelp);

            var elevationPath = Path.Combine(directoryName, "HOK.RoomElevation.dll");
            var elevationButton = splitButton.AddPushButton(new PushButtonData("Elevation Creator", "Elevation Creator", elevationPath, "HOK.RoomElevation.ElevationCommand"));
            elevationButton.LargeImage = LoadBitmapImage(assembly, "elevation.png");
            elevationButton.ToolTip = "Place an elevation marker within a room and rotate the marker to be perpendicular to a selected wall.";
            elevationButton.SetContextualHelp(contextualHelp);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private BitmapImage LoadBitmapImage(Assembly assembly, string imageName)
        {
            var image = new BitmapImage();
            try
            {
                var prefix = typeof(AppCommand).Namespace + ".Resources.";
                var stream = assembly.GetManifestResourceStream(prefix + imageName);

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
