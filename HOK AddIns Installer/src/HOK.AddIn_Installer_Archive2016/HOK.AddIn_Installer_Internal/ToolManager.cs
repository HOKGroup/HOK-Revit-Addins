using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace HOK.AddIn_Installer_Internal
{
    public enum Tool2013
    {
        Default=0,
        ElementTools=1,
        ParameterTools=2,
        ExportRevisions=3,
        BatchExport=4,
        DoorMark=5,
        ImportImage=6,
        SheetManager=7,
        BCFReader=8,
        MassTool=9,
        RevitData=10,
        Analysis=11,
        Utility=12,
        CreateFinishes=13,
        ModelReporting=14,
        HOKNavigator=15,
        SmartBCF=16,
        FileMonitor=17,
        ProjectMonitor=18,
    }

    public enum Tool2014
    {
        Default = 0,
        ElementTools = 1,
        ParameterTools = 2,
        ExportRevisions = 3,
        BatchExport = 4,
        DoorMark = 5,
        ImportImage = 6,
        SheetManager = 7,
        BCFReader = 8,
        MassTool = 9,
        RevitData = 10,
        Analysis = 11,
        Utility=12,
        ModelReporting=14,
        HOKNavigator=15,
        ModelManager=16,
        ColorEditor=17,
        SmartBCF=18,
        FileMonitor=19,
        ProjectMonitor= 20
    }

    public enum Tool2015
    {
        Default = 0,
        ElementTools = 1,
        ParameterTools = 2,
        ExportRevisions = 3,
        BatchExport = 4,
        DoorMark = 5,
        ImportImage = 6,
        SheetManager = 7,
        BCFReader = 8,
        MassTool = 9,
        RevitData = 10,
        Analysis = 11,
        Utility = 12,
        ModelReporting = 14,
        HOKNavigator = 15,
        ModelManager = 16,
        ColorEditor = 17,
        SmartBCF=18,
        FileMonitor=19,
        ProjectMonitor= 20
    }

    //External Application Beta Only
    public enum Tool2016
    {
        SmartBCF=1,
        FileMonitor=2,
        ProjectMonitor=3
    }

    public enum TargetSoftware
    {
        Revit_2013=0,
        Revit_2014=1,
        Revit_2015=2,
        Revit_2016=3,
        Dynamo=4
    }

    public class ToolManager
    {
        private string masterDirectory = "";
        private string installDirectory = "";
        private string programDirectory = "";
        private string betaDirectory = "";
        private StringBuilder strBuilder = new StringBuilder();

        public string MasterDirectory { get { return masterDirectory; } set { masterDirectory = value; } }
        public string InstallDirectory { get { return installDirectory; } set { installDirectory = value; } }
        public string ProgramDirectory { get { return programDirectory; } set { programDirectory = value; } }
        public string BetaDirectory { get { return betaDirectory; } set { betaDirectory = value; } }

        public ToolManager()
        {
        }

        public Dictionary<Tool2013, RevitToolProperties> Get2013Dictionary(Dictionary<Tool2013, RevitToolProperties> deprecatedTools)
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\2013";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\2013";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2013";
            programDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Autodesk\REVIT\Addins\2013";

            Dictionary<Tool2013, RevitToolProperties> dictionary = new Dictionary<Tool2013, RevitToolProperties>();

            Array arrayTools = Enum.GetValues(typeof(Tool2013));
            ProgressForm progressForm = new ProgressForm("Gathering information about installed components in 2013. . .");
            progressForm.SetMaximumValue(arrayTools.Length);
            progressForm.Show();
            progressForm.Refresh();

            strBuilder = new StringBuilder();
            foreach (Tool2013 tool in arrayTools)
            {
                try
                {
                    progressForm.StepForward();
                    if (deprecatedTools.ContainsKey(tool)) { continue; }

                    RevitToolProperties tp = new RevitToolProperties();
                    List<string> fileNames = new List<string>();
                    switch (tool)
                    {
                        case Tool2013.Default:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\AdWindows.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\dao.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\dao360.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\DBManagedServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\ManagedMC3.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Access.Dao.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Access.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Excel.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Vbe.Interop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\office.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\UIFramework.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\UIFrameworkServices.dll");
                            fileNames.Add("\\HOK.RibbonTab.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RibbonTab.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Tooltip.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Help.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Installer.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\info.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\hok.png");

                            tp.ToolName = "Default";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RibbonTab.dll";
                            tp.DLLName = "HOK.RibbonTab.dll";
                            tp.ImageIndex = -1;
                            tp.BetaOnly = false;
                            break;
                        case Tool2013.ElementTools:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ElementTools.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\element.ico");

                            tp.ToolName = "Element Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ElementTools.dll";
                            tp.DLLName = "HOK.ElementTools.dll";
                            tp.ImageIndex = 0;
                            tp.BetaOnly = false;
                            break;
                        case Tool2013.ParameterTools:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ParameterTools.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\parameter.ico");

                            tp.ToolName = "Parameter Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ParameterTools.dll";
                            tp.DLLName = "HOK.ParameterTools.dll";
                            tp.ImageIndex = 1;
                            tp.BetaOnly = false;
                            break;
                        case Tool2013.SheetManager:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.SheetManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sheet.ico");

                            tp.ToolName = "Sheet Manager";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.SheetManager.dll";
                            tp.DLLName = "HOK.SheetManager.dll";
                            tp.ImageIndex = 2;
                            tp.BetaOnly = false;
                            break;
                        case Tool2013.BCFReader:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.BCFReader.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\ICSharpCode.SharpZipLib.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\comment.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\markup.xsd");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\visinfo.xsd");

                            tp.ToolName = "BCF Reader";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.BCFReader.dll";
                            tp.DLLName = "HOK.BCFReader.dll";
                            tp.ImageIndex = 3;
                            tp.BetaOnly = false;
                            break;
                        case Tool2013.MassTool:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomsToMass.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\cube.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\refresh.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\tooltip.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\shape.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Mass Shared Parameters.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Mass.rfa");

                            tp.ToolName = "Mass Tool";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RoomsToMass.dll";
                            tp.DLLName = "HOK.RoomsToMass.dll";
                            tp.ImageIndex = 4;
                            tp.BetaOnly = false;
                            break;
                        case Tool2013.RevitData:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RevitDBManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\default.bmp");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\editor.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\eye.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sync.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\view.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\default.bmp");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\editor.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\eye.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sync.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\view.ico");

                            tp.ToolName = "Revit Data Manager";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RevitDBManager.dll";
                            tp.DLLName = "HOK.RevitDBManager.dll";
                            tp.ImageIndex = 5;
                            tp.BetaOnly = false;
                            break;
                        case Tool2013.Analysis:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.AVFManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\chart.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\DefaultSettings.xml");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\PointOfView.rfa");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.LPDCalculator.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\bulb.png");

                            tp.ToolName = "Analysis Tool";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.AVFManager.dll";
                            tp.DLLName = "HOK.AVFManager.dll";
                            tp.ImageIndex = 6;
                            tp.BetaOnly = false;
                            break;

                        case Tool2013.Utility:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Utilities.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Arrowhead.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.CeilingHeight.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.DoorRoom.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.FinishCreator.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.LevelManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomElevation.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomUpdater.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ViewDepth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.WorksetView.dll");

                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\height.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\level.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\finish.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\camera.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\viewTooltip.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\arrowhead.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\workset.png");

                            tp.ToolName = "Utility Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.Utilities.dll";
                            tp.DLLName = "HOK.Utilities.dll";
                            tp.ImageIndex = 7;
                            tp.BetaOnly = false;
                            break;
                        case Tool2013.SmartBCF:
                            fileNames.Add("\\HOK.SmartBCF.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.SmartBCF.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Core.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Drive.v2.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Client.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Spreadsheets.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\log4net.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.Desktop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Newtonsoft.Json.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Primitives.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Zlib.Portable.dll");
                            
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\walker.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Addins Shared Parameters.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\client_secrets_samrtBCF.json");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK smartBCF.p12");
                            
                            tp.ToolName = "Smart BCF";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.SmartBCF.dll";
                            tp.DLLName = "HOK.SmartBCF.dll";
                            tp.ImageIndex = 10;
                            tp.BetaOnly = true;
                            break;

                        case Tool2013.FileMonitor:
                            fileNames.Add("\\HOK.FileOpeningMonitor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.FileOnpeningMonitor.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\fmeserverapidotnet.dll");

                            tp.ToolName = "Central File Monitor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.FileOnpeningMonitor.dll";
                            tp.DLLName = "HOK.FileOnpeningMonitor.dll";
                            tp.ImageIndex = 11;
                            tp.BetaOnly = true;
                            break;
                        case Tool2013.ProjectMonitor:
                            fileNames.Add("\\HOK.ProjectMonitor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ProjectMonitor.dll");

                            tp.ToolName = "Project Monitor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ProjectMonitor.dll";
                            tp.DLLName = "HOK.ProjectMonitor.dll";
                            tp.ImageIndex = 12;
                            tp.BetaOnly = true;
                            break;
                    }
                    tp.TargetSoftWareEnum = TargetSoftware.Revit_2013;
                    tp.ToolEnum2013 = tool;
                    tp.FilePaths = fileNames;

                    if (!tp.BetaOnly)
                    {
                        tp.ReleaseVersionInfo = GetReleaseVersion(tp.DllPath);
                        tp.ReleaseDate = GetReleaseDate(tp.DllPath);
                    }

                    tp.InstallVersionInfo = GetInstalledVersion(tp);

                    string betaPath = betaDirectory + tp.DllPath;
                    tp.BetaVersionInfo = GetBetaReleaseVersion(betaPath);
                    tp.BetaDate = GetBetaReleaseDate(betaPath);
                    dictionary.Add(tool, tp);
                }
                catch (Exception ex)
                {
                    string message = tool.ToString() + ": " + ex.Message;
                    strBuilder.AppendLine(message);
                }
            }

            progressForm.Close();

            if (strBuilder.Length > 0)
            {
                MessageBox.Show(strBuilder.ToString(), "ToolManager:Get2013Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dictionary;
        }

        public Dictionary<Tool2014, RevitToolProperties> Get2014Dictionary(Dictionary<Tool2014, RevitToolProperties> deprecatedTools)
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\2014";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\2014";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2014";
            programDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Autodesk\REVIT\Addins\2014";

            Dictionary<Tool2014, RevitToolProperties> dictionary = new Dictionary<Tool2014, RevitToolProperties>();

            Array arrayTools = Enum.GetValues(typeof(Tool2014));
            ProgressForm progressForm = new ProgressForm("Gathering information about installed components in 2014. . .");
            progressForm.SetMaximumValue(arrayTools.Length);
            progressForm.Show();
            progressForm.Refresh();

            strBuilder = new StringBuilder();
            foreach (Tool2014 tool in arrayTools)
            {
                try
                {
                    progressForm.StepForward();
                    if (deprecatedTools.ContainsKey(tool)) { continue; }
                    RevitToolProperties tp = new RevitToolProperties();
                    List<string> fileNames = new List<string>();
                    switch (tool)
                    {
                        case Tool2014.Default:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\AdWindows.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\dao.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\dao360.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\DBManagedServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\ManagedMC3.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Access.Dao.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Access.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Excel.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Vbe.Interop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\office.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\UIFramework.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\UIFrameworkServices.dll");

                            fileNames.Add("\\HOK.RibbonTab.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RibbonTab.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Tooltip.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Help.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Installer.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\hok.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\info.ico");

                            tp.ToolName = "Default";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RibbonTab.dll";
                            tp.DLLName = "HOK.RibbonTab.dll";
                            tp.ImageIndex = -1;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.ElementTools:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ElementTools.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\element.ico");

                            tp.ToolName = "Element Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ElementTools.dll";
                            tp.DLLName = "HOK.ElementTools.dll";
                            tp.ImageIndex = 0;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.ParameterTools:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ParameterTools.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\parameter.ico");

                            tp.ToolName = "Parameter Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ParameterTools.dll";
                            tp.DLLName = "HOK.ParameterTools.dll";
                            tp.ImageIndex = 1;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.SheetManager:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.SheetManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sheet.ico");

                            tp.ToolName = "Sheet Manager";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.SheetManager.dll";
                            tp.DLLName = "HOK.SheetManager.dll";
                            tp.ImageIndex = 2;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.BCFReader:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.BCFReader.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\ICSharpCode.SharpZipLib.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\comment.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\markup.xsd");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\visinfo.xsd");

                            tp.ToolName = "BCF Reader";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.BCFReader.dll";
                            tp.DLLName = "HOK.BCFReader.dll";
                            tp.ImageIndex = 3;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.MassTool:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomsToMass.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\cube.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\refresh.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\tooltip.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\shape.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Mass Shared Parameters.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Mass.rfa");

                            tp.ToolName = "Mass Tool";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RoomsToMass.dll";
                            tp.DLLName = "HOK.RoomsToMass.dll";
                            tp.ImageIndex = 4;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.RevitData:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RevitDBManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\default.bmp");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\editor.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\eye.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sync.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\view.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\default.bmp");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\editor.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\eye.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sync.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\view.ico");

                            tp.ToolName = "Revit Data Manager";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RevitDBManager.dll";
                            tp.DLLName = "HOK.RevitDBManager.dll";
                            tp.ImageIndex = 5;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.Analysis:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.AVFManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\chart.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\DefaultSettings.xml");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\PointOfView.rfa");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.LPDCalculator.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\bulb.png");

                            tp.ToolName = "Analysis Tool";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.AVFManager.dll";
                            tp.DLLName = "HOK.AVFManager.dll";
                            tp.ImageIndex = 6;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.Utility:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Utilities.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Arrowhead.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.CeilingHeight.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.DoorRoom.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.FinishCreator.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.LevelManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomElevation.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomUpdater.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ViewDepth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.WorksetView.dll");

                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\height.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\level.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\finish.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\camera.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\viewTooltip.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\arrowhead.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\workset.png");

                            tp.ToolName = "Utility Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.Utilities.dll";
                            tp.DLLName = "HOK.Utilities.dll";
                            tp.ImageIndex = 7;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.ModelManager:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ModelManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Core.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Drive.v2.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Client.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Spreadsheets.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\log4net.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.Desktop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Newtonsoft.Json.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Primitives.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Zlib.Portable.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\client_secrets_ProjectReplicator.json");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK Project Replicator.p12");

                            tp.ToolName = "Model Manager";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ModelManager.dll";
                            tp.DLLName = "HOK.ModelManager.dll";
                            tp.ImageIndex = 8;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.ColorEditor:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ColorSchemeEditor.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\ICSharpCode.SharpZipLib.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Xceed.Wpf.Toolkit.dll");
                            
                            tp.ToolName = "Color Editor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ColorSchemeEditor.dll";
                            tp.DLLName = "HOK.ColorSchemeEditor.dll";
                            tp.ImageIndex = 9;
                            tp.BetaOnly = false;
                            break;
                        case Tool2014.SmartBCF:
                            fileNames.Add("\\HOK.SmartBCF.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.SmartBCF.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Core.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Drive.v2.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Client.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Spreadsheets.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\log4net.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.Desktop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Newtonsoft.Json.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Primitives.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Zlib.Portable.dll");

                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\walker.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Addins Shared Parameters.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\client_secrets_samrtBCF.json");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK smartBCF.p12");
                            
                            tp.ToolName = "Smart BCF";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.SmartBCF.dll";
                            tp.DLLName = "HOK.SmartBCF.dll";
                            tp.ImageIndex = 10;
                            tp.BetaOnly = true;
                            break;

                        case Tool2014.FileMonitor:
                            fileNames.Add("\\HOK.FileOpeningMonitor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.FileOnpeningMonitor.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\fmeserverapidotnet.dll");

                            tp.ToolName = "Central File Monitor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.FileOnpeningMonitor.dll";
                            tp.DLLName = "HOK.FileOnpeningMonitor.dll";
                            tp.ImageIndex = 11;
                            tp.BetaOnly = true;
                            break;
                        case Tool2014.ProjectMonitor:
                            fileNames.Add("\\HOK.ProjectMonitor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ProjectMonitor.dll");

                            tp.ToolName = "Project Monitor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ProjectMonitor.dll";
                            tp.DLLName = "HOK.ProjectMonitor.dll";
                            tp.ImageIndex = 12;
                            tp.BetaOnly = true;
                            break;

                    }
                    tp.TargetSoftWareEnum = TargetSoftware.Revit_2014;
                    tp.ToolEnum2014 = tool;
                    tp.FilePaths = fileNames;

                    if (!tp.BetaOnly)
                    {
                        tp.ReleaseVersionInfo = GetReleaseVersion(tp.DllPath);
                        tp.ReleaseDate = GetReleaseDate(tp.DllPath);
                    }

                    tp.InstallVersionInfo = GetInstalledVersion(tp);

                    string betaPath = betaDirectory + tp.DllPath;
                    tp.BetaVersionInfo = GetBetaReleaseVersion(betaPath);
                    tp.BetaDate = GetBetaReleaseDate(betaPath);
                    dictionary.Add(tool, tp);
                }
                catch (Exception ex)
                {
                    string message = tool.ToString() + ": " + ex.Message;
                    strBuilder.AppendLine(message);
                }
            }

            progressForm.Close();

            if (strBuilder.Length > 0)
            {
                MessageBox.Show(strBuilder.ToString(), "ToolManager:Get2014Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dictionary;
        }

        public Dictionary<Tool2015, RevitToolProperties> Get2015Dictionary(Dictionary<Tool2015, RevitToolProperties> deprecatedTools)
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\2015";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\2015";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2015";
            programDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Autodesk\REVIT\Addins\2015";

            Dictionary<Tool2015, RevitToolProperties> dictionary = new Dictionary<Tool2015, RevitToolProperties>();

            Array arrayTools = Enum.GetValues(typeof(Tool2015));
            ProgressForm progressForm = new ProgressForm("Gathering information about installed components in 2015. . .");
            progressForm.SetMaximumValue(arrayTools.Length);
            progressForm.Show();
            progressForm.Refresh();

            strBuilder = new StringBuilder();
            foreach (Tool2015 tool in arrayTools)
            {
                try
                {
                    progressForm.StepForward();
                    if (deprecatedTools.ContainsKey(tool)) { continue; }
                    RevitToolProperties tp = new RevitToolProperties();
                    List<string> fileNames = new List<string>();
                    switch (tool)
                    {
                        case Tool2015.Default:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\AdWindows.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\dao.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\dao360.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\DBManagedServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\ManagedMC3.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Access.Dao.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Access.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Excel.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Vbe.Interop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\office.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\UIFramework.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\UIFrameworkServices.dll");

                            fileNames.Add("\\HOK.RibbonTab.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RibbonTab.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Tooltip.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Help.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Installer.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\hok.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\info.ico");

                            tp.ToolName = "Default";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RibbonTab.dll";
                            tp.DLLName = "HOK.RibbonTab.dll";
                            tp.ImageIndex = -1;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.ElementTools:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ElementTools.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\element.ico");

                            tp.ToolName = "Element Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ElementTools.dll";
                            tp.DLLName = "HOK.ElementTools.dll";
                            tp.ImageIndex = 0;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.ParameterTools:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ParameterTools.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\parameter.ico");

                            tp.ToolName = "Parameter Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ParameterTools.dll";
                            tp.DLLName = "HOK.ParameterTools.dll";
                            tp.ImageIndex = 1;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.SheetManager:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.SheetManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sheet.ico");

                            tp.ToolName = "Sheet Manager";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.SheetManager.dll";
                            tp.DLLName = "HOK.SheetManager.dll";
                            tp.ImageIndex = 2;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.BCFReader:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.BCFReader.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\ICSharpCode.SharpZipLib.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\comment.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\markup.xsd");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\visinfo.xsd");

                            tp.ToolName = "BCF Reader";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.BCFReader.dll";
                            tp.DLLName = "HOK.BCFReader.dll";
                            tp.ImageIndex = 3;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.MassTool:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomsToMass.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\cube.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\refresh.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\tooltip.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\shape.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Mass Shared Parameters.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Mass.rfa");

                            tp.ToolName = "Mass Tool";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RoomsToMass.dll";
                            tp.DLLName = "HOK.RoomsToMass.dll";
                            tp.ImageIndex = 4;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.RevitData:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RevitDBManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\default.bmp");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\editor.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\eye.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sync.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\view.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\default.bmp");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\editor.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\eye.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sync.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\view.ico");

                            tp.ToolName = "Revit Data Manager";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RevitDBManager.dll";
                            tp.DLLName = "HOK.RevitDBManager.dll";
                            tp.ImageIndex = 5;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.Analysis:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.AVFManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\chart.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\DefaultSettings.xml");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\PointOfView.rfa");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.LPDCalculator.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\bulb.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ViewAnalysis.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Addins Shared Parameters.txt");

                            tp.ToolName = "Analysis Tool";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.AVFManager.dll";
                            tp.DLLName = "HOK.AVFManager.dll";
                            tp.ImageIndex = 6;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.Utility:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Utilities.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Arrowhead.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.CeilingHeight.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.DoorRoom.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.FinishCreator.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.LevelManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomElevation.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomUpdater.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ViewDepth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.WorksetView.dll");

                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\height.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\level.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\finish.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\camera.ico");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\viewTooltip.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\arrowhead.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\workset.png");

                            tp.ToolName = "Utility Tools";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.Utilities.dll";
                            tp.DLLName = "HOK.Utilities.dll";
                            tp.ImageIndex = 7;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.ModelManager:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ModelManager.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Core.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Drive.v2.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Client.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Spreadsheets.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\log4net.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.Desktop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Newtonsoft.Json.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Primitives.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Zlib.Portable.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\client_secrets_ProjectReplicator.json");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK Project Replicator.p12");

                            tp.ToolName = "Model Manager";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ModelManager.dll";
                            tp.DLLName = "HOK.ModelManager.dll";
                            tp.ImageIndex = 8;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.ColorEditor:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ColorSchemeEditor.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\ICSharpCode.SharpZipLib.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Xceed.Wpf.Toolkit.dll");

                            tp.ToolName = "Color Editor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ColorSchemeEditor.dll";
                            tp.DLLName = "HOK.ColorSchemeEditor.dll";
                            tp.ImageIndex = 9;
                            tp.BetaOnly = false;
                            break;
                        case Tool2015.SmartBCF:
                            fileNames.Add("\\HOK.SmartBCF.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.SmartBCF.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Auth.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Core.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.Drive.v2.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.Apis.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Client.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Spreadsheets.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\log4net.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.Desktop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Threading.Tasks.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Newtonsoft.Json.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Net.Http.Primitives.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Zlib.Portable.dll");

                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\walker.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Addins Shared Parameters.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\client_secrets_samrtBCF.json");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK smartBCF.p12");
                            
                            tp.ToolName = "Smart BCF";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.SmartBCF.dll";
                            tp.DLLName = "HOK.SmartBCF.dll";
                            tp.ImageIndex = 10;
                            tp.BetaOnly = true;
                            break;
                            
                        case Tool2015.FileMonitor:
                            fileNames.Add("\\HOK.FileOpeningMonitor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.FileOnpeningMonitor.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\fmeserverapidotnet.dll");

                            tp.ToolName = "Central File Monitor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.FileOnpeningMonitor.dll";
                            tp.DLLName = "HOK.FileOnpeningMonitor.dll";
                            tp.ImageIndex = 11;
                            tp.BetaOnly = true;
                            break;

                        case Tool2015.ProjectMonitor:
                            fileNames.Add("\\HOK.ProjectMonitor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ProjectMonitor.dll");

                            tp.ToolName = "Project Monitor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ProjectMonitor.dll";
                            tp.DLLName = "HOK.ProjectMonitor.dll";
                            tp.ImageIndex = 12;
                            tp.BetaOnly = true;
                            break;

                    }
                    tp.TargetSoftWareEnum = TargetSoftware.Revit_2015;
                    tp.ToolEnum2015 = tool;
                    tp.FilePaths = fileNames;

                    if (!tp.BetaOnly)
                    {
                        tp.ReleaseVersionInfo = GetReleaseVersion(tp.DllPath);
                        tp.ReleaseDate = GetReleaseDate(tp.DllPath);
                    }

                    tp.InstallVersionInfo = GetInstalledVersion(tp);

                    string betaPath = betaDirectory + tp.DllPath;
                    tp.BetaVersionInfo = GetBetaReleaseVersion(betaPath);
                    tp.BetaDate = GetBetaReleaseDate(betaPath);
                    dictionary.Add(tool, tp);
                }
                catch (Exception ex)
                {
                    string message = tool.ToString() + ": " + ex.Message;
                    strBuilder.AppendLine(message);
                }
            }

            progressForm.Close();

            if (strBuilder.Length > 0)
            {
                MessageBox.Show(strBuilder.ToString(), "ToolManager:Get2015Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dictionary;
        }

        public Dictionary<Tool2016, RevitToolProperties> Get2016Dictionary(Dictionary<Tool2016, RevitToolProperties> deprecatedTools)
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\2016";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\2016";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2016";
            programDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Autodesk\REVIT\Addins\2016";

            Dictionary<Tool2016, RevitToolProperties> dictionary = new Dictionary<Tool2016, RevitToolProperties>();

            Array arrayTools = Enum.GetValues(typeof(Tool2016));
            ProgressForm progressForm = new ProgressForm("Gathering information about installed components in 2016. . .");
            progressForm.SetMaximumValue(arrayTools.Length);
            progressForm.Show();
            progressForm.Refresh();

            strBuilder = new StringBuilder();
            foreach (Tool2016 tool in arrayTools)
            {
                try
                {
                    progressForm.StepForward();
                    if (deprecatedTools.ContainsKey(tool)) { continue; }
                    RevitToolProperties tp = new RevitToolProperties();
                    List<string> fileNames = new List<string>();
                    switch (tool)
                    {
                        case Tool2016.SmartBCF:
                            fileNames.Add("\\HOK.SmartBCF.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.SmartBCF.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.Auth.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.Auth.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.Core.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.Drive.v2.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.PlatformServices.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.GData.Client.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.GData.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.GData.Spreadsheets.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\log4net.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Microsoft.Threading.Tasks.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Microsoft.Threading.Tasks.Extensions.Desktop.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Microsoft.Threading.Tasks.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Newtonsoft.Json.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\System.Net.Http.Extensions.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\System.Net.Http.Primitives.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Zlib.Portable.dll");

                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Resources\\walker.png");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Resources\\Addins Shared Parameters.txt");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\Resources\\HOK smartBCF.p12");

                            tp.ToolName = "Smart BCF";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents_Beta\\HOK.SmartBCF.dll";
                            tp.DLLName = "HOK.SmartBCF.dll";
                            tp.ImageIndex = 10;
                            tp.BetaOnly = true;
                            break;

                        case Tool2016.FileMonitor:
                            fileNames.Add("\\HOK.FileOpeningMonitor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.FileOnpeningMonitor.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\fmeserverapidotnet.dll");

                            tp.ToolName = "Central File Monitor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents_Beta\\HOK.FileOnpeningMonitor.dll";
                            tp.DLLName = "HOK.FileOnpeningMonitor.dll";
                            tp.ImageIndex = 11;
                            tp.BetaOnly = true;
                            break;

                        case Tool2016.ProjectMonitor:
                            fileNames.Add("\\HOK.ProjectMonitor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.ProjectMonitor.dll");

                            tp.ToolName = "Project Monitor";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents_Beta\\HOK.ProjectMonitor.dll";
                            tp.DLLName = "HOK.ProjectMonitor.dll";
                            tp.ImageIndex = 12;
                            tp.BetaOnly = true;
                            break;

                    }
                    tp.TargetSoftWareEnum = TargetSoftware.Revit_2016;
                    tp.ToolEnum2016 = tool;
                    tp.FilePaths = fileNames;

                    if (!tp.BetaOnly)
                    {
                        tp.ReleaseVersionInfo = GetReleaseVersion(tp.DllPath);
                        tp.ReleaseDate = GetReleaseDate(tp.DllPath);
                    }

                    tp.InstallVersionInfo = GetInstalledVersion(tp);

                    string betaPath = betaDirectory + tp.DllPath;
                    tp.BetaVersionInfo = GetBetaReleaseVersion(betaPath);
                    tp.BetaDate = GetBetaReleaseDate(betaPath);
                    dictionary.Add(tool, tp);
                }
                catch (Exception ex)
                {
                    string message = tool.ToString() + ": " + ex.Message;
                    strBuilder.AppendLine(message);
                }
            }

            progressForm.Close();

            if (strBuilder.Length > 0)
            {
                MessageBox.Show(strBuilder.ToString(), "Collecting 2016 Tools Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dictionary;
        }


        public Dictionary<string, DynamoToolProperties> GetDynamoDictionary(Dictionary<string, DynamoToolProperties> deprecatedTools)
        {
            Dictionary<string, DynamoToolProperties> dictionary = new Dictionary<string, DynamoToolProperties>();
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Dynamo\Standard Definitions";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Dynamo\Beta Definitions";
            installDirectory = @"C:\Autodesk\Dynamo\Core\definitions";
            programDirectory = @"C:\Autodesk\Dynamo\Core\definitions";

            List<string> standardFiles = new List<string>();
            standardFiles.AddRange(Directory.GetFiles(masterDirectory, "*.dyf", SearchOption.TopDirectoryOnly));
            standardFiles.AddRange(Directory.GetFiles(masterDirectory, "*.dyn", SearchOption.TopDirectoryOnly));

            List<string> betaFiles = new List<string>();
            betaFiles.AddRange(Directory.GetFiles(betaDirectory, "*.dyf", SearchOption.TopDirectoryOnly));
            betaFiles.AddRange(Directory.GetFiles(betaDirectory, "*.dyn", SearchOption.TopDirectoryOnly));

            ProgressForm progressForm = new ProgressForm("Gathering information about installed Dynamo definitions. . .");
            progressForm.SetMaximumValue(standardFiles.Count + betaFiles.Count);
            progressForm.Show();
            progressForm.Refresh();

            strBuilder = new StringBuilder();
            foreach(string filePath in standardFiles)
            {
                progressForm.StepForward();
                string defName = Path.GetFileNameWithoutExtension(filePath);

                if (deprecatedTools.ContainsKey(defName)) { continue; }
                try
                {
                    DynamoToolProperties tp = new DynamoToolProperties();
                    tp.ToolName = defName;

                    FileInfo releasedInfo = new FileInfo(filePath);
                    tp.ReleasedFileInfo = releasedInfo;

                    string extension = Path.GetExtension(filePath);
                    tp.FileType = (extension.Contains("dyf")) ? "Custom Node" : "Workspace";

                    string fileName = Path.GetFileName(filePath);
                    tp.FileName = fileName;

                    string installedPath = Path.Combine(installDirectory, fileName);
                    if (File.Exists(installedPath))
                    {
                        FileInfo installedInfo = new FileInfo(installedPath);
                        tp.InstalledFileInfo = installedInfo;
                    }

                    if (!dictionary.ContainsKey(tp.ToolName))
                    {
                        dictionary.Add(tp.ToolName, tp);
                    }
                }
                catch (Exception ex)
                {
                    string message = defName + ": " + ex.Message;
                    strBuilder.AppendLine(message);
                }
            }

            foreach(string filePath in betaFiles)
            {
                progressForm.StepForward();
                string defName = Path.GetFileNameWithoutExtension(filePath);
                if (deprecatedTools.ContainsKey(defName)) { continue; }

                try
                {
                    FileInfo betaInfo = new FileInfo(filePath);
                    string extension = Path.GetExtension(filePath);
                   
                    string fileName = Path.GetFileName(filePath);
                    
                    string installedPath = Path.Combine(installDirectory, fileName);

                    if (dictionary.ContainsKey(defName))
                    {
                        DynamoToolProperties tp = dictionary[defName];
                        tp.BetaFileInfo = betaInfo;
                        tp.FileType = (extension.Contains("dyf")) ? "Custom Node" : "Workspace";
                        dictionary.Remove(tp.ToolName);
                        dictionary.Add(tp.ToolName, tp);
                    }
                    else
                    {
                        DynamoToolProperties tp = new DynamoToolProperties();
                        tp.ToolName = defName;
                        tp.FileName = fileName;
                        tp.FileType = (extension.Contains("dyf")) ? "Custom Node" : "Workspace";
                        tp.BetaFileInfo = betaInfo;
                        if (File.Exists(installedPath))
                        {
                            FileInfo installedInfo = new FileInfo(installedPath);
                            tp.InstalledFileInfo = installedInfo;
                            dictionary.Add(tp.ToolName, tp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = defName + ": " + ex.Message;
                    strBuilder.AppendLine(message);
                }
            }
            
            progressForm.Close();

            if (strBuilder.Length > 0)
            {
                MessageBox.Show(strBuilder.ToString(), "ToolManager:GetDynamoDictionary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dictionary;
        }

        private FileVersionInfo GetReleaseVersion(string filePath)
        {
            FileVersionInfo versionInfo = null;
            try
            {
                string dllPath = masterDirectory + filePath;
                if (File.Exists(dllPath))
                {
                    versionInfo = FileVersionInfo.GetVersionInfo(dllPath);
                }
                else
                {
                    strBuilder.AppendLine("A master file cannot be found.\n" + filePath);
                    //MessageBox.Show("A master file cannot be found.\n" + filePath + "\n You may not be in the HOK network connection.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                strBuilder.AppendLine("File Not Found: " + filePath + "\n" + ex.Message);
                //MessageBox.Show("Failed to get the version of a realsed dll.\n" + filePath + "\n" + ex.Message, "MainForm:GetReleaseVersion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return versionInfo;
        }

        private string GetReleaseDate(string filePath)
        {
            string date = "";
            try
            {
                string dllPath = masterDirectory + filePath;
                if (File.Exists(dllPath))
                {
                    FileInfo fileInfo = new FileInfo(dllPath);
                    date = fileInfo.LastWriteTime.Date.ToString("d");
                }
            }
            catch (Exception ex)
            {
                strBuilder.AppendLine("File Not Found: " + filePath + "\n" + ex.Message);
                //MessageBox.Show("Failed to get the date of a released dll.\n" + filePath + "\n" + ex.Message, "MainForm:GetReleaseDate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return date;
        }

        private FileVersionInfo GetInstalledVersion(RevitToolProperties tp)
        {
            FileVersionInfo versionInfo = null;
            try
            {
                string dllPath = installDirectory + tp.DllPath;
                
                if (File.Exists(dllPath))
                {
                    versionInfo = FileVersionInfo.GetVersionInfo(dllPath);
                }
            }
            catch (Exception ex)
            {
                strBuilder.AppendLine("Cannot Move: " + tp.DllPath + "\n" + ex.Message);
                //MessageBox.Show("Failed to get the version of an installed dll.\n" + tp.DllPath + "\n" + ex.Message, "ToolManager:GetInstalledVersion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return versionInfo;
        }

        private FileVersionInfo GetBetaReleaseVersion(string dllPath)
        {
            FileVersionInfo versionInfo = null;
            try
            {
                if (File.Exists(dllPath))
                {
                    versionInfo = FileVersionInfo.GetVersionInfo(dllPath);
                }
            }
            catch (Exception ex)
            {
                strBuilder.AppendLine("File Not Found: " + dllPath + "\n" + ex.Message);
            }
            return versionInfo;
        }

        private string GetBetaReleaseDate(string dllPath)
        {
            string date = "";
            try
            {
                if (File.Exists(dllPath))
                {
                    FileInfo fileInfo = new FileInfo(dllPath);
                    date = fileInfo.LastWriteTime.Date.ToString("d");
                }
            }
            catch (Exception ex)
            {
                strBuilder.AppendLine("File Not Found: " + dllPath + "\n" + ex.Message);
                //MessageBox.Show("Failed to get the date of a released dll.\n" + filePath + "\n" + ex.Message, "MainForm:GetReleaseDate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return date;
        }

    }

    public class RevitToolProperties
    {
        public Tool2013 ToolEnum2013 { get; set; }
        public Tool2014 ToolEnum2014 { get; set; }
        public Tool2015 ToolEnum2015 { get; set; }
        public Tool2016 ToolEnum2016 { get; set; }
        public string ToolName { get; set; }
        public List<string> FilePaths { get; set; }
        public string DLLName { get; set; }
        public string DllPath { get; set; }
        public FileVersionInfo ReleaseVersionInfo { get; set; }
        //public string ReleaseVersion { get; set; }
        public string ReleaseDate { get; set; }
        public FileVersionInfo InstallVersionInfo { get; set; }
        //public string InstallVersion { get; set; }
        public FileVersionInfo BetaVersionInfo { get; set; }
        //public string BetaVersion { get; set; }
        public string BetaDate { get; set; }
        public bool BetaOnly { get; set; }
        public TargetSoftware TargetSoftWareEnum { get; set; }
        public int ImageIndex { get; set; }
    }

    public class DynamoToolProperties
    {
        public string ToolName { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public FileInfo ReleasedFileInfo { get; set; }
        public FileInfo InstalledFileInfo { get; set; }
        public FileInfo BetaFileInfo { get; set; }
    }

    
}
