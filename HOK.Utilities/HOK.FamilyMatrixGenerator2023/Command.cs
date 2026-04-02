#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace HOK.FamilyMatrixGenerator2023
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var m_dlg = new Forms.FamilyMatrixGenForm(commandData.Application.ActiveUIDocument.Document);
                m_dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Failed To Load Addin", ex.Message);
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
