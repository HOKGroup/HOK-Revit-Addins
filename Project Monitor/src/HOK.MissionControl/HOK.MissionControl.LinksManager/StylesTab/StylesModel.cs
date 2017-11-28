using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.ElementWrapers;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;

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

        /// <summary>
        /// Deletes selected styles from the model.
        /// </summary>
        /// <param name="styles">List of styles to delete.</param>
        /// <returns>List of deleted styles.</returns>
        public List<CategoryWrapper> Delete(List<CategoryWrapper> styles)
        {
            var deleted = new List<CategoryWrapper>();
            using (var trans = new Transaction(_doc, "Delete Styles"))
            {
                trans.Start();
                StatusBarManager.InitializeProgress("Deleting Styles:", styles.Count);

                foreach (var style in styles)
                {
                    StatusBarManager.StepForward();
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

                StatusBarManager.FinalizeProgress();
                trans.Commit();
            }

            return deleted;
        }

        /// <summary>
        /// Collects all of the imported styles in the model.
        /// </summary>
        private void CollectStyles()
        {
            var wrappers = new ObservableCollection<CategoryWrapper>();

            var allStyles = new FilteredElementCollector(_doc)
                .OfClass(typeof(GraphicsStyle))
                .Cast<GraphicsStyle>()
                .ToList();

            if (!allStyles.Any()) return;

            foreach (var style in allStyles)
            {
                if (style.GraphicsStyleCategory.Name.Contains(".dwg"))
                {
                    var subCats = style.GraphicsStyleCategory.SubCategories;
                    foreach (Category subCat in subCats)
                    {
                        wrappers.Add(new CategoryWrapper(subCat){ ParentName = style.GraphicsStyleCategory.Name });
                    }
                }
                else if (style.Name == "Imports in Families")
                {
                    var subCats = style.GraphicsStyleCategory.SubCategories;
                    foreach (Category subCat in subCats)
                    {
                        wrappers.Add(new CategoryWrapper(subCat));
                    }
                }
            }

            Styles = new ObservableCollection<CategoryWrapper>(wrappers.OrderBy(x => x.Name));
        }
    }
}
