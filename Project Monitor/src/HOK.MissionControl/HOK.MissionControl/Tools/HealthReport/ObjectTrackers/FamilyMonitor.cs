using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
using System.IO;
using HOK.MissionControl.Properties;

namespace HOK.MissionControl.Tools.HealthReport.ObjectTrackers
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
                if (!MonitorUntilities.IsUpdaterOn(project, config, UpdaterGuid)) return;
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
                    totalFamilies++;
                    var instances = CountFamilyInstances(family);
                    if (instances == 0) unusedFamilies++;
                    if(family.FamilyCategory.Name == "Mass") inPlaceFamilies++ ;


                    if (!family.IsEditable) continue;

                    try
                    {
                        var famDoc = doc.EditFamily(family);
                        var myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var path = myDocPath + "\\temp_" + famDoc.Title;
                        famDoc.SaveAs(path);

                        var size = new FileInfo(path).Length;
                        var sizeStr = BytesToString(size);
                        if (size > 1000000) oversizedFamilies++; // >1MB

                        var famItem = new FamilyItem
                        {
                            name = family.Name,
                            elementId = family.Id.IntegerValue,
                            size = sizeStr,
                            sizeValue = size,
                            instances = instances
                        };

                        suspectFamilies.Add(famItem);
                        famDoc.Close(false);
                        TryToDelete(path);
                    }
                    catch
                    {
                        // ignored
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

                ServerUtil.PostStats(familyStats, worksetDocumentId, "familystats");

                AppCommand.RunExport = false;

                var dialogResult = MessageBox.Show("Thank you for exporting!", "Mission Control", MessageBoxButtons.OK);
                if (dialogResult == DialogResult.Yes)
                {
                    ThreadPool.QueueUserWorkItem(CloseDocument);
                }
            }
            catch (Exception e)
            {
                LogUtil.AppendLog("LinkMonitor-PublishData: " + e.Message);
            }
        }

        /// <summary>
        /// Since Revit doesn't allow for closing of ActiveDocument we need to mimic it with "CTRL + F4"
        /// </summary>
        /// <param name="stateInfo"></param>
        private static void CloseDocument(object stateInfo)
        {
            try
            {
                SendKeys.SendWait("^{F4}");
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Converts byte size to human readable size ex. kB, MB etc.
        /// </summary>
        /// <param name="byteCount"></param>
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
        /// <param name="path"></param>
        private static void TryToDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // ignored
            }
        }

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
                    var famSymbol = (FamilySymbol) f.Document.GetElement(id);
                    var filter = new FamilyInstanceFilter(Doc, famSymbol.Id);
                    count = count + new FilteredElementCollector(Doc).WherePasses(filter).GetElementCount();
                }
            }
            return count;
        }
    }
}
