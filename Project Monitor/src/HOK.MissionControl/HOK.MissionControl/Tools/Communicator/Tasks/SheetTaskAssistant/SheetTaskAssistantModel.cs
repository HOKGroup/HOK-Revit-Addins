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
            var task = (SheetItem)wrapper.Task;
            if (item == null && task.identifier == "")
            {
                // (Konrad) It's a creation task.
                AppCommand.CommunicatorHandler.SheetTask = task;
                AppCommand.CommunicatorHandler.Request.Make(RequestId.CreateSheet);
                AppCommand.CommunicatorEvent.Raise();
            }
            else
            {
                AppCommand.CommunicatorHandler.SheetItem = item;
                AppCommand.CommunicatorHandler.SheetTask = task;
                AppCommand.CommunicatorHandler.Request.Make(RequestId.UpdateSheet);
                AppCommand.CommunicatorEvent.Raise();
            }
        }

        /// <summary>
        /// Posts approved changes to MongoDB. Mongo will fire socket event when complete.
        /// </summary>
        /// <param name="wrapper">Sheet Task Wrapper containing approved task.</param>
        public void Approve(SheetTaskWrapper wrapper)
        {
            var sheetsDataId = AppCommand.SheetsData.Id;
            if (string.IsNullOrEmpty(sheetsDataId)) return;

            var t = wrapper.Task as SheetItem;
            var e = wrapper.Element as SheetItem;
            
            if (e == null)
            {
                // (Konrad) It's a create sheet task (element associated with task is null). Let's approve that.
                var newSheet = AppCommand.CommunicatorHandler.SheetItem;
                if (newSheet == null) return;

                ServerUtilities.Post<SheetData>(newSheet, "sheets/" + sheetsDataId + "/sheetchanges/approvenewsheet");
            }
            else
            {
                ServerUtilities.Post<SheetData>(t, "sheets/" + sheetsDataId + "/sheetchanges/approve");
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
