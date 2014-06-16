using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Documents;
using Google.GData.Spreadsheets;

namespace HOK.ModelManager.GoogleDocs
{
    
    public class GoogleDocInfo
    {
        private string docId = "";
        private string docTitle = "";
        private Document docObj = null;
        private SpreadsheetEntry sheetEntry = null;
        private string folderId = "";
        private string folderName = "";
        private string fileLocation = "";
        private string projectNumeber = "";
        private string projectName = "";
        private string companyName = "";
        private string fileIdentifier = "";
        private ModelManagerMode mode=ModelManagerMode.ProjectReplication;
        private List<LinkInfo> linkInfoList = new List<LinkInfo>();

        public string DocId { get { return docId; } set { docId = value; } }
        public string DocTitle { get { return docTitle; } set { docTitle = value; } }
        public Document DocObj { get { return docObj; } set { docObj = value; } }
        public SpreadsheetEntry SheetEntry { get { return sheetEntry; } set { sheetEntry = value; } }
        public string FolderId { get { return folderId; } set { folderId = value; } }
        public string FolderName { get { return folderName; } set { folderName = value; } }
        public string FileLocation { get { return fileLocation; } set { fileLocation = value; } }
        public string ProjectNumber { get { return projectNumeber; } set { projectNumeber = value; } }
        public string ProjectName { get { return projectName; } set { projectName = value; } }
        public string CompanyName { get { return companyName; } set { companyName = value; } }
        public string FileIdentifier { get { return fileIdentifier; } set { fileIdentifier = value; } }
        public ModelManagerMode ManagerMode { get { return mode; } set { mode = value; } }
        public List<LinkInfo> LinkInfoList { get { return linkInfoList; } set { linkInfoList = value; } }

        public GoogleDocInfo(Document doc)
        {
            docId = doc.Id;
            docObj = doc;
            if (doc.ParentFolders.Count > 0)
            {
                folderId = doc.ParentFolders[0];
            }
        }

       

       
    }


}
