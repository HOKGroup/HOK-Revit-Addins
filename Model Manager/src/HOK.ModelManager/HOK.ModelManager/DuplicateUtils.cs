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
        public static StringBuilder errorMessage = new StringBuilder();

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

                            ViewDrafting sourceView = previewMap.SourceViewProperties.ViewDraftingObj;
                            ICollection<ElementId> referenceCalloutIds = sourceView.GetReferenceCallouts();

                            ElementId viewId = new ElementId(preview.SourceViewProperties.LinkedView.ViewId);
                            using (Transaction trans = new Transaction(preview.RecipientModelInfo.Doc, "Delete Existing Contents"))
                            {
                                trans.Start();
                                try
                                {
                                    FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                                    failOpt.SetFailuresPreprocessor(new WarningMessagePreprocessor());
                                    trans.SetFailureHandlingOptions(failOpt);

                                    FilteredElementCollector collector = new FilteredElementCollector(preview.RecipientModelInfo.Doc, viewId);
                                    collector.WherePasses(new ElementCategoryFilter(ElementId.InvalidElementId, true));
                                    ICollection<ElementId> toDelete = collector.ToElementIds();
                                    ICollection<ElementId> deletedIds = preview.RecipientModelInfo.Doc.Delete(toDelete);
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    string message = ex.Message;
                                    trans.RollBack();
                                }
                            }

                            copiedView = preview.RecipientModelInfo.Doc.GetElement(viewId) as ViewDrafting;
                            if (null != copiedView)
                            {
                                int numOfCopied = DuplicateDetailingAcrossViews(preview.SourceViewProperties.ViewDraftingObj, copiedView);
                            }
                            if (referenceCalloutIds.Count > 0)
                            {
                                bool placedCallout = DuplicateReferenceCallouts(sourceView, copiedView);
                            }

                            if (createSheet)
                            {
                                
                                //delete existing viewport
                                ElementClassFilter filter = new ElementClassFilter(typeof(Viewport));
                                FilteredElementCollector collector = new FilteredElementCollector(preview.RecipientModelInfo.Doc);
                                List<Viewport> viewports=collector.WherePasses(filter).Cast<Viewport>().ToList<Viewport>();

                                var query = from element in viewports
                                            where element.ViewId == viewId
                                            select element;
                                if (query.Count() > 0)
                                {
                                    Viewport viewport = query.First();
                                    using (Transaction trans = new Transaction(preview.RecipientModelInfo.Doc, "Delete exisitng viewport"))
                                    {
                                        trans.Start();
                                        try
                                        {
                                            FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                                            failOpt.SetFailuresPreprocessor(new WarningMessagePreprocessor());
                                            trans.SetFailureHandlingOptions(failOpt);

                                            preview.RecipientModelInfo.Doc.Delete(viewport.Id);
                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            string message = ex.Message;
                                            trans.RollBack();
                                        }
                                    }
                                }
                                if (preview.SourceViewProperties.IsOnSheet && null != preview.SourceViewProperties.SheetObj)
                                {
                                    ViewSheet copiedSheet = DuplicateSheet(preview);
                                    if (null != copiedView && null != copiedSheet)
                                    {
                                        if (Viewport.CanAddViewToSheet(preview.RecipientModelInfo.Doc, copiedSheet.Id, copiedView.Id))
                                        {
                                            Viewport recipientViewport = DuplicateViewPort(preview, copiedSheet, copiedView);
                                        }
                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            if (preview.SourceViewProperties.IsOnSheet && createSheet)
                            {
                                ViewSheet copiedSheet = DuplicateSheet(preview);
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
                errorMessage.AppendLine(previewMap.SourceViewProperties.ViewName + ": errors in duplicating detailing across views\n"+ex.Message);
                //MessageBox.Show("Failed to duplicate drafiting views.\n" + ex.Message, "Duplicate Drafting Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return preview;
        }
        
        private static ViewDrafting DuplicateDraftingViews(PreviewMap previewMap)
        {
            ViewDrafting viewDrafting = null;
            try
            {
                Document fromDoc=previewMap.SourceModelInfo.Doc;
                Document toDoc = previewMap.RecipientModelInfo.Doc;

                ViewDrafting sourceView=previewMap.SourceViewProperties.ViewDraftingObj;
                ICollection<ElementId> referenceCalloutIds = sourceView.GetReferenceCallouts();
                
                CopyPasteOptions options = new CopyPasteOptions();
                options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

                ICollection<ElementId> copiedIds=null;
                using (Transaction transaction = new Transaction(toDoc, "Duplicate Draftingviews"))
                {
                    transaction.Start();
                    try
                    {
                        List<ElementId> viewIds = new List<ElementId>();
                        viewIds.Add(sourceView.Id);

                        //view-specific item
                        copiedIds = ElementTransformUtils.CopyElements(fromDoc, viewIds, toDoc, Transform.Identity, options);

                        FailureHandlingOptions failureOptions = transaction.GetFailureHandlingOptions();
                        failureOptions.SetFailuresPreprocessor(new HidePasteDuplicateTypesPreprocessor());
                        transaction.Commit(failureOptions);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        transaction.RollBack();
                    }
                }

                if (null!=copiedIds)
                {
                    ElementId viewId = copiedIds.First();
                    ViewDrafting copiedView = toDoc.GetElement(viewId) as ViewDrafting;
                    if (null != copiedView)
                    {
                        int numOfCopied = DuplicateDetailingAcrossViews(sourceView, copiedView);
                    }
                    if (referenceCalloutIds.Count > 0)
                    {
                        bool placedCallout = DuplicateReferenceCallouts(sourceView, copiedView);
                    }
                    viewDrafting = copiedView;
                }
            }
            catch (Exception ex)
            {
                errorMessage.AppendLine(previewMap.SourceViewProperties.ViewName+": errors in duplicating drafting views.\n"+ex.Message);
                //MessageBox.Show("Failed to duplicate drafintg views.\n" + ex.Message, "Duplicate DraftingViews", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewDrafting;
        }

        private static ViewSheet DuplicateSheet(PreviewMap previewMap)
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
                    //ElementId titleBlockTypeId = GetTitleBlockId(previewMap);
                    using (Transaction transaction = new Transaction(toDoc, "Create Sheet"))
                    {
                        transaction.Start();
                        try
                        {
                            FailureHandlingOptions failOpt = transaction.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningMessagePreprocessor());
                            transaction.SetFailureHandlingOptions(failOpt);

                            viewSheet = ViewSheet.Create(toDoc, ElementId.InvalidElementId);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                            transaction.RollBack();
                        }
                    }
                    if (null != viewSheet)
                    {
                        using (Transaction trans = new Transaction(toDoc, "Copy Title Block"))
                        {
                            FilteredElementCollector vCollector = new FilteredElementCollector(previewMap.SourceModelInfo.Doc, previewMap.SourceViewProperties.SheetObj.Id);
                            List<FamilyInstance> instances = vCollector.OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof(FamilyInstance)).ToElements().Cast<FamilyInstance>().ToList();
                            if (instances.Count > 0)
                            {
                                List<ElementId> copiedIds = new List<ElementId>();
                                CopyPasteOptions options = new CopyPasteOptions();
                                options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

                                foreach (FamilyInstance instance in instances)
                                {
                                    try
                                    {
                                        trans.Start();
                                        List<ElementId> titleBlock = new List<ElementId>();
                                        titleBlock.Add(instance.Id);

                                        ICollection<ElementId> copiedId = ElementTransformUtils.CopyElements(previewMap.SourceViewProperties.SheetObj, titleBlock, viewSheet, Transform.Identity, options);
                                        copiedIds.AddRange(copiedId);

                                        FailureHandlingOptions failureOptions = trans.GetFailureHandlingOptions();
                                        failureOptions.SetFailuresPreprocessor(new HidePasteDuplicateTypesPreprocessor());
                                        trans.Commit(failureOptions);

                                        if (null != copiedId)
                                        {
                                            trans.Start();
                                            LocationPoint slocation = instance.Location as LocationPoint;
                                            XYZ sourcePoint = slocation.Point;

                                            FamilyInstance copiedTitleBlock = toDoc.GetElement(copiedId.First()) as FamilyInstance;
                                            LocationPoint tlocation = copiedTitleBlock.Location as LocationPoint;
                                            XYZ targetPoint = tlocation.Point;

                                            XYZ moveVector = sourcePoint - targetPoint;
                                            ElementTransformUtils.MoveElement(toDoc, copiedTitleBlock.Id, moveVector);
                                            trans.Commit(failureOptions);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string message = ex.Message;
                                        trans.RollBack();
                                    }
                                }
                            }
                        }

                        using (Transaction transaction = new Transaction(toDoc, "Write Sheet Parameter"))
                        {
                            transaction.Start();
                            FailureHandlingOptions failOpt = transaction.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningMessagePreprocessor());
                            transaction.SetFailureHandlingOptions(failOpt);

                            try
                            {
                                foreach (Parameter param in previewMap.SourceViewProperties.SheetObj.Parameters)
                                {
                                    if (!param.IsReadOnly)
                                    {
#if RELEASE2014
                                    Parameter rParam = viewSheet.get_Parameter(param.Definition.Name);
#elif RELEASE2015 || RELEASE2016
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
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                                transaction.RollBack();
                            }
                        }
                        return viewSheet;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage.AppendLine(previewMap.SourceViewProperties.SheetName+": failed to create sheet.\n"+ex.Message);
                //MessageBox.Show(previewMap.SourceViewProperties.SheetName + " Failed to create sheet.\n" + ex.Message, "Duplicate Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                                if (null != recipientViewport)
                                {
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
                                    else
                                    {
                                        CopyPasteOptions options = new CopyPasteOptions();
                                        options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

                                        List<ElementId> typeIds = new List<ElementId>();
                                        typeIds.Add(sourceViewport.GetTypeId());

                                        ICollection<ElementId> copiedTypeIds = ElementTransformUtils.CopyElements(preview.SourceModelInfo.Doc, typeIds, preview.RecipientModelInfo.Doc, Transform.Identity, options);
                                        if (copiedTypeIds.Count > 0)
                                        {
                                            viewportTypeId = copiedTypeIds.First();
                                            ElementId typeId = recipientViewport.ChangeTypeId(viewportTypeId);
                                        }
                                    }
                                    FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                                    failOpt.SetFailuresPreprocessor(new WarningMessagePreprocessor());
                                    trans.Commit(failOpt);

                                    if (null != recipientViewport)
                                    {
                                        trans.Start("Wirte Parameter Values");
                                        foreach (Parameter param in sourceViewport.Parameters)
                                        {
                                            if (!param.IsReadOnly)
                                            {
#if RELEASE2014
                                                Parameter rParam = recipientViewport.get_Parameter(param.Definition.Name);
#elif RELEASE2015 || RELEASE2016
                                            Parameter rParam = recipientViewport.LookupParameter(param.Definition.Name);
#endif

                                                if (null != rParam)
                                                {
                                                    if (!rParam.IsReadOnly)
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
                                        }

                                        FailureHandlingOptions failureOptions = trans.GetFailureHandlingOptions();
                                        failureOptions.SetFailuresPreprocessor(new HideSameParameterValuePreprocessor());
                                        trans.Commit(failureOptions);
                                    }
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
                errorMessage.AppendLine("Failed to duplicate viewports.\n"+ex.Message);
                //MessageBox.Show("Failed to find the viewport.\n"+ex.Message, "Find Source Viewport", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    preview.SourceViewProperties.Status = LinkStatus.Linked;
                    preview.SourceViewProperties.LinkedView = preview.RecipientViewProperties;

                    LinkInfo linkInfo = new LinkInfo();
                    linkInfo.ItemType = LinkItemType.DraftingView;
                    linkInfo.SourceModelName = preview.SourceModelInfo.DocTitle;
                    linkInfo.SourceModelPath = preview.SourceModelInfo.DocCentralPath;
                    linkInfo.SourceModelId = preview.SourceModelInfo.RevitDocumentID;
                    linkInfo.SourceItemId = preview.SourceViewProperties.ViewId;
                    linkInfo.SourceItemName = preview.SourceViewProperties.ViewName;
                    linkInfo.DestModelName = preview.RecipientModelInfo.DocTitle;
                    linkInfo.DestModelPath = preview.RecipientModelInfo.DocCentralPath;
                    linkInfo.DestItemId = preview.RecipientViewProperties.ViewId;
                    linkInfo.DestItemName = preview.RecipientViewProperties.ViewName;
                    linkInfo.LinkModified = DateTime.Now.ToString("G");
                    linkInfo.LinkModifiedBy = Environment.UserName;
                    preview.ViewLinkInfo = linkInfo;

                }
            }
            catch (Exception ex)
            {
                errorMessage.AppendLine("Failed to udpate the information of preview map.\n"+ex.Message);
                //MessageBox.Show("Failed to update the information of preview map.\n"+ex.Message, "Update Preview Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return preview;
        }


        private static int DuplicateDetailingAcrossViews(View fromView, View toView)
        {
            int numberOfCopiedElements = 0;
            try
            {
                List<ElementId> elementIdsToExclude = new List<ElementId>();

                ICollection<ElementId> referenceCalloutIds = fromView.GetReferenceCallouts();
                elementIdsToExclude.AddRange(referenceCalloutIds);
                ICollection<ElementId> referenceElevationIds = fromView.GetReferenceElevations();
                elementIdsToExclude.AddRange(referenceElevationIds);
                ICollection<ElementId> referenceSectionIds = fromView.GetReferenceSections();
                elementIdsToExclude.AddRange(referenceSectionIds);

                FilteredElementCollector collector = new FilteredElementCollector(fromView.Document, fromView.Id);
                if (elementIdsToExclude.Count > 0)
                {
                    collector.Excluding(elementIdsToExclude);
                }
                collector.WherePasses(new ElementCategoryFilter(ElementId.InvalidElementId, true));

                ICollection<ElementId> toCopy = collector.ToElementIds();

                CopyPasteOptions options = new CopyPasteOptions();
                options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());
                if (toCopy.Count > 0)
                {
                    using (Transaction t2 = new Transaction(toView.Document, "Duplicate view detailing"))
                    {
                        t2.Start();
                        try
                        {
                            ICollection<ElementId> copiedElements = ElementTransformUtils.CopyElements(fromView, toCopy, toView, Transform.Identity, options);
                            numberOfCopiedElements = copiedElements.Count;

                            FailureHandlingOptions failureOptions = t2.GetFailureHandlingOptions();
                            failureOptions.SetFailuresPreprocessor(new HidePasteDuplicateTypesPreprocessor());
                            t2.Commit(failureOptions);
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                            t2.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Failed to duplicate detialing across views.\n"+ex.Message, "Duplicate Detailing Across Views", MessageBoxButton.OK, MessageBoxImage.Warning); 
                errorMessage.AppendLine(toView.Name + ": errors in duplicating detailing across views.\n"+ex.Message);
            }
            return numberOfCopiedElements;
        }

        private static bool DuplicateReferenceCallouts(View fromView, View toView)
        {
            bool result = false;
            try
            {
                Document fromDoc = fromView.Document;
                Document toDoc = toView.Document;

                CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
                copyPasteOptions.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

                ICollection<ElementId> referenceCalloutIds = fromView.GetReferenceCallouts();
                if (referenceCalloutIds.Count > 0)
                {
                    foreach (ElementId eId in referenceCalloutIds)
                    {
                        XYZ firstPoint = null;
                        XYZ secondPoint = null;
                        bool cornerFound = GetCalloutCornerPoints(fromDoc, eId, out firstPoint, out secondPoint);

                        Element callout = fromDoc.GetElement(eId);
                        if(null!=callout)
                        {
                            using (Transaction trans = new Transaction(toDoc, "Duplicate Reference Callout"))
                            {
                                try
                                {
                                    trans.Start();

                                    toDoc.Regenerate();
                                    FilteredElementCollector collector = new FilteredElementCollector(toDoc);
                                    List<ViewDrafting> views = collector.OfClass(typeof(ViewDrafting)).ToElements().Cast<ViewDrafting>().ToList();
                                    var viewFound = from view in views where view.Name == callout.Name select view;
                                    if (viewFound.Count() > 0)
                                    {
                                        ViewDrafting referenceView = viewFound.First();
                                        ViewSection.CreateReferenceCallout(toDoc, toView.Id, referenceView.Id, firstPoint, secondPoint);
                                    }
                                    
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    string message = ex.Message;
                                    trans.RollBack();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                //MessageBox.Show("Failed to duplicate reference callouts.\n"+ex.Message, "Duplicate Reference Callouts", MessageBoxButton.OK, MessageBoxImage.Warning);
                errorMessage.AppendLine(toView.Name + ": errors in duplicating reference callouts");
            }
            return result;
        }

        private static bool GetCalloutCornerPoints(Document doc, ElementId calloutId, out XYZ firstPoint, out XYZ secondPoint)
        {
            bool result = false;
            firstPoint = new XYZ(0, 0, 0);
            secondPoint = new XYZ(0, 0, 0);
            try
            {
                double minX = double.MaxValue;
                double minY = double.MaxValue;
                double minZ = double.MaxValue;
                double maxX = double.MinValue;
                double maxY = double.MinValue;
                double maxZ = double.MinValue;
               
                ViewCropRegionShapeManager cropRegion = View.GetCropRegionShapeManagerForReferenceCallout(doc, calloutId);
                CurveLoop curveLoop = cropRegion.GetCropRegionShape();
                foreach (Curve curve in curveLoop)
                {
                    XYZ point = curve.GetEndPoint(0);
                    if (point.X < minX) { minX = point.X; }
                    if (point.Y < minY) { minY = point.Y; }
                    if (point.Z < minZ) { minZ = point.Z; }
                    if (point.X > maxX) { maxX = point.X; }
                    if (point.Y > maxY) { maxY = point.Y; }
                    if (point.Z > maxZ) { maxZ = point.Z; }
                }

                if (curveLoop.Count() > 0)
                {
                    firstPoint = new XYZ(minX, minY, minZ);
                    secondPoint = new XYZ(maxX, maxY, maxZ);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                errorMessage.AppendLine("Failed to get corner points of callout.\n" + ex.Message);
                //MessageBox.Show("Failed to get corner points of callout\n" + ex.Message, "Get Corner Points of Callout", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
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

    class HideSameParameterValuePreprocessor : IFailuresPreprocessor
    {

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
            {
                FailureDefinitionId defId = failure.GetFailureDefinitionId();
                FailureDefinitionId nameInUse = BuiltInFailures.GeneralFailures.NameNotUnique;//same detail number

                // Delete any "Can't paste duplicate types.  Only non duplicate types will be pasted." warnings
                if (defId == nameInUse)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
                else if (failure.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
            }
            return FailureProcessingResult.Continue;
        }
    }

    class WarningMessagePreprocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
            {
                if (failure.GetSeverity() == FailureSeverity.Warning)
                {
                    string elementIds = "";
                    if(null != failure.GetFailingElementIds())
                    {
                        foreach (ElementId eId in failure.GetFailingElementIds())
                        {
                            elementIds += " [" + eId + "] ";
                        }
                    }

                    string description = failure.GetDescriptionText();

                    DuplicateUtils.errorMessage.AppendLine("Warnings - Element Ids:" + elementIds);
                    DuplicateUtils.errorMessage.AppendLine(description);
                    failuresAccessor.DeleteWarning(failure);
                }
            }
            return FailureProcessingResult.Continue;
        }
    }
}
