using HOK.Keynote.ClassModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HOK.KeynoteEditor.Utils
{
    public class KeynoteSetFilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            KeynoteSetInfo setInfo = new KeynoteSetInfo();
            if (values.Length == 2)
            {
                ObservableCollection<KeynoteSetInfo> setList = values[0] as ObservableCollection<KeynoteSetInfo>;
                KeynoteProjectInfo projectInfo = values[1] as KeynoteProjectInfo;
                if (null != setList && null != projectInfo)
                {
                    var setInfoFound = from set in setList where projectInfo.keynoteSet_id == set._id select set;
                    if (setInfoFound.Count() > 0)
                    {
                        setInfo = setInfoFound.First();
                    }
                }
            }
            return setInfo.name;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
