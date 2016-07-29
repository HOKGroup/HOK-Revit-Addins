using Autodesk.Revit.DB;
using HOK.SheetManager.AddIn.Classes;
using HOK.SheetManager.AddIn.Utils;
using HOK.SheetManager.AddIn.Windows;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.SheetManager.AddIn.Updaters
{
    public class RevisionUpdater : IUpdater
    {
        private AddInId m_appId;
        private UpdaterId m_updaterId;
        private Guid linkedProjectId = Guid.Empty;
        private SheetManagerConfiguration configuration = null;
        private UpdaterDataManager dataManager = null;

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
        
        public RevisionUpdater(AddInId addInId, Guid updaterGuid, SheetManagerConfiguration config)
        {
            m_appId = addInId;
            m_updaterId = new UpdaterId(m_appId, updaterGuid);
            linkedProjectId = config.ModelId;
            configuration = config;

            dataManager = new UpdaterDataManager(configuration.DatabaseFile);
            CollectRevisionParamIds();
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
                if (IsSheetManagerOn) { return; }
                if (SheetDataWriter.dbFile != configuration.DatabaseFile) { SheetDataWriter.OpenDatabase(configuration.DatabaseFile); }

                Document doc = data.GetDocument();

                foreach (ElementId revisionId in data.GetAddedElementIds())
                {
                    Revision revision = doc.GetElement(revisionId) as Revision;
                    if (null != revision)
                    {
                        bool inserted = InsertRevision(revision);
                    }
                }
                foreach (ElementId revisionId in data.GetModifiedElementIds())
                {
                    Revision revision = doc.GetElement(revisionId) as Revision;
                    if (null != revision)
                    {
                        List<ElementId> parameterChanged = new List<ElementId>();
                        foreach (ElementId paramId in revisionParameters.Keys)
                        {
                            if(data.IsChangeTriggered(revisionId, Element.GetChangeTypeParameter(paramId)))
                            {
                                parameterChanged.Add(paramId);
                            }
                        }
                        bool updated = UpdateRevision(revision, parameterChanged);
                    }
                }
                foreach (ElementId revisionId in data.GetDeletedElementIds())
                {
                    bool deleted = DeleteRevision(revisionId);
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool InsertRevision(Revision revision)
        {
            bool revisionInserted = false;
            try
            {
                RevitRevision rvtRevision = new RevitRevision(Guid.NewGuid(), revision.Description, revision.IssuedBy, revision.IssuedTo, revision.RevisionDate);
                NumberType revNumType = (NumberType)Enum.Parse(typeof(NumberType), revision.NumberType.ToString());
                LinkedRevision linkedRevision = new LinkedRevision(Guid.NewGuid(), rvtRevision.Id, revision.SequenceNumber, revision.RevisionNumber, revNumType, linkedProjectId, revision.UniqueId, true);
                rvtRevision.LinkedRevisions.Add(linkedRevision);

                bool revisionDBUpdated = SheetDataWriter.ChangeRevisionItem(rvtRevision, CommandType.INSERT);
                bool linkedRevisionDBUpdated = SheetDataWriter.ChangeLinkedRevision(linkedRevision, CommandType.INSERT);

                List<RevisionOnSheet> rosList = new List<RevisionOnSheet>();
                List<Guid> sheetIds = dataManager.GetSheetIds();
                foreach (Guid sheetId in sheetIds)
                {
                    RevisionOnSheet ros = new RevisionOnSheet(Guid.NewGuid(), sheetId, rvtRevision, false);
                    rosList.Add(ros);
                }

                bool rosDBUpdated = SheetDataWriter.InsertMultipleRevisionOnSheet(rosList);
                revisionInserted = (revisionDBUpdated && linkedRevisionDBUpdated && rosDBUpdated) ? true : false;

                if (!idMaps.ContainsKey(revision.Id))
                {
                    idMaps.Add(revision.Id, revision.UniqueId);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return revisionInserted;
        }

        private bool UpdateRevision(Revision revision, List<ElementId> paramIds)
        {
            bool updated = false;
            try
            {
                LinkedRevision linkedRevision = dataManager.GetLinkedRevision(revision.UniqueId, linkedProjectId);
                if (null != linkedRevision)
                {
                    foreach (ElementId paramId in paramIds)
                    {
                        string paramName = revisionParameters[paramId];
                        Parameter param = revision.LookupParameter(paramName);
                        if (null != param)
                        {
                            string paramValue = "";
                            switch (paramId.IntegerValue)
                            {
                                case (int)BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM:
                                    paramValue = param.AsInteger().ToString();
                                    updated = SheetDataWriter.ChangeLinkedRevision(linkedRevision.Id, "LinkedRevision_Sequence", paramValue, CommandType.UPDATE);
                                    break;
                                case (int)BuiltInParameter.PROJECT_REVISION_REVISION_NUM:
                                    paramValue = param.AsString();
                                    updated = SheetDataWriter.ChangeLinkedRevision(linkedRevision.Id, "LinkedRevision_Number", paramValue, CommandType.UPDATE);
                                    break;
                                case (int)BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION:
                                    paramValue = param.AsString();
                                    updated = SheetDataWriter.ChangeRevisionItem(linkedRevision.RevisionId.ToString(), "Revision_Description", paramValue);
                                    break;
                                case (int)BuiltInParameter.PROJECT_REVISION_REVISION_DATE:
                                    paramValue = param.AsString();
                                    updated = SheetDataWriter.ChangeRevisionItem(linkedRevision.RevisionId.ToString(), "Revision_Date", paramValue);
                                    break;
                                case (int)BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO:
                                    paramValue = param.AsString();
                                    updated = SheetDataWriter.ChangeRevisionItem(linkedRevision.RevisionId.ToString(), "Revision_IssuedTo", paramValue);
                                    break;
                                case (int)BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_BY:
                                    paramValue = param.AsString();
                                    updated = SheetDataWriter.ChangeRevisionItem(linkedRevision.RevisionId.ToString(), "Revision_IssuedBy", paramValue);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }

        private bool DeleteRevision(ElementId revisionId)
        {
            bool deleted = false;
            try
            {
                if (idMaps.ContainsKey(revisionId))
                {
                    string uniqueId = idMaps[revisionId];
                    LinkedRevision linkedRevision = dataManager.GetLinkedRevision(uniqueId, linkedProjectId);
                    if (null != linkedRevision)
                    {
                        if (linkedRevision.IsSource)
                        {
                            MessageBoxResult msgResult = MessageBox.Show("Would you like to delete the revision item in the linked database?", "Delete Revision Source Item", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (msgResult == MessageBoxResult.Yes)
                            {
                                RevitRevision rvtRevision = new RevitRevision();
                                rvtRevision.Id = linkedRevision.RevisionId;

                                bool revisionDeleted = SheetDataWriter.ChangeRevisionItem(rvtRevision, CommandType.DELETE);
                                bool rosDeleted = SheetDataWriter.DeleteRevisionOnSheet("RevisionsOnSheet_Revision_Id", rvtRevision.Id.ToString());
                                bool linkDeleted = SheetDataWriter.ChangeLinkedRevision(linkedRevision, CommandType.DELETE);
                                deleted = (revisionDeleted && rosDeleted && linkDeleted) ? true : false;
                            }
                            else
                            {
                                deleted = SheetDataWriter.ChangeLinkedRevision(linkedRevision, CommandType.DELETE);
                            }
                        }
                        else
                        {
                            deleted = SheetDataWriter.ChangeLinkedRevision(linkedRevision, CommandType.DELETE);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return deleted;
        }

        public void CloseDataManager()
        {
            dataManager.CloseDatabse();
        }

        public string GetAdditionalInformation()
        {
            return m_updaterId.GetGUID().ToString();
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Views;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "RevisionDBUpdater";
        }
    }
}
