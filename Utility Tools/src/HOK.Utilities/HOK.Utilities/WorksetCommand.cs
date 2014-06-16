using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HOK.Utilities.WorksetView;

namespace HOK.Utilities
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    class WorksetCommand:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;

            ViewCreatorForm worksetForm = new ViewCreatorForm(m_app);
            if (worksetForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                worksetForm.Close();
            }

            return Result.Succeeded;
        }
    }
}
