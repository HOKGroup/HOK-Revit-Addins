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
using Autodesk.Revit.DB;

namespace HOK.SheetDataManager
{
    /// <summary>
    /// Interaction logic for CommandMessageBox.xaml
    /// </summary>
    public partial class CommandMessageBox : Window
    {
        private Dictionary<Guid, MessageInfo> messages = new Dictionary<Guid, MessageInfo>();

        public CommandMessageBox(Dictionary<Guid, MessageInfo> dictionary, string title)
        {
            messages = dictionary;

            InitializeComponent();
            this.Title = title;

            dataGridMessage.ItemsSource = messages.Values.ToList();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }

    public class MessageInfo
    {
        private Guid dbId = Guid.Empty;
        private string itemName = "";
        private string message = "";

        public Guid DBId { get { return dbId; } set { dbId = value; } }
        public string ItemName { get { return itemName; } set { itemName = value; } }
        public string Message { get { return message; } set { message = value; } }

        public MessageInfo()
        {

        }

        public MessageInfo(Guid id, RevitSheet sheet, string messageStr)
        {
            dbId = id;
            itemName = sheet.Number + " " + sheet.Name;
            message = messageStr;
        }

        public MessageInfo(Guid id, string name, string messageStr)
        {
            dbId = id;
            itemName = name;
            message = messageStr;
        }
    }
}
