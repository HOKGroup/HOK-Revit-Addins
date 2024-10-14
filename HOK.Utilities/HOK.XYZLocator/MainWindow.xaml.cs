using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

namespace HOK.XYZLocator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UIApplication m_app;
        private Document m_doc;
        private XYZLocation location = new XYZLocation();
        private ObservableCollection<ProjectLocationProperties> projectLocations = new ObservableCollection<ProjectLocationProperties>();
        public XYZLocation Location { get { return location; } set { location = value; } }

        public MainWindow(UIApplication app)
        {
            m_app = app;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            this.Title = "XYZ Locator v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CollectProjectLocaiton();
        }

        private void CollectProjectLocaiton()
        {
            try
            {
                ProjectLocationSet locationSet = m_doc.ProjectLocations;
                ProjectLocationSetIterator iter = locationSet.ForwardIterator();
                while (iter.MoveNext())
                {
                    ProjectLocation location = iter.Current as ProjectLocation;
                    ProjectLocationProperties plp = new ProjectLocationProperties(location);
                    projectLocations.Add(plp);
                }

                comboBoxSite1.ItemsSource = null;
                comboBoxSite1.ItemsSource = projectLocations;

                comboBoxSite2.ItemsSource = null;
                comboBoxSite2.ItemsSource = projectLocations;

                if (projectLocations.Count > 0)
                {
                    comboBoxSite1.SelectedIndex = 0;
                    comboBoxSite2.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void comboBoxSite1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                radioButtonSiteName1.Content = ((ProjectLocationProperties)comboBoxSite1.SelectedItem).Name;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void comboBoxSite2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                radioButtonSiteName2.Content = ((ProjectLocationProperties)comboBoxSite2.SelectedItem).Name;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                location.Location1 = (ProjectLocationProperties)comboBoxSite1.SelectedItem;
                if ((bool)radioButtonSiteName1.IsChecked)
                {
                    location.Description1 = location.Location1.Name;
                    location.Origin1 = OriginType.SurveyPoint;
                }
                else if ((bool)radioButtonProjectBase1.IsChecked)
                {
                    location.Description1 = "Project Base Point";
                    location.Origin1 = OriginType.BasePoint;
                }
                else if ((bool)radioButtonInternal1.IsChecked)
                {
                    location.Description1 = "Internal Origin";
                    location.Origin1 = OriginType.InternalOrigin;
                }

                location.Location2 = (ProjectLocationProperties)comboBoxSite2.SelectedItem;
                if ((bool)radioButtonSiteName2.IsChecked)
                {
                    location.Description2 = location.Location2.Name;
                    location.Origin2 = OriginType.SurveyPoint;
                }
                else if ((bool)radioButtonProjectBase2.IsChecked)
                {
                    location.Description2 = "Project Base Point";
                    location.Origin2 = OriginType.BasePoint;
                }
                else if ((bool)radioButtonInternal2.IsChecked)
                {
                    location.Description2 = "Internal Origin";
                    location.Origin2 = OriginType.InternalOrigin;
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        
       

    }
}
