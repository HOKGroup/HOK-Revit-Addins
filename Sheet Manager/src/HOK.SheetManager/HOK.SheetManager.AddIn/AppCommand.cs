using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using HOK.SheetManager.AddIn.Classes;
using HOK.SheetManager.AddIn.Updaters;
using HOK.SheetManager.AddIn.Utils;
using HOK.SheetManager.AddIn.Windows;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HOK.SheetManager.AddIn
{
    public class AppCommand :IExternalApplication
    {
        internal static AppCommand thisApp = null;
        public MainWindow mainWindow = null;
        private string tabName = "  HOK - Beta";
        private string versionNumber = "";
        public static AddInId addinId = null;

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            try
            {
                thisApp = this;
                mainWindow = null;

                versionNumber = application.ControlledApplication.VersionNumber;
                addinId = application.ActiveAddInId;
                UpdaterUtil.addinId = addinId;

                try { application.CreateRibbonTab(tabName); }
                catch { }

                RibbonPanel panel = application.CreateRibbonPanel(tabName, "Sheet Data");
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

                BitmapSource sheetImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.sync.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                PushButton sheetButton = panel.AddItem(new PushButtonData("SheetDataManager", "Sheet Data Manager", currentAssembly, "HOK.SheetManager.AddIn.Command")) as PushButton;
                sheetButton.LargeImage = sheetImage;
                sheetButton.AvailabilityClassName = "HOK.SheetManager.AddIn.Availability";

                string instructionFile = @"V:\RVT-Data\HOK Program\Documentation\SheetManagerTools_Instruction.pdf";
                if (File.Exists(instructionFile))
                {
                    ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, instructionFile);
                    sheetButton.SetContextualHelp(contextualHelp);
                }

                //application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(DocumentOpened);
                //application.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(DocumentClosing);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return Result.Succeeded;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            try
            {
                if (mainWindow != null && mainWindow.IsVisible)
                {
                    mainWindow.Close();
                }

                //application.ControlledApplication.DocumentOpened -= new EventHandler<DocumentOpenedEventArgs>(DocumentOpened);
                //application.ControlledApplication.DocumentClosing -= new EventHandler<DocumentClosingEventArgs>(DocumentClosing);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return Result.Succeeded;
        }

        public void ShowWindow(UIApplication uiapp)
        {
            try
            {
                if (mainWindow == null)
                {
                    Document doc = uiapp.ActiveUIDocument.Document;
                    SheetManagerConfiguration config = DataStorageUtil.GetConfiguration(doc);
                    AddInViewModel viewModel = new AddInViewModel(config);
                    viewModel.Handler = new SheetManagerHandler(uiapp);
                    viewModel.ExtEvent = ExternalEvent.Create(viewModel.Handler);

                    SheetUpdater.IsSheetManagerOn = true;
                    RevisionUpdater.IsSheetManagerOn = true;

                    mainWindow = new MainWindow();
                    mainWindow.DataContext = viewModel;
                    mainWindow.Closed += WindowClosed;
                    mainWindow.Show();
                   
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void WindowClosed(object sender, System.EventArgs e)
        {
            mainWindow = null;

            SheetUpdater.IsSheetManagerOn = false;
            RevisionUpdater.IsSheetManagerOn = false;
        }

        public void DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            try
            {
                Document doc = args.Document;
                if (null != doc)
                {
                    SheetManagerConfiguration config = DataStorageUtil.GetConfiguration(doc);
                    if (config.AutoUpdate && !string.IsNullOrEmpty(config.DatabaseFile))
                    {
                        if (File.Exists(config.DatabaseFile))
                        {
                            //register updater
                            bool registered = UpdaterUtil.RegisterUpdaters(doc, config);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to trigger the document opened event.\n" + ex.Message, "Document Opened Event", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DocumentClosing(object sender, DocumentClosingEventArgs args)
        {
            try
            {
                Document doc = args.Document;
                if (null != doc)
                {
                    //unregister updater
                    SheetManagerConfiguration config = DataStorageUtil.GetConfiguration(doc);
                    bool unregistered = UpdaterUtil.UnregisterUpdaters(doc, config);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unregister updater.\n" + ex.Message, "Sheet Manager : Unregister Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }

    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }
}
