using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using ARUP.IssueTracker.Classes.BCF2;

namespace ARUP.IssueTracker.Classes
{
    public class Jira : INotifyPropertyChanged
    {
        public int startAt { get; set; }
        public int maxResults { get; set; }
        private int total { get; set; }
        public int Total
        {
            get
            {
                return total;

            }

            set
            {
                total = value;
                NotifyPropertyChanged("Total");
            }
        }
        private BCF1.BCF bcf_1 { get; set; }
        public BCF1.BCF Bcf_1
        {
            get
            {
                return bcf_1;
            }

            set
            {
                bcf_1 = value;
                NotifyPropertyChanged("Bcf");
            }
        }
        private BcfFile bcf { get; set; }
        public BcfFile Bcf
        {
            get
            {
                return bcf;
            }

            set
            {
                bcf = value;
                NotifyPropertyChanged("Bcf");
            }
        }
        private Self self { get; set; }
        public Self Self
        {
            get
            {
                return self;
            }

            set
            {
                self = value;
                NotifyPropertyChanged("Self");
            }
        }
        private ObservableCollection<Resolution> resolutionCollection;
        private ObservableCollection<Priority> prioritiesCollection;
        private ObservableCollection<Filter> filtersCollection;
        private ObservableCollection<Issuetype> typesCollection;
        private ObservableCollection<Project> projectsCollection;
        private ObservableCollection<Issue> issuesCollection;
        private ObservableCollection<Component> componentsCollection;

        public ObservableCollection<Component> ComponentsCollection
        {
            get
            {
                return componentsCollection;
            }

            set
            {
                componentsCollection = value;
                NotifyPropertyChanged("ComponentsCollection");
            }
        }
        public ObservableCollection<Resolution> ResolutionCollection
        {
            get
            {
                return resolutionCollection;
            }

            set
            {
                resolutionCollection = value;
                NotifyPropertyChanged("ResolutionCollection");
            }
        }
        public ObservableCollection<Priority> PrioritiesCollection
        {
            get
            {
                return prioritiesCollection;
            }

            set
            {
                prioritiesCollection = value;
                NotifyPropertyChanged("PrioritiesCollection");
            }
        }
        public ObservableCollection<Filter> FiltersCollection
        {
            get
            {
                return filtersCollection;
            }

            set
            {
                filtersCollection = value;
                NotifyPropertyChanged("FiltersCollection");
            }
        }
        public ObservableCollection<Issuetype> TypesCollection
        {
            get
            {
                return typesCollection;
            }

            set
            {
                typesCollection = value;
                NotifyPropertyChanged("TypesCollection");
            }
        }
        public ObservableCollection<Project> ProjectsCollection
        {
            get
            {
                return projectsCollection;
            }

            set
            {
                projectsCollection = value;
                NotifyPropertyChanged("ProjectsCollection");
            }
        }
        
        public ObservableCollection<Issue> IssuesCollection
        {
            get
            {
                return issuesCollection;
            }

            set
            {
                issuesCollection = value;
                NotifyPropertyChanged("IssuesCollection");
            }
        }


        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string formattedissuecount
        {
            get
            {
                string c;
                if (issuesCollection.Count == 1)
                    c = issuesCollection.Count.ToString() + " Issue";
                else if (issuesCollection.Count == 0)
                    c = "No Issues";
                else
                    c = issuesCollection.Count.ToString() + " Issues";
                return c;
            }

        }
    }
}
