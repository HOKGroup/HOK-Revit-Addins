using System;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.Core.Utilities;

namespace HOK.SmartBCF
{
    [Transaction(TransactionMode.Manual)]
    public class BCFCommand : IExternalCommand
    {
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
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

        /// <summary>
        /// Verifies that active view is 3D View type.
        /// </summary>
        /// <returns>True if active view is 3D type or view exists in the model, otherwise false.</returns>
        private bool IsValidView()
        {
            try
            {
                var view3d = m_doc.ActiveView as View3D;
                if (view3d != null) return true;

                // (Konrad) Even though the current active view is not a 3D View, if 3D View exists in the model
                // we can still set it to active when External Event Handler is initialized.
                var v = new FilteredElementCollector(m_doc)
                    .OfClass(typeof(View3D))
                    .WhereElementIsNotElementType()
                    .Cast<View3D>()
                    .FirstOrDefault(x => !x.IsTemplate && !x.IsPerspective && x.Name.Contains("{3D}"));
                
                if (v != null) return true;

                Log.AppendLog(LogMessageType.ERROR, "Invalid view. Could not find any 3D Views in the model.");
                MessageBox.Show(
                    "Please open a 3d view to navigate elements associated with BCF before running smartBCF.",
                    "Background 3D View", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                MessageBox.Show("Faild to find valid 3d view to navigate.\n" + e.Message, "Find Background 3D View",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return false;
        }
    }
}
