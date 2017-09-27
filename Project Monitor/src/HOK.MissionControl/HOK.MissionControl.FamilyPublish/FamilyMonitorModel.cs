using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.Core.Schemas;
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

        ///// <summary>
        ///// Publishes information about linked models/images/object styles in the model.
        ///// </summary>
        //public void PublishData()
        //{
        //    try
        //    {
        //        if (!MonitorUtilities.IsUpdaterOn(Project, Config, UpdaterGuid)) return;
        //        if (string.IsNullOrEmpty(RecordId)) return;

        //        var suspectFamilies = new List<FamilyItem>();
        //        var totalFamilies = 0;
        //        var unusedFamilies = 0;
        //        var oversizedFamilies = 0;
        //        var inPlaceFamilies = 0;
        //        var families = new FilteredElementCollector(Doc).OfClass(typeof(Family)).Cast<Family>().ToList();

        //        StatusBarManager.InitializeProgress("Exporting Family Info:", families.Count);
        //        foreach (var family in families)
        //        {
        //            StatusBarManager.StepForward();

        //            var sizeCheck = false;
        //            var instanceCheck = false;
        //            var nameCheck = false;

        //            totalFamilies++;
        //            var instances = CountFamilyInstances(family);
        //            if (instances == 0)
        //            {
        //                unusedFamilies++;
        //                instanceCheck = true;
        //            }

        //            if (family.IsInPlace) inPlaceFamilies++;
        //            if (!family.IsEditable) continue;

        //            try
        //            {
        //                long size;
        //                var famDoc = Doc.EditFamily(family);

        //                var refPlanes = new FilteredElementCollector(famDoc)
        //                    .OfClass(typeof(ReferencePlane))
        //                    .GetElementCount();

        //                var filter = new LogicalAndFilter(new List<ElementFilter>
        //                {
        //                    new ElementClassFilter(typeof(LinearArray)),
        //                    new ElementClassFilter(typeof(RadialArray))
        //                });
        //                var arrays = new FilteredElementCollector(famDoc)
        //                    .WherePasses(filter)
        //                    .GetElementCount();

        //                var voids = new FilteredElementCollector(famDoc)
        //                    .OfClass(typeof(Extrusion))
        //                    .Cast<Extrusion>()
        //                    .Count(x => !x.IsSolid);

        //                var nestedFamilies = new FilteredElementCollector(famDoc)
        //                    .OfClass(typeof(Family))
        //                    .GetElementCount();

        //                var parameters = new FilteredElementCollector(famDoc)
        //                    .OfClass(typeof(ParameterElement))
        //                    .GetElementCount();

        //                var storedPath = famDoc.PathName;
        //                if (File.Exists(storedPath))
        //                {
        //                    size = new FileInfo(storedPath).Length;
        //                }
        //                else
        //                {
        //                    var myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //                    var path = myDocPath + "\\temp_" + famDoc.Title;
        //                    famDoc.SaveAs(path);

        //                    size = new FileInfo(path).Length;
        //                    TryToDelete(path);
        //                }
        //                famDoc.Close(false);

        //                var sizeStr = BytesToString(size);
        //                if (size > 1000000)
        //                {
        //                    oversizedFamilies++; // >1MB
        //                    sizeCheck = true;
        //                }

        //                if (!family.Name.Contains("_HOK_I") && !family.Name.Contains("_HOK_M")) nameCheck = true;

        //                // (Konrad) We only want to export families that don't have proper name, are oversized or unplaced.
        //                if (nameCheck || sizeCheck || instanceCheck)
        //                {
        //                    var famItem = new FamilyItem
        //                    {
        //                        name = family.Name,
        //                        elementId = family.Id.IntegerValue,
        //                        size = sizeStr,
        //                        sizeValue = size,
        //                        instances = instances,

        //                        arrayCount = arrays,
        //                        refPlaneCount = refPlanes,
        //                        voidCount = voids,
        //                        nestedFamilyCount = nestedFamilies,
        //                        parametersCount = parameters
        //                    };

        //                    suspectFamilies.Add(famItem);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
        //            }
        //        }

        //        var familyStats = new FamilyStat
        //        {
        //            suspectFamilies = suspectFamilies,
        //            totalFamilies = totalFamilies,
        //            unusedFamilies = unusedFamilies,
        //            inPlaceFamilies = inPlaceFamilies,
        //            oversizedFamilies = oversizedFamilies,
        //            createdBy = Environment.UserName
        //        };

        //        ServerUtilities.PostToMongoDB(familyStats, "healthrecords", RecordId, "familystats");
        //        StatusBarManager.FinalizeProgress();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
        //    }
        //}

        /// <summary>
        /// Publishes information about linked models/images/object styles in the model.
        /// </summary>
        public void PublishData()
        {
            try
            {
                if (!MonitorUtilities.IsUpdaterOn(Project, Config, UpdaterGuid)) return;
                if (string.IsNullOrEmpty(RecordId)) return;

                var famData = ServerUtilities.GetDataByCentralPath<FamilyStat>(CentralPath, "families");
                FamilyStat famStat = null;
                if (famData.Any())
                {
                    foreach (var fam in famData)
                    {
                        if (!string.Equals(fam.centralPath.ToLower(), CentralPath.ToLower())) continue;
                        famStat = fam;
                        break;
                    }
                }

                var famDict = new Dictionary<string, FamilyItem>();
                var famOutput = new List<FamilyItem>();
                if (famStat != null)
                {
                    famDict = famStat.families.ToDictionary(x => x.name, x => x);
                }

                var totalFamilies = 0;
                var unusedFamilies = 0;
                var oversizedFamilies = 0;
                var inPlaceFamilies = 0;
                var families = new FilteredElementCollector(Doc)
                    .OfClass(typeof(Family))
                    .Cast<Family>()
                    .ToList();

                StatusBarManager.InitializeProgress("Exporting Family Info:", families.Count);
                foreach (var family in families)
                {
                    StatusBarManager.StepForward();
                    if (!family.IsEditable) continue;

                    var sizeCheck = false;
                    var instanceCheck = false;
                    var nameCheck = false;

                    totalFamilies++;
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

                        var parameters = new FilteredElementCollector(famDoc)
                            .OfClass(typeof(ParameterElement))
                            .GetElementCount();

                        var storedPath = famDoc.PathName;
                        if (File.Exists(storedPath))
                        {
                            size = new FileInfo(storedPath).Length;
                        }
                        else
                        {
                            var myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            var path = myDocPath + "\\temp_" + famDoc.Title;
                            famDoc.SaveAs(path);

                            size = new FileInfo(path).Length;
                            TryToDelete(path);
                        }
                        famDoc.Close(false);

                        var sizeStr = StringUtilities.BytesToString(size);
                        if (size > 1000000)
                        {
                            oversizedFamilies++; // >1MB
                            sizeCheck = true;
                        }
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

                        // (Konrad) We need to check if new family is already stored in MongoDB.
                        // If it was stored, get its tasks list.
                        // If it was not stored, add it to list.
                        // If it no longer exists mark it as deleted.
                        if (famDict.ContainsKey(famItem.name))
                        {
                            famItem.tasks.AddRange(famDict[famItem.name].tasks);
                            famOutput.Add(famItem);
                            famDict.Remove(famItem.name);
                        }
                        else
                        {
                            famOutput.Add(famItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                    }
                }

                foreach (var item in famDict.Values.ToList())
                {
                    item.isDeleted = true;
                    famOutput.Add(item);
                }

                var familyStats = new FamilyStat
                {
                    centralPath = CentralPath,
                    totalFamilies = totalFamilies,
                    unusedFamilies = unusedFamilies,
                    inPlaceFamilies = inPlaceFamilies,
                    oversizedFamilies = oversizedFamilies,
                    createdBy = Environment.UserName.ToLower(),
                    families = famOutput
                };

                if (famStat == null)
                {
                    famStat = ServerUtilities.Post<FamilyStat>(familyStats, "families");
                    ServerUtilities.UpdateField(new {key = famStat.Id}, "healthrecords/" + RecordId + "/addfamilies");
                }
                else
                {
                    ServerUtilities.UpdateField(familyStats, "families/" + famStat.Id);
                }

                StatusBarManager.FinalizeProgress();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
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