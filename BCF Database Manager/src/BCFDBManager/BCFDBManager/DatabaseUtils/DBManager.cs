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
    public enum ConflictMode
    {
        ROLLBACK, ABORT, FAIL, IGNORE, REPLACE
    }

    public static class DBManager
    {
        public static ProgressBar progressBar = null;
        public static TextBlock statusLabel = null;

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateStatusLabelDelegate(System.Windows.DependencyProperty dp, Object value);

        public static Dictionary<string/*fileId*/, BCFZIP> ReadDatabase(string dbFile, bool fullRead)
        {
            Dictionary<string/*fileId*/, BCFZIP> bcfFiles = new Dictionary<string/*fileId*/, BCFZIP>();
            try
            {
                UpdateStatusLabelDelegate updateLabelDelegate = new UpdateStatusLabelDelegate(statusLabel.SetValue);
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Reading BCF database..." });

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

                                if (fullRead)
                                {
                                    string projectGuid = reader.GetString(reader.GetOrdinal("Project_Guid"));
                                    ProjectExtension projectExt = new ProjectExtension();
                                    projectExt.Guid = projectGuid;
                                    bcfZip.ProjectFile = projectExt;

                                    string versionGuid = reader.GetString(reader.GetOrdinal("Version_Guid"));
                                    Version version = new Version();
                                    version.Guid = versionGuid;
                                    bcfZip.VersionFile = version;
                                }

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
                            if(fullRead)
                            {
                                bcfZip.ProjectFile = ReadProjectInfo(connection, bcfZip.ProjectFile.Guid);
                                bcfZip.VersionFile = ReadVersionInfo(connection, bcfZip.VersionFile.Guid);
                            }
                            
                            List<string> topicIds = ReadTopicIds(connection, fileId);
                            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                            progressBar.Value = 0;
                            progressBar.Maximum = topicIds.Count;

                            double value = 0;
                            Dictionary<string, BCFComponent> components = new Dictionary<string, BCFComponent>();
                            foreach (string topicId in topicIds)
                            {
                                value++;
                                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                                BCFComponent component = ReadBCFComponent(connection, topicId, fullRead);
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

        private static BCFComponent ReadBCFComponent(SQLiteConnection connection, string topicGuid, bool fullRead)
        {
            BCFComponent component = new BCFComponent();
            try
            {
                //read markup information only
                component.MarkupInfo = ReadMarkupInfo(connection, topicGuid, fullRead);

                if(fullRead)
                {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read BCF component.\n" + ex.Message, "Read BCF Component", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return component;
        }

        private static Markup ReadMarkupInfo(SQLiteConnection connection, string topicGuid , bool fullRead)
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
                            topic.Guid = reader.GetString(reader.GetOrdinal("Guid")); ;
                            topic.TopicType = reader.GetString(reader.GetOrdinal("TopicType"));
                            topic.TopicStatus = reader.GetString(reader.GetOrdinal("TopicStatus"));
                            topic.Title = reader.GetString(reader.GetOrdinal("Title"));
                            topic.ReferenceLink = reader.GetString(reader.GetOrdinal("ReferenceLink"));
                            topic.Description = reader.GetString(reader.GetOrdinal("Description"));
                            topic.Priority = reader.GetString(reader.GetOrdinal("Priority"));
                            topic.Index = reader.GetString(reader.GetOrdinal("TopicIndex"));
                            
                            if (reader["CreationDate"] != System.DBNull.Value)
                            {
                                topic.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
                            }
                            topic.CreationAuthor = reader.GetString(reader.GetOrdinal("CreationAuthor"));

                            if (reader["ModifiedDate"] != System.DBNull.Value)
                            {
                                topic.ModifiedDateSpecified = true;
                                topic.ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"));
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

                    if(fullRead)
                    {
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
                                    headerFile.Date = reader.GetDateTime(reader.GetOrdinal("Date"));
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
                        #endregion
                    }
                    
                    markup.Topic = topic;

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
                            if (reader["Date"] != System.DBNull.Value)
                            {
                                comment.Date = reader.GetDateTime(reader.GetOrdinal("Date"));
                            }

                            comment.Author = reader.GetString(reader.GetOrdinal("Author"));
                            comment.Comment1 = reader.GetString(reader.GetOrdinal("Comment"));
                            
                            if (reader["ModifiedDate"] != System.DBNull.Value)
                            {
                                comment.ModifiedDateSpecified = true;
                                comment.ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"));
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
                            viewPoint.Guid =reader.GetString(reader.GetOrdinal("CameraViewPoint"));
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

                                UpdateStatusLabelDelegate updateLabelDelegate = new UpdateStatusLabelDelegate(statusLabel.SetValue);
                                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Creating database tables..." });

                                UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                                progressBar.Value = 0;
                                progressBar.Maximum = tableInfo.Count;

                                var tableOrders = ((TableNames[])Enum.GetValues(typeof(TableNames))).OrderBy(x => x);

                                double value = 0;
                                foreach (TableNames tn in tableOrders)
                                {
                                    value++;
                                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

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

        public static bool WriteDatabase(string dbFile, BCFZIP bcfzip, ConflictMode mode)
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
                        bool projectValuesInserted = InsertProjectValues(bcfzip, connection, mode);
                        bool versionValuesInserted = InsertVersionValues(bcfzip, connection, mode);
                        bool componentValuesInserted = InsertBCFComponentValues(bcfzip, connection, mode);
                        bool customValuesInserted = InsertCustomValues(bcfzip, connection, mode);
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

        private static bool InsertProjectValues(BCFZIP bcfZip, SQLiteConnection connection, ConflictMode mode)
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
                            strBuilder.Append("INSERT OR "+mode.ToString()+" INTO ProjectExtension (Guid, ExtensionSchema) VALUES ");
                            strBuilder.Append("(@guid, @extSchema)");
                            cmd.Parameters.AddWithValue("@guid", pExt.Guid);
                            cmd.Parameters.AddWithValue("@extSchema", pExt.ExtensionSchema);
                            cmd.CommandText = strBuilder.ToString();
                            int insertedRow = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();

                            strBuilder = new StringBuilder();
                            strBuilder.Append("INSERT OR "+mode.ToString()+" INTO Project (Guid, ProjectId, ProjectName, ProjectExtension_Guid) VALUES ");
                            strBuilder.Append("(@guid, @projectId, @projectName, @projectExtGuid)");
                            cmd.Parameters.AddWithValue("@guid", pExt.Project.Guid);
                            cmd.Parameters.AddWithValue("@projectId", pExt.Project.ProjectId);
                            cmd.Parameters.AddWithValue("@projectName", pExt.Project.Name);
                            cmd.Parameters.AddWithValue("@projectExtGuid", pExt.Guid);
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

        private static bool InsertVersionValues(BCFZIP bcfZip, SQLiteConnection connection, ConflictMode mode)
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
                                strBuilder.Append("INSERT OR "+mode.ToString()+" INTO Version (Guid, VersionId, DetailedVersion) VALUES ");
                                strBuilder.Append("(@guid, @versionId, @detailedVersion)");
                                cmd.Parameters.AddWithValue("@guid", version.Guid);
                                cmd.Parameters.AddWithValue("@versionId", version.VersionId);
                                cmd.Parameters.AddWithValue("@detailedVersion", version.DetailedVersion);
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

        private static bool InsertBCFComponentValues(BCFZIP bcfzip, SQLiteConnection connection, ConflictMode mode)
        {
            bool inserted = false;
            try
            {
                UpdateStatusLabelDelegate updateLabelDelegate = new UpdateStatusLabelDelegate(statusLabel.SetValue);
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, "Writing BCF Information into database tables..." });

                UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                progressBar.Value = 0;
                progressBar.Maximum = bcfzip.BCFComponents.Count;

                double value = 0;
                foreach (string guid in bcfzip.BCFComponents.Keys)
                {
                    value++;
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                    BCFComponent bcfComponent = bcfzip.BCFComponents[guid];
                    bool insertedMarkup = InsertMarkupValues(bcfComponent, connection, mode);
                    bool insertedVisInfo = InsertVisInfoValues(bcfComponent, connection, mode);
                }
                inserted = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert BCF Component values./n" + ex.Message, "Insert BCF Components Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertMarkupValues(BCFComponent bcfComponent, SQLiteConnection connection, ConflictMode mode)
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
                            string topicSql = "INSERT OR "+mode.ToString()+" INTO Topic (Guid, TopicType, TopicStatus, Title, ReferenceLink, Description, Priority, TopicIndex, CreationDate, CreationAuthor, ModifiedDate, ModifiedAuthor, AssignedTo) VALUES ";
                            topicSql += "(@guid, @topicType, @topicStatus, @title, @referenceLink, @description, @priority, @topicIndex, @creationDate, @creationAuthor, @modifiedDate,  @modifiedAuthor, @assignedTo )";

                            cmd.Parameters.AddWithValue("@guid", topic.Guid);
                            cmd.Parameters.AddWithValue("@topicType", topic.TopicType);
                            cmd.Parameters.AddWithValue("@topicStatus", topic.TopicStatus);
                            cmd.Parameters.AddWithValue("@title", topic.Title);
                            cmd.Parameters.AddWithValue("@referenceLink", topic.ReferenceLink);
                            cmd.Parameters.AddWithValue("@description", topic.Description);
                            cmd.Parameters.AddWithValue("@priority", topic.Priority);
                            cmd.Parameters.AddWithValue("@topicIndex", topic.Index);
                            cmd.Parameters.AddWithValue("@creationDate", topic.CreationDate);
                            cmd.Parameters.AddWithValue("@creationAuthor", topic.CreationAuthor);
                            cmd.Parameters.AddWithValue("@modifiedDate", topic.ModifiedDate);
                            cmd.Parameters.AddWithValue("@modifiedAuthor", topic.ModifiedAuthor);
                            cmd.Parameters.AddWithValue("@assignedTo", topic.AssignedTo);

                            cmd.CommandText = topicSql;
                            try
                            {
                                int insertedRow = cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "/n" + ex.Message;
                            }
                            #endregion 

                            #region
                            if (topic.Labels.Count > 0)
                            {
                                cmd.Parameters.Add("@label", DbType.String);
                                cmd.Parameters.Add("@topicGuid", DbType.String);

                                foreach (string label in topic.Labels)
                                {
                                    string labelSql = "INSERT OR " + mode.ToString() + " INTO Labels (Label, Topic_Guid) VALUES ";
                                    labelSql += "(@label, @topicGuid)";
                                    cmd.Parameters["@label"].Value = label;
                                    cmd.Parameters["@topicGuid"].Value = topic.Guid;
                                    try
                                    {
                                        cmd.CommandText = labelSql;
                                        int insertedRows = cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException ex)
                                    {
                                        string message = cmd.CommandText + "/n" + ex.Message;
                                    }
                                }
                                cmd.Parameters.Clear();
                            }
                            #endregion

                            #region HeaderFile
                            List<HeaderFile> headerFiles = markup.Header;
                            if (headerFiles.Count > 0)
                            {
                                cmd.Parameters.Add("@guid", DbType.String);
                                cmd.Parameters.Add("@ifcProject", DbType.String);
                                cmd.Parameters.Add("@ifcSpatialStructureElement", DbType.String);
                                cmd.Parameters.Add("@isExternal", DbType.Boolean);
                                cmd.Parameters.Add("@fileName", DbType.String);
                                cmd.Parameters.Add("@date", DbType.DateTime);
                                cmd.Parameters.Add("@reference", DbType.String);
                                cmd.Parameters.Add("@topicGuid", DbType.String);


                                foreach (HeaderFile file in headerFiles)
                                {
                                    string headerSql = "INSERT OR " + mode.ToString() + " INTO HeaderFile (Guid, IfcProject, IfcSpatialStructureElement, isExternal, FileName, Date, Reference, Topic_Guid) VALUES ";
                                    headerSql += "(@guid, @ifcProject, @ifcSpatialStructureElement, @isExternal, @fileName, @date, @reference, @topicGuid)";

                                    cmd.Parameters["@guid"].Value = file.Guid;
                                    cmd.Parameters["@ifcProject"].Value = file.IfcProject;
                                    cmd.Parameters["@ifcSpatialStructureElement"].Value = file.IfcSpatialStructureElement;
                                    cmd.Parameters["@isExternal"].Value = file.isExternal;
                                    cmd.Parameters["@fileName"].Value = file.Filename;
                                    cmd.Parameters["@date"].Value = file.Date;
                                    cmd.Parameters["@reference"].Value = file.Reference;
                                    cmd.Parameters["@topicGuid"].Value = topic.Guid;

                                    try
                                    {
                                        cmd.CommandText = headerSql;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException ex)
                                    {
                                        string message = cmd.CommandText + "/n" + ex.Message;
                                    }
                                }
                               
                                cmd.Parameters.Clear();
                            }
                            #endregion

                            #region BimSnippet
                            BimSnippet bimSnippet = topic.BimSnippet;
                            if (!string.IsNullOrEmpty(bimSnippet.Reference))
                            {
                                string bimSnippetSql = "INSERT OR " + mode.ToString() + " INTO BimSnippet (Guid, SnippetType, isExternal, Reference, ReferenceSchema, FileContent, Topic_Guid) VALUES ";
                                bimSnippetSql += "(@guid, @snippetType, @isExternal, @reference, @referenceSchema, @fileContent, @topicGuid)";

                                cmd.Parameters.AddWithValue("@guid", bimSnippet.Guid);
                                cmd.Parameters.AddWithValue("@snippetType", bimSnippet.SnippetType);
                                cmd.Parameters.AddWithValue("@isExternal", bimSnippet.isExternal);
                                cmd.Parameters.AddWithValue("@reference", bimSnippet.Reference);
                                cmd.Parameters.AddWithValue("@referenceSchema", bimSnippet.ReferenceSchema);
                                cmd.Parameters.AddWithValue("@fileContent", bimSnippet.FileContent);
                                cmd.Parameters.AddWithValue("@topicGuid", topic.Guid);

                                try
                                {
                                    cmd.CommandText = bimSnippetSql;
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
                                cmd.Parameters.Add("@guid", DbType.String);
                                cmd.Parameters.Add("@isExternal", DbType.Boolean);
                                cmd.Parameters.Add("@referenceDoc", DbType.String);
                                cmd.Parameters.Add("@fileContent", DbType.Binary);
                                cmd.Parameters.Add("@description", DbType.String);
                                cmd.Parameters.AddWithValue("@topicGuid", topic.Guid);

                                foreach (TopicDocumentReferences docRef in docReferences)
                                {
                                    string docRefSql = "INSERT OR " + mode.ToString() + " INTO DocumentReferences (Guid, isExternal, ReferenceDocument, FileContent, Description, Topic_Guid) VALUES ";
                                    docRefSql += "(@guid, @isExternal, @referenceDoc, @fileContent, @description, @topicGuid)";

                                    cmd.Parameters["@guid"].Value = docRef.Guid;
                                    cmd.Parameters["@isExternal"].Value = docRef.isExternal;
                                    cmd.Parameters["@referenceDoc"].Value = docRef.ReferencedDocument;
                                    cmd.Parameters["@fileContent"].Value = docRef.FileContent;
                                    cmd.Parameters["@description"].Value = docRef.Description;

                                    try
                                    {
                                        cmd.CommandText = docRefSql;
                                        int insertedRows = cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException ex)
                                    {
                                        string message = cmd.CommandText + "/n" + ex.Message;
                                    }
                                }

                                cmd.Parameters.Clear();
                            }
                            #endregion

                            #region RelatedTopics
                            List<TopicRelatedTopics> relatedTopics = topic.RelatedTopics;
                            if (relatedTopics.Count > 0)
                            {
                                cmd.Parameters.Add("@guid", DbType.String);
                                cmd.Parameters.AddWithValue("@topicGuid", topic.Guid);

                                foreach (TopicRelatedTopics relTopic in relatedTopics)
                                {
                                    string relTopicSql = "INSERT OR " + mode.ToString() + " INTO RelatedTopics (Guid, Topic_Guid) VALUES ";
                                    relTopicSql += "(@guid, @topicGuid)";

                                    cmd.Parameters["@guid"].Value = relTopic.Guid;
                                    cmd.Parameters["@topicGuid"].Value = topic.Guid;

                                    try
                                    {
                                        cmd.CommandText = relTopicSql;
                                        int insertedRows = cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException ex)
                                    {
                                        string message = cmd.CommandText + "/n" + ex.Message;
                                    }
                                }
                                cmd.Parameters.Clear();
                            }
                            #endregion

                            #region Comment
                            List<Comment> comments = markup.Comment;
                            if (comments.Count > 0)
                            {
                                cmd.Parameters.Add("@guid", DbType.String);
                                cmd.Parameters.Add("@verbalStatus", DbType.String);
                                cmd.Parameters.Add("@status", DbType.String);
                                cmd.Parameters.Add("@date", DbType.DateTime);
                                cmd.Parameters.Add("@author", DbType.String);
                                cmd.Parameters.Add("@comment", DbType.String);
                                cmd.Parameters.Add("@modifiedDate", DbType.DateTime);
                                cmd.Parameters.Add("@modifiedAuthor", DbType.String);
                                cmd.Parameters.AddWithValue("@topicGuid", topic.Guid);
                                cmd.Parameters.Add("@viewpointGuid", DbType.String);
                                
                                foreach (Comment comment in comments)
                                {
                                    string commentSql = "INSERT OR " + mode.ToString() + " INTO Comment (Guid, VerbalStatus, Status, Date, Author, Comment, ModifiedDate, ModifiedAuthor, Topic_Guid) VALUES ";
                                    commentSql += "(@guid, @verbalStatus, @status, @date, @author, @comment, @modifiedDate, @modifiedAuthor, @topicGuid)";

                                    cmd.Parameters["@guid"].Value = comment.Guid;
                                    cmd.Parameters["@verbalStatus"].Value = comment.VerbalStatus;
                                    cmd.Parameters["@status"].Value = comment.Status;
                                    cmd.Parameters["@date"].Value = comment.Date;
                                    cmd.Parameters["@author"].Value = comment.Author;
                                    cmd.Parameters["@comment"].Value = comment.Comment1;
                                    cmd.Parameters["@modifiedDate"].Value = comment.ModifiedDate;
                                    cmd.Parameters["@modifiedAuthor"].Value = comment.ModifiedAuthor;

                                    try
                                    {
                                        cmd.CommandText = commentSql;
                                        int insertedRows = cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException ex)
                                    {
                                        string message = cmd.CommandText + "/n" + ex.Message;
                                    }

                                    CommentViewpoint cv = comment.Viewpoint;
                                    if (!string.IsNullOrEmpty(cv.Guid))
                                    {
                                        string viewpointSql = "INSERT OR " + mode.ToString() + " INTO Viewpoint (Guid, Comment_Guid) VALUES ";
                                        viewpointSql += "(@viewpointGuid, @guid)";

                                        cmd.Parameters["@viewpointGuid"].Value = cv.Guid;

                                        try
                                        {
                                            cmd.CommandText = viewpointSql;
                                            int insertedRow = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "/n" + ex.Message;
                                        }
                                    }
                                }
                                cmd.Parameters.Clear();
                            }
                            #endregion

                            #region Viewpoints
                            List<ViewPoint> viewpoints = markup.Viewpoints;
                            if (viewpoints.Count > 0)
                            {
                                cmd.Parameters.Add("@guid", DbType.String);
                                cmd.Parameters.Add("@viewpoint", DbType.String);
                                cmd.Parameters.Add("@snapshot", DbType.String);
                                cmd.Parameters.Add("@snapshotImage", DbType.Binary);
                                cmd.Parameters.AddWithValue("@topicGuid", topic.Guid);

                                foreach (ViewPoint viewPoint in viewpoints)
                                {
                                    string viewpointSql = "INSERT OR " + mode.ToString() + " INTO Viewpoints (Guid, Viewpoint, Snapshot, Snapshot_Image, Topic_Guid) VALUES ";
                                    viewpointSql += "(@guid, @viewpoint, @snapshot, @snapshotImage, @topicGuid)";

                                    cmd.Parameters["@guid"].Value = viewPoint.Guid;
                                    cmd.Parameters["@viewpoint"].Value = viewPoint.Viewpoint;
                                    cmd.Parameters["@snapshot"].Value = viewPoint.Snapshot;
                                    cmd.Parameters["@snapshotImage"].Value = viewPoint.SnapshotImage;

                                    try
                                    {
                                        cmd.CommandText = viewpointSql;
                                        int insertedRows = cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException ex)
                                    {
                                        string message = cmd.CommandText + "/n" + ex.Message;
                                    }
                                }
                                cmd.Parameters.Clear();
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

        private static bool InsertVisInfoValues(BCFComponent bcfComponent, SQLiteConnection connection, ConflictMode mode)
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
                                   
                                    #region Bitmaps
                                    List<VisualizationInfoBitmaps> bitMaps = visInfo.Bitmaps;
                                    if (bitMaps.Count > 0)
                                    {
                                        cmd.Parameters.Add("@guid", DbType.String);
                                        cmd.Parameters.Add("@bitmap", DbType.String);
                                        cmd.Parameters.Add("@bitmapImage", DbType.Binary);
                                        cmd.Parameters.Add("@reference", DbType.String);
                                        cmd.Parameters.Add("@location", DbType.String);
                                        cmd.Parameters.Add("@normal", DbType.String);
                                        cmd.Parameters.Add("@up", DbType.String);
                                        cmd.Parameters.Add("@height", DbType.Double);
                                        cmd.Parameters.AddWithValue("@viewpointGuid", viewpoint_guid);

                                        cmd.Parameters.Add("@pointGuid", DbType.String);
                                        cmd.Parameters.Add("@pointX", DbType.Double);
                                        cmd.Parameters.Add("@pointY", DbType.Double);
                                        cmd.Parameters.Add("@pointZ", DbType.Double);

                                        cmd.Parameters.Add("@directionGuid", DbType.String);
                                        cmd.Parameters.Add("@directionX", DbType.Double);
                                        cmd.Parameters.Add("@directionY", DbType.Double);
                                        cmd.Parameters.Add("@directionZ", DbType.Double);

                                        foreach (VisualizationInfoBitmaps bitmap in bitMaps)
                                        {
                                            string bitmapSql = "INSERT OR " + mode.ToString() + " INTO Bitmaps (Guid, Bitmap, Bitmap_Image, Reference, Location, Normal, Up, Height, Viewpoints_Guid) VALUES ";
                                            bitmapSql += "(@guid, @bitmap, @bitmapImage, @reference, @location, @normal, @up, @height, @viewpointGuid)";

                                            cmd.Parameters["@guid"].Value = bitmap.Guid;
                                            cmd.Parameters["@bitmap"].Value = bitmap.Bitmap.ToString();
                                            cmd.Parameters["@bitmapImage"].Value = bitmap.BitmapImage;
                                            cmd.Parameters["@reference"].Value = bitmap.Reference;
                                            cmd.Parameters["@location"].Value = bitmap.Location.Guid;
                                            cmd.Parameters["@normal"].Value = bitmap.Normal.Guid;
                                            cmd.Parameters["@up"].Value = bitmap.Up.Guid;
                                            cmd.Parameters["@height"].Value = bitmap.Height;

                                            try
                                            {
                                                cmd.CommandText = bitmapSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                            string pointSql = "INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z) VALUES ";
                                            pointSql += "(@pointGuid, @pointX, @pointY, @pointZ)";
                                            cmd.Parameters["@pointGuid"].Value = bitmap.Location.Guid;
                                            cmd.Parameters["@pointX"].Value = bitmap.Location.X;
                                            cmd.Parameters["@pointY"].Value = bitmap.Location.Y;
                                            cmd.Parameters["@pointZ"].Value = bitmap.Location.Z;

                                            try
                                            {
                                                cmd.CommandText = pointSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                            string directionSql = "INSERT OR " + mode.ToString() + " INTO Direction (Guid, X, Y, Z) VALUES ";
                                            directionSql += "(@directionGuid, @directionX, @directionY, @directionZ)";
                                            cmd.Parameters["@directionGuid"].Value = bitmap.Normal.Guid;
                                            cmd.Parameters["@directionX"].Value = bitmap.Normal.X;
                                            cmd.Parameters["@directionY"].Value = bitmap.Normal.Y;
                                            cmd.Parameters["@directionZ"].Value = bitmap.Normal.Z;

                                            try
                                            {
                                                cmd.CommandText = directionSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                            cmd.Parameters["@directionGuid"].Value = bitmap.Up.Guid;
                                            cmd.Parameters["@directionX"].Value = bitmap.Up.X;
                                            cmd.Parameters["@directionY"].Value = bitmap.Up.Y;
                                            cmd.Parameters["@directionZ"].Value = bitmap.Up.Z;

                                            try
                                            {
                                                cmd.CommandText = directionSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                        }
                                        cmd.Parameters.Clear();
                                    }

                                    #endregion

                                    #region Components
                                    List<Component> components = visInfo.Components;
                                    if (components.Count > 0)
                                    {
                                        cmd.Parameters.Add("@guid", DbType.String);
                                        cmd.Parameters.Add("@ifcGuid", DbType.String);
                                        cmd.Parameters.Add("@selected", DbType.Boolean);
                                        cmd.Parameters.Add("@visible", DbType.Boolean);
                                        cmd.Parameters.Add("@color", DbType.Binary);
                                        cmd.Parameters.Add("@originatingSystem", DbType.String);
                                        cmd.Parameters.Add("@authoringToolId", DbType.String);
                                        cmd.Parameters.AddWithValue("@viewpointGuid", viewpoint_guid);


                                        foreach(Component comp in components)
                                        {
                                            string compSql = "INSERT OR " + mode.ToString() + " INTO Components (Guid, IfcGuid, Selected, Visible, Color, OriginatingSystem, AuthoringToolId, Viewpoints_Guid) VALUES ";
                                            compSql += "(@guid, @ifcGuid, @selected, @visible, @color, @originatingSystem, @authoringToolId, @viewpointGuid)";

                                            cmd.Parameters["@guid"].Value = comp.Guid;
                                            cmd.Parameters["@ifcGuid"].Value = comp.IfcGuid;
                                            cmd.Parameters["@selected"].Value = comp.Selected;
                                            cmd.Parameters["@visible"].Value = comp.Visible;
                                            cmd.Parameters["@color"].Value = comp.Color;
                                            cmd.Parameters["@originatingSystem"].Value = comp.OriginatingSystem;
                                            cmd.Parameters["@authoringToolId"].Value = comp.AuthoringToolId;

                                            try
                                            {
                                                cmd.CommandText = compSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }
                                        }
                                        cmd.Parameters.Clear();
                                    }
                                    #endregion

                                    #region ClippingPlane
                                    List<ClippingPlane> clippingPlanes = visInfo.ClippingPlanes;
                                    if (clippingPlanes.Count > 0)
                                    {
                                        cmd.Parameters.Add("@guid", DbType.String);
                                        cmd.Parameters.Add("@location", DbType.String);
                                        cmd.Parameters.Add("@direction", DbType.String);
                                        cmd.Parameters.AddWithValue("@viewpointGuid", viewpoint_guid);

                                        cmd.Parameters.Add("@locationGuid", DbType.String);
                                        cmd.Parameters.Add("@locationX", DbType.Double);
                                        cmd.Parameters.Add("@locationY", DbType.Double);
                                        cmd.Parameters.Add("@locationZ", DbType.Double);

                                        cmd.Parameters.Add("@directionGuid", DbType.String);
                                        cmd.Parameters.Add("@directionX", DbType.Double);
                                        cmd.Parameters.Add("@directionY", DbType.Double);
                                        cmd.Parameters.Add("@directionZ", DbType.Double);

                                        foreach (ClippingPlane plane in clippingPlanes)
                                        {
                                            string planeSql = "INSERT OR " + mode.ToString() + " INTO ClippingPlane (Guid, Location, Direction, Viewpoints_Guid) VALUES ";
                                            planeSql += "(@guid, @location, @direction, viewpointGuid)";

                                            cmd.Parameters["@guid"].Value = plane.Guid;
                                            cmd.Parameters["@location"].Value = plane.Location.Guid;
                                            cmd.Parameters["@direction"].Value = plane.Direction.Guid;

                                            try
                                            {
                                                cmd.CommandText = planeSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                            string locationSql = "INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z) VALUES ";
                                            locationSql += "(@locationGuid, @locationX, @locationY, @locationZ)";
                                            cmd.Parameters["@locationGuid"].Value = plane.Location.Guid;
                                            cmd.Parameters["@locationX"].Value = plane.Location.X;
                                            cmd.Parameters["@locationY"].Value = plane.Location.Y;
                                            cmd.Parameters["@locationZ"].Value = plane.Location.Z;

                                            try
                                            {
                                                cmd.CommandText = locationSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                            string directionSql = "INSERT OR " + mode.ToString() + " INTO Direction (Guid, X, Y, Z) VALUES ";
                                            directionSql += "(@directionGuid, @directionX, @directionY, @directionZ)";
                                            cmd.Parameters["@directionGuid"].Value = plane.Direction.Guid;
                                            cmd.Parameters["@directionX"].Value = plane.Direction.X;
                                            cmd.Parameters["@directionY"].Value = plane.Direction.Y;
                                            cmd.Parameters["@directionZ"].Value = plane.Direction.Z;

                                            try
                                            {
                                                cmd.CommandText = directionSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                        }
                                        cmd.Parameters.Clear();
                                    }
                                    #endregion

                                    #region Lines
                                    List<Line> lines = visInfo.Lines;
                                    if (lines.Count > 0)
                                    {
                                        cmd.Parameters.Add("@guid", DbType.String);
                                        cmd.Parameters.Add("@startPoint", DbType.String);
                                        cmd.Parameters.Add("@endPoint", DbType.String);
                                        cmd.Parameters.AddWithValue("@viewpointGuid", viewpoint_guid);

                                        cmd.Parameters.Add("@pointGuid", DbType.String);
                                        cmd.Parameters.Add("@pointX", DbType.Double);
                                        cmd.Parameters.Add("@pointY", DbType.Double);
                                        cmd.Parameters.Add("@pointZ", DbType.Double);

                                        foreach (Line line in lines)
                                        {
                                            string lineSql = "INSERT OR " + mode.ToString() + " INTO Lines (Guid, StartPoint, EndPoint, Viewpoints_Guid) VALUES ";
                                            lineSql += "(@guid, @startPoint, @endPoint, @viewpointGuid)";

                                            cmd.Parameters["@guid"].Value = line.Guid;
                                            cmd.Parameters["@startPoint"].Value = line.StartPoint.Guid;
                                            cmd.Parameters["@endPoint"].Value = line.EndPoint.Guid;

                                            try
                                            {
                                                cmd.CommandText = lineSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                            string pointSql = "INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z) VALUES ";
                                            pointSql += "(@pointGuid, @pointX, @pointY, @pointZ)";

                                            cmd.Parameters["@pointGuid"].Value = line.StartPoint.Guid;
                                            cmd.Parameters["@pointX"].Value = line.StartPoint.X;
                                            cmd.Parameters["@pointY"].Value = line.StartPoint.Y;
                                            cmd.Parameters["@pointZ"].Value = line.StartPoint.Z;

                                            try
                                            {
                                                cmd.CommandText = pointSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }

                                            cmd.Parameters["@pointGuid"].Value = line.EndPoint.Guid;
                                            cmd.Parameters["@pointX"].Value = line.EndPoint.X;
                                            cmd.Parameters["@pointY"].Value = line.EndPoint.Y;
                                            cmd.Parameters["@pointZ"].Value = line.EndPoint.Z;

                                            try
                                            {
                                                cmd.CommandText = pointSql;
                                                int insertedRows = cmd.ExecuteNonQuery();
                                            }
                                            catch (SQLiteException ex)
                                            {
                                                string message = cmd.CommandText + "\n" + ex.Message;
                                            }
                                        }
                                        cmd.Parameters.Clear();
                                    }
                                    #endregion

                                    #region OrthogonalCamera
                                    OrthogonalCamera orthoCamera = visInfo.OrthogonalCamera;
                                    if (orthoCamera.ViewToWorldScale != 0)
                                    {
                                        string orthoSql = "INSERT OR " + mode.ToString() + " INTO OrthogonalCamera (Guid, CameraViewPoint, CameraDirection, CameraUpVector, ViewToWorldScale, Viewpoints_Guid) VALUES ";
                                        orthoSql += "(@guid, @cameraviewPoint, @cameraDirection, @cameraUpVector, @viewToWorldScale, @viewpointGuid)";

                                        cmd.Parameters.AddWithValue("@guid", orthoCamera.Guid);
                                        cmd.Parameters.AddWithValue("@cameraviewPoint", orthoCamera.CameraViewPoint.Guid);
                                        cmd.Parameters.AddWithValue("@cameraDirection", orthoCamera.CameraDirection.Guid);
                                        cmd.Parameters.AddWithValue("@cameraUpVector", orthoCamera.CameraUpVector.Guid);
                                        cmd.Parameters.AddWithValue("@viewToWorldScale", orthoCamera.ViewToWorldScale);
                                        cmd.Parameters.AddWithValue("@viewpointGuid", viewpoint_guid);

                                        try
                                        {
                                            cmd.CommandText = orthoSql;
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.Parameters.Add("@pointGuid", DbType.String);
                                        cmd.Parameters.Add("@pointX", DbType.Double);
                                        cmd.Parameters.Add("@pointY", DbType.Double);
                                        cmd.Parameters.Add("@pointZ", DbType.Double);

                                        string pointSql = "INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z) VALUES ";
                                        pointSql += "(@pointGuid, @pointX, @pointY, @pointZ)";

                                        cmd.Parameters["@pointGuid"].Value = orthoCamera.CameraViewPoint.Guid;
                                        cmd.Parameters["@pointX"].Value = orthoCamera.CameraViewPoint.X;
                                        cmd.Parameters["@pointY"].Value = orthoCamera.CameraViewPoint.Y;
                                        cmd.Parameters["@pointZ"].Value = orthoCamera.CameraViewPoint.Z;

                                        try
                                        {
                                            cmd.CommandText = pointSql;
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.Parameters.Add("@directionGuid", DbType.String);
                                        cmd.Parameters.Add("@directionX", DbType.Double);
                                        cmd.Parameters.Add("@directionY", DbType.Double);
                                        cmd.Parameters.Add("@directionZ", DbType.Double);

                                        string directionSql = "INSERT OR " + mode.ToString() + " INTO Direction (Guid, X, Y, Z) VALUES ";
                                        directionSql += "(@directionGuid, @directionX, @directionY, @directionZ)";

                                        cmd.Parameters["@directionGuid"].Value = orthoCamera.CameraDirection.Guid;
                                        cmd.Parameters["@directionX"].Value = orthoCamera.CameraDirection.X;
                                        cmd.Parameters["@directionY"].Value = orthoCamera.CameraDirection.Y;
                                        cmd.Parameters["@directionZ"].Value = orthoCamera.CameraDirection.Z;

                                        try
                                        {
                                            cmd.CommandText = directionSql;
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.Parameters["@directionGuid"].Value = orthoCamera.CameraUpVector.Guid;
                                        cmd.Parameters["@directionX"].Value = orthoCamera.CameraUpVector.X;
                                        cmd.Parameters["@directionY"].Value = orthoCamera.CameraUpVector.Y;
                                        cmd.Parameters["@directionZ"].Value = orthoCamera.CameraUpVector.Z;

                                        try
                                        {
                                            cmd.CommandText = directionSql;
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.Parameters.Clear();
                                    }
                                    #endregion

                                    #region PerspectiveCamera
                                    PerspectiveCamera persCamera = visInfo.PerspectiveCamera;
                                    if (persCamera.FieldOfView != 0)
                                    {
                                        string persSql = "INSERT OR " + mode.ToString() + " INTO PerspectiveCamera (Guid, CameraViewPoint, CameraDirection, CameraUpVector, FieldOfView, Viewpoints_Guid) VALUES ";
                                        persSql += "(@guid, @cameraviewPoint, @cameraDirection, @cameraUpVector, @fieldOfView, @viewpointGuid)";

                                        cmd.Parameters.AddWithValue("@guid", persCamera.Guid);
                                        cmd.Parameters.AddWithValue("@cameraviewPoint", persCamera.CameraViewPoint.Guid);
                                        cmd.Parameters.AddWithValue("@cameraDirection", persCamera.CameraDirection.Guid);
                                        cmd.Parameters.AddWithValue("@cameraUpVector", persCamera.CameraUpVector.Guid);
                                        cmd.Parameters.AddWithValue("@fieldOfView", persCamera.FieldOfView);
                                        cmd.Parameters.AddWithValue("@viewpointGuid", viewpoint_guid);

                                        try
                                        {
                                            cmd.CommandText = persSql;
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.Parameters.Add("@pointGuid", DbType.String);
                                        cmd.Parameters.Add("@pointX", DbType.Double);
                                        cmd.Parameters.Add("@pointY", DbType.Double);
                                        cmd.Parameters.Add("@pointZ", DbType.Double);

                                        string pointSql = "INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z) VALUES ";
                                        pointSql += "(@pointGuid, @pointX, @pointY, @pointZ)";

                                        cmd.Parameters["@pointGuid"].Value = persCamera.CameraViewPoint.Guid;
                                        cmd.Parameters["@pointX"].Value = persCamera.CameraViewPoint.X;
                                        cmd.Parameters["@pointY"].Value = persCamera.CameraViewPoint.Y;
                                        cmd.Parameters["@pointZ"].Value = persCamera.CameraViewPoint.Z;

                                        try
                                        {
                                            cmd.CommandText = pointSql;
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.Parameters.Add("@directionGuid", DbType.String);
                                        cmd.Parameters.Add("@directionX", DbType.Double);
                                        cmd.Parameters.Add("@directionY", DbType.Double);
                                        cmd.Parameters.Add("@directionZ", DbType.Double);

                                        string directionSql = "INSERT OR " + mode.ToString() + " INTO Direction (Guid, X, Y, Z) VALUES ";
                                        directionSql += "(@directionGuid, @directionX, @directionY, @directionZ)";

                                        cmd.Parameters["@directionGuid"].Value = persCamera.CameraDirection.Guid;
                                        cmd.Parameters["@directionX"].Value = persCamera.CameraDirection.X;
                                        cmd.Parameters["@directionY"].Value = persCamera.CameraDirection.Y;
                                        cmd.Parameters["@directionZ"].Value = persCamera.CameraDirection.Z;

                                        try
                                        {
                                            cmd.CommandText = directionSql;
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.Parameters["@directionGuid"].Value = persCamera.CameraUpVector.Guid;
                                        cmd.Parameters["@directionX"].Value = persCamera.CameraUpVector.X;
                                        cmd.Parameters["@directionY"].Value = persCamera.CameraUpVector.Y;
                                        cmd.Parameters["@directionZ"].Value = persCamera.CameraUpVector.Z;

                                        try
                                        {
                                            cmd.CommandText = directionSql;
                                            int insertedRows = cmd.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException ex)
                                        {
                                            string message = cmd.CommandText + "\n" + ex.Message;
                                        }

                                        cmd.Parameters.Clear();
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

        private static bool InsertCustomValues(BCFZIP bcfzip, SQLiteConnection connection, ConflictMode mode)
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
                            strBuilder.Append("INSERT OR " + mode.ToString() + " INTO BCFFileInfo (Guid, FileName, FilePath, UploadedBy, UploadedDate, CreationDate, Project_Guid, Version_Guid) VALUES ");
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
                            strBuilder.Append("INSERT OR " + mode.ToString() + " INTO FileTopics (Topic_Guid, File_Guid) VALUES ");
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
