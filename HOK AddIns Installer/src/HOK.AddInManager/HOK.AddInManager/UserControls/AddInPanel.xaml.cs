using System;
using System.Windows;
using System.Windows.Controls;
using HOK.AddInManager.Classes;
using HOK.Core.Utilities;

namespace HOK.AddInManager.UserControls
{
    /// <summary>
    /// Interaction logic for AddInPanel.xaml
    /// </summary>
    public partial class AddInPanel
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
                var comboBox = Core.WpfUtilities.DataGrid.FindVisualParent<ComboBox>(e.OriginalSource as UIElement);
                if (!(dataGridAddins.SelectedItems?.Count > 1) || !userSelection) return;

                userSelection = false;
                selectedIndex = comboBox.SelectedIndex;
                       
                foreach (AddinInfo info in dataGridAddins.SelectedItems)
                {
                    var rowIndex = dataGridAddins.Items.IndexOf(info);
                    var row = (DataGridRow)dataGridAddins.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                    var rowComboBox = Core.WpfUtilities.DataGrid.FindVisualChild<ComboBox>(row);
                    if (null != rowComboBox)
                    {
                        rowComboBox.SelectedIndex = selectedIndex;
                    }
                }

                dataGridAddins.SelectedItems.Clear();
                userSelection = true;
            }
            catch (Exception ex)
            {
                Log.AppendLog("HOK.AddInManager.UserControls.AddInPanel.comboBoxLoadType_SelectionChanged: " + ex.Message);
            }
        }
    }
}
