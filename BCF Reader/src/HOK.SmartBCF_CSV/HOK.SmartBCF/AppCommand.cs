using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using HOK.SmartBCF.Utils;
using HOK.SmartBCF.Walker;

namespace HOK.SmartBCF
{
    public class AppCommand : IExternalApplication
    {
        internal static AppCommand thisApp = null;
        private WalkerWindow walkerWindow;
        private string tabName = "";

        public Result OnShutdown(UIControlledApplication application)
        {
            if (walkerWindow != null && walkerWindow.IsVisible)
            {
                walkerWindow.Close();
            }

            
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            thisApp = this;
            walkerWindow = null;

            tabName = "   HOK   ";
            try { application.CreateRibbonTab(tabName); }
            catch { }

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            RibbonPanel rp = application.CreateRibbonPanel(tabName, "BCF");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            BitmapSource walkerImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.walker.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            PushButton walkerButton = rp.AddItem(new PushButtonData("smartBCF", "smartBCF", currentAssembly, "HOK.SmartBCF.BCFCommand")) as PushButton;
            walkerButton.LargeImage = walkerImage;

            return Result.Succeeded;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = new AssemblyName(args.Name);
                string[] assemblyNames = new string[] { "System.Net.Http.Primitives", "Newtonsoft.Json" };
                if (assemblyNames.Contains(name.Name))
                {
                    string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                    string assemblyToLoad = Path.Combine(Path.GetDirectoryName(currentAssembly), name.Name+".dll");
                    Assembly assemblyLoaded = Assembly.LoadFrom(assemblyToLoad);
                    if (null != assemblyLoaded) { return assemblyLoaded; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load assembly.\n" + ex.Message);
            }
            return null;
        }

        public void ShowWalker(UIApplication uiapp)
        {
            if (walkerWindow == null)
            {
                WalkerHandler handler = new WalkerHandler(uiapp);
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                walkerWindow = new WalkerWindow(exEvent, handler);
                walkerWindow.Closed += WindowClosed;
                walkerWindow.Show();
            }
        }

        public void WakeWalkerUp()
        {
            if (walkerWindow != null)
            {
                walkerWindow.WakeUp();
            }
        }

        public void WindowClosed(object sender, System.EventArgs e)
        {
            walkerWindow = null;
        }

    }


    public static class AbortFlag
    {
        private static bool abortFlag = false;

        public static bool GetAbortFlag()
        {
            return abortFlag;
        }

        public static void SetAbortFlag(bool abort)
        {
            abortFlag = abort;
        }
    }
}
