using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        public static SpreadsheetsService sheetsService;
        public static string colorSheetId = "";
        public static WorksheetEntry colorSheet;
        public static string currentSheetId = ""; //fileId of Google Spreadsheet
        public static WorksheetEntry currentSheet; //worksheet of ViewPoint

        //private static string userName = "bsmart@hokbuildingsmart.com";
        //private static string passWord = "HOKb$mart";

        private static string keyFile = "HOK smartBCF.p12";
        private static string serviceAccountEmail = "756603983986-lrc8dm2b0nl381cepd60q2o7fo8df3bg@developer.gserviceaccount.com";

        private static string[] markupCols = new[] { "IssueGuid", "IssueTopic", "CommentGuid", "Comment", "Status", "VerbalStatus", "Author", "Date" };
        private static string[] viewpointCols = new[] { "IssueGuid", "ComponentIfcGuid", "AuthoringToolId", "Action", "Responsible" };
        private static string[] colorschemeCols = new[] { "ColorSchemeId", "SchemeName", "ParameterName", "ParameterValue", "ColorR", "ColorG", "ColorB"};
        private static string[] categoryCols = new[] { "CategoryName" };

        private static Random random = new Random();

        private static SpreadsheetsService GetUserCredential()
        {
            SpreadsheetsService service = null;
            try
            {
                var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var currentDirectory = System.IO.Path.GetDirectoryName(currentAssembly);
                var keyFilePath = System.IO.Path.Combine(currentDirectory, "Resources\\" + keyFile);

                var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);

                var credential = new ServiceAccountCredential(new
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
                var csvBuilder = new StringBuilder();
                //write columns
                for (var i = 0; i < colorschemeCols.Length; i++)
                {
                    if (i != 0)
                    {
                        csvBuilder.Append(',');
                    }

                    csvBuilder.Append(markupCols[i]);
                }
                csvBuilder.Append("\n");

                var specialChar = new[] { '"', ',' };
                foreach (var scheme in colorSchemeInfo.ColorSchemes)
                {
                    var schemeId = scheme.SchemeId;
                    var schemeName = scheme.SchemeName;
                    var paramName = scheme.ParameterName;

                    foreach (var definition in scheme.ColorDefinitions)
                    {
                        var strArray = new[] { schemeId, schemeName, paramName, definition.ParameterValue, 
                            definition.Color[0].ToString(), definition.Color[1].ToString(), definition.Color[2].ToString() };

                        var rowBuilder = new StringBuilder();
                        for (var i = 0; i < strArray.Length; i++)
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
                    var csvByteArray = Encoding.UTF8.GetBytes(csvBuilder.ToString());
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
                var csvBuilder = new StringBuilder();
                //write columns
                csvBuilder.AppendLine(categoryCols[0]);

                var specialChar = new[] { '"', ',' };
                foreach (var categoryName in categoryNames)
                {
                    csvBuilder.AppendLine(categoryName);
                }

                if (csvBuilder.ToString().Length > 0)
                {
                    var csvByteArray = Encoding.UTF8.GetBytes(csvBuilder.ToString());
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
                var csvBuilder = new StringBuilder();
                //write columns
                for (var i = 0; i < markupCols.Length; i++)
                {
                    if (i != 0)
                    {
                        csvBuilder.Append(',');
                    }

                    csvBuilder.Append(markupCols[i]);
                }
                csvBuilder.Append("\n");

                var specialChar = new[] { '"', ','};
                foreach (var bcf in bcfZip.BCFComponents)
                {
                    var bcfTopic = bcf.Markup.Topic;
                    if (null != bcf.Markup.Comment)
                    {
                        foreach (var comment in bcf.Markup.Comment)
                        {
                            var strArray = new[] { bcfTopic.Guid, bcfTopic.Title, 
                                comment.Guid, comment.Comment1, comment.Status, comment.VerbalStatus, comment.Author, comment.Date.ToString() };

                            var rowBuilder = new StringBuilder();

                            for (var i = 0; i < strArray.Length; i++)
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
                    var csvByteArray = Encoding.UTF8.GetBytes(csvBuilder.ToString());
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
                var csvBuilder = new StringBuilder();
                //write columns
                for (var i = 0; i < viewpointCols.Length; i++)
                {
                    if (i != 0)
                    {
                        csvBuilder.Append(',');
                    }
                    csvBuilder.Append(viewpointCols[i]);
                }
                csvBuilder.Append("\n");

                var specialChar = new[] { '"', ',' };
                foreach (var bcf in bcfZip.BCFComponents)
                {
                    foreach (var component in bcf.VisualizationInfo.Components)
                    {
                        var strArray = new[] { bcf.GUID, component.IfcGuid, component.AuthoringToolId, "MOVE", "ARCHITECTURE" };

                        var rowBuilder = new StringBuilder();
                        for (var i = 0; i < strArray.Length; i++)
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
                    var csvByteArray = Encoding.UTF8.GetBytes(csvBuilder.ToString());
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
            var created = false;
            wsFeed = null;
            try
            {
                var worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                wsFeed = sheetsService.Query(worksheetquery);
                if (wsFeed.Entries.Count > 0)
                {
                    var worksheet = (WorksheetEntry)wsFeed.Entries[0];
                    worksheet.Title.Text = "MarkUp";
                    worksheet.Update();

                    var cellQuery = new CellQuery(worksheet.CellFeedLink);
                    var cellFeed = sheetsService.Query(cellQuery);
                    
                    for (var i = 0; i < markupCols.Length; i++)
                    {
                        var colText = markupCols[i];
                        var cell = new CellEntry(1, Convert.ToUInt16(i+1), colText);
                        cellFeed.Insert(cell);
                    }

                    //write issue data
                    var listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                    var listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    var listFeed = sheetsService.Query(listQuery);

                    // "Issue Guid", "Issue Topic"
                    foreach (var bcf in bcfZip.BCFComponents)
                    {
                        var bcfTopic = bcf.Markup.Topic;
                        if (null != bcf.Markup.Comment)
                        {
                            foreach (var comment in bcf.Markup.Comment)
                            {
                                var row = new ListEntry();
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
                                    var insertedRow = sheetsService.Insert(listFeed, row);
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
            var created = false;
            try
            {
                var wsEntry = new WorksheetEntry(10000, 10, "ViewPoint");
                var worksheet = sheetsService.Insert(wsFeed, wsEntry);

                if (null != worksheet)
                {
                    var cellQuery = new CellQuery(worksheet.CellFeedLink);
                    var cellFeed = sheetsService.Query(cellQuery);

                    //write headers
                    for (var i = 0; i < viewpointCols.Length; i++)
                    {
                        var colText = viewpointCols[i];
                        var cell = new CellEntry(1, Convert.ToUInt16(i + 1), colText);
                        cellFeed.Insert(cell);
                    }

                    //write issue data
                    var listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                    var listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    var listFeed = sheetsService.Query(listQuery);

                    // "Issue Guid", "Component IfcGuid", "Authoring Tool Id"
                    foreach (var bcf in bcfZip.BCFComponents)
                    {
                        foreach (var component in bcf.VisualizationInfo.Components)
                        {
                            var row = new ListEntry();
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[0].ToLower(), Value = bcf.GUID });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[1].ToLower(), Value = component.IfcGuid });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[2].ToLower(), Value = component.AuthoringToolId });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[3].ToLower(), Value = "MOVE" });
                            row.Elements.Add(new ListEntry.Custom() { LocalName = viewpointCols[4].ToLower(), Value = "ARCHITECTURE" });

                            try
                            {
                                var insertedRow = sheetsService.Insert(listFeed, row);
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
                    var worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + colorSheetId + "/private/full");
                    var wsFeed = sheetsService.Query(worksheetquery);

                    if (wsFeed.Entries.Count > 0)
                    {
                        var worksheet = (WorksheetEntry)wsFeed.Entries[0];
                        worksheet.Title.Text = "ColorScheme";
                        wsEntry = (WorksheetEntry)worksheet.Update();
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }

                if (null != wsEntry)
                {
                    //create headers
                    var cellQuery = new CellQuery(wsEntry.CellFeedLink);
                    var cellFeed = sheetsService.Query(cellQuery);

                    for (var i = 0; i < colorschemeCols.Length; i++)
                    {
                        var colText = colorschemeCols[i];
                        var cell = new CellEntry(1, Convert.ToUInt16(i + 1), colText);
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
            var result = false;
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
                    var listFeedLink = colorSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                    var listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    var listFeed = sheetsService.Query(listQuery);

                    for (var i = listFeed.Entries.Count - 1; i > -1; i--)
                    {
                        var row = (ListEntry)listFeed.Entries[i];
                        row.Delete();
                    }

                    // "ColorSchemeId", "SchemeName", "ParameterName", "ParameterValue", "ColorValue"
                    foreach (var scheme in colorSchemeInfo.ColorSchemes)
                    {
                        var schemeId = scheme.SchemeId;
                        var schemeName = scheme.SchemeName;
                        var paramName = scheme.ParameterName;

                        foreach (var definition in scheme.ColorDefinitions)
                        {
                            var row = new ListEntry();
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
            var result = false;
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
                        var listFeedLink = colorSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                        var listQuery = new ListQuery(listFeedLink.HRef.ToString());
                        var listFeed = sheetsService.Query(listQuery);

                        if (modifyItem == ModifyItem.Add)
                        {
                            var row = new ListEntry();
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
                            for (var i = listFeed.Entries.Count - 1; i > -1; i--)
                            {
                                var row = (ListEntry)listFeed.Entries[i];
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
            var result = false;
            try
            {
                var sheetQuery = new SpreadsheetQuery();
                sheetQuery.Title = "Color Schemes";

                var feed = sheetsService.Query(sheetQuery);
                WorksheetEntry categorySheet = null;
                foreach (SpreadsheetEntry sheet in feed.Entries)
                {
                    if (sheet.Id.AbsoluteUri.Contains(colorSheetId))
                    {
                        var wsFeed = sheet.Worksheets;
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
                            var wsEntry = new WorksheetEntry(10000, 10, "ElementCategories");
                            categorySheet = sheetsService.Insert(wsFeed, wsEntry);
                            if (null != categorySheet)
                            {
                                var cellQuery = new CellQuery(categorySheet.CellFeedLink);
                                var cellFeed = sheetsService.Query(cellQuery);

                                //write headers
                                for (var i = 0; i < categoryCols.Length; i++)
                                {
                                    var colText = categoryCols[i];
                                    var cell = new CellEntry(1, Convert.ToUInt16(i + 1), colText);
                                    cellFeed.Insert(cell);
                                }
                            }
                        }

                        //Update color schemes
                        if (null != categorySheet)
                        {
                            var listFeedLink = categorySheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                            var listQuery = new ListQuery(listFeedLink.HRef.ToString());
                            var listFeed = sheetsService.Query(listQuery);

                            var existingCategories = new List<string>();
                            foreach (ListEntry row in listFeed.Entries)
                            {
                                var catName = row.Elements[0].Value;

                                if (!existingCategories.Contains(catName))
                                {
                                    existingCategories.Add(catName);
                                }
                            }

                            foreach (var categoryName in categoryNames)
                            {
                                if (existingCategories.Contains(categoryName)) { continue; }

                                var row = new ListEntry();
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
            var schemeInfo = new ColorSchemeInfo();
            try
            {
                var aScheme = new ColorScheme(); //Action
                aScheme.SchemeName = "BCF Action";
                aScheme.SchemeId = Guid.NewGuid().ToString();
                aScheme.ParameterName = "BCF_Action";
                aScheme.DefinitionBy = DefinitionType.ByValue;

                var cd = new ColorDefinition();
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

                var rScheme = new ColorScheme(); //Responsibility
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
            var colorBytes = new byte[3];
            colorBytes[0] = (byte)random.Next(256);
            colorBytes[1] = (byte)random.Next(256);
            colorBytes[2] = (byte)random.Next(256);
            return colorBytes;
        }

        public static Dictionary<string, IssueEntry> ReadIssues(string fileId, string fileName)
        {
            var issueDictionary = new Dictionary<string, IssueEntry>();
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCredential();
                }
                if (null != sheetsService)
                {
                    var worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                    var wsFeed = sheetsService.Query(worksheetquery);
                    if (wsFeed.Entries.Count > 1)
                    {
                        var worksheet = (WorksheetEntry)wsFeed.Entries[0]; //markup sheet
                        var listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                        var listQuery = new ListQuery(listFeedLink.HRef.ToString());
                        var listFeed = sheetsService.Query(listQuery);

                        foreach (ListEntry row in listFeed.Entries)
                        {
                            var issueEntry = new IssueEntry();
                            issueEntry.BCFName = fileName;
                            
                            try
                            {
                                var issueId = row.Elements[0].Value;
                                var issueTopic = row.Elements[1].Value;
                                var commentId = row.Elements[2].Value;
                                var commentStr = row.Elements[3].Value;
                                var status = row.Elements[4].Value;
                                var verbalStatus = row.Elements[5].Value;
                                var author = row.Elements[6].Value;
                                var date = row.Elements[7].Value;

                                issueEntry.IssueId = issueId;
                                issueEntry.IssueTopic = issueTopic;

                                var comment = new Comment();
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
                                var message = ex.Message;
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
                                    var issueId = row.Elements[0].Value;
                                    var elementId = int.Parse(row.Elements[2].Value);
                                    var action = row.Elements[3].Value;
                                    var responsibleParty = row.Elements[4].Value;

                                    var ep = new ElementProperties(elementId);
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
                                    var message = ex.Message;
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
            var schemeInfo = new ColorSchemeInfo();
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCredential();
                }
                if (null != sheetsService && !string.IsNullOrEmpty(fileId))
                {
                    if (!isImported) { colorSheetId = fileId; }
                   
                    var worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                    var wsFeed = sheetsService.Query(worksheetquery);
                    if (wsFeed.Entries.Count > 0)
                    {
                        var categoryNames = new List<string>();
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
                                        var catName = element.Value;
                                        if (categoryNames.Contains(catName))
                                        {
                                            categoryNames.Add(catName);
                                        }
                                    }
                                }
                            }
                        }

                        var workSheetEntry = (WorksheetEntry)wsFeed.Entries[0]; //ColorScheme sheet
                        if (!isImported) { colorSheet = workSheetEntry; }
                        listFeedLink = workSheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                        listQuery = new ListQuery(listFeedLink.HRef.ToString());
                        listFeed = sheetsService.Query(listQuery);
                        foreach (ListEntry row in listFeed.Entries)
                        {
                            var schemeId = "";
                            var schemeName = "";
                            var paramName = "";
                            var paramValue = "";
                            var colorBytes = new byte[3];
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
                                for (var i = 0; i < schemeInfo.ColorSchemes.Count; i++)
                                {
                                    if (schemeInfo.ColorSchemes[i].SchemeId == schemeId)
                                    {
                                        var cd = new ColorDefinition();
                                        cd.ParameterValue = paramValue;
                                        cd.Color = colorBytes;

                                        var windowColor = System.Windows.Media.Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
                                        cd.BackgroundColor = new SolidColorBrush(windowColor);

                                        schemeInfo.ColorSchemes[i].ColorDefinitions.Add(cd);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                var scheme = new ColorScheme
                                {
                                    SchemeId = schemeId,
                                    SchemeName = schemeName,
                                    Categories = categoryNames,
                                    ParameterName = paramName,
                                    DefinitionBy = DefinitionType.ByValue
                                };

                                var cd = new ColorDefinition();
                                cd.ParameterValue = paramValue;
                                cd.Color = colorBytes;

                                var windowColor = System.Windows.Media.Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
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
                var worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                var wsFeed = sheetsService.Query(worksheetquery);

                if (wsFeed.Entries.Count > 0)
                {
                    worksheetEntry = (WorksheetEntry)wsFeed.Entries[worksheetIndex];
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return worksheetEntry;
        }

        public static bool UpdateElementProperties(ElementProperties ep, BCFParameters bcfParam, string fileId)
        {
            var updated = false;
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
                        var cellValue = "";
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
                            var cellFeedLink = currentSheet.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null);
                            var cellQuery = new CellQuery(cellFeedLink.HRef.ToString());
                            cellQuery.MinimumRow = cellQuery.MaximumRow = cellAddress.Row;
                            cellQuery.MinimumColumn = cellQuery.MaximumColumn = cellAddress.Col;
                            var cellFeed = sheetsService.Query(cellQuery);
                            if (cellFeed.Entries.Count > 0)
                            {
                                var cellEntry = cellFeed.Entries[0] as CellEntry;
                                cellEntry.Cell.InputValue = cellValue;
                                var updatedCell = cellEntry.Update();

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
        public int ElementId { get; set; } = -1;
        public Element ElementObj { get; set; }
        public ElementId ElementIdObj { get; set; } = Autodesk.Revit.DB.ElementId.InvalidElementId;
        public string ElementName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int CategoryId { get; set; } = -1;
        public string IssueId { get; set; } = "";
        public string Action { get; set; } = "";
        public string ResponsibleParty { get; set; } = "";
        public Dictionary<string, CellAddress> CellEntries { get; set; } = new Dictionary<string, CellAddress>();

        public ElementProperties(Element element)
        {
            ElementId = element.Id.IntegerValue;
            ElementObj = element;
            ElementIdObj = element.Id;
            ElementName = element.Name;
            if (null != element.Category)
            {
                CategoryName = element.Category.Name;
                CategoryId = element.Category.Id.IntegerValue;
            }
        }

        public ElementProperties(int id)
        {
            ElementId = id;
        }
    }

    public class IssueEntry
    {
        public string BCFName { get; set; } = "";
        public string IssueId { get; set; } = "";
        public string IssueTopic { get; set; } = "";
        public BitmapImage Snapshot { get; set; } = null;
        public int NumElements { get; set; } = 0;
        public bool IsSelected { get; set; } = false;
        public Dictionary<int, ElementProperties> ElementDictionary { get; set; } = new Dictionary<int, ElementProperties>();
        public Dictionary<string, Comment> CommentDictionary { get; set; } = new Dictionary<string, Comment>();
    }

    public class CellAddress
    {
        public uint Row { get; set; }
        public uint Col { get; set; }
        public string IdString { get; set; }

        public CellAddress()
        {
        }

        public CellAddress(uint rowNum, uint colNum)
        {
            Row = rowNum;
            Col = colNum;
            IdString = $"R{Row}C{Col}";
        }
    }
}
