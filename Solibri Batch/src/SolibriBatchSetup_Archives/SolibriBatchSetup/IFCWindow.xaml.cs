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
using Microsoft.Win32;
using SolibriBatchSetup.Schema;
using System.Collections.ObjectModel;

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for IFCWindow.xaml
    /// </summary>
    public partial class IFCWindow : Window
    {
        private ProcessUnit unit = new ProcessUnit();
        private string[] disciplines = new string[]
        {
            "Architectural", "Air Conditioning", "Building Services", "Electrical", "Heat", "Structural", "Ventilation", "Plumbing", "Sprinkler", "Inventory", "Facility Management",
            "Landscape", "Prefab Concrete", "Steel Structure", "Site Operations", "Cooling", "Special Piping", "Process", "HVAC"
        };
        public ProcessUnit Unit { get { return unit; } set { unit = value; } }
 
        public IFCWindow(ProcessUnit pUnit)
        {
            unit = pUnit;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                dataGridIfc.ItemsSource = null;
                ObservableCollection<OpenModel> openIfcs = new ObservableCollection<OpenModel>(unit.IfcFiles.OrderBy(o => o.File).ToList());
                dataGridIfc.ItemsSource = openIfcs;

                disciplines = disciplines.OrderBy(o => o).ToArray();
                DataGridColumnDiscipline.ItemsSource = disciplines;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        private void buttonAddSolibri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Solibri Model Checker Files (*.smc)|*.smc";
                saveDialog.Title = "Assign a Solibri File Path";
                if (saveDialog.ShowDialog() == true)
                {
                    string solibriPath = saveDialog.FileName;
                    textBoxSolibri.Text = solibriPath;
                    
                    unit.OpenSolibri = new OpenModel(solibriPath);
                    unit.SaveSolibri = new SaveModel(solibriPath);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCombine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(unit.OpenSolibri.File))
                {
                    
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please select a solibri file path to combine IFC files.", "Solibri File Path", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add configuration unit to combine ifc files to smc.\n" + ex.Message, "Combine IFC files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

       
        
    }
}
