using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ParameterWindow.xaml
    /// </summary>
    public partial class ParameterWindow : Window
    {
        private RevitSheetData rvtSheetData = null;

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }

        public ParameterWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rvtSheetData = this.DataContext as RevitSheetData;
        }

        private void dataGridParameters_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                List<string> paramNames = GetExistingParameters();

                DataGridRow row = e.Row;
                TextBox textBox = e.EditingElement as TextBox;
                if (null != row && null != textBox)
                {
                    SheetParameter oldParameter = row.Item as SheetParameter;
                    string updatedParameter = textBox.Text;

                    if (paramNames.Contains(updatedParameter))
                    {
                        e.Cancel = true;
                        MessageBox.Show(updatedParameter + " already exists.\nPlease enter a different name.", "Existing Sheet Parameter", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    int index = rvtSheetData.SheetParameters.IndexOf(oldParameter);
                    this.RvtSheetData.SheetParameters.RemoveAt(index);
                    
                    oldParameter.ParameterName = updatedParameter;
                    this.RvtSheetData.SheetParameters.Insert(index, oldParameter);

                    bool databaseUpdated = SheetDataWriter.ChangeSheetParameter(oldParameter, CommandType.UPDATE);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private List<string> GetExistingParameters()
        {
            List<string> paramNames = new List<string>();
            try
            {
                var parameterNames = from param in rvtSheetData.SheetParameters select param.ParameterName;
                if (parameterNames.Count() > 0)
                {
                    paramNames = parameterNames.ToList();
                }
                paramNames.Add("Sheet Number");
                paramNames.Add("Sheet Name");
                paramNames.Add("Discipline");
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return paramNames;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool added = false;
                List<string> paramNames = GetExistingParameters();

                for (int i = 1; i < 21; i++)
                {
                    string paramName = "New Parameter " + i;
                    if (!paramNames.Contains(paramName))
                    {
                        SheetParameter sheetParam = new SheetParameter(Guid.NewGuid(), paramName, "TEXT");
                        this.RvtSheetData.SheetParameters.Add(sheetParam);
                        bool databaseUpdated = SheetDataWriter.ChangeSheetParameter(sheetParam, CommandType.INSERT);

                        //update RevitSheet

                        List<SheetParameterValue> paramValues = new List<SheetParameterValue>();
                        for (int sheetIndex = 0; sheetIndex < rvtSheetData.Sheets.Count; sheetIndex++)
                        {
                            SheetParameterValue sheetParamValue = new SheetParameterValue(Guid.NewGuid(), rvtSheetData.Sheets[sheetIndex].Id, sheetParam, "");
                            this.RvtSheetData.Sheets[sheetIndex].SheetParameters.Add(sheetParam.ParameterId, sheetParamValue);
                            paramValues.Add(sheetParamValue);
                        }

                        databaseUpdated = SheetDataWriter.InsertMultipleParameterValue(paramValues);

                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    MessageBox.Show("Please rename existing parameters before you add more parameters.", "Rename Parameter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridParameters.SelectedItem)
                {
                    SheetParameter selectedParam = dataGridParameters.SelectedItem as SheetParameter;

                    MessageBoxResult msgResult = MessageBox.Show("Are you sure you want to delete the parameter ["+selectedParam.ParameterName+"]?\nAll data currently stored under the parameter will be lost.", "Delete Parameter", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (msgResult == MessageBoxResult.Yes)
                    {
                        this.RvtSheetData.SheetParameters.Remove(selectedParam);
                        bool updatedTable = SheetDataWriter.ChangeSheetParameter(selectedParam, CommandType.DELETE);

                        //delete from RevitSheet
                        for (int i = 0; i < rvtSheetData.Sheets.Count; i++)
                        {
                            this.RvtSheetData.Sheets[i].SheetParameters.Remove(selectedParam.ParameterId);
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
