using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Tools.DTMTool.DTMUtils
{
    public static class GridUtils
    {
        public static Dictionary<string/*centralPath*/, Dictionary<ElementId, Outline>> gridExtents = new Dictionary<string, Dictionary<ElementId, Outline>>();
        public static Dictionary<string/*centralPath*/, List<ElementId>> gridParameters = new Dictionary<string, List<ElementId>>();

        public static void CollectGridExtents(Document doc, string centralPath)
        {
            try
            {
                var collector = new FilteredElementCollector(doc);
                var grids = collector.OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().ToElements().Cast<Grid>().ToList();
                var paramIds = new List<ElementId>();
                if (grids.Count > 0)
                {
                    var firstGrid = grids.First();
                    foreach (Parameter param in firstGrid.Parameters)
                    {
                        paramIds.Add(param.Id);
                    }
                }
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
                var message = ex.Message;
                LogUtil.AppendLog("GridUtils-CollectGridExtents:" + ex.Message);
            }
        }

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
                var message = ex.Message;
                LogUtil.AppendLog("GridUtils-ExtentGeometryChanged:" + ex.Message);
            }
            return changed;
        }
    }
}
