using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using ARUP.IssueTracker.Classes;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for UploadBCF.xaml
    /// </summary>
    public partial class UploadBCF : Window
    {
        public ObservableCollection<Project> ProjectsCollection = new ObservableCollection<Project>();
        public int itemCount = 0;
        public int projIndex = 0;

        //private ObservableCollection<Issuetype> typesCollection = new ObservableCollection<Issuetype>();
        public ObservableCollection<Component> compCollection = new ObservableCollection<Component>();
        //private ObservableCollection<Priority> PrioritiesCollection = new ObservableCollection<Priority>();
        public List<User> assignees = new List<User>();
        public List<Component> SelectedComponents = new List<Component>(); 

        public UploadBCF()
        {
            InitializeComponent();
        }
        public void setValues(){
            string s = (itemCount > 1) ? "s" : "";
            description.Content = "You are about to send "+itemCount.ToString()+" Issue"+s+" to Jira.";
            //projCombo.ItemsSource = ProjectsCollection;
            //projCombo.SelectedIndex = projIndex;
        }
        private void OKBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void projCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (issueTypeCombo.Items.Count != 0)
                issueTypeCombo.SelectedIndex = 0;
        }

        //private void ChangeAssign_OnClick(object sender, RoutedEventArgs e)
        //{
        //    // = getAssigneesIssue();
        //    if (!assignees.Any())
        //    {
        //        MessageBox.Show("You don't have permission to Assign people to this Issue");
        //        return;
        //        //jira.issuesCollection[jiraPan.issueList.SelectedIndex].transitions = response2.Data.transitions;
        //    }
        //    ChangeAssignee cv = new ChangeAssignee(); cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //    cv.SetList(assignees);

        //    cv.Title = "Assign to";
        //    cv.ShowDialog();
        //    if (cv.DialogResult.HasValue && cv.DialogResult.Value)
        //    {
        //        User assign = (cv.valuesList.SelectedIndex >= cv.valuesList.Items.Count || cv.valuesList.SelectedIndex == -1) ? null : (User)cv.valuesList.SelectedItem;
        //        ChangeAssign.Content = (assign != null) ? assign.name : "none";

        //    }
        //}

        //private void ChangeComponents_OnClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {

        //        ChangeValue cv = new ChangeValue();
        //        cv.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //        List<Component> components = compCollection.ToList<Component>();
        //        cv.valuesList.ItemsSource = components;
        //        cv.valuesList.SelectionMode = SelectionMode.Multiple;

        //        cv.Title = "Change Components";
        //        DataTemplate componentTemplate = cv.FindResource("componentTemplate") as DataTemplate;
        //        cv.valuesList.ItemTemplate = componentTemplate;
        //        // ChangeStatus ChangSt = new ChangeStatus(jira.issuesCollection[jiraPan.issueList.SelectedIndex].transitions);
        //        cv.ShowDialog();
        //        if (cv.DialogResult.HasValue && cv.DialogResult.Value)
        //        {
        //            SelectedComponents = new List<Component>();

        //            foreach (var c in cv.valuesList.SelectedItems)
        //            {
        //                Component cc = c as Component;
        //                SelectedComponents.Add(cc);

        //            }

        //            string componentsout = "none";

        //            if (SelectedComponents != null && SelectedComponents.Any())
        //            {
        //                componentsout = "";
        //                foreach (var c in SelectedComponents)
        //                    componentsout += c.name + ", ";
        //                componentsout = componentsout.Remove(componentsout.Count() - 2);
        //            }
        //            ChangeComponents.Content = componentsout;





        //        }
        //    }
        //    catch (System.Exception ex1)
        //    {
        //        MessageBox.Show("exception: " + ex1);
        //    }
        //}
    }
}
