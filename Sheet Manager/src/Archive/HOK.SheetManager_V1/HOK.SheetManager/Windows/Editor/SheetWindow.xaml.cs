using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using HOK.SheetManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.SheetManager.Windows.Editor
{
    /// <summary>
    /// Interaction logic for SheetWindow.xaml
    /// </summary>
    public partial class SheetWindow : Window
    {
        private RevitSheetData rvtSheetData = null;
        private CellCopyInfo copyInfo = new CellCopyInfo();
        private List<int> tempDragRows = new List<int>();

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }

        public SheetWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtSheetData = this.DataContext as RevitSheetData;
                foreach (SheetParameter sheetParameter in rvtSheetData.SheetParameters)
                {
                    DataGridTextColumn txtColumn = new DataGridTextColumn();
                    txtColumn.Header = sheetParameter.ParameterName;
                    txtColumn.Binding = new Binding("SheetParameters[" + sheetParameter.ParameterId + "].ParameterValue");
                    txtColumn.Width = DataGridLength.Auto;
                    dataGridSheet.Columns.Add(txtColumn);
                }

                comboBoxField.ItemsSource = from column in dataGridSheet.Columns select column.Header.ToString();
                comboBoxField.SelectedIndex = 0;

                dataGridDisciplineComboBox.ItemsSource = rvtSheetData.Disciplines;
                dataGridDisciplineComboBox.DisplayMemberPath = "Name";

                rvtSheetData.Sheets.CollectionChanged += OnCollectionChanged;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.NewItems != null && e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (RevitSheet sheet in e.NewItems)
                    {
                        int index = rvtSheetData.Sheets.IndexOf(sheet);
                        Guid sheetId = Guid.NewGuid();
                        this.RvtSheetData.Sheets[index].Id = sheetId;
                        bool sheetDBUpdated = SheetDataWriter.ChangeSheetItem(rvtSheetData.Sheets[index], CommandType.INSERT);

                        foreach (SheetParameter sheetParam in rvtSheetData.SheetParameters)
                        {
                            SheetParameterValue paramValue = new SheetParameterValue();
                            paramValue.ParameterValueId = Guid.NewGuid();
                            paramValue.Parameter = sheetParam;
                            paramValue.SheetId = sheetId;

                            this.RvtSheetData.Sheets[index].SheetParameters.Add(sheetParam.ParameterId, paramValue);
                        }
                        bool sheetParamDBUpdated = SheetDataWriter.InsertMultipleParameterValue(RvtSheetData.Sheets[index].SheetParameters.Values.ToList());

                        foreach (RevitRevision revision in rvtSheetData.Revisions)
                        {
                            RevisionOnSheet ros = new RevisionOnSheet(Guid.NewGuid(), sheetId, revision, false);
                            this.RvtSheetData.Sheets[index].SheetRevisions.Add(revision.Id, ros);
                        }
                        bool rosDBUpdated = SheetDataWriter.InsertMultipleRevisionOnSheet(RvtSheetData.Sheets[index].SheetRevisions.Values.ToList());
                    }
                }
                if (e.OldItems != null && e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (RevitSheet sheet in e.OldItems)
                    {
                        bool sheetDBUpdated = SheetDataWriter.ChangeSheetItem(sheet, CommandType.DELETE);
                        bool sheetParamDBUpdated = SheetDataWriter.DeleteSheetParameterValue(sheet.Id.ToString());
                        bool rosDBUpdated = SheetDataWriter.DeleteRevisionOnSheet("RevisionsOnSheet_Sheet_Id", sheet.Id.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        #region filter function
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
                        case "Discipline":
                            cv.Filter = o => { RevitSheet sheet = o as RevitSheet; return (sheet.DisciplineObj.Name.ToUpper().Contains(searchText.ToUpper())); };
                            break;
                        default:
                            var paramFound = from param in rvtSheetData.SheetParameters where param.ParameterName == fieldName select param;
                            if (paramFound.Count() > 0)
                            {
                                SheetParameter sheetParam = paramFound.First();
                                cv.Filter = o => { RevitSheet sheet = o as RevitSheet; return (sheet.SheetParameters[sheetParam.ParameterId].ParameterValue.ToString().ToUpper().Contains(searchText.ToUpper())); };
                            }
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
        #endregion

        #region linked projects
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
        #endregion

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
                        dataGridLinks.ItemsSource = selectedSheet.LinkedSheets;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        #region drag and drop copy
        private void dataGridSheet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //collect drag source
                copyInfo = new CellCopyInfo();
                DataGridCell cell = DataGridUtils.FindVisualParent<DataGridCell>(e.OriginalSource as UIElement);
                if (e.RightButton == MouseButtonState.Pressed && null != cell)
                {
                    DataGridUtils.FindDragSource(dataGridSheet, dataGridSheet.SelectedCells, ref copyInfo);
                    DataObject dataObject = new DataObject(copyInfo);
                    tempDragRows = new List<int>();
                    DragDrop.DoDragDrop(cell, dataObject, DragDropEffects.Copy);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridSheet_PreviewDragOver(object sender, DragEventArgs e)
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
                                    this.RvtSheetData.Sheets.RemoveAt(index);
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
                            this.RvtSheetData.Sheets.Add(new RevitSheet());
                            tempDragRows.Add(rvtSheetData.Sheets.Count - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridSheet_PreviewDrop(object sender, DragEventArgs e)
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

        #endregion

        private void dataGridSheet_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                DataGridRow row = e.Row;
                if (null != row )
                {
                    RevitSheet oldSheet = row.Item as RevitSheet;
                    string propertyName = e.Column.Header.ToString();

                    switch (propertyName)
                    {
                        case "Sheet Number":
                            TextBox textBoxNumber = e.EditingElement as TextBox;
                            if (null != textBoxNumber)
                            {
                                string updatedNumber = textBoxNumber.Text;
                                var numbers = from sheet in rvtSheetData.Sheets select sheet.Number;
                                if (numbers.Contains(updatedNumber))
                                {
                                    e.Cancel = true;
                                    MessageBox.Show(updatedNumber + " already exists in the list of sheets.\nPlease enter a different sheet name","Existing Sheet Number", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }
                                else
                                {
                                    bool databaseUpdated = SheetDataWriter.ChangeSheetItem(oldSheet.Id.ToString(), "Sheet_Number", textBoxNumber.Text);
                                }
                            }
                            break;
                        case "Sheet Name":
                            TextBox textBoxName = e.EditingElement as TextBox;
                            if (null != textBoxName)
                            {
                                bool databaseUpdated = SheetDataWriter.ChangeSheetItem(oldSheet.Id.ToString(), "Sheet_Name", textBoxName.Text);
                            }
                            break;
                        case "Discipline":
                            ComboBox comboBoxDiscipline = e.EditingElement as ComboBox;
                            if (null != comboBoxDiscipline)
                            {
                                Discipline selectedDiscipline = comboBoxDiscipline.SelectedItem as Discipline;
                                if (null != selectedDiscipline)
                                {
                                    bool databaseUpdated = SheetDataWriter.ChangeSheetItem(oldSheet.Id.ToString(), "Sheet_Discipline_Id", selectedDiscipline.Id.ToString());
                                }
                            }
                            break;
                        default:
                            //sheetParameter changed
                            TextBox textBoxParam = e.EditingElement as TextBox;
                            var paramValueFound = from paramValue in oldSheet.SheetParameters.Values where paramValue.Parameter.ParameterName == propertyName select paramValue;
                            if (paramValueFound.Count() > 0 && null != textBoxParam)
                            {
                                SheetParameterValue paramValue = paramValueFound.First();
                                paramValue.ParameterValue = textBoxParam.Text;
                                bool databaseUpdated = SheetDataWriter.ChangeSheetParameterValue(paramValue, CommandType.UPDATE);   
                            }
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            rvtSheetData.Sheets.CollectionChanged -= OnCollectionChanged;
        }

    }
}
