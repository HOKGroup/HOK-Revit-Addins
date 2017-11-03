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
            AppCommand.CommunicatorHandler.SheetItem = (SheetItem) wrapper.Element;
            AppCommand.CommunicatorHandler.SheetTask = (SheetItem) wrapper.Task;
            AppCommand.CommunicatorHandler.Request.Make(RequestId.UpdateSheet);
            AppCommand.CommunicatorEvent.Raise();
        }

        /// <summary>
        /// Posts approved changes to MongoDB. Mongo will fire socket event when complete.
        /// </summary>
        /// <param name="wrapper">Sheet Task Wrapper containing approved task.</param>
        public void Approve(SheetTaskWrapper wrapper)
        {
            var t = (SheetItem)wrapper.Task;
            var sheetsDataId = AppCommand.SheetsData.Id;
            if (string.IsNullOrEmpty(sheetsDataId)) return;

            ServerUtilities.Post<SheetData>(t, "sheets/" + sheetsDataId + "/sheetchanges/approve");
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
