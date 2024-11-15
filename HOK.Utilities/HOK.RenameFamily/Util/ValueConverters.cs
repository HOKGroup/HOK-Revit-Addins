using HOK.RenameFamily.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HOK.RenameFamily.Util
{
    public class FamilyTypeFilter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<FamilyTypeProperties> filteredItems = new ObservableCollection<FamilyTypeProperties>();
            if (values.Length == 3)
            {
                if (null != values[0] && null != values[1] && null != values[2])
                {
                    string modelName = values[0].ToString();
                    string categoryName = values[1].ToString();
                    ObservableCollection<FamilyTypeProperties> items = values[2] as ObservableCollection<FamilyTypeProperties>;

                    if (!string.IsNullOrEmpty(modelName) && !string.IsNullOrEmpty(categoryName) && null != items)
                    {
                        var itemFound = from item in items where item.ModelName == modelName && item.CategoryName == categoryName select item;
                        if (itemFound.Count() > 0)
                        {
                            filteredItems = new ObservableCollection<FamilyTypeProperties>(itemFound.OrderBy(o => o.FamilyName).ToList());
                        }
                    }
                }
            }
            return filteredItems;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
