using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HOK.ModelManager.ReplicateViews;
using Autodesk.Revit.DB;
using System.Windows;
using HOK.ModelManager.GoogleDocs;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

namespace HOK.ModelManager
{
    class DuplicateUtils
    {

        public static PreviewMap DuplicateView(PreviewMap previewMap, bool createSheet)
        {
            PreviewMap preview = previewMap;
            try
            {    
                CopyPasteOptions options = new CopyPasteOptions();
                options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

                Document toDoc = preview.RecipientModelInfo.Doc;
                List<ElementId> viewIds = new List<ElementId>();
                using (TransactionGroup tg = new TransactionGroup(toDoc, "Duplicate across documents with detailing"))
                {
                    tg.Start();

                    if (preview.IsEnabled)
                    {
                        ViewDrafting copiedView = null;
                        if (null != preview.SourceViewProperties.LinkedView) //already exist in recipient model
                        {
                            ElementId viewId = new ElementId(preview.SourceViewProperties.LinkedView.ViewId);
                            using (Transaction trans = new Transaction(preview.RecipientModelInfo.Doc, "Delete Existing Contents"))
                            {
                                trans.Start();
                                FilteredElementCollector collector = new FilteredElementCollector(preview.RecipientModelInfo.Doc, viewId);
                                collector.WherePasses(new ElementCategoryFilter(ElementId.InvalidElementId, true));
                                ICollection<ElementId> toDelete = collector.ToElementIds();
                                ICollection<ElementId> deletedIds = preview.RecipientModelInfo.Doc.Delete(toDelete);
                                trans.Commit();
                            }

                            copiedView = preview.RecipientModelInfo.Doc.GetElement(viewId) as ViewDrafting;
                            if (null != copiedView)
                            {
                                int numOfCopied = DuplicateDetailingAcrossViews(previewMap.SourceViewProperties.ViewDraftingObj, copiedView);
                            }
                        }
                        else
                        {
                            if (preview.SourceViewProperties.IsOnSheet && createSheet)
                            {
                                ViewSheet copiedSheet = DuplicateSheets(preview);
                                copiedView = DuplicateDraftingViews(preview);

                                if (null != copiedSheet && null != copiedView)
                                {
                                    if (Viewport.CanAddViewToSheet(preview.RecipientModelInfo.Doc, copiedSheet.Id, copiedView.Id))
                                    {
                                        Viewport viewport = DuplicateViewPort(preview, copiedSheet, copiedView);
                                    }
                                }
                            }
                            else
                            {
                                copiedView = DuplicateDraftingViews(preview);
                            }
                        }
                        preview = UpdatePreviewMap(preview, copiedView);
                    }
                    tg.Assimilate();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate drafiting views.\n" + ex.Message, "Duplicate Drafting Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return preview;
        }
        
        private static ViewDrafting DuplicateDraftingViews(PreviewMap previewMap)
        {
            ViewDrafting viewDrafting = null;
            try
            {
                Document toDoc = previewMap.RecipientModelInfo.Doc;
                CopyPasteOptions options = new CopyPasteOptions();
                options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());
                
                ICollection<ElementId> copiedIds;
                using (Transaction transaction = new Transaction(toDoc, "Duplicate Draftingviews"))
                {
                    transaction.Start();
                    List<ElementId> viewIds = new List<ElementId>();
                    viewIds.Add(new ElementId(previewMap.SourceViewProperties.ViewId));

                    copiedIds = ElementTransformUtils.CopyElements(previewMap.SourceModelInfo.Doc, viewIds, toDoc, Transform.Identity, options);

                    FailureHandlingOptions failureOptions = transaction.GetFailureHandlingOptions();
                    failureOptions.SetFailuresPreprocessor(new HidePasteDuplicateTypesPreprocessor());
                    transaction.Commit(failureOptions);
                }

                if (copiedIds.Count > 0)
                {
                    ElementId viewId = copiedIds.First();
                    ViewDrafting copiedView = toDoc.GetElement(viewId) as ViewDrafting;
                    if (null != copiedView)
                    {
                        int numOfCopied = DuplicateDetailingAcrossViews(previewMap.SourceViewProperties.ViewDraftingObj, copiedView);
                    }
                    viewDrafting = copiedView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate drafintg views.\n"+ex.Message, "Duplicate DraftingViews", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewDrafting;
        }

        private static ViewSheet DuplicateSheets(PreviewMap previewMap)
        {
            ViewSheet viewSheet = null;
            try
            {
                Document toDoc = previewMap.RecipientModelInfo.Doc;
                //check existing viewsheets in recipient model
                FilteredElementCollector collector = new FilteredElementCollector(toDoc); ;
                List<ViewSheet> viewsheets = collector.OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements().Cast<ViewSheet>().ToList();
                var sheets = from view in viewsheets where view.ViewName == previewMap.SourceViewProperties.SheetName && view.SheetNumber == previewMap.SourceViewProperties.SheetNumber select view;
                if (sheets.Count() > 0)
                {
                    viewSheet = sheets.First();
                }
                else //create sheet from source model
                {
                    ElementId titleBlockTypeId = GetTitleBlockId(previewMap);
                    if (titleBlockTypeId != ElementId.InvalidElementId)
                    {
                        using (Transaction transaction = new Transaction(toDoc, "Create Sheet"))
                        {
                            transaction.Start();
                            viewSheet = ViewSheet.Create(toDoc, titleBlockTypeId);
                            transaction.Commit();
                        }
                        if (null != viewSheet)
                        {
                            using (Transaction trans = new Transaction(toDoc, "Move Title Block"))
                            {
                                trans.Start();
                                try 
                                {
                                    FilteredElementCollector vCollector = new FilteredElementCollector(previewMap.SourceModelInfo.Doc, previewMap.SourceViewProperties.SheetObj.Id);
                                    List<FamilyInstance> instances = vCollector.OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof(FamilyInstance)).ToElements().Cast<FamilyInstance>().ToList();
                                    if (instances.Count > 0)
                                    {
                                        FamilyInstance sourceTitleBlock = instances.First();
                                        LocationPoint slocation = sourceTitleBlock.Location as LocationPoint;
                                        XYZ sourcePoint = slocation.Point;

                                        vCollector = new FilteredElementCollector(toDoc, viewSheet.Id);
                                        List<FamilyInstance> blocks = vCollector.OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof(FamilyInstance)).ToElements().Cast<FamilyInstance>().ToList();
                                        if (blocks.Count > 0)
                                        {
                                            FamilyInstance targetTitleBlock = blocks.First();
                                            LocationPoint tlocation = targetTitleBlock.Location as LocationPoint;
                                            XYZ targetPoint = tlocation.Point;

                                            XYZ moveVector = sourcePoint - targetPoint;
                                            ElementTransformUtils.MoveElement(toDoc, targetTitleBlock.Id, moveVector);
                                        }
                                    }
                                    trans.Commit();
                                }
                                catch
                                {
                                    trans.RollBack();
                                }
                            }

                            using (Transaction transaction = new Transaction(toDoc, "Write Sheet Parameter"))
                            {
                                transaction.Start();
                                foreach (Parameter param in previewMap.SourceViewProperties.SheetObj.Parameters)
                                {
                                    if (!param.IsReadOnly)
                                    {
#if RELEASE2014
                                        Parameter rParam = viewSheet.get_Parameter(param.Definition.Name);
#elif RELEASE2015
                                        Parameter rParam = viewSheet.LookupParameter(param.Definition.Name);
#endif

                                        if (null != rParam)
                                        {
                                            if (rParam.StorageType == param.StorageType)
                                            {
                                                switch (param.StorageType)
                                                {
                                                    case StorageType.Double:
                                                        rParam.Set(param.AsDouble());
                                                        break;
                                                    case StorageType.Integer:
                                                        rParam.Set(param.AsInteger());
                                                        break;
                                                    case StorageType.String:
                                                        rParam.Set(param.AsString());
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                                transaction.Commit();
                            }
                            return viewSheet;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(previewMap.SourceViewProperties.SheetName+" Failed to create sheet.\n"+ex.Message, "Duplicate Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewSheet;
        }

        private static Viewport DuplicateViewPort(PreviewMap preview, ViewSheet copiedSheet, ViewDrafting copiedView)
        {
            Viewport recipientViewport = null;
            Viewport sourceViewport = null;
            try
            {
                if (null != preview.SourceViewProperties.SheetObj)
                {
                    sourceViewport = preview.SourceViewProperties.ViewportObj;

                    if (null != sourceViewport)
                    {
                        using (Transaction trans = new Transaction(preview.RecipientModelInfo.Doc, "Place Viewports"))
                        {
                            try
                            {
                                trans.Start("Create Viewport");
                                XYZ viewportLocation = preview.SourceViewProperties.ViewLocation;
                                string viewportTypeName = preview.SourceViewProperties.ViewportTypeName;

                                recipientViewport = Viewport.Create(preview.RecipientModelInfo.Doc, copiedSheet.Id, copiedView.Id, viewportLocation);
                                ElementId viewportTypeId = ElementId.InvalidElementId;
                                List<ElementId> elementTypeIds = recipientViewport.GetValidTypes().ToList();
                                foreach (ElementId typeId in elementTypeIds)
                                {
                                    ElementType eType = preview.RecipientModelInfo.Doc.GetElement(typeId) as ElementType;
                                    if (eType.Name == viewportTypeName)
                                    {
                                        viewportTypeId = typeId; break;
                                    }
                                }

                                if (viewportTypeId != ElementId.InvalidElementId)
                                {
                                    ElementId typeId = recipientViewport.ChangeTypeId(viewportTypeId);
                                }
                                trans.Commit();

                                if (null != recipientViewport)
                                {
                                    trans.Start("Wirte Parameter Values");
                                    foreach (Parameter param in sourceViewport.Parameters)
                                    {
                                        if (!param.IsReadOnly)
                                        {
#if RELEASE2014
                                            Parameter rParam = recipientViewport.get_Parameter(param.Definition.Name);
#elif RELEASE2015
                                            Parameter rParam = recipientViewport.LookupParameter(param.Definition.Name);
#endif

                                            if (null != rParam)
                                            {
                                                if (rParam.StorageType == param.StorageType)
                                                {
                                                    switch (param.StorageType)
                                                    {
                                                        case StorageType.Double:
                                                            rParam.Set(param.AsDouble());
                                                            break;
                                                        case StorageType.Integer:
                                                            rParam.Set(param.AsInteger());
                                                            break;
                                                        case StorageType.String:
                                                            rParam.Set(param.AsString());
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Failed to duplicate the viewport.\n" + ex.Message, "Duplicate Viewport", MessageBoxButton.OK, MessageBoxImage.Warning);
                                trans.RollBack();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to find the viewport.\n"+ex.Message, "Find Source Viewport", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            
            return recipientViewport;
        }

        private static PreviewMap UpdatePreviewMap(PreviewMap previewMap, ViewDrafting copiedView)
        {
            PreviewMap preview = previewMap;
            try
            {
                if (null != copiedView)
                {
                    ViewProperties vp = new ViewProperties(preview.RecipientModelInfo.Doc, copiedView);
                    vp.Status = LinkStatus.Linked;
                    vp.LinkedView = preview.SourceViewProperties;
                    preview.RecipientViewProperties = vp;

                    LinkInfo linkInfo = new LinkInfo();
                    linkInfo.ItemType = LinkItemType.DraftingView;
                    linkInfo.SourceModelName = preview.SourceModelInfo.DocTitle;
                    linkInfo.SourceModelPath = preview.SourceModelInfo.DocCentralPath;
                    linkInfo.SourceItemId = preview.SourceViewProperties.ViewId;
                    linkInfo.SourceItemName = preview.SourceViewProperties.ViewName;
                    linkInfo.DestModelName = preview.RecipientModelInfo.DocTitle;
                    linkInfo.DestModelPath = preview.RecipientModelInfo.DocCentralPath;
                    linkInfo.DestItemId = preview.RecipientViewProperties.ViewId;
                    linkInfo.DestItemName = preview.RecipientViewProperties.ViewName;
                    preview.ViewLinkInfo = linkInfo;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the information of preview map.\n"+ex.Message, "Update Preview Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return preview;
        }

        private static ElementId GetTitleBlockId(PreviewMap previewMap)
        {
            ElementId titleBlockTypeId = ElementId.InvalidElementId;
            try
            {
                 ElementId sourceSheetId = previewMap.SourceViewProperties.SheetObj.Id;
                 FilteredElementCollector collector = new FilteredElementCollector(previewMap.SourceModelInfo.Doc, sourceSheetId);
                 ICollection<ElementId> instanceIds = collector.OfCategory(BuiltInCategory.OST_TitleBlocks).ToElementIds().ToList();
                 if (instanceIds.Count > 0)
                 {
                     ElementId instanceId = instanceIds.First();
                     FamilyInstance instance = previewMap.SourceModelInfo.Doc.GetElement(instanceId) as FamilyInstance;
                     if (null != instance)
                     {
                         ElementId typeId = instance.GetTypeId();
                         ElementType elementType = previewMap.SourceModelInfo.Doc.GetElement(typeId) as ElementType;
                         if (null != elementType)
                         {
                             //check whether the same type of title block exists in the recipieent document or not
                             FilteredElementCollector rCollector = new FilteredElementCollector(previewMap.RecipientModelInfo.Doc);
                             List<Element> elements = rCollector.OfCategory(BuiltInCategory.OST_TitleBlocks).WhereElementIsElementType().ToElements().ToList();
                             var titleBlockTypes = from elem in elements where elem.Name == elementType.Name select elem;
                             if (titleBlockTypes.Count() > 0)
                             {
                                 titleBlockTypeId = titleBlockTypes.First().Id;
                                 return titleBlockTypeId;
                             }
                             else //duplicate element types
                             {
                                 CopyPasteOptions options = new CopyPasteOptions();
                                 options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());
                                 
                                 List<ElementId> typeIds = new List<ElementId>();
                                 typeIds.Add(typeId);

                                 ICollection<ElementId> copiedTypeIds;
                                 using (Transaction trans = new Transaction(previewMap.RecipientModelInfo.Doc, "Duplicate TitleBlock"))
                                 {
                                     trans.Start();
                                     copiedTypeIds = ElementTransformUtils.CopyElements(previewMap.SourceModelInfo.Doc, typeIds, previewMap.RecipientModelInfo.Doc, Transform.Identity, options);

                                     FailureHandlingOptions failureOptions = trans.GetFailureHandlingOptions();
                                     failureOptions.SetFailuresPreprocessor(new HidePasteDuplicateTypesPreprocessor());
                                     trans.Commit(failureOptions);
                                 }
                                 if (copiedTypeIds.Count > 0)
                                 {
                                     titleBlockTypeId = copiedTypeIds.First();
                                     return titleBlockTypeId;
                                 }
                             }
                         }
                     }
                 }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get title block Id.\n"+ex.Message, "GetTitleBockId", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return titleBlockTypeId;
        }

        private static PreviewMap TransferProperties(PreviewMap previewMap, ProgressBar progressBar)
        {

            return previewMap;
        }

        private static int DuplicateDetailingAcrossViews(View fromView, View toView)
        {
            // Collect view-specific elements in source view
            FilteredElementCollector collector = new FilteredElementCollector(fromView.Document, fromView.Id);

            // Skip elements which don't have a category.  In testing, this was
            // the revision table and the extents element, which should not be copied as they will
            // be automatically created for the copied view.
            collector.WherePasses(new ElementCategoryFilter(ElementId.InvalidElementId, true));

            // Get collection of elements to copy for CopyElements()
            ICollection<ElementId> toCopy = collector.ToElementIds();

            // Return value
            int numberOfCopiedElements = 0;

            if (toCopy.Count > 0)
            {
                using (Transaction t2 = new Transaction(toView.Document, "Duplicate view detailing"))
                {
                    t2.Start();
                    // Set handler to skip the duplicate types dialog
                    CopyPasteOptions options = new CopyPasteOptions();
                    options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

                    // Copy the elements using no transformation
                    ICollection<ElementId> copiedElements = ElementTransformUtils.CopyElements(fromView, toCopy, toView, Transform.Identity, options);
                    numberOfCopiedElements = copiedElements.Count;

                    // Set failure handler to skip any duplicate types warnings that are posted.
                    FailureHandlingOptions failureOptions = t2.GetFailureHandlingOptions();
                    failureOptions.SetFailuresPreprocessor(new HidePasteDuplicateTypesPreprocessor());
                    t2.Commit(failureOptions);
                }
            }

            return numberOfCopiedElements;
        }
    }


    /// <summary>
    /// A handler to accept duplicate types names created by the copy/paste operation.
    /// </summary>
    class HideAndAcceptDuplicateTypeNamesHandler : IDuplicateTypeNamesHandler
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


    /// <summary>
    /// A failure preprocessor to hide the warning about duplicate types being pasted.
    /// </summary>
    class HidePasteDuplicateTypesPreprocessor : IFailuresPreprocessor
    {
        #region IFailuresPreprocessor Members

        /// <summary>
        /// Implementation of the IFailuresPreprocessor.
        /// </summary>
        /// <param name="failuresAccessor"></param>
        /// <returns></returns>
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
            {
                FailureDefinitionId defId = failure.GetFailureDefinitionId();
                FailureDefinitionId cannotCopy = BuiltInFailures.CopyPasteFailures.CannotCopyDuplicates;
                
                // Delete any "Can't paste duplicate types.  Only non duplicate types will be pasted." warnings
                if (defId==cannotCopy)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
                else if (failure.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
            }

            // Handle any other errors interactively
            return FailureProcessingResult.Continue;
        }

        #endregion
    }
}
