using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using HOK.ElementFlatter.Class;
using HOK.ElementFlatter.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementFlatter.Commands
{
    public static class DirectShapeCreator
    {
        private const string appGUID = "Flatten";
        public static DirectShapeLibrary shapeLibrary = null;

        public static DirectShapeInfo CreateDirectShapes(Document doc, CategoryInfo catInfo, ElementId elementId)
        {
            DirectShapeInfo createdShape = null;
            try
            {
                Element element = doc.GetElement(elementId);
                if (null != element)
                {
                    DirectShapeInfo shapeInfo = CreateDirectShape(doc, element);
                    if (null != shapeInfo)
                    {
                        createdShape = shapeInfo;

                        List<FamilyInstance> hostedElements = FindHostedElements(doc, element);
                        List<DirectShapeInfo> hostedShapes = new List<DirectShapeInfo>();
                        if (hostedElements.Count > 0)
                        {
                            for (int i = hostedElements.Count - 1; i > -1; i--)
                            {
                                FamilyInstance instance = hostedElements[i];
                                DirectShapeInfo info = CreateDirectShape(doc, instance);
                                if (null != info)
                                {
                                    hostedShapes.Add(info);
                                }
                            }
                        }

                        if (hostedShapes.Count > 0)
                        {
                            var familyIds = from ds in hostedShapes select ds.OriginId;
                        }

                        RunCategoryAction(doc, catInfo, element);

                        doc.Delete(element.Id);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                List<ElementId> elementIds = new List<ElementId>();
                elementIds.Add(elementId);
                LogMessageInfo messageInfo = new LogMessageInfo(LogMessageType.EXCEPTION, ex.Message, DateTime.Now, elementIds);
                LogManager.AppendLog(messageInfo);
            }
            return createdShape;
        }

        private static DirectShapeInfo CreateDirectShape(Document doc, Element element)
        {
            DirectShapeInfo shapeInfo = null;
            bool isFamilyInstance = false;
            try
            {
                if (element.GetType() == typeof(FamilyInstance)) { isFamilyInstance = true; }

                if (element.GroupId != ElementId.InvalidElementId)
                {
                    Group group = doc.GetElement(element.GroupId) as Group;
                    if (null != group)
                    {
                        ICollection<ElementId> members = group.UngroupMembers();
                    }
                }

                if (isFamilyInstance)
                {
                    ICollection<ElementId> subComponents = (element as FamilyInstance).GetSubComponentIds();
                    foreach (ElementId subId in subComponents)
                    {
                        Element subComponent = doc.GetElement(subId);
                        if (null != subComponent)
                        {
                            DirectShapeInfo subDirectShapeInfo = CreateDirectShape(doc, subComponent);
                        }
                    }
                }

                Options geoOptions = new Options();
                GeometryElement geoElement = element.get_Geometry(geoOptions);
                if (null != geoElement)
                {
                    try
                    {
                        DirectShape directShape = null;
                        ElementId shapeTypeId = ElementId.InvalidElementId;

                        string appDataGUID = element.Id.IntegerValue.ToString();
                        List<GeometryObject> shapeGeometries = FindElementGeometry(geoElement);
                        
                        if (shapeGeometries.Count > 0)
                        {
                            directShape = DirectShape.CreateElement(doc, element.Category.Id, appGUID, appDataGUID);
                            //shapeLibrary.AddDefinition(element.UniqueId, shapeGeometries);
                            directShape.SetShape(shapeGeometries);
                        }

                        if (null != directShape)
                        {
                            directShape.SetName(element.Name);
#if RELEASE2016
                            DirectShapeOptions dsOptions = directShape.GetOptions();
                            dsOptions.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                            directShape.SetOptions(dsOptions);
#endif

                            shapeInfo = new DirectShapeInfo(directShape.Id, element.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        List<ElementId> elementIds = new List<ElementId>();
                        elementIds.Add(element.Id);
                        LogMessageInfo messageInfo = new LogMessageInfo(LogMessageType.EXCEPTION, "Cannot Set Geometry of DirectShape.\n" + ex.Message, DateTime.Now, elementIds);
                        LogManager.AppendLog(messageInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                List<ElementId> elementIds = new List<ElementId>();
                elementIds.Add(element.Id);
                LogMessageInfo messageInfo = new LogMessageInfo(LogMessageType.EXCEPTION, "Cannot Create DirectShape.\n" + ex.Message, DateTime.Now, elementIds);
                LogManager.AppendLog(messageInfo);
            }

            return shapeInfo;
        }

        private static List<FamilyInstance> FindHostedElements(Document doc, Element element)
        {
            List<FamilyInstance> hostedElements = new List<FamilyInstance>();
            try
            {
                BoundingBoxXYZ bb = element.get_BoundingBox(null);
                if (null != bb)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    BoundingBoxIntersectsFilter intersectFilter = new BoundingBoxIntersectsFilter(new Outline(bb.Min, bb.Max));
                    BoundingBoxIsInsideFilter insideFilter = new BoundingBoxIsInsideFilter(new Outline(bb.Min, bb.Max));
                    LogicalOrFilter orFilter = new LogicalOrFilter(intersectFilter, insideFilter);
                    List<FamilyInstance> elements = collector.OfClass(typeof(FamilyInstance)).WherePasses(orFilter).ToElements().Cast<FamilyInstance>().ToList();
                    var elementFound = from instance in elements where null != instance.Host && instance.Host.Id == element.Id select instance;
                    if (elementFound.Count() > 0)
                    {
                        hostedElements = elementFound.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                List<ElementId> elementIds = new List<ElementId>();
                elementIds.Add(element.Id);
                LogMessageInfo messageInfo = new LogMessageInfo(LogMessageType.EXCEPTION, "Cannot Find Hosted Elements.\n" + ex.Message, DateTime.Now, elementIds);
                LogManager.AppendLog(messageInfo);
            }
            return hostedElements;
        }

        private static List<GeometryObject> FindElementGeometry(GeometryElement geoElement)
        {
            List<GeometryObject> geoObjects = new List<GeometryObject>();
            try
            {
                var solidGeometries = from geoObj in geoElement 
                                      where geoObj.GetType() == typeof(Solid) && (geoObj as Solid).Volume != 0
                                      select geoObj;
                if (solidGeometries.Count() > 0)
                {
                    geoObjects.AddRange(solidGeometries);
                }
                var meshGeometries = from geoObj in geoElement where geoObj.GetType() == typeof(Mesh) select geoObj;
                if (meshGeometries.Count() > 0)
                {
                    geoObjects.AddRange(meshGeometries);
                }

                var geoInstances = from geoObj in geoElement where geoObj.GetType() == typeof(GeometryInstance) select geoObj as GeometryInstance;
                if (geoInstances.Count() > 0)
                {
                    foreach (GeometryInstance geoInst in geoInstances)
                    {
                        GeometryElement geoElem2 = geoInst.GetSymbolGeometry(geoInst.Transform);
                        geoObjects.AddRange(FindElementGeometry(geoElem2));
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.AppendLog(LogMessageType.EXCEPTION, "Cannot Find Element Geometry. " + ex.Message);
                string message = ex.Message;
            }
            return geoObjects;
        }

        private static void RunCategoryAction(Document doc, CategoryInfo catInfo, Element element)
        {
            try
            {
                switch (catInfo.BltCategory)
                {
                    case BuiltInCategory.OST_Topography:
                        TopographySurface surface = element as TopographySurface;
                        if (null != surface)
                        {
                            IList<ElementId> subRegionIds = surface.GetHostedSubRegionIds();
                            if (subRegionIds.Count > 0)
                            {
                                foreach (ElementId eId in subRegionIds)
                                {
                                    Element subRegion = doc.GetElement(eId);
                                    if (null != subRegion)
                                    {
                                        DirectShapeInfo shapeInfo = CreateDirectShape(doc, subRegion);
                                    }
                                }
                            }
                        }
                        break;
                    case BuiltInCategory.OST_Stairs:
                        if (Stairs.IsByComponent(doc, element.Id))
                        {
                            Stairs stair = element as Stairs;
                            ICollection<ElementId> railingIds = stair.GetAssociatedRailings();
                            if (railingIds.Count > 0)
                            {
                                foreach (ElementId eId in railingIds)
                                {
                                    Element railing = doc.GetElement(eId);
                                    if (null != railing)
                                    {
                                        DirectShapeInfo shpaeInfo = CreateDirectShape(doc, railing);
                                    }
                                }
                            }
                        }
                        else
                        {
                            FilteredElementCollector collector = new FilteredElementCollector(doc); 
                            List<Railing> railings = collector.OfClass(typeof(Railing)).WhereElementIsNotElementType().ToElements().Cast<Railing>().ToList();
                            var associatedRailings = from railing in railings where railing.HasHost == true && railing.HostId == element.Id select railing;
                            if (associatedRailings.Count() > 0)
                            {
                                foreach (Element railing in associatedRailings)
                                {
                                    DirectShapeInfo shapeInfo = CreateDirectShape(doc, railing);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                List<ElementId> elementIds = new List<ElementId>();
                elementIds.Add(element.Id);
                LogMessageInfo messageInfo = new LogMessageInfo(LogMessageType.EXCEPTION, catInfo.Name + " Cannot Run Category-specific Action.\n" + ex.Message, DateTime.Now, elementIds);
                LogManager.AppendLog(messageInfo);
            }
        }
    }
}
