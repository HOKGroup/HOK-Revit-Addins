#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
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
            AddinUtilities.PublishAddinLog(new AddinLog("Sheet Manager-Excel", commandData.Application.Application.VersionNumber));

            var m_Settings = new clsSettings(commandData);

            try
            {
                using m_dlg = new ProjectSheetManager(m_Settings);
                {
                    m_dlg.ShowDialog();
                    return Result.Succeeded;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
