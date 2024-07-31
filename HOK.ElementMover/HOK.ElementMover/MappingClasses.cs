using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.ElementMover
{
    public class LinkedInstanceProperties
    {
        private readonly BuiltInCategory[] customCategories =
        {
            BuiltInCategory.OST_Rooms,
            BuiltInCategory.OST_Levels,
            BuiltInCategory.OST_Grids,
            BuiltInCategory.OST_VolumeOfInterest
        };

        public RevitLinkInstance Instance { get; set; }
        public ElementId InstanceId { get; set; }
        public string InstanceName { get; set; } = "";
        public Transform TransformValue { get; set; }
        public Document LinkedDocument { get; set; }
        public string LinkDocTitle { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public Dictionary<ElementId, CategoryProperties> Categories { get; set; } = new Dictionary<ElementId, CategoryProperties>();
        public Dictionary<ElementId, LinkedElementInfo> LinkedElements { get; set; } = new Dictionary<ElementId, LinkedElementInfo>();
        public Dictionary<ElementId, LinkedFamilyInfo> LinkedFamilies { get; set; } = new Dictionary<ElementId, LinkedFamilyInfo>();

        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            Instance = instance;
            InstanceId = Instance.Id;

            CollectLinkInstanceInfo();
            if (LinkedDocument != null)
            {
                CollectCategories(); //model element + Rooms, Levels, Grids
                CollectElementMaps();
                CollectFamilyMaps();
            }
        }

        /// <summary>
        /// Collects basic info about the link instance (name/transform)
        /// </summary>
        private void CollectLinkInstanceInfo()
        {
            try
            {
                var param = Instance.get_Parameter(BuiltInParameter.RVT_LINK_INSTANCE_NAME);
                if (param != null) InstanceName = param.AsString();
                if (Instance.GetTotalTransform() != null) TransformValue = Instance.GetTotalTransform();

                // (Konrad) If link instance is unloaded, the DOC will be null.
                LinkedDocument = Instance.GetLinkDocument();
                if (LinkedDocument != null) LinkDocTitle = LinkedDocument.Title;

                if (!string.IsNullOrEmpty(InstanceName) && !string.IsNullOrEmpty(LinkDocTitle))
                {
                    DisplayName = LinkDocTitle + " - " + InstanceName;
                }
                else
                {
                    DisplayName = "Unloaded Link - " + InstanceName;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CollectCategories()
        {
            try
            {
                //Categories that have material quantities
                var collector = new FilteredElementCollector(LinkedDocument);
                var elements = collector.WhereElementIsNotElementType().ToElements().ToList();
                var elementCategories = from element in elements where null != element.Category select element.Category;
                var modelCategories = from category in elementCategories where category.HasMaterialQuantities && category.CategoryType == CategoryType.Model select category;

                var categoryNames = from category in modelCategories select category.Name;
                var categoryNameList = categoryNames.Distinct().ToList();

                var categoryObjects = LinkedDocument.Settings.Categories;
                foreach (var catName in categoryNameList)
                {
                    if (!categoryObjects.Contains(catName)) { continue; }

                    var category = categoryObjects.get_Item(catName);
                    if (null != category)
                    {
                        var catProperties = new CategoryProperties(category);
                        var categoryFound = from modelCat in modelCategories where modelCat.Id == category.Id select modelCat;
                        var itemCount = categoryFound.Count();
                        if (itemCount > 0)
                        {
                            catProperties.ItemCount = itemCount;
                            if (!Categories.ContainsKey(catProperties.CategoryId))
                            {
                                Categories.Add(catProperties.CategoryId, catProperties);
                            }
                        }
                    }
                }

                //Categories that belongs to MEP curves
                collector = new FilteredElementCollector(LinkedDocument);
                var mepCurves = collector.OfClass(typeof(MEPCurve)).ToElements().ToList();
                if (mepCurves.Count > 0)
                {
                    var mepCategories = from mepCurve in mepCurves select mepCurve.Category.Name;
                    var mepCategoryNameList = mepCategories.Distinct().ToList();
                    foreach (var catName in mepCategoryNameList)
                    {
                        if (!categoryObjects.Contains(catName)) { continue; }

                        var category = categoryObjects.get_Item(catName);
                        if (null != category)
                        {
                            var catProperties = new CategoryProperties(category);
                            var categoryFound = from mepCurve in mepCurves where mepCurve.Category.Id == category.Id select mepCurve;
                            var itemCount = categoryFound.Count();
                            if (itemCount > 0)
                            {
                                catProperties.ItemCount = itemCount;
                                if (!Categories.ContainsKey(catProperties.CategoryId))
                                {
                                    Categories.Add(catProperties.CategoryId, catProperties);
                                }
                            }
                        }
                    }
                }

                foreach (var bltCategory in customCategories)
                {
                    var catPriority = 0;
                    if (bltCategory == BuiltInCategory.OST_Rooms) { catPriority = 2; }

                    collector = new FilteredElementCollector(LinkedDocument);
                    var customCatElements = collector.OfCategory(bltCategory).ToElementIds();
                    if (customCatElements.Count > 0)
                    {
                        var category = categoryObjects.get_Item(bltCategory);
                        if (null != category)
                        {
                            var catProperties = new CategoryProperties(category);
                            catProperties.Priority = catPriority;
                            catProperties.ItemCount = customCatElements.Count;
                            if (!Categories.ContainsKey(catProperties.CategoryId))
                            {
                                Categories.Add(catProperties.CategoryId, catProperties);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }

        private void CollectElementMaps()
        {
            try
            {
                var hostDoc = Instance.Document;
                var categoryIds = Categories.Keys.ToList();
                foreach (var catId in categoryIds)
                {
                    var collector = new FilteredElementCollector(hostDoc);
                    var elements = collector.OfCategoryId(catId).WhereElementIsNotElementType().ToElements().ToList();
                    var elementWithEntities = from element in elements where element.GetEntitySchemaGuids().Count > 0 select element;
                    if (elementWithEntities.Count() > 0)
                    {
                        elements = elementWithEntities.ToList();
                    }

                    foreach (var element in elements)
                    {
                        if (null == element.Location) { continue; } // unplaced rooms

                        var linkInfo = MoverDataStorageUtil.GetLinkedElementInfo(element);
                        if (null != linkInfo)
                        {
                            if (linkInfo.SourceLinkInstanceId != InstanceId) { continue; }
                            if (element.Id != linkInfo.LinkedElementId) { continue; }

                            var sourceElement = LinkedDocument.GetElement(linkInfo.SourceElementId);
                            if (null != sourceElement)
                            {
                                linkInfo = new LinkedElementInfo(linkInfo.LinkElementType, sourceElement, element, InstanceId, TransformValue);
                                if (!LinkedElements.ContainsKey(linkInfo.LinkedElementId))
                                {
                                    LinkedElements.Add(linkInfo.LinkedElementId, linkInfo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect element maps.\n" + ex.Message, "Collect Element Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CollectFamilyMaps()
        {
            try
            {
                var hostDoc = Instance.Document;
                var categoryIds = Categories.Keys.ToList();
                foreach (var catId in categoryIds)
                {
                    var collector = new FilteredElementCollector(hostDoc);
                    var elements = collector.OfCategoryId(catId).WhereElementIsElementType().ToElements().ToList();
                    foreach (var element in elements)
                    {
                        var elementType = element as ElementType;
                        if (null != elementType)
                        {
                            var familyInfo = MoverDataStorageUtil.GetLinkedFamilyInfo(elementType);
                            if (null != familyInfo)
                            {
                                if (familyInfo.SourceLinkInstanceId != InstanceId) { continue; }
                                if (element.Id != familyInfo.TargetTypeId) { continue; }

                                var sourceType = LinkedDocument.GetElement(familyInfo.SourceTypeId) as ElementType;
                                if (null != sourceType)
                                {
                                    var sourceTypeInfo = new ElementTypeInfo(sourceType);
                                    var targetTypeInfo = new ElementTypeInfo(elementType);

                                    familyInfo = new LinkedFamilyInfo(familyInfo.SourceLinkInstanceId, sourceTypeInfo, targetTypeInfo);
                                    if (!LinkedFamilies.ContainsKey(familyInfo.TargetTypeId))
                                    {
                                        LinkedFamilies.Add(familyInfo.TargetTypeId, familyInfo);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect family maps.\n" + ex.Message, "Collect Family Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class CategoryProperties
    {
        //priority 0: Levels, Grids, Scope Boxes
        //priority 1: Floors, Walls
        //priority 2: Ceilings, Roofs, Stairs
        //priority 4: Family Instances
        //priority 5: Rooms

        public bool Selected { get; set; } = false;
        public ElementId CategoryId { get; set; } = ElementId.InvalidElementId;
        public string CategoryName { get; set; } = "";
        public int ItemCount { get; set; } = 0;
        public int Priority { get; set; } = 1;

        public CategoryProperties(Category category)
        {
            CategoryId = category.Id;
            CategoryName = category.Name;
        }
    }

    public enum LinkType
    {
        ByCopy, ByMap, None
    }

    public class LinkedElementInfo
    {
        //For Tree View

        private long[] customCategories = new long[] { (long)BuiltInCategory.OST_Rooms, (long)BuiltInCategory.OST_Levels, (long)BuiltInCategory.OST_Grids, (long)BuiltInCategory.OST_VolumeOfInterest };

        public LinkType LinkElementType { get; set; } = LinkType.None;
        public ElementId SourceElementId { get; set; } = ElementId.InvalidElementId;
        public string SourceUniqueId { get; set; } = "";
        public ElementId LinkedElementId { get; set; } = ElementId.InvalidElementId;
        public string LinkedUniqueId { get; set; } = "";
        public ElementId SourceLinkInstanceId { get; set; } = ElementId.InvalidElementId;

        //For Tree View
        public string CategoryName { get; set; } = "";

        public string FamilyName { get; set; } = "";
        public string FamilyTypeName { get; set; } = "";
        public string LinkDisplayText { get; set; } = "";
        public string ToolTipText { get; set; } = "";
        public bool Matched { get; set; } = false;

        public LinkedElementInfo()
        {
        }

        public LinkedElementInfo(LinkType linkType, Element sourceElement, Element linkedElement, ElementId linkInstanceId, Transform transform)
        {
            LinkElementType = linkType;

            SourceElementId = sourceElement.Id;
            SourceUniqueId = sourceElement.UniqueId;
            LinkedElementId = linkedElement.Id;
            LinkedUniqueId = linkedElement.UniqueId;
            SourceLinkInstanceId = linkInstanceId;
            Matched = CompareLocation(sourceElement, linkedElement, transform);

            //tree view
            CategoryName = linkedElement.Category.Name;
#if REVIT2024_OR_GREATER
            if (customCategories.Contains(linkedElement.Category.Id.Value))
#else
            if (customCategories.Contains(linkedElement.Category.Id.IntegerValue))
#endif
            {
                LinkDisplayText = "Source Element: " + sourceElement.Name + ", Target Element: " + linkedElement.Name;
            }
            else
            {
                var typeId = linkedElement.GetTypeId();
                if (typeId != ElementId.InvalidElementId)
                {
                    var elementType = linkedElement.Document.GetElement(typeId) as ElementType;
                    if (null != elementType)
                    {
                        var typeInfo = new ElementTypeInfo(elementType);
                        FamilyName = typeInfo.FamilyName;
                        FamilyTypeName = typeInfo.Name;
#if REVIT2024_OR_GREATER
                        LinkDisplayText = "Source Id: " + SourceElementId.Value + ", Target Id: " + LinkedElementId.Value;
#else
                        LinkDisplayText = "Source Id: " + SourceElementId.IntegerValue + ", Target Id: " + LinkedElementId.IntegerValue;
#endif
                    }
                }
            }
            ToolTipText = (linkType == LinkType.ByCopy) ? "Created by Duplication" : "Defined by Users";

        }

        public static bool CompareLocation(Element sourceElement, Element linkedElement, Transform transform)
        {
            var identical = false;
            try
            {
                if (null != sourceElement.Location && null != linkedElement.Location)
                {
                    if (sourceElement.Location is LocationPoint && linkedElement.Location is LocationPoint)
                    {
                        var sourceLocation = sourceElement.Location as LocationPoint;
                        var targetLocation = linkedElement.Location as LocationPoint;
                        var sourcePt = transform.OfPoint(sourceLocation.Point);
                        var targetPt = targetLocation.Point;
                        if (sourcePt.IsAlmostEqualTo(targetPt))
                        {
                            if (sourceLocation.Rotation == targetLocation.Rotation)
                            {
                                identical = true;
                            }
                        }
                    }
                    else if (sourceElement.Location is LocationCurve && linkedElement.Location is LocationCurve)
                    {
                        var sourceLocation = sourceElement.Location as LocationCurve;
                        var targetLocation = linkedElement.Location as LocationCurve;
                        var sourceCurve = sourceLocation.Curve.CreateTransformed(transform);
                        var targetCurve = targetLocation.Curve;
                        var result = sourceCurve.Intersect(targetCurve);
                        if (result == SetComparisonResult.Equal)
                        {
                            identical = true;
                        }
                    }
                    else
                    {
                        identical = true;
                    }
                }
                else if (null == sourceElement.Location && null == linkedElement.Location)
                {
                    identical = true;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return identical;
        }

        public static bool MoveLocation(Element sourceElement, Element linkedElement, Transform transform)
        {
            var moved = false;
            try
            {
                if (null != sourceElement.Location && null != linkedElement.Location)
                {
                    if (sourceElement.Location is LocationPoint && linkedElement.Location is LocationPoint)
                    {
                        var sourceLocation = sourceElement.Location as LocationPoint;
                        var targetLocation = linkedElement.Location as LocationPoint;
                        var sourcePt = transform.OfPoint(sourceLocation.Point);
                        var targetPt = targetLocation.Point;
                        targetLocation.Point = sourcePt;
                        var axis = Line.CreateBound(targetPt, new XYZ(targetPt.X, targetPt.Y, targetPt.Z + 10));
                        var rotated = targetLocation.Rotate(axis, sourceLocation.Rotation);
                        moved = rotated;
                    }
                    else if (sourceElement.Location is LocationCurve && linkedElement.Location is LocationCurve)
                    {
                        var sourceLocation = sourceElement.Location as LocationCurve;
                        var targetLocation = linkedElement.Location as LocationCurve;
                        var sourceCurve = sourceLocation.Curve.CreateTransformed(transform);
                        var targetCurve = targetLocation.Curve;
                        targetLocation.Curve = sourceCurve;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return moved;
        }
    }

    public class LinkedFamilyInfo
    {
        public ElementId SourceLinkInstanceId { get; set; } = ElementId.InvalidElementId;
        public ElementId SourceTypeId { get; set; } = ElementId.InvalidElementId;
        public ElementId TargetTypeId { get; set; } = ElementId.InvalidElementId;
        public string CategoryName { get; set; } = "";
        public string SourceFamilyName { get; set; } = "";
        public string SourceTypeName { get; set; } = "";
        public string TargetFamilyName { get; set; } = "";
        public string TargetTypeName { get; set; } = "";

        public LinkedFamilyInfo()
        {
        }

        public LinkedFamilyInfo(ElementId linkInstanceId, ElementTypeInfo sourceType, ElementTypeInfo targetType)
        {
            SourceLinkInstanceId = linkInstanceId;
            SourceTypeId = sourceType.Id;
            TargetTypeId = targetType.Id;

            CategoryName = sourceType.CategoryName;
            SourceFamilyName = sourceType.FamilyName;
            SourceTypeName = sourceType.Name;
            TargetFamilyName = targetType.FamilyName;
            TargetTypeName = targetType.Name;
        }
    }

    public class ElementTypeInfo
    {
        public ElementId Id { get; set; }
        public string Name { get; set; }
        public string FamilyName { get; set; }
        public string CategoryName { get; set; } = "";

        public ElementTypeInfo(ElementType elementType)
        {
            Id = elementType.Id;
            Name = elementType.Name;
            FamilyName = elementType.FamilyName;
            if (null != elementType.Category)
            {
                CategoryName = elementType.Category.Name;
            }
        }
    }
}
