using HOK.Keynote.ClassModels;
using HOK.Keynote.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace HOK.KeynoteEditor.UserControls
{
    /// <summary>
    /// Interaction logic for KeynotePanel.xaml
    /// </summary>
    public partial class KeynotePanel : Window
    {
        public KeynotePanel()
        {
            InitializeComponent();
            this.Title = "Keynote Editor v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; 
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void textBoxKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBoxKey = sender as TextBox;
            TreeViewModel tvm = (TreeViewModel)textBoxKey.DataContext;
            KeynoteInfo keynote = tvm.KeynoteItem;
            keynote.key = textBoxKey.Text;

            if (KeynoteViewModel.KeynoteToUpdate.ContainsKey(keynote._id))
            {
                KeynoteViewModel.KeynoteToUpdate.Remove(keynote._id);
            }
            KeynoteViewModel.KeynoteToUpdate.Add(keynote._id, keynote);

            KeynoteViewModel.UserChanged = true;
            statusLable.Text = "In Progress..";
        }

        private void textBoxDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBoxDescription = sender as TextBox;
            TreeViewModel tvm = (TreeViewModel)textBoxDescription.DataContext;
            KeynoteInfo keynote = tvm.KeynoteItem;
            keynote.description = textBoxDescription.Text;

            if (KeynoteViewModel.KeynoteToUpdate.ContainsKey(keynote._id))
            {
                KeynoteViewModel.KeynoteToUpdate.Remove(keynote._id);
            }
            KeynoteViewModel.KeynoteToUpdate.Add(keynote._id, keynote);

            KeynoteViewModel.UserChanged = true;
            statusLable.Text = "In Progress..";
        }

    }
}
