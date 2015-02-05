using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace HOK.RoomElevation
{
    public class TreeviewModel:INotifyPropertyChanged
    {
        private string name = "";
        private object tag = null;
        private string toolTip = "";
        private RoomElevationProperties roomProperties = null;
        private ElevationViewProperties viewProperties = null;
        private List<TreeviewModel> children = new List<TreeviewModel>();
        private bool isInitiallySelected = false;
        private bool? ischecked = false;
        private bool isEnabled = true;
        private TreeviewModel parent = null;
        private Brush textColor = new SolidColorBrush(Colors.Black);

        public string Name { get { return name; } set { name = value; } }
        public object Tag { get { return tag; } set { tag = value; } }
        public string ToolTip { get { return toolTip; } set { toolTip = value; } }
        public RoomElevationProperties RoomProperties { get { return roomProperties; } set { roomProperties = value; } }
        public ElevationViewProperties ViewProperties { get { return viewProperties; } set { viewProperties = value; } }
        public List<TreeviewModel> Children { get { return children; } set { children = value; } }
        public bool IsInitiallySelected { get { return isInitiallySelected; } set { isInitiallySelected = value; } }
        public bool? _isChecked { get { return ischecked; } set { ischecked = value; } }
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; } }
        public TreeviewModel _parent { get { return parent; } set { parent = value; } }

        public Brush TextColor { get { return textColor; } set { textColor = value; } }

        public TreeviewModel(RoomElevationProperties rep)
        {
            RoomProperties = rep;
            ViewProperties = null;
            string roomNumber = rep.RoomNumber;
            string roomName = rep.RoomName;
            
            Name = roomNumber +" - "+roomName;
            Tag = rep;
            ToolTip = "Element ID:" + rep.RoomId.ToString();

            if (rep.ElevationViews.Count > 0)
            {
                TextColor = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                TextColor = new SolidColorBrush(Colors.Black);
            }
        }

        public TreeviewModel(ElevationViewProperties evp)
        {
            RoomProperties = null;
            ViewProperties = evp;
            string viewName = evp.ViewName;
            Name = viewName;
            Tag = evp;
            ToolTip = "Element ID:" + evp.ViewId.ToString();
            TextColor = new SolidColorBrush(Colors.Gray);
            IsEnabled = false;
        }

        public static List<TreeviewModel> SetTreeView(Dictionary<int, RoomElevationProperties> dictionary, bool isLinkedRoom)
        {
            List<TreeviewModel> treeView = new List<TreeviewModel>();
            try
            {
                foreach (RoomElevationProperties rep in dictionary.Values)
                {
                    if (rep.IsLinked == isLinkedRoom)
                    {
                        TreeviewModel roomItem = new TreeviewModel(rep);
                        treeView.Add(roomItem);

                        List<TreeviewModel> childrenList = new List<TreeviewModel>();
                        foreach (int markId in rep.ElevationViews.Keys)
                        {
                            foreach(ElevationViewProperties evp in rep.ElevationViews[markId].Values)
                            {
                                TreeviewModel viewItem = new TreeviewModel(evp);
                                childrenList.Add(viewItem);
                            }
                        }
                        
                        childrenList = childrenList.OrderBy(child => child.name).ToList();
                        roomItem.Children = childrenList;
                        roomItem.Initialize();
                    }
                }
                treeView = treeView.OrderBy(item => item.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create the tree view for the rooms.\n"+ex.Message, "Room Elevation Creator", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return treeView;
        }

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked) return;
            _isChecked = value;
            if (updateChildren && _isChecked.HasValue) { Children.ForEach(c => c.SetIsChecked(_isChecked, true, false)); }
            if (updateParent && _parent != null) { _parent.VerifyCheckedState(); }
            NotifyPropertyChanged("IsChecked");
        }

        public void VerifyCheckedState()
        {
            bool? state = null;

            for (int i = 0; i < Children.Count; ++i)
            {
                bool? current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }

            SetIsChecked(state, false, true);
        }

        public void Initialize()
        {
            foreach (TreeviewModel child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
