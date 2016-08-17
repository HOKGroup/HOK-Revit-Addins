using HOK.SheetManager.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Database
{
    public enum CommandType
    {
        INSERT, UPDATE, DELETE, NONE
    }

    public class SheetDataWriter
    {
        public static string dbFile = "";
        public static SQLiteConnection connection = null;
        public static SQLiteCommand command = null;

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
                    command.Parameters.Add("@date", System.Data.DbType.DateTime);
                    command.Parameters.Add("@include", System.Data.DbType.Boolean);
                    command.Parameters.Add("@isSource", System.Data.DbType.Boolean);

                    opened = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return opened;
        }

        public static bool CreateDatabase(string dbFile)
        {
            bool created = false;
            try
            {
                SQLiteConnection.CreateFile(dbFile);
                if (File.Exists(dbFile))
                {
                    bool openedDatabase = OpenDatabase(dbFile);
                    bool createdTables = CreateTables(dbFile);
                    if (openedDatabase && createdTables)
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
                        created = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return created;
        }

        public static bool CreateTables(string databaseFile)
        {
            bool created = false;
            try
            {
                if (null != connection && null != command)
                {
                    //Discipline
                    ResourceManager rManager = HOK.SheetManager.Core.Properties.Resources.ResourceManager;
                    string[] tableNames = DatabaseResources.TableNames;
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
            catch (Exception ex)
            {
                string message = ex.Message;
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
                        command.Parameters.Clear();
                        connection.Close();
                        dbFile = "";
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static bool InsertRevitViewType(RevitViewType vt)
        {
            bool inserted = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = @"INSERT INTO ViewTypes(ViewType_Id, ViewType_Name, ViewType_Enum) " +
                        "VALUES ('" + vt.Id.ToString() + "', '" + vt.Name + "', '" + vt.ViewType.ToString() + "')";
                    if (command.ExecuteNonQuery() > 0) { inserted = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        public static bool UpdateViewOnSheet(Guid sheetId, Guid viewId)
        {
            bool updated = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = "UPDATE Views SET View_Sheet_Id = '" + sheetId.ToString() + "' WHERE View_Id = '" + viewId.ToString() + "'";
                    if (command.ExecuteNonQuery() > 0) { updated = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }

        public static bool ChangeReplaceItem(RevitItemMapper item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO ReplaceItems (ReplaceItem_Id, ReplaceItem_Type, ReplaceItem_Parameter, ReplaceItem_Source_Id, ReplaceItem_Source_Value, ReplaceItem_Target_Value) " +
                                "VALUES ('" + item.ItemId.ToString() + "', '" + item.ItemType.ToString() + "', '" + item.ParameterName + "', '"+item.SourceId+"', '" + item.SourceValue + "', '" + item.TargetValue + "')";
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
                            command.CommandText = @"INSERT INTO Sheets (Sheet_Id, Sheet_Number, Sheet_Name, Sheet_Discipline_Id)" +
                                "VALUES ('" + item.Id.ToString() + "', '" + item.Number + "', '" + item.Name + "', '" + item.DisciplineObj.Id.ToString() + "')";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE Sheets SET Sheet_Number = '" + item.Number + "', Sheet_Name = '" + item.Name + "', Sheet_Discipline_Id ='" + item.DisciplineObj.Id.ToString() + "'"+
                                " WHERE Sheet_Id = '" + item.Id.ToString() + "'";
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

        public static bool ChangeSheetItem(string sheetId, string propertyName, string propertyValue)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = @"UPDATE Sheets SET " + propertyName + " = '" + propertyValue + "' WHERE Sheet_Id = '" + sheetId + "'";
                    if (command.ExecuteNonQuery() > 0) { result = true; }
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
                                "VALUES ('" + item.Id.ToString() + "', '" + item.Description + "', '" + item.IssuedBy + "', '" + item.IssuedTo + "', '" + item.Date + "', '" + item.Document.Id.ToString() + "' )";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            if (item.Document.Id != Guid.Empty)
                            {
                                command.CommandText = @"INSERT OR REPLACE INTO RevisionDocuments (Document_Id, Document_Title, Document_Path) " +
                                    "VALUES ('" + item.Document.Id.ToString() + "', '" + item.Document.Title + "', '" + item.Document.Path + "')";
                                if (command.ExecuteNonQuery() > 0) { result = true; }
                            }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE Revisions SET Revision_Description = '" + item.Description + "', Revision_IssuedBy ='" + item.IssuedBy + "', Revision_IssuedTo = '" + item.IssuedTo + "', Revision_Date = '" + item.Date + "', " +
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

        public static bool ChangeRevisionItem(string revisionId, string propertyName, object propertyValue)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = @"UPDATE Revisions SET "+propertyName+" = '" + propertyValue + "' WHERE Revision_Id = '" + revisionId + "'";
                    if (command.ExecuteNonQuery() > 0) { result = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        public static bool UpdateRevisionDocument(RevitRevision rvtRevision, RevisionDocument revisionDoc)
        {
            bool updated = false;
            try
            {
                if (null != connection && null != command)
                {
                    if (null != revisionDoc)
                    {
                        command.CommandText = @"UPDATE Revisions SET Revision_Document_Id = '" + revisionDoc.Id.ToString() + "' WHERE Revision_Id = '" + rvtRevision.Id.ToString() + "'";
                        if (command.ExecuteNonQuery() > 0) { updated = true; }

                        command.CommandText = @"INSERT OR REPLACE INTO RevisionDocuments (Document_Id, Document_Title, Document_Path) " +
                            "VALUES ('" + revisionDoc.Id.ToString() + "', '" + revisionDoc.Title + "', '" + revisionDoc.Path + "')";
                        if (command.ExecuteNonQuery() > 0) { updated = true; }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
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
                            command.CommandText = @"UPDATE Discipline SET Discipline_Name = '" + item.Name + "' " +
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
                            command.CommandText = @"INSERT INTO Views (View_Id, View_Name, View_Sheet_Id, View_ViewType_Id, View_LocationX, View_LocationY) " +
                                "VALUES ('" + item.Id.ToString() + "', '" + item.Name + "', '" + item.Sheet.Id.ToString() + "', '" + item.ViewType.Id.ToString() + "', " + item.LocationU + ", " + item.LocationV + ")";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE Views SET View_Name = '" + item.Name + "', View_Sheet_Id = '" + item.Sheet.Id.ToString() + "', View_ViewType_Id = '" + item.ViewType.Id.ToString() + "', View_LocationX = " + item.LocationU + ", View_LocationY =" + item.LocationV +
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

        public static bool InsertMultipleViewItems(List<RevitView> viewItems)
        {
            bool inserted = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = @"INSERT INTO Views (View_Id, View_Name, View_Sheet_Id, View_ViewType_Id, View_LocationX, View_LocationY) VALUES";
                    foreach (RevitView viewItem in viewItems)
                    {
                        command.CommandText += "('" + viewItem.Id.ToString() + "', '" + viewItem.Name + "', '" + viewItem.Sheet.Id.ToString() + "','" + viewItem.ViewType.Id.ToString() + "', "+viewItem.LocationU+", "+viewItem.LocationV+"),";
                    }
                    command.CommandText = command.CommandText.Remove(command.CommandText.Length - 1);
                    if (command.ExecuteNonQuery() > 0) { inserted = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        public static bool ChangeViewItem(string viewId, string propertyName, object propertyValue)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (propertyName)
                    {
                        case "View Name":
                            command.CommandText = @"UPDATE Views SET View_Name = '" + propertyValue + "' WHERE View_Id = '" + viewId + "'";
                            break;
                        case "Sheet Number":
                            command.CommandText = @"UPDATE Views SET View_Sheet_Id = '" + propertyValue + "' WHERE View_Id = '" + viewId + "'";
                            break;
                        case "View Type":
                            command.CommandText = @"UPDATE Views SET View_ViewType_Id = '" + propertyValue + "' WHERE View_Id = '" + viewId + "'";
                            break;
                        case "X":
                            command.CommandText = @"UPDATE Views SET View_LocationX = " + propertyValue + " WHERE View_Id = '" + viewId + "'";
                            break;
                        case "Y":
                            command.CommandText = @"UPDATE Views SET View_LocationY = " + propertyValue + " WHERE View_Id = '" + viewId + "'";
                            break;

                    }
                    
                    if (command.ExecuteNonQuery() > 0) { result = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        public static bool ChangeSheetParameter(SheetParameter item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO SheetParameters (Parameter_Id, Parameter_Name, Parameter_Type)" +
                                "VALUES ('" + item.ParameterId.ToString() + "', '" + item.ParameterName + "', '" + item.ParameterType + "')";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE SheetParameters SET Parameter_Name = '" + item.ParameterName + "', Parameter_Type = '" + item.ParameterType + "'"+
                            " WHERE Parameter_Id = '" + item.ParameterId.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM SheetParameters WHERE Parameter_Id = '" + item.ParameterId.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            command.CommandText = "DELETE FROM SheetParameterValues WHERE ParameterValue_Parameter_Id = '" + item.ParameterId.ToString() + "'";
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

        public static bool ChangeSheetParameterValue(SheetParameterValue item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO SheetParameterValues (ParameterValue_Id, ParameterValue_Sheet_Id, ParameterValue_Parameter_Id, ParameterValue_Parameter_Value)" +
                                "VALUES ('" + item.ParameterValueId.ToString() + "', '" + item.SheetId.ToString() + "', '" + item.Parameter.ParameterId.ToString() + "', '"+item.ParameterValue+"')";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE SheetParameterValues SET ParameterValue_Parameter_Value = '" + item.ParameterValue + "'" +
                            " WHERE ParameterValue_Id = '" + item.ParameterValueId.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM SheetParameterValues WHERE ParameterValue_Id = '" + item.ParameterValueId.ToString() + "'";
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

        public static bool DeleteSheetParameterValue(string sheetId)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = "DELETE FROM SheetParameterValues WHERE ParameterValue_Sheet_Id = '" + sheetId + "'";
                    if (command.ExecuteNonQuery() > 0) { result = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        public static bool InsertMultipleParameterValue(List<SheetParameterValue> paramValues)
        {
            bool inserted = false;
            try
            {
                if (null != connection && null != command && paramValues.Count > 0)
                {
                    string cmdText = @"INSERT INTO SheetParameterValues (ParameterValue_Id, ParameterValue_Sheet_Id, ParameterValue_Parameter_Id, ParameterValue_Parameter_Value) VALUES";
                    foreach (SheetParameterValue paramValue in paramValues)
                    {
                        cmdText += "('" + paramValue.ParameterValueId.ToString() + "', '" + paramValue.SheetId.ToString() + "', '" + paramValue.Parameter.ParameterId.ToString() + "','"+paramValue.ParameterValue+"'),";
                    }
                    cmdText = cmdText.Remove(cmdText.Length - 1);
                    command.CommandText = cmdText;

                    if (command.ExecuteNonQuery() > 0) { inserted = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        public static bool ChangeRevisionOnSheet(RevisionOnSheet ros, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO RevisionsOnSheet(RevisionsOnSheet_Id, RevisionsOnSheet_Sheet_Id, RevisionsOnSheet_Revision_Id, RevisionsOnSheet_Include) " +
                                          "VALUES ('" + ros.MapId.ToString() + "', '" + ros.SheetId.ToString() + "', '" + ros.RvtRevision.Id.ToString() + "', @include)";
                            command.Parameters["@include"].Value = ros.Include;
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE RevisionsOnSheet SET RevisionsOnSheet_Include = @include WHERE RevisionsOnSheet_Id ='" + ros.MapId.ToString() + "'";
                            command.Parameters["@include"].Value = ros.Include;
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM RevisionsOnSheet WHERE RevisionsOnSheet_Id = '" + ros.MapId.ToString() + "'";
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

        public static bool InsertMultipleRevisionOnSheet(List<RevisionOnSheet> rosList)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command && rosList.Count > 0)
                {
                    string cmdText = "";
                    cmdText = @"INSERT INTO RevisionsOnSheet(RevisionsOnSheet_Id, RevisionsOnSheet_Sheet_Id, RevisionsOnSheet_Revision_Id, RevisionsOnSheet_Include) VALUES";
                    foreach (RevisionOnSheet ros in rosList)
                    {
                        cmdText += "('" + ros.MapId.ToString() + "', '" + ros.SheetId.ToString() + "', '" + ros.RvtRevision.Id.ToString() + "', @include),";
                        command.Parameters["@include"].Value = ros.Include;
                    }
                    cmdText = cmdText.Remove(cmdText.Length - 1);
                    command.CommandText = cmdText;

                    if (command.ExecuteNonQuery() > 0) { result = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        public static bool DeleteRevisionOnSheet(string propertyId, string idValue)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    command.CommandText = @"DELETE FROM RevisionsOnSheet WHERE " + propertyId + " = '" + idValue + "'";
                    if (command.ExecuteNonQuery() > 0) { result = true; }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        #region Linked Items
        public static bool ChangeLinkedProject(LinkedProject item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO LinkedProjects (LinkedProject_Id, LinkedProject_Number, LinkedProject_Name, LinkedProject_FilePath, LinkedProject_LinkedDate, LinkedProject_LinkedBy)" +
                                "VALUES ('" + item.Id.ToString() + "', '" + item.ProjectNumber + "', '" + item.ProjectName + "', '" + item.FilePath + "', @date, '"+item.LinkedBy+"')";
                            command.Parameters["@date"].Value = item.LinkedDate;
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE LinkedProjects SET LinkedProject_Number = '" + item.ProjectNumber + "', LinkedProject_Name = '" + item.ProjectName + "', "+
                                "LinkedProject_FilePath ='" + item.FilePath + "', LinkedProject_LinkedDate = @date, LinkedProject_LinkedBy = '"+item.LinkedBy+"'"+
                                " WHERE LinkedProject_Id = '" + item.Id.ToString() + "'";
                            command.Parameters["@date"].Value = item.LinkedDate;
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM LinkedProjects WHERE LinkedProject_Id = '" + item.Id.ToString() + "'";
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

        public static bool ChangeLinkedRevision(LinkedRevision item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO LinkedRevisions (LinkedRevision_Id, LinkedRevision_Sequence, LinkedRevision_Number, LinkedRevision_NumberType, LinkedRevision_Revision_Id, LinkedRevision_Project_Id, LinkedRevision_Element_Id, LinkedRevision_IsSource)" +
                                "VALUES ('" + item.Id.ToString() + "', " + item.Sequence + ", '" + item.Number + "', '" + item.NumberType.ToString() + "', '"+item.RevisionId.ToString()+"', '" + item.LinkProject.Id.ToString() + "', '"+item.LinkedElementId+"', @isSource)";
                            command.Parameters["@isSource"].Value = item.IsSource;
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE LinkedRevisions SET LinkedRevision_Sequence = " + item.Sequence + ", LinkedRevision_Number = '" + item.Number + "', " +
                                "LinkedRevision_NumberType ='" + item.NumberType.ToString() + "', LinkedRevision_Revision_Id = '"+item.RevisionId.ToString()+"', LinkedRevision_Project_Id = '" + item.LinkProject.Id.ToString() + "', " +
                                "LinkedRevision_Element_Id = '"+ item.LinkedElementId+"', LinkedRevision_IsSource = @isSource"+
                                " WHERE LinkedRevision_Id = '" + item.Id.ToString() + "'";
                            command.Parameters["@isSource"].Value = item.IsSource;
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM LinkedRevisions WHERE LinkedRevision_Id = '" + item.Id.ToString() + "'";
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

        public static bool ChangeLinkedRevision( Guid id, string propertyName, string propertyValue, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE LinkedRevisions SET " + propertyName + " = '" + propertyValue + "' WHERE LinkedRevision_Id = '" + id.ToString() + "'";
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM LinkedRevisions WHERE "+propertyName+" = '" + propertyValue + "'";
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

        public static bool ChangeLinkedSheet(LinkedSheet item, CommandType cmdType)
        {
            bool result = false;
            try
            {
                if (null != connection && null != command)
                {
                    switch (cmdType)
                    {
                        case CommandType.INSERT:
                            command.CommandText = @"INSERT INTO LinkedSheets (LinkedSheet_Id, LinkedSheet_Sheet_Id, LinkedSheet_Project_Id, LinkedSheet_Element_Id, LinkedSheet_IsSource)" +
                                "VALUES ('" + item.Id.ToString() + "', '" + item.SheetId.ToString() + "', '" + item.LinkProject.Id.ToString() + "', '" + item.LinkedElementId.ToString() + "', @isSource)";
                            command.Parameters["@isSource"].Value = item.IsSource;
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.UPDATE:
                            command.CommandText = @"UPDATE LinkedSheets SET LinkedSheet_Sheet_Id = '" + item.SheetId.ToString() + "', LinkedSheet_Project_Id = '" + item.LinkProject.Id.ToString() + "', " +
                                "LinkedSheet_Element_Id ='" + item.LinkedElementId + "', LinkedSheet_IsSource = @isSource" +
                                " WHERE LinkedSheet_Id = '" + item.Id.ToString() + "'";
                            command.Parameters["@isSource"].Value = item.IsSource;
                            if (command.ExecuteNonQuery() > 0) { result = true; }
                            break;
                        case CommandType.DELETE:
                            command.CommandText = "DELETE FROM LinkedSheets WHERE LinkedSheet_Id = '" + item.Id.ToString() + "'";
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
        #endregion
    }
}
