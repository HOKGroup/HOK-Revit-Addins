/********************************************
(C) Copyright 2011 HOK SF

Code managed by Jinsol Kim, Atlanta
*********************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Reflection;
using Autodesk.Revit.UI;

namespace HOK.RibbonTab
{

    public class AppCommand : IExternalApplication
    {
        private UIControlledApplication m_app;
        private string tabName = "";
        private string currentDirectory = "";
        private string currentAssembly = "";
        private Assembly assembly = null;
        private string tooltipFileName = "HOK.Tooltip.txt";
        private Dictionary<string, ButtonData> buttonDictionary = new Dictionary<string, ButtonData>();

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            m_app = application;
            //Create a custom riboon tab
            tabName = "   HOK   ";
            try
            {
                m_app.CreateRibbonTab(tabName);
            }
            catch { }

            currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            currentDirectory = Path.GetDirectoryName(currentAssembly);
            assembly = Assembly.GetExecutingAssembly();
            string tooltipTxt = currentDirectory + "/Resources/" + tooltipFileName;
            ReadToolTips(tooltipTxt);

            CreateHOKPushButtons();
            CreateCustomPushButtons();
            CreateDataPushButtons();
            CreateAVFPushButtons();

            return Result.Succeeded;
        }

        private void CreateHOKPushButtons()
        {
            try
            {
                bool utilityExist = false;
                RibbonPanel utilPanel = m_app.CreateRibbonPanel(tabName, "Utilities");
                //HOK Utilities
                if (File.Exists(currentDirectory + "/HOK.Utilities.dll") || File.Exists(currentDirectory + "/HOK.ElementTools.dll") || File.Exists(currentDirectory + "/HOK.ParameterTools.dll") || File.Exists(currentDirectory + "/HOK.ColorSchemeEditor.dll"))
                {
                    SplitButtonData splitButtonData = new SplitButtonData("HOKUtilities", "HOK Utilities");
                    SplitButton splitButton = utilPanel.AddItem(splitButtonData) as SplitButton;
                    ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\HOK Utilities_Instruction.pdf");
                    splitButton.SetContextualHelp(contextualHelp);

                    //ElementTools
                    if (File.Exists(currentDirectory + "/HOK.ElementTools.dll"))
                    {
                        PushButton pb1 = splitButton.AddPushButton(new PushButtonData("Element Tools", "Element Tools", currentDirectory + "/HOK.ElementTools.dll", "HOK.ElementTools.cmdElementTools")) as PushButton;
                        pb1.LargeImage = LoadBitmapImage(assembly, "element.ico");
                        pb1.ToolTip = "Room and Area Elements Tools";
                        AddToolTips(pb1);
                        utilityExist = true;
                    }

                    //ParameterTools
                    if (File.Exists(currentDirectory + "/HOK.ParameterTools.dll"))
                    {
                        PushButton pb2 = splitButton.AddPushButton(new PushButtonData("Parameter Tools", "Parameter Tools", currentDirectory + "/HOK.ParameterTools.dll", "HOK.ParameterTools.cmdParameterTools")) as PushButton;
                        pb2.LargeImage = LoadBitmapImage(assembly, "parameter.ico");
                        pb2.ToolTip = "Parameter Tools";
                        AddToolTips(pb2);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.FinishCreator.dll"))
                    {
                        PushButton pb3 = splitButton.AddPushButton(new PushButtonData("Finish Creator", "Finish Creator", currentDirectory + "/HOK.FinishCreator.dll", "HOK.FinishCreator.FinishCommand")) as PushButton;
                        pb3.LargeImage = LoadBitmapImage(assembly, "finish.png");
                        pb3.ToolTip = "Create floor finishes from the selected rooms.";
                        AddToolTips(pb3);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.CeilingHeight.dll"))
                    {
                        PushButton pb4 = splitButton.AddPushButton(new PushButtonData("Ceiling Height", "Ceiling Heights", currentDirectory + "/HOK.CeilingHeight.dll", "HOK.CeilingHeight.CeilingCommand")) as PushButton;
                        pb4.LargeImage = LoadBitmapImage(assembly, "height.png");
                        pb4.ToolTip = "Select rooms to measure the height from floors to ceilings.";
                        AddToolTips(pb4);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.LevelManager.dll"))
                    {
                        PushButton pb5 = splitButton.AddPushButton(new PushButtonData("Level Manager", "Level Manager", currentDirectory + "/HOK.LevelManager.dll", "HOK.LevelManager.LevelCommand")) as PushButton;
                        pb5.LargeImage = LoadBitmapImage(assembly, "level.png");
                        pb5.ToolTip = "Rehost elements from one level to anather. ";
                        AddToolTips(pb5);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.ViewDepth.dll"))
                    {
                        PushButton pb18 = splitButton.AddPushButton(new PushButtonData("View Depth", "View Depth", currentDirectory + "/HOK.ViewDepth.dll", "HOK.ViewDepth.ViewCommand")) as PushButton;
                        pb18.LargeImage = LoadBitmapImage(assembly, "camera.ico");
                        pb18.ToolTip = "Override the graphics of the element based on the distance";
                        pb18.ToolTipImage = LoadBitmapImage(assembly, "viewTooltip.png");
                        AddToolTips(pb18);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.Arrowhead.dll"))
                    {
                        PushButton pb19 = splitButton.AddPushButton(new PushButtonData("Leader Arrowhead", "Leader Arrowhead", currentDirectory + "/HOK.Arrowhead.dll", "HOK.Arrowhead.ArrowCommand")) as PushButton;
                        pb19.LargeImage = LoadBitmapImage(assembly, "arrowhead.png");
                        pb19.ToolTip = "Assign a leader arrowhead style to all tag types.";
                        AddToolTips(pb19);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.WorksetView.dll"))
                    {
                        PushButton pb19 = splitButton.AddPushButton(new PushButtonData("View Creator", "View Creator", currentDirectory + "/HOK.WorksetView.dll", "HOK.WorksetView.WorksetCommand")) as PushButton;
                        pb19.LargeImage = LoadBitmapImage(assembly, "workset.png");
                        pb19.ToolTip = "Create 3D Views for each workset.";
                        AddToolTips(pb19);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.ColorSchemeEditor.dll"))
                    {
                        PushButton pb20 = splitButton.AddPushButton(new PushButtonData("Color Editor", "Color Editor", currentDirectory + "/HOK.ColorSchemeEditor.dll", "HOK.ColorSchemeEditor.Command")) as PushButton;
                        pb20.LargeImage = LoadBitmapImage(assembly, "color32.png");
                        pb20.ToolTip = "Create color schemes by categories and parameter values.";
                        AddToolTips(pb20);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.DoorRoom.dll"))
                    {
                        PushButton pb21 = splitButton.AddPushButton(new PushButtonData("Door Link", "Door Link", currentDirectory + "/HOK.DoorRoom.dll", "HOK.DoorRoom.DoorCommand")) as PushButton;
                        pb21.LargeImage = LoadBitmapImage(assembly, "door.png");
                        pb21.ToolTip = "Set shared parameters with To and From room data.";
                        AddToolTips(pb21);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.RoomUpdater.dll"))
                    {
                        PushButton pb22 = splitButton.AddPushButton(new PushButtonData("Room Updater", "Room Updater", currentDirectory + "/HOK.RoomUpdater.dll", "HOK.RoomUpdater.RoomCommand")) as PushButton;
                        pb22.LargeImage = LoadBitmapImage(assembly, "container.png");
                        pb22.ToolTip = "Populate room parameters values into enclosed elements.";
                        AddToolTips(pb22);
                        utilityExist = true;
                    }

                    if (File.Exists(currentDirectory + "/HOK.RoomElevation.dll"))
                    {
                        PushButton pb23 = splitButton.AddPushButton(new PushButtonData("Room Elevation", "Room Elevation", currentDirectory + "/HOK.RoomElevation.dll", "HOK.RoomElevation.ElevationCommand")) as PushButton;
                        pb23.LargeImage = LoadBitmapImage(assembly, "elevation.png");
                        pb23.ToolTip = "Create elevation views by selecting rooms and walls to be faced.";
                        AddToolTips(pb23);
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
                MessageBox.Show("Failed to create the Utilities Panel.\n" + ex.Message, "Create Utilites Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateCustomPushButtons()
        {
            try
            {
                bool fileExist = false;
                RibbonPanel hokPanel = m_app.CreateRibbonPanel(tabName, "Customizations");

                if (File.Exists(currentDirectory + "/HOK.SheetManager.dll"))
                {
                    //SheetManager
                    PushButton pb6 = hokPanel.AddItem(new PushButtonData("Sheet Manager", "Sheet Manager", currentDirectory + "/HOK.SheetManager.dll", "HOK.SheetManager.cmdSheetManager")) as PushButton;
                    pb6.LargeImage = LoadBitmapImage(assembly, "sheet.ico");
                    pb6.ToolTip = "Sheet Manager";
                    AddToolTips(pb6);
                    hokPanel.AddSeparator();
                    fileExist = true;
                }

                if (File.Exists(currentDirectory + "/HOK.ModelManager.dll"))
                {
                    //ModelManager
                    SplitButtonData splitButtonData = new SplitButtonData("ModelManager", "Model Manager");
                    SplitButton splitButton = hokPanel.AddItem(splitButtonData) as SplitButton;
                    ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\ModelManager_Instruction.pdf");
                    splitButton.SetContextualHelp(contextualHelp);

                    PushButton pb16 = splitButton.AddPushButton(new PushButtonData("Project Replication", "Project Replication", currentDirectory + "/HOK.ModelManager.dll", "HOK.ModelManager.ProjectCommand"));
                    pb16.LargeImage = LoadBitmapImage(assembly, "project.png");
                    pb16.ToolTip = "Model Manager - Project Replication";
                    AddToolTips(pb16);
                    hokPanel.AddSeparator();
                    fileExist = true;
                }

                if (File.Exists(currentDirectory + "/HOK.BCFReader.dll"))
                {
                    PushButton pb7 = hokPanel.AddItem(new PushButtonData("BCF Reader", "BCF Reader", currentDirectory + "/HOK.BCFReader.dll", "HOK.BCFReader.Command")) as PushButton;
                    pb7.LargeImage = LoadBitmapImage(assembly, "comment.ico");
                    pb7.ToolTip = "BIM Collaboration Format (BCF) Reader";
                    AddToolTips(pb7);
                    hokPanel.AddSeparator();
                    fileExist = true;
                }

                if (File.Exists(currentDirectory + "/HOK.RoomsToMass.dll"))
                {
                    #region Create a SplitButton for user to create Mass or Transfer Data
                    SplitButtonData splitButtonData = new SplitButtonData("MassTool", "3D Mass");
                    SplitButton splitButton = hokPanel.AddItem(splitButtonData) as SplitButton;
                    splitButton.IsSynchronizedWithCurrentItem = true;
                    ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, @"V:\RVT-Data\HOK Program\Documentation\Mass Tool_Instruction.pdf");
                    splitButton.SetContextualHelp(contextualHelp);

                    PushButton pb8 = splitButton.AddPushButton(new PushButtonData("Create Mass", "Create Mass", currentDirectory + "/HOK.RoomsToMass.dll", "HOK.RoomsToMass.Command"));
                    pb8.LargeImage = LoadBitmapImage(assembly, "cube.png");
                    pb8.ToolTip = "Creates 3D Mass from rooms, areas and floors.";
                    pb8.ToolTipImage = LoadBitmapImage(assembly, "tooltip.png");
                    AddToolTips(pb8);

                    PushButton pb9 = splitButton.AddPushButton(new PushButtonData("Update Data", "Update Data", currentDirectory + "/HOK.RoomsToMass.dll", "HOK.RoomsToMass.DataCommand"));
                    pb9.LargeImage = LoadBitmapImage(assembly, "refresh.png");
                    pb9.ToolTip = "Transfer parameters values between Rooms, Areas, Floors and Masses";
                    AddToolTips(pb9);

                    PushButton pb10 = splitButton.AddPushButton(new PushButtonData("Mass Commands", "Mass Commands", currentDirectory + "/HOK.RoomsToMass.dll", "HOK.RoomsToMass.AssignerCommand"));
                    pb10.LargeImage = LoadBitmapImage(assembly, "shape.png");
                    pb10.ToolTip = "Assign parameters or split elements";
                    AddToolTips(pb10);
                    fileExist = true;

                    #endregion
                }

                if (!fileExist)
                {
                    hokPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create the Customizations panel.\n" + ex.Message, "Create Customizations Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateDataPushButtons()
        {
            try
            {
                if (File.Exists(currentDirectory + "/HOK.RevitDBManager.dll"))
                {
                    RibbonPanel dataPanel = m_app.CreateRibbonPanel(tabName, "Revit Data");
                    bool exist = false;
                    //DBManager_Sychronize
                    PushButton pb11 = dataPanel.AddItem(new PushButtonData("Data Sync", "Data Sync", currentDirectory + "/HOK.RevitDBManager.dll", "RevitDBManager.Command")) as PushButton;
                    pb11.LargeImage = LoadBitmapImage(assembly, "sync.ico");
                    pb11.ToolTip = "Data Sync";
                    AddToolTips(pb11);

                    //DBManager_Setup
                    PushButton pb12 = dataPanel.AddItem(new PushButtonData("Setup", "  Setup  ", currentDirectory + "/HOK.RevitDBManager.dll", "RevitDBManager.EditorCommand")) as PushButton;
                    pb12.LargeImage = LoadBitmapImage(assembly, "editor.ico");
                    pb12.ToolTip = "Setup";
                    AddToolTips(pb12);

                    //DBManager_Data Editor
                    PushButton pb13 = dataPanel.AddItem(new PushButtonData("Data Editor", "Data Editor", currentDirectory + "/HOK.RevitDBManager.dll", "RevitDBManager.ViewerCommand")) as PushButton;
                    pb13.LargeImage = LoadBitmapImage(assembly, "view.ico");
                    pb13.ToolTip = "Data Editor";
                    AddToolTips(pb13);

                    exist = true;

                    if (!exist)
                    {
                        dataPanel.Visible = false;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create the Revit Data panel.\n" + ex.Message, "Create Revit Data Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateAVFPushButtons()
        {
            try
            {
                if (File.Exists(currentDirectory + "/HOK.AVFManager.dll") || File.Exists(currentDirectory + "/HOK.LPDCalculator.dll") || File.Exists(currentDirectory + "/HOK.ViewAnalysis.dll"))
                {
                    RibbonPanel avfPanel = m_app.CreateRibbonPanel(tabName, "Analysis");

                    SplitButtonData splitButtonData = new SplitButtonData("HOKAnalysis", "HOK Analysis");
                    SplitButton splitButton = avfPanel.AddItem(splitButtonData) as SplitButton;
                    splitButton.IsSynchronizedWithCurrentItem = true;

                    if (File.Exists(currentDirectory + "/HOK.AVFManager.dll"))
                    {
                        PushButton pb14 = splitButton.AddPushButton(new PushButtonData("AVF", "  AVF  ", currentDirectory + "/HOK.AVFManager.dll", "HOK.AVFManager.Command")) as PushButton;
                        pb14.LargeImage = LoadBitmapImage(assembly, "chart.ico");
                        pb14.ToolTip = "Analysis Visualization Framework";
                        AddToolTips(pb14);

                    }

                    if (File.Exists(currentDirectory + "/HOK.LPDCalculator.dll"))
                    {
                        PushButton pb15 = splitButton.AddPushButton(new PushButtonData("LPD Analysis", "LPD Analysis", currentDirectory + "/HOK.LPDCalculator.dll", "HOK.LPDCalculator.Command")) as PushButton;
                        pb15.LargeImage = LoadBitmapImage(assembly, "bulb.png");
                        pb15.ToolTip = "Calculating Lighting Power Density";
                        AddToolTips(pb15);
                    }
#if RELEASE2015
                    if (File.Exists(currentDirectory + "/HOK.ViewAnalysis.dll"))
                    {
                        PushButton pb24 = splitButton.AddPushButton(new PushButtonData("LEED View Analysis", "LEED View Analysis", currentDirectory + "/HOK.ViewAnalysis.dll", "HOK.ViewAnalysis.Command")) as PushButton;
                        pb24.LargeImage = LoadBitmapImage(assembly, "eq.ico");
                        pb24.ToolTip = "Calculating Area with Views for LEED EQc 8.2";
                        AddToolTips(pb24);
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create Analysis panel.\n" + ex.Message, "Create Analysis Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

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
                MessageBox.Show("Failed to read the information of tool tips.\n" + ex.Message, "Read Tool Tips", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Failed to add tool tips.\n" + ex.Message, "Add Tool Tips", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                MessageBox.Show("Failed to load the embedded resource image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return image;
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
