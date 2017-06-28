using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Classes;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Tools.RevisionTracker
{
    public class RevisionUpdater : IUpdater
    {
        private UpdaterId updaterId = null;
        private Guid linkedProjectId = Guid.Empty;

        private Guid updaterGuid = new Guid("0504A758-BF15-4C90-B996-A795D92B42DB");
        public Guid UpdaterGuid { get { return updaterGuid; } set { updaterGuid = value; } }

        private BuiltInParameter[] bltParameters = 
        { 
            BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM, 
            BuiltInParameter.PROJECT_REVISION_REVISION_NUM, 
            BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION,
            BuiltInParameter.PROJECT_REVISION_REVISION_DATE,
            BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO,
            BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_BY
        };

        private Dictionary<ElementId/*paramId*/, string/*paramName*/> revisionParameters = new Dictionary<ElementId, string>();
        private Dictionary<ElementId/*elementId*/, string/*uniqueId*/> idMaps = new Dictionary<ElementId, string>();

        public static bool IsSheetManagerOn = false;
        public Dictionary<ElementId, string> RevisionParameters { get { return revisionParameters; } set { revisionParameters = value; } }
        public Dictionary<ElementId, string> IdMaps { get { return idMaps; } set { idMaps = value; } }

        public RevisionUpdater(AddInId addInId)
        {
            updaterId = new UpdaterId(addInId, updaterGuid);

            CollectRevisionParamIds();
        }

        public bool Register(Document doc, ProjectUpdater pUpdater)
        {
            bool registered = false;
            try
            {
                if (!UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                {
                    UpdaterRegistry.RegisterUpdater(this, doc);
                    LogUtil.AppendLog("Revision Updater Registered.");
                    registered = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DoorUpdater-Register:" + ex.Message);
            }
            return registered;
        }

        public void Unregister(Document doc)
        {
            try
            {
                if (UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                {
                    UpdaterRegistry.UnregisterUpdater(updaterId, doc);
                    LogUtil.AppendLog("Revision Updater Removed.");
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DoorUpdater-Unregister:" + ex.Message);
            }
        }

        private void CollectRevisionParamIds()
        {
            try
            {
                foreach (BuiltInParameter bltParam in bltParameters)
                {
                    ElementId paramId = new ElementId((int)bltParam);
                    string paramName = LabelUtils.GetLabelFor(bltParam);
                    revisionParameters.Add(paramId, paramName);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void CollectIdMaps(Document doc)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> sheets = collector.OfCategory(BuiltInCategory.OST_Revisions).WhereElementIsNotElementType().ToList();
                idMaps = sheets.ToDictionary(o => o.Id, o => o.UniqueId);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        public string GetAdditionalInformation()
        {
            return updaterId.GetGUID().ToString();
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Views;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "RevisionDBUpdater";
        }
    }
}
