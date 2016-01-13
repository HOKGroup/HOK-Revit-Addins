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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HOK.SmartBCF.BCFDBWriter;
using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.Utils;
using HOK.SmartBCF.Windows;
using Microsoft.Win32;

namespace HOK.SmartBCF.UserControls
{
    /// <summary>
    /// Interaction logic for CommandPanel.xaml
    /// </summary>
    public partial class CommandPanel : UserControl
    {
        private BCFViewModel viewModel = null;

        public CommandPanel()
        {
            InitializeComponent();
            ProgressManager.progressBar = progressBar;
            ProgressManager.statusLabel = statusLable;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as BCFViewModel;
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (string file in files)
                    {
                        string fileExtension = System.IO.Path.GetExtension(file);
                        if (fileExtension.Contains("bcfzip"))
                        {
                            if (viewModel.BCFFiles.Count > 0 && !string.IsNullOrEmpty(viewModel.DatabaseFile))
                            {
                                //add bcf to db
                                bool createNewDB = false;
                                OptionWindow optionWindow = new OptionWindow();
                                if ((bool)optionWindow.ShowDialog())
                                {
                                    createNewDB = optionWindow.CreateNewDB;
                                    if (createNewDB)
                                    {
                                        viewModel.SaveDatabase(file);
                                    }
                                    else
                                    {
                                        viewModel.AddBCF(file);
                                    }
                                }
                            }
                            else
                            {
                                //new bcf DB
                                viewModel.SaveDatabase(file);
                            }
                            break;
                        }
                        else if (fileExtension.Contains("sqlite"))
                        {
                            viewModel.OpenDatabase(file);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to drop files.\n" + ex.Message, "File Drop", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        
       

        
    }
}
