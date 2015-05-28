using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;

namespace HOK.ViewAnalysis
{
    public static class BuildingEnvelopUtil
    {
        public static List<LinkElementId> FindExteriorWallsByAnalyzer(Document doc, View3D view, Room room)
        {
            List<LinkElementId> exteriorElementIds = new List<LinkElementId>();
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Analyze Building Envelope");
                try
                {
                    BuildingEnvelopeAnalyzerOptions options = new BuildingEnvelopeAnalyzerOptions();
                    options.AnalyzeEnclosedSpaceVolumes = false;
                    options.GridCellSize = FindFloorHeight(doc, room, view);
                    options.OptimizeGridCellSize = false;
                    BuildingEnvelopeAnalyzer analyzer = BuildingEnvelopeAnalyzer.Create(doc, options);
                    if (null != analyzer)
                    {
                        IList<LinkElementId> linkIds = analyzer.GetBoundingElements();

                        if (linkIds.Count > 0)
                        {
                            exteriorElementIds.AddRange(linkIds);
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    MessageBox.Show("Bounding elements cannot be defined by Revit API.\nPlease manually set exterior walls and check LEED_IsExteriorWall parameter.", "Building Envelope", MessageBoxButton.OK, MessageBoxImage.Warning);
                    trans.RollBack();
                }
            }
            return exteriorElementIds;
        }

        public static bool SetExteriorWallParameter(Document doc, List<LinkElementId> elementIds)
        {
            bool result = false;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Set Exterior Walls");
                try
                {
                    foreach (LinkElementId linkId in elementIds)
                    {
                        if (linkId.HostElementId != ElementId.InvalidElementId)
                        {
                            Element wall = doc.GetElement(linkId.HostElementId);
                            if (null != wall)
                            {
                                Parameter param = wall.LookupParameter(LEEDParameters.LEED_IsExteriorWall.ToString());
                                if (null != param)
                                {
                                    if (!param.IsReadOnly)
                                    {
                                        param.Set(1);
                                    }
                                }
                            }
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to set parameter value.\n"+ex.Message, "Set Exterior Walls", MessageBoxButton.OK, MessageBoxImage.Warning);
                    trans.RollBack();
                }
            }
            return result;
        }

        private static double FindFloorHeight(Document doc, Room room, View3D view)
        {
            double floorHeight = 10;
            try
            {
                LocationPoint locationPoint = room.Location as LocationPoint;
                XYZ roomPoint = locationPoint.Point;

                List<ElementFilter> filters = new List<ElementFilter>();
                filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_Roofs));
                filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_Floors));
                filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_RvtLinks));
                LogicalOrFilter orFilter = new LogicalOrFilter(filters);

                ReferenceIntersector intersector = new ReferenceIntersector(orFilter, FindReferenceTarget.Element, view);
                intersector.FindReferencesInRevitLinks = true;
                IList<ReferenceWithContext> contexts = intersector.Find(roomPoint, new XYZ(0, 0, 1));
                if (null != contexts)
                {
                    //floorHeight = 8.0;
                    foreach (ReferenceWithContext context in contexts)
                    {
                        if(context.Proximity>0)
                        {
                            Reference reference = context.GetReference();
                            Element element = null;

                            if (reference.LinkedElementId != ElementId.InvalidElementId)
                            {
                                RevitLinkInstance instance = doc.GetElement(reference.ElementId) as RevitLinkInstance;
                                if (null != instance)
                                {
                                    Document linkedDoc = instance.GetLinkDocument();
                                    if (null != linkedDoc)
                                    {
                                        element = linkedDoc.GetElement(reference.LinkedElementId);
                                    }
                                }
                            }
                            else if (reference.ElementId != ElementId.InvalidElementId)
                            {
                                element = doc.GetElement(reference.ElementId);
                            }

                            if (null != element)
                            {
                                if (null != element.Category)
                                {
                                    if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Floors || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Roofs)
                                    {
                                        floorHeight = context.Proximity; break;
                                    }
                                }
                            }        
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find the floor height.\n" + ex.Message, "Find Floor Height", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            return floorHeight;
        }

        public static List<LinkElementId> FindExteriorWallsByParameter(Document doc, Dictionary<int, LinkedInstanceData> linkedInstances)
        {
            List<LinkElementId> exteriorElementIds = new List<LinkElementId>();
            try
            {
                List<LinkElementId> fromHostDoc = FindExteriorWallsByParameterValues(doc, true, ElementId.InvalidElementId);
                if (fromHostDoc.Count > 0)
                {
                    exteriorElementIds.AddRange(fromHostDoc);
                }

                foreach (LinkedInstanceData instanceData in linkedInstances.Values)
                {
                    List<LinkElementId> fromLinkDoc = FindExteriorWallsByParameterValues(instanceData.LinkedDocument, false, instanceData.Instance.Id);
                    if (fromLinkDoc.Count > 0)
                    {
                        exteriorElementIds.AddRange(fromLinkDoc);
                    }
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find exterior walls.\n" + ex.Message, "Find Exterior Walls by Parameter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return exteriorElementIds;
        }

        public static List<LinkElementId> FindExteriorWallsByParameterValues(Document doc, bool isHost, ElementId linkInstanceId)
        {
            List<LinkElementId> exteriorElementIds = new List<LinkElementId>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                Wall wall = collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements().Cast<Wall>().First();
                Parameter param = wall.LookupParameter(LEEDParameters.LEED_IsExteriorWall.ToString());
                ElementId paramId = ElementId.InvalidElementId;
                if (null != param)
                {
                    paramId = param.Id;
                }

                ParameterValueProvider pvp = new ParameterValueProvider(paramId);
                FilterNumericEquals fnrv = new FilterNumericEquals();
                FilterIntegerRule paramFr = new FilterIntegerRule(pvp, fnrv, 1);
                ElementParameterFilter paramFilter = new ElementParameterFilter(paramFr);

                collector = new FilteredElementCollector(doc);
                List<ElementId> eIds = collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().WherePasses(paramFilter).ToElementIds().ToList();
                foreach (ElementId eId in eIds)
                {
                    if (isHost)
                    {
                        LinkElementId linkId = new LinkElementId(eId);
                        exteriorElementIds.Add(linkId);
                    }
                    else
                    {
                        LinkElementId linkId = new LinkElementId(linkInstanceId, eId);
                        exteriorElementIds.Add(linkId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find exterior walls.\n" + ex.Message, "Find Exterior Walls by Parameter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return exteriorElementIds;
        }

    }
}
