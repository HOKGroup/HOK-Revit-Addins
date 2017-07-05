using System;
using System.Windows;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace HOK.BetaToolsManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]

    public class InstallerCommand:IExternalCommand
    {
        private UIApplication m_app;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;

                AppCommand.thisApp.ShowInstaller(m_app);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("Failed to initialize Installer command.\n"+ex.Message , "HOK Beta Tool Manager - Installer Command ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }
    }
}
