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

using System.ComponentModel;
using ARUP.IssueTracker.Classes;

namespace ARUP.IssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for Waiter.xaml
    /// </summary>
    public partial class Waiter : UserControl
    {
        public int processes = 0;

        public Waiter()
        {
            InitializeComponent();
        }

        public void Start()
        {
            this.Visibility = System.Windows.Visibility.Visible;
        }

        void Stop()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
