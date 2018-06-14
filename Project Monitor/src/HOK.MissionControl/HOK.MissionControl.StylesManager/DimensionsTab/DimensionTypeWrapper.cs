using System.ComponentModel;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.StylesManager.DimensionsTab
{
    public sealed class DimensionTypeWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ElementId Id { get; set; }
        public object Self { get; set; }

        public DimensionTypeWrapper()
        {
        }

        public DimensionTypeWrapper(DimensionType type)
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
