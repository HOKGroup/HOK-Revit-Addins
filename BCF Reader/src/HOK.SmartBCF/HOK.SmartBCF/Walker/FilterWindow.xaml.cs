using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Autodesk.Revit.DB;

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        private List<CategoryInfo> categoryInfoList = new List<CategoryInfo>();
        private List<CategoryInfo> initCategoryInfoList = new List<CategoryInfo>();

        private BitmapImage checkAllImage = null;
        private BitmapImage checkNoneImage = null;
        
        private bool isCheckAll = true;

        public List<CategoryInfo> CategoryInfoList { get { return categoryInfoList; } set { categoryInfoList = value; } }

        public FilterWindow(List<CategoryInfo> categoryList)
        {
            foreach (CategoryInfo catInfo in categoryList)
            {
                CategoryInfo categoryInfo = new CategoryInfo(catInfo.CategoryName, catInfo.IsSelected);
                initCategoryInfoList.Add(categoryInfo);
            }
            categoryInfoList = categoryList;
            categoryInfoList = categoryInfoList.OrderBy(o => o.CategoryName).ToList();

            InitializeComponent();
            checkAllImage = LoadBitmapImage("checkbox_yes.png");
            checkNoneImage = LoadBitmapImage("checkbox_no.png");
            buttonCheckImage.Source = checkAllImage;
            dataGridCategory.ItemsSource = categoryInfoList;
        }

        private BitmapImage LoadBitmapImage(string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string prefix = typeof(AppCommand).Namespace + ".Resources.";
                Stream stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load check box button image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
        }

        private void buttonCheck_Click(object sender, RoutedEventArgs e)
        {
            if (isCheckAll)
            {
                //check all
                List<CategoryInfo> categoryList = dataGridCategory.ItemsSource as List<CategoryInfo>;
                dataGridCategory.ItemsSource = null;
                for (int i = 0; i < categoryList.Count; i++)
                {
                    categoryList[i].IsSelected = true;
                }
                dataGridCategory.ItemsSource = categoryList;
                isCheckAll = false; //change to check none
                buttonCheckImage.Source = checkNoneImage;
            }
            else
            {
                //check none
                List<CategoryInfo> categoryList = dataGridCategory.ItemsSource as List<CategoryInfo>;
                dataGridCategory.ItemsSource = null;
                for (int i = 0; i < categoryList.Count; i++)
                {
                    categoryList[i].IsSelected = false;
                }
                dataGridCategory.ItemsSource = categoryList;
                isCheckAll = true; //change to check all
                buttonCheckImage.Source = checkAllImage;
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            categoryInfoList = (List<CategoryInfo>)dataGridCategory.ItemsSource;
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            dataGridCategory.ItemsSource = null;
            categoryInfoList = new List<CategoryInfo>();
            categoryInfoList = initCategoryInfoList;
            this.DialogResult = false;
        }
    }

    public class CategoryInfo
    {
        private string categoryName = "";
        private bool isSelected = true;

        public string CategoryName { get { return categoryName; } set { categoryName = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public CategoryInfo(string catName, bool selected)
        {
            categoryName = catName;
            isSelected = selected;
        }
    }

}
