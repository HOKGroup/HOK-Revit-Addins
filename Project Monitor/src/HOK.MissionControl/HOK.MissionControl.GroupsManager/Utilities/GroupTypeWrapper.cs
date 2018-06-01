using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.GroupsManager.Utilities
{
    public sealed class GroupTypeWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public ElementId Id { get; set; }
        public int MemberCount { get; set; }
        public List<ElementId> Instances { get; set; } = new List<ElementId>();
        public bool IsArray { get; set; }

        public GroupTypeWrapper()
        {
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("IsSelected"); }
        }

        public override bool Equals(object obj)
        {
            var item = obj as GroupTypeWrapper;
            return item != null && Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
