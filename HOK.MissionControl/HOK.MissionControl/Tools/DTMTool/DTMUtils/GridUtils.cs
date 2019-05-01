using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Tools.DTMTool.DTMUtils
{
    public static class GridUtils
    {
        public static Dictionary<string/*centralPath*/, Dictionary<ElementId, Outline>> gridExtents = new Dictionary<string, Dictionary<ElementId, Outline>>();
        public static Dictionary<string/*centralPath*/, IEnumerable<ElementId>> gridParameters = new Dictionary<string, IEnumerable<ElementId>>();

        /// <summary>
        /// Updates all of the Grid Extents objects.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="centralPath">Document Central File Path.</param>
        public static void CollectGridExtents(Document doc, string centralPath)
        {
            try
            {
                var grids = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Grids)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Grid>()
                    .ToList();

                // (Konrad) We are only interested in watching editable parameters. There is no need to watch them all.
                IEnumerable<ElementId> paramIds;
                if (grids.Any()) paramIds = from Parameter param 
                                            in grids.First().Parameters
                                            where !param.IsReadOnly
                                            select param.Id;
                else return;

                if (gridParameters.ContainsKey(centralPath))
                {
                    gridParameters.Remove(centralPath);
                }
                gridParameters.Add(centralPath, paramIds);

                var extents = new Dictionary<ElementId, Outline>();
                foreach (var grid in grids)
                {
                    if (!extents.ContainsKey(grid.Id))
                    {
                        extents.Add(grid.Id, grid.GetExtents());
                    }
                }

                if (gridExtents.ContainsKey(centralPath))
                {
                    gridExtents.Remove(centralPath);
                }
                gridExtents.Add(centralPath, extents);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Checks if Grid Extent has changed.
        /// </summary>
        /// <param name="centralPath">Document central file path.</param>
        /// <param name="currentGridId">Current Grid Id.</param>
        /// <param name="currentOutline">Current Outline.</param>
        /// <returns></returns>
        public static bool ExtentGeometryChanged(string centralPath, ElementId currentGridId, Outline currentOutline)
        {
            var changed = false;
            try
            {
                if (gridExtents.ContainsKey(centralPath))
                {
                    if (gridExtents[centralPath].ContainsKey(currentGridId))
                    {
                        var oldOutline = gridExtents[centralPath][currentGridId];
                        var oldMin = oldOutline.MinimumPoint;
                        var oldMax = oldOutline.MaximumPoint;

                        var curMin = currentOutline.MinimumPoint;
                        var curMax = currentOutline.MaximumPoint;

                        if (!oldMin.IsAlmostEqualTo(curMin) || !oldMax.IsAlmostEqualTo(curMax))
                        {
                            changed = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return changed;
        }
    }
}
