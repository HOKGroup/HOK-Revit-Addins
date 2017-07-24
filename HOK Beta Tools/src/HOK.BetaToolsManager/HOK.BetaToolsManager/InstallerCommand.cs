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
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                var version = commandData.Application.Application.VersionNumber;

                //var model = new AddinInstallerModel(version);
                //var viewModel = new AddinInstallerViewModel(model);
                var model = AppCommand.Instance.ViewModel;
                var viewModel = new AddinInstallerViewModel(model);
                var view = new AddinInstallerWindow
                {
                    DataContext = viewModel
                };

                view.ShowDialog();

                //AppCommand.thisApp.ShowInstaller(m_app);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize Installer command.\n"+ex.Message , "HOK Beta Tool Manager - Installer Command ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }
    }
}
