using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using HOK.SheetManager.Utils;
using HOK.SheetManager.Windows.Editor;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HOK.SheetManager.UserControls
{
    public class EditorViewModel : INotifyPropertyChanged
    {
        private string dbFile = "";
        private RevitSheetData rvtSheetData = new RevitSheetData();
        private RevitSheet selectedSheet = null;

        private bool databaseOpened = false;
        private string statusText = "Ready";

        //commands
        private RelayCommand openDBCommand;
        private RelayCommand createDBCommand;
        private RelayCommand reloadDBCommand;
        private RelayCommand projectCommand;
        private RelayCommand disciplineCommand;
        private RelayCommand sheetCommand;
        private RelayCommand parameterCommand;
        private RelayCommand viewCommand;
        private RelayCommand revisionCommand;
        private RelayCommand revisionOnSheetCommand;
        private RelayCommand renameCommand;
        private RelayCommand helpCommand;
        private RelayCommand addViewCommand;
        private RelayCommand removeViewCommand;
        private RelayCommand addRevisionCommand;
        private RelayCommand removeRevisionCommand;

        public string DBFile { get { return dbFile; } set { dbFile = value; NotifyPropertyChanged("DatabaseOpened"); } }
        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; NotifyPropertyChanged("RvtSheetData"); } }
        public RevitSheet SelectedSheet { get { return selectedSheet; } set { selectedSheet = value; NotifyPropertyChanged("SelectedSheet"); } }
        public bool DatabaseOpened { get { return databaseOpened; } set { databaseOpened = value; NotifyPropertyChanged("DatabaseOpened"); } }
        public string StatusText { get { return statusText; } set { statusText = value; NotifyPropertyChanged("StatusText"); } }

        public ICommand OpenDBCommand { get { return openDBCommand; } }
        public ICommand CreateDBCommand { get { return createDBCommand; } }
        public ICommand ReloadDBCommand { get { return reloadDBCommand; } }
        public ICommand ProjectCommand { get { return projectCommand; } }
        public ICommand DisciplineCommand { get { return disciplineCommand; } }
        public ICommand SheetCommand { get { return sheetCommand; } }
        public ICommand ParameterCommand { get { return parameterCommand; } }
        public ICommand ViewCommand { get { return viewCommand; } }
        public ICommand RevisionCommand { get { return revisionCommand; } }
        public ICommand RevisionOnSheetCommand { get { return revisionOnSheetCommand; } }
        public ICommand RenameCommand { get { return renameCommand; } }
        public ICommand HelpCommand { get { return helpCommand; } }
        public ICommand AddViewCommand { get { return addViewCommand; } }
        public ICommand RemoveViewCommand { get { return removeViewCommand; } }
        public ICommand AddRevisionCommand { get { return addRevisionCommand; } }
        public ICommand RemoveRevisionCommand { get { return removeRevisionCommand; } }


        public EditorViewModel()
        {
            openDBCommand = new RelayCommand(param => this.OpenDBExecuted(param));
            createDBCommand = new RelayCommand(param => this.CreateDBExecuted(param));
            reloadDBCommand = new RelayCommand(param => this.ReloadDBExecuted(param));
            projectCommand = new RelayCommand(param => this.ProjectExecuted(param));
            disciplineCommand = new RelayCommand(param => this.DisciplineExecuted(param));
            sheetCommand = new RelayCommand(param => this.SheetExecuted(param));
            parameterCommand = new RelayCommand(param => this.ParameterExecuted(param));
            viewCommand = new RelayCommand(param => this.ViewExecuted(param));
            revisionCommand = new RelayCommand(param => this.RevisionExecuted(param));
            revisionOnSheetCommand = new RelayCommand(param => this.RevisionOnSheetExecuted(param));
            renameCommand = new RelayCommand(param => this.RenameExecuted(param));
            helpCommand = new RelayCommand(param => this.HelpExecuted(param));
            addViewCommand = new RelayCommand(param => this.AddViewExecuted(param));
            removeViewCommand = new RelayCommand(param => this.RemoveViewExecuted(param));
            addRevisionCommand = new RelayCommand(param => this.AddRevisionExecuted(param));
            removeRevisionCommand = new RelayCommand(param => this.RemoveRevisionExecuted(param));
        }

        public void OpenDBExecuted(object param)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Open a Sheet Database File";
                openDialog.DefaultExt = ".sqlite";
                openDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)openDialog.ShowDialog())
                {
                    this.DBFile= openDialog.FileName;
                    this.RvtSheetData = SheetDataReader.ReadSheetDatabase(dbFile, rvtSheetData);
                    this.RvtSheetData.SelectedDisciplineIndex = 0;
                    this.DatabaseOpened = true;
                    this.StatusText = dbFile;

                    bool opened = SheetDataWriter.OpenDatabase(dbFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open the database file.\n"+ex.Message, "Open Database File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CreateDBExecuted(object param)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "Create a New Sheet Data File";
                saveDialog.DefaultExt = ".sqlite";
                saveDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)saveDialog.ShowDialog())
                {
                    this.DBFile = saveDialog.FileName;
                    bool created = SheetDataWriter.CreateDatabase(dbFile);
                    if (created)
                    {
                        this.RvtSheetData = SheetDataReader.ReadSheetDatabase(dbFile, rvtSheetData);
                        this.RvtSheetData.SelectedDisciplineIndex = 0;
                        this.DatabaseOpened = true;
                        this.StatusText = dbFile;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create the database file.\n" + ex.Message, "Create Database File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void ReloadDBExecuted(object param)
        {
            try
            {
                this.RvtSheetData = SheetDataReader.ReadSheetDatabase(dbFile, rvtSheetData);
                this.RvtSheetData.SelectedDisciplineIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to reload the database file.\n" + ex.Message, "Reload Database File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void ProjectExecuted(object param)
        {
            try
            {
                ProjectWindow projectWindow = new ProjectWindow();
                projectWindow.DataContext = this.RvtSheetData;
                if ((bool)projectWindow.ShowDialog())
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open project items.\n"+ex.Message, "Open Project Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DisciplineExecuted(object param)
        {
            try
            {
                DisciplineWindow disciplineWindow = new DisciplineWindow();
                disciplineWindow.DataContext = this.RvtSheetData;
                if ((bool)disciplineWindow.ShowDialog())
                {
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open discipline items.\n" + ex.Message, "Open Discipline Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void SheetExecuted(object param)
        {
            try
            {
                SheetWindow sheetWindow = new SheetWindow();
                sheetWindow.DataContext = this.RvtSheetData;
                if ((bool)sheetWindow.ShowDialog())
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open sheet items.\n" + ex.Message, "Open Sheet Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void ParameterExecuted(object param)
        {
            try
            {
                ParameterWindow paramWindow = new ParameterWindow();
                paramWindow.DataContext = this.RvtSheetData;
                if ((bool)paramWindow.ShowDialog())
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open parameter info.\n" + ex.Message, "Open Parameter Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void ViewExecuted(object param)
        {
            try
            {
                ViewWindow viewWindow = new ViewWindow();
                viewWindow.DataContext = this.RvtSheetData;
                if ((bool)viewWindow.ShowDialog())
                {
                    RefreshSelectedSheet();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open view items.\n"+ex.Message, "Open View Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        public void RevisionExecuted(object param)
        {
            try
            {
                RevisionWindow revisionWindow = new RevisionWindow();
                revisionWindow.DataContext = this.RvtSheetData;
                if ((bool)revisionWindow.ShowDialog())
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open revision items.\n" + ex.Message, "Open Revision Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RevisionOnSheetExecuted(object param)
        {
            try
            {
                MatrixWindow matrixWindow = new MatrixWindow();
                matrixWindow.DataContext = this.RvtSheetData;
                if ((bool)matrixWindow.ShowDialog())
                {
                    RefreshSelectedSheet();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open Revision On Sheet items.\n" + ex.Message, "Open Revision On Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RenameExecuted(object param)
        {
            try
            {
                ItemMapperWindow mapperWindow = new ItemMapperWindow();
                mapperWindow.DataContext = this.RvtSheetData;
                if ((bool)mapperWindow.ShowDialog())
                {
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void HelpExecuted(object param)
        {
            try
            {
                string helpFile = @"V:\RVT-Data\HOK Program\Documentation\SheetManagerTools_Instruction.pdf";
                if (File.Exists(helpFile))
                {
                    Process.Start(helpFile);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void AddViewExecuted(object param)
        {
            try
            {
                if (null != selectedSheet)
                {
                    ItemWindow itemWindow = new ItemWindow(AddItemType.Views, selectedSheet);
                    itemWindow.DataContext = this.RvtSheetData;
                    if ((bool)itemWindow.ShowDialog())
                    {
                        RefreshSelectedSheet();
                    }
                }
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void RemoveViewExecuted(object param)
        {
            try
            {
                if (null != param)
                {
                    RevitView selectedView = param as RevitView;
                    int index = rvtSheetData.Views.IndexOf(selectedView);
                    this.RvtSheetData.Views[index].Sheet = new RevitSheet();
                    bool dbUpdated = SheetDataWriter.UpdateViewOnSheet(Guid.Empty, selectedView.Id);

                    RefreshSelectedSheet();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void AddRevisionExecuted(object param)
        {
            try
            {
                if (null != selectedSheet)
                {
                    ItemWindow itemWindow = new ItemWindow(AddItemType.Revisions, selectedSheet);
                    itemWindow.DataContext = this.RvtSheetData;
                    if ((bool)itemWindow.ShowDialog())
                    {
                        RefreshSelectedSheet();
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void RemoveRevisionExecuted(object param)
        {
            try
            {
                if (null != param && null!=selectedSheet)
                {
                    RevitRevision selectedRevision = param as RevitRevision;
                    int sheetIndex = rvtSheetData.Sheets.IndexOf(selectedSheet);
                    this.RvtSheetData.Sheets[sheetIndex].SheetRevisions[selectedRevision.Id].Include = false;
                    bool dbUpdated = SheetDataWriter.ChangeRevisionOnSheet(rvtSheetData.Sheets[sheetIndex].SheetRevisions[selectedRevision.Id], CommandType.UPDATE);

                    RefreshSelectedSheet();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void RefreshSelectedSheet()
        {
            RevitSheet tempSheet = selectedSheet;
            this.SelectedSheet = null;
            this.SelectedSheet = tempSheet;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

    }
}
