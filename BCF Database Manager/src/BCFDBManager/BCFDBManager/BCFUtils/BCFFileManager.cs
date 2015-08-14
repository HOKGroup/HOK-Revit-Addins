using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;

namespace BCFDBManager.BCFUtils
{

    public static class BCFFileManager
    {
        
        public static BCFZIP ReadBCF(string bcfPath)
        {
            BCFZIP bcfZip = new BCFZIP(bcfPath);
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(bcfPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryFullName = entry.FullName;
                        string guid = "";
                        string fileName = "";

                        byte[] buffer = null;
                        Stream entryStream = entry.Open();
                        using (MemoryStream ms = new MemoryStream())
                        {
                            entryStream.CopyTo(ms);
                            buffer = ms.ToArray();
                        }

                        if (entryFullName.Contains("/"))
                        {
                            string[] names = entryFullName.Split('/');
                            if (names.Length == 2)
                            {
                                guid = names[0];
                                fileName = names[1];
                            }
                        }
                        else if (entryFullName.Contains(".version"))
                        {
                            fileName = entryFullName;
                            XmlSerializer serializer = new XmlSerializer(typeof(Version));
                            Stream stream = new MemoryStream(buffer);
                            XmlReader reader = XmlReader.Create(stream);
                            if (serializer.CanDeserialize(reader))
                            {
                                bcfZip.VersionFile = (Version)serializer.Deserialize(reader);
                                continue;
                            }
                        }
                        else if (entryFullName.Contains(".bcfp"))
                        {
                            fileName = entryFullName;
                            XmlSerializer serializer = new XmlSerializer(typeof(ProjectExtension));
                            Stream stream = new MemoryStream(buffer);
                            XmlReader reader = XmlReader.Create(stream);
                            if (serializer.CanDeserialize(reader))
                            {
                                bcfZip.ProjectFile = (ProjectExtension)serializer.Deserialize(reader);
                                continue;
                            }
                        }

                        if (null != buffer)
                        {
                            BCFComponent bcfComponent = new BCFComponent();

                            if (bcfZip.BCFComponents.ContainsKey(guid))
                            {
                                bcfComponent = bcfZip.BCFComponents[guid];
                            }

                            if (entry.Name.EndsWith(".bcf"))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(Markup));
                                Stream stream = new MemoryStream(buffer);
                                XmlReader reader = XmlReader.Create(stream);
                                if (serializer.CanDeserialize(reader))
                                {
                                    bcfComponent.MarkupInfo = (Markup)serializer.Deserialize(reader);
                                }
                            }
                            else if (entry.Name.EndsWith(".bcfv"))
                            {
                                XmlSerializer serializer2 = new XmlSerializer(typeof(VisualizationInfo));
                                Stream stream = new MemoryStream(buffer);
                                XmlReader reader2 = XmlReader.Create(stream);
                                if (serializer2.CanDeserialize(reader2))
                                {
                                    if (!bcfComponent.Viewpoints.ContainsKey(entry.Name))
                                    {
                                        VisualizationInfo visInfo = (VisualizationInfo)serializer2.Deserialize(reader2);
                                        bcfComponent.Viewpoints.Add(entry.Name, visInfo);
                                    }
                                }
                            }
                            else if (!bcfComponent.BCFFiles.ContainsKey(entry.Name))
                            {
                                BCFFileItem fileItem = new BCFFileItem(entry.Name, buffer);
                                bcfComponent.BCFFiles.Add(fileItem.FileName, fileItem);
                            }

                            if (bcfZip.BCFComponents.ContainsKey(guid))
                            {
                                bcfZip.BCFComponents.Remove(guid);
                            }

                            bcfZip.BCFComponents.Add(guid, bcfComponent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read BCF.\n" + ex.Message, "Read BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfZip;
        }

        public static BCFZIP MapBinaryData(BCFZIP zipFile)
        {
            BCFZIP bcfzip = zipFile;
            try
            {
                List<string> guids = bcfzip.BCFComponents.Keys.ToList();
                foreach (string guid in guids)
                {
                    BCFComponent component = bcfzip.BCFComponents[guid];
                    Dictionary<string, BCFFileItem> fileItems = component.BCFFiles;

                    Markup markup = component.MarkupInfo;
                    Topic topic = markup.Topic;
                    
                    //BimSnippet
                    BimSnippet bimSnippet = topic.BimSnippet;
                    if (!string.IsNullOrEmpty(bimSnippet.Reference))
                    {
                        if (fileItems.ContainsKey(bimSnippet.Reference))
                        {
                            BCFFileItem fileItem = fileItems[bimSnippet.Reference];
                            bimSnippet.FileContent = fileItem.FileContents;
                            fileItems.Remove(fileItem.FileName);
                        }
                    }
                    topic.BimSnippet = bimSnippet;

                    //DocumentReferences
                    List<TopicDocumentReferences> docReferences = topic.DocumentReferences;
                    for (int i = docReferences.Count-1; i > -1; i--)
                    {
                        TopicDocumentReferences docRef = docReferences[i];
                        if (!string.IsNullOrEmpty(docRef.ReferencedDocument))
                        {
                            if (fileItems.ContainsKey(docRef.ReferencedDocument))
                            {
                                BCFFileItem fileItem = fileItems[docRef.ReferencedDocument];
                                docRef.FileContent = fileItem.FileContents;
                                fileItems.Remove(fileItem.FileName);
                                docReferences.RemoveAt(i);
                                docReferences.Insert(i, docRef);
                            }
                        }
                    }
                    topic.DocumentReferences = docReferences;
                    markup.Topic = topic;

                    //Viewpoints
                    List<ViewPoint> viewPoints = markup.Viewpoints;
                    for (int i = viewPoints.Count - 1; i > -1; i--)
                    {
                        ViewPoint vp = viewPoints[i];
                        if (!string.IsNullOrEmpty(vp.Snapshot))
                        {
                            if (fileItems.ContainsKey(vp.Snapshot))
                            {
                                BCFFileItem fileItem = fileItems[vp.Snapshot];
                                vp.SnapshotImage = fileItem.FileContents;
                                fileItems.Remove(fileItem.FileName);
                                viewPoints.RemoveAt(i);
                                viewPoints.Insert(i, vp);
                            }
                        }
                    }

                    markup.Viewpoints = viewPoints;
                    component.MarkupInfo = markup;

                    List<string> visGuids = component.Viewpoints.Keys.ToList();
                    foreach (string visGuid in visGuids)
                    {
                        VisualizationInfo visInfo = component.Viewpoints[visGuid];
                        //bitmaps
                        List<VisualizationInfoBitmaps> bitmaps = visInfo.Bitmaps;
                        for (int i = bitmaps.Count - 1; i > -1; i--)
                        {
                            VisualizationInfoBitmaps bitmap = bitmaps[i];
                            if (!string.IsNullOrEmpty(bitmap.Reference))
                            {
                                string bitmapName = Path.GetFileName(bitmap.Reference);
                                if (fileItems.ContainsKey(bitmapName))
                                {
                                    BCFFileItem fileItem = fileItems[bitmapName];
                                    bitmap.BitmapImage = fileItem.FileContents;
                                    fileItems.Remove(fileItem.FileName);
                                    bitmaps.RemoveAt(i);
                                    bitmaps.Insert(i, bitmap);
                                }
                            }
                        }

                        visInfo.Bitmaps = bitmaps;

                        component.Viewpoints.Remove(visGuid);
                        component.Viewpoints.Add(visGuid, visInfo);
                    }

                    bcfzip.BCFComponents.Remove(guid);
                    bcfzip.BCFComponents.Add(guid, component);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create mappings for bcf binary data.\n"+ex.Message, "Map Binary Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfzip;
        }

        private static DataSet ReadXMLSchemas()
        {
            DataSet schemaSet = new DataSet();
            try
            {
                FileType[] schemaTypes = new FileType[] { FileType.Project, FileType.Version, FileType.Markup, FileType.ViewPoint };
                foreach (FileType fType in schemaTypes)
                {
                    Stream schemaStream = GetSchemaStream(fType);
                    if (null != schemaStream)
                    {
                        DataSet dataset = new DataSet();
                        dataset.ReadXmlSchema(schemaStream);
                        
                        if (dataset.Tables.Count > 0)
                        {
                            foreach (DataTable dt in dataset.Tables)
                            {
                                schemaSet.Merge(dt);
                            }
                        }
                    }
                }

                //add primary keys


            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return schemaSet;
        }

        public static Stream GetSchemaStream(FileType filetype)
        {
            Stream schemaStream = null;
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string schemaFileName = "";
                switch (filetype)
                {
                    case FileType.Project:
                        schemaFileName = "project.xsd";
                        break;
                    case FileType.Version:
                        schemaFileName = "version.xsd";
                        break;
                    case FileType.Markup:
                        schemaFileName = "markup.xsd";
                        break;
                    case FileType.ViewPoint:
                        schemaFileName = "visinfo.xsd";
                        break;
                }
                if (!string.IsNullOrEmpty(schemaFileName))
                {
                    schemaStream = assembly.GetManifestResourceStream("BCFDBManager.Schemas." + schemaFileName);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return schemaStream;
        }
    }

    public class BCFZIP
    {
        private string fileId = "";
        private string zipFileName = "";
        private string zipFilePath = "";
        private string uploadedBy = "";
        private DateTime uploadedDate = DateTime.Now;
        private DateTime creationDate = DateTime.Now;
        private Dictionary<string/*guid*/, BCFComponent> bcfComponents = new Dictionary<string, BCFComponent>();
        private ProjectExtension projectFile = null;
        private Version versionFile = null;

        public string FileId { get { return fileId; } set { fileId = value; } }
        public string ZipFileName { get { return zipFileName; } set { zipFileName = value; } }
        public string ZipFilePath { get { return zipFilePath; } set { zipFilePath = value; } }
        public string UploadedBy { get { return uploadedBy; } set { uploadedBy = value; } }
        public DateTime UploadedDate { get { return uploadedDate; } set { uploadedDate = value; } }
        public DateTime CreationDate { get { return creationDate; } set { creationDate = value; } }
        public Dictionary<string, BCFComponent> BCFComponents { get { return bcfComponents; } set { bcfComponents = value; } }
        public ProjectExtension ProjectFile { get { return projectFile; } set { projectFile = value; } }
        public Version VersionFile { get { return versionFile; } set { versionFile = value; } }

        public BCFZIP()
        {
        }

        public BCFZIP(string filePath)
        {
            fileId = Guid.NewGuid().ToString();
            zipFilePath = filePath;
            zipFileName = Path.GetFileName(zipFilePath);
            uploadedBy = System.Environment.UserName;
            FileInfo fi = new FileInfo(zipFilePath);
            creationDate = fi.CreationTime;
        }
    }

    public class BCFComponent
    {
        private string guid = "";
        private Markup markupInfo = null;
        private Dictionary<string, VisualizationInfo> viewpoints = new Dictionary<string, VisualizationInfo>();
        private Dictionary<string/*fileName*/, BCFFileItem> bcfFiles = new Dictionary<string, BCFFileItem>();
        
        public string GUID { get { return guid; } set { guid = value; } }
        public Markup MarkupInfo { get { return markupInfo; } set { markupInfo = value; } }
        public Dictionary<string, VisualizationInfo> Viewpoints { get { return viewpoints; } set { viewpoints = value; } }
        public Dictionary<string, BCFFileItem> BCFFiles { get { return bcfFiles; } set { bcfFiles = value; } }
        
        public BCFComponent()
        {
        }
    }

    public class BCFFileItem
    {
        private string fileName = "";
        private byte[] fileContents;

        public string FileName { get { return fileName; } set { fileName = value; } }
        public byte[] FileContents { get { return fileContents; } set { fileContents = value; } }

        public BCFFileItem()
        {
        }

        public BCFFileItem(string name, byte[] contents)
        {
            fileName = name;
            fileContents = contents;
        }
    }

    public enum FileType
    {
        Markup/*markup.bcf*/, ViewPoint /*viewpoint.bcfv*/, SnapShot /*snapshot.png*/, Project/*project.bcfp*/, Version/*bcf.version*/, Bitmap/*.png or .jpg*/, None
    }

}
