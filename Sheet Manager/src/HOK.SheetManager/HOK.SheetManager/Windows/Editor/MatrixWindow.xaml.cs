using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using HOK.SheetManager.Utils;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.SheetManager.Windows.Editor
{
    /// <summary>
    /// Interaction logic for MatrixWindow.xaml
    /// </summary>
    public partial class MatrixWindow : Window
    {
        private RevitSheetData rvtSheetData = null;
        private bool userMode = false;

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }

        public MatrixWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtSheetData = this.DataContext as RevitSheetData;
                foreach (RevitRevision revision in rvtSheetData.Revisions)
                {
                    DataGridTemplateColumn column = new DataGridTemplateColumn();
                    column.Header = revision.Description;

                    DataTemplate dataTemplate = new DataTemplate();
                    FrameworkElementFactory elementFactory = new FrameworkElementFactory(typeof(CheckBox));
                    elementFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    elementFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                    elementFactory.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(OnChecked));
                    elementFactory.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(OnUnchecked));
                    elementFactory.SetBinding(ToggleButton.IsCheckedProperty, new Binding("SheetRevisions[" + revision.Id + "].Include") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    dataTemplate.VisualTree = elementFactory;
                    column.CellTemplate = dataTemplate;
                    
                    dataGridMatrix.Columns.Add(column);
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (userMode && null != sender)
                {
                   
                    CheckBox checkBox = sender as CheckBox;
                    RevisionOnSheet ros = GetRevisionOnSheet(checkBox);
                    if (null != ros)
                    {
                        bool dbUpdated = SheetDataWriter.ChangeRevisionOnSheet(ros, CommandType.UPDATE);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to fire chekcbox event.\n" + ex.Message, "Checkbox Checked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (userMode && null != sender)
                {
                    CheckBox checkBox = sender as CheckBox;
                    RevisionOnSheet ros = GetRevisionOnSheet(checkBox);
                    if (null != ros)
                    {
                        bool dbUpdated = SheetDataWriter.ChangeRevisionOnSheet(ros, CommandType.UPDATE);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to fire checkbox event.\n" + ex.Message, "Checkbox Unchecked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private RevisionOnSheet GetRevisionOnSheet(CheckBox checkBox)
        {
            RevisionOnSheet ros = null;
            Guid revisionId = Guid.Empty;
            try
            {
                Binding binding = BindingOperations.GetBinding(checkBox, ToggleButton.IsCheckedProperty);
                string sortMemberPath = binding.Path.Path;
                var mg = Regex.Match(sortMemberPath, @"\[(.*?)\]");
                if (mg.Success)
                {
                    string value = mg.Groups[1].Value;
                    revisionId = new Guid(value);
                }

                var parent = VisualTreeHelper.GetParent(checkBox);
                while (parent != null && parent.GetType() != typeof(DataGridRow))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                DataGridRow datarow = parent as DataGridRow;
                if (null != datarow)
                {
                    RevitSheet sheet = datarow.Item as RevitSheet;
                    if (null != sheet)
                    {
                        if (sheet.SheetRevisions.ContainsKey(revisionId))
                        {
                            ros = sheet.SheetRevisions[revisionId];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to checked Ids.\n" + ex.Message, "Get Checked Ids", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return ros;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = true;
        }

        private void dataGridMatrix_MouseEnter(object sender, MouseEventArgs e)
        {
            userMode = true;
            dataGridMatrix.MouseEnter -= dataGridMatrix_MouseEnter;
        }

        private void menuItemCheckSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IList<DataGridCellInfo> selectedCells = dataGridMatrix.SelectedCells;
                foreach (DataGridCellInfo cell in selectedCells)
                {
                    FrameworkElement cellContent= cell.Column.GetCellContent(cell.Item);
                    DataGridCell gridCell = (DataGridCell)cellContent.Parent;

                    CheckBox checkBox = DataGridUtils.FindVisualChild<CheckBox>(gridCell);
                    if (null != checkBox)
                    {
                        checkBox.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void menuItemUncheckSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IList<DataGridCellInfo> selectedCells = dataGridMatrix.SelectedCells;
                foreach (DataGridCellInfo cell in selectedCells)
                {
                    FrameworkElement cellContent = cell.Column.GetCellContent(cell.Item);
                    DataGridCell gridCell = (DataGridCell)cellContent.Parent;

                    CheckBox checkBox = DataGridUtils.FindVisualChild<CheckBox>(gridCell);
                    if (null != checkBox)
                    {
                        checkBox.IsChecked = false;
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
