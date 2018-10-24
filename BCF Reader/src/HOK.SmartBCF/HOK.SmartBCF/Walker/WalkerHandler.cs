using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.SmartBCF.GoogleUtils;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.Walker
{
    public class WalkerHandler : IExternalEventHandler
    {
        private UIApplication m_app;
        private Dictionary<int /*categoryId*/, BuiltInCategory> catDictionary = new Dictionary<int, BuiltInCategory>();
        private List<string> categoryNames = new List<string>();
        private ProgressWindow progressWindow;

        public Document ActiveDoc { get; set; }
        public Request Request { get; } = new Request();
        public View3D ActiveView { get; set; }
        public FolderHolders GoogleFolders { get; set; }
        public Dictionary<string, LinkedBcfFileInfo> BCFFileDictionary { get; set; } = new Dictionary<string, LinkedBcfFileInfo>();
        public Dictionary<string, Dictionary<string, IssueEntry>> BCFDictionary { get; set; } = new Dictionary<string, Dictionary<string, IssueEntry>>();
        public List<CategoryInfo> CategoryInfoList { get; set; } = new List<CategoryInfo>();

        public ColorSchemeInfo SchemeInfo { get; set; }
        public WalkerWindow WalkerWindow { get; set; } = null;
        public ElementProperties CurrentElement { get; set; } = null;
        public IssueEntry SelectedIssue { get; set; } = null;
        public Comment SelectedComment { get; set; } = null;
        public string BCFProjectId { get; set; } = "";
        public string BCFColorSchemeId { get; set; } = "";
        public string CategorySheetId { get; set; } = "";
        public bool IsHighlightOn { get; set; } = false;
        public bool IsIsolateOn { get; set; } = false;
        public bool IsSectionBoxOn { get; set; } = false;
        public bool IsProjectIdChanged { get; set; } = false;

        public WalkerHandler(UIApplication uiapp)
        {
            try
            {
                m_app = uiapp;
                ActiveDoc = uiapp.ActiveUIDocument.Document;
               
                var view3d = ActiveDoc.ActiveView as View3D;
                if (view3d != null)
                {
                    ActiveView = view3d;
                }
                else
                {
                    var v = new FilteredElementCollector(ActiveDoc)
                        .OfClass(typeof(View3D))
                        .WhereElementIsNotElementType()
                        .Cast<View3D>()
                        .FirstOrDefault(x => !x.IsTemplate && !x.IsPerspective && x.Name.Contains("{3D}"));

                    if (v != null)
                    {
                        using (var trans = new Transaction(ActiveDoc, "Open 3D View"))
                        {
                            try
                            {
                                trans.Start();
                                uiapp.ActiveUIDocument.ActiveView = ActiveView;
                                trans.Commit();

                                // (Konrad) Set active view.
                                ActiveView = v;
                            }
                            catch (Exception e)
                            {
                                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                                trans.RollBack();
                            }
                        }
                    }
                }

                BCFProjectId = ParameterUtil.GetBCFProjectId(m_app);
                if (!string.IsNullOrEmpty(BCFProjectId))
                {
                    GoogleFolders = FileManager.FindGoogleFolders(BCFProjectId);
                    if (null != GoogleFolders.ColorSheet)
                    {
                        BCFColorSchemeId = GoogleFolders.ColorSheet.Id;
                    }
                    if (null != GoogleFolders.CategorySheet)
                    {
                        CategorySheetId = GoogleFolders.CategorySheet.Id;
                    }
                    if (!string.IsNullOrEmpty(BCFColorSchemeId) && !string.IsNullOrEmpty(CategorySheetId))
                    {
                        SchemeInfo = FileManager.ReadColorSchemes(BCFColorSchemeId, CategorySheetId, false);
                    }

                    BCFFileDictionary = DataStorageUtil.ReadLinkedBCFFileInfo(ActiveDoc, BCFProjectId);
                    BCFDictionary = GetBCFDictionary(ActiveDoc);

                    var bltCategories = catDictionary.Values.ToList();
                    var unused = ParameterUtil.CreateBCFParameters(m_app, bltCategories);

                    foreach (var catName in categoryNames)
                    {
                        var catInfo = new CategoryInfo(catName, true);
                        CategoryInfoList.Add(catInfo);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a BCF Project Id in Project Information.", "Empty BCF Project Id", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to initialize External Event handler.\n"+ex.Message, "External Event Handler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Dictionary<string/*spreadsheetId*/, Dictionary<string/*issueId*/, IssueEntry>> GetBCFDictionary(Document doc)
        {
            var dictionary = new Dictionary<string, Dictionary<string, IssueEntry>>();
            try
            {
                AbortFlag.SetAbortFlag(false);
                progressWindow = new ProgressWindow("Loading BCF issues and images...");
                progressWindow.Show();

                var markupIds = BCFFileDictionary.Keys.ToList();
                foreach (var markupId in markupIds)
                {
                    var bcfFileInfo = BCFFileDictionary[markupId];
                    if (null != FileManager.FindFileById(bcfFileInfo.MarkupFileId) && null != FileManager.FindFileById(bcfFileInfo.ViewpointFileId))
                    {
                        var issueDictionary = GetBCFIssueInfo(doc, bcfFileInfo);
                        if (AbortFlag.GetAbortFlag()) { return new Dictionary<string, Dictionary<string, IssueEntry>>(); }
                        if (!dictionary.ContainsKey(markupId) && issueDictionary.Count > 0)
                        {
                            dictionary.Add(markupId, issueDictionary);
                        }
                    }
                    else
                    {
                        BCFFileDictionary.Remove(markupId);
                    }
                }

                if (!string.IsNullOrEmpty(CategorySheetId))
                {
                    var stream = BCFParser.CreateCategoryStream(categoryNames);
                    if (null != stream)
                    {
                        var file = FileManager.UpdateSpreadsheet(stream, CategorySheetId, BCFProjectId);
                    }
                }

                if (progressWindow.IsActive) { progressWindow.Close(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get BCF dictionary.\n"+ex.Message, "Get BCF Dictionary", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return dictionary;
        }

        private Dictionary<string, IssueEntry> GetBCFIssueInfo(Document doc, LinkedBcfFileInfo bcfFileInfo)
        {
            var issueDictionary = new Dictionary<string, IssueEntry>();
            try
            {
                issueDictionary = FileManager.ReadIssues(bcfFileInfo);

                var issueIds = issueDictionary.Keys.ToList();
                progressWindow.SetMaximum(issueIds.Count);

                double progressValue = 0;
                foreach (var issueId in issueIds)
                {
                    if (AbortFlag.GetAbortFlag()) { progressWindow.Close();  return new Dictionary<string, IssueEntry>(); }

                    progressValue++;
                    progressWindow.SetProgressValue(progressValue);

                    var issueEntry = issueDictionary[issueId];
                    var elementIds = issueEntry.ElementDictionary.Keys.ToList();
                    foreach (var elementId in elementIds)
                    {
                        var property = issueEntry.ElementDictionary[elementId];

                        var element = ActiveDoc.GetElement(new ElementId(elementId));
                        if (null != element)
                        {
                            if (null != element.Category)
                            {
                                if (!categoryNames.Contains(element.Category.Name))
                                {
                                    categoryNames.Add(element.Category.Name);
                                }

                                if (element.Category.AllowsBoundParameters)
                                {
                                    var categoryId = element.Category.Id.IntegerValue;
                                    if (!catDictionary.ContainsKey(categoryId))
                                    {
                                        var bltCategory = (BuiltInCategory)categoryId;
                                        if (bltCategory != BuiltInCategory.INVALID)
                                        {
                                            catDictionary.Add(categoryId, bltCategory);
                                        }
                                    }
                                }
                            }

                            var ep = new ElementProperties(element);
                            ep.IssueId = property.IssueId;
                            ep.Action = property.Action;
                            ep.ResponsibleParty = property.ResponsibleParty;
                            ep.CellEntries = property.CellEntries;

                            issueDictionary[issueId].ElementDictionary.Remove(elementId);
                            issueDictionary[issueId].ElementDictionary.Add(elementId, ep);
                        }
                    }
                    issueDictionary[issueId].NumElements = issueDictionary[issueId].ElementDictionary.Count;
                    if (null == issueDictionary[issueId].Snapshot)
                    {
                        if (bcfFileInfo.SharedLinkId == BCFProjectId && null != GoogleFolders)
                        {
                            issueDictionary[issueId].Snapshot = FileManager.DownloadImage(issueId, GoogleFolders.ActiveImgFolder.Id);
                        }
                        else if (bcfFileInfo.SharedLinkId == BCFProjectId)
                        {
                            GoogleFolders = FileManager.FindGoogleFolders(BCFProjectId);
                            if (null != GoogleFolders)
                            {
                                issueDictionary[issueId].Snapshot = FileManager.DownloadImage(issueId, GoogleFolders.ActiveImgFolder.Id);
                            }
                        }
                        else
                        {
                            var tempFolders = FileManager.FindGoogleFolders(bcfFileInfo.SharedLinkId);
                            if (null != tempFolders)
                            {
                                issueDictionary[issueId].Snapshot = FileManager.DownloadImage(issueId, tempFolders.ActiveImgFolder.Id);
                            }
                        }
                    }
                }

                if (BCFDictionary.ContainsKey(bcfFileInfo.MarkupFileId))
                {
                    BCFDictionary.Remove(bcfFileInfo.MarkupFileId);
                }
                BCFDictionary.Add(bcfFileInfo.MarkupFileId, issueDictionary);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add issue items into BCF dictionary.\n"+ex.Message, "Add BCF to Dictionary", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return issueDictionary;
        }

        public void Execute(UIApplication app)
        {
            try
            {
                ActiveDoc = app.ActiveUIDocument.Document;

                switch (Request.Take())
                {
                    case RequestId.None:
                        return;
                    case RequestId.ReadLinkedFileInfo:
                        break;
                    case RequestId.UpdateLinkedFileInfo:
                        var updated = DataStorageUtil.UpdateLinkedBCFFileInfo(ActiveDoc, BCFFileDictionary);
                        var dictionary = new Dictionary<string, Dictionary<string, IssueEntry>>();
                        
                        var numCat = categoryNames.Count;
                        AbortFlag.SetAbortFlag(false);
                        progressWindow = new ProgressWindow("Loading BCF issues and images...");
                        progressWindow.Show();

                        foreach (var markupId in BCFFileDictionary.Keys)
                        {
                            var bcfInfo = BCFFileDictionary[markupId];
                            if (BCFDictionary.ContainsKey(markupId))
                            {
                                dictionary.Add(markupId, BCFDictionary[markupId]);
                            }
                            else
                            {
                                var issueDictionary = GetBCFIssueInfo(ActiveDoc, bcfInfo);
                                if (issueDictionary.Count > 0)
                                {
                                    dictionary.Add(markupId, issueDictionary);
                                }
                            }
                        }
                        if (progressWindow.IsActive) { progressWindow.Close(); }
                        
                        BCFDictionary = dictionary;

                        if (numCat != categoryNames.Count)
                        {
                            var stream = BCFParser.CreateCategoryStream(categoryNames);
                            if (null != stream)
                            {
                                var file = FileManager.UpdateSpreadsheet(stream, CategorySheetId, BCFProjectId);
                            }

                            var bltCategories = catDictionary.Values.ToList();
                            var parameterCreated = ParameterUtil.CreateBCFParameters(m_app, bltCategories);

                            foreach (var catName in categoryNames)
                            {
                                var catFound = from category in CategoryInfoList where category.CategoryName == catName select category;
                                if (catFound.Count() == 0)
                                {
                                    var catInfo = new CategoryInfo(catName, true);
                                    CategoryInfoList.Add(catInfo);
                                }
                            }
                        }

                        if (null != WalkerWindow)
                        {
                            WalkerWindow.BCFFileDictionary = BCFFileDictionary;
                            WalkerWindow.BCFDictionary = BCFDictionary;
                            WalkerWindow.CategoryInfoList = CategoryInfoList;
                            WalkerWindow.CurrentIndex = 0;
                            WalkerWindow.DisplayLinkedBCF();
                        }

                        var updatedId = ParameterUtil.SetBCFProjectId(ActiveDoc, BCFProjectId);
                        SchemeInfo = FileManager.ReadColorSchemes(BCFColorSchemeId, CategorySheetId, false);
                        
                        if (null != WalkerWindow)
                        {
                            WalkerWindow.SchemeInfo = SchemeInfo;
                            WalkerWindow.DisplayColorscheme(SchemeInfo);
                        }

                        break;
                    case RequestId.ReadProjectId:
                        BCFProjectId = ParameterUtil.GetBCFProjectId(app);
                        break;
                    case RequestId.UpdateProjectId:
                        var updatedProjectId = ParameterUtil.SetBCFProjectId(ActiveDoc, BCFProjectId);
                        break;
                    case RequestId.ReadBCFParameterInfo:
                        break;
                    case RequestId.UpdateBCFParameterInfo:
                        var updatedParameters = ParameterUtil.UpdateBCFParameters(ActiveDoc, CurrentElement, SelectedIssue, SelectedComment);
                        break;
                    case RequestId.UpdateAction:
                        var actionUpdated = ParameterUtil.UpdateBCFParameter(ActiveDoc, CurrentElement, BCFParameters.BCF_Action, CurrentElement.Action);
                        break;
                    case RequestId.UpdateResponsibility:
                        var responsibilityUpdated = ParameterUtil.UpdateBCFParameter(ActiveDoc, CurrentElement, BCFParameters.BCF_Responsibility, CurrentElement.ResponsibleParty);
                        break;
                    case RequestId.CreateIssue:
                        break;
                    case RequestId.UpdateViews:
                        IsolateElement(IsIsolateOn, ActiveDoc);
                        CreateSectionBox(IsSectionBoxOn, ActiveDoc);
                        HighlightElement(IsHighlightOn, ActiveDoc);
                        break;
                    case RequestId.UpdateParameterByComment:
                        UpdateParameters(ActiveDoc);
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute an external event.\n"+ex.Message, "Execute Event", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return;
        }

        //execute or cancel
        private void HighlightElement(bool execute, Document doc)
        {
            try
            {
                var uidoc=new UIDocument(doc);
                using (var trans = new Transaction(doc))
                {
                    trans.Start("Highlight");
                    try
                    {
                        var element = ActiveDoc.GetElement(new ElementId(CurrentElement.ElementId));
                        if (null != element)
                        {
                            if (execute)
                            {
#if RELEASE2013||RELEASE2014
                            SelElementSet selElements = SelElementSet.Create();
                            selElements.Add(element);
                            uidoc.Selection.Elements = selElements;
                            uidoc.ShowElements(element);
#elif RELEASE2015 || RELEASE2016
                                var selectedIds = new List<ElementId>();
                                selectedIds.Add(element.Id);
                                uidoc.Selection.SetElementIds(selectedIds);
                                uidoc.ShowElements(element.Id);
#endif
                            }
                            else
                            {
#if RELEASE2013||RELEASE2014
                            SelElementSet selElementSet = SelElementSet.Create();
                            uidoc.Selection.Elements = selElementSet;
#elif RELEASE2015 || RELEASE2016

                                uidoc.Selection.SetElementIds(new List<ElementId>());
#endif
                            }
                            uidoc.RefreshActiveView();
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(CurrentElement.ElementName+": Failed to highlight elements.\n"+ex.Message, "Highlight Element", MessageBoxButton.OK, MessageBoxImage.Warning);
                        trans.RollBack();
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to highlight elements.\n"+ex.Message, "Highlight Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void IsolateElement(bool execute, Document doc)
        {
            try
            {
                var uidoc = new UIDocument(doc);
                var element = ActiveDoc.GetElement(new ElementId(CurrentElement.ElementId));
                
                if (null != element && null!=ActiveView)
                {
                    if (ActiveView.IsInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate))
                    {
                        using (var trans = new Transaction(doc))
                        {
                            trans.Start("Reset View");
                            try
                            {
                                ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                                uidoc.RefreshActiveView();
                                trans.Commit();
                            }
                            catch
                            {
                                trans.RollBack();
                            }
                        }
                    }
                    if (execute)
                    {
                        using (var trans = new Transaction(doc))
                        {
                            trans.Start("Isolate");
                            try
                            {
                                var elementIds = new List<ElementId>();
                                elementIds.Add(element.Id);
                                ActiveView.IsolateElementsTemporary(elementIds);
                                uidoc.RefreshActiveView();
                                trans.Commit();
                            }
                            catch { trans.RollBack(); }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to highlight elements.\n" + ex.Message, "Highlight Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateSectionBox(bool execute, Document doc)
        {
            try
            {
                var element = ActiveDoc.GetElement(new ElementId(CurrentElement.ElementId));
                if (null != element)
                {
                    using (var trans = new Transaction(doc))
                    {
                        trans.Start("Section Box");
                        try
                        {
                            if (execute)
                            {
                                var boundingBox = element.get_BoundingBox(null);
                                if (null != boundingBox)
                                {
                                    var minXYZ = new XYZ(boundingBox.Min.X - 3, boundingBox.Min.Y - 3, boundingBox.Min.Z - 3);
                                    var maxXYZ = new XYZ(boundingBox.Max.X + 3, boundingBox.Max.Y + 3, boundingBox.Max.Z + 3);
                                    var offsetBoundingBox = new BoundingBoxXYZ();
                                    offsetBoundingBox.Min = minXYZ;
                                    offsetBoundingBox.Max = maxXYZ;
#if RELEASE2013
                                    activeView.SectionBox = offsetBoundingBox;
#else
                                    ActiveView.SetSectionBox(offsetBoundingBox);
                                    ActiveView.GetSectionBox().Enabled = true;
#endif
                                }
                            }
                            else
                            {
#if RELEASE2013
                                activeView.SectionBox = null;                              
#else
                                var parameter = ActiveView.get_Parameter(BuiltInParameter.VIEWER_MODEL_CLIP_BOX_ACTIVE);
                                if (null != parameter)
                                {
                                    parameter.Set(0);
                                }
                                ActiveView.GetSectionBox().Enabled = false;
#endif
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a section box around the selected element.\n"+ex.Message, "Create Section Box", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateParameters(Document doc)
        {
            try
            {
                if (null != SelectedIssue && null != SelectedComment)
                {
                    foreach (var elementId in SelectedIssue.ElementDictionary.Keys)
                    {
                        var ep = SelectedIssue.ElementDictionary[elementId];
                        var parameterUpdated = ParameterUtil.UpdateBCFParameters(ActiveDoc, ep, SelectedIssue, SelectedComment);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update parameters by selected issues.\n"+ex.Message, "Update Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string GetName()
        {
            return "BCF Walker Event Handler";
        }
    }

    public enum RequestId : int
    {
        None = 0,

        ReadLinkedFileInfo = 1,

        UpdateLinkedFileInfo=2, 

        ReadProjectId=3,

        UpdateProjectId=4,

        ReadBCFParameterInfo=5,

        UpdateBCFParameterInfo=6,

        UpdateAction=7,

        UpdateResponsibility=8,

        CreateIssue=9,

        UpdateViews=10,

        UpdateParameterByComment=11,

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
}
