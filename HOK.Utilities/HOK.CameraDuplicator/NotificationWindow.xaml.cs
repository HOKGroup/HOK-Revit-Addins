using System.Collections.Generic;
using System.Windows;

namespace HOK.CameraDuplicator
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow
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
            Close();
        }
    }

    public class MissingItem
    {
        public string ViewName { get; set; }
        public string MissingType { get; set; }
        public string ItemName { get; set; }

        public MissingItem(string vName, string typeName, string iName)
        {
            ViewName = vName;
            MissingType = typeName;
            ItemName = iName;
        }
    }
}
