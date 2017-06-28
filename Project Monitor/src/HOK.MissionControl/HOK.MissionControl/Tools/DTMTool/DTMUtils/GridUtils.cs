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
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Grid> grids = collector.OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().ToElements().Cast<Grid>().ToList();
                List<ElementId> paramIds = new List<ElementId>();
                if (grids.Count > 0)
                {
                    Grid firstGrid = grids.First();
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

                Dictionary<ElementId, Outline> extents = new Dictionary<ElementId, Outline>();
                foreach (Grid grid in grids)
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
                string message = ex.Message;
                LogUtil.AppendLog("GridUtils-CollectGridExtents:" + ex.Message);
            }
        }

        public static bool ExtentGeometryChanged(string centralPath, ElementId currentGridId, Outline currentOutline)
        {
            bool changed = false;
            try
            {
                if (gridExtents.ContainsKey(centralPath))
                {
                    if (gridExtents[centralPath].ContainsKey(currentGridId))
                    {
                        Outline oldOutline = gridExtents[centralPath][currentGridId];
                        XYZ oldMin = oldOutline.MinimumPoint;
                        XYZ oldMax = oldOutline.MaximumPoint;

                        XYZ curMin = currentOutline.MinimumPoint;
                        XYZ curMax = currentOutline.MaximumPoint;

                        if (!oldMin.IsAlmostEqualTo(curMin) || !oldMax.IsAlmostEqualTo(curMax))
                        {
                            changed = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("GridUtils-ExtentGeometryChanged:" + ex.Message);
            }
            return changed;
        }
    }
}
