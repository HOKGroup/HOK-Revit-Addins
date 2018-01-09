using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Core.Schemas.Sheets;

namespace HOK.MissionControl.Tools.SheetTracker
{
    public class SheetTracker
    {
        public void SynchSheets(Document doc)
        {
            try
            {
                var centralPath = BasicFileInfo.Extract(doc.PathName).CentralPath;
                var currentProject = MissionControlSetup.Projects[centralPath];
                var currentConfig = MissionControlSetup.Configurations[centralPath];

                var sheets = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Sheets)
                    .WhereElementIsNotElementType()
                    .Cast<ViewSheet>()
                    .Select(x => new SheetItem(x, centralPath))
                    .ToDictionary(key => key.uniqueId, value => value);

                var revisions = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Revisions)
                    .WhereElementIsNotElementType()
                    .Select(x => new RevisionItem(x))
                    .ToDictionary(key => key.uniqueId, value => value);

                var refreshProject = false;
                if (!MissionControlSetup.SheetsIds.ContainsKey(centralPath))
                {
                    AppCommand.SheetsData = ServerUtilities.GetByCentralPath<SheetData>(centralPath, "sheets/centralpath");
                    if (AppCommand.SheetsData == null)
                    {
                        // (Konrad) This route executes only when we are posting Sheets data for the first time.
                        var data = new SheetData
                        {
                            centralPath = centralPath,
                            sheets = sheets.Values.ToList(),
                            revisions = revisions.Values.ToList()
                        };

                        AppCommand.SheetsData = ServerUtilities.Post<SheetData>(data, "sheets");
                        ServerUtilities.Put(currentProject, "projects/" + currentProject.Id + "/addsheets/" + AppCommand.SheetsData.Id);
                        refreshProject = true;
                    }

                    if (!MissionControlSetup.SheetsIds.ContainsKey(centralPath)) MissionControlSetup.SheetsIds.Add(centralPath, AppCommand.SheetsData.Id); // store sheets record
                }
                else
                {
                    // (Konrad) This route executes when we are synching sheet updates.
                    var mongoSheets = ServerUtilities.GetByCentralPath<SheetData>(centralPath, "sheets/centralpath");
                    if (mongoSheets != null)
                    {
                        var finalList = new List<SheetItem>();
                        foreach (var ms in mongoSheets.sheets)
                        {
                            if (sheets.ContainsKey(ms.uniqueId))
                            {
                                // sheet still exists in our model
                                // we can update it
                                var localSheet = sheets[ms.uniqueId];
                                localSheet.tasks = ms.tasks; // preserve mongo tasks
                                localSheet.Id = ms.Id; // preserve mongoIds
                                localSheet.collectionId = ms.collectionId; // preserve mongoIds

                                finalList.Add(localSheet);
                                sheets.Remove(ms.uniqueId);
                            }
                            else
                            {
                                var task = ms.tasks.LastOrDefault();
                                if (task != null && task.isNewSheet)
                                {
                                    finalList.Add(ms); // this already has the ids
                                }
                                else
                                {
                                    // sheet was deleted locally but still exists in mongo
                                    ms.isDeleted = true;
                                    finalList.Add(ms);
                                }
                            }
                        }
                        finalList.AddRange(sheets.Values); // add whatever was not stored in mongo before

                        var data = new SheetData
                        {
                            centralPath = centralPath,
                            sheets = finalList,
                            revisions = revisions.Values.ToList(),
                            Id = mongoSheets.Id
                        };

                        ServerUtilities.Post<SheetData>(data, "sheets/" + mongoSheets.Id);
                        AppCommand.SheetsData = data;
                    }
                }

                if (refreshProject)
                {
                    var projectFound = ServerUtilities.Get<Project>("projects/configid/" + currentConfig.Id);
                    //var projectFound = ServerUtilities.GetProjectByConfigurationId(currentConfig.Id);
                    if (null == projectFound) return;
                    MissionControlSetup.Projects[centralPath] = projectFound;
                }

                AppCommand.LunchCommunicator();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
