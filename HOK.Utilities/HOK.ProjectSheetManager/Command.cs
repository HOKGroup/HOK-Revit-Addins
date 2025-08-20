namespace HOK.ProjectSheetManager
{
    [Transaction(TransactionMode.Manual)]
    public class Command : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                var m_Settings = new Classes.Settings(ExternalCommandData);
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
