using System.Windows;
using ARUP.IssueTracker.Classes;
using System.Collections.Generic;
using System.Linq;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for Components.xaml
    /// </summary>
    public partial class ComponentsList : Window
    {
        List<ARUP.IssueTracker.Classes.BCF2.Component> components;
        IComponentController componentController;

        public ComponentsList(ARUP.IssueTracker.Classes.BCF2.Component[] components, IComponentController componentController)
        {
            InitializeComponent();

            this.componentController = componentController;
            this.components = components.ToList();
            componentsList.ItemsSource = this.components;

           
            //ObservableCollection<BCFComponent> ComponentsCollection = new ObservableCollection<BCFComponent>();
           
            //if (v.Element("VisualizationInfo").Elements("Components").Any() && v.Element("VisualizationInfo").Elements("Components").Elements("Component").Any())
            //{

            //    IEnumerable<BCFComponent> result = from c in v.Element("VisualizationInfo").Elements("Components").Elements("Component")
            //                                          select new BCFComponent()
            //                                          {
            //                                              ifcguid = (string)c.Attribute("IfcGuid"),
            //                                              authoringid = (string)c.Element("AuthoringToolId"),
            //                                              origsystem = (string)c.Element("OriginatingSystem")
            //                                          };

                //foreach (var item in v.Element("VisualizationInfo").Elements("Components").Elements("Component"))
                //{
                //    BCFComponent bc = new BCFComponent();
                //    bc.ifcguid = (item.Attributes("IfcGuid").Any()) ? item.Attributes("IfcGuid").First().Value.ToString() : "";
                //    bc.authoringid = (item.Elements("AuthoringToolId").Any()) ? item.Element("AuthoringToolId").Value.ToString() : "";
                //    bc.origsystem = (item.Elements("OriginatingSystem").Any()) ? item.Element("OriginatingSystem").Value.ToString() : "";
                //    ComponentsCollection.Add(bc);
                //}
                //componentsList.ItemsSource = result;
                //componentsList.Items.Refresh();
            //}
            //MessageBox.Show(ComponentsCollection.Count().ToString());
            
        }

        private void selectElementsButton_Click(object sender, RoutedEventArgs e)
        {
            if(componentController != null)
            {
                componentController.selectElements(components.Select(c => c.AuthoringToolId).ToList());
            }
        }
    }
}
