using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using System.Drawing;
using ICSharpCode.SharpZipLib.Checksums;
using System.Xml.Schema;
using System.Collections;

namespace HOK.BCFReader.GenericClasses
{
    public class BCFReaderClass
    {
        private Dictionary<string/*topicID*/, BCF> bcfFiles = new Dictionary<string,BCF>();
        private Dictionary<string, BCF> refBCF = new Dictionary<string, BCF>();
        private string bcfPath="";
        private string tempBcfPath="";
        private string markupPath="";
        private string imagePath="";
        private string viewPath="";
        private bool readSucceeded = true;
        private INIDataManager iniDataManager;
        private bool foundRef = false;

        public Dictionary<string,BCF> BcfFiles { get { return bcfFiles; } set { bcfFiles = value; } }

        public BCFReaderClass(string path, INIDataManager iniManager)
        {
            bcfPath = path;
            iniDataManager = iniManager;
            foundRef = StoreRefData();
            ChangeToZip();
        }

        private bool StoreRefData()
        {
            bool result = false;
            try
            {
                if (null != iniDataManager.RefDictionary)
                {
                    if (iniDataManager.RefDictionary.Count > 0 && iniDataManager.RefDictionary.ContainsKey(bcfPath) )
                    {
                        refBCF = iniDataManager.RefDictionary[bcfPath];
                        result = true;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Failed to store reference data.\n", "BCFReaderClass:StoreRefData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        //change the file extension from bcfzip to zip and read xml files
        public void ChangeToZip()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(bcfPath);
                fileInfo.MoveTo(Path.ChangeExtension(bcfPath, ".zip"));
                tempBcfPath = fileInfo.FullName;
                string baseFolder = tempBcfPath.Remove(tempBcfPath.Length - fileInfo.Extension.Length);
                DecompressFiles(tempBcfPath, baseFolder);

                string[] directories = Directory.GetDirectories(baseFolder);
                if (directories.Length > 0)
                {
                    for(int i=0; i<directories.Length;  i++)
                    {
                        string guidFolder = directories[i];
                        markupPath = Path.Combine(guidFolder, "markup.bcf");
                        imagePath = Path.Combine(guidFolder, "snapshot.png");
                        viewPath = Path.Combine(guidFolder, "viewpoint.bcfv");
                        BCF bcf = new BCF();
                        bcf=ReadMarkUp(bcf);
                        bcf = GetSanpShot(bcf);
                        bcf = ReadViewPoint(bcf);
                        if (readSucceeded)
                        {
                            bcf.IssueNumber = (i + 1).ToString();
                            if (foundRef)
                            {
                                bcf = CombineReferenceData(bcf);
                            }
                            bcfFiles.Add(bcf.MarkUp.MarkUpTopic.TopicGuid,bcf);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the file extension as zip.\n" + ex.Message, "BCFReader:ChangeToZip", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void DecompressFiles(String ZipPath, String Destination)
        {
            try
            {
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(ZipPath)))
                {
                    ZipEntry entry;
                    String tmpEntry = String.Empty;

                    while ((entry = s.GetNextEntry()) != null)
                    {
                        string dirName = Destination;
                        string fileName = Path.GetFileName(entry.Name);

                        if (dirName != "")
                        {
                            Directory.CreateDirectory(dirName);
                        }

                        if (fileName != string.Empty)
                        {
                            if (entry.Name.IndexOf(".ini") < 0)
                            {
                                String FileName = dirName + "\\" + entry.Name;
                                FileName = FileName.Replace("\\ ", "\\");
                                String FolderName = Path.GetDirectoryName(FileName);
                                if (Directory.Exists(FolderName) == false)
                                {
                                    Directory.CreateDirectory(FolderName);
                                }
                                FileStream fStream = File.Create(FileName);
                                int StreamSize = 2048;
                                byte[] buffer = new byte[2048];

                                while (true)
                                {
                                    StreamSize = s.Read(buffer, 0, buffer.Length);
                                    if (StreamSize > 0)
                                    {
                                        fStream.Write(buffer, 0, StreamSize);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                fStream.Close();
                            }
                        }
                    }
                    s.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unzip the file.\n" + ex.Message, "BCFReader:DecompressFiles", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public BCF ReadMarkUp(BCF bcfMarkup)
        {
            try
            {
                if (File.Exists(markupPath))
                {
                    BCFMarkUp markup = new BCFMarkUp();
                    List<IfcFile> ifcFiles = new List<IfcFile>();
                    Topic topic = new Topic();
                    List<Comment> comments = new List<Comment>();

                    using (FileStream filestream = new FileStream(markupPath,FileMode.Open))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(filestream);

                        XmlNode headerNode = xmlDoc.SelectSingleNode("Markup/Header");
                        foreach (XmlNode fileNode in headerNode.ChildNodes)
                        {
                            IfcFile ifcFile = new IfcFile();
                            ifcFile.IfcGuid = fileNode.Attributes[0].Value;

                            foreach (XmlNode childnode in fileNode.ChildNodes)
                            {
                                if (childnode.Name == "Filename") { ifcFile.FileName = childnode.InnerText; }
                                if (childnode.Name == "Date") { ifcFile.Date = DateTime.Parse(childnode.InnerText); }
                            }
                            ifcFiles.Add(ifcFile);
                        }

                        XmlNode TopicNode = xmlDoc.SelectSingleNode("Markup/Topic");
                        topic.TopicGuid = TopicNode.Attributes[0].Value;
                        foreach (XmlNode node in TopicNode.ChildNodes)
                        {
                            if (node.Name == "ReferenceLink") { topic.ReferenceLink = node.InnerText; }
                            if (node.Name == "Title") { topic.Title = node.InnerText; }
                        }
                        
                        XmlNodeList commentNodes = xmlDoc.SelectNodes("Markup/Comment");
                        foreach (XmlNode commentNode in commentNodes)
                        {
                            Comment comment = new Comment();
                            comment.CommentGuid = commentNode.Attributes[0].Value;

                            foreach (XmlNode node in commentNode.ChildNodes)
                            {
                                if (node.Name == "VerbalStatus") { comment.VerbalStatus = node.InnerText; }
                                if (node.Name == "Status") 
                                {
                                    switch (node.InnerText)
                                    {
                                        case "Error":
                                            comment.Status = CommentStatus.Error;
                                            break;
                                        case "Info":
                                            comment.Status = CommentStatus.Info;
                                            break;
                                        case "Unknown":
                                            comment.Status = CommentStatus.Unknown;
                                            break;
                                        case "Warning":
                                            comment.Status = CommentStatus.Warning;
                                            break;
                                    }
                                }
                                if (node.Name == "Date") { comment.Date = DateTime.Parse(node.InnerText); }
                                if (node.Name == "Author") { comment.Author = node.InnerText; }
                                if (node.Name == "Comment") { comment.CommentString = node.InnerText; }
                                if (node.Name == "Topic") 
                                {
                                    Topic linkedtopic = new Topic();
                                    linkedtopic.TopicGuid = node.Attributes[0].Value;
                                    comment.Topic = linkedtopic;
                                }
                            }
                            comments.Add(comment);
                        }
                        filestream.Close();
                    }
                    markup.Header = ifcFiles;
                    markup.MarkUpTopic = topic;
                    markup.Comments = comments;
                    bcfMarkup.MarkUp = markup;
                }
                return bcfMarkup;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read markup.bcf \n" + ex.Message, "BCFReaderClass:ReadMarkUp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                readSucceeded = false;
                return bcfMarkup;
            }
        }

        public BCF ReadViewPoint(BCF bcfViewPoint)
        {
            try
            {
                if (File.Exists(viewPath))
                {
                    XmlTextReader reader = new XmlTextReader(viewPath);
                    BCFViewPoint viewpoint = new BCFViewPoint();
                    List<Component> components = new List<Component>();
                    PerspectiveCameraProperties perspective = new PerspectiveCameraProperties();
                    Point point = new Point();

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Components")
                        {
                            Component component = new Component();
                            reader.Read();
                            while (true)
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "Component":
                                            if (null != component.IfcGuid) { components.Add(component); }
                                            component = new Component();
                                            component.IfcGuid = reader.GetAttribute("IfcGuid");
                                            break;
                                        case "OriginatingSystem":
                                            component.OriginatingSystem = reader.ReadElementContentAsString();
                                            break;
                                        case "AuthoringToolId":
                                            component.AuthoringToolId = reader.ReadElementContentAsString();
                                            break;
                                    }
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Components")
                                {
                                    if (null != component.IfcGuid)
                                    {
                                        components.Add(component);
                                    }
                                    break;
                                }
                                reader.Read();
                            }
                        }
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "PerspectiveCamera")
                        {
                            while (true)
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "CameraViewPoint":
                                            point = new Point();
                                            reader.ReadToFollowing("X");  point.X = reader.ReadElementContentAsDouble();
                                            reader.ReadToFollowing("Y");  point.Y= reader.ReadElementContentAsDouble();
                                            reader.ReadToFollowing("Z");  point.Z= reader.ReadElementContentAsDouble();
                                            perspective.CameraViewPoint = point;
                                            break;
                                        case "CameraDirection":
                                            point = new Point();
                                            reader.ReadToFollowing("X");  point.X = reader.ReadElementContentAsDouble();
                                            reader.ReadToFollowing("Y");  point.Y= reader.ReadElementContentAsDouble();
                                            reader.ReadToFollowing("Z");  point.Z= reader.ReadElementContentAsDouble();
                                            perspective.CameraDirection = point;
                                            break;
                                        case "CameraUpVector":
                                            point = new Point();
                                            reader.ReadToFollowing("X");  point.X = reader.ReadElementContentAsDouble();
                                            reader.ReadToFollowing("Y");  point.Y= reader.ReadElementContentAsDouble();
                                            reader.ReadToFollowing("Z");  point.Z= reader.ReadElementContentAsDouble();
                                            perspective.CameraUpVector = point;
                                            break;
                                        case "FieldOfView":
                                            perspective.FieldOfView = reader.ReadElementContentAsDouble();
                                            break;
                                    }
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "PerspectiveCamera")
                                {
                                    break;
                                }
                                reader.Read();
                            }
                        }
                    }
                    reader.Close();

                    viewpoint.Components = components;
                    viewpoint.PerspectiveCamera = perspective;
                    bcfViewPoint.ViewPoint = viewpoint;
                }
                return bcfViewPoint;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read viewpoint.bcfv \n"+ex.Message, "BCFReaderClass:ReadViewPoint", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                readSucceeded = false;
                return bcfViewPoint;
            }
        }

        public BCF GetSanpShot(BCF bcfImage)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                    bcfImage.SnapShot = Image.FromStream(fs);
                    fs.Close();
                }
                return bcfImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get snapshot.png \n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                readSucceeded = false;
                return bcfImage;
            }
        }

        private BCF CombineReferenceData(BCF rawBcf)
        {
            try
            {
                if (refBCF.ContainsKey(rawBcf.MarkUp.MarkUpTopic.TopicGuid))
                {
                    BCF bcfReference = refBCF[rawBcf.MarkUp.MarkUpTopic.TopicGuid];
                    rawBcf.IssueNumber = bcfReference.IssueNumber;

                    List<Comment> comments = rawBcf.MarkUp.Comments;
                    for (int i = comments.Count-1; i >-1; i--)
                    {
                        Comment comment = comments[i];
                        foreach (Comment refComment in bcfReference.MarkUp.Comments)
                        {
                            if (refComment.CommentGuid == comment.CommentGuid)
                            {
                                comment.Action = refComment.Action;
                                comments.RemoveAt(i);
                                comments.Add(comment);
                                break;
                            }
                        }
                    }
                    rawBcf.MarkUp.Comments = comments;
                }
                return rawBcf;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read markup.bcf \n" + ex.Message, "BCFReaderClass:CombineReferenceData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                readSucceeded = false;
                return rawBcf;
            }
        }

        public void WriteMarkUp()
        {
            try
            {
                BCF bcfToWrite = new BCF();
                FileInfo fileInfo = new FileInfo(bcfPath);
                string baseFolder = bcfPath.Remove(bcfPath.Length - fileInfo.Extension.Length);

                string[] directories = Directory.GetDirectories(baseFolder);
                if (directories.Length > 0)
                {
                    for (int i = 0; i < directories.Length; i++)
                    {
                        string guidFolder = directories[i];
                        markupPath = Path.Combine(guidFolder, "markup.bcf");

                        using (FileStream fileStream = File.Open(markupPath, FileMode.OpenOrCreate))
                        {
                            XmlDocument xmlDoc = new XmlDocument();

                            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                            string schemaPath = Path.GetDirectoryName(currentAssembly) + "\\Resources\\markup.xsd";

                            XmlTextReader reader = new XmlTextReader(schemaPath);
                            XmlSchema schema = XmlSchema.Read(reader, null);

                            xmlDoc.Schemas.Add(schema);
                            xmlDoc.Load(fileStream);

                            XmlNode singlenode = xmlDoc.SelectSingleNode("Markup/Topic");
                            string guid = singlenode.Attributes[0].Value;
                            if (bcfFiles.ContainsKey(guid))
                            {
                                bcfToWrite = bcfFiles[guid];
                                DeleteCommentNodes(xmlDoc, bcfToWrite);
                                UpdateCommentNodes(xmlDoc, bcfToWrite);
                                AddCommentNodes(xmlDoc, bcfToWrite);
                                fileStream.Close();
                                xmlDoc.Schemas.Compile();
                                xmlDoc.Save(markupPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write markup.bcf \n" + ex.Message, "BCFReaderClass:WriteMarkUp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteCommentNodes(XmlDocument xmlDoc, BCF bcf)
        {
            try
            {
                bool found = false;
                XmlNodeList nodes = xmlDoc.SelectNodes("Markup/Comment");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    found = false;
                    foreach (Comment comment in bcf.MarkUp.Comments)
                    {
                        if (node.Attributes[0].Value == comment.CommentGuid) //update existing Comment
                        {
                            found = true;
                        }
                    }
                    if (found) { continue; }
                    else { xmlDoc.DocumentElement.RemoveChild(node); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete comment nodes. \n" + ex.Message, "BCFReaderClass:DeleteCommentNodes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateCommentNodes(XmlDocument xmlDoc, BCF bcf)
        {
            try
            {
                XmlNodeList nodes = xmlDoc.SelectNodes("Markup/Comment");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    foreach (Comment comment in bcf.MarkUp.Comments)
                    {
                        if (node.Attributes[0].Value == comment.CommentGuid) //update existing Comment
                        {
                            foreach (XmlNode childNode in node.ChildNodes)
                            {
                                switch (childNode.Name)
                                {
                                    case "VerbalStatus":
                                        childNode.InnerText= comment.VerbalStatus;
                                        break;
                                    case "Status":
                                        childNode.InnerText= comment.Status.ToString();
                                        break;
                                    case "Date":
                                        DateTimeOffset offset = new DateTimeOffset(comment.Date);
                                        childNode.InnerText = offset.ToString("yyyy-MM-dd'T'HH:mm:sszzz");
                                        break;
                                    case "Author":
                                        childNode.InnerText= comment.Author;
                                        break;
                                    case "Comment":
                                        childNode.InnerText= comment.CommentString;
                                        break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update comment nodes.\n" + ex.Message, "BCFReaderClass:UpdateCommentNodes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddCommentNodes(XmlDocument xmlDoc, BCF bcf)
        {
            try
            {
                bool found = false;
                XmlNodeList nodes = xmlDoc.SelectNodes("Markup/Comment");
                foreach (Comment comment in bcf.MarkUp.Comments)
                {
                    found = false;
                    foreach (XmlNode node in nodes)
                    {
                        if (node.Attributes[0].Value == comment.CommentGuid) //update existing Comment
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        XmlElement root = xmlDoc.DocumentElement;
                        XmlElement commentElement = xmlDoc.CreateElement("Comment");
                        XmlAttribute attribute = xmlDoc.CreateAttribute("Guid");
                        attribute.Value = comment.CommentGuid;
                        commentElement.SetAttributeNode(attribute);
                        root.AppendChild(commentElement);

                        XmlNode newNode = xmlDoc.CreateNode("element", "VerbalStatus", "");
                        newNode.InnerText = comment.VerbalStatus;
                        commentElement.AppendChild(newNode);

                        newNode = xmlDoc.CreateNode("element", "Status", "");
                        newNode.InnerText = comment.Status.ToString();
                        commentElement.AppendChild(newNode);

                        newNode = xmlDoc.CreateNode("element", "Date", "");
                        DateTimeOffset offset = new DateTimeOffset(comment.Date);
                        newNode.InnerText = offset.ToString("yyyy-MM-dd'T'HH:mm:sszzz");
                        commentElement.AppendChild(newNode);

                        newNode = xmlDoc.CreateNode("element", "Author", "");
                        newNode.InnerText = comment.Author;
                        commentElement.AppendChild(newNode);

                        newNode = xmlDoc.CreateNode("element", "Comment", "");
                        newNode.InnerText = comment.CommentString;
                        commentElement.AppendChild(newNode);

                        XmlElement newElement = xmlDoc.CreateElement("Topic");
                        XmlAttribute newAttribute = xmlDoc.CreateAttribute("Guid");
                        newAttribute.Value = bcf.MarkUp.MarkUpTopic.TopicGuid;
                        newElement.SetAttributeNode(newAttribute);
                        commentElement.AppendChild(newElement);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add comment nodes.\n" + ex.Message, "BCFReaderClass:AddCommentNodes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ChangeToBcfzip()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(tempBcfPath);
                string zipPath = fileInfo.FullName;
                string baseFolder = zipPath.Remove(zipPath.Length - fileInfo.Extension.Length);

                ZipFile zipFile = new ZipFile(zipPath);
                zipFile.BeginUpdate();

                foreach (string directory in Directory.GetDirectories(baseFolder))
                {
                    DirectoryInfo dinfo = new DirectoryInfo(directory);
                    foreach (FileInfo finfo in dinfo.GetFiles())
                    {
                        string fileName = dinfo.Name + "/" + finfo.Name;
                        ZipEntry entry = zipFile.GetEntry(fileName);
                        if (null != entry)
                        {
                            zipFile.Delete(entry);
                        }
                        StaticDiskDataSource source = new StaticDiskDataSource(finfo.FullName);
                        zipFile.Add(source, fileName);
                    }
                }
                zipFile.CommitUpdate();
                zipFile.Close();

                if (File.Exists(zipPath))
                {
                    fileInfo = new FileInfo(zipPath);
                    fileInfo.MoveTo(Path.ChangeExtension(tempBcfPath, ".bcfzip"));
                    bcfPath = fileInfo.FullName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the file extension as bcfzip.\n" + ex.Message, "BCFReader:ChangeToBcfzip", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
