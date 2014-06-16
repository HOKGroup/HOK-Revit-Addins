using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace HOK.ColorSchemeEditor
{
    /// <summary>
    /// Interaction logic for SchemeWindow.xaml
    /// </summary>
    public partial class SchemeWindow : Window
    {
        private ColorSchemeInfo colorSchemeInfo = new ColorSchemeInfo();
        private List<ColorScheme> selectedColorSchemes = new List<ColorScheme>();

        public ColorSchemeInfo SelectedColorSchemeInfo { get { return colorSchemeInfo; } set { colorSchemeInfo = value; } }
        public List<ColorScheme> SelectedColorSchemes { get { return selectedColorSchemes; } set { selectedColorSchemes = value; } }

        public SchemeWindow(ColorSchemeInfo schemeInfo)
        {
            colorSchemeInfo = schemeInfo;
            InitializeComponent();
            DisplaySchemes();
        }

        private void DisplaySchemes()
        {
            try
            {
                List<SchemeInfo> schemeInfoList = new List<SchemeInfo>();
                foreach (ColorScheme scheme in colorSchemeInfo.ColorSchemes)
                {
                    SchemeInfo schemeInfo = new SchemeInfo(scheme);
                    schemeInfoList.Add(schemeInfo);
                }

                schemeInfoList.OrderBy(o => o.SchemeName).ToList();
                listViewSchemes.ItemsSource = schemeInfoList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to dsiplay color schemes.\n" + ex.Message, "Display Color Schemes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

       

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<SchemeInfo> schemeInfoList = listViewSchemes.ItemsSource as List<SchemeInfo>;
                var selectedItems = from item in schemeInfoList where item.IsChecked == true select item;
                if (selectedItems.Count() > 0)
                {
                    selectedColorSchemes = new List<ColorScheme>();
                    foreach (SchemeInfo schemeInfo in selectedItems)
                    {
                        ColorScheme updatedScheme = SetUserDefined(schemeInfo.ColorSchemeObj);
                        selectedColorSchemes.Add(updatedScheme);
                    }
                }

                if (selectedColorSchemes.Count > 0)
                {
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to gather the information of the selected color schemes.\n"+ex.Message, "Color Scheme Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private ColorScheme SetUserDefined(ColorScheme colorScheme)
        {
            ColorScheme updatedScheme = new ColorScheme(colorScheme);
            try
            {
                List<ColorDefinition> colorDefinitions = new List<ColorDefinition>();
                foreach (ColorDefinition definition in colorScheme.ColorDefinitions)
                {
                    definition.UserDefined = true;
                    colorDefinitions.Add(definition);
                }
                updatedScheme.ColorDefinitions = colorDefinitions;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set user defined.\n"+ex.Message, "Set User Defined", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedScheme;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            List<SchemeInfo> schemeInfoList = new List<SchemeInfo>();
            List<SchemeInfo> sourceList = listViewSchemes.ItemsSource as List<SchemeInfo>;
            foreach (SchemeInfo info in sourceList)
            {
                info.IsChecked = true;
                schemeInfoList.Add(info);
            }

            schemeInfoList.OrderBy(o => o.SchemeName).ToList();
            listViewSchemes.ItemsSource = null;
            listViewSchemes.ItemsSource = schemeInfoList;
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            List<SchemeInfo> schemeInfoList = new List<SchemeInfo>();
            List<SchemeInfo> sourceList = listViewSchemes.ItemsSource as List<SchemeInfo>;
            foreach (SchemeInfo info in sourceList)
            {
                info.IsChecked = false;
                schemeInfoList.Add(info);
            }

            schemeInfoList.OrderBy(o => o.SchemeName).ToList();
            listViewSchemes.ItemsSource = null;
            listViewSchemes.ItemsSource = schemeInfoList;
        }
    }

    public class SchemeInfo : INotifyPropertyChanged
    {
        private ColorScheme colorScheme = new ColorScheme();
        private string schemeName = "";
        private int count = 0;
        private bool isChecked = false;

        public ColorScheme ColorSchemeObj { get { return colorScheme; } set { colorScheme = value; } }
        public string SchemeName { get { return schemeName; } set { schemeName = value; } }
        public int Count { get { return count; } set { count = value; } }
        public bool IsChecked { get { return isChecked; } set { isChecked = value; } }

        public SchemeInfo(ColorScheme scheme)
        {
            colorScheme = scheme;
            schemeName = scheme.SchemeName;
            count = scheme.ColorDefinitions.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
