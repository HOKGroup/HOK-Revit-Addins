//using System;
//using System.IO;
//using System.Windows.Media.Imaging;
//using Autodesk.Revit.UI;

//namespace RevitDBManager
//{
//    class AppCommand : IExternalApplication
//    {
//        public Result OnStartup(UIControlledApplication application)
//        {
//            var rp = application.CreateRibbonPanel("HOK Revit Data");
//            var currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

//            var pb1 = rp.AddItem(new PushButtonData("Data Sync", "Data Sync", currentAssembly, "RevitDBManager.Command")) as PushButton;
//            var uriImage = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/sync.ico");
//            var largeImage = new BitmapImage(uriImage);
//            pb1.LargeImage = largeImage;

//            //previously Editor, changed to Setup
//            var pb2 = rp.AddItem(new PushButtonData("Setup", "  Setup  ", currentAssembly, "RevitDBManager.EditorCommand")) as PushButton;
//            var uriImage2 = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/editor.ico");
//            var largeImage2 = new BitmapImage(uriImage2);
//            pb2.LargeImage = largeImage2;

//            //previously Viewer, changed to Editor
//            var pb3 = rp.AddItem(new PushButtonData("Data Editor", "Data Editor", currentAssembly, "RevitDBManager.ViewerCommand")) as PushButton;
//            var uriImage3 = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/view.ico");
//            var largeImage3 = new BitmapImage(uriImage3);
//            pb3.LargeImage = largeImage3;

//            return Result.Succeeded;
//        }

//        public Result OnShutdown(UIControlledApplication application)
//        {
//            return Result.Succeeded;
//        }
//    }
//}
