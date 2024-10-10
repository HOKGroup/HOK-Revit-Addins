#region References

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Messaging;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.GroupsManager.Utilities;
using Nice3point.Revit.Toolkit.External;
// ReSharper disable UnusedMember.Global

#endregion

namespace HOK.MissionControl.GroupsManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class GroupsManagerCommand : ExternalCommand
    {
        public override void Execute()
        {
            var uiApp = Context.Application;
            var doc = Context.ActiveDocument;
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontinue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("MissionControl-GroupsManager", Application.VersionNumber));

                var model = new GroupsManagerModel(doc);
                var viewModel = new GroupsManagerViewModel(model);
                var view = new GroupsManagerView
                {
                    DataContext = viewModel
                };

                var unused = new WindowInteropHelper(view)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };
                
                view.Show();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }
}
