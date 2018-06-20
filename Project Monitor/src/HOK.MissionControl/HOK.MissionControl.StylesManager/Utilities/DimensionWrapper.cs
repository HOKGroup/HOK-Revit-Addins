using System.ComponentModel;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.StylesManager.Utilities
{
    public sealed class DimensionWrapper : INotifyPropertyChanged
    {
        public ElementId DimensionId { get; set; }
        public int Hash { get; set; }
        public ElementId OwnerViewId { get; set; }
        public string OwnerViewType { get; set; }
        public double? Value { get; set; }
        public string ValueString { get; set; }
        public string ValueOverride { get; set; }
        public bool? IsValueOverrideHuge { get; set; }
        public bool IsFiltered { get; set; }

        public DimensionWrapper()
        {
        }

        public DimensionWrapper(Dimension d)
        {
            Hash = d.GetHashCode();
            Value = d.Value;
            ValueOverride = d.ValueOverride;
        }

        public DimensionWrapper(DimensionSegment ds)
        {
            Hash = ds.GetHashCode();
            Value = ds.Value;
            ValueOverride = ds.ValueOverride;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("IsSelected"); }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; RaisePropertyChanged("IsVisible"); }
        }

        public override bool Equals(object obj)
        {
            var item = obj as DimensionWrapper;
            return item != null && Hash.Equals(item.Hash);
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
