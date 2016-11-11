using System;
using System.Windows;
using System.Windows.Controls;
using ARUP.IssueTracker.Classes;
using ARUP.IssueTracker.Classes.BCF2;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;

namespace ARUP.IssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for BCFPanel.xaml
    /// </summary>
    public partial class BCFPanel : UserControl
    {
        //public event EventHandler<IntArg> ComponentsShowBCFEH;
        //public event EventHandler<StringArg> OpenImageEH;

        public event RoutedEventHandler open3dViewEvent;
        public MainPanel mainPanel = null;

        public BCFPanel()
        {
            InitializeComponent();
        }

        //private void ComponentsShow(object sender, RoutedEventArgs e)
        //{

        //    try
        //    {
        //        if (issueList.SelectedIndex != -1)
        //        {
        //            if (ComponentsShowBCFEH != null)
        //            {
        //                ComponentsShowBCFEH(this, new IntArg(issueList.SelectedIndex));
        //            }
        //        }
        //    }
        //    catch (System.Exception ex1)
        //    {
        //        MessageBox.Show("exception: " + ex1);
        //    }
        //}

        //private void OpenImage(object sender, RoutedEventArgs e)
        //{
        //    if (OpenImageEH != null)
        //    {
        //        OpenImageEH(this, new StringArg((string)((Button)sender).Tag));
        //    }
        //}
        public int listIndex
        {
            get { return issueList.SelectedIndex; }
            set { issueList.SelectedIndex = value; }
        }

        private void popup_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.Popup popup = sender as System.Windows.Controls.Primitives.Popup;
            if (popup != null)
                popup.IsOpen = false;
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
                mainPanel.ComponentsShowBCF(sender, e);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OpenLinkProjBtn_Click(object sender, RoutedEventArgs e)
        {
            string url = (string)((Button)sender).Tag;
            if(!string.IsNullOrWhiteSpace(url))
                System.Diagnostics.Process.Start(url);
        }

        private void switchTextBlockTextBoxVisibility(TextBlock tBlock, TextBox tBox)
        {
            if (tBlock.Visibility == Visibility.Collapsed)
            {
                tBlock.Visibility = Visibility.Visible;
                tBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                tBlock.Visibility = Visibility.Collapsed;
                tBox.Visibility = Visibility.Visible;
            }
        }

        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            switchTextBlockTextBoxVisibility(statusTextBlock, statusTextBox);            
        }

        private void statusTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                ChangeStatus.Focus();              
            }
        }        

        private void statusTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            switchTextBlockTextBoxVisibility(statusTextBlock, statusTextBox);
        }

        private void ChangePriority_Click(object sender, RoutedEventArgs e)
        {
            switchTextBlockTextBoxVisibility(priorityTextBlock, priorityTextBox);
        }

        private void ChangeType_Click(object sender, RoutedEventArgs e)
        {
            switchTextBlockTextBoxVisibility(typeTextBlock, typeTextBox);
        }

        private void ChangeAssign_Click(object sender, RoutedEventArgs e)
        {
            switchTextBlockTextBoxVisibility(assignedToTextBlock, assignedToTextBox);
        }

        private void priorityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            switchTextBlockTextBoxVisibility(priorityTextBlock, priorityTextBox);
        }

        private void priorityTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ChangePriority.Focus();
            }
        }

        private void typeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            switchTextBlockTextBoxVisibility(typeTextBlock, typeTextBox);
        }

        private void typeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ChangeType.Focus();
            }
        }

        private void assignedToTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ChangeAssign.Focus();
            }
        }

        private void assignedToTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            switchTextBlockTextBoxVisibility(assignedToTextBlock, assignedToTextBox);
        }

        // Disable this function for now
        /*private void labelButton_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                Button btn = sender as Button;
                TextBlock tBolck = btn.Content as TextBlock;

                MessageBoxResult result = MessageBox.Show(string.Format("Do you want to delete this label {0}?", tBolck.Text), "Delete Label", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Markup markup = issueList.SelectedItem as Markup;
                    int index = Array.FindIndex(markup.Topic.Labels, s => s == tBolck.Text);
                    Array.Clear(markup.Topic.Labels, index, 1);
                    labelList.Items.Refresh();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }            
        }*/

    }
}
