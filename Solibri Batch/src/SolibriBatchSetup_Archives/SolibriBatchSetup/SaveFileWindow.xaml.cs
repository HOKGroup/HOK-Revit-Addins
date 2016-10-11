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

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for SaveFileWindow.xaml
    /// </summary>
    public partial class SaveFileWindow : Window
    {
        private string directoryName = "";
        private string configFileName = "";

        public string ConfigFileName { get { return configFileName; } set { configFileName = value; } }

        public SaveFileWindow(string name)
        {
            directoryName = name;
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBoxFileName.Text))
                {
                    string fileName = textBoxFileName.Text;
                    if (fileName.Contains(".xml"))
                    {
                        configFileName = System.IO.Path.Combine(directoryName, fileName);
                    }
                    else
                    {
                        configFileName = System.IO.Path.Combine(directoryName, fileName + ".xml");
                    }
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please enter a file name", "File Name", MessageBoxButton.OK, MessageBoxImage.Information);
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
