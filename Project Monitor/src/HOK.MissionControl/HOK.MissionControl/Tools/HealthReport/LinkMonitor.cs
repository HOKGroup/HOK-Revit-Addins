using System;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Links;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class LinkMonitor
    {
        /// <summary>
        /// Publishes information about linked models/images/object styles in the model.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="linksId">Id of the Links Document in MongoDB.</param>
        public void PublishData(Document doc, string linksId)
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
                    .Select(x => new DwgFileInfo { Name = x.Name, ElementId = x.Id.IntegerValue })
                    .ToDictionary(key => key.ElementId, value => value);

                var totalImportInstance = 0;
                foreach (var ii in new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)))
                {
                    totalImportInstance++;
                    var id = ii.GetTypeId().IntegerValue;
                    if (!cadLinksDic.ContainsKey(id)) continue;

                    if (cadLinksDic[id].Instances == 0)
                    {
                        cadLinksDic[id].IsViewSpecific = ii.ViewSpecific;
                        cadLinksDic[id].IsLinked = ((ImportInstance)ii).IsLinked;
                    }
                    cadLinksDic[id].Instances = cadLinksDic[id].Instances + 1;
                }

                var linkStats = new LinkDataItem
                {
                    TotalImportedDwg = totalImportInstance,
                    ImportedDwgFiles = cadLinksDic.Values.ToList(),
                    UnusedLinkedImages = totalUnusedImages,
                    TotalDwgStyles = totalDwgStyles,
                    TotalImportedStyles = totalImportedStyles,
                    TotalLinkedModels = totalLinkedCad + totalLinkedRvt,
                    TotalLinkedDwg = totalLinkedCad
                };

                if (ServerUtilities.Post(linkStats, "links/" + linksId + "/linkstats", 
                    out LinkDataItem unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Links Data.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}