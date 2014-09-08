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
        private Request m_request = new Request();
        private Dictionary<string/*spreadsheetId*/, LinkedBcfFileInfo> bcfFileDictionary = new Dictionary<string, LinkedBcfFileInfo>();
        private Dictionary<string/*spreadsheetId*/, Dictionary<string/*issueId*/, IssueEntry>> bcfDictionary = new Dictionary<string, Dictionary<string, IssueEntry>>();
       
        private ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
        private WalkerWindow walkerWindow = null;
        private ElementProperties currentElement = null;

        public Document ActiveDoc { get { return m_doc; } set { m_doc = value; } }
        public Request Request { get { return m_request; } }
        public Dictionary<string, LinkedBcfFileInfo> BCFFileDictionary { get { return bcfFileDictionary; } set { bcfFileDictionary = value; } }
        public Dictionary<string, Dictionary<string, IssueEntry>> BCFDictionary { get { return bcfDictionary; } set { bcfDictionary = value; } }
        public ColorSchemeInfo SchemeInfo { get { return schemeInfo; } set { schemeInfo = value; } }
        public WalkerWindow WalkerWindow { get { return walkerWindow; } set { walkerWindow = value; } }
        public ElementProperties CurrentElement { get { return currentElement; } set { currentElement = value; } }

        public WalkerHandler(UIApplication uiapp)
        {
            try
            {
                m_app = uiapp;
                m_doc = uiapp.ActiveUIDocument.Document;
                bcfFileDictionary = GetLinkedBCFFileInfo(m_doc);
                bcfDictionary = GetBCFDictionary(m_doc);
                schemeInfo = GetColorSchemeInfo();

            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to initialize External Event handler.\n"+ex.Message, "External Event Handler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Dictionary<string, LinkedBcfFileInfo> GetLinkedBCFFileInfo(Document doc)
        {
            Dictionary<string, LinkedBcfFileInfo> fileDictionary = new Dictionary<string, LinkedBcfFileInfo>();
            try
            {
                fileDictionary = DataStorageUtil.ReadDataStorage(doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get linked BCF file Info from data storage.\n"+ex.Message, "Get Linked BCF File Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return fileDictionary;
        }

        private Dictionary<string/*spreadsheetId*/, Dictionary<string/*issueId*/, IssueEntry>> GetBCFDictionary(Document doc)
        {
            Dictionary<string, Dictionary<string, IssueEntry>> bcfDcitionary = new Dictionary<string, Dictionary<string, IssueEntry>>();
            try
            {
                foreach (string fileId in bcfFileDictionary.Keys)
                {
                    LinkedBcfFileInfo bcfFileInfo = bcfFileDictionary[fileId];
                    Dictionary<string, IssueEntry> dictionary = BCFParser.ReadIssues(fileId, bcfFileInfo.BCFName);
                    List<string> categoryNames = new List<string>();//for colorscheme
                    List<string> issueIds = dictionary.Keys.ToList();
                    foreach (string issueId in issueIds)
                    {
                        IssueEntry issueEntry = dictionary[issueId];
                        List<int> elementIds = issueEntry.ElementDictionary.Keys.ToList();
                        foreach (int elementId in elementIds)
                        {
                            Element element = m_doc.GetElement(new ElementId(elementId));
                            if (null != element)
                            {
                                if (null != element.Category)
                                {
                                    if (!categoryNames.Contains(element.Category.Name))
                                    {
                                        categoryNames.Add(element.Category.Name);
                                    }
                                }

                                ElementProperties ep = new ElementProperties(element);
                                dictionary[issueId].ElementDictionary.Remove(elementId);
                                dictionary[issueId].ElementDictionary.Add(elementId, ep);
                            }
                        }
                    }

                    if (!bcfDcitionary.ContainsKey(fileId))
                    {
                        bcfDcitionary.Add(fileId, dictionary);
                    }

                    bool updatedCategory = BCFParser.UpdateCategories(fileId, categoryNames);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get BCF dictionary.\n"+ex.Message, "Get BCF Dictionary", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfDcitionary;
        }

        private ColorSchemeInfo GetColorSchemeInfo()
        {
            ColorSchemeInfo info = new ColorSchemeInfo();
            try
            {
                //combine color definitions from different color scheme
                foreach (string fileId in bcfFileDictionary.Keys)
                {
                    ColorSchemeInfo csi = BCFParser.ReadColorSchemes(fileId);
                    foreach (ColorScheme scheme in csi.ColorSchemes)
                    {
                        var schemes = from s in info.ColorSchemes where s.SchemeName == scheme.SchemeName select s;
                        if (schemes.Count() > 0)
                        {
                            for (int i = 0; i < info.ColorSchemes.Count; i++)
                            {
                                if (info.ColorSchemes[i].SchemeName == scheme.SchemeName)
                                {
                                    foreach (ColorDefinition cd in scheme.ColorDefinitions)
                                    {
                                        var definitions = from def in info.ColorSchemes[i].ColorDefinitions where def.ParameterValue == cd.ParameterValue select def;
                                        if (definitions.Count() == 0)
                                        {
                                            info.ColorSchemes[i].ColorDefinitions.Add(cd);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        else
                        {
                            info.ColorSchemes.Add(scheme);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get color scheme info.\n"+ex.Message, "Get Color Scheme Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return info;
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
                        bool updated = DataStorageUtil.UpdateDataStorage(m_doc, bcfFileDictionary);
                        bcfDictionary = GetBCFDictionary(m_doc);
                        schemeInfo = GetColorSchemeInfo();

                        if (null != walkerWindow && bcfDictionary.Count > 0)
                        {
                            walkerWindow.BCFDictionary = bcfDictionary;
                            walkerWindow.SchemeInfo = schemeInfo;
                            walkerWindow.CurrentIndex = 0;
                            walkerWindow.DisplayLinkedBCF();
                            walkerWindow.DisplayColorscheme(schemeInfo);
                        }
                        break;
                    case RequestId.ReadParameterInfo:
                        break;
                    case RequestId.UpdateParameterInfo:
                        break;
                    case RequestId.HighlightElement:
                        HighlightElement(true, m_doc);
                        break;
                    case RequestId.CancelHighlight:
                        HighlightElement(false, m_doc);
                        break;
                    case RequestId.IsolateElement:
                        IsolateElement(true, m_doc);
                        break;
                    case RequestId.CancelIsolate:
                        IsolateElement(false, m_doc);
                        break;
                    case RequestId.CreateSectionBox:
                        CreateSectionBox(true, m_doc);
                        break;
                    case RequestId.CancelSectionBox:
                        CreateSectionBox(false, m_doc);
                        break;
                    case RequestId.CreateIssue:
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
                Element element = m_doc.GetElement(new ElementId(currentElement.ElementId));
                if(null!=element)
                {
                    if(execute)
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
                View activeView = doc.ActiveView;
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
                View3D view3d = doc.ActiveView as View3D;
                if (null != view3d)
                {
                    if (execute)
                    {
                        Element element = m_doc.GetElement(new ElementId(currentElement.ElementId));
                        if (null != element)
                        {
                            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
                            if (null != boundingBox)
                            {
                                using(Transaction trans=new Transaction(doc))
                                {
                                    trans.Start("Create Section Box");
                                    try
                                    {
#if RELEASE2013
                                        view3d.SectionBox = boundingBox;
#else
                                        view3d.SetSectionBox(boundingBox);
                                        view3d.GetSectionBox().Enabled = true;
#endif
                                        trans.Commit();
                                    }
                                    catch
                                    {
                                        trans.RollBack();
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            try
                            {
                                trans.Start("Disable Section Box");
#if RELEASE2013
                                view3d.SectionBox = null;
#else
                                view3d.SetSectionBox(null);
                                view3d.GetSectionBox().Enabled = false;
#endif
                                trans.Commit();
                            }
                            catch { trans.RollBack(); }
                        }
                    }
                    
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a section box around the selected element.\n"+ex.Message, "Create Section Box", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        ReadParameterInfo=3,

        UpdateParameterInfo=4,

        HighlightElement=5,

        CancelHighlight=6,

        IsolateElement=7,

        CancelIsolate=8,

        CreateSectionBox=9,

        CancelSectionBox=10,

        CreateIssue=11,
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
