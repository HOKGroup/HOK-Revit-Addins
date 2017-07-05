using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.BetaToolsManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

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
