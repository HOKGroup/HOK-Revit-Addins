using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Classes
{
    public class RevitSheetData : INotifyPropertyChanged
    {
        //database file info
        private string filePath = "";

        //sheet info
        private ObservableCollection<Discipline> disciplines = new ObservableCollection<Discipline>();
        private ObservableCollection<RevitSheet> sheets = new ObservableCollection<RevitSheet>();
        private ObservableCollection<RevitRevision> revisions = new ObservableCollection<RevitRevision>();
        private ObservableCollection<RevitView> views = new ObservableCollection<RevitView>();
        private ObservableCollection<RevitViewType> viewTypes = new ObservableCollection<RevitViewType>();
        private ObservableCollection<RevitItemMapper> itemMaps = new ObservableCollection<RevitItemMapper>();
        private ObservableCollection<LinkedProject> linkedProjects = new ObservableCollection<LinkedProject>();
        private ObservableCollection<SheetParameter> sheetParameters = new ObservableCollection<SheetParameter>();
        private int selectedDisciplineIndex = -1;

        public string FilePath { get { return filePath; } set { filePath = value; NotifyPropertyChanged("FilePath"); } }
        public ObservableCollection<Discipline> Disciplines { get { return disciplines; } set { disciplines = value; NotifyPropertyChanged("Disciplines"); } }
        public ObservableCollection<RevitSheet> Sheets { get { return sheets; } set { sheets = value; NotifyPropertyChanged("Sheets"); } }
        public ObservableCollection<RevitRevision> Revisions { get { return revisions; } set { revisions = value; NotifyPropertyChanged("Revisions"); } }
        public ObservableCollection<RevitView> Views { get { return views; } set { views = value; NotifyPropertyChanged("Views"); } }
        public ObservableCollection<RevitViewType> ViewTypes { get { return viewTypes; } set { viewTypes = value; NotifyPropertyChanged("ViewTypes"); } }
        public ObservableCollection<RevitItemMapper> ItemMaps { get { return itemMaps; } set { itemMaps = value; NotifyPropertyChanged("ItemMaps"); } }
        public ObservableCollection<LinkedProject> LinkedProjects { get { return linkedProjects; } set { linkedProjects = value; NotifyPropertyChanged("LinkedProjects"); } }
        public ObservableCollection<SheetParameter> SheetParameters { get { return sheetParameters; } set { sheetParameters = value; NotifyPropertyChanged("SheetParameters"); } }
        public int SelectedDisciplineIndex { get { return selectedDisciplineIndex; } set { selectedDisciplineIndex = value; NotifyPropertyChanged("SelectedDisciplineIndex"); } }

        public RevitSheetData()
        {
           
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
