using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.ElementFlatter.Class
{
    public class CategoryInfo : INotifyPropertyChanged
    {
        public BuiltInCategory BltCategory { get; set; }
        public string Name { get; set; }
        public ElementId CategoryId { get; set; }
        public List<ElementId> ElementIds { get; set; } = new List<ElementId>();
        public List<DirectShapeInfo> CreatedShapes { get; set; } = new List<DirectShapeInfo>();

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; NotifyPropertyChanged("IsChecked"); }
        }

        public CategoryInfo(Category category)
        {
            Name = category.Name;
            CategoryId = category.Id;
            BltCategory = (BuiltInCategory)CategoryId.IntegerValue;
        }

        /// <summary>
        /// Retrieves all Elements associated with that Category except DirectShapes.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public List<ElementId> GetElementIds(Document doc)
        {
            var eIds = new List<ElementId>();
            try
            {
                var collector = new FilteredElementCollector(doc);
                var catFilter = new ElementCategoryFilter(BltCategory);
                var dsFilter = new ElementClassFilter(typeof(DirectShape), true);
                var andFilter = new LogicalAndFilter(catFilter, dsFilter);
                eIds = collector.WherePasses(andFilter).WhereElementIsNotElementType().ToElementIds().ToList();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return eIds;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
