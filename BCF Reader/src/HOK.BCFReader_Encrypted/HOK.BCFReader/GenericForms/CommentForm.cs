using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HOK.BCFReader.GenericClasses;

namespace HOK.BCFReader.GenericForms
{
    public partial class CommentForm : Form
    {
        private Comment userComment;

        public Comment CommentInfo { get { return userComment; } set { userComment = value; } }

        public CommentForm(Comment comment)
        {
            userComment = comment;
            InitializeComponent();
            DisplayCommentInfo();
        }

        private void DisplayCommentInfo()
        {
            try
            {
                textBoxAuthor.Text = Environment.UserName;

                for (int i = 0; i < comboBoxType.Items.Count; i++)
                {
                    if (comboBoxType.Items[i].ToString() == userComment.Status.ToString())
                    {
                        comboBoxType.SelectedIndex = i;
                    }
                }

                if (null != userComment.VerbalStatus)
                {
                    for (int i = 0; i < comboBoxStatus.Items.Count; i++)
                    {
                        if (comboBoxStatus.Items[i].ToString() == userComment.VerbalStatus)
                        {
                            comboBoxStatus.SelectedIndex = i;
                        }
                    }
                }

                if (null != userComment.Action)
                {
                    for (int i = 0; i < comboBoxAction.Items.Count; i++)
                    {
                        if (comboBoxAction.Items[i].ToString() == userComment.Action)
                        {
                            comboBoxAction.SelectedIndex = i;
                        }
                    }
                }


                if (null != userComment.CommentString)
                {
                    richTextBoxComment.Text = userComment.CommentString;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to display comment information.\n"+ex.Message, "CommentForm:DisplayCommentInfo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                bool result = true;
                userComment.Author = textBoxAuthor.Text;
                userComment.Date = DateTime.Now;
                if (comboBoxType.SelectedIndex > -1)
                {
                    switch (comboBoxType.SelectedItem.ToString())
                    {
                        case "Error":
                            userComment.Status = CommentStatus.Error;
                            break;
                        case "Info":
                            userComment.Status = CommentStatus.Info;
                            break;
                        case "Unknown":
                            userComment.Status = CommentStatus.Unknown;
                            break;
                        case "Warning":
                            userComment.Status = CommentStatus.Warning;
                            break;
                    }
                }
                else { result = false; MessageBox.Show("Please select a Comment Type."); }

                if (comboBoxStatus.SelectedIndex > -1)
                {
                    userComment.VerbalStatus = comboBoxStatus.SelectedItem.ToString();
                }
                else { result = false; MessageBox.Show("Please select a Comment Status."); }

                if (comboBoxAction.SelectedIndex > -1)
                {
                    userComment.Action = comboBoxAction.SelectedItem.ToString();
                }
                else { result = false; MessageBox.Show("Please select an Action."); }

                if (richTextBoxComment.Text.Length > 0)
                {
                    userComment.CommentString = richTextBoxComment.Text;
                }
                else { result = false; MessageBox.Show("Please enter a text for the comments."); }

                if (result)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save comment information.\n" + ex.Message, "CommentForm:buttonOK_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
