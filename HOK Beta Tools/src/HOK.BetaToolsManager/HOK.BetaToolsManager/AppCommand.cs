using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
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
        SmartBCF
    }

    public class AppCommand:IExternalApplication
    {
        private UIControlledApplication m_app;
        internal static AppCommand thisApp = null;
        private string tabName = "  HOK - Beta";
        private string versionNumber = "";
        private ToolManager toolManager = null;
        private Dictionary<ToolEnum, ToolProperties> toolInfoDictionary = new Dictionary<ToolEnum, ToolProperties>();
        private Dictionary<string, ButtonData> buttonDictionary = new Dictionary<string, ButtonData>();
        private string currentDirectory = "";
        private string currentAssembly = "";
        private string tooltipFileName = "HOK.Tooltip.txt";

        private RibbonPanel utilityPanel = null;
        private SplitButton utilitySplitButton = null;
        private RibbonPanel customizePanel = null;
        private SplitButton massSplitButton = null;
        private RibbonPanel dataPanel = null;
        private RibbonPanel analysisPanel = null;
        private SplitButton analysisSplitButton = null;

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
            toolInfoDictionary = toolManager.ToolInfoDictionary;

            try { m_app.CreateRibbonTab(tabName); }
            catch { }
          
            currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            currentDirectory = Path.GetDirectoryName(currentAssembly);
            string tooltipTxt = currentDirectory + "/Resources/" + tooltipFileName;
            ReadToolTips(tooltipTxt);

            CreateInstallerPanel();
            if (toolInfoDictionary.Count > 0)
            {
                CreateUtilitiesPanel();
                CreateCustomPanel();
                CreateDataPanel();
                CreateAnalysisPanel();
            }

            return Result.Succeeded;
        }

        public void ShowInstaller(UIApplication uiapp)
        {
            BetaInstallerWindow installerWindow = new BetaInstallerWindow(versionNumber, toolInfoDictionary);
            if (installerWindow.ShowDialog() == true)
            {
                toolInfoDictionary = installerWindow.ToolInfoDictionary;
                CreateUtilitiesPanel();
                CreateCustomPanel();
                CreateDataPanel();
                CreateAnalysisPanel();
            }
        }

        private Dictionary<string, PushButton> CheckExistingButtons(SplitButton splitButton)
        {
            Dictionary<string, PushButton> pushButtons = new Dictionary<string, PushButton>();
            try
            {
                List<PushButton> buttons = splitButton.GetItems().ToList();
                foreach (PushButton bttn in buttons)
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

        private Dictionary<string, PushButton> CheckExistingButtons(RibbonPanel ribbonPanel)
        {
            Dictionary<string, PushButton> pushButtons = new Dictionary<string, PushButton>();
            try
            {
                List<RibbonItem> ribbonItems = ribbonPanel.GetItems().ToList();
                foreach (RibbonItem item in ribbonItems)
                {
                    PushButton button = item as PushButton;
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
                RibbonPanel installerPanel = m_app.CreateRibbonPanel(tabName, "Installer");

                PushButton installerButton = installerPanel.AddItem(new PushButtonData("Beta Installer", "Beta Installer", currentAssembly, "HOK.BetaToolsManager.InstallerCommand")) as PushButton;
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
                
                if (toolInfoDictionary.ContainsKey(ToolEnum.Utility) && toolInfoDictionary.ContainsKey(ToolEnum.ElementTools) && toolInfoDictionary.ContainsKey(ToolEnum.ParameterTools) && toolInfoDictionary.ContainsKey(ToolEnum.ColorEditor))
                {
                    utilitiesTP = toolInfoDictionary[ToolEnum.Utility];
                    elementTP = toolInfoDictionary[ToolEnum.ElementTools];
                    parameterTP = toolInfoDictionary[ToolEnum.ParameterTools];
                    colorTP = toolInfoDictionary[ToolEnum.ColorEditor];
                }

                if (null != utilitiesTP && null != elementTP && null != parameterTP && null != colorTP)
                {
                    if (utilitiesTP.InstallExist || elementTP.InstallExist || parameterTP.InstallExist || colorTP.InstallExist)
                    {
                        if (null == utilityPanel && null == utilitySplitButton)
                        {
                            utilityPanel = m_app.CreateRibbonPanel(tabName, "Utilities");

                            SplitButtonData splitButtonData = new SplitButtonData("HOKUtilities", "HOK Utilities");
                            utilitySplitButton = utilityPanel.AddItem(splitButtonData) as SplitButton;
                            ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\HOK Utilities_Instruction.pdf");
                            utilitySplitButton.SetContextualHelp(contextualHelp);
                        }
                    }
                    if (null != utilityPanel && null != utilitySplitButton)
                    {
                        Dictionary<string, PushButton> utilityButtons = CheckExistingButtons(utilitySplitButton);

                        if (elementTP.InstallExist)
                        {
                            
                            if (!utilityButtons.ContainsKey("Element Tools"))
                            {
                                //install
                                PushButton elementButton = utilitySplitButton.AddPushButton(new PushButtonData("Element Tools", "Element Tools", elementTP.TempAssemblyPath, "HOK.ElementTools.cmdElementTools")) as PushButton;
                                elementButton.LargeImage = ImageUtil.LoadBitmapImage("element.ico");
                                elementButton.ToolTip = "Room and Area Elements Tools";
                                AddToolTips(elementButton);
                            }
                            else
                            {
                                //update
                                PushButton elementButton = utilityButtons["Element Tools"];
                                elementButton.AssemblyName = elementTP.TempAssemblyPath;
                            }
                            
                        }
                        else if (!elementTP.InstallExist && utilityButtons.ContainsKey("Element Tools"))
                        {
                            //removed tools by beta installer
                            PushButton elementButton = utilityButtons["Element Tools"];
                            elementButton.Enabled = false;
                        }

                        if (parameterTP.InstallExist)
                        {
                            if (!utilityButtons.ContainsKey("Parameter Tools"))
                            {
                                //install
                                PushButton parameterButton = utilitySplitButton.AddPushButton(new PushButtonData("Parameter Tools", "Parameter Tools", parameterTP.TempAssemblyPath, "HOK.ParameterTools.cmdParameterTools")) as PushButton;
                                parameterButton.LargeImage = ImageUtil.LoadBitmapImage("parameter.ico");
                                parameterButton.ToolTip = "Parameter Tools";
                                AddToolTips(parameterButton);
                            }
                            else
                            {
                                //update
                                PushButton parameterButton = utilityButtons["Parameter Tools"];
                                parameterButton.AssemblyName = parameterTP.TempAssemblyPath;
                            }
                            
                        }
                        else if (!parameterTP.InstallExist && utilityButtons.ContainsKey("Parameter Tools"))
                        {
                            PushButton parameterButton = utilityButtons["Parameter Tools"];
                            parameterButton.Enabled = false;
                        }

                        if (utilitiesTP.InstallExist)
                        {
                            string directoryName = Path.GetDirectoryName(utilitiesTP.InstallPath);
                            string finishPath = Path.Combine(directoryName, "HOK.FinishCreator.dll");
                            finishPath = GetTempInstallPath(finishPath);
                            if (File.Exists(finishPath))
                            {
                                if (!utilityButtons.ContainsKey("Finish Creator"))
                                {
                                    //install
                                    PushButton finishButton = utilitySplitButton.AddPushButton(new PushButtonData("Finish Creator", "Finish Creator", finishPath, "HOK.FinishCreator.FinishCommand")) as PushButton;
                                    finishButton.LargeImage = ImageUtil.LoadBitmapImage("finish.png");
                                    finishButton.ToolTip = "Create floor finishes from the selected rooms.";
                                    AddToolTips(finishButton);
                                }
                                else
                                {
                                    //update
                                    PushButton finishButton = utilityButtons["Finish Creator"];
                                    finishButton.AssemblyName = finishPath;
                                }
                            }

                            string ceilingPath =Path.Combine(directoryName, "HOK.CeilingHeight.dll");
                            ceilingPath = GetTempInstallPath(ceilingPath);
                            if (File.Exists(ceilingPath))
                            {
                                if (!utilityButtons.ContainsKey("Ceiling Height"))
                                {
                                    PushButton ceilingButton = utilitySplitButton.AddPushButton(new PushButtonData("Ceiling Height", "Ceiling Heights", ceilingPath, "HOK.CeilingHeight.CeilingCommand")) as PushButton;
                                    ceilingButton.LargeImage = ImageUtil.LoadBitmapImage("height.png");
                                    ceilingButton.ToolTip = "Select rooms to measure the height from floors to ceilings.";
                                    AddToolTips(ceilingButton);
                                    //install
                                }
                                else
                                {
                                    //update
                                    PushButton ceilingButton = utilityButtons["Ceiling Height"];
                                    ceilingButton.AssemblyName = ceilingPath;
                                }
                            }

                            string levelPath = Path.Combine(directoryName, "HOK.LevelManager.dll");
                            levelPath = GetTempInstallPath(levelPath);
                            if (File.Exists(levelPath))
                            {
                                if (!utilityButtons.ContainsKey("Level Manager"))
                                {
                                    PushButton levelButton = utilitySplitButton.AddPushButton(new PushButtonData("Level Manager", "Level Manager", levelPath, "HOK.LevelManager.LevelCommand")) as PushButton;
                                    levelButton.LargeImage = ImageUtil.LoadBitmapImage("level.png");
                                    levelButton.ToolTip = "Rehost elements from one level to anather. ";
                                    AddToolTips(levelButton);
                                }
                                else
                                {
                                    PushButton levelButton = utilityButtons["Level Manager"];
                                    levelButton.AssemblyName = levelPath;
                                }
                            }
#if RELEASE2013
#else
                            string viewPath = Path.Combine(directoryName, "HOK.ViewDepth.dll");
                            viewPath = GetTempInstallPath(viewPath);
                            if (File.Exists(viewPath))
                            {
                                if (!utilityButtons.ContainsKey("View Depth"))
                                {
                                    //install
                                    PushButton viewButton = utilitySplitButton.AddPushButton(new PushButtonData("View Depth", "View Depth", viewPath, "HOK.ViewDepth.ViewCommand")) as PushButton;
                                    viewButton.LargeImage = ImageUtil.LoadBitmapImage("camera.ico");
                                    viewButton.ToolTip = "Override the graphics of the element based on the distance";
                                    viewButton.ToolTipImage = ImageUtil.LoadBitmapImage("viewTooltip.png");
                                    AddToolTips(viewButton);
                                }
                                else
                                {
                                    //update
                                    PushButton viewButton = utilityButtons["View Depth"];
                                    viewButton.AssemblyName = viewPath;
                                }
                            }
#endif

                            string arrowPath = Path.Combine( directoryName, "HOK.Arrowhead.dll"); 
                            arrowPath = GetTempInstallPath(arrowPath);
                            if (File.Exists(arrowPath))
                            {
                                if (!utilityButtons.ContainsKey("Leader Arrowhead"))
                                {
                                    //install
                                    PushButton leaderButton = utilitySplitButton.AddPushButton(new PushButtonData("Leader Arrowhead", "Leader Arrowhead", arrowPath, "HOK.Arrowhead.ArrowCommand")) as PushButton;
                                    leaderButton.LargeImage = ImageUtil.LoadBitmapImage("arrowhead.png");
                                    leaderButton.ToolTip = "Assign a leader arrowhead style to all tag types.";
                                    AddToolTips(leaderButton);
                                }
                                else
                                {
                                    //update
                                    PushButton leaderButton = utilityButtons["Leader Arrowhead"];
                                    leaderButton.AssemblyName = arrowPath;
                                }
                            }

                            string worksetPath = Path.Combine(directoryName, "HOK.WorksetView.dll"); 
                            worksetPath = GetTempInstallPath(worksetPath);
                            if (File.Exists(worksetPath))
                            {
                                if (!utilityButtons.ContainsKey("View Creator"))
                                {
                                    //install
                                    PushButton worksetButton = utilitySplitButton.AddPushButton(new PushButtonData("View Creator", "View Creator", worksetPath, "HOK.WorksetView.WorksetCommand")) as PushButton;
                                    worksetButton.LargeImage = ImageUtil.LoadBitmapImage("workset.png");
                                    worksetButton.ToolTip = "Create 3D Views for each workset.";
                                    AddToolTips(worksetButton);
                                }
                                else
                                {
                                    //update
                                    PushButton worksetButton = utilityButtons["View Creator"];
                                    worksetButton.AssemblyName = worksetPath;
                                }
                            }

                            string doorPath = Path.Combine(directoryName, "HOK.DoorRoom.dll"); 
                            doorPath = GetTempInstallPath(doorPath);
                            if (File.Exists(doorPath))
                            {
                                if (!utilityButtons.ContainsKey("Door Link"))
                                {
                                    //install
                                    PushButton doorButton = utilitySplitButton.AddPushButton(new PushButtonData("Door Link", "Door Link", doorPath, "HOK.DoorRoom.DoorCommand")) as PushButton;
                                    doorButton.LargeImage = ImageUtil.LoadBitmapImage("door.png");
                                    doorButton.ToolTip = "Set shared parameters with To and From room data.";
                                    AddToolTips(doorButton);
                                }
                                else
                                {
                                    //update
                                    PushButton doorButton = utilityButtons["Door Link"];
                                    doorButton.AssemblyName = doorPath;
                                }
                            }

                            string roomPath = Path.Combine(directoryName, "HOK.RoomUpdater.dll");
                            roomPath = GetTempInstallPath(roomPath);
                            if (File.Exists(roomPath))
                            {
                                if (!utilityButtons.ContainsKey("Room Updater"))
                                {
                                    //install
                                    PushButton roomButton = utilitySplitButton.AddPushButton(new PushButtonData("Room Updater", "Room Updater", roomPath, "HOK.RoomUpdater.RoomCommand")) as PushButton;
                                    roomButton.LargeImage = ImageUtil.LoadBitmapImage("container.png");
                                    roomButton.ToolTip = "Populate room parameters values into enclosed elements.";
                                    AddToolTips(roomButton);
                                }
                                else
                                {
                                    //update
                                    PushButton roomButton = utilityButtons["Room Updater"];
                                    roomButton.AssemblyName = roomPath;
                                }
                            }

                            string elevationPath =Path.Combine (directoryName , "HOK.RoomElevation.dll"); 
                            elevationPath = GetTempInstallPath(elevationPath);
                            if (File.Exists(elevationPath))
                            {
                                if (!utilityButtons.ContainsKey("Room Elevation"))
                                {
                                    PushButton elevationButton = utilitySplitButton.AddPushButton(new PushButtonData("Room Elevation", "Room Elevation", elevationPath, "HOK.RoomElevation.ElevationCommand")) as PushButton;
                                    elevationButton.LargeImage = ImageUtil.LoadBitmapImage("elevation.png");
                                    elevationButton.ToolTip = "Create elevation views by selecting rooms and walls to be faced.";
                                    AddToolTips(elevationButton);
                                }
                                else
                                {
                                    PushButton elevationButton = utilityButtons["Room Elevation"];
                                    elevationButton.AssemblyName = elevationPath;
                                }
                            }

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
                                    PushButton cameraButton = utilityButtons["Camera Duplicator"];
                                    cameraButton.AssemblyName = cameraPath;
                                }
                            }
                        }
                        else
                        {
                            if (utilityButtons.ContainsKey("Finish Creator"))
                            {
                                PushButton finishButton = utilityButtons["Finish Creator"];
                                finishButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Ceiling Height"))
                            {
                                PushButton ceilingButton = utilityButtons["Ceiling Height"];
                                ceilingButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Level Manager"))
                            {
                                PushButton levelButton = utilityButtons["Level Manager"];
                                levelButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("View Depth"))
                            {
                                PushButton viewButton = utilityButtons["View Depth"];
                                viewButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Leader Arrowhead"))
                            {
                                PushButton leaderButton = utilityButtons["Leader Arrowhead"];
                                leaderButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("View Creator"))
                            {
                                PushButton worksetButton = utilityButtons["View Creator"];
                                worksetButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Door Link"))
                            {
                                PushButton doorButton = utilityButtons["Door Link"];
                                doorButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Room Updater"))
                            {
                                PushButton roomButton = utilityButtons["Room Updater"];
                                roomButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Room Elevation"))
                            {
                                PushButton elevationButton = utilityButtons["Room Elevation"];
                                elevationButton.Enabled = false;
                            }
                            if (utilityButtons.ContainsKey("Camera Duplicator"))
                            {
                                PushButton cameraButton = utilityButtons["Camera Duplicator"];
                                cameraButton.Enabled = false;
                            }
                        }

#if RELEASE2014||RELEASE2015 || RELEASE2016
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

                if (toolInfoDictionary.ContainsKey(ToolEnum.SheetManager) && toolInfoDictionary.ContainsKey(ToolEnum.BCFReader) && toolInfoDictionary.ContainsKey(ToolEnum.MassTool))
                {
                    sheetTP = toolInfoDictionary[ToolEnum.SheetManager];
                    bcfTP = toolInfoDictionary[ToolEnum.BCFReader];
                    massTP = toolInfoDictionary[ToolEnum.MassTool];
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
                        Dictionary<string, PushButton> customButtons = CheckExistingButtons(customizePanel);

                        if (sheetTP.InstallExist)
                        {
                            if (!customButtons.ContainsKey("Sheet Manager"))
                            {
                                PushButton sheetButton = customizePanel.AddItem(new PushButtonData("Sheet Manager", "Sheet Manager", sheetTP.TempAssemblyPath, "HOK.SheetManager.cmdSheetManager")) as PushButton;
                                sheetButton.LargeImage = ImageUtil.LoadBitmapImage("sheet.ico");
                                sheetButton.ToolTip = "Sheet Manager";
                                AddToolTips(sheetButton);
                                customizePanel.AddSeparator();
                            }
                            else
                            {
                                PushButton sheetButton = customButtons["Sheet Manager"];
                                sheetButton.AssemblyName = sheetTP.TempAssemblyPath;
                            }
                        }
                        else if (!sheetTP.InstallExist && customButtons.ContainsKey("Sheet Manager"))
                        {
                            PushButton sheetButton = customButtons["Sheet Manager"];
                            sheetButton.Enabled = false;
                        }


                        if (bcfTP.InstallExist)
                        {
                            if (!customButtons.ContainsKey("BCF Reader"))
                            {
                                PushButton bcfButton = customizePanel.AddItem(new PushButtonData("BCF Reader", "BCF Reader", bcfTP.TempAssemblyPath, "HOK.BCFReader.Command")) as PushButton;
                                bcfButton.LargeImage = ImageUtil.LoadBitmapImage("comment.ico");
                                bcfButton.ToolTip = "BIM Collaboration Format (BCF) Reader";
                                AddToolTips(bcfButton);
                                customizePanel.AddSeparator();
                            }
                            else
                            {
                                PushButton bcfButton = customButtons["BCF Reader"];
                                bcfButton.AssemblyName = bcfTP.TempAssemblyPath;
                            }
                        }
                        else if (!bcfTP.InstallExist && customButtons.ContainsKey("BCF Reader"))
                        {
                            PushButton bcfButton = customButtons["BCF Reader"];
                            bcfButton.Enabled = false;
                        }

                        if (massTP.InstallExist)
                        {
                            if (null == massSplitButton)
                            {
                                SplitButtonData splitButtonData = new SplitButtonData("MassTool", "3D Mass");
                                massSplitButton = customizePanel.AddItem(splitButtonData) as SplitButton;
                                massSplitButton.IsSynchronizedWithCurrentItem = true;
                                ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\Mass Tool_Instruction.pdf");
                                massSplitButton.SetContextualHelp(contextualHelp);
                            }
                            if (null != massSplitButton)
                            {
                                Dictionary<string, PushButton> massButtons = CheckExistingButtons(massSplitButton);

                                if (!massButtons.ContainsKey("Create Mass"))
                                {
                                    PushButton createButton = massSplitButton.AddPushButton(new PushButtonData("Create Mass", "Create Mass", massTP.TempAssemblyPath, "HOK.RoomsToMass.Command"));
                                    createButton.LargeImage = ImageUtil.LoadBitmapImage("cube.png");
                                    createButton.ToolTip = "Creates 3D Mass from rooms, areas and floors.";
                                    createButton.ToolTipImage = ImageUtil.LoadBitmapImage("tooltip.png");
                                    AddToolTips(createButton);
                                }
                                else
                                {
                                    PushButton createButton = massButtons["Create Mass"];
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
                                    PushButton commandButton = massSplitButton.AddPushButton(new PushButtonData("Mass Commands", "Mass Commands", massTP.TempAssemblyPath, "HOK.RoomsToMass.AssignerCommand"));
                                    commandButton.LargeImage = ImageUtil.LoadBitmapImage("shape.png");
                                    commandButton.ToolTip = "Assign parameters or split elements";
                                    AddToolTips(commandButton);
                                }
                                else
                                {
                                    PushButton commandButton = massButtons["Mass Commands"];
                                    commandButton.AssemblyName = massTP.TempAssemblyPath;
                                }
                            }
                        }
                        else if (!massTP.InstallExist && null != massSplitButton)
                        {
                            Dictionary<string, PushButton> massButtons = CheckExistingButtons(massSplitButton);
                            if (massButtons.ContainsKey("Create Mass"))
                            {
                                PushButton createButton = massButtons["Create Mass"];
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
                                PushButton commandButton = massButtons["Mass Commands"];
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
                
                if (toolInfoDictionary.ContainsKey(ToolEnum.RevitData))
                {
                    dataTP = toolInfoDictionary[ToolEnum.RevitData];
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
                            Dictionary<string, PushButton> dataButtons = CheckExistingButtons(dataPanel);

                            if (!dataButtons.ContainsKey("Data Sync"))
                            {
                                PushButton syncButton = dataPanel.AddItem(new PushButtonData("Data Sync", "Data Sync", dataTP.TempAssemblyPath, "RevitDBManager.Command")) as PushButton;
                                syncButton.LargeImage = ImageUtil.LoadBitmapImage("sync.ico");
                                syncButton.ToolTip = "Data Sync";
                                AddToolTips(syncButton);
                            }
                            else
                            {
                                PushButton syncButton = dataButtons["Data Sync"];
                                syncButton.AssemblyName = dataTP.TempAssemblyPath;
                            }

                            if (!dataButtons.ContainsKey("Setup"))
                            {
                                //DBManager_Setup
                                PushButton setupButton = dataPanel.AddItem(new PushButtonData("Setup", "  Setup  ", dataTP.TempAssemblyPath, "RevitDBManager.EditorCommand")) as PushButton;
                                setupButton.LargeImage = ImageUtil.LoadBitmapImage("editor.ico");
                                setupButton.ToolTip = "Setup";
                                AddToolTips(setupButton);
                            }
                            else
                            {
                                PushButton setupButton = dataButtons["Setup"];
                                setupButton.AssemblyName = dataTP.TempAssemblyPath;
                            }

                            if (!dataButtons.ContainsKey("Data Editor"))
                            {
                                //DBManager_Data Editor
                                PushButton editorButton = dataPanel.AddItem(new PushButtonData("Data Editor", "Data Editor", dataTP.TempAssemblyPath, "RevitDBManager.ViewerCommand")) as PushButton;
                                editorButton.LargeImage = ImageUtil.LoadBitmapImage("view.ico");
                                editorButton.ToolTip = "Data Editor";
                                AddToolTips(editorButton);
                            }
                            else
                            {
                                PushButton editorButton = dataButtons["Data Editor"];
                                editorButton.AssemblyName = dataTP.TempAssemblyPath;
                            }

                        }
                    }
                    else if(!dataTP.InstallExist && null != dataPanel)
                    {
                        Dictionary<string, PushButton> dataButtons = CheckExistingButtons(dataPanel);
                        if (dataButtons.ContainsKey("Data Sync"))
                        {
                            PushButton syncButton = dataButtons["Data Sync"];
                            syncButton.Enabled = false;
                        }
                        if (dataButtons.ContainsKey("Setup"))
                        {
                            PushButton setupButton = dataButtons["Setup"];
                            setupButton.Enabled = false;
                        }
                        if (dataButtons.ContainsKey("Data Editor"))
                        {
                            PushButton editorButton = dataButtons["Data Editor"];
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

                if (toolInfoDictionary.ContainsKey(ToolEnum.AVF) && toolInfoDictionary.ContainsKey(ToolEnum.LPDAnalysis) && toolInfoDictionary.ContainsKey(ToolEnum.LEEDView))
                {
                    avfTP = toolInfoDictionary[ToolEnum.AVF];
                    lpdTP = toolInfoDictionary[ToolEnum.LPDAnalysis];
                    leedTP = toolInfoDictionary[ToolEnum.LEEDView];
                }

                if (null != avfTP && null != lpdTP && null != leedTP)
                {
                    if (avfTP.InstallExist || lpdTP.InstallExist || leedTP.InstallExist)
                    {
                        if (null == analysisPanel && null == analysisSplitButton)
                        {
                            analysisPanel = m_app.CreateRibbonPanel(tabName, "Analysis");

                            SplitButtonData splitButtonData = new SplitButtonData("HOKAnalysis", "HOK Analysis");
                            analysisSplitButton = analysisPanel.AddItem(splitButtonData) as SplitButton;
                            analysisSplitButton.IsSynchronizedWithCurrentItem = true;
                        }
                    }
                    if (null != analysisPanel && null != analysisSplitButton)
                    {
                        Dictionary<string, PushButton> analysisButtons = CheckExistingButtons(analysisSplitButton);
                        if (avfTP.InstallExist)
                        {
                            if (!analysisButtons.ContainsKey("AVF"))
                            {
                                //install
                                PushButton avfButton = analysisSplitButton.AddPushButton(new PushButtonData("AVF", "  AVF  ", avfTP.TempAssemblyPath, "HOK.AVFManager.Command")) as PushButton;
                                avfButton.LargeImage = ImageUtil.LoadBitmapImage("chart.ico");
                                avfButton.ToolTip = "Analysis Visualization Framework";
                                AddToolTips(avfButton);
                            }
                            else
                            {
                                //update
                                PushButton avfButton = analysisButtons["AVF"];
                                avfButton.AssemblyName = avfTP.TempAssemblyPath;
                            }

                        }
                        else if (!avfTP.InstallExist && analysisButtons.ContainsKey("AVF"))
                        {
                            //remove tools by beta installer
                            PushButton avfButton = analysisButtons["AVF"];
                            avfButton.Enabled = false;
                        }

                        if (lpdTP.InstallExist)
                        {
                            if (!analysisButtons.ContainsKey("LPD Analysis"))
                            {
                                //install
                                PushButton lpdButton = analysisSplitButton.AddPushButton(new PushButtonData("LPD Analysis", "LPD Analysis", lpdTP.TempAssemblyPath, "HOK.LPDCalculator.Command")) as PushButton;
                                lpdButton.LargeImage = ImageUtil.LoadBitmapImage("bulb.png");
                                lpdButton.ToolTip = "Calculating Lighting Power Density";
                                AddToolTips(lpdButton);
                            }
                            else
                            {
                                //update
                                PushButton lpdButton = analysisButtons["LPD Analysis"];
                                lpdButton.AssemblyName = lpdTP.TempAssemblyPath;
                            }
                        }
                        else if (!lpdTP.InstallExist && analysisButtons.ContainsKey("LPD Analysis"))
                        {
                            //remove tools by beta installer
                            PushButton lpdButton = analysisButtons["LPD Analysis"];
                            lpdButton.Enabled = false;
                        }

                        if (leedTP.InstallExist)
                        {
                            if (!analysisButtons.ContainsKey("LEED View Analysis"))
                            {
                                //install
                                PushButton leedButton = analysisSplitButton.AddPushButton(new PushButtonData("LEED View Analysis", "LEED View Analysis", leedTP.TempAssemblyPath, "HOK.ViewAnalysis.Command")) as PushButton;
                                leedButton.LargeImage = ImageUtil.LoadBitmapImage("eq.ico");
                                leedButton.ToolTip = "Calculating Area with Views for LEED IEQc 8.2";
                                AddToolTips(leedButton);
                            }
                            else
                            {
                                //update
                                PushButton leedButton = analysisButtons["LEED View Analysis"];
                                leedButton.AssemblyName = leedTP.TempAssemblyPath;
                            }
                        }
                        else if (!leedTP.InstallExist && analysisButtons.ContainsKey("LEED View Analysis"))
                        {
                            //remove tools by beta installer
                            PushButton leedButton = analysisButtons["LEED View Analysis"];
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

        private string GetTempInstallPath(string installPath)
        {
            string tempPath = "";
            try
            {
                string fileName = Path.GetFileName(installPath);
                tempPath = Path.Combine(toolManager.TempInstallDirectory, fileName);
                if (!File.Exists(tempPath))
                {
                    tempPath = installPath;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return tempPath;
        }

        private void ReadToolTips(string txtfile)
        {
            try
            {
                using (StreamReader reader = File.OpenText(txtfile))
                {
                    string line;
                    int index = 0;
                    ButtonData buttonData = new ButtonData();
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
                MessageBox.Show("Failed to read the information of tool tips.\n" + ex.Message, "Read Tool Tips", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddToolTips(PushButton button)
        {
            try
            {
                if (buttonDictionary.ContainsKey(button.Name))
                {
                    ButtonData buttonData = buttonDictionary[button.Name];
                    button.LongDescription = buttonData.Description;
                    ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, buttonData.HelpUrl);
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
