using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using ARUP.IssueTracker.Classes;
using ARUP.IssueTracker.Windows;
using Autodesk.Navisworks.Api;

namespace ARUP.IssueTracker.Navisworks
{
    /// <summary>
    /// Interaction logic for AddIssueNavis.xaml
    /// </summary>
    public partial class AddIssueNavis : Window
    {
        private Dictionary<SavedViewpoint, string> savedViewpoints = new Dictionary<SavedViewpoint, string>();
        private ObservableCollection<Issuetype> typesCollection = new ObservableCollection<Issuetype>();
        private ObservableCollection<Component> compCollection = new ObservableCollection<Component>();
        private ObservableCollection<Priority> PrioritiesCollection = new ObservableCollection<Priority>();
        private List<User> assignees = new List<User>();
        public List<Component> SelectedComponents = new List<Component>();

        public AddIssueNavis(Dictionary<SavedViewpoint, string> _savedViewpoints, ObservableCollection<Issuetype> _typesCollection,
            List<User> _assignees, ObservableCollection<Component> _compCollection, ObservableCollection<Priority> _PrioritiesCollection, bool comp, bool prior, bool assign)
        {
            InitializeComponent();
            savedViewpoints = _savedViewpoints;
            issueList.ItemsSource = savedViewpoints;

            if (null != _typesCollection)
            {
                typesCollection = _typesCollection;
                issueTypeCombo.ItemsSource = typesCollection;
                issueTypeCombo.SelectedIndex = 0;
            }
            if (!comp)
            {
                compCollection = _compCollection;
            }
            else
                ChangeComponentsLabel.Visibility = ChangeComponents.Visibility = System.Windows.Visibility.Collapsed;

            if (!assign && null != _assignees)
            {
                assignees = _assignees;
            }
            else
                assigneeStack.Visibility = System.Windows.Visibility.Collapsed;
            if (!prior && null != _PrioritiesCollection)
            {
                PrioritiesCollection = _PrioritiesCollection;
                priorityCombo.ItemsSource = PrioritiesCollection;
                priorityCombo.SelectedIndex = 0;
            }
            else
                PriorityStack.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_OK(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(TitleBox.Text))
            //{
            //    MessageBox.Show("Please insert an Issue Title.", "Title required", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}
            DialogResult = true;
        }

        private void ChangeAssign_OnClick(object sender, RoutedEventArgs e)
        {
            // = getAssigneesIssue();
            if (!assignees.Any())
            {
                MessageBox.Show("You don't have permission to Assign people to this Issue");
                return;
                //jira.issuesCollection[jiraPan.issueList.SelectedIndex].transitions = response2.Data.transitions;
            }
            ChangeAssignee cv = new ChangeAssignee(); cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            cv.SetList(assignees);
            cv.valuesList.SelectedIndex = (ChangeAssign.Content.ToString() != "none") ? IndexByName.Get(ChangeAssign.Content.ToString(), "name", assignees) : -1;
            cv.Title = "Assign to";
            cv.ShowDialog();
            if (cv.DialogResult.HasValue && cv.DialogResult.Value)
            {
                User assign = (cv.valuesList.SelectedIndex >= cv.valuesList.Items.Count || cv.valuesList.SelectedIndex == -1) ? null : (User)cv.valuesList.SelectedItem;
                ChangeAssign.Content = (assign != null) ? assign.name : "none";

            }
        }

        private void ChangeComponents_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

                ChangeValue cv = new ChangeValue();
                cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                List<Component> components = compCollection.ToList<Component>();
                cv.valuesList.ItemsSource = components;
                cv.valuesList.SelectionMode = SelectionMode.Multiple;

                cv.Title = "Change Components";
                DataTemplate componentTemplate = cv.FindResource("componentTemplate") as DataTemplate;
                cv.valuesList.ItemTemplate = componentTemplate;
                cv.valuesList.SelectedIndex = -1;
                if (SelectedComponents != null && SelectedComponents.Any())
                {

                    foreach (var o in SelectedComponents)
                    {
                        var selindex = components.IndexOf(o);
                        if (selindex != -1)
                            cv.valuesList.SelectedItems.Add(cv.valuesList.Items[selindex]);
                    }
                }
                cv.ShowDialog();
                if (cv.DialogResult.HasValue && cv.DialogResult.Value)
                {
                    SelectedComponents = new List<Component>();

                    foreach (var c in cv.valuesList.SelectedItems)
                    {
                        Component cc = c as Component;
                        SelectedComponents.Add(cc);

                    }

                    string componentsout = "none";

                    if (SelectedComponents != null && SelectedComponents.Any())
                    {
                        componentsout = "";
                        foreach (var c in SelectedComponents)
                            componentsout += c.name + ", ";
                        componentsout = componentsout.Remove(componentsout.Count() - 2);
                    }
                    ChangeComponents.Content = componentsout;





                }
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
    }
}
