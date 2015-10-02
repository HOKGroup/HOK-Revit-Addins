using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace HOK.ElementMover
{
    public class LinkedInstanceProperties
    {
        private RevitLinkInstance m_instance = null;
        private ElementId instanceId = ElementId.InvalidElementId;
        private string instanceName = "";
        private Autodesk.Revit.DB.Transform transformValue = null;
        private Document linkedDocument = null;
        private string linkDocTitle = "";
        private string displayName = "";
        private Dictionary<ElementId, CategoryProperties> categories = new Dictionary<ElementId, CategoryProperties>();
        private Dictionary<ElementId/*targetElementInHost*/, LinkedElementInfo> linkedElements = new Dictionary<ElementId, LinkedElementInfo>();
        private Dictionary<ElementId/*ElementType in host*/, LinkedFamilyInfo> linkedFamilies = new Dictionary<ElementId, LinkedFamilyInfo>();
        
        private BuiltInCategory[] customCategories = new BuiltInCategory[] { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels, BuiltInCategory.OST_Grids, BuiltInCategory.OST_VolumeOfInterest };

        public RevitLinkInstance Instance { get { return m_instance; } set { m_instance = value; } }
        public ElementId InstanceId { get { return instanceId; } set { instanceId = value; } }
        public string InstanceName { get { return instanceName; } set { instanceName = value; } }
        public Autodesk.Revit.DB.Transform TransformValue { get { return transformValue; } set { transformValue = value; } }
        public Document LinkedDocument { get { return linkedDocument; } set { linkedDocument = value; } }
        public string LinkDocTitle { get { return linkDocTitle; } set { linkDocTitle = value; } }
        public string DisplayName { get { return displayName; } set { displayName = value; } }
        public Dictionary<ElementId, CategoryProperties> Categories { get { return categories; } set { categories = value; } }
        public Dictionary<ElementId, LinkedElementInfo> LinkedElements { get { return linkedElements; } set { linkedElements = value; } }
        public Dictionary<ElementId, LinkedFamilyInfo> LinkedFamilies { get { return linkedFamilies; } set { linkedFamilies = value; } }
        
        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            m_instance = instance;
            instanceId = m_instance.Id;

            CollectLinkInstanceInfo();
            if (null != linkedDocument)
            {
                CollectCategories(); //model element + Rooms, Levels, Grids
                CollectElementMaps();
                CollectFamilyMaps();
            }
        }

        private void CollectLinkInstanceInfo()
        {
            try
            {
                Parameter param = m_instance.get_Parameter(BuiltInParameter.RVT_LINK_INSTANCE_NAME);
                if (null != param)
                {
                    instanceName = param.AsString();
                }

                if (null != m_instance.GetTotalTransform())
                {
                    transformValue = m_instance.GetTotalTransform();
                }

#if RELEASE2014||RELEASE2015||RELEASE2016
                linkedDocument = m_instance.GetLinkDocument();
                if (null != linkedDocument)
                {
                    linkDocTitle = linkedDocument.Title;
                }
#endif
                if (!string.IsNullOrEmpty(instanceName) && !string.IsNullOrEmpty(linkDocTitle))
                {
                    displayName = linkDocTitle + " - " + instanceName;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void CollectCategories()
        {
            try
            {
                //Categories that have material quantities
                FilteredElementCollector collector = new FilteredElementCollector(linkedDocument);
                List<Element> elements = collector.WhereElementIsNotElementType().ToElements().ToList();
                var elementCategories = from element in elements where null != element.Category select element.Category;
#if RELEASE2014
                var modelCategories = from category in elementCategories where category.HasMaterialQuantities select category;
#elif RELEASE2015|| RELEASE2016
                var modelCategories = from category in elementCategories where  category.HasMaterialQuantities && category.CategoryType == CategoryType.Model select category;
#endif

                var categoryNames = from category in modelCategories select category.Name;
                List<string> categoryNameList = categoryNames.Distinct().ToList();

                Autodesk.Revit.DB.Categories categoryObjects = linkedDocument.Settings.Categories;
                foreach (string catName in categoryNameList)
                {
                    if (!categoryObjects.Contains(catName)) { continue; }
                   
                    Category category = categoryObjects.get_Item(catName);
                    if (null != category)
                    {
                        CategoryProperties catProperties = new CategoryProperties(category);
                        var categoryFound = from modelCat in modelCategories where modelCat.Id == category.Id select modelCat;
                        int itemCount = categoryFound.Count();
                        if (itemCount > 0)
                        {
                            catProperties.ItemCount = itemCount;
                            if (!categories.ContainsKey(catProperties.CategoryId))
                            {
                                categories.Add(catProperties.CategoryId, catProperties);
                            }
                        }
                    }
                }

                //Categories that belongs to MEP curves
                collector = new FilteredElementCollector(linkedDocument);
                List<Element> mepCurves = collector.OfClass(typeof(MEPCurve)).ToElements().ToList();
                if (mepCurves.Count > 0)
                {
                    var mepCategories = from mepCurve in mepCurves select mepCurve.Category.Name;
                    List<string> mepCategoryNameList = mepCategories.Distinct().ToList();
                    foreach (string catName in mepCategoryNameList)
                    {
                        if (!categoryObjects.Contains(catName)) { continue; }

                        Category category = categoryObjects.get_Item(catName);
                        if (null != category)
                        {
                            CategoryProperties catProperties = new CategoryProperties(category);
                            var categoryFound = from mepCurve in mepCurves where mepCurve.Category.Id == category.Id select mepCurve;
                            int itemCount= categoryFound.Count();
                            if (itemCount > 0)
                            {
                                catProperties.ItemCount = itemCount;
                                if (!categories.ContainsKey(catProperties.CategoryId))
                                {
                                    categories.Add(catProperties.CategoryId, catProperties);
                                }
                            }
                        }
                    }
                }

                foreach (BuiltInCategory bltCategory in customCategories)
                {
                    collector = new FilteredElementCollector(linkedDocument);
                    ICollection<ElementId> customCatElements = collector.OfCategory(bltCategory).ToElementIds();
                    if (customCatElements.Count > 0)
                    {
                        Category category = categoryObjects.get_Item(bltCategory);
                        if (null != category)
                        {
                            CategoryProperties catProperties = new CategoryProperties(category);
                            catProperties.ItemCount = customCatElements.Count;
                            if (!categories.ContainsKey(catProperties.CategoryId))
                            {
                                categories.Add(catProperties.CategoryId, catProperties);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void CollectElementMaps()
        {
            try
            {
                Document hostDoc = m_instance.Document;
                List<ElementId> categoryIds = categories.Keys.ToList();
                foreach (ElementId catId in categoryIds)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(hostDoc);
                    List<Element> elements = collector.OfCategoryId(catId).WhereElementIsNotElementType().ToElements().ToList();
                    var elementWithEntities = from element in elements where element.GetEntitySchemaGuids().Count > 0 select element;
                    if (elementWithEntities.Count() > 0)
                    {
                        elements = elementWithEntities.ToList();
                    }

                    foreach (Element element in elements)
                    {
                        if (null == element.Location) { continue; } // unplaced rooms

                        LinkedElementInfo linkInfo = MoverDataStorageUtil.GetLinkedElementInfo(element);
                        if (null != linkInfo)
                        {
                            if (linkInfo.SourceLinkInstanceId != instanceId) { continue; }
                            if (element.Id != linkInfo.LinkedElementId) { continue; }

                            Element sourceElement = linkedDocument.GetElement(linkInfo.SourceElementId);
                            if (null != sourceElement)
                            {
                                linkInfo = new LinkedElementInfo(linkInfo.LinkElementType, sourceElement, element, instanceId, transformValue);
                                if (!linkedElements.ContainsKey(linkInfo.LinkedElementId))
                                {
                                    linkedElements.Add(linkInfo.LinkedElementId, linkInfo);
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
                Document hostDoc = m_instance.Document;
                List<ElementId> categoryIds = categories.Keys.ToList();
                foreach (ElementId catId in categoryIds)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(hostDoc);
                    List<Element> elements = collector.OfCategoryId(catId).WhereElementIsElementType().ToElements().ToList();
                    foreach (Element element in elements)
                    {
                        ElementType elementType = element as ElementType;
                        if (null != elementType)
                        {
                            LinkedFamilyInfo familyInfo = MoverDataStorageUtil.GetLinkedFamilyInfo(elementType);
                            if (null != familyInfo)
                            {
                                if (familyInfo.SourceLinkInstanceId != instanceId) { continue; }
                                if (element.Id != familyInfo.TargetTypeId) { continue; }

                                ElementType sourceType = linkedDocument.GetElement(familyInfo.SourceTypeId) as ElementType;
                                if (null != sourceType)
                                {
                                    ElementTypeInfo sourceTypeInfo = new ElementTypeInfo(sourceType);
                                    ElementTypeInfo targetTypeInfo = new ElementTypeInfo(elementType);

                                    familyInfo = new LinkedFamilyInfo(familyInfo.SourceLinkInstanceId, sourceTypeInfo, targetTypeInfo);
                                    if (!linkedFamilies.ContainsKey(familyInfo.TargetTypeId))
                                    {
                                        linkedFamilies.Add(familyInfo.TargetTypeId, familyInfo);
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
        private bool selected = false;
        private ElementId categoryId = ElementId.InvalidElementId;
        private string categoryName = "";
        private int itemCount = 0;

        public bool Selected { get { return selected; } set { selected = value; } }
        public ElementId CategoryId { get { return categoryId; } set { categoryId = value; } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; } }
        public int ItemCount { get { return itemCount; } set { itemCount = value; } }

        public CategoryProperties(Category category)
        {
            categoryId = category.Id;
            categoryName = category.Name;
        }
    }

    public enum LinkType
    {
        ByCopy, ByMap, None
    }

    public class LinkedElementInfo
    {
        private LinkType linkElementType = LinkType.None;
        private ElementId sourceElementId = ElementId.InvalidElementId; //source element in links
        private string sourceUniqueId = "";
        private ElementId linkedElementId = ElementId.InvalidElementId; //linked element in the host model
        private string linkedUniqueId = "";
        private ElementId sourceLinkInstanceId = ElementId.InvalidElementId;

        //For Tree View
        private string categoryName = "";
        private string familyName = "";
        private string familyTypeName = "";
        private string linkDisplayText = "";
        private string tooltipText = "";
        private bool matched = false;

        private int[] customCategories = new int[] { (int)BuiltInCategory.OST_Rooms, (int)BuiltInCategory.OST_Levels, (int)BuiltInCategory.OST_Grids, (int)BuiltInCategory.OST_VolumeOfInterest };

        public LinkType LinkElementType { get { return linkElementType; } set { linkElementType = value; } }
        public ElementId SourceElementId { get { return sourceElementId; } set { sourceElementId = value; } }
        public string SourceUniqueId { get { return sourceUniqueId; } set { sourceUniqueId = value; } }
        public ElementId LinkedElementId { get { return linkedElementId; } set { linkedElementId = value; } }
        public string LinkedUniqueId { get { return linkedUniqueId; } set { linkedUniqueId = value; } }
        public ElementId SourceLinkInstanceId { get { return sourceLinkInstanceId; } set { sourceLinkInstanceId = value; } }

        //For Tree View
        public string CategoryName { get { return categoryName; } set { categoryName = value; } }
        public string FamilyName { get { return familyName; } set { familyName = value; } }
        public string FamilyTypeName { get { return familyTypeName; } set { familyTypeName = value; } }
        public string LinkDisplayText { get { return linkDisplayText; } set { linkDisplayText = value; } }
        public string ToolTipText { get { return tooltipText; } set { tooltipText = value; } }
        public bool Matched { get { return matched; } set { matched = value; } }

        public LinkedElementInfo()
        {
        }

        public LinkedElementInfo(LinkType linkType, Element sourceElement, Element linkedElement, ElementId linkInstanceId, Transform transform)
        {
            linkElementType = linkType;

            sourceElementId = sourceElement.Id;
            sourceUniqueId = sourceElement.UniqueId;
            linkedElementId = linkedElement.Id;
            linkedUniqueId = linkedElement.UniqueId;
            sourceLinkInstanceId = linkInstanceId;
            matched = CompareLocation(sourceElement, linkedElement, transform);

            //tree view
            categoryName = linkedElement.Category.Name;
            if (customCategories.Contains(linkedElement.Category.Id.IntegerValue))
            {
                linkDisplayText = "Source Element: " + sourceElement.Name + ", Target Element: " + linkedElement.Name;
            }
            else
            {
                ElementId typeId = linkedElement.GetTypeId();
                if (typeId != ElementId.InvalidElementId)
                {
                    ElementType elementType = linkedElement.Document.GetElement(typeId) as ElementType;
                    if (null != elementType)
                    {
                        ElementTypeInfo typeInfo = new ElementTypeInfo(elementType);
                        familyName = typeInfo.FamilyName;
                        familyTypeName = typeInfo.Name;
                        linkDisplayText = "Source Id: " + sourceElementId.IntegerValue + ", Target Id: " + linkedElementId.IntegerValue;
                    }
                }
            }
            tooltipText = (linkType == LinkType.ByCopy) ? "Created by Duplication" : "Defined by Users";
            
        }

        public static bool CompareLocation(Element sourceElement, Element linkedElement, Transform transform)
        {
            bool identical = false;
            try
            {
                if (null != sourceElement.Location && null != linkedElement.Location)
                {
                    if (sourceElement.Location is LocationPoint && linkedElement.Location is LocationPoint)
                    {
                        LocationPoint sourceLocation = sourceElement.Location as LocationPoint;
                        LocationPoint targetLocation = linkedElement.Location as LocationPoint;
                        XYZ sourcePt = transform.OfPoint(sourceLocation.Point);
                        XYZ targetPt = targetLocation.Point;
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
                        LocationCurve sourceLocation = sourceElement.Location as LocationCurve;
                        LocationCurve targetLocation = linkedElement.Location as LocationCurve;
                        Curve sourceCurve = sourceLocation.Curve.CreateTransformed(transform);
                        Curve targetCurve = targetLocation.Curve;
                        SetComparisonResult result = sourceCurve.Intersect(targetCurve);
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
                else if(null == sourceElement.Location && null== linkedElement.Location)
                {
                    identical = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return identical;
        }

        public static bool MoveLocation(Element sourceElement, Element linkedElement, Transform transform)
        {
            bool moved = false;
            try
            {
                if (null != sourceElement.Location && null != linkedElement.Location)
                {
                    if (sourceElement.Location is LocationPoint && linkedElement.Location is LocationPoint)
                    {
                        LocationPoint sourceLocation = sourceElement.Location as LocationPoint;
                        LocationPoint targetLocation = linkedElement.Location as LocationPoint;
                        XYZ sourcePt = transform.OfPoint(sourceLocation.Point);
                        XYZ targetPt = targetLocation.Point;
                        targetLocation.Point = sourcePt;
                        Line axis = Line.CreateBound(targetPt, new XYZ(targetPt.X, targetPt.Y, targetPt.Z + 10));
                        bool rotated = targetLocation.Rotate(axis, sourceLocation.Rotation);
                        moved = rotated;
                    }
                    else if (sourceElement.Location is LocationCurve && linkedElement.Location is LocationCurve)
                    {
                        LocationCurve sourceLocation = sourceElement.Location as LocationCurve;
                        LocationCurve targetLocation = linkedElement.Location as LocationCurve;
                        Curve sourceCurve = sourceLocation.Curve.CreateTransformed(transform);
                        Curve targetCurve = targetLocation.Curve;
                        targetLocation.Curve = sourceCurve;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return moved;
        }
    }

    public class LinkedFamilyInfo
    {
        private ElementId sourceLinkInstanceId = ElementId.InvalidElementId;
        private ElementId sourceTypeId = ElementId.InvalidElementId;
        private ElementId targetTypeId = ElementId.InvalidElementId;

        private string categoryName = "";
        private string sourceFamilyName = "";
        private string sourceTypeName = "";
        private string targetFamilyName = "";
        private string targetTypeName = "";

        public ElementId SourceLinkInstanceId { get { return sourceLinkInstanceId; } set { sourceLinkInstanceId = value; } }
        public ElementId SourceTypeId { get { return sourceTypeId; } set { sourceTypeId = value; } }
        public ElementId TargetTypeId { get { return targetTypeId; } set { targetTypeId = value; } }
        
        public string CategoryName { get { return categoryName; } set { categoryName = value; } }
        public string SourceFamilyName { get { return sourceFamilyName; } set { sourceFamilyName = value; } }
        public string SourceTypeName { get { return sourceTypeName; } set { sourceTypeName = value; } }
        public string TargetFamilyName { get { return targetFamilyName; } set { targetFamilyName = value; } }
        public string TargetTypeName { get { return targetTypeName; } set { targetTypeName = value; } }

        public LinkedFamilyInfo()
        {
        }

        public LinkedFamilyInfo(ElementId linkInstanceId, ElementTypeInfo sourceType, ElementTypeInfo targetType)
        {
            sourceLinkInstanceId = linkInstanceId;
            sourceTypeId = sourceType.Id;
            targetTypeId = targetType.Id;

            categoryName = sourceType.CategoryName;
            sourceFamilyName = sourceType.FamilyName;
            sourceTypeName = sourceType.Name;
            targetFamilyName = targetType.FamilyName;
            targetTypeName = targetType.Name;
        }
    }

    public class ElementTypeInfo
    {
        private ElementId id = ElementId.InvalidElementId;
        private string name = "";
        private string familyName = "";
        private string categoryName = "";

        public ElementId Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public string FamilyName { get { return familyName; } set { familyName = value; } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; } }

        public ElementTypeInfo(ElementType elementType)
        {
            id = elementType.Id;
            name = elementType.Name;
#if RELEASE2014
            Parameter param = elementType.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            if (null != param)
            {
                familyName = param.AsString();
            }
#elif RELEASE2015 || RELEASE2016
            familyName = elementType.FamilyName;
#endif
            if (null != elementType.Category)
            {
                categoryName = elementType.Category.Name;
            }
        }
    }

}
