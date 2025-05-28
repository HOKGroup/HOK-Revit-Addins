#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.ProjectSheetManager.Classes;
using HOK.ProjectSheetManager.Forms;
#endregion

namespace HOK.ProjectSheetManager
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            var m_Settings = new Classes.Settings(commandData);

            try
            {
                var m_dlg = new ProjectSheetManagerForm(m_Settings);
                m_dlg.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Failed To Load", message);
                return Result.Failed;
            }
        }
    }
}
