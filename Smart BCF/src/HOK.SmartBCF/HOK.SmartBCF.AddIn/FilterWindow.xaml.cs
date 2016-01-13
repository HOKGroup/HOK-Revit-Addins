using Autodesk.Revit.DB;
using HOK.SmartBCF.AddIn.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace HOK.SmartBCF.AddIn
{
    /// <summary>
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        private ComponentViewModel compViewModel = null;
        private ObservableCollection<CategoryProperties> categories = new ObservableCollection<CategoryProperties>();

        public ComponentViewModel CompViewModel { get { return compViewModel; } set { compViewModel = value; } }
        public ObservableCollection<CategoryProperties> Categories { get { return categories; } set { categories = value; } }

        public FilterWindow(ComponentViewModel viewModel)
        {
            compViewModel = viewModel;
            InitializeComponent();

            if (compViewModel.RvtComponents.Count > 0)
            {
                var allCategories = from comp in compViewModel.RvtComponents where comp.Category.CategoryId != ElementId.InvalidElementId select comp.Category;
                List <CategoryProperties> categoryList = allCategories.GroupBy(o => o.CategoryId).Select(o => o.First()).ToList();
                ComponentCategoryFilter.UpdateCategoryList(categoryList);
                categories = ComponentCategoryFilter.Categories;
                listboxCategory.ItemsSource = categories;
            }
        }

        private void buttonAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].Selected = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].Selected = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

    public static class ComponentCategoryFilter
    {
        private static ObservableCollection<CategoryProperties> categories = new ObservableCollection<CategoryProperties>();

        public static ObservableCollection<CategoryProperties> Categories { get { return categories; } set { categories = value; } }

        public static void UpdateCategoryList(List<CategoryProperties> catList)
        {
            try
            {
                foreach (CategoryProperties cp in catList)
                {
                    var found = from cat in categories where cat.CategoryId == cp.CategoryId select cat;
                    if (found.Count() == 0)
                    {
                        categories.Add(cp);
                    }
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
        }
    }




}
