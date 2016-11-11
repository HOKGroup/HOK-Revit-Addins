using System;
using System.Windows;
using System.Windows.Controls;
using ARUP.IssueTracker.Classes;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Collections.Generic;
using ARUP.IssueTracker.Classes.BCF2;

namespace ARUP.IssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class JiraPanel : UserControl
    {
        public event RoutedEventHandler open3dViewEvent;

        /// <summary>
        /// For MainPanel to send auto-complete items
        /// </summary>
        private MainPanel mainPanel = null;

        public JiraPanel()
        {            
            InitializeComponent();
            
            // default invisible
            //open3dView.Visibility = System.Windows.Visibility.Collapsed;
            //showComponents.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// For setting auto-complete items
        /// </summary>
        /// <param name="mainPanel">An instance of the MainPanel</param>
        public void SetMainPanel(MainPanel mainPanel)
        {
            this.mainPanel = mainPanel;
        }

        public int projIndex
        {
            get { return projCombo.SelectedIndex; }
            set { projCombo.SelectedIndex = value; }
        }
       

        public int listIndex
        {
            get { return issueList.SelectedIndex; }
            set { issueList.SelectedIndex = value; }
        }

        /// <summary>
        /// For indicating filter active/inactive
        /// </summary>
        public bool IsFilterActive
        {
            get
            {
                string filters = string.Empty;
                if (customFilters.IsEnabled)
                {
                    filters = this.Filters + this.Assignation + this.Creator;
                }

                return filters == string.Empty ? false : true;
            }

        }

        /// <summary>
        /// Filter for all project-specific attributes
        /// </summary>
        public string Filters
        {
            get
            {
                string filters = string.Empty;
                if (customFilters.IsEnabled)
                {                   
                    string labelSearchFilter = string.IsNullOrWhiteSpace(labelSearchComboBox.Text) ? string.Empty : string.Format("+AND+labels={0}", labelSearchComboBox.Text);
                    string textSearchFilter = string.IsNullOrWhiteSpace(textSearchTextBox.Text) ? string.Empty : string.Format("+AND+text+~+\"{0}\"", textSearchTextBox.Text);
                    filters = statusfilter.Result + typefilter.Result + priorityfilter.Result + componentfilter.Result + labelSearchFilter + textSearchFilter;
                }
                return filters;
            }
        }

        /// <summary>
        /// Filter for assignation
        /// </summary>
        public string Assignation
        {
            get
            {
                string assignation = "";
                if (unassignedRadioButton.IsChecked.Value)
                {
                    assignation = "+AND+assignee=EMPTY";
                }
                else if (assignedRadioButton.IsChecked.Value)
                {
                    assignation = "+AND+assignee!=EMPTY";
                }
                else if (assignedToMeRadioButton.IsChecked.Value)
                {
                    assignation = "+AND+assignee=currentUser()";
                }
                else if (allAssignationRadioButton.IsChecked.Value)
                {
                    assignation = "";
                }
                return assignation;
            }
        }

        /// <summary>
        /// Filter for creator
        /// </summary>
        public string Creator
        {
            get
            {
                string creator = "";
                if (meCreatorRadioButton.IsChecked.Value)
                {
                    creator = "+AND+creator=currentUser()";
                }
                else if (othersCreatorRadioButton.IsChecked.Value)
                {
                    creator = "+AND+creator!=currentUser()";
                }
                else if (allCreatorRadioButton.IsChecked.Value)
                {
                    creator = "";
                }
                
                return creator;
            }
        }

        public string Order
        {
            get
            {
                string ordertype = "DESC";
                string orderdate = "Updated";
                foreach (RadioButton rb in grouptype.Children)
                {
                    if (rb.IsChecked.Value)
                        ordertype = rb.Content.ToString();

                }
                foreach (RadioButton rb in groupdate.Children)
                {
                    if (rb.IsChecked.Value)
                    {
                        orderdate = rb.Content.ToString();
                    }

                }

                return "+ORDER+BY+" + orderdate + "+" + ordertype;
            }

        }
        public void clearFilters_Click(object sender, RoutedEventArgs e)
        {
            statusfilter.Clear();
            typefilter.Clear();
            priorityfilter.Clear();
            componentfilter.Clear();
            allCreatorRadioButton.IsChecked = true;
            allAssignationRadioButton.IsChecked = true;
            labelSearchComboBox.Text = string.Empty;
            textSearchTextBox.Text = string.Empty;

                //IM.getIssues();
        }
        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            //   IM.ChangeStatus_Click();
        }
        private void ChangePriority_Click(object sender, RoutedEventArgs e)
        {
            //  IM.ChangePriority_Click();
        }
       
        
        private void ChangeType_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeAssign_Click(object sender, RoutedEventArgs e)
        {

        }

        private void labelSearchComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string text = string.Empty;
            if (ComboBoxInputCheck(sender, e, out text))
            {
                // Make sure mainPanel is not null
                if (this.mainPanel != null)
                {
                    this.mainPanel.getLabels(text);
                }
            }             
        }

        /// <summary>
        /// For setting auto-complete items
        /// </summary>
        /// <param name="labels">Suggested names of labels</param>
        public void SetAutoCompleteItems(List<string> labels)
        {
            if (labels.Count > 0 && labelSearchComboBox.Items.Count == 0)
            {
                labels.ForEach(label => labelSearchComboBox.Items.Add(label));
                labelSearchComboBox.IsDropDownOpen = true;
            }            
        }

        private TextBox GetTextBoxInComboBox(ComboBox cbox)
        {
            TextBox txt = cbox.Template.FindName("PART_EditableTextBox", cbox) as TextBox;
            return txt;
        }        

        private bool ComboBoxInputCheck(object sender, System.Windows.Input.KeyEventArgs e, out string text)
        {
            ComboBox cbox = sender as ComboBox;
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                text = string.Empty;
                return false;
            }
            else if (e.Key == Key.Left || e.Key == Key.Right)
            {
                cbox.IsDropDownOpen = false;
                text = string.Empty;
                return false;
            }

            cbox.Items.Clear();
            TextBox txt = GetTextBoxInComboBox(cbox);
            if (string.IsNullOrWhiteSpace(txt.Text))
            {
                text = string.Empty;
                return false;
            }

            text = txt.Text;
            return true;
        }

        private void OpenImageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mainPanel != null) 
            {
                mainPanel.OpenImage(sender, e);
            }
        }

        private void showComponents_Click(object sender, RoutedEventArgs e)
        {
            if (mainPanel != null)
            {
                mainPanel.ComponentsShowJira(sender, e);
            }
        }

        private void open3dView_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                if (mainPanel != null && open3dViewEvent != null)
                {
                    open3dViewEvent(sender, e);
                }
                else 
                {
                    MessageBox.Show("3D views can only be opened in Revit or Navisworks", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }            
        }
    }
       
}
