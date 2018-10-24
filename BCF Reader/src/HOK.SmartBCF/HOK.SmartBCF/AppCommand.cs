#region References

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.SmartBCF.Walker;

#endregion

namespace HOK.SmartBCF
{
    public class AppCommand : IExternalApplication
    {
        internal static AppCommand thisApp;
        private WalkerWindow walkerWindow;
        private const string tabName = "  HOK - Beta";

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

            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var rp = application.CreateRibbonPanel(tabName, "BCF");
            var currentAssembly = Assembly.GetAssembly(GetType()).Location;
            var walkerImage = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.walker.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            var walkerButton = rp.AddItem(new PushButtonData("smartBCF", "smartBCF", currentAssembly, "HOK.SmartBCF.BCFCommand")) as PushButton;
            walkerButton.LargeImage = walkerImage;

            return Result.Succeeded;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = new AssemblyName(args.Name);
                var assemblyNames = new[] { "System.Net.Http.Primitives", "Newtonsoft.Json" };
                if (assemblyNames.Contains(name.Name))
                {
                    var currentAssembly = Assembly.GetAssembly(GetType()).Location;
                    var assemblyToLoad = Path.Combine(Path.GetDirectoryName(currentAssembly), name.Name+".dll");
                    var assemblyLoaded = Assembly.LoadFrom(assemblyToLoad);

                    return assemblyLoaded;
                }
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                MessageBox.Show("Failed to load assembly.\n" + e.Message);
            }
            return null;
        }

        /// <summary>
        /// Shows the Walker Window
        /// </summary>
        /// <param name="uiapp"></param>
        public void ShowWalker(UIApplication uiapp)
        {
            if (walkerWindow != null) return;

            var handler = new WalkerHandler(uiapp);
            var exEvent = ExternalEvent.Create(handler);

            walkerWindow = new WalkerWindow(exEvent, handler);
            walkerWindow.Closed += WindowClosed;
            walkerWindow.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public void WakeWalkerUp()
        {
            walkerWindow?.WakeUp();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WindowClosed(object sender, EventArgs e)
        {
            // TODO: Remove event handler?
            walkerWindow = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class AbortFlag
    {
        private static bool abortFlag;

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
