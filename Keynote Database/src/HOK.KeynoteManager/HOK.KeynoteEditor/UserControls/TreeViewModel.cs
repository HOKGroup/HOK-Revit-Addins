using HOK.Keynote.ClassModels;
using HOK.Keynote.REST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.KeynoteEditor.UserControls
{
    public class TreeViewModel : INotifyPropertyChanged
    {
        private KeynoteInfo keynoteItem = new KeynoteInfo();
        private ObservableCollection<TreeViewModel> childrenItems = new ObservableCollection<TreeViewModel>();
        private TreeViewModel parentItem = null;
        private bool isEditoMode = false;
        private string toolTip = "";
        private bool isSelected = false;

        public KeynoteInfo KeynoteItem { get { return keynoteItem; } set { keynoteItem = value; NotifyPropertyChanged("KeynoteItem"); } }
        public ObservableCollection<TreeViewModel> ChildrenItems { get { return childrenItems; } set { childrenItems = value; NotifyPropertyChanged("ChildrenItems"); } }
        public TreeViewModel ParentItem { get { return parentItem; } set { parentItem = value; NotifyPropertyChanged("ParentItem"); } }
        public bool IsEditMode { get { return isEditoMode; } set { isEditoMode = value; NotifyPropertyChanged("IsEditMode"); } }
        public string ToolTip { get { return toolTip; } set { toolTip = value; NotifyPropertyChanged("ToolTip"); } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public TreeViewModel(KeynoteInfo keynoteInfo)
        {
            keynoteItem = keynoteInfo;
            
        }

        public void Initialize()
        {
            for (int i = 0; i < childrenItems.Count; i++)
            {
                childrenItems[i].parentItem = this;
                childrenItems[i].Initialize();
            }
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
