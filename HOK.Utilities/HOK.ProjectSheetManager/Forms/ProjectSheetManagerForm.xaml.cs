using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.ProjectSheetManager.Classes;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        // Public Counter Properties
        public int NewSheetCount;
        public int CreateFailCount;
        public int UpdatedSheetCount;
        public int UpdatedParameterCount;
        public int UpdatedFailCount;

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

            if (File.Exists(addinSettings.ExcelPath()))
            {
                try
                {
                    txtBxIOFilePath.Text = addinSettings.ExcelPath();
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.WARNING, "Failed to set Excel file path. Message: " + ex.Message);
                }
            }

            // Get all titleblock families
            var titleBlocks = new FilteredElementCollector(addinSettings.Document)
                                  .WhereElementIsElementType()
                                  .OfCategory(BuiltInCategory.OST_TitleBlocks).ToElements();

            // Constructing the tree view, top node is family name, child is type name
            foreach (FamilySymbol x in titleBlocks)
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
            foreach (DataRow row in dataTable.Rows)
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

                    switch (cmbBxSelectionFilter.SelectedIndex)
                    {
                        case 0:
                            treeItem.IsSelected = true;
                            break;
                        case 1:
                            if (!addinSettings.Sheets.ContainsKey(sheetNumber))
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

                        if (sheetItem.IsPlaceholder)
                        {
                            isPlaceholderSheet = true;
                            treeItem.Header = sheetNumber + ": " + sheetName + " (Placeholder)";
                        }

                        // The Titleblock Instance
                        Element tblkItem = null;
                        if (addinSettings.clsTblksList.ContainsKey(sheetNumber))
                        {
                            tblkItem = addinSettings.clsTblksList[sheetNumber].Element;
                        }

                        // Iterate the columns in the datarow
                        foreach (DataColumn dc in dataTable.Columns)
                        {
                            Autodesk.Revit.DB.Parameter revitParam = sheetItem.LookupParameter(dc.ColumnName);
                            if (revitParam != null)
                            {
                                Classes.Parameter param = new Classes.Parameter(revitParam);

                                try
                                {
                                    if (param.DisplayUnitType.ToUpper() == "YESNO")
                                    {
                                        bool isDiscrepancy = false;
                                        switch (row[param.Name].ToString().ToUpper())
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
            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                if (dataColumn.ColumnName == columnName)
                {
                    return true;
                }
            }
            Autodesk.Revit.UI.TaskDialog.Show("Processing Stopped", "No column named \"" + columnName + "\" found.");
            return false;

        }

        private void trViewTitleblocks_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var treeViewItem = ((System.Windows.Controls.TreeView)sender).SelectedItem as TreeViewItem;
            // If a parent node is selected, select it's first child node instead
            try
            {
                if (treeViewItem != null && (treeViewItem.Parent as System.Windows.Controls.TreeView).Name == "trViewTitleblocks")
                {
                    foreach (TreeViewItem parentNode in trViewTitleblocks.Items)
                    {
                        if (parentNode.IsSelected == true && parentNode.Items.Count > 0)
                        {
                            parentNode.IsExpanded = true;
                            (parentNode.Items[0] as TreeViewItem).IsSelected = true;
                        }
                    }
                }
            }
            catch { }
        }

        private void lstBxSheetSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScanSheets();
        }

        private void btnAddViewsSheets_Click(object sender, RoutedEventArgs e)
        {
            Transaction trAddViewsToSheets = new Transaction(this.addinSettings.Document, "HOK Add Views to Sheets");
            trAddViewsToSheets.Start();

            System.Data.DataTable dataTableLocal;
            XYZ pointInsert = null;
            Autodesk.Revit.DB.Parameter parameterViewSheetInfo;
            double offsetU = 0.0;
            double offsetV = 0.0;
            string viewsNotPlaced = "";
            bool placed = false;
            string unplacedViews = "";
            string placedViews = "";

            // Read the data from excel to local table
            excelUtility.FillDataTableFromExcelWorksheet(lstBxSheetSource.SelectedItem.ToString());
            dataTableLocal = excelUtility.DataTable;

            // Ensure correct columns are present
            if (!CheckColumnPresent(dataTableLocal, "Sheet Number"))
                return;
            if (!CheckColumnPresent(dataTableLocal, "View Name"))
                return;
            if (!CheckColumnPresent(dataTableLocal, "U"))
                return;
            if (!CheckColumnPresent(dataTableLocal, "V"))
                return;

            // Process each row in rename table
            foreach (DataRow row in dataTableLocal.Rows)
            {
                string sheetNumber = row["Sheet Number"].ToString();

                // Check for missing values and ignore them
                if (String.IsNullOrEmpty(sheetNumber))
                    continue;
                if (row["View Name"].ToString() == "")
                    continue;
                if (row["U"].ToString() == "")
                    continue;
                if (row["V"].ToString() == "")
                    continue;

                // See if sheet and view exist; if not ignore step
                if (!addinSettings.Sheets.ContainsKey(sheetNumber))
                    continue;
                if (!addinSettings.Views.ContainsKey(row["View Name"].ToString()))
                    continue;

                if (trViewSheetElements.Items.Contains(sheetNumber))
                {
                    TreeViewItem trSheetItem = (trViewSheetElements.Items[trViewSheetElements.Items.IndexOf(sheetNumber)] as TreeViewItem);
                    if (trSheetItem != null && trSheetItem.IsSelected)
                    {
                        // Get the sheet and view and place the view on the sheet
                        ViewSheet selectedViewSheet = addinSettings.Sheets[sheetNumber];
                        Autodesk.Revit.DB.View selectedView = addinSettings.Views[row["View Name"].ToString()];

                        // Confirm the UV values are valid
                        try
                        {
                            offsetU = Convert.ToDouble(row["U"].ToString());
                            offsetV = Convert.ToDouble(row["V"].ToString());
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Sheet Manager Add-in", "Error while trying to convert U or V value to a number.\n\nError message: " + ex.Message);
                            return;
                        }

                        // Try to place the view
                        try
                        {
                            parameterViewSheetInfo = selectedView.get_Parameter(BuiltInParameter.VIEW_SHEET_VIEWPORT_INFO);
                            if (parameterViewSheetInfo.AsString().ToUpper() == "NOT IN A SHEET")
                            {
                                BoundingBoxXYZ boundingBox = selectedView.get_BoundingBox(selectedView);

                                pointInsert = XYZ.Zero;
                                if (Viewport.CanAddViewToSheet(addinSettings.Document, selectedViewSheet.Id, selectedView.Id))
                                {
                                    Viewport createdViewport = Viewport.Create(addinSettings.Document, selectedViewSheet.Id, selectedView.Id, pointInsert);
                                    XYZ centerPoint = createdViewport.GetBoxCenter();
                                    Outline outline = createdViewport.GetBoxOutline();
                                    XYZ diffToMove = new XYZ(offsetU + outline.MaximumPoint.X, offsetV + outline.MaximumPoint.Y, 0);
                                    ElementTransformUtils.MoveElement(addinSettings.Document, createdViewport.Id, diffToMove);

                                    placed = true;
                                    placedViews += selectedViewSheet.SheetNumber + " : " + selectedView.Name + "\n";
                                }
                                
                                // Case for if the selected view is a schedule
                                else if ((selectedView as ViewSchedule) != null)
                                {
                                    ScheduleSheetInstance createdScheduleInstance = ScheduleSheetInstance.Create(addinSettings.Document, selectedViewSheet.Id, selectedView.Id, pointInsert);
                                    XYZ centerPoint = createdScheduleInstance.Point;
                                    BoundingBoxXYZ bBox = createdScheduleInstance.get_BoundingBox(selectedViewSheet);
                                    XYZ diffToMove = new XYZ(offsetU - bBox.Min.X, offsetV - bBox.Min.Y, 0);
                                    ElementTransformUtils.MoveElement(addinSettings.Document, createdScheduleInstance.Id, diffToMove);

                                    placed = true;
                                    placedViews += selectedViewSheet.SheetNumber + " : " + selectedView.Name + "\n";
                                }
                            }
                            else
                            {
                                unplacedViews += selectedViewSheet.SheetNumber + " : " + selectedView.Name + "\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Sheet Manager", "Failed to place views. Error message: " + ex.Message);
                        }
                    }
                }
            }
            // Commit and clear the datatable
            trAddViewsToSheets.Commit();
            dataTableLocal.Clear();

            // Completed message
            if(placed == false)
            {
                if(unplacedViews.Length > 0)
                {
                    viewsNotPlaced = "Unplaced Views: The following views already exist on sheets\n\n";
                    viewsNotPlaced += unplacedViews;
                    TaskDialog td = new TaskDialog("Sheet Manager");
                    td.MainInstruction = "Processing stopped...";
                    td.MainContent = viewsNotPlaced;
                    td.Show();
                }
                else
                {
                    TaskDialog td = new TaskDialog("Sheet Manager");
                    td.MainInstruction = "Processing stopped...";
                    td.MainContent = "No Views Placed";
                    td.Show();
                }
            }
            else
            {
                viewsNotPlaced = "Placed Views\n\n";
                viewsNotPlaced += placedViews.ToString() + "\n\n";
                viewsNotPlaced += "Unplaced Views: The following views already exist on sheets:\n\n";
                viewsNotPlaced += unplacedViews.ToString();
                TaskDialog td = new TaskDialog("Sheet Manager");
                td.MainInstruction = "Processing Completed!";
                td.MainContent = viewsNotPlaced;
                td.Show();
            }
        }

        private void btnRenumSheets_Click(object sender, RoutedEventArgs e)
        {
            List<string> listTestRenumber = new List<string>();

            // Get sheet values
            excelUtility.FillDataTableFromExcelWorksheet("Renumber Sheets");
            DataTable dataTable = excelUtility.DataTable;

            // Make sure the correct columns are present
            if (!CheckColumnPresent(dataTable, "OldNumber"))
                return;
            if (!CheckColumnPresent(dataTable, "NewNumber"))
                return;

            // Check for login errors in rename
            foreach(string sheetNumber in addinSettings.Sheets.Keys)
            {
                if (trViewSheetElements.Items.Contains(sheetNumber))
                {
                    if (((TreeViewItem)trViewSheetElements.Items[trViewSheetElements.Items.IndexOf(sheetNumber)]).IsSelected)
                        listTestRenumber.Add(sheetNumber);
                }
            }

            using(Transaction tr = new Transaction(addinSettings.Document, "HOK Sheet Renumber"))
            {
                tr.Start();
                try
                {
                    foreach(DataRow row in dataTable.Rows)
                    {
                        // Check for missing values and ignore them
                        if (row["OldNumber"].ToString() == "")
                            continue;
                        if (row["NewNumber"].ToString() == "")
                            continue;

                        // See if sheet exists, otherwise ignore it. If found, check that new number doesn't exist.
                        if (listTestRenumber.Contains(row["OldNumber"]))
                        {
                            if (listTestRenumber.Contains(row["NewNumber"]))
                            {
                                TaskDialog taskDialog = new TaskDialog("Sheet Manager");
                                taskDialog.MainInstruction = "Logical error at Old Number: ";
                                taskDialog.MainContent = row["OldNumber"].ToString() + " New Number: " + row["NewNumber"].ToString();
                                taskDialog.Show();

                                dataTable.Clear();
                                if (excelUtility.DataTable != null)
                                    excelUtility.DataTable.Clear();
                                return;
                            }
                            listTestRenumber.Remove(row["OldNumber"].ToString());
                            listTestRenumber.Add(row["NewNumber"].ToString());
                        }
                    }

                    // Process each row in rename table
                    foreach(DataRow row in dataTable.Rows)
                    {
                        string oldNumber = row["OldNumber"].ToString();
                        string newNumber = row["NewNumber"].ToString();

                        // Check for missing vlaues and ignore them
                        if (oldNumber == "")
                            continue;
                        if (newNumber == "")
                            continue;

                        // See if sheet exists, otherwise ignore it. If found renumber and update dictionary
                        if (addinSettings.Sheets.ContainsKey(oldNumber) && trViewSheetElements.Items.Contains(oldNumber) && !addinSettings.Sheets.ContainsKey(newNumber))
                        {
                            if (trViewSheetElements.Items.Contains(oldNumber))
                            {
                                if (((TreeViewItem)trViewSheetElements.Items[trViewSheetElements.Items.IndexOf(oldNumber)]).IsSelected)
                                {
                                    ViewSheet viewSheet = addinSettings.Sheets[oldNumber];
                                    viewSheet.SheetNumber = newNumber;
                                    addinSettings.Sheets.Add(newNumber, addinSettings.Sheets[oldNumber]);
                                    addinSettings.Sheets.Remove(oldNumber);
                                    viewSheet.SheetNumber = newNumber;
                                }
                            }
                        }
                    }
                    tr.Commit();
                }
                catch (Exception ex)
                {
                    tr.RollBack();
                    string message = ex.Message;
                }
            }

            dataTable.Clear();
            if (excelUtility.DataTable != null)
                excelUtility.DataTable.Clear();

            TaskDialog td = new TaskDialog("Sheet Manager - Renumber Sheets");
            td.MainInstruction = "Processing Completed";
            td.MainContent = "Old sheet numbers were replaced with new numbers";
        }

        private void btnRenameViews_Click(object sender, RoutedEventArgs e)
        {
            List<string> listRename = new List<string>();

            // Get sheet values
            excelUtility.FillDataTableFromExcelWorksheet("Rename Views");
            DataTable dataTable = excelUtility.DataTable;

            // Make sure correct columns are present
            if (!CheckColumnPresent(dataTable, "OldName"))
                return;
            if (!CheckColumnPresent(dataTable, "NewName"))
                return;

            int countView = 0;

            using(Transaction tr = new Transaction(addinSettings.Document, "HOK View Rename"))
            {
                tr.Start();
                try
                {
                    foreach(DataRow row in dataTable.Rows)
                    {
                        // Check for missing values and ignore them
                        if (row["OldName"].ToString() == "")
                            continue;
                        if (row["NewName"].ToString() == "")
                            continue;

                        string oldName = row["OldName"].ToString();
                        string newName = row["NewName"].ToString();

                        // See if sheet exists, otherwise ignore it. If found renumber and udpate dictionary
                        if (addinSettings.Views.ContainsKey(oldName) && addinSettings.Views.ContainsKey(newName))
                        {
                            View view = addinSettings.Views[oldName];
                            view.Name = newName;
                            addinSettings.Views.Add(newName, addinSettings.Views[oldName]);
                            addinSettings.Views.Remove(oldName);
                            view.Name = newName;
                            countView++;
                        }
                    }
                    tr.Commit();
                }
                catch (Exception ex)
                {
                    tr.RollBack();
                    string message = ex.Message;
                }
            }

            // Empty the datatable
            dataTable.Clear();
            if(excelUtility.DataTable != null)
            {
                excelUtility.DataTable.Clear();
            }

            TaskDialog td = new TaskDialog("Sheet Manager - Rename Views");
            td.MainInstruction = "Processing Completed";
            td.MainContent = countView.ToString() + " old view names were replaced with new names.";
            td.Show();
        }

        private void btnExportSheetData_Click(object sender, RoutedEventArgs e)
        {
            // First test if there are any sheets. If no sheets, then nothing to export
            if (addinSettings.Sheets.Count < 1)
                return;

            // Get the first sheet
            ViewSheet viewSheet = addinSettings.Sheets.First().Value;
            List<string> parameterList = new List<string>();

            // Data references
            DataRow dataRow = null;
            DataRow[] dataRowArray = Array.Empty<DataRow>();

            TaskDialog td = new TaskDialog("Sheet Manager - Export Sheet Data");
            td.MainInstruction = "Warning";
            td.MainContent = "This command will overwrite data.\n\nBe sure that the proper data table is selected before proceeding";
            td.AllowCancellation = true;
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Continue with Export");
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Cancel and do Nother");

            TaskDialogResult result = td.Show();
            switch(result)
            {
                case TaskDialogResult.CommandLink1:
                    break;
                case TaskDialogResult.CommandLink2:
                    TaskDialog.Show("Cancelling Export", "Cancelling", TaskDialogCommonButtons.Ok);
                    return;
            }

            // Get the sheet values
            excelUtility.FillDataTableFromExcelWorksheet(lstBxSheetSource.SelectedItem.ToString());
            DataTable dataTable = excelUtility.DataTable;

            // Is there a "Sheet Number" column?
            if (!CheckColumnPresent(dataTable, "Sheet Number"))
                return;
            if (addinSettings.Sheets.Count == 0)
            {
                TaskDialog.Show("Sheet Manager - Export Sheet Data", "Nothing to Process");
                return;
            }

            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                Autodesk.Revit.DB.Parameter paramTblk = viewSheet.LookupParameter(dataColumn.ColumnName);
                

                if (paramTblk != null)
                {
                    parameterList.Add(dataColumn.ColumnName);
                }
            }

            foreach (string sheetNumber in addinSettings.Sheets.Keys)
            {
                viewSheet = addinSettings.Sheets[sheetNumber];

                Autodesk.Revit.DB.Parameter paramTblk = viewSheet.LookupParameter(sheetNumber);

                dataRowArray = dataTable.Select("[Sheet Number] = '" + paramTblk.AsString() + "'");

                if(dataRowArray.Length > 1)
                {
                    // Duplicate record found
                    td = new TaskDialog("Sheet Manager - Export Sheet Data");
                    td.MainInstruction = "Processing Stopped";
                    td.MainContent = "Duplicate Sheet Number in Table: " + sheetNumber;
                    td.Show();
                    return;
                }
                if (dataRowArray.Length == 0)
                {
                    // No record, make a new one
                    dataRow = dataTable.NewRow();
                    foreach(string parameterName in parameterList)
                    {
                        paramTblk = viewSheet.LookupParameter(parameterName);

                        Classes.Parameter para = new Classes.Parameter(paramTblk);
                        if(paramTblk == null)
                        {
                            continue;
                        }
                        dataRow[parameterName] = para.Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
                else
                {
                    // Existing record
                    dataRow = dataRowArray[0];
                    foreach(string paramName in parameterList)
                    {
                        paramTblk = viewSheet.LookupParameter(paramName);

                        Classes.Parameter param = new Classes.Parameter(paramTblk);
                        if (paramTblk == null)
                            continue;
                        if(paramTblk.AsString() != dataRow[paramName].ToString())
                        {
                            dataRow[paramName] = param.Value;
                        }
                    }
                }
            }

            // Write the changes back to the excel file
            excelUtility.FillExcelWorksheetFromDataTable(lstBxSheetSource.SelectedItem.ToString());
            excelUtility.DataTable.Clear();

            // Completed Message
            td = new TaskDialog("Sheet Manager - Export Sheet Data");
            td.MainInstruction = "Processing Completed";
            td.MainContent = "All data in Revit is successfully exported to Excel";
            td.Show();
        }

        private void FillTemplateList()
        {
            DataTable dataTableLocal = new DataTable();

            lstBxSheetSource.Items.Clear();

            try
            {
                excelUtility.FillDataTableFromExcelSheetNames("TemplateId");
                dataTableLocal = excelUtility.DataTable;

                foreach(DataRow row in dataTableLocal.Rows)
                {
                    lstBxSheetSource.Items.Add(row["TemplateId"].ToString());
                }
                if (lstBxSheetSource.Items.Count == 0)
                {
                    TaskDialog.Show("Sheet Manager", "No valid sheets in Excel file worksheet \"TempalteId\".");
                    return;
                }
                lstBxSheetSource.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }

            // Enable command buttons if we have titleblock in the model
            IList<Element> titleBlocks = new FilteredElementCollector(addinSettings.Document)
                                        .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                        .WhereElementIsElementType()
                                        .ToElements();

            if(titleBlocks.Count == 0)
            {
                TaskDialog.Show("Sheet Manager", "Please add a titleblock to your model before using this application.", TaskDialogCommonButtons.Ok);
                return;
            }
            else
            {
                btnCreateUpdateSheets.IsEnabled = true;
                btnRenumSheets.IsEnabled = true;
                btnAddViewsSheets.IsEnabled = true;
                btnExportSheetData.IsEnabled = true;
            }
        }

        private bool DoSheet(DataRow infoRow, List<string> paramNames)
        {
            bool sheetExists = false;
            ViewSheet sheetView = null;
            Element sheetTitleBlock = null;

            string sheetNumber = infoRow["Sheet Number"].ToString();
            string sheetName = infoRow["Sheet Name"].ToString();

            // Does the sheet exist in the model?
            if (addinSettings.Sheets.ContainsKey(sheetNumber))
            {
                sheetExists = true;
                sheetView = addinSettings.Sheets[sheetNumber];

                // Do we want to update values?
                if(chkBxUpdateExistingValues.IsChecked == true)
                {
                    return true;
                }
                return UpdateExistingElement(infoRow, paramNames, sheetView, sheetTitleBlock, sheetExists);
            }
            else
            {
                using(Transaction tr = new Transaction(addinSettings.Document, "HOK Sheet Manager, Created: " + sheetNumber))
                {
                    tr.Start();
                    try
                    {

                    }
                    catch(Exception ex)
                    {
                        tr.RollBack();
                        CreateFailCount++;
                        TaskDialog.Show("Sheet Manager Error Encountered", "Sheet Number: " + sheetNumber + "\n Sheet Name: " + sheetName + "\n" + ex.Message);
                        return false;
                    }
                }
            }
            return false;
        }

        private bool UpdateExistingElement(DataRow infoRow, List<string> paramNames, ViewSheet sheetView, Element sheetTitleBlock, bool sheetExists)
        {
            using (Transaction tr = new Transaction(addinSettings.Document))
            {
                if(tr.Start("HOK Sheet Manager - Updated: " + infoRow["Sheet Number"].ToString()) == TransactionStatus.Started)
                {
                    try
                    {
                        // Update all discrepancies
                        bool isDiscrepancy = false;

                        // Iterate all parameter names
                        foreach(string x in paramNames)
                        {
                            // Skip sheet number and sheet name
                            if (x.ToUpper() == "SHEET NUMBER" || x.ToUpper() == "NAME")
                                continue;

                            // Avoid double searching in titleblock family later
                            bool paramFoundInSheet = false;

                            // If the param is in the sheet, update it and continue for
                            if(sheetView != null)
                            {
                                // Does the param exist?
                                Autodesk.Revit.DB.Parameter param = sheetView.LookupParameter(x);

                                if(param != null)
                                {
                                    // No need to search the TB element
                                    paramFoundInSheet = true;

                                    Classes.Parameter clsParam = new Classes.Parameter(param);
                                    if(clsParam != null)
                                    {
                                        // Special Handling for YesNo
                                        if(clsParam.DisplayUnitType.ToUpper() == "YESNO")
                                        {
                                            switch(infoRow[clsParam.Name].ToString().ToUpper())
                                            {
                                                case "1":
                                                    if (double.Parse(clsParam.Value) != 1)
                                                        isDiscrepancy = true;
                                                    break;
                                                case "Y":
                                                    if (double.Parse(clsParam.Value) != 1)
                                                        isDiscrepancy = true;
                                                    break;
                                                case "YES":
                                                    if (double.Parse(clsParam.Value) != 1)
                                                        isDiscrepancy = true;
                                                    break;
                                                case "X":
                                                    if (double.Parse(clsParam.Value) != 1)
                                                        isDiscrepancy = true;
                                                    break;
                                                case "0":
                                                    if (double.Parse(clsParam.Value) != 1)
                                                        isDiscrepancy = true;
                                                    break;
                                                case "N":
                                                    if (double.Parse(clsParam.Value) != 1)
                                                        isDiscrepancy = true;
                                                    break;
                                                case "NO":
                                                    if (double.Parse(clsParam.Value) != 1)
                                                        isDiscrepancy = true;
                                                    break;
                                                case "":
                                                    if (double.Parse(clsParam.Value) != 1)
                                                        isDiscrepancy = true;
                                                    break;
                                            }

                                            if(isDiscrepancy == true)
                                            {
                                                clsParam.Value = infoRow[clsParam.Name].ToString();

                                                UpdatedParameterCount++;
                                            }
                                        }
                                        else
                                        {
                                            if (clsParam.Value != infoRow[clsParam.Name].ToString())
                                            {
                                                isDiscrepancy = true;
                                                clsParam.Value = infoRow[clsParam.Name].ToString();

                                                UpdatedParameterCount++;
                                            }
                                        }
                                    }
                                }
                            }

                            // Not in sheet, check the titleblock
                            if (sheetView.IsPlaceholder == false && paramFoundInSheet == false)
                            {
                                // Do we have a valid titleblock element
                                if(sheetTitleBlock == null)
                                {
                                    // Is it already collected?
                                    if (addinSettings.clsTblksList.ContainsKey(infoRow["Sheet Number"].ToString()))
                                        sheetTitleBlock = addinSettings.clsTblksList[infoRow["Sheet Number"].ToString()].Element;
                                    else
                                    {
                                        // Find the titleblock family
                                        IList<Element> docTitleblocks = new FilteredElementCollector(addinSettings.Document)
                                                                            .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                                                            .ToElements();

                                        // Find the right one
                                        foreach(Element tb in docTitleblocks)
                                        {
                                            // Identify by sheet number
                                            Autodesk.Revit.DB.Parameter sheetNumberParam = tb.LookupParameter("Sheet Number");

                                            if(sheetNumberParam != null)
                                            {
                                                // Try and get the sheet number param
                                                Classes.Parameter snParam = new Classes.Parameter(sheetNumberParam);
                                                if(snParam != null)
                                                {
                                                    // Does the sheet number match what we're after
                                                    if(snParam.Value.ToUpper() == infoRow["Sheet Number"].ToString().ToUpper())
                                                    {
                                                        // This is the element we need
                                                        sheetTitleBlock = tb;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                // Do the updates to the titleblock family
                                else
                                {
                                    // Does the parameter exist here?
                                    Autodesk.Revit.DB.Parameter param = sheetTitleBlock.LookupParameter(x);

                                    if (param != null)
                                    {
                                        Classes.Parameter clsParam = new Classes.Parameter(param);
                                        if(clsParam != null)
                                        {
                                            // Special handling for YesNo
                                            // Special Handling for YesNo
                                            if (clsParam.DisplayUnitType.ToUpper() == "YESNO")
                                            {
                                                switch (infoRow[clsParam.Name].ToString().ToUpper())
                                                {
                                                    case "1":
                                                        if (double.Parse(clsParam.Value) != 1)
                                                            isDiscrepancy = true;
                                                        break;
                                                    case "Y":
                                                        if (double.Parse(clsParam.Value) != 1)
                                                            isDiscrepancy = true;
                                                        break;
                                                    case "YES":
                                                        if (double.Parse(clsParam.Value) != 1)
                                                            isDiscrepancy = true;
                                                        break;
                                                    case "X":
                                                        if (double.Parse(clsParam.Value) != 1)
                                                            isDiscrepancy = true;
                                                        break;
                                                    case "0":
                                                        if (double.Parse(clsParam.Value) != 1)
                                                            isDiscrepancy = true;
                                                        break;
                                                    case "N":
                                                        if (double.Parse(clsParam.Value) != 1)
                                                            isDiscrepancy = true;
                                                        break;
                                                    case "NO":
                                                        if (double.Parse(clsParam.Value) != 1)
                                                            isDiscrepancy = true;
                                                        break;
                                                    case "":
                                                        if (double.Parse(clsParam.Value) != 1)
                                                            isDiscrepancy = true;
                                                        break;
                                                }

                                                if (isDiscrepancy == true)
                                                {
                                                    clsParam.Value = infoRow[clsParam.Name].ToString();

                                                    UpdatedParameterCount++;
                                                }
                                            }
                                            else
                                            {
                                                if (clsParam.Value != infoRow[clsParam.Name].ToString())
                                                {
                                                    isDiscrepancy = true;
                                                    clsParam.Value = infoRow[clsParam.Name].ToString();

                                                    UpdatedParameterCount++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        tr.Commit();
                        if(sheetExists == true)
                        {
                            UpdatedSheetCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        UpdatedFailCount++;
                        tr.RollBack();
                    }
                }
            }
            return true;
        }

        private bool CheckCreationPrerequisites()
        {
            // If no titleblock in model, do not allow this command
            IList<Element> titleblocks = new FilteredElementCollector(addinSettings.Document)
                                            .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                            .WhereElementIsElementType()
                                            .ToElements();
            if (titleblocks.Count == 0)
            {
                TaskDialog.Show("Sheet Manager", "Add a titleblock prior to running this command.");
                return false;
            }

            // If no titleblock selection, do not allow this command
            if(trViewTitleblocks.SelectedItem == null)
            {
                TaskDialog.Show("Sheet Manager", "Select a titleblock prior to running this command.");
                return false;
            }
            return true;
        }

        private void btnCreateUpdateSheets_Click(object sender, RoutedEventArgs e)
        {
            // Requirements
            if (CheckCreationPrerequisites() == false)
                return;

            // Get the datatable
            excelUtility.FillDataTableFromExcelWorksheet(lstBxSheetSource.SelectedItem.ToString());

            DataTable dataTable = excelUtility.DataTable;

            // Sheet Number column is required
            if (!CheckColumnPresent(dataTable, "Sheet Number"))
                return;
            if (!CheckColumnPresent(dataTable, "Sheet Name"))
                return;

            NewSheetCount = 0;
            CreateFailCount = 0;
            UpdatedSheetCount = 0;
            UpdatedParameterCount = 0;

            // List of params in datatable
            List<string> paramNameList = new List<string>();
            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                paramNameList.Add(dataColumn.ColumnName);
            }

            // Process each row in the template table
            foreach(DataRow row in dataTable.Rows)
            {
                string sheetNumber = row["Sheet Number"].ToString();

                // Avoid Blank Sheet Numbers
                if (string.IsNullOrEmpty(sheetNumber))
                    continue;

                if (trViewSheetElements.Items.Contains(sheetNumber))
                {
                    if (((TreeViewItem)trViewSheetElements.Items[trViewSheetElements.Items.IndexOf(sheetNumber)]).IsSelected == true)
                    {
                        // Process the sheet
                        DoSheet(row, paramNameList);
                    }
                }
            }

            // Empty the data table
            dataTable.Clear();
            if (excelUtility.DataTable != null)
                excelUtility.DataTable.Clear();

            

            // Completed message
            if(CreateFailCount > 0)
            {
                TaskDialog td = new TaskDialog("Sheet Manager");
                td.MainInstruction = "Processing Completed";
                td.MainContent = NewSheetCount.ToString() + " New Sheets Created\n" +
                                 UpdatedSheetCount.ToString() + " Existing Sheets Updated\n" +
                                 UpdatedParameterCount.ToString() + " Parameters Updated\n" +
                                 UpdatedFailCount.ToString() + " Sheets Skipped due to Elements lock by users\n" +
                                 "------------------------------------------------------------\n" +
                                 CreateFailCount.ToString() + " sheets failed to create.";
                td.Show();
            }
            else
            {
                TaskDialog td = new TaskDialog("Sheet Manager");
                td.MainInstruction = "Processing Completed";
                td.MainContent = NewSheetCount.ToString() + " New Sheets Created\n" +
                                 UpdatedSheetCount.ToString() + " Existing Sheets Updated\n" +
                                 UpdatedParameterCount.ToString() + " Parameters Updated\n" +
                                 UpdatedFailCount.ToString() + " Sheets Skipped due to Elements lock by users\n";
                td.Show();
            }

            addinSettings.GetSheetsAndTitleblockInstances();
            ScanSheets();
        }
    }
}
