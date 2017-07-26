using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Reflection;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using Autodesk.Revit.DB;

namespace HOK.RibbonTab
{
    public class AppCommand : IExternalApplication
    {
        private UIControlledApplication m_app;
        private string tabName = "";
        private string currentDirectory = "";
        private string currentAssembly = "";
        private Assembly assembly;
        private const string tooltipFileName = "HOK.Tooltip.txt";
        private readonly Dictionary<string, ButtonData> buttonDictionary = new Dictionary<string, ButtonData>();


        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpening += OnDocumentOpening;
            application.ControlledApplication.DocumentCreating += OnDocumentCreating;
            m_app = application;
            tabName = "   HOK   ";

            try
            {
                m_app.CreateRibbonTab(tabName);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            currentAssembly = Assembly.GetAssembly(GetType()).Location;
            currentDirectory = Path.GetDirectoryName(currentAssembly);
            assembly = Assembly.GetExecutingAssembly();
            var tooltipTxt = currentDirectory + "/Resources/" + tooltipFileName;
            ReadToolTips(tooltipTxt);

            CreateHOKPushButtons();
            CreateCustomPushButtons();
            //CreateDataPushButtons();
            CreateAVFPushButtons();
            //CreateMissionControlPushButtons();

            return Result.Succeeded;
        }

        private static void OnDocumentCreating(object sender, DocumentCreatingEventArgs args)
        {
            try
            {
                Log.Initialize("HOK_Tools", "New Document");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private static void OnDocumentOpening(object source, DocumentOpeningEventArgs args)
        {
            try
            {
                var pathName = args.PathName;
                if (string.IsNullOrEmpty(pathName) || args.DocumentType != DocumentType.Project) return;

                var fileInfo = BasicFileInfo.Extract(pathName);
                var centralPath = fileInfo.CentralPath;
                Log.Initialize("HOK_Tools", string.IsNullOrEmpty(centralPath) ? string.Empty : centralPath);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpening -= OnDocumentOpening;

            Log.WriteLog();
            return Result.Succeeded;
        }

        //private void CreateMissionControlPushButtons()
        //{
        //    try
        //    {
        //        // TODO: It would be nice to automatically prompt user once a week, to run this export.
        //        var missionControlPanel = m_app.CreateRibbonPanel(tabName, "Mission Control");
        //        var assemblyPath = currentDirectory + "/HOK.MissionControl.FamilyPublish.dll";

        //        var pb1 = new PushButtonData(
        //            "PublishFamilyDataCommand",
        //            "Publish Family" + Environment.NewLine + "Data",
        //            assemblyPath,
        //            "HOK.MissionControl.FamilyPublish.FamilyPublishCommand")
        //        {
        //            ToolTip = "Mission Control Family Export Tool."
        //        };
        //        var fpAssembly = Assembly.LoadFrom(assemblyPath);
        //        pb1.LargeImage = ButtonUtil.LoadBitmapImage(fpAssembly, "HOK.MissionControl.FamilyPublish", "publishFamily_32x32.png");

        //        missionControlPanel.AddItem(pb1);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
        //    }
        //}

        /// <summary>
        /// Creates all of the Utilities buttons.
        /// </summary>
        private void CreateHOKPushButtons()
        {
            try
            {
                var utilityExist = false;
                var utilPanel = m_app.CreateRibbonPanel(tabName, "Utilities");

                if (File.Exists(currentDirectory + "/HOK.ElementTools.dll") || File.Exists(currentDirectory + "/HOK.ParameterTools.dll"))
                {
                    var splitButtonData = new SplitButtonData("HOKUtilities", "HOK Utilities");
                    var splitButton = (SplitButton)utilPanel.AddItem(splitButtonData);
                    var contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\HOK Utilities_Instruction.pdf");
                    splitButton.SetContextualHelp(contextualHelp);

                    if (File.Exists(currentDirectory + "/HOK.ElementTools.dll"))
                    {
                        var pb1 = splitButton.AddPushButton(new PushButtonData("Element Tools", "Element Tools", currentDirectory + "/HOK.ElementTools.dll", "HOK.ElementTools.cmdElementTools"));
                        //pb1.LargeImage = LoadBitmapImage(assembly, "element.ico");
                        pb1.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "element.ico");
                        pb1.ToolTip = "Room and Area Elements Tools";
                        AddToolTips(pb1);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.ParameterTools.dll"))
                    {
                        var pb2 = splitButton.AddPushButton(new PushButtonData("Parameter Tools", "Parameter Tools", currentDirectory + "/HOK.ParameterTools.dll", "HOK.ParameterTools.cmdParameterTools"));
                        pb2.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "parameter.ico");
                        pb2.ToolTip = "Parameter Tools";
                        AddToolTips(pb2);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.FinishCreator.dll"))
                    {
                        var pb3 = splitButton.AddPushButton(new PushButtonData("Finish Creator", "Finish Creator", currentDirectory + "/HOK.FinishCreator.dll", "HOK.FinishCreator.FinishCommand"));
                        pb3.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "finish.png");
                        pb3.ToolTip = "Create floor finishes from the selected rooms.";
                        AddToolTips(pb3);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.CeilingHeight.dll"))
                    {
                        var pb4 = splitButton.AddPushButton(new PushButtonData("Ceiling Height", "Ceiling Heights", currentDirectory + "/HOK.CeilingHeight.dll", "HOK.CeilingHeight.CeilingCommand"));
                        pb4.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "height.png");
                        pb4.ToolTip = "Select rooms to measure the height from floors to ceilings.";
                        AddToolTips(pb4);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.LevelManager.dll"))
                    {
                        var pb5 = splitButton.AddPushButton(new PushButtonData("Level Manager", "Level Manager", currentDirectory + "/HOK.LevelManager.dll", "HOK.LevelManager.LevelCommand"));
                        pb5.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "level.png");
                        pb5.ToolTip = "Rehost elements from one level to anather. ";
                        AddToolTips(pb5);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.ViewDepth.dll"))
                    {
                        var pb18 = splitButton.AddPushButton(new PushButtonData("View Depth", "View Depth", currentDirectory + "/HOK.ViewDepth.dll", "HOK.ViewDepth.ViewCommand"));
                        pb18.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "camera.ico");
                        pb18.ToolTip = "Override the graphics of the element based on the distance";
                        pb18.ToolTipImage = LoadBitmapImage(assembly, "viewTooltip.png");
                        AddToolTips(pb18);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.Arrowhead.dll"))
                    {
                        var pb19 = splitButton.AddPushButton(new PushButtonData("Leader Arrowhead", "Leader Arrowhead", currentDirectory + "/HOK.Arrowhead.dll", "HOK.Arrowhead.ArrowCommand"));
                        pb19.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "arrowhead_32.png");
                        pb19.ToolTip = "Assign a leader arrowhead style to all tag types.";
                        AddToolTips(pb19);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.WorksetView.dll"))
                    {
                        var pb19 = splitButton.AddPushButton(new PushButtonData("View Creator", "View Creator", currentDirectory + "/HOK.WorksetView.dll", "HOK.WorksetView.WorksetCommand"));
                        pb19.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "workset.png");
                        pb19.ToolTip = "Create 3D Views for each workset.";
                        AddToolTips(pb19);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.DoorRoom.dll"))
                    {
                        var pb21 = splitButton.AddPushButton(new PushButtonData("Door Link", "Door Link", currentDirectory + "/HOK.DoorRoom.dll", "HOK.DoorRoom.DoorCommand"));
                        pb21.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "doorTool_32.png");
                        pb21.ToolTip = "Set shared parameters with To and From room data.";
                        AddToolTips(pb21);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.RoomUpdater.dll"))
                    {
                        var pb22 = splitButton.AddPushButton(new PushButtonData("Room Updater", "Room Updater", currentDirectory + "/HOK.RoomUpdater.dll", "HOK.RoomUpdater.RoomCommand"));
                        pb22.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "container.png");
                        pb22.ToolTip = "Populate room parameters values into enclosed elements.";
                        AddToolTips(pb22);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.RoomElevation.dll"))
                    {
                        var pb23 = splitButton.AddPushButton(new PushButtonData("Room Elevation", "Room Elevation", currentDirectory + "/HOK.RoomElevation.dll", "HOK.RoomElevation.ElevationCommand"));
                        pb23.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "elevation.png");
                        pb23.ToolTip = "Create elevation views by selecting rooms and walls to be faced.";
                        AddToolTips(pb23);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.CameraDuplicator.dll"))
                    {
                        var pb25 = splitButton.AddPushButton(new PushButtonData("View Mover", "View Mover", currentDirectory + "/HOK.CameraDuplicator.dll", "HOK.CameraDuplicator.CameraCommand"));
                        pb25.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "cameraview.png");
                        pb25.ToolTip = "Duplicate camera views of plan views from one project to the other.";
                        AddToolTips(pb25);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.RenameFamily.dll"))
                    {
                        var pb26 = splitButton.AddPushButton(new PushButtonData("Rename Family", "Rename Family", currentDirectory + "/HOK.RenameFamily.dll", "HOK.RenameFamily.RenameCommand"));
                        pb26.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "update.png");
                        pb26.ToolTip = "Rename families and types as assigned in .csv file.";
                        AddToolTips(pb26);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.XYZLocator.dll"))
                    {
                        var pb27 = splitButton.AddPushButton(new PushButtonData("XYZ Locator", "XYZ Locator", currentDirectory + "/HOK.XYZLocator.dll", "HOK.XYZLocator.XYZCommand"));
                        pb27.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "location.ico");
                        pb27.ToolTip = "Report location of a 3D family using shared coordinates";
                        AddToolTips(pb27);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.RoomMeasure.dll"))
                    {
                        var pb28 = splitButton.AddPushButton(new PushButtonData("Room W X L", "Room W X L", currentDirectory + "/HOK.RoomMeasure.dll", "HOK.RoomMeasure.MeasureCommand"));
                        pb28.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "kruler.png");
                        pb28.ToolTip = "Measuring the width and length of all rooms in the project"; 
                        AddToolTips(pb28);
                        utilityExist = true;
                    }

                }
                if (!utilityExist)
                {
                    utilPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void CreateCustomPushButtons()
        {
            try
            {
                var fileExist = false;
                var created = m_app.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Customizations");
                var hokPanel = created ?? m_app.CreateRibbonPanel(tabName, "Customizations");

                if (File.Exists(currentDirectory + "/HOK.SheetManager.dll"))
                {
                    var pb6 = (PushButton)hokPanel.AddItem(new PushButtonData("Sheet Manager", "Sheet" + Environment.NewLine + " Manager", currentDirectory + "/HOK.SheetManager.dll", "HOK.SheetManager.cmdSheetManager"));
                    pb6.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "sheetManager_32.png");
                    pb6.ToolTip = "Sheet Manager";
                    AddToolTips(pb6);
                    fileExist = true;
                }

                if (File.Exists(currentDirectory + "/HOK.ElementFlatter.dll"))
                {
                    var efAssemblyPath = currentDirectory + "/HOK.ElementFlatter.dll";
                    var pbFlatter = (PushButton) hokPanel.AddItem(new PushButtonData("FlattenCommand", "Flatten" + Environment.NewLine + "Model", efAssemblyPath, "HOK.ElementFlatter.Command"));
                    pbFlatter.ToolTip = "Element Flatter";
                    var efAssembly = Assembly.LoadFrom(efAssemblyPath);
                    pbFlatter.LargeImage = ButtonUtil.LoadBitmapImage(efAssembly, "HOK.ElementFlatter", "elementFlattener_32.png");
                    AddToolTips(pbFlatter);
                    fileExist = true;
                }

                //if (File.Exists(currentDirectory + "/HOK.ModelManager.dll"))
                //{
                //    var splitButtonData = new SplitButtonData("ModelManager", "Model Manager");
                //    var splitButton = (SplitButton)hokPanel.AddItem(splitButtonData);
                //    var contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\ModelManager_Instruction.pdf");
                //    splitButton.SetContextualHelp(contextualHelp);

                //    var pb16 = splitButton.AddPushButton(new PushButtonData("Project Replication", "Project Replication", currentDirectory + "/HOK.ModelManager.dll", "HOK.ModelManager.ProjectCommand"));
                //    pb16.LargeImage = LoadBitmapImage(assembly, "project.png");
                //    pb16.ToolTip = "Model Manager - Project Replication";
                //    AddToolTips(pb16);
                //    hokPanel.AddSeparator();
                //    fileExist = true;
                //}

                if (File.Exists(currentDirectory + "/HOK.RoomsToMass.dll"))
                {
                    var splitButtonData = new SplitButtonData("MassTool", "3D Mass");
                    var splitButton = (SplitButton)hokPanel.AddItem(splitButtonData);
                    splitButton.IsSynchronizedWithCurrentItem = true;
                    var contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\Mass Tool_Instruction.pdf");
                    splitButton.SetContextualHelp(contextualHelp);

                    var pb8 = splitButton.AddPushButton(new PushButtonData("Create Mass", "Create Mass", currentDirectory + "/HOK.RoomsToMass.dll", "HOK.RoomsToMass.Command"));
                    //pb8.LargeImage = LoadBitmapImage(assembly, "createMass_32.png");
                    pb8.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "createMass_32.png");
                    pb8.ToolTip = "Creates 3D Mass from rooms, areas and floors.";
                    pb8.ToolTipImage = LoadBitmapImage(assembly, "tooltip.png");
                    AddToolTips(pb8);

                    var pb10 = splitButton.AddPushButton(new PushButtonData("Mass Commands", "Mass Commands", currentDirectory + "/HOK.RoomsToMass.dll", "HOK.RoomsToMass.AssignerCommand"));
                    //pb10.LargeImage = LoadBitmapImage(assembly, "massCommands_32.png");
                    pb10.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "massCommands_32.png");
                    pb10.ToolTip = "Assign parameters or split elements";
                    AddToolTips(pb10);
                    fileExist = true;
                }

                if (!fileExist)
                {
                    hokPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        //private void CreateDataPushButtons()
        //{
        //    try
        //    {
        //        if (!File.Exists(currentDirectory + "/HOK.RevitDBManager.dll")) return;

        //        var dataPanel = m_app.CreateRibbonPanel(tabName, "Revit Data");

        //        var pb11 = (PushButton)dataPanel.AddItem(new PushButtonData("Data Sync", "Data Sync", currentDirectory + "/HOK.RevitDBManager.dll", "RevitDBManager.Command"));
        //        pb11.LargeImage = LoadBitmapImage(assembly, "sync.ico");
        //        pb11.ToolTip = "Data Sync";
        //        AddToolTips(pb11);

                //var pb12 = (PushButton)dataPanel.AddItem(new PushButtonData("Setup", "  Setup  ", currentDirectory + "/HOK.RevitDBManager.dll", "RevitDBManager.EditorCommand"));
                //pb12.LargeImage = LoadBitmapImage(assembly, "editor.ico");
                //pb12.ToolTip = "Setup";
                //AddToolTips(pb12);

        //        var pb13 = (PushButton)dataPanel.AddItem(new PushButtonData("Data Editor", "Data Editor", currentDirectory + "/HOK.RevitDBManager.dll", "RevitDBManager.ViewerCommand"));
        //        pb13.LargeImage = LoadBitmapImage(assembly, "view.ico");
        //        pb13.ToolTip = "Data Editor";
        //        AddToolTips(pb13);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
        //    }
        //}

        /// <summary>
        /// Creates all Analysis related buttons.
        /// </summary>
        private void CreateAVFPushButtons()
        {
            try
            {
                if (!File.Exists(currentDirectory + "/HOK.AVFManager.dll") &&
                    !File.Exists(currentDirectory + "/HOK.LPDCalculator.dll") &&
                    !File.Exists(currentDirectory + "/HOK.ViewAnalysis.dll")) return;
                var avfPanel = m_app.CreateRibbonPanel(tabName, "Analysis");

                var splitButtonData = new SplitButtonData("HOKAnalysis", "HOK Analysis");
                var splitButton = (SplitButton)avfPanel.AddItem(splitButtonData);
                splitButton.IsSynchronizedWithCurrentItem = true;

                if (File.Exists(currentDirectory + "/HOK.AVFManager.dll"))
                {
                    var pb14 = splitButton.AddPushButton(new PushButtonData("AVF", "  AVF  ", currentDirectory + "/HOK.AVFManager.dll", "HOK.AVFManager.Command"));
                    pb14.LargeImage = LoadBitmapImage(assembly, "chart.ico");
                    pb14.ToolTip = "Analysis Visualization Framework";
                    AddToolTips(pb14);
                }

                if (File.Exists(currentDirectory + "/HOK.LPDCalculator.dll"))
                {
                    var assemblyPath = currentDirectory + "/HOK.LPDCalculator.dll";
                    var ass = Assembly.LoadFrom(assemblyPath);
                    var pb15 = splitButton.AddPushButton(new PushButtonData("LPD Analysis", "LPD Analysis", currentDirectory + "/HOK.LPDCalculator.dll", "HOK.LPDCalculator.Command"));
                    pb15.LargeImage = ButtonUtil.LoadBitmapImage(ass, "HOK.LPDCalculator", "lpdCalculator_32.png");
                    pb15.ToolTip = "Calculating Lighting Power Density";
                    AddToolTips(pb15);
                }

                if (!File.Exists(currentDirectory + "/HOK.ViewAnalysis.dll")) return;

                var pb24 = splitButton.AddPushButton(new PushButtonData("LEED View Analysis", "LEED View Analysis", currentDirectory + "/HOK.ViewAnalysis.dll", "HOK.ViewAnalysis.Command"));
                pb24.LargeImage = LoadBitmapImage(assembly, "eq.ico");
                pb24.ToolTip = "Calculating Area with Views for LEED IEQc 8.2";
                AddToolTips(pb24);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Reads TXT file containing all of the Tooltip descriptions.
        /// </summary>
        /// <param name="txtfile">Path to Tooltips file.</param>
        private void ReadToolTips(string txtfile)
        {
            try
            {
                using (var reader = File.OpenText(txtfile))
                {
                    string line;
                    var index = 0;
                    var buttonData = new ButtonData();
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("------------------")) { buttonData = new ButtonData(); continue; }

                        switch (index % 3)
                        {
                            case 0:
                                buttonData.ButtonName = line;
                                index++;
                                break;
                            case 1:
                                buttonData.Description = line;
                                index++;
                                break;
                            case 2:
                                buttonData.HelpUrl = line;
                                buttonDictionary.Add(buttonData.ButtonName, buttonData);
                                index = 0;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void AddToolTips(PushButton button)
        {
            try
            {
                if (!buttonDictionary.ContainsKey(button.Name)) return;

                var buttonData = buttonDictionary[button.Name];
                button.LongDescription = buttonData.Description;
                var contextualHelp = new ContextualHelp(ContextualHelpType.Url, buttonData.HelpUrl);
                button.SetContextualHelp(contextualHelp);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private static BitmapImage LoadBitmapImage(Assembly currentAssembly, string imageName)
        {
            var image = new BitmapImage();
            try
            {
                var prefix = typeof(AppCommand).Namespace + ".Resources.";
                var stream = currentAssembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return image;
        }
    }

    /// <summary>
    /// Placeholder Button Data generated from TXT
    /// </summary>
    internal class ButtonData
    {
        public string ButtonName { get; set; }
        public string Description { get; set; }
        public string HelpUrl { get; set; }
    }

    /// <summary>
    /// Determines if Button is enabled in ZeroDocument state.
    /// </summary>
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true;
        }
    }
}
