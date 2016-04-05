using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using HOK.SmartBCF.AddIn.Util;
using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.UserControls;
using HOK.SmartBCF.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Component = HOK.SmartBCF.Schemas.Component;

namespace HOK.SmartBCF.AddIn
{
    public class ComponentViewModel:INotifyPropertyChanged
    {
        private BCFViewModel bcfView;
        private BCFHandler m_handler;
        private ExternalEvent m_event;
        private Dictionary<string, RoomProperties> roomDictionary = new Dictionary<string, RoomProperties>();
        private Dictionary<string/*ifcProjectGuid*/, RevitLinkProperties> linkDictionary = new Dictionary<string, RevitLinkProperties>();

        private RevitComponent selectedComponent = null;
        private int selectedComponentIndex =-1;
        private BCFZIP selectedBCF = null;
        private Markup selectedMarkup = null;
        private Comment selectedComment = null;
        private RevitExtension selectedAction = null;
        private RevitExtension selectedResponsibility = null;
        private ObservableCollection<RevitComponent> rvtComponents = new ObservableCollection<RevitComponent>();
        private ObservableCollection<RevitComponent> filteredComponents = new ObservableCollection<RevitComponent>();
        private ObservableCollection<Comment> comments = new ObservableCollection<Comment>();
        private string indexText = "";
       
        //View Option
        private bool isHighlightChecked = false;
        private bool isIsolateChecked = false;
        private bool isSectionBoxChecked = false;
        //Command
        private RelayCommand moveComponentCommand;
        private RelayCommand writeParametersCommand;
        private RelayCommand applyViewCommand;
        private RelayCommand highlightCommand;
        private RelayCommand isolateCommand;
        private RelayCommand sectionboxCommand;
        private RelayCommand componentChangedCommand;
        private bool uiChanged = false;

        public BCFViewModel BCFView { get { return bcfView; } set { bcfView = value; NotifyPropertyChanged("BCFView"); } }
        public Dictionary<string, RoomProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; NotifyPropertyChanged("RoomDictionary"); } }
        public Dictionary<string, RevitLinkProperties> LinkDictionary { get { return linkDictionary; } set { linkDictionary = value; NotifyPropertyChanged("LinkDictionary"); } }
        public RevitComponent SelectedComponent { get { return selectedComponent; } set { selectedComponent = value; NotifyPropertyChanged("SelectedComponent"); } }
        public int SelectedComponentIndex { get { return selectedComponentIndex; } set { selectedComponentIndex = value; SetIndexText(); NotifyPropertyChanged("SelectedComponentIndex"); } }
        public BCFZIP SelectedBCF { get { return selectedBCF; } set { selectedBCF = value; NotifyPropertyChanged("SelectedBCF"); } }
        public Markup SelectedMarkup { get { return selectedMarkup; } set { selectedMarkup = value; NotifyPropertyChanged("SelectedMarkup"); } }
        public Comment SelectedComment { get { return selectedComment; } set { selectedComment = value; NotifyPropertyChanged("SelectedComment"); } }
        public RevitExtension SelectedAction { get { return selectedAction; } set { selectedAction = value; NotifyPropertyChanged("SelectedAction"); } }
        public RevitExtension SelectedResponsibility { get { return selectedResponsibility; } set { selectedResponsibility = value; NotifyPropertyChanged("SelectedResponsibility"); } }
        public ObservableCollection<RevitComponent> RvtComponents { get { return rvtComponents; } set { rvtComponents = value; NotifyPropertyChanged("RvtComponents"); } }
        public ObservableCollection<RevitComponent> FilteredComponents { get { return filteredComponents; } set { filteredComponents = value; NotifyPropertyChanged("FilteredComponents"); } }
        public ObservableCollection<Comment> Comments { get { return comments; } set { comments = value; NotifyPropertyChanged("Comments"); } }
        public string IndexText { get { return indexText; } set { indexText = value; NotifyPropertyChanged("IndexText"); } }
        
        //View Option
        public bool IsHighlightChecked { get { return isHighlightChecked; } set { isHighlightChecked = value; NotifyPropertyChanged("IsHighlightChecked"); } }
        public bool IsIsolateChecked { get { return isIsolateChecked; } set { isIsolateChecked = value; NotifyPropertyChanged("IsIsolateChecked"); } }
        public bool IsSectionBoxChecked { get { return isSectionBoxChecked; } set { isSectionBoxChecked = value; NotifyPropertyChanged("IsSectionBoxChecked"); } }
        //Command
        public ICommand MoveComponentCommand { get { return moveComponentCommand; } }
        public ICommand WriteParametersCommand { get { return writeParametersCommand; } }
        public ICommand ApplyViewCommand { get { return applyViewCommand; } }
        public ICommand HighlightCommand { get { return highlightCommand; } }
        public ICommand IsolateCommand { get { return isolateCommand; } }
        public ICommand SectionboxCommand { get { return sectionboxCommand; } }
        public ICommand ComponentChangedCommand { get { return componentChangedCommand; } }
        public bool UIChanged { get { return uiChanged; } set { uiChanged = value; } }

        public ComponentViewModel(BCFViewModel bcfViewModel, Dictionary<string, RoomProperties> rooms, Dictionary<string, RevitLinkProperties> links,  BCFHandler handler, ExternalEvent extEvent)
        {
            try
            {
                bcfView = bcfViewModel;
                m_handler = handler;
                m_event = extEvent;
                roomDictionary = rooms;
                linkDictionary = links;

                moveComponentCommand = new RelayCommand(param => this.MoveComponentExecuted(param));
                writeParametersCommand = new RelayCommand(param => this.WriteParametersExecuted(param));
                applyViewCommand = new RelayCommand(param => this.ApplyViewExecuted(param));
                highlightCommand = new RelayCommand(param => this.HighlightExecuted(param));
                isolateCommand = new RelayCommand(param => this.IsolateExecuted(param));
                sectionboxCommand = new RelayCommand(param => this.SectionBoxExecuted(param));
                componentChangedCommand = new RelayCommand(param => this.ComponentChanged(param));

                RefreshComponents();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void RefreshComponents()
        {
            try
            {
                this.SelectedBCF = bcfView.BCFFiles[bcfView.SelectedIndex];
                this.SelectedMarkup = selectedBCF.Markups[selectedBCF.SelectedMarkup];
                ViewPoint selectedViewPoint = selectedMarkup.SelectedViewpoint;
                if (null != selectedViewPoint)
                {
                    int viewpointInex = selectedMarkup.Viewpoints.IndexOf(selectedViewPoint);

                    this.Comments = new ObservableCollection<Comment>();
                    this.SelectedComment = null;
                    
                    var commentFound = from comment in selectedMarkup.Comment where comment.Viewpoint.Guid == selectedViewPoint.Guid select comment;
                    if (commentFound.Count() > 0)
                    {
                        this.Comments = new ObservableCollection<Comment>(commentFound.ToList());
                        this.SelectedComment = comments[0];
                    }

                    if (null != selectedViewPoint.VisInfo)
                    {
                        this.SelectedComponent = null;
                        this.SelectedComponentIndex = -1;
                        this.RvtComponents.Clear();

                        var selectedProjects = from header in selectedMarkup.Header where linkDictionary.ContainsKey(header.IfcProject) select header.IfcProject;
                        List<string> ifcProjectGuids = (selectedProjects.Count() > 0) ? selectedProjects.ToList() : linkDictionary.Keys.ToList();

                        ObservableCollection<Component> componentsToUpdate = new ObservableCollection<Component>();
                        foreach (Component comp in selectedViewPoint.VisInfo.Components)
                        {
                            int compIndex = selectedViewPoint.VisInfo.Components.IndexOf(comp);
                            RevitComponent rvtComponent = null;
                            if (!string.IsNullOrEmpty(comp.AuthoringToolId))
                            {
                                foreach (string projectId in ifcProjectGuids)
                                {
                                    RevitLinkProperties rlp = linkDictionary[projectId];
                                    rvtComponent = new RevitComponent(comp, rlp);
                                    if (rvtComponent.ElementExist && rvtComponent.ElementMatched)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        rvtComponent = null;
                                    }
                                }          
                            }
                            else if (roomDictionary.ContainsKey(comp.IfcGuid))
                            {
                                RoomProperties rp = roomDictionary[comp.IfcGuid];
                                rvtComponent = new RevitComponent(comp, rp);
                            }

                            if (null != rvtComponent)
                            {
                                var foundAction = from ext in selectedBCF.ExtensionColor.Extensions where ext.Guid == rvtComponent.Action.Guid select ext;
                                if (foundAction.Count() > 0)
                                {
                                    rvtComponent.Action = foundAction.First();
                                }

                                var foundResponsibility = from ext in selectedBCF.ExtensionColor.Extensions where ext.Guid == rvtComponent.Responsibility.Guid select ext;
                                if (foundResponsibility.Count() > 0)
                                {
                                    rvtComponent.Responsibility = foundResponsibility.First();
                                }

                                if (string.IsNullOrEmpty(comp.ElementName))
                                {
                                    componentsToUpdate.Add(comp);

                                    bcfView.BCFFiles[bcfView.SelectedIndex].Markups[selectedBCF.SelectedMarkup].Viewpoints[viewpointInex].VisInfo.Components[compIndex].ElementName = rvtComponent.ElementName;
                                }
                                this.RvtComponents.Add(rvtComponent);
                            }
                        }

                        if (componentsToUpdate.Count > 0)
                        {
                            bool updatedDB = BCFDBWriter.BCFDBWriter.UpdateComponents(componentsToUpdate);
                        }

                        ApplyCategoryFilter();

                        UpdateBackgroundView();
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void ApplyCategoryFilter()
        {
            try
            {
                ObservableCollection<CategoryProperties> categories = ComponentCategoryFilter.Categories;
                this.FilteredComponents.Clear();

                foreach (CategoryProperties cp in categories)
                {
                    var components = from comp in rvtComponents where comp.Category.CategoryId == cp.CategoryId select comp;
                    if (components.Count() > 0)
                    {
                        List<RevitComponent> componentFound = components.ToList();
                        foreach (RevitComponent rvtComp in componentFound)
                        {
                            int index = rvtComponents.IndexOf(rvtComp);
                            this.RvtComponents[index].Category.Selected = cp.Selected;
                        }
                    }
                }

                var selectedComponents = from comp in rvtComponents where comp.Category.Selected select comp;
                if (selectedComponents.Count() > 0)
                {
                    this.FilteredComponents = new ObservableCollection<RevitComponent>(selectedComponents.ToList());
                    this.SelectedComponentIndex = 0;
                    this.SelectedComponent = filteredComponents[selectedComponentIndex];
                    SetIndexText();
                }
                else
                {
                    this.IndexText = "Components Not Found";
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void UpdateBackgroundView()
        {
            try
            {
                this.IsHighlightChecked = false;
                this.IsIsolateChecked = false;
                this.IsSectionBoxChecked = false;

                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.SetViewPointView);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void MoveComponentExecuted(object param)
        {
            try
            {
                if (null != param)
                {
                    string direction = param.ToString().ToLower();
                    if (direction == "forward")
                    {
                        if (selectedComponentIndex < filteredComponents.Count - 1)
                        {
                            int index = selectedComponentIndex + 1;
                            selectedComponent = filteredComponents[index];
                            this.SelectedComponentIndex = index;
                        }
                    }
                    else if (direction == "backward")
                    {
                        if (selectedComponentIndex > 0)
                        {
                            int index = selectedComponentIndex - 1;
                            selectedComponent = filteredComponents[index];
                            this.SelectedComponentIndex = index;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void WriteParametersExecuted(object param)
        {
            try
            {
                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.WriteParameters);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void ApplyViewExecuted(object param)
        {
            try
            {
                selectedComponent = filteredComponents[selectedComponentIndex];
                if (selectedComponent.IsLinked)
                {
                    this.IsHighlightChecked = false;
                    this.IsIsolateChecked = false;
                }

                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.ApplyViews);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void HighlightExecuted(object param)
        {
            try
            {
                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.HighlightElement);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void IsolateExecuted(object param)
        {
            try
            {
                if (isIsolateChecked) { this.IsSectionBoxChecked = false; }

                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.IsolateElement);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void SectionBoxExecuted(object param)
        {
            try
            {
                if (isSectionBoxChecked) { this.IsIsolateChecked = false; }

                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.PlaceSectionBox);
                m_event.Raise();
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
                if (selectedComponentIndex > -1)
                {
                    uiChanged = true;
                    bool updated = BCFDBWriter.BCFDBWriter.UpdateComponent(filteredComponents[selectedComponentIndex]);
                    UpdateBCFView();
                    uiChanged = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void UpdateBCFView()
        {
            try
            {
                selectedComponent = filteredComponents[selectedComponentIndex];
                int fileIndex = bcfView.SelectedIndex;
                int markupIndex = bcfView.BCFFiles[fileIndex].SelectedMarkup;
                ViewPoint selectedViewpoint = bcfView.BCFFiles[fileIndex].Markups[markupIndex].SelectedViewpoint;
                int viewpointIndex = bcfView.BCFFiles[fileIndex].Markups[markupIndex].Viewpoints.IndexOf(selectedViewpoint);
                var componentFound = from comp in selectedViewpoint.VisInfo.Components where comp.Guid == selectedComponent.Guid select comp;
                if (componentFound.Count() > 0)
                {
                    int componentIndex = selectedViewpoint.VisInfo.Components.IndexOf(componentFound.First());
                    if (componentIndex > -1)
                    {
                        bcfView.BCFFiles[fileIndex].Markups[markupIndex].Viewpoints[viewpointIndex].VisInfo.Components[componentIndex].Action = filteredComponents[selectedComponentIndex].Action;
                        bcfView.BCFFiles[fileIndex].Markups[markupIndex].Viewpoints[viewpointIndex].VisInfo.Components[componentIndex].Responsibility = filteredComponents[selectedComponentIndex].Responsibility;
                        bcfView.BCFFiles[fileIndex].Markups[markupIndex].SelectedViewpoint = bcfView.BCFFiles[fileIndex].Markups[markupIndex].Viewpoints[viewpointIndex];
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void SetIndexText()
        {
            this.IndexText = (selectedComponentIndex + 1) + " of " + filteredComponents.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
