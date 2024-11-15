using System.ComponentModel;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.StylesManager.Utilities
{
    public sealed class DimensionTypeWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ElementId Id { get; set; }
        public string Type { get; set; }
        public int Count { get; set; } = 0;
        public bool? IsUsingProjectUnits { get; set; }
        public string Size { get; set; }

        public DimensionTypeWrapper()
        {
        }

        public DimensionTypeWrapper(DimensionType dt)
        {
            Name = dt.Name;
            Id = dt.Id;
            Type = dt.StyleType.ToString();
            IsUsingProjectUnits = null;
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; RaisePropertyChanged("IsEnabled"); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("IsSelected"); }
        }

        public override bool Equals(object obj)
        {
            var item = obj as DimensionTypeWrapper;
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
