using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace HOK.SheetDataEditor
{
    /// <summary>
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        private RevitSheetData sheetData = null;
        private Dictionary<Guid, RevitView> itemDictionary = new Dictionary<Guid, RevitView>();
        private CellCopyInfo copyInfo = new CellCopyInfo();
        private ObservableCollection<object> viewItems = new ObservableCollection<object>();
        private bool cellIsEditing = false;
        private List<int> tempDragRows = new List<int>();

        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }

        public ViewWindow(RevitSheetData data)
        {
            sheetData = data;
            itemDictionary = sheetData.Views.ToDictionary(o => o.Key, o => o.Value);

            InitializeComponent();

            DisplayViewItems();
        }

        private void DisplayViewItems()
        {
            try
            {
                List<RevitSheet> sheets = sheetData.Sheets.Values.OrderBy(o => o.Number).ToList();
                dataGridSheetComboBox.ItemsSource = null;
                dataGridSheetComboBox.ItemsSource = sheets;
                dataGridSheetComboBox.DisplayMemberPath = "Number";

                List<RevitViewType> viewTypes = sheetData.ViewTypes.Values.OrderBy(o => o.Name).ToList();
                dataGridViewTypeComboBox.ItemsSource = null;
                dataGridViewTypeComboBox.ItemsSource = viewTypes;
                dataGridViewTypeComboBox.DisplayMemberPath = "Name";

                dataGridView.ItemsSource = null;
                List<RevitView> viewList = itemDictionary.Values.OrderBy(o => o.Name).ToList();
                foreach (RevitView view in viewList)
                {
                    viewItems.Add(view);
                }
                bool dummyAdded = false;
                if (viewItems.Count == 0)
                {
                    viewItems.Add(new RevitView());
                    dummyAdded = true;
                }
                dataGridView.ItemsSource = viewItems;
                if (dummyAdded)
                {
                    viewItems.RemoveAt(0);
                }

                List<string> columnNames = new List<string>();
                foreach (DataGridColumn column in dataGridView.Columns)
                {
                    columnNames.Add(column.Header.ToString());
                }
                comboBoxField.ItemsSource = columnNames;
                comboBoxField.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display view items.\n" + ex.Message, "Display View Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //collect drag source
                DataGridCell cell = DataGridUtils.FindVisualParent<DataGridCell>(e.OriginalSource as UIElement);
                if (e.RightButton == MouseButtonState.Pressed && null != cell)
                {
                    copyInfo = DataGridUtils.FindDragSource(dataGridView, dataGridView.SelectedCells);
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

        private void dataGridView_DragOver(object sender, DragEventArgs e)
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
                            DataGridUtils.PaintDragCells(dataGridView, copyInfo, true);

                            tempDragRows = tempDragRows.OrderBy(o => o).ToList();
                            for (int i = tempDragRows.Count - 1; i > -1; i--)
                            {
                                int index = tempDragRows[i];
                                if (index > rowIndex)
                                {
                                    viewItems.RemoveAt(index);
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

                        Rect gridRect = VisualTreeHelper.GetDescendantBounds(dataGridView);
                        Point point = e.GetPosition(dataGridView);
                        if (gridRect.Contains(point))
                        {
                            viewItems.Add(new RevitView());
                            tempDragRows.Add(viewItems.Count - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                //copy elements
                if (e.AllowedEffects == DragDropEffects.Copy)
                {
                    if (copyInfo.RowSourceStartIndex != -1 && copyInfo.ColumnSourceStartIndex != -1)
                    {
                        bool copied = DataGridUtils.CopyRowItems(dataGridView, copyInfo);
                    }
                }

                copyInfo = new CellCopyInfo();
                DataGridUtils.PaintDragCells(dataGridView, copyInfo, false);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridView_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!cellIsEditing && e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
                {
                    foreach (DataGridCellInfo cellInfo in dataGridView.SelectedCells)
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

        private void dataGridView_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            cellIsEditing = true;
        }

        private void dataGridView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            cellIsEditing = false;
        }

        private bool CleanEmptyItems()
        {
            bool result = false;
            try
            {
                viewItems = (ObservableCollection<object>)dataGridView.ItemsSource;
                itemDictionary = new Dictionary<Guid, RevitView>();
                foreach (RevitView rView in viewItems)
                {
                    RevitView newItem = rView;
                    if (string.IsNullOrEmpty(newItem.Name) || string.IsNullOrEmpty(newItem.Sheet.Number) || string.IsNullOrEmpty(newItem.ViewType.Name)) { continue; }
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

                Dictionary<Guid, RevitView> existingItems = sheetData.Views;
                foreach (Guid itemId in itemDictionary.Keys)
                {
                    RevitView viewItem = itemDictionary[itemId];
                    if (existingItems.ContainsKey(itemId))
                    {
                        //update
                        bool updated = DatabaseUtil.ChangeViewItem(viewItem, CommandType.UPDATE);
                        existingItems.Remove(itemId);
                    }
                    else
                    {
                        //insert new item
                        bool inserted = DatabaseUtil.ChangeViewItem(viewItem, CommandType.INSERT);
                    }
                }

                foreach (RevitView viewItem in existingItems.Values)
                {
                    //remove deleted items
                    bool deleted = DatabaseUtil.ChangeViewItem(viewItem, CommandType.DELETE);
                }

                sheetData.Views = itemDictionary;
                this.DialogResult = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply changes into the database.\n" + ex.Message, "Apply Changes on Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

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
                ICollectionView cv = CollectionViewSource.GetDefaultView(dataGridView.ItemsSource);
                if (!string.IsNullOrEmpty(searchText))
                {
                    switch (fieldName)
                    {
                        case "View Name":
                            cv.Filter = o => { RevitView view = o as RevitView; return (view.Name.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Sheet Number":
                            cv.Filter = o => { RevitView view = o as RevitView; return (view.Sheet.Number.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "View Type":
                            cv.Filter = o => { RevitView view = o as RevitView; return (view.ViewType.Name.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "U":
                            cv.Filter = o => { RevitView view = o as RevitView; return (view.LocationU.ToString().Contains(searchText.ToUpper())); };
                            break;
                        case "V":
                            cv.Filter = o => { RevitView view = o as RevitView; return (view.LocationV.ToString().Contains(searchText.ToUpper())); };
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
    }
}
