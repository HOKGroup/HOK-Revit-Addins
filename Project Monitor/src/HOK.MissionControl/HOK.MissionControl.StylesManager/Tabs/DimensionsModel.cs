using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.MissionControl.StylesManager.Utilities;

namespace HOK.MissionControl.StylesManager.Tabs
{
    public class DimensionsModel
    {
        private Document _doc { get; set; }

        public DimensionsModel(Document doc)
        {
            _doc = doc;
        }

        public ObservableCollection<DimensionTypeWrapper> CollectDimensions()
        {
            var dTypes = new FilteredElementCollector(_doc)
                    .OfClass(typeof(DimensionType))
                    .Cast<DimensionType>()
                    .Where(x => !string.IsNullOrEmpty(x.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString()))
                    .ToDictionary(x => x.Id, x => new DimensionTypeWrapper(x));

            var dInstances = new FilteredElementCollector(_doc)
                .OfClass(typeof(Dimension))
                .WhereElementIsNotElementType()
                .Cast<Dimension>();

            foreach (var d in dInstances)
            {
                var key = d.GetTypeId();
                if (dTypes.ContainsKey(key))
                {
                    dTypes[key].Count = dTypes[key].Count + 1; // increment
                }
            }

            return new ObservableCollection<DimensionTypeWrapper>(dTypes.Values.OrderBy(x => x.Name));
        }
    }
}
