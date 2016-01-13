using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Management;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.SheetDataEditor
{
    public enum CommandType
    {
        INSERT, UPDATE, DELETE, NONE
    }
    public static class DatabaseUtil
    {
        private static string dbFile = "";
        private static SQLiteConnection connection = null;
        private static SQLiteCommand command = null;
       
        public static bool OpenDatabase(string databaseFile)
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
                    connection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;", true);
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

        public static bool CreateDatabase(string databaseFile)
        {
            bool created = false;
            try
            {
                SQLiteConnection.CreateFile(databaseFile);
                if (File.Exists(databaseFile))
                {
                    bool createdTables = CreateTables(databaseFile);
                    if (createdTables)
                    {
                        Dictionary<Guid, RevitViewType> viewTypes = DatabaseResources.GetDefaultViewTypes();
                        foreach (RevitViewType vt in viewTypes.Values)
                        {
                            bool inserted = InsertRevitViewType(vt);
                        }
                        Dictionary<Guid, Discipline> disciplines = DatabaseResources.GetDefaultDsiciplines();
                        foreach (Discipline discipline in disciplines.Values)
                        {
                            bool inserted = ChangeDisciplineItem(discipline, CommandType.INSERT);
                        }
                    }

                    created = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create database file.\n"+ex.Message, "Create Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        public static bool CreateTables(string databaseFile)
        {
            bool created = false;
            try
            {
                if (OpenDatabase(databaseFile))
                {
                    if (null != connection && null != command)
                    {
                        //Discipline
                        ResourceManager rManager = Properties.Resources.ResourceManager;
                        string[] tableNames = DatabaseResources.tableNames;
                        int count = 0;
                        foreach (string tableName in tableNames)
                        {
                            command.CommandText = rManager.GetString(tableName);
                            command.ExecuteNonQuery();
                            count++;
                        }

                        if (count == tableNames.Length) { created = true; }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create tables.\n"+ex.Message, "Create Tables", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        public static void CloseDatabse()
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

        public static RevitSheetData ReadDatabase(string databaseFile)
        {
            if (!OpenDatabase(databaseFile))
            {
                return null;
            }

            RevitSheetData sheetData = new RevitSheetData(databaseFile);
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read database file.\n"+ex.Message, "Read Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sheetData;
        }

        private static Dictionary<Guid, Discipline> ReadDiscipline()
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

        private static Dictionary<Guid, RevitSheet> ReadRevitSheets(RevitSheetData sheetData)
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

        private static Dictionary<Guid, LinkedSheet> ReadLinkedSheets(RevitSheetData sheetData)
        {
            Dictionary<Guid, LinkedSheet> linkedSheets = new Dictionary<Guid, LinkedSheet>();
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
                MessageBox.Show("Failed to read linked sheets.\n" + ex.Message, "Read Linked Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedSheets;
        }

        private static Dictionary<Guid, RevitRevision> ReadRevisions(RevitSheetData sheetData)
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
                MessageBox.Show("Failed to read revisions.\n"+ex.Message, "Read Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return revisions;
        }

        private static Dictionary<Guid, LinkedRevision> ReadLinkedRevisions(RevitSheetData sheetData)
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
                MessageBox.Show("Failed to read linked revisions.\n" + ex.Message, "Read Linked Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedRevisions;
        }

        public static Dictionary<Guid, RevisionOnSheet> ReadRevisionMatrix()
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

        private static Dictionary<Guid, RevitViewType> ReadViewTypes()
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

        private static Dictionary<Guid, RevitView> ReadViews(RevitSheetData sheetData)
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
                                    view.SetSheet(sheetData.Sheets[sheetId]);
                                }
                            }
                        }

                        string viewTypeIdStr = reader["View_ViewType_Id"].ToString();
                        Guid viewTypeId = new Guid(viewTypeIdStr);
                        if (sheetData.ViewTypes.ContainsKey(viewTypeId))
                        {
                            view.SetViewType(sheetData.ViewTypes[viewTypeId]);
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

        public static Dictionary<Guid, ReplaceItem> ReadReplaceItems()
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
                MessageBox.Show("Failed to read replace items.\n"+ex.Message, "Read Replace Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return items;
        }

        public static Dictionary<Guid, LinkedProject> ReadLinkedProjects()
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
                MessageBox.Show("Failed to read linked projects.\n"+ex.Message, "Read Linked Projects", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return projects;
        }

        private static bool InsertRevitViewType(RevitViewType vt)
        {
            bool inserted = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = @"INSERT INTO ViewTypes(ViewType_Id, ViewType_Name, ViewType_Enum) "+
                        "VALUES ('"+vt.Id.ToString()+"', '"+vt.Name+"', '"+vt.ViewType.ToString()+"')";
                    if(command.ExecuteNonQuery() > 0) {inserted = true;}
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        public static bool InsertRevisionOnSheet(RevisionOnSheet ros)
        {
            bool inserted = false;
            try
            {
                if (null != connection && null != command)
                {
                    bool deletedExisting = DeleteRevisionOnSheet(ros.SheetId, ros.RevisionId);

                    command.CommandText = @"INSERT INTO RevisionsOnSheet(RevisionsOnSheet_Id, RevisionsOnSheet_Sheet_Id, RevisionsOnSheet_Revision_Id) " +
                                          "VALUES ('"+ros.MapId.ToString()+"', '"+ros.SheetId.ToString()+"', '"+ros.RevisionId.ToString()+"')";
                    if (command.ExecuteNonQuery() > 0) { inserted = true; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert revision on sheet item int the database.\n"+ex.Message, "Insert Revision On Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        public static bool DeleteRevisionOnSheet(Guid sheetId, Guid revisionId)
        {
            bool deleted = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = @"DELETE FROM RevisionsOnSheet WHERE RevisionsOnSheet_Sheet_Id = '" + sheetId.ToString() + "' AND RevisionsOnSheet_Revision_Id = '" + revisionId.ToString() + "'";
                    if (command.ExecuteNonQuery() > 0) { deleted = true; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete revision on sheet item.\n"+ex.Message, "Delete Revision on Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deleted;
        }

        public static bool UpdateViewOnSheet(Guid sheetId, Guid viewId, bool selected)
        {
            bool updated = false;
            try
            {
                if (null != connection && null != command)
                {
                    if (selected)
                    {
                        command.CommandText = "UPDATE Views SET View_Sheet_Id = '"+sheetId.ToString()+"' WHERE View_Id = '"+viewId.ToString()+"'";
                    }
                    else
                    {
                        command.CommandText = "UPDATE Views SET View_Sheet_Id = '' WHERE View_Id = '" + viewId.ToString() + "'";
                    }
                    int updatedRows = command.ExecuteNonQuery();
                    if (updatedRows > 0) { updated = true; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set the information of a sheet on a view item.\n"+ex.Message, "Update View On Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        public static bool ChangeReplaceItem(ReplaceItem item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO ReplaceItems (ReplaceItem_Id, ReplaceItem_Type, ReplaceItem_Parameter, ReplaceItem_Source_Value, ReplaceItem_Target_Value) " +
                                "VALUES ('"+item.ItemId.ToString()+"', '" + item.ItemType.ToString() + "', '" + item.ParameterName + "', '" + item.SourceValue + "', '" + item.TargetValue + "')";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = "UPDATE ReplaceItems SET ReplaceItem_Source_Value = '" + item.SourceValue.ToString() + "', ReplaceItem_Target_Value = '" + item.TargetValue.ToString() + "'  WHERE ReplaceItem_Id = '" + item.ItemId.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM ReplaceItems WHERE ReplaceItem_Id = '" + item.ItemId.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        public static bool ChangeSheetItem(RevitSheet item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO Sheets (Sheet_Id, Sheet_Number, Sheet_Name, Sheet_VolumeNumber, Sheet_Discipline_Id, Sheet_DrawingType, Sheet_SortedDiscipline) " +
                                "VALUES ('" + item.Id.ToString() + "', '" + item.Number + "', '" + item.Name + "', '" + item.VolumeNumber + "', '" + item.DisciplineObj.Id.ToString() + "', '"+item.DrawingType+"', '"+item.SortedDiscipline+"' )";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE Sheets SET Sheet_Number = '" + item.Number + "', Sheet_Name = '" + item.Name + "', Sheet_VolumeNumber = '" + item.VolumeNumber+"', Sheet_Discipline_Id ='"+item.DisciplineObj.Id.ToString()+"', "+
                                "Sheet_DrawingType ='" + item.DrawingType + "', Sheet_SortedDiscipline ='" + item.SortedDiscipline + "' WHERE Sheet_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM Sheets WHERE Sheet_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        public static bool ChangeRevisionItem(RevitRevision item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO Revisions (Revision_Id, Revision_Description, Revision_IssuedBy, Revision_IssuedTo, Revision_Date, Revision_Document_Id) " +
                                "VALUES ('" + item.Id.ToString() + "', '" + item.Description + "', '" + item.IssuedBy + "', '" + item.IssuedTo + "', '" + item.Date + "', '"+item.Document.Id.ToString()+"' )";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            if (item.Document.Id != Guid.Empty)
                            {
                                command.CommandText = @"INSERT OR REPLACE INTO RevisionDocuments (Document_Id, Document_Title, Document_Path) " +
                                    "VALUES ('" + item.Document.Id.ToString() + "', '" + item.Document.Title + "', '" + item.Document.Path + "')";
                                if (command.ExecuteNonQuery() > 0) { result = true; }
                            }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE Revisions SET Revision_Description = '" + item.Description + "', Revision_IssuedBy ='" + item.IssuedBy + "', Revision_IssuedTo = '" +item.IssuedTo+"', Revision_Date = '"+item.Date+"', "+
                                "Revision_Document_Id ='" + item.Document.Id.ToString() + "' WHERE Revision_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            if (item.Document.Id != Guid.Empty)
                            {
                                command.CommandText = @"INSERT OR REPLACE INTO RevisionDocuments (Document_Id, Document_Title, Document_Path) " +
                                    "VALUES ('" + item.Document.Id.ToString() + "', '" + item.Document.Title + "', '" + item.Document.Path + "')";
                                if (command.ExecuteNonQuery() > 0) { result = true; }
                            }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM Revisions WHERE Revision_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            if (item.Document.Id != Guid.Empty)
                            {
                                command.CommandText = @"DELETE FROM RevisionDocuments WHERE Document_Id = '" + item.Document.Id.ToString() + "'"; 
                                if (command.ExecuteNonQuery() > 0) { result = true; }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        public static bool ChangeDisciplineItem(Discipline item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO Discipline (Discipline_Id, Discipline_Name) " +
                                "VALUES ('" + item.Id.ToString() + "', '" + item.Name + "')";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE Discipline SET Discipline_Name = '" + item.Name + "' "+
                                "WHERE Discipline_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM Discipline WHERE Discipline_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            command.CommandText = @"UPDATE Sheets SET Sheet_Discipline_Id = '" + Guid.Empty.ToString() + "' " +
                                "WHERE Sheet_Discipline_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        public static bool ChangeViewItem(RevitView item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO Views (View_Id, View_Name, View_Sheet_Id, View_ViewType_Id, View_LocationU, View_LocationV) " +
                                "VALUES ('" + item.Id.ToString() + "', '" + item.Name + "', '" + item.Sheet.Id.ToString() + "', '" + item.ViewType.Id.ToString() + "', " + item.LocationU+ ", " + item.LocationV + ")";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE Views SET View_Name = '" + item.Name + "', View_Sheet_Id = '" + item.Sheet.Id.ToString() + "', View_ViewType_Id = '" + item.ViewType.Id.ToString() + "', View_LocationU = " + item.LocationU + ", View_LocationV =" +item.LocationV+
                                " WHERE View_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM Views WHERE View_Id = '" + item.Id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

    }
}
