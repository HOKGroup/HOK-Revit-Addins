using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.GoogleUtils
{
    public static class BCFParser
    {
        public static SpreadsheetsService sheetsService = null;

        private static string userName = "bsmart@hokbuildingsmart.com";
        private static string passWord = "HOKb$mart";

        private static string[] markupCols = new string[] { "IssueGuid", "IssueTopic", "Action", "Responsible"};
        private static string[] viewpointCols = new string[] { "IssueGuid", "ComponentIfcGuid", "AuthoringToolId" };
        private static string[] colorschemeCols = new string[] { "ColorSchemeId", "SchemeName", "ParameterName", "ParameterValue", "ColorR", "ColorG", "ColorB"};
        private static string[] categoryCols = new string[] { "CategoryName" };

        private static Random random = new Random();

        public static bool ConverToGoogleDoc(BCFZIP bcfzip, string fileId)
        {
            bool result = false;
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCrendential();
                }
                if (null != sheetsService)
                {
                    bool createdMarkup = CreateMarkupSheet(bcfzip, fileId);
                    bool createdVisInfo = CreateViewSheet(bcfzip, fileId);
                    bool createdColorInfo = CreateColorSheet(bcfzip, fileId);
                    bool createdCategoryInfo = CreateCategorySheet(bcfzip, fileId);
                    result = (createdMarkup && createdVisInfo && createdColorInfo && createdCategoryInfo);
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to convert bcf data to google spread sheet.\n"+ex.Message, "Convert to Google Doc", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private static SpreadsheetsService GetUserCrendential()
        {
            SpreadsheetsService service = null;
            try
            {
                service = new SpreadsheetsService("HOK smartBCF");
                service.setUserCredentials(userName, passWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get user credential.\n"+ex.Message, "Get User Credential", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return service;
        }

        private static bool CreateMarkupSheet(BCFZIP bcfZip, string fileId)
        {
            bool created = false;
            try
            {
                WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                WorksheetFeed wsFeed = sheetsService.Query(worksheetquery);
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

                    // "Issue Guid", "Issue Topic", "Action", "Responsible"
                    foreach (BCFComponent bcf in bcfZip.BCFComponents)
                    {
                        Topic bcfTopic = bcf.Markup.Topic;
                        ListEntry row = new ListEntry();
                        row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[0].ToLower(), Value =bcfTopic.Guid});
                        row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[1].ToLower() , Value = bcfTopic.Title});
                        row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[2].ToLower() , Value = "Delete" });
                        row.Elements.Add(new ListEntry.Custom() { LocalName = markupCols[3].ToLower(), Value = "Architecture" });

                        sheetsService.Insert(listFeed, row);
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

        private static bool CreateViewSheet(BCFZIP bcfZip, string fileId)
        {
            bool created = false;
            try
            {
                string title = System.IO.Path.GetFileNameWithoutExtension(bcfZip.FileName);
                SpreadsheetQuery sheetQuery = new SpreadsheetQuery();
                sheetQuery.Title = title;

                SpreadsheetFeed feed = sheetsService.Query(sheetQuery);
                WorksheetEntry worksheet = null;
                foreach (SpreadsheetEntry sheet in feed.Entries)
                {
                    if (sheet.Id.AbsoluteUri.Contains(fileId))
                    {
                        WorksheetEntry wsEntry = new WorksheetEntry(10000, 10, "ViewPoint");
                        WorksheetFeed wsFeed = sheet.Worksheets;
                        worksheet = sheetsService.Insert(wsFeed, wsEntry);
                        break;
                    }
                }

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

                            sheetsService.Insert(listFeed, row);
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

        private static bool CreateColorSheet(BCFZIP bcfZip, string fileId)
        {
            bool created = false;
            try
            {
                string title = System.IO.Path.GetFileNameWithoutExtension(bcfZip.FileName);
                SpreadsheetQuery sheetQuery = new SpreadsheetQuery();
                sheetQuery.Title = title;

                SpreadsheetFeed feed = sheetsService.Query(sheetQuery);
                WorksheetEntry worksheet = null;
                foreach (SpreadsheetEntry sheet in feed.Entries)
                {
                    if (sheet.Id.AbsoluteUri.Contains(fileId))
                    {
                        WorksheetEntry wsEntry = new WorksheetEntry(10000, 10, "ColorScheme");
                        WorksheetFeed wsFeed = sheet.Worksheets;
                        worksheet = sheetsService.Insert(wsFeed, wsEntry);
                        break;
                    }
                }

                if (null != worksheet)
                {
                    CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
                    CellFeed cellFeed = sheetsService.Query(cellQuery);

                    //write headers
                    for (int i = 0; i < colorschemeCols.Length; i++)
                    {
                        string colText = colorschemeCols[i];
                        CellEntry cell = new CellEntry(1, Convert.ToUInt16(i + 1), colText);
                        cellFeed.Insert(cell);
                    }

                    ColorSchemeInfo schemeInfo = null;
                    foreach (BCFComponent bcf in bcfZip.BCFComponents)
                    {
                        if (bcf.ColorSchemeInfo.ColorSchemes.Count > 0)
                        {
                            schemeInfo = bcf.ColorSchemeInfo; break;
                        }
                    }

                    if (null == schemeInfo)
                    {
                        //create default scheme info
                        schemeInfo = CreateDefaultSchemeInfo();
                    }

                    if (null != schemeInfo)
                    {
                        //write color scheme data
                        AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                        ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                        ListFeed listFeed = sheetsService.Query(listQuery);

                        // "ColorSchemeId", "SchemeName", "ParameterName", "ParameterValue", "ColorValue"
                        foreach (ColorScheme scheme in schemeInfo.ColorSchemes)
                        {
                            string schemeId = scheme.SchemeId;
                            string schemeName = scheme.SchemeName;
                            string paramName=scheme.ParameterName;

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

                        created = true;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create viewpoint sheet.\n" + ex.Message, "Create Viewpoint Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        private static ColorSchemeInfo CreateDefaultSchemeInfo()
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

        private static bool CreateCategorySheet(BCFZIP bcfZip, string fileId)
        {
            bool created = false;
            try
            {
                string title = System.IO.Path.GetFileNameWithoutExtension(bcfZip.FileName);
                SpreadsheetQuery sheetQuery = new SpreadsheetQuery();
                sheetQuery.Title = title;

                SpreadsheetFeed feed = sheetsService.Query(sheetQuery);
                WorksheetEntry worksheet = null;
                foreach (SpreadsheetEntry sheet in feed.Entries)
                {
                    if (sheet.Id.AbsoluteUri.Contains(fileId))
                    {
                        WorksheetEntry wsEntry = new WorksheetEntry(10000, 10, "ElementCategories");
                        WorksheetFeed wsFeed = sheet.Worksheets;
                        worksheet = sheetsService.Insert(wsFeed, wsEntry);
                        break;
                    }
                }

                if (null != worksheet)
                {
                    CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
                    CellFeed cellFeed = sheetsService.Query(cellQuery);

                    //write headers
                    for (int i = 0; i < categoryCols.Length; i++)
                    {
                        string colText = categoryCols[i];
                        CellEntry cell = new CellEntry(1, Convert.ToUInt16(i + 1), colText);
                        cellFeed.Insert(cell);
                    }

                    created = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create viewpoint sheet.\n" + ex.Message, "Create Viewpoint Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        public static Dictionary<string, IssueEntry> ReadIssues(string fileId, string fileName)
        {
            Dictionary<string/*issueId*/, IssueEntry> issueDictionary = new Dictionary<string, IssueEntry>();
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCrendential();
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
                            IssueEntry ii = new IssueEntry();
                            ii.BCFName = fileName;
                            foreach (ListEntry.Custom element in row.Elements)
                            {
                                switch (element.LocalName)
                                {
                                    case "issueguid":
                                        ii.IssueId = element.Value;
                                        break;
                                    case "issuetopic":
                                        ii.IssueTopic = element.Value;
                                        break;
                                    case "action":
                                        ii.Action = element.Value;
                                        break;
                                    case "responsible":
                                        ii.Responsible = element.Value;
                                        break;
                                }
                            }
                            if (!issueDictionary.ContainsKey(ii.IssueId))
                            {
                                issueDictionary.Add(ii.IssueId, ii);
                            }
                        }

                        if (issueDictionary.Count > 0)
                        {
                            worksheet = (WorksheetEntry)wsFeed.Entries[1]; //viewInfo sheet
                            listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                            listQuery = new ListQuery(listFeedLink.HRef.ToString());
                            listFeed = sheetsService.Query(listQuery);

                            foreach (ListEntry row in listFeed.Entries)
                            {
                                string issueId = "";
                                int elementId = 0;
                                foreach (ListEntry.Custom element in row.Elements)
                                {
                                    if (element.LocalName == "issueguid")
                                    {
                                        issueId = element.Value;
                                    }
                                    if (element.LocalName == "authoringtoolid")
                                    {
                                        int.TryParse(element.Value, out elementId);
                                    }

                                    if (elementId != 0)
                                    {
                                        ElementProperties ep = new ElementProperties(elementId);
                                        if (issueDictionary.ContainsKey(issueId))
                                        {
                                            if (!issueDictionary[issueId].ElementDictionary.ContainsKey(elementId))
                                            {
                                                issueDictionary[issueId].ElementDictionary.Add(ep.ElementId, ep);
                                            }
                                        }
                                    }
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

        public static ColorSchemeInfo ReadColorSchemes(string fileId)
        {
            ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
            try
            {
                if (null == sheetsService)
                {
                    sheetsService = GetUserCrendential();
                }
                if (null != sheetsService)
                {
                    
                    WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                    WorksheetFeed wsFeed = sheetsService.Query(worksheetquery);
                    if (wsFeed.Entries.Count > 3)
                    {
                        WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[3]; //ElementCategories sheet
                        AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                        ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                        ListFeed listFeed = sheetsService.Query(listQuery);

                        List<string> categoryNames = new List<string>();
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

                        worksheet = (WorksheetEntry)wsFeed.Entries[2]; //ColorScheme sheet
                        listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
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

        public static bool UpdateCategories(string fileId, List<string> categoryNames)
        {
            bool result = false;
            try
            {
                WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + fileId + "/private/full");
                WorksheetFeed wsFeed = sheetsService.Query(worksheetquery);
                if (wsFeed.Entries.Count > 3)
                {
                    WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[3]; //ElementCategories sheet
                    AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                    ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                    ListFeed listFeed = sheetsService.Query(listQuery);

                    for (int i = listFeed.Entries.Count - 1; i > -1; i--)
                    {
                        ListEntry row = (ListEntry)listFeed.Entries[i];
                        sheetsService.Delete(row, true);
                    }
                    
                    foreach (string catName in categoryNames)
                    {
                        ListEntry row = new ListEntry();
                        row.Elements.Add(new ListEntry.Custom() { LocalName = categoryCols[0].ToLower(), Value = catName });
                        sheetsService.Insert(listFeed, row);
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update categories.\n"+ex.Message, "Update Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }


    }

    public class ElementProperties
    {
        private int elementId = -1;
        private Element elementObj = null;
        private string elementName = "";
        private string categoryName = "";

        public int ElementId { get { return elementId; } set { elementId = value; } }
        public Element ElementObj { get { return elementObj; } set { elementObj = value; } }
        public string ElementName { get { return elementName; } set { elementName = value; } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; } }

        public ElementProperties(Element element)
        {
            elementId = element.Id.IntegerValue;
            elementObj = element;
            elementName = element.Name;
            if (null != element.Category)
            {
                categoryName = element.Category.Name;
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
        private string action = "";
        private string responsible = "";
        private Dictionary<int, ElementProperties> elementDictionary = new Dictionary<int, ElementProperties>();

        public string BCFName { get { return bcfName; } set { bcfName = value; } }
        public string IssueId { get { return issueId; } set { issueId = value; } }
        public string IssueTopic { get { return issueTopic; } set { issueTopic = value; } }
        public string Action { get { return action; } set { action = value; } }
        public string Responsible { get { return responsible; } set { responsible = value; } }
        public Dictionary<int, ElementProperties> ElementDictionary { get { return elementDictionary; } set { elementDictionary = value; } }

        public IssueEntry()
        {
           
        }
    }
}
