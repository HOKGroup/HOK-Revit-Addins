using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml.Serialization;
using System.Xml;

namespace HOK.ColorSchemeEditor.BCFUtils
{
    public class BCFUtil
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private BCFZIP bcfZip = new BCFZIP();
        private BCFComponent selectedBCF = null;
        private string colorFolder = "";
        //private string tempZipPath = "";
        //private string tempZipFolder = "";
        //private string bcfPath = "";
        private bool bcfExists = false;
        private bool bcfRead = false;

        //public string BCFPath { get { return bcfPath; } set { bcfPath = value; } }
        public BCFZIP BCFZip { get { return bcfZip; } set { bcfZip = value; } }
        public BCFComponent SelectedBCF { get { return selectedBCF; } set { selectedBCF = value; } }
        public bool BCFExists { get { return bcfExists; } set { bcfExists = value; } }
        public bool BCFRead { get { return bcfRead; } set { bcfRead = value; } }

        public BCFUtil(UIApplication application)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;

            string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string hokFolder = Path.Combine(localFolder, "HOK");
            colorFolder = Path.Combine(hokFolder, "Color Scheme Editor");

            if (!Directory.Exists(hokFolder))
            {
                Directory.CreateDirectory(hokFolder);
            }
            if (!Directory.Exists(colorFolder))
            {
                Directory.CreateDirectory(colorFolder);
            }
        }

        public void CleanLocalFolder()
        {
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(colorFolder);
                foreach (FileInfo fileInfo in dInfo.GetFiles())
                {
                    fileInfo.Delete();
                }
                foreach (DirectoryInfo subDir in dInfo.GetDirectories())
                {
                    subDir.Delete(true);
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void FindFilePaths(string bcfPath, out string tempBcfPath, out string tempZipPath, out string tempZipFolder)
        {
            tempBcfPath = "";
            tempZipPath = "";
            tempZipFolder = "";
            try
            {
                tempBcfPath = Path.Combine(colorFolder, Path.GetFileName(bcfPath));
                tempZipPath = Path.ChangeExtension(tempBcfPath, ".zip");
                tempZipFolder = Path.Combine(colorFolder, Path.GetFileNameWithoutExtension(bcfPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find file paths.\n" + ex.Message, "Find File Paths", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Failed to read BCF.\n" + ex.Message, "Read BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfZip;
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
                    /*
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    snapshot.FileImage = Image.FromStream(fs);
                    fs.Close();
                    */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read snapshot.\n" + ex.Message, "Read Snapshot", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return snapshot;
        }

        public BCFComponent CreateBCF(string bcfPath)
        {
            BCFComponent bcfComponent = new BCFComponent();
            try
            {
                string[] directories = bcfPath.Split(Path.DirectorySeparatorChar);
                string tempBcfPath = "";
                string tempZipPath = "";
                string tempZipFolder = "";
                FindFilePaths(bcfPath, out tempBcfPath, out tempZipPath, out tempZipFolder);

                if (!Directory.Exists(tempZipFolder))
                {
                    Directory.CreateDirectory(tempZipFolder);
                }

                bcfComponent.Markup = WriteDefaultMarkup();
                string guidFolder = Path.Combine(tempZipFolder, bcfComponent.Markup.Topic.Guid);
                if (!Directory.Exists(guidFolder))
                {
                    Directory.CreateDirectory(guidFolder);
                }
                bcfComponent.DirectoryPath = guidFolder;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create BCF.\n" + ex.Message, "Create BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfComponent;
        }

        private Markup WriteDefaultMarkup()
        {
            Markup markup = new Markup();
            try
            {
                HeaderFile[] headerfiles = new HeaderFile[1];
                HeaderFile header = new HeaderFile();
                header.IfcProject = GUIDUtil.CreateGUID(m_doc.ProjectInformation);
                header.Filename = m_doc.Title;
                header.Date = DateTime.Now;
                headerfiles[0] = header;
                markup.Header = headerfiles;

                Topic topic = new Topic();
                topic.Guid = Guid.NewGuid().ToString();
                topic.Title = "Color Schemes Editor" ;

                BimSnippet bimSnippet = new BimSnippet();
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                string schemaPath = Path.GetDirectoryName(currentAssembly) + "\\Resources\\colorscheme.xsd";
                bimSnippet.ReferenceSchema = schemaPath;
                bimSnippet.Reference = "colorscheme.xml";
                topic.BimSnippet = bimSnippet;
                markup.Topic = topic;

                Comment[] comments = new Comment[1];
                Comment comment = new Comment();
                comment.Guid = Guid.NewGuid().ToString();
                comment.Date = header.Date;
                comment.Author = Environment.UserName;
                comment.Comment1 = "Color Schemes and Color Filters by Add-Ins";
                CommentTopic commentTopic = new CommentTopic();
                commentTopic.Guid = topic.Guid;
                comment.Topic = commentTopic;
                comments[0] = comment;
                markup.Comment = comments;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to wirte the default Markup.\n" + ex.Message, "Write Default Markup", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return markup;
        }

        public VisualizationInfo GetVisInfo(Dictionary<int, ElementProperties> elementDictionary, string imagePath)
        {
            VisualizationInfo visInfo = new VisualizationInfo();
            try
            {
                visInfo.Components = new Component[elementDictionary.Count];

                int i = 0;
                foreach (ElementProperties ep in elementDictionary.Values)
                {
                    Component component = new Component();
                    component.IfcGuid = ep.IfcGuid;
                    component.Selected = true;
                    component.Visible = true;
                    component.Color = ep.Definition.Color;
                    component.OriginatingSystem = m_app.Application.VersionName;
                    component.AuthoringToolId = ep.ElementId.ToString();

                    visInfo.Components[i] = component;
                    i++;
                }

                visInfo.OrthogonalCamera = GetOrthogonalCamera(elementDictionary, imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the visualization information.\n"+ex.Message, "Get Visualization Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return visInfo;
        }

        private OrthogonalCamera GetOrthogonalCamera(Dictionary<int, ElementProperties> elementDictionary, string imagePath)
        {
            OrthogonalCamera orthoCamera = new OrthogonalCamera();
            try
            {
                BoundingBoxXYZ boundingBox = new BoundingBoxXYZ();
                boundingBox.Enabled = true;
                for (int i = 0; i < 3; i++)
                {
                    boundingBox.set_MinEnabled(i, true);
                    boundingBox.set_MaxEnabled(i, true);
                    boundingBox.set_BoundEnabled(0, i, true);
                    boundingBox.set_BoundEnabled(1, i, true);
                }

                BoundingBoxXYZ tempBoundingBox = elementDictionary.First().Value.RevitElement.get_BoundingBox(null);
                tempBoundingBox.Enabled = true;

                double maxX = tempBoundingBox.Max.X;
                double maxY = tempBoundingBox.Max.Y;
                double maxZ = tempBoundingBox.Max.Z;
                double minX = tempBoundingBox.Min.X;
                double minY = tempBoundingBox.Min.Y;
                double minZ = tempBoundingBox.Min.Z;

                List<ElementId> elementIds = new List<ElementId>();
                Dictionary<int, Category> categories = new Dictionary<int, Category>();
                foreach (ElementProperties ep in elementDictionary.Values)
                {
                    Element element = ep.RevitElement;
                    if (null != element)
                    {
                        try
                        {
                            if (!categories.ContainsKey(element.Category.Id.IntegerValue)) { categories.Add(element.Category.Id.IntegerValue, element.Category); }
                            BoundingBoxXYZ bbBox = element.get_BoundingBox(null);
                            bbBox.Enabled = true;
                            elementIds.Add(element.Id);
                            if (null != boundingBox)
                            {
                                if (bbBox.Max.X > maxX) { maxX = bbBox.Max.X; }
                                if (bbBox.Max.Y > maxY) { maxY = bbBox.Max.Y; }
                                if (bbBox.Max.Z > maxZ) { maxZ = bbBox.Max.Z; }
                                if (bbBox.Min.X < minX) { minX = bbBox.Min.X; }
                                if (bbBox.Min.Y < minY) { minY = bbBox.Min.Y; }
                                if (bbBox.Min.Z < minZ) { minZ = bbBox.Min.Z; }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                XYZ xyzMax = new XYZ(maxX, maxY, maxZ);
                XYZ xyzMin = new XYZ(minX, minY, minZ);

                boundingBox.set_Bounds(0, xyzMin);
                boundingBox.set_Bounds(1, xyzMax);


                ViewFamilyType view3dFamilyType = null;
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Element> elements = collector.OfClass(typeof(ViewFamilyType)).ToElements().ToList();
                foreach (Element element in elements)
                {
                    ViewFamilyType viewfamilytype = element as ViewFamilyType;
                    if (viewfamilytype.ViewFamily == ViewFamily.ThreeDimensional)
                    {
                        view3dFamilyType = viewfamilytype; break;
                    }
                }

                if (null != view3dFamilyType)
                {
                    using (TransactionGroup transGroup = new TransactionGroup(m_doc))
                    {
                        transGroup.Start("Start Creating View 3D");
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Create View");
                           
                            View3D view3d = View3D.CreateIsometric(m_doc, view3dFamilyType.Id);
                            view3d.SetSectionBox(boundingBox);
                            view3d.GetSectionBox().Enabled = true;
                            view3d.DetailLevel = ViewDetailLevel.Fine;

                            foreach (Category category in categories.Values)
                            {
                                if (category.get_AllowsVisibilityControl(view3d))
                                {
#if RELEASE2017 || RELEASE2018
                                    view3d.SetCategoryHidden(category.Id, false);
#else
                                    view3d.SetVisibility(category, true);
#endif

                                }
                            }
                                                  
                            view3d.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE).Set(4);

                            //m_app.ActiveUIDocument.ActiveView = view3d;
                            //m_app.ActiveUIDocument.RefreshActiveView();

                            XYZ eyePostion = view3d.GetOrientation().EyePosition;
                            Point viewPoint = new Point();
                            viewPoint.X = eyePostion.X; viewPoint.Y = eyePostion.Y; viewPoint.Z = eyePostion.Z;
                            orthoCamera.CameraViewPoint = viewPoint;

                            XYZ forwardDirection = view3d.GetOrientation().ForwardDirection;
                            Direction fDirection = new Direction();
                            fDirection.X = forwardDirection.X; fDirection.Y = forwardDirection.Y; fDirection.Z = forwardDirection.Z;
                            orthoCamera.CameraDirection = fDirection;

                            XYZ upDirection = view3d.GetOrientation().UpDirection;
                            Direction uDirection = new Direction();
                            uDirection.X = upDirection.X; uDirection.Y = upDirection.Y; uDirection.Z = upDirection.Z;
                            orthoCamera.CameraUpVector = uDirection;

                            orthoCamera.ViewToWorldScale = view3d.Scale;
                            m_app.ActiveUIDocument.RefreshActiveView();
                            trans.Commit();

                            trans.Start("Export Image");
                            //create snapshot.png
                            ImageExportOptions option = new ImageExportOptions();
                            option.HLRandWFViewsFileType = ImageFileType.PNG;
                            option.ImageResolution = ImageResolution.DPI_300;
                            option.ShouldCreateWebSite = false;
                            option.ExportRange = ExportRange.SetOfViews;
                            option.FilePath = imagePath;
                            List<ElementId> viewIds = new List<ElementId>();
                            viewIds.Add(view3d.Id);
                            option.SetViewsAndSheets(viewIds);

                            if (ImageExportOptions.IsValidFileName(option.FilePath))
                            {
                                m_doc.ExportImage(option);
                            }
                            trans.Commit();
                        }
                        transGroup.RollBack();
                    }

                    if (File.Exists(imagePath)) { File.Delete(imagePath); }
                    string[] fileNames = Directory.GetFiles(Path.GetDirectoryName(imagePath), "snapshot*");
                    foreach (string fName in fileNames)
                    {
                        if (Path.GetExtension(fName) == ".png" || Path.GetExtension(fName) == ".jpg")
                        {
                            File.Move(fName, imagePath);
                            if (File.Exists(fName)) { File.Delete(fName); }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the orthogonal camera.\n" + ex.Message, "Get Orthogonal Camera", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return orthoCamera;
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
                    string colorPath = Path.Combine(guidFolder, "colorscheme.xml");

                    bool markupResult = WriteMarkup(markupPath, bcfComponent.Markup);
                    bool viewResult = WriteVisualizationInfo(viewPath, bcfComponent.VisualizationInfo);
                    bool colorResult = WriteColorSchemeInfo(colorPath, bcfComponent.ColorSchemeInfo);
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
            string[] fileNames = new string[] { "colorscheme.xml", "markup.bcf", "snapshot.png", "viewpoint.bcfv" };
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
        public Image FileImage { get; set; }
    }

    public class ElementProperties
    {
        private Element revitElement = null;
        private int elementIdInteger = 0;
        private ElementId elementId = ElementId.InvalidElementId;
        private ColorDefinition definition = new ColorDefinition();
        private string ifcGuid = "";

        public Element RevitElement { get { return revitElement; } set { revitElement = value; } }
        public int ElementIdInt { get { return elementIdInteger; } set { elementIdInteger = value; } }
        public ElementId ElementId { get { return elementId; } set { elementId = value; } }
        public ColorDefinition Definition { get { return definition; } set { definition = value; } }
        public string IfcGuid { get { return ifcGuid; } set { ifcGuid = value; } }

        public ElementProperties(Element element, ColorDefinition colorDef)
        {
            revitElement = element;
            elementId = element.Id;
            elementIdInteger = element.Id.IntegerValue;
            definition = colorDef;
            ifcGuid = GUIDUtil.CreateGUID(revitElement);
        }

        public ElementProperties(Element element)
        {
            revitElement = element;
            elementId = element.Id;
            elementIdInteger = element.Id.IntegerValue;
            ifcGuid = GUIDUtil.CreateGUID(revitElement);
        }
    }
}
