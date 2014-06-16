using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows;

namespace HOK.ModelManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class ModelCommand:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                if (m_app.Application.Documents.Size > 1)
                {
                    ManagerWindow managerWindow = new ManagerWindow(m_app, ModelManagerMode.ModelBuilder);
                    managerWindow.Show();
                }
                else
                {
                    MessageBox.Show("Please open more than two Revit documents before running this tool.\n A source model and a recipient model are required.", "Opened Revit Documents Required!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize Model Builder.\n" + ex.Message, "Model Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Result.Cancelled;
            }
           
        }
    }
}
