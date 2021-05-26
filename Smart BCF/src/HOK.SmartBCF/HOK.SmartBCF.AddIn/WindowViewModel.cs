using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using HOK.SmartBCF.AddIn.Util;
using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.UserControls;
using HOK.SmartBCF.Utils;
using HOK.SmartBCF.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HOK.SmartBCF.AddIn
{
    internal class WindowViewModel
    {
        private BCFHandler m_handler;
        private ExternalEvent m_event;
        private BCFViewModel bcfViewModel;
        private ComponentViewModel componentViewModel;
        private AddViewModel addViewModel;
        private Dictionary<string/*ifcProjectGuid*/, RevitLinkProperties> linkDictionary = new Dictionary<string, RevitLinkProperties>();
        private Dictionary<string/*ifcGuid*/, RevitComponent> compDictionary = new Dictionary<string, RevitComponent>();

        private RelayCommand componentCommad;
        private RelayCommand issueChangedCommand;
        private RelayCommand componentChangedCommand;
        private RelayCommand addViewCommand;

        public BCFViewModel BCFView { get { return bcfViewModel; } set { bcfViewModel = value; } }
        public ComponentViewModel ComponentView { get { return componentViewModel; } set { componentViewModel = value; } }
        public AddViewModel AddView { get { return addViewModel; } set { addViewModel = value; } }
        public Dictionary<string, RevitLinkProperties> LinkDictionary { get { return linkDictionary; } set { linkDictionary = value; } }
        public Dictionary<string, RevitComponent> CompDictionary { get { return compDictionary; } set { compDictionary = value; } }

        public ICommand ComponentCommand { get { return componentCommad; } }
        public ICommand IssueChangedCommand { get { return issueChangedCommand; } }
        public ICommand ComponentChangedCommand { get { return componentChangedCommand; } }
        public ICommand AddViewCommand { get { return addViewCommand; } }

        public WindowViewModel(ExternalEvent exEvent, BCFHandler handler)
        {
            m_event = exEvent;
            m_handler = handler;
            bcfViewModel = new BCFViewModel(true);
            linkDictionary = CollectLinkInfo();
            compDictionary = CollectElementInfo();

            componentCommad = new RelayCommand(param => this.ComponentExecuted(param));
            issueChangedCommand = new RelayCommand(param => this.IssueChanged(param));
            componentChangedCommand = new RelayCommand(param => this.ComponentChanged(param));
            addViewCommand = new RelayCommand(param => this.AddViewCommandExecuted(param));
        }

        public Dictionary<string, RevitLinkProperties> CollectLinkInfo()
        {
            Dictionary<string, RevitLinkProperties> dictionary = new Dictionary<string, RevitLinkProperties>();
            try
            {
                RevitLinkProperties rlp = new RevitLinkProperties(m_handler.ActiveDoc);
                if (!string.IsNullOrEmpty(rlp.IfcProjectGuid))
                {
                    dictionary.Add(rlp.IfcProjectGuid, rlp);
                }

                FilteredElementCollector collector = new FilteredElementCollector(m_handler.ActiveDoc);
                List<RevitLinkInstance> instances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();
                if (instances.Count > 0)
                {
                    foreach (RevitLinkInstance instance in instances)
                    {
                        RevitLinkProperties p = new RevitLinkProperties(instance);
                        if (null != p.LinkedDocument && !dictionary.ContainsKey(p.IfcProjectGuid))
                        {
                            dictionary.Add(p.IfcProjectGuid, p);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return dictionary;
        }

        public Dictionary<string, RevitComponent> CollectElementInfo()
        {
            Dictionary<string, RevitComponent> dictionary = new Dictionary<string, RevitComponent>();
            try
            {
                foreach (RevitLinkProperties rlp in linkDictionary.Values)
                {
                    Document doc = rlp.LinkedDocument;
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    IList<Element> foundElements = collector
                        .WhereElementIsNotElementType()
                        .WhereElementIsViewIndependent()
                        .WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(false), new ElementIsElementTypeFilter(true))).ToElements();

                    var distinctElements = from elem in foundElements select elem;
                    var elementsFiltered = from elem in distinctElements where null != elem.Category && elem.Category.CategoryType == CategoryType.Model select elem;

                    if (elementsFiltered.Count() > 0)
                    {
                        foreach (Element element in elementsFiltered)
                        {
                            RevitComponent rvtComp = new RevitComponent()
                            {
                                ElementId = element.Id,
                                IfcGuid = element.IfcGUID(),
                                IfcProjectGuid = rlp.IfcProjectGuid
                            };

                            if (!dictionary.ContainsKey(rvtComp.IfcGuid))
                            {
                                dictionary.Add(rvtComp.IfcGuid, rvtComp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return dictionary;
        }

        public void ComponentExecuted(object param)
        {
            try
            {
                if (componentViewModel == null)
                {
                    componentViewModel = new ComponentViewModel(bcfViewModel, linkDictionary, compDictionary, m_handler, m_event);
                    if (componentViewModel.RvtComponents.Count > 0)
                    {
                        ComponentWindow compWindow = new ComponentWindow(componentViewModel);
                        compWindow.Closed += WindowClosed;
                        compWindow.Show();
                    }
                    else
                    {
                        MessageBox.Show("Components don't exist under the selected markup.", "Empty Components", MessageBoxButton.OK, MessageBoxImage.Information);
                        componentViewModel = null;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void IssueChanged(object param)
        {
            try
            {
                if (null != componentViewModel)
                {
                    componentViewModel.RefreshComponents();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void ComponentChanged(object param)
        {
            try
            {
                if (null != param && null != componentViewModel)
                {
                    if (!componentViewModel.UIChanged)
                    {
                        Component component = param as Component;
                        if (null != component)
                        {
                            var compFound = from comp in componentViewModel.RvtComponents where comp.Guid == component.Guid select comp;
                            if (compFound.Count() > 0)
                            {
                                int compIndex = componentViewModel.RvtComponents.IndexOf(compFound.First());
                                if (compIndex > -1)
                                {
                                    componentViewModel.RvtComponents[compIndex].Action = component.Action;
                                    componentViewModel.RvtComponents[compIndex].Responsibility = component.Responsibility;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void AddViewCommandExecuted(object param)
        {
            try
            {
                if (addViewModel == null)
                {
                    addViewModel = new AddViewModel(bcfViewModel, m_event, m_handler);
                    AddViewWindow viewWindow = new AddViewWindow();
                    viewWindow.DataContext = addViewModel;
                    viewWindow.Closed += WindowClosed;
                    viewWindow.Show();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void WindowClosed(object sender, System.EventArgs e)
        {
            componentViewModel = null;
            addViewModel = null;
        }

    }
}
