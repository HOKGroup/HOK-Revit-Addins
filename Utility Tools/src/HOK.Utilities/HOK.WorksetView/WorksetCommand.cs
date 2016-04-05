using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace HOK.WorksetView
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    class WorksetCommand : IExternalCommand
    {
        private UIApplication m_app = null;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;

            ViewCreatorWindow mainWindow = new ViewCreatorWindow(m_app);
            if ((bool)mainWindow.ShowDialog())
            {
                mainWindow.Close();
            }
            return Result.Succeeded;
        }
    }
}
