using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Spreadsheets;
using Google.GData.Client;
using System.Windows;
using Google.Documents;
using System.Text.RegularExpressions;
using Google.GData.Documents;
using HOK.ModelManager.ReplicateViews;



namespace HOK.ModelManager.GoogleDocs
{
    public class GoogleDocsUtil
    {
        private GDataCredentials credentials=null;
        private SpreadsheetsService sheetService = null;
        private DocumentsService docService = null;
        private DocumentsRequest docRequest = null;
        private Document projectReplicationFolder = null;
        private Document modelBuilderFolder = null;
        private string[] prHeader;
        private string[] mbHeader;

        private List<TreeViewModel> folderTreeViews = new List<TreeViewModel>();
        private Dictionary<string/*folderKey*/, Document> folderDictionary = new Dictionary<string, Document>();
        private Dictionary<string/*folderKey*/, Dictionary<string/*docKey*/, Document>> docDictionary = new Dictionary<string, Dictionary<string, Document>>();
        private bool googleDocActivated = false;

        public bool GoogleDocActivated { get { return googleDocActivated; } set { googleDocActivated = value; } }
        
        public GoogleDocsUtil()
        {
            if (SetCredentialInfo())
            {
                if (GetDocumentInfo())
                {
                    projectReplicationFolder = GetFolderByTiltle("Project Replication");
                    modelBuilderFolder = GetFolderByTiltle("Model Builder");

                    prHeader = new string[] { "ItemType", "SourceName", "SourcePath", "DestinationPath", "ItemSourceID", "ItemSourceName",	
                        "ItemDestinationID",	"ItemDestinationName",	"ItemDestinationImage1", "ItemDestinationImage2", "LinkModified", "LinkModifiedBy" };
                    mbHeader = new string[] { "ItemType", "SourceName", "SourcePath", "DestinationPath", "ItemSourceID", "ItemSourceName",	
                        "ItemDestinationID",	"ItemDestinationName",	"ItemDestinationImage1", "ItemDestinationImage2", "LinkModified", "LinkModifiedBy" };
                    googleDocActivated = true;
                }
            }
        }
         
        private bool SetCredentialInfo()
        {
            bool result = false;
            try
            {
                sheetService = new SpreadsheetsService("Model Manager");
                sheetService.setUserCredentials("bsmart@hokbuildingsmart.com", "HOKb$mart");

                docService = new DocumentsService("Model Manager");
                docService.setUserCredentials("bsmart@hokbuildingsmart.com", "HOKb$mart");

                credentials = new GDataCredentials("bsmart@hokbuildingsmart.com", "HOKb$mart");
                RequestSettings settings = new RequestSettings("Model Manager", credentials);
                settings.AutoPaging = true;
                settings.PageSize = 100;
                if (null != settings)
                {
                    docRequest = new DocumentsRequest(settings);
                    result = true;
                }
            }
            catch (Exception ex)
            {
              MessageBox.Show("Failed to get Google Docs information.\nPlease check Internet connection.\n"+ex.Message, "Set Google Credential", MessageBoxButton.OK, MessageBoxImage.Warning);
              result = false;
            }
            return result;
        }

        private bool GetDocumentInfo()
        {
            bool result = false;
            try
            {
                Feed<Document> feed = docRequest.GetFolders();
                foreach (Document doc in feed.Entries)
                {
                    if(!folderDictionary.ContainsKey(doc.Self))
                    {
                        folderDictionary.Add(doc.Self, doc);
                    }
                }

                feed = docRequest.GetSpreadsheets();
                foreach (Document doc in feed.Entries)
                {
                    if (doc.ParentFolders.Count > 0)
                    {
                        Dictionary<string, Document> dictionary = new Dictionary<string, Document>();
                        string parentFolderKey = doc.ParentFolders[0];
                        if (docDictionary.ContainsKey(parentFolderKey))
                        {
                            dictionary = docDictionary[parentFolderKey];
                            docDictionary.Remove(parentFolderKey);
                        }
                        dictionary.Add(doc.Id, doc);
                        docDictionary.Add(parentFolderKey, dictionary);
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get folder information.\nPlease check Internet connection.\n"+ ex.Message, "Get Google Docs Information", MessageBoxButton.OK, MessageBoxImage.Information);
                result = false;
            }
            return result;
        }

        private Document GetFolderByTiltle(string title)
        {
            Document folder = null;
            try
            {
                var folders = from afolder in folderDictionary.Values where afolder.Title == title select afolder;
                folder = folders.First();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(title+": Failed to get Google Folder info.\n" + ex.Message, "Get Folder Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return folder;
        }

        public GoogleDocInfo GetGoogleDocInfo(ModelInfo modelInfo, ModelManagerMode mode)
        {
            GoogleDocInfo googleDocInfo = null;
            try
            {
                Document folderDoc = GetGoogleFolder(modelInfo, mode, false);
                Document sheetDoc = null;
                if(null!=folderDoc)
                {
                    if (docDictionary.ContainsKey(folderDoc.Self))
                    {
                        string sheetTitle = (modelInfo.HOKStandard) ? modelInfo.DocTitle : modelInfo.FileIdentifier;

                        var documents = from doc in docDictionary[folderDoc.Self].Values where doc.Title == sheetTitle select doc;
                        if (documents.Count() > 0)
                        {
                            sheetDoc = documents.First();
                        }
                    }

                    if (null != sheetDoc)
                    {
                        googleDocInfo = new GoogleDocInfo(sheetDoc);
                        googleDocInfo.DocTitle = modelInfo.DocTitle;
                        googleDocInfo.FolderName = folderDoc.Title;
                        googleDocInfo.ManagerMode = mode;
                        if (modelInfo.HOKStandard)
                        {
                            googleDocInfo.FileLocation = modelInfo.FileLocation;
                            googleDocInfo.ProjectNumber = modelInfo.ProjectNumber;
                            googleDocInfo.ProjectName = modelInfo.ProjectName;
                        }
                        else
                        {
                            googleDocInfo.CompanyName = modelInfo.CompanyName;
                            googleDocInfo.FileIdentifier = modelInfo.FileIdentifier;
                        }

                        SpreadsheetEntry sheetEntry = GetSpreadsheetEntry(sheetDoc);
                        if (null != sheetEntry)
                        {
                            googleDocInfo.SheetEntry = sheetEntry;
                            googleDocInfo.LinkInfoList = GetLinkInfo(sheetEntry);
                        }
                    }
                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get Google Doc Info from Revit's Model Info.\n"+ex.Message, "Get Google Doc Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return googleDocInfo;
        }

        private GoogleDocInfo GetSpreadsheetInfo(Document spreadsheet)
        {
            GoogleDocInfo docInfo = null;
            try
            {
                docInfo = new GoogleDocInfo(spreadsheet);
                docInfo.SheetEntry = GetSpreadsheetEntry(spreadsheet);
                if (folderDictionary.ContainsKey(docInfo.FolderId))
                {
                    Document folderDoc = folderDictionary[docInfo.FolderId];
                    docInfo.FolderName = folderDoc.Title;

                    //get project info
                    string regPattern = @"([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)";
                    Regex regex = new Regex(regPattern, RegexOptions.IgnoreCase);
                    Match match = regex.Match(docInfo.FolderName);
                    if (match.Success)
                    {
                        docInfo.ProjectNumber = match.Groups[1].Value;
                        docInfo.ProjectName = match.Groups[2].Value;
                    }

                    //get file location
                    if (folderDoc.ParentFolders.Count > 0)
                    {
                        if (folderDictionary.ContainsKey(folderDoc.ParentFolders[0]))
                        {
                            docInfo.FileLocation = folderDictionary[folderDoc.ParentFolders[0]].Title;
                        }
                        if (folderDoc.ParentFolders.Contains(projectReplicationFolder.Id))
                        {
                            docInfo.ManagerMode = ModelManagerMode.ProjectReplication;
                        }
                        else if (folderDoc.ParentFolders.Contains(modelBuilderFolder.Id))
                        {
                            docInfo.ManagerMode = ModelManagerMode.ModelBuilder;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get Google Spreadsheet info.\n" + ex.Message, "Get Spreadsheet Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return docInfo;
        }

        private SpreadsheetEntry GetSpreadsheetEntry(Document doc)
        {
            SpreadsheetEntry sheetEntry = null;
            try
            {
                string[] strSeparators = new string[] { "key=" };
                string docId = doc.ResourceId.Replace("spreadsheet:", "");

                Google.GData.Spreadsheets.SpreadsheetQuery query = new Google.GData.Spreadsheets.SpreadsheetQuery();
                query.Title = doc.Title;


                SpreadsheetFeed feed = sheetService.Query(query);
                if (feed.Entries.Count > 0)
                {
                    foreach (AtomEntry entry in feed.Entries)
                    {
                        SpreadsheetEntry sEntry = (SpreadsheetEntry)entry;
                        string sheetUri = sEntry.AlternateUri.Content;
                        if (sheetUri.Contains(docId))
                        {
                            sheetEntry = sEntry; break;
                        }

                        string[] keys = sEntry.AlternateUri.Content.Split(strSeparators, StringSplitOptions.None);
                        if (keys.Length == 2)
                        {
                            if (doc.Id.Contains(keys[1]))
                            {
                                sheetEntry = sEntry; break;
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get spreadsheet entry.\n"+ex.Message, "Get Spreadsheet Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sheetEntry;
        }

        public GoogleDocInfo CreateSpreadsheet(ModelInfo modelInfo, ModelManagerMode mode)
        {
            GoogleDocInfo docInfo = null;
            try
            {
                Document folderDoc = GetGoogleFolder(modelInfo, mode, true);
                if (null != folderDoc)
                {
                    Document doc = new Document();
                    doc.Type = Document.DocumentType.Spreadsheet;
                    string sheetTitle = (modelInfo.HOKStandard) ? modelInfo.DocTitle : modelInfo.FileIdentifier;
                    doc.Title = sheetTitle;

                    Document spreadsheet = docRequest.Insert<Document>(new Uri(folderDoc.AtomEntry.Content.AbsoluteUri), doc);
                    if (null != spreadsheet)
                    {
                        SpreadsheetEntry sheetEntry = GetSpreadsheetEntry(spreadsheet);
                        if (CreateHeaders(sheetEntry, mode))
                        {
                            //docInfo = GetSpreadsheetInfo(spreadsheet);
                            docInfo = new GoogleDocInfo(spreadsheet);
                            docInfo.DocTitle = modelInfo.DocTitle;
                            docInfo.SheetEntry = sheetEntry;
                            docInfo.FolderName = folderDoc.Title;
                            docInfo.ManagerMode = mode;

                            if (modelInfo.HOKStandard)
                            {
                                docInfo.FileLocation = modelInfo.FileLocation;
                                docInfo.ProjectNumber = modelInfo.ProjectNumber;
                                docInfo.ProjectName = modelInfo.ProjectName;
                            }
                            else
                            {
                                docInfo.CompanyName = modelInfo.CompanyName;
                                docInfo.FileIdentifier = modelInfo.FileIdentifier;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a new spreadsheet.\n"+ex.Message, "Create Spreadsheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return docInfo;
        }

        public bool CreateHeaders(SpreadsheetEntry sheetEntry, ModelManagerMode mode)
        {
            bool result = false;
            try
            {
                string[] headers = (mode == ModelManagerMode.ProjectReplication) ? prHeader : mbHeader;
               
                WorksheetFeed wsFeed = sheetEntry.Worksheets;
                WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];

                CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
                CellFeed cellFeed = sheetService.Query(cellQuery);

                for (int i = 0; i < headers.Length; i++)
                {
                    CellEntry cellEntry = new CellEntry(1, (uint)(i + 1), headers[i]);
                    cellFeed.Insert(cellEntry);
                }

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create headers.\n"+ex.Message, "Create Headers", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private Document CreateFolder(Document parentFolder, string folderName)
        {
            Document folderDoc = null;
            try
            {
                Document doc = new Document();
                doc.Type = Document.DocumentType.Folder;
                doc.Title = folderName;

                folderDoc = docRequest.Insert<Document>(new Uri(parentFolder.AtomEntry.Content.AbsoluteUri), doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(folderName + ": Failed to create folder.\n" + ex.Message, "Create GoogleDoc Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return folderDoc;
        }

        public Document GetGoogleFolder(ModelInfo modelInfo, ModelManagerMode mode, bool create)
        {
            Document folderDocument = null;
            try
            {
                string modeId = (mode == ModelManagerMode.ProjectReplication) ? projectReplicationFolder.Self : modelBuilderFolder.Self;

                if (modelInfo.HOKStandard)
                {
                    string projectName = modelInfo.ProjectNumber + " " + modelInfo.ProjectName;
                    string location = (!string.IsNullOrEmpty(modelInfo.FileLocation)) ? modelInfo.FileLocation : modelInfo.UserLocation;
                    
                    var officeTypes = from folder in folderDictionary.Values where folder.ParentFolders.Contains(modeId) && folder.Title == "HOK Offices" select folder;
                    if (officeTypes.Count() > 0)
                    {
                        Document officeType = officeTypes.First();
                        var offices = from folder in folderDictionary.Values where folder.ParentFolders.Contains(officeType.Self) && folder.Title == location select folder;
                        if (offices.Count() > 0)
                        {
                            Document office = offices.First();
                            var projects = from folder in folderDictionary.Values where folder.ParentFolders.Contains(office.Self) && folder.Title == projectName select folder;
                            if (projects.Count() > 0)
                            {
                                folderDocument = projects.First();
                            }
                            else if (projects.Count() == 0 && create)
                            {
                                folderDocument = CreateFolder(office, projectName);
                            }
                        }
                    }
                }
                else //External Users
                {
                    string companyName=modelInfo.CompanyName;
                    string fileIdentifier=modelInfo.FileIdentifier;
                    
                    var externalUsers = from folder in folderDictionary.Values where folder.ParentFolders.Contains(modeId) && folder.Title == "External Users" select folder;
                    if (externalUsers.Count() > 0)
                    {
                        Document externalUsersFolder = externalUsers.First();
                        var companyNames = from folder in folderDictionary.Values where folder.ParentFolders.Contains(externalUsersFolder.Self) && folder.Title == companyName select folder;
                        if (companyNames.Count() > 0)
                        {
                            folderDocument = companyNames.First();
                        }
                        else if (companyNames.Count() == 0 && create)
                        {
                            folderDocument = CreateFolder(externalUsersFolder, companyName);
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get google Folder.\n"+ex.Message, "GetGoogleFolder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return folderDocument;
        }

        public List<LinkInfo> GetLinkInfo(SpreadsheetEntry sheetEntry)
        {
            List<LinkInfo> linkInfoList = new List<LinkInfo>();
            try
            {
                WorksheetFeed wsFeed = sheetEntry.Worksheets;
                WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];
                AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                ListFeed listFeed = sheetService.Query(listQuery);
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
                    linkInfoList.Add(lf);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(sheetEntry.Title+" Failed to get link item info.\n"+ex.Message, "GetLinkInfo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return linkInfoList;
        }

        public void UpdateAllLinkInfo(SpreadsheetEntry sheetEntry, List<LinkInfo> linkInfoList)
        {
            try
            {
                //insert or find specific value to update
                WorksheetFeed wsFeed = sheetEntry.Worksheets;
                WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];
                AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                ListFeed listFeed = sheetService.Query(listQuery);
                for (int i = listFeed.Entries.Count-1; i >-1 ; i--)
                {
                    ListEntry entry=(ListEntry)listFeed.Entries[i];
                    entry.Delete();
                }

                foreach (LinkInfo info in linkInfoList)
                {
                    ListEntry row = new ListEntry();
                    row.Elements.Add(new ListEntry.Custom(){LocalName="ItemType", Value=info.ItemType.ToString()});
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "SourceName", Value = info.SourceModelName });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "SourcePath", Value = info.SourceModelPath });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "DestinationPath", Value = info.DestModelPath });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "ItemSource_ID", Value = info.SourceItemId.ToString() });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "ItemSource_Name", Value = info.SourceItemName });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "ItemDestination_ID", Value = info.DestItemId.ToString() });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "ItemDestination_Name", Value = info.DestItemName });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "ItemDestination_Image1", Value = info.DestImagePath1 });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "ItemDestination_Image2", Value = info.DestImagePath2});
                    sheetService.Insert(listFeed, row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(sheetEntry.Title+" Failed to update link info.\n"+ex.Message, "Update Link Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool UpdateLinkInfo(SpreadsheetEntry sheetEntry, List<LinkInfo> linkInfoList)
        {
            bool result = false;
            try
            {
                if (null != sheetEntry)
                {
                    //insert or find specific value to update
                    WorksheetFeed wsFeed = sheetEntry.Worksheets;
                    WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];
                    AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());

                    foreach (LinkInfo info in linkInfoList)
                    {
                        try
                        {
                            listQuery.SpreadsheetQuery = "itemsourceid=\"" + info.SourceItemId + "\" and sourcename = \"" + info.SourceModelName + "\"";
                            ListFeed listFeed = sheetService.Query(listQuery);
                            if (listFeed.Entries.Count >0)
                            {
                                ListEntry row = (ListEntry)listFeed.Entries[0];
                                foreach (ListEntry.Custom element in row.Elements)
                                {
                                    switch (element.LocalName)
                                    {
                                        case "itemtype":
                                            element.Value = info.ItemType.ToString();
                                            break;
                                        case "sourcename":
                                            element.Value = info.SourceModelName;
                                            break;
                                        case "sourcepath":
                                            element.Value = info.SourceModelPath;
                                            break;
                                        case "destinationpath":
                                            element.Value = info.DestModelPath;
                                            break;
                                        case "itemsourceid":
                                            element.Value = info.SourceItemId.ToString();
                                            break;
                                        case "itemsourcename":
                                            element.Value = info.SourceItemName;
                                            break;
                                        case "itemdestinationid":
                                            element.Value = info.DestItemId.ToString();
                                            break;
                                        case "itemdestinationname":
                                            element.Value = info.DestItemName;
                                            break;
                                        case "itemdestinationimage1":
                                            element.Value = info.DestImagePath1;
                                            break;
                                        case "itemdestinationimage2":
                                            element.Value = info.DestImagePath2;
                                            break;
                                        case "linkmodified":
                                            info.LinkModified = DateTime.Now.ToString("G");
                                            element.Value = info.LinkModified;
                                            break;
                                        case "linkmodifiedby":
                                            info.LinkModifiedBy = Environment.UserName;
                                            element.Value = info.LinkModifiedBy;
                                            break;
                                    }
                                }
                                row.Update();
                            }
                            else
                            {
                                listQuery = new ListQuery(listFeedLink.HRef.ToString());
                                listFeed = sheetService.Query(listQuery);

                                info.LinkModified = DateTime.Now.ToString("G");
                                info.LinkModifiedBy = Environment.UserName;

                                ListEntry row = new ListEntry();
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "itemtype", Value = info.ItemType.ToString() });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "sourcename", Value = info.SourceModelName });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "sourcepath", Value = info.SourceModelPath });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "destinationpath", Value = info.DestModelPath });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "itemsourceid", Value = info.SourceItemId.ToString() });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "itemsourcename", Value = info.SourceItemName });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "itemdestinationid", Value = info.DestItemId.ToString() });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "itemdestinationname", Value = info.DestItemName });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "itemdestinationimage1", Value = info.DestImagePath1 });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "itemdestinationimage2", Value = info.DestImagePath2 });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "linkmodified", Value = info.LinkModified });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = "linkmodifiedby", Value = info.LinkModifiedBy });
                                sheetService.Insert(listFeed, row);
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(sheetEntry.Title + " Failed to update link info.\n" + ex.Message, "Update Link Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public bool DeleteLinkInfo(SpreadsheetEntry sheetEntry, List<LinkInfo> linkInfoList)
        {
            bool result = false;
            try
            {
                if (null != sheetEntry)
                {
                    //insert or find specific value to update
                    WorksheetFeed wsFeed = sheetEntry.Worksheets;
                    WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];
                    AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());

                    foreach (LinkInfo info in linkInfoList)
                    {
                        try
                        {
                            listQuery.SpreadsheetQuery = "itemsourceid=\"" + info.SourceItemId + "\" and itemdestinationid = \"" + info.DestItemId + "\"";
                            ListFeed listFeed = sheetService.Query(listQuery);
                            if (listFeed.Entries.Count > 0)
                            {
                                ListEntry row = (ListEntry)listFeed.Entries[0];
                                row.Delete();
                            }
                        }
                        catch { }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete link information from Google Doc.\n"+ex.Message, "Delete Link Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }
    }
}
