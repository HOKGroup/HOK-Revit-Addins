using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.UserControls;
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
using WinForm = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private BCFViewModel viewModel = null;
        private RevitExtensionInfo extInfo = new RevitExtensionInfo();
        private int primaryIndex = -1;
        private bool colorChanged = false;
        private List<RevitExtension> deletedExts = new List<RevitExtension>(); //to delete from database

        private string[] parameters = { "BCF_Action", "BCF_Responsibility" };

        public BCFViewModel ViewModel { get { return viewModel; } set { viewModel = value; } }
        public RevitExtensionInfo ExtInfo { get { return extInfo; } set { extInfo = value; } }
        public int PrimaryIndex { get { return primaryIndex; } set { primaryIndex = value; } }
        public bool ColorChanged { get { return colorChanged; } set { colorChanged = value; } }

        public SettingsWindow(BCFViewModel bcfViewModel)
        {
            viewModel = bcfViewModel;
            primaryIndex = viewModel.PrimaryFileIndex;
            extInfo = viewModel.BCFFiles[primaryIndex].ExtensionColor;
            InitializeComponent();

            comboBoxParameter.ItemsSource = null;
            comboBoxParameter.ItemsSource = parameters;
 
            dataGridColor.ItemsSource = extInfo.Extensions;

            comboBoxPrimary.ItemsSource = viewModel.BCFFiles;
            comboBoxPrimary.DisplayMemberPath = "ZipFileName";
            comboBoxPrimary.SelectedIndex = viewModel.PrimaryFileIndex;
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (deletedExts.Count > 0)
                {
                    bool deleted = BCFDBWriter.BCFDBWriter.ReplaceObsoleteExtensions(deletedExts);
                }

                if (colorChanged)
                {
                    bool updated = BCFDBWriter.BCFDBWriter.UpdateExtensions(extInfo);
                    for (int i = 0; i < viewModel.BCFFiles.Count; i++)
                    {
                        viewModel.BCFFiles[i].ExtensionColor = extInfo;
                    }
                }
                
                if (null != comboBoxPrimary.SelectedItem)
                {
                    viewModel.PrimaryFileIndex = comboBoxPrimary.SelectedIndex;
                    for (int i = 0; i < viewModel.BCFFiles.Count; i++)
                    {
                        bool isPrimary = (i == viewModel.PrimaryFileIndex)? true : false;
                        viewModel.BCFFiles[i].IsPrimary = isPrimary;
                    }
                    bool updated = BCFDBWriter.BCFDBWriter.UpdateBCFFileInfo(viewModel.BCFFiles);
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply settings.\n" + ex.Message, "Apply Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridRow row = WPFUtil.FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);
                if (null != row)
                {
                    RevitExtension selectedExt = row.Item as RevitExtension;
                    if (null != selectedExt)
                    {
                        if (selectedExt.Guid == Guid.Empty.ToString()) { return; }

                        WinForm.ColorDialog colorDialog = new WinForm.ColorDialog();
                        if (colorDialog.ShowDialog() == WinForm.DialogResult.OK)
                        {
                            System.Drawing.Color selectedColor = colorDialog.Color;
                            Button button = e.OriginalSource as Button;
                            if (null != button)
                            {
                                int index = extInfo.Extensions.IndexOf(selectedExt);
                                extInfo.Extensions[index].Color[0] = selectedColor.R;
                                extInfo.Extensions[index].Color[1] = selectedColor.G;
                                extInfo.Extensions[index].Color[2] = selectedColor.B;
                                button.Background = new SolidColorBrush(Color.FromRgb(selectedColor.R, selectedColor.G, selectedColor.B));
                            }
                            colorChanged = true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
            
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonAddItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RevitExtension rvtExtension = new RevitExtension();
                rvtExtension.Guid = Guid.NewGuid().ToString();
                rvtExtension.ParameterName = parameters[0];
                rvtExtension.ParameterValue = "New Parameter";
                Random random = new Random();
                rvtExtension.Color = new byte[3];
                rvtExtension.Color[0] = (byte)random.Next(255);
                rvtExtension.Color[1] = (byte)random.Next(255);
                rvtExtension.Color[2] = (byte)random.Next(255);
                extInfo.Extensions.Add(rvtExtension);
                colorChanged = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridColor.SelectedItem)
                {
                    RevitExtension selectedExt = dataGridColor.SelectedItem as RevitExtension;
                    if (null != selectedExt)
                    {
                        deletedExts.Add(selectedExt);
                        bool deleted = extInfo.Extensions.Remove(selectedExt);
                        colorChanged = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridColor_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (null != e.Row)
                {
                    DataGridRow row = e.Row;
                    RevitExtension extension = row.Item as RevitExtension;
                    if (null != extension)
                    {
                        if (extension.Guid == Guid.Empty.ToString())
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
