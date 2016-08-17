using Autodesk.Revit.DB;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.SheetManager.AddIn.Utils
{
    public static class LinkStatusChecker
    {
        public static void CheckLinkStatus(Document doc, Guid projectId, ref RevitSheetData sheetData, bool autoUpdate)
        {
            try
            {
                CheckRevisionLinks(doc, projectId, ref sheetData);
                if (autoUpdate)
                {
                    InsertRevisionElements(doc, projectId, ref sheetData);
                }
               
                CheckSheetLinks(doc, projectId, ref sheetData);
                if (autoUpdate)
                {
                    InsertSheetElements(doc, projectId, ref sheetData);
                }

                CheckViewLinks(doc, projectId, ref sheetData);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check link status.\n" + ex.Message, "Sheet Manager Updater - Check Link Status", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return;
        }

        private static void CheckRevisionLinks(Document doc, Guid projectId, ref RevitSheetData sheetData)
        {
            try
            {
                for (int i = 0; i < sheetData.Revisions.Count; i++)
                {
                    RevitRevision rvtRevision = sheetData.Revisions[i];

                    var linkedRevisionFound = from lrevision in sheetData.Revisions[i].LinkedRevisions where lrevision.LinkProject.Id == projectId select lrevision;
                    if (linkedRevisionFound.Count() > 0)
                    {
                        LinkedRevision linkedRevision = linkedRevisionFound.First();
                        Revision revision = doc.GetElement(linkedRevision.LinkedElementId) as Revision;
                        if (null != revision)
                        {
                            sheetData.Revisions[i].LinkStatus.IsLinked = true;
                            sheetData.Revisions[i].LinkStatus.CurrentLinkedId = linkedRevision.LinkedElementId;
                            sheetData.Revisions[i].LinkStatus.LinkedElementId = revision.Id.IntegerValue;
                            sheetData.Revisions[i].LinkStatus.ToolTip = "Linked Revision ElementId: " + revision.Id.IntegerValue;

                            if (revision.Description != rvtRevision.Description || revision.IssuedBy != rvtRevision.IssuedBy || revision.IssuedTo != rvtRevision.IssuedTo || revision.RevisionDate != rvtRevision.Date)
                            {
                                sheetData.Revisions[i].LinkStatus.Modified = true;
                                sheetData.Revisions[i].LinkStatus.ToolTip = "Revision parameter values are different from the linked element.";

                            }
                        }
                        else
                        {
                            //item deleted
                            sheetData.Revisions[i].LinkStatus.Modified = true;
                            sheetData.Revisions[i].LinkStatus.ToolTip = "Linked Revision Not Found.";
                            sheetData.Revisions[i].LinkedRevisions.Remove(linkedRevision);
                            //bool revisionDBUpdated = SheetDataWriter.ChangeLinkedRevision(linkedRevision, CommandType.DELETE);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check link status of revisions.\n" + ex.Message, "Sheet Manager Updater - Check Revision Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void CheckSheetLinks(Document doc, Guid projectId, ref RevitSheetData sheetData)
        {
            try
            {
                Dictionary<Guid/*revisionId*/, int/*elementId*/> revDictionary = new Dictionary<Guid, int>();
                var linkedRevisions = from revision in sheetData.Revisions where revision.LinkStatus.IsLinked select revision;
                if (linkedRevisions.Count() > 0)
                {
                    revDictionary = linkedRevisions.ToDictionary(o => o.Id, o => o.LinkStatus.LinkedElementId);
                }
               
                for (int i = 0; i < sheetData.Sheets.Count; i++)
                {
                    RevitSheet rvtSheet = sheetData.Sheets[i];

                    var linkedSheetFound = from lsheet in sheetData.Sheets[i].LinkedSheets where lsheet.LinkProject.Id == projectId select lsheet;
                    if (linkedSheetFound.Count() > 0)
                    {
                        LinkedSheet linkedSheet = linkedSheetFound.First();
                        ViewSheet viewSheet = doc.GetElement(linkedSheet.LinkedElementId) as ViewSheet;
                        if (null != viewSheet)
                        {

                            sheetData.Sheets[i].LinkStatus.IsLinked = true;
                            sheetData.Sheets[i].LinkStatus.CurrentLinkedId = linkedSheet.LinkedElementId;
                            sheetData.Sheets[i].LinkStatus.LinkedElementId = viewSheet.Id.IntegerValue;
                            sheetData.Sheets[i].LinkStatus.ToolTip = "Linked Sheet ElementId: " + viewSheet.Id.IntegerValue;

                            //parameter check 
                            string toolTip = "";
                            if (CompareSheetParameters(viewSheet, sheetData.Sheets[i], sheetData, out toolTip))
                            {
                                sheetData.Sheets[i].LinkStatus.Modified = true;
                                sheetData.Sheets[i].LinkStatus.ToolTip = toolTip;
                            }
                            var viewRevisionIds = from elementId in viewSheet.GetAllRevisionIds() select elementId.IntegerValue;
                            //revision check
                            foreach (Guid revisionId in rvtSheet.SheetRevisions.Keys)
                            {
                                RevisionOnSheet ros = rvtSheet.SheetRevisions[revisionId];

                                var revisionFound = from revision in sheetData.Revisions where revision.Id == revisionId select revision;
                                if (revisionFound.Count() > 0)
                                {
                                    RevitRevision rvtRevision = revisionFound.First();
                                    sheetData.Sheets[i].SheetRevisions[revisionId].RvtRevision = rvtRevision;

                                    if (rvtRevision.LinkStatus.IsLinked)
                                    {
                                        if (ros.Include && viewRevisionIds.Contains(rvtRevision.LinkStatus.LinkedElementId))
                                        {
                                            sheetData.Sheets[i].SheetRevisions[revisionId].LinkStatus.IsLinked = true;
                                        }
                                    }
                                }
                                
                            }
                        }
                        else
                        {
                            //item deleted
                            sheetData.Sheets[i].LinkStatus.Modified = true;
                            sheetData.Sheets[i].LinkStatus.ToolTip = "Linked Sheet Not Found.";
                            sheetData.Sheets[i].LinkedSheets.Remove(linkedSheet);
                            //bool linkedSheetDBUpdated = SheetDataWriter.ChangeLinkedSheet(linkedSheet, CommandType.DELETE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check link status of sheets.\n" + ex.Message, "Sheet Manager Updater - Check Sheet Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static bool CompareSheetParameters(ViewSheet viewSheet, RevitSheet rvtSheet, RevitSheetData sheetData, out string tooltip)
        {
            bool modified = false;
            tooltip = "";
            try
            {
                if (viewSheet.SheetNumber != rvtSheet.Number || viewSheet.ViewName != rvtSheet.Name)
                {
                    modified = true;
                    tooltip = "Sheet number or sheet name is different from the linked element.";
                }
                else
                {
                    foreach (SheetParameter sheetParam in sheetData.SheetParameters)
                    {
                        if (rvtSheet.SheetParameters.ContainsKey(sheetParam.ParameterId))
                        {
                            SheetParameterValue paramValue = rvtSheet.SheetParameters[sheetParam.ParameterId];
                            Parameter param = viewSheet.LookupParameter(sheetParam.ParameterName);
                            string paramValueStr = "";
                            if (null != param && param.HasValue)
                            {
                                switch (param.StorageType)
                                {
                                    case StorageType.Double:
                                        paramValueStr = param.AsDouble().ToString();
                                        break;
                                    case StorageType.ElementId:
                                        paramValueStr = param.AsElementId().IntegerValue.ToString();
                                        break;
                                    case StorageType.Integer:
                                        paramValueStr = param.AsInteger().ToString();
                                        break;
                                    case StorageType.String:
                                        paramValueStr = param.AsString();
                                        break;
                                }
                            }
                            if (paramValueStr != paramValue.ParameterValue.ToString())
                            {
                                modified = true;
                                tooltip = sheetParam.ParameterName + " values are different from the linked element.";
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
            return modified;     
        }

        private static void CheckViewLinks(Document doc, Guid projectId, ref RevitSheetData sheetData)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<View> viewsInRevit = collector.OfClass(typeof(View)).WhereElementIsNotElementType().ToElements().Cast<View>().ToList();

                collector = new FilteredElementCollector(doc);
                List<Viewport> viewports = collector.OfCategory(BuiltInCategory.OST_Viewports).ToElements().Cast<Viewport>().ToList();

                for (int i = 0; i < sheetData.Views.Count; i++)
                {
                    RevitView rvtView = sheetData.Views[i];
                    ViewType rvtViewType = (ViewType)Enum.Parse(typeof(ViewType), rvtView.ViewType.ViewType.ToString());
                  
                    var viewFound = from view in viewsInRevit where view.ViewName == rvtView.Name && view.ViewType == rvtViewType select view;
                    if (viewFound.Count() > 0)
                    {
                        View existingView = viewFound.First();

                        sheetData.Views[i].LinkStatus.CurrentLinkedId = existingView.UniqueId;
                        sheetData.Views[i].LinkStatus.LinkedElementId = existingView.Id.IntegerValue;
                        sheetData.Views[i].LinkStatus.ToolTip = "Linked View ElementId: " + existingView.Id.IntegerValue;

                        var viewportFound = from viewport in viewports
                                            where viewport.SheetId.IntegerValue == rvtView.Sheet.LinkStatus.LinkedElementId && viewport.ViewId == existingView.Id
                                            select viewport;
                        if (viewportFound.Count() > 0)
                        {
                            sheetData.Views[i].LinkStatus.IsLinked = true;
                        }
                    }
                    else
                    {
                        sheetData.Views[i].LinkStatus.ToolTip = "The view does not exist.";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check link status of views.\n" + ex.Message, "Sheet Manager Updater - Check View Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static bool InsertRevisionElements(Document doc, Guid projectId, ref RevitSheetData sheetData)
        {
            bool inserted = false;
            try
            {
                var linkedRevisionIds = from revision in sheetData.Revisions 
                                        where revision.LinkStatus.IsLinked && revision.LinkStatus.LinkedElementId != -1 
                                        select revision.LinkStatus.LinkedElementId;

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Revision> revisions = collector.OfCategory(BuiltInCategory.OST_Revisions).WhereElementIsNotElementType().ToElements().Cast<Revision>().ToList();

                var revisionsToInsert = from revision in revisions where !linkedRevisionIds.Contains(revision.Id.IntegerValue) select revision;
                if (revisionsToInsert.Count() > 0)
                {
                    foreach (Revision revision in revisionsToInsert)
                    {
                        bool revisionInserted = InsertNewRevision(revision, projectId, ref sheetData);
                        if (!revisionInserted) { inserted = false; }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert revision elements.\n" + ex.Message, "Sheet Manager Updater - Insert View Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        public static bool InsertNewRevision(Revision revision, Guid projectId, ref RevitSheetData sheetData)
        {
            bool inserted = false;
            try
            {
                RevitRevision rvtRevision = new RevitRevision(Guid.NewGuid(), revision.Description, revision.IssuedBy, revision.IssuedTo, revision.RevisionDate);
                NumberType revNumType = (NumberType)Enum.Parse(typeof(NumberType), revision.NumberType.ToString());
                LinkedRevision linkedRevision = new LinkedRevision(Guid.NewGuid(), rvtRevision.Id, revision.SequenceNumber, revision.RevisionNumber, revNumType, projectId, revision.UniqueId, true);
                rvtRevision.LinkedRevisions.Add(linkedRevision);

                RevitLinkStatus linkStatus = new RevitLinkStatus();
                linkStatus.IsLinked = true;
                linkStatus.CurrentLinkedId = revision.UniqueId;
                linkStatus.LinkedElementId = revision.Id.IntegerValue;
                linkStatus.ToolTip = "Linked Revision ElementId: " + revision.Id.IntegerValue;

                rvtRevision.LinkStatus = linkStatus;

                sheetData.Revisions.Add(rvtRevision);
                bool revisionDBUpdated = SheetDataWriter.ChangeRevisionItem(rvtRevision, CommandType.INSERT);
                bool linkedRevisionDBUpdated = SheetDataWriter.ChangeLinkedRevision(linkedRevision, CommandType.INSERT);

                List<RevisionOnSheet> rosList = new List<RevisionOnSheet>();
                for (int sheetIndex = 0; sheetIndex < sheetData.Sheets.Count; sheetIndex++)
                {
                    RevisionOnSheet ros = new RevisionOnSheet(Guid.NewGuid(), sheetData.Sheets[sheetIndex].Id, rvtRevision, false);
                    sheetData.Sheets[sheetIndex].SheetRevisions.Add(rvtRevision.Id, ros);
                    rosList.Add(ros);
                }

                bool rosDBUpdated = SheetDataWriter.InsertMultipleRevisionOnSheet(rosList);
                inserted = (revisionDBUpdated && linkedRevisionDBUpdated && rosDBUpdated) ? true : false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert a new revision element.\n" + ex.Message, "Insert New Revision", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertSheetElements(Document doc, Guid projectId, ref RevitSheetData sheetData)
        {
            bool inserted = false;
            try
            {
                var linkedSheetIds = from sheet in sheetData.Sheets
                                     where sheet.LinkStatus.IsLinked && sheet.LinkStatus.LinkedElementId != -1
                                     select sheet.LinkStatus.LinkedElementId;

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<ViewSheet> sheets = collector.OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements().Cast<ViewSheet>().ToList();

                var sheetsToInsert = from sheet in sheets where !linkedSheetIds.Contains(sheet.Id.IntegerValue) select sheet;
                if (sheetsToInsert.Count() > 0)
                {
                    foreach (ViewSheet sheet in sheetsToInsert)
                    {
                        bool insertedSheet = InsertNewSheet(sheet, projectId, ref sheetData);
                        if (!insertedSheet) { inserted = false; }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert sheet elements.\n" + ex.Message, "Sheet Manager Updater - Insert Sheet Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        public static bool InsertNewSheet(ViewSheet sheet, Guid projectId, ref RevitSheetData sheetData)
        {
            bool inserted = false;
            try
            {
                var sheetNumberExisting = from rvtSheet in sheetData.Sheets where rvtSheet.Number == sheet.SheetNumber select rvtSheet;
                if (sheetNumberExisting.Count() > 0)
                {
                    RevitSheet rvtSheet = sheetNumberExisting.First();
                    int sheetIndex = sheetData.Sheets.IndexOf(rvtSheet);

                    LinkedSheet linkedSheet = new LinkedSheet(Guid.NewGuid(), rvtSheet.Id, new LinkedProject(projectId), sheet.UniqueId, false);
                    sheetData.Sheets[sheetIndex].LinkedSheets.Add(linkedSheet);

                    bool linkedSheetDBUpdated = SheetDataWriter.ChangeLinkedSheet(linkedSheet, CommandType.INSERT);

                    RevitLinkStatus linkStatus = new RevitLinkStatus();
                    linkStatus.IsLinked = true;
                    linkStatus.CurrentLinkedId = sheet.UniqueId;
                    linkStatus.LinkedElementId = sheet.Id.IntegerValue;
                    linkStatus.ToolTip = "Linked Sheet ElementId: " + sheet.Id.IntegerValue;

                    string toolTip = "";
                    if (CompareSheetParameters(sheet, rvtSheet, sheetData, out toolTip))
                    {
                        linkStatus.Modified = true;
                        linkStatus.ToolTip = toolTip;
                    }

                    sheetData.Sheets[sheetIndex].LinkStatus = linkStatus;
                    inserted = true;
                }
                else
                {
                    RevitSheet rvtSheet = new RevitSheet(Guid.NewGuid(), sheet.SheetNumber, sheet.ViewName);
                    LinkedSheet linkedSheet = new LinkedSheet(Guid.NewGuid(), rvtSheet.Id, new LinkedProject(projectId), sheet.UniqueId, true);
                    rvtSheet.LinkedSheets.Add(linkedSheet);
                    bool sheetDBUpdated = SheetDataWriter.ChangeSheetItem(rvtSheet, CommandType.INSERT);
                    bool linkedSheetDBUpdated = SheetDataWriter.ChangeLinkedSheet(linkedSheet, CommandType.INSERT);

                    RevitLinkStatus linkStatus = new RevitLinkStatus();
                    linkStatus.IsLinked = true;
                    linkStatus.CurrentLinkedId = sheet.UniqueId;
                    linkStatus.LinkedElementId = sheet.Id.IntegerValue;
                    linkStatus.ToolTip = "Linked Sheet ElementId: " + sheet.Id.IntegerValue;

                    rvtSheet.LinkStatus = linkStatus;

                    foreach (SheetParameter sheetParam in sheetData.SheetParameters)
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

                        rvtSheet.SheetParameters.Add(sheetParam.ParameterId, paramValue);
                    }
                    bool sheetParamDBUpdated = SheetDataWriter.InsertMultipleParameterValue(rvtSheet.SheetParameters.Values.ToList());

                    List<RevisionOnSheet> rosList = new List<RevisionOnSheet>();
                    foreach (RevitRevision revision in sheetData.Revisions)
                    {
                        RevisionOnSheet ros = new RevisionOnSheet(Guid.NewGuid(), rvtSheet.Id, revision, false);
                        rvtSheet.SheetRevisions.Add(revision.Id, ros);
                        rosList.Add(ros);
                    }
                    bool rosDBUpdated = SheetDataWriter.InsertMultipleRevisionOnSheet(rosList);
                    sheetData.Sheets.Add(rvtSheet);

                    inserted = (sheetDBUpdated && linkedSheetDBUpdated && rosDBUpdated) ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add a new sheet element.\n" + ex.Message, "New Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

    }
}
