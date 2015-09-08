using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.ElementMover
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class MoverCommand:IExternalCommand
    {
        private UIApplication m_app = null;
        private Document m_doc = null;

        private Dictionary<ElementId, LinkedInstanceProperties> linkInstances = new Dictionary<ElementId, LinkedInstanceProperties>();

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
           
            AppCommand.thisApp.ShowMover(m_app);

            return Result.Succeeded;
        }

    }
}
