using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for OpenFileWindow.xaml
    /// </summary>
    public partial class OpenFileWindow : Window
    {
        private string directoryName = "";
        private ObservableCollection<ConfigFileProperties> configFiles = new ObservableCollection<ConfigFileProperties>();
        private string configFileName = "";
        
        public string ConfigFileName { get { return configFileName; } set { configFileName = value; } }

        public OpenFileWindow(string directory)
        {
            directoryName = directory;
            InitializeComponent();
            GetConfigFiles();
        }

        private void GetConfigFiles()
        {
            try
            {
                if (Directory.Exists(directoryName))
                {
                    string[] files = Directory.GetFiles(directoryName, "*.xml");
                    foreach (string file in files)
                    {
                        ConfigFileProperties config = new ConfigFileProperties(file);
                        configFiles.Add(config);
                    }
                    dataGridFile.ItemsSource = null;
                    dataGridFile.ItemsSource = configFiles;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find configuration files.\n" + ex.Message, "Configuration Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridFile.SelectedItem)
                {
                    ConfigFileProperties selectedConfig = dataGridFile.SelectedItem as ConfigFileProperties;
                    configFileName = selectedConfig.FullFileName;
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }

    public class ConfigFileProperties
    {
        private string fullFileName = "";
        private string fileNameOnly = "";

        public string FullFileName { get { return fullFileName; } set { fullFileName = value; } }
        public string FileNameOnly { get { return fileNameOnly; } set { fileNameOnly = value; } }
        
        public ConfigFileProperties(string fileName)
        {
            fullFileName = fileName;
            fileNameOnly = System.IO.Path.GetFileNameWithoutExtension(fullFileName);
        }
    }
}
