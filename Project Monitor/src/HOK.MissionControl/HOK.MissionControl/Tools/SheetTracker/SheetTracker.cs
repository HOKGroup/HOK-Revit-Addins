using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.SheetTracker
{
    public class SheetTracker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void SynchSheets(Document doc)
        {
            try
            {
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                var sheets = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Sheets)
                    .WhereElementIsNotElementType()
                    .Cast<ViewSheet>()
                    .Select(x => new SheetItem(x, centralPath))
                    .ToDictionary(key => key.UniqueId, value => value);

                var revisions = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Revisions)
                    .WhereElementIsNotElementType()
                    .Select(x => new RevisionItem(x))
                    .ToDictionary(key => key.UniqueId, value => value);

                var finalList = new List<SheetItem>();
                foreach (var ms in AppCommand.SheetsData.Sheets)
                {
                    if (sheets.ContainsKey(ms.UniqueId))
                    {
                        // sheet still exists in our model
                        // we can update it
                        var localSheet = sheets[ms.UniqueId];
                        localSheet.Tasks = ms.Tasks; // preserve mongo tasks
                        localSheet.Id = ms.Id; // preserve mongoIds
                        localSheet.CollectionId = ms.CollectionId; // preserve mongoIds

                        finalList.Add(localSheet);
                        sheets.Remove(ms.UniqueId);
                    }
                    else
                    {
                        var task = ms.Tasks.LastOrDefault();
                        if (task != null && task.IsNewSheet)
                        {
                            finalList.Add(ms); // this already has the ids
                        }
                        else
                        {
                            // sheet was deleted locally but still exists in mongo
                            ms.IsDeleted = true;
                            finalList.Add(ms);
                        }
                    }
                }
                finalList.AddRange(sheets.Values); // add whatever was not stored in mongo before

                var data = new SheetData
                {
                    CentralPath = centralPath.ToLower(),
                    Sheets = finalList,
                    Revisions = revisions.Values.ToList(),
                    Id = AppCommand.SheetsData.Id
                };

                if (!ServerUtilities.Post(data, "sheets/" + AppCommand.SheetsData.Id, 
                    out SheetData unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Sheets.");
                }

                AppCommand.SheetsData = data;
                Communicator.CommunicatorUtilities.LaunchCommunicator();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
