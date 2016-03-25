using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Microsoft.Win32;


namespace HOK.SheetDataEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string databaseFile = "";
        private RevitSheetData sheetData = null;
        private RevitSheet selectedSheet = null;

        public string DatabaseFile { get { return databaseFile; } set { databaseFile = value; } }
        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Sheet Data Editor - v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Open a Sheet Database File";
                openFileDialog.DefaultExt = ".sqlite";
                openFileDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)openFileDialog.ShowDialog())
                {
                    databaseFile = openFileDialog.FileName;
                    sheetData = DatabaseUtil.ReadDatabase(databaseFile);
                    DisplaySheetData();
                    statusLable.Text = databaseFile;
                    buttonProject.IsEnabled = true;
                    buttonDiscipline.IsEnabled = true;
                    buttonSheet.IsEnabled = true;
                    buttonView.IsEnabled = true;
                    buttonRevision.IsEnabled = true;
                    buttonMatrix.IsEnabled = true;
                    buttonReplace.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open the database file.\n"+ex.Message, "Open Database File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Create a New Database File";
                saveFileDialog.DefaultExt = ".sqlite";
                saveFileDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)saveFileDialog.ShowDialog())
                {
                    databaseFile = saveFileDialog.FileName;
                    //create database file
                    bool created =  DatabaseUtil.CreateDatabase(databaseFile);
                    if (created)
                    {
                        sheetData = DatabaseUtil.ReadDatabase(databaseFile);
                    }

                    DisplaySheetData();

                    buttonProject.IsEnabled = true;
                    buttonDiscipline.IsEnabled = true;
                    buttonSheet.IsEnabled = true;
                    buttonView.IsEnabled = true;
                    buttonRevision.IsEnabled = true;
                    buttonMatrix.IsEnabled = true;
                    buttonReplace.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a new database file.\n" + ex.Message, "New Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplaySheetData()
        {
            try
            {
                if (null != sheetData)
                {
                    List<Discipline> disciplines = sheetData.Disciplines.Values.OrderBy(o => o.Name).ToList();
                    comboBoxDiscipline.ItemsSource = null;
                    comboBoxDiscipline.ItemsSource = disciplines;
                    comboBoxDiscipline.SelectedIndex = 0;

                    List<RevitViewType> viewTypes = sheetData.ViewTypes.Values.OrderBy(o => o.Name).ToList();
                    dataGridComboBoxViewType.ItemsSource = null;
                    dataGridComboBoxViewType.ItemsSource = viewTypes;
                    dataGridComboBoxViewType.DisplayMemberPath = "Name";

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display sheet data.\n"+ex.Message, "Display Sheet Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxDiscipline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxDiscipline.SelectedItem && null != sheetData)
                {
                    Discipline selectedDiscipline = comboBoxDiscipline.SelectedItem as Discipline;
                    dataGridSheets.ItemsSource = null;
                    dataGridView.ItemsSource = null;
                    dataGridRevisions.ItemsSource = null;

                    var collectedSheets = from sheet in sheetData.Sheets.Values where sheet.DisciplineObj.Id == selectedDiscipline.Id select sheet;
                    if (collectedSheets.Count() > 0)
                    {
                        List<RevitSheet> sheetList = collectedSheets.OrderBy(o => o.Number).ToList();
                        dataGridSheets.ItemsSource = sheetList;
                        dataGridSheets.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a discipline item.\n"+ex.Message, "Select a Discipline", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridSheets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridSheets.SelectedItem)
                {
                    selectedSheet = dataGridSheets.SelectedItem as RevitSheet;
                    dataGridView.ItemsSource = null;
                    dataGridRevisions.ItemsSource = null;

                    if (null != selectedSheet)
                    {
                        DisplayViewItems();
                        DisplayRevisions();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a sheet item.\n"+ex.Message, "Select a Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayViewItems()
        {
            try
            {
                dataGridView.ItemsSource = null;
                var views = from view in sheetData.Views.Values where null != view.Sheet select view;
                var collectedViews = from view in views where view.Sheet.Id == selectedSheet.Id select view;
                if (collectedViews.Count() > 0)
                {
                    List<RevitView> viewList = collectedViews.OrderBy(o => o.Name).ToList();
                    dataGridView.ItemsSource = viewList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display view items.\n"+ex.Message, "Display View Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayRevisions()
        {
            try
            {
                dataGridRevisions.ItemsSource = null;
                var maps = from map in sheetData.RevisionMatrix.Values where map.SheetId == selectedSheet.Id select map;
                if (maps.Count() > 0)
                {
                    List<RevisionOnSheet> revisionOnSheets = maps.ToList();
                    List<RevitRevision> revisionList = new List<RevitRevision>();
                    foreach (RevisionOnSheet ros in revisionOnSheets)
                    {
                        Guid revisionId = ros.RevisionId;
                        if (sheetData.Revisions.ContainsKey(revisionId))
                        {
                            revisionList.Add(sheetData.Revisions[revisionId]);
                        }
                    }
                    dataGridRevisions.ItemsSource = revisionList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display revision items.\n"+ex.Message, "Display Revision Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonMatrix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MatrixWindow matrixWindow = new MatrixWindow(sheetData);
                matrixWindow.Owner = this;
                if ((bool)matrixWindow.ShowDialog() == true)
                {
                    sheetData = matrixWindow.SheetData;
                    DisplayRevisions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open the window for the revision schedule.\n"+ex.Message, "Revisions on Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region Manage Views
        private void buttonAddView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != sheetData && null != selectedSheet)
                {
                    ItemWindow itemWindow = new ItemWindow(AddItemType.Views, sheetData, selectedSheet.Id);
                    itemWindow.Owner = this;
                    if (itemWindow.ShowDialog() == true)
                    {
                        sheetData = itemWindow.SheetData;
                        DisplayViewItems();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add view items.\n"+ex.Message, "Add View Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRemoveView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridView.SelectedItem)
                {
                    RevitView selectedView = (RevitView)dataGridView.SelectedItem;
                    if (null != selectedView)
                    {
                        bool deleted = DatabaseUtil.UpdateViewOnSheet(selectedSheet.Id, selectedView.Id, false);
                        if (deleted && sheetData.Views.ContainsKey(selectedView.Id))
                        {
                            selectedView.Sheet = null;
                            selectedView.SheetNumber = "";
                            sheetData.Views.Remove(selectedView.Id);
                            sheetData.Views.Add(selectedView.Id, selectedView);
                            DisplayViewItems();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove view items.\n"+ex.Message, "Remove View Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region Manage Revisions
        private void buttonAddRevision_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != sheetData && null != selectedSheet)
                {
                    ItemWindow itemWindow = new ItemWindow(AddItemType.Revisions, sheetData, selectedSheet.Id);
                    itemWindow.Owner = this;
                    if (itemWindow.ShowDialog() == true)
                    {
                        sheetData = itemWindow.SheetData;
                        DisplayRevisions();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add revision items.\n"+ex.Message, "Add Revision Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRemoveRevision_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRevisions.SelectedItem)
                {
                    RevitRevision selectedRevision = (RevitRevision)dataGridRevisions.SelectedItem;
                    if(null != selectedRevision)
                    {
                        bool deleted = DatabaseUtil.DeleteRevisionOnSheet(selectedSheet.Id, selectedRevision.Id);
                        if (deleted)
                        {
                            var foundMatrix = from matrix in sheetData.RevisionMatrix.Values where matrix.RevisionId == selectedRevision.Id && matrix.SheetId == selectedSheet.Id select matrix.MapId;
                            if (foundMatrix.Count() > 0)
                            {
                                Guid mapId = foundMatrix.First();
                                if (sheetData.RevisionMatrix.ContainsKey(mapId))
                                {
                                    sheetData.RevisionMatrix.Remove(mapId);
                                    DisplayRevisions();
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove revision items.\n" + ex.Message, "Remove Revision Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DatabaseUtil.CloseDatabse();
        }

        private void buttonReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReplaceWindow replaceWindow = new ReplaceWindow(sheetData);
                replaceWindow.Owner = this;
                if (replaceWindow.ShowDialog() == true)
                {
                    sheetData = replaceWindow.SheetData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to edit items to be replaced.\n"+ex.Message, "Replace Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProjectWindow projectWindow = new ProjectWindow(sheetData);
                projectWindow.Owner = this;
                if (projectWindow.ShowDialog() == true)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display linked projects.\n"+ex.Message, "Linked Projects", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonSheet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SheetWindow sheetWindow = new SheetWindow(sheetData);
                sheetWindow.Owner = this;
                if (sheetWindow.ShowDialog() == true)
                {
                    sheetData = sheetWindow.SheetData;
                    DisplaySheetData();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonRevision_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RevisionWindow revisionWindow = new RevisionWindow(sheetData);
                revisionWindow.Owner = this;
                if (revisionWindow.ShowDialog() == true)
                {
                    sheetData = revisionWindow.SheetData;
                    DisplaySheetData();
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDiscipline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DisciplineWindow disciplineWindow = new DisciplineWindow(sheetData);
                disciplineWindow.Owner = this;
                if (disciplineWindow.ShowDialog() == true)
                {
                    sheetData = disciplineWindow.SheetData;
                    DisplaySheetData();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewWindow viewWindow = new ViewWindow(sheetData);
                viewWindow.Owner = this;
                if (viewWindow.ShowDialog() == true)
                {
                    sheetData = viewWindow.SheetData;
                    DisplaySheetData();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sheetData = DatabaseUtil.ReadDatabase(databaseFile);
                DisplaySheetData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to reload data.\n" + ex.Message, "Reload Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string helpDocument = @"V:\RVT-Data\HOK Program\Documentation\SheetManagerTools_Instruction.pdf";
            if (File.Exists(helpDocument))
            {
                Process.Start(helpDocument);
            }
        }
    }



}
