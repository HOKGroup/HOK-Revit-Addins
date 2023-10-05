using Autodesk.Revit.DB;
using HOK.SheetManager.AddIn.Classes;
using HOK.SheetManager.AddIn.Utils;
using HOK.SheetManager.AddIn.Windows;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.SheetManager.AddIn.Updaters
{
    public class SheetUpdater : IUpdater
    {
        private AddInId m_appId;
        private UpdaterId m_updaterId;
        private Guid linkedProjectId = Guid.Empty;
        private SheetManagerConfiguration configuration = null;
        private UpdaterDataManager dataManager = null;

        private BuiltInParameter[] bltParameters = 
        { 
            BuiltInParameter.SHEET_NUMBER,
            BuiltInParameter.SHEET_NAME
        };
        private Dictionary<ElementId/*paramId*/, string/*paramName*/> sheetParameters = new Dictionary<ElementId, string>();
        private Dictionary<ElementId/*elementId*/, string/*uniqueId*/> idMaps = new Dictionary<ElementId, string>();
        private List<SheetParameter> rvtSheetParameters = new List<SheetParameter>(); //sheet parameters in database

        public static bool IsSheetManagerOn = false;
        public Dictionary<ElementId, string> SheetParameters { get { return sheetParameters; } set { sheetParameters = value; } }
        public Dictionary<ElementId, string> IdMaps { get { return idMaps; } set { idMaps = value; } }

        public SheetUpdater(AddInId addInId, Guid updaterGuid, SheetManagerConfiguration config)
        {
            m_appId = addInId;
            m_updaterId = new UpdaterId(m_appId, updaterGuid);
            linkedProjectId = config.ModelId;
            configuration = config;

            dataManager = new UpdaterDataManager(configuration.DatabaseFile);
            rvtSheetParameters = dataManager.GetSheetParameters();
            
            //update project id
            CollectSheetParamIds();
        }


        private void CollectSheetParamIds()
        {
            try
            {
                foreach (BuiltInParameter bltParam in bltParameters)
                {
                    ElementId paramId = new ElementId((int)bltParam);
                    string paramName = LabelUtils.GetLabelFor(bltParam);
                    if (!sheetParameters.ContainsKey(paramId))
                    {
                        sheetParameters.Add(paramId, paramName);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void CollectCustomSheetParamIds(Document doc)
        {
            try
            {
                if (rvtSheetParameters.Count > 0)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    List<ViewSheet> sheets = collector.OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().Cast<ViewSheet>().ToList();
                    if (sheets.Count() > 0)
                    {
                        ViewSheet sheet = sheets.First();
                        foreach (SheetParameter customParam in rvtSheetParameters)
                        {
                            Parameter param = sheet.LookupParameter(customParam.ParameterName);
                            if (null != param)
                            {
                                if (!sheetParameters.ContainsKey(param.Id))
                                {
                                    sheetParameters.Add(param.Id, customParam.ParameterName);
                                }
                            }
                        }
                    }
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
                List<Element> sheets = collector.OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToList();
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

                foreach (ElementId sheetId in data.GetAddedElementIds())
                {
                    ViewSheet viewSheet = doc.GetElement(sheetId) as ViewSheet;
                    if (null != viewSheet)
                    {
                        bool inserted = InsertSheet(viewSheet);
                    }
                }

                foreach (ElementId sheetId in data.GetModifiedElementIds())
                {
                    ViewSheet viewSheet = doc.GetElement(sheetId) as ViewSheet;
                    if (null != viewSheet)
                    {
                        List<ElementId> parameterChanged = new List<ElementId>();
                        foreach (ElementId paramId in sheetParameters.Keys)
                        {
                            if(data.IsChangeTriggered(sheetId, Element.GetChangeTypeParameter(paramId)))
                            {
                                parameterChanged.Add(paramId);
                            }
                        }

                        bool updated = UpdateSheet(viewSheet, parameterChanged);
                    }
                }

                foreach (ElementId sheetId in data.GetDeletedElementIds())
                {
                    //bool deleted = DeleteSheet(sheetId);
                }
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool InsertSheet(ViewSheet sheet)
        {
            bool sheetInserted = false;
            try
            {
                // insert into the database connected
                RevitSheet rvtSheet = new RevitSheet(Guid.NewGuid(), sheet.SheetNumber, sheet.ViewName);
                LinkedSheet linkedSheet = new LinkedSheet(Guid.NewGuid(), rvtSheet.Id, new LinkedProject(linkedProjectId), sheet.UniqueId, true);
                rvtSheet.LinkedSheets.Add(linkedSheet);
                bool sheetDBUpdated = SheetDataWriter.ChangeSheetItem(rvtSheet, CommandType.INSERT);
                bool linkedSheetDBUpdated = SheetDataWriter.ChangeLinkedSheet(linkedSheet, CommandType.INSERT);

                List<SheetParameterValue> paramValues = new List<SheetParameterValue>();
                foreach (SheetParameter sheetParam in rvtSheetParameters)
                {
                    SheetParameterValue paramValue = new SheetParameterValue();
                    paramValue.ParameterValueId = Guid.NewGuid();
                    paramValue.Parameter = sheetParam;
                    paramValue.SheetId = rvtSheet.Id;

                    Parameter param = sheet.LookupParameter(sheetParam.ParameterName);
                    if (null != param)
                    {
                        switch (param.StorageType)
                        {
                            case StorageType.Double:
                                paramValue.ParameterValue = param.AsDouble().ToString();
                                break;
                            case StorageType.ElementId:
                                paramValue.ParameterValue = param.AsElementId().IntegerValue.ToString();
                                break;
                            case StorageType.Integer:
                                paramValue.ParameterValue = param.AsInteger().ToString();
                                break;
                            case StorageType.String:
                                paramValue.ParameterValue = param.AsString();
                                break;
                        }
                    }
                    paramValues.Add(paramValue);
                }
                bool sheetParamDBUpdated = SheetDataWriter.InsertMultipleParameterValue(paramValues);

                List<Guid> revisionIds = dataManager.GetRevisionIds();
                List<RevisionOnSheet> rosList = new List<RevisionOnSheet>();

                foreach (Guid revisionId in revisionIds)
                {
                    RevitRevision rvtRevision = new RevitRevision();
                    rvtRevision.Id = revisionId;
                    RevisionOnSheet ros = new RevisionOnSheet(Guid.NewGuid(), rvtSheet.Id, rvtRevision, false);
                    rvtSheet.SheetRevisions.Add(rvtRevision.Id, ros);
                    rosList.Add(ros);
                }
                bool rosDBUpdated = SheetDataWriter.InsertMultipleRevisionOnSheet(rosList);

                sheetInserted = (sheetDBUpdated && linkedSheetDBUpdated && rosDBUpdated) ? true : false;

                if (sheetInserted && !idMaps.ContainsKey(sheet.Id))
                {
                    idMaps.Add(sheet.Id, sheet.UniqueId);
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return sheetInserted;
        }

        private bool UpdateSheet(ViewSheet viewSheet, List<ElementId> paramIds)
        {
            bool updated = false;
            try
            {
                LinkedSheet lsheet = dataManager.GetLinkedSheet(viewSheet.UniqueId, linkedProjectId);
                if (null != lsheet)
                {
                    foreach (ElementId paramId in paramIds)
                    {
                        string paramName = sheetParameters[paramId];
                        Parameter param = viewSheet.LookupParameter(paramName);
                        if (null != param)
                        {
                            string paramValue = "";
                            switch (param.StorageType)
                            {
                                case StorageType.Double:
                                    paramValue = param.AsDouble().ToString();
                                    break;
                                case StorageType.ElementId:
                                    paramValue = param.AsElementId().IntegerValue.ToString();
                                    break;
                                case StorageType.Integer:
                                    paramValue = param.AsInteger().ToString();
                                    break;
                                case StorageType.String:
                                    paramValue = param.AsString();
                                    break;
                            }

                            var paramFound = from sheetParam in rvtSheetParameters where sheetParam.ParameterName == paramName select sheetParam;
                            if (paramFound.Count() > 0)
                            {
                                SheetParameter sheetParam = paramFound.First();
                                SheetParameterValue sheetParamValue = dataManager.GetSheetParameterValue(sheetParam.ParameterId, lsheet.SheetId);
                                sheetParamValue.ParameterValue = paramValue;
                                updated = SheetDataWriter.ChangeSheetParameterValue(sheetParamValue, CommandType.UPDATE);
                            }
                            else if (paramId.IntegerValue == (int)BuiltInParameter.SHEET_NAME)
                            {
                                updated = SheetDataWriter.ChangeSheetItem(lsheet.SheetId.ToString(), "Sheet_Name", paramValue);

                            }
                            else if (paramId.IntegerValue == (int)BuiltInParameter.SHEET_NUMBER)
                            {
                                updated = SheetDataWriter.ChangeSheetItem(lsheet.SheetId.ToString(), "Sheet_Number", paramValue);
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

        private bool DeleteSheet(ElementId sheetId)
        {
            bool deleted = false;
            try
            {
                if (idMaps.ContainsKey(sheetId))
                {
                    string uniqueId = idMaps[sheetId];
                    LinkedSheet linkedSheet = dataManager.GetLinkedSheet(uniqueId, linkedProjectId);
                    if (null != linkedSheet)
                    {
                        if (linkedSheet.IsSource)
                        {
                            MessageBoxResult msgResult = MessageBox.Show("Would you like to delete the sheet item in the linked database?", "Delete Sheet Source Item", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (msgResult == MessageBoxResult.Yes)
                            {
                                RevitSheet rvtSheet = new RevitSheet() { Id = linkedSheet.SheetId };
                                bool sheetDeleted = SheetDataWriter.ChangeSheetItem(rvtSheet, CommandType.DELETE);
                                bool paramDeleted = SheetDataWriter.DeleteSheetParameterValue(rvtSheet.Id.ToString());
                                bool revisionDeleted = SheetDataWriter.DeleteRevisionOnSheet("RevisionsOnSheet_Sheet_Id", rvtSheet.Id.ToString());
                                bool linkDeleted = SheetDataWriter.ChangeLinkedSheet(linkedSheet, CommandType.DELETE);
                                deleted = (sheetDeleted && paramDeleted && revisionDeleted && linkDeleted) ? true : false;
                            }
                            else
                            {
                                deleted = SheetDataWriter.ChangeLinkedSheet(linkedSheet, CommandType.DELETE);
                            }
                        }
                        else
                        {
                            deleted = SheetDataWriter.ChangeLinkedSheet(linkedSheet, CommandType.DELETE);
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
            return "SheetDBUpdater";
        }
    }
}
