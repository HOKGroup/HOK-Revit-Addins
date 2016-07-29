using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.SheetManager.AddIn.Windows
{
    /// <summary>
    /// Interaction logic for RevisionWindow.xaml
    /// </summary>
    public partial class RevisionWindow : Window
    {
        private Guid linkedProjectId = Guid.Empty;
        private RevitSheetData rvtSheetData = null;
        private Document currentDoc = null;
        private SheetManagerHandler m_handler = null;
        private ExternalEvent m_event = null;
        
        private bool selectionMode = false;

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }
        public Document CurrentDoc { get { return currentDoc; } set { currentDoc = value; } }
        

        public RevisionWindow(Guid projectId, SheetManagerHandler handler, ExternalEvent extEvent)
        {
            linkedProjectId = projectId;
            m_handler = handler;
            m_event = extEvent;

            currentDoc = m_handler.CurrentDocument;

            m_handler.Request.Make(RequestId.GetRevisions);
            m_event.Raise();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtSheetData = this.DataContext as RevitSheetData;
                dataGridRevisionRvt.ItemsSource = m_handler.RevisionCollection;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void dataGridRevisionDB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridRevisionDB.SelectedItems && !selectionMode)
                {
                    selectionMode = true;

                    dataGridRevisionRvt.SelectedItems.Clear();
                    List<RevitRevision> selectedRevisions = dataGridRevisionDB.SelectedItems.OfType<RevitRevision>().ToList();
                    var linkedRevisionIds = from revision in selectedRevisions where revision.LinkStatus.IsLinked select revision.LinkStatus.CurrentLinkedId;
                    if (linkedRevisionIds.Count() > 0)
                    {
                        var revisionFound = from revision in m_handler.RevisionCollection where linkedRevisionIds.Contains(revision.UniqueId) select revision;
                        if (revisionFound.Count() > 0)
                        {
                            foreach (object obj in revisionFound)
                            {
                                dataGridRevisionRvt.SelectedItems.Add(obj);
                            }
                        }
                    }

                    selectionMode = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridRevisionRvt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridRevisionRvt.SelectedItems && !selectionMode)
                {
                    selectionMode = true;

                    dataGridRevisionDB.SelectedItems.Clear();
                    List<Revision> selectedRevisions = dataGridRevisionRvt.SelectedItems.OfType<Revision>().ToList();
                    var uniqueIds = from revision in selectedRevisions select revision.UniqueId;
                    if (uniqueIds.Count() > 0)
                    {
                        var revisionFound = from revision in rvtSheetData.Revisions where revision.LinkStatus.IsLinked && (uniqueIds.Contains(revision.LinkStatus.CurrentLinkedId)) select revision;
                        if (revisionFound.Count() > 0)
                        {
                            foreach (object obj in revisionFound)
                            {
                                dataGridRevisionDB.SelectedItems.Add(obj);
                            }
                        }
                    }


                    selectionMode = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDbToRevit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRevisionDB.SelectedItems)
                {
                    List<RevitRevision> selectedRevisions = dataGridRevisionDB.SelectedItems.OfType<RevitRevision>().ToList();
                    var selectedIds = from revision in selectedRevisions select revision.Id;
                    if (selectedIds.Count() > 0)
                    {
                        for (int i = 0; i < rvtSheetData.Revisions.Count; i++)
                        {
                            RevitRevision rvtRevision = rvtSheetData.Revisions[i];
                            if (selectedIds.Contains(rvtRevision.Id))
                            {
                                rvtSheetData.Revisions[i].LinkStatus.IsSelected = true;
                            }
                            else
                            {
                                rvtSheetData.Revisions[i].LinkStatus.IsSelected = false;
                            }
                        }
                    }
                    selectionMode = true;
                    //insert or update
                    m_handler.Request.Make(RequestId.UpdateRevision);
                    m_event.Raise();

                    selectionMode = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }



        private void ButtonRevitToDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //write database
                if (null != dataGridRevisionRvt.SelectedItems)
                {
                    List<Guid> sheetIds = new List<Guid>();
                    var sheetsFound = from sheet in rvtSheetData.Sheets select sheet.Id;
                    if (sheetsFound.Count() > 0)
                    {
                        sheetIds = sheetsFound.ToList();
                    }

                    foreach (object item in dataGridRevisionRvt.SelectedItems)
                    {
                        Revision revision = (Revision)item;
                        var revisionFound = from rev in rvtSheetData.Revisions where rev.LinkStatus.IsLinked && rev.LinkStatus.CurrentLinkedId == revision.UniqueId select rev;
                        if (revisionFound.Count() > 0)
                        {
                            //update DB
                            int index = rvtSheetData.Revisions.IndexOf(revisionFound.First());
                            bool updated = UpdateRevision(revision, index);
                        }
                        else
                        {
                            //insert into DB
                            bool inserted = InsertRevisionToDB(revision, sheetIds);
                        }
                    }
                }
                //create revision items && linked revisions and write database

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool InsertRevisionToDB(Revision revision, List<Guid> sheetIds)
        {
            bool inserted = false;
            try
            {
                RevitRevision rvtRevision = new RevitRevision(Guid.NewGuid(), revision.Description, revision.IssuedBy, revision.IssuedTo, revision.RevisionDate);
                NumberType revNumType = (NumberType)Enum.Parse(typeof(NumberType), revision.NumberType.ToString());
                LinkedRevision linkedRevision = new LinkedRevision(Guid.NewGuid(), rvtRevision.Id, revision.SequenceNumber, revision.RevisionNumber, revNumType, linkedProjectId, revision.UniqueId, true);
                rvtRevision.LinkStatus.IsLinked = true;
                rvtRevision.LinkStatus.IsSelected = false;
                rvtRevision.LinkStatus.Modified = false;
                rvtRevision.LinkStatus.CurrentLinkedId = revision.UniqueId;
                rvtRevision.LinkStatus.LinkedElementId = revision.Id.IntegerValue;
                rvtRevision.LinkStatus.ToolTip = "Linked Revision ElementId: " + revision.Id.IntegerValue;
                rvtRevision.LinkedRevisions.Add(linkedRevision);
                rvtSheetData.Revisions.Add(rvtRevision);

                bool revisionDBUpdated = SheetDataWriter.ChangeRevisionItem(rvtRevision, CommandType.INSERT);
                bool linkedRevisionDBUpdated = SheetDataWriter.ChangeLinkedRevision(linkedRevision, CommandType.INSERT);

                List<RevisionOnSheet> rosList = new List<RevisionOnSheet>();
                foreach (Guid sheetId in sheetIds)
                {
                    RevisionOnSheet ros = new RevisionOnSheet(Guid.NewGuid(), sheetId, rvtRevision, false);
                    rosList.Add(ros);
                }

                bool rosDBUpdated = SheetDataWriter.InsertMultipleRevisionOnSheet(rosList);
                inserted = (revisionDBUpdated && linkedRevisionDBUpdated && rosDBUpdated) ? true : false;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        private bool UpdateRevision(Revision revision, int revisionIndex)
        {
            bool updated = false;
            try
            {
                rvtSheetData.Revisions[revisionIndex].Date = revision.RevisionDate;
                rvtSheetData.Revisions[revisionIndex].Description = revision.Description;
                rvtSheetData.Revisions[revisionIndex].IssuedBy = revision.IssuedBy;
                rvtSheetData.Revisions[revisionIndex].IssuedTo = revision.IssuedTo;

                rvtSheetData.Revisions[revisionIndex].LinkStatus.IsLinked = true;
                rvtSheetData.Revisions[revisionIndex].LinkStatus.IsSelected = false;
                rvtSheetData.Revisions[revisionIndex].LinkStatus.Modified = false;
                rvtSheetData.Revisions[revisionIndex].LinkStatus.CurrentLinkedId = revision.UniqueId;
                rvtSheetData.Revisions[revisionIndex].LinkStatus.LinkedElementId = revision.Id.IntegerValue;
                rvtSheetData.Revisions[revisionIndex].LinkStatus.ToolTip = "Linked Revision ElementId: " + revision.Id.IntegerValue;

                updated = SheetDataWriter.ChangeRevisionItem(rvtSheetData.Revisions[revisionIndex], CommandType.UPDATE);

                var linkFound = from link in rvtSheetData.Revisions[revisionIndex].LinkedRevisions where link.LinkedElementId == revision.UniqueId && link.LinkProject.Id == linkedProjectId select link;
                if (linkFound.Count() > 0)
                {
                    int linkIndex = rvtSheetData.Revisions[revisionIndex].LinkedRevisions.IndexOf(linkFound.First());
                    rvtSheetData.Revisions[revisionIndex].LinkedRevisions[linkIndex].Number = revision.RevisionNumber;
                    rvtSheetData.Revisions[revisionIndex].LinkedRevisions[linkIndex].Sequence = revision.SequenceNumber;

                    updated = SheetDataWriter.ChangeLinkedRevision(rvtSheetData.Revisions[revisionIndex].LinkedRevisions[linkIndex], CommandType.UPDATE);
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }
        
    }
}
