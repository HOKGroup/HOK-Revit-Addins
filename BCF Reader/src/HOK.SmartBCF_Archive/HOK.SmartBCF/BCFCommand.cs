using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.SmartBCF
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class BCFCommand:IExternalCommand
    {
        private Document m_doc = null;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_doc = commandData.Application.ActiveUIDocument.Document;
                if (IsValidView())
                {
                    AppCommand.thisApp.ShowWalker(commandData.Application);
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private bool IsValidView()
        {
            bool result = false;
            try
            {
                View3D view3d = m_doc.ActiveView as View3D;
                if (null != view3d)
                {
                    return true;
                }

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<View3D> view3ds = collector.OfClass(typeof(View3D)).WhereElementIsNotElementType().ToElements().Cast<View3D>().ToList();
                var viewfound = from view in view3ds where view.IsTemplate==false && view.IsPerspective == false && view.ViewName == "{3D}" select view;
                if (viewfound.Count() > 0)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Please open a 3d view to navigate elements associated with BCF before running smartBCF.", "Background 3D View", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Faild to find valid 3d view to navigate.\n"+ex.Message, "Find Background 3D View", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }
    }
}
