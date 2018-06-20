using System.ComponentModel;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.StylesManager.Utilities
{
    public sealed class TextStyleWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ElementId Id { get; set; }
        public int Count { get; set; } = 0;
        public string Font { get; set; }
        public double Size { get; set; }
        public string SizeString { get; set; }

        public TextStyleWrapper()
        {
        }

        public TextStyleWrapper(Element tnt)
        {
            Name = tnt.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
            Id = tnt.Id;
            Font = tnt.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
            Size = tnt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
            SizeString = tnt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
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
            var item = obj as TextStyleWrapper;
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
