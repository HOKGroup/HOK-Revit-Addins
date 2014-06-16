using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using HOK.Utilities.LevelManager;
using Autodesk.Revit.DB;

namespace HOK.Utilities
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class LevelCommand : IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;

                LevelManagerForm managerForm = new LevelManagerForm(m_app);
                managerForm.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not run Level Manager.\n" + ex.Message, "Error: Level Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Result.Cancelled;
            }
        }
    }
}
