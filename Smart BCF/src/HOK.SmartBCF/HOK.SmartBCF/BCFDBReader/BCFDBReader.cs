using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using HOK.SmartBCF.Utils;
using HOK.SmartBCF.Schemas;
using Version = HOK.SmartBCF.Schemas.Version;
using Point = HOK.SmartBCF.Schemas.Point;

namespace HOK.SmartBCF.BCFDBReader
{
    public static class BCFDBReader
    {
        public static SQLiteConnection connection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbFile"></param>
        /// <returns></returns>
        public static string GetConnectionString(string dbFile)
        {
            var connectionString = "";
            try
            {
                var connectionBuilder = new SQLiteConnectionStringBuilder
                {
                    DataSource = dbFile,
                    Version = 3,
                    ForeignKeys = true
                };
                connectionString = connectionBuilder.ConnectionString;
            }
            catch (Exception)
            {
                // ignored
            }
            return connectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbFile"></param>
        /// <returns></returns>
        public static Dictionary<string, BCFZIP> ReadDatabase(string dbFile)
        {
            var bcfDictionary = new Dictionary<string, BCFZIP>();
            try
            {
                var connectionString = GetConnectionString(dbFile);
                using (connection = new SQLiteConnection(connectionString, true))
                {
                    connection.Open();
                    //read tables
                    //file Info
                    var bcfFileInfo = GetBCFFileInfoTable();
                    var projectExts = GetProjectExtensionTable();
                    var projects = GetProjectTable();
                    var versions = GetVersionTable();

                    //markup
                    var topics = GetTopicTables();
                    var labels = GetLabelTable();
                    var headerFiles = GetHeaderFileTable();
                    var bimSnippets = GetBimSnippetTable();
                    var docReferences = GetDocumentReferencesTable();
                    var relTopics = GetRelTopicTables();
                    var comments = GetCommentTable();
                    var viewpoints = GetViewpointsTable();

                    //visualizationInfo
                    var extensions = GetRevitExtensionTable();
                    var components = GetComponentTable();
                    var points = GetPointTable();
                    var directions = GetDirectionTable();
                    var bitmaps = GetBitmapsTable();
                    var clippingPlanes = GetClippingPlaneTable();
                    var lines = GetLineTable();
                    var orthoCameras = GetOrthoCameraTable();
                    var persCameras = GetPerspectiveTable();

                    //combine tables to class structure
                    foreach (var fileId in bcfFileInfo.Keys)
                    {
                        //get file Info
                        var bcfZip = bcfFileInfo[fileId];
                        var extInfo = new RevitExtensionInfo();
                        extInfo.Extensions = new ObservableCollection<RevitExtension>(extensions.Values.ToList());
                        bcfZip.ExtensionColor = extInfo;

                        var projectExtFound = from projectExt in projectExts.Values where projectExt.FileId == bcfZip.FileId select projectExt;
                        if (projectExtFound.Any())
                        {
                            var projectExt = projectExtFound.First();
                            var projectFound = from project in projects.Values where project.ExtensionGuid == projectExt.Guid select project;
                            if (projectFound.Any())
                            {
                                projectExt.Project = projectFound.First();
                            }
                            bcfZip.ProjectFile = projectExt;
                        }

                        var versionFound = from version in versions.Values where version.FileId == bcfZip.FileId select version;
                        if (versionFound.Any())
                        {
                            bcfZip.VersionFile = versionFound.First();
                        }

                        var topicIds = new List<string>();
                        var topicFound = from topic in topics.Values where topic.FileId == bcfZip.FileId select topic.Guid;
                        if (topicFound.Any())
                        {
                            topicIds = topicFound.ToList();
                        }

                        ProgressManager.InitializeProgress("Reading "+bcfZip.ZipFileName+"...", topicIds.Count);
                        ProgressManager.databaseFilePath = dbFile;
                        foreach (var topicId in topicIds)
                        {
                            //Get Markup
                            ProgressManager.StepForward();

                            #region Combine Markup
                            var markup = new Markup
                            {
                                Topic = topics[topicId]
                            };
                            markup.Topic.Labels = (labels.ContainsKey(topicId)) ? labels[topicId] : new List<string>();

                            var snippetFound = from snippet in bimSnippets.Values where snippet.TopicGuid == topicId select snippet;
                            if (snippetFound.Any())
                            {
                                markup.Topic.BimSnippet = snippetFound.First();
                            }

                            var docRefFound = from docRef in docReferences.Values where docRef.TopicGuid == topicId select docRef;
                            if (docRefFound.Any())
                            {
                                markup.Topic.DocumentReferences = docRefFound.ToList();
                            }

                            var relTopicFound = from relTopic in relTopics.Values where relTopic.TopicGuid == topicId select relTopic;
                            if (relTopicFound.Any())
                            {
                                markup.Topic.RelatedTopics = relTopicFound.ToList();
                            }

                            var headerFound = from header in headerFiles.Values where header.TopicGuid == topicId select header;
                            if (headerFound.Any())
                            {
                                markup.Header = headerFound.ToList();
                            }

                            var commentList = new ObservableCollection<Comment>();
                            var commentFound = from comment in comments.Values where comment.TopicGuid == topicId select comment;
                            if (commentFound.Any())
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
                            for (var i = 0; i < markup.Viewpoints.Count; i++)
                            {
                                var viewPointGuid = markup.Viewpoints[i].Guid;

                                var visInfo = new VisualizationInfo();
                                visInfo.ViewPointGuid = viewPointGuid;


                                var componentFound = from comp in components.Values where comp.ViewPointGuid == viewPointGuid select comp;
                                if (componentFound.Any())
                                {
                                    var componentCollection = new ObservableCollection<Component>();
                                    foreach (var comp in componentFound)
                                    {
                                        var compItem = comp;
                                        var foundAction = from ext in bcfZip.ExtensionColor.Extensions where ext.Guid == compItem.Action.Guid select ext;
                                        if (foundAction.Any())
                                        {
                                            compItem.Action = foundAction.First();
                                        }

                                        var foundResponsibility = from ext in bcfZip.ExtensionColor.Extensions where ext.Guid == compItem.Responsibility.Guid select ext;
                                        if (foundResponsibility.Any())
                                        {
                                            compItem.Responsibility = foundResponsibility.First();
                                        }

                                        componentCollection.Add(compItem);
                                    }
                                    visInfo.Components = componentCollection;
                                }

                                var orthoCameraFound = from ortho in orthoCameras.Values where ortho.ViewPointGuid == viewPointGuid select ortho;
                                if (orthoCameraFound.Any())
                                {
                                    var orthoCamera = orthoCameraFound.First();
                                    orthoCamera.CameraViewPoint = (points.ContainsKey(orthoCamera.CameraViewPoint.Guid)) ? points[orthoCamera.CameraViewPoint.Guid] : new Point();
                                    orthoCamera.CameraDirection = (directions.ContainsKey(orthoCamera.CameraDirection.Guid)) ? directions[orthoCamera.CameraDirection.Guid] : new Direction();
                                    orthoCamera.CameraUpVector = (directions.ContainsKey(orthoCamera.CameraUpVector.Guid)) ? directions[orthoCamera.CameraUpVector.Guid] : new Direction();

                                    visInfo.OrthogonalCamera = orthoCamera;
                                }

                                var persCameraFound = from pers in persCameras.Values where pers.ViewPointGuid == viewPointGuid select pers;
                                if (persCameraFound.Any())
                                {
                                    var persCamera = persCameraFound.First();
                                    persCamera.CameraViewPoint = (points.ContainsKey(persCamera.CameraViewPoint.Guid)) ? points[persCamera.CameraViewPoint.Guid] : new Point();
                                    persCamera.CameraDirection = (directions.ContainsKey(persCamera.CameraDirection.Guid)) ? directions[persCamera.CameraDirection.Guid] : new Direction();
                                    persCamera.CameraUpVector = (directions.ContainsKey(persCamera.CameraUpVector.Guid)) ? directions[persCamera.CameraUpVector.Guid] : new Direction();

                                    visInfo.PerspectiveCamera = persCamera;
                                }

                                var lineFound = from line in lines.Values where line.ViewPointGuid == viewPointGuid select line;
                                if (lineFound.Any())
                                {
                                    var lineList = new List<Line>();
                                    foreach (var line in lineFound)
                                    {
                                        var lineItem = line;
                                        lineItem.StartPoint = (points.ContainsKey(lineItem.StartPoint.Guid)) ? points[lineItem.StartPoint.Guid] : new Point();
                                        lineItem.EndPoint = (points.ContainsKey(lineItem.EndPoint.Guid)) ? points[lineItem.EndPoint.Guid] : new Point();
                                        lineList.Add(lineItem);
                                    }
                                    visInfo.Lines = lineList;
                                }

                                var clippingPlaneFound = from plane in clippingPlanes.Values where plane.ViewPointGuid == viewPointGuid select plane;
                                if (clippingPlaneFound.Any())
                                {
                                    var planeList = new List<ClippingPlane>();
                                    foreach (var plane in clippingPlaneFound)
                                    {
                                        var planeItem = plane;
                                        planeItem.Location = (points.ContainsKey(planeItem.Location.Guid)) ? points[planeItem.Location.Guid] : new Point();
                                        planeItem.Direction = (directions.ContainsKey(planeItem.Direction.Guid)) ? directions[planeItem.Direction.Guid] : new Direction();
                                        planeList.Add(planeItem);
                                    }
                                    visInfo.ClippingPlanes = planeList;
                                }

                                var bitmapFound = from bitmap in bitmaps.Values where bitmap.ViewPointGuid == viewPointGuid select bitmap;
                                if (bitmapFound.Any())
                                {
                                    var bitmapList = new List<VisualizationInfoBitmaps>();
                                    foreach (var bitmap in bitmapFound)
                                    {
                                        var bitmapItem = bitmap;
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
                    ProgressManager.FinalizeProgress();
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
            var bcfFiles = new Dictionary<string, BCFZIP>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM BCFFileInfo";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var bcfZip = new BCFZIP();
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
            var bimSnippets = new Dictionary<string, BimSnippet>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM BimSnippet";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var bimSnippet = new BimSnippet();
                            bimSnippet.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            bimSnippet.SnippetType = reader.GetString(reader.GetOrdinal("SnippetType"));
                            bimSnippet.isExternal = reader.GetBoolean(reader.GetOrdinal("isExternal"));
                            bimSnippet.Reference = reader.GetString(reader.GetOrdinal("Reference"));
                            bimSnippet.ReferenceSchema = reader.GetString(reader.GetOrdinal("ReferenceSchema"));
                            if (reader["FileContent"] != DBNull.Value)
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
            var bitmaps = new Dictionary<string, VisualizationInfoBitmaps>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Bitmaps";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var bitmap = new VisualizationInfoBitmaps();
                            bitmap.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            bitmap.Bitmap = (BitmapFormat)Enum.Parse(typeof(BitmapFormat), reader.GetString(reader.GetOrdinal("Bitmap")));
                            if (reader["Bitmap_Image"] != DBNull.Value)
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
            var clippingPlanes = new Dictionary<string, ClippingPlane>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM ClippingPlane";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var cp = new ClippingPlane();
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
            var comments = new Dictionary<string, Comment>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Comment";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var comment = new Comment();
                            comment.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            comment.VerbalStatus = reader.GetString(reader.GetOrdinal("VerbalStatus"));
                            comment.Status = reader.GetString(reader.GetOrdinal("Status"));
                            if (reader["Date"] != DBNull.Value)
                            {
                                comment.Date = reader.GetDateTime(reader.GetOrdinal("Date"));
                            }

                            comment.Author = reader.GetString(reader.GetOrdinal("Author"));
                            comment.Comment1 = reader.GetString(reader.GetOrdinal("Comment"));

                            if (reader["ModifiedDate"] != DBNull.Value)
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
            var components = new Dictionary<string, Component>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Components";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var comp = new Component();
                            comp.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            comp.IfcGuid = reader.GetString(reader.GetOrdinal("IfcGuid"));
                            comp.Selected = reader.GetBoolean(reader.GetOrdinal("Selected"));
                            comp.Visible = reader.GetBoolean(reader.GetOrdinal("Visible"));
                            if (reader["Color"] != DBNull.Value)
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
            var directions = new Dictionary<string, Direction>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Direction";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var direction = new Direction();
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
            var docReferences = new Dictionary<string, TopicDocumentReferences>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM DocumentReferences";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var docRef = new TopicDocumentReferences();
                            docRef.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            docRef.isExternal = reader.GetBoolean(reader.GetOrdinal("isExternal"));
                            docRef.ReferencedDocument = reader.GetString(reader.GetOrdinal("ReferenceDocument"));
                            if (reader["FileContent"] != DBNull.Value)
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
            var headerFiles = new Dictionary<string, HeaderFile>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM HeaderFile";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var headerFile = new HeaderFile();
                            headerFile.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            headerFile.IfcProject = reader.GetString(reader.GetOrdinal("IfcProject"));
                            headerFile.IfcSpatialStructureElement = reader.GetString(reader.GetOrdinal("IfcSpatialStructureElement"));
                            headerFile.isExternal = reader.GetBoolean(reader.GetOrdinal("isExternal"));
                            headerFile.Filename = reader.GetString(reader.GetOrdinal("FileName"));

                            if (reader["Date"] != DBNull.Value)
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
            var labels = new Dictionary<string, List<string>>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Labels";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var label = reader.GetString(reader.GetOrdinal("Label"));
                            var topicGuid = reader.GetString(reader.GetOrdinal("Topic_Guid"));

                            if (labels.ContainsKey(topicGuid))
                            {
                                labels[topicGuid].Add(label);
                            }
                            else
                            {
                                var labelList = new List<string>();
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
            var lines = new Dictionary<string, Line>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Lines";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var line = new Line();
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
            var orthoCameras = new Dictionary<string, OrthogonalCamera>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM OrthogonalCamera";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var orthoCamera = new OrthogonalCamera();
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
            var persCameras = new Dictionary<string, PerspectiveCamera>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM PerspectiveCamera";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var persCamera = new PerspectiveCamera();
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
            var points = new Dictionary<string, Point>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Point";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var point = new Point();
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
            var projects = new Dictionary<string, Project>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Project";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var project = new Project
                            {
                                Guid = reader.GetString(reader.GetOrdinal("Guid")),
                                ProjectId = reader.GetString(reader.GetOrdinal("ProjectId")),
                                Name = reader.GetString(reader.GetOrdinal("ProjectName")),
                                ExtensionGuid = reader.GetString(reader.GetOrdinal("ProjectExtension_Guid"))
                            };

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
            var projectExts = new Dictionary<string, ProjectExtension>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM ProjectExtension";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var projectExt = new ProjectExtension();
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
            var relTopics = new Dictionary<string, TopicRelatedTopics>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM RelatedTopics";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var relTopic = new TopicRelatedTopics();
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
            var extensions = new Dictionary<string, RevitExtension>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM RevitExtensions";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var extension = new RevitExtension
                            {
                                Guid = reader.GetString(reader.GetOrdinal("Guid")),
                                ParameterName = reader.GetString(reader.GetOrdinal("ParameterName")),
                                ParameterValue = reader.GetString(reader.GetOrdinal("ParameterValue"))
                            };
                            if (reader["Color"] != DBNull.Value)
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
            var topics = new Dictionary<string, Topic>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Topic";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var topic = new Topic();
                            topic.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            var topicType = reader.GetString(reader.GetOrdinal("TopicType"));
                            topic.TopicType = (string.IsNullOrEmpty(topicType)) ? TopicType.Unknown : (TopicType)Enum.Parse(typeof(TopicType), topicType);
                            var topicStatus = reader.GetString(reader.GetOrdinal("TopicStatus"));
                            topic.TopicStatus = (string.IsNullOrEmpty(topicStatus)) ? TopicStatus.Open : (TopicStatus)Enum.Parse(typeof(TopicStatus), topicStatus);
                            topic.Title = reader.GetString(reader.GetOrdinal("Title"));
                            topic.ReferenceLink = reader.GetString(reader.GetOrdinal("ReferenceLink"));
                            topic.Description = reader.GetString(reader.GetOrdinal("Description"));
                            topic.Priority = reader.GetString(reader.GetOrdinal("Priority"));
                            topic.Index = reader.GetInt32(reader.GetOrdinal("TopicIndex"));

                            if (reader["CreationDate"] != DBNull.Value)
                            {
                                topic.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
                            }
                            topic.CreationAuthor = reader.GetString(reader.GetOrdinal("CreationAuthor"));

                            if (reader["ModifiedDate"] != DBNull.Value)
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
            var versions = new Dictionary<string, Version>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Version";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var version = new Version
                            {
                                Guid = reader.GetString(reader.GetOrdinal("Guid")),
                                VersionId = reader.GetString(reader.GetOrdinal("VersionId")),
                                DetailedVersion = reader.GetString(reader.GetOrdinal("DetailedVersion")),
                                FileId = reader.GetString(reader.GetOrdinal("File_Guid"))
                            };

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
            var viewpoints = new Dictionary<string, ViewPoint>();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Viewpoints";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var vp = new ViewPoint();
                            vp.Guid = reader.GetString(reader.GetOrdinal("Guid"));
                            vp.Viewpoint = reader.GetString(reader.GetOrdinal("Viewpoint"));
                            vp.Snapshot = reader.GetString(reader.GetOrdinal("Snapshot"));
                            if (reader["Snapshot_Image"] != DBNull.Value)
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
