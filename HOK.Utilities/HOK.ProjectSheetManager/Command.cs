#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.ProjectSheetManager.Classes;
using HOK.ProjectSheetManager.Forms;
using Nice3point.Revit.Toolkit.External;
#endregion

namespace HOK.ProjectSheetManager
{
    [Transaction(TransactionMode.Manual)]
    public class Command : ExternalCommand
    {
        public override void Execute()
        {

            var m_Settings = new Classes.Settings(ExternalCommandData);

            try
            {
                var m_dlg = new ProjectSheetManagerForm(m_Settings);
                m_dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Failed To Load", ex.Message);
            }
        }
    }
}
