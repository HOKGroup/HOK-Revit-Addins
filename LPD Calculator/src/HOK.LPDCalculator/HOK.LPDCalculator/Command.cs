using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;


namespace HOK.LPDCalculator
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class Command:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;

            string docPath = GetCentralPath(m_doc);
            if (!string.IsNullOrEmpty(docPath))
            {
                CommandForm commandForm = new CommandForm(m_app);
                commandForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please save the current Revit project before running the LPD Analysis.", "File Not Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return Result.Succeeded;
        }

        private string GetCentralPath(Document doc)
        {
            string docCentralPath = "";
            try
            {
                if (doc.IsWorkshared)
                {
                    ModelPath modelPath = doc.GetWorksharingCentralModelPath();
                    string centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        docCentralPath = centralPath;
                    }
                    else
                    {
                        //detached
                        docCentralPath = doc.PathName;
                    }
                }
                else
                {
                    docCentralPath = doc.PathName;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                docCentralPath = doc.PathName;
            }
            return docCentralPath;
        }

    }
}
