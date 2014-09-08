using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace BCFTest
{
    class Program
    {
        static void Main(string[] args)
        {
           
            string path = @" https://drive.google.com/folderview?id=0B-522_RmbwJ4OFh4dWxvdDdRajg&usp=sharing";
            Regex regURL = new Regex("(?<=id=)\\w+(?=&usp)", RegexOptions.None);
            Match regMatch = regURL.Match(path);
            if (regMatch.Success)
            {
                string id = regMatch.Groups[0].Value;
            }

            string[] seperator = new string[] { "folderview?id=", "&usp=sharing" };
            string[] matchedStr = path.Split(seperator, StringSplitOptions.RemoveEmptyEntries);


            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "756603983986-ht1lgljr5m3tn8b429fen871lfutet7d.apps.googleusercontent.com",
                    ClientSecret = "TTtpuUjaJg7SQ6Wuew3G6YH7",
                },
                new[] { DriveService.Scope.Drive, DriveService.Scope.DriveAppdata },
               "user",
                CancellationToken.None).Result;

            // Create the service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Drive API Sample",
            });

            
          /*
            string bcfPath = @"B:\Revit Projects\BCF\New BCF report.bcfzip";
            string title = System.IO.Path.GetFileNameWithoutExtension(bcfPath);
            File body = new File();
            body.Title = title;
            body.Description = "parsed BCF";
            body.MimeType = "application/vnd.google-apps.spreadsheet";
            body.Parents = new List<ParentReference>() { new ParentReference() { Id = "0B-522_RmbwJ4OFh4dWxvdDdRajg" } };
            body.Shared = true;
            body.WritersCanShare = true;
            body.Editable = true;
            
            //string bcfPath = @"B:\Revit Projects\BCF\Space Requirement - Copy\26a18cdd-c419-44c0-9a48-76e420226b49\snapshot.png";
            byte[] byteArray = System.IO.File.ReadAllBytes(bcfPath);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

            FilesResource.InsertRequest request = service.Files.Insert(body);
            File file = request.Execute();

            Permission permission = new Permission();
            permission.Value = "";
            permission.Role = "writer";
            permission.Type = "anyone";
            service.Permissions.Insert(permission, file.Id).Execute();
            
            Console.WriteLine("File id: " + file.Id);
            Console.WriteLine("File ETag: " + file.ETag);
            Console.WriteLine("Press Enter to end this process.");

        
         
            OAuth2Parameters parameters = new OAuth2Parameters();
            parameters.ClientId = "756603983986-ht1lgljr5m3tn8b429fen871lfutet7d.apps.googleusercontent.com";
            parameters.ClientSecret = "TTtpuUjaJg7SQ6Wuew3G6YH7";
            parameters.RedirectUri = "urn:ietf:wg:oauth:2.0:oob";
            parameters.Scope = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds";

            string authorizationUrl = OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
            Console.WriteLine(authorizationUrl);
            Process.Start(authorizationUrl);
            Console.WriteLine("Please visit the URL above to authorize your OAuth "
             + "request token.  Once that is complete, type in your access code to "
             + "continue...");
            parameters.AccessCode = Console.ReadLine();
            OAuthUtil.GetAccessToken(parameters);
            string accessToken = parameters.AccessToken;
            Console.WriteLine("OAuth Access Token: " + accessToken);

            GOAuth2RequestFactory requestFactory =
                     new GOAuth2RequestFactory(null, "HOK smartBCF", parameters);
         



            SpreadsheetsService sheetservice = new SpreadsheetsService("HOK smartBCF");
            sheetservice.setUserCredentials("bsmart@hokbuildingsmart.com", "HOKb$mart");

            WorksheetQuery worksheetquery = new WorksheetQuery("https://spreadsheets.google.com/feeds/worksheets/" + file.Id + "/private/full");
            WorksheetFeed wsFeed = sheetservice.Query(worksheetquery);
            WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];
            worksheet.Title.Text = "BCF Test";
           

            CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
            CellFeed cellFeed = sheetservice.Query(cellQuery);

            CellEntry cell = new CellEntry(1, 1, "Issue Number");
            cellFeed.Insert(cell);
            cell = new CellEntry(1, 2, "Issue Topic");
            cellFeed.Insert(cell);
            */
         

            Console.ReadLine();
        }
    }
}
