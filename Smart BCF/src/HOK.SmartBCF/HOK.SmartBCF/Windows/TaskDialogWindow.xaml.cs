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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HOK.SmartBCF.BCFDBWriter;

namespace HOK.SmartBCF.Windows
{
    /// <summary>
    /// Interaction logic for TaskDialogWindow.xaml
    /// </summary>
    /// 
    public enum TaskDialogOption
    {
        MERGE = 0, REPLACE = 1, IGNORE = 2, NONE = 3
    }

    public partial class TaskDialogWindow : Window
    {
        private TaskDialogOption selectedOption = TaskDialogOption.NONE;

        public TaskDialogOption SelectedOption { get { return selectedOption; } set { selectedOption = value; } }

        public TaskDialogWindow(string questionText)
        {
            InitializeComponent();
            textBlockQuestion.Text = questionText;
        }

        private void buttonReplace_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = TaskDialogOption.REPLACE;
            this.DialogResult = true;
        }

        private void buttonMerge_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = TaskDialogOption.MERGE;
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = TaskDialogOption.IGNORE;
            this.DialogResult = true;
        }
    }

}
