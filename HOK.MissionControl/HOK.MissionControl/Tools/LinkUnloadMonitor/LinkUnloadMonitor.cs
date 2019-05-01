using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Tools.LinkUnloadMonitor
{
    public static class LinkUnloadMonitor
    {
        //public Guid UpdaterGuid { get; set; } = new Guid("90391154-67BB-452E-A1A7-A07A98B94F86");

        /// <summary>
        /// Creates an idling task that will bind our own Reload Latest command to existing one.
        /// </summary>
        public static void CreateLinkUnloadOverride(UIApplication app)
        {
            try
            {
                var commandId = RevitCommandId.LookupCommandId("ID_UNLOAD_FOR_ALL_USERS");
                if (commandId == null || !commandId.CanHaveBinding) return;

                var binding = app.CreateAddInCommandBinding(commandId);
                binding.Executed += OnUnloadForAllUsers;
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        /// <summary>
        /// Removes ability to Unload a link for All Users.
        /// </summary>
        private static void OnUnloadForAllUsers(object sender, ExecutedEventArgs args)
        {
            var ssWindow = new LinkUnloadMonitorView();
            var o = ssWindow.ShowDialog();
            if (o != null && ssWindow.IsActive) ssWindow.Close();
        }
    }
}
