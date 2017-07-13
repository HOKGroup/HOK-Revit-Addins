using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using HOK.Core.Utilities;
using HOK.ElementFlatter.Class;

namespace HOK.ElementFlatter.Commands
{
    public static class DirectShapeCreator
    {
        public static DirectShapeLibrary shapeLibrary = null;

        public static DirectShapeInfo CreateDirectShapes(Document doc, CategoryInfo catInfo, ElementId elementId)
        {
            DirectShapeInfo createdShape = null;
            try
            {
                var element = doc.GetElement(elementId);
                if (element != null)
                {
                    createdShape = CreateDirectShape(doc, element);
                    RunCategoryAction(doc, catInfo, element);

                    try
                    {
                        doc.Delete(element.Id);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return createdShape;
        }

        private static DirectShapeInfo CreateDirectShape(Document doc, Element element)
        {
            DirectShapeInfo shapeInfo = null;
            var isFamilyInstance = false;
            try
            {
                if (element.GetType() == typeof(FamilyInstance))
                {
                    isFamilyInstance = true;
                }

                if (element.GroupId != ElementId.InvalidElementId)
                {
                    var group = doc.GetElement(element.GroupId) as Group;
                    group?.UngroupMembers();
                }

                if (isFamilyInstance)
                {
                    var subComponents = ((FamilyInstance)element).GetSubComponentIds();
                    foreach (var subId in subComponents)
                    {
                        var subComponent = doc.GetElement(subId);
                        if (subComponent != null)
                        {
                            CreateDirectShape(doc, subComponent); //TODO: same as above. It's worth creating them, but they are not returned.
                        }
                    }
                }

                var geoOptions = new Options();
                var geoElement = element.get_Geometry(geoOptions);
                if (null != geoElement)
                {
                    try
                    {
                        DirectShape directShape = null;
#if RELEASE2015 || RELEASE2016
                        var shapeTypeId = ElementId.InvalidElementId;
                        var appDataGUID = element.Id.IntegerValue.ToString();
#endif
                        var shapeGeometries = FindElementGeometry(geoElement);
                        
                        if (shapeGeometries.Count > 0)
                        {
#if RELEASE2017 || RELEASE2018
                            directShape = DirectShape.CreateElement(doc, element.Category.Id);
#else
                            directShape = DirectShape.CreateElement(doc, element.Category.Id, appGUID, appDataGUID);
#endif
                            directShape.SetShape(shapeGeometries);
                        }

                        if (null != directShape)
                        {
                            directShape.SetName(element.Name);
#if RELEASE2016 || RELEASE2017 || RELEASE2018
                            var dsOptions = directShape.GetOptions();
                            dsOptions.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                            directShape.SetOptions(dsOptions);
#endif
                            shapeInfo = new DirectShapeInfo(directShape.Id, element.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, "Cannot Set Geometry of DirectShape-" + ex.Message + element.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, "Cannot Create DirectShape-" + ex.Message + element.Id);
            }

            return shapeInfo;
        }

        private static List<GeometryObject> FindElementGeometry(GeometryElement geoElement)
        {
            var geoObjects = new List<GeometryObject>();
            try
            {
                var solidGeometries = geoElement
                    .Where(x => (x.GetType() == typeof(Solid) && ((Solid)x).Volume > 0) || x.GetType() == typeof(Mesh))
                    .ToList();
                geoObjects.AddRange(solidGeometries);

                //var meshGeometries = 
                //var meshGeometries = from geoObj in geoElement where geoObj.GetType() == typeof(Mesh) select geoObj;
                //if (meshGeometries.Count() > 0)
                //{
                //    geoObjects.AddRange(meshGeometries);
                //}

                var geoInstances = geoElement.Where(x => x.GetType() == typeof(GeometryInstance)).Cast<GeometryInstance>().ToList();
                //var geoInstances = from geoObj in geoElement where geoObj.GetType() == typeof(GeometryInstance) select geoObj as GeometryInstance;
                if (geoInstances.Any())
                {
                    foreach (var geoInst in geoInstances)
                    {
                        var geoElem2 = geoInst.GetSymbolGeometry(geoInst.Transform);
                        geoObjects.AddRange(FindElementGeometry(geoElem2));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, "Cannot Find Element Geometry-" + ex.Message);
            }
            return geoObjects;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="catInfo"></param>
        /// <param name="element"></param>
        private static void RunCategoryAction(Document doc, CategoryInfo catInfo, Element element)
        {
            try
            {
                switch (catInfo.BltCategory)
                {
                    case BuiltInCategory.OST_Topography:
                        var surface = (TopographySurface)element;
                        var subRegionIds = surface.GetHostedSubRegionIds();
                        if (subRegionIds?.Count > 0)
                        {
                            foreach (var eId in subRegionIds)
                            {
                                var subRegion = doc.GetElement(eId);
                                if (null != subRegion)
                                {
                                    CreateDirectShape(doc, subRegion);
                                }
                            }
                        }
                        break;
                    case BuiltInCategory.OST_Stairs:
                        if (Stairs.IsByComponent(doc, element.Id))
                        {
                            var stair = (Stairs) element;
                            var railingIds = stair.GetAssociatedRailings();
                            if (railingIds.Count > 0)
                            {
                                foreach (var eId in railingIds)
                                {
                                    var railing = doc.GetElement(eId);
                                    if (null != railing)
                                    {
                                        CreateDirectShape(doc, railing);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var railings = new FilteredElementCollector(doc)
                                .OfClass(typeof(Railing))
                                .WhereElementIsNotElementType()
                                .Cast<Railing>()
                                .Where(x => x.HasHost && x.HostId == element.Id)
                                .ToList();

                            if (railings.Any())
                            {
                                foreach (var railing in railings)
                                {
                                    CreateDirectShape(doc, railing);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message + "-" + element.Id);
            }
        }
    }
}
