using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using HOK.SheetManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace HOK.SheetManager.Windows.Editor
{
    /// <summary>
    /// Interaction logic for ItemMapperWindow.xaml
    /// </summary>
    public partial class ItemMapperWindow : Window
    {
        private RevitSheetData rvtSheetData = null;
        private List<ItemMap> itemMaps = new List<ItemMap>();

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }
        public List<ItemMap> ItemMaps { get { return itemMaps; } set { itemMaps = value; } }

        public ItemMapperWindow()
        {
            InitializeComponent();
            CollectItemMaps();
        }

        public void CollectItemMaps()
        {
            ItemMap itemMap = new ItemMap(MappingType.Sheet);
            itemMap.ParameterNames.Add("Sheet Name");
            itemMap.ParameterNames.Add("Sheet Number");
            itemMaps.Add(itemMap);

            ItemMap itemMap2 = new ItemMap(MappingType.View);
            itemMap2.ParameterNames.Add("View Name");
            itemMaps.Add(itemMap2);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rvtSheetData = this.DataContext as RevitSheetData;

            comboBoxType.ItemsSource = itemMaps;
            comboBoxType.DisplayMemberPath = "TypeName";
            comboBoxType.SelectedIndex = 0;

        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.NewItems != null && e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (RevitItemMapper item in e.NewItems)
                    {
                        int index = rvtSheetData.ItemMaps.IndexOf(item);
                        Guid itemMapId = Guid.NewGuid();
                        this.RvtSheetData.ItemMaps[index].ItemId = itemMapId;

                        bool dbUpdated = SheetDataWriter.ChangeReplaceItem(rvtSheetData.ItemMaps[index], CommandType.INSERT);
                    }
                }
                if (e.OldItems != null && e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (RevitItemMapper item in e.OldItems)
                    {
                        bool dbUpdated = SheetDataWriter.ChangeReplaceItem(item, CommandType.DELETE);
                    }
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void comboBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxType.SelectedItem)
                {
                    ItemMap selectedItem = comboBoxType.SelectedItem as ItemMap;
                    comboBoxParameter.ItemsSource = selectedItem.ParameterNames;
                    comboBoxParameter.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridItem_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                DataGridRow row = e.Row;
                if (null != row)
                {
                    RevitItemMapper item = row.Item as RevitItemMapper;
                    string propertyName = e.Column.Header.ToString();

                    switch (propertyName)
                    {
                        case "Source Value":
                            TextBox sourceTextBox = e.EditingElement as TextBox;
                            if (null != sourceTextBox)
                            {
                                var itemFound = from itemMap in rvtSheetData.ItemMaps where itemMap.SourceValue == sourceTextBox.Text && itemMap.ItemType == item.ItemType && itemMap.ItemId != item.ItemId select item;
                                if (itemFound.Count() > 0)
                                {
                                    MessageBoxResult msgResult = MessageBox.Show("[" + sourceTextBox.Text + "] Item already exists in the list. \nPlease enter a different value.", "Existing Value", MessageBoxButton.OK, MessageBoxImage.Information);
                                    if (msgResult == MessageBoxResult.OK)
                                    {
                                        e.Cancel = true;
                                    }
                                }
                                else
                                {
                                    item.SourceValue = sourceTextBox.Text;
                                    bool dbUpdated = SheetDataWriter.ChangeReplaceItem(item, CommandType.UPDATE);
                                }
                            }
                            break;
                        case "Target Value":
                            TextBox targetTextBox = e.EditingElement as TextBox;
                            if (null != targetTextBox)
                            {
                                item.TargetValue = targetTextBox.Text;
                                bool dbUpdated = SheetDataWriter.ChangeReplaceItem(item, CommandType.UPDATE);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

   
        private void buttonAddItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxType.SelectedItem && null != comboBoxParameter.SelectedItem)
                {
                    ItemMap itemMap = comboBoxType.SelectedItem as ItemMap;
                    string parameter = comboBoxParameter.SelectedItem.ToString();

                    if (null != itemMap && !string.IsNullOrEmpty(parameter))
                    {
                        string sampleSourceName = "";
                        double suffix = 0;
                        if (dataGridItem.Items.Count > 0)
                        {
                            RevitItemMapper lastItem = dataGridItem.Items[dataGridItem.Items.Count - 1] as RevitItemMapper;
                            sampleSourceName = lastItem.SourceValue.ToString();

                            if (DataGridUtils.GetSuffix(sampleSourceName, out suffix))
                            {
                                sampleSourceName = sampleSourceName.Replace(suffix.ToString(), (suffix + 1).ToString());
                            }
                            else
                            {
                                sampleSourceName += " " + (suffix + 1).ToString();
                            }
                        }

                        if (string.IsNullOrEmpty(sampleSourceName))
                        {
                            sampleSourceName = "New Item 1";
                        }

                        RevitItemMapper itemMapper = new RevitItemMapper(Guid.NewGuid(), itemMap.ItemMapType, parameter, sampleSourceName, "");
                        this.RvtSheetData.ItemMaps.Add(itemMapper);
                        bool dbUpdated = SheetDataWriter.ChangeReplaceItem(itemMapper, CommandType.INSERT);
                    }
                }
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
                if (null != dataGridItem.SelectedCells)
                {
                    var selectedItems = from cell in dataGridItem.SelectedCells select cell.Item;
                    if (selectedItems.Count() > 0)
                    {
                        List<RevitItemMapper> items = selectedItems.Cast<RevitItemMapper>().Distinct().ToList();

                        foreach (RevitItemMapper item in items)
                        {
                            this.RvtSheetData.ItemMaps.Remove(item);
                            bool dbUpdated = SheetDataWriter.ChangeReplaceItem(item, CommandType.DELETE);
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

    public class ItemMap
    {
        private string typeName = "";
        private MappingType itemMapType = MappingType.None;
        private List<string> parameterNames = new List<string>();

        public string TypeName { get { return typeName; } set { typeName = value; } }
        public MappingType ItemMapType { get { return itemMapType; } set { itemMapType = value; } }
        public List<string> ParameterNames { get { return parameterNames; } set { parameterNames = value; } }

        public ItemMap(MappingType mapType)
        {
            itemMapType = mapType;
            typeName = itemMapType.ToString();
        }
    }
}
