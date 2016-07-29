using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace HOK.SheetManager.Utils
{
    public static class DataGridUtils
    {
        public static T FindVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            try
            {
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
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return child;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            try
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
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return null;
        }

        public static void FindDragSource(DataGrid datagrid, IList<DataGridCellInfo> selectedCells, ref CellCopyInfo copyInfo)
        {
            try
            {
                List<object> bindingItems = datagrid.ItemsSource.Cast<object>().ToList();

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

                copyInfo.MultipleCells = (copyInfo.RowSourceStartIndex != copyInfo.RowSourceEndIndex) ? true : false;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void PaintDragCells(DataGrid datagrid, CellCopyInfo copyInfo, bool dragActive)
        {
            try
            {
                for (int i = 0; i < datagrid.Items.Count; i++)
                {
                    for (int j = 0; j < datagrid.Columns.Count; j++)
                    {
                        DataGridCell cell = GetCell(datagrid, i, j);
                        if (null != cell)
                        {
                            TextBlock textBlock = FindVisualChild<TextBlock>(cell);
                            if (null!= textBlock && dragActive)
                            {
                                if (j >= copyInfo.ColumnSourceStartIndex && j <= copyInfo.ColumnSourceEndIndex && i > copyInfo.RowSourceEndIndex && i <= copyInfo.RowTargetEndIndex)
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
                List<object> bindingItems = datagrid.ItemsSource.Cast<object>().ToList();
                List<string> columnNames = (from column in datagrid.Columns select column.Header.ToString()).ToList();
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
                        if (j<columnNames.Count)
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
                                    bool dbUpdated = UpdateDatabaseItem(targetItem, columnName, convertedValue);
                                }
                            }
                            else
                            {
                                //regular copy and paste
                                object sourceValue = GetPropertyValue(sourceItem, columnName);
                                targetItem = SetPropertyValue(targetItem, columnName, sourceValue);
                                bool dbUpdated = UpdateDatabaseItem(targetItem, columnName, sourceValue);
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

        public static List<object> CleanUpItemSource(DataGrid datagrid)
        {
            List<object> bindingItems = datagrid.ItemsSource.Cast<object>().ToList();
            try
            {
                List<string> columnNames = (from column in datagrid.Columns select column.Header.ToString()).ToList();
                for (int i = bindingItems.Count - 1; i > -1; i--)
                {
                    //finding empty items
                    object item = bindingItems[i];
                    if (IsNewItem(item))
                    {
                        bool hasValue = false;
                        foreach (string columnName in columnNames)
                        {
                            object value = GetPropertyValue(item, columnName);
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
                if (item.GetType() == typeof(RevitItemMapper))
                {
                    RevitItemMapper replaceItem = item as RevitItemMapper;
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
                List<object> bindingItems = datagrid.ItemsSource.Cast<object>().ToList();
                List<string> columnNames = (from column in datagrid.Columns select column.Header.ToString()).ToList();

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

        public static bool GetSuffix(object value, out double suffix)
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

        public static DataGridCell GetCell(DataGrid dataGrid, int row, int column)
        {
            try
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
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return null;
        }

        public static DataGridRow GetRow(DataGrid dataGrid, int index)
        {
            try
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
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return null;
        }

        public static DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            DataGridCell cell = null;
            try
            {
                if (cellInfo.IsValid)
                {
                    FrameworkElement cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
                    if (null != cellContent)
                    {
                        cell = cellContent.Parent as DataGridCell;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return cell;
        }

        public static object GetPropertyValue(object item, string propertyName)
        {
            object value = null;
            try
            {
                if (item.GetType() == typeof(RevitSheet))
                {
                    RevitSheet sheet = item as RevitSheet;
                    switch (propertyName)
                    {
                        case "Sheet Number":
                            value = sheet.Number;
                            break;
                        case "Sheet Name":
                            value = sheet.Name;
                            break;
                        case "Discipline":
                            value = sheet.DisciplineObj;
                            break;
                        default:
                            var paramValueFound = from paramVal in sheet.SheetParameters.Values where paramVal.Parameter.ParameterName == propertyName select paramVal;
                            if (paramValueFound.Count() > 0)
                            {
                                SheetParameterValue paramValue = paramValueFound.First();
                                value = paramValue.ParameterValue;
                            }
                            break;

                    }
                }
                else
                {
                    value = item.GetType().GetProperty(propertyName).GetValue(item, null);
                }
                
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
                if (updatedItem.GetType() == typeof(RevitSheet))
                {
                    switch (propertyName)
                    {
                        case "Sheet Number":
                            (updatedItem as RevitSheet).Number = value.ToString();
                            break;
                        case "Sheet Name":
                            (updatedItem as RevitSheet).Name = value.ToString();
                            break;
                        case "Discipline":
                            (updatedItem as RevitSheet).DisciplineObj = value as Discipline;
                            break;
                        default:
                            RevitSheet sheet = updatedItem as RevitSheet;
                            var paramIdFound = from paramVal in sheet.SheetParameters.Values where paramVal.Parameter.ParameterName == propertyName select paramVal.Parameter.ParameterId;
                            if (paramIdFound.Count() > 0)
                            {
                                Guid paramId = paramIdFound.First();
                                if ((updatedItem as RevitSheet).SheetParameters.ContainsKey(paramId))
                                {
                                    (updatedItem as RevitSheet).SheetParameters[paramId].ParameterValue = value.ToString();
                                }
                            }
                            break;
                    }
                }
                else
                {
                    updatedItem.GetType().GetProperty(propertyName).SetValue(updatedItem, value);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updatedItem;
        }

        private static bool UpdateDatabaseItem(object item, string propertyName, object propertyValue)
        {
            bool databaseUpdated = false;
            try
            {
                if (item.GetType() == typeof(RevitSheet))
                {
                    RevitSheet sheet = item as RevitSheet;
                    switch (propertyName)
                    {
                        case "Sheet Number":
                            databaseUpdated = SheetDataWriter.ChangeSheetItem(sheet.Id.ToString(), propertyName, propertyValue.ToString());
                            break;
                        case "Sheet Name":
                            databaseUpdated = SheetDataWriter.ChangeSheetItem(sheet.Id.ToString(), propertyName, propertyValue.ToString());
                            break;
                        case "Discipline":
                            Discipline discipline = propertyValue as Discipline;
                            if (null != discipline)
                            {
                                databaseUpdated = SheetDataWriter.ChangeSheetItem(sheet.Id.ToString(), propertyName, discipline.Id.ToString());
                            }
                            break;
                        default:
                            var paramValueFound = from paramValue in sheet.SheetParameters.Values where paramValue.Parameter.ParameterName == propertyName select paramValue;
                            if (paramValueFound.Count() > 0)
                            {
                                SheetParameterValue paramValue = paramValueFound.First();
                                paramValue.ParameterValue = propertyValue.ToString();
                                databaseUpdated = SheetDataWriter.ChangeSheetParameterValue(paramValue, HOK.SheetManager.Database.CommandType.UPDATE);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return databaseUpdated;
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
