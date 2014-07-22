using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using HOK.RoomsToMass.ParameterAssigner;
using Autodesk.Revit.UI.Events;

namespace HOK.RoomsToMass
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class AssignerCommand:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;
               
                
                Form_LinkedFiles linkedFilesForm = new Form_LinkedFiles(m_app);
                if (linkedFilesForm.ShowDialog() == DialogResult.OK)
                {
                    Form_Assigner assignForm = new Form_Assigner(m_app);
                    assignForm.WorksetDictionary = linkedFilesForm.WorksetDictionary;
                    assignForm.IntegratedMassList = linkedFilesForm.IntegratedMassList;
                    assignForm.LinkedMassDictionary = linkedFilesForm.LinkedMassDictionary;
                    assignForm.ElementDictionary = linkedFilesForm.ElementDictionary;
                    assignForm.ElementCategories = linkedFilesForm.ElementCategories;
                    assignForm.MassParameters = linkedFilesForm.MassParameters;
                    assignForm.SelectedSourceType = linkedFilesForm.SelectedSourceType;
                    assignForm.ParameterMaps = linkedFilesForm.ParameterMaps;

                    linkedFilesForm.Close();

                    if (assignForm.ShowDialog() == DialogResult.OK)
                    {
                        assignForm.Close();

                    }
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
