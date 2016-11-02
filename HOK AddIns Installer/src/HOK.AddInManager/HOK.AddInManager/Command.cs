using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.AddInManager.Classes;
using HOK.AddInManager.UserControls;
using HOK.AddInManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.AddInManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class Command:IExternalCommand
    {
        private UIApplication m_app;
        private Dictionary<string/*toolName*/, LoadType> tempSettings = new Dictionary<string, LoadType>();

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
           
            try
            {
                Addins addins = AppCommand.thisApp.addins;
                StoreTempCollection(addins.AddinCollection);
                AddInViewModel viewModel = new AddInViewModel(addins);
                MainWindow mainWindow = new MainWindow();
                mainWindow.DataContext = viewModel;
                if (true == mainWindow.ShowDialog())
                {
                    //write setting
                    AppCommand.thisApp.addins = mainWindow.ViewModel.AddinsObj;

                    //load addins
                    AppCommand.thisApp.RemoveToolsBySettings();
                    AppCommand.thisApp.AddToolsBySettings();
                    AppCommand.thisApp.LoadToolsBySettings();
                }
                else
                {
                    OverrideTempSettings();
                }
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
            }
            return Result.Succeeded;
        }

        private void StoreTempCollection(ObservableCollection<AddinInfo> origCollection)
        {
            try
            {
                tempSettings.Clear();
                foreach (AddinInfo addin in origCollection)
                {
                    tempSettings.Add(addin.ToolName, addin.ToolLoadType);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void OverrideTempSettings()
        {
            try
            {
                foreach (string addinName in tempSettings.Keys)
                {
                    var addinFound = from info in AppCommand.thisApp.addins.AddinCollection where info.ToolName == addinName select info;
                    if (addinFound.Count() > 0)
                    {
                        int index = AppCommand.thisApp.addins.AddinCollection.IndexOf(addinFound.First());
                        AppCommand.thisApp.addins.AddinCollection[index].ToolLoadType = tempSettings[addinName];
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
