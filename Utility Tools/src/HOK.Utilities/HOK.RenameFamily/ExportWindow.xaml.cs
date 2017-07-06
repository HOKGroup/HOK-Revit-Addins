using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.RenameFamily.Classes;
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.RenameFamily
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private UIApplication m_app;
        private Document m_doc;
        private List<ElementType> elementTypes = new List<ElementType>();
        private ObservableCollection<CategoryInfo> categories = new ObservableCollection<CategoryInfo>();
        private List<FamilyTypeProperties> typeProperties = new List<FamilyTypeProperties>();
        private string fileName = "";
        private string[] columnNames = new string[] { "Model", "Type ID", "Family Name", "Type Name" };

        public List<ElementType> ElementTypes { get { return elementTypes; } set { elementTypes = value; } }
        public List<FamilyTypeProperties> TypeProperties { get { return typeProperties; } set { typeProperties = value; } }
        public ObservableCollection<CategoryInfo> Categories { get { return categories; } set { categories = value; } }
        public string FileName { get { return fileName; } set { fileName = value; } }

        public ExportWindow(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayCategories();
        }

        private void DisplayCategories()
        {
            try
            {
                categories.Clear();
                var categoryFound = from eType in elementTypes where null != eType.Category && !string.IsNullOrEmpty(eType.Category.Name) select eType.Category;
                if (categoryFound.Count() > 0)
                {
                    List<Category> categoryList = categoryFound.GroupBy(o => o.Name).Select(o => o.First()).ToList();
                    categoryList = categoryList.OrderBy(o => o.Name).ToList();
                    foreach (Category cat in categoryList)
                    {
                        CategoryInfo catInfo = new CategoryInfo(cat);
                        this.Categories.Add(catInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display categories.\n" + ex.Message, "Display Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                typeProperties.Clear();

                var selectedCategories = from cat in categories where cat.IsSelected select cat;
                if (selectedCategories.Count() > 0)
                {
                    List<CategoryInfo> categoryList = selectedCategories.ToList();
                   
                    string modelName = m_doc.Title;
                    
                    foreach (CategoryInfo catInfo in categoryList)
                    {
                        var elementTypeFound = from eType in elementTypes where null != eType.Category && eType.Category.Id == catInfo.CategoryId select eType;
                        if (elementTypeFound.Count() > 0)
                        {
                            foreach (ElementType eType in elementTypeFound)
                            {
                                string familyName = "";
                                familyName = eType.FamilyName;
                                FamilyTypeProperties ftp = new FamilyTypeProperties(modelName, eType.Id.IntegerValue, familyName, eType.Name);
                                ftp.SetCurrentFamily(eType);
                                typeProperties.Add(ftp);
                            }
                        }
                    }

                    if (typeProperties.Count > 0)
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Title = "Save a CSV File";
                        saveFileDialog.DefaultExt = ".csv";
                        saveFileDialog.Filter = "Comma Separated Values (.csv)|*.csv";

                        if ((bool)saveFileDialog.ShowDialog())
                        {
                            fileName = saveFileDialog.FileName;
                            if (WriteCSV(fileName, typeProperties))
                            {
                                MessageBox.Show("File successfully saved in \n" + fileName, "File Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                                this.DialogResult = true;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export family and type names.\n" + ex.Message, "Export Names", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool WriteCSV(string filePath,  List<FamilyTypeProperties> familyTypes)
        {
            bool result = false;
            try
            {
                List<string> contents = new List<string>();
                contents.Add(columnNames[0] + ", " + columnNames[1] + ", " + columnNames[2] + ", " + columnNames[3]);
                foreach (FamilyTypeProperties ftp in familyTypes)
                {
                    string row = "";
                    row += ConvertStringForCSV(ftp.ModelName) + ", ";
                    row += ftp.FamilyTypeIdInt + ", ";
                    row += ConvertStringForCSV(ftp.FamilyName) + ", ";
                    row += ConvertStringForCSV(ftp.TypeName);
                    contents.Add(row);
                }

                File.WriteAllLines(filePath, contents);
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write csv file.\n" + ex.Message, "Write CSV", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private string ConvertStringForCSV(string originStr)
        {
            string convertedStr = originStr;
            try
            {
                if (convertedStr.Contains(",") || convertedStr.Contains("\"") || convertedStr.Contains("\n") || convertedStr.Contains("\r"))
                {
                    convertedStr = '"' + convertedStr.Replace("\"", "\"\"") + '"';
                }  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to convert to string for csv.\n" + ex.Message, "Convert String for CSV", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return convertedStr;
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    this.Categories[i].IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonUncheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    this.Categories[i].IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }

    public class CategoryInfo : INotifyPropertyChanged
    {
        private ElementId categoryId = ElementId.InvalidElementId;
        private string categoryName = "";
        private bool isSelected = false;

        public ElementId CategoryId { get { return categoryId; } set { categoryId = value; NotifyPropertyChanged("CategoryId"); } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; NotifyPropertyChanged("CategoryName"); } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public CategoryInfo(Category category)
        {
            categoryId = category.Id;
            categoryName = category.Name;
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
