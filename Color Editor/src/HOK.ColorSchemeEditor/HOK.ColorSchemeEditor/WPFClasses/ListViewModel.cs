using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace HOK.ColorSchemeEditor.WPFClasses
{
    //this class will be used for list view bindings for color schemes
    public class ListViewModel:INotifyPropertyChanged, IComparable
    {
        private ColorScheme colorScheme;
        private string schemeName = "";
        private string viewName = "";

        public ColorScheme ItemContent { get { return colorScheme; } set { colorScheme = value; NotifyPropertyChanged("ItemContent"); } }
        public string SchemeName { get { return schemeName; } set { schemeName = value; NotifyPropertyChanged("SchemeName"); } }
        public string ViewName { get { return viewName; } set { viewName = value; NotifyPropertyChanged("ViewName"); } }

        public ListViewModel(ColorScheme scheme)
        {
            colorScheme = new ColorScheme(scheme);
            schemeName = scheme.SchemeName;
            viewName = scheme.ViewName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public int CompareTo(object obj)
        {
            ListViewModel viewModel = obj as ListViewModel;
            if (viewModel == null)
            {
                throw new ArgumentException("Object is not ListViewModel");
            }
            return this.SchemeName.CompareTo(viewModel.SchemeName);
        }
    }

    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public void Sort()
        {
            Sort(Comparer<T>.Default);
        }

        public void Sort(IComparer<T> comparer)
        {
            int i, j;
            T index;
            
            for(i=1;i<Count;i++)
            {
                index = this[i];
                j = i;

                while ((j > 0) && (comparer.Compare(this[j - 1], index) == 1))
                {
                    this[j] = this[j - 1];
                    j = j - 1;
                }

                this[j] = index;
            }
        }
    }

   
}
