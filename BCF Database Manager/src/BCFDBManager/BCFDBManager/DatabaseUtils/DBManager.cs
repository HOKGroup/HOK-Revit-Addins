using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BCFDBManager.BCFUtils;
using System.Windows.Threading;

namespace BCFDBManager.DatabaseUtils
{
    public static class DBManager
    {
        public static Dictionary<string/*fileId*/, BCFZIP> ReadDatabase(string dbFile)
        {
            Dictionary<string/*fileId*/, BCFZIP> bcfFiles = new Dictionary<string/*fileId*/, BCFZIP>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        //read sequnce
                        //BCFFileInfo, FileTopics, 
                        //ProjectExtention, Version

                        cmd.CommandText = "SELECT * FROM BCFFileInfo";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BCFZIP bcfZip = new BCFZIP();
                                bcfZip.FileId = reader.GetString(reader.GetOrdinal("Guid"));
                                bcfZip.ZipFileName = reader.GetString(reader.GetOrdinal("FileName"));
                                bcfZip.ZipFilePath = reader.GetString(reader.GetOrdinal("FilePath"));
                                bcfZip.UploadedBy = reader.GetString(reader.GetOrdinal("UploadedBy"));
                                bcfZip.UploadedDate = reader.GetDateTime(reader.GetOrdinal("UploadedDate"));
                                bcfZip.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
                                
                                string projectGuid = reader.GetString(reader.GetOrdinal("Project_Guid"));
                                ProjectExtension projectExt = new ProjectExtension();
                                projectExt.Guid = projectGuid;
                                bcfZip.ProjectFile = projectExt;

                                string versionGuid = reader.GetString(reader.GetOrdinal("Version_Guid"));
                                Version version = new Version();
                                version.Guid = versionGuid;
                                bcfZip.VersionFile = version;

                                if (!bcfFiles.ContainsKey(bcfZip.FileId))
                                {
                                    bcfFiles.Add(bcfZip.FileId, bcfZip);
                                }
                            }
                        }

                        List<string> fileIds = bcfFiles.Keys.ToList();
                        foreach (string fileId in fileIds)
                        {
                            BCFZIP bcfZip = bcfFiles[fileId];
                            
                            bcfZip.ProjectFile = ReadProjectInfo(connection, bcfZip.ProjectFile.Guid);
                            bcfZip.VersionFile = ReadVersionInfo(connection, bcfZip.VersionFile.Guid);

                            List<string> topicIds = ReadTopicIds(connection, fileId);
                            Dictionary<string, BCFComponent> components = new Dictionary<string, BCFComponent>();
                            foreach (string topicId in topicIds)
                            {
                                BCFComponent component = ReadBCFComponent(connection, topicId);
                                if (!components.ContainsKey(topicId))
                                {
                                    components.Add(topicId, component);
                                }
                            }
                            bcfZip.BCFComponents = components;

                            bcfFiles.Remove(fileId);
                            bcfFiles.Add(fileId, bcfZip);
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read database from the file, "+dbFile+"\n"+ex.Message, "Read Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfFiles;
        }

        private static List<string> ReadTopicIds(SQLiteConnection connection, string fileId)
        {
            List<string> topicIds = new List<string>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT Topic_Guid FROM FileTopics WHERE File_Guid = '" + fileId + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string topicId = reader.GetString(reader.GetOrdinal("Topic_Guid"));
                            if (!topicIds.Contains(topicId))
                            {
                                topicIds.Add(topicId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read topic Ids.\n" + ex.Message, "Read Topic Guids", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return topicIds;
        }

        private static ProjectExtension ReadProjectInfo(SQLiteConnection connection, string projectGuid)
        {
            ProjectExtension projectExt = new ProjectExtension();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Project WHERE Guid = '"+projectGuid+"'";
                    
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Project project = new Project();
                            project.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            project.ProjectId = reader.GetString(reader.GetOrdinal("ProjectId"));
                            project.Name = reader.GetString(reader.GetOrdinal("ProjectName"));
                            projectExt.Guid = reader.GetString(reader.GetOrdinal("ProjectExtension_Guid"));
                            projectExt.Project = project;
                        }
                    }

                    if (!string.IsNullOrEmpty(projectExt.Guid))
                    {
                        cmd.CommandText = "SELECT * FROM ProjectExtension WHERE Guid = '"+projectExt.Guid+"'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                projectExt.ExtensionSchema = reader.GetString(reader.GetOrdinal("ExtensionSchema"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read project information.\n" + ex.Message, "Read Project Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return projectExt;
        }

        private static Version ReadVersionInfo(SQLiteConnection connection, string versionGuid)
        {
            Version version = new Version();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Version WHERE Guid = '" + versionGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            version.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            version.VersionId = reader.GetString(reader.GetOrdinal("VersionId"));
                            version.DetailedVersion = reader.GetString(reader.GetOrdinal("DetailedVersion"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read version information.\n"+ex.Message, "Read Version Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return version;
        }

        private static BCFComponent ReadBCFComponent(SQLiteConnection connection, string topicGuid)
        {
            BCFComponent component = new BCFComponent();
            try
            {
                component.MarkupInfo = ReadMarkupInfo(connection, topicGuid);
                Dictionary<string, VisualizationInfo> viewPoints = new Dictionary<string, VisualizationInfo>();
                foreach (ViewPoint vp in component.MarkupInfo.Viewpoints)
                {
                    VisualizationInfo visInfo = ReadVisInfo(connection, vp.Guid);
                    if (!viewPoints.ContainsKey(vp.Guid))
                    {
                        viewPoints.Add(vp.Guid, visInfo);
                    }
                }
                component.Viewpoints = viewPoints;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read BCF component.\n" + ex.Message, "Read BCF Component", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return component;
        }

        private static Markup ReadMarkupInfo(SQLiteConnection connection, string topicGuid)
        {
            Markup markup = new Markup();
            try
            {
                //Topic, Labels, HeaderFile, BimSnippet, DocumentReferences, RelatedTopics, Comment, Viewpoint, Viewpoints
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    Topic topic = new Topic();

                    #region Topic
                    cmd.CommandText = "SELECT * FROM Topic WHERE Guid = '" + topicGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topic.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            topic.TopicType = reader.GetString(reader.GetOrdinal("TopicType"));
                            topic.TopicStatus = reader.GetString(reader.GetOrdinal("TopicStatus"));
                            topic.Title = reader.GetString(reader.GetOrdinal("Title"));
                            topic.ReferenceLink = reader.GetString(reader.GetOrdinal("ReferenceLink"));
                            topic.Description = reader.GetString(reader.GetOrdinal("Description"));
                            topic.Priority = reader.GetString(reader.GetOrdinal("Priority"));
                            topic.Index = reader.GetString(reader.GetOrdinal("TopicIndex"));
                            topic.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
                            topic.CreationAuthor = reader.GetString(reader.GetOrdinal("CreationAuthor"));

                            if (reader["ModifiedDate"] != System.DBNull.Value)
                            {
                                topic.ModifiedDateSpecified = true;
                                topic.ModifiedDate = Convert.ToDateTime(reader["ModifiedDate"]);
                            }
                           
                            topic.ModifiedAuthor = reader.GetString(reader.GetOrdinal("ModifiedAuthor"));
                            topic.AssignedTo = reader.GetString(reader.GetOrdinal("AssignedTo"));
                        }
                    }
                    #endregion

                    #region Labels
                    cmd.CommandText = "SELECT Label FROM Labels WHERE Topic_Guid = '" + topicGuid + "'";
                    List<string> labels = new List<string>();
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string label = reader.GetString(reader.GetOrdinal("Label"));
                            labels.Add(label);
                        }
                    }
                    topic.Labels = labels;
                    #endregion

                    #region HeaderFile
                    List<HeaderFile> headerFiles = new List<HeaderFile>();
                    cmd.CommandText = "SELECT * FROM HeaderFile WHERE Topic_Guid = '" + topicGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            HeaderFile headerFile = new HeaderFile();
                            headerFile.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            headerFile.IfcProject = reader.GetString(reader.GetOrdinal("IfcProject"));
                            headerFile.IfcSpatialStructureElement = reader.GetString(reader.GetOrdinal("IfcSpatialStructureElement"));
                            headerFile.isExternal = reader.GetBoolean(reader.GetOrdinal("isExternal"));
                            headerFile.Filename = reader.GetString(reader.GetOrdinal("FileName"));

                            if (reader["Date"] != System.DBNull.Value)
                            {
                                headerFile.DateSpecified = true;
                                headerFile.Date = Convert.ToDateTime(reader["Date"]);
                            }
                            
                            headerFile.Reference = reader.GetString(reader.GetOrdinal("Reference"));
                            headerFiles.Add(headerFile);
                        }
                    }
                    markup.Header = headerFiles;
                    #endregion

                    #region BimSnippet
                    BimSnippet bimSnippet = null;
                    cmd.CommandText = "SELECT * FROM BimSnippet WHERE Topic_Guid = '" + topicGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bimSnippet = new BimSnippet();
                            bimSnippet.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            bimSnippet.SnippetType = reader.GetString(reader.GetOrdinal("SnippetType"));
                            bimSnippet.isExternal = reader.GetBoolean(reader.GetOrdinal("isExternal"));
                            bimSnippet.Reference = reader.GetString(reader.GetOrdinal("Reference"));
                            bimSnippet.ReferenceSchema = reader.GetString(reader.GetOrdinal("ReferenceSchema"));
                            if (reader["FileContent"] != System.DBNull.Value)
                            {
                                bimSnippet.FileContent = (byte[])reader["FileContent"];
                            }
                        }
                    }
                    if (null != bimSnippet)
                    {
                        topic.BimSnippet = bimSnippet;
                    }
                    #endregion

                    #region DocumentReferences
                    List<TopicDocumentReferences> docReferences = new List<TopicDocumentReferences>();
                    cmd.CommandText = "SELECT * FROM DocumentReferences WHERE Topic_Guid = '" + topicGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TopicDocumentReferences docRef = new TopicDocumentReferences();
                            docRef.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            docRef.isExternal = reader.GetBoolean(reader.GetOrdinal("isExternal"));
                            docRef.ReferencedDocument = reader.GetString(reader.GetOrdinal("ReferenceDocument"));
                            if (reader["FileContent"] != System.DBNull.Value)
                            {
                                docRef.FileContent = (byte[])reader["FileContent"];
                            }
                            docRef.Description = reader.GetString(reader.GetOrdinal("Description"));
                            docReferences.Add(docRef);
                        }
                    }
                    topic.DocumentReferences = docReferences;
                    #endregion

                    #region RelatedTopics
                    List<TopicRelatedTopics> relTopics = new List<TopicRelatedTopics>();
                    cmd.CommandText = "SELECT * FROM RelatedTopics WHERE Topic_Guid = '" + topicGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TopicRelatedTopics relTopic = new TopicRelatedTopics();
                            relTopic.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            relTopics.Add(relTopic);
                        }
                    }
                    topic.RelatedTopics = relTopics;
                    markup.Topic = topic;
                    #endregion

                    #region comment
                    List<Comment> comments = new List<Comment>();
                    cmd.CommandText = "SELECT * FROM Comment WHERE Topic_Guid = '" + topicGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Comment comment = new Comment();
                            comment.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            comment.VerbalStatus = reader.GetString(reader.GetOrdinal("VerbalStatus"));
                            comment.Status = reader.GetString(reader.GetOrdinal("Status"));
                            comment.Date = reader.GetDateTime(reader.GetOrdinal("Date"));
                            comment.Author = reader.GetString(reader.GetOrdinal("Author"));
                            comment.Comment1 = reader.GetString(reader.GetOrdinal("Comment"));
                            
                            if (reader["ModifiedDate"] != System.DBNull.Value)
                            {
                                comment.ModifiedDateSpecified = true;
                                comment.ModifiedDate = Convert.ToDateTime(reader["ModifiedDate"]);
                            }
                            comment.ModifiedAuthor = reader.GetString(reader.GetOrdinal("ModifiedAuthor"));
                            comments.Add(comment);
                        }
                    }
                    #endregion

                    #region ViewPoint
                    for (int i = comments.Count - 1; i > -1; i--)
                    {
                        Comment comment = comments[i];
                        CommentViewpoint viewPoint = new CommentViewpoint();
                        cmd.CommandText = "SELECT * FROM Viewpoint WHERE Comment_Guid = '" + comment.Guid + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                viewPoint.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            }
                        }
                        comment.Viewpoint = viewPoint;
                        comments.RemoveAt(i);
                        comments.Insert(i, comment);
                    }
                    markup.Comment = comments;
                    #endregion

                    #region ViewPoints
                    List<ViewPoint> viewPoints = new List<ViewPoint>();
                    cmd.CommandText = "SELECT * FROM Viewpoints WHERE Topic_Guid = '" + topicGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ViewPoint vp = new ViewPoint();
                            vp.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            vp.Viewpoint = reader.GetString(reader.GetOrdinal("Viewpoint"));
                            vp.Snapshot = reader.GetString(reader.GetOrdinal("Snapshot"));
                            if (reader["Snapshot_Image"] != System.DBNull.Value)
                            {
                                vp.SnapshotImage = (byte[])reader["Snapshot_Image"];
                            }
                            viewPoints.Add(vp);
                        }
                    }
                    markup.Viewpoints = viewPoints;
                    #endregion
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read Markup Information.\n" + ex.Message, "Read Markup Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return markup;
        }

        private static VisualizationInfo ReadVisInfo(SQLiteConnection connection, string visGuid)
        {
            VisualizationInfo visInfo = new VisualizationInfo();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    //Bitmaps, ClippingPlane, Components, Lines, OrthogonalCamera, PerspectiveCamera
                    #region Bitmaps
                    List<VisualizationInfoBitmaps> bitmaps = new List<VisualizationInfoBitmaps>();
                    cmd.CommandText = "SELECT * FROM Bitmaps WHERE Viewpoints_Guid = '" + visGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            VisualizationInfoBitmaps bitmap = new VisualizationInfoBitmaps();
                            bitmap.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            bitmap.Bitmap = (BitmapFormat)Enum.Parse(typeof(BitmapFormat), reader.GetString(reader.GetOrdinal("Bitmap")));
                            if (reader["Bitmap_Image"] != System.DBNull.Value)
                            {
                                bitmap.BitmapImage = (byte[])reader["Bitmap_Image"];
                            }

                            Point point = new Point();
                            point.Guid = reader.GetString(reader.GetOrdinal("Location"));
                            bitmap.Location = point;

                            Direction normal = new Direction();
                            normal.Guid = reader.GetString(reader.GetOrdinal("Normal"));
                            bitmap.Normal = normal;

                            Direction up = new Direction();
                            up.Guid = reader.GetString(reader.GetOrdinal("Up"));
                            bitmap.Up = up;
                                 
                            bitmap.Reference = reader.GetString(reader.GetOrdinal("Reference"));
                            bitmap.Height = reader.GetDouble(reader.GetOrdinal("Height"));
                            bitmaps.Add(bitmap);
                        }
                    }

                    for (int i = bitmaps.Count - 1; i > -1; i--)
                    {
                        VisualizationInfoBitmaps bitmap = bitmaps[i];
                        cmd.CommandText = "SELECT * FROM Point WHERE Guid = '" + bitmap.Location.Guid + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Point pt = new Point();
                                pt.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                                pt.X = reader.GetDouble(reader.GetOrdinal("X"));
                                pt.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                                pt.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                                bitmap.Location = pt;
                            }
                        }

                        cmd.CommandText = "SELECT * FROM Direction WHERE Guid = '" + bitmap.Normal.Guid + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Direction direction = new Direction();
                                direction.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                                direction.X = reader.GetDouble(reader.GetOrdinal("X"));
                                direction.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                                direction.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                                bitmap.Normal = direction;
                            }
                        }

                        cmd.CommandText = "SELECT * FROM Direction WHERE Guid = '" + bitmap.Up.Guid + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Direction direction = new Direction();
                                direction.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                                direction.X = reader.GetDouble(reader.GetOrdinal("X"));
                                direction.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                                direction.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                                bitmap.Up= direction;
                            }
                        }
                        bitmaps.RemoveAt(i);
                        bitmaps.Insert(i, bitmap);
                    }
                    visInfo.Bitmaps = bitmaps;
                    #endregion

                    #region ClippingPlane
                    List<ClippingPlane> clippingPlanes = new List<ClippingPlane>();
                    cmd.CommandText = "SELECT * FROM ClippingPlane WHERE Viewpoints_Guid = '" + visGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ClippingPlane cp = new ClippingPlane();
                            cp.Guid = reader.GetString(reader.GetOrdinal("Guid"));

                            Point point = new Point();
                            point.Guid = reader.GetString(reader.GetOrdinal("Location"));
                            cp.Location = point;

                            Direction direction = new Direction();
                            direction.Guid = reader.GetString(reader.GetOrdinal("Direction"));
                            cp.Direction = direction;

                            clippingPlanes.Add(cp);
                        }
                    }

                    for (int i = clippingPlanes.Count - 1; i > -1; i--)
                    {
                        ClippingPlane cp = clippingPlanes[i];
                        cmd.CommandText = "SELECT * FROM Point WHERE Guid = '" + cp.Location.Guid + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Point pt = new Point();
                                pt.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                                pt.X = reader.GetDouble(reader.GetOrdinal("X"));
                                pt.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                                pt.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                                cp.Location = pt;
                            }
                        }

                        cmd.CommandText = "SELECT * FROM Direction WHERE Guid = '" + cp.Direction.Guid + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Direction direction = new Direction();
                                direction.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                                direction.X = reader.GetDouble(reader.GetOrdinal("X"));
                                direction.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                                direction.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                                cp.Direction = direction;
                            }
                        }
                        clippingPlanes.RemoveAt(i);
                        clippingPlanes.Insert(i, cp);
                    }
                    visInfo.ClippingPlanes = clippingPlanes;
                    #endregion

                    #region Components
                    List<Component> components = new List<Component>();
                    cmd.CommandText = "SELECT * FROM Components WHERE Viewpoints_Guid = '" + visGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Component comp = new Component();
                            comp.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            comp.IfcGuid = reader.GetString(reader.GetOrdinal("IfcGuid"));
                            comp.Selected = reader.GetBoolean(reader.GetOrdinal("Selected"));
                            comp.Visible = reader.GetBoolean(reader.GetOrdinal("Visible"));
                            if (reader["Color"] != System.DBNull.Value)
                            {
                                comp.Color = (byte[])reader["Color"];
                            }
                            comp.OriginatingSystem = reader.GetString(reader.GetOrdinal("OriginatingSystem"));
                            comp.AuthoringToolId = reader.GetString(reader.GetOrdinal("AuthoringToolId"));
                            components.Add(comp);
                        }
                    }
                    visInfo.Components = components;
                    #endregion

                    #region Lines
                    List<Line> lines = new List<Line>();
                    cmd.CommandText = "SELECT * FROM Lines WHERE Viewpoints_Guid = '" + visGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Line line = new Line();
                            line.Guid = reader.GetString(reader.GetOrdinal("Guid"));

                            Point startPoint = new Point();
                            startPoint.Guid = reader.GetString(reader.GetOrdinal("StartPoint"));
                            line.StartPoint = startPoint;

                            Point endPoint = new Point();
                            endPoint.Guid = reader.GetString(reader.GetOrdinal("EndPoint"));
                            line.EndPoint = endPoint;

                            lines.Add(line);  
                        }
                    }

                    for (int i = lines.Count - 1; i > -1; i--)
                    {
                        Line line = lines[i];
                        Point startPoint = line.StartPoint;
                        cmd.CommandText = "SELECT * FROM Point WHERE Guid = '" + startPoint.Guid + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                startPoint.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                                startPoint.X = reader.GetDouble(reader.GetOrdinal("X"));
                                startPoint.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                                startPoint.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                            }
                        }

                        Point endPoint = line.EndPoint;
                        cmd.CommandText = "SELECT * FROM Point WHERE Guid = '" + endPoint.Guid + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                endPoint.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                                endPoint.X = reader.GetDouble(reader.GetOrdinal("X"));
                                endPoint.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                                endPoint.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                            }
                        }

                        line.StartPoint = startPoint;
                        line.EndPoint = endPoint;

                        lines.RemoveAt(i);
                        lines.Insert(i, line);
                    }
                    visInfo.Lines = lines;
                    #endregion

                    #region OrthogonalCamera
                    OrthogonalCamera orthoCamera = null;
                    Point viewPoint = new Point();
                    Direction viewDirection = new Direction();
                    Direction upVector = new Direction();
                    cmd.CommandText = "SELECT * FROM OrthogonalCamera WHERE Viewpoints_Guid = '" + visGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orthoCamera = new OrthogonalCamera();
                            orthoCamera.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            viewPoint.Guid = reader.GetString(reader.GetOrdinal("CameraViewPoint"));
                            viewDirection.Guid = reader.GetString(reader.GetOrdinal("CameraDirection"));
                            upVector.Guid = reader.GetString(reader.GetOrdinal("CameraUpVector"));
                            orthoCamera.ViewToWorldScale = reader.GetDouble(reader.GetOrdinal("ViewToWorldScale")); 
                        }
                    }

                    cmd.CommandText = "SELECT * FROM Point WHERE Guid = '" + viewPoint.Guid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            viewPoint.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            viewPoint.X = reader.GetDouble(reader.GetOrdinal("X"));
                            viewPoint.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                            viewPoint.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                        }
                    }
                    if (null != orthoCamera)
                    {
                        orthoCamera.CameraViewPoint = viewPoint;
                    }
                    
                    cmd.CommandText = "SELECT * FROM Direction WHERE Guid = '" + viewDirection.Guid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            viewDirection.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            viewDirection.X = reader.GetDouble(reader.GetOrdinal("X"));
                            viewDirection.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                            viewDirection.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                        }
                    }
                    if (null != orthoCamera)
                    {
                        orthoCamera.CameraDirection = viewDirection;
                    }
                    
                    cmd.CommandText = "SELECT * FROM Direction WHERE Guid = '" + upVector.Guid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            upVector.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            upVector.X = reader.GetDouble(reader.GetOrdinal("X"));
                            upVector.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                            upVector.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                        }
                    }
                    if (null != orthoCamera)
                    {
                        orthoCamera.CameraUpVector = upVector;
                        visInfo.OrthogonalCamera = orthoCamera;
                    }
                    
                    #endregion

                    #region PerspectiveCamera
                    PerspectiveCamera persCamera = null;
                    viewPoint = new Point();
                    viewDirection = new Direction();
                    upVector = new Direction();
                    cmd.CommandText = "SELECT * FROM PerspectiveCamera WHERE Viewpoints_Guid = '" + visGuid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            persCamera = new PerspectiveCamera();
                            persCamera.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            viewPoint.Guid = reader.GetString(reader.GetOrdinal("CameraViewPoint"));
                            viewDirection.Guid = reader.GetString(reader.GetOrdinal("CameraDirection"));
                            upVector.Guid = reader.GetString(reader.GetOrdinal("CameraUpVector"));
                            persCamera.FieldOfView = reader.GetDouble(reader.GetOrdinal("FieldOfView"));
                        }
                    }

                    cmd.CommandText = "SELECT * FROM Point WHERE Guid = '" + viewPoint.Guid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            viewPoint.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            viewPoint.X = reader.GetDouble(reader.GetOrdinal("X"));
                            viewPoint.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                            viewPoint.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                        }
                    }
                    if (null != persCamera)
                    {
                        persCamera.CameraViewPoint = viewPoint;
                    }
                    
                    cmd.CommandText = "SELECT * FROM Direction WHERE Guid = '" + viewDirection.Guid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            viewDirection.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            viewDirection.X = reader.GetDouble(reader.GetOrdinal("X"));
                            viewDirection.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                            viewDirection.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                        }
                    }
                    if (null != persCamera)
                    {
                        persCamera.CameraDirection = viewDirection;
                    }

                    cmd.CommandText = "SELECT * FROM Direction WHERE Guid = '" + upVector.Guid + "'";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            upVector.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            upVector.X = reader.GetDouble(reader.GetOrdinal("X"));
                            upVector.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                            upVector.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                        }
                    }
                    if (null != persCamera)
                    {
                        persCamera.CameraUpVector = upVector;
                        visInfo.PerspectiveCamera = persCamera;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read Visualization Information.\n" + ex.Message, "Read Visualization Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return visInfo;
        }

        private static List<string> GetTableNames(string dbFile)
        {
            List<string> tableNames = new List<string>();
            try
            {
                DataTable nameTable = new DataTable();
                using (SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
                {
                    m_dbConnection.Open();
                    string query = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
                    
                    
                    using (SQLiteCommand cmd = new SQLiteCommand(query, m_dbConnection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            nameTable.Load(reader);
                        }
                    }
                    m_dbConnection.Close();
                }

                if (nameTable.Rows.Count > 0)
                {
                    foreach (DataRow row in nameTable.Rows)
                    {
                        string tableName = row[0].ToString();
                        if (!tableNames.Contains(tableName))
                        {
                            tableNames.Add(tableName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get table names.\n"+ex.Message, "Get Table Names", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return tableNames;
        }

        public static bool CreateTables(string dbFile, BCFZIP bcfzip)
        {
            bool written = false;
            try
            {
                if (!File.Exists(dbFile))
                {
                    SQLiteConnection.CreateFile(dbFile);
                }

                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
                {
                    connection.Open();
                    try
                    {
                        //create tables
                        bool tableInfoCollected = TableManager.GetTablesInfo();
                        Dictionary<string, TableProperties> tableInfo = new Dictionary<string, TableProperties>();
                        if (tableInfoCollected)
                        {
                            tableInfo = TableManager.TablesInfo;
                        }

                        if (tableInfo.Count > 0)
                        {
                            using (SQLiteCommand command = new SQLiteCommand(connection))
                            {
                                //enable foreign key support
                                command.CommandText = "PRAGMA foreign_keys = ON";
                                command.ExecuteNonQuery();

                                var tableOrders = ((TableNames[])Enum.GetValues(typeof(TableNames))).OrderBy(x => x);

                                foreach (TableNames tn in tableOrders)
                                {
                                    string tableName = tn.ToString();
                                    if (tableInfo.ContainsKey(tableName))
                                    {
                                        TableProperties tp = tableInfo[tableName];
                                        try
                                        {
                                            command.CommandText = SqlCommandGenerator.CreateTableQuery(tp);
                                            command.ExecuteNonQuery();
                                        }
                                        catch (Exception ex)
                                        {
                                            string message = ex.Message;
                                        }
                                    }
                                }
                            }
                        }

                        written = true;
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write database.\n" + ex.Message, "Write Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return written;
        }

        public static bool WriteDatabase(string dbFile, BCFZIP bcfzip)
        {
            bool result = false;
            try
            {
                result = true;
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
                {
                    connection.Open();
                    try
                    {
                        bool projectValuesInserted = InsertProjectValues(bcfzip, connection);
                        bool versionValuesInserted = InsertVersionValues(bcfzip, connection);
                        bool componentValuesInserted = InsertBCFComponentValues(bcfzip, connection);
                        bool customValuesInserted = InsertCustomValues(bcfzip, connection);
                        result = projectValuesInserted && versionValuesInserted && componentValuesInserted && customValuesInserted;
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                    connection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Failed to write database.\n" + ex.Message, "Write Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private static bool InsertProjectValues(BCFZIP bcfZip, SQLiteConnection connection)
        {
            bool inserted = false;
            try
            {
                using (SQLiteTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            ProjectExtension pExt = bcfZip.ProjectFile;

                            StringBuilder strBuilder = new StringBuilder();
                            strBuilder.Append("INSERT INTO ProjectExtension (Guid, ExtensionSchema) VALUES ");
                            strBuilder.Append("('"+pExt.Guid+"', '"+pExt.ExtensionSchema+"')");
                            cmd.CommandText = strBuilder.ToString();
                            int insertedRow = cmd.ExecuteNonQuery();

                            strBuilder = new StringBuilder();
                            strBuilder.Append("INSERT INTO Project (Guid, ProjectId, ProjectName, ProjectExtension_Guid) VALUES ");
                            strBuilder.Append("('" + pExt.Project.Guid + "', '" + pExt.Project.ProjectId + "', '" + pExt.Project.Name + "', '" + pExt.Guid + "')");
                            cmd.CommandText = strBuilder.ToString();
                            insertedRow = cmd.ExecuteNonQuery();
                            
                        }
                        trans.Commit();
                        inserted = true;
                    }
                    catch (SQLiteException ex)
                    {
                        trans.Rollback();
                        string message = ex.Message;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert project tables values.\n"+ex.Message, "Insert Project Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertVersionValues(BCFZIP bcfZip, SQLiteConnection connection)
        {
            bool inserted = false;
            try
            {
                using (SQLiteTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            Version version = bcfZip.VersionFile;
                           
                            StringBuilder strBuilder = new StringBuilder();
                            if (!string.IsNullOrEmpty(version.VersionId))
                            {
                                strBuilder.Append("INSERT INTO Version (Guid, VersionId, DetailedVersion) VALUES ");
                                strBuilder.Append("('" + version.Guid + "', '" + version.VersionId + "', '"+version.DetailedVersion+"')");
                                cmd.CommandText = strBuilder.ToString();
                                int insertedRow = cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                        inserted = true;
                    }
                    catch (SQLiteException ex)
                    {
                        trans.Rollback();
                        string message = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert Version tables values.\n" + ex.Message, "Insert Version Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertBCFComponentValues(BCFZIP bcfzip, SQLiteConnection connection)
        {
            bool inserted = false;
            try
            {
                foreach (string guid in bcfzip.BCFComponents.Keys)
                {
                    BCFComponent bcfComponent = bcfzip.BCFComponents[guid];
                    bool insertedMarkup = InsertMarkupValues(bcfComponent, connection);
                    bool insertedVisInfo = InsertVisInfoValues(bcfComponent, connection);
                }
                inserted = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert BCF Component values./n" + ex.Message, "Insert BCF Components Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertMarkupValues(BCFComponent bcfComponent, SQLiteConnection connection)
        {
            bool inserted = false;

            try
            {
                //  Topic = 3, Header = 4, File = 5, BimSnippet = 6, DocumentReferences = 7, RelatedTopics = 8, Comment = 9, Viewpoint = 10, Viewpoints = 11
                using (SQLiteTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            Markup markup = bcfComponent.MarkupInfo;
                            
                            #region Topic
                            Topic topic = markup.Topic;
                            StringBuilder strBuilder = new StringBuilder();
                            strBuilder.Append("INSERT INTO Topic (Guid, TopicType, TopicStatus, Title, ReferenceLink, Description, Priority, TopicIndex, CreationDate, CreationAuthor, ModifiedDate, ModifiedAuthor, AssignedTo) VALUES ");
                            strBuilder.Append("('" + topic.Guid + "', '" + topic.TopicType + "', '" + topic.TopicStatus + "', '" + topic.Title+"', '"+ topic.ReferenceLink + "', '"+ topic.Description + "', '"+topic.Priority+"', '");
                            strBuilder.Append(topic.Index+ "', @creationDate, '" + topic.CreationAuthor + "', @modifiedDate, '" + topic.ModifiedAuthor + "', '" + topic.AssignedTo + "') ");

                            cmd.CommandText = strBuilder.ToString();
                            cmd.Prepare();
                            cmd.Parameters.Add("@creationDate", DbType.DateTime);
                            cmd.Parameters["@creationDate"].Value = topic.CreationDate;
                            cmd.Parameters.Add("@modifiedDate", DbType.DateTime);
                            cmd.Parameters["@modifiedDate"].Value = topic.ModifiedDate;
                            try
                            {
                                int insertedRow = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "/n" + ex.Message;
                            }
                            
                            cmd.Parameters.Clear();
                            #endregion 

                            #region
                            if (topic.Labels.Count > 0)
                            {
                                strBuilder = new StringBuilder();
                                strBuilder.Append("INSERT INTO Labels (Label, Topic_Guid) VALUES ");

                                foreach (string label in topic.Labels)
                                {
                                    strBuilder.Append("('" + label + "', '" + topic.Guid + "'), ");
                                }

                                strBuilder.Remove(strBuilder.Length - 2, 2);
                                cmd.CommandText = strBuilder.ToString();
                                try
                                {
                                    int insertedRows = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "/n" + ex.Message;
                                }
                            }
                            #endregion

                            #region HeaderFile
                            List<HeaderFile> headerFiles = markup.Header;
                            if (headerFiles.Count > 0)
                            {
                                strBuilder = new StringBuilder();
                                strBuilder.Append("INSERT INTO HeaderFile (Guid, IfcProject, IfcSpatialStructureElement, isExternal, FileName, Date, Reference, Topic_Guid) VALUES ");
                                for (int i = 0; i < headerFiles.Count; i++)
                                {
                                    HeaderFile file = headerFiles[i];
                                    string dateParam = "@date" + i;
                                    strBuilder.Append("('" + file.Guid + "', '" + file.IfcProject + "', '" + file.IfcSpatialStructureElement + "', " + Convert.ToInt32(file.isExternal) + ", '" + file.Filename + "', ");
                                    strBuilder.Append(dateParam + ", '" + file.Reference + "', '" + topic.Guid + "' ), ");

                                    cmd.Parameters.Add(dateParam, DbType.DateTime);
                                    cmd.Parameters[dateParam].Value = file.Date;
                                }
                                strBuilder.Remove(strBuilder.Length - 2, 2);

                                cmd.CommandText = strBuilder.ToString();
                                try
                                {
                                    int insertedRow = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "/n" + ex.Message;
                                }
                                cmd.Parameters.Clear();
                            }
                            #endregion

                            #region BimSnippet
                            BimSnippet bimSnippet = topic.BimSnippet;
                            if (!string.IsNullOrEmpty(bimSnippet.Reference))
                            {
                                strBuilder = new StringBuilder();
                                strBuilder.Append("INSERT INTO BimSnippet (Guid, SnippetType, isExternal, Reference, ReferenceSchema, FileContent, Topic_Guid) VALUES ");
                                strBuilder.Append("('"+bimSnippet.Guid+"', '"+bimSnippet.SnippetType+"', "+ Convert.ToInt32(bimSnippet.isExternal)+", '"+bimSnippet.Reference+"', '"+bimSnippet.ReferenceSchema+"', @fileContent, '"+topic.Guid+"')");
                                if (!string.IsNullOrEmpty(bimSnippet.Reference) && null != bimSnippet.FileContent)
                                {
                                    cmd.Parameters.Add("@fileContent", DbType.Binary, bimSnippet.FileContent.Length);
                                    cmd.Parameters["@fileContent"].Value = bimSnippet.FileContent;
                                }
                                else
                                {
                                    cmd.Parameters.Add("@fileContent", DbType.Binary);
                                    cmd.Parameters["@fileContent"].Value = null;
                                }
                               
                                cmd.CommandText = strBuilder.ToString();
                                try
                                {
                                    int insertedRow = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "/n" + ex.Message;
                                }
                                cmd.Parameters.Clear();
                            }
                            #endregion

                            #region DocumentReferences
                            List<TopicDocumentReferences> docReferences = topic.DocumentReferences;
                            if (docReferences.Count > 0)
                            {
                                strBuilder = new StringBuilder();
                                strBuilder.Append("INSERT INTO DocumentReferences (Guid, isExternal, ReferenceDocument, FileContent, Description, Topic_Guid) VALUES ");
                                for (int i = 0; i < docReferences.Count; i++)
                                {
                                    TopicDocumentReferences docRef = docReferences[i];
                                    string contentParam = "@fileContent" + i;
                                    strBuilder.Append("('" + docRef.Guid + "', " + Convert.ToInt32(docRef.isExternal) + ", '" + docRef.ReferencedDocument + "', "+contentParam+", '" + docRef.Description + "', '"+topic.Guid+"'), ");
                                    if (!string.IsNullOrEmpty(docRef.ReferencedDocument) && null != docRef.FileContent)
                                    {
                                        cmd.Parameters.Add(contentParam, DbType.Binary, docRef.FileContent.Length);
                                        cmd.Parameters[contentParam].Value = docRef.FileContent;
                                    }
                                    else
                                    {
                                        cmd.Parameters.Add(contentParam, DbType.Binary);
                                        cmd.Parameters[contentParam].Value = null;
                                    }
                                }
                                strBuilder.Remove(strBuilder.Length - 2, 2);

                                cmd.CommandText = strBuilder.ToString();
                                try
                                {
                                    int insertedRow = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "/n" + ex.Message;
                                }
                                cmd.Parameters.Clear();
                            }
                            #endregion

                            #region RelatedTopics
                            List<TopicRelatedTopics> relatedTopics = topic.RelatedTopics;
                            if (relatedTopics.Count > 0)
                            {
                                strBuilder = new StringBuilder();
                                strBuilder.Append("INSERT INTO RelatedTopics (Guid, Topic_Guid) VALUES ");
                                foreach (TopicRelatedTopics relTopic in relatedTopics)
                                {
                                    strBuilder.Append("('" + relTopic.Guid + "', '" + topic.Guid + "'), ");
                                }
                                strBuilder.Remove(strBuilder.Length - 2, 2);

                                cmd.CommandText = strBuilder.ToString();
                                try
                                {
                                    int insertedRow = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "/n" + ex.Message;
                                }
                            }
                            #endregion

                            #region Comment
                            List<Comment> comments = markup.Comment;
                            if (comments.Count > 0)
                            {
                                strBuilder = new StringBuilder();
                                strBuilder.Append("INSERT INTO Comment (Guid, VerbalStatus, Status, Date, Author, Comment, ModifiedDate, ModifiedAuthor, Topic_Guid) VALUES ");

                                for (int i = 0; i < comments.Count; i++)
                                {
                                    Comment comment = comments[i];
                                    string dateParam = "@date" + i;
                                    string modifiedDateParam = "@modifiedDate" + i;
                                    strBuilder.Append("('"+comment.Guid+"', '"+comment.VerbalStatus+"', '"+comment.Status+"', "+dateParam+", '"+comment.Author+"', '"+comment.Comment1+"', "+modifiedDateParam+", '"+comment.ModifiedAuthor+"', '"+topic.Guid+"'), ");
                                    cmd.Parameters.Add(dateParam, DbType.DateTime);
                                    cmd.Parameters[dateParam].Value = comment.Date;
                                    cmd.Parameters.Add(modifiedDateParam, DbType.DateTime);
                                    if (comment.ModifiedDateSpecified) { cmd.Parameters[modifiedDateParam].Value = comment.ModifiedDate; }
                                }
                                strBuilder.Remove(strBuilder.Length - 2, 2);

                                cmd.CommandText = strBuilder.ToString();
                                try
                                {
                                    int insertedRow = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "/n" + ex.Message;
                                }
                                cmd.Parameters.Clear();
                                     
                            }
                            #endregion

                            #region Viewpoints
                            List<ViewPoint> viewpoints = markup.Viewpoints;
                            if (viewpoints.Count > 0)
                            {
                                strBuilder = new StringBuilder();
                                strBuilder.Append("INSERT INTO Viewpoints (Guid, Viewpoint, Snapshot, Snapshot_Image, Topic_Guid) VALUES ");

                                for (int i = 0; i < viewpoints.Count; i++)
                                {
                                    ViewPoint vp = viewpoints[i];
                                    string imageParam = "@image" + i;
                                    strBuilder.Append("('" + vp.Guid + "', '" + vp.Viewpoint + "', '" + vp.Snapshot + "', " + imageParam + ", '" + topic.Guid + "'), ");
                                    if (!string.IsNullOrEmpty(vp.Snapshot) && null != vp.SnapshotImage)
                                    {
                                        cmd.Parameters.Add(imageParam, DbType.Binary, vp.SnapshotImage.Length);
                                        cmd.Parameters[imageParam].Value = vp.SnapshotImage;
                                    }
                                    else
                                    {
                                        cmd.Parameters.Add(imageParam, DbType.Binary);
                                        cmd.Parameters[imageParam].Value = null;
                                    }

                                    strBuilder.Remove(strBuilder.Length - 2, 2);

                                    cmd.CommandText = strBuilder.ToString();
                                    try
                                    {
                                        int insertedRow = cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException ex)
                                    {
                                        string message = cmd.CommandText + "/n" + ex.Message;
                                    }
                                    cmd.Parameters.Clear();
                                }
                            }
                            #endregion

                            #region commentViewpoint
                            if (markup.Comment.Count > 0)
                            {
                                strBuilder = new StringBuilder();
                                strBuilder.Append("INSERT INTO Viewpoint (Guid, Comment_Guid) VALUES ");

                                foreach (Comment comment in markup.Comment)
                                {
                                    CommentViewpoint cv= comment.Viewpoint;
                                    if (null != cv)
                                    {
                                        strBuilder.Append("('" + cv.Guid + "', '" + comment.Guid + "'), ");
                                    }
                                }
                                strBuilder.Remove(strBuilder.Length - 2, 2);

                                cmd.CommandText = strBuilder.ToString();
                                try
                                {
                                    int insertedRow = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "/n" + ex.Message;
                                }
                            }
                            #endregion

                        }
                        trans.Commit();
                        inserted = true;
                    }
                    catch (SQLiteException ex)
                    {
                        trans.Rollback();
                        string message = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert Markup tables values.\n" + ex.Message, "Insert Markup Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertVisInfoValues(BCFComponent bcfComponent, SQLiteConnection connection)
        {
            bool inserted = false;
            try
            {
                using (SQLiteTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            foreach (string fileName in bcfComponent.Viewpoints.Keys)
                            {
                                var viewPointIds = from viewPoint in bcfComponent.MarkupInfo.Viewpoints where viewPoint.Viewpoint == fileName select viewPoint.Guid;
                                string viewpoint_guid = "";
                                if (viewPointIds.Count() > 0)
                                {
                                    viewpoint_guid = viewPointIds.First();
                                }

                                if (!string.IsNullOrEmpty(viewpoint_guid))
                                {
                                    VisualizationInfo visInfo = bcfComponent.Viewpoints[fileName];
                                    StringBuilder strBuilder = new StringBuilder();

                                    #region Bitmaps
                                    List<VisualizationInfoBitmaps> bitMaps = visInfo.Bitmaps;
                                    if (bitMaps.Count > 0)
                                    {
                                        StringBuilder pointBuilder = new StringBuilder();
                                        StringBuilder directionBuilder = new StringBuilder();

                                        strBuilder.Append("INSERT INTO Bitmaps (Guid, Bitmap, Bitmap_Image, Reference, Location, Normal, Up, Height, Viewpoints_Guid) VALUES ");
                                        pointBuilder.Append("INSERT INTO Point (Guid, X, Y, Z) VALUES ");
                                        directionBuilder.Append("INSERT INTO Direction (Guid, X, Y, Z) VALUES ");

                                        for (int i = 0; i < bitMaps.Count; i++)
                                        {
                                            VisualizationInfoBitmaps bitmap = bitMaps[i];
                                            string imageParam = "@image" + i;
                                            strBuilder.Append("('"+bitmap.Guid+"', '"+bitmap.Bitmap+"', "+imageParam+", '"+bitmap.Reference+"', '"+bitmap.Location.Guid+"', '"+bitmap.Normal.Guid+"', '"+bitmap.Up.Guid+"', "+bitmap.Height+", '"+viewpoint_guid+"'), ");
                                            pointBuilder.Append("('" + bitmap.Location.Guid + "', " + bitmap.Location.X + ", " + bitmap.Location.Y + ", " + bitmap.Location.Z + "), ");
                                            directionBuilder.Append("('" + bitmap.Normal.Guid + "', " + bitmap.Normal.X + ", " + bitmap.Normal.Y + ", " + bitmap.Normal.Z + "), ");
                                            directionBuilder.Append("('" + bitmap.Up.Guid + "', " + bitmap.Up.X + ", " + bitmap.Up.Y + ", " + bitmap.Up.Z + "), ");

                                            string bitmapName = Path.GetFileName(bitmap.Reference);
                                            if (!string.IsNullOrEmpty(bitmapName) && null != bitmap.BitmapImage)
                                            {
                                                cmd.Parameters.Add(imageParam, DbType.Binary, bitmap.BitmapImage.Length);
                                                cmd.Parameters[imageParam].Value = bitmap.BitmapImage;
                                            }
                                            else
                                            {
                                                cmd.Parameters.Add(imageParam, DbType.Binary);
                                                cmd.Parameters[imageParam].Value = null;
                                            }
                                        }

                                        strBuilder.Remove(strBuilder.Length - 2, 2);
                                        pointBuilder.Remove(pointBuilder.Length - 2, 2);
                                        directionBuilder.Remove(directionBuilder.Length - 2, 2);

                                        cmd.CommandText = pointBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.CommandText = directionBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.CommandText = strBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }
                                        cmd.Parameters.Clear();
                                    }

                                    #endregion

                                    #region Components
                                    List<Component> components = visInfo.Components;
                                    if (components.Count > 0)
                                    {
                                        strBuilder = new StringBuilder();
                                        strBuilder.Append("INSERT INTO Components (Guid, IfcGuid, Selected, Visible, Color, OriginatingSystem, AuthoringToolId, Viewpoints_Guid) VALUES ");

                                        for (int i = 0; i < components.Count; i++)
                                        {
                                            Component comp = components[i];
                                            string colorParam = "@color" + i;
                                            strBuilder.Append("('"+comp.Guid+"', '"+comp.IfcGuid+"', "+ Convert.ToInt32(comp.Selected)+", "+Convert.ToInt32(comp.Visible)+", "+colorParam+", '"+comp.OriginatingSystem+"', '"+comp.AuthoringToolId+"', '"+viewpoint_guid+"'), ");

                                            if (null != comp.Color)
                                            {
                                                cmd.Parameters.Add(colorParam, DbType.Binary, comp.Color.Length);
                                                cmd.Parameters[colorParam].Value = comp.Color;
                                            }
                                            else
                                            {
                                                cmd.Parameters.Add(colorParam, DbType.Binary);
                                                cmd.Parameters[colorParam].Value = null;
                                            }
                                        }
                                        strBuilder.Remove(strBuilder.Length - 2, 2);

                                        cmd.CommandText = strBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }
                                        cmd.Parameters.Clear();
                                    }
                                    #endregion

                                    #region ClippingPlane
                                    List<ClippingPlane> clippingPlanes = visInfo.ClippingPlanes;
                                    if (clippingPlanes.Count > 0)
                                    {
                                        strBuilder = new StringBuilder();
                                        StringBuilder pointBuilder = new StringBuilder();
                                        StringBuilder directionBuilder = new StringBuilder();

                                        strBuilder.Append("INSERT INTO ClippingPlane (Guid, Location, Direction, Viewpoints_Guid) VALUES ");
                                        pointBuilder.Append("INSERT INTO Point (Guid, X, Y, Z) VALUES ");
                                        directionBuilder.Append("INSERT INTO Direction (Guid, X, Y, Z) VALUES ");

                                        for (int i = 0; i < clippingPlanes.Count; i++)
                                        {
                                            ClippingPlane plane = clippingPlanes[i];
                                            strBuilder.Append("('" + plane.Guid + "', '" + plane.Location.Guid + "', '" + plane.Direction.Guid + "', '" + viewpoint_guid + "'), ");
                                            pointBuilder.Append("('"+plane.Location.Guid+"', "+ plane.Location.X+", "+plane.Location.Y+", "+plane.Location.Z+"), ");
                                            directionBuilder.Append("('" + plane.Direction.Guid + "', " + plane.Direction.X + ", " + plane.Direction.Y + ", " + plane.Direction.Z + "), ");
                                        }

                                        strBuilder.Remove(strBuilder.Length - 2, 2);
                                        pointBuilder.Remove(pointBuilder.Length - 2, 2);
                                        directionBuilder.Remove(directionBuilder.Length - 2, 2);

                                        cmd.CommandText = pointBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }
 
                                        cmd.CommandText = directionBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.CommandText = strBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                    }
                                    #endregion

                                    #region Lines
                                    List<Line> lines = visInfo.Lines;
                                    if (lines.Count > 0)
                                    {
                                        strBuilder = new StringBuilder();
                                        StringBuilder pointBuilder = new StringBuilder();

                                        strBuilder.Append("INSERT INTO Lines (Guid, StartPoint, EndPoint, Viewpoints_Guid) VALUES ");
                                        pointBuilder.Append("INSERT INTO Point (Guid, X, Y, Z) VALUES ");

                                        for (int i = 0; i < lines.Count; i++)
                                        {
                                            Line line = lines[i];
                                            strBuilder.Append("('" + line.Guid + "', '" + line.StartPoint.Guid + "', '" + line.EndPoint.Guid + "', '" + viewpoint_guid + "'), ");
                                            pointBuilder.Append("('" + line.StartPoint.Guid + "', " + line.StartPoint.X + ", " + line.StartPoint.Y + ", " + line.StartPoint.Z + "), ");
                                            pointBuilder.Append("('" + line.EndPoint.Guid + "', " + line.EndPoint.X + ", " + line.EndPoint.Y + ", " + line.EndPoint.Z + "), ");
                                        }

                                        strBuilder.Remove(strBuilder.Length - 2, 2);
                                        pointBuilder.Remove(pointBuilder.Length - 2, 2);

                                        cmd.CommandText = pointBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.CommandText = strBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }
                                    }
                                    #endregion

                                    #region OrthogonalCamera
                                    OrthogonalCamera orthoCamera = visInfo.OrthogonalCamera;
                                    if (orthoCamera.ViewToWorldScale != 0)
                                    {
                                        strBuilder = new StringBuilder();
                                        StringBuilder pointBuilder = new StringBuilder();
                                        StringBuilder directionBuilder = new StringBuilder();

                                        strBuilder.Append("INSERT INTO OrthogonalCamera (Guid, CameraViewPoint, CameraDirection, CameraUpVector, ViewToWorldScale, Viewpoints_Guid) VALUES ");
                                        strBuilder.Append("('" + orthoCamera.Guid + "', '" + orthoCamera.CameraViewPoint.Guid + "', '" + orthoCamera.CameraDirection.Guid + "', '" + orthoCamera.CameraUpVector.Guid + "', " + orthoCamera.ViewToWorldScale + ", '" + viewpoint_guid + "')");

                                        pointBuilder.Append("INSERT INTO Point (Guid, X, Y, Z) VALUES ");
                                        pointBuilder.Append("('" + orthoCamera.CameraViewPoint.Guid + "', " + orthoCamera.CameraViewPoint.X + ", " + orthoCamera.CameraViewPoint.Y + ", " + orthoCamera.CameraViewPoint.Z + ")");

                                        directionBuilder.Append("INSERT INTO Direction (Guid, X, Y, Z) VALUES ");
                                        directionBuilder.Append("('" + orthoCamera.CameraDirection.Guid + "', " + orthoCamera.CameraDirection.X + ", " + orthoCamera.CameraDirection.Y + ", " + orthoCamera.CameraDirection.Z + "), ");
                                        directionBuilder.Append("('" + orthoCamera.CameraUpVector.Guid + "', " + orthoCamera.CameraUpVector.X + ", " + orthoCamera.CameraUpVector.Y + ", " + orthoCamera.CameraUpVector.Z + ")");

                                        cmd.CommandText = pointBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.CommandText = directionBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.CommandText = strBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }
                                    }
                                    #endregion

                                    #region PerspectiveCamera
                                    PerspectiveCamera persCamera = visInfo.PerspectiveCamera;
                                    if (persCamera.FieldOfView != 0)
                                    {
                                        strBuilder = new StringBuilder();
                                        StringBuilder pointBuilder = new StringBuilder();
                                        StringBuilder directionBuilder = new StringBuilder();

                                        strBuilder.Append("INSERT INTO PerspectiveCamera (Guid, CameraViewPoint, CameraDirection, CameraUpVector, FieldOfView, Viewpoints_Guid) VALUES ");
                                        strBuilder.Append("('" + persCamera.Guid + "', '" + persCamera.CameraViewPoint.Guid + "', '" + persCamera.CameraDirection.Guid + "', '" + persCamera.CameraUpVector.Guid + "', " + persCamera.FieldOfView + ", '" + viewpoint_guid + "')");

                                        pointBuilder.Append("INSERT INTO Point (Guid, X, Y, Z) VALUES ");
                                        pointBuilder.Append("('" + persCamera.CameraViewPoint.Guid + "', " + persCamera.CameraViewPoint.X + ", " + persCamera.CameraViewPoint.Y + ", " + persCamera.CameraViewPoint.Z + ")");

                                        directionBuilder.Append("INSERT INTO Direction (Guid, X, Y, Z) VALUES ");
                                        directionBuilder.Append("('" + persCamera.CameraDirection.Guid + "', " + persCamera.CameraDirection.X + ", " + persCamera.CameraDirection.Y + ", " + persCamera.CameraDirection.Z + "), ");
                                        directionBuilder.Append("('" + persCamera.CameraUpVector.Guid + "', " + persCamera.CameraUpVector.X + ", " + persCamera.CameraUpVector.Y + ", " + persCamera.CameraUpVector.Z + ")");

                                        cmd.CommandText = pointBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.CommandText = directionBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.CommandText = strBuilder.ToString();
                                        try
                                        {
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        trans.Commit();
                        inserted = true;
                    }
                    catch (SQLiteException ex)
                    {
                        trans.Rollback();
                        string message = ex.Message;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert Visualization Information tables values.\n" + ex.Message, "Insert Visualization Information Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertCustomValues(BCFZIP bcfzip, SQLiteConnection connection)
        {
            bool inserted = false;
            try
            {
                using (SQLiteTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            StringBuilder strBuilder = new StringBuilder();
                            strBuilder.Append("INSERT INTO BCFFileInfo (Guid, FileName, FilePath, UploadedBy, UploadedDate, CreationDate, Project_Guid, Version_Guid) VALUES ");
                            strBuilder.Append("('" + bcfzip.FileId + "', '" + bcfzip.ZipFileName + "', '" + bcfzip.ZipFilePath + "', '" + bcfzip.UploadedBy + "', @uploadedDate, @creationDate, '"+bcfzip.ProjectFile.Guid+"', '"+bcfzip.VersionFile.Guid+"')");

                            cmd.Parameters.Add("@uploadedDate", DbType.DateTime);
                            cmd.Parameters["@uploadedDate"].Value = bcfzip.UploadedDate;
                            cmd.Parameters.Add("@creationDate", DbType.DateTime);
                            cmd.Parameters["@creationDate"].Value = bcfzip.CreationDate;

                            cmd.CommandText = strBuilder.ToString();
                            try
                            {
                                int insertedRows = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }

                            strBuilder = new StringBuilder();
                            strBuilder.Append("INSERT INTO FileTopics (Topic_Guid, File_Guid) VALUES ");
                            foreach (string guid in bcfzip.BCFComponents.Keys)
                            {
                                strBuilder.Append("('" + guid + "', '" + bcfzip.FileId + "'), ");
                            }
                            strBuilder.Remove(strBuilder.Length - 2, 2);

                            cmd.CommandText = strBuilder.ToString();
                            try
                            {
                                int insertedRows = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }
                        }
                        trans.Commit();
                        inserted = true;
                    }
                    catch (SQLiteException ex)
                    {
                        trans.Rollback();
                        string message = ex.Message;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert Visualization Information tables values.\n" + ex.Message, "Insert Visualization Information Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }
    }
}
