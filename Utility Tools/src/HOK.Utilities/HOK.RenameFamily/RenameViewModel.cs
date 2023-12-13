using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.RenameFamily.Classes;
using HOK.RenameFamily.Util;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.RenameFamily
{
    public class RenameViewModel : INotifyPropertyChanged
    {
        private UIApplication m_app;
        private Document m_doc;
        private string fileName = "";
        private List<ElementType> elementTypes = new List<ElementType>();
        private ObservableCollection<string> modelNames = new ObservableCollection<string>();
        private ObservableCollection<string> categoryNames = new ObservableCollection<string>();
        private int selectedModelIndex = -1;
        private int selectedCategoryIndex = -1;
        private ObservableCollection<FamilyTypeProperties> typeProperties = new ObservableCollection<FamilyTypeProperties>();
        private string[] columnNames = new string[] { "Model", "Type ID", "Family Name", "Type Name" };
        private string statusText = "Ready.";

        private RelayCommand importFileCommand;
        private RelayCommand checkAllCommand;
        private RelayCommand uncheckAllCommand;
        private RelayCommand renameAllCommand;
        private RelayCommand renameSelectedCommand;
        private RelayCommand exportCommand;

        public string FileName { get { return fileName; } set { fileName = value; NotifyPropertyChanged("FileName"); } }
        public ObservableCollection<string> ModelNames { get { return modelNames; } set { modelNames = value; NotifyPropertyChanged("ModelNames"); } }
        public ObservableCollection<string> CategoryNames { get { return categoryNames; } set { categoryNames = value; NotifyPropertyChanged("CategoryNames"); } }
        public int SelectedModelIndex { get { return selectedModelIndex; } set { selectedModelIndex = value; NotifyPropertyChanged("SelectedModelIndex"); } }
        public int SelectedCategoryIndex { get { return selectedCategoryIndex; } set { selectedCategoryIndex = value; NotifyPropertyChanged("SelectedCategoryIndex"); } }
        public ObservableCollection<FamilyTypeProperties> TypeProperties { get { return typeProperties; } set { typeProperties = value; NotifyPropertyChanged("TypeProperties"); } }
        public string StatusText { get { return statusText; } set { statusText = value; NotifyPropertyChanged("StatusText"); } }

        public ICommand ImportFileCommand { get { return importFileCommand; } }
        public ICommand CheckAllCommand { get { return checkAllCommand; } }
        public ICommand UncheckAllCommand { get { return uncheckAllCommand; } }
        public ICommand RenameAllCommand { get { return renameAllCommand; } }
        public ICommand RenameSelectedCommand { get { return renameSelectedCommand; } }
        public ICommand ExportCommand { get { return exportCommand; } }

        public RenameViewModel(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            importFileCommand = new RelayCommand(param => this.ImportFileExecuted(param));
            checkAllCommand = new RelayCommand(param => this.CheckAllExecuted(param));
            uncheckAllCommand = new RelayCommand(param => this.UncheckAllExecuted(param));
            renameAllCommand = new RelayCommand(param => this.RenameAllExecuted(param));
            renameSelectedCommand = new RelayCommand(param => this.RenameSelectedExecuted(param));
            exportCommand = new RelayCommand(param => this.ExportExecuted(param));

            CollectRevitFamilies();
        }

        private void CollectRevitFamilies()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                elementTypes = collector.OfClass(typeof(ElementType)).ToElements().Cast<ElementType>().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect family symbols.\n" + ex.Message, "Collect Family Symbols", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void ImportFileExecuted(object param)
        {
            try
            {
               OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Open a CSV File";
                openFileDialog.DefaultExt = ".csv";
                openFileDialog.Filter = "Comma Separated Values (.csv)|*.csv";
                if ((bool)openFileDialog.ShowDialog())
                {
                    this.FileName = openFileDialog.FileName;

                    ReadCSV(fileName);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import csv file.\n" + ex.Message, "Import CSV File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ReadCSV(string filePath)
        {
            try
            {
                modelNames.Clear();
                categoryNames.Clear();
                typeProperties.Clear();

                if (File.Exists(filePath))
                {
                    using (TextFieldParser parser = new TextFieldParser(filePath))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");

                        bool firstRow = true;
                        bool formatMismatched = false;

                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();
                            if (firstRow)
                            {
                                for (int i = 0; i < columnNames.Length; i++)
                                {
                                    if (fields[i] != columnNames[i])
                                    {
                                        formatMismatched = true; break;
                                    }
                                }

                                if (formatMismatched)
                                {
                                    MessageBox.Show("Column names should be the following orders.\n[" + columnNames[0] + "], [" + columnNames[1] + "], [" + columnNames[2] + "], [" + columnNames[3] + "]", "Column Names", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                                firstRow = false;
                            }
                            else
                            {
                                string modelName = fields[0];
                                int typeId = -1;
                                int.TryParse(fields[1], out typeId);
                                string familyName = fields[2];
                                string typeName = fields[3];

                                FamilyTypeProperties ftp = new FamilyTypeProperties(modelName, typeId, familyName, typeName);
                                var symbolFound = from elementType in elementTypes where GetElementIdValue(elementType.Id) == typeId select elementType;
                                if (symbolFound.Count() > 0)
                                {
                                    ftp.SetCurrentFamily(symbolFound.First());
                                    this.TypeProperties.Add(ftp);
                                }
                            }
                        }
                    }

                    if (typeProperties.Count > 0)
                    {
                        var modelNameFound = from ftp in typeProperties select ftp.ModelName;
                        if (modelNameFound.Count() > 0)
                        {
                            this.ModelNames = new ObservableCollection<string>(modelNameFound.Distinct().OrderBy(o => o).ToList());
                        }

                        var categoryNameFound = from ftp in TypeProperties select ftp.CategoryName;
                        if (categoryNameFound.Count() > 0)
                        {
                            this.CategoryNames = new ObservableCollection<string>(categoryNameFound.Distinct().OrderBy(o => o).ToList());
                        }

                        this.SelectedModelIndex = 0;
                        this.SelectedCategoryIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read the csv file.\n" + ex.Message, "Read CSV File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CheckAllExecuted(object param)
        {
            try
            {
                if (selectedModelIndex > -1 && selectedCategoryIndex > -1)
                {
                    string modelName = modelNames[selectedModelIndex];
                    string categoryName = categoryNames[selectedCategoryIndex];

                    var itemFound = from item in typeProperties where item.ModelName == modelName && item.CategoryName == categoryName select item;
                    if (itemFound.Count() > 0)
                    {
                        foreach (FamilyTypeProperties ftp in itemFound)
                        {
                            int index = typeProperties.IndexOf(ftp);
                            this.TypeProperties[index].IsSelected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all items.\n" + ex.Message, "Check All Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void UncheckAllExecuted(object param)
        {
            try
            {
                if (selectedModelIndex > -1 && selectedCategoryIndex > -1)
                {
                    string modelName = modelNames[selectedModelIndex];
                    string categoryName = categoryNames[selectedCategoryIndex];

                    var itemFound = from item in typeProperties where item.ModelName == modelName && item.CategoryName == categoryName select item;
                    if (itemFound.Count() > 0)
                    {
                        foreach (FamilyTypeProperties ftp in itemFound)
                        {
                            int index = typeProperties.IndexOf(ftp);
                            this.TypeProperties[index].IsSelected = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck all items.\n" + ex.Message, "Uncheck All Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RenameAllExecuted(object param)
        {
            using (TransactionGroup tg = new TransactionGroup(m_doc))
            {
                tg.Start("Rename Families");
                try
                {
                    ProgressManager.InitializeProgress("Renaming.. ", typeProperties.Count);
                    for (int i = 0; i < typeProperties.Count; i++)
                    {
                        ProgressManager.StepForward();
                        FamilyTypeProperties ftp = typeProperties[i];
                        if (ftp.IsLinked) { continue; }

                        ElementType eType = m_doc.GetElement(ftp.FamilyTypeId) as ElementType;
                        if (null != eType)
                        {
                            using (Transaction trans = new Transaction(m_doc))
                            {
                                trans.Start("Rename");
                                try
                                {
                                    if (eType is FamilySymbol)
                                    {
                                        (eType as FamilySymbol).Family.Name = ftp.FamilyName;
                                    }
                                   
                                    eType.Name = ftp.TypeName;
                                    trans.Commit();

                                    typeProperties[i].CurrentFamilyName = ftp.FamilyName;
                                    typeProperties[i].CurrentTypeName = ftp.TypeName;
                                    typeProperties[i].IsLinked = true;
                                    typeProperties[i].ToolTip = "Current Family Name: " + ftp.FamilyName + ", Current Tyle Name: " + ftp.TypeName;
                                    typeProperties[i].IsSelected = false;
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    string message = ex.Message;
                                }
                            }
                        } 
                    }
                    ProgressManager.FinalizeProgress();
                    this.StatusText = fileName;
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Failed to rename families and types.\n" + ex.Message, "Rename Families and Types", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public void RenameSelectedExecuted(object param)
        {
            using (TransactionGroup tg = new TransactionGroup(m_doc))
            {
                tg.Start("Rename Families");
                try
                {
                    if (selectedModelIndex > -1 && selectedCategoryIndex > -1)
                    {
                        string modelName = modelNames[selectedModelIndex];
                        string categoryName = categoryNames[selectedCategoryIndex];

                        var itemFound = from item in typeProperties where item.ModelName == modelName && item.CategoryName == categoryName && item.IsSelected select item;
                        ProgressManager.InitializeProgress("Renaming.. ", itemFound.Count());
                        if (itemFound.Count() > 0)
                        {
                            foreach (FamilyTypeProperties ftp in itemFound)
                            {
                                ProgressManager.StepForward();
                                if (ftp.IsLinked) { continue; }
                                int index = typeProperties.IndexOf(ftp);

                                ElementType symbol = m_doc.GetElement(ftp.FamilyTypeId) as ElementType;
                                if (null != symbol)
                                {
                                    using (Transaction trans = new Transaction(m_doc))
                                    {
                                        trans.Start("Rename");
                                        try
                                        {
                                            if (symbol is FamilySymbol)
                                            {
                                                (symbol as FamilySymbol).Family.Name = ftp.FamilyName;
                                            }
                                            symbol.Name = ftp.TypeName;

                                            trans.Commit();

                                            typeProperties[index].CurrentFamilyName = ftp.FamilyName;
                                            typeProperties[index].CurrentTypeName = ftp.TypeName;
                                            typeProperties[index].IsLinked = true;
                                            typeProperties[index].ToolTip = "Current Family Name: " + ftp.FamilyName + ", Current Tyle Name: " + ftp.TypeName;
                                            typeProperties[index].IsSelected = false;
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.RollBack();
                                            string message = ex.Message;
                                        }
                                    }
                                }
                            }
                        }
                        ProgressManager.FinalizeProgress();
                        this.StatusText = fileName;
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Failed to rename families and types.\n" + ex.Message, "Rename Families and Types", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public void ExportExecuted(object param)
        {
            try
            {
                ExportWindow exportWindow = new ExportWindow(m_app);
                exportWindow.ElementTypes = elementTypes;
                if ((bool)exportWindow.ShowDialog())
                {
                    modelNames.Clear();
                    categoryNames.Clear();
                    typeProperties.Clear();

                    this.FileName = exportWindow.FileName;
                    this.TypeProperties = new ObservableCollection<FamilyTypeProperties>(exportWindow.TypeProperties);

                    if (typeProperties.Count > 0)
                    {
                        var modelNameFound = from ftp in typeProperties select ftp.ModelName;
                        if (modelNameFound.Count() > 0)
                        {
                            this.ModelNames = new ObservableCollection<string>(modelNameFound.Distinct().OrderBy(o => o).ToList());
                        }

                        var categoryNameFound = from ftp in TypeProperties select ftp.CategoryName;
                        if (categoryNameFound.Count() > 0)
                        {
                            this.CategoryNames = new ObservableCollection<string>(categoryNameFound.Distinct().OrderBy(o => o).ToList());
                        }

                        this.SelectedModelIndex = 0;
                        this.SelectedCategoryIndex = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export to csv file.\n" + ex.Message, "Export CSV File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
