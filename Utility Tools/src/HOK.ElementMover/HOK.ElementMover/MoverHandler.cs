using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.ElementMover
{
    public class MoverHandler : IExternalEventHandler
    {
        private UIApplication m_app;
        private LinkedInstanceProperties selectedLink;
        public Document CurrentDocument { get; private set; }
        public MainWindow MainWindowInstance { get; set; } = null;
        public MappingWindow MappingWindowInstance { get; set; } = null;
        public FamilyWindow FamilyWindowInstance { get; set; } = null;
        public Request MoverRequest { get; } = new Request();
        public Dictionary<ElementId, LinkedInstanceProperties> LinkInstances { get; set; } = new Dictionary<ElementId, LinkedInstanceProperties>();
        public LinkedInstanceProperties SelectedLink { get { return selectedLink; } set { selectedLink = value; } }
        public List<LinkedElementInfo> LinkedElementToDelete { get; set; } = new List<LinkedElementInfo>();
        public List<LinkedFamilyInfo> LinkedFamilyToDelete { get; set; } = new List<LinkedFamilyInfo>();
        public LinkedElementInfo SelectedLinkedInfo { get; set; } = null;
        public LinkedFamilyInfo SelectedFamilyInfo { get; set; } = null;
        public UpdateMode SelectedUpdateMode { get; set; } = UpdateMode.None;

        public MoverHandler(UIApplication uiapp)
        {
            try
            {
                m_app = uiapp;
                CurrentDocument = uiapp.ActiveUIDocument.Document;

                CollectRvtLinks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize the external event hanlder.\n" + ex.Message, "External Event Hanlder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CollectRvtLinks()
        {
            try
            {
                var collector = new FilteredElementCollector(CurrentDocument);
                var instances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();
                if (instances.Count > 0)
                {
                    foreach (var instance in instances)
                    {
                        var lip = new LinkedInstanceProperties(instance);
                        if (!LinkInstances.ContainsKey(lip.InstanceId))
                        {
                            LinkInstances.Add(lip.InstanceId, lip);
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
                MessageBox.Show("Failed to collect Revit Link Instances.\n" + ex.Message, "Collect Revit Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Execute(UIApplication app)
        {
            try
            {
                CurrentDocument = app.ActiveUIDocument.Document;

                switch (MoverRequest.Take())
                {
                    case RequestId.SelectLinkInstance:
                        MainWindowInstance.DozeOff();
                        var picked = PickLinkInstance(out selectedLink);
                        if (picked)
                        {
                            MainWindowInstance.DisplayCategories(selectedLink);
                        }
                        MainWindowInstance.WakeUp();
                        break;
                    case RequestId.DuplicateElements:
                        MainWindowInstance.DozeOff();
                        try
                        {
                            selectedLink = ElementMoverUtil.DuplicateSelectedCategories(selectedLink, CurrentDocument, SelectedUpdateMode);
                            if (LinkInstances.ContainsKey(selectedLink.InstanceId))
                            {
                                LinkInstances.Remove(selectedLink.InstanceId);
                            }
                            LinkInstances.Add(selectedLink.InstanceId, selectedLink);
                            MainWindowInstance.UpdateLinkedInstanceProperties();
                        }
                        catch (Exception ex)
                        {
                            var message = ex.Message;
                        }
                        MainWindowInstance.WakeUp();
                        break;
                    case RequestId.SelectMappingElements:
                        MappingWindowInstance.DozeOff();
                        Element sourceElement = null;
                        Element targetElement = null;
                        var pickedMap = PickMappingElements(out sourceElement, out targetElement);
                        if (pickedMap)
                        {
                            if (LinkInstances.ContainsKey(selectedLink.InstanceId))
                            {
                                LinkInstances.Remove(selectedLink.InstanceId);
                            }
                            LinkInstances.Add(selectedLink.InstanceId, selectedLink);
                            MappingWindowInstance.RefreshLinkInstance();
                        }
                        MappingWindowInstance.WakeUp();
                        break;
                    case RequestId.DeleteMappingElements:
                        MappingWindowInstance.DozeOff();
                        if (LinkedElementToDelete.Count > 0)
                        {
                            using (var trans = new Transaction(CurrentDocument))
                            {
                                trans.Start("Delete Element Maps");
                                try
                                {
                                    foreach (var linkedInfo in LinkedElementToDelete)
                                    {
                                        if (selectedLink.LinkedElements.ContainsKey(linkedInfo.LinkedElementId))
                                        {
                                            selectedLink.LinkedElements.Remove(linkedInfo.LinkedElementId);
                                            var linkedElement = CurrentDocument.GetElement(linkedInfo.LinkedElementId);
                                            if (null != linkedElement)
                                            {
                                                var removed = MoverDataStorageUtil.RemoveLinkedElementInfo(linkedElement);
                                            }
                                        }
                                    }
                                    trans.Commit();
                                    if (LinkInstances.ContainsKey(selectedLink.InstanceId))
                                    {
                                        LinkInstances.Remove(selectedLink.InstanceId);
                                        LinkInstances.Add(selectedLink.InstanceId, selectedLink);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    MessageBox.Show("Failed to delete element maps.\n" + ex.Message, "Delete Element Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            MappingWindowInstance.RefreshLinkInstance();
                        }
                        MappingWindowInstance.WakeUp();
                        break;
                    case RequestId.ShowElement:
                        MappingWindowInstance.DozeOff();
                        if (null != SelectedLinkedInfo)
                        {
                            if (null != CurrentDocument.GetElement(SelectedLinkedInfo.LinkedElementId))
                            {
                                HighlightElement();
                            }
                        }
                        MappingWindowInstance.WakeUp();
                        break;
                    case RequestId.AddFamilyMapping:
                        FamilyWindowInstance.DozeOff();
                        if (null != SelectedFamilyInfo)
                        {
                            var tType = CurrentDocument.GetElement(SelectedFamilyInfo.TargetTypeId) as ElementType;
                            if (null != tType)
                            {
                                using (var trans = new Transaction(CurrentDocument))
                                {
                                    trans.Start("Add Family Map");
                                    try
                                    {
                                        if (selectedLink.LinkedFamilies.ContainsKey(SelectedFamilyInfo.TargetTypeId))
                                        {
                                            selectedLink.LinkedFamilies.Remove(SelectedFamilyInfo.TargetTypeId);
                                        }
                                        selectedLink.LinkedFamilies.Add(SelectedFamilyInfo.TargetTypeId, SelectedFamilyInfo);

                                        var updated = MoverDataStorageUtil.UpdateLinkedFamilyInfo(SelectedFamilyInfo, tType);

                                        if (LinkInstances.ContainsKey(selectedLink.InstanceId))
                                        {
                                            LinkInstances.Remove(selectedLink.InstanceId);
                                            LinkInstances.Add(selectedLink.InstanceId, selectedLink);
                                        }
                                        trans.Commit();
                                        MappingWindowInstance.RefreshLinkInstance();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.RollBack();
                                        var message = ex.Message;
                                    }
                                }
                            }
                        }

                        FamilyWindowInstance.WakeUp();
                        break;
                    case RequestId.DeleteFamilyMapping:
                        MappingWindowInstance.DozeOff();
                        if (null != SelectedFamilyInfo)
                        {
                            using (var trans = new Transaction(CurrentDocument))
                            {
                                trans.Start("Delete Family Map");
                                try
                                {
                                    foreach (var familyInfo in LinkedFamilyToDelete)
                                    {
                                        if (selectedLink.LinkedFamilies.ContainsKey(familyInfo.TargetTypeId))
                                        {
                                            selectedLink.LinkedFamilies.Remove(familyInfo.TargetTypeId);
                                            var tType = CurrentDocument.GetElement(familyInfo.TargetTypeId) as ElementType;
                                            if (null != tType)
                                            {
                                                var removed = MoverDataStorageUtil.RemoveLinkedFamilyInfo(tType);
                                            }
                                        }
                                    }
                                    trans.Commit();
                                    if (LinkInstances.ContainsKey(selectedLink.InstanceId))
                                    {
                                        LinkInstances.Remove(selectedLink.InstanceId);
                                        LinkInstances.Add(selectedLink.InstanceId, selectedLink);
                                    }
                                    MappingWindowInstance.RefreshLinkInstance();
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    var message = ex.Message;
                                }
                            }
                        }
                        MappingWindowInstance.WakeUp();
                        break;
                    case RequestId.None:
                        return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute external event handler.\n" + ex.Message, "Execute External Event Handler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool PickLinkInstance(out LinkedInstanceProperties lip)
        {
            var picked = false;
            lip = null;
            using (var trans = new Transaction(CurrentDocument))
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
                        if (LinkInstances.ContainsKey(elementId))
                        {
                            lip = LinkInstances[elementId];
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
            using (var trans = new Transaction(CurrentDocument))
            {
                trans.Start("Pick Mapping Elements");
                try
                {
                    var selection = m_app.ActiveUIDocument.Selection;
                    var reference = PickLinkedElement();
                    if (null != reference)
                    {
                        var linkedInstanceId = reference.ElementId;
                        if (LinkInstances.ContainsKey(linkedInstanceId) && reference.LinkedElementId != ElementId.InvalidElementId)
                        {
                            var lip = LinkInstances[linkedInstanceId];
                            var linkedDoc = lip.LinkedDocument;

                            var linkedElement = linkedDoc.GetElement(reference.LinkedElementId); //element in linked model
                            if (null != linkedElement)
                            {
                                if (null != linkedElement.Category)
                                {
                                    var categoryId = linkedElement.Category.Id;
                                    var categoryName = linkedElement.Category.Name;
                                    ISelectionFilter selFilter = new TargetElementSelectionFilter(categoryId);
                                    var secondReference = selection.PickObject(ObjectType.Element, "Pick a target item in the host model. The required category should be " + categoryName);
                                    if (null != secondReference)
                                    {
                                        var eId = secondReference.ElementId;
                                        var element = CurrentDocument.GetElement(eId);
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
                                            var targetType = CurrentDocument.GetElement(targetTypeId) as ElementType;
                                            if (null != targetType)
                                            {
                                                targetTypeInfo = new ElementTypeInfo(targetType);
                                            }

                                            if (null != sourceTypeInfo && null != targetType)
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
                    MessageBox.Show("Failed to pick mapping elements.\n" + ex.Message, "Pick Mapping Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    if (reference.ElementId == selectedLink.InstanceId) return reference;

                    var msgResult = MessageBox.Show("Please select a linked element from the selected link instance.\n" + selectedLink.DisplayName, "Linked Instance Mismatch", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    switch (msgResult)
                    {
                        case MessageBoxResult.OK:
                            PickLinkedElement();
                            break;
                        case MessageBoxResult.Cancel:
                            return null;
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
            using (var trans = new Transaction(CurrentDocument))
            {
                trans.Start("Select Element");
                try
                {
                    var uidoc = new UIDocument(CurrentDocument);
                    var selectedIds = new List<ElementId>
                    {
                        SelectedLinkedInfo.LinkedElementId
                    };
                    uidoc.Selection.SetElementIds(selectedIds);
                    uidoc.ShowElements(SelectedLinkedInfo.LinkedElementId);
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
                if (null != MappingWindowInstance)
                {
                    MappingWindowInstance.DozeOff();
                    MappingWindowInstance.RefreshLinkInstance();
                    MappingWindowInstance.WakeUp();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply changes made on the current document.\n" + ex.Message, "Apply Document Changes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string GetName()
        {
            return "Element Mover Event Handler";
        }
    }

    public enum RequestId
    {
        None = 0,
        SelectLinkInstance = 1,
        DuplicateElements = 2,
        SelectMappingElements = 3,
        DeleteMappingElements = 4,
        ShowElement = 5,
        AddFamilyMapping = 6,
        DeleteFamilyMapping = 7
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
#if RELEASE2024
            return elem.Category.Id.Value == (long)BuiltInCategory.OST_RvtLinks;
#else
            return elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RvtLinks;
#endif
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class TargetElementSelectionFilter : ISelectionFilter
    {
        private readonly ElementId categoryId;

        public TargetElementSelectionFilter(ElementId catId)
        {
            categoryId = catId;
        }

        public bool AllowElement(Element elem)
        {
            return elem.Category.Id == categoryId;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
