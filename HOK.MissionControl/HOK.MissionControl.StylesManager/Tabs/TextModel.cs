using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.MissionControl.StylesManager.Utilities;

namespace HOK.MissionControl.StylesManager.Tabs
{
    public class TextModel
    {
        private Document _doc { get; set; }

        public TextModel(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<TextStyleWrapper> CollectTextStyles()
        {
            var textTypes = new FilteredElementCollector(_doc)
                .OfClass(typeof(TextNoteType))
                .ToDictionary(x => x.Id, x => new TextStyleWrapper(x));

            var textInstances = new FilteredElementCollector(_doc)
                .OfClass(typeof(TextNote))
                .WhereElementIsNotElementType();

            foreach (var t in textInstances)
            {
                var key = t.GetTypeId();
                if (textTypes.ContainsKey(key))
                {
                    // increment instance count
                    textTypes[key].Count = textTypes[key].Count + 1;
                }
            }

            return new ObservableCollection<TextStyleWrapper>(textTypes.Values);
        }
    }
}
