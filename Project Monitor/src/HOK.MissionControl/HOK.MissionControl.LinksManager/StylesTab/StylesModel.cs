using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.ElementWrapers;
using HOK.Core.Utilities;

namespace HOK.MissionControl.LinksManager.StylesTab
{
    public class StylesModel
    {
        private readonly Document _doc;
        public ObservableCollection<CategoryWrapper> Styles { get; set; }

        public StylesModel(Document doc)
        {
            _doc = doc;
            CollectStyles();
        }

        public List<CategoryWrapper> Delete(ObservableCollection<CategoryWrapper> styles)
        {
            var deleted = new List<CategoryWrapper>();
            using (var trans = new Transaction(_doc, "Delete Styles"))
            {
                trans.Start();

                foreach (var style in styles)
                {
                    if (!style.IsSelected) continue;
                    try
                    {
                        _doc.Delete(style.Id);
                        deleted.Add(style);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }

                trans.Commit();
            }

            return deleted;
        }

        private void CollectStyles()
        {
            var wrappers = new ObservableCollection<CategoryWrapper>();

            var allStyles = new FilteredElementCollector(_doc)
                .OfClass(typeof(GraphicsStyle))
                .Cast<GraphicsStyle>()
                .ToList();

            if (!allStyles.Any()) return;

            // TODO: How to get all SubCategories here?
            //foreach (var style in allStyles)
            //{
            //    if (style.GraphicsStyleCategory.Name.Contains(".dwg"))
            //    {
            //        wrappers.Add(new CategoryWrapper(style.GraphicsStyleCategory));
            //    }
            //    else if (style.Name == "Imports in Families")
            //    {
            //        foreach (var subCategory in style.GraphicsStyleCategory.SubCategories)
            //        {
            //            wrappers.Add(new CategoryWrapper(subCategory));
            //        }
                    
            //    }
            //}

            Styles = new ObservableCollection<CategoryWrapper>(wrappers.OrderBy(x => x.Name));
        }
    }
}
