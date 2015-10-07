using System;
using System.Collections.Generic;
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
using Microsoft.Win32;

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private AutorunSettings settings = new AutorunSettings();

        public AutorunSettings Settings { get { return settings; } set { settings = value; } }

        public SettingWindow(AutorunSettings autoSetting)
        {
            settings = autoSetting;
            InitializeComponent();
            DisplaySetting();
        }

        private void DisplaySetting()
        {
            try
            {
                textBoxExe.Text = settings.ExeFile;
                if (settings.RunRemote)
                {
                    radioButtonRemote.IsChecked = true;
                }
                else
                {
                    radioButtonLocal.IsChecked = true;
                }
                textBoxRemote.Text = settings.RemoteDirectory;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display settings.\n" + ex.Message, "Display Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBoxExe.Text))
                {
                    settings.ExeFile = textBoxExe.Text;
                    settings.RunRemote = (bool)radioButtonRemote.IsChecked;
                    if (settings.RunRemote)
                    {
                        if (!string.IsNullOrEmpty(textBoxRemote.Text))
                        {
                            settings.RemoteDirectory = textBoxRemote.Text;
                            this.DialogResult = true;
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid directory name in the remote machine.", "Remote Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        this.DialogResult = true;
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid executable file for Solibri.", "Select a Solbri Executable", MessageBoxButton.OK, MessageBoxImage.Warning); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply settings.\n" + ex.Message, "Apply Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOpenExe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Application (*.exe)|*.exe";
                openFileDialog.RestoreDirectory = false;
                if (!File.Exists(settings.ExeFile)) { openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(settings.ExeFile); }
                openFileDialog.Multiselect = false;
                openFileDialog.Title = "Open Solibri Application Executable";

                if (openFileDialog.ShowDialog() == true)
                {
                    settings.ExeFile = openFileDialog.FileName;
                    textBoxExe.Text = settings.ExeFile;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to assign an excutable for Solibri.\n"+ex.Message, "Select an Excutable", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class AutorunSettings
    {
        private string exeFile = @"C:\Program Files\Solibri\SMCv9.5\Solibri Model Checker v9.5.exe";
        private bool runRemote = true;
        private string remoteDirectory = @"\\NY-BAT-D001\SolibriBatch";

        public string ExeFile { get { return exeFile; } set { exeFile = value; } }
        public bool RunRemote { get { return runRemote; } set { runRemote = value; } }
        public string RemoteDirectory { get { return remoteDirectory; } set { remoteDirectory = value; } }

        public AutorunSettings()
        {
        }
    }
}
