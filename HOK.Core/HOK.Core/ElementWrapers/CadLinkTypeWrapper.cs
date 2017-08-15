using Autodesk.Revit.DB;
using System.ComponentModel;

namespace HOK.Core.ElementWrapers
{
    public sealed class CadLinkTypeWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Instances { get; set; }
        public ElementId Id { get; set; }
        public bool IsLinked { get; set; }
        public bool IsViewSpecific { get; set; }

        public CadLinkTypeWrapper()
        {
        }

        public CadLinkTypeWrapper(CADLinkType type)
        {
            Name = type.Name;
            Id = type.Id;
        }

        private bool _hasInstances;
        public bool HasInstances
        {
            get
            {
                _hasInstances = Instances > 0;
                return _hasInstances;
            }
            set { _hasInstances = value; RaisePropertyChanged("HasInstances"); }
        }

        public override bool Equals(object obj)
        {
            var item = obj as CadLinkTypeWrapper;
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
