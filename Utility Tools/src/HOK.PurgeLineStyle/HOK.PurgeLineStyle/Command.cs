using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.PurgeLineStyle
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class Command:IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<Element> graphicsStyles = collector.OfClass(typeof(GraphicsStyle)).ToElements().ToList();

            List<ElementId> elementsToDelete = new List<ElementId>();
            foreach (Element element in graphicsStyles)
            {
                GraphicsStyle gs = element as GraphicsStyle;
                if (null != gs.GraphicsStyleCategory)
                {
                    if (null != gs.GraphicsStyleCategory.Parent)
                    {
                        if (gs.GraphicsStyleCategory.Parent.Id.IntegerValue == (int)BuiltInCategory.OST_Lines)
                        {
                            Category category = gs.GraphicsStyleCategory;
                            if (category.Name.StartsWith("SW"))
                            {
                                elementsToDelete.Add(category.Id);
                            }
                        }
                    }
                }
            }
            int count = elementsToDelete.Count;
            if (count > 0)
            {
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Delete Line Styles");
                    try
                    {
                        ICollection<ElementId> deletedIds = doc.Delete(elementsToDelete);
                        trans.Commit();
                        if (deletedIds.Count > 0)
                        {
                            int deletedCount = deletedIds.Count;
                            MessageBox.Show(deletedCount.ToString() + " line styles are successfully deleted.", "Purge Line Styles");
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionMsg = ex.Message;
                        trans.RollBack();
                    }
                }
            }
            

            return Result.Succeeded;
        }
    }
}
