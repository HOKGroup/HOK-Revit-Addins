using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementFlatter.Class
{
    public class CategoryInfo:INotifyPropertyChanged
    {
        private BuiltInCategory bltCategory = BuiltInCategory.INVALID;
        private string name = "";
        private ElementId categoryId = ElementId.InvalidElementId;
        private List<ElementId> elementIds = new List<ElementId>();
        private List<DirectShapeInfo> createdShapes = new List<DirectShapeInfo>();
        private bool isChecked = false;

        public BuiltInCategory BltCategory { get { return bltCategory; } set { bltCategory = value; } }
        public string Name { get { return name; } set { name = value; } }
        public ElementId CategoryId { get { return categoryId; } set { categoryId = value; } }
        public List<ElementId> ElementIds { get { return elementIds; } set { elementIds = value; } }
        public List<DirectShapeInfo> CreatedShapes { get { return createdShapes; } set { createdShapes = value; } }
        public bool IsChecked { get { return isChecked; } set { isChecked = value; } }

        public CategoryInfo(Category category)
        {
            name = category.Name;
            categoryId = category.Id;
            bltCategory = (BuiltInCategory)categoryId.IntegerValue;
        }

        public List<ElementId> GetElementIds(Document doc)
        {
            List<ElementId> eIds = new List<ElementId>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ElementCategoryFilter catFilter = new ElementCategoryFilter(bltCategory);
                ElementClassFilter dsFilter = new ElementClassFilter(typeof(DirectShape), true);
                LogicalAndFilter andFilter = new LogicalAndFilter(catFilter, dsFilter);
                eIds = collector.WherePasses(andFilter).WhereElementIsNotElementType().ToElementIds().ToList();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return eIds;
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
}
