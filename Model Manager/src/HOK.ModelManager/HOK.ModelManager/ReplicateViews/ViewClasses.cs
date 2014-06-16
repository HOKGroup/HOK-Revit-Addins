using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using HOK.ModelManager.GoogleDocs;

namespace HOK.ModelManager.ReplicateViews
{
    public class ViewProperties
    {
        private Document m_doc = null;
        private string docName = "";
        private ViewDrafting m_view = null;
        private string uniqueId = "";
        private int viewId = 0;
        private string viewName = "";
        private int viewTypeID = 0;
        private string viewTypeName = "";
        private bool isOnSheet = false;
        private ViewSheet sheetObj = null;
        private string sheetNumber = "None";
        private string sheetName = "None";
        private XYZ viewLocation = null;
        private string viewportTypeName = "";
        private Viewport viewport = null;
        private LinkStatus status = LinkStatus.None;
        private ViewProperties linkedView = null;

        public ViewDrafting ViewDraftingObj { get { return m_view; } set { m_view = value; } }
        public string UniqueId { get { return uniqueId; } set { uniqueId = value; } }
        public string DocName { get { return docName; } set { docName = value; } }
        public int ViewId { get { return viewId; } set { viewId = value; } }
        public string ViewName { get { return viewName; } set { viewName = value; } }
        public int ViewTypeID { get { return viewTypeID; } set { viewTypeID = value; } }
        public string ViewTypeName { get { return viewTypeName; } set { viewTypeName = value; } }
        public bool IsOnSheet { get { return isOnSheet; } set { isOnSheet = value; } }
        public ViewSheet SheetObj { get { return sheetObj; } set { sheetObj = value; } }
        public string SheetNumber { get { return sheetNumber; } set { sheetNumber = value; } }
        public string SheetName { get { return sheetName; } set { sheetName = value; } }
        public XYZ ViewLocation { get { return viewLocation; } set { viewLocation = value; } }
        public string ViewportTypeName { get { return viewportTypeName; } set { viewportTypeName = value; } }
        public Viewport ViewportObj { get { return viewport; } set { viewport = value; } }
        public LinkStatus Status { get { return status; } set { status = value; } }
        public ViewProperties LinkedView { get { return linkedView; } set { linkedView = value; } }

        public ViewProperties(Document doc, ViewDrafting view)
        {
            m_doc = doc;
            m_view = view;
            GetInfo();
        }

        public void GetInfo()
        {
            try
            {
                docName = m_doc.Title;
                uniqueId = m_view.UniqueId;
                viewId = m_view.Id.IntegerValue;
                viewName = m_view.Name;
                viewTypeID = m_view.GetTypeId().IntegerValue;
                ViewFamilyType viewType = m_doc.GetElement(m_view.GetTypeId()) as ViewFamilyType;
                if (null != viewType)
                {
                    viewTypeName = viewType.Name;
                }

                Parameter sheetNumberParam = m_view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER);
                Parameter sheetNameParam = m_view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NAME);
                if (null != sheetNumberParam && null != sheetNameParam)
                {
                    if (!string.IsNullOrEmpty(sheetNumberParam.AsString()) && !string.IsNullOrEmpty(sheetNameParam.AsString()))
                    {
                        sheetNumber = sheetNumberParam.AsString();
                        sheetName = sheetNameParam.AsString();
                        isOnSheet = true;
                        sheetObj = FindSheet(sheetNumber, sheetName);
                        viewLocation = FindViewLocation(sheetObj);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public ViewSheet FindSheet(string sheetNumber, string sheetName)
        {
            ViewSheet viewSheet = null;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<ViewSheet> elements = collector.OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements().Cast<ViewSheet>().ToList();
                var sheets = from elem in elements where elem.SheetNumber == sheetNumber && elem.ViewName == sheetName select elem;
                if (sheets.Count() > 0)
                {
                    viewSheet = sheets.First();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return viewSheet;
        }

        public XYZ FindViewLocation(ViewSheet sheet)
        {
            XYZ position = null;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc, sheet.Id);
                List<Viewport> elements = collector.OfClass(typeof(Viewport)).WhereElementIsNotElementType().ToElements().Cast<Viewport>().ToList();
                var viewports = from elem in elements where elem.ViewId == m_view.Id select elem;
                if (viewports.Count() > 0)
                {
                    ViewportObj = viewports.First();
                    position = viewport.GetBoxCenter();

                    ElementType elementType = m_doc.GetElement(viewport.GetTypeId()) as ElementType;
                    viewportTypeName = elementType.Name;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return position;
        }
    }

    public class PreviewMap
    {
        //public Document SourceDocument { get; set; }
        //public Document RecipientDocument { get; set; }
        public ModelInfo SourceModelInfo { get; set; }
        public ModelInfo RecipientModelInfo { get; set; }
        public ViewProperties SourceViewProperties { get; set; }
        public ViewProperties RecipientViewProperties { get; set; }
        public LinkInfo ViewLinkInfo { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ViewMapClass
    {
        //input
        private ModelInfo sourceInfo = null;
        private ModelInfo recipientInfo = null;
        private Dictionary<int, ViewProperties> sourceViews = new Dictionary<int, ViewProperties>();
        private Dictionary<int, ViewProperties> recipientViews = new Dictionary<int, ViewProperties>();
        private List<LinkInfo> linkInfoList = new List<LinkInfo>();


        public ModelInfo SourceInfo { get { return sourceInfo; } set { sourceInfo = value; } }
        public ModelInfo RecipientInfo { get { return recipientInfo; } set { recipientInfo = value; } }
        public Dictionary<int, ViewProperties> SourceViews { get { return sourceViews; } set { sourceViews = value; } }
        public Dictionary<int, ViewProperties> RecipientViews { get { return recipientViews; } set { recipientViews = value; } }
        public List<LinkInfo> LinkInfoList { get { return linkInfoList; } set { linkInfoList = value; } }

        public ViewMapClass(ModelInfo sInfo, ModelInfo rInfo, Dictionary<int, ViewProperties> sDictionary, Dictionary<int, ViewProperties> rDictionary, List<LinkInfo> linkInfo)
        {
            sourceInfo = sInfo;
            recipientInfo = rInfo;

            foreach (ViewProperties vp in sDictionary.Values)
            {
                vp.Status = LinkStatus.None;
                sourceViews.Add(vp.ViewId, vp);
            }

            foreach (ViewProperties vp in rDictionary.Values)
            {
                vp.Status = LinkStatus.None;
                recipientViews.Add(vp.ViewId, vp);
            }

            var links = from link in linkInfo where link.SourceModelName == sInfo.DocTitle select link;
            linkInfoList = links.ToList();

            SetLinkStatus();
        }

        private void SetLinkStatus()
        {
            try
            {
                
                foreach (LinkInfo info in linkInfoList)
                {
                    int sourceId = info.SourceItemId;
                    int destId = info.DestItemId;
                    if (sourceViews.ContainsKey(sourceId) && recipientViews.ContainsKey(destId))
                    {
                        ViewProperties sView = sourceViews[sourceId];
                        ViewProperties rView = recipientViews[destId];
                        sView.Status = LinkStatus.Linked;
                        rView.Status = LinkStatus.Linked;
                        sView.LinkedView = rView;
                        rView.LinkedView = sView;

                        sourceViews.Remove(sourceId);
                        sourceViews.Add(sourceId, sView);

                        recipientViews.Remove(destId);
                        recipientViews.Add(destId, rView);
                    }
                    else if (sourceViews.ContainsKey(sourceId) && !recipientViews.ContainsKey(destId))
                    {
                        ViewProperties sView = sourceViews[sourceId];
                        sView.Status = LinkStatus.MissingFromRecipient;

                        sourceViews.Remove(sourceId);
                        sourceViews.Add(sourceId, sView);
                    }
                    else if (!sourceViews.ContainsKey(sourceId) && recipientViews.ContainsKey(destId))
                    {
                        ViewProperties rView = recipientViews[destId];
                        rView.Status = LinkStatus.MissingFromSource;

                        recipientViews.Remove(destId);
                        recipientViews.Add(destId, rView);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }
}
