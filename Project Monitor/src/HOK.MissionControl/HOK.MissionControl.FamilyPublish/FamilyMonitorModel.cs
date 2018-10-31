#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Configurations;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.FamilyPublish.Properties;
#endregion

namespace HOK.MissionControl.FamilyPublish
{
    public class FamilyMonitorModel
    {
        public static Guid UpdaterGuid { get; set; } = new Guid(Properties.Resources.HealthReportTrackerGuid);
        public static Document Doc { get; set; }
        public Configuration Config { get; set; }
        public Project Project { get; set; }
        private string FamiliesId { get; }
        private string CentralPath { get; }

        public FamilyMonitorModel(Document doc, Configuration config, Project project, string familiesId, string centralPath)
        {
            Doc = doc;
            Config = config;
            Project = project;
            FamiliesId = familiesId;
            CentralPath = centralPath;
        }

        /// <summary>
        /// Publishes information about linked models/images/object styles in the model.
        /// </summary>
        public void PublishData()
        {
            try
            {
                if (!MonitorUtilities.IsUpdaterOn(Project, Config, UpdaterGuid)) return;
                if (string.IsNullOrEmpty(FamiliesId)) return;

                var unusedFamilies = 0;
                var oversizedFamilies = 0;
                var inPlaceFamilies = 0;
                var families = new FilteredElementCollector(Doc)
                    .OfClass(typeof(Family))
                    .Cast<Family>()
                    .ToList();

                var config = MissionControlSetup.Configurations.ContainsKey(CentralPath)
                    ? MissionControlSetup.Configurations[CentralPath]
                    : null;

                var familyNameCheck = new List<string> { "HOK_I", "HOK_M" }; //defaults
                if (config != null)
                {
                    familyNameCheck = config.Updaters.First(x => string.Equals(x.UpdaterId,
                        Properties.Resources.HealthReportTrackerGuid, StringComparison.OrdinalIgnoreCase)).UserOverrides.FamilyNameCheck.Values;
                }

                //var count = 0;
                var famOutput = new List<FamilyItem>();
                StatusBarManager.InitializeProgress("Exporting Family Info:", families.Count);
                foreach (var family in families)
                {
                    // (Konrad) Uncomment for Debug. 
                    //count++;
                    //if (count > 5) continue;
                    if (StatusBarManager.Cancel)
                    {
                        StatusBarManager.CancelProgress();
                        return;
                    }
                    StatusBarManager.StepForward();

                    var sizeCheck = false;
                    var instanceCheck = false;
                    var nameCheck = false;

                    var instances = CountFamilyInstances(family);
                    if (instances == 0)
                    {
                        unusedFamilies++;
                        instanceCheck = true;
                    }
                    if (family.IsInPlace) inPlaceFamilies++;

                    long size = 0;
                    var refPlanes = 0;
                    var arrays = 0;
                    var voids = 0;
                    var nestedFamilies = 0;
                    var parameters = 0;
                    var sizeStr = "0Kb";
                    var images = 0;
                    try
                    {
                        var famDoc = Doc.EditFamily(family);

                        var storedPath = famDoc.PathName;
                        if (File.Exists(storedPath))
                        {
                            size = new FileInfo(storedPath).Length;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(famDoc.Title)) continue; // could cause an exception

                            var myDocPath = IsCitrixMachine(Environment.MachineName) 
                                ? "B:\\Temp" 
                                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            
                            var path = myDocPath + "\\temp_" + famDoc.Title;
                            if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1) continue; // could cause an exception

                            if (File.Exists(path)) TryToDelete(path);
                            famDoc.SaveAs(path);
                            size = new FileInfo(path).Length;
                            TryToDelete(path);
                        }

                        refPlanes = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(ReferencePlane))
                            .GetElementCount();

                        var filter = new LogicalOrFilter(new List<ElementFilter>
                        {
                            new ElementClassFilter(typeof(LinearArray)),
                            new ElementClassFilter(typeof(RadialArray))
                        });

                        arrays = new FilteredElementCollector(famDoc)
                            .WherePasses(filter)
                            .GetElementCount();

                        images = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(ImageType))
                            .WhereElementIsElementType()
                            .GetElementCount();

                        voids = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(Extrusion))
                            .Cast<Extrusion>()
                            .Count(x => !x.IsSolid);

                        nestedFamilies = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(Family))
                            .GetElementCount();
#if RELEASE2015
                        //(Konrad) Since Revit 2015 API doesn't support this we will just skip it.
                        parameters = 0; 
#else
                        parameters = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(ParameterElement))
                            .GetElementCount();
#endif
                        famDoc.Close(false);

                        sizeStr = StringUtilities.BytesToString(size);
                        if (size > 1000000)
                        {
                            oversizedFamilies++; // >1MB
                            sizeCheck = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                        Log.AppendLog(LogMessageType.ERROR, "Failed to retrieve size, refPlanes, arrays, voids...");
                    }

                    if (!familyNameCheck.Any(family.Name.Contains)) nameCheck = true;

                    var famItem = new FamilyItem
                    {
                        Name = family.Name,
                        ElementId = family.Id.IntegerValue,
                        Size = sizeStr,
                        SizeValue = size,
                        Instances = instances,
                        ArrayCount = arrays,
                        RefPlaneCount = refPlanes,
                        VoidCount = voids,
                        NestedFamilyCount = nestedFamilies,
                        ParametersCount = parameters,
                        ImageCount = images,
                        Tasks = new List<FamilyTask>()
                    };

                    if (nameCheck || sizeCheck || instanceCheck) famItem.IsFailingChecks = true;
                    else famItem.IsFailingChecks = false;

                    famOutput.Add(famItem);
                }

                if (!ServerUtilities.GetByCentralPath(CentralPath, "families/centralpath", out FamilyData famStat))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to retrieve Families data.");
                    return;
                }
                
                var famDict = new Dictionary<string, FamilyItem>();
                if (famStat != null)
                {
                    famDict = famStat.Families.ToDictionary(x => x.Name, x => x);
                }

                // (Konrad) I know it's not efficient to iterate this list yet again, but
                // I want to minimize the amount of time between publishing families and
                // retrieving them from database to avoid someone addind tasks to DB while
                // we are exporting and hence losing them after the export.
                foreach (var family in famOutput)
                {
                    if (famDict.ContainsKey(family.Name))
                    {
                        family.Tasks.AddRange(famDict[family.Name].Tasks);
                        famDict.Remove(family.Name);
                    }
                }

                foreach (var item in famDict.Values.ToList())
                {
                    item.IsDeleted = true;
                    famOutput.Add(item);
                }

                var familyStats = new FamilyData
                {
                    CentralPath = CentralPath.ToLower(),
                    TotalFamilies = famOutput.Count,
                    UnusedFamilies = unusedFamilies,
                    InPlaceFamilies = inPlaceFamilies,
                    OversizedFamilies = oversizedFamilies,
                    CreatedBy = Environment.UserName.ToLower(),
                    CreatedOn = DateTime.UtcNow,
                    Families = famOutput
                };

                if (!ServerUtilities.Put(familyStats, "families/" + famStat.Id))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Families data.");
                }
                StatusBarManager.FinalizeProgress();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Checks if user machine is Citrix server or desktop/laptop.
        /// </summary>
        /// <param name="machineName">Environment.MachineName</param>
        /// <returns>True if machine is a Citrix server or false if it's not.</returns>
        private static bool IsCitrixMachine(string machineName)
        {
            // (Konrad) All Citrix servers sit on a Physical Hardware the name will contain/end with SVR. 
            // Virtual server would end with VS. 
            return machineName.Substring(machineName.Length - 3).ToLower() == "svr";
        }

        /// <summary>
        /// Deletes created temp file.
        /// </summary>
        /// <param name="path">File to be deleted.</param>
        private static void TryToDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Returns number of placed Family Instances of given Family.
        /// </summary>
        /// <param name="f">Family.</param>
        /// <returns></returns>
        private static int CountFamilyInstances(Family f)
        {
            var count = 0;
            var famSymbolIds = f.GetFamilySymbolIds();

            if (famSymbolIds.Count == 0) count = 0;
            else
            {
                foreach (var id in famSymbolIds)
                {
                    var famSymbol = (FamilySymbol)f.Document.GetElement(id);
                    var filter = new FamilyInstanceFilter(Doc, famSymbol.Id);
                    count = count + new FilteredElementCollector(Doc).WherePasses(filter).GetElementCount();
                }
            }
            return count;
        }
    }
}