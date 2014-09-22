using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using HOK.Utilities.Arrowhead;
using Autodesk.Revit.DB;

namespace HOK.Utilities
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class ArrowCommand:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;

            HeadAssignerWindow assignerWindow = new HeadAssignerWindow(m_app);
            if (assignerWindow.ShowDialog()==true)
            {
                assignerWindow.Close();
            }

            return Result.Succeeded;
        }
    }
}
