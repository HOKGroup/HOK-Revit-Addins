﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;

namespace HOK.ElementMover
{
    public static class ElementMoverUtil
    {
        public static ProgressBar progressBar = null;
        public static TextBlock statusLabel = null;
        private static StringBuilder messageBuilder = new StringBuilder();

        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);
        private delegate void UpdateStatusLabelDelegate(DependencyProperty dp, object value);

        public static LinkedInstanceProperties DuplicateSelectedCategories(LinkedInstanceProperties lip, Document recipientDoc, UpdateMode updateMode)
        {
            var updatedLIP = lip;
            try
            {
                messageBuilder = new StringBuilder();
                var updateLabelDelegate = new UpdateStatusLabelDelegate(statusLabel.SetValue);

                progressBar.Visibility = System.Windows.Visibility.Visible;
                var categories = lip.Categories.Values.ToList();
                //categories = categories.OrderBy(o => o.Priority).ToList();
                foreach (var cp in categories)
                {
                    if (cp.Selected)
                    {
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Copying " + cp.CategoryName + "..." });
                        updatedLIP = DuplicateElements(recipientDoc, updatedLIP, cp, updateMode);
                    }
                }
                progressBar.Visibility = System.Windows.Visibility.Hidden;
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Ready"});

                MessageBox.Show(messageBuilder.ToString(), "Duplicate Elements", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to move elements.\n" + ex.Message, "Move Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedLIP;
        }

        private static LinkedInstanceProperties DuplicateElements(Document recipientDoc, LinkedInstanceProperties lip, CategoryProperties cp, UpdateMode updateMode)
        {
            var updatedLIP = lip;
            try
            {
                var collector = new FilteredElementCollector(updatedLIP.LinkedDocument);
                var elementsToCopy = collector.OfCategoryId(cp.CategoryId).WhereElementIsNotElementType().ToElements().ToList();

                var updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                progressBar.Value = 0;
                progressBar.Maximum = elementsToCopy.Count;

                var duplicated = 0;
                using (var tGroup = new TransactionGroup(recipientDoc))
                {
                    tGroup.Start("Duplicate Elements");
                    tGroup.IsFailureHandlingForcedModal = false;
                    var failureHanlder = new FailureHandler();

                    try
                    {
                        var options = new CopyPasteOptions();
                        options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

                        double value = 0;
                        foreach (var sourceElement in elementsToCopy) //elements from link
                        {
                            value++;
                            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                            var foundElements = from element in updatedLIP.LinkedElements.Values where element.SourceUniqueId == sourceElement.UniqueId && element.SourceLinkInstanceId == updatedLIP.InstanceId select element;
                            if (foundElements.Count() > 0)
                            {
                                var linkInfo = foundElements.First();
                                var linkedElement = recipientDoc.GetElement(linkInfo.LinkedElementId);
                                if (null != linkedElement)
                                {
                                    if (linkInfo.LinkElementType == LinkType.ByMap || updateMode == UpdateMode.UpdateLocationOnly)
                                    {
                                        if (!linkInfo.Matched)
                                        {
                                            using (var trans = new Transaction(recipientDoc))
                                            {
                                                trans.Start("Move Element");
                                                var failureOption = trans.GetFailureHandlingOptions();
                                                failureOption.SetForcedModalHandling(false);
                                                failureOption.SetFailuresPreprocessor(failureHanlder);
                                                failureOption.SetClearAfterRollback(true);
                                                trans.SetFailureHandlingOptions(failureOption);

                                                try
                                                {
                                                    var moved = LinkedElementInfo.MoveLocation(sourceElement, linkedElement, updatedLIP.TransformValue);
                                                    linkInfo.Matched = moved;
                                                    if (updatedLIP.LinkedElements.ContainsKey(linkedElement.Id))
                                                    {
                                                        updatedLIP.LinkedElements.Remove(linkedElement.Id);
                                                        updatedLIP.LinkedElements.Add(linkedElement.Id, linkInfo);
                                                    }
                                                    trans.Commit();
                                                    duplicated++;
                                                }
                                                catch (Exception ex)
                                                {
                                                    trans.RollBack();
                                                    var message = ex.Message;
                                                }
                                            }
                                        }
                                        continue;
                                    }
                                    else if (updateMode == UpdateMode.ReplaceElements)
                                    {
                                        using (var trans = new Transaction(recipientDoc))
                                        {
                                            trans.Start("Delete Element");
                                            var failureOption = trans.GetFailureHandlingOptions();
                                            failureOption.SetForcedModalHandling(false);
                                            failureOption.SetFailuresPreprocessor(failureHanlder);
                                            failureOption.SetClearAfterRollback(true);
                                            trans.SetFailureHandlingOptions(failureOption);

                                            try
                                            {
                                                var deletedIds = recipientDoc.Delete(linkInfo.LinkedElementId);
                                                if (updatedLIP.LinkedElements.ContainsKey(linkInfo.LinkedElementId))
                                                {
                                                    updatedLIP.LinkedElements.Remove(linkInfo.LinkedElementId);
                                                }
                                                trans.Commit();
                                            }
                                            catch (Exception ex)
                                            {
                                                var message = ex.Message;
                                                trans.RollBack();
                                            }
                                        }
                                    }
                                }
                            }

                            var elementIds = new List<ElementId>();
                            elementIds.Add(sourceElement.Id);
                            try
                            {
                                Element copiedElement = null;
                                var linkType = LinkType.None;
                                using (var trans = new Transaction(recipientDoc))
                                {
                                    trans.Start("Copy Element");
                                    var failureOption = trans.GetFailureHandlingOptions();
                                    failureOption.SetForcedModalHandling(false);
                                    failureOption.SetFailuresPreprocessor(failureHanlder);
                                    failureOption.SetClearAfterRollback(true);
                                    trans.SetFailureHandlingOptions(failureOption);

                                    try
                                    {
                                        copiedElement = CopyByFamilyMaps(recipientDoc, updatedLIP, sourceElement, options);
                                        if (null != copiedElement)
                                        {
                                            linkType = LinkType.ByMap;
                                        }
                                        else
                                        {
                                            linkType = LinkType.ByCopy;
                                            var copiedElementIds = ElementTransformUtils.CopyElements(updatedLIP.LinkedDocument, elementIds, recipientDoc, updatedLIP.TransformValue, options);
                                            if (copiedElementIds.Count > 0)
                                            {
                                                var copiedElementId = copiedElementIds.First();
                                                copiedElement = recipientDoc.GetElement(copiedElementId);
                                            }
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.RollBack();
                                        var message = ex.Message;
                                    }
                                }

                                if (null != copiedElement)
                                {
                                    using (var trans = new Transaction(recipientDoc))
                                    {
                                        trans.Start("Update Link Info");
                                        var failureOption = trans.GetFailureHandlingOptions();
                                        failureOption.SetForcedModalHandling(false);
                                        failureOption.SetFailuresPreprocessor(failureHanlder);
                                        failureOption.SetClearAfterRollback(true);
                                        trans.SetFailureHandlingOptions(failureOption);

                                        try
                                        {
                                            var linkInfo = new LinkedElementInfo(linkType, sourceElement, copiedElement, updatedLIP.InstanceId, updatedLIP.TransformValue);
                                            if (!linkInfo.Matched)
                                            {
                                                var moved = LinkedElementInfo.MoveLocation(sourceElement, copiedElement, updatedLIP.TransformValue);
                                                linkInfo.Matched = moved;
                                            }
                                            var updated = MoverDataStorageUtil.UpdateLinkedElementInfo(linkInfo, copiedElement);
                                            if (!updatedLIP.LinkedElements.ContainsKey(linkInfo.LinkedElementId))
                                            {
                                                updatedLIP.LinkedElements.Add(linkInfo.LinkedElementId, linkInfo);
                                            }
                                            duplicated++;
                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.RollBack();
                                            var message = ex.Message;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var message = ex.Message;
                            }
                        }

                 
                        if (updatedLIP.Categories.ContainsKey(cp.CategoryId))
                        {
                            cp.Selected = false;
                            updatedLIP.Categories.Remove(cp.CategoryId);
                            updatedLIP.Categories.Add(cp.CategoryId, cp);
                        }

                        tGroup.Assimilate();
                    }
                    catch (Exception ex)
                    {
                        tGroup.RollBack();
                        MessageBox.Show("Failed to duplicate elements in the category "+cp.CategoryName+"\n"+ex.Message, "Duplicate Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                messageBuilder.AppendLine(duplicated.ToString()  + " of "+elementsToCopy.Count.ToString()+" elements in " + cp.CategoryName + " has been successfully copied or updated.");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate elements.\n"+ex.Message, "Duplicate Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedLIP;
        }

        private static Element CopyByFamilyMaps(Document recipientDoc, LinkedInstanceProperties lip, Element sourceElement, CopyPasteOptions options)
        {
            Element copiedElement = null;
            try
            {
                var foundTypes = from sType in lip.LinkedFamilies.Values
                                 where sType.SourceLinkInstanceId == lip.InstanceId && sType.SourceTypeId == sourceElement.GetTypeId()
                                 select sType;
                if (foundTypes.Count() > 0)
                {
                    var familyInfo = foundTypes.First();
                    var targetType = recipientDoc.GetElement(familyInfo.TargetTypeId) as ElementType;
                    if (null != targetType)
                    {
                        if (sourceElement is FamilyInstance && targetType is FamilySymbol)
                        {
                            var sourceInstance = sourceElement as FamilyInstance;
                            var targetSymbol = targetType as FamilySymbol;
                            if (!targetSymbol.IsActive) { targetSymbol.Activate(); }

                            var location = sourceInstance.Location;
                            if (null != location)
                            {
                                Element hostElement = null;
                                if (null != sourceInstance.Host)
                                {
                                    var hostFound = from host in lip.LinkedElements.Values
                                                    where host.SourceLinkInstanceId == lip.InstanceId && host.SourceElementId == sourceInstance.Host.Id
                                                    select host;

                                    if (hostFound.Count() > 0)
                                    {
                                        var linkedElementInfo = hostFound.First();
                                        hostElement = recipientDoc.GetElement(linkedElementInfo.LinkedElementId);
                                    }
                                }
                                Level hostLevel = null;
                                if (null != sourceInstance.LevelId)
                                {
                                    var levelFound = from level in lip.LinkedElements.Values
                                                     where level.SourceLinkInstanceId == lip.InstanceId && level.SourceElementId == sourceInstance.LevelId
                                                     select level;
                                    if (levelFound.Count() > 0)
                                    {
                                        var linkedElementInfo = levelFound.First();
                                        hostLevel = recipientDoc.GetElement(linkedElementInfo.LinkedElementId) as Level;
                                    }
                                }

                                if (location is LocationPoint)
                                {
                                    var locationPt = location as LocationPoint;
                                    var point = locationPt.Point;
                                    point = lip.TransformValue.OfPoint(point);

                                    if (null != hostElement && null != hostLevel)
                                    {
                                        copiedElement = recipientDoc.Create.NewFamilyInstance(point, targetSymbol, hostElement, hostLevel, sourceInstance.StructuralType);
                                    }
                                    else if (null != hostElement)
                                    {
                                        copiedElement = recipientDoc.Create.NewFamilyInstance(point, targetSymbol, hostElement, sourceInstance.StructuralType);
                                    }
                                    else if (null != hostLevel)
                                    {
                                        copiedElement = recipientDoc.Create.NewFamilyInstance(point, targetSymbol, hostLevel, sourceInstance.StructuralType);
                                    }                                    
                                    else
                                    {
                                        copiedElement = recipientDoc.Create.NewFamilyInstance(point, targetSymbol, sourceInstance.StructuralType);
                                    }
                                }
                                else if (location is LocationCurve)
                                {
                                    var locationCv = location as LocationCurve;
                                    var curve = locationCv.Curve;
                                    curve = curve.CreateTransformed(lip.TransformValue);
                                    //check existing level mapping
                                    if (null != hostLevel)
                                    {
                                        copiedElement = recipientDoc.Create.NewFamilyInstance(curve, targetSymbol, hostLevel, sourceInstance.StructuralType);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //System Families : Copy Element and change type
                            var elementIds = new List<ElementId>();
                            elementIds.Add(sourceElement.Id);
                            var copiedElementIds = ElementTransformUtils.CopyElements(lip.LinkedDocument, elementIds, recipientDoc, lip.TransformValue, options);
                            if (copiedElementIds.Count > 0)
                            {
                                var copiedElementId = copiedElementIds.First();
                                copiedElement = recipientDoc.GetElement(copiedElementId);
                                if(copiedElement.CanHaveTypeAssigned())
                                {
                                    copiedElement.ChangeTypeId(familyInfo.TargetTypeId);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to place elements by family maps.\n"+ex.Message, "Place Family Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return copiedElement;
        }

        private static FamilyInstance PlaceFamilyInstance(Document recipientDoc, LinkedInstanceProperties lip, Element sourceElement)
        {
            FamilyInstance placedInstance = null;
            try
            {
                var sourceInstance = sourceElement as FamilyInstance;
                if (null != sourceInstance)
                {
                    var sourceSymbol = sourceInstance.Symbol;
                    var foundSymbols = from symbol in lip.LinkedFamilies.Values
                                       where symbol.SourceLinkInstanceId == lip.InstanceId && symbol.SourceTypeId == sourceSymbol.Id
                                       select symbol;

                    if (foundSymbols.Count() > 0)
                    {
                        var linkedFamilyInfo = foundSymbols.First();
                        var targetSymbol = recipientDoc.GetElement(linkedFamilyInfo.TargetTypeId) as FamilySymbol;
                        if (null != targetSymbol)
                        {
                            var location = sourceInstance.Location;
                            if (null != location)
                            {
                                if (location is LocationPoint)
                                {
                                    var locationPt = location as LocationPoint;
                                    var point = locationPt.Point;
                                    point = lip.TransformValue.OfPoint(point);
                                    placedInstance = recipientDoc.Create.NewFamilyInstance(point, targetSymbol, sourceInstance.StructuralType);
                                }
                                else if (location is LocationCurve)
                                {
                                    var locationCv = location as LocationCurve;
                                    var curve = locationCv.Curve;
                                    curve = curve.CreateTransformed(lip.TransformValue);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to place family instances by defined maps.\n"+ex.Message, "Place Family Instance", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return placedInstance;
        }
        
    }
    public class HideAndAcceptDuplicateTypeNamesHandler : IDuplicateTypeNamesHandler
    {
        #region IDuplicateTypeNamesHandler Members

        /// <summary>
        /// Implementation of the IDuplicateTypeNameHandler
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            // Always use duplicate destination types when asked
            return DuplicateTypeAction.UseDestinationTypes;
        }

        #endregion
    }

    public class FailureHandler : IFailuresPreprocessor
    {
        private List<FailureMessageInfo> failureMessageInfoList = new List<FailureMessageInfo>();

        public List<FailureMessageInfo> FailureMessageInfoList { get { return failureMessageInfoList; } set { failureMessageInfoList = value; } }

        public FailureHandler()
        {
        }

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {

            var failureMessages = failuresAccessor.GetFailureMessages();
            //if (failureMessages.Count == 0) { return FailureProcessingResult.Continue; }

            var needRollBack = false;
            foreach (var fma in failureMessages)
            {
                var messageInfo = new FailureMessageInfo();
                
                var severity = fma.GetSeverity();
                try
                {
                    if (severity == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(fma);
                    }
                    else
                    {
                        needRollBack = true;
                    }
                    messageInfo.TransactionName = failuresAccessor.GetTransactionName();
                    messageInfo.ErrorSeverity = severity.ToString();
                    try
                    {
                        messageInfo.ErrorMessage = fma.GetDescriptionText();
                        messageInfo.FailingElementIds = fma.GetFailingElementIds().ToList();
                    }
                    catch
                    {
                    }
                }
                catch { messageInfo.ErrorMessage = "Unknown Error"; }
                failureMessageInfoList.Add(messageInfo);
            }

            if (needRollBack) { return FailureProcessingResult.ProceedWithRollBack; }
            else { return FailureProcessingResult.Continue; }
        }
    }

    public class FailureMessageInfo
    {
        public string TransactionName { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorSeverity { get; set; }
        public List<ElementId> FailingElementIds { get; set; }
    }
}
