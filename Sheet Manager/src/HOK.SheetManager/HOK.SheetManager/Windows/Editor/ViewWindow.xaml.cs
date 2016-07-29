using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        private RevitSheetData rvtSheetData = null;

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }

        public ViewWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtSheetData = this.DataContext as RevitSheetData;

                comboBoxField.ItemsSource = from column in dataGridView.Columns select column.Header.ToString();
                comboBoxField.SelectedIndex = 0;

                dataGridSheetComboBox.ItemsSource = rvtSheetData.Sheets;

                dataGridViewTypeComboBox.ItemsSource = rvtSheetData.ViewTypes;

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool added = false;
                var viewNames = from view in rvtSheetData.Views select view.Name;
                for (int i = 1; i < 100; i++)
                {
                    string viewName = "New View " + i;
                    if (!viewNames.Contains(viewName))
                    {
                        RevitView rvtView = new RevitView(Guid.NewGuid(), viewName);
                        this.RvtSheetData.Views.Add(rvtView);
                        added = SheetDataWriter.ChangeViewItem(rvtView, CommandType.INSERT);

                        break;
                    }
                }

                if (!added)
                {
                    MessageBox.Show("Please assign view names before you add more view items.", "View Name", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add view items.\n" + ex.Message, "Add Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridView.SelectedItems)
                {
                    List<RevitView> viewsToDelete = dataGridView.SelectedItems.Cast<RevitView>().ToList();

                    foreach (RevitView rvtView in viewsToDelete)
                    {
                        this.RvtSheetData.Views.Remove(rvtView);
                        bool dbUpdated = SheetDataWriter.ChangeViewItem(rvtView, CommandType.DELETE);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete view items.\n" + ex.Message, "Delete Views", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        case "X":
                            double uVal;
                            if (double.TryParse(searchText, out uVal))
                            {
                                cv.Filter = o => { RevitView view = o as RevitView; return (view.LocationU == uVal); };
                            }
                            break;
                        case "Y":
                            double vVal;
                            if (double.TryParse(searchText, out vVal))
                            {
                                cv.Filter = o => { RevitView view = o as RevitView; return (view.LocationV == vVal); };
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

        private void dataGridView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                DataGridRow row = e.Row;
                if (null != row)
                {
                    RevitView oldView = row.Item as RevitView;
                    string propertyName = e.Column.Header.ToString();

                    switch (propertyName)
                    {
                        case "View Name":
                            TextBox textBoxName = e.EditingElement as TextBox;
                            if (null != textBoxName)
                            {
                                bool databaseUpdated = SheetDataWriter.ChangeViewItem(oldView.Id.ToString(), propertyName, textBoxName.Text);
                            }
                            break;
                        case "Sheet Number":
                            ComboBox comboBoxSheet = e.EditingElement as ComboBox;
                            if (null != comboBoxSheet)
                            {
                                RevitSheet selectedSheet = comboBoxSheet.SelectedItem as RevitSheet;
                                if (null != selectedSheet)
                                {
                                    bool databaseUpdated = SheetDataWriter.ChangeViewItem(oldView.Id.ToString(), propertyName, selectedSheet.Id.ToString());
                                }
                            }
                            break;
                        case "View Type":
                            ComboBox comboBoxViewType= e.EditingElement as ComboBox;
                            if (null != comboBoxViewType)
                            {

                                RevitViewType selectedViewType = comboBoxViewType.SelectedItem as RevitViewType;
                                if (null != selectedViewType)
                                {
                                    bool databaseUpdated = SheetDataWriter.ChangeViewItem(oldView.Id.ToString(), propertyName, selectedViewType.Id.ToString());
                                }
                            }
                            break;
                        case "U":
                            TextBox textBoxU = e.EditingElement as TextBox;
                            if (null != textBoxU)
                            {
                                double uValue;
                                if(double.TryParse(textBoxU.Text, out uValue))
                                {
                                    bool databaseUpdated = SheetDataWriter.ChangeViewItem(oldView.Id.ToString(), propertyName, uValue);
                                }
                            }
                            break;
                        case "V":
                             TextBox textBoxV = e.EditingElement as TextBox;
                             if (null != textBoxV)
                            {
                                double vValue;
                                if (double.TryParse(textBoxV.Text, out vValue))
                                {
                                    bool databaseUpdated = SheetDataWriter.ChangeViewItem(oldView.Id.ToString(), propertyName, vValue);
                                }
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

       
    }
}
