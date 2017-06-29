using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace HOK.WorksetView
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    class WorksetCommand : IExternalCommand
    {
        private UIApplication m_app;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;

            var mainWindow = new ViewCreatorWindow(m_app);
            var showDialog = mainWindow.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                mainWindow.Close();
            }
            return Result.Succeeded;
        }
    }
}
