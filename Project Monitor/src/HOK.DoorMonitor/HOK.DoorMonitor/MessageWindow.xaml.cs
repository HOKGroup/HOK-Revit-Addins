using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.DoorMonitor
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {

        private ObservableCollection<MonitorMessage> messageCollection = new ObservableCollection<MonitorMessage>();
        private List<MonitorMessage> selectedItems = new List<MonitorMessage>();

        public ObservableCollection<MonitorMessage> MessageCollection { get { return messageCollection; } set { messageCollection = value; } }
        public List<MonitorMessage> SelectedItems { get { return selectedItems; } set { selectedItems = value; } }

        public MessageWindow(List<MonitorMessage> messages)
        {
            messages = messages.OrderBy(o => o.ElementId).ToList();
            foreach (MonitorMessage message in messages)
            {
                messageCollection.Add(message);
            }

            InitializeComponent();

            if (messageCollection.Count > 0)
            {
                listBoxMessage.ItemsSource = messageCollection;
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonShow_Click(object sender, RoutedEventArgs e)
        {
            selectedItems = new List<MonitorMessage>();
            messageCollection = (ObservableCollection<MonitorMessage>)listBoxMessage.ItemsSource;
            foreach (MonitorMessage message in messageCollection)
            {
                if (message.IsChecked)
                {
                    selectedItems.Add(message);
                }
            }
            this.DialogResult = true;


        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<MonitorMessage> items = (ObservableCollection<MonitorMessage>)listBoxMessage.ItemsSource;
            for (int i = 0; i < items.Count; i++)
            {
                items[i].IsChecked = true;
            }
            messageCollection = items;
            listBoxMessage.ItemsSource = null;
            listBoxMessage.ItemsSource = messageCollection;
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<MonitorMessage> items = (ObservableCollection<MonitorMessage>)listBoxMessage.ItemsSource;
            for (int i = 0; i < items.Count; i++)
            {
                items[i].IsChecked = false;
            }
            messageCollection = items;
            listBoxMessage.ItemsSource = null;
            listBoxMessage.ItemsSource = messageCollection;
        }
    }

    public class MonitorMessage
    {
        private int elementId = -1;
        private Element element=null;
        private string elementName = "";
        private string parameterName = "";
        private string description = "";
        private string message = "";
        private bool isChecked = false;

        public int ElementId { get { return elementId; } set { elementId = value; } }
        public Element ElementObj { get { return element; } set { element=value; } }
        public string ElementName { get { return elementName; } set { elementName = value; } }
        public string ParameterName { get { return parameterName; } set { parameterName = value; } }
        public string Description { get { return description; } set { description = value; } }
        public string Message { get { return message; } set { message = value; } }
        public bool IsChecked { get { return isChecked; } set { isChecked = value; } }

        public MonitorMessage(Element elem, string paramName, string strMessage)
        {
            element = elem;
            elementId = element.Id.IntegerValue;
            elementName = element.Name;
            parameterName = paramName;
            description = strMessage;
            message = elementName + " [" + elementId + "] - "+parameterName+" : " + description;
        }
    }
}
