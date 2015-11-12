#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace HOK.SheetDataManager
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        private UIApplication m_app = null;
        private Document m_doc = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                MainWindow mainWindow = new MainWindow(m_app);
                if (mainWindow.ShowDialog() == true)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to excute Sheet Data Manager.\n"+ex.Message, "Sheet Data Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }
    }
}
