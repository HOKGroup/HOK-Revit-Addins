using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BCFDBManager
{
    /// <summary>
    /// Interaction logic for CommentWindow.xaml
    /// </summary>

    public enum CommentMode
    {
        ADD, EDIT, DELETE, NONE
    }

    public partial class CommentWindow : Window
    {
        private Markup selectedMarkup = null;
        private Comment selectedComment = null;
        private CommentMode commentMode = CommentMode.NONE;
        
        private string[] statusItems = new string[] { "Error", "Info", "Unknown", "Warning"  };
        private string[] verbalStatusItems = new string[] { "Assigned", "Closed", "Open", "Resolved" };

        public Markup SelectedMarkup { get { return selectedMarkup; } set { selectedMarkup = value; } }
        public Comment SelectedComment { get { return selectedComment; } set { selectedComment = value; } }

        public CommentWindow(Markup markup, Comment comment, CommentMode mode)
        {
            selectedMarkup = markup;
            selectedComment = comment;
            commentMode = mode;

            InitializeComponent();

            textBoxTopic.Text = selectedMarkup.Topic.Title;
            comboBoxStatus.ItemsSource = statusItems;
            comboboxVerbalStatus.ItemsSource = verbalStatusItems;

            DisplayCommentInfo();
        }

        private void DisplayCommentInfo()
        {
            try
            {
                if (commentMode == CommentMode.ADD)
                {
                    this.Title = "Add a Comment";

                    comboBoxStatus.SelectedIndex = 0;
                    comboboxVerbalStatus.SelectedIndex = 0;

                    selectedComment.Author = Environment.UserName;
                    textBoxAuthor.Text = selectedComment.Author;

                    selectedComment.Date = DateTime.Now;
                    textBoxDate.Text = selectedComment.Date.ToString();

                    labelModifiedAuthor.Visibility = Visibility.Hidden;
                    textBoxModifiedAuthor.Visibility = Visibility.Hidden;
                    labelModifiedDate.Visibility = Visibility.Hidden;
                    textBoxModifiedDate.Visibility = Visibility.Hidden;
                }
                else if (commentMode == CommentMode.EDIT)
                {
                    this.Title = "Edit a Comment";

                    textBoxComment.Text = selectedComment.Comment1;
                    if (statusItems.Contains(selectedComment.Status))
                    {
                        comboBoxStatus.SelectedIndex = Array.IndexOf(statusItems, selectedComment.Status);
                    }
                    if (verbalStatusItems.Contains(selectedComment.VerbalStatus))
                    {
                        comboboxVerbalStatus.SelectedIndex = Array.IndexOf(verbalStatusItems, selectedComment.VerbalStatus);
                    }
                    textBoxAuthor.Text = selectedComment.Author;
                    textBoxDate.Text = selectedComment.Date.ToString();

                    selectedComment.ModifiedAuthor = Environment.UserName;
                    textBoxModifiedAuthor.Text = selectedComment.ModifiedAuthor;

                    selectedComment.ModifiedDate = DateTime.Now;
                    textBoxModifiedDate.Text = selectedComment.ModifiedDate.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the information of the comment item.\n" + ex.Message, "Display Comment Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool SaveComment()
        {
            bool saved = false;
            try
            {
                if (!string.IsNullOrEmpty(textBoxComment.Text))
                {
                    selectedComment.Comment1 = textBoxComment.Text;
                }
                else
                {
                    MessageBox.Show("Please enter a comment value.", "Missing Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (null != comboBoxStatus.SelectedItem)
                {
                    selectedComment.Status = comboBoxStatus.SelectedItem.ToString();
                }
                else
                {
                    MessageBox.Show("Please select an item for the status of the comment.", "Missing Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (null != comboboxVerbalStatus.SelectedItem)
                {
                    selectedComment.VerbalStatus = comboboxVerbalStatus.SelectedItem.ToString();
                }
                else
                {
                    MessageBox.Show("Please select an item for the BCF status of the comment.", "Missing BCF Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                saved = true;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save a comment item.\n"+ex.Message, "Save Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SaveComment())
                {
                    if (commentMode == CommentMode.ADD)
                    {
                        selectedMarkup.Comment.Add(selectedComment);
                        this.DialogResult = true;
                    }
                    else if (commentMode == CommentMode.EDIT)
                    {
                        int commentIndex = selectedMarkup.Comment.FindIndex(o => o.Guid == selectedComment.Guid);
                        if (commentIndex > -1)
                        {
                            selectedMarkup.Comment.RemoveAt(commentIndex);
                            selectedMarkup.Comment.Add(selectedComment);
                            this.DialogResult = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to make changes on comment items.\n" + ex.Message, "Apply Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
