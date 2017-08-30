using System;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class LinkMonitor
    {
        /// <summary>
        /// Publishes information about linked models/images/object styles in the model.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="recordId">Id of the HealthRecord in MongoDB.</param>
        /// <param name="config">Configuration for the model.</param>
        /// <param name="project">Project for the model.</param>
        public static void PublishData(Document doc, string recordId, Configuration config, Project project)
        {
            try
            {
                var dwgStyles = new FilteredElementCollector(doc)
                    .OfClass(typeof(GraphicsStyle))
                    .Cast<GraphicsStyle>()
                    .Where(x => x.GraphicsStyleCategory.Name.Contains(".dwg"))
                    .Select(x => x.GraphicsStyleCategory);

                var totalDwgStyles = dwgStyles.Sum(x => x.SubCategories.Size);

                var familyStyles = new FilteredElementCollector(doc)
                    .OfClass(typeof(GraphicsStyle))
                    .Cast<GraphicsStyle>()
                    .Where(x => x.GraphicsStyleCategory.Name == "Imports in Families")
                    .Select(x => x.GraphicsStyleCategory);
                var totalImportedStyles = familyStyles.Sum(x => x.SubCategories.Size);

                // (Konrad) Collect info about Images
                var allPlacedImageIds = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_RasterImages)
                    .Select(x => x.GetTypeId())
                    .ToList();

                var totalUnusedImages = new FilteredElementCollector(doc)
                    .OfClass(typeof(ImageType))
                    .Excluding(allPlacedImageIds)
                    .GetElementCount();

                // (Konrad) Collect all Linked Model info
                var totalLinkedCad = new FilteredElementCollector(doc)
                    .OfClass(typeof(CADLinkType))
                    .GetElementCount();

                var totalLinkedRvt = new FilteredElementCollector(doc)
                    .OfClass(typeof(RevitLinkType))
                    .GetElementCount();

                var cadLinksDic = new FilteredElementCollector(doc)
                    .OfClass(typeof(CADLinkType))
                    .Select(x => new DwgFileInfo { name = x.Name, elementId = x.Id.IntegerValue })
                    .ToDictionary(key => key.elementId, value => value);

                var totalImportInstance = 0;
                foreach (var ii in new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)))
                {
                    totalImportInstance++;
                    var id = ii.GetTypeId().IntegerValue;
                    if (!cadLinksDic.ContainsKey(id)) continue;

                    if (cadLinksDic[id].instances == 0)
                    {
                        cadLinksDic[id].isViewSpecific = ii.ViewSpecific;
                        cadLinksDic[id].isLinked = ((ImportInstance)ii).IsLinked;
                    }
                    cadLinksDic[id].instances = cadLinksDic[id].instances + 1;
                }

                var linkStats = new LinkStat
                {
                    totalImportedDwg = totalImportInstance,
                    importedDwgFiles = cadLinksDic.Values.ToList(),
                    unusedLinkedImages = totalUnusedImages,
                    totalDwgStyles = totalDwgStyles,
                    totalImportedStyles = totalImportedStyles,
                    totalLinkedModels = totalLinkedCad + totalLinkedRvt,
                    totalLinkedDwg = totalLinkedCad
                };

                ServerUtilities.PostToMongoDB(linkStats, "healthrecords", recordId, "linkstats");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}