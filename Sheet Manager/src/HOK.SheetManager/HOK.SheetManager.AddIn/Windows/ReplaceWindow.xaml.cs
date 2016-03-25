using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SheetManager.AddIn.Classes;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
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

namespace HOK.SheetManager.AddIn.Windows
{
    /// <summary>
    /// Interaction logic for ReplaceWindow.xaml
    /// </summary>
    public partial class ReplaceWindow : Window
    {
        private MappingType mappingType = MappingType.None;
        private string parameterName = "";
        private RevitSheetData rvtSheetData = null;
        private List<RevitItemMapper> selectedItems = new List<RevitItemMapper>();

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }
        public List<RevitItemMapper> SelectedItems { get { return selectedItems; } set { selectedItems = value; } }

        public ReplaceWindow(MappingType mapType, string paramName)
        {
            mappingType = mapType;
            parameterName = paramName;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtSheetData = this.DataContext as RevitSheetData;

                FindItemsSource();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void FindItemsSource()
        {
            try
            {
                dataGridItems.ItemsSource = null;
                switch (mappingType)
                {
                    case MappingType.Sheet:
                        dataGridItems.ItemsSource = FindSheetItems();
                        break;
                    case MappingType.View:
                        dataGridItems.ItemsSource = FindViewItems();
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private ObservableCollection<RevitItemMapper> FindSheetItems()
        {
            ObservableCollection<RevitItemMapper> sheetItems = new ObservableCollection<RevitItemMapper>();
            try
            {
                switch (parameterName)
                {
                    case "Sheet Number":
                        var existingSheetNumbers = from sheet in rvtSheetData.Sheets where sheet.LinkStatus.IsLinked select sheet.Number;
                        if (existingSheetNumbers.Count() > 0)
                        {
                            var numberItemFound = from item in rvtSheetData.ItemMaps
                                                  where item.ParameterName == parameterName && item.ItemType == mappingType && existingSheetNumbers.Contains(item.SourceValue)
                                                  select item;
                            if (numberItemFound.Count() > 0)
                            {
                                sheetItems = new ObservableCollection<RevitItemMapper>(numberItemFound.ToList());
                            }
                        }
                        break;
                    case "Sheet Name":
                        var existingSheetNames = from sheet in rvtSheetData.Sheets where sheet.LinkStatus.IsLinked select sheet.Name;
                        if (existingSheetNames.Count() > 0)
                        {
                            var nameItemFound = from item in rvtSheetData.ItemMaps
                                                where item.ParameterName == parameterName && item.ItemType == mappingType && existingSheetNames.Contains(item.SourceValue)
                                                select item;
                            if (nameItemFound.Count() > 0)
                            {
                                sheetItems = new ObservableCollection<RevitItemMapper>(nameItemFound.ToList());
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return sheetItems;
        }

        private ObservableCollection<RevitItemMapper> FindViewItems()
        {
            ObservableCollection<RevitItemMapper> viewItems = new ObservableCollection<RevitItemMapper>();
            try
            {
                var existingViewNames = from view in rvtSheetData.Views where view.LinkStatus.IsLinked select view.Name;
                if (existingViewNames.Count() > 0)
                {
                    var nameItemFound = from item in rvtSheetData.ItemMaps
                                         where item.ParameterName == parameterName && item.ItemType == mappingType && existingViewNames.Contains(item.SourceValue)
                                         select item;
                    if (nameItemFound.Count() > 0)
                    {
                        viewItems = new ObservableCollection<RevitItemMapper>(nameItemFound.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return viewItems;
        }

        private void buttonReplace_Click(object sender, RoutedEventArgs e)
        {
            UpdateSource();
            this.DialogResult = true;
        }

       
        private void UpdateSource()
        {
            try
            {
                ObservableCollection<RevitItemMapper> mapItems = dataGridItems.ItemsSource as ObservableCollection<RevitItemMapper>;
                var selectedFound = from item in mapItems where item.IsSelected select item;
                if (selectedFound.Count() > 0)
                {
                    selectedItems = selectedFound.ToList();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<RevitItemMapper> mapItems = dataGridItems.ItemsSource as ObservableCollection<RevitItemMapper>;
                for (int i = 0; i < mapItems.Count; i++)
                {
                    mapItems[i].IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<RevitItemMapper> mapItems = dataGridItems.ItemsSource as ObservableCollection<RevitItemMapper>;
                for (int i = 0; i < mapItems.Count; i++)
                {
                    mapItems[i].IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
