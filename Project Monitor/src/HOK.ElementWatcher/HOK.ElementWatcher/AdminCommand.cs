using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.ElementWatcher.Classes;
using HOK.ElementWatcher.Settings;
using HOK.ElementWatcher.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.ElementWatcher
{
     [Transaction(TransactionMode.Manual)]
    public class AdminCommand :IExternalCommand
    {
         private UIApplication m_app = null;
         private Document m_doc = null;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                UserWindow userWindow = new UserWindow();
                if ((bool)userWindow.ShowDialog())
                {
                    string projectFileId = DataStorageUtil.GetProjectFileId(m_doc).ToString();
                    if (AppCommand.Instance.Configurations.ContainsKey(projectFileId))
                    {
                        DTMConfigurations config = AppCommand.Instance.Configurations[projectFileId];

                        AdminViewModel viewModel = new AdminViewModel(config);
                        AdminWindow adminWindow = new AdminWindow();
                        adminWindow.DataContext = viewModel;
                        if ((bool)adminWindow.ShowDialog())
                        {
                            config = adminWindow.ViewModel.Configuration;
                            AppCommand.Instance.Configurations.Remove(projectFileId);
                            AppCommand.Instance.Configurations.Add(projectFileId, config);
                            AppCommand.Instance.ApplyConfiguration(m_doc, config);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                MessageBox.Show("Failed to execute Admin Command.\n" + ex.Message, "Admin Command", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }
    }
}
