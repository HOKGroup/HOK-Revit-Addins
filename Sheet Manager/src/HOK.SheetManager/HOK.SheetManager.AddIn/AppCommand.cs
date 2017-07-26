using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Utils;
using HOK.SheetManager.AddIn.Classes;
using HOK.SheetManager.AddIn.Updaters;
using HOK.SheetManager.AddIn.Utils;
using HOK.SheetManager.AddIn.Windows;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;

namespace HOK.SheetManager.AddIn
{
    public class AppCommand : IExternalApplication
    {
        internal static AppCommand thisApp;
        public MainWindow mainWindow;
        private const string tabName = "  HOK - Beta";
        private string versionNumber = "";
        public static AddInId addinId;
        public Dictionary<string, SheetManagerConfiguration> configDictionary = new Dictionary<string, SheetManagerConfiguration>();

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            try
            {
                thisApp = this;
                mainWindow = null;

                versionNumber = application.ControlledApplication.VersionNumber;
                addinId = application.ActiveAddInId;
                UpdaterUtil.addinId = addinId;

                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }

                var panel = application.CreateRibbonPanel(tabName, "Sheet Data");
                var currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

                var sheetImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.sync.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                var sheetButton = (PushButton)panel.AddItem(new PushButtonData("SheetDataManager", "Sheet Data Manager", currentAssembly, "HOK.SheetManager.AddIn.Command"));
                sheetButton.LargeImage = sheetImage;

                var instructionFile = @"V:\RVT-Data\HOK Program\Documentation\SheetManagerTools_Instruction.pdf";
                if (File.Exists(instructionFile))
                {
                    var contextualHelp = new ContextualHelp(ContextualHelpType.Url, instructionFile);
                    sheetButton.SetContextualHelp(contextualHelp);
                }

                application.ControlledApplication.DocumentOpened += DocumentOpened;
                application.ControlledApplication.DocumentClosing += DocumentClosing;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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

                application.ControlledApplication.DocumentOpened -= DocumentOpened;
                application.ControlledApplication.DocumentClosing -= DocumentClosing;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return Result.Succeeded;
        }

        public void ShowWindow(UIApplication uiapp)
        {
            try
            {
                if (mainWindow != null) return;

                var doc = uiapp.ActiveUIDocument.Document;
                var centralPath = RevitUtil.GetCentralFilePath(doc);
                SheetManagerConfiguration config;
                if (configDictionary.ContainsKey(centralPath))
                {
                    config = configDictionary[centralPath];
                }
                else
                {
                    config = new SheetManagerConfiguration(doc);
                    configDictionary.Add(config.CentralPath, config);
                }

                var viewModel = new AddInViewModel(config)
                {
                    Handler = new SheetManagerHandler(uiapp)
                };
                viewModel.ExtEvent = ExternalEvent.Create(viewModel.Handler);

                SheetUpdater.IsSheetManagerOn = true;
                RevisionUpdater.IsSheetManagerOn = true;

                mainWindow = new MainWindow
                {
                    DataContext = viewModel
                };
                mainWindow.Closed += WindowClosed;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
                var doc = args.Document;
                if (null == doc) return;

                var sheetConfig = new SheetManagerConfiguration(doc);
                if (doc.IsWorkshared)
                {
                    var configFound = ServerUtilities.GetConfigurationByCentralPath(sheetConfig.CentralPath);
                    if (null != configFound)
                    {
                        foreach (var updater in configFound.updaters)
                        {
                            if (updater.updaterName != "Sheet Tracker") continue;

                            sheetConfig.AutoUpdate = updater.isUpdaterOn;
                            sheetConfig.DatabaseFile = configFound.SheetDatabase;
                            break;
                        }
                    }
                }

                if (sheetConfig.AutoUpdate && !string.IsNullOrEmpty(sheetConfig.DatabaseFile))
                {
                    if (File.Exists(sheetConfig.DatabaseFile))
                    {
                        //update project info
                        var dbManager = new UpdaterDataManager(sheetConfig.DatabaseFile);
                        var projects = dbManager.GetLinkedProjects();
                        var projectFound = projects.Where(x => x.FilePath == sheetConfig.CentralPath).ToList();
                        if (projectFound.Any())
                        {
                            var linkedProject = projectFound.First();
                            sheetConfig.ModelId = linkedProject.Id;
                        }
                        else
                        {
                            var dbOpened = SheetDataWriter.OpenDatabase(sheetConfig.DatabaseFile);
                            if (dbOpened)
                            {
                                var linkedProject = new LinkedProject(sheetConfig.ModelId)
                                {
                                    FilePath = sheetConfig.CentralPath,
                                    ProjectNumber = doc.ProjectInformation.Number,
                                    ProjectName = doc.ProjectInformation.Name,
                                    LinkedBy = Environment.UserName,
                                    LinkedDate = DateTime.Now
                                };
                                SheetDataWriter.ChangeLinkedProject(linkedProject, CommandType.INSERT);
                                SheetDataWriter.CloseDatabse();
                            }
                        }

                        UpdaterUtil.RegisterUpdaters(doc, sheetConfig);
                    }
                }

                if (!configDictionary.ContainsKey(sheetConfig.CentralPath))
                {
                    configDictionary.Add(sheetConfig.CentralPath, sheetConfig);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Handles removal of updaters when Document closes.
        /// </summary>
        public void DocumentClosing(object sender, DocumentClosingEventArgs args)
        {
            try
            {
                var doc = args.Document;
                if (null == doc) return;

                var centralPath = RevitUtil.GetCentralFilePath(doc);
                if (!configDictionary.ContainsKey(centralPath)) return;

                var config = configDictionary[centralPath];
                UpdaterUtil.UnregisterUpdaters(doc, config);
                configDictionary.Remove(centralPath);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

    }
}
