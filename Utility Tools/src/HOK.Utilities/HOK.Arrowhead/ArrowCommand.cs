using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace HOK.Arrowhead
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ArrowCommand : IExternalCommand
    {
        private UIApplication m_app;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;

            var assignerWindow = new HeadAssignerWindow(m_app);
            if (assignerWindow.ShowDialog() == true)
            {
                assignerWindow.Close();
            }

            return Result.Succeeded;
        }
    }
}
