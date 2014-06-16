using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HOK.ColorSchemeEditor.BCFUtils;
using System.Windows;


namespace HOK.ColorSchemeEditor
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class Command:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                BCFUtil bcfUtil = new BCFUtil(m_app);
                bcfUtil.CleanLocalFolder();

                MainWindow mainWindow = new MainWindow(m_app);
                if (true == mainWindow.ShowDialog())
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize the Color Scheme Editor.\n"+ex.Message, "Color Scheme Editor", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }
    }
}
