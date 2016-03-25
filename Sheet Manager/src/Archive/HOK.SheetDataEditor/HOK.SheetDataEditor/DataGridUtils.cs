using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace HOK.SheetDataEditor
{
    public static class DataGridUtils
    {
        public static T FindVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = FindVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        public static CellCopyInfo FindDragSource(DataGrid datagrid, IList<DataGridCellInfo> selectedCells)
        {
            CellCopyInfo copyInfo = new CellCopyInfo();
            try
            {
                ObservableCollection<object> bindingItems = datagrid.ItemsSource as ObservableCollection<object>;
                
                foreach (DataGridCellInfo cellInfo in selectedCells)
                {
                    int columnIndex = cellInfo.Column.DisplayIndex;
                    if (copyInfo.ColumnSourceStartIndex == -1 && copyInfo.ColumnSourceEndIndex == -1)
                    {
                        copyInfo.ColumnSourceStartIndex = columnIndex;
                        copyInfo.ColumnSourceEndIndex = columnIndex;
                    }
                    else if (columnIndex < copyInfo.ColumnSourceStartIndex)
                    {
                        copyInfo.ColumnSourceStartIndex = columnIndex;
                    }
                    else if (columnIndex > copyInfo.ColumnSourceEndIndex)
                    {
                        copyInfo.ColumnSourceEndIndex = columnIndex;
                    }

                    
                    int rowIndex = bindingItems.IndexOf(cellInfo.Item);
                    if (rowIndex > -1)
                    {
                        if (copyInfo.RowSourceStartIndex == -1 && copyInfo.RowSourceEndIndex == -1)
                        {
                            copyInfo.RowSourceStartIndex = rowIndex;
                            copyInfo.RowSourceEndIndex = rowIndex;
                        }
                        else if (rowIndex < copyInfo.RowSourceStartIndex)
                        {
                            copyInfo.RowSourceStartIndex = rowIndex;
                        }
                        else if (rowIndex > copyInfo.RowSourceEndIndex)
                        {
                            copyInfo.RowSourceEndIndex = rowIndex;
                        }
                    }
                }

                copyInfo.MultipleCells = (copyInfo.RowSourceStartIndex!=copyInfo.RowSourceEndIndex) ? true : false;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return copyInfo;
        }

        public static void PaintDragCells(DataGrid datagrid, CellCopyInfo copyInfo, bool dragActive)
        {
            try
            {
                List<DataGridCell> allCells = new List<DataGridCell>();
                for (int i = 0; i < datagrid.Items.Count; i++)
                {
                    for (int j = 0; j < datagrid.Columns.Count; j++)
                    {
                        DataGridCell cell = GetCell(datagrid, i, j);
                        TextBlock textBlock = FindVisualChild<TextBlock>(cell);
                        if (dragActive)
                        {
                            if(j >= copyInfo.ColumnSourceStartIndex && j <= copyInfo.ColumnSourceEndIndex && i > copyInfo.RowSourceEndIndex && i <= copyInfo.RowTargetEndIndex)
                            {
                                textBlock.Background = Brushes.AliceBlue;
                            }
                            else
                            {
                                textBlock.Background = null;
                            }
                        }
                        else
                        {
                            textBlock.Background = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static bool CopyRowItems(DataGrid datagrid, CellCopyInfo copyInfo)
        {
            bool copied = false;
            try
            {
                ObservableCollection<object> bindingItems = datagrid.ItemsSource as ObservableCollection<object>;
                Dictionary<int, string> columnNames = GetColumnNames(datagrid);
                Dictionary<int, double> numericSteps = FindNumericStep(datagrid, copyInfo);

                int copiedRow = 0;
                int numSource = copyInfo.RowSourceEndIndex - copyInfo.RowSourceStartIndex + 1;
                object lastSourceItem = bindingItems[copyInfo.RowSourceEndIndex];

                for (int i = copyInfo.RowSourceEndIndex + 1; i < copyInfo.RowTargetEndIndex + 1; i++)
                {
                    if (i == bindingItems.Count) { break; }
                   
                    object targetItem = bindingItems[i];
                    int addIndex = copiedRow % numSource;
                    int sourceIndex = copyInfo.RowSourceStartIndex + addIndex;
                    object sourceItem = bindingItems[sourceIndex];

                    for (int j = copyInfo.ColumnSourceStartIndex; j < copyInfo.ColumnSourceEndIndex + 1; j++)
                    {
                        if (columnNames.ContainsKey(j))
                        {
                            string columnName = columnNames[j];
                            if (numericSteps.ContainsKey(j))
                            {
                                //numeric increase copy
                                double step = numericSteps[j];

                                object sourceValue = GetPropertyValue(lastSourceItem, columnName);
                                double sourceSuffix = 0;
                                bool sourceSuffixFound = GetSuffix(sourceValue, out sourceSuffix);

                                if (sourceSuffixFound)
                                {
                                    double suffixValue = sourceSuffix + ((double)(copiedRow + 1) * step);
                                    string targetValue = sourceValue.ToString().Replace(sourceSuffix.ToString(), suffixValue.ToString());
                                    object convertedValue = Convert.ChangeType(targetValue, sourceValue.GetType());
                                    targetItem = SetPropertyValue(targetItem, columnName, convertedValue);
                                }
                            }
                            else
                            {
                                //regular copy and paste
                                object sourceValue = GetPropertyValue(sourceItem, columnName);
                                targetItem = SetPropertyValue(targetItem, columnName, sourceValue);
                            }
                        }
                    }

                    bindingItems.RemoveAt(i);
                    bindingItems.Insert(i, targetItem);
                    copiedRow++;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return copied;
        }

        public static ObservableCollection<object> CleanUpItemSource(DataGrid datagrid)
        {
            ObservableCollection<object> bindingItems = datagrid.ItemsSource as ObservableCollection<object>;
            try
            {
                Dictionary<int, string> columnNames = GetColumnNames(datagrid);
                List<string> propertyNames = columnNames.Values.ToList();
                for (int i = bindingItems.Count-1; i >-1; i--)
                {
                    //finding empty items
                    object item = bindingItems[i];
                    if (IsNewItem(item))
                    {
                        bool hasValue = false;
                        foreach (string pName in propertyNames)
                        {
                            object value = GetPropertyValue(item, pName);
                            if (!string.IsNullOrEmpty(value.ToString()))
                            {
                                hasValue = true;
                                break;
                            }
                        }
                        if (!hasValue)
                        {
                            bindingItems.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return bindingItems;
        }

        private static bool IsNewItem(object item)
        {
            bool isNewItem = false;
            try
            {
                if (item.GetType() == typeof(ReplaceItem))
                {
                    ReplaceItem replaceItem = item as ReplaceItem;
                    if (replaceItem.ItemId == Guid.Empty)
                    {
                        isNewItem = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return isNewItem;
        }

        private static Dictionary<int/*columnIndex*/, double/*step*/> FindNumericStep(DataGrid datagrid, CellCopyInfo copyInfo)
        {
            Dictionary<int, double> steps = new Dictionary<int, double>();
            try
            {
                ObservableCollection<object> bindingItems = datagrid.ItemsSource as ObservableCollection<object>;
                Dictionary<int, string> columnNames = GetColumnNames(datagrid);

                if (copyInfo.MultipleCells)
                {
                    for (int j = copyInfo.ColumnSourceStartIndex; j < copyInfo.ColumnSourceEndIndex + 1; j++)
                    {
                        double step = 0;
                        for (int i = copyInfo.RowSourceStartIndex; i < copyInfo.RowSourceEndIndex; i++)
                        {
                            object currentValue = GetPropertyValue(bindingItems[i], columnNames[j]);
                            double currentSuffix = 0;
                            bool currentSuffixFound = GetSuffix(currentValue, out currentSuffix);

                            object nextValue = GetPropertyValue(bindingItems[i + 1], columnNames[j]);
                            double nextSuffix = 0;
                            bool nextSuffixFound = GetSuffix(nextValue, out nextSuffix);

                            if (currentSuffixFound && nextSuffixFound)
                            {
                                double tempStep = nextSuffix - currentSuffix;
                                if (step == 0) { step = tempStep; }
                                else if (step != tempStep) { break; }
                            }
                            else
                            {
                                break;
                            }

                        }
                        if (step != 0)
                        {
                            steps.Add(j, step);
                        }
                    }
                }
                else
                {
                    for (int j = copyInfo.ColumnSourceStartIndex; j < copyInfo.ColumnSourceEndIndex + 1; j++)
                    {
                        object value = GetPropertyValue(bindingItems[copyInfo.RowSourceStartIndex], columnNames[j]);
                        double suffix = 0;
                        bool suffixFound = GetSuffix(value, out suffix);
                        if (suffixFound)
                        {
                            steps.Add(j, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return steps;
        }

        private static bool GetSuffix(object value, out double suffix)
        {
            bool suffixFound = false;
            suffix = 0;
            try
            {
                if (value.GetType() == typeof(string))
                {
                    string suffixStr = new string(value.ToString().Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());
                    if (!string.IsNullOrEmpty(suffixStr))
                    {
                        if (double.TryParse(suffixStr, out suffix))
                        {
                            suffixFound = true;
                        }
                    }
                }
                else if (value.GetType() == typeof(double))
                {
                    suffix = (double)value;
                    suffixFound = true;
                }
                else if (value.GetType() == typeof(int))
                {
                    suffix = (double)value;
                    suffixFound = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return suffixFound;
        }

        private static Dictionary<int, string> GetColumnNames(DataGrid grid)
        {
            Dictionary<int, string> columnNames = new Dictionary<int, string>();
            try
            {
                foreach (DataGridColumn column in grid.Columns)
                {
                    if (!columnNames.ContainsKey(column.DisplayIndex))
                    {
                        columnNames.Add(column.DisplayIndex, column.SortMemberPath);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return columnNames;
        }

        public static DataGridCell GetCell(DataGrid dataGrid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(dataGrid, row);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        public static DataGridRow GetRow(DataGrid dataGrid, int index)
        {
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            DataGridCell cell = null;
            if (cellInfo.IsValid)
            {
                FrameworkElement cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
                if (null != cellContent)
                {
                    cell = cellContent.Parent as DataGridCell;
                }
            }
            return cell;
        }

        public static object GetPropertyValue(object item, string propertyName)
        {
            object value = null;
            try
            {
                value = item.GetType().GetProperty(propertyName).GetValue(item, null);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return value;
        }

        public static object SetPropertyValue(object item, string propertyName, object value)
        {
            object updatedItem = item;
            try
            {
                updatedItem.GetType().GetProperty(propertyName).SetValue(updatedItem, value);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updatedItem;
        }
    }

    public class CellCopyInfo
    {
        private bool multipleCells = false;
        private int rowSourceStartIndex = -1;
        private int rowSourceEndIndex = -1;
        private int rowTargetEndIndex = -1;
        private int columnSourceStartIndex = -1;
        private int columnSourceEndIndex = -1;
        //only row copy is allowed

        public bool MultipleCells { get { return multipleCells; } set { multipleCells = value; } }
        public int RowSourceStartIndex { get { return rowSourceStartIndex; } set { rowSourceStartIndex = value; } }
        public int RowSourceEndIndex { get { return rowSourceEndIndex; } set { rowSourceEndIndex = value; } }
        public int RowTargetEndIndex { get { return rowTargetEndIndex; } set { rowTargetEndIndex = value; } }
        public int ColumnSourceStartIndex { get { return columnSourceStartIndex; } set { columnSourceStartIndex = value; } }
        public int ColumnSourceEndIndex { get { return columnSourceEndIndex; } set { columnSourceEndIndex = value; } }

        public CellCopyInfo()
        {
        }
    }
}
