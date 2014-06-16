using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HOK.ModelManager.ReplicateViews;
using System.Windows.Resources;
using HOK.ModelManager.GoogleDocs;
using System.Diagnostics;

namespace HOK.ModelManager
{
    public enum ModelManagerMode
    {
        ProjectReplication=0,
        ModelBuilder=1
    }

    public enum TreeViewSortBy
    {
        Sheet=0,
        ViewType
    }
    /// <summary>
    /// Interaction logic for ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private UIApplication m_app;
        private ModelManagerMode m_mode;
        private Document m_doc;
        private ProjectViewManager projectView = null;
        private ModelViewManager modelView = null;
        private bool verifiedUser = false;

        public bool VerifiedUser { get { return verifiedUser; } set { verifiedUser = value; } }
        public ManagerWindow(UIApplication uiapp, ModelManagerMode mode)
        {
            try
            {
                m_app = uiapp;
                m_doc = m_app.ActiveUIDocument.Document;
                m_mode = mode;

                InitializeComponent();
               
                ControlUI(mode);

                comboBoxViewBy.Items.Add("Show Sheets");
                comboBoxViewBy.Items.Add("Show View Types");
                comboBoxViewBy.SelectedIndex = 0;

                if (m_mode == ModelManagerMode.ProjectReplication)
                {
                    projectView = new ProjectViewManager(m_app); //get drafting views info
                    verifiedUser = projectView.VerifiedUser;
                    this.Title = "HOK Model Manager - Project Replication v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                else if (m_mode == ModelManagerMode.ModelBuilder)
                {
                    modelView = new ModelViewManager(m_app);
                    this.Title = "HOK Model Manager - Model Builder v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }

                if (verifiedUser)
                {
                    DisplayDocuments();
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize Model Manager.\n"+ex.Message, "Model Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ControlUI(ModelManagerMode mode)
        {
            if (mode == ModelManagerMode.ProjectReplication)
            {
                buttonOpen.Visibility = System.Windows.Visibility.Hidden;
                buttonRefresh.Visibility = System.Windows.Visibility.Hidden;
                buttonManage.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void DisplayDocuments()
        {
            try
            {
                if (m_mode == ModelManagerMode.ProjectReplication)
                {
                    List<string> sources = projectView.ModelInfoDictionary.Keys.ToList();
                    comboBoxSource.ItemsSource = sources;
                    comboBoxSource.SelectedIndex = 0;

                    comboBoxRecipient.ItemsSource = sources;
                    if (!string.IsNullOrEmpty(m_doc.Title))
                    {
                        int rSelected = sources.IndexOf(m_doc.Title);
                        if (rSelected == 0)
                        {
                            comboBoxSource.SelectedIndex = 1;
                            comboBoxRecipient.SelectedIndex = 0;
                        }
                        else
                        {
                            comboBoxRecipient.SelectedIndex = rSelected;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the lists of documents.\n"+ex.Message, "Display Documents", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboBoxSource.SelectedIndex > -1 && comboBoxRecipient.SelectedIndex > -1)
                {
                    if (m_mode == ModelManagerMode.ProjectReplication)
                    {
                        if (comboBoxSource.SelectedIndex == comboBoxRecipient.SelectedIndex)
                        {
                            labelWarning.Visibility = System.Windows.Visibility.Visible;
                            treeViewSource.ItemsSource = null;
                            treeViewRecipient.ItemsSource = null;
                        }
                        else
                        {
                            labelWarning.Visibility = System.Windows.Visibility.Hidden;
                            TreeViewSortBy sortBy = (TreeViewSortBy)comboBoxViewBy.SelectedIndex;
                            projectView.RefreshTreeView(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString(), treeViewSource, treeViewRecipient, sortBy);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh the treeview from the source model.\n"+ex.Message, "Tree View for Source Model", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxRecipient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboBoxSource.SelectedIndex > -1 && comboBoxRecipient.SelectedIndex > -1)
                {
                    if (m_mode == ModelManagerMode.ProjectReplication)
                    {
                        if (comboBoxSource.SelectedIndex == comboBoxRecipient.SelectedIndex)
                        {
                            labelWarning.Visibility = System.Windows.Visibility.Visible;
                            treeViewSource.ItemsSource = null;
                            treeViewRecipient.ItemsSource = null;
                        }
                        else
                        {
                            labelWarning.Visibility = System.Windows.Visibility.Hidden;
                            TreeViewSortBy sortBy = (TreeViewSortBy)comboBoxViewBy.SelectedIndex;
                            projectView.RefreshTreeView(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString(), treeViewSource, treeViewRecipient, sortBy);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh the treeview from the recipient model.\n"+ex.Message, "Tree View for Recipient Model", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxViewBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboBoxSource.SelectedIndex > -1 && comboBoxRecipient.SelectedIndex > -1)
                {
                    if (m_mode == ModelManagerMode.ProjectReplication)
                    {
                        if (comboBoxSource.SelectedIndex == comboBoxRecipient.SelectedIndex)
                        {
                            labelWarning.Visibility = System.Windows.Visibility.Visible;
                            treeViewSource.ItemsSource = null;
                            treeViewRecipient.ItemsSource = null;
                        }
                        else
                        {
                            checkBoxLinked.IsChecked = false;
                            TreeViewSortBy sortBy = (TreeViewSortBy)comboBoxViewBy.SelectedIndex;
                            projectView.RefreshTreeView(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString(), treeViewSource, treeViewRecipient, sortBy);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh the treeview based upon the selection of View By.\n"+ex.Message, "View By", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void treeViewSource_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                TreeViewSortBy sortBy = (TreeViewSortBy)comboBoxViewBy.SelectedIndex;
                TreeViewModel selectedItem = e.NewValue as TreeViewModel;
                if (null != selectedItem)
                {
                    if (null == selectedItem._parent && selectedItem.Children.Count > 0) //root
                    {
                        projectView.RefreshTreeView(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString(), treeViewSource, treeViewRecipient, sortBy);
                    }
                    else if (null != selectedItem._parent && selectedItem.Children.Count > 0)//sheet name or view type
                    {
                        string selectedHeader = ((TreeViewModel)e.NewValue).Name;
                        string filterString = ((TreeViewModel)e.NewValue).Tag.ToString();
                        projectView.RefreshTreeViewBySelection(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString(), treeViewSource, treeViewRecipient, sortBy, filterString);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh the treeview by the selection.\n" + ex.Message, "Selection of the Treeview Source", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                List<TreeViewModel> treeviewModels = treeViewSource.ItemsSource as List<TreeViewModel>;
                Uri resourceUri = null;
                if (buttonCheck.Tag.ToString() == "Checked")
                {
                    buttonCheck.Tag = "Unchecked";
                    resourceUri = new Uri("HOK.ModelManager;component/Images/uncheck.png", UriKind.Relative);
                    treeviewModels.First().IsChecked = true;
                
                }
                else if (buttonCheck.Tag.ToString() == "Unchecked")
                {
                    buttonCheck.Tag = "Checked";
                    resourceUri = new Uri("HOK.ModelManager;component/Images/check.png", UriKind.Relative);
                    treeviewModels.First().IsChecked = false;
                }
               
                StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);

                buttonCheck.Content = new Image { Source = temp };

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the image on the button.\n"+ex.Message, "Button Check", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               bool duplicated = projectView.UpdateDraftingViews(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString(), treeViewSource, treeViewRecipient, (bool)checkBoxSheet.IsChecked ,statusLable, progressBar);
               if (duplicated)
               {
                   if (comboBoxSource.SelectedIndex > -1 && comboBoxRecipient.SelectedIndex > -1)
                   {
                       if (m_mode == ModelManagerMode.ProjectReplication)
                       {
                           TreeViewSortBy sortBy = (TreeViewSortBy)comboBoxViewBy.SelectedIndex;
                           projectView.RefreshTreeView(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString(), treeViewSource, treeViewRecipient, sortBy);
                       }
                   }
               }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize settings before updating views.\n"+ex.Message, "Update Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonFix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxSource.SelectedIndex > -1 && comboBoxRecipient.SelectedIndex > -1)
                {
                    if (m_mode == ModelManagerMode.ProjectReplication)
                    {
                        ViewMapClass viewMapClass = projectView.GetViewMap(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString());
                        FixLinkWindow fixLinkWindow = new FixLinkWindow(viewMapClass);
                        Nullable<bool> dlResult = fixLinkWindow.ShowDialog();
                        if (dlResult == true)
                        {
                            progressBar.Visibility = System.Windows.Visibility.Visible;
                            statusLable.Visibility = System.Windows.Visibility.Visible;
                            Action emptyDelegate = delegate() { };
                            statusLable.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, emptyDelegate);
                            progressBar.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, emptyDelegate);

                            viewMapClass = fixLinkWindow.FixedViewMap;
                            List<LinkInfo> deletingLinks = fixLinkWindow.DeletingLinks; //to clean up broken links
                            bool updatedGoogleDoc = projectView.UpdateGoogleDoc(viewMapClass.RecipientInfo.DocTitle, viewMapClass.LinkInfoList, deletingLinks);
                            bool updatedViewDictionary = projectView.UpdateViewDictionary(viewMapClass);

                            TreeViewSortBy sortBy = (TreeViewSortBy)comboBoxViewBy.SelectedIndex;
                            projectView.RefreshTreeView(comboBoxSource.SelectedItem.ToString(), comboBoxRecipient.SelectedItem.ToString(), treeViewSource, treeViewRecipient, sortBy);

                            progressBar.Visibility = System.Windows.Visibility.Hidden;
                            statusLable.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to fix links.\n"+ex.Message, "Fix Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxLinked_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxLinked.IsChecked == true)
            {
                List<TreeViewModel> treeviewModels = treeViewSource.ItemsSource as List<TreeViewModel>;
                foreach (TreeViewModel rootNode in treeviewModels)
                {
                    foreach (TreeViewModel secondNode in rootNode.Children)
                    {
                        foreach (TreeViewModel viewNode in secondNode.Children)
                        {
                            if (viewNode.Status==LinkStatus.Linked)
                            {
                                viewNode.IsChecked = true;
                            }
                        }
                    }
                }
            }
        }

        private void checkBoxLinked_Unchecked(object sender, RoutedEventArgs e)
        {
            if (checkBoxLinked.IsChecked == false)
            {
                List<TreeViewModel> treeviewModels = treeViewSource.ItemsSource as List<TreeViewModel>;
                foreach (TreeViewModel rootNode in treeviewModels)
                {
                    foreach (TreeViewModel secondNode in rootNode.Children)
                    {
                        foreach (TreeViewModel viewNode in secondNode.Children)
                        {
                            if (viewNode.Status == LinkStatus.Linked)
                            {
                                viewNode.IsChecked = false;
                            }
                        }
                    }
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }

    public static class UIElementUtil
    {
        public static void PerformStep(ProgressBar progrssBar)
        {
            if (progrssBar.Value < progrssBar.Maximum)
            {
                progrssBar.Value += 1;
                progrssBar.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new System.Threading.ThreadStart(delegate { progrssBar.Value += 1; }));
            }
        }
    }
}
