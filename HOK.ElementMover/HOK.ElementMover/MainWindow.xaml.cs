﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ComponentManager = Autodesk.Windows.ComponentManager;

namespace HOK.ElementMover
{
    public enum UpdateMode
    {
        UpdateLocationOnly, ReplaceElements, None
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ExternalEvent m_event;
        private MoverHandler m_handler;

        private Dictionary<ElementId/*instanceId*/, LinkedInstanceProperties> linkInstances = new Dictionary<ElementId, LinkedInstanceProperties>();
        private LinkedInstanceProperties selectedLink;

        private MappingWindow mappingWindow;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public MainWindow(ExternalEvent extEvent, MoverHandler handler)
        {
            m_event = extEvent;
            m_handler = handler;
            m_handler.MainWindowInstance = this;

            linkInstances = m_handler.LinkInstances;

            InitializeComponent();

            Title = "Element Mover v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            
            var instanceList = linkInstances.Values.OrderBy(o => o.DisplayName).ToList();
            comboBoxLinkModel.ItemsSource = instanceList;
            comboBoxLinkModel.DisplayMemberPath = "DisplayName";
            comboBoxLinkModel.SelectedIndex = 0;

            ElementMoverUtil.progressBar = progressBar;
            ElementMoverUtil.statusLabel = statusLable;
        }

        private void SetFocus()
        {
            var hBefore = GetForegroundWindow();
            SetForegroundWindow(ComponentManager.ApplicationWindow);
            SetForegroundWindow(hBefore);
        }

        private void comboBoxLinkModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxLinkModel.SelectedItem)
                {
                    selectedLink = (LinkedInstanceProperties)comboBoxLinkModel.SelectedItem;
                    m_handler.SelectedLink = selectedLink;
                    var catList = selectedLink.Categories.Values.OrderBy(o => o.CategoryName).ToList();
                    dataGridCategory.ItemsSource = catList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a linked model.\n"+ex.Message, "Link Model Selection Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DisplayCategories(LinkedInstanceProperties lip)
        {
            try
            {
                var instanceList = (List<LinkedInstanceProperties>)comboBoxLinkModel.ItemsSource;
                if (instanceList.Count > 0)
                {
                    for (var i = 0; i < instanceList.Count; i++)
                    {
                        if (instanceList[i].InstanceId == lip.InstanceId)
                        {
                            comboBoxLinkModel.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display category information from the selected Revit Link.\n"+ex.Message, "Display Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var catList = (List<CategoryProperties>)dataGridCategory.ItemsSource;
                var updatedList = new List<CategoryProperties>();
                foreach (var cp in catList)
                {
                    cp.Selected = true;
                    updatedList.Add(cp);
                }
                dataGridCategory.ItemsSource = null;
                dataGridCategory.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select all category items.\n" + ex.Message, "Select All Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var catList = (List<CategoryProperties>)dataGridCategory.ItemsSource;
                var updatedList = new List<CategoryProperties>();
                foreach (var cp in catList)
                {
                    cp.Selected = false;
                    updatedList.Add(cp);
                }
                dataGridCategory.ItemsSource = null;
                dataGridCategory.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unselect all category items.\n" + ex.Message, "Unselect All Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            if (null == mappingWindow)
            {
                this.Close();
            }
        }

        private void buttonDuplicate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedLink)
                {
                    var catList = (List<CategoryProperties>)dataGridCategory.ItemsSource;
                    var dictionary = catList.Distinct().ToDictionary(x => x.CategoryId, x => x);
                    
                    selectedLink.Categories = dictionary;
                    m_handler.SelectedLink = selectedLink;

                    var updateMode = UpdateMode.None;
                    if ((bool)radioButtonLocation.IsChecked) { updateMode = UpdateMode.UpdateLocationOnly; }
                    if ((bool)radioButtonReplace.IsChecked) { updateMode = UpdateMode.ReplaceElements; }
                    
                    m_handler.SelectedUpdateMode = updateMode;

                    m_handler.MoverRequest.Make(RequestId.DuplicateElements);
                    m_event.Raise();
                    SetFocus();
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate elements.\n"+ex.Message, "Duplicate Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void UpdateLinkedInstanceProperties()
        {
            try
            {
                linkInstances = m_handler.LinkInstances;

                var instanceList = linkInstances.Values.OrderBy(o => o.DisplayName).ToList();
                comboBoxLinkModel.ItemsSource = null;
                comboBoxLinkModel.ItemsSource = instanceList;
                DisplayCategories(m_handler.SelectedLink);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the linked instance properties.\n"+ex.Message, "Update Linked Instance", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_handler.MoverRequest.Make(RequestId.SelectLinkInstance);
                m_event.Raise();
                SetFocus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a link instance from the project.\n" + ex.Message, "Select Revit Link", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null == mappingWindow)
                {
                    mappingWindow = new MappingWindow(m_event, m_handler) {Owner = this};
                    mappingWindow.Closed += WindowClosed;
                    mappingWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create maps of elements between elements in a linked model and the host model.\n" + ex.Message, "Element Mapping Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void WindowClosed(object sender, System.EventArgs e)
        {
            mappingWindow = null;
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
            comboBoxLinkModel.IsEnabled = status;
            buttonLink.IsEnabled = status;
            buttonMapping.IsEnabled = status;
            buttonDuplicate.IsEnabled = status;
            buttonClose.IsEnabled = status;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != mappingWindow)
            {
                e.Cancel = true;
            }
        }
    }

   
}
