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
using HOK.SmartBCF.GoogleUtils;
using HOK.SmartBCF.Utils;
using ComponentManager = Autodesk.Windows.ComponentManager;

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for WalkerWindow.xaml
    /// </summary>
    public partial class WalkerWindow : Window
    {
        private WalkerHandler m_handler;
        private ExternalEvent m_event;

        private Dictionary<string/*fileId*/, LinkedBcfFileInfo> bcfFileDictionary = new Dictionary<string, LinkedBcfFileInfo>();
        private Dictionary<string/*fileId*/, Dictionary<string, IssueEntry>> bcfDictionary = new Dictionary<string, Dictionary<string, IssueEntry>>();
        private List<ElementProperties> elementList = new List<ElementProperties>();
        private ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
       
        private List<ColorDefinition> actionDefinitons = new List<ColorDefinition>();
        private List<ColorDefinition> responsibilityDefinitions = new List<ColorDefinition>();
        
        private int currentIndex = 0;
        private ElementProperties selElementProperties = null;

        public WalkerHandler Handler { get { return m_handler; } set { m_handler = value; } }
        public Dictionary<string, LinkedBcfFileInfo> BCFFileDictionary { get { return bcfFileDictionary; } set { bcfFileDictionary = value; } }
        public Dictionary<string, Dictionary<string, IssueEntry>> BCFDictionary { get { return bcfDictionary; } set { bcfDictionary = value; } }
        public ColorSchemeInfo SchemeInfo { get { return schemeInfo; } set { schemeInfo = value; } }
        public int CurrentIndex { get { return currentIndex; } set { currentIndex = value; } }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public WalkerWindow(ExternalEvent exEvent, WalkerHandler handler)
        {
            m_event = exEvent;
            m_handler = handler;
            m_handler.WalkerWindow = this;
            InitializeComponent();

            expanderBCF.Header = "Show BCF Info";
            //expanderRowDefinition.Height = new GridLength(40);
            this.MinHeight = 535;
            this.Height = 535;
            labelStep.Content = "";

            bcfFileDictionary = m_handler.BCFFileDictionary;
            bcfDictionary = m_handler.BCFDictionary;
            schemeInfo = m_handler.SchemeInfo;

            if (bcfDictionary.Count > 0)
            {
                currentIndex = 0;
                DisplayLinkedBCF();
                DisplayColorscheme(schemeInfo);
            }
            else
            {
                //MessageBox.Show("Elements cannot be found in the active Revit document.\n", "Elements Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetFocus()
        {
            IntPtr hBefore = GetForegroundWindow();
            SetForegroundWindow(ComponentManager.ApplicationWindow);
            SetForegroundWindow(hBefore);
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
            buttonBackward.IsEnabled = status;
            buttonForward.IsEnabled = status;
            buttonNewIssue.IsEnabled = status;
            checkBoxHighlight.IsEnabled = status;
            checkBoxIsolate.IsEnabled = status;
            checkBoxSection.IsEnabled = status;
        }

        public void DisplayLinkedBCF()
        {
            try
            {

                InitializeUIComponents();
                List<LinkedBcfFileInfo> bcfFileInfoList = bcfFileDictionary.Values.ToList();
                bcfFileInfoList = bcfFileInfoList.OrderBy(o => o.BCFName).ToList();
                comboBoxBCF.ItemsSource = bcfFileInfoList;
                comboBoxBCF.DisplayMemberPath = "BCFName";
                comboBoxBCF.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display linked BCFs.\n"+ex.Message, "Display Linked BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DisplayElement(int index)
        {
            try
            {
                ElementProperties ep = elementList[index];
                selElementProperties = ep;
                textBoxRevit.Text = ep.ElementName;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the current element.\n"+ex.Message, "Display Element", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DisplayColorscheme(ColorSchemeInfo info)
        {
            try
            {
                foreach (ColorScheme scheme in info.ColorSchemes)
                {
                    if (scheme.SchemeName == "BCF Action")
                    {
                        foreach (ColorDefinition definition in scheme.ColorDefinitions)
                        {
                            actionDefinitons.Add(definition);
                        }
                    }
                    else if (scheme.SchemeName == "BCF Responsibility")
                    {
                        foreach (ColorDefinition definition in scheme.ColorDefinitions)
                        {
                            responsibilityDefinitions.Add(definition);
                        }
                    }
                }

                actionDefinitons = actionDefinitons.OrderBy(o => o.ParameterValue).ToList();
                responsibilityDefinitions = responsibilityDefinitions.OrderBy(o => o.ParameterValue).ToList();

                comboBoxAction.ItemsSource = null;
                comboBoxResponsible.ItemsSource = null;

                comboBoxAction.ItemsSource = actionDefinitons;
                comboBoxResponsible.ItemsSource = responsibilityDefinitions;

                comboBoxAction.SelectedIndex = 0;
                comboBoxResponsible.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display color scheme info.\n"+ex.Message, "Display Color Scheme Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateIndex()
        {
            labelStep.Content = (currentIndex + 1).ToString() + " / " + elementList.Count.ToString();
        }

        #region UI Component events

        private void Window_Closed(object sender, EventArgs e)
        {
            m_event.Dispose();
            m_event = null;
            m_handler = null;
        }

        private void expanderBCF_Collapsed(object sender, RoutedEventArgs e)
        {
            expanderBCF.Header = "Show BCF Info";
            //expanderRowDefinition.Height = new GridLength(40);
            this.MinHeight = 535;
            this.Height = 535;
        }

        private void expanderBCF_Expanded(object sender, RoutedEventArgs e)
        {
            expanderBCF.Header = "Hide BCF Info";
            GridLength expandedHeight = new GridLength(1, GridUnitType.Star);
            expanderRowDefinition.Height = expandedHeight;
            this.MinHeight = 900;
            this.Height = 900;
        }

        private void buttonManage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BCFWindow bcfWindow = new BCFWindow(bcfFileDictionary);
                if (bcfWindow.ShowDialog() == true)
                {
                    bcfFileDictionary = new Dictionary<string, LinkedBcfFileInfo>();
                    bcfFileDictionary = bcfWindow.BCFFileDictionary;
                    bcfWindow.Close();

                    m_handler.BCFFileDictionary = bcfFileDictionary;
                    m_handler.Request.Make(RequestId.UpdateLinkedFileInfo);
                    m_event.Raise();
                    SetFocus();
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open BCF window.\n"+ex.Message, "Open Manage BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void InitializeUIComponents()
        {
            currentIndex = 0;
            textBoxRevit.Text = "";
            labelStep.Content = "";
            comboBoxBCF.ItemsSource = null;
            comboBoxIssue.ItemsSource = null;
            comboBoxAction.ItemsSource = null;
            comboBoxResponsible.ItemsSource = null;
            checkBoxHighlight.IsChecked = false;
            checkBoxIsolate.IsChecked = false;
            checkBoxSection.IsChecked = false;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void comboBoxBCF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                if (null != comboBoxBCF.SelectedItem)
                {
                    LinkedBcfFileInfo fileInfo = (LinkedBcfFileInfo)comboBoxBCF.SelectedItem;
                    if(bcfDictionary.ContainsKey(fileInfo.BCFFileId))
                    {
                        Dictionary<string, IssueEntry> dictionary = bcfDictionary[fileInfo.BCFFileId];
                        List<IssueEntry> issueList = dictionary.Values.OrderBy(o => o.IssueTopic).ToList();
                        comboBoxIssue.ItemsSource = null;
                        comboBoxIssue.ItemsSource = issueList;
                        comboBoxIssue.DisplayMemberPath = "IssueTopic";
                        comboBoxIssue.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to trigger the event of selection changed for BCF Name.\n"+ex.Message, "comboBoxBCF SelectionChanged", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxIssue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxIssue.SelectedItem)
                {
                    IssueEntry selectedIssue = comboBoxIssue.SelectedItem as IssueEntry;
                    elementList = selectedIssue.ElementDictionary.Values.ToList();
                    currentIndex = 0;
                    UpdateIndex();
                    DisplayElement(currentIndex);

                    for (int i = 0; i < comboBoxAction.Items.Count; i++)
                    {
                        ColorDefinition definition = (ColorDefinition)comboBoxAction.Items[i];
                        if (definition.ParameterValue == selectedIssue.Action)
                        {
                            comboBoxAction.SelectedIndex = i;
                        }
                    }

                    for (int i = 0; i < comboBoxResponsible.Items.Count; i++)
                    {
                        ColorDefinition definition = (ColorDefinition)comboBoxResponsible.Items[i];
                        if (definition.ParameterValue == selectedIssue.Responsible)
                        {
                            comboBoxResponsible.SelectedIndex = i;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to trigger the evnet of selection changed for Issue.\n"+ex.Message, "comboBoxIssue SelectionChanged", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentIndex < elementList.Count - 1)
                {
                    currentIndex++;
                    UpdateIndex();
                    DisplayElement(currentIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to iterate elements forward.\n" + ex.Message, "Forward Button Clicked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonBackward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                    UpdateIndex();
                    DisplayElement(currentIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to iterate elements backward.\n" + ex.Message, "Backward Button Clicked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region CheckBox Changed
        private void checkBoxHighlight_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selElementProperties)
                {
                    m_handler.CurrentElement = selElementProperties;
                    m_handler.Request.Make(RequestId.HighlightElement);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to highlight elements.\n"+ex.Message, "Highlight Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxHighlight_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selElementProperties)
                {
                    m_handler.CurrentElement = selElementProperties;
                    m_handler.Request.Make(RequestId.CancelHighlight);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to cancel to highlight elements.\n"+ex.Message, "Cancel Highlight Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxIsolate_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selElementProperties)
                {
                    m_handler.CurrentElement = selElementProperties;
                    m_handler.Request.Make(RequestId.IsolateElement);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to isolate elements.\n"+ex.Message, "Isolate Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxIsolate_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selElementProperties)
                {
                    m_handler.CurrentElement = selElementProperties;
                    m_handler.Request.Make(RequestId.CancelIsolate);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to cancel to isolate elements.\n"+ex.Message, "Cancel Isolate Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxSection_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selElementProperties)
                {
                    m_handler.CurrentElement = selElementProperties;
                    m_handler.Request.Make(RequestId.CreateSectionBox);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create section box.\n"+ex.Message, "Create Section Box", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxSection_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selElementProperties)
                {
                    m_handler.CurrentElement = selElementProperties;
                    m_handler.Request.Make(RequestId.CancelSectionBox);
                    m_event.Raise();
                    SetFocus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to cancel to create section box.\n"+ex.Message, "Cancel Create Section Box", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

    }

    
}
