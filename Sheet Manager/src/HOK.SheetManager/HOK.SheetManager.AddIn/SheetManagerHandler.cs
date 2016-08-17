using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SheetManager.AddIn.Updaters;
using HOK.SheetManager.AddIn.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.SheetManager.AddIn
{
    public class SheetManagerHandler : IExternalEventHandler
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private Request m_request = new Request();
        private AddInViewModel viewModel = null;
        private ObservableCollection<Revision> revisionCollection = new ObservableCollection<Revision>();

        public Request Request { get { return m_request; } }
        public AddInViewModel ViewModel { get { return viewModel; } set { viewModel = value; } }
        public Document CurrentDocument { get { return m_doc; } set { m_doc = value; } }
        public ObservableCollection<Revision> RevisionCollection { get { return revisionCollection; } set { revisionCollection = value; } }

        public SheetManagerHandler(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;
        }

        public void Execute(UIApplication app)
        {
            m_doc = app.ActiveUIDocument.Document;
            try
            {
                switch (Request.Take())
                {
                    case RequestId.UpdateSheet:
                        viewModel.UpdateSheets(m_doc);
                        break;
                    case RequestId.UpdateRevision:
                        viewModel.UpdateRevisions(m_doc);
                        GetRevisionsInRevit();
                        break;
                    case RequestId.UpdateRevisionOnSheet:
                        viewModel.UpdateRevisionOnSheets(m_doc);
                        break;
                    case RequestId.PlaceView:
                        viewModel.PlaceViews(m_doc);
                        break;
                    case RequestId.ImportView:
                        viewModel.ImportViews(m_doc);
                        break;
                    case RequestId.RenumberSheet:
                        viewModel.RenumberSheets(m_doc);
                        break;
                    case RequestId.RenameView:
                        viewModel.RenameViews(m_doc);
                        break;
                    case RequestId.StoreConfiguration:
                        viewModel.StoreConfiguration(m_doc);
                        break;
                    case RequestId.GetRevisions:
                        GetRevisionsInRevit();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute Sheet Manager Handler.\n" + ex.Message, "Execute Sheet Manager Handler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetRevisionsInRevit()
        {
            try
            {
                revisionCollection.Clear();

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Revision> revisions = collector.OfCategory(BuiltInCategory.OST_Revisions).ToElements().Cast<Revision>().ToList();
                revisions = revisions.OrderBy(o => o.SequenceNumber).ToList();

                for (int i = 0; i < revisions.Count; i++)
                {
                    revisionCollection.Add(revisions[i]);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        public string GetName()
        {
            return "Sheet Manager Handler";
        }
    }

    public enum RequestId : int
    {
        None = 0, UpdateSheet = 1, UpdateRevision = 2, UpdateRevisionOnSheet = 3, PlaceView = 4, ImportView = 5, RenumberSheet = 6, RenameView = 7, StoreConfiguration = 8, GetRevisions =9
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
