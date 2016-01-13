using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HOK.SmartBCF.AddIn.Util
{
    [ValueConversion(typeof(ObservableCollection<RevitComponent>), typeof(ObservableCollection<RevitComponent>))]
    public class ComponentConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<RevitComponent> selectedComponents = new ObservableCollection<RevitComponent>();
            if (null != value)
            {
                ObservableCollection<RevitComponent> components = value as ObservableCollection<RevitComponent>;
                var selected = from comp in components where comp.Category.Selected select comp;
                if (selected.Count() > 0)
                {
                    selectedComponents = new ObservableCollection<RevitComponent>(selected.ToList());
                }
            }
            return selectedComponents;
            
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
