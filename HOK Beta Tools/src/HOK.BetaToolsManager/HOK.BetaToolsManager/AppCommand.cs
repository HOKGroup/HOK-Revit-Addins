using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Autodesk.Revit.UI;

namespace HOK.BetaToolsManager
{
    public enum ToolEnum
    {
        None = 0,
        ElementTools,
        ParameterTools,
        SheetManager,
        BCFReader,
        MassTool,
        RevitData,
        AVF,
        LPDAnalysis,
        LEEDView,
        Utility,
        ColorEditor, 
        SmartBCF,
        ElementFlatter
    }

    public class AppCommand : IExternalApplication
    {
        private UIControlledApplication m_app;
        internal static AppCommand thisApp;
        private const string tabName = "  HOK - Beta";
        private string versionNumber = "";
        private ToolManager toolManager;
        private Dictionary<ToolEnum, ToolProperties> ToolInfoDictionary = new Dictionary<ToolEnum, ToolProperties>();
        private readonly Dictionary<string, ButtonData> ButtonDictionary = new Dictionary<string, ButtonData>();
        private string currentDirectory = "";
        private string currentAssembly = "";
        private const string tooltipFileName = "HOK.Tooltip.txt";

        private RibbonPanel utilityPanel;
        private SplitButton utilitySplitButton;
        private RibbonPanel customizePanel;
        private SplitButton massSplitButton;
        private RibbonPanel dataPanel;
        private RibbonPanel analysisPanel;
        private SplitButton analysisSplitButton;
        private RibbonPanel shapePanel;

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            thisApp = this;

            m_app = application;
            versionNumber = m_app.ControlledApplication.VersionNumber;
            toolManager = new ToolManager(versionNumber);
            ToolInfoDictionary = toolManager.ToolInfoDictionary;

            try
            {
                m_app.CreateRibbonTab(tabName);
            }
            catch
            {
                // ignored
            }

            currentAssembly = System.Reflection.Assembly.GetAssembly(GetType()).Location;
            currentDirectory = Path.GetDirectoryName(currentAssembly);
            var tooltipTxt = currentDirectory + "/Resources/" + tooltipFileName;
            ReadToolTips(tooltipTxt);

            CreateInstallerPanel();
            if (!ToolInfoDictionary.Any()) return Result.Succeeded;

            CreateUtilitiesPanel();
            CreateCustomPanel();
            CreateDataPanel();
            CreateAnalysisPanel();
            CreateDirectShapePanel();

            return Result.Succeeded;
        }

        public void ShowInstaller(UIApplication uiapp)
        {
            var installerWindow = new BetaInstallerWindow(versionNumber, ToolInfoDictionary);
            if (installerWindow.ShowDialog() != true) return;

            ToolInfoDictionary = installerWindow.ToolInfoDictionary;
            CreateUtilitiesPanel();
            CreateCustomPanel();
            CreateDataPanel();
            CreateAnalysisPanel();
            CreateDirectShapePanel();
        }

        private static Dictionary<string, PushButton> CheckExistingButtons(SplitButton splitButton)
        {
            var pushButtons = new Dictionary<string, PushButton>();
            try
            {
                var buttons = splitButton.GetItems().ToList();
                foreach (var bttn in buttons)
                {
                    if (!pushButtons.ContainsKey(bttn.Name))
                    {
                        bttn.Enabled = true; //set true as default
                        pushButtons.Add(bttn.Name, bttn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get names of push buttons.\n"+ex.Message, "Check Exisiting Buttons", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return pushButtons;
        }

        private static Dictionary<string, PushButton> CheckExistingButtons(RibbonPanel ribbonPanel)
        {
            var pushButtons = new Dictionary<string, PushButton>();
            try
            {
                var ribbonItems = ribbonPanel.GetItems().ToList();
                foreach (var item in ribbonItems)
                {
                    var button = item as PushButton;
                    if (null != button)
                    {
                        button.Enabled = true; //set true as default
                        pushButtons.Add(button.Name, button);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get names of buttons.\n"+ex.Message , "Check Exisiting Buttons", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return pushButtons;
        }

        private void CreateInstallerPanel()
        {
            try
            {
                var installerPanel = m_app.CreateRibbonPanel(tabName, "Installer");

                var installerButton = (PushButton)installerPanel.AddItem(new PushButtonData("Beta Installer", "Beta Installer", currentAssembly, "HOK.BetaToolsManager.InstallerCommand"));
                installerButton.LargeImage = ImageUtil.LoadBitmapImage("settings.png");
                installerButton.Image = installerButton.LargeImage;
                installerButton.ToolTip = "HOK Beta Tools Installer";
                installerButton.AvailabilityClassName = "HOK.BetaToolsManager.Availability";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create Installer Panel.\n" + ex.Message, "Create Installer Panel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateUtilitiesPanel()
        {
            try
            {
                ToolProperties utilitiesTP = null; //utilities tool properties
                ToolProperties elementTP = null; //element tools properties
                ToolProperties parameterTP = null; // Parameter Tools Properties
                ToolProperties colorTP = null; //color editor properties
                
                if (ToolInfoDictionary.ContainsKey(ToolEnum.Utility) && ToolInfoDictionary.ContainsKey(ToolEnum.ElementTools) && ToolInfoDictionary.ContainsKey(ToolEnum.ParameterTools) && ToolInfoDictionary.ContainsKey(ToolEnum.ColorEditor))
                {
                    utilitiesTP = ToolInfoDictionary[ToolEnum.Utility];
                    elementTP = ToolInfoDictionary[ToolEnum.ElementTools];
                    parameterTP = ToolInfoDictionary[ToolEnum.ParameterTools];
                    colorTP = ToolInfoDictionary[ToolEnum.ColorEditor];
                }

                if (null != utilitiesTP && null != elementTP && null != parameterTP && null != colorTP)
                {
                    if (utilitiesTP.InstallExist || elementTP.InstallExist || parameterTP.InstallExist || colorTP.InstallExist)
                    {
                        if (null == utilityPanel && null == utilitySplitButton)
                        {
                            utilityPanel = m_app.CreateRibbonPanel(tabName, "Utilities");

                            var splitButtonData = new SplitButtonData("HOKUtilities", "HOK Utilities");
                            utilitySplitButton = (SplitButton)utilityPanel.AddItem(splitButtonData);
                            var contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\HOK Utilities_Instruction.pdf");
                            utilitySplitButton.SetContextualHelp(contextualHelp);
                        }
                    }
                    if (null != utilityPanel && null != utilitySplitButton)
                    {
                        var utilityButtons = CheckExistingButtons(utilitySplitButton);

                        if (elementTP.InstallExist)
                        {
                            
                            if (!utilityButtons.ContainsKey("Element Tools"))
                            {
                                //install
                                var elementButton = utilitySplitButton.AddPushButton(new PushButtonData("Element Tools", "Element Tools", elementTP.TempAssemblyPath, "HOK.ElementTools.cmdElementTools"));
                                elementButton.LargeImage = ImageUtil.LoadBitmapImage("element.ico");
                                elementButton.ToolTip = "Room and Area Elements Tools";
                                AddToolTips(elementButton);
                            }
                            else
                            {
                                //update
                                var elementButton = utilityButtons["Element Tools"];
                                elementButton.AssemblyName = elementTP.TempAssemblyPath;
                            }
                            
                        }
                        else if (!elementTP.InstallExist && utilityButtons.ContainsKey("Element Tools"))
                        {
                            //removed tools by beta installer
                            var elementButton = utilityButtons["Element Tools"];
                            elementButton.Enabled = false;
                        }

                        if (parameterTP.InstallExist)
                        {
                            if (!utilityButtons.ContainsKey("Parameter Tools"))
                            {
                                //install
                                var parameterButton = utilitySplitButton.AddPushButton(new PushButtonData("Parameter Tools", "Parameter Tools", parameterTP.TempAssemblyPath, "HOK.ParameterTools.cmdParameterTools"));
                                parameterButton.LargeImage = ImageUtil.LoadBitmapImage("parameter.ico");
                                parameterButton.ToolTip = "Parameter Tools";
                                AddToolTips(parameterButton);
                            }
                            else
                            {
                                //update
                                var parameterButton = utilityButtons["Parameter Tools"];
                                parameterButton.AssemblyName = parameterTP.TempAssemblyPath;
                            }
                            
                        }
                        else if (!parameterTP.InstallExist && utilityButtons.ContainsKey("Parameter Tools"))
                        {
                            var parameterButton = utilityButtons["Parameter Tools"];
                            parameterButton.Enabled = false;
                        }

                        if (utilitiesTP.InstallExist)
                        {
                            var directoryName = Path.GetDirectoryName(utilitiesTP.InstallPath);
                            var finishPath = Path.Combine(directoryName, "HOK.FinishCreator.dll");
                            finishPath = GetTempInstallPath(finishPath);
                            if (File.Exists(finishPath))
                            {
                                if (!utilityButtons.ContainsKey("Finish Creator"))
                                {
                                    //install
                                    var finishButton = utilitySplitButton.AddPushButton(new PushButtonData("Finish Creator", "Finish Creator", finishPath, "HOK.FinishCreator.FinishCommand"));
                                    finishButton.LargeImage = ImageUtil.LoadBitmapImage("finish.png");
                                    finishButton.ToolTip = "Create floor finishes from the selected rooms.";
                                    AddToolTips(finishButton);
                                }
                                else
                                {
                                    //update
                                    var finishButton = utilityButtons["Finish Creator"];
                                    finishButton.AssemblyName = finishPath;
                                }
                            }

                            var ceilingPath =Path.Combine(directoryName, "HOK.CeilingHeight.dll");
                            ceilingPath = GetTempInstallPath(ceilingPath);
                            if (File.Exists(ceilingPath))
                            {
                                if (!utilityButtons.ContainsKey("Ceiling Height"))
                                {
                                    var ceilingButton = utilitySplitButton.AddPushButton(new PushButtonData("Ceiling Height", "Ceiling Heights", ceilingPath, "HOK.CeilingHeight.CeilingCommand"));
                                    ceilingButton.LargeImage = ImageUtil.LoadBitmapImage("height.png");
                                    ceilingButton.ToolTip = "Select rooms to measure the height from floors to ceilings.";
                                    AddToolTips(ceilingButton);
                                    //install
                                }
                                else
                                {
                                    //update
                                    var ceilingButton = utilityButtons["Ceiling Height"];
                                    ceilingButton.AssemblyName = ceilingPath;
                                }
                            }

                            var levelPath = Path.Combine(directoryName, "HOK.LevelManager.dll");
                            levelPath = GetTempInstallPath(levelPath);
                            if (File.Exists(levelPath))
                            {
                                if (!utilityButtons.ContainsKey("Level Manager"))
                                {
                                    var levelButton = utilitySplitButton.AddPushButton(new PushButtonData("Level Manager", "Level Manager", levelPath, "HOK.LevelManager.LevelCommand"));
                                    levelButton.LargeImage = ImageUtil.LoadBitmapImage("level.png");
                                    levelButton.ToolTip = "Rehost elements from one level to anather. ";
                                    AddToolTips(levelButton);
                                }
                                else
                                {
                                    var levelButton = utilityButtons["Level Manager"];
                                    levelButton.AssemblyName = levelPath;
                                }
                            }
#if RELEASE2013
#else
                            var viewPath = Path.Combine(directoryName, "HOK.ViewDepth.dll");
                            viewPath = GetTempInstallPath(viewPath);
                            if (File.Exists(viewPath))
                            {
                                if (!utilityButtons.ContainsKey("View Depth"))
                                {
                                    //install
                                    var viewButton = utilitySplitButton.AddPushButton(new PushButtonData("View Depth", "View Depth", viewPath, "HOK.ViewDepth.ViewCommand"));
                                    viewButton.LargeImage = ImageUtil.LoadBitmapImage("camera.ico");
                                    viewButton.ToolTip = "Override the graphics of the element based on the distance";
                                    viewButton.ToolTipImage = ImageUtil.LoadBitmapImage("viewTooltip.png");
                                    AddToolTips(viewButton);
                                }
                                else
                                {
                                    //update
                                    var viewButton = utilityButtons["View Depth"];
                                    viewButton.AssemblyName = viewPath;
                                }
                            }
#endif

                            var arrowPath = Path.Combine( directoryName, "HOK.Arrowhead.dll"); 
                            arrowPath = GetTempInstallPath(arrowPath);
                            if (File.Exists(arrowPath))
                            {
                                if (!utilityButtons.ContainsKey("Leader Arrowhead"))
                                {
                                    //install
                                    var leaderButton = utilitySplitButton.AddPushButton(new PushButtonData("Leader Arrowhead", "Leader Arrowhead", arrowPath, "HOK.Arrowhead.ArrowCommand"));
                                    leaderButton.LargeImage = ImageUtil.LoadBitmapImage("arrowhead.png");
                                    leaderButton.ToolTip = "Assign a leader arrowhead style to all tag types.";
                                    AddToolTips(leaderButton);
                                }
                                else
                                {
                                    //update
                                    var leaderButton = utilityButtons["Leader Arrowhead"];
                                    leaderButton.AssemblyName = arrowPath;
                                }
                            }

                            var worksetPath = Path.Combine(directoryName, "HOK.WorksetView.dll"); 
                            worksetPath = GetTempInstallPath(worksetPath);
                            if (File.Exists(worksetPath))
                            {
                                if (!utilityButtons.ContainsKey("View Creator"))
                                {
                                    //install
                                    var worksetButton = utilitySplitButton.AddPushButton(new PushButtonData("View Creator", "View Creator", worksetPath, "HOK.WorksetView.WorksetCommand"));
                                    worksetButton.LargeImage = ImageUtil.LoadBitmapImage("workset.png");
                                    worksetButton.ToolTip = "Create 3D Views for each workset.";
                                    AddToolTips(worksetButton);
                                }
                                else
                                {
                                    //update
                                    var worksetButton = utilityButtons["View Creator"];
                                    worksetButton.AssemblyName = worksetPath;
                                }
                            }

                            var doorPath = Path.Combine(directoryName, "HOK.DoorRoom.dll"); 
                            doorPath = GetTempInstallPath(doorPath);
                            if (File.Exists(doorPath))
                            {
                                if (!utilityButtons.ContainsKey("Door Link"))
                                {
                                    //install
                                    var doorButton = utilitySplitButton.AddPushButton(new PushButtonData("Door Link", "Door Link", doorPath, "HOK.DoorRoom.DoorCommand"));
                                    doorButton.LargeImage = ImageUtil.LoadBitmapImage("door.png");
                                    doorButton.ToolTip = "Set shared parameters with To and From room data.";
                                    AddToolTips(doorButton);
                                }
                                else
                                {
                                    //update
                                    var doorButton = utilityButtons["Door Link"];
                                    doorButton.AssemblyName = doorPath;
                                }
                            }

                            var roomPath = Path.Combine(directoryName, "HOK.RoomUpdater.dll");
                            roomPath = GetTempInstallPath(roomPath);
                            if (File.Exists(roomPath))
                            {
                                if (!utilityButtons.ContainsKey("Room Updater"))
                                {
                                    //install
                                    var roomButton = utilitySplitButton.AddPushButton(new PushButtonData("Room Updater", "Room Updater", roomPath, "HOK.RoomUpdater.RoomCommand"));
                                    roomButton.LargeImage = ImageUtil.LoadBitmapImage("container.png");
                                    roomButton.ToolTip = "Populate room parameters values into enclosed elements.";
                                    AddToolTips(roomButton);
                                }
                                else
                                {
                                    //update
                                    var roomButton = utilityButtons["Room Updater"];
                                    roomButton.AssemblyName = roomPath;
                                }
                            }

                            var elevationPath =Path.Combine (directoryName , "HOK.RoomElevation.dll"); 
                            elevationPath = GetTempInstallPath(elevationPath);
                            if (File.Exists(elevationPath))
                            {
                                if (!utilityButtons.ContainsKey("Room Elevation"))
                                {
                                    var elevationButton = utilitySplitButton.AddPushButton(new PushButtonData("Room Elevation", "Room Elevation", elevationPath, "HOK.RoomElevation.ElevationCommand"));
                                    elevationButton.LargeImage = ImageUtil.LoadBitmapImage("elevation.png");
                                    elevationButton.ToolTip = "Create elevation views by selecting rooms and walls to be faced.";
                                    AddToolTips(elevationButton);
                                }
                                else
                                {
                                    var elevationButton = utilityButtons["Room Elevation"];
                                    elevationButton.AssemblyName = elevationPath;
                                }
                            }

#if RELEASE2014|| RELEASE2015|| RELEASE2016|| RELEASE2017

                            string cameraPath = Path.Combine(directoryName, "HOK.CameraDuplicator.dll");
                            cameraPath = GetTempInstallPath(cameraPath);
                            if (File.Exists(cameraPath))
                            {
                                if (!utilityButtons.ContainsKey("View Mover"))
                                {
                                    PushButton cameraButton = utilitySplitButton.AddPushButton(new PushButtonData("View Mover", "View Mover", cameraPath, "HOK.CameraDuplicator.CameraCommand")) as PushButton;
                                    cameraButton.LargeImage = ImageUtil.LoadBitmapImage("cameraview.png");
                                    cameraButton.ToolTip = "Duplicate camera views and plan views from one project to the other.";
                                    AddToolTips(cameraButton);
                                }
                                else
                                {
                                    PushButton cameraButton = utilityButtons["View Mover"];
                                    cameraButton.AssemblyName = cameraPath;
                                }
                            }
#endif
                            var renamePath = Path.Combine(directoryName, "HOK.RenameFamily.dll");
                            renamePath = GetTempInstallPath(renamePath);
                            if (File.Exists(renamePath))
                            {
                                if (!utilityButtons.ContainsKey("Rename Family"))
                                {
                                    var renameButton = utilitySplitButton.AddPushButton(new PushButtonData("Rename Family", "Rename Family", renamePath, "HOK.RenameFamily.RenameCommand"));
                                    renameButton.LargeImage = ImageUtil.LoadBitmapImage("update.png");
                                    renameButton.ToolTip = "Rename families and types as assigned in .csv file.";
                                    AddToolTips(renameButton);
                                }
                                else
                                {
                                    var renameButton = utilityButtons["Rename Family"];
                                    renameButton.AssemblyName = renamePath;
                                }
                            }

                            var xyzPath = Path.Combine(directoryName, "HOK.XYZLocator.dll");
                            xyzPath = GetTempInstallPath(xyzPath);
                            if (File.Exists(xyzPath))
                            {
                                if (!utilityButtons.ContainsKey("XYZ Locator"))
                                {
                                    var xyzButton = utilitySplitButton.AddPushButton(new PushButtonData("XYZ Locator", "XYZ Locator", xyzPath, "HOK.XYZLocator.XYZCommand"));
                                    xyzButton.LargeImage = ImageUtil.LoadBitmapImage("location.ico");
                                    xyzButton.ToolTip = "Report location of a 3D family using shared coordinates";
                                    AddToolTips(xyzButton);
                                }
                                else
                                {
                                    var xyzButton = utilityButtons["XYZ Locator"];
                                    xyzButton.AssemblyName = xyzPath;
                                }
                            }

#if RELEASE2015|| RELEASE2016 || RELEASE2017
                            string measurePath = Path.Combine(directoryName, "HOK.RoomMeasure.dll");
                            measurePath = GetTempInstallPath(measurePath);
                            if (File.Exists(measurePath))
                            {
                                if (!utilityButtons.ContainsKey("Room W X L"))
                                {
                                    PushButton measureButton = utilitySplitButton.AddPushButton(new PushButtonData("Room W X L", "Room W X L", measurePath, "HOK.RoomMeasure.MeasureCommand")) as PushButton;
                                    measureButton.LargeImage = ImageUtil.LoadBitmapImage("kruler.png");
                                    measureButton.ToolTip = "Measuring the width and length of all rooms in the project";
                                    AddToolTips(measureButton);
                                }
                                else
                                {
                                    PushButton measureButton = utilityButtons["Room W X L"];
                                    measureButton.AssemblyName = measurePath;
                                }
                            }
#endif
                        }
                        else
                        {
                            if (utilityButtons.ContainsKey("Finish Creator"))
                            {
                                var finishButton = utilityButtons["Finish Creator"];
                                finishButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Ceiling Height"))
                            {
                                var ceilingButton = utilityButtons["Ceiling Height"];
                                ceilingButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Level Manager"))
                            {
                                var levelButton = utilityButtons["Level Manager"];
                                levelButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("View Depth"))
                            {
                                var viewButton = utilityButtons["View Depth"];
                                viewButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Leader Arrowhead"))
                            {
                                var leaderButton = utilityButtons["Leader Arrowhead"];
                                leaderButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("View Creator"))
                            {
                                var worksetButton = utilityButtons["View Creator"];
                                worksetButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Door Link"))
                            {
                                var doorButton = utilityButtons["Door Link"];
                                doorButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Room Updater"))
                            {
                                var roomButton = utilityButtons["Room Updater"];
                                roomButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Room Elevation"))
                            {
                                var elevationButton = utilityButtons["Room Elevation"];
                                elevationButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("View Mover"))
                            {
                                var cameraButton = utilityButtons["View Mover"];
                                cameraButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Rename Family"))
                            {
                                var renameButton = utilityButtons["Rename Family"];
                                renameButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Room W X L"))
                            {
                                var measureButton = utilityButtons["Room W X L"];
                                measureButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("XYZ Locator"))
                            {
                                var xyzButton = utilityButtons["XYZ Locator"];
                                xyzButton.Enabled = false;
                            }
                        }

#if RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        if (colorTP.InstallExist)
                        {
                            if (!utilityButtons.ContainsKey("Color Editor"))
                            {
                                PushButton colorButton = utilitySplitButton.AddPushButton(new PushButtonData("Color Editor", "Color Editor", colorTP.TempAssemblyPath, "HOK.ColorSchemeEditor.Command")) as PushButton;
                                colorButton.LargeImage = ImageUtil.LoadBitmapImage("color32.png");
                                colorButton.ToolTip = "Create color schemes by categories and parameter values.";
                                AddToolTips(colorButton);
                            }
                            else
                            {
                                PushButton colorButton = utilityButtons["Color Editor"];
                                colorButton.AssemblyName = colorTP.TempAssemblyPath;
                            }
                            
                        }
                        else if (!colorTP.InstallExist && utilityButtons.ContainsKey("Color Editor"))
                        {
                            PushButton colorButton = utilityButtons["Color Editor"];
                            colorButton.Enabled = false;
                        }
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create Utilities panel.\n"+ex.Message, "Create Utilities Panel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateCustomPanel()
        {
            try
            {
                ToolProperties sheetTP = null; //sheet manager properties
                ToolProperties bcfTP = null; // BCF Reader Properties
                ToolProperties massTP = null; //color editor properties

                if (ToolInfoDictionary.ContainsKey(ToolEnum.SheetManager) && ToolInfoDictionary.ContainsKey(ToolEnum.BCFReader) && ToolInfoDictionary.ContainsKey(ToolEnum.MassTool))
                {
                    sheetTP = ToolInfoDictionary[ToolEnum.SheetManager];
                    bcfTP = ToolInfoDictionary[ToolEnum.BCFReader];
                    massTP = ToolInfoDictionary[ToolEnum.MassTool];
                }
                if (null != sheetTP  && null != bcfTP && null != massTP)
                {
                    if (sheetTP.InstallExist ||  bcfTP.InstallExist || massTP.InstallExist)
                    {
                        if (null == customizePanel)
                        {
                            customizePanel = m_app.CreateRibbonPanel(tabName, "Customizations");
                        }
                    }
                    if (null != customizePanel)
                    {
                        var customButtons = CheckExistingButtons(customizePanel);

                        if (sheetTP.InstallExist)
                        {
                            if (!customButtons.ContainsKey("Sheet Manager"))
                            {
                                var sheetButton = (PushButton)customizePanel.AddItem(new PushButtonData("Sheet Manager", "Sheet Manager", sheetTP.TempAssemblyPath, "HOK.SheetManager.cmdSheetManager"));
                                sheetButton.LargeImage = ImageUtil.LoadBitmapImage("sheet.ico");
                                sheetButton.ToolTip = "Sheet Manager";
                                AddToolTips(sheetButton);
                                customizePanel.AddSeparator();
                            }
                            else
                            {
                                var sheetButton = customButtons["Sheet Manager"];
                                sheetButton.AssemblyName = sheetTP.TempAssemblyPath;
                            }
                        }
                        else if (!sheetTP.InstallExist && customButtons.ContainsKey("Sheet Manager"))
                        {
                            var sheetButton = customButtons["Sheet Manager"];
                            sheetButton.Enabled = false;
                        }


                        if (bcfTP.InstallExist)
                        {
                            if (!customButtons.ContainsKey("BCF Reader"))
                            {
                                var bcfButton = (PushButton)customizePanel.AddItem(new PushButtonData("BCF Reader", "BCF Reader", bcfTP.TempAssemblyPath, "HOK.BCFReader.Command"));
                                bcfButton.LargeImage = ImageUtil.LoadBitmapImage("comment.ico");
                                bcfButton.ToolTip = "BIM Collaboration Format (BCF) Reader";
                                AddToolTips(bcfButton);
                                customizePanel.AddSeparator();
                            }
                            else
                            {
                                var bcfButton = customButtons["BCF Reader"];
                                bcfButton.AssemblyName = bcfTP.TempAssemblyPath;
                            }
                        }
                        else if (!bcfTP.InstallExist && customButtons.ContainsKey("BCF Reader"))
                        {
                            var bcfButton = customButtons["BCF Reader"];
                            bcfButton.Enabled = false;
                        }

                        if (massTP.InstallExist)
                        {
                            if (null == massSplitButton)
                            {
                                var splitButtonData = new SplitButtonData("MassTool", "3D Mass");
                                massSplitButton = (SplitButton)customizePanel.AddItem(splitButtonData);
                                massSplitButton.IsSynchronizedWithCurrentItem = true;
                                var contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\Mass Tool_Instruction.pdf");
                                massSplitButton.SetContextualHelp(contextualHelp);
                            }
                            if (null != massSplitButton)
                            {
                                var massButtons = CheckExistingButtons(massSplitButton);

                                if (!massButtons.ContainsKey("Create Mass"))
                                {
                                    var createButton = massSplitButton.AddPushButton(new PushButtonData("Create Mass", "Create Mass", massTP.TempAssemblyPath, "HOK.RoomsToMass.Command"));
                                    createButton.LargeImage = ImageUtil.LoadBitmapImage("cube.png");
                                    createButton.ToolTip = "Creates 3D Mass from rooms, areas and floors.";
                                    createButton.ToolTipImage = ImageUtil.LoadBitmapImage("tooltip.png");
                                    AddToolTips(createButton);
                                }
                                else
                                {
                                    var createButton = massButtons["Create Mass"];
                                    createButton.AssemblyName = massTP.TempAssemblyPath;
                                }
#if RELEASE2013 || RELEASE2014
                                if (!massButtons.ContainsKey("Update Data"))
                                {
                                    PushButton updateButton = massSplitButton.AddPushButton(new PushButtonData("Update Data", "Update Data", massTP.TempAssemblyPath, "HOK.RoomsToMass.DataCommand"));
                                    updateButton.LargeImage = ImageUtil.LoadBitmapImage("refresh.png");
                                    updateButton.ToolTip = "Transfer parameters values between Rooms, Areas, Floors and Masses";
                                    AddToolTips(updateButton);
                                }
                                else
                                {
                                    PushButton updateButton = massButtons["Update Data"];
                                    updateButton.AssemblyName = massTP.TempAssemblyPath;
                                }
#endif
                                if (!massButtons.ContainsKey("Mass Commands"))
                                {
                                    var commandButton = massSplitButton.AddPushButton(new PushButtonData("Mass Commands", "Mass Commands", massTP.TempAssemblyPath, "HOK.RoomsToMass.AssignerCommand"));
                                    commandButton.LargeImage = ImageUtil.LoadBitmapImage("shape.png");
                                    commandButton.ToolTip = "Assign parameters or split elements";
                                    AddToolTips(commandButton);
                                }
                                else
                                {
                                    var commandButton = massButtons["Mass Commands"];
                                    commandButton.AssemblyName = massTP.TempAssemblyPath;
                                }
                            }
                        }
                        else if (!massTP.InstallExist && null != massSplitButton)
                        {
                            var massButtons = CheckExistingButtons(massSplitButton);
                            if (massButtons.ContainsKey("Create Mass"))
                            {
                                var createButton = massButtons["Create Mass"];
                                createButton.Enabled = false;
                            }
#if RELEASE2013|| RELEASE2014
                            if (massButtons.ContainsKey("Update Data"))
                            {
                                PushButton updateButton = massButtons["Update Data"];
                                updateButton.Enabled = false;
                            }
#endif
                            if (massButtons.ContainsKey("Mass Commands"))
                            {
                                var commandButton = massButtons["Mass Commands"];
                                commandButton.Enabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create Customizations panel.\n" + ex.Message, "Create Customization Panel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateDataPanel()
        {
            try
            {
                ToolProperties dataTP = null; //sheet manager properties
                
                if (ToolInfoDictionary.ContainsKey(ToolEnum.RevitData))
                {
                    dataTP = ToolInfoDictionary[ToolEnum.RevitData];
                }
                if (null != dataTP)
                {
                    if (dataTP.InstallExist)
                    {
                        if (null == dataPanel)
                        {
                            dataPanel = m_app.CreateRibbonPanel(tabName, "Revit Data");
                        }
                        if (null != dataPanel)
                        {
                            var dataButtons = CheckExistingButtons(dataPanel);

                            if (!dataButtons.ContainsKey("Data Sync"))
                            {
                                var syncButton = (PushButton)dataPanel.AddItem(new PushButtonData("Data Sync", "Data Sync", dataTP.TempAssemblyPath, "RevitDBManager.Command"));
                                syncButton.LargeImage = ImageUtil.LoadBitmapImage("sync.ico");
                                syncButton.ToolTip = "Data Sync";
                                AddToolTips(syncButton);
                            }
                            else
                            {
                                var syncButton = dataButtons["Data Sync"];
                                syncButton.AssemblyName = dataTP.TempAssemblyPath;
                            }

                            if (!dataButtons.ContainsKey("Setup"))
                            {
                                //DBManager_Setup
                                var setupButton = (PushButton)dataPanel.AddItem(new PushButtonData("Setup", "  Setup  ", dataTP.TempAssemblyPath, "RevitDBManager.EditorCommand"));
                                setupButton.LargeImage = ImageUtil.LoadBitmapImage("editor.ico");
                                setupButton.ToolTip = "Setup";
                                AddToolTips(setupButton);
                            }
                            else
                            {
                                var setupButton = dataButtons["Setup"];
                                setupButton.AssemblyName = dataTP.TempAssemblyPath;
                            }

                            if (!dataButtons.ContainsKey("Data Editor"))
                            {
                                //DBManager_Data Editor
                                var editorButton = (PushButton)dataPanel.AddItem(new PushButtonData("Data Editor", "Data Editor", dataTP.TempAssemblyPath, "RevitDBManager.ViewerCommand"));
                                editorButton.LargeImage = ImageUtil.LoadBitmapImage("view.ico");
                                editorButton.ToolTip = "Data Editor";
                                AddToolTips(editorButton);
                            }
                            else
                            {
                                var editorButton = dataButtons["Data Editor"];
                                editorButton.AssemblyName = dataTP.TempAssemblyPath;
                            }

                        }
                    }
                    else if(!dataTP.InstallExist && null != dataPanel)
                    {
                        var dataButtons = CheckExistingButtons(dataPanel);
                        if (dataButtons.ContainsKey("Data Sync"))
                        {
                            var syncButton = dataButtons["Data Sync"];
                            syncButton.Enabled = false;
                        }
                        if (dataButtons.ContainsKey("Setup"))
                        {
                            var setupButton = dataButtons["Setup"];
                            setupButton.Enabled = false;
                        }
                        if (dataButtons.ContainsKey("Data Editor"))
                        {
                            var editorButton = dataButtons["Data Editor"];
                            editorButton.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create Data panel.\n"+ex.Message , "Create Data Panel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateAnalysisPanel()
        {
            try
            {
                ToolProperties avfTP = null; //AVF properties
                ToolProperties lpdTP = null; //LPD analysis properties
                ToolProperties leedTP = null; //LEED view analysis properties

                if (ToolInfoDictionary.ContainsKey(ToolEnum.AVF) && ToolInfoDictionary.ContainsKey(ToolEnum.LPDAnalysis) && ToolInfoDictionary.ContainsKey(ToolEnum.LEEDView))
                {
                    avfTP = ToolInfoDictionary[ToolEnum.AVF];
                    lpdTP = ToolInfoDictionary[ToolEnum.LPDAnalysis];
                    leedTP = ToolInfoDictionary[ToolEnum.LEEDView];
                }

                if (null != avfTP && null != lpdTP && null != leedTP)
                {
                    if (avfTP.InstallExist || lpdTP.InstallExist || leedTP.InstallExist)
                    {
                        if (null == analysisPanel && null == analysisSplitButton)
                        {
                            analysisPanel = m_app.CreateRibbonPanel(tabName, "Analysis");

                            var splitButtonData = new SplitButtonData("HOKAnalysis", "HOK Analysis");
                            analysisSplitButton = (SplitButton)analysisPanel.AddItem(splitButtonData);
                            analysisSplitButton.IsSynchronizedWithCurrentItem = true;
                        }
                    }
                    if (null != analysisPanel && null != analysisSplitButton)
                    {
                        var analysisButtons = CheckExistingButtons(analysisSplitButton);
                        if (avfTP.InstallExist)
                        {
                            if (!analysisButtons.ContainsKey("AVF"))
                            {
                                //install
                                var avfButton = analysisSplitButton.AddPushButton(new PushButtonData("AVF", "  AVF  ", avfTP.TempAssemblyPath, "HOK.AVFManager.Command"));
                                avfButton.LargeImage = ImageUtil.LoadBitmapImage("chart.ico");
                                avfButton.ToolTip = "Analysis Visualization Framework";
                                AddToolTips(avfButton);
                            }
                            else
                            {
                                //update
                                var avfButton = analysisButtons["AVF"];
                                avfButton.AssemblyName = avfTP.TempAssemblyPath;
                            }

                        }
                        else if (!avfTP.InstallExist && analysisButtons.ContainsKey("AVF"))
                        {
                            //remove tools by beta installer
                            var avfButton = analysisButtons["AVF"];
                            avfButton.Enabled = false;
                        }

                        if (lpdTP.InstallExist)
                        {
                            if (!analysisButtons.ContainsKey("LPD Analysis"))
                            {
                                //install
                                var lpdButton = analysisSplitButton.AddPushButton(new PushButtonData("LPD Analysis", "LPD Analysis", lpdTP.TempAssemblyPath, "HOK.LPDCalculator.Command"));
                                lpdButton.LargeImage = ImageUtil.LoadBitmapImage("bulb.png");
                                lpdButton.ToolTip = "Calculating Lighting Power Density";
                                AddToolTips(lpdButton);
                            }
                            else
                            {
                                //update
                                var lpdButton = analysisButtons["LPD Analysis"];
                                lpdButton.AssemblyName = lpdTP.TempAssemblyPath;
                            }
                        }
                        else if (!lpdTP.InstallExist && analysisButtons.ContainsKey("LPD Analysis"))
                        {
                            //remove tools by beta installer
                            var lpdButton = analysisButtons["LPD Analysis"];
                            lpdButton.Enabled = false;
                        }

                        if (leedTP.InstallExist)
                        {
                            if (!analysisButtons.ContainsKey("LEED View Analysis"))
                            {
                                //install
                                var leedButton = analysisSplitButton.AddPushButton(new PushButtonData("LEED View Analysis", "LEED View Analysis", leedTP.TempAssemblyPath, "HOK.ViewAnalysis.Command"));
                                leedButton.LargeImage = ImageUtil.LoadBitmapImage("eq.ico");
                                leedButton.ToolTip = "Calculating Area with Views for LEED IEQc 8.2";
                                AddToolTips(leedButton);
                            }
                            else
                            {
                                //update
                                var leedButton = analysisButtons["LEED View Analysis"];
                                leedButton.AssemblyName = leedTP.TempAssemblyPath;
                            }
                        }
                        else if (!leedTP.InstallExist && analysisButtons.ContainsKey("LEED View Analysis"))
                        {
                            //remove tools by beta installer
                            var leedButton = analysisButtons["LEED View Analysis"];
                            leedButton.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create Analysis panel.\n"+ex.Message, "Create Analysis Panel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateDirectShapePanel()
        {
            try
            {
                ToolProperties flatterTP = null; //element flatter properties

                if (ToolInfoDictionary.ContainsKey(ToolEnum.ElementFlatter))
                {
                    flatterTP = ToolInfoDictionary[ToolEnum.ElementFlatter];
                }
                if (null != flatterTP)
                {
                    if (flatterTP.InstallExist)
                    {
                        if (null == shapePanel)
                        {
                            shapePanel = m_app.CreateRibbonPanel(tabName, "Direct Shape");
                        }
                        if (null != shapePanel)
                        {
                            var shapeButtons = CheckExistingButtons(shapePanel);

                            if (!shapeButtons.ContainsKey("Element Flatter"))
                            {
                                var flatterButton = (PushButton)shapePanel.AddItem(new PushButtonData("Element Flatter", "Element Flatter", flatterTP.TempAssemblyPath, "HOK.ElementFlatter.Command"));
                                flatterButton.LargeImage = ImageUtil.LoadBitmapImage("create.ico");
                                flatterButton.ToolTip = "Flatten elements as direct shapes";
                                AddToolTips(flatterButton);
                            }
                            else
                            {
                                var flatterButton = shapeButtons["Element Flatter"];
                                flatterButton.AssemblyName = flatterTP.TempAssemblyPath;
                            }
                        }
                    }
                    else if (!flatterTP.InstallExist && null != shapePanel)
                    {
                        var shapeButtons = CheckExistingButtons(shapePanel);
                        if (shapeButtons.ContainsKey("Element Flatter"))
                        {
                            var flatterButton = shapeButtons["Element Flatter"];
                            flatterButton.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create Direct Shape panel.\n" + ex.Message, "Create Direct Shape Panel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GetTempInstallPath(string installPath)
        {
            var tempPath = "";
            try
            {
                var fileName = Path.GetFileName(installPath);
                tempPath = Path.Combine(toolManager.TempInstallDirectory, fileName);
                if (!File.Exists(tempPath))
                {
                    tempPath = installPath;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return tempPath;
        }

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
                                ButtonDictionary.Add(buttonData.ButtonName, buttonData);
                                index = 0;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read the information of tool tips.\n" + ex.Message, "Read Tool Tips", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddToolTips(PushButton button)
        {
            try
            {
                if (ButtonDictionary.ContainsKey(button.Name))
                {
                    var buttonData = ButtonDictionary[button.Name];
                    button.LongDescription = buttonData.Description;
                    var contextualHelp = new ContextualHelp(ContextualHelpType.Url, buttonData.HelpUrl);
                    button.SetContextualHelp(contextualHelp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add tool tips.\n" + ex.Message, "Add Tool Tips", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
    }

    class ButtonData
    {
        public string ButtonName { get; set; }
        public string Description { get; set; }
        public string HelpUrl { get; set; }
    }

    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }
}
