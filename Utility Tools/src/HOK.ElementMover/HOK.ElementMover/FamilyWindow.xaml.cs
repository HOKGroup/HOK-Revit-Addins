using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;

namespace HOK.ElementMover
{
    /// <summary>
    /// Interaction logic for FamilyWindow.xaml
    /// </summary>
    public partial class FamilyWindow : Window
    {
        private ExternalEvent m_event = null;
        private MoverHandler m_handler = null;

        private LinkedInstanceProperties selectedLink = null;
        private LinkedFamilyInfo familyInfo = null;
        private Document linkedDoc = null;
        private Document hostDoc = null;

        private List<ElementTypeInfo> sourceTypesInfo = new List<ElementTypeInfo>();
        private List<ElementTypeInfo> targetTypesInfo = new List<ElementTypeInfo>();

        //private List<ElementType> sourceTypes = new List<ElementType>();
        //private List<ElementType> targetTypes = new List<ElementType>();

        public LinkedInstanceProperties SelectedLink { get { return selectedLink; } set { selectedLink = value; } }
        public LinkedFamilyInfo FamilyInfo { get { return familyInfo; } set { familyInfo = value; } }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public FamilyWindow(ExternalEvent extEvent, MoverHandler handler)
        {
            m_event = extEvent;
            m_handler = handler;
            m_handler.FamilyWindowInstance = this;

            selectedLink = m_handler.SelectedLink;
            familyInfo = m_handler.SelectedFamilyInfo;

            linkedDoc = selectedLink.LinkedDocument;
            hostDoc = m_handler.CurrentDocument;
            CollectElementTypes();

            InitializeComponent();

            List<CategoryProperties> categories = selectedLink.Categories.Values.OrderBy(o => o.CategoryName).ToList();
            comboBoxCategory.ItemsSource = null;
            comboBoxCategory.ItemsSource = categories;
            comboBoxCategory.SelectedIndex = 0;

        }

        private void CollectElementTypes()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(linkedDoc);
                List<ElementType> sTypes = collector.OfClass(typeof(ElementType)).ToElements().Cast<ElementType>().ToList();
                var sElementTypes = from type in sTypes where null != type.Category select type;
                sElementTypes = from type in sElementTypes where selectedLink.Categories.ContainsKey(type.Category.Id) select type;
                foreach (ElementType eType in sElementTypes)
                {
                    ElementTypeInfo eTypeInfo = new ElementTypeInfo(eType);
                    sourceTypesInfo.Add(eTypeInfo);
                }

                collector = new FilteredElementCollector(hostDoc);
                List<ElementType> tTypes = collector.OfClass(typeof(ElementType)).ToElements().Cast<ElementType>().ToList();
                var tElementTypes = from type in tTypes where null != type.Category select type;
                tElementTypes = from type in tElementTypes where selectedLink.Categories.ContainsKey(type.Category.Id) select type;
                foreach (ElementType eType in tElementTypes)
                {
                    ElementTypeInfo eTypeInfo = new ElementTypeInfo(eType);
                    targetTypesInfo.Add(eTypeInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect family symbols.\n"+ex.Message, "Collect Family Symbols", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxCategory.SelectedItem)
                {
                    comboBoxSourceFamily.ItemsSource = null;
                    comboBoxTargetFamily.ItemsSource = null;
                    comboBoxSourceType.ItemsSource = null;
                    comboBoxTargetType.ItemsSource = null;

                    CategoryProperties cp = (CategoryProperties)comboBoxCategory.SelectedItem;
                    var sourceFamilies = from stype in sourceTypesInfo where stype.CategoryName == cp.CategoryName select stype.FamilyName;
                    if (sourceFamilies.Count() > 0)
                    {
                        List<string> sourceFamilyNames = sourceFamilies.Distinct().OrderBy(o => o).ToList();
                        comboBoxSourceFamily.ItemsSource = sourceFamilyNames;
                        comboBoxSourceFamily.SelectedIndex = 0;
                    }

                    var targetFamilies = from ttype in targetTypesInfo where ttype.CategoryName == cp.CategoryName select ttype.FamilyName;
                    if (targetFamilies.Count() > 0)
                    {
                        List<string> targetFamilyNames = targetFamilies.Distinct().OrderBy(o => o).ToList();
                        comboBoxTargetFamily.ItemsSource = targetFamilyNames;
                        comboBoxTargetFamily.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a category.\n"+ex.Message, "Select a Category", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }

        private void comboBoxSourceFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxSourceFamily.SelectedItem)
                {
                    string selectedFamily = comboBoxSourceFamily.SelectedItem.ToString();
                    var elementTypes = from sType in sourceTypesInfo
                                       where sType.FamilyName == selectedFamily
                                       select sType;
                    if (elementTypes.Count() > 0)
                    {
                        List<ElementTypeInfo> types = elementTypes.OrderBy(o => o.Name).ToList();
                        
                        comboBoxSourceType.ItemsSource = null;
                        comboBoxSourceType.ItemsSource = types;
                        comboBoxSourceType.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a source family.\n"+ex.Message, "Select a Source Family", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxTargetFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxTargetFamily.SelectedItem)
                {
                    string selectedFamily = comboBoxTargetFamily.SelectedItem.ToString();
                    var elementTypes = from tType in targetTypesInfo
                                       where tType.FamilyName== selectedFamily
                                       select tType;
                    if (elementTypes.Count() > 0)
                    {
                        List<ElementTypeInfo> types = elementTypes.OrderBy(o => o.Name).ToList();

                        comboBoxTargetType.ItemsSource = null;
                        comboBoxTargetType.ItemsSource = types;
                        comboBoxTargetType.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a target family.\n"+ex.Message, "Select a Target Family", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxCategory.SelectedItem && null != comboBoxSourceFamily.SelectedItem && null != comboBoxTargetFamily.SelectedItem
                    && null != comboBoxSourceType.SelectedItem && null != comboBoxTargetType.SelectedItem)
                {
                    ElementTypeInfo sourceType = (ElementTypeInfo)comboBoxSourceType.SelectedItem;
                    ElementTypeInfo targetType = (ElementTypeInfo)comboBoxTargetType.SelectedItem;
                    if (null != sourceType && null != targetType)
                    {
                        familyInfo = new LinkedFamilyInfo(selectedLink.InstanceId, sourceType, targetType);
                        m_handler.SelectedFamilyInfo = familyInfo;
                        m_handler.MoverRequest.Make(RequestId.AddFamilyMapping);
                        m_event.Raise();
                        SetFocus();

                       
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Please make sure you select a category and source and target families", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add/edit the selected family map.\n" + ex.Message, "Family Map", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }

        private void SetFocus()
        {
            IntPtr hBefore = GetForegroundWindow();
            SetForegroundWindow(ComponentManager.ApplicationWindow);
            SetForegroundWindow(hBefore);
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void WakeUp()
        {
            EnableCommands(true);
        }

        public void DozeOff()
        {
            EnableCommands(false);
        }

        private void EnableCommands(bool status)
        {
            comboBoxCategory.IsEnabled = status;
            comboBoxSourceFamily.IsEnabled = status;
            comboBoxSourceType.IsEnabled = status;
            comboBoxTargetFamily.IsEnabled = status;
            comboBoxTargetType.IsEnabled = status;
            buttonApply.IsEnabled = status;
            buttonCancel.IsEnabled = status;
        }

    }
}
