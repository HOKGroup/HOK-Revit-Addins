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
using System.Windows.Shapes;
using System.ComponentModel;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for ChangeValue.xaml
    /// </summary>
    public partial class ChangeValue : Window
    {
        //string name;
        public ChangeValue()
        {
            InitializeComponent();
           // TypeDescriptor.GetProperties(this.valuesList)["ItemsSource"].AddValueChanged(this.valuesList, new EventHandler(ListView_ItemsSourceChanged));
   

           
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }



        //public void setIndex<T>(T entity)
        //{
            
        //   for (var i =0; i<valuesList.Items.Count;i++)
        //   {
        //       if(entity == e[i])
        //       {

        //       }
        //   }
        //}

       
    }
}
