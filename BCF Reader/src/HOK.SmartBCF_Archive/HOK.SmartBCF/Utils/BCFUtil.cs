using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using Autodesk.Revit.DB;
using ICSharpCode.SharpZipLib.Zip;

namespace HOK.SmartBCF.Utils
{
    public class BCFUtil
    {
        private string bcfFolder = "";

        public BCFUtil()
        {
            string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            bcfFolder = Path.Combine(localFolder, "smartBCF");

            if (!Directory.Exists(bcfFolder))
            {
                Directory.CreateDirectory(bcfFolder);
            }
            CleanLocalFolder();
        }

        public void CleanLocalFolder()
        {
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(bcfFolder);
                foreach (FileInfo fileInfo in dInfo.GetFiles())
                {
                    try
                    {
                        fileInfo.Delete();
                    }
                    catch { }
                }
                foreach (DirectoryInfo subDir in dInfo.GetDirectories())
                {
                    try
                    {
                        subDir.Delete(true);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to clean local directories.\n" + ex.Message, "BCFParser: Clean Local Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public BCFZIP ReadBCF(string bcfPath)
        {
            BCFZIP bcfZip = new BCFZIP();
            try
            {
                string tempBcfPath = "";
                string tempZipPath = "";
                string tempZipFolder = "";

                FindFilePaths(bcfPath, out tempBcfPath, out tempZipPath, out tempZipFolder);

                bcfZip.FileName = bcfPath;

                byte[] buf;
                using (FileStream fileStream = new FileStream(bcfPath, FileMode.Open))
                {
                    buf = new byte[fileStream.Length];
                    fileStream.Read(buf, 0, buf.Length);
                    fileStream.Close();
                }

                if (File.Exists(tempBcfPath)) { File.Delete(tempBcfPath); }
                if (null != buf)
                {
                    using (FileStream fileStream = new FileStream(tempBcfPath, FileMode.OpenOrCreate))
                    {
                        fileStream.Write(buf, 0, buf.Length);
                        fileStream.Close();
                    }
                }

                if (File.Exists(tempZipPath)) { File.Delete(tempZipPath); }
                File.Move(tempBcfPath, tempZipPath);

                if (Directory.Exists(tempZipFolder)) { Directory.Delete(tempZipFolder, true); }

                if (DecompressFiles(tempZipPath, tempZipFolder))
                {
                    string[] directories = Directory.GetDirectories(tempZipFolder);
                    if (directories.Length > 0)
                    {
                        BCFComponent[] bcfComponents = new BCFComponent[directories.Length];
                        for (int i = 0; i < directories.Length; i++)
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(directories[i]);
                            string guidFolder = directoryInfo.FullName;
                            string markupPath = Path.Combine(guidFolder, "markup.bcf");
                            string imagePath = Path.Combine(guidFolder, "snapshot.png");
                            string viewPath = Path.Combine(guidFolder, "viewpoint.bcfv");
                            string colorPath = Path.Combine(guidFolder, "colorscheme.xml");

                            BCFComponent bcfComponent = new BCFComponent();
                            bool isReadable = false;
                            bcfComponent.DirectoryPath = guidFolder;
                            bcfComponent.GUID = directoryInfo.Name;
                            bcfComponent.Markup = ReadMarkup(markupPath, out isReadable);
                            bcfComponent.Snapshot = ReadSnapshot(imagePath, out isReadable);
                            bcfComponent.VisualizationInfo = ReadVisualizationInfo(viewPath, out isReadable);
                            bcfComponent.ColorSchemeInfo = ReadColorSchemeInfo(colorPath, out isReadable);

                            bcfComponents[i] = bcfComponent;
                        }
                        bcfZip.BCFComponents = bcfComponents;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return bcfZip;
        }

        private void FindFilePaths(string bcfPath, out string tempBcfPath, out string tempZipPath, out string tempZipFolder)
        {
            tempBcfPath = "";
            tempZipPath = "";
            tempZipFolder = "";
            try
            {
                tempBcfPath = Path.Combine(bcfFolder, Path.GetFileName(bcfPath));
                tempZipPath = Path.ChangeExtension(tempBcfPath, ".zip");
                tempZipFolder = Path.Combine(bcfFolder, Path.GetFileNameWithoutExtension(bcfPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find file path.\n" + ex.Message, "BCFParser: Find File Paths", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool DecompressFiles(String zipPath, String destination)
        {
            bool result = false;
            try
            {
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipPath)))
                {
                    ZipEntry entry;
                    String tmpEntry = String.Empty;

                    while ((entry = s.GetNextEntry()) != null)
                    {
                        string dirName = destination;
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
                if (Directory.Exists(destination))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unzip the file.\n" + ex.Message, "DecompressFiles", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        private Markup ReadMarkup(string filePath, out bool isReadable)
        {
            Markup markup = new Markup();
            isReadable = false;
            try
            {
                if (File.Exists(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Markup));
                    FileStream fs = new FileStream(filePath, FileMode.Open);
                    XmlReader reader = XmlReader.Create(fs);
                    if (serializer.CanDeserialize(reader))
                    {
                        markup = (Markup)serializer.Deserialize(reader);
                        isReadable = true;
                    }
                    reader.Close();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read markup.\n" + ex.Message, "ReadMarkup", MessageBoxButton.OK, MessageBoxImage.Warning);
                isReadable = false;
                return null;
            }
            return markup;
        }

        private VisualizationInfo ReadVisualizationInfo(string filePath, out bool isReadable)
        {
            VisualizationInfo visInfo = new VisualizationInfo();
            isReadable = false;
            try
            {
                if (File.Exists(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VisualizationInfo));
                    FileStream fs = new FileStream(filePath, FileMode.Open);
                    XmlReader reader = XmlReader.Create(fs);
                    if (serializer.CanDeserialize(reader))
                    {
                        visInfo = (VisualizationInfo)serializer.Deserialize(reader);
                        isReadable = true;
                    }
                    reader.Close();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read markup.\n" + ex.Message, "ReadMarkup", MessageBoxButton.OK, MessageBoxImage.Warning);
                isReadable = false;
                return null;
            }
            return visInfo;
        }

        private ColorSchemeInfo ReadColorSchemeInfo(string filePath, out bool isReadable)
        {
            ColorSchemeInfo colorSchemeInfo = new ColorSchemeInfo();
            isReadable = false;
            try
            {
                if (File.Exists(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ColorSchemeInfo));
                    FileStream fs = new FileStream(filePath, FileMode.Open);
                    XmlReader reader = XmlReader.Create(fs);
                    if (serializer.CanDeserialize(reader))
                    {
                        colorSchemeInfo = (ColorSchemeInfo)serializer.Deserialize(reader);
                        isReadable = true;
                    }
                    reader.Close();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read markup.\n" + ex.Message, "ReadMarkup", MessageBoxButton.OK, MessageBoxImage.Warning);
                isReadable = false;
                return null;
            }
            return colorSchemeInfo;
        }

        private Snapshot ReadSnapshot(string filePath, out bool isReadable)
        {
            Snapshot snapshot = new Snapshot();
            snapshot.FilePath = filePath;
            isReadable = false;
            try
            {
                if (File.Exists(filePath))
                {
                    FileStream fs = File.OpenRead(filePath);
                    MemoryStream ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    fs.Close();


                    BitmapImage bitImage = new BitmapImage();
                    bitImage.BeginInit();
                    bitImage.StreamSource = ms;
                    //bitImage.UriSource = new Uri(filePath);
                    bitImage.EndInit();
                    snapshot.FileImage = bitImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read snapshot.\n" + ex.Message, "Read Snapshot", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return snapshot;
        }

        public bool WriteBCF(BCFComponent bcfComponent)
        {
            bool result = false;
            try
            {
                string guidFolder = bcfComponent.DirectoryPath;

                if (Directory.Exists(guidFolder))
                {
                    string markupPath = Path.Combine(guidFolder, "markup.bcf");
                    string imagePath = Path.Combine(guidFolder, "snapshot.png");
                    string viewPath = Path.Combine(guidFolder, "viewpoint.bcfv");

                    bool markupResult = WriteMarkup(markupPath, bcfComponent.Markup);
                    bool viewResult = WriteVisualizationInfo(viewPath, bcfComponent.VisualizationInfo);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write bcf.\n" + ex.Message, "Write BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        public bool WriteMarkup(string filePath, Markup markup)
        {
            bool result = false;
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(Markup));
                StreamWriter writer = new StreamWriter(filePath);
                serializer.Serialize(writer, markup);
                writer.Close();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(filePath + "\nFailed to write markup.\n" + ex.Message, "Write Markup", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        public bool WriteVisualizationInfo(string filePath, VisualizationInfo visInfo)
        {
            bool result = false;
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(VisualizationInfo));
                StreamWriter writer = new StreamWriter(filePath);
                serializer.Serialize(writer, visInfo);
                writer.Close();
                result = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(filePath + "\nFailed to write visualization info.\n" + ex.Message, "Write Visualization Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        public bool WriteColorSchemeInfo(string filePath, ColorSchemeInfo colorSchemeInfo)
        {
            bool result = false;
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(ColorSchemeInfo));
                StreamWriter writer = new StreamWriter(filePath);
                serializer.Serialize(writer, colorSchemeInfo);
                writer.Close();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(filePath + "\nFailed to write color scheme info.\n" + ex.Message, "Write Color Scheme Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        public bool ChangeToBcfzip(string bcfPath)
        {
            bool bcfCreated = false;
            string[] fileNames = new string[] { "markup.bcf", "snapshot.png", "viewpoint.bcfv" };
            try
            {
                string tempBcfPath = "";
                string tempZipPath = "";
                string tempZipFolder = "";
                FindFilePaths(bcfPath, out tempBcfPath, out tempZipPath, out tempZipFolder);

                if (Directory.Exists(tempZipFolder))
                {
                    ZipFile zipFile = null;
                    if (File.Exists(tempZipPath))
                    {
                        zipFile = new ZipFile(tempZipPath);
                    }
                    else
                    {
                        zipFile = ZipFile.Create(tempZipPath);
                    }

                    zipFile.BeginUpdate();

                    foreach (string directory in Directory.GetDirectories(tempZipFolder))
                    {
                        DirectoryInfo dinfo = new DirectoryInfo(directory);
                        foreach (FileInfo finfo in dinfo.GetFiles())
                        {
                            if (fileNames.Contains(finfo.Name))
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
                    }
                    zipFile.CommitUpdate();
                    zipFile.Close();

                    if (File.Exists(tempZipPath))
                    {
                        string tempbcf = Path.ChangeExtension(tempZipPath, ".bcfzip");
                        if (File.Exists(tempbcf)) { File.Delete(tempbcf); }

                        File.Move(tempZipPath, tempbcf);
                        File.Copy(tempbcf, bcfPath, true);
                        bcfCreated = true;
                        //Directory.Delete(tempZipFolder, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the file extension as bcfzip.\n" + ex.Message, "BCFReader:ChangeToBcfzip", MessageBoxButton.OK, MessageBoxImage.Warning);
                bcfCreated = false;
            }
            return bcfCreated;
        }
    }

    public class BCFZIP
    {
        public string FileName { get; set; }
        public BCFComponent[] BCFComponents { get; set; }
    }

    public class BCFComponent
    {
        public string DirectoryPath { get; set; }
        public string GUID { get; set; }
        public Markup Markup { get; set; }
        public VisualizationInfo VisualizationInfo { get; set; }
        public ColorSchemeInfo ColorSchemeInfo { get; set; }
        public Snapshot Snapshot { get; set; } //filePath
    }

    public class Snapshot
    {
        public string FilePath { get; set; }
        public BitmapImage FileImage { get; set; }
    }

    public class IssueInfo : INotifyPropertyChanged
    {
        private BCFComponent m_component = null;
        private string issueNumber = "";
        private string issueTitle = "";

        public BCFComponent Component { get { return m_component; } set { m_component = value; } }
        public string IssueNumber { get { return issueNumber; } set { issueNumber = value; } }
        public string IssueTitle { get { return issueTitle; } set { issueTitle = value; } }

        public IssueInfo(BCFComponent component)
        {
            m_component = component;
            issueTitle = m_component.Markup.Topic.Title;
            if (null != m_component.Markup.Topic.Index)
            {
                issueNumber = m_component.Markup.Topic.Index;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }

    public class CommentInfo : INotifyPropertyChanged
    {
        private Comment m_comment = null;
        private string date = "";
        private string status = "";
        private string author = "";
        private string commentStr = "";
        private string verbalStatus = "";

        public Comment CommentObj { get { return m_comment; } set { m_comment = value; } }
        public string Date { get { return date; } set { date = value; } }
        public string Status { get { return status; } set { status = value; } }
        public string Author { get { return author; } set { author = value; } }
        public string CommentStr { get { return commentStr; } set { commentStr = value; } }
        public string VerbalStatus { get { return verbalStatus; } set { verbalStatus = value; } }

        public CommentInfo(Comment comment)
        {
            m_comment = comment;
            date = comment.Date.ToString();
            status = comment.Status;
            author = comment.Author;
            commentStr = comment.Comment1;
            verbalStatus = comment.VerbalStatus;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }

    public class ComponentInfo : INotifyPropertyChanged
    {
        private Component m_component = null;
        private bool isSelected = false;
        private string ifcGuid = "";
        private string revitId = "";
        private string familyName = "";
        private string symbolName = "";
        private Element revitElement = null;

        public Component ComponentObj { get { return m_component; } set { m_component = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
        public string IfcGuid { get { return ifcGuid; } set { ifcGuid = value; } }
        public string RevitId { get { return revitId; } set { revitId = value; } }
        public string FamilyName { get { return familyName; } set { familyName = value; } }
        public string SymbolName { get { return symbolName; } set { symbolName = value; } }
        public Element RevitElement { get { return revitElement; } set { revitElement = value; } }

        public ComponentInfo(Component component)
        {
            m_component = component;
            ifcGuid = component.IfcGuid;
            revitId = component.AuthoringToolId;
        }

        public ComponentInfo(Component component, Document doc)
        {
            m_component = component;
            ifcGuid = component.IfcGuid;
            revitId = component.AuthoringToolId;
            ElementId eId = new ElementId(int.Parse(revitId));
            Element element = doc.GetElement(eId);
            if (null != element)
            {
                revitElement = element;
                Parameter param = element.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                if (null != param)
                {
                    familyName = param.AsString();
                }
                param = element.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME);
                if (null != param)
                {
                    symbolName = param.AsString();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
