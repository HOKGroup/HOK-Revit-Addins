using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;

namespace HOK.SheetDataManager
{
    public class DatabaseManager
    {
        private string dbFile = "";
        private bool dbOpened = false;
        private SQLiteConnection connection = null;
        private SQLiteCommand command = null;
        private bool allowToDelete = false;

        public string DBFile { get { return dbFile; } set { dbFile = value; } }
        public bool DBOpened { get { return dbOpened; } set { dbOpened = value; } }
        public SQLiteConnection DBConnection { get { return connection; } set { connection = value; } }
        public SQLiteCommand DBCommand { get { return command; } set { command = value; } }
        public bool AllowToDelete { get { return allowToDelete; } set { allowToDelete = value; } }

        private Dictionary<string/*paramName*/, string/*fieldName*/> parameterMaps = new Dictionary<string, string>();

        public DatabaseManager(string databaseFile)
        {
            dbFile = databaseFile;
            CreateParameterMaps();
            dbOpened = OpenDatabase(dbFile);
        }

        private void CreateParameterMaps()
        {
            try
            {
                //sheet parameters
                parameterMaps.Add("Sheet Number", "Sheet_Number");
                parameterMaps.Add("Sheet Name", "Sheet_Name");
                parameterMaps.Add("Volume Number", "Sheet_VolumeNumber");
                parameterMaps.Add("Sorted Discipline", "Sheet_SortedDiscipline");
                parameterMaps.Add("Drawing Type", "Sheet_DrawingType");

                //revision parameters
                parameterMaps.Add("Revision Description", "Revision_Description");
                parameterMaps.Add("Revision Date", "Revision_Date");
                parameterMaps.Add("Issued to", "Revision_IssuedTo");
                parameterMaps.Add("Issued by", "Revision_IssuedBy");

                //linked revisions
                parameterMaps.Add("Revision Sequence", "LinkedRevision_Sequence");
                parameterMaps.Add("Revision Number", "LinkedRevision_Number");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create parameter maps.\n" + ex.Message, "Create Parameter Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool OpenDatabase(string databaseFile)
        {
            bool opened = false;
            try
            {
                if (!string.IsNullOrEmpty(databaseFile))
                {
                    dbFile = databaseFile;
                    if (null != connection)
                    {
                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            connection.Close();
                        }
                    }
                    connection = new SQLiteConnection("Data Source=" + databaseFile + ";Version=3;");
                    connection.Open();
                   
                    command = new SQLiteCommand(connection);
                    opened = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(databaseFile + "\nFailed to open the database file.\n" + ex.Message, "Open Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return opened;
        }

        public void CloseDatabse()
        {
            try
            {
                if (null != connection)
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(dbFile + "\nFailed to close the database file.\n" + ex.Message, "Close Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public RevitSheetData ReadDatabase()
        {
            RevitSheetData sheetData = new RevitSheetData(dbFile);
            try
            {
                if (dbOpened)
                {
                    sheetData.Disciplines = ReadDiscipline();
                    sheetData.LinkedProjects = ReadLinkedProjects();
                    sheetData.Sheets = ReadRevitSheets(sheetData);
                    sheetData.LinkedSheets = ReadLinkedSheets(sheetData);
                    sheetData.Revisions = ReadRevisions(sheetData);
                    sheetData.LinkedRevisions = ReadLinkedRevisions(sheetData);
                    sheetData.RevisionMatrix = ReadRevisionMatrix();
                    sheetData.ViewTypes = ReadViewTypes();
                    sheetData.Views = ReadViews(sheetData);
                    sheetData.ReplaceItems = ReadReplaceItems();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read database file.\n" + ex.Message, "Read Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sheetData;
        }

        private Dictionary<Guid, Discipline> ReadDiscipline()
        {
            Dictionary<Guid, Discipline> disciplines = new Dictionary<Guid, Discipline>();
            try
            {
                command.CommandText = "SELECT * FROM Discipline";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Discipline discipline = new Discipline();
                        discipline.Id = reader.GetGuid(reader.GetOrdinal("Discipline_Id"));
                        discipline.Name = reader.GetString(reader.GetOrdinal("Discipline_Name"));
                        if (!disciplines.ContainsKey(discipline.Id))
                        {
                            disciplines.Add(discipline.Id, discipline);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read Discipline table.\n" + ex.Message, "Read Disciplines", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return disciplines;
        }

        private Dictionary<Guid, RevitSheet> ReadRevitSheets(RevitSheetData sheetData)
        {
            Dictionary<Guid, RevitSheet> sheets = new Dictionary<Guid, RevitSheet>();
            try
            {
                command.CommandText = "SELECT * FROM Sheets";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RevitSheet sheet = new RevitSheet();
                        sheet.Id = reader.GetGuid(reader.GetOrdinal("Sheet_Id"));
                        sheet.Number = reader.GetString(reader.GetOrdinal("Sheet_Number"));
                        sheet.Name = reader.GetString(reader.GetOrdinal("Sheet_Name"));
                        sheet.VolumeNumber = reader.GetString(reader.GetOrdinal("Sheet_VolumeNumber"));
                        Guid disciplineId = reader.GetGuid(reader.GetOrdinal("Sheet_Discipline_Id"));
                        if (sheetData.Disciplines.ContainsKey(disciplineId))
                        {
                            Discipline discipline = sheetData.Disciplines[disciplineId];
                            sheet.DisciplineObj = discipline;
                        }
                        sheet.DrawingType = reader.GetString(reader.GetOrdinal("Sheet_DrawingType"));
                        sheet.SortedDiscipline = reader.GetString(reader.GetOrdinal("Sheet_SortedDiscipline"));

                        if (!sheets.ContainsKey(sheet.Id))
                        {
                            sheets.Add(sheet.Id, sheet);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read sheets table.\n" + ex.Message, "Read Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sheets;
        }

        private Dictionary<Guid, LinkedSheet> ReadLinkedSheets(RevitSheetData sheetData)
        {
            Dictionary<Guid, LinkedSheet> linkedSheets = new Dictionary<Guid,LinkedSheet>();
            try
            {
                command.CommandText = "SELECT * FROM LinkedSheets";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LinkedSheet lSheet = new LinkedSheet();
                        lSheet.Id = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Id"));
                        lSheet.SheetId = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Sheet_Id"));
                        Guid projectId = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Project_Id"));
                        if (sheetData.LinkedProjects.ContainsKey(projectId))
                        {
                            lSheet.LinkProject = sheetData.LinkedProjects[projectId];
                        }
                        lSheet.LinkedElementId = reader.GetString(reader.GetOrdinal("LinkedSheet_Element_Id"));
                        lSheet.IsSource = reader.GetBoolean(reader.GetOrdinal("LinkedSheet_IsSource"));

                        if (!linkedSheets.ContainsKey(lSheet.Id))
                        {
                            linkedSheets.Add(lSheet.Id, lSheet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read linked sheets.\n"+ex.Message, "Read Linked Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedSheets;
        }

        private Dictionary<Guid, RevitRevision> ReadRevisions(RevitSheetData sheetData)
        {
            Dictionary<Guid, RevitRevision> revisions = new Dictionary<Guid, RevitRevision>();
            try
            {
                command.CommandText = @"SELECT *, RevisionDocuments.Document_Path As Path, RevisionDocuments.Document_Title As Title, RevisionDocuments.Document_Contents As Contents
                FROM Revisions LEFT JOIN RevisionDocuments ON Revisions.Revision_Document_Id = RevisionDocuments.Document_Id";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RevitRevision revision = new RevitRevision();
                        revision.Id = reader.GetGuid(reader.GetOrdinal("Revision_Id"));
                        revision.Description = reader.GetString(reader.GetOrdinal("Revision_Description"));
                        revision.IssuedBy = reader.GetString(reader.GetOrdinal("Revision_IssuedBy"));
                        revision.IssuedTo = reader.GetString(reader.GetOrdinal("Revision_IssuedTo"));
                        revision.Date = reader.GetString(reader.GetOrdinal("Revision_Date"));

                        if (reader["Revision_Document_Id"] != System.DBNull.Value)
                        {
                            string documentId = reader["Revision_Document_Id"].ToString();
                            if (!string.IsNullOrEmpty(documentId))
                            {
                                RevisionDocument document = new RevisionDocument();
                                document.Id = new Guid(documentId);
                                document.Path = reader["Path"].ToString();
                                document.Title = reader["Title"].ToString();
                                if (reader["Contents"] != System.DBNull.Value)
                                {
                                    document.Contents = (byte[])reader["Contents"];
                                }
                                revision.Document = document;
                            }
                        }

                        if (!revisions.ContainsKey(revision.Id))
                        {
                            revisions.Add(revision.Id, revision);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read revisions.\n" + ex.Message, "Read Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return revisions;
        }

        private Dictionary<Guid, LinkedRevision> ReadLinkedRevisions(RevitSheetData sheetData)
        {
            Dictionary<Guid, LinkedRevision> linkedRevisions = new Dictionary<Guid, LinkedRevision>();
            try
            {
                command.CommandText = "SELECT * FROM LinkedRevisions";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LinkedRevision lRevision = new LinkedRevision();
                        lRevision.Id = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Id"));
                        lRevision.RevisionId = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Revision_Id"));
                        lRevision.Sequence = reader.GetInt32(reader.GetOrdinal("LinkedRevision_Sequence"));
                        lRevision.Number = reader.GetString(reader.GetOrdinal("LinkedRevision_Number"));
                        lRevision.NumberType = (RevisionNumberType)Enum.Parse(typeof(RevisionNumberType), reader.GetString(reader.GetOrdinal("LinkedRevision_NumberType")));

                        Guid projectId = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Project_Id"));
                        if (sheetData.LinkedProjects.ContainsKey(projectId))
                        {
                            LinkedProject project = sheetData.LinkedProjects[projectId];
                            lRevision.LinkProject = project;
                        }

                        lRevision.LinkedElementId = reader.GetString(reader.GetOrdinal("LinkedRevision_Element_Id"));
                        lRevision.IsSource = reader.GetBoolean(reader.GetOrdinal("LinkedRevision_IsSource"));

                        if (!linkedRevisions.ContainsKey(lRevision.Id))
                        {
                            linkedRevisions.Add(lRevision.Id, lRevision);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read linked revisions.\n"+ex.Message, "Read Linked Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedRevisions;
        }

        public Dictionary<Guid, RevisionOnSheet> ReadRevisionMatrix()
        {
            Dictionary<Guid, RevisionOnSheet> revisionOnSheets = new Dictionary<Guid, RevisionOnSheet>();
            try
            {
                command.CommandText = "SELECT * FROM RevisionsOnSheet";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid mapId = reader.GetGuid(reader.GetOrdinal("RevisionsOnSheet_Id"));
                        Guid sheetId = reader.GetGuid(reader.GetOrdinal("RevisionsOnSheet_Sheet_Id"));
                        Guid revisionId = reader.GetGuid(reader.GetOrdinal("RevisionsOnSheet_Revision_Id"));

                        if (!revisionOnSheets.ContainsKey(mapId))
                        {
                            RevisionOnSheet ros = new RevisionOnSheet(mapId, sheetId, revisionId);
                            revisionOnSheets.Add(mapId, ros);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read revisions.\n" + ex.Message, "Read Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return revisionOnSheets;
        }

        private Dictionary<Guid, RevitViewType> ReadViewTypes()
        {
            Dictionary<Guid, RevitViewType> viewTypes = new Dictionary<Guid, RevitViewType>();
            try
            {
                command.CommandText = "SELECT * FROM ViewTypes";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RevitViewType viewType = new RevitViewType();
                        viewType.Id = reader.GetGuid(reader.GetOrdinal("ViewType_Id"));
                        viewType.Name = reader.GetString(reader.GetOrdinal("ViewType_Name"));
                        viewType.ViewType = (ViewTypeEnum)Enum.Parse(typeof(ViewTypeEnum), reader.GetString(reader.GetOrdinal("ViewType_Enum")));

                        if (!viewTypes.ContainsKey(viewType.Id))
                        {
                            viewTypes.Add(viewType.Id, viewType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read Discipline table.\n" + ex.Message, "Read Disciplines", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewTypes;
        }

        private Dictionary<Guid, RevitView> ReadViews(RevitSheetData sheetData)
        {
            Dictionary<Guid, RevitView> views = new Dictionary<Guid, RevitView>();
            try
            {
                command.CommandText = @"SELECT * FROM Views";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RevitView view = new RevitView();
                        view.Id = reader.GetGuid(reader.GetOrdinal("View_Id"));
                        view.Name = reader.GetString(reader.GetOrdinal("View_Name"));
                        if (reader["View_Sheet_Id"] != System.DBNull.Value)
                        {
                            string sheetIdStr = reader["View_Sheet_Id"].ToString();
                            if (!string.IsNullOrEmpty(sheetIdStr))
                            {
                                Guid sheetId = new Guid(sheetIdStr);
                                if (sheetData.Sheets.ContainsKey(sheetId))
                                {
                                    view.Sheet = sheetData.Sheets[sheetId];
                                }
                            }
                        }

                        string viewTypeIdStr = reader["View_ViewType_Id"].ToString();
                        Guid viewTypeId = new Guid(viewTypeIdStr);
                        if (sheetData.ViewTypes.ContainsKey(viewTypeId))
                        {
                            view.ViewType = sheetData.ViewTypes[viewTypeId];
                        }
                        view.LocationU = reader.GetDouble(reader.GetOrdinal("View_LocationU"));
                        view.LocationV = reader.GetDouble(reader.GetOrdinal("View_LocationV"));

                        if (!views.ContainsKey(view.Id))
                        {
                            views.Add(view.Id, view);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read view table.\n" + ex.Message, "Read Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return views;
        }

        public Dictionary<Guid, ReplaceItem> ReadReplaceItems()
        {
            Dictionary<Guid, ReplaceItem> items = new Dictionary<Guid, ReplaceItem>();
            try
            {
                command.CommandText = @"SELECT * FROM ReplaceItems";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ReplaceItem item = new ReplaceItem();
                        item.ItemId = reader.GetGuid(reader.GetOrdinal("ReplaceItem_Id"));
                        item.ItemType = (ReplaceType)Enum.Parse(typeof(ReplaceType), reader.GetString(reader.GetOrdinal("ReplaceItem_Type")));
                        item.ParameterName = reader.GetString(reader.GetOrdinal("ReplaceItem_Parameter"));
                        item.SourceValue = reader.GetString(reader.GetOrdinal("ReplaceItem_Source_Value"));
                        item.TargetValue = reader.GetString(reader.GetOrdinal("ReplaceItem_Target_Value"));

                        if (!items.ContainsKey(item.ItemId))
                        {
                            items.Add(item.ItemId, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read replace items.\n" + ex.Message, "Read Replace Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return items;
        }

        public Dictionary<Guid, LinkedProject> ReadLinkedProjects()
        {
            Dictionary<Guid, LinkedProject> projects = new Dictionary<Guid, LinkedProject>();
            try
            {
                command.CommandText = "SELECT * FROM LinkedProjects";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LinkedProject project = new LinkedProject();
                        project.Id = new Guid(reader.GetString(reader.GetOrdinal("LinkedProject_Id")));
                        project.ProjectNumber = reader.GetString(reader.GetOrdinal("LinkedProject_Number"));
                        project.ProjectName = reader.GetString(reader.GetOrdinal("LinkedProject_Name"));
                        project.FilePath = reader.GetString(reader.GetOrdinal("LinkedProject_FilePath"));
                        project.LinkedDate = reader.GetDateTime(reader.GetOrdinal("LinkedProject_LinkedDate"));
                        project.LinkedBy = reader.GetString(reader.GetOrdinal("LinkedProject_LinkedBy"));

                        if (!projects.ContainsKey(project.Id))
                        {
                            projects.Add(project.Id, project);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read linked projects.\n" + ex.Message, "Read Linked Projects", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return projects;
        }

        public bool InsertSheetInfo(SheetInfo sheetInfo)
        {
            bool inserted = false;
            LinkedSheet linkedSheet = sheetInfo.LinkedSheetItem;
            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        if (null != linkedSheet)
                        {
                            cmd.Transaction = trans;

                            cmd.CommandText =  @"INSERT INTO Sheets (Sheet_Id, Sheet_Number, Sheet_Name, Sheet_VolumeNumber, Sheet_Discipline_Id, Sheet_DrawingType, Sheet_SortedDiscipline) " +
                                "VALUES ('" + linkedSheet.SheetId.ToString() + "', '" + sheetInfo.SheetNumber + "', '" + sheetInfo.SheetName + "', '" + sheetInfo.VolumeNumber + "', '" +
                                Guid.Empty.ToString() + "', '" + sheetInfo.DrawingType + "', '" + sheetInfo.SortedDiscipline + "' )";

                            try
                            {
                                inserted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                                cmd.Parameters.Clear();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Sheet Number: " + sheetInfo.SheetNumber + "\nFailed to insert sheet information.\n" + ex.Message, "Insert Sheet Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            bool updatedLink = UpdateLinkedSheet(linkedSheet);
            return inserted;
        }

        public bool UpdateSheetInfo(Guid sheetGuid, string parameterName, object value)
        {
            bool updated = false;
            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        if(parameterMaps.ContainsKey(parameterName))
                        {
                             cmd.Transaction = trans;
                             string fieldName = parameterMaps[parameterName];

                             string commandStr = @"UPDATE Sheets SET " + fieldName + " = @value WHERE Sheet_Id = @sheetId";

                             cmd.Parameters.AddWithValue("@value", value);
                             cmd.Parameters.AddWithValue("@sheetId", sheetGuid.ToString());

                             cmd.CommandText = commandStr;
                             try
                             {
                                 updated = (cmd.ExecuteNonQuery() > 0) ? true : false;
                                 cmd.Parameters.Clear();
                             }
                             catch (SQLiteException ex)
                             {
                                 string message = cmd.CommandText + "\n" + ex.Message;
                             }
                        }
                        trans.Commit();
                    }
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to update sheet inforamtion.\nParameter name: " + parameterName+"\n"+ex.Message, "Update Sheet Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return updated;
        }

        public LinkedSheet GetLinkedSheet(string elementId, Guid projectId)
        {
            LinkedSheet linkedSheet = null;
            try
            {
                command.CommandText = "SELECT * FROM LinkedSheets WHERE LinkedSheet_Element_Id = '" + elementId + "' AND LinkedSheet_Project_Id = '" + projectId.ToString() + "'";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        linkedSheet = new LinkedSheet();
                        linkedSheet.Id = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Id"));
                        linkedSheet.SheetId = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Sheet_Id"));
                        linkedSheet.LinkProject = new LinkedProject(projectId);
                        linkedSheet.LinkedElementId = reader.GetString(reader.GetOrdinal("LinkedSheet_Element_Id"));
                        linkedSheet.IsSource = reader.GetBoolean(reader.GetOrdinal("LinkedSheet_IsSource"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get linked sheet information.\n"+ex.Message, "Get Linked Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedSheet;
        }

        public bool UpdateLinkedSheet(LinkedSheet linkedSheet)
        {
            bool updated = false;
            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        cmd.CommandText = @"INSERT OR REPLACE INTO LinkedSheets (LinkedSheet_Id, LinkedSheet_Sheet_Id, LinkedSheet_Project_Id, LinkedSheet_Element_Id, LinkedSheet_IsSource) "
                                + "VALUES ('"+linkedSheet.Id.ToString()+"', '"+ linkedSheet.SheetId.ToString()+"', '"+linkedSheet.LinkProject.Id.ToString()+"', '"+linkedSheet.LinkedElementId+"', @isSource)";

                        cmd.Parameters.AddWithValue("@isSource", linkedSheet.IsSource);

                        try
                        {
                            updated = (cmd.ExecuteNonQuery() > 0) ? true : false;
                            cmd.Parameters.Clear();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "\n" + ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to update the item of the linked revision.\n" + ex.Message, "Update Linked Revision", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return updated;
        }

        public bool DeleteSheetInfo(Guid sheetGuid, Guid projectId, bool deleteSource)
        {
            bool deleted = false;
            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        if (deleteSource)
                        {
                            try
                            {
                                cmd.CommandText = @"DELETE FROM Sheets WHERE Sheet_Id = '" + sheetGuid.ToString() + "'";
                                deleted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }
                        }

                        try
                        {
                            cmd.CommandText = @"DELETE FROM LinkedSheets WHERE LinkedSheet_Sheet_Id = '" + sheetGuid.ToString() + "' AND LinkedSheet_Project_Id ='" + projectId.ToString() + "'";
                            deleted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "\n" + ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to delete sheet information.\n"+ex.Message, "Delete Sheet Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return deleted;
        }

        public bool InsertRevisionInfo(RevisionInfo revisionInfo)
        {
            bool inserted = false;
            LinkedRevision linkedRevision = revisionInfo.LinkedRevisionItem;

            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = @"INSERT OR IGNORE INTO Revisions (Revision_Id, Revision_Description, Revision_IssuedBy, Revision_IssuedTo, Revision_Date) "
                            + "VALUES ('"+linkedRevision.RevisionId.ToString()+"', '"+ revisionInfo.RevisionDescription+"', '"+revisionInfo.IssuedBy+"', '"+revisionInfo.IssuedTo+"', '"+revisionInfo.Date+"')";

                        try
                        {
                            inserted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                            cmd.Parameters.Clear();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "\n" + ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Revision Description: " + revisionInfo.RevisionDescription + "\nFailed to insert revision information.\n" + ex.Message, "Insert Revision Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            bool insertedLink = UpdateLinkedRevision(linkedRevision);
            return inserted;
        }

        public bool UpdateRevisionInfo(Guid revisionGuid, string revisionUniqueId, string parameterName, object value)
        {
            bool updated = false;
            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        if (parameterMaps.ContainsKey(parameterName))
                        {
                            cmd.Transaction = trans;

                            cmd.Parameters.AddWithValue("@value", value);
                            cmd.Parameters.AddWithValue("@revisionId", revisionGuid.ToString());
                            cmd.Parameters.AddWithValue("@uniqueId", revisionUniqueId);

                            string fieldName = parameterMaps[parameterName];
                            if (fieldName.Contains("LinkedRevision"))
                            {
                                cmd.CommandText = @"UPDATE LinkedRevisions SET " + fieldName + " = @value WHERE LinkedRevision_Revision_Id = @revisionId AND LinkedRevision_Element_Id = @uniqueId";
                            }
                            else
                            {
                                cmd.CommandText = @"UPDATE Revisions SET " + fieldName + " = @value WHERE Revision_Id = @revisionId";
                            }

                            try
                            {
                                updated = (cmd.ExecuteNonQuery() > 0) ? true : false;
                                cmd.Parameters.Clear();
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }
                        }
                        trans.Commit();
                    }
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to update revision inforamtion.\nParameter name: " + parameterName + "\n" + ex.Message, "Update Revision Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return updated;
        }

        public LinkedRevision GetLinkedRevision(string elementId, Guid projectId)
        {
            LinkedRevision linkedRevision = null;
            try
            {
                command.CommandText = "SELECT * FROM LinkedRevisions WHERE LinkedRevision_Element_Id ='"+elementId+"' AND LinkedRevision_Project_Id = '" +projectId.ToString()+"'";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        linkedRevision = new LinkedRevision();
                        linkedRevision.Id = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Id"));
                        linkedRevision.RevisionId = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Revision_Id"));
                        linkedRevision.Sequence = reader.GetInt16(reader.GetOrdinal("LinkedRevision_Sequence"));
                        linkedRevision.Number = reader.GetString(reader.GetOrdinal("LinkedRevision_Number"));
                        linkedRevision.NumberType = (RevisionNumberType)Enum.Parse(typeof(RevisionNumberType), reader.GetString(reader.GetOrdinal("LinkedRevision_NumberType")));
                        linkedRevision.LinkProject = new LinkedProject(projectId);
                        linkedRevision.LinkedElementId = elementId;
                        linkedRevision.IsSource = reader.GetBoolean(reader.GetOrdinal("LinkedRevision_IsSource"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get linked revision information.\n" + ex.Message, "Get Linked Revision", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedRevision;
        }

        public bool UpdateLinkedRevision(LinkedRevision linkedRevision)
        {
            bool updated = false;
            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        cmd.CommandText = @"INSERT OR REPLACE INTO LinkedRevisions (LinkedRevision_Id, LinkedRevision_Sequence, LinkedRevision_Number, LinkedRevision_NumberType, LinkedRevision_Revision_Id, LinkedRevision_Project_Id, LinkedRevision_Element_Id, LinkedRevision_IsSource) "
                           + "VALUES ('"+linkedRevision.Id.ToString()+"', "+linkedRevision.Sequence+", '"+linkedRevision.Number+"', '"+linkedRevision.NumberType.ToString()+"', '"+
                           linkedRevision.RevisionId.ToString()+"', '"+ linkedRevision.LinkProject.Id.ToString()+"', '"+linkedRevision.LinkedElementId+"', @isSource)";

                        cmd.Parameters.AddWithValue("@isSource", linkedRevision.IsSource);

                        try
                        {
                            updated = (cmd.ExecuteNonQuery() > 0) ? true : false;
                            cmd.Parameters.Clear();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "\n" + ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to update the item of the linked revision.\n" + ex.Message, "Update Linked Revision", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return updated;
        }

        public bool DeleteRevisionInfo(Guid revisionGuid, Guid projectId, bool deleteSource)
        {
            bool deleted = false;
            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        if (deleteSource)
                        {
                            try
                            {
                                cmd.CommandText = @"DELETE FROM Revisions WHERE Revision_Id = '" + revisionGuid.ToString() + "'";
                                deleted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                            }
                            catch (SQLiteException ex)
                            {
                                string message = cmd.CommandText + "\n" + ex.Message;
                            }
                        }

                        try
                        {
                            cmd.CommandText = @"DELETE FROM LinkedRevisions WHERE LinkedRevision_Revision_Id = '" + revisionGuid.ToString() + "' AND LinkedRevision_Project_Id ='" + projectId.ToString() + "'";
                            deleted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "\n" + ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (SQLiteException ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to delete sheet information.\n" + ex.Message, "Delete Sheet Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return deleted;
        }

        public bool InsertProjectInfo(LinkedProject projectInfo)
        {
            bool inserted = false;
            using (SQLiteTransaction trans = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        string commandStr = @"INSERT INTO LinkedProjects (LinkedProject_Id, LinkedProject_Number, LinkedProject_Name, LinkedProject_FilePath, LinkedProject_LinkedDate, LinkedProject_LinkedBy) "
                       + "VALUES ('"+projectInfo.Id.ToString()+"', '"+projectInfo.ProjectNumber+"', '"+projectInfo.ProjectName+"', '"+projectInfo.FilePath+"', @linkedDate, '"+projectInfo.LinkedBy+"')";

                        cmd.Parameters.AddWithValue("@linkedDate", projectInfo.LinkedDate);

                        cmd.CommandText = commandStr;
                        try
                        {
                            inserted = (cmd.ExecuteNonQuery() > 0) ? true : false;
                            cmd.Parameters.Clear();
                        }
                        catch (SQLiteException ex)
                        {
                            string message = cmd.CommandText + "\n" + ex.Message;
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Failed to insert linked project information.\n" + ex.Message, "Insert Project Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return inserted;
        }

    }
}
