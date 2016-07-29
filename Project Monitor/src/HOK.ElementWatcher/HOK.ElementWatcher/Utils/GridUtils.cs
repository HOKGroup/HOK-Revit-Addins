using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementWatcher.Utils
{
    public static class GridUtils
    {
        public static Dictionary<Guid/*docId*/, Dictionary<ElementId, Outline>> gridExtents = new Dictionary<Guid, Dictionary<ElementId, Outline>>();
        public static Dictionary<Guid/*docId*/, List<ElementId>> gridParameters = new Dictionary<Guid, List<ElementId>>();

        public static void CollectGridExtents(Document doc, Guid docId)
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
                if (gridParameters.ContainsKey(docId))
                {
                    gridParameters.Remove(docId);
                }
                gridParameters.Add(docId, paramIds);

                Dictionary<ElementId, Outline> extents = new Dictionary<ElementId, Outline>();
                foreach (Grid grid in grids)
                {
                    if (!extents.ContainsKey(grid.Id))
                    {
                        extents.Add(grid.Id, grid.GetExtents());
                    }
                }
                if (gridExtents.ContainsKey(docId))
                {
                    gridExtents.Remove(docId);
                }
                gridExtents.Add(docId, extents);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static bool ExtentGeometryChanged(Guid docId, ElementId currentGridId, Outline currentOutline)
        {
            bool changed = false;
            try
            {
                if (gridExtents.ContainsKey(docId))
                {
                    if (gridExtents[docId].ContainsKey(currentGridId))
                    {
                        Outline oldOutline = gridExtents[docId][currentGridId];
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
            }
            return changed;
        }
    }
}
