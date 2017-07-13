using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.FamilyPublish.Properties;

namespace HOK.MissionControl.FamilyPublish
{
    public class FamilyMonitor
    {
        public static Guid UpdaterGuid { get; set; } = new Guid(Resources.HealthReportTrackerGuid);
        public static Document Doc { get; set; }

        /// <summary>
        /// Publishes information about linked models/images/object styles in the model.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="config">Configuration for the model.</param>
        /// <param name="project">Project for the model.</param>
        public static void PublishData(Document doc, Configuration config, Project project)
        {
            try
            {
                if (!MonitorUtilities.IsUpdaterOn(project, config, UpdaterGuid)) return;
                var worksetDocumentId = project.worksets.FirstOrDefault();
                if (string.IsNullOrEmpty(worksetDocumentId)) return;

                Doc = doc;

                var suspectFamilies = new List<FamilyItem>();
                var totalFamilies = 0;
                var unusedFamilies = 0;
                var oversizedFamilies = 0;
                var inPlaceFamilies = 0;
                foreach (var family in new FilteredElementCollector(doc).OfClass(typeof(Family)).Cast<Family>())
                {
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

                    if (family.FamilyCategory.Name == "Mass") inPlaceFamilies++;
                    if (!family.IsEditable) continue;

                    try
                    {
                        var famDoc = doc.EditFamily(family);
                        var myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var path = myDocPath + "\\temp_" + famDoc.Title;
                        famDoc.SaveAs(path);

                        var size = new FileInfo(path).Length;
                        var sizeStr = BytesToString(size);
                        if (size > 1000000)
                        {
                            oversizedFamilies++; // >1MB
                            sizeCheck = true;
                        }

                        if (!family.Name.Contains("_HOK_I") && !family.Name.Contains("_HOK_M")) nameCheck = true;

                        // (Konrad) We only want to export families that don't have proper name, are oversized or unplaced.
                        if (nameCheck || sizeCheck || instanceCheck)
                        {
                            var famItem = new FamilyItem
                            {
                                name = family.Name,
                                elementId = family.Id.IntegerValue,
                                size = sizeStr,
                                sizeValue = size,
                                instances = instances
                            };

                            suspectFamilies.Add(famItem);
                        }
                        famDoc.Close(false);
                        TryToDelete(path);
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                    }
                }

                var familyStats = new FamilyStat
                {
                    suspectFamilies = suspectFamilies,
                    totalFamilies = totalFamilies,
                    unusedFamilies = unusedFamilies,
                    inPlaceFamilies = inPlaceFamilies,
                    oversizedFamilies = oversizedFamilies,
                    createdBy = Environment.UserName
                };

                ServerUtilities.PostToMongoDB(familyStats, "worksets", worksetDocumentId, "familystats");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Converts byte size to human readable size ex. kB, MB etc.
        /// </summary>
        /// <param name="byteCount">Size of the file in bytes.</param>
        /// <returns></returns>
        private static string BytesToString(long byteCount)
        {
            string[] suf = { "b", "kb", "mb", "gb", "tb", "pb", "eb" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return Math.Sign(byteCount) * num + suf[place];
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

            if (famSymbolIds.Count == 0)
            {
                count = 0;
            }
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