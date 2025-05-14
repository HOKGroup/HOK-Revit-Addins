using Autodesk.Revit.DB;
using HOK.ProjectSheetManager.Classes;
using System;
using System.Collections.Generic;
using System.Data;
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
        private bool excelActivated;
        private string excelFilePath = "";

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

        private void btnConnectExcel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
