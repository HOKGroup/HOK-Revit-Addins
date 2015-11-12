using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace HOK.SheetDataEditor
{
    /// <summary>
    /// Interaction logic for ProjectWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        private RevitSheetData sheetData = null;

        public ProjectWindow(RevitSheetData data)
        {
            sheetData = data;
            InitializeComponent();
            DisplayProjectInfo();
        }

        private void DisplayProjectInfo()
        {
            try
            {
                List<LinkedProject> projects = sheetData.LinkedProjects.Values.OrderBy(o => o.ProjectNumber).ToList();
                dataGridProject.ItemsSource = null;
                dataGridProject.ItemsSource = projects;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the information of linked projects.\n"+ex.Message, "Display Linked Projects", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
