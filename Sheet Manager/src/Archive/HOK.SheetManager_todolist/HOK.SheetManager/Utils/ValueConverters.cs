using HOK.SheetManager.Classes;
using HOK.SheetManager.Windows.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace HOK.SheetManager.Utils
{
    public class ProjectFilterConverter : IMultiValueConverter
    {
        private ObservableCollection<RevitProject> projects = new ObservableCollection<RevitProject>();
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                int count = int.Parse(values[0].ToString());
                ObservableCollection<RevitProject> items = values[1] as ObservableCollection<RevitProject>;

                if (null != items)
                {
                    var distinctItems = items.GroupBy(item => item.ProjectNumber).Select(group => group.First());
                    if (distinctItems.Count() > 0)
                    {
                        projects = new ObservableCollection<RevitProject>(distinctItems.ToList());
                    }
                }
            }
            return projects;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileFilterConverter : IMultiValueConverter
    {
        private ObservableCollection<RevitProject> projects = new ObservableCollection<RevitProject>();
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                RevitProject selectedProject = values[0] as RevitProject;
                ObservableCollection<RevitProject> items = values[1] as ObservableCollection<RevitProject>;

                if (null != selectedProject && null != items)
                {
                    var filteredItems = from item in items where item.ProjectNumber == selectedProject.ProjectNumber select item;
                    if (filteredItems.Count() > 0)
                    {
                        projects = new ObservableCollection<RevitProject>(filteredItems.ToList());
                    }
                }
            }
            return projects;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DisciplineFilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                ObservableCollection<RevitSheet> sheets = values[0] as ObservableCollection<RevitSheet>;
                Discipline selectedDiscipline = values[1] as Discipline;
                if (null != sheets && null != selectedDiscipline)
                {
                    var selectedSheets = from sheet in sheets where sheet.DisciplineObj.Id == selectedDiscipline.Id select sheet;
                    return new ObservableCollection<RevitSheet>(selectedSheets.OrderBy(o => o.Number).ToList());
                }
                else
                {
                    return new ObservableCollection<RevitSheet>();
                }
            }
            else
            {
                return new ObservableCollection<RevitSheet>();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<RevitView> filteredViews = new ObservableCollection<RevitView>();
            if (values.Length == 2)
            {
                ObservableCollection<RevitView> views = values[0] as ObservableCollection<RevitView>;
                RevitSheet selectedSheet = values[1] as RevitSheet;

                if (null != views && null != selectedSheet)
                {
                    var selectedViews = from view in views where view.Sheet.Id == selectedSheet.Id select view;
                    filteredViews = new ObservableCollection<RevitView>(selectedViews.OrderBy(o => o.Name).ToList());
                }
            }
            return filteredViews;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class RevisionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<RevitRevision> filteredRevisions = new ObservableCollection<RevitRevision>();
            if (values.Length == 2)
            {
                ObservableCollection<RevitRevision> revisions = values[0] as ObservableCollection<RevitRevision>;
                RevitSheet selectedSheet = values[1] as RevitSheet;

                if (null != revisions && null != selectedSheet)
                {
                    var includeRevisionIds = from ros in selectedSheet.SheetRevisions.Values where ros.Include select ros.RvtRevision.Id;
                    if (includeRevisionIds.Count() > 0)
                    {
                        var revisionFound = from rev in revisions where includeRevisionIds.Contains(rev.Id) select rev;
                        if (revisionFound.Count() > 0)
                        {
                            filteredRevisions = new ObservableCollection<RevitRevision>(revisionFound.ToList());
                        }
                    }
                }

            }
            return filteredRevisions;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RevisionIncludeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<RevisionOnSheet> rosCollection = new ObservableCollection<RevisionOnSheet>();
            if (null != value)
            {
                Dictionary<Guid, RevisionOnSheet>.ValueCollection valueCollection = value as Dictionary<Guid, RevisionOnSheet>.ValueCollection;
                if (null != valueCollection)
                {
                    var includedRevision = from ros in valueCollection where ros.Include select ros;
                    if (includedRevision.Count() > 0)
                    {
                        rosCollection = new ObservableCollection<RevisionOnSheet>(includedRevision.ToList());
                    }
                }
            }
            return rosCollection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LinkedSheetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<LinkedSheet> linkedSheets = new ObservableCollection<LinkedSheet>();
            if (null != value)
            {
                IList<DataGridCellInfo> cellInfoList = value as IList<DataGridCellInfo>;
                if (null != cellInfoList)
                {
                    if (cellInfoList.Count > 0)
                    {
                        DataGridCellInfo cellInfo = cellInfoList.First();
                        RevitSheet selectedSheet = cellInfo.Item as RevitSheet;
                        if (null != selectedSheet)
                        {
                            linkedSheets = selectedSheet.LinkedSheets;
                        }
                    }
                }
            }
            return linkedSheets;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SheetNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string sheetNumberName = "";
            if (values.Length == 2)
            {
                string sheetNumber = values[0].ToString();
                string sheetName = values[1].ToString();

                sheetNumberName = sheetNumber + " - " + sheetName;
            }
            return sheetNumberName;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemMapperConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<RevitItemMapper> filteredItems = new ObservableCollection<RevitItemMapper>();
            if (values.Length == 4)
            {
                ObservableCollection<RevitItemMapper> items = values[0] as ObservableCollection<RevitItemMapper>;
                ItemMap selectedItemType = values[1] as ItemMap;
                object selectedParameter = values[2];

                if (null != items && null != selectedItemType && null != selectedParameter)
                {
                    var itemFound = from item in items where item.ItemType == selectedItemType.ItemMapType && item.ParameterName == selectedParameter.ToString() select item;
                    if (itemFound.Count() > 0)
                    {
                        filteredItems = new ObservableCollection<RevitItemMapper>(itemFound.ToList());
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
