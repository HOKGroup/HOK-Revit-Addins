using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace HOK.ModelManager.GoogleDocs
{
    public static class GoogleSheetUtil
    {
        public static SpreadsheetsService sheetsService = null;

        private static string userName = "bsmart@hokbuildingsmart.com";
        private static string passWord = "HOKb$mart";

        private static string[] headers = new string[]{ "ItemType", "SourceName", "SourcePath", "SourceModelId", "DestinationPath", "ItemSourceID", "ItemSourceName",	
                        "ItemDestinationID",	"ItemDestinationName",	"ItemDestinationImage1", "ItemDestinationImage2", "LinkModified", "LinkModifiedBy" };

        private static string[] googleHeaders;

        public static ObservableCollection<LinkInfo> GetLinkInfo(string sheetId)
        {
            ObservableCollection<LinkInfo> linkInfoCollection = new ObservableCollection<LinkInfo>();
            try
            {
                WorksheetEntry wsEntry = GetWorkSheetEntry(sheetId);
                if (null != wsEntry)
                {
                    AtomLink listFeedLink = wsEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    ListFeed listFeed = sheetsService.Query(listQuery);
                    foreach (ListEntry row in listFeed.Entries)
                    {
                        LinkInfo lf = new LinkInfo();
                        foreach (ListEntry.Custom element in row.Elements)
                        {
                            switch (element.LocalName)
                            {
                                case "itemtype":
                                    lf.ItemType = lf.GetLinkItemType(element.Value);
                                    break;
                                case "sourcename":
                                    lf.SourceModelName = element.Value;
                                    break;
                                case "sourcepath":
                                    lf.SourceModelPath = element.Value;
                                    break;
                                case "sourcemodelid":
                                    lf.SourceModelId = element.Value;
                                    break;
                                case "destinationpath":
                                    lf.DestModelPath = element.Value;
                                    break;
                                case "itemsourceid":
                                    lf.SourceItemId = int.Parse(element.Value);
                                    break;
                                case "itemsourcename":
                                    lf.SourceItemName = element.Value;
                                    break;
                                case "itemdestinationid":
                                    lf.DestItemId = int.Parse(element.Value);
                                    break;
                                case "itemdestinationname":
                                    lf.DestItemName = element.Value;
                                    break;
                                case "itemdestinationimage1":
                                    lf.DestImagePath1 = element.Value;
                                    break;
                                case "itemdestinationimage2":
                                    lf.DestImagePath2 = element.Value;
                                    break;
                            }
                        }
                        linkInfoCollection.Add(lf);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get link information.\n" + ex.Message, "Get Link Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkInfoCollection;
        }

        private static SpreadsheetsService GetUserCredential()
        {
            SpreadsheetsService sheetService = null;
            try
            {
                sheetService = new SpreadsheetsService("HOK Project Replicator");
                sheetService.setUserCredentials(userName, passWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get user credential.\n"+ex.Message, "Google Spreadsheet : User Credential", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sheetService;
        }

        private static WorksheetEntry GetWorkSheetEntry(string sheetId)
        {
            WorksheetEntry wsEntry = null;
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCredential();
                }

                if(null != sheetsService)
                {
                    WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + sheetId + "/private/full");
                    WorksheetFeed wsFeed = sheetsService.Query(worksheetquery);

                    if (wsFeed.Entries.Count > 0)
                    {
                        wsEntry = (WorksheetEntry)wsFeed.Entries[0];
                    }

                    if (null != wsEntry)
                    {
                        //create headers if missing
                        bool updatedHeaders = UpdateHeaders(wsEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get Google Worksheet.\n" + ex.Message, "Get Work Sheet Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
               
            }
            return wsEntry;
        }

        private static bool UpdateHeaders(WorksheetEntry wsEntry)
        {
            bool updated = false;
            try
            {
                googleHeaders = new string[headers.Length];

                CellQuery cellQuery = new CellQuery(wsEntry.CellFeedLink);
                cellQuery.MaximumRow = 1;
                cellQuery.MinimumRow = 1;
                cellQuery.MinimumColumn = 1;
                cellQuery.MaximumColumn = (uint)headers.Length;

                CellFeed cellFeed = sheetsService.Query(cellQuery);
                if (cellFeed.Entries.Count > 0)
                {
                    for (int i = 0; i < cellFeed.Entries.Count; i++)
                    {
                        CellEntry cell = (CellEntry)cellFeed.Entries[i];
                        googleHeaders[i] = cell.InputValue;
                    }

                    if (!googleHeaders.Contains("SourceModelId"))
                    {
                        string headerText = "SourceModelId"; // new column
                        CellEntry cell = new CellEntry(1, (uint)headers.Length, headerText);
                        cellFeed.Insert(cell);
                        googleHeaders[headers.Length - 1] = headerText;
                    }
                }
                else
                {
                    for (int i = 0; i < headers.Length; i++)
                    {
                        string headerText = headers[i];
                        CellEntry cell = new CellEntry(1, Convert.ToUInt16(i + 1), headerText);
                        cellFeed.Insert(cell);
                    }
                    updated = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update headers in Google Worksheet.\n"+ex.Message, "Update Headers", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        public static bool WriteLinkIfo(string sheetId, ObservableCollection<LinkInfo> linkInfoCollection)
        {
            bool result = false;
            try
            {
                WorksheetEntry wsEntry = GetWorkSheetEntry(sheetId);

                if (null != wsEntry)
                {
                    var toBeDeleted = from entry in linkInfoCollection where entry.ItemStatus == LinkItemStatus.Deleted select entry;
                    if (toBeDeleted.Count() > 0)
                    {
                        result = DeleteLinkInfo(wsEntry, toBeDeleted.ToList());
                    }

                    var toBeUpdated = from entry in linkInfoCollection where entry.ItemStatus == LinkItemStatus.Updated select entry;
                    if (toBeUpdated.Count() > 0)
                    {
                        result = UpdateLinkInfo(wsEntry, toBeUpdated.ToList());
                    }

                    var toBeAdded = from entry in linkInfoCollection where entry.ItemStatus == LinkItemStatus.Added select entry;
                    if (toBeAdded.Count() > 0)
                    {
                        result = AddLinkInfo(wsEntry, toBeAdded.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write the link information.\n"+ex.Message, "Write Link Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        private static bool DeleteLinkInfo(WorksheetEntry wsEntry, List<LinkInfo> linkToDelete)
        {
            bool deleted = false;
            try
            {
                LinkInfo firstLink = linkToDelete.First();
                
                AtomLink listFeedLink = wsEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                listQuery.SpreadsheetQuery = "sourcemodelid = \"" + firstLink.SourceModelId + "\"";
                ListFeed listFeed = sheetsService.Query(listQuery);
                if (listFeed.Entries.Count > 0)
                {
                    foreach (ListEntry row in listFeed.Entries)
                    {
                        try
                        {
                            string itemSourceId = row.Elements[Array.IndexOf(googleHeaders, "ItemSourceID")].Value;
                            string itemDestId = row.Elements[Array.IndexOf(googleHeaders, "ItemDestinationID")].Value;

                            var linkFound = from linkInfo in linkToDelete where linkInfo.SourceItemId.ToString() == itemSourceId && linkInfo.DestItemId.ToString() == itemDestId select linkInfo;
                            if (linkFound.Count() > 0)
                            {
                                row.Delete();
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete link information.\n"+ex.Message, "Delete Link Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deleted;
        }

        private static bool UpdateLinkInfo(WorksheetEntry wsEntry, List<LinkInfo> linkToUpdate)
        {
            bool updated = false;
            try
            {
                LinkInfo firstLinkInfo = linkToUpdate.First();

                AtomLink listFeedLink = wsEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                listQuery.SpreadsheetQuery = "sourcemodelid = \"" + firstLinkInfo.SourceModelId + "\"";

                ListFeed listFeed = sheetsService.Query(listQuery);
                if (listFeed.Entries.Count > 0)
                {
                    foreach (ListEntry row in listFeed.Entries)
                    {
                        string itemSourceId = row.Elements[Array.IndexOf(googleHeaders, "ItemSourceID")].Value;
                        var linkFound = from linkInfo in linkToUpdate where linkInfo.SourceItemId.ToString() == itemSourceId select linkInfo;
                        if (linkFound.Count() > 0)
                        {
                            try
                            {
                                LinkInfo linkInfo = linkFound.First();
                                row.Elements[Array.IndexOf(googleHeaders, "ItemType")].Value = linkInfo.ItemType.ToString();
                                row.Elements[Array.IndexOf(googleHeaders, "SourceName")].Value = linkInfo.SourceModelName;
                                row.Elements[Array.IndexOf(googleHeaders, "SourcePath")].Value = linkInfo.SourceModelPath;
                                row.Elements[Array.IndexOf(googleHeaders, "DestinationPath")].Value = linkInfo.DestModelPath;
                                row.Elements[Array.IndexOf(googleHeaders, "ItemSourceName")].Value = linkInfo.SourceItemName;
                                row.Elements[Array.IndexOf(googleHeaders, "ItemDestinationID")].Value = linkInfo.DestItemId.ToString();
                                row.Elements[Array.IndexOf(googleHeaders, "ItemDestinationName")].Value = linkInfo.DestItemName;
                                row.Elements[Array.IndexOf(googleHeaders, "ItemDestinationImage1")].Value = linkInfo.DestImagePath1;
                                row.Elements[Array.IndexOf(googleHeaders, "ItemDestinationImage2")].Value = linkInfo.DestImagePath2;
                                row.Elements[Array.IndexOf(googleHeaders, "LinkModified")].Value = linkInfo.LinkModified;
                                row.Elements[Array.IndexOf(googleHeaders, "LinkModifiedBy")].Value = linkInfo.LinkModifiedBy;
                                row.Update();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                            }
                        }
                    }
                    updated = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the link information.\n"+ex.Message, "Update Link Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        private static bool AddLinkInfo(WorksheetEntry wsEntry, List<LinkInfo> linkToAdd)
        {
            bool added = false;
            try
            {
                AtomLink listFeedLink = wsEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                ListFeed listFeed = sheetsService.Query(listQuery);

                foreach (LinkInfo info in linkToAdd)
                {
                    ListEntry row = new ListEntry();
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "itemtype", Value = info.ItemType.ToString() });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "sourcename", Value = info.SourceModelName });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "sourcepath", Value = info.SourceModelPath });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "sourcemodelid", Value = info.SourceModelId });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "destinationpath", Value = info.DestModelPath });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "itemsourceid", Value = info.SourceItemId.ToString() });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "itemsourcename", Value = info.SourceItemName });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "itemdestinationid", Value = info.DestItemId.ToString() });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "itemdestinationname", Value = info.DestItemName });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "itemdestinationimage1", Value = info.DestImagePath1 });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "itemdestinationimage2", Value = info.DestImagePath2 });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "linkmodified", Value = info.LinkModified });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "linkmodifiedby", Value = info.LinkModifiedBy });
                    sheetsService.Insert(listFeed, row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add the link information.\n"+ex.Message, "Add Link Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return added;
        }

    }
}
