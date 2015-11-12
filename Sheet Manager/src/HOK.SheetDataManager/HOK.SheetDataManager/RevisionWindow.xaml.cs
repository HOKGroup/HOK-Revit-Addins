using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.SheetDataManager
{
    /// <summary>
    /// Interaction logic for RevisionWindow.xaml
    /// </summary>
    public partial class RevisionWindow : Window
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private DatabaseManager dbManager = null;
        private RevitSheetData sheetData = null;
        private Dictionary<string, RevisionInfo> existingRevisions = new Dictionary<string, RevisionInfo>();
        private LinkedProject currentProject = null;
        private ObservableCollection<RevitRevision> revisionItems = new ObservableCollection<RevitRevision>();

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }
        public Dictionary<string, RevisionInfo> ExistingRevisions { get { return existingRevisions; } set { existingRevisions = value; } }

        public RevisionWindow(UIApplication uiapp, DatabaseManager databaseManager, RevitSheetData rvtSheetData, Dictionary<string, RevisionInfo> revisions,  LinkedProject project)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            dbManager = databaseManager;
            existingRevisions = revisions;

            sheetData = rvtSheetData;
            currentProject = project;

            InitializeComponent();

            DisplayRevisions();
        }

        private void DisplayRevisions()
        {
            try
            {
                List<RevitRevision> revisions = sheetData.Revisions.Values.ToList();
                revisions = revisions.OrderBy(o => o.Date).ToList();
                revisionItems = new ObservableCollection<RevitRevision>(revisions);

                dataGridRevision.ItemsSource = null;
                dataGridRevision.ItemsSource = revisionItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display revisions.\n" + ex.Message, "Display Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                revisionItems = (ObservableCollection<RevitRevision>)dataGridRevision.ItemsSource;
                var selectedRevisions = from item in revisionItems where item.IsSelected select item;
                if (selectedRevisions.Count() > 0)
                {
                    statusLable.Text = "Creating / Updating Revisions...";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = selectedRevisions.Count();

                    double value = 0;
                    
                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                    using (TransactionGroup tg = new TransactionGroup(m_doc))
                    {
                        tg.Start("Update Revisions");
                        try
                        {
                            foreach (RevitRevision revisionItem in selectedRevisions)
                            {
                                value++;
                                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Update Revisions");
                                    try
                                    {
                                        RevitRevision rvtRevision = revisionItem;
                                        Revision revision = null;
                                        if (rvtRevision.Linked && !string.IsNullOrEmpty(rvtRevision.CurrentLinkedId))
                                        {
                                            if (rvtRevision.Modified)
                                            {
                                                //parameter update
                                                revision = m_doc.GetElement(rvtRevision.CurrentLinkedId) as Revision;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                           revision = Revision.Create(m_doc);
                                        }

                                        if (null != revision)
                                        {
                                            revision.Description = rvtRevision.Description;
                                            revision.IssuedTo = rvtRevision.IssuedTo;
                                            revision.IssuedBy = rvtRevision.IssuedBy;
                                            revision.RevisionDate = rvtRevision.Date;

                                            rvtRevision.Modified = false;
                                            rvtRevision.IsSelected = false;
                                            rvtRevision.Linked = true;
                                            rvtRevision.CurrentLinkedId = revision.UniqueId;

                                            rvtRevision.ToolTip = "Linked Revision ElementId: " + revision.Id.IntegerValue;

                                            RevisionInfo revisionInfo = new RevisionInfo(revision);
                                            LinkedRevision linkedRevision = dbManager.GetLinkedRevision(revisionInfo.RevisionUniqueId, currentProject.Id);
                                            if (null == linkedRevision)
                                            {
                                                linkedRevision = new LinkedRevision(Guid.NewGuid(), rvtRevision.Id, revision.SequenceNumber, revision.RevisionNumber, revision.NumberType, currentProject, revision.UniqueId, false);
                                                sheetData.LinkedRevisions.Add(linkedRevision.Id, linkedRevision);

                                                bool updatedLink = dbManager.UpdateLinkedRevision(linkedRevision);
                                            }
                                            revisionInfo.LinkedRevisionItem = linkedRevision;

                                            if (existingRevisions.ContainsKey(revisionInfo.RevisionUniqueId))
                                            {
                                                existingRevisions.Remove(revisionInfo.RevisionUniqueId);
                                            }
                                            existingRevisions.Add(revisionInfo.RevisionUniqueId, revisionInfo);
                                        }

                                        if (sheetData.Revisions.ContainsKey(rvtRevision.Id))
                                        {
                                            sheetData.Revisions.Remove(rvtRevision.Id);
                                            sheetData.Revisions.Add(rvtRevision.Id, rvtRevision);
                                        }

                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.RollBack();
                                        string message = ex.Message;
                                    }
                                }
                            }
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to update revisions.\n" + ex.Message, "Update Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";

                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update revisions.\n" + ex.Message, "Update Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (RevitRevision revisionItem in revisionItems)
                {
                    revisionItem.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select all revision items.\n" + ex.Message, "Check All Revision Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (RevitRevision revisionItem in revisionItems)
                {
                    revisionItem.IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unselect all revision items.\n" + ex.Message, "Uncheck All Revision Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
