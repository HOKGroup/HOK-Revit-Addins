using System;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.Communicator.Tasks.SheetTaskAssistant
{
    public class SheetTaskAssistantModel
    {
        /// <summary>
        /// Firest external command to update propsed changes to sheets.
        /// </summary>
        /// <param name="wrapper">Sheet Task Wrapper object.</param>
        public void SubmitSheetTask(SheetTaskWrapper wrapper)
        {
            var item = (SheetItem)wrapper.Element;
            var task = (SheetTask)wrapper.Task;

            AppCommand.CommunicatorHandler.SheetItem = item;
            AppCommand.CommunicatorHandler.SheetTask = task;
            AppCommand.CommunicatorHandler.Request.Make(task.IsNewSheet
                ? RequestId.CreateSheet
                : RequestId.UpdateSheet);

            AppCommand.CommunicatorEvent.Raise();
        }

        /// <summary>
        /// Posts approved changes to MongoDB. Mongo will fire socket event when complete.
        /// </summary>
        /// <param name="wrapper">Sheet Task Wrapper containing approved task.</param>
        /// <param name="centralPath"></param>
        public void Approve(SheetTaskWrapper wrapper, string centralPath)
        {
            var sheetsDataId = string.Empty;
            if (MissionControlSetup.SheetsData.ContainsKey(centralPath))
                sheetsDataId = MissionControlSetup.SheetsData[centralPath].Id;
            if (string.IsNullOrEmpty(sheetsDataId)) return;

            var t = (SheetTask)wrapper.Task;
            var e = (SheetItem)wrapper.Element;

            t.CompletedBy = Environment.UserName.ToLower();
            t.CompletedOn = DateTime.UtcNow;

            // body needs to be updated with a new identifier object or mongo side will fail.
            if (e.IsNewSheet)
            {
                // (Konrad) It's a create sheet task (element associated with task is null). Let's approve that.
                var newSheet = AppCommand.CommunicatorHandler.SheetItem;
                if (newSheet == null) return;

                wrapper.Element = newSheet;
                //ServerUtilities.Post<SheetData>(wrapper, "sheets/" + sheetsDataId + "/approvenewsheet");
            }
            else
            {
                t.SheetId = e.Id;
                //ServerUtilities.Post<SheetData>(t, "sheets/" + sheetsDataId + "/updatetasks");
            } 
        }

        /// <summary>
        /// Firest external command to set active view in Revit.
        /// </summary>
        /// <param name="sheet">Sheet item with reference to view.</param>
        public void OpenView(SheetItem sheet)
        {
            AppCommand.CommunicatorHandler.SheetItem = sheet;
            AppCommand.CommunicatorHandler.Request.Make(RequestId.OpenView);
            AppCommand.CommunicatorEvent.Raise();
        }
    }
}
