using System;
using System.Windows.Data;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Schemas.Families;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class TypeToResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value?.GetType() == typeof(SheetTask))
            {
                return "../../../Resources/sheetTask_24x24.png";
            }
            if (value?.GetType() == typeof(FamilyTask))
            {
                return "../../../Resources/familyTask_24x24.png";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
