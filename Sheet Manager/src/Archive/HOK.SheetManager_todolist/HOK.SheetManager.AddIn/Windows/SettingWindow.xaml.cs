using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SheetManager.AddIn.Classes;
using HOK.SheetManager.AddIn.Utils;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.SheetManager.AddIn.Windows
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private Document m_doc = null;
        private SheetManagerConfiguration configuration = null;

        public SheetManagerConfiguration Configuration { get { return configuration; } set { configuration = value; } }

        public SettingWindow(Document doc, SheetManagerConfiguration config)
        {
            m_doc = doc;
            configuration = config;
           
            InitializeComponent();
            DisplayItems();
        }

        private void DisplayItems()
        {
            try
            {
                List<TitleBlockType> titleBlcokTypes = new List<TitleBlockType>();

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<FamilySymbol> symbols = collector.OfCategory(BuiltInCategory.OST_TitleBlocks).WhereElementIsElementType().ToElements().Cast<FamilySymbol>().ToList();
                if (symbols.Count > 0)
                {
                    foreach (FamilySymbol symbol in symbols)
                    {
                        TitleBlockType typeItem = new TitleBlockType(symbol);
                        titleBlcokTypes.Add(typeItem);
                    }

                    titleBlcokTypes = titleBlcokTypes.OrderBy(o => o.DisplayName).ToList();

                    comboBoxTitleblock.ItemsSource = null;
                    comboBoxTitleblock.ItemsSource = titleBlcokTypes;
                    comboBoxTitleblock.DisplayMemberPath = "DisplayName";
                    if (configuration.TitleblockId != ElementId.InvalidElementId)
                    {
                        int index = titleBlcokTypes.FindIndex(o => o.TypeId == configuration.TitleblockId);
                        if (index > -1)
                        {
                            comboBoxTitleblock.SelectedIndex = index;
                        }
                    }
                    else
                    {
                        comboBoxTitleblock.SelectedIndex = 0;
                    }
                }

                if (configuration.IsPlaceholder)
                {
                    radioButtonPlaceholder.IsChecked = true;
                }
                else
                {
                    radioButtonView.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display items.\n" + ex.Message, "Display Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxTitleblock.SelectedItem)
                {
                    TitleBlockType typeItem = (TitleBlockType)comboBoxTitleblock.SelectedItem;
                    configuration.TitleblockId = typeItem.TypeId;
                    if ((bool)radioButtonView.IsChecked)
                    {
                        configuration.IsPlaceholder = false;
                    }
                    if ((bool)radioButtonPlaceholder.IsChecked)
                    {
                        configuration.IsPlaceholder = true;
                    }

                    this.DialogResult = true;
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

    public class TitleBlockType
    {
        private ElementId typeId = ElementId.InvalidElementId;
        private string familyName = "";
        private string typeName = "";
        private string displayName = "";

        public ElementId TypeId { get { return typeId; } set { typeId = value; } }
        public string FamilyName { get { return familyName; } set { familyName = value; } }
        public string TypeName { get { return typeName; } set { typeName = value; } }
        public string DisplayName { get { return displayName; } set { displayName = value; } }

        public TitleBlockType()
        {
        }

        public TitleBlockType(FamilySymbol symbol)
        {
            typeId = symbol.Id;
            familyName = symbol.Family.Name;
            typeName = symbol.Name;
            displayName = familyName + " : " + typeName;
        }
    }
}
