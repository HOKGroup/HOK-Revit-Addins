using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using HOK.RoomsToMass.DataTransfer;

namespace HOK.RoomsToMass
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class DataCommand:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                Form_DataTransfer dataTransferForm = new Form_DataTransfer(m_app);
                if (dataTransferForm.ShowDialog() == DialogResult.OK)
                {
                    dataTransferForm.Close();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: cannot start Transfer Data.\n" + ex.Message, "Transfer Data");
                return Result.Failed;
            }
        }
    }
}
