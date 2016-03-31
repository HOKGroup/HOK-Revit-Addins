using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.ElementFlatter.Class;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.ElementFlatter
{
    [Transaction(TransactionMode.Manual)]
    public class Command:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;

            CommandViewModel viewModel = new CommandViewModel(m_app);

            CommandWindow cmdWindow = new CommandWindow();
            cmdWindow.DataContext = viewModel;
            if ((bool)cmdWindow.ShowDialog())
            {
            }

            return Result.Succeeded;
        }

       
    }
}
