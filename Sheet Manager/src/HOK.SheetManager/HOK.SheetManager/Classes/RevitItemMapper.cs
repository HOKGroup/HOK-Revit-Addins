using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Classes
{
    public class RevitItemMapper : INotifyPropertyChanged
    {
        private Guid itemId = Guid.Empty;
        private MappingType itemType = MappingType.None;
        private string parameterName = "";
        private Guid sourceId = Guid.Empty;
        private string sourceValue = "";
        private string targetValue = "";
        private bool isSelected = false;

        public Guid ItemId { get { return itemId; } set { itemId = value; NotifyPropertyChanged("ItemId"); } }
        public MappingType ItemType { get { return itemType; } set { itemType = value; NotifyPropertyChanged("ItemType"); } }
        public string ParameterName { get { return parameterName; } set { parameterName = value; NotifyPropertyChanged("ParameterName"); } }
        public Guid SourceId { get { return sourceId; } set { sourceId = value; NotifyPropertyChanged("SourceId"); } }
        public string SourceValue { get { return sourceValue; } set { sourceValue = value; NotifyPropertyChanged("SourceValue"); } }
        public string TargetValue { get { return targetValue; } set { targetValue = value; NotifyPropertyChanged("TargetValue"); } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public RevitItemMapper()
        {
        }

        public RevitItemMapper(Guid id, MappingType mapType, string param, Guid s_Id, string sVal, string tVal)
        {
            itemId = id;
            itemType = mapType;
            parameterName = param;
            sourceId = s_Id;
            sourceValue = sVal;
            targetValue = tVal;
        }

        public RevitItemMapper(Guid id, MappingType mapType, string param, string sVal, string tVal)
        {
            itemId = id;
            itemType = mapType;
            parameterName = param;
            sourceValue = sVal;
            targetValue = tVal;
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

    public enum MappingType
    {
        Sheet, View, None
    }
}
