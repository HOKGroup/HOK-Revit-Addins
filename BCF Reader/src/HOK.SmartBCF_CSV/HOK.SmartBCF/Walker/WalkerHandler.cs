using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HOK.SmartBCF.GoogleUtils;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.Walker
{
    public class WalkerHandler : IExternalEventHandler
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private View3D activeView = null;
        private Request m_request = new Request();
        private FolderHolders googleFolders = null;
        private Dictionary<string/*spreadsheetId*/, LinkedBcfFileInfo> bcfFileDictionary = new Dictionary<string, LinkedBcfFileInfo>();
        private Dictionary<string/*spreadsheetId*/, Dictionary<string/*issueId*/, IssueEntry>> bcfDictionary = new Dictionary<string, Dictionary<string, IssueEntry>>();
        private Dictionary<int /*categoryId*/, BuiltInCategory> catDictionary = new Dictionary<int, BuiltInCategory>();
        private List<string> categoryNames = new List<string>();
        private List<CategoryInfo> categoryInfoList = new List<CategoryInfo>();
        
        private ColorSchemeInfo schemeInfo =null;
        private WalkerWindow walkerWindow = null;
        private ElementProperties currentElement = null;
        private IssueEntry selectedIssue = null;
        private Comment selectedComment = null;
        private string bcfProjectId = "";
        private string bcfColorSchemeId = "";
        private string categorySheetId = "";
        private bool isHighlightOn = false;
        private bool isIsolateOn = false;
        private bool isSectionBoxOn = false;
        private bool isProjectIdChanged = false;

        private ProgressWindow progressWindow;

        public Document ActiveDoc { get { return m_doc; } set { m_doc = value; } }
        public Request Request { get { return m_request; } }
        public View3D ActiveView { get { return activeView; } set { activeView = value; } }
        public FolderHolders GoogleFolders { get { return googleFolders; } set { googleFolders = value; } }
        public Dictionary<string, LinkedBcfFileInfo> BCFFileDictionary { get { return bcfFileDictionary; } set { bcfFileDictionary = value; } }
        public Dictionary<string, Dictionary<string, IssueEntry>> BCFDictionary { get { return bcfDictionary; } set { bcfDictionary = value; } }
        public List<CategoryInfo> CategoryInfoList { get { return categoryInfoList; } set { categoryInfoList = value; } }

        public ColorSchemeInfo SchemeInfo { get { return schemeInfo; } set { schemeInfo = value; } }
        public WalkerWindow WalkerWindow { get { return walkerWindow; } set { walkerWindow = value; } }
        public ElementProperties CurrentElement { get { return currentElement; } set { currentElement = value; } }
        public IssueEntry SelectedIssue { get { return selectedIssue; } set { selectedIssue = value; } }
        public Comment SelectedComment { get { return selectedComment; } set { selectedComment = value; } }
        public string BCFProjectId { get { return bcfProjectId; } set { bcfProjectId = value; } }
        public string BCFColorSchemeId { get { return bcfColorSchemeId; } set { bcfColorSchemeId = value; } }
        public string CategorySheetId { get { return categorySheetId; } set { categorySheetId = value; } }
        public bool IsHighlightOn { get { return isHighlightOn; } set { isHighlightOn = value; } }
        public bool IsIsolateOn { get { return isIsolateOn; } set { isIsolateOn = value; } }
        public bool IsSectionBoxOn { get { return isSectionBoxOn; } set { isSectionBoxOn = value; } }
        public bool IsProjectIdChanged { get { return isProjectIdChanged; } set { isProjectIdChanged = value; } }

        public WalkerHandler(UIApplication uiapp)
        {
            try
            {
                m_app = uiapp;
                m_doc = uiapp.ActiveUIDocument.Document;
               
                View3D view3d = m_doc.ActiveView as View3D;
                if (null != view3d)
                {
                    activeView = view3d;
                }
                else
                {
                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<View3D> view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                    var viewfound = from view in view3ds where view.IsTemplate== false && view.IsPerspective == false && view.ViewName =="{3D}" select view;
                    if (viewfound.Count() > 0)
                    {
                        activeView = viewfound.First();
                        using (Transaction trans = new Transaction(m_doc, "Open 3D View"))
                        {
                            try
                            {
                                trans.Start();
                                uiapp.ActiveUIDocument.ActiveView = activeView;
                                trans.Commit();
                            }
                            catch
                            {
                                trans.RollBack();
                            }
                        }
                    }
                }

                bcfProjectId = ParameterUtil.GetBCFProjectId(m_app);
                if (!string.IsNullOrEmpty(bcfProjectId))
                {
                    googleFolders = FileManager.FindGoogleFolders(bcfProjectId);
                    if (null != googleFolders.ColorSheet)
                    {
                        bcfColorSchemeId = googleFolders.ColorSheet.Id;
                    }
                    if (null != googleFolders.CategorySheet)
                    {
                        categorySheetId = googleFolders.CategorySheet.Id;
                    }
                    if (!string.IsNullOrEmpty(bcfColorSchemeId) && !string.IsNullOrEmpty(categorySheetId))
                    {
                        schemeInfo = FileManager.ReadColorSchemes(bcfColorSchemeId, categorySheetId, false);
                    }

                    bcfFileDictionary = DataStorageUtil.ReadLinkedBCFFileInfo(m_doc, bcfProjectId);
                    bcfDictionary = GetBCFDictionary(m_doc);

                    List<BuiltInCategory> bltCategories = catDictionary.Values.ToList();
                    bool parameterCreated = ParameterUtil.CreateBCFParameters(m_app, bltCategories);

                    foreach (string catName in categoryNames)
                    {
                        CategoryInfo catInfo = new CategoryInfo(catName, true);
                        categoryInfoList.Add(catInfo);
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
            Dictionary<string, Dictionary<string, IssueEntry>> dictionary = new Dictionary<string, Dictionary<string, IssueEntry>>();
            try
            {
                AbortFlag.SetAbortFlag(false);
                progressWindow = new ProgressWindow("Loading BCF issues and images...");
                progressWindow.Show();

                List<string> markupIds = bcfFileDictionary.Keys.ToList();
                foreach (string markupId in markupIds)
                {
                    LinkedBcfFileInfo bcfFileInfo = bcfFileDictionary[markupId];
                    if (null != FileManager.FindFileById(bcfFileInfo.MarkupFileId) && null != FileManager.FindFileById(bcfFileInfo.ViewpointFileId))
                    {
                        Dictionary<string, IssueEntry> issueDictionary = GetBCFIssueInfo(doc, bcfFileInfo);
                        if (AbortFlag.GetAbortFlag()) { return new Dictionary<string, Dictionary<string, IssueEntry>>(); }
                        if (!dictionary.ContainsKey(markupId) && issueDictionary.Count > 0)
                        {
                            dictionary.Add(markupId, issueDictionary);
                        }
                    }
                    else
                    {
                        bcfFileDictionary.Remove(markupId);
                    }
                }

                if (!string.IsNullOrEmpty(categorySheetId))
                {
                    System.IO.MemoryStream stream = BCFParser.CreateCategoryStream(categoryNames);
                    if (null != stream)
                    {
                        Google.Apis.Drive.v2.Data.File file = FileManager.UpdateSpreadsheet(stream, categorySheetId, bcfProjectId);
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
            Dictionary<string, IssueEntry> issueDictionary = new Dictionary<string, IssueEntry>();
            try
            {
                issueDictionary = FileManager.ReadIssues(bcfFileInfo);

                List<string> issueIds = issueDictionary.Keys.ToList();
                progressWindow.SetMaximum(issueIds.Count);

                double progressValue = 0;
                foreach (string issueId in issueIds)
                {
                    if (AbortFlag.GetAbortFlag()) { progressWindow.Close();  return new Dictionary<string, IssueEntry>(); }

                    progressValue++;
                    progressWindow.SetProgressValue(progressValue);

                    IssueEntry issueEntry = issueDictionary[issueId];
                    List<int> elementIds = issueEntry.ElementDictionary.Keys.ToList();
                    foreach (int elementId in elementIds)
                    {
                        ElementProperties property = issueEntry.ElementDictionary[elementId];

                        Element element = m_doc.GetElement(new ElementId(elementId));
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
                                    int categoryId = element.Category.Id.IntegerValue;
                                    if (!catDictionary.ContainsKey(categoryId))
                                    {
                                        BuiltInCategory bltCategory = (BuiltInCategory)categoryId;
                                        if (bltCategory != BuiltInCategory.INVALID)
                                        {
                                            catDictionary.Add(categoryId, bltCategory);
                                        }
                                    }
                                }
                            }

                            ElementProperties ep = new ElementProperties(element);
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
                        if (bcfFileInfo.SharedLinkId == bcfProjectId && null != googleFolders)
                        {
                            issueDictionary[issueId].Snapshot = FileManager.DownloadImage(issueId, googleFolders.ActiveImgFolder.Id);
                        }
                        else if (bcfFileInfo.SharedLinkId == bcfProjectId)
                        {
                            googleFolders = FileManager.FindGoogleFolders(bcfProjectId);
                            if (null != googleFolders)
                            {
                                issueDictionary[issueId].Snapshot = FileManager.DownloadImage(issueId, googleFolders.ActiveImgFolder.Id);
                            }
                        }
                        else
                        {
                            FolderHolders tempFolders = FileManager.FindGoogleFolders(bcfFileInfo.SharedLinkId);
                            if (null != tempFolders)
                            {
                                issueDictionary[issueId].Snapshot = FileManager.DownloadImage(issueId, tempFolders.ActiveImgFolder.Id);
                            }
                        }
                    }
                }

                if (bcfDictionary.ContainsKey(bcfFileInfo.MarkupFileId))
                {
                    bcfDictionary.Remove(bcfFileInfo.MarkupFileId);
                }
                bcfDictionary.Add(bcfFileInfo.MarkupFileId, issueDictionary);
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
                m_doc = app.ActiveUIDocument.Document;

                switch (Request.Take())
                {
                    case RequestId.None:
                        return;
                    case RequestId.ReadLinkedFileInfo:
                        break;
                    case RequestId.UpdateLinkedFileInfo:
                        bool updated = DataStorageUtil.UpdateLinkedBCFFileInfo(m_doc, bcfFileDictionary);
                        Dictionary<string, Dictionary<string, IssueEntry>> dictionary = new Dictionary<string, Dictionary<string, IssueEntry>>();
                        
                        int numCat = categoryNames.Count;
                        AbortFlag.SetAbortFlag(false);
                        progressWindow = new ProgressWindow("Loading BCF issues and images...");
                        progressWindow.Show();

                        foreach (string markupId in bcfFileDictionary.Keys)
                        {
                            LinkedBcfFileInfo bcfInfo = bcfFileDictionary[markupId];
                            if (bcfDictionary.ContainsKey(markupId))
                            {
                                dictionary.Add(markupId, bcfDictionary[markupId]);
                            }
                            else
                            {
                                Dictionary<string, IssueEntry> issueDictionary = GetBCFIssueInfo(m_doc, bcfInfo);
                                if (issueDictionary.Count > 0)
                                {
                                    dictionary.Add(markupId, issueDictionary);
                                }
                            }
                        }
                        if (progressWindow.IsActive) { progressWindow.Close(); }
                        
                        bcfDictionary = dictionary;

                        if (numCat != categoryNames.Count)
                        {
                            System.IO.MemoryStream stream = BCFParser.CreateCategoryStream(categoryNames);
                            if (null != stream)
                            {
                                Google.Apis.Drive.v2.Data.File file = FileManager.UpdateSpreadsheet(stream, categorySheetId, bcfProjectId);
                            }

                            List<BuiltInCategory> bltCategories = catDictionary.Values.ToList();
                            bool parameterCreated = ParameterUtil.CreateBCFParameters(m_app, bltCategories);

                            foreach (string catName in categoryNames)
                            {
                                var catFound = from category in categoryInfoList where category.CategoryName == catName select category;
                                if (catFound.Count() == 0)
                                {
                                    CategoryInfo catInfo = new CategoryInfo(catName, true);
                                    categoryInfoList.Add(catInfo);
                                }
                            }
                        }

                        if (null != walkerWindow)
                        {
                            walkerWindow.BCFFileDictionary = bcfFileDictionary;
                            walkerWindow.BCFDictionary = bcfDictionary;
                            walkerWindow.CategoryInfoList = categoryInfoList;
                            walkerWindow.CurrentIndex = 0;
                            walkerWindow.DisplayLinkedBCF();
                        }

                        bool updatedId = ParameterUtil.SetBCFProjectId(m_doc, bcfProjectId);
                        schemeInfo = FileManager.ReadColorSchemes(bcfColorSchemeId, categorySheetId, false);
                        
                        if (null != walkerWindow)
                        {
                            walkerWindow.SchemeInfo = schemeInfo;
                            walkerWindow.DisplayColorscheme(schemeInfo);
                        }

                        break;
                    case RequestId.ReadProjectId:
                        bcfProjectId = ParameterUtil.GetBCFProjectId(app);
                        break;
                    case RequestId.UpdateProjectId:
                        bool updatedProjectId = ParameterUtil.SetBCFProjectId(m_doc, bcfProjectId);
                        break;
                    case RequestId.ReadBCFParameterInfo:
                        break;
                    case RequestId.UpdateBCFParameterInfo:
                        bool updatedParameters = ParameterUtil.UpdateBCFParameters(m_doc, currentElement, selectedIssue, selectedComment);
                        break;
                    case RequestId.UpdateAction:
                        bool actionUpdated = ParameterUtil.UpdateBCFParameter(m_doc, currentElement, BCFParameters.BCF_Action, currentElement.Action);
                        break;
                    case RequestId.UpdateResponsibility:
                        bool responsibilityUpdated = ParameterUtil.UpdateBCFParameter(m_doc, currentElement, BCFParameters.BCF_Responsibility, currentElement.ResponsibleParty);
                        break;
                    case RequestId.CreateIssue:
                        break;
                    case RequestId.UpdateViews:
                        IsolateElement(isIsolateOn, m_doc);
                        CreateSectionBox(isSectionBoxOn, m_doc);
                        HighlightElement(isHighlightOn, m_doc);
                        break;
                    case RequestId.UpdateParameterByComment:
                        UpdateParameters(m_doc);
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
                UIDocument uidoc=new UIDocument(doc);
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Highlight");
                    try
                    {
                        Element element = m_doc.GetElement(new ElementId(currentElement.ElementId));
                        if (null != element)
                        {
                            if (execute)
                            {
#if RELEASE2013||RELEASE2014
                            SelElementSet selElements = SelElementSet.Create();
                            selElements.Add(element);
                            uidoc.Selection.Elements = selElements;
                            uidoc.ShowElements(element);
#elif RELEASE2015
                                List<ElementId> selectedIds = new List<ElementId>();
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
#elif RELEASE2015

                                uidoc.Selection.SetElementIds(new List<ElementId>());
#endif
                            }
                            uidoc.RefreshActiveView();
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(currentElement.ElementName+": Failed to highlight elements.\n"+ex.Message, "Highlight Element", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                UIDocument uidoc = new UIDocument(doc);
                Element element = m_doc.GetElement(new ElementId(currentElement.ElementId));
                
                if (null != element && null!=activeView)
                {
                    if (activeView.IsInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate))
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Reset View");
                            try
                            {
                                activeView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
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
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Isolate");
                            try
                            {
                                List<ElementId> elementIds = new List<ElementId>();
                                elementIds.Add(element.Id);
                                activeView.IsolateElementsTemporary(elementIds);
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
                Element element = m_doc.GetElement(new ElementId(currentElement.ElementId));
                if (null != element)
                {
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Section Box");
                        try
                        {
                            if (execute)
                            {
                                BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
                                if (null != boundingBox)
                                {
                                    XYZ minXYZ = new XYZ(boundingBox.Min.X - 3, boundingBox.Min.Y - 3, boundingBox.Min.Z - 3);
                                    XYZ maxXYZ = new XYZ(boundingBox.Max.X + 3, boundingBox.Max.Y + 3, boundingBox.Max.Z + 3);
                                    BoundingBoxXYZ offsetBoundingBox = new BoundingBoxXYZ();
                                    offsetBoundingBox.Min = minXYZ;
                                    offsetBoundingBox.Max = maxXYZ;
#if RELEASE2013
                                    activeView.SectionBox = offsetBoundingBox;
#else
                                    activeView.SetSectionBox(offsetBoundingBox);
                                    activeView.GetSectionBox().Enabled = true;
#endif
                                }
                            }
                            else
                            {
#if RELEASE2013
                                activeView.SectionBox = null;                              
#else
                                Parameter parameter = activeView.get_Parameter(BuiltInParameter.VIEWER_MODEL_CLIP_BOX_ACTIVE);
                                if (null != parameter)
                                {
                                    parameter.Set(0);
                                }
                                activeView.GetSectionBox().Enabled = false;
#endif
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a section box around the selected element.\n"+ex.Message, "Create Section Box", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateParameters(Document doc)
        {
            try
            {
                if (null != selectedIssue && null != selectedComment)
                {
                    foreach (int elementId in selectedIssue.ElementDictionary.Keys)
                    {
                        ElementProperties ep = selectedIssue.ElementDictionary[elementId];
                        bool parameterUpdated = ParameterUtil.UpdateBCFParameters(m_doc, ep, selectedIssue, selectedComment);
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
