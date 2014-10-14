using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        private List<CategoryInfo> categoryInfoList = new List<CategoryInfo>();
        private ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
        private FolderHolders googleFolders = null;
       
        private List<ColorDefinition> actionDefinitons = new List<ColorDefinition>();
        private List<ColorDefinition> responsibilityDefinitions = new List<ColorDefinition>();
        
        private int currentIndex = 0;
        private LinkedBcfFileInfo selectedBCF = null;
        private IssueEntry selectedIssue = null;
        private ElementProperties selElementProperties = null;
        private string bcfProjectId = "";
        private string bcfColorSchemeId = "";
        private bool isHighlightOn = false;
        private bool isIsolateOn = false;
        private bool isSectionBoxOn = false;
        private bool isFilterOn = false;
        private bool freezeHandler = false;

        private BitmapImage filterOnImage = null;
        private BitmapImage filterOffImage = null;

        public WalkerHandler Handler { get { return m_handler; } set { m_handler = value; } }
        public Dictionary<string, LinkedBcfFileInfo> BCFFileDictionary { get { return bcfFileDictionary; } set { bcfFileDictionary = value; } }
        public Dictionary<string, Dictionary<string, IssueEntry>> BCFDictionary { get { return bcfDictionary; } set { bcfDictionary = value; } }
        public List<CategoryInfo> CategoryInfoList { get { return categoryInfoList; } set { categoryInfoList = value; } }
        public ColorSchemeInfo SchemeInfo { get { return schemeInfo; } set { schemeInfo = value; } }
        public FolderHolders GoogleFolders { get { return googleFolders; } set { googleFolders = value; } }
        public int CurrentIndex { get { return currentIndex; } set { currentIndex = value; } }
        public string BCFProjectId { get { return bcfProjectId; } set { bcfProjectId = value; } }
        public string BCFColorSchemeId { get { return bcfColorSchemeId; } set { bcfColorSchemeId = value; } }
        

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private delegate void UpdateLableDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateProgressDelegate(System.Windows.DependencyProperty dp, Object value);

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

            filterOnImage = LoadBitmapImage("filter.png");
            filterOffImage = LoadBitmapImage("filter_empty.png");

            buttonFilterImage.Source = filterOffImage;

            bcfFileDictionary = m_handler.BCFFileDictionary;
            bcfDictionary = m_handler.BCFDictionary;
            categoryInfoList = m_handler.CategoryInfoList;
            schemeInfo = m_handler.SchemeInfo;
            bcfProjectId = m_handler.BCFProjectId;
            bcfColorSchemeId = m_handler.BCFColorSchemeId;
            googleFolders = m_handler.GoogleFolders;

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

        private BitmapImage LoadBitmapImage(string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string prefix = typeof(AppCommand).Namespace + ".Resources.";
                Stream stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load check box button image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
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
                comboBoxBCF.ItemsSource = null;
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
                if (elementList.Count > 0)
                {
                    ElementProperties ep = elementList[index];
                    selElementProperties = ep;
                    m_handler.CurrentElement = selElementProperties;

                    if (!string.IsNullOrEmpty(ep.ElementName))
                    {
                        textBoxRevit.Text = ep.ElementName;
                    }
                    else
                    {
                        textBoxRevit.Text = "Element Not Found - " + ep.ElementId.ToString();
                    }

                    freezeHandler = true;
                    if (comboBoxAction.HasItems)
                    {
                        for (int i = 0; i < comboBoxAction.Items.Count; i++)
                        {
                            ColorDefinition definition = (ColorDefinition)comboBoxAction.Items[i];
                            if (definition.ParameterValue == selElementProperties.Action)
                            {
                                comboBoxAction.SelectedIndex = i; break;
                            }
                        }
                    }

                    if (comboBoxResponsible.HasItems)
                    {
                        for (int i = 0; i < comboBoxResponsible.Items.Count; i++)
                        {
                            ColorDefinition definition = (ColorDefinition)comboBoxResponsible.Items[i];
                            if (definition.ParameterValue == selElementProperties.ResponsibleParty)
                            {
                                comboBoxResponsible.SelectedIndex = i; break;
                            }
                        }
                    }
                    freezeHandler = false;
                }
                else if (elementList.Count == 0)
                {
                    textBoxRevit.Text = "";
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the current element.\n"+ex.Message, "Display Element", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateElementParameters()
        {
            if (null != selElementProperties)
            {
                m_handler.CurrentElement = selElementProperties;
                m_handler.SelectedIssue = (IssueEntry)comboBoxIssue.SelectedItem;
                if (null != dataGridComments.SelectedItem)
                {
                    m_handler.SelectedComment = (Comment)dataGridComments.SelectedItem;
                }
                m_handler.Request.Make(RequestId.UpdateBCFParameterInfo);
                m_event.Raise();
                SetFocus();
            }
        }

        private void UpdateViews()
        {
            if (null != selElementProperties)
            {
                m_handler.CurrentElement = selElementProperties;
                m_handler.IsHighlightOn = isHighlightOn;
                m_handler.IsIsolateOn = isIsolateOn;
                m_handler.IsSectionBoxOn = isSectionBoxOn;
                m_handler.Request.Make(RequestId.UpdateViews);
                m_event.Raise();
                SetFocus();
            }
        }

        public void DisplayColorscheme(ColorSchemeInfo info)
        {
            try
            {
                if (info.ColorSchemes.Count > 0)
                {
                    comboBoxAction.ItemsSource = null;
                    comboBoxResponsible.ItemsSource = null;

                    actionDefinitons = new List<ColorDefinition>();
                    responsibilityDefinitions = new List<ColorDefinition>();

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

                    comboBoxAction.ItemsSource = actionDefinitons;
                    comboBoxResponsible.ItemsSource = responsibilityDefinitions;

                    freezeHandler = true;
                    if (null != selElementProperties)
                    {
                        for (int i = 0; i < comboBoxAction.Items.Count; i++)
                        {
                            ColorDefinition definition = (ColorDefinition)comboBoxAction.Items[i];
                            if (definition.ParameterValue == selElementProperties.Action)
                            {
                                comboBoxAction.SelectedIndex = i; break;
                            }
                        }
                        for (int i = 0; i < comboBoxResponsible.Items.Count; i++)
                        {
                            ColorDefinition definition = (ColorDefinition)comboBoxResponsible.Items[i];
                            if (definition.ParameterValue == selElementProperties.ResponsibleParty)
                            {
                                comboBoxResponsible.SelectedIndex = i; break;
                            }
                        }
                    }

                    freezeHandler = false;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display color scheme info.\n"+ex.Message, "Display Color Scheme Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateIndex()
        {
            if (elementList.Count > 0)
            {
                labelStep.Content = (currentIndex + 1).ToString() + " / " + elementList.Count.ToString();
            }
            else
            {
                labelStep.Content = "0 / 0";
            }
        }

        private List<ElementProperties> ApplyCategoryFilter(List<ElementProperties> elements)
        {
            List<ElementProperties> filteredList = new List<ElementProperties>();
            try
            {
                var catNames = from category in categoryInfoList where category.IsSelected select category.CategoryName;
                foreach (ElementProperties ep in elements)
                {
                    if (catNames.Contains(ep.CategoryName))
                    {
                        filteredList.Add(ep);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply category filters.\n"+ex.Message, "Apply Category Filters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return filteredList;
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

        private void comboBoxBCF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                if (null != comboBoxBCF.SelectedItem)
                {
                    LinkedBcfFileInfo fileInfo = (LinkedBcfFileInfo)comboBoxBCF.SelectedItem;
                    selectedBCF = fileInfo;
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
                    IssueEntry issueEntry = comboBoxIssue.SelectedItem as IssueEntry;
                    string issueId = issueEntry.IssueId;
                    if (null != selectedBCF)
                    {
                        selectedIssue = bcfDictionary[selectedBCF.BCFFileId][issueId];
                        imageIssue.Source = selectedIssue.Snapshot;
                        labelIssueTopic.Content = selectedIssue.IssueTopic;

                        List<Comment> comments = selectedIssue.CommentDictionary.Values.ToList();
                        comments = comments.OrderBy(o => o.Comment1).ToList();
                        
                        dataGridComments.ItemsSource = null;
                        dataGridComments.ItemsSource = comments;
                        if (comments.Count > 0)
                        {
                            dataGridComments.SelectedIndex = 0;
                        }
                    }

                    elementList = selectedIssue.ElementDictionary.Values.ToList();
                    if (isFilterOn)
                    {
                        elementList = ApplyCategoryFilter(elementList);
                    }
                   
                    currentIndex = 0;
                    UpdateElementParameters();
                    selElementProperties = null;
                    UpdateIndex();
                    DisplayElement(currentIndex);
                    UpdateViews();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to trigger the evnet of selection changed for Issue.\n"+ex.Message, "comboBoxIssue SelectionChanged", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxAction.SelectedItem && freezeHandler==false)
                {
                    ColorDefinition selectedAction = (ColorDefinition)comboBoxAction.SelectedItem;

                    if (null!=selectedBCF && null!=selectedIssue && null!=selElementProperties)
                    {
                        selElementProperties.Action = selectedAction.ParameterValue;

                        if (bcfDictionary.ContainsKey(selectedBCF.BCFFileId))
                        {
                            if (bcfDictionary[selectedBCF.BCFFileId].ContainsKey(selectedIssue.IssueId))
                            {
                                if (bcfDictionary[selectedBCF.BCFFileId][selectedIssue.IssueId].ElementDictionary.ContainsKey(selElementProperties.ElementId))
                                {
                                    bcfDictionary[selectedBCF.BCFFileId][selectedIssue.IssueId].ElementDictionary.Remove(selElementProperties.ElementId);
                                    bcfDictionary[selectedBCF.BCFFileId][selectedIssue.IssueId].ElementDictionary.Add(selElementProperties.ElementId, selElementProperties);
                                }
                            }
                        }

                        bool updatedSheet = BCFParser.UpdateElementProperties(selElementProperties, BCFParameters.BCF_Action, selectedBCF.BCFFileId);

                        m_handler.CurrentElement = selElementProperties;
                        m_handler.Request.Make(RequestId.UpdateAction);
                        m_event.Raise();
                        SetFocus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set action item.\n" + ex.Message, "Set Action Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxResponsible_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxResponsible.SelectedItem && freezeHandler==false)
                {
                    ColorDefinition selectedResponsible = (ColorDefinition)comboBoxResponsible.SelectedItem;

                    if (null != selectedBCF && null != selectedIssue && null != selElementProperties)
                    {
                        selElementProperties.ResponsibleParty = selectedResponsible.ParameterValue;

                        if (bcfDictionary.ContainsKey(selectedBCF.BCFFileId))
                        {
                            if (bcfDictionary[selectedBCF.BCFFileId].ContainsKey(selectedIssue.IssueId))
                            {
                                if (bcfDictionary[selectedBCF.BCFFileId][selectedIssue.IssueId].ElementDictionary.ContainsKey(selElementProperties.ElementId))
                                {
                                    bcfDictionary[selectedBCF.BCFFileId][selectedIssue.IssueId].ElementDictionary.Remove(selElementProperties.ElementId);
                                    bcfDictionary[selectedBCF.BCFFileId][selectedIssue.IssueId].ElementDictionary.Add(selElementProperties.ElementId, selElementProperties);
                                }
                            }
                        }

                        bool updatedSheet = BCFParser.UpdateElementProperties(selElementProperties, BCFParameters.BCF_Responsibility, selectedBCF.BCFFileId);

                        m_handler.CurrentElement = selElementProperties;
                        m_handler.Request.Make(RequestId.UpdateResponsibility);
                        m_event.Raise();
                        SetFocus();
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set responsibility itme.\n" + ex.Message, "Set Responsibility Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonManage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateLableDelegate updateLabelDelegate = new UpdateLableDelegate(statusLable.SetValue);
                UpdateProgressDelegate updateProgressDelegate = new UpdateProgressDelegate(progressBar.SetValue);

                BCFWindow bcfWindow = new BCFWindow(bcfFileDictionary, googleFolders);
                if (bcfWindow.ShowDialog() == true)
                {
                    Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Loading BCF Information..." });
                    
                    if (bcfWindow.BCFFileDictionary.Count > 0)
                    {
                        bcfFileDictionary = new Dictionary<string, LinkedBcfFileInfo>();
                        bcfFileDictionary = bcfWindow.BCFFileDictionary;
                        bcfProjectId = bcfWindow.BCFProjectId;
                        googleFolders = bcfWindow.GoogleFolders;
                        bcfColorSchemeId = googleFolders.ColorSheet.Id;
                        bcfWindow.Close();

                        m_handler.BCFFileDictionary = bcfFileDictionary;
                        m_handler.BCFProjectId = bcfProjectId;
                        m_handler.BCFColorSchemeId = bcfColorSchemeId;
                        m_handler.GoogleFolders = googleFolders;
                        m_handler.Request.Make(RequestId.UpdateLinkedFileInfo);
                        m_event.Raise();
                        SetFocus();
                    }
                    
                    
                    Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Ready" });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open BCF window.\n" + ex.Message, "Open Manage BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentIndex < elementList.Count - 1)
                {
                    currentIndex++;
                    UpdateElementParameters();
                    UpdateIndex();
                    DisplayElement(currentIndex);
                    UpdateViews();
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
                    UpdateElementParameters();
                    UpdateIndex();
                    DisplayElement(currentIndex);
                    UpdateViews();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to iterate elements backward.\n" + ex.Message, "Backward Button Clicked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusWindow statusWindow = new StatusWindow(schemeInfo, bcfColorSchemeId);
                if (statusWindow.ShowDialog() == true)
                {
                    schemeInfo = statusWindow.SchemeInfo;
                    actionDefinitons = statusWindow.ActionDefinitions;
                    responsibilityDefinitions = statusWindow.ResponsibleDefinitions;
                    statusWindow.Close();

                    comboBoxAction.ItemsSource = null;
                    comboBoxResponsible.ItemsSource = null;

                    comboBoxAction.ItemsSource = actionDefinitons;
                    comboBoxResponsible.ItemsSource = responsibilityDefinitions;

                    freezeHandler = true;
                    if (null != selElementProperties)
                    {
                        for (int i = 0; i < comboBoxAction.Items.Count; i++)
                        {
                            ColorDefinition definition = (ColorDefinition)comboBoxAction.Items[i];
                            if (definition.ParameterValue == selElementProperties.Action)
                            {
                                comboBoxAction.SelectedIndex = i;
                            }
                        }

                        for (int i = 0; i < comboBoxResponsible.Items.Count; i++)
                        {
                            ColorDefinition definition = (ColorDefinition)comboBoxResponsible.Items[i];
                            if (definition.ParameterValue == selElementProperties.ResponsibleParty)
                            {
                                comboBoxResponsible.SelectedIndex = i;
                            }
                        }
                    }
                    freezeHandler = false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set color schemes.\n"+ex.Message, "Color Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region CheckBox Changed
       
        private void checkBoxHighlight_Checked(object sender, RoutedEventArgs e)
        {
            isHighlightOn = true;
            UpdateViews();
        }

        private void checkBoxHighlight_Unchecked(object sender, RoutedEventArgs e)
        {
            isHighlightOn = false;
            UpdateViews();
        }

        private void checkBoxIsolate_Checked(object sender, RoutedEventArgs e)
        {
            isIsolateOn = true;
            UpdateViews();
        }

        private void checkBoxIsolate_Unchecked(object sender, RoutedEventArgs e)
        {
            isIsolateOn = false;
            UpdateViews();
        }

        private void checkBoxSection_Checked(object sender, RoutedEventArgs e)
        {
            isSectionBoxOn = true;
            UpdateViews();
        }

        private void checkBoxSection_Unchecked(object sender, RoutedEventArgs e)
        {
            isSectionBoxOn = false;
            UpdateViews();
        }

        #endregion

        private void buttonSelected_Click(object sender, RoutedEventArgs e)
        {
            if (null != dataGridComments.SelectedItem)
            {
                Comment selectedComment = (Comment)dataGridComments.SelectedItem;

                m_handler.SelectedComment = selectedComment;
                m_handler.SelectedIssue = selectedIssue;
                m_handler.Request.Make(RequestId.UpdateParameterByComment);
                m_event.Raise();
                SetFocus();
            }
        }

        private void buttonFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FilterWindow filterWindow = new FilterWindow(categoryInfoList);
                if (filterWindow.ShowDialog() == true)
                {
                    categoryInfoList = filterWindow.CategoryInfoList;
                    var categoryUnselected = from category in categoryInfoList where category.IsSelected == false select category;
                    if (null != selectedIssue)
                    {
                        elementList = selectedIssue.ElementDictionary.Values.ToList();
                        if (categoryUnselected.Count() > 0)
                        {
                            elementList = ApplyCategoryFilter(elementList);
                            isFilterOn = true;
                            buttonFilterImage.Source = filterOnImage;
                        }
                        else if (categoryUnselected.Count() == 0)
                        {
                            isFilterOn = false;
                            buttonFilterImage.Source = filterOffImage;
                        }

                        currentIndex = 0;
                        UpdateElementParameters();
                        selElementProperties = null;
                        UpdateIndex();
                        DisplayElement(currentIndex);
                        UpdateViews();
                    }
                }
                else
                {
                    categoryInfoList = filterWindow.CategoryInfoList;
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to enable filter.\n"+ex.Message, "Filter On/Off", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }

    
}
