using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Visibility = System.Windows.Visibility;

namespace HOK.MissionControl.FamilyPublish
{
    /// <summary>
    /// Class attributes are used for beta tools management.
    /// </summary>
    [Name(nameof(Properties.Resources.FamilyPublish_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.FamilyPublish_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.FamilyPublish_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.FamilyPublish_PanelName), typeof(Properties.Resources))]
    [ButtonText(nameof(Properties.Resources.FamilyPublish_ButtonText), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.FamilyPublish_Namespace), typeof(Properties.Resources))]
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class FamilyPublishCommand : IExternalCommand
    {
        public Dictionary<string, Configuration> ConfigDictionary { get; set; } = new Dictionary<string, Configuration>();
        public Dictionary<string, Project> ProjectDictionary { get; set; } = new Dictionary<string, Project>();

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("MissionControl-PublishFamilyData", commandData.Application.Application.VersionNumber));

                var pathName = doc.PathName;
                if (string.IsNullOrEmpty(pathName))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Path is Empty. File was not saved yet.");
                    var dialog = new FamilyMonitorView
                    {
                        DataContext =
                            new FamilyMonitorViewModel(null,
                                "...establish a connection to Mission Control for unsaved files.")
                    };
                    dialog.ShowDialog();

                    return Result.Failed;
                }
                

                var centralPath = BasicFileInfo.Extract(pathName).CentralPath;
                if (string.IsNullOrEmpty(centralPath))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Could not get Central Path.");
                    var dialog = new FamilyMonitorView
                    {
                        DataContext =
                            new FamilyMonitorViewModel(null,
                                "...get a Central File Path. Only Workshared projects can be added to Mission Control.")
                    };
                    dialog.ShowDialog();

                    return Result.Failed;
                }
                

                var configFound = ServerUtilities.GetConfigurationByCentralPath(centralPath);
                if (configFound != null)
                {
                    if (ConfigDictionary.ContainsKey(centralPath))
                    {
                        ConfigDictionary.Remove(centralPath);
                    }
                    ConfigDictionary.Add(centralPath, configFound);

                    var projectFound = ServerUtilities.GetProjectByConfigurationId(configFound.Id);
                    if (null != projectFound)
                    {
                        if (ProjectDictionary.ContainsKey(centralPath))
                        {
                            ProjectDictionary.Remove(centralPath);
                        }
                        ProjectDictionary.Add(centralPath, projectFound);
                    }
                }

                if (!ProjectDictionary.ContainsKey(centralPath) || !ConfigDictionary.ContainsKey(centralPath))
                {
                    Log.AppendLog(LogMessageType.ERROR, "No Config Found.");
                    var dialog = new FamilyMonitorView
                    {
                        DataContext =
                            new FamilyMonitorViewModel(null,
                                "...find your project in Mission Control database. Please make sure that it was added.")
                    };
                    dialog.ShowDialog();

                    return Result.Failed;
                }

                var model = new FamilyMonitorModel(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath]);
                var viewModel =
                    new FamilyMonitorViewModel(model, "...make this any faster. Hang in there!")
                    {
                        ExecuteFamilyPublish = true
                    };
                var view = new FamilyMonitorView
                {
                    DataContext = viewModel,
                    CloseButton = { Visibility = Visibility.Collapsed }
                };
                view.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }
    }
}
