using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HOK.SmartBCF.Schemas;
using Version = HOK.SmartBCF.Schemas.Version;
using Point = HOK.SmartBCF.Schemas.Point;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.BCFDBReader
{
    public static class BCFDBReader
    {
        public static SQLiteConnection connection = null;

        public static string GetConnectionString(string dbFile)
        {
            string connectionString = "";
            try
            {
                SQLiteConnectionStringBuilder connectionBuilder = new SQLiteConnectionStringBuilder();
                connectionBuilder.DataSource = dbFile;
                connectionBuilder.Version = 3;
                connectionBuilder.ForeignKeys = true;
                connectionString = connectionBuilder.ConnectionString;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return connectionString;
        }

        public static Dictionary<string, BCFZIP> ReadDatabase(string dbFile)
        {
            Dictionary<string, BCFZIP> bcfDictionary = new Dictionary<string, BCFZIP>();
            try
            {
                string connectionString = GetConnectionString(dbFile);
                using (connection = new SQLiteConnection(connectionString, true))
                {
                    connection.Open();
                    //read tables
                    //file Info
                    Dictionary<string, BCFZIP> bcfFileInfo = GetBCFFileInfoTable();
                    Dictionary<string, ProjectExtension> projectExts = GetProjectExtensionTable();
                    Dictionary<string, Project> projects = GetProjectTable();
                    Dictionary<string, Version> versions = GetVersionTable();

                    //markup
                    Dictionary<string, Topic> topics = GetTopicTables();
                    Dictionary<string, List<string>> labels = GetLabelTable();
                    Dictionary<string, HeaderFile> headerFiles = GetHeaderFileTable();
                    Dictionary<string, BimSnippet> bimSnippets = GetBimSnippetTable();
                    Dictionary<string, TopicDocumentReferences> docReferences = GetDocumentReferencesTable();
                    Dictionary<string, TopicRelatedTopics> relTopics = GetRelTopicTables();
                    Dictionary<string, Comment> comments = GetCommentTable();
                    Dictionary<string, ViewPoint> viewpoints = GetViewpointsTable();

                    //visualizationInfo
                    Dictionary<string, RevitExtension> extensions = GetRevitExtensionTable();
                    Dictionary<string, Component> components = GetComponentTable();
                    Dictionary<string, Point> points = GetPointTable();
                    Dictionary<string, Direction> directions = GetDirectionTable();
                    Dictionary<string, VisualizationInfoBitmaps> bitmaps = GetBitmapsTable();
                    Dictionary<string, ClippingPlane> clippingPlanes = GetClippingPlaneTable();
                    Dictionary<string, Line> lines = GetLineTable();
                    Dictionary<string, OrthogonalCamera> orthoCameras = GetOrthoCameraTable();
                    Dictionary<string, PerspectiveCamera> persCameras = GetPerspectiveTable();

                    //combine tables to class structure
                    foreach (string fileId in bcfFileInfo.Keys)
                    {
                        //get file Info
                        BCFZIP bcfZip = bcfFileInfo[fileId];
                        RevitExtensionInfo extInfo = new RevitExtensionInfo();
                        extInfo.Extensions = new ObservableCollection<RevitExtension>(extensions.Values.ToList());
                        bcfZip.ExtensionColor = extInfo;

                        var projectExtFound = from projectExt in projectExts.Values where projectExt.FileId == bcfZip.FileId select projectExt;
                        if (projectExtFound.Count() > 0)
                        {
                            ProjectExtension projectExt = projectExtFound.First();
                            var projectFound = from project in projects.Values where project.ExtensionGuid == projectExt.Guid select project;
                            if (projectFound.Count() > 0)
                            {
                                projectExt.Project = projectFound.First();
                            }
                            bcfZip.ProjectFile = projectExt;
                        }

                        var versionFound = from version in versions.Values where version.FileId == bcfZip.FileId select version;
                        if (versionFound.Count() > 0)
                        {
                            bcfZip.VersionFile = versionFound.First();
                        }

                        List<string> topicIds = new List<string>();
                        var topicFound = from topic in topics.Values where topic.FileId == bcfZip.FileId select topic.Guid;
                        if (topicFound.Count() > 0)
                        {
                            topicIds = topicFound.ToList();
                        }

                        ProgressManager.InitializeProgress("Reading "+bcfZip.ZipFileName+"...", topicIds.Count);
                        foreach (string topicId in topicIds)
                        {
                            //Get Markup
                            ProgressManager.StepForward();

                            #region Combine Markup
                            Markup markup = new Markup();
                            markup.Topic = topics[topicId];
                            markup.Topic.Labels = (labels.ContainsKey(topicId)) ? labels[topicId] : new List<string>();

                            var snippetFound = from snippet in bimSnippets.Values where snippet.TopicGuid == topicId select snippet;
                            if (snippetFound.Count() > 0)
                            {
                                markup.Topic.BimSnippet = snippetFound.First();
                            }

                            var docRefFound = from docRef in docReferences.Values where docRef.TopicGuid == topicId select docRef;
                            if (docRefFound.Count() > 0)
                            {
                                markup.Topic.DocumentReferences = docRefFound.ToList();
                            }

                            var relTopicFound = from relTopic in relTopics.Values where relTopic.TopicGuid == topicId select relTopic;
                            if (relTopicFound.Count() > 0)
                            {
                                markup.Topic.RelatedTopics = relTopicFound.ToList();
                            }

                            var headerFound = from header in headerFiles.Values where header.TopicGuid == topicId select header;
                            if (headerFound.Count() > 0)
                            {
                                markup.Header = headerFound.ToList();
                            }

                            ObservableCollection<Comment> commentList = new ObservableCollection<Comment>();
                            var commentFound = from comment in comments.Values where comment.TopicGuid == topicId select comment;
                            if (commentFound.Count() > 0)
                            {
                                commentList = new ObservableCollection<Comment>(commentFound.ToList());
                                markup.Comment = commentList;
                            }

                            var viewpointFound = from viewpoint in viewpoints.Values where viewpoint.TopicGuid == topicId select viewpoint;
                            markup.Viewpoints = new ObservableCollection<ViewPoint>(viewpointFound.ToList());

                            if (markup.Viewpoints.Count > 0)
                            {
                                markup.TopicImage = markup.Viewpoints[0].SnapshotImage;
                            }

                            #endregion

                            //Get Visualization Info
                            #region Combine Visualization Info
                            for (int i = 0; i < markup.Viewpoints.Count; i++)
                            {
                                string viewPointGuid = markup.Viewpoints[i].Guid;

                                VisualizationInfo visInfo = new VisualizationInfo();
                                visInfo.ViewPointGuid = viewPointGuid;


                                var componentFound = from comp in components.Values where comp.ViewPointGuid == viewPointGuid select comp;
                                if (componentFound.Count() > 0)
                                {
                                    ObservableCollection<Component> componentCollection = new ObservableCollection<Component>();
                                    foreach (Component comp in componentFound)
                                    {
                                        Component compItem = comp;
                                        var foundAction = from ext in bcfZip.ExtensionColor.Extensions where ext.Guid == compItem.Action.Guid select ext;
                                        if (foundAction.Count() > 0)
                                        {
                                            compItem.Action = foundAction.First();
                                        }

                                        var foundResponsibility = from ext in bcfZip.ExtensionColor.Extensions where ext.Guid == compItem.Responsibility.Guid select ext;
                                        if (foundResponsibility.Count() > 0)
                                        {
                                            compItem.Responsibility = foundResponsibility.First();
                                        }

                                        componentCollection.Add(compItem);
                                    }
                                    visInfo.Components = componentCollection;
                                }

                                var orthoCameraFound = from ortho in orthoCameras.Values where ortho.ViewPointGuid == viewPointGuid select ortho;
                                if (orthoCameraFound.Count() > 0)
                                {
                                    OrthogonalCamera orthoCamera = orthoCameraFound.First();
                                    orthoCamera.CameraViewPoint = (points.ContainsKey(orthoCamera.CameraViewPoint.Guid)) ? points[orthoCamera.CameraViewPoint.Guid] : new Point();
                                    orthoCamera.CameraDirection = (directions.ContainsKey(orthoCamera.CameraDirection.Guid)) ? directions[orthoCamera.CameraDirection.Guid] : new Direction();
                                    orthoCamera.CameraUpVector = (directions.ContainsKey(orthoCamera.CameraUpVector.Guid)) ? directions[orthoCamera.CameraUpVector.Guid] : new Direction();

                                    visInfo.OrthogonalCamera = orthoCamera;
                                }

                                var persCameraFound = from pers in persCameras.Values where pers.ViewPointGuid == viewPointGuid select pers;
                                if (persCameraFound.Count() > 0)
                                {
                                    PerspectiveCamera persCamera = persCameraFound.First();
                                    persCamera.CameraViewPoint = (points.ContainsKey(persCamera.CameraViewPoint.Guid)) ? points[persCamera.CameraViewPoint.Guid] : new Point();
                                    persCamera.CameraDirection = (directions.ContainsKey(persCamera.CameraDirection.Guid)) ? directions[persCamera.CameraDirection.Guid] : new Direction();
                                    persCamera.CameraUpVector = (directions.ContainsKey(persCamera.CameraUpVector.Guid)) ? directions[persCamera.CameraUpVector.Guid] : new Direction();

                                    visInfo.PerspectiveCamera = persCamera;
                                }

                                var lineFound = from line in lines.Values where line.ViewPointGuid == viewPointGuid select line;
                                if (lineFound.Count() > 0)
                                {
                                    List<Line> lineList = new List<Line>();
                                    foreach (Line line in lineFound)
                                    {
                                        Line lineItem = line;
                                        lineItem.StartPoint = (points.ContainsKey(lineItem.StartPoint.Guid)) ? points[lineItem.StartPoint.Guid] : new Point();
                                        lineItem.EndPoint = (points.ContainsKey(lineItem.EndPoint.Guid)) ? points[lineItem.EndPoint.Guid] : new Point();
                                        lineList.Add(lineItem);
                                    }
                                    visInfo.Lines = lineList;
                                }

                                var clippingPlaneFound = from plane in clippingPlanes.Values where plane.ViewPointGuid == viewPointGuid select plane;
                                if (clippingPlaneFound.Count() > 0)
                                {
                                    List<ClippingPlane> planeList = new List<ClippingPlane>();
                                    foreach (ClippingPlane plane in clippingPlaneFound)
                                    {
                                        ClippingPlane planeItem = plane;
                                        planeItem.Location = (points.ContainsKey(planeItem.Location.Guid)) ? points[planeItem.Location.Guid] : new Point();
                                        planeItem.Direction = (directions.ContainsKey(planeItem.Direction.Guid)) ? directions[planeItem.Direction.Guid] : new Direction();
                                        planeList.Add(planeItem);
                                    }
                                    visInfo.ClippingPlanes = planeList;
                                }

                                var bitmapFound = from bitmap in bitmaps.Values where bitmap.ViewPointGuid == viewPointGuid select bitmap;
                                if (bitmapFound.Count() > 0)
                                {
                                    List<VisualizationInfoBitmaps> bitmapList = new List<VisualizationInfoBitmaps>();
                                    foreach (VisualizationInfoBitmaps bitmap in bitmapFound)
                                    {
                                        VisualizationInfoBitmaps bitmapItem = bitmap;
                                        bitmapItem.Location = (points.ContainsKey(bitmapItem.Location.Guid)) ? points[bitmapItem.Location.Guid] : new Point();
                                        bitmapItem.Normal = (directions.ContainsKey(bitmapItem.Normal.Guid)) ? directions[bitmapItem.Normal.Guid] : new Direction();
                                        bitmapItem.Up = (directions.ContainsKey(bitmapItem.Up.Guid)) ? directions[bitmap.Up.Guid] : new Direction();
                                        bitmapList.Add(bitmapItem);
                                    }
                                    visInfo.Bitmaps = bitmapList;
                                }

                                markup.Viewpoints[i].VisInfo = visInfo;
                            }
                            if (markup.Viewpoints.Count > 0) { markup.SelectedViewpoint = markup.Viewpoints.First(); }
                            #endregion

                            bcfZip.Markups.Add(markup);
                        }
                        bcfZip.Markups = new ObservableCollection<Markup>(bcfZip.Markups.OrderBy(o => o.Topic.Index).ToList());
                        bcfZip.SelectedMarkup =0;
                        bcfDictionary.Add(bcfZip.FileId, bcfZip);
                    }
                    ProgressManager.FinalizeProgress(dbFile);
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read BCF tables in the database.\n" + ex.Message, "Read BCF Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfDictionary;
        }

        private static Dictionary<string/*guid*/, BCFZIP> GetBCFFileInfoTable()
        {
            Dictionary<string, BCFZIP> bcfFiles = new Dictionary<string, BCFZIP>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
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
                            bcfZip.IsPrimary = reader.GetBoolean(reader.GetOrdinal("IsPrimary"));
                            if (!bcfFiles.ContainsKey(bcfZip.FileId))
                            {
                                bcfFiles.Add(bcfZip.FileId, bcfZip);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get BCF File information.\n" + ex.Message, "Get BCF File Info Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfFiles;
        }

        private static Dictionary<string, BimSnippet> GetBimSnippetTable()
        {
            Dictionary<string, BimSnippet> bimSnippets = new Dictionary<string, BimSnippet>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM BimSnippet";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BimSnippet bimSnippet = new BimSnippet();
                            bimSnippet.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            bimSnippet.SnippetType = reader.GetString(reader.GetOrdinal("SnippetType"));
                            bimSnippet.isExternal = reader.GetBoolean(reader.GetOrdinal("isExternal"));
                            bimSnippet.Reference = reader.GetString(reader.GetOrdinal("Reference"));
                            bimSnippet.ReferenceSchema = reader.GetString(reader.GetOrdinal("ReferenceSchema"));
                            if (reader["FileContent"] != System.DBNull.Value)
                            {
                                bimSnippet.FileContent = (byte[])reader["FileContent"];
                            }
                            bimSnippet.TopicGuid = reader.GetString(reader.GetOrdinal("Topic_Guid"));

                            if (!bimSnippets.ContainsKey(bimSnippet.Guid))
                            {
                                bimSnippets.Add(bimSnippet.Guid, bimSnippet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get BIM Snippet table.\n" + ex.Message, "Get BIM Snippet Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bimSnippets;
        }

        private static Dictionary<string, VisualizationInfoBitmaps> GetBitmapsTable()
        {
            Dictionary<string, VisualizationInfoBitmaps> bitmaps = new Dictionary<string, VisualizationInfoBitmaps>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Bitmaps";
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

                            bitmap.Reference = reader.GetString(reader.GetOrdinal("Reference"));
                            bitmap.Location.Guid = reader.GetString(reader.GetOrdinal("Location"));
                            bitmap.Normal.Guid = reader.GetString(reader.GetOrdinal("Normal"));
                            bitmap.Up.Guid = reader.GetString(reader.GetOrdinal("Up"));
                            bitmap.Height = reader.GetDouble(reader.GetOrdinal("Height"));
                            bitmap.ViewPointGuid = reader.GetString(reader.GetOrdinal("Viewpoints_Guid"));

                            if (!bitmaps.ContainsKey(bitmap.Guid))
                            {
                                bitmaps.Add(bitmap.Guid, bitmap);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get Bitmaps table.\n" + ex.Message, "Get Bitmap Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bitmaps;
        }

        private static Dictionary<string, ClippingPlane> GetClippingPlaneTable()
        {
            Dictionary<string, ClippingPlane> clippingPlanes = new Dictionary<string, ClippingPlane>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM ClippingPlane";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ClippingPlane cp = new ClippingPlane();
                            cp.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            cp.Location.Guid = reader.GetString(reader.GetOrdinal("Location"));
                            cp.Direction.Guid = reader.GetString(reader.GetOrdinal("Direction"));
                            cp.ViewPointGuid = reader.GetString(reader.GetOrdinal("Viewpoints_Guid"));

                            if (!clippingPlanes.ContainsKey(cp.Guid))
                            {
                                clippingPlanes.Add(cp.Guid, cp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get ClippingPlane table.\n" + ex.Message, "Get Clipping Plane Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return clippingPlanes;
        }

        private static Dictionary<string, Comment> GetCommentTable()
        {
            Dictionary<string, Comment> comments = new Dictionary<string, Comment>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Comment";
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
                            comment.TopicGuid = reader.GetString(reader.GetOrdinal("Topic_Guid"));
                            comment.Viewpoint.Guid = reader.GetString(reader.GetOrdinal("Viewpoint_Guid"));

                            if (!comments.ContainsKey(comment.Guid))
                            {
                                comments.Add(comment.Guid, comment);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get Comment table.\n" + ex.Message, "Get Comment Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return comments;
        }

        private static Dictionary<string, Component> GetComponentTable()
        {
            Dictionary<string, Component> components = new Dictionary<string, Component>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Components";
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
                            comp.ViewPointGuid = reader.GetString(reader.GetOrdinal("Viewpoints_Guid"));
                            comp.ElementName = reader.GetString(reader.GetOrdinal("ElementName"));
                            comp.Action.Guid = reader.GetString(reader.GetOrdinal("Action"));
                            comp.Responsibility.Guid = reader.GetString(reader.GetOrdinal("Responsibility"));

                            if (!components.ContainsKey(comp.Guid))
                            {
                                components.Add(comp.Guid, comp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get component table.\n" + ex.Message, "Get Component Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return components;
        }

        private static Dictionary<string, Direction> GetDirectionTable()
        {
            Dictionary<string, Direction> directions = new Dictionary<string, Direction>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Direction";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Direction direction = new Direction();
                            direction.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            direction.X = reader.GetDouble(reader.GetOrdinal("X"));
                            direction.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                            direction.Z = reader.GetDouble(reader.GetOrdinal("Z"));
                            if (!directions.ContainsKey(direction.Guid))
                            {
                                directions.Add(direction.Guid, direction);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get direction table.\n" + ex.Message, "Get Direction Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return directions;
        }

        private static Dictionary<string, TopicDocumentReferences> GetDocumentReferencesTable()
        {
            Dictionary<string, TopicDocumentReferences> docReferences = new Dictionary<string, TopicDocumentReferences>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM DocumentReferences";
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
                            docRef.TopicGuid = reader.GetString(reader.GetOrdinal("Topic_Guid"));
                            if (!docReferences.ContainsKey(docRef.Guid))
                            {
                                docReferences.Add(docRef.Guid, docRef);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get document references table.\n" + ex.Message, "Get Document References Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return docReferences;
        }

        private static Dictionary<string, HeaderFile> GetHeaderFileTable()
        {
            Dictionary<string, HeaderFile> headerFiles = new Dictionary<string, HeaderFile>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM HeaderFile";
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
                            headerFile.TopicGuid = reader.GetString(reader.GetOrdinal("Topic_Guid"));

                            if (!headerFiles.ContainsKey(headerFile.Guid))
                            {
                                headerFiles.Add(headerFile.Guid, headerFile);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get header files.\n" + ex.Message, "Get Header Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return headerFiles;
        }

        private static Dictionary<string/*topicId*/, List<string> /*labels*/> GetLabelTable()
        {
            Dictionary<string, List<string>> labels = new Dictionary<string, List<string>>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Labels";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string label = reader.GetString(reader.GetOrdinal("Label"));
                            string topicGuid = reader.GetString(reader.GetOrdinal("Topic_Guid"));

                            if (labels.ContainsKey(topicGuid))
                            {
                                labels[topicGuid].Add(label);
                            }
                            else
                            {
                                List<string> labelList = new List<string>();
                                labelList.Add(label);
                                labels.Add(topicGuid, labelList);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get label table.\n" + ex.Message, "Get Label Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return labels;
        }

        private static Dictionary<string, Line> GetLineTable()
        {
            Dictionary<string, Line> lines = new Dictionary<string, Line>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Lines";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Line line = new Line();
                            line.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            line.StartPoint.Guid = reader.GetString(reader.GetOrdinal("StartPoint"));
                            line.EndPoint.Guid = reader.GetString(reader.GetOrdinal("EndPoint"));
                            line.ViewPointGuid = reader.GetString(reader.GetOrdinal("Viewpoints_Guid"));

                            if (!lines.ContainsKey(line.Guid))
                            {
                                lines.Add(line.Guid, line);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get Line table.\n" + ex.Message, "Get Line Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return lines;
        }

        private static Dictionary<string, OrthogonalCamera> GetOrthoCameraTable()
        {
            Dictionary<string, OrthogonalCamera> orthoCameras = new Dictionary<string, OrthogonalCamera>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM OrthogonalCamera";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OrthogonalCamera orthoCamera = new OrthogonalCamera();
                            orthoCamera.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            orthoCamera.CameraViewPoint.Guid = reader.GetString(reader.GetOrdinal("CameraViewPoint"));
                            orthoCamera.CameraDirection.Guid = reader.GetString(reader.GetOrdinal("CameraDirection"));
                            orthoCamera.CameraUpVector.Guid = reader.GetString(reader.GetOrdinal("CameraUpVector"));
                            orthoCamera.ViewToWorldScale = reader.GetDouble(reader.GetOrdinal("ViewToWorldScale"));
                            orthoCamera.ViewPointGuid = reader.GetString(reader.GetOrdinal("Viewpoints_Guid"));

                            if (!orthoCameras.ContainsKey(orthoCamera.Guid))
                            {
                                orthoCameras.Add(orthoCamera.Guid, orthoCamera);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get orthogonal camera.\n" + ex.Message, "Get Orthogonal Camera", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return orthoCameras;
        }

        private static Dictionary<string, PerspectiveCamera> GetPerspectiveTable()
        {
            Dictionary<string, PerspectiveCamera> persCameras = new Dictionary<string, PerspectiveCamera>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM PerspectiveCamera";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PerspectiveCamera persCamera = new PerspectiveCamera();
                            persCamera.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            persCamera.CameraViewPoint.Guid = reader.GetString(reader.GetOrdinal("CameraViewPoint"));
                            persCamera.CameraDirection.Guid = reader.GetString(reader.GetOrdinal("CameraDirection"));
                            persCamera.CameraUpVector.Guid = reader.GetString(reader.GetOrdinal("CameraUpVector"));
                            persCamera.FieldOfView = reader.GetDouble(reader.GetOrdinal("FieldOfView"));
                            persCamera.ViewPointGuid = reader.GetString(reader.GetOrdinal("Viewpoints_Guid"));

                            if (!persCameras.ContainsKey(persCamera.Guid))
                            {
                                persCameras.Add(persCamera.Guid, persCamera);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get perspective camera.\n" + ex.Message, "Get Perspective Camera", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return persCameras;
        }

        private static Dictionary<string, Point> GetPointTable()
        {
            Dictionary<string, Point> points = new Dictionary<string, Point>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Point";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Point point = new Point();
                            point.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            point.X = reader.GetDouble(reader.GetOrdinal("X"));
                            point.Y = reader.GetDouble(reader.GetOrdinal("Y"));
                            point.Z = reader.GetDouble(reader.GetOrdinal("Z"));

                            if (!points.ContainsKey(point.Guid))
                            {
                                points.Add(point.Guid, point);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get point.\n" + ex.Message, "Get Point Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return points;
        }

        private static Dictionary<string, Project> GetProjectTable()
        {
            Dictionary<string, Project> projects = new Dictionary<string, Project>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Project";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Project project = new Project();
                            project.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            project.ProjectId = reader.GetString(reader.GetOrdinal("ProjectId"));
                            project.Name = reader.GetString(reader.GetOrdinal("ProjectName"));
                            project.ExtensionGuid = reader.GetString(reader.GetOrdinal("ProjectExtension_Guid"));

                            if (!projects.ContainsKey(project.Guid))
                            {
                                projects.Add(project.Guid, project);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get project.\n" + ex.Message, "Get Project Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return projects;
        }

        private static Dictionary<string, ProjectExtension> GetProjectExtensionTable()
        {
            Dictionary<string, ProjectExtension> projectExts = new Dictionary<string, ProjectExtension>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM ProjectExtension";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProjectExtension projectExt = new ProjectExtension();
                            projectExt.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            projectExt.ExtensionSchema = reader.GetString(reader.GetOrdinal("ExtensionSchema"));
                            projectExt.FileId = reader.GetString(reader.GetOrdinal("File_Guid"));
                            if (!projectExts.ContainsKey(projectExt.Guid))
                            {
                                projectExts.Add(projectExt.Guid, projectExt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get project extension.\n" + ex.Message, "Get Project Extension Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return projectExts;
        }

        private static Dictionary<string, TopicRelatedTopics> GetRelTopicTables()
        {
            Dictionary<string, TopicRelatedTopics> relTopics = new Dictionary<string, TopicRelatedTopics>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM RelatedTopics";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TopicRelatedTopics relTopic = new TopicRelatedTopics();
                            relTopic.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            relTopic.TopicGuid = reader.GetString(reader.GetOrdinal("Topic_Guid"));

                            if (!relTopics.ContainsKey(relTopic.Guid))
                            {
                                relTopics.Add(relTopic.Guid, relTopic);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get related topic.\n" + ex.Message, "Get Related Topic Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return relTopics;
        }

        private static Dictionary<string, RevitExtension> GetRevitExtensionTable()
        {
            Dictionary<string, RevitExtension> extensions = new Dictionary<string, RevitExtension>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM RevitExtensions";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RevitExtension extension = new RevitExtension();
                            extension.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            extension.ParameterName = reader.GetString(reader.GetOrdinal("ParameterName"));
                            extension.ParameterValue = reader.GetString(reader.GetOrdinal("ParameterValue"));
                            if (reader["Color"] != System.DBNull.Value)
                            {
                                extension.Color = (byte[])reader["Color"];
                            }

                            if (!extensions.ContainsKey(extension.Guid))
                            {
                                extensions.Add(extension.Guid, extension);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get related topic.\n" + ex.Message, "Get Related Topic Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return extensions;
        }

        private static Dictionary<string, Topic> GetTopicTables()
        {
            Dictionary<string, Topic> topics = new Dictionary<string, Topic>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Topic";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Topic topic = new Topic();
                            topic.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            string topicType = reader.GetString(reader.GetOrdinal("TopicType"));
                            topic.TopicType = (string.IsNullOrEmpty(topicType)) ? TopicType.Unknown : (TopicType)Enum.Parse(typeof(TopicType), topicType);
                            string topicStatus = reader.GetString(reader.GetOrdinal("TopicStatus"));
                            topic.TopicStatus = (string.IsNullOrEmpty(topicStatus)) ? TopicStatus.Open : (TopicStatus)Enum.Parse(typeof(TopicStatus), topicStatus);
                            topic.Title = reader.GetString(reader.GetOrdinal("Title"));
                            topic.ReferenceLink = reader.GetString(reader.GetOrdinal("ReferenceLink"));
                            topic.Description = reader.GetString(reader.GetOrdinal("Description"));
                            topic.Priority = reader.GetString(reader.GetOrdinal("Priority"));
                            topic.Index = reader.GetInt32(reader.GetOrdinal("TopicIndex"));

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
                            topic.FileId = reader.GetString(reader.GetOrdinal("File_Guid"));

                            if (!topics.ContainsKey(topic.Guid))
                            {
                                topics.Add(topic.Guid, topic);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get topic.\n" + ex.Message, "Get Topic Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return topics;
        }

        private static Dictionary<string, Version> GetVersionTable()
        {
            Dictionary<string, Version> versions = new Dictionary<string, Version>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Version";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Version version = new Version();
                            version.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            version.VersionId = reader.GetString(reader.GetOrdinal("VersionId"));
                            version.DetailedVersion = reader.GetString(reader.GetOrdinal("DetailedVersion"));
                            version.FileId = reader.GetString(reader.GetOrdinal("File_Guid"));

                            if (!versions.ContainsKey(version.Guid))
                            {
                                versions.Add(version.Guid, version);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get version.\n" + ex.Message, "Get Version Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return versions;
        }

        private static Dictionary<string, ViewPoint> GetViewpointsTable()
        {
            Dictionary<string, ViewPoint> viewpoints = new Dictionary<string, ViewPoint>();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Viewpoints";
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
                            vp.TopicGuid = reader.GetString(reader.GetOrdinal("Topic_Guid"));

                            if (!viewpoints.ContainsKey(vp.Guid))
                            {
                                viewpoints.Add(vp.Guid, vp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get viewpoints.\n" + ex.Message, "Get Viewpoints Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewpoints;
        }
    }
}
