using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Classes
{
    public enum ViewTypeEnum
    {
        Undefined, FloorPlan, CeilingPlan, Elevation, ThreeD, Schedule, DrawingSheet, ProjectBrowser, Report, DraftingView, Legend, SystemBrowser, EngineeringPlan,
        AreaPlan, Section, Detail, CostReport, LoadsReport, PresureLossReport, ColumnSchedule, PanelSchedule, Walkthrough, Rendering, Internal
    }

    public class RevitView : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string name = "";
        private RevitSheet sheet = new RevitSheet();
        private RevitViewType viewType = new RevitViewType();
        private double locationU = 0;
        private double locationV = 0;

        private RevitLinkStatus linkStatus = new RevitLinkStatus();

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }
        public RevitSheet Sheet { get { return sheet; } set { sheet = value; NotifyPropertyChanged("sheet"); } }
        public RevitViewType ViewType { get { return viewType; } set { viewType = value; NotifyPropertyChanged("ViewType"); } }
        public double LocationU { get { return locationU; } set { locationU = value; NotifyPropertyChanged("LocationU"); } }
        public double LocationV { get { return locationV; } set { locationV = value; NotifyPropertyChanged("LocationV"); } }

        public RevitLinkStatus LinkStatus { get { return linkStatus; } set { linkStatus = value; NotifyPropertyChanged("LinkStatus"); } }

        public RevitView()
        {
        }

        public RevitView(Guid viewId, string viewName)
        {
            id = viewId;
            name = viewName;
        }

        public RevitView(Guid viewId, string viewName, RevitSheet rvtSheet, RevitViewType rvtViewType, double u, double v)
        {
            id = viewId;
            name = viewName;
            sheet = rvtSheet;
            viewType = rvtViewType;
            locationU = u;
            locationV = v;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
    }

    public class RevitViewType : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string name = "";
        private ViewTypeEnum viewType = ViewTypeEnum.Undefined;

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }
        public ViewTypeEnum ViewType { get { return viewType; } set { viewType = value; NotifyPropertyChanged("ViewType"); } }

        public RevitViewType()
        { }

        public RevitViewType(Guid guid, string viewTypeName, ViewTypeEnum viewTypeEnum)
        {
            id = guid;
            name = viewTypeName;
            viewType = viewTypeEnum;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
    }
}
