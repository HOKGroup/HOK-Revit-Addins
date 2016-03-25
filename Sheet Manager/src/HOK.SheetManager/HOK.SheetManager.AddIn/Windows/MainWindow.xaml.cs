using Autodesk.Revit.UI;
using HOK.SheetManager.AddIn.Updaters;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using HOK.SheetManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace HOK.SheetManager.AddIn.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AddInViewModel viewModel = null;

        public MainWindow()
        {

            InitializeComponent();

            this.Title = "Sheet Manager - AddIn v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

           
            ProgressManager.progressBar = progressBar;
            ProgressManager.statusLabel = statusLable;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as AddInViewModel;
            viewModel.RvtSheetData.SheetParameters.CollectionChanged += OnCollectionChanged;

            if (!string.IsNullOrEmpty(viewModel.Configuration.DatabaseFile))
            {
                bool opend = viewModel.OpenDatabase(viewModel.Configuration.DatabaseFile);
            }
        }

        private void AddParameterColumn(SheetParameter parameter)
        {
            var columnHeaders = from column in dataGridSheets.Columns where null != column.Header select column.Header.ToString();
            try
            {
                int index = viewModel.RvtSheetData.SheetParameters.IndexOf(parameter);
                if (!columnHeaders.Contains(parameter.ParameterName))
                {
                    //add column
                    DataGridTextColumn txtColumn = new DataGridTextColumn();
                    txtColumn.Header = parameter.ParameterName;
                    txtColumn.Binding = new Binding("SheetParameters[" + parameter.ParameterId + "].ParameterValue");
                    dataGridSheets.Columns.Add(txtColumn);
                }
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
                var columnHeaders = from column in dataGridSheets.Columns where null!= column.Header select column.Header.ToString();
                if (e.Action == NotifyCollectionChangedAction.Reset && dataGridSheets.Columns.Count > 4)
                {
                    for (int i = dataGridSheets.Columns.Count - 1; i > 3; i--)
                    {
                        dataGridSheets.Columns.RemoveAt(i);
                    }
                }
                if (e.NewItems != null && columnHeaders.Count() > 0)
                {
                    foreach (object item in e.NewItems)
                    {
                        SheetParameter parameter = item as SheetParameter;
                        AddParameterColumn(parameter);
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (object item in e.OldItems)
                    {
                        SheetParameter parameter = item as SheetParameter;
                        if (columnHeaders.Contains(parameter.ParameterName))
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

        private void Window_Drop(object sender, DragEventArgs e)
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
                            bool opened = viewModel.OpenDatabase(file);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to close Sheet Manager AddIn.\n" + ex.Message, "Window Closing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
