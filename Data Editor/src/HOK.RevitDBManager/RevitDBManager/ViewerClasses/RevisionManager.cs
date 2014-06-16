using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace RevitDBManager.ViewerClasses
{
    public class RevisionManager
    {
        private UIApplication m_app;
        private Document m_doc;
        private Dictionary<int/*sequence*/, RevisionData> revisionDictionary = new Dictionary<int, RevisionData>();
        
        public Dictionary<int, RevisionData> RevisionDictionary { get { return revisionDictionary; } set { revisionDictionary = value; } }

        public RevisionManager(UIApplication application)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;

            CollectRevisions();
            CollectSheet();
        }

        private void CollectRevisions()
        {
            ElementCategoryFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Revisions);
            FilteredElementCollector collector = new FilteredElementCollector(m_doc);
            List<Element> revisionElements = collector.WherePasses(catFilter).ToElements().ToList();

            foreach (Element element in revisionElements)
            {
                RevisionData rd = new RevisionData();
                Parameter param = element.get_Parameter(BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM);
                if (null != param) { rd.Sequence = param.AsInteger(); }

                param = element.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_NUM);
                if (null != param) { rd.Number = param.AsString(); }

                param = element.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION);
                if (null != param) { rd.Description = param.AsString(); }

                param = element.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DATE);
                if (null != param) { rd.Date = param.AsString(); }

                param = element.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED);
                if (null != param) { rd.Issued = Convert.ToBoolean(param.AsInteger()); }

                param = element.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO);
                if (null != param) { rd.IssuedTo = param.AsString(); }

                param = element.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_BY);
                if (null != param) { rd.IssuedBy = param.AsString(); }

                if (!revisionDictionary.ContainsKey(rd.Sequence))
                {
                    revisionDictionary.Add(rd.Sequence, rd);
                }
            }
        }

        private void CollectSheet()
        {
            ElementCategoryFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Sheets);
            FilteredElementCollector collector = new FilteredElementCollector(m_doc);
            List<Element> sheetElements = collector.WherePasses(catFilter).ToElements().ToList();

        }

        public void DisplaySheetView(DataGridView dataGridView)
        {
            
        }
    }

    public class RevisionData
    {
        public RevisionData()
        {
        }

        public int Sequence { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }
        public bool Issued { get; set; }
        public string IssuedTo { get; set; }
        public string IssuedBy { get; set; }
    }

    public class SheetData
    {
    }
}
