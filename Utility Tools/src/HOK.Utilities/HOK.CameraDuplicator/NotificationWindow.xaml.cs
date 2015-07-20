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

namespace HOK.CameraDuplicator
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private List<MissingItem> missingItems = new List<MissingItem>();

        public NotificationWindow(List<MissingItem> items)
        {
            missingItems = items;
            InitializeComponent();
            dataGridMissing.ItemsSource = missingItems;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class MissingItem
    {
        private string viewName = "";
        private string missingType = "";
        private string itemName = "";

        public string ViewName { get { return viewName; } set { viewName = value; } }
        public string MissingType { get { return missingType; } set { missingType = value; } }
        public string ItemName { get { return itemName; } set { itemName = value; } }

        public MissingItem(string vName, string typeName, string iName)
        {
            viewName = vName;
            missingType = typeName;
            itemName = iName;
        }
    }
}
