using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace HOK.SheetDataEditor
{
    /// <summary>
    /// Interaction logic for RevisionWindow.xaml
    /// </summary>
    public partial class RevisionWindow : Window
    {
        private RevitSheetData sheetData = null;
        private Dictionary<Guid, RevitRevision> itemDictionary = new Dictionary<Guid, RevitRevision>();
        private CellCopyInfo copyInfo = new CellCopyInfo();
        private ObservableCollection<object> revisionItems = new ObservableCollection<object>();
        private bool cellIsEditing = false;
        private List<int> tempDragRows = new List<int>();

        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }

        public RevisionWindow(RevitSheetData data)
        {
            sheetData = data;
            itemDictionary = sheetData.Revisions.ToDictionary(o => o.Key, o => o.Value);
            InitializeComponent();
            DisplayRevisionItems();
        }

        private void DisplayRevisionItems()
        {
            try
            {
                dataGridRevision.ItemsSource = null;
                foreach (RevitRevision revision in itemDictionary.Values)
                {
                    revisionItems.Add(revision);
                }
                bool dummyAdded = false;
                if (revisionItems.Count == 0)
                {
                    revisionItems.Add(new RevitRevision());
                    dummyAdded = true;
                }
                dataGridRevision.ItemsSource = revisionItems;
                if (dummyAdded)
                {
                    revisionItems.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display revision items.\n"+ex.Message, "Display Revision Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridRevision_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGridCell cell = DataGridUtils.FindVisualParent<DataGridCell>(e.OriginalSource as UIElement);
                if (e.RightButton == MouseButtonState.Pressed && null != cell)
                {
                    copyInfo = DataGridUtils.FindDragSource(dataGridRevision, dataGridRevision.SelectedCells);
                    DataObject dataObject = new DataObject(copyInfo);
                    tempDragRows = new List<int>();
                    DragDrop.DoDragDrop(cell, dataObject, DragDropEffects.Copy);
                }
                else
                {
                    copyInfo = new CellCopyInfo();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridRevision_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                //define drop target && draw rectangle
                e.Effects = DragDropEffects.Copy;
                if (copyInfo.RowSourceStartIndex != -1 && copyInfo.ColumnSourceStartIndex != -1)
                {
                    DataGridCell cell = DataGridUtils.FindVisualParent<DataGridCell>(e.OriginalSource as UIElement);
                    if (null != cell)
                    {
                        DataGridRow row = DataGridUtils.FindVisualParent<DataGridRow>(cell);
                        if (null != row)
                        {
                            int rowIndex = row.GetIndex();
                            if (copyInfo.RowSourceEndIndex <= rowIndex)
                            {
                                copyInfo.RowTargetEndIndex = rowIndex;
                            }

                            //visualize cell update
                            DataGridUtils.PaintDragCells(dataGridRevision, copyInfo, true);

                            tempDragRows = tempDragRows.OrderBy(o => o).ToList();
                            for (int i = tempDragRows.Count - 1; i > -1; i--)
                            {
                                int index = tempDragRows[i];
                                if (index > rowIndex)
                                {
                                    revisionItems.RemoveAt(index);
                                    tempDragRows.RemoveAt(i);
                                }
                            }
                        }
                    }
                    else
                    {
                        //add cells for drop target
                        DataGridColumnHeader columnHeader = DataGridUtils.FindVisualParent<DataGridColumnHeader>(e.OriginalSource as UIElement);
                        if (null != columnHeader) { return; }

                        Rect gridRect = VisualTreeHelper.GetDescendantBounds(dataGridRevision);
                        Point point = e.GetPosition(dataGridRevision);
                        if (gridRect.Contains(point))
                        {
                            revisionItems.Add(new RevitRevision());
                            tempDragRows.Add(revisionItems.Count - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridRevision_Drop(object sender, DragEventArgs e)
        {
            try
            {
                //copy elements
                if (e.AllowedEffects == DragDropEffects.Copy)
                {
                    if (copyInfo.RowSourceStartIndex != -1 && copyInfo.ColumnSourceStartIndex != -1)
                    {
                        bool copied = DataGridUtils.CopyRowItems(dataGridRevision, copyInfo);
                    }
                }

                copyInfo = new CellCopyInfo();
                DataGridUtils.PaintDragCells(dataGridRevision, copyInfo, false);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridRevision_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!cellIsEditing && e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
                {
                    foreach (DataGridCellInfo cellInfo in dataGridRevision.SelectedCells)
                    {
                        DataGridUtils.SetPropertyValue(cellInfo.Item, cellInfo.Column.SortMemberPath, null);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridRevision_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            cellIsEditing = true;
        }

        private void dataGridRevision_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            cellIsEditing = false;
        }

        private void buttonDocument_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = DataGridUtils.FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);
            if (null != row)
            {
                int rowIndex = row.GetIndex();
                RevitRevision revision = revisionItems[rowIndex] as RevitRevision;
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
                            Guid docId = revision.Document.Id;
                            if (docId == Guid.Empty)
                            {
                                revision.Document = new RevisionDocument(Guid.NewGuid(), fileName);
                            }
                            else
                            {
                                revision.Document = new RevisionDocument(docId, fileName);
                            }
                        }
                        revisionItems.RemoveAt(rowIndex);
                        revisionItems.Insert(rowIndex, revision);
                    }
                }
            }
        }

        private bool CleanEmptyItems()
        {
            bool result = false;
            try
            {
                revisionItems = (ObservableCollection<object>)dataGridRevision.ItemsSource;
                itemDictionary = new Dictionary<Guid, RevitRevision>();
                foreach (RevitRevision rRevision in revisionItems)
                {
                    RevitRevision newItem = rRevision;
                    if (string.IsNullOrEmpty(newItem.Description)) { continue; }
                    if (newItem.Id == Guid.Empty)
                    {
                        newItem.Id = Guid.NewGuid();
                    }
                    if (!itemDictionary.ContainsKey(newItem.Id))
                    {
                        itemDictionary.Add(newItem.Id, newItem);
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CleanEmptyItems();
                //insert into database

                Dictionary<Guid, RevitRevision> existingItems = sheetData.Revisions;
                foreach (Guid itemId in itemDictionary.Keys)
                {
                    RevitRevision revisionItem = itemDictionary[itemId];
                    if (existingItems.ContainsKey(itemId))
                    {
                        //update
                        bool updated = DatabaseUtil.ChangeRevisionItem(revisionItem, CommandType.UPDATE);
                        existingItems.Remove(itemId);
                    }
                    else
                    {
                        //insert new item
                        bool inserted = DatabaseUtil.ChangeRevisionItem(revisionItem, CommandType.INSERT);
                    }
                }

                foreach (RevitRevision revisionItem in existingItems.Values)
                {
                    //remove deleted items
                    bool deleted = DatabaseUtil.ChangeRevisionItem(revisionItem, CommandType.DELETE);
                }

                sheetData.Revisions = itemDictionary;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply changes on Revisions.\n" + ex.Message, "Apply Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void expanderRevisions_Collapsed(object sender, RoutedEventArgs e)
        {
            try
            {
                expanderRevisions.Header = "Show Linked Revisions";
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
                expanderRevisions.Header = "Hide Linked Revisions";
                GridLength expandedHeight = new GridLength(1, GridUnitType.Star);
                expanderRowDefinition.Height = expandedHeight;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

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
                        var linkedRevisions = from link in sheetData.LinkedRevisions.Values where link.RevisionId == selectedRevision.Id select link;
                        if (linkedRevisions.Count() > 0)
                        {
                            dataGridLinks.ItemsSource = linkedRevisions.ToList(); 
                        }
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
