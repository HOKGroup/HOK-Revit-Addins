using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Utils;
using System.Linq;
using HOK.MissionControl.Core.Schemas.Sheets;

namespace HOK.MissionControl.Tools.SheetTracker
{
    public class SheetTracker
    {
        public void SynchSheets(Document doc)
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
                AppCommand.SheetsData = ServerUtilities.GetByCentralPath<SheetData>(centralPath, "sheets/centralPath");
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
                else
                {
                    // (Konrad) Compare current Sheets against any staged changes.
                    MissionControlSetup.SheetsIds.Add(centralPath, AppCommand.SheetsData.Id); // store sheets record
                }
            }

            if (refreshProject)
            {
                var projectFound = ServerUtilities.GetProjectByConfigurationId(currentConfig.Id);
                if (null == projectFound) return;
                MissionControlSetup.Projects[centralPath] = projectFound;
            }
        }
    }
}
