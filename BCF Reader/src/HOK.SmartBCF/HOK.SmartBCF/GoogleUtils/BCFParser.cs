using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Google.Apis.Auth.OAuth2;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.GoogleUtils
{
    public enum ModifyItem
    {
        Add=0,
        Edit=1,
        Delete=2
    }
    public static class BCFParser
    {
        public static SpreadsheetsService sheetsService = null;
        public static string colorSheetId = "";
        public static WorksheetEntry colorSheet = null;
        public static string currentSheetId = ""; //fileId of Google Spreadsheet
        public static WorksheetEntry currentSheet = null; //worksheet of ViewPoint

        //private static string userName = "bsmart@hokbuildingsmart.com";
        //private static string passWord = "HOKb$mart";

        private static string keyFile = "HOK smartBCF.p12";
        private static string serviceAccountEmail = "756603983986-lrc8dm2b0nl381cepd60q2o7fo8df3bg@developer.gserviceaccount.com";

        private static string[] markupCols = new string[] { "IssueGuid", "IssueTopic", "CommentGuid", "Comment", "Status", "VerbalStatus", "Author", "Date" };
        private static string[] viewpointCols = new string[] { "IssueGuid", "ComponentIfcGuid", "AuthoringToolId", "Action", "Responsible" };
        private static string[] colorschemeCols = new string[] { "ColorSchemeId", "SchemeName", "ParameterName", "ParameterValue", "ColorR", "ColorG", "ColorB"};
        private static string[] categoryCols = new string[] { "CategoryName" };

        private static Random random = new Random();

        private static SpreadsheetsService GetUserCredential()
        {
            SpreadsheetsService service = null;
            try
            {
                string currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string currentDirectory = System.IO.Path.GetDirectoryName(currentAssembly);
                string keyFilePath = System.IO.Path.Combine(currentDirectory, "Resources\\" + keyFile);

                var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(new
                ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = new[] { "https://spreadsheets.google.com/feeds/" }
                }.FromCertificate(certificate));

                credential.RequestAccessTokenAsync(System.Threading.CancellationToken.None).Wait();

                var requestFactory = new GDataRequestFactory("My App User Agent");
                requestFactory.CustomHeaders.Add(string.Format("Authorization: Bearer {0}", credential.Token.AccessToken));

                service = new SpreadsheetsService("HOK smartBCF");
                service.RequestFactory = requestFactory;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get user credential.\n"+ex.Message, "Get User Credential", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return service;
        }

        public static System.IO.MemoryStream CreateColorSchemeStream(ColorSchemeInfo colorSchemeInfo)
        {
            System.IO.MemoryStream csvStream = null;
            try
            {
                StringBuilder csvBuilder = new StringBuilder();
                //write columns
                for (int i = 0; i < colorschemeCols.Length; i++)
                {
                    if (i != 0)
                    {
                        csvBuilder.Append(',');
                    }

                    csvBuilder.Append(markupCols[i]);
                }
                csvBuilder.Append("\n");

                char[] specialChar = new char[] { '"', ',' };
                foreach (ColorScheme scheme in colorSchemeInfo.ColorSchemes)
                {
                    string schemeId = scheme.SchemeId;
                    string schemeName = scheme.SchemeName;
                    string paramName = scheme.ParameterName;

                    foreach (ColorDefinition definition in scheme.ColorDefinitions)
                    {
                        string[] strArray = new string[] { schemeId, schemeName, paramName, definition.ParameterValue, 
                            definition.Color[0].ToString(), definition.Color[1].ToString(), definition.Color[2].ToString() };

                        StringBuilder rowBuilder = new StringBuilder();
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            if (i != 0) { rowBuilder.Append(','); }
                            if (strArray[i].IndexOfAny(specialChar) != -1)
                            {
                                rowBuilder.AppendFormat("\"{0}\"", strArray[i].Replace("\"", "\"\""));
                            }
                            else
                            {
                                rowBuilder.Append(strArray[i]);
                            }
                        }
                        csvBuilder.AppendLine(rowBuilder.ToString());
                    }
                }

                if (csvBuilder.ToString().Length > 0)
                {
                    byte[] csvByteArray = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
                    csvStream = new System.IO.MemoryStream(csvByteArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write Color Scheme CSV.\n"+ex.Message, "Create Color Scheme Stream", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return csvStream;
        }

        public static System.IO.MemoryStream CreateCategoryStream(List<string> categoryNames)
        {
            System.IO.MemoryStream csvStream = null;
            try
            {
                StringBuilder csvBuilder = new StringBuilder();
                //write columns
                csvBuilder.AppendLine(categoryCols[0]);

                char[] specialChar = new char[] { '"', ',' };
                foreach (string categoryName in categoryNames)
                {
                    csvBuilder.AppendLine(categoryName);
                }

                if (csvBuilder.ToString().Length > 0)
                {
                    byte[] csvByteArray = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
                    csvStream = new System.IO.MemoryStream(csvByteArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write Element Categories CSV\n" + ex.Message, "Write Element Category CSV", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return csvStream;
        }

        public static System.IO.MemoryStream CreateMarkupStream(BCFZIP bcfZip)
        {
            System.IO.MemoryStream csvStream = null;
            try
            {
                StringBuilder csvBuilder = new StringBuilder();
                //write columns
                for (int i = 0; i < markupCols.Length; i++)
                {
                    if (i != 0)
                    {
                        csvBuilder.Append(',');
                    }

                    csvBuilder.Append(markupCols[i]);
                }
                csvBuilder.Append("\n");

                char[] specialChar = new char[] { '"', ','};
                foreach (BCFComponent bcf in bcfZip.BCFComponents)
                {
                    Topic bcfTopic = bcf.Markup.Topic;
                    if (null != bcf.Markup.Comment)
                    {
                        foreach (Comment comment in bcf.Markup.Comment)
                        {
                            string[] strArray = new string[] { bcfTopic.Guid, bcfTopic.Title, 
                                comment.Guid, comment.Comment1, comment.Status, comment.VerbalStatus, comment.Author, comment.Date.ToString() };

                            StringBuilder rowBuilder = new StringBuilder();

                            for (int i = 0; i < strArray.Length; i++)
                            {
                                if (i != 0) { rowBuilder.Append(','); }
                                if (strArray[i].IndexOfAny(specialChar) != -1)
                                {
                                    rowBuilder.AppendFormat("\"{0}\"", strArray[i].Replace("\"", "\"\""));
                                }
                                else
                                {
                                    rowBuilder.Append(strArray[i]);
                                }
                            }
                            csvBuilder.AppendLine(rowBuilder.ToString());
                        }
                    }
                }

                if (csvBuilder.ToString().Length > 0)
                {
                    byte[] csvByteArray = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
                    csvStream = new System.IO.MemoryStream(csvByteArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write Markup to CSV\n" + ex.Message, "Write Markup CSV", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return csvStream;
        }

        public static System.IO.MemoryStream CreateViewpointStream(BCFZIP bcfZip)
        {
            System.IO.MemoryStream csvStream = null;
            try
            {
                StringBuilder csvBuilder = new StringBuilder();
                //write columns
                for (int i = 0; i < viewpointCols.Length; i++)
                {
                    if (i != 0)
                    {
                        csvBuilder.Append(',');
                    }
                    csvBuilder.Append(viewpointCols[i]);
                }
                csvBuilder.Append("\n");

                char[] specialChar = new char[] { '"', ',' };
                foreach (BCFComponent bcf in bcfZip.BCFComponents)
                {
                    foreach (Component component in bcf.VisualizationInfo.Components)
                    {
                        string[] strArray = new string[] { bcf.GUID, component.IfcGuid, component.AuthoringToolId, "MOVE", "ARCHITECTURE" };

                        StringBuilder rowBuilder = new StringBuilder();
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            if (i != 0) { rowBuilder.Append(','); }
                            if (strArray[i].IndexOfAny(specialChar) != -1)
                            {
                                rowBuilder.AppendFormat("\"{0}\"", strArray[i].Replace("\"", "\"\""));
                            }
                            else
                            {
                                rowBuilder.Append(strArray[i]);
                            }
                        }
                        csvBuilder.AppendLine(rowBuilder.ToString());
                    }
                }

                if (csvBuilder.ToString().Length > 0)
                {
                    byte[] csvByteArray = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
                    csvStream = new System.IO.MemoryStream(csvByteArray);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write ViewPoint to CSV.\n"+ex.Message, "Write Viewpoint CSV", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return csvStream;
        }

        private static bool CreateMarkupSheet(BCFZIP bcfZip, string fileId, out WorksheetFeed wsFeed)
        {
            bool created = false;
            wsFeed = null;
            try
            {
                WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                wsFeed = sheetsService.Query(worksheetquery);
                if (wsFeed.Entries.Count > 0)
                {
                    WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];
                    worksheet.Title.Text = "MarkUp";
                    worksheet.Update();

                    CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
                    CellFeed cellFeed = sheetsService.Query(cellQuery);
                    
                    for (int i = 0; i < markupCols.Length; i++)
                    {
                        string colText = markupCols[i];
                        CellEntry cell = new CellEntry(1, Convert.ToUInt16(i+1), colText);
                        cellFeed.Insert(cell);
                    }

                    //write issue data
                    AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    ListFeed listFeed = sheetsService.Query(listQuery);

                    // "Issue Guid", "Issue Topic"
                    foreach (BCFComponent bcf in bcfZip.BCFComponents)
                    {
                        Topic bcfTopic = bcf.Markup.Topic;
                        if (null != bcf.Markup.Comment)
                        {
                            foreach (Comment comment in bcf.Markup.Comment)
                            {
                                ListEntry row = new ListEntry();
                                row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[0].ToLower(), Value = bcfTopic.Guid });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[1].ToLower(), Value = bcfTopic.Title });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[2].ToLower(), Value = comment.Guid });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[3].ToLower(), Value = comment.Comment1 });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[4].ToLower(), Value = comment.Status });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[5].ToLower(), Value = comment.VerbalStatus });
                                row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[6].ToLower(), Value = comment.Author});
                                row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[7].ToLower(), Value = comment.Date.ToString() });
                                
                                try
                                {
                                    ListEntry insertedRow = sheetsService.Insert(listFeed, row);
                                }
                                catch { System.Threading.Thread.Sleep(500); }
                            }
                        }
                    }

                    created = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create markup sheet.\n"+ex.Message, "Create Markup Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        private static bool CreateViewSheet(BCFZIP bcfZip, WorksheetFeed wsFeed)
        {
            bool created = false;
            try
            {
                WorksheetEntry wsEntry = new WorksheetEntry(10000, 10, "ViewPoint");
                WorksheetEntry worksheet = sheetsService.Insert(wsFeed, wsEntry);

                if (null != worksheet)
                {
                    CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
                    CellFeed cellFeed = sheetsService.Query(cellQuery);

                    //write headers
                    for (int i = 0; i < viewpointCols.Length; i++)
                    {
                        string colText = viewpointCols[i];
                        CellEntry cell = new CellEntry(1, Convert.ToUInt16(i + 1), colText);
                        cellFeed.Insert(cell);
                    }

                    //write issue data
                    AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    ListFeed listFeed = sheetsService.Query(listQuery);

                    // "Issue Guid", "Component IfcGuid", "Authoring Tool Id"
                    foreach (BCFComponent bcf in bcfZip.BCFComponents)
                    {
                        foreach (Component component in bcf.VisualizationInfo.Components)
                        {
                            ListEntry row = new ListEntry();
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[0].ToLower(), Value = bcf.GUID });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[1].ToLower(), Value = component.IfcGuid });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[2].ToLower(), Value = component.AuthoringToolId });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[3].ToLower(), Value = "MOVE" });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[4].ToLower(), Value = "ARCHITECTURE" });

                            try
                            {
                                ListEntry insertedRow = sheetsService.Insert(listFeed, row);
                            }
                            catch { System.Threading.Thread.Sleep(500); }
                        }
                    }
                    created = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create viewpoint sheet.\n"+ex.Message, "Create Viewpoint Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        private static WorksheetEntry FindColorSheet(string sheetId)
        {
            WorksheetEntry wsEntry = null;
            try
            {
                try
                {
                    WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + colorSheetId + "/private/full");
                    WorksheetFeed wsFeed = sheetsService.Query(worksheetquery);

                    if (wsFeed.Entries.Count > 0)
                    {
                        WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];
                        worksheet.Title.Text = "ColorScheme";
                        wsEntry = (WorksheetEntry)worksheet.Update();
                    }
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                if (null != wsEntry)
                {
                    //create headers
                    CellQuery cellQuery = new CellQuery(wsEntry.CellFeedLink);
                    CellFeed cellFeed = sheetsService.Query(cellQuery);

                    for (int i = 0; i < colorschemeCols.Length; i++)
                    {
                        string colText = colorschemeCols[i];
                        CellEntry cell = new CellEntry(1, Convert.ToUInt16(i + 1), colText);
                        cellFeed.Insert(cell);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to fine color shcemes spreadsheet.\n" + ex.Message, "Find Color Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wsEntry;
        }

        public static bool WriteColorSheet(ColorSchemeInfo colorSchemeInfo, string sheetId)
        {
            bool result = false;
            try
            {
                if (colorSheetId != sheetId)
                {
                    //first time colorsheet access
                    colorSheetId = sheetId;
                    colorSheet = FindColorSheet(colorSheetId);
                }
                
                //Update color schemes
                if (null != colorSheet)
                {
                    AtomLink listFeedLink = colorSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    ListFeed listFeed = sheetsService.Query(listQuery);

                    for (int i = listFeed.Entries.Count - 1; i > -1; i--)
                    {
                        ListEntry row = (ListEntry)listFeed.Entries[i];
                        row.Delete();
                    }

                    // "ColorSchemeId", "SchemeName", "ParameterName", "ParameterValue", "ColorValue"
                    foreach (ColorScheme scheme in colorSchemeInfo.ColorSchemes)
                    {
                        string schemeId = scheme.SchemeId;
                        string schemeName = scheme.SchemeName;
                        string paramName = scheme.ParameterName;

                        foreach (ColorDefinition definition in scheme.ColorDefinitions)
                        {
                            ListEntry row = new ListEntry();
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[0].ToLower(), Value = schemeId });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[1].ToLower(), Value = schemeName });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[2].ToLower(), Value = paramName });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[3].ToLower(), Value = definition.ParameterValue });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[4].ToLower(), Value = definition.Color[0].ToString() });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[5].ToLower(), Value = definition.Color[1].ToString() });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[6].ToLower(), Value = definition.Color[2].ToString() });
                            sheetsService.Insert(listFeed, row);
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update common color sheet.\n"+ex.Message, "Update Color Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static bool UpdateColorSheet(ColorScheme colorScheme, ColorDefinition oldDefinition, ColorDefinition newDefinition, ModifyItem modifyItem, string sheetId)
        {
            bool result = false;
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCredential();
                }

                if (null != sheetsService)
                {
                    if (colorSheetId != sheetId)
                    {
                        colorSheetId = sheetId;
                        colorSheet = FindColorSheet(colorSheetId);
                    }
                    if (null != colorSheet)
                    {
                        AtomLink listFeedLink = colorSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                        ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                        ListFeed listFeed = sheetsService.Query(listQuery);

                        if (modifyItem == ModifyItem.Add)
                        {
                            ListEntry row = new ListEntry();
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[0].ToLower(), Value = colorScheme.SchemeId });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[1].ToLower(), Value = colorScheme.SchemeName });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[2].ToLower(), Value = colorScheme.ParameterName });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[3].ToLower(), Value = newDefinition.ParameterValue });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[4].ToLower(), Value = newDefinition.Color[0].ToString() });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[5].ToLower(), Value = newDefinition.Color[1].ToString() });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = colorschemeCols[6].ToLower(), Value = newDefinition.Color[2].ToString() });
                            sheetsService.Insert(listFeed, row);
                        }
                        else
                        {
                            ListEntry rowFound = null;
                            for (int i = listFeed.Entries.Count - 1; i > -1; i--)
                            {
                                ListEntry row = (ListEntry)listFeed.Entries[i];
                                if (row.Elements[0].Value == colorScheme.SchemeId && row.Elements[3].Value == oldDefinition.ParameterValue)
                                {
                                    rowFound = row;
                                    break;
                                }
                            }
                            if (null != rowFound)
                            {
                                if (modifyItem == ModifyItem.Edit)
                                {
                                    rowFound.Elements[3].Value = newDefinition.ParameterValue;
                                    rowFound.Elements[4].Value = newDefinition.Color[0].ToString();
                                    rowFound.Elements[5].Value = newDefinition.Color[1].ToString();
                                    rowFound.Elements[6].Value = newDefinition.Color[2].ToString();
                                    rowFound.Update();
                                }
                                else if (modifyItem == ModifyItem.Delete)
                                {
                                    rowFound.Delete();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update color sheet.\n"+ex.Message, "Update Color Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static bool UpdateCommonCategorySheet(List<string> categoryNames, string colorSheetId)
        {
            bool result = false;
            try
            {
                SpreadsheetQuery sheetQuery = new SpreadsheetQuery();
                sheetQuery.Title = "Color Schemes";

                SpreadsheetFeed feed = sheetsService.Query(sheetQuery);
                WorksheetEntry categorySheet = null;
                foreach (SpreadsheetEntry sheet in feed.Entries)
                {
                    if (sheet.Id.AbsoluteUri.Contains(colorSheetId))
                    {
                        WorksheetFeed wsFeed = sheet.Worksheets;
                        if (wsFeed.Entries.Count > 0)
                        {
                            foreach (WorksheetEntry entry in wsFeed.Entries)
                            {
                                if (entry.Title.Text == "ElementCategories")
                                {
                                    categorySheet = entry;
                                }
                            }
                        }

                        if (null == categorySheet)
                        {
                            //crate element categories work sheet
                            WorksheetEntry wsEntry = new WorksheetEntry(10000, 10, "ElementCategories");
                            categorySheet = sheetsService.Insert(wsFeed, wsEntry);
                            if (null != categorySheet)
                            {
                                CellQuery cellQuery = new CellQuery(categorySheet.CellFeedLink);
                                CellFeed cellFeed = sheetsService.Query(cellQuery);

                                //write headers
                                for (int i = 0; i < categoryCols.Length; i++)
                                {
                                    string colText = categoryCols[i];
                                    CellEntry cell = new CellEntry(1, Convert.ToUInt16(i + 1), colText);
                                    cellFeed.Insert(cell);
                                }
                            }
                        }

                        //Update color schemes
                        if (null != categorySheet)
                        {
                            AtomLink listFeedLink = categorySheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                            ListFeed listFeed = sheetsService.Query(listQuery);

                            List<string> existingCategories = new List<string>();
                            foreach (ListEntry row in listFeed.Entries)
                            {
                                string catName = row.Elements[0].Value;

                                if (!existingCategories.Contains(catName))
                                {
                                    existingCategories.Add(catName);
                                }
                            }

                            foreach (string categoryName in categoryNames)
                            {
                                if (existingCategories.Contains(categoryName)) { continue; }

                                ListEntry row = new ListEntry();
                                row.Elements.Add(new ListEntry.Custom() { LocalName = categoryCols[0].ToLower(), Value = categoryName });
                                sheetsService.Insert(listFeed, row);
                            }

                        }

                        break;
                    }
                }
                result = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to update common category sheet.\n"+ex.Message, "Update Common Category Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static ColorSchemeInfo CreateDefaultSchemeInfo()
        {
            ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
            try
            {
                ColorScheme aScheme = new ColorScheme(); //Action
                aScheme.SchemeName = "BCF Action";
                aScheme.SchemeId = Guid.NewGuid().ToString();
                aScheme.ParameterName = "BCF_Action";
                aScheme.DefinitionBy = DefinitionType.ByValue;

                ColorDefinition cd = new ColorDefinition();
                cd.ParameterValue = "DELETE";
                cd.Color = GetRandomColor();
                aScheme.ColorDefinitions.Add(cd);

                cd = new ColorDefinition();
                cd.ParameterValue = "MOVE";
                cd.Color = GetRandomColor();
                aScheme.ColorDefinitions.Add(cd);

                cd = new ColorDefinition();
                cd.ParameterValue = "ADD";
                cd.Color = GetRandomColor();
                aScheme.ColorDefinitions.Add(cd);

                cd = new ColorDefinition();
                cd.ParameterValue = "CHANGE TYPE";
                cd.Color = GetRandomColor();
                aScheme.ColorDefinitions.Add(cd);

                schemeInfo.ColorSchemes.Add(aScheme);

                ColorScheme rScheme = new ColorScheme(); //Responsibility
                rScheme.SchemeName = "BCF Responsibility";
                rScheme.SchemeId = Guid.NewGuid().ToString();
                rScheme.ParameterName = "BCF_Responsibility";
                rScheme.DefinitionBy = DefinitionType.ByValue;

                cd = new ColorDefinition();
                cd.ParameterValue = "ARCHITECTURE";
                cd.Color = GetRandomColor();
                rScheme.ColorDefinitions.Add(cd);

                cd = new ColorDefinition();
                cd.ParameterValue = "STRUCTURE";
                cd.Color = GetRandomColor();
                rScheme.ColorDefinitions.Add(cd);

                cd = new ColorDefinition();
                cd.ParameterValue = "MEP";
                cd.Color = GetRandomColor();
                rScheme.ColorDefinitions.Add(cd);

                schemeInfo.ColorSchemes.Add(rScheme);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create default color scheme info.\n"+ex.Message, "Create Default Scheme Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schemeInfo;
        }

        private static byte[] GetRandomColor()
        {
            byte[] colorBytes = new byte[3];
            colorBytes[0] = (byte)random.Next(256);
            colorBytes[1] = (byte)random.Next(256);
            colorBytes[2] = (byte)random.Next(256);
            return colorBytes;
        }

        public static Dictionary<string, IssueEntry> ReadIssues(string fileId, string fileName)
        {
            Dictionary<string/*issueId*/, IssueEntry> issueDictionary = new Dictionary<string, IssueEntry>();
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCredential();
                }
                if (null != sheetsService)
                {
                    WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                    WorksheetFeed wsFeed = sheetsService.Query(worksheetquery);
                    if (wsFeed.Entries.Count > 1)
                    {
                        WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0]; //markup sheet
                        AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                        ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                        ListFeed listFeed = sheetsService.Query(listQuery);

                        foreach (ListEntry row in listFeed.Entries)
                        {
                            IssueEntry issueEntry = new IssueEntry();
                            issueEntry.BCFName = fileName;
                            
                            try
                            {
                                string issueId = row.Elements[0].Value;
                                string issueTopic = row.Elements[1].Value;
                                string commentId = row.Elements[2].Value;
                                string commentStr = row.Elements[3].Value;
                                string status = row.Elements[4].Value;
                                string verbalStatus = row.Elements[5].Value;
                                string author = row.Elements[6].Value;
                                string date = row.Elements[7].Value;

                                issueEntry.IssueId = issueId;
                                issueEntry.IssueTopic = issueTopic;

                                Comment comment = new Comment();
                                comment.Topic.Guid = issueId;
                                comment.Guid = commentId;
                                comment.Comment1 = commentStr;
                                comment.Status = status;
                                comment.VerbalStatus = verbalStatus;
                                comment.Author = author;
                                comment.Date = DateTime.Parse(date);
                               
                                if (!issueDictionary.ContainsKey(issueEntry.IssueId))
                                {
                                    issueEntry.CommentDictionary.Add(comment.Guid, comment);
                                    issueDictionary.Add(issueEntry.IssueId, issueEntry);
                                }
                                else
                                {
                                    issueDictionary[issueEntry.IssueId].CommentDictionary.Add(comment.Guid, comment);
                                }
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                                continue;
                            }
                        }

                        if (issueDictionary.Count > 0)
                        {
                            //viewInfo sheet
                            worksheet = (WorksheetEntry)wsFeed.Entries[1]; 
                            listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                            listQuery = new ListQuery(listFeedLink.HRef.ToString());
                            listFeed = sheetsService.Query(listQuery);

                            foreach (ListEntry row in listFeed.Entries)
                            {
                                try
                                {
                                    string issueId = row.Elements[0].Value;
                                    int elementId = int.Parse(row.Elements[2].Value);
                                    string action = row.Elements[3].Value;
                                    string responsibleParty = row.Elements[4].Value;

                                    ElementProperties ep = new ElementProperties(elementId);
                                    ep.IssueId = issueId;
                                    ep.Action = action;
                                    ep.ResponsibleParty = responsibleParty;
                                    if (issueDictionary.ContainsKey(issueId))
                                    {
                                        if (!issueDictionary[issueId].ElementDictionary.ContainsKey(elementId))
                                        {
                                            issueDictionary[issueId].ElementDictionary.Add(ep.ElementId, ep);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string message = ex.Message;
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read issues from Google spreadsheet.\n"+ex.Message, "Read Issues", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return issueDictionary;
        }

        public static ColorSchemeInfo ReadColorSchemes(string fileId, bool isImported)
        {
            ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCredential();
                }
                if (null != sheetsService && !string.IsNullOrEmpty(fileId))
                {
                    if (!isImported) { colorSheetId = fileId; }
                   
                    WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                    WorksheetFeed wsFeed = sheetsService.Query(worksheetquery);
                    if (wsFeed.Entries.Count > 0)
                    {
                        List<string> categoryNames = new List<string>();
                        WorksheetEntry categorySheet = null;
                        AtomLink listFeedLink = null;
                        ListQuery listQuery = null;
                        ListFeed listFeed = null;
                        if (wsFeed.Entries.Count > 1)
                        {
                            categorySheet = (WorksheetEntry)wsFeed.Entries[1]; //ElementCategories sheet
                            listFeedLink = categorySheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                            listQuery = new ListQuery(listFeedLink.HRef.ToString());
                            listFeed = sheetsService.Query(listQuery);

                            foreach (ListEntry row in listFeed.Entries)
                            {
                                foreach (ListEntry.Custom element in row.Elements)
                                {
                                    if (element.LocalName == "categoryname")
                                    {
                                        string catName = element.Value;
                                        if (categoryNames.Contains(catName))
                                        {
                                            categoryNames.Add(catName);
                                        }
                                    }
                                }
                            }
                        }

                        WorksheetEntry workSheetEntry = (WorksheetEntry)wsFeed.Entries[0]; //ColorScheme sheet
                        if (!isImported) { colorSheet = workSheetEntry; }
                        listFeedLink = workSheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                        listQuery = new ListQuery(listFeedLink.HRef.ToString());
                        listFeed = sheetsService.Query(listQuery);
                        foreach (ListEntry row in listFeed.Entries)
                        {
                            string schemeId = "";
                            string schemeName = "";
                            string paramName = "";
                            string paramValue = "";
                            byte[] colorBytes = new byte[3];
                            //"ColorSchemeId", "SchemeName", "ParameterName", "ParameterValue", "ColorR", "ColorG", "ColorB"
                            foreach (ListEntry.Custom element in row.Elements)
                            {
                                switch (element.LocalName)
                                {
                                    case "colorschemeid":
                                        schemeId = element.Value;
                                        break;
                                    case "schemename":
                                        schemeName = element.Value;
                                        break;
                                    case "parametername":
                                        paramName = element.Value;
                                        break;
                                    case "parametervalue":
                                        paramValue = element.Value;
                                        break;
                                    case "colorr":
                                        colorBytes[0] = byte.Parse(element.Value);
                                        break;
                                    case "colorg":
                                        colorBytes[1] = byte.Parse(element.Value);
                                        break;
                                    case "colorb":
                                        colorBytes[2] = byte.Parse(element.Value);
                                        break;
                                }
                            }

                            var schemes = from scheme in schemeInfo.ColorSchemes where scheme.SchemeId == schemeId select scheme;
                            if (schemes.Count() > 0)
                            {
                                for (int i = 0; i < schemeInfo.ColorSchemes.Count; i++)
                                {
                                    if (schemeInfo.ColorSchemes[i].SchemeId == schemeId)
                                    {
                                        ColorDefinition cd = new ColorDefinition();
                                        cd.ParameterValue = paramValue;
                                        cd.Color = colorBytes;

                                        System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
                                        cd.BackgroundColor = new SolidColorBrush(windowColor);

                                        schemeInfo.ColorSchemes[i].ColorDefinitions.Add(cd);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                ColorScheme scheme = new ColorScheme();
                                scheme.SchemeId = schemeId;
                                scheme.SchemeName = schemeName;
                                scheme.Categories = categoryNames;
                                scheme.ParameterName = paramName;
                                scheme.DefinitionBy = DefinitionType.ByValue;

                                ColorDefinition cd = new ColorDefinition();
                                cd.ParameterValue = paramValue;
                                cd.Color = colorBytes;

                                System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
                                cd.BackgroundColor = new SolidColorBrush(windowColor);

                                scheme.ColorDefinitions.Add(cd);

                                schemeInfo.ColorSchemes.Add(scheme);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read color schemes.\n"+ex.Message, "Read Color Schemes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schemeInfo;
        }

        public static WorksheetEntry FindWorksheet(string fileId, int worksheetIndex)
        {
            WorksheetEntry worksheetEntry = null;
            try
            {
                WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                WorksheetFeed wsFeed = sheetsService.Query(worksheetquery);

                if (wsFeed.Entries.Count > 0)
                {
                    worksheetEntry = (WorksheetEntry)wsFeed.Entries[worksheetIndex];
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return worksheetEntry;
        }

        public static bool UpdateElementProperties(ElementProperties ep, BCFParameters bcfParam, string fileId)
        {
            bool updated = false;
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCredential();
                }

                if (null != sheetsService)
                {
                    if (fileId != currentSheetId)
                    {
                        currentSheet = FindWorksheet(fileId, 0);//viewpoint worksheet
                        currentSheetId = fileId;
                    }
                    if (null != currentSheet)
                    {
                        CellAddress cellAddress = null;
                        string cellValue = "";
                        if(bcfParam == BCFParameters.BCF_Action && ep.CellEntries.ContainsKey(viewpointCols[3]))
                        {
                            cellAddress = ep.CellEntries[viewpointCols[3]];
                            cellValue = ep.Action;
                        }
                        else if (bcfParam == BCFParameters.BCF_Responsibility && ep.CellEntries.ContainsKey(viewpointCols[4]))
                        {
                            cellAddress = ep.CellEntries[viewpointCols[4]];
                            cellValue = ep.ResponsibleParty;
                        }
                        if (null != cellAddress)
                        {
                            AtomLink cellFeedLink = currentSheet.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null);
                            CellQuery cellQuery = new CellQuery(cellFeedLink.HRef.ToString());
                            cellQuery.MinimumRow = cellQuery.MaximumRow = cellAddress.Row;
                            cellQuery.MinimumColumn = cellQuery.MaximumColumn = cellAddress.Col;
                            CellFeed cellFeed = sheetsService.Query(cellQuery);
                            if (cellFeed.Entries.Count > 0)
                            {
                                CellEntry cellEntry = cellFeed.Entries[0] as CellEntry;
                                cellEntry.Cell.InputValue = cellValue;
                                AtomEntry updatedCell = cellEntry.Update();

                            }

                            /*
                            CellFeed cellFeed = sheetsService.Query(cellQuery);

                            CellFeed batchRequest = new CellFeed(new Uri(cellFeed.Self), sheetsService);
                            CellEntry batchEntry = new CellEntry(cellAddress.Row, cellAddress.Col);
                            batchEntry.Id = new AtomId(string.Format("{0}/{1}", cellFeed.Self, cellAddress.IdString));
                            batchEntry.BatchData = new GDataBatchEntryData(cellAddress.IdString, GDataBatchOperationType.query);
                            batchRequest.Entries.Add(batchEntry);

                            CellFeed queryBatchResponse = (CellFeed)sheetsService.Batch(batchRequest, new Uri(cellFeed.Batch));
                            CellFeed batchUpdateRequest = new CellFeed(cellQuery.Uri, sheetsService);
                            foreach (CellEntry entry in queryBatchResponse.Entries)
                            {
                                CellEntry batchUpdateEntry = entry;
                                batchUpdateEntry.InputValue = cellValue;
                                batchUpdateEntry.BatchData = new GDataBatchEntryData(GDataBatchOperationType.update);
                                batchUpdateRequest.Entries.Add(batchUpdateEntry);
                                break;
                            }

                            CellFeed batchUpdateResponse = (CellFeed)sheetsService.Batch(batchUpdateRequest, new Uri(cellFeed.Batch));

                            updated = true;
                            foreach (CellEntry entry in batchUpdateResponse.Entries)
                            {
                                string batchId = entry.BatchData.Id;
                                if (entry.BatchData.Status.Code != 200)
                                {
                                    updated = false;
                                    string reason = entry.BatchData.Status.Reason;
                                }
                            }
                            */
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update issue entry.\n" + ex.Message, "Update Issue Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }
    }

    public class ElementProperties
    {
        private int elementId = -1;
        private Element elementObj = null;
        private ElementId elementIdObj = Autodesk.Revit.DB.ElementId.InvalidElementId;
        private string elementName = "";
        private string categoryName = "";
        private int categoryId = -1;
        private string issueId = "";
        private string action = "";
        private string responsibleParty = "";
        private Dictionary<string, CellAddress> cellEntries = new Dictionary<string, CellAddress>();

        public int ElementId { get { return elementId; } set { elementId = value; } }
        public Element ElementObj { get { return elementObj; } set { elementObj = value; } }
        public ElementId ElementIdObj { get { return elementIdObj; } set { elementIdObj = value; } }
        public string ElementName { get { return elementName; } set { elementName = value; } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; } }
        public int CategoryId { get { return categoryId; } set { categoryId = value; } }
        public string IssueId { get { return issueId; } set { issueId = value; } }
        public string Action { get { return action; } set { action = value; } }
        public string ResponsibleParty { get { return responsibleParty; } set { responsibleParty = value; } }
        public Dictionary<string, CellAddress> CellEntries { get { return cellEntries; } set { cellEntries = value; } }

        public ElementProperties(Element element)
        {
            elementId = element.Id.IntegerValue;
            elementObj = element;
            elementIdObj = element.Id;
            elementName = element.Name;
            if (null != element.Category)
            {
                categoryName = element.Category.Name;
                categoryId = element.Category.Id.IntegerValue;
            }
        }

        public ElementProperties(int id)
        {
            elementId = id;
        }
    }

    public class IssueEntry
    {
        private string bcfName = "";
        private string issueId = "";
        private string issueTopic = "";
        private BitmapImage snapshot = null;
        private int numElements = 0;
        private bool isSelected = false;
        private Dictionary<int, ElementProperties> elementDictionary = new Dictionary<int, ElementProperties>();
        private Dictionary<string, Comment> commentDictionary = new Dictionary<string, Comment>();

        public string BCFName { get { return bcfName; } set { bcfName = value; } }
        public string IssueId { get { return issueId; } set { issueId = value; } }
        public string IssueTopic { get { return issueTopic; } set { issueTopic = value; } }
        public BitmapImage Snapshot { get { return snapshot; } set { snapshot = value; } }
        public int NumElements { get { return numElements; } set { numElements = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
        public Dictionary<int, ElementProperties> ElementDictionary { get { return elementDictionary; } set { elementDictionary = value; } }
        public Dictionary<string, Comment> CommentDictionary { get { return commentDictionary; } set { commentDictionary = value; } }

        public IssueEntry()
        {
           
        }
    }

    public class CellAddress
    {
        private uint row = 0;
        private uint col = 0;
        private string idString = "";

        public uint Row { get { return row; } set { row = value; } }
        public uint Col { get { return col; } set { col = value; } }
        public string IdString { get { return idString; } set { idString = value; } }

        public CellAddress(uint rowNum, uint colNum)
        {
            row = rowNum;
            col = colNum;
            idString = string.Format("R{0}C{1}", row, col);
        }
    }

}
