using HOK.AddInManager.Classes;
using HOK.AddInManager.Utils;
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

namespace HOK.AddInManager.UserControls
{
    /// <summary>
    /// Interaction logic for AddInPanel.xaml
    /// </summary>
    public partial class AddInPanel : UserControl
    {
        private int selectedIndex = -1;
        private bool userSelection = true;

        public AddInPanel()
        {
          
            InitializeComponent();
        }

        private void comboBoxLoadType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox comboBox = DataGridUtils.FindVisualParent<ComboBox>(e.OriginalSource as UIElement);
                if (null != dataGridAddins.SelectedItems)
                {
                    if (dataGridAddins.SelectedItems.Count > 1 && userSelection)
                    {
                        userSelection = false;
                        selectedIndex = comboBox.SelectedIndex;
                       
                        foreach (AddinInfo info in dataGridAddins.SelectedItems)
                        {
                            int rowIndex = dataGridAddins.Items.IndexOf(info);
                            DataGridRow row = (DataGridRow)dataGridAddins.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                            ComboBox rowComboBox = DataGridUtils.FindVisualChild<ComboBox>(row);
                            if (null != rowComboBox)
                            {
                                rowComboBox.SelectedIndex = selectedIndex;
                            }
                        }

                        dataGridAddins.SelectedItems.Clear();
                        userSelection = true;
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
