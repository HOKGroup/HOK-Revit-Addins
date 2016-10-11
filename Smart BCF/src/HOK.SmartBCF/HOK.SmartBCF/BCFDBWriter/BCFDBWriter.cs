using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.Utils;
using Version = HOK.SmartBCF.Schemas.Version;

namespace HOK.SmartBCF.BCFDBWriter
{
    public static class BCFDBWriter
    {
        private static SQLiteConnection sqlConnection = null;
        private static string databaseFile = "";

        public static bool ConnectDatabase(string dbFile)
        {
            bool connected = false;
            try
            {
                databaseFile = dbFile;
                if (null != sqlConnection)
                {
                    if (sqlConnection.State == ConnectionState.Open)
                    {
                        sqlConnection.Close();
                    }
                }

                string connectionString = GetConnectionString(dbFile);
                sqlConnection = new SQLiteConnection(connectionString, true);
                sqlConnection.Open();

                while (sqlConnection.State != ConnectionState.Connecting)
                {
                    switch (sqlConnection.State)
                    {
                        case ConnectionState.Open:
                            return true;
                        case ConnectionState.Broken:
                            return false;
                        case ConnectionState.Closed:
                            return false;
                        case ConnectionState.Executing:
                            break;
                        case ConnectionState.Fetching:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return connected;
        }

        public static bool CloseConnection()
        {
            bool closed = false;
            try
            {
                if (null != sqlConnection)
                {
                    if (sqlConnection.State == ConnectionState.Open)
                    {
                        sqlConnection.Close();
                        sqlConnection = null;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return closed;
        }

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

        public static bool CreateTables()
        {
            bool created = false;
            try
            {
                if (!File.Exists(databaseFile))
                {
                    SQLiteConnection.CreateFile(databaseFile);
                }

                if (File.Exists(databaseFile))
                {
                    using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
                    {
                        try
                        {
                            using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                            {
                                ResourceManager rManager = Properties.Resources.ResourceManager;
                                var tableOrders = ((BCFTables[])Enum.GetValues(typeof(BCFTables))).OrderBy(x => x);

                                ProgressManager.InitializeProgress("Creating Database tables..", tableOrders.Count());
                                ProgressManager.databaseFilePath = databaseFile;
                                foreach (BCFTables tName in tableOrders)
                                {
                                    ProgressManager.StepForward();
                                    cmd.CommandText = rManager.GetString(tName.ToString());
                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        string message = tName.ToString() + "\n" + ex.Message;
                                    }
                                }
                                ProgressManager.FinalizeProgress();
                            }
                            trans.Commit();
                            created = true;
                        }
                        catch (SQLiteException ex)
                        {
                            trans.Rollback();
                            string message = ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a database file.\n" + ex.Message, "Create Tables", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        public static bool WriteDatabase(BCFZIP bcfzip, ConflictMode mode)
        {
            bool result = false;
            try
            {
                ProgressManager.InitializeProgress("Writing Database..", 5);
                ProgressManager.databaseFilePath = databaseFile;
                bool fileInfoValuesInserted = InsertBCFFileInfo(bcfzip, mode); ProgressManager.StepForward();
                bool projectValuesInserted = InsertProjectValues(bcfzip, mode); ProgressManager.StepForward();
                bool versionValuesInserted = InsertVersionValues(bcfzip, mode); ProgressManager.StepForward();
                bool extensionValuesInserted = InsertExtensionValues(bcfzip); ProgressManager.StepForward();
                bool markupValuesInserted = InsertMarkupValues(bcfzip, mode); ProgressManager.StepForward();
                result = (fileInfoValuesInserted && projectValuesInserted && versionValuesInserted  && markupValuesInserted);
                ProgressManager.FinalizeProgress();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Failed to write database.\n" + ex.Message, "Write Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static BCFZIP MergeDatabase(BCFZIP bcfzip, BCFZIP oldBCF)
        {
            BCFZIP mergedBCF = null;
            try
            {
                if (null == sqlConnection) { return null; }

                mergedBCF = oldBCF;
                using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            for (int i = 0; i < bcfzip.Markups.Count; i++)
                            {
                                Markup markupToAdd = bcfzip.Markups[i];
                                var existingMarkups = from markup in oldBCF.Markups where markup.Topic.Guid == markup.Topic.Guid select markup;
                                if (existingMarkups.Count() > 0)
                                {
                                    Markup existingMarkup = existingMarkups.First();
                                    int index = oldBCF.Markups.IndexOf(existingMarkup);

                                    //Insert Comments
                                    cmd.Parameters.Add("@date", DbType.DateTime);
                                    cmd.Parameters.Add("@modifiedDate", DbType.DateTime);
                                    cmd.Parameters.Add("@comment", DbType.String);

                                    foreach (Comment comment in markupToAdd.Comment)
                                    {
                                        var existingComments = from exComment in existingMarkup.Comment where exComment.Guid == comment.Guid select exComment;
                                        if (existingComments.Count() == 0)
                                        {
                                            bool insertedComment = InsertCommentValue(existingMarkup.Topic.Guid, comment, cmd, ConflictMode.IGNORE);
                                            mergedBCF.Markups[index].Comment.Add(comment);
                                        }
                                    }
                                    cmd.Parameters.Clear();
                                }
                                else
                                {
                                    //Insert Markup
                                    bool insertedMarkup = InsertMarkupValue(oldBCF.FileId, markupToAdd, cmd, ConflictMode.IGNORE);
                                    if (insertedMarkup)
                                    {
                                        mergedBCF.Markups.Add(markupToAdd);
                                    }
                                }
                            }
                        }
                        trans.Commit();
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
                MessageBox.Show("Failed to merge BCF information.\n"+ex.Message, "Merge Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return mergedBCF;
        }

        private static bool InsertBCFFileInfo(BCFZIP bcfzip, ConflictMode mode)
        {
            bool inserted = false;
            try
            {
                using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO BCFFileInfo (Guid, FileName, FilePath, UploadedBy, UploadedDate, CreationDate, IsPrimary) VALUES " +
                                "('" + bcfzip.FileId + "', '" + bcfzip.ZipFileName + "', '" + bcfzip.ZipFilePath + "', '" + bcfzip.UploadedBy + "', @uploadedDate, @creationDate, @isPrimary)";

                            cmd.Parameters.AddWithValue("@uploadedDate", bcfzip.UploadedDate);
                            cmd.Parameters.AddWithValue("@creationDate", bcfzip.CreationDate);
                            cmd.Parameters.AddWithValue("@isPrimary", bcfzip.IsPrimary);

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

        private static bool InsertProjectValues(BCFZIP bcfZip, ConflictMode mode)
        {
            bool inserted = false;
            try
            {
                ProjectExtension pExt = bcfZip.ProjectFile;
                if (null != pExt)
                {
                    using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
                    {
                        try
                        {
                            using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                            {
                                cmd.Transaction = trans;

                                cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO ProjectExtension (Guid, ExtensionSchema, File_Guid) VALUES " +
                                    "('" + pExt.Guid + "', '" + pExt.ExtensionSchema + "', '" + bcfZip.FileId + "')";

                                inserted = (cmd.ExecuteNonQuery() > 0) ? true : false;

                                cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Project (Guid, ProjectId, ProjectName, ProjectExtension_Guid) VALUES " +
                                    "('" + pExt.Project.Guid + "', '" + pExt.Project.ProjectId + "', '" + pExt.Project.Name + "', '" + pExt.Guid + "')";

                                inserted = (cmd.ExecuteNonQuery() > 0) ? true : false;

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
                else
                {
                    inserted = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert project tables values.\n" + ex.Message, "Insert Project Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertVersionValues(BCFZIP bcfZip, ConflictMode mode)
        {
            bool inserted = false;
            try
            {
                Version version = bcfZip.VersionFile;

                if (null != version)
                {
                    using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
                    {
                        try
                        {
                            using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                            {
                                cmd.Transaction = trans;

                                StringBuilder strBuilder = new StringBuilder();
                                if (!string.IsNullOrEmpty(version.VersionId))
                                {
                                    cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Version (Guid, VersionId, DetailedVersion, File_Guid) VALUES " +
                                        "('" + version.Guid + "', '" + version.VersionId + "', '" + version.DetailedVersion + "', '" + bcfZip.FileId + "')";

                                    inserted = (cmd.ExecuteNonQuery() > 0) ? true : false;
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
                else
                {
                    inserted = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert Version tables values.\n" + ex.Message, "Insert Version Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static bool InsertMarkupValues(BCFZIP bcfZip, ConflictMode mode)
        {
            bool inserted = false;
            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        foreach (Markup markup in bcfZip.Markups)
                        {
                            bool markupInserted = InsertMarkupValue(bcfZip.FileId, markup, cmd, mode);
                        }
                    }
                    trans.Commit();
                    inserted = true;
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to insert markup values.\n" + ex.Message, "Insert Markup", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return inserted;
        }

        private static bool InsertMarkupValue(string fileId, Markup markup, SQLiteCommand cmd, ConflictMode mode)
        {
            bool inserted = false;
            try
            {
                #region Topic
                Topic topic = markup.Topic;
                cmd.CommandText = "INSERT OR " + mode.ToString() + " INTO Topic (Guid, TopicType, TopicStatus, Title, ReferenceLink, Description, Priority, TopicIndex, CreationDate, CreationAuthor, ModifiedDate, ModifiedAuthor, AssignedTo, File_Guid) VALUES "
                    + "('" + topic.Guid + "', '" + topic.TopicType + "', '" + topic.TopicStatus + "', @title, '" + topic.ReferenceLink + "', @description, '" + topic.Priority + "', " + topic.Index + ", " +
                "@creationDate, '" + topic.CreationAuthor + "', @modifiedDate, '" + topic.ModifiedAuthor + "', '" + topic.AssignedTo + "', '" + fileId + "')";

                cmd.Parameters.AddWithValue("@title", topic.Title);
                cmd.Parameters.AddWithValue("@description", topic.Description);
                cmd.Parameters.AddWithValue("@creationDate", topic.CreationDate);
                cmd.Parameters.AddWithValue("@modifiedDate", topic.ModifiedDate);

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

                #region Labels
                if (topic.Labels.Count > 0)
                {
                    foreach (string label in topic.Labels)
                    {
                        cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Labels (Label, Topic_Guid) VALUES " +
                            "( @label, '" + topic.Guid + "')";

                        cmd.Parameters.AddWithValue("@label", label);
                        try
                        {
                            int insertedRows = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "/n" + ex.Message;
                        }
                    }
                }
                #endregion

                #region HeaderFile
                List<HeaderFile> headerFiles = markup.Header;
                if (headerFiles.Count > 0)
                {
                    cmd.Parameters.Add("@isExternal", DbType.Boolean);
                    cmd.Parameters.Add("@date", DbType.DateTime);

                    foreach (HeaderFile file in headerFiles)
                    {
                        cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO HeaderFile (Guid, IfcProject, IfcSpatialStructureElement, isExternal, FileName, Date, Reference, Topic_Guid) VALUES " +
                            "('" + file.Guid + "', '" + file.IfcProject + "', '" + file.IfcSpatialStructureElement + "', @isExternal, '" + file.Filename + "', @date, '" + file.Reference + "', '" + topic.Guid + "')";

                        cmd.Parameters["@isExternal"].Value = file.isExternal;
                        cmd.Parameters["@date"].Value = file.Date;

                        try
                        {
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

                #region BimSnippet
                BimSnippet bimSnippet = topic.BimSnippet;
                if (!string.IsNullOrEmpty(bimSnippet.Reference))
                {
                    cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO BimSnippet (Guid, SnippetType, isExternal, Reference, ReferenceSchema, FileContent, Topic_Guid) VALUES " +
                        "('" + bimSnippet.Guid + "', '" + bimSnippet.SnippetType + "', @isExternal, '" + bimSnippet.Reference + "', '" + bimSnippet.ReferenceSchema + "', @fileContent, '" + topic.Guid + "')";

                    cmd.Parameters.AddWithValue("@isExternal", bimSnippet.isExternal);
                    cmd.Parameters.AddWithValue("@fileContent", bimSnippet.FileContent);

                    try
                    {
                        int insertedRow = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    catch (SQLiteException ex)
                    {
                        string message = cmd.CommandText + "/n" + ex.Message;
                    }
                }
                #endregion

                #region DocumentReferences
                List<TopicDocumentReferences> docReferences = topic.DocumentReferences;
                if (docReferences.Count > 0)
                {
                    cmd.Parameters.Add("@isExternal", DbType.Boolean);
                    cmd.Parameters.Add("@fileContent", DbType.Binary);

                    foreach (TopicDocumentReferences docRef in docReferences)
                    {
                        cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO DocumentReferences (Guid, isExternal, ReferenceDocument, FileContent, Description, Topic_Guid) VALUES "
                            + "('" + docRef.Guid + "', @isExternal, '" + docRef.ReferencedDocument + "', @fileContent, '" + docRef.Description + "', '" + topic.Guid + "')";

                        cmd.Parameters["@isExternal"].Value = docRef.isExternal;
                        cmd.Parameters["@fileContent"].Value = docRef.FileContent;

                        try
                        {
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
                    foreach (TopicRelatedTopics relTopic in relatedTopics)
                    {
                        cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO RelatedTopics (Guid, Topic_Guid) VALUES " +
                            "('" + relTopic.Guid + "', '" + topic.Guid + "')";

                        try
                        {
                            int insertedRows = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "/n" + ex.Message;
                        }
                    }
                }
                #endregion

                #region Viewpoints
                ObservableCollection<ViewPoint> viewpoints = markup.Viewpoints;
                if (viewpoints.Count > 0)
                {
                    cmd.Parameters.Add("@snapshotImage", DbType.Binary);

                    foreach (ViewPoint viewPoint in viewpoints)
                    {
                        cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Viewpoints (Guid, Viewpoint, Snapshot, Snapshot_Image, Topic_Guid) VALUES " +
                            "('" + viewPoint.Guid + "', '" + viewPoint.Viewpoint + "', '" + viewPoint.Snapshot + "',  @snapshotImage, '" + topic.Guid + "')";

                        cmd.Parameters["@snapshotImage"].Value = viewPoint.SnapshotImage;

                        try
                        {
                            int insertedRows = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "/n" + ex.Message;
                        }

                        bool insertedVisInfo = InsertVisInfoValues(fileId, viewPoint, cmd, mode);
                    }
                    cmd.Parameters.Clear();
                }
                #endregion

                #region Comment
                ObservableCollection<Comment> comments = markup.Comment;
                if (comments.Count > 0)
                {
                    cmd.Parameters.Add("@date", DbType.DateTime);
                    cmd.Parameters.Add("@modifiedDate", DbType.DateTime);
                    cmd.Parameters.Add("@comment", DbType.String);

                    foreach (Comment comment in comments)
                    {
                        bool insertedComment = InsertCommentValue(topic.Guid, comment, cmd, mode);
                    }
                    cmd.Parameters.Clear();
                }
                #endregion
                inserted = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        private static bool InsertCommentValue(string topicId, Comment comment, SQLiteCommand cmd, ConflictMode mode)
        {
            bool inserted = false;
            try
            {
                cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Comment (Guid, VerbalStatus, Status, Date, Author, Comment, ModifiedDate, ModifiedAuthor, Topic_Guid, Viewpoint_Guid) VALUES " +
                            "('" + comment.Guid + "', '" + comment.VerbalStatus + "', '" + comment.Status + "', @date, '" + comment.Author + "', @comment, @modifiedDate, '" + comment.ModifiedAuthor + "', '" + topicId + "', '" + comment.Viewpoint.Guid + "')";

                cmd.Parameters["@comment"].Value = comment.Comment1;
                cmd.Parameters["@date"].Value = comment.Date;
                cmd.Parameters["@modifiedDate"].Value = comment.ModifiedDate;

                try
                {
                    inserted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                }
                catch (SQLiteException ex)
                {
                    string message = cmd.CommandText + "/n" + ex.Message;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        private static bool InsertExtensionValues(BCFZIP bcfZip)
        {
            bool inserted = false;
            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        cmd.Parameters.Add("@color", DbType.Binary);
                        RevitExtensionInfo extColor = bcfZip.ExtensionColor;
                        foreach (RevitExtension extension in extColor.Extensions)
                        {
                            try
                            {
                                cmd.CommandText = @"INSERT OR IGNORE INTO RevitExtensions (Guid, ParameterName, ParameterValue, Color) VALUES "
                                                   + "('" + extension.Guid + "', '" + extension.ParameterName + "', '" + extension.ParameterValue + "', @color)";
                                cmd.Parameters["@color"].Value = extension.Color;

                                inserted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                            }
                            catch (SQLiteException ex)
                            {
                                string message = ex.Message;
                            }
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to insert extension values.\n" + ex.Message, "Insert Extension Values", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return inserted;
        }

        private static bool InsertVisInfoValues(String fileId, ViewPoint viewPoint, SQLiteCommand cmd, ConflictMode mode)
        {
            bool inserted = false;
            try
            {
                string viewpoint_guid = viewPoint.Guid;
                if (null != viewPoint.VisInfo)
                {
                    VisualizationInfo visInfo = viewPoint.VisInfo;

                    #region Bitmaps
                    List<VisualizationInfoBitmaps> bitMaps = visInfo.Bitmaps;
                    if (bitMaps.Count > 0)
                    {
                        cmd.Parameters.Add("@bitmapImage", DbType.Binary);
                        cmd.Parameters.Add("@height", DbType.Double);

                        foreach (VisualizationInfoBitmaps bitmap in bitMaps)
                        {
                            string locationGuid = (null != bitmap.Location) ? bitmap.Location.Guid : "";
                            string normalGuid = (null != bitmap.Normal) ? bitmap.Normal.Guid : "";
                            string upGuid = (null != bitmap.Up) ? bitmap.Up.Guid : "";

                            cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Bitmaps (Guid, Bitmap, Bitmap_Image, Reference, Location, Normal, Up, Height, Viewpoints_Guid) VALUES " +
                                "('" + bitmap.Guid + "', '" + bitmap.Bitmap.ToString() + "', @bitmapImage, '" + bitmap.Reference + "', '" + locationGuid + "', '" + normalGuid + "', '" + upGuid + "', @height, '" + viewpoint_guid + "')";

                            cmd.Parameters["@bitmapImage"].Value = bitmap.BitmapImage;
                            cmd.Parameters["@height"].Value = bitmap.Height;

                            try
                            {
                                int insertedRows = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }

                            if (null != bitmap.Location)
                            {
                                cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                               "('" + bitmap.Location.Guid + "', " + bitmap.Location.X + ", " + bitmap.Location.Y + ", " + bitmap.Location.Z + ", '"+viewpoint_guid+"', '" + fileId + "')";

                                try
                                {
                                    int insertedRows = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "\n" + ex.Message;
                                }
                            }

                            if (null != bitmap.Normal && null != bitmap.Up)
                            {
                                cmd.CommandText = "INSERT OR " + mode.ToString() + " INTO Direction (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                                "('" + bitmap.Normal.Guid + "', " + bitmap.Normal.X + ", " + bitmap.Normal.Y + ", " + bitmap.Normal.Z + ", '" + viewpoint_guid + "', '" + fileId + "'), " +
                                "('" + bitmap.Up.Guid + "', " + bitmap.Up.X + ", " + bitmap.Up.Y + ", " + bitmap.Up.Z + ", '" +viewpoint_guid+"', '"+ fileId + "')";

                                try
                                {
                                    int insertedRows = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "\n" + ex.Message;
                                }
                            }
                        }
                        cmd.Parameters.Clear();
                    }

                    #endregion

                    #region Components
                    ObservableCollection<Component> components = visInfo.Components;
                    if (components.Count > 0)
                    {

                        cmd.Parameters.Add("@selected", DbType.Boolean);
                        cmd.Parameters.Add("@visible", DbType.Boolean);
                        cmd.Parameters.Add("@color", DbType.Binary);
                        cmd.Parameters.Add("@elementName", DbType.String);

                        foreach (Component comp in components)
                        {
                            cmd.CommandText = "INSERT OR " + mode.ToString() + " INTO Components (Guid, IfcGuid, Selected, Visible, Color, OriginatingSystem, AuthoringToolId, Viewpoints_Guid, ElementName, Action, Responsibility) VALUES " +
                                "('" + comp.Guid + "', '" + comp.IfcGuid + "', @selected, @visible, @color, '" + comp.OriginatingSystem + "', '" + comp.AuthoringToolId + "', '" + viewpoint_guid + "', @elementName, '" + comp.Action.Guid + "', '" + comp.Responsibility.Guid + "')";

                            cmd.Parameters["@selected"].Value = comp.Selected;
                            cmd.Parameters["@visible"].Value = comp.Visible;
                            cmd.Parameters["@color"].Value = comp.Color;
                            cmd.Parameters["@elementName"].Value = comp.ElementName;

                            try
                            {
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

                        foreach (ClippingPlane plane in clippingPlanes)
                        {
                            string locationGuid = (null != plane.Location) ? plane.Location.Guid : "";
                            string directionGuid = (null != plane.Direction) ? plane.Direction.Guid : "";

                            cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO ClippingPlane (Guid, Location, Direction, Viewpoints_Guid) VALUES " +
                                "('" + plane.Guid + "', '" + locationGuid + "', '" + directionGuid + "', '" + viewpoint_guid + "')";

                            try
                            {
                                int insertedRows = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }

                            if (null != plane.Location)
                            {
                                cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                                "('" + plane.Location.Guid + "', " + plane.Location.X + ", " + plane.Location.Y + ", " + plane.Location.Z + ", '"+viewpoint_guid+"', '" + fileId + "')";

                                try
                                {
                                    int insertedRows = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "\n" + ex.Message;
                                }
                            }

                            if (null != plane.Direction)
                            {
                                cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Direction (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                                "('" + plane.Direction.Guid + "', " + plane.Direction.X + ", " + plane.Direction.Y + ", " + plane.Direction.Z + ", '"+viewpoint_guid+"', '" + fileId + "')";

                                try
                                {
                                    int insertedRows = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "\n" + ex.Message;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Lines
                    List<Line> lines = visInfo.Lines;
                    if (lines.Count > 0)
                    {
                        foreach (Line line in lines)
                        {
                            if (null != line.StartPoint && null != line.EndPoint)
                            {
                                cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Lines (Guid, StartPoint, EndPoint, Viewpoints_Guid) VALUES "
                                               + "('" + line.Guid + "', '" + line.StartPoint.Guid + "', '" + line.EndPoint.Guid + "', '" + viewpoint_guid + "')";

                                try
                                {
                                    int insertedRows = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "\n" + ex.Message;
                                }

                                cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                                    "('" + line.StartPoint.Guid + "', " + line.StartPoint.X + ", " + line.StartPoint.Y + ", " + line.StartPoint.Z + ",'" + viewpoint_guid + "', '" + fileId + "'), " +
                                    "('" + line.EndPoint.Guid + "', " + line.EndPoint.X + ", " + line.EndPoint.Y + ", " + line.EndPoint.Z + ", '"+ viewpoint_guid+"', '" + fileId + "')";

                                try
                                {
                                    int insertedRows = cmd.ExecuteNonQuery();
                                }
                                catch (SQLiteException ex)
                                {
                                    string message = cmd.CommandText + "\n" + ex.Message;
                                }
                            }
                        }
                    }
                    #endregion

                    #region OrthogonalCamera
                    OrthogonalCamera orthoCamera = visInfo.OrthogonalCamera;
                    if (orthoCamera.ViewToWorldScale != 0)
                    {
                        string viewpointGuid = (null != orthoCamera.CameraViewPoint) ? orthoCamera.CameraViewPoint.Guid : "";
                        string directionGuid = (null != orthoCamera.CameraDirection) ? orthoCamera.CameraDirection.Guid : "";
                        string upVectorGuid = (null != orthoCamera.CameraUpVector) ? orthoCamera.CameraUpVector.Guid : "";

                        cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO OrthogonalCamera (Guid, CameraViewPoint, CameraDirection, CameraUpVector, ViewToWorldScale, Viewpoints_Guid) VALUES " +
                            "('" + orthoCamera.Guid + "', '" + viewpointGuid + "', '" + directionGuid + "', '" + upVectorGuid + "', @scale, '" + viewpoint_guid + "')";

                        cmd.Parameters.AddWithValue("@scale", orthoCamera.ViewToWorldScale);
                        try
                        {
                            int insertedRows = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "\n" + ex.Message;
                        }

                        if (null != orthoCamera.CameraViewPoint)
                        {
                            cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                            "('" + orthoCamera.CameraViewPoint.Guid + "', " + orthoCamera.CameraViewPoint.X + ", " + orthoCamera.CameraViewPoint.Y + ", " + orthoCamera.CameraViewPoint.Z + ", '"+viewpoint_guid+"', '" + fileId + "')";

                            try
                            {
                                int insertedRows = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }
                        }

                        if (null != orthoCamera.CameraDirection && null != orthoCamera.CameraUpVector)
                        {
                            cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Direction (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                            "('" + orthoCamera.CameraDirection.Guid + "', " + orthoCamera.CameraDirection.X + ", " + orthoCamera.CameraDirection.Y + ", " + orthoCamera.CameraDirection.Z + ", '" +viewpoint_guid+"', '"+ fileId + "'), " +
                            "('" + orthoCamera.CameraUpVector.Guid + "', " + orthoCamera.CameraUpVector.X + ", " + orthoCamera.CameraUpVector.Y + ", " + orthoCamera.CameraUpVector.Z + ", '"+viewpoint_guid+"', '" + fileId + "')";

                            try
                            {
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

                    #region PerspectiveCamera
                    PerspectiveCamera persCamera = visInfo.PerspectiveCamera;
                    if (persCamera.FieldOfView != 0)
                    {
                        string viewPointGuid = (null != persCamera.CameraViewPoint) ? persCamera.CameraViewPoint.Guid : "";
                        string directionGuid = (null != persCamera.CameraDirection) ? persCamera.CameraDirection.Guid : "";
                        string upVectorGuid = (null != persCamera.CameraUpVector) ? persCamera.CameraUpVector.Guid : "";

                        cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO PerspectiveCamera (Guid, CameraViewPoint, CameraDirection, CameraUpVector, FieldOfView, Viewpoints_Guid) VALUES " +
                            "('" + persCamera.Guid + "', '" + viewPointGuid + "', '" + directionGuid + "', '" + upVectorGuid + "', @fieldOfView, '" + viewpoint_guid + "')";

                        cmd.Parameters.AddWithValue("@fieldOfView", persCamera.FieldOfView);
                        try
                        {
                            int insertedRows = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "\n" + ex.Message;
                        }

                        if (null != persCamera.CameraViewPoint)
                        {
                            cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Point (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                            "('" + persCamera.CameraViewPoint.Guid + "', " + persCamera.CameraViewPoint.X + ", " + persCamera.CameraViewPoint.Y + ", " + persCamera.CameraViewPoint.Z + ", '"+viewpoint_guid+"', '" + fileId + "')";

                            try
                            {
                                int insertedRows = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }
                        }

                        if (null != persCamera.CameraDirection && null != persCamera.CameraUpVector)
                        {
                            cmd.CommandText = @"INSERT OR " + mode.ToString() + " INTO Direction (Guid, X, Y, Z, Viewpoints_Guid, File_Guid) VALUES " +
                            "('" + persCamera.CameraDirection.Guid + "', " + persCamera.CameraDirection.X + ", " + persCamera.CameraDirection.Y + ", " + persCamera.CameraDirection.Z + ", '"+viewpoint_guid+"', '" + fileId + "'), " +
                            "('" + persCamera.CameraUpVector.Guid + "', " + persCamera.CameraUpVector.X + ", " + persCamera.CameraUpVector.Y + ", " + persCamera.CameraUpVector.Z + ", '" + viewpoint_guid + "', '" + fileId + "')";

                            try
                            {
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
                    inserted = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert Visualization Information tables values.\n" + ex.Message, "Insert Visualization Information Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        public static bool DeleteBCF(BCFZIP bcfzip)
        {
            bool deleted = false;
            try
            {
                if (null == sqlConnection) { return false; }

                using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            //Delete Markup

                            ProgressManager.InitializeProgress("Deleting existing BCF info.. ", 7);

                            foreach (Markup markup in bcfzip.Markups)
                            {
                                bool deletedMarkup = DeleteMarkup(markup, cmd);
                            }
                            ProgressManager.StepForward();
                            //Project Info
                            try
                            {
                                cmd.CommandText = "DELETE FROM Project WHERE Guid ='" + bcfzip.ProjectFile.Project.Guid + "'";
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                            }
                            ProgressManager.StepForward();

                            try
                            {
                                cmd.CommandText = "DELETE FROM ProjectExtension WHERE Guid ='" + bcfzip.ProjectFile.Guid + "'";
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                            }
                            ProgressManager.StepForward();

                            //Version
                            try
                            {
                                cmd.CommandText = "DELETE FROM Version WHERE Guid ='" + bcfzip.VersionFile.Guid + "'";
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                            }
                            ProgressManager.StepForward();

                            //Point
                            try
                            {
                                cmd.CommandText = "DELETE FROM Point WHERE File_Guid ='" + bcfzip.FileId + "'";
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                            }
                            ProgressManager.StepForward();

                            //Direction
                            try
                            {
                                cmd.CommandText = "DELETE FROM Direction WHERE File_Guid ='" + bcfzip.FileId + "'";
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                            }
                            ProgressManager.StepForward();

                            try
                            {
                                cmd.CommandText = "DELETE FROM BCFFileInfo WHERE Guid ='" + bcfzip.FileId + "'";
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                            }
                            ProgressManager.StepForward();
                            ProgressManager.FinalizeProgress();
                        }
                        trans.Commit();
                        deleted = true;
                    }
                    catch (SQLiteException ex)
                    {
                        trans.Rollback();
                        string message = ex.Message;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Failed to delete the selected BCF from the database.\n" + ex.Message, "Delete Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deleted;
        }

        private static bool DeleteMarkup(Markup markup, SQLiteCommand cmd)
        {
            bool deleted = false;
            try
            {
                string topicId = markup.Topic.Guid;
                //HeaderFile
                try
                {
                    cmd.CommandText = "DELETE FROM HeaderFile WHERE Topic_Guid ='" + topicId + "'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                //BimSnippet
                try
                {
                    cmd.CommandText = "DELETE FROM BimSnippet WHERE Topic_Guid ='" + topicId + "'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                //DocumentReferences
                try
                {
                    cmd.CommandText = "DELETE FROM DocumentReferences WHERE Topic_Guid ='" + topicId + "'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                //Comment
                try
                {
                    cmd.CommandText = "DELETE FROM Comment WHERE Topic_Guid ='" + topicId + "'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                //RelatedTopics
                try
                {
                    cmd.CommandText = "DELETE FROM RelatedTopics WHERE Topic_Guid ='" + topicId + "'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                //Labels
                try
                {
                    cmd.CommandText = "DELETE FROM Labels WHERE Topic_Guid ='" + topicId + "'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                //Delete Viewpoints
                foreach (ViewPoint vp in markup.Viewpoints)
                {
                    string viewpointId = vp.Guid;
                    //Components
                    try
                    {
                        cmd.CommandText = "DELETE FROM Components WHERE Viewpoints_Guid ='" + viewpointId + "'";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }

                    //Bitmaps
                    try
                    {
                        cmd.CommandText = "DELETE FROM Bitmaps WHERE Viewpoints_Guid ='" + viewpointId + "'";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }

                    //ClippingPlane
                    try
                    {
                        cmd.CommandText = "DELETE FROM ClippingPlane WHERE Viewpoints_Guid ='" + viewpointId + "'";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }

                    //Lines
                    try
                    {
                        cmd.CommandText = "DELETE FROM Lines WHERE Viewpoints_Guid ='" + viewpointId + "'";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }

                    //OrthogonalCamera
                    try
                    {
                        cmd.CommandText = "DELETE FROM OrthogonalCamera WHERE Viewpoints_Guid ='" + viewpointId + "'";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }

                    //PerspectiveCamera
                    try
                    {
                        cmd.CommandText = "DELETE FROM PerspectiveCamera WHERE Viewpoints_Guid ='" + viewpointId + "'";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }

                    //Viewpoint
                    try
                    {
                        cmd.CommandText = "DELETE FROM Viewpoints WHERE Guid ='" + viewpointId + "'";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }

                try
                {
                    cmd.CommandText = "DELETE FROM Topic WHERE Guid ='" + topicId + "'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                deleted = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return deleted;
        }

        public static bool UpdateTopic(Topic topic)
        {
            bool updated = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.CommandText = @"UPDATE Topic SET TopicType = '" + topic.TopicType + "', TopicStatus = '" + topic.TopicStatus + "' WHERE Guid = '" + topic.Guid + "'";
                            cmd.ExecuteNonQuery();
                            updated = true;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

        public static bool UpdateTopicAssign(Topic topic)
        {
            bool updated = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.CommandText = @"UPDATE Topic SET AssignedTo = '" + topic.AssignedTo + "' WHERE Guid = '" + topic.Guid + "'";
                            cmd.ExecuteNonQuery();
                            updated = true;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

        public static bool InsertComment(Comment comment)
        {
            bool inserted = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.CommandText = @"INSERT OR REPLACE INTO Comment (Guid, VerbalStatus, Status, Date, Author, Comment, ModifiedDate, ModifiedAuthor, Topic_Guid, Viewpoint_Guid) VALUES " +
                            "('" + comment.Guid + "', '" + comment.VerbalStatus + "', '" + comment.Status + "', @date, '" + comment.Author + "', @comment, @modifiedDate, '" + comment.ModifiedAuthor + "', '" + comment.TopicGuid + "', '" + comment.Viewpoint.Guid + "')";

                            cmd.Parameters.AddWithValue("@date", comment.Date);
                            cmd.Parameters.AddWithValue("@modifiedDate", comment.ModifiedDate);
                            cmd.Parameters.AddWithValue("@comment", comment.Comment1);

                            cmd.ExecuteNonQuery();
                            inserted = true;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return inserted;
        }

        public static bool UpdateComment(Comment comment)
        {
            bool updated = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.CommandText = @"UPDATE Comment SET Comment = @comment, ModifiedDate = @modifiedDate, ModifiedAuthor = @modifiedAuthor WHERE Guid = '" + comment.Guid + "'";

                            cmd.Parameters.AddWithValue("@date", comment.Date);
                            cmd.Parameters.AddWithValue("@modifiedDate", comment.ModifiedDate);
                            cmd.Parameters.AddWithValue("@modifiedAuthor", comment.ModifiedAuthor);
                            cmd.Parameters.AddWithValue("@comment", comment.Comment1);

                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            updated = true;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

        public static bool DeleteComment(Comment comment)
        {
            bool deleted = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.CommandText = "DELETE FROM Comment WHERE Guid ='" + comment.Guid + "'";

                            cmd.ExecuteNonQuery();
                            deleted = true;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return deleted;
        }

        public static bool UpdateComponent(Component component)
        {
            bool updated = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.CommandText = @"UPDATE Components SET Color = @color, Action = '" + component.Action.Guid + "', Responsibility = '" + component.Responsibility.Guid + "', ElementName =@elementName  WHERE Guid = '" + component.Guid + "'";
                            cmd.Parameters.AddWithValue("@color", component.Color);
                            cmd.Parameters.AddWithValue("@elementName", component.ElementName);

                            updated = (cmd.ExecuteNonQuery() > 0) ? true : false;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

        public static bool UpdateComponents(ObservableCollection<Component> components)
        {
            bool updated = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.Parameters.Add("@color", DbType.Binary);
                            cmd.Parameters.Add("@elementName", DbType.String);

                            foreach (Component comp in components)
                            {
                                cmd.CommandText = @"UPDATE Components SET Color = @color, Action = '" + comp.Action.Guid + "', Responsibility = '" + comp.Responsibility.Guid + "', ElementName =@elementName  WHERE Guid = '" + comp.Guid + "'";
                                cmd.Parameters["@color"].Value = comp.Action.Color;
                                cmd.Parameters["@elementName"].Value = comp.ElementName;
                                updated = (cmd.ExecuteNonQuery() > 0) ? true : false;
                            }
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

        public static bool UpdateExtensions(RevitExtensionInfo extInfo)
        {
            bool updated = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.CommandText = @"DELETE FROM RevitExtensions";
                            int deletedRows = cmd.ExecuteNonQuery();

                            int insertedRows = 0;
                            if (deletedRows > 0)
                            {
                                cmd.Parameters.Add("@color", DbType.Binary);
                                foreach (RevitExtension extension in extInfo.Extensions)
                                {
                                    cmd.CommandText = @"INSERT INTO RevitExtensions (Guid, ParameterName, ParameterValue, Color) VALUES "
                                                   + "('" + extension.Guid + "', '" + extension.ParameterName + "', '" + extension.ParameterValue + "', @color)";
                                    cmd.Parameters["@color"].Value = extension.Color;

                                    insertedRows += cmd.ExecuteNonQuery();
                                }
                                cmd.Parameters.Clear();
                                updated = (insertedRows == extInfo.Extensions.Count) ? true : false;
                            }
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

        public static bool ReplaceObsoleteExtensions(List<RevitExtension> extensions)
        {
           bool updated = false;
           if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            foreach (RevitExtension ext in extensions)
                            {
                                if (ext.ParameterName.Contains("Action"))
                                {
                                    cmd.CommandText = @"UPDATE Components SET Action = '" + Guid.Empty.ToString() + "' WHERE Action = '" + ext.Guid + "'";
                                }
                                else if (ext.ParameterName.Contains("Responsibility"))
                                {
                                    cmd.CommandText = @"UPDATE Components SET Responsibility = '" + Guid.Empty.ToString() + "' WHERE Responsibility = '" + ext.Guid + "'";
                                }
                                cmd.ExecuteNonQuery();
                            }
                            updated = true;
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

        public static bool UpdateBCFFileInfo(ObservableCollection<BCFZIP> bcfFiles)
        {
            bool updated = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.Parameters.Add("@isPrimary", DbType.Boolean);
                            foreach (BCFZIP bcf in bcfFiles)
                            {
                                cmd.CommandText = @"UPDATE BCFFileInfo SET IsPrimary = @isPrimary WHERE Guid = '" + bcf.FileId + "'";
                                cmd.Parameters["@isPrimary"].Value = bcf.IsPrimary;

                                cmd.ExecuteNonQuery();
                            }

                            cmd.Parameters.Clear();
                            updated = true;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

        public static bool InsertViewPoint(string fileId, ViewPoint viewPoint)
        {
            bool inserted = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.Parameters.Add("@snapshotImage", DbType.Binary);

                            cmd.CommandText = @"INSERT OR IGNORE INTO Viewpoints (Guid, Viewpoint, Snapshot, Snapshot_Image, Topic_Guid) VALUES " +
                                   "('" + viewPoint.Guid + "', '" + viewPoint.Viewpoint + "', '" + viewPoint.Snapshot + "',  @snapshotImage, '" + viewPoint.TopicGuid + "')";

                            cmd.Parameters["@snapshotImage"].Value = viewPoint.SnapshotImage;

                            try
                            {
                                int insertedRows = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "/n" + ex.Message;
                            }

                            bool insertedVisInfo = InsertVisInfoValues(fileId, viewPoint, cmd, ConflictMode.IGNORE);
                            cmd.Parameters.Clear();
                            inserted = true;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return inserted;
        }

        public static bool DeleteViewPoint(string viewpoint_guid)
        {
            bool deleted = false;
            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.CommandText = "DELETE FROM Viewpoints WHERE Guid ='" + viewpoint_guid + "'";
                            try
                            {
                                int deletedRows = cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "/n" + ex.Message;
                            }

                            bool deletedVisInfo = DeleteVisInfoValues(viewpoint_guid, cmd);

                            deleted = true;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return deleted;
        }

        private static bool DeleteVisInfoValues(String viewpoint_guid, SQLiteCommand cmd)
        {
            bool inserted = false;
            try
            {
                //Bitmaps
                cmd.CommandText = "DELETE FROM Bitmaps WHERE Viewpoints_Guid ='" + viewpoint_guid + "'";
                cmd.ExecuteNonQuery();

                //Components
                cmd.CommandText = "DELETE FROM Components WHERE Viewpoints_Guid ='" + viewpoint_guid + "'";
                cmd.ExecuteNonQuery();

                //ClippingPlane
                cmd.CommandText = "DELETE FROM ClippingPlane WHERE Viewpoints_Guid ='" + viewpoint_guid + "'";
                cmd.ExecuteNonQuery();

                //Lines
                cmd.CommandText = "DELETE FROM Lines WHERE Viewpoints_Guid ='" + viewpoint_guid + "'";
                cmd.ExecuteNonQuery();

                //OrthogonalCamera
                cmd.CommandText = "DELETE FROM OrthogonalCamera WHERE Viewpoints_Guid ='" + viewpoint_guid + "'";
                cmd.ExecuteNonQuery();

                //PerspectiveCamera
                cmd.CommandText = "DELETE FROM PerspectiveCamera WHERE Viewpoints_Guid ='" + viewpoint_guid + "'";
                cmd.ExecuteNonQuery();

                //Point
                cmd.CommandText = "DELETE FROM Point WHERE Viewpoints_Guid ='" + viewpoint_guid + "'";
                cmd.ExecuteNonQuery();

                //Direction
                cmd.CommandText = "DELETE FROM Direction WHERE Viewpoints_Guid ='" + viewpoint_guid + "'";
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete Visualization Information tables values.\n" + ex.Message, "Delete Visualization Information Tables Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        public static bool UpdateViewPointImage(ViewPoint viewPoint)
        {
            bool updated = false;
            if (null == sqlConnection) { return false; }

            using (SQLiteTransaction trans = sqlConnection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = sqlConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            cmd.Parameters.Add("@snapshotImage", DbType.Binary);
                            cmd.CommandText = @"UPDATE Viewpoints SET Snapshot_Image = @snapshotImage WHERE Guid = '" + viewPoint.Guid + "'";
                            cmd.Parameters["@snapshotImage"].Value = viewPoint.SnapshotImage;

                            int updatedRows = cmd.ExecuteNonQuery();

                            cmd.Parameters.Clear();
                            updated = (updatedRows > 0) ? true : false;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    string message = ex.Message;
                }
            }
            return updated;
        }

    }
}
