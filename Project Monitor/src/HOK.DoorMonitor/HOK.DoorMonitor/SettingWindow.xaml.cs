using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace HOK.DoorMonitor
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private Document m_doc;
        private MonitorProjectSetup projectSetup = new MonitorProjectSetup();

        public MonitorProjectSetup ProjectSetup { get { return projectSetup; } set { projectSetup = value; } }

        public SettingWindow(Document doc, MonitorProjectSetup setup)
        {
            m_doc = doc;
            projectSetup = setup;
            InitializeComponent();
            this.Title = "Project Setup Information v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            FillStateList();
            DisplayStoredValues();
        }

        private void FillStateList()
        {
            StringCollection stateNames = Properties.Settings.Default.StateNames;
            comboBoxState.ItemsSource = stateNames;
        }

        private void DisplayStoredValues()
        {
            checkBoxEnabled.IsChecked = projectSetup.IsMonitorOn;
            for (int i = 0; i < comboBoxState.Items.Count; i++)
            {
                string stateItem = comboBoxState.Items[i].ToString();
                if (stateItem == projectSetup.ProjectState)
                {
                    comboBoxState.SelectedIndex = i;
                    break;
                }
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (VerifySelection())
            {
                projectSetup = SaveMonitorSetup();
                ProjectSetupDataStroageUtil.StoreProjectSetup(m_doc, projectSetup);
                this.DialogResult = true;
            }
        }

        private bool VerifySelection()
        {
            bool valid = false;
            if (comboBoxState.SelectedIndex > -1)
            {
                valid = true;
            }
            else
            {
                MessageBox.Show("Please select a project state in the list.", "Project State Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return valid;
        }

        private MonitorProjectSetup SaveMonitorSetup()
        {
            MonitorProjectSetup projectSetup = new MonitorProjectSetup();
            try
            {
                projectSetup.IsMonitorOn = (bool)checkBoxEnabled.IsChecked;
                projectSetup.ProjectState = comboBoxState.SelectedItem.ToString();
                if (projectSetup.ProjectState == "CA") { projectSetup.IsStateCA = true; }
                    
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return projectSetup;
        }

    }
}
