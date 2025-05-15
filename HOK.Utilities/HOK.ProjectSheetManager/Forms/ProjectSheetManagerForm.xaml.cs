using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.ProjectSheetManager.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HOK.ProjectSheetManager.Forms
{
    /// <summary>
    /// Interaction logic for SheetManager.xaml
    /// </summary>
    public partial class ProjectSheetManagerForm : Window
    {
        private Classes.Settings addinSettings;
        private UtilitySQL utilitySql;
        private FamilySymbol TitleBlock;
        private IList<FamilySymbol> TitleBlocks;
        private List<ViewSheet> Sheets;
        private string excelFilePath = "";
        private ExcelUtility excelUtility;

        public ProjectSheetManagerForm(Classes.Settings settings)
        {
            InitializeComponent();
            List<string> selectionModes = new List<string>() { "Select All Sheets", "New Sheets Only", "Existing Sheets Only" };
            this.cmbBxSelectionFilter.ItemsSource = selectionModes;

            // Button Visibility Setup
            btnCreateUpdateSheets.IsEnabled = false;
            btnExportSheetData.IsEnabled = false;
            btnRenumSheets.IsEnabled = false;
            btnRenameViews.IsEnabled = false;
            btnAddViewsSheets.IsEnabled = false;

            excelUtility = new ExcelUtility(settings);
            addinSettings = settings;

            this.Title = "Sheet Manager - " + addinSettings.ApplicationVersion();

            if(File.Exists(addinSettings.ExcelPath()))
            {
                try
                {
                    txtBxIOFilePath.Text = addinSettings.ExcelPath();
                }
                catch(Exception ex)
                {
                    Log.AppendLog(LogMessageType.WARNING, "Failed to set Excel file path. Message: " + ex.Message);
                }
            }

            // Get all titleblock families
            var titleBlocks = new FilteredElementCollector(addinSettings.Document)
                                  .WhereElementIsElementType()
                                  .OfCategory(BuiltInCategory.OST_TitleBlocks).ToElements();

            // Constructing the tree view, top node is family name, child is type name
            foreach(FamilySymbol x in titleBlocks)
            {
                // Parent Node
                TreeViewItem nodeParent = new TreeViewItem();
                nodeParent.Tag = x.Family.Name;
                nodeParent.Header = x.Family.Name;

                try
                {
                    if (this.trViewTitleblocks.Items.Cast<TreeViewItem>().Any(item => item.Header.ToString() == x.Family.Name) == false)
                    {
                        this.trViewTitleblocks.Items.Add(nodeParent);
                    }
                }
                catch { }

                // Child Node
                TreeViewItem nodeChild = new TreeViewItem();
                nodeChild.Tag = x.Name;
                nodeChild.Header = x.Name;

                try
                {
                    var titleBlockNodeParent = trViewTitleblocks.Items.Cast<TreeViewItem>().First(item => item.Header.ToString() == x.Family.Name);
                    if (titleBlockNodeParent.Items.Cast<TreeViewItem>().Any(item => item.Header.ToString() == x.Name) == false)
                    {
                        this.trViewTitleblocks.Items.Cast<TreeViewItem>().First(item => item.Header.ToString() == x.Family.Name).Items.Add(nodeChild);
                    }
                }
                catch { }
            }

            try
            {
                ((TreeViewItem)((TreeViewItem)trViewTitleblocks.Items[0]).Items[0]).IsSelected = true;
            }
            catch
            {

            }
        }

        private void txtBxIOFilePath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var openFileDlg = new System.Windows.Forms.OpenFileDialog();
            openFileDlg.Filter = "Excel File|*.xlsx";
            openFileDlg.Multiselect = false;
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                txtBxIOFilePath.Text = openFileDlg.FileName;
            }
            excelFilePath = txtBxIOFilePath.Text;
        }

        private void txtBxIOFilePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            excelFilePath = txtBxIOFilePath.Text;
        }
        private void ScanSheets()
        {
            // Fill the datatable
            excelUtility.FillDataTableFromExcelWorksheet(lstBxSheetSource.SelectedItem.ToString());

            // Datatable object
            DataTable dataTable = excelUtility.DataTable;

            // Verify existence of the "Sheet Number" column.
            if (!CheckColumnPresent(dataTable, "Sheet Number"))
            {
                return;
            }

            // Clear view sheets tree nodes
            this.trViewSheetElements.Items.Clear();

            // Show sheets in the tree view, icons to show updated vs new sheets
            foreach(DataRow row in dataTable.Rows)
            {
                string sheetNumber = row["Sheet Number"].ToString();
                string sheetName = "";

                if (dataTable.Columns.Contains("Sheet Name"))
                {
                    sheetName = row["Sheet Name"].ToString();
                }

                if (sheetNumber == "")
                {
                    continue;
                }
                else
                {
                    // Create the tree parent node
                    TreeViewItem treeItem = new TreeViewItem();
                    treeItem.Header = sheetNumber + ": " + sheetName;
                    treeItem.Tag = sheetNumber;
                    treeItem.Foreground = System.Windows.Media.Brushes.Black;
                    treeItem.IsSelected = false;

                    switch(cmbBxSelectionFilter.SelectedIndex)
                    {
                        case 0:
                            treeItem.IsSelected = true;
                            break;
                        case 1:
                            if(!addinSettings.Sheets.ContainsKey(sheetNumber))
                            {
                                treeItem.IsSelected = true;
                            }
                            break;
                        case 2:
                            if (addinSettings.Sheets.ContainsKey(sheetNumber))
                            {
                                treeItem.IsSelected = true;
                            }
                            break;
                    }

                    trViewSheetElements.Items.Add(treeItem);

                    // Checking to see if it is an existing sheet
                    if (addinSettings.Sheets.ContainsKey(sheetNumber) && addinSettings.clsSheetsList.ContainsKey(sheetNumber))
                    {
                        // Find discrepancies
                        treeItem.Foreground = System.Windows.Media.Brushes.Gray;

                        // The sheet element
                        ViewSheet sheetItem = addinSettings.clsSheetsList[sheetNumber].RevitSheet;
                        bool isPlaceholderSheet = false;

                        if(sheetItem.IsPlaceholder)
                        {
                            isPlaceholderSheet = true;
                            treeItem.Header = sheetNumber + ": " + sheetName + " (Placeholder)";
                        }

                        // The Titleblock Instance
                        Element tblkItem = null;
                        if(addinSettings.clsTblksList.ContainsKey(sheetNumber))
                        {
                            tblkItem = addinSettings.clsTblksList[sheetNumber].Element;
                        }

                        // Iterate the columns in the datarow
                        foreach(DataColumn dc in dataTable.Columns)
                        {
                            Autodesk.Revit.DB.Parameter revitParam = sheetItem.LookupParameter(dc.ColumnName);
                            if(revitParam != null)
                            {
                                Classes.Parameter param = new Classes.Parameter(revitParam);

                                try
                                {
                                    if(param.DisplayUnitType.ToUpper() == "YESNO")
                                    {
                                        bool isDiscrepancy = false;
                                        switch(row[param.Name].ToString().ToUpper())
                                        {
                                            case "1":
                                                if (double.Parse(param.Value) != 1)
                                                    isDiscrepancy = true;
                                                break;
                                            case "Y":
                                                if (double.Parse(param.Value) != 1)
                                                    isDiscrepancy = true;
                                                break;
                                            case "YES":
                                                if (double.Parse(param.Value) != 1)
                                                    isDiscrepancy = true;
                                                break;
                                            case "X":
                                                if (double.Parse(param.Value) != 1)
                                                    isDiscrepancy = true;
                                                break;
                                            case "0":
                                                if (double.Parse(param.Value) != 0)
                                                    isDiscrepancy = true;
                                                break;
                                            case "N":
                                                if (double.Parse(param.Value) != 0)
                                                    isDiscrepancy = true;
                                                break;
                                            case "NO":
                                                if (double.Parse(param.Value) != 0)
                                                    isDiscrepancy = true;
                                                break;
                                            case "":
                                                if (double.Parse(param.Value) != 0)
                                                    isDiscrepancy = true;
                                                break;
                                        }
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CheckColumnPresent(DataTable dataTable, string columnName)
        {
            foreach(DataColumn dataColumn in dataTable.Columns)
            {
                if(dataColumn.ColumnName == columnName)
                {
                    return true;
                }
            }
            Autodesk.Revit.UI.TaskDialog.Show("Processing Stopped", "No column named \"" + columnName + "\" found.");
            return false;

        }
    }
}
