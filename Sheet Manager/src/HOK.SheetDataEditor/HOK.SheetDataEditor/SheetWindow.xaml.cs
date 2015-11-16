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
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class SheetWindow : Window
    {
        private RevitSheetData sheetData = null;
        private Dictionary<Guid, RevitSheet> itemDictionary = new Dictionary<Guid, RevitSheet>();
        private CellCopyInfo copyInfo = new CellCopyInfo();
        private ObservableCollection<object> sheetItems = new ObservableCollection<object>();
        private bool cellIsEditing = false;
        private List<int> tempDragRows = new List<int>();
        

        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }

        public SheetWindow(RevitSheetData data)
        {
            sheetData = data;
            itemDictionary = sheetData.Sheets.ToDictionary(o => o.Key, o => o.Value);
            InitializeComponent();

            DisplaySheetItems();
        }

        private void DisplaySheetItems()
        {
            try
            {
                List<Discipline> disciplines = sheetData.Disciplines.Values.OrderBy(o => o.Name).ToList();
                dataGridDisciplineComboBox.ItemsSource = null;
                dataGridDisciplineComboBox.ItemsSource = disciplines;
                dataGridDisciplineComboBox.DisplayMemberPath = "Name";

                dataGridSheet.ItemsSource = null;
                List<RevitSheet> sheetList = itemDictionary.Values.OrderBy(o => o.Number).ToList();
                foreach (RevitSheet sheet in sheetList)
                {
                    sheetItems.Add(sheet);
                }
                bool dummyAdded = false;
                if (sheetItems.Count == 0)
                {
                    sheetItems.Add(new RevitSheet());
                    dummyAdded = true;
                }
                dataGridSheet.ItemsSource = sheetItems;
                if (dummyAdded)
                {
                    sheetItems.RemoveAt(0);
                }

                List<string> columnNames = new List<string>();
                foreach (DataGridColumn column in dataGridSheet.Columns)
                {
                    columnNames.Add(column.Header.ToString());
                }
                comboBoxField.ItemsSource = columnNames;
                comboBoxField.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display sheet items.\n" + ex.Message, "Display Sheet Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

      

        private void dataGridSheet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //collect drag source
                DataGridCell cell = DataGridUtils.FindVisualParent<DataGridCell>(e.OriginalSource as UIElement);
                if (e.RightButton == MouseButtonState.Pressed && null != cell)
                {
                    copyInfo = DataGridUtils.FindDragSource(dataGridSheet, dataGridSheet.SelectedCells);
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

        private void dataGridSheet_DragOver(object sender, DragEventArgs e)
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
                            DataGridUtils.PaintDragCells(dataGridSheet, copyInfo, true);

                            tempDragRows = tempDragRows.OrderBy(o => o).ToList();
                            for (int i = tempDragRows.Count - 1; i > -1; i--)
                            {
                                int index = tempDragRows[i];
                                if (index > rowIndex)
                                {
                                    sheetItems.RemoveAt(index);
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

                        Rect gridRect = VisualTreeHelper.GetDescendantBounds(dataGridSheet);
                        Point point = e.GetPosition(dataGridSheet);
                        if (gridRect.Contains(point))
                        {
                            sheetItems.Add(new RevitSheet());
                            tempDragRows.Add(sheetItems.Count - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridSheet_Drop(object sender, DragEventArgs e)
        {
            try
            {
                //copy elements
                if (e.AllowedEffects == DragDropEffects.Copy)
                {
                    if (copyInfo.RowSourceStartIndex != -1 && copyInfo.ColumnSourceStartIndex != -1)
                    {
                        bool copied = DataGridUtils.CopyRowItems(dataGridSheet, copyInfo);
                    }
                }

                copyInfo = new CellCopyInfo();
                DataGridUtils.PaintDragCells(dataGridSheet, copyInfo, false);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool CleanEmptyItems()
        {
            bool result = false;
            try
            {
                sheetItems = (ObservableCollection<object>)dataGridSheet.ItemsSource;
                itemDictionary = new Dictionary<Guid, RevitSheet>();
                foreach (RevitSheet rSheet in sheetItems)
                {
                    RevitSheet newItem = rSheet;
                    if (string.IsNullOrEmpty(newItem.Number) || string.IsNullOrEmpty(newItem.Name)) { continue; }
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

                Dictionary<Guid, RevitSheet> existingItems = sheetData.Sheets;
                foreach (Guid itemId in itemDictionary.Keys)
                {
                    RevitSheet sheetItem = itemDictionary[itemId];
                    if (existingItems.ContainsKey(itemId))
                    {
                        //update
                        bool updated = DatabaseUtil.ChangeSheetItem(sheetItem, CommandType.UPDATE);
                        existingItems.Remove(itemId);
                    }
                    else
                    {
                        //insert new item
                        bool inserted = DatabaseUtil.ChangeSheetItem(sheetItem, CommandType.INSERT);
                    }
                }

                foreach (RevitSheet sheetItem in existingItems.Values)
                {
                    //remove deleted items
                    bool deleted = DatabaseUtil.ChangeSheetItem(sheetItem, CommandType.DELETE);
                }

                sheetData.Sheets = itemDictionary;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply changes into the database.\n" + ex.Message, "Apply Changes on Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void SearchByText(string fieldName, string searchText)
        {
            try
            {
                ICollectionView cv = CollectionViewSource.GetDefaultView(dataGridSheet.ItemsSource);
                if (!string.IsNullOrEmpty(searchText))
                {
                    switch (fieldName)
                    {
                        case "Sheet Number":
                            cv.Filter = o => { RevitSheet sheet = o as RevitSheet; return (sheet.Number.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Sheet Name":
                            cv.Filter = o => { RevitSheet sheet = o as RevitSheet; return (sheet.Name.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Volume Number":
                            cv.Filter = o => { RevitSheet sheet = o as RevitSheet; return (sheet.VolumeNumber.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Discipline":
                            cv.Filter = o => { RevitSheet sheet = o as RevitSheet; return (sheet.DisciplineObj.Name.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Drawing Type":
                            cv.Filter = o => { RevitSheet sheet = o as RevitSheet; return (sheet.DrawingType.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        case "Sorted Discipline":
                            cv.Filter = o => { RevitSheet sheet = o as RevitSheet; return (sheet.SortedDiscipline.ToUpper().Contains(searchText.ToUpper())); };
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

        private void expanderSheets_Collapsed(object sender, RoutedEventArgs e)
        {
            try
            {
                expanderSheets.Header = "Show Linked Sheets";
                GridLength collapsedHeight = new GridLength(40, GridUnitType.Pixel);
                expanderRowDefinition.Height = collapsedHeight;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void expanderSheets_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                expanderSheets.Header = "Hide Linked Sheets";
                GridLength expandedHeight = new GridLength(1, GridUnitType.Star);
                expanderRowDefinition.Height = expandedHeight;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridSheet_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (dataGridSheet.SelectedCells.Count > 0)
                {
                    DataGridCellInfo cellInfo = dataGridSheet.SelectedCells.First();
                    RevitSheet selectedSheet = cellInfo.Item as RevitSheet;
                    if (null != selectedSheet)
                    {
                        dataGridLinks.ItemsSource = null;
                        var linkedSheets = from link in sheetData.LinkedSheets.Values where link.SheetId == selectedSheet.Id select link;
                        if (linkedSheets.Count() > 0)
                        {
                            dataGridLinks.ItemsSource = linkedSheets.ToList();
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
