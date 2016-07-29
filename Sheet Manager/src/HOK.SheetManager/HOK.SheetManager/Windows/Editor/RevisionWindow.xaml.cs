using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using HOK.SheetManager.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
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

namespace HOK.SheetManager.Windows.Editor
{
    /// <summary>
    /// Interaction logic for RevisionWindow.xaml
    /// </summary>
    public partial class RevisionWindow : Window
    {
        private RevitSheetData rvtSheetData = null;

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }

        public RevisionWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtSheetData = this.DataContext as RevitSheetData;

                comboBoxField.ItemsSource = from column in dataGridRevision.Columns select column.Header.ToString();
                comboBoxField.SelectedIndex = 0;
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridRevision_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                DataGridRow row = e.Row;
                string columnName = e.Column.Header.ToString();

                if (null != row && !string.IsNullOrEmpty(columnName))
                {
                    RevitRevision oldRevision = row.Item as RevitRevision;
                    if (columnName != "Document")
                    {
                        TextBox textBox = e.EditingElement as TextBox;
                        string propertyValue = textBox.Text;

                        string propertyName = "";
                        switch (columnName)
                        {
                            case "Description":
                                propertyName = "Revision_Description";
                                break;
                            case "Issued By":
                                propertyName = "Revision_IssuedBy";
                                break;
                            case "Issued To":
                                propertyName = "Revision_IssuedTo";
                                break;
                            case "Date":
                                propertyName = "Revision_Date";
                                break;
                        }

                        bool databaseUpdated = SheetDataWriter.ChangeRevisionItem(oldRevision.Id.ToString(), propertyName, propertyValue);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridRow row = DataGridUtils.FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);
                if (null != row)
                {
                    RevitRevision revision = row.Item as RevitRevision;
                    if (null != revision)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Title = "Open a Revision Document";
                        openFileDialog.Filter = "All files (*.*)|*.*";

                        if ((bool)openFileDialog.ShowDialog())
                        {
                            string fileName = openFileDialog.FileName;
                            if (File.Exists(fileName))
                            {
                                int index = rvtSheetData.Revisions.IndexOf(revision);
                                RevisionDocument revisionDoc = revision.Document;
                                revisionDoc.Path = fileName;
                                revisionDoc.Title = System.IO.Path.GetFileName(fileName);
                                if (revisionDoc.Id == Guid.Empty)
                                {
                                    revisionDoc.Id = Guid.NewGuid();
                                    this.RvtSheetData.Revisions[index].Document.Id =  revisionDoc.Id;
                                }
                                this.RvtSheetData.Revisions[index].Document.Path =  revisionDoc.Path;
                                this.RvtSheetData.Revisions[index].Document.Title = revisionDoc.Title;

                                bool databaseUpdated = SheetDataWriter.UpdateRevisionDocument(revision, revisionDoc);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        #region Filter Function
        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (null != comboBoxField.SelectedItem)
                {
                    string fieldName = comboBoxField.SelectedItem.ToString();
                    string searchText = textBoxSearch.Text;
                    SearchByText(fieldName, searchText);
                }
            }
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxField.SelectedItem)
                {
                    string fieldName = comboBoxField.SelectedItem.ToString();
                    string searchText = textBoxSearch.Text;
                    SearchByText(fieldName, searchText);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void SearchByText(string fieldName, string searchText)
        {
            try
            {
                ICollectionView cv = CollectionViewSource.GetDefaultView(dataGridRevision.ItemsSource);
                if (!string.IsNullOrEmpty(searchText))
                {

                    switch (fieldName)
                    {
                        case "Description":
                            cv.Filter = o => { RevitRevision revision = o as RevitRevision; return (revision.Description.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Issued By":
                            cv.Filter = o => { RevitRevision revision = o as RevitRevision; return (revision.IssuedBy.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Issued To":
                            cv.Filter = o => { RevitRevision revision = o as RevitRevision; return (revision.IssuedTo.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Date":
                            cv.Filter = o => { RevitRevision revision = o as RevitRevision; return (revision.Date.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Document":
                            cv.Filter = o => { RevitRevision revision = o as RevitRevision; return (revision.Document.Path.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                       
                    }
                }
                else
                {
                    cv.Filter = null;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        #endregion

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool added = false;
                var revisionDescriptions = from revision in rvtSheetData.Revisions select revision.Description;
                for (int i = 1; i < 100; i++)
                {
                    string revisionDescription = "New Revision " + i;
                    if (!revisionDescriptions.Contains(revisionDescription))
                    {
                        RevitRevision rvtRevision = new RevitRevision(Guid.NewGuid(), revisionDescription, "", "", "");
                        this.RvtSheetData.Revisions.Add(rvtRevision);
                        bool sheetDBUpdated = SheetDataWriter.ChangeRevisionItem(rvtRevision, CommandType.INSERT);

                        //update Revision On Sheet
                        if (rvtSheetData.Sheets.Count > 0)
                        {
                            List<RevisionOnSheet> rosList = new List<RevisionOnSheet>();
                            for (int sheetIndex = 0; sheetIndex < rvtSheetData.Sheets.Count; sheetIndex++)
                            {
                                RevisionOnSheet ros = new RevisionOnSheet(Guid.NewGuid(), rvtSheetData.Sheets[sheetIndex].Id, rvtRevision, false);
                                if (!rvtSheetData.Sheets[sheetIndex].SheetRevisions.ContainsKey(ros.RvtRevision.Id))
                                {
                                    this.RvtSheetData.Sheets[sheetIndex].SheetRevisions.Add(ros.RvtRevision.Id, ros);
                                    rosList.Add(ros);
                                }
                            }

                            bool rosDBUpdated = SheetDataWriter.InsertMultipleRevisionOnSheet(rosList);
                            added = rosDBUpdated;
                        }
                        else
                        {
                            added = true;
                        }

                        break;
                    }
                }
                if (!added)
                {
                    MessageBox.Show("Please assign descriptions of revision items before you add more revisions.", "Revision Description", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRevision.SelectedItems)
                {
                    StringBuilder strBuilder = new StringBuilder();
                    strBuilder.AppendLine("Would you like to delete following revision items and related information?\n");

                    List<RevitRevision> revisions = new List<RevitRevision>();
                    foreach (object selectedItem in dataGridRevision.SelectedItems)
                    {
                        RevitRevision rvtRevision = selectedItem as RevitRevision;
                        strBuilder.AppendLine("\"" + rvtRevision.Description + "\"");
                        revisions.Add(rvtRevision);
                    }

                    if (revisions.Count > 0)
                    {
                        MessageBoxResult msgResult = MessageBox.Show(strBuilder.ToString(), "Delete Revisions", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgResult == MessageBoxResult.Yes)
                        {
                            foreach (RevitRevision rvtRevision in revisions)
                            {
                                this.RvtSheetData.Revisions.Remove(rvtRevision);
                                bool revisionDeleted = SheetDataWriter.ChangeRevisionItem(rvtRevision, CommandType.DELETE);
                                bool rosDeleted = SheetDataWriter.DeleteRevisionOnSheet("RevisionsOnSheet_Revision_Id", rvtRevision.Id.ToString());
                                bool linkedDeleted = SheetDataWriter.ChangeLinkedRevision(rvtRevision.Id, "LinkedRevision_Revision_Id", rvtRevision.Id.ToString(), CommandType.DELETE);
                            }

                            for (int sheetIndex = 0; sheetIndex < rvtSheetData.Sheets.Count; sheetIndex++)
                            {
                                foreach (RevitRevision rvtRevision in revisions)
                                {
                                    if (rvtSheetData.Sheets[sheetIndex].SheetRevisions.ContainsKey(rvtRevision.Id))
                                    {
                                        this.RvtSheetData.Sheets[sheetIndex].SheetRevisions.Remove(rvtRevision.Id);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        #region Linked Revisions
        private void dataGridRevision_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (dataGridRevision.SelectedCells.Count > 0)
                {
                    DataGridCellInfo cellInfo = dataGridRevision.SelectedCells.First();
                    RevitRevision selectedRevision = cellInfo.Item as RevitRevision;
                    if (null != selectedRevision)
                    {
                        dataGridLinks.ItemsSource = null;
                        dataGridLinks.ItemsSource = selectedRevision.LinkedRevisions;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void expanderRevisions_Collapsed(object sender, RoutedEventArgs e)
        {
            try
            {
                expanderRevisions.Header = "Show Linked Sheets";
                GridLength collapsedHeight = new GridLength(40, GridUnitType.Pixel);
                expanderRowDefinition.Height = collapsedHeight;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void expanderRevisions_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                expanderRevisions.Header = "Hide Linked Sheets";
                GridLength expandedHeight = new GridLength(1, GridUnitType.Star);
                expanderRowDefinition.Height = expandedHeight;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        #endregion

    }
}
