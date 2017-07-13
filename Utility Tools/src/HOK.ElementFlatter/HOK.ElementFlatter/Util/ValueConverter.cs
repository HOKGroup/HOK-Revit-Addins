//using HOK.ElementFlatter.Class;
//using System;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Windows.Data;

//namespace HOK.ElementFlatter.Util
//{
//    public class CategoryFilterConverter : IValueConverter
//    {
//        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
//        {
//            var categoryWithElements = new ObservableCollection<CategoryInfo>();
//            if (value != null)
//            {
//                var categories = value as ObservableCollection<CategoryInfo>;
//                if (null != categories)
//                {
//                    var selectedCategories = from cat in categories where cat.ElementIds.Count > 0 select cat;
//                    if (selectedCategories.Any())
//                    {
//                        categoryWithElements = new ObservableCollection<CategoryInfo>(selectedCategories.ToList());
//                    }
//                }
//            }
//            return categoryWithElements;
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
