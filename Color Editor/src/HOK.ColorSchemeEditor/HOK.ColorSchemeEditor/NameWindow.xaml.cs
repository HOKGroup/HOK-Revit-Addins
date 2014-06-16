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
using Autodesk.Revit.DB;
using HOK.ColorSchemeEditor.BCFUtils;

namespace HOK.ColorSchemeEditor
{
    /// <summary>
    /// Interaction logic for NameWindow.xaml
    /// </summary>
    public partial class NameWindow : Window
    {

        private string titleText = "";
        private string groupboxText = "";
        private string nameText = "";
        private List<string> existingList = new List<string>();
        private StorageType selectedStorageType = StorageType.None;

        public string NameText { get { return nameText; } set { nameText = value; } }
        public StorageType SelectedStorageType { get { return selectedStorageType; } set { selectedStorageType = value; } }

        public NameWindow(string title, string groupbox, string name, List<string> names)
        {

            titleText = title;
            groupboxText = groupbox;
            nameText = name;
            existingList = names;

            InitializeComponent();
            textBoxName.Text = nameText;
            this.Title = titleText;
            this.groupBox.Header = groupboxText;

        }

        public NameWindow(string title, string groupbox, string name, List<string> names, StorageType storageType)
        {

            titleText = title;
            groupboxText = groupbox;
            nameText = name;
            existingList = names;
            selectedStorageType = storageType;

            InitializeComponent();
            textBoxName.Text = nameText;
            this.Title = titleText;
            this.groupBox.Header = groupboxText;

        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBoxName.Text))
                {
                    if (!existingList.Contains(textBoxName.Text))
                    {
                        if (selectedStorageType != StorageType.None)
                        {
                            bool validated = false;
                            switch (selectedStorageType)
                            {
                                case StorageType.String:
                                    validated = true;
                                    break;
                                case StorageType.ElementId:
                                    validated = true;
                                    break;
                                case StorageType.Double:
                                    double dblValue = 0;
                                    if (double.TryParse(textBoxName.Text, out dblValue))
                                    {
                                        validated = true;
                                    }
                                    break;
                                case StorageType.Integer:
                                    int intValue = 0;
                                    if (int.TryParse(textBoxName.Text, out intValue))
                                    {
                                        validated = true;
                                    }
                                    break;
                            }
                            if (validated)
                            {
                                nameText = textBoxName.Text;
                                this.DialogResult = true;
                            }
                            else
                            {
                                MessageBox.Show("Please enter a valid type of a value, "+selectedStorageType.ToString(), "Invalid Value Type", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            nameText = textBoxName.Text;
                            this.DialogResult = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show(textBoxName.Text + " already exist.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("The name cannot be empty.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a new name.\n"+ex.Message, "Create a New Name", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void textBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(textBoxName.Text))
                {
                    nameText = textBoxName.Text;
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Name cannot be empty.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
