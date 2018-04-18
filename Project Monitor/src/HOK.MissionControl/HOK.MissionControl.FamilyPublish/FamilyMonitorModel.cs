using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.FamilyPublish.Properties;

namespace HOK.MissionControl.FamilyPublish
{
    public class FamilyMonitorModel
    {
        public static Guid UpdaterGuid { get; set; } = new Guid(Resources.HealthReportTrackerGuid);
        public static Document Doc { get; set; }
        public Configuration Config { get; set; }
        public Project Project { get; set; }
        private string RecordId { get; }
        private string CentralPath { get; }

        public FamilyMonitorModel(Document doc, Configuration config, Project project, string recordId, string centralPath)
        {
            Doc = doc;
            Config = config;
            Project = project;
            RecordId = recordId;
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
                if (string.IsNullOrEmpty(RecordId)) return;

                var unusedFamilies = 0;
                var oversizedFamilies = 0;
                var inPlaceFamilies = 0;
                var families = new FilteredElementCollector(Doc)
                    .OfClass(typeof(Family))
                    .Cast<Family>()
                    .ToList();

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
                    if (!family.IsEditable) continue;

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

                    try
                    {
                        long size;
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

                        var refPlanes = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(ReferencePlane))
                            .GetElementCount();

                        var filter = new LogicalAndFilter(new List<ElementFilter>
                        {
                            new ElementClassFilter(typeof(LinearArray)),
                            new ElementClassFilter(typeof(RadialArray))
                        });

                        var arrays = new FilteredElementCollector(famDoc)
                            .WherePasses(filter)
                            .GetElementCount();

                        var voids = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(Extrusion))
                            .Cast<Extrusion>()
                            .Count(x => !x.IsSolid);

                        var nestedFamilies = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(Family))
                            .GetElementCount();
#if RELEASE2015
                        //(Konrad) Since Revit 2015 API doesn't support this we will just skip it.
                        const int parameters = 0; 
#else
                        var parameters = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(ParameterElement))
                            .GetElementCount();
#endif
                        famDoc.Close(false);

                        var sizeStr = StringUtilities.BytesToString(size);
                        if (size > 1000000)
                        {
                            oversizedFamilies++; // >1MB
                            sizeCheck = true;
                        }
                        //TODO: This should use the settings from mongoDB
                        if (!family.Name.Contains("_HOK_I") && !family.Name.Contains("_HOK_M")) nameCheck = true;

                        var famItem = new FamilyItem
                        {
                            name = family.Name,
                            elementId = family.Id.IntegerValue,
                            size = sizeStr,
                            sizeValue = size,
                            instances = instances,
                            arrayCount = arrays,
                            refPlaneCount = refPlanes,
                            voidCount = voids,
                            nestedFamilyCount = nestedFamilies,
                            parametersCount = parameters,
                            tasks = new List<FamilyTask>()
                        };
                        
                        if (nameCheck || sizeCheck || instanceCheck) famItem.isFailingChecks = true;
                        else famItem.isFailingChecks = false;

                        famOutput.Add(famItem);
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                    }
                }

                var famStat = ServerUtilities.GetByCentralPath<FamilyData>(CentralPath, "families/centralpath");
                var famDict = new Dictionary<string, FamilyItem>();
                if (famStat != null)
                {
                    famDict = famStat.families.ToDictionary(x => x.name, x => x);
                }

                // (Konrad) I know it's not efficient to iterate this list yet again, but
                // I want to minimize the amount of time between publishing families and
                // retrieving them from database to avoid someone addind tasks to DB while
                // we are exporting and hence losing them after the export.
                foreach (var family in famOutput)
                {
                    if (famDict.ContainsKey(family.name))
                    {
                        family.tasks.AddRange(famDict[family.name].tasks);
                        famDict.Remove(family.name);
                    }
                }

                foreach (var item in famDict.Values.ToList())
                {
                    item.isDeleted = true;
                    famOutput.Add(item);
                }

                var familyStats = new FamilyData
                {
                    centralPath = CentralPath.ToLower(),
                    totalFamilies = famOutput.Count,
                    unusedFamilies = unusedFamilies,
                    inPlaceFamilies = inPlaceFamilies,
                    oversizedFamilies = oversizedFamilies,
                    createdBy = Environment.UserName.ToLower(),
                    createdOn = DateTime.UtcNow,
                    families = famOutput
                };

                if (famStat == null)
                {
                    famStat = ServerUtilities.Post<FamilyData>(familyStats, "families");
                    ServerUtilities.Put(new {key = famStat.Id}, "healthrecords/" + RecordId + "/addfamilies");
                }
                else
                {
                    ServerUtilities.Put(familyStats, "families/" + famStat.Id);
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