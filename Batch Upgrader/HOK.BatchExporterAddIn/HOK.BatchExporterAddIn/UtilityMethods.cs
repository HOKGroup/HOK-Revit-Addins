using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;
using System.IO;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, All = "HKEY_CURRENT_USER")]

namespace HOK.BatchExporterAddIn
{
    public enum RevitUtility
    {
        None=0, CreateNewCentral, UpgradeVersion, CheckStandards
    }

    public class UtilityMethods
    {
        private UIApplication uiApp;

        private ProjectSettings projectSettings =new ProjectSettings();

        public UtilityMethods(UIApplication uiApplication, ProjectSettings ps)
        {
            uiApp = uiApplication;
            projectSettings = ps;
        }

        public void Upgrade()
        {
            bool logCreated = false;
            try
            {
                LogFileManager.SelectedProject = projectSettings;
                //LogFileManager.LogDirectory = projectSettings.UpgradeOptions.UpgradeVersionSaveAsOptions.LogLocation;
                logCreated = LogFileManager.CreateLogFile();
                LogFileManager.ClearLogFile();
                LogFileManager.AppendLog(DateTime.Now.ToString() + ": Started Batch Upgrader for " + projectSettings.ProjectName);

                Stopwatch stopwatch = new Stopwatch();
                foreach (FileItem fileItem in projectSettings.FileItems)
                {
                    string revitFile = fileItem.RevitFile;
                    string outputFolder = fileItem.OutputFolder;

                    if (File.Exists(revitFile))
                    {
                        stopwatch.Reset();
                        stopwatch.Start();
                        Document doc = null;
                        LogFileManager.AppendLog("");
                        doc = OpenRevitDocument(revitFile);
                        bool upgraded = false;
                        if (null != doc)
                        {
                            upgraded = UpgradeRevitProject(doc, revitFile);
                            bool closedDoc = doc.Close(false);
                        }
                        stopwatch.Stop();
                        TimeSpan ts = stopwatch.Elapsed;
                        string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                        if (upgraded) { LogFileManager.AppendLog(DateTime.Now.ToString() + ": Successfully Upgraded to the Version "+projectSettings.UpgradeOptions.UpgradeVersion +" Run time - " + elapsedTime); }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogFileManager.AppendLog("[Error] Failed to initialize Upgrade", message);
            }
            finally
            {
                if (logCreated)
                {
                    LogFileManager.AppendLog(DateTime.Now.ToString() + ": End Batch Processor");
                    LogFileManager.WriteLogFile();
                }
            }
        }

        public Document OpenRevitDocument(string revitFileName)
        {
            Document doc = null;
            try
            {
                LogFileManager.AppendLog("Open Revit Document: " + revitFileName);
                BasicFileInfo basicFileInfo = BasicFileInfo.Extract(revitFileName);
                FileOpenOptions fileOpenOptions=projectSettings.UpgradeOptions.UpgradeVersionOpenOptions;

                if (basicFileInfo.IsWorkshared)
                {
                    ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(revitFileName);
                    OpenOptions openOptions = new OpenOptions();
                    openOptions.Audit = fileOpenOptions.Audit;
                    if (fileOpenOptions.DetachAndPreserveWorksets)
                    {
                        openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
                    }

                    IList<WorksetPreview> wsPreviews = new List<WorksetPreview>();
                    IList<WorksetId> worksetIds = new List<WorksetId>();
                    WorksetConfiguration wsConfiguration = new WorksetConfiguration();

                    try
                    {
                        wsPreviews = WorksharingUtils.GetUserWorksetInfo(modelPath);
                        if (wsPreviews.Count > 0)
                        {
                            foreach (WorksetPreview wsPreview in wsPreviews)
                            {
                                worksetIds.Add(wsPreview.Id);
                            }

                            if (fileOpenOptions.OpenAllWorkset)
                            {
                                wsConfiguration.Open(worksetIds);
                            }
                            else
                            {
                                wsConfiguration.Close(worksetIds); 
                            }
                            openOptions.SetOpenWorksetsConfiguration(wsConfiguration);
                        }
                    }
                    catch(Exception ex)
                    {
                        LogFileManager.AppendLog("[Warning] Open Worksets", ex.Message);
                    }
                    doc = uiApp.Application.OpenDocumentFile(modelPath, openOptions);
                }
                else
                {
                    doc = uiApp.Application.OpenDocumentFile(revitFileName);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogFileManager.AppendLog("[Error] Open Revit Document", message);
            }
            return doc;
        }
        
        public bool UpgradeRevitProject(Document document, string revitFileName)
        {
            bool upgraded = false;
            try
            {
                BasicFileInfo basicFileInfo = BasicFileInfo.Extract(revitFileName);
                FileSaveAsOptions fileSaveAsOptions = projectSettings.UpgradeOptions.UpgradeVersionSaveAsOptions;
                
                LogFileManager.AppendLog("Upgrade Revit Project: " + revitFileName);
                LogFileManager.AppendLog("The Original Revit file was saved in " + basicFileInfo.SavedInVersion);

                SaveAsOptions saveAsOptions = new SaveAsOptions();
                saveAsOptions.OverwriteExistingFile = true;

                if (basicFileInfo.IsWorkshared)
                {
                    WorksharingSaveAsOptions worksharingSaveAsOptions = new WorksharingSaveAsOptions();
                    worksharingSaveAsOptions.OpenWorksetsDefault = FindWorksetOption(fileSaveAsOptions.WorksetConfiguration);
                    worksharingSaveAsOptions.SaveAsCentral = fileSaveAsOptions.MakeCentral;

                    saveAsOptions.MaximumBackups = fileSaveAsOptions.NumOfBackups;
                    saveAsOptions.SetWorksharingOptions(worksharingSaveAsOptions);
                }

                bool isFinalUpgrade = projectSettings.UpgradeOptions.IsFinalUpgrade;
                if (isFinalUpgrade)
                {
                    string backupDirectory = FindBackupDirectory(revitFileName);
                    if (!string.IsNullOrEmpty(backupDirectory))
                    {
                        string fileName = Path.GetFileName(revitFileName);
                        string newFilePath = Path.Combine(backupDirectory, fileName);
                        File.Copy(revitFileName, newFilePath, true);
                        if (File.Exists(newFilePath))
                        {
                            document.SaveAs(revitFileName, saveAsOptions);
                            LogFileManager.AppendLog("Backup Saved: " + newFilePath);
                            if (fileSaveAsOptions.Relinquish)
                            {
                                RelinquishOptions roptions = new RelinquishOptions(false);
                                roptions.UserWorksets = true;
                                TransactWithCentralOptions coptions = new TransactWithCentralOptions();
                                WorksharingUtils.RelinquishOwnership(document, roptions, coptions);
                                LogFileManager.AppendLog("Relinquish all worksets created by the current user.");
                            }
                            upgraded = true;
                        }
                    }
                    else
                    {
                        LogFileManager.AppendLog("File Not Saved", "The backup directory cannot be found.");
                        upgraded = false;
                    }
                }
                else
                {
                    string reviewDirectory = FindReviewDirectory(revitFileName);
                    if (string.IsNullOrEmpty(reviewDirectory)) { reviewDirectory = fileSaveAsOptions.ReviewLocation; }
                    string fileName = Path.GetFileName(revitFileName);
                    if (!string.IsNullOrEmpty(reviewDirectory))
                    {
                        revitFileName = Path.Combine(reviewDirectory, fileName);
                        document.SaveAs(revitFileName, saveAsOptions);
                        LogFileManager.AppendLog("File Saved: " + revitFileName);
                        if (fileSaveAsOptions.Relinquish)
                        {
                            RelinquishOptions roptions = new RelinquishOptions(false);
                            roptions.UserWorksets = true;
                            TransactWithCentralOptions coptions = new TransactWithCentralOptions();
                            WorksharingUtils.RelinquishOwnership(document, roptions, coptions);
                            LogFileManager.AppendLog("Relinquish all worksets created by the current user.");
                        }
                        upgraded = true;
                    }
                    else
                    {
                        LogFileManager.AppendLog("File Not Saved", "The review directory cannot be found.");
                        upgraded = false;
                    }
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogFileManager.AppendLog("[Error] Upgrade Revit Project", message);
            }
            return upgraded;
        }

        private SimpleWorksetConfiguration FindWorksetOption(string worksetConfig)
        {
            try
            {
                switch (worksetConfig)
                {
                    case "AllEditable":
                        return SimpleWorksetConfiguration.AllEditable;
                    case "AllWorksets":
                        return SimpleWorksetConfiguration.AllWorksets;
                    case "AskUserToSpecify":
                        return SimpleWorksetConfiguration.AskUserToSpecify;
                    case "LastViewed":
                        return SimpleWorksetConfiguration.LastViewed;
                    default:
                        return SimpleWorksetConfiguration.AskUserToSpecify;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return SimpleWorksetConfiguration.AskUserToSpecify;
            }
        }

        

        private string FindDiscipline(string revitFileName)
        {
            string discipline = "";
            if (revitFileName.Contains("Revit-AC"))
            {
                discipline = "Revit-AC";
            }
            else if (revitFileName.Contains("Revit-ME"))
            {
                discipline = "Revit-ME";
            }
            else if (revitFileName.Contains("Revit-ST"))
            {
                discipline = "Revit-ST";
            }
            else if (revitFileName.Contains("REVIT-AC"))
            {
                discipline = "REVIT-AC"; //2010 version
            }
            else if (revitFileName.Contains("REVIT-ME"))
            {
                discipline = "REVIT-ME"; //2010 version
            }
            else if (revitFileName.Contains("REVIT-ST"))
            {
                discipline = "REVIT-ST"; //2010 version
            }

            return discipline;
        }

        public bool MakeCopyOfRevitProject(string revitFileName)
        {
            bool result = false;
            try
            {
                //check folder hierarchy   
                string fileName = Path.GetFileName(revitFileName);
                string curDirectory = Path.GetDirectoryName(revitFileName);
                DirectoryInfo softwareDirectory = Directory.GetParent(curDirectory);
                if (!string.IsNullOrEmpty(curDirectory) && null != softwareDirectory)
                {
                    if (softwareDirectory.Name == "Software")
                    {
                        DirectoryInfo ebimDirectory = Directory.GetParent(softwareDirectory.FullName);
                        if (ebimDirectory.Name == "E-BIM")
                        {
                            string reviewDirectory = ebimDirectory.FullName + "\\Review";
                            string backupDirectory = reviewDirectory + "\\Backup";
                            if (Directory.Exists(reviewDirectory))
                            {
                                if (!Directory.Exists(backupDirectory))
                                {
                                    Directory.CreateDirectory(backupDirectory);
                                }
                                string newFilePath = Path.Combine(backupDirectory, fileName);
                                File.Copy(revitFileName, newFilePath, true);
                                result = true;
                            }
                        }
                    }
                }

                if (!result)
                {
                    string backupDirectory = curDirectory + "\\Backup";
                    if (!Directory.Exists(backupDirectory))
                    {
                        Directory.CreateDirectory(backupDirectory);
                    }
                    string newfilePath = Path.Combine(backupDirectory, fileName);
                    File.Copy(revitFileName, newfilePath, true);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogFileManager.AppendLog("[Error] Make a Copy of Revit Project", message);
            }
            return result;
        }
        
        private string FindBackupDirectory(string revitFileName)
        {
            /*E-BIM\Software\Revit-AC*/
            /*E-BIM\Review\Upgrade Test*/
            string backupDirectoryName = "";
            try
            {
                if (revitFileName.ToUpper().Contains("E-BIM") && revitFileName.ToUpper().Contains("REVIT"))
                {
                    string curDirectory = Path.GetDirectoryName(revitFileName);
                    if (!curDirectory.ToUpper().Contains("REVIT"))
                    {
                        DirectoryInfo revitDirectory = Directory.GetParent(curDirectory);
                        while (!revitDirectory.Name.ToUpper().Contains("REVIT"))
                        {
                            revitDirectory = Directory.GetParent(revitDirectory.FullName);
                        }
                        curDirectory=revitDirectory.FullName;
                    }

                    string backupPath = Path.Combine(curDirectory, "Backup");
                    if (!Directory.Exists(backupPath))
                    {
                        Directory.CreateDirectory(backupPath);
                    }
                    string datePath = Path.Combine(backupPath, DateTime.Today.Date.ToString("yyyy-MM-dd"));
                    if (!Directory.Exists(datePath))
                    {
                        Directory.CreateDirectory(datePath);
                    }

                    backupDirectoryName = datePath;
                }
                else
                {
                    //LogFileManager.AppendLog("This file is not located in HOK standard folder structure.");
                    string curDirectory = Path.GetDirectoryName(revitFileName);
                    string backupPath = Path.Combine(curDirectory, "Backup");
                    if (!Directory.Exists(backupPath))
                    {
                        Directory.CreateDirectory(backupPath);
                    }
                    string datePath = Path.Combine(backupPath, DateTime.Today.Date.ToString("yyyy-MM-dd"));
                    if (!Directory.Exists(datePath))
                    {
                        Directory.CreateDirectory(datePath);
                    }

                    backupDirectoryName = datePath;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogFileManager.AppendLog("[Error] Find Review Directory", message);
            }
            return backupDirectoryName;
        }

        private string FindReviewDirectory(string revitFileName)
        {
            /*E-BIM\Software\Revit-AC*/
            /*E-BIM\Review\Upgrade Test*/
            string reviewDirectoryName = "";
            try
            {
                string curDirectory = Path.GetDirectoryName(revitFileName);
                if (revitFileName.ToUpper().Contains("E-BIM") && revitFileName.ToUpper().Contains("REVIT"))
                {
                    DirectoryInfo reviewDirectory = null;
                    if (revitFileName.Contains("Software")) //upper version than 2011
                    {
                        DirectoryInfo softwareDirectory = Directory.GetParent(curDirectory);
                        while (softwareDirectory.Name != "Software")
                        {
                            softwareDirectory = Directory.GetParent(softwareDirectory.FullName);
                        }

                        DirectoryInfo bimDirectory = Directory.GetParent(softwareDirectory.FullName);
                       
                        foreach (DirectoryInfo directoryInfo in bimDirectory.GetDirectories())
                        {
                            if (directoryInfo.Name == "Review")
                            {
                                reviewDirectory = directoryInfo;
                                break;
                            }
                        }
                    }
                    else //2010
                    {
                        DirectoryInfo bimDirectory = Directory.GetParent(curDirectory);
                        while (bimDirectory.Name != "E-BIM")
                        {
                            bimDirectory = Directory.GetParent(bimDirectory.FullName);
                        }
                        foreach (DirectoryInfo directoryInfo in bimDirectory.GetDirectories())
                        {
                            if (directoryInfo.Name == "Review")
                            {
                                reviewDirectory = directoryInfo;
                                break;
                            }
                        }

                        if (null == reviewDirectory)
                        {
                            string createdReviewFolder = Path.Combine(bimDirectory.FullName, "Review");
                            if (!Directory.Exists(createdReviewFolder))
                            {
                                reviewDirectory = Directory.CreateDirectory(createdReviewFolder);
                            }
                        }
                    }
                    
                    if (null != reviewDirectory)
                    {
                        string upgradeTest = Path.Combine(reviewDirectory.FullName, "UpgradeTest");
                        if (!Directory.Exists(upgradeTest))
                        {
                            DirectoryInfo upgradeTestDir = Directory.CreateDirectory(upgradeTest);
                        }
                        string discipline = FindDiscipline(revitFileName);
                        string revitdirectory = Path.Combine(upgradeTest, discipline);
                        if (!Directory.Exists(revitdirectory))
                        {
                            DirectoryInfo revitDirectoryInfo = Directory.CreateDirectory(revitdirectory);
                        }
                        if (Directory.Exists(revitdirectory))
                        {
                            reviewDirectoryName = revitdirectory;
                        }
                    }
                }
                else
                {
                    LogFileManager.AppendLog("This file is not located in HOK standard folder structure.");
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogFileManager.AppendLog("[Error] Find Review Directory", message);
            }
            return reviewDirectoryName;
        }


        public bool CheckStandards()
        {
            bool result = false;
            try
            {
                LogFileManager.AppendLog("Check Standards: ");
                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogFileManager.AppendLog("[Error] Check Standards", message);
                return false;
            }
            return result;
        }
    }
}
