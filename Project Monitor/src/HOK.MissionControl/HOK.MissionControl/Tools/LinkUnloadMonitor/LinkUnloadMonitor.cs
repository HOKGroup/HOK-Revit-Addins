using System;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Tools.LinkUnloadMonitor
{
    public class LinkUnloadMonitor
    {
        public Guid UpdaterGuid { get; set; } = new Guid("90391154-67BB-452E-A1A7-A07A98B94F86");

        /// <summary>
        /// Creates an idling task that will bind our own Reload Latest command to existing one.
        /// </summary>
        public void CreateLinkUnloadOverride()
        {
            AppCommand.EnqueueTask(app =>
            {
                try
                {
                    var commandId = RevitCommandId.LookupCommandId("ID_UNLOAD_FOR_ALL_USERS");
                    if (commandId == null || !commandId.CanHaveBinding) return;

                    var binding = app.CreateAddInCommandBinding(commandId);
                    binding.Executed += AppCommand.OnUnloadForAllUsers;
                }
                catch (Exception e)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                }
            });
        }
    }
}
