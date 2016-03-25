using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace HOK.SheetDataEditor
{
    /// <summary>
    /// Interaction logic for ReplaceWindow.xaml
    /// </summary>
    public partial class ReplaceWindow : Window
    {
        private RevitSheetData sheetData = null;
        private Dictionary<Guid, ReplaceItem> itemDictionary = new Dictionary<Guid, ReplaceItem>();
        private string[] itemType = new string[] { "Sheet", "View" };
        private string[] sheetParameters = new string[] { "Sheet Name", "Sheet Number" };
        private string[] viewParameters = new string[] { "View Name" };
        private ReplaceType selectedType = ReplaceType.None;
        private string selectedParameter = "";
        private bool isEditEnding = false;
        private List<int> tempDragRows = new List<int>();

        private ObservableCollection<object> replaceItems = new ObservableCollection<object>();
        private CellCopyInfo copyInfo = new CellCopyInfo();

        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }
       
        public ReplaceWindow(RevitSheetData data)
        {
            sheetData = data;
            itemDictionary = sheetData.ReplaceItems.ToDictionary(o => o.Key, o => o.Value);
            InitializeComponent();
            comboBoxType.ItemsSource = itemType;
            comboBoxType.SelectedIndex = 0;
        }

        private void DisplayItems()
        {
            try
            {
                replaceItems = new ObservableCollection<object>();

                dataGridItem.ItemsSource = null;
                bool dummyAdded = false;
                if (selectedType!=ReplaceType.None && !string.IsNullOrEmpty(selectedParameter))
                {
                    var mappedItems = from item in itemDictionary.Values where item.ItemType == selectedType && item.ParameterName == selectedParameter select item;
                    if (mappedItems.Count() > 0)
                    {
                        mappedItems = mappedItems.OrderBy(o => o.SourceValue).ToList();
                        foreach (ReplaceItem item in mappedItems)
                        {
                            replaceItems.Add(item);
                        }
                    }
                }
                if (replaceItems.Count == 0)
                {
                    replaceItems.Add(new ReplaceItem());
                    dummyAdded = true;
                }
                dataGridItem.ItemsSource = replaceItems;
                if (dummyAdded)
                {
                    replaceItems.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display items.\n"+ex.Message, "Display Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveChanges()
        {
            try
            {
                if (selectedType != ReplaceType.None && !string.IsNullOrEmpty(selectedParameter))
                {
                    if (null != dataGridItem.ItemsSource)
                    {

                        var mappedItems = from item in itemDictionary.Values where item.ItemType == selectedType && item.ParameterName == selectedParameter select item;
                        if (mappedItems.Count() > 0)
                        {
                            List<ReplaceItem> itemList = mappedItems.ToList();
                            foreach (ReplaceItem rItem in itemList)
                            {
                                if (itemDictionary.ContainsKey(rItem.ItemId))
                                {
                                    itemDictionary.Remove(rItem.ItemId);
                                }
                            }
                        }

                        replaceItems = (ObservableCollection<object>)dataGridItem.ItemsSource;
                        foreach (ReplaceItem rItem in replaceItems)
                        {
                            ReplaceItem newItem = rItem;
                            if (rItem.ItemId == Guid.Empty)
                            {
                                if (string.IsNullOrEmpty(rItem.SourceValue) && string.IsNullOrEmpty(rItem.TargetValue)) { continue; }
                                newItem = new ReplaceItem(Guid.NewGuid(), selectedType, selectedParameter, rItem.SourceValue, rItem.TargetValue);
                            }
                            if (!itemDictionary.ContainsKey(newItem.ItemId))
                            {
                                itemDictionary.Add(newItem.ItemId, newItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save changes on items.\n" + ex.Message, "Save Changes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxType.SelectedItem)
                {
                    SaveChanges();
                    selectedType = (ReplaceType)Enum.Parse(typeof(ReplaceType), comboBoxType.SelectedItem.ToString());
                    if (selectedType == ReplaceType.Sheet)
                    {
                        comboBoxParameter.ItemsSource = null;
                        comboBoxParameter.ItemsSource = sheetParameters;
                    }
                    else if (selectedType == ReplaceType.View)
                    {
                        comboBoxParameter.ItemsSource = null;
                        comboBoxParameter.ItemsSource = viewParameters;
                    }
                    comboBoxParameter.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void comboBoxParameter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxParameter.SelectedItem)
                {
                    SaveChanges();
                    selectedParameter = comboBoxParameter.SelectedItem.ToString();
                    DisplayItems();
                }
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

        private void CopyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (null != dataGridItem.SelectedItems)
                {
                    /*
                    copiedItems = new List<ReplaceItem>();
                    foreach (object selectedItem in dataGridItem.SelectedItems)
                    {
                        copiedItems.Add((ReplaceItem)selectedItem);
                    }
                     */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to paste items.\n" + ex.Message, "Paste Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PasteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (dataGridItem.SelectedIndex > -1)
                {/*
                    int startIndex = dataGridItem.SelectedIndex;
                    List<ReplaceItem> items = (List<ReplaceItem>)dataGridItem.ItemsSource;
                    List<ReplaceItem> updatedItems = items;
                    for (int i = 0; i < copiedItems.Count; i++)
                    {
                        int index = startIndex + i;
                        if (index < items.Count)
                        {
                            updatedItems.RemoveAt(index);
                            updatedItems.Insert(index, copiedItems[i]);
                        }
                        else
                        {
                            updatedItems.Add(copiedItems[i]);
                        }
                    }

                    dataGridItem.ItemsSource = null;
                    dataGridItem.ItemsSource = updatedItems;
                  */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to paste items.\n" + ex.Message, "Paste Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedType!=ReplaceType.None && !string.IsNullOrEmpty(selectedParameter))
                {
                    SaveChanges();
                    Dictionary<Guid, ReplaceItem> existingItems = sheetData.ReplaceItems;
                    foreach (Guid itemId in itemDictionary.Keys)
                    {
                        ReplaceItem item = itemDictionary[itemId];
                        if (existingItems.ContainsKey(itemId))
                        {
                            //update
                            bool updated = DatabaseUtil.ChangeReplaceItem(item, CommandType.UPDATE);
                            existingItems.Remove(itemId);
                        }
                        else
                        {
                            //insert new item
                            bool inserted = DatabaseUtil.ChangeReplaceItem(item, CommandType.INSERT);
                        }
                    }


                    foreach (Guid itemId in existingItems.Keys)
                    {
                        //remove deleted items
                        ReplaceItem item = existingItems[itemId];
                        bool deleted = DatabaseUtil.ChangeReplaceItem(item, CommandType.DELETE);
                    }

                    sheetData.ReplaceItems = itemDictionary;
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply the items to be replaced.\n"+ex.Message, "Apply Replace Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //collect drag source
                DataGridCell cell = DataGridUtils.FindVisualParent<DataGridCell>(e.OriginalSource as UIElement);
                if (e.RightButton == MouseButtonState.Pressed && null != cell)
                {
                    copyInfo = DataGridUtils.FindDragSource(dataGridItem, dataGridItem.SelectedCells);
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

        private void dataGridItem_DragOver(object sender, DragEventArgs e)
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
                            DataGridUtils.PaintDragCells(dataGridItem, copyInfo, true);

                            tempDragRows = tempDragRows.OrderBy(o => o).ToList();
                            for (int i = tempDragRows.Count - 1; i > -1; i--)
                            {
                                int index = tempDragRows[i];
                                if (index > rowIndex)
                                {
                                    replaceItems.RemoveAt(index);
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
                        
                        Rect gridRect = VisualTreeHelper.GetDescendantBounds(dataGridItem);
                        Point point = e.GetPosition(dataGridItem);
                        if (gridRect.Contains(point))
                        {
                            replaceItems.Add(new ReplaceItem());
                            tempDragRows.Add(replaceItems.Count - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridItem_Drop(object sender, DragEventArgs e)
        {
            try
            {
                //copy elements
                if (e.AllowedEffects == DragDropEffects.Copy)
                {
                    if (copyInfo.RowSourceStartIndex != -1 && copyInfo.ColumnSourceStartIndex != -1)
                    {
                        bool copied = DataGridUtils.CopyRowItems(dataGridItem, copyInfo);
                    }
                }

                copyInfo = new CellCopyInfo();
                DataGridUtils.PaintDragCells(dataGridItem, copyInfo, false);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridItem_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (isEditEnding) { return; }
            try
            {
                if (null != e.EditingElement && null != e.Column)
                {
                    isEditEnding = true;
                    TextBox textBox = e.EditingElement as TextBox;
                    if (null != textBox && e.Column.SortMemberPath == "SourceValue")
                    {
                        string sourceValue = textBox.Text;
                        var itemFound = from rItem in itemDictionary.Values where rItem.SourceValue == sourceValue select rItem;
                        if (itemFound.Count() > 0)
                        {
                            MessageBoxResult msgResult = MessageBox.Show("[" + sourceValue + "] Item already exists in the list. \nPlease enter a different value.", "Existing Value", MessageBoxButton.OK, MessageBoxImage.Information);
                            if (msgResult == MessageBoxResult.OK)
                            {
                                e.Cancel = true;
                                (sender as DataGrid).CancelEdit(DataGridEditingUnit.Cell);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            finally
            {
                isEditEnding = false;
            }
        }

        
    }

    
}
