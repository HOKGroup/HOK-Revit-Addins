using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.ElementMover
{
    public class MoverHandler:IExternalEventHandler
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private MainWindow mainWindow = null;
        private MappingWindow mappingWindow = null;
        private FamilyWindow familyWindow = null;
        private Request m_request = new Request();

        private Dictionary<ElementId, LinkedInstanceProperties> linkInstances = new Dictionary<ElementId, LinkedInstanceProperties>();
        private LinkedInstanceProperties selectedLink = null;
        private List<LinkedElementInfo> linkedElementToDelete = new List<LinkedElementInfo>();
        private List<LinkedFamilyInfo> linkedFamilyToDelete = new List<LinkedFamilyInfo>();
        private LinkedElementInfo selectedLinkedInfo = null;
        private LinkedFamilyInfo selectedFamilyInfo = null;
        private UpdateMode selectedUpdateMode = UpdateMode.None;

        public Document CurrentDocument { get { return m_doc; } }
        public MainWindow MainWindowInstance { get { return mainWindow; } set { mainWindow = value; } }
        public MappingWindow MappingWindowInstance { get { return mappingWindow; } set { mappingWindow = value; } }
        public FamilyWindow FamilyWindowInstance { get { return familyWindow; } set { familyWindow = value; } }
        public Request MoverRequest { get { return m_request; } }
        public Dictionary<ElementId, LinkedInstanceProperties> LinkInstances { get { return linkInstances; } set { linkInstances = value; } }
        public LinkedInstanceProperties SelectedLink { get { return selectedLink; } set { selectedLink = value; } }
        public List<LinkedElementInfo> LinkedElementToDelete { get { return linkedElementToDelete; } set { linkedElementToDelete = value; } }
        public List<LinkedFamilyInfo> LinkedFamilyToDelete { get { return linkedFamilyToDelete; } set { linkedFamilyToDelete = value; } }
        public LinkedElementInfo SelectedLinkedInfo { get { return selectedLinkedInfo; } set { selectedLinkedInfo = value; } }
        public LinkedFamilyInfo SelectedFamilyInfo { get { return selectedFamilyInfo; } set { selectedFamilyInfo = value; } }
        public UpdateMode SelectedUpdateMode { get { return selectedUpdateMode; } set { selectedUpdateMode = value; } }
    
        public MoverHandler(UIApplication uiapp)
        {
            try
            {
                m_app = uiapp;
                m_doc = uiapp.ActiveUIDocument.Document;

                CollectRvtLinks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize the external event hanlder.\n"+ex.Message, "External Event Hanlder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CollectRvtLinks()
        {
            try
            {
                var collector = new FilteredElementCollector(m_doc);
                var instances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();
                if (instances.Count > 0)
                {
                    foreach (var instance in instances)
                    {
                        var lip = new LinkedInstanceProperties(instance);
                        if (!linkInstances.ContainsKey(lip.InstanceId))
                        {
                            linkInstances.Add(lip.InstanceId, lip);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("The current project doesn't contain any Revit link instances.", "Revit Link Instances Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect Revit Link Instances.\n"+ex.Message, "Collect Revit Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Execute(UIApplication app)
        {
            try
            {
                m_doc = app.ActiveUIDocument.Document;

                switch (MoverRequest.Take())
                {
                    case RequestId.SelectLinkInstance:
                        mainWindow.DozeOff();
                        var picked = PickLinkInstance(out selectedLink);
                        if (picked)
                        {
                            mainWindow.DisplayCategories(selectedLink);
                        }
                        mainWindow.WakeUp();
                        break;
                    case RequestId.DuplicateElements:
                        mainWindow.DozeOff();
                        try
                        {
                            selectedLink = ElementMoverUtil.DuplicateSelectedCategories(selectedLink, m_doc, selectedUpdateMode);
                            if (linkInstances.ContainsKey(selectedLink.InstanceId))
                            {
                                linkInstances.Remove(selectedLink.InstanceId);
                            }
                            linkInstances.Add(selectedLink.InstanceId, selectedLink);
                            mainWindow.UpdateLinkedInstanceProperties();
                        }
                        catch (Exception ex)
                        {
                            var message = ex.Message;
                        }
                        mainWindow.WakeUp();
                        break;
                    case RequestId.SelectMappingElements:
                        mappingWindow.DozeOff();
                        Element sourceElement = null;
                        Element targetElement = null;
                        var pickedMap = PickMappingElements(out sourceElement, out targetElement);
                        if (pickedMap)
                        {
                            if (linkInstances.ContainsKey(selectedLink.InstanceId))
                            {
                                linkInstances.Remove(selectedLink.InstanceId);
                            }
                            linkInstances.Add(selectedLink.InstanceId, selectedLink);
                            mappingWindow.RefreshLinkInstance();
                        }
                        mappingWindow.WakeUp();
                        break;
                    case RequestId.DeleteMappingElements:
                        mappingWindow.DozeOff();
                        if (linkedElementToDelete.Count > 0)
                        {
                            using (var trans = new Transaction(m_doc))
                            {
                                trans.Start("Delete Element Maps");
                                try
                                {
                                    foreach (var linkedInfo in linkedElementToDelete)
                                    {
                                        if (selectedLink.LinkedElements.ContainsKey(linkedInfo.LinkedElementId))
                                        {
                                            selectedLink.LinkedElements.Remove(linkedInfo.LinkedElementId);
                                            var linkedElement = m_doc.GetElement(linkedInfo.LinkedElementId);
                                            if (null != linkedElement)
                                            {
                                                var removed = MoverDataStorageUtil.RemoveLinkedElementInfo(linkedElement);
                                            }
                                        }
                                    }
                                    trans.Commit();
                                    if (linkInstances.ContainsKey(selectedLink.InstanceId))
                                    {
                                        linkInstances.Remove(selectedLink.InstanceId);
                                        linkInstances.Add(selectedLink.InstanceId, selectedLink);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    MessageBox.Show("Failed to delete element maps.\n"+ex.Message, "Delete Element Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            mappingWindow.RefreshLinkInstance();
                        }
                        mappingWindow.WakeUp();
                        break;
                    case RequestId.ShowElement:
                        mappingWindow.DozeOff();
                        if (null != selectedLinkedInfo)
                        {
                            if (null != m_doc.GetElement(selectedLinkedInfo.LinkedElementId))
                            {
                                HighlightElement();
                            }
                        }
                        mappingWindow.WakeUp();
                        break;
                    case RequestId.AddFamilyMapping:
                        familyWindow.DozeOff();
                        if (null != selectedFamilyInfo)
                        {
                            var tType = m_doc.GetElement(selectedFamilyInfo.TargetTypeId) as ElementType;
                            if (null != tType)
                            {
                                using (var trans = new Transaction(m_doc))
                                {
                                    trans.Start("Add Family Map");
                                    try
                                    {
                                        if (selectedLink.LinkedFamilies.ContainsKey(selectedFamilyInfo.TargetTypeId))
                                        {
                                            selectedLink.LinkedFamilies.Remove(selectedFamilyInfo.TargetTypeId);
                                        }
                                        selectedLink.LinkedFamilies.Add(selectedFamilyInfo.TargetTypeId, selectedFamilyInfo);

                                        var updated = MoverDataStorageUtil.UpdateLinkedFamilyInfo(selectedFamilyInfo, tType);

                                        if (linkInstances.ContainsKey(selectedLink.InstanceId))
                                        {
                                            linkInstances.Remove(selectedLink.InstanceId);
                                            linkInstances.Add(selectedLink.InstanceId, selectedLink);
                                        }
                                        trans.Commit();
                                        mappingWindow.RefreshLinkInstance();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.RollBack();
                                        var message = ex.Message;
                                    }
                                }
                            }
                        }
                      
                        familyWindow.WakeUp();
                        break;
                    case RequestId.DeleteFamilyMapping:
                        mappingWindow.DozeOff();
                        if (null != selectedFamilyInfo)
                        {
                            using (var trans = new Transaction(m_doc))
                            {
                                trans.Start("Delete Family Map");
                                try
                                {
                                    foreach (var familyInfo in linkedFamilyToDelete)
                                    {
                                        if (selectedLink.LinkedFamilies.ContainsKey(familyInfo.TargetTypeId))
                                        {
                                            selectedLink.LinkedFamilies.Remove(familyInfo.TargetTypeId);
                                            var tType = m_doc.GetElement(familyInfo.TargetTypeId) as ElementType;
                                            if (null != tType)
                                            {
                                                var removed = MoverDataStorageUtil.RemoveLinkedFamilyInfo(tType);
                                            }
                                        }
                                    }
                                    trans.Commit();
                                    if (linkInstances.ContainsKey(selectedLink.InstanceId))
                                    {
                                        linkInstances.Remove(selectedLink.InstanceId);
                                        linkInstances.Add(selectedLink.InstanceId, selectedLink);
                                    }
                                    mappingWindow.RefreshLinkInstance();
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    var message = ex.Message;
                                }
                            }
                        }
                        mappingWindow.WakeUp();
                        break;
                    case RequestId.None:
                        return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to execute external event handler.\n" + ex.Message, "Execute External Event Handler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool PickLinkInstance(out LinkedInstanceProperties lip)
        {
            var picked = false;
            lip = null;
            using (var trans = new Transaction(m_doc))
            {
                trans.Start("Pick Revit Link");
                try
                {
                    var selection = m_app.ActiveUIDocument.Selection;
                    ISelectionFilter selectFilter = new LinkInstanceSelectionFilter();
                    var reference = selection.PickObject(ObjectType.Element, selectFilter, "Select a Revit Link instance to retreive elements for source items.");
                    if (null != reference)
                    {
                        var elementId = reference.ElementId;
                        if (linkInstances.ContainsKey(elementId))
                        {
                            lip = linkInstances[elementId];
                            picked = true;
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    MessageBox.Show("Failed to select Revit Link.\n" + ex.Message, "Pick Revit Link Instance", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return picked;
        }

        private bool PickMappingElements(out Element sourceElement/*in link*/, out Element targetElement/*in host*/)
        {
            var picked = false;
            sourceElement = null;
            targetElement = null; 
            using (var trans = new Transaction(m_doc))
            {
                trans.Start("Pick Mapping Elements");
                try
                {
                    var selection = m_app.ActiveUIDocument.Selection;
                    var reference = PickLinkedElement();
                    if (null != reference)
                    {
                        var linkedInstanceId = reference.ElementId;
                        if (linkInstances.ContainsKey(linkedInstanceId) && reference.LinkedElementId!=ElementId.InvalidElementId)
                        {
                            var lip = linkInstances[linkedInstanceId];
                            var linkedDoc = lip.LinkedDocument;

                            var linkedElement = linkedDoc.GetElement(reference.LinkedElementId); //element in linked model
                            if (null != linkedElement)
                            {
                                if(null!=linkedElement.Category)
                                {
                                    var categoryId = linkedElement.Category.Id;
                                    var categoryName = linkedElement.Category.Name;
                                    ISelectionFilter selFilter = new TargetElementSelectionFilter(categoryId);
                                    var secondReference = selection.PickObject(ObjectType.Element, "Pick a target item in the host model. The required category should be "+categoryName);
                                    if (null != secondReference)
                                    {
                                        var eId = secondReference.ElementId;
                                        var element = m_doc.GetElement(eId);
                                        if (null != element)
                                        {
                                            ElementTypeInfo sourceTypeInfo = null;
                                            var sourceTypeId = linkedElement.GetTypeId();
                                            var sourceType = linkedDoc.GetElement(sourceTypeId) as ElementType;
                                            if (null != sourceType)
                                            {
                                                sourceTypeInfo = new ElementTypeInfo(sourceType);
                                            }

                                            ElementTypeInfo targetTypeInfo = null;
                                            var targetTypeId = element.GetTypeId();
                                            var targetType = m_doc.GetElement(targetTypeId) as ElementType;
                                            if (null != targetType)
                                            {
                                                targetTypeInfo = new ElementTypeInfo(targetType);
                                            }

                                            if (null!=sourceTypeInfo && null!=targetType) 
                                            {
                                                sourceElement = linkedElement;
                                                targetElement = element;
                                                picked = true;
                                            }
                                            else
                                            {
                                                var strBuilder = new StringBuilder();
                                                strBuilder.AppendLine("Source Family Name: " + sourceTypeInfo.FamilyName + ", Source Type Name: " + sourceTypeInfo.Name);
                                                strBuilder.AppendLine("Target Family Name: " + targetTypeInfo.FamilyName + ", Target Type Name: " + targetTypeInfo.Name);
                                                strBuilder.AppendLine("");
                                                strBuilder.AppendLine("Would you like to proceed with creating a map?");
                                                var result = MessageBox.Show(strBuilder.ToString(), "Mismatch Name", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                                if (result == MessageBoxResult.Yes)
                                                {
                                                    sourceElement = linkedElement;
                                                    targetElement = element;
                                                    picked = true;
                                                }
                                                else
                                                {
                                                    picked = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (picked && null != sourceElement && null != targetElement)
                    {
                        var linkInfo = new LinkedElementInfo(LinkType.ByMap, sourceElement, targetElement, selectedLink.InstanceId, selectedLink.TransformValue);
                        if (selectedLink.LinkedElements.ContainsKey(linkInfo.LinkedElementId))
                        {
                            selectedLink.LinkedElements.Remove(linkInfo.LinkedElementId);
                        }
                        selectedLink.LinkedElements.Add(linkInfo.LinkedElementId, linkInfo);

                        var updated = MoverDataStorageUtil.UpdateLinkedElementInfo(linkInfo, targetElement);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    MessageBox.Show("Failed to pick mapping elements.\n"+ex.Message, "Pick Mapping Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return picked;
        }

        private Reference PickLinkedElement()
        {
            Reference reference = null;
            try
            {
                var selection = m_app.ActiveUIDocument.Selection;
                reference = selection.PickObject(ObjectType.LinkedElement, "Pick a linked element in a linked instance " + selectedLink.DisplayName);
                if (null != reference)
                {
                    if (reference.ElementId == selectedLink.InstanceId)
                    {
                        return reference;
                    }
                    else
                    {
                        var msgResult = MessageBox.Show("Please select a linked element from the selected link instance.\n" + selectedLink.DisplayName, "Linked Instance Mismatch", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if (msgResult == MessageBoxResult.OK)
                        {
                            PickLinkedElement();
                        }
                        else if (msgResult == MessageBoxResult.Cancel)
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return reference;
        }

        private void HighlightElement()
        {
            using (var trans = new Transaction(m_doc))
            {
                trans.Start("Select Element");
                try
                {
                    var uidoc = new UIDocument(m_doc);
#if RELEASE2014
                    Element element = m_doc.GetElement(selectedLinkedInfo.LinkedElementId);
                    if(null!=element)
                    {
                        SelElementSet selElements = uidoc.Selection.Elements;
                        selElements.Insert(element);

                        uidoc.Selection.Elements = selElements;
                    }
#elif RELEASE2015||RELEASE2016 || RELEASE2017
                    List<ElementId> selectedIds = new List<ElementId>();
                    selectedIds.Add(selectedLinkedInfo.LinkedElementId);
                    uidoc.Selection.SetElementIds(selectedIds);
#endif

                    uidoc.ShowElements(selectedLinkedInfo.LinkedElementId);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    MessageBox.Show("Failed to select an element.\n" + ex.Message, "Select Element", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public void ApplyDocumentChanged()
        {
            try
            {
                if (null != mappingWindow)
                {
                    mappingWindow.DozeOff();
                    mappingWindow.RefreshLinkInstance();
                    mappingWindow.WakeUp();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply changes made on the current document.\n"+ex.Message, "Apply Document Changes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string GetName()
        {
            return "Element Mover Event Handler";
        }
    }

    public enum RequestId : int
    {
        None = 0,

        SelectLinkInstance = 1,

        DuplicateElements = 2,

        SelectMappingElements = 3,

        DeleteMappingElements = 4,

        ShowElement = 5,

        AddFamilyMapping = 6,

        DeleteFamilyMapping =7
    }

    public class Request
    {
        private int m_request = (int)RequestId.None;

        public RequestId Take()
        {
            return (RequestId)Interlocked.Exchange(ref m_request, (int)RequestId.None);
        }

        public void Make(RequestId request)
        {
            Interlocked.Exchange(ref m_request, (int)request);
        }
    }

    public class LinkInstanceSelectionFilter : ISelectionFilter
    {

        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RvtLinks)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class TargetElementSelectionFilter : ISelectionFilter
    {
        private ElementId categoryId = ElementId.InvalidElementId;

        public TargetElementSelectionFilter(ElementId catId)
        {
            categoryId = catId;
        }

        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id == categoryId)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
        
}
