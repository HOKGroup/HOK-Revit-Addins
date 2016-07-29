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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.SheetManager.UserControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EditorCommandPanel : UserControl
    {
        private EditorViewModel viewModel = null;

        public EditorCommandPanel()
        {
            InitializeComponent();
            ProgressManager.progressBar = progressBar;
            ProgressManager.statusLabel = statusLable;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as EditorViewModel;
            viewModel.RvtSheetData.SheetParameters.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                var columnHeaders = from column in dataGridSheets.Columns where null!=column.Header select column.Header.ToString();
                List<string> columnNames = columnHeaders.ToList();

                if (e.NewItems != null && columnHeaders.Count() > 0)
                {
                    foreach (object item in e.NewItems)
                    {
                        SheetParameter parameter = item as SheetParameter;
                        int index = viewModel.RvtSheetData.SheetParameters.IndexOf(parameter);
                        if (!columnNames.Contains(parameter.ParameterName))
                        {
                            //add column
                            DataGridTextColumn txtColumn = new DataGridTextColumn();
                            txtColumn.Header = parameter.ParameterName;
                            txtColumn.Binding = new Binding("SheetParameters[" + parameter.ParameterId + "].ParameterValue");
                            dataGridSheets.Columns.Insert(index + 3, txtColumn);
                        }
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (object item in e.OldItems)
                    {
                        SheetParameter parameter = item as SheetParameter;
                        if (columnNames.Contains(parameter.ParameterName))
                        {
                            //remove column
                            var columnFound = from column in dataGridSheets.Columns where column.Header.ToString() == parameter.ParameterName select column;
                            if (columnFound.Count() > 0)
                            {
                                dataGridSheets.Columns.Remove(columnFound.First());
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

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (string file in files)
                    {
                        string fileExtension = System.IO.Path.GetExtension(file);
                        if (fileExtension.Contains("sqlite"))
                        {
                            viewModel.DBFile = file;
                            viewModel.RvtSheetData = SheetDataReader.ReadSheetDatabase(file, viewModel.RvtSheetData);
                            viewModel.RvtSheetData.SelectedDisciplineIndex = 0;
                            viewModel.DatabaseOpened = true;
                            viewModel.StatusText = file;

                            bool opened = SheetDataWriter.OpenDatabase(file);
                            break;
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to drop files.\n" + ex.Message, "File Drop", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

      
    }
}
