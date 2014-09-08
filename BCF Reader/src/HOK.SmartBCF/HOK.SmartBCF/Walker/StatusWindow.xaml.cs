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

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window
    {
        private List<ColorDefinition> actionDefinitions = new List<ColorDefinition>();
        private List<ColorDefinition> responsibleDefinitions = new List<ColorDefinition>();

        public List<ColorDefinition> ActionDefinitions { get { return actionDefinitions; } set { actionDefinitions = value; } }
        public List<ColorDefinition> ResponsibleDefinitions { get { return responsibleDefinitions; } set { responsibleDefinitions = value; } }

        public StatusWindow(List<ColorDefinition> actionList, List<ColorDefinition> resList)
        {
            actionDefinitions = actionList;
            responsibleDefinitions = resList;

            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void buttonActionColor_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonResColor_Click(object sender, RoutedEventArgs e)
        {

        }

        
       
    }
}
