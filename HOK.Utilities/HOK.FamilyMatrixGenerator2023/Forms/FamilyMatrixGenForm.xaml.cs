using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HOK.FamilyMatrixGenerator2023.Forms
{
    /// <summary>
    /// Interaction logic for FamilyMatrixGenForm.xaml
    /// </summary>
    public partial class FamilyMatrixGenForm : Window
    {
        private Document RevitDoc;
        private List<FamilySymbol> DocFamilySymbols;
        private List<FamilySymbol> SelectedFamilySymbols;
        private View CurrentView;
        public FamilyMatrixGenForm(Document revitDoc)
        {
            InitializeComponent();
            this.RevitDoc = revitDoc;
            this.CurrentView = revitDoc.ActiveView;
            lblViewName.Content = CurrentView.Name;

            // Collect all unplaced families in the project
            var placedSymbolIds = new FilteredElementCollector(RevitDoc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Select(fi => fi.GetTypeId())
                .ToHashSet();
            var familyCollection = new FilteredElementCollector(RevitDoc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(fs => !placedSymbolIds.Contains(fs.Id))
                .ToList();

            foreach(var familyInstance in familyCollection)
            {
                this.lstViewRevitFamilies.Items.Add(familyInstance);
            }

            DocFamilySymbols = familyCollection;

            // Add the category filters
            var categories = familyCollection.Select(fi => fi.Category);
            foreach (var category in categories)
            {
                if(!cmbBxSelectionFilter.Items.Contains(category.Name))
                    cmbBxSelectionFilter.Items.Add(category.Name);
            }
        }

        private void btnPlaceFamilySymbols_Click(object sender, RoutedEventArgs e)
        {
            // Get the setting values from the UI
            double originX = Convert.ToDouble(txtBxOriginX.Text);
            double originY = Convert.ToDouble(txtBxOriginY.Text);
            double originZ = Convert.ToDouble(txtBxOriginZ.Text);

            double xSpacingVal = Convert.ToDouble(txtBxSpacingX.Text);
            double ySpacingVal = Convert.ToDouble(txtBxSpacingY.Text);

            int numColumns = Convert.ToInt32(txtBxNumColumns.Text);

            XYZ origin = new XYZ(originX, originY, originZ); // grid origin in the model
            double spacingX = xSpacingVal;             // horizontal spacing (feet)
            double spacingY = ySpacingVal;             // vertical spacing (feet)
            int columns = numColumns;               // how many per row

            // Place a new family instance on the currently open view in Revit in a grid format
            using (Transaction t = new Transaction(RevitDoc, "Place Unplace Families on Grid"))
            {
                t.Start();

                for (int i = 0; i < SelectedFamilySymbols.Count; i++)
                {
                    int row = i / columns;
                    int col = i % columns;

                    XYZ targetPos = new XYZ(
                        origin.X + col * spacingX,
                        origin.Y + row * spacingY,
                        origin.Z
                    );

                    FamilySymbol symbol = SelectedFamilySymbols[i];

                    // FamilySymbol must be activated before placement
                    if (!symbol.IsActive)
                        symbol.Activate();

                    switch (symbol.Family.FamilyPlacementType)
                    {
                        case FamilyPlacementType.ViewBased:
                            RevitDoc.Create.NewFamilyInstance(
                                targetPos,
                                symbol,
                                RevitDoc.ActiveView);
                            break;

                        case FamilyPlacementType.OneLevelBased:
                        case FamilyPlacementType.OneLevelBasedHosted: // e.g. doors/windows — will place without a wall host
                        case FamilyPlacementType.TwoLevelsBased:
                            RevitDoc.Create.NewFamilyInstance(
                                targetPos,
                                symbol,
                                RevitDoc.ActiveView.GenLevel,
                                StructuralType.NonStructural);
                            break;

                        case FamilyPlacementType.WorkPlaneBased:
                            // Needs an active workplane — use the level's plane
                            RevitDoc.Create.NewFamilyInstance(
                                targetPos,
                                symbol,
                                RevitDoc.ActiveView.GenLevel,
                                StructuralType.NonStructural);
                            break;

                        case FamilyPlacementType.CurveBased:
                        case FamilyPlacementType.CurveBasedDetail:
                            // Requires two points to define the curve — skip or log
                            TaskDialog.Show("Failed to Place Family on Current View",
                                $"Skipping curve-based family: {symbol.Family.Name}");
                            break;

                        case FamilyPlacementType.Invalid:
                        default:
                            TaskDialog.Show("Failed to Place Family on Current View",
                                $"Skipping unsupported placement type: {symbol.Family.Name} " +
                                $"({symbol.Family.FamilyPlacementType})");
                            break;
                    }
                }
                t.Commit();
            }
            TaskDialog.Show("Placed Families", "Placed " + SelectedFamilySymbols.Count + " families on view:\n" + RevitDoc.ActiveView.Name);
            this.Activate();
        }

        private void lstViewRevitFamilies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedFamilySymbols = new List<FamilySymbol>();
            foreach (FamilySymbol famSymbol in lstViewRevitFamilies.SelectedItems)
            {
                SelectedFamilySymbols.Add(famSymbol);
            }

            lblSelectedFamilies.Content = "Number of Selected Sheets: " + SelectedFamilySymbols.Count;
        }

        private void cmbBxSelectionFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Filtered family symbols
            var filteredFamilies = DocFamilySymbols.Where(f => f.Category.Name == cmbBxSelectionFilter.SelectedItem.ToString()).ToList();
            lstViewRevitFamilies.Items.Clear();
            foreach (var familyInstance in filteredFamilies)
            {
                this.lstViewRevitFamilies.Items.Add(familyInstance);
            }
        }

        private void btnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            lstViewRevitFamilies.SelectAll();
        }

        private void btnCheckNone_Click(object sender, RoutedEventArgs e)
        {
            lstViewRevitFamilies.SelectedItems.Clear();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Regex for only digits. Use "[^0-9.-]+" to allow decimals and negatives.
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
