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
using ComponentManager = Autodesk.Windows.ComponentManager;

namespace HOK.ElementMover
{
    /// <summary>
    /// Interaction logic for MappingWindow.xaml
    /// </summary>
    public partial class MappingWindow : Window
    {
        private ExternalEvent m_event = null;
        private MoverHandler m_handler = null;
        private FamilyWindow familyWindow = null;

        private Dictionary<ElementId/*instanceId*/, LinkedInstanceProperties> linkInstances = new Dictionary<ElementId, LinkedInstanceProperties>();
        private LinkedInstanceProperties selectedLink = null;

        public Dictionary<ElementId, LinkedInstanceProperties> LinkInstances { get { return linkInstances; } set { linkInstances = value; } }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public MappingWindow(ExternalEvent extEvent, MoverHandler handler)
        {
            m_event = extEvent;
            m_handler = handler;
            m_handler.MappingWindowInstance = this;

            linkInstances = m_handler.LinkInstances;
            selectedLink = m_handler.SelectedLink;

            InitializeComponent();

            var instances = linkInstances.Values.OrderBy(o => o.DisplayName).ToList();
            comboBoxLink.ItemsSource = instances;
            comboBoxLink.DisplayMemberPath = "DisplayName";
            var selectedIndex = instances.FindIndex(o => o.InstanceId == selectedLink.InstanceId);
            if (selectedIndex > -1) { comboBoxLink.SelectedIndex = selectedIndex; }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxLink.SelectedItem)
                {
                    var lip = (LinkedInstanceProperties)comboBoxLink.SelectedItem;
                    m_handler.SelectedLink = lip;
                    m_handler.MoverRequest.Make(RequestId.SelectMappingElements);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add an element map.\n"+ex.Message, "Add Element Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxLink.SelectedItem)
                {
                    var lip = (LinkedInstanceProperties)comboBoxLink.SelectedItem;
                    m_handler.SelectedLink = lip;

                    var linkedInfoToDelete = new List<LinkedElementInfo>();
                    var treeviewModels = treeViewMapping.ItemsSource as List<TreeViewElementModel>;
                    foreach (var categoryNode in treeviewModels)
                    {
                        var infoList = FindElementMappingNode(categoryNode);
                        if (infoList.Count > 0)
                        {
                            linkedInfoToDelete.AddRange(infoList);
                        }
                    }

                    m_handler.LinkedElementToDelete = linkedInfoToDelete;
                    m_handler.MoverRequest.Make(RequestId.DeleteMappingElements);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove an element map.\n"+ex.Message, "Remove Element Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<LinkedElementInfo> FindElementMappingNode(TreeViewElementModel parentNode)
        {
            var linkedElements = new List<LinkedElementInfo>();
            try
            {
                foreach (var node in parentNode.ChildrenNodes)
                {
                    if (node.NodeType == TreeViewNodeType.ElementMapping)
                    {
                        if (node.IsChecked == true)
                        {
                            var linkedInfo = node.Tag as LinkedElementInfo;
                            linkedElements.Add(linkedInfo);
                        }
                    }
                    else
                    {
                        var infoList = FindElementMappingNode(node);
                        if (infoList.Count > 0)
                        {
                            linkedElements.AddRange(infoList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find mapping node.\n" + ex.Message, "Find Mapping Node", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedElements;
        }

        public void RefreshLinkInstance()
        {
            try
            {
                linkInstances = m_handler.LinkInstances;
                selectedLink = m_handler.SelectedLink;
                var instances = linkInstances.Values.OrderBy(o => o.DisplayName).ToList();
                comboBoxLink.ItemsSource = null;
                comboBoxLink.ItemsSource = instances;

                var selectedIndex = instances.FindIndex(o => o.InstanceId == selectedLink.InstanceId);
                if (selectedIndex > -1) { comboBoxLink.SelectedIndex = selectedIndex; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh Link Instance Info.\n" + ex.Message, "Refresh Link Instance", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SetFocus()
        {
            var hBefore = GetForegroundWindow();
            SetForegroundWindow(ComponentManager.ApplicationWindow);
            SetForegroundWindow(hBefore);
        }

        private void comboBoxLink_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxLink.SelectedItem)
                {
                    selectedLink = (LinkedInstanceProperties)comboBoxLink.SelectedItem;
                    treeViewMapping.ItemsSource = null;
                    treeViewMapping.ItemsSource = TreeViewElementModel.SetTreeView(selectedLink);
                    
                    treeViewFamilyMapping.ItemsSource = null;
                    treeViewFamilyMapping.ItemsSource = TreeViewFamilyModel.SetTreeView(selectedLink);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a Revit Link.\n"+ex.Message, "Select a Revit Link", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
            comboBoxLink.IsEnabled = status;
            buttonAdd.IsEnabled = status;
            buttonRemove.IsEnabled = status;
            buttonAddFamily.IsEnabled = status;
            buttonRemoveFamily.IsEnabled = status;
            buttonOK.IsEnabled = status;
        }

        private void treeViewMapping_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var selectedItem = e.NewValue as TreeViewElementModel;
                if (null != selectedItem)
                {
                    if (selectedItem.NodeType == TreeViewNodeType.ElementMapping)
                    {
                        var linkInfo = selectedItem.Tag as LinkedElementInfo;
                        if (null != linkInfo)
                        {
                            m_handler.SelectedLinkedInfo = linkInfo;
                            m_handler.MoverRequest.Make(RequestId.ShowElement);
                            m_event.Raise();
                            SetFocus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to navigate to the selected item.\n"+ex.Message, "Selected Mapping Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddFamily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != m_handler.SelectedLink)
                {
                    if (null == familyWindow)
                    {
                        m_handler.SelectedFamilyInfo = null;
                        familyWindow = new FamilyWindow(m_event, m_handler);
                        familyWindow.Owner = this;
                        familyWindow.Closed += WindowClosed;
                        familyWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add a family map.\n"+ex.Message, "Add Family Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRemoveFamily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxLink.SelectedItem)
                {
                    var lip = (LinkedInstanceProperties)comboBoxLink.SelectedItem;
                    m_handler.SelectedLink = lip;

                    var linkedInfoToDelete = new List<LinkedFamilyInfo>();
                    var treeviewModels = treeViewFamilyMapping.ItemsSource as List<TreeViewFamilyModel>;
                    foreach (var categoryNode in treeviewModels)
                    {
                        var infoList = FindFamilyMappingNode(categoryNode);
                        if (infoList.Count > 0)
                        {
                            linkedInfoToDelete.AddRange(infoList);
                        }
                    }

                    m_handler.LinkedFamilyToDelete = linkedInfoToDelete;
                    m_handler.MoverRequest.Make(RequestId.DeleteFamilyMapping);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove the family map.\n"+ex.Message, "Remove Family Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<LinkedFamilyInfo> FindFamilyMappingNode(TreeViewFamilyModel parentNode)
        {
            var linkedFamilies = new List<LinkedFamilyInfo>();
            try
            {
                foreach (var node in parentNode.ChildrenNodes)
                {
                    if (node.NodeType == TreeViewNodeType.FamilyType)
                    {
                        if (node.IsChecked == true)
                        {
                            var linkedInfo = node.Tag as LinkedFamilyInfo;
                            if (null != linkedInfo)
                            {
                                linkedFamilies.Add(linkedInfo);
                            }
                        }
                    }
                    else
                    {
                        var infoList = FindFamilyMappingNode(node);
                        if (infoList.Count > 0)
                        {
                            linkedFamilies.AddRange(infoList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find mapping node.\n" + ex.Message, "Find Mapping Node", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedFamilies;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowClosed(object sender, System.EventArgs e)
        {
            familyWindow = null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != familyWindow)
            {
                e.Cancel = true;
            }
        }

        
    }
}
