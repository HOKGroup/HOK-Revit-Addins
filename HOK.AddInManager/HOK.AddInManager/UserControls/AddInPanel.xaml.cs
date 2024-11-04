using System;
using System.Windows;
using System.Windows.Controls;
using HOK.AddInManager.Classes;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;

namespace HOK.AddInManager.UserControls
{
    /// <summary>
    /// Interaction logic for AddInPanel.xaml
    /// </summary>
    public partial class AddInPanel
    {
        private bool _userSelection = true;

        public AddInPanel()
        {
            InitializeComponent();
        }

        private void comboBoxLoadType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var postMessage = false;
            try
            {
                var comboBox = DataGridUtilities.FindVisualParent<ComboBox>(e.OriginalSource as UIElement);
                var selectedValue = (LoadType)comboBox.SelectedItem;

                if (!(dataGridAddins.SelectedItems.Count > 1) || !_userSelection)
                {
                    foreach (AddinInfo info in dataGridAddins.SelectedItems)
                    {
                        if (info.DropdownOptionsFlag == 1 && selectedValue == LoadType.Always)
                        {
                            StatusBarManager.StatusLabel.Text =
                                "One/more selected Addins will need Revit restart.";
                        }
                    }

                    return;
                }

                _userSelection = false;
                foreach (AddinInfo info in dataGridAddins.SelectedItems)
                {
                    if (info.DropdownOptionsFlag == 1 && selectedValue == LoadType.Always) postMessage = true;
                    var rowIndex = dataGridAddins.Items.IndexOf(info);
                    var row = (DataGridRow)dataGridAddins.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                    var rowComboBox = DataGridUtilities.FindVisualChild<ComboBox>(row);
                    if (rowComboBox != null)
                    {
                        // (Konrad) If user select ThisSessionOnly, that will not be available for
                        // tools like Dynamo that need a restart to function. In that case we set
                        // their selection to Always. 
                        var i = rowComboBox.Items.IndexOf(selectedValue);
                        if (i == -1) postMessage = true;
                        rowComboBox.SelectedIndex = i == -1 ? 0 : i;
                    }
                }

                dataGridAddins.SelectedItems.Clear();
                _userSelection = true;
                StatusBarManager.StatusLabel.Text = postMessage 
                    ? "One/more selected Addins will need Revit restart."
                    : "Ready";
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
