using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SheetManager.AddIn.Updaters;
using HOK.SheetManager.AddIn.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.AddIn
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        private UIApplication m_app = null;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                AppCommand.thisApp.ShowWindow(m_app);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
            }
            return Result.Succeeded;
        }
    }
}
