using HOK.SheetManager.Classes;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.AddIn.Updaters
{
    public class UpdaterDataManager
    {
        private string dbFile = "";
        private SQLiteConnection connection = null;
        private SQLiteCommand command = null;

        public UpdaterDataManager(string file)
        {
            dbFile = file;
            bool opened = OpenDatabase(dbFile);
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
                    connection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;", true);
                    connection.Open();

                    command = new SQLiteCommand(connection);

                    opened = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return opened;
        }

        public List<LinkedProject> GetLinkedProjects()
        {
            List<LinkedProject> projects = new List<LinkedProject>();
            try
            {
                if (null != connection && null != command)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        cmd.CommandText = "SELECT * FROM LinkedProjects";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
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

                                projects.Add(project);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return projects;
        }

        public List<Guid> GetSheetIds()
        {
            List<Guid> sheetIds = new List<Guid>();
            try
            {
                if (null != connection && null != command)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        cmd.CommandText = "SELECT Sheet_Id FROM Sheets";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Guid sheetId = reader.GetGuid(reader.GetOrdinal("Sheet_Id"));
                                sheetIds.Add(sheetId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return sheetIds;
        }

        public List<SheetParameter> GetSheetParameters()
        {
            List<SheetParameter> parameters = new List<SheetParameter>();
            try
            {
                if (null != connection && null != command)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        cmd.CommandText = "SELECT * FROM SheetParameters";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SheetParameter parameter = new SheetParameter();
                                parameter.ParameterId = reader.GetGuid(reader.GetOrdinal("Parameter_Id"));
                                parameter.ParameterName = reader.GetString(reader.GetOrdinal("Parameter_Name"));
                                parameter.ParameterType = reader.GetString(reader.GetOrdinal("Parameter_Type"));
                                parameters.Add(parameter);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return parameters;
        }

        public List<Guid> GetRevisionIds()
        {
            List<Guid> revisionIds = new List<Guid>();
            try
            {
                if (null != connection && null != command)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        cmd.CommandText = "SELECT Revision_Id FROM Revisions";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Guid revisionId = reader.GetGuid(reader.GetOrdinal("Revision_Id"));
                                revisionIds.Add(revisionId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return revisionIds;
        }

        public LinkedRevision GetLinkedRevision(string uniqueId, Guid projectGuid)
        {
            LinkedRevision lRevision = null;
            try
            {
                if (null != connection && null != command)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        cmd.CommandText = "SELECT * FROM LinkedRevisions WHERE LinkedRevision_Element_Id= '" + uniqueId + "' AND LinkedRevision_Project_Id = '" + projectGuid.ToString() + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lRevision = new LinkedRevision();
                                lRevision.Id = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Id"));
                                lRevision.RevisionId = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Revision_Id"));
                                lRevision.Sequence = reader.GetInt32(reader.GetOrdinal("LinkedRevision_Sequence"));
                                lRevision.Number = reader.GetString(reader.GetOrdinal("LinkedRevision_Number"));
                                lRevision.NumberType = (NumberType)Enum.Parse(typeof(NumberType), reader.GetString(reader.GetOrdinal("LinkedRevision_NumberType")));

                                Guid projectId = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Project_Id"));
                                lRevision.LinkProject = new LinkedProject(projectId);

                                lRevision.LinkedElementId = reader.GetString(reader.GetOrdinal("LinkedRevision_Element_Id"));
                                lRevision.IsSource = reader.GetBoolean(reader.GetOrdinal("LinkedRevision_IsSource"));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return lRevision;
        }

        public LinkedSheet GetLinkedSheet(string uniqueId, Guid projectGuid)
        {
            LinkedSheet lSheet = null;
            try
            {
                if (null != connection && null != command)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        cmd.CommandText = "SELECT * FROM LinkedSheets WHERE LinkedSheet_Element_Id= '" + uniqueId + "' AND LinkedSheet_Project_Id = '" + projectGuid.ToString() + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lSheet = new LinkedSheet();
                                lSheet.Id = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Id"));
                                lSheet.SheetId = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Sheet_Id"));
                                Guid projectId = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Project_Id"));
                                lSheet.LinkProject = new LinkedProject(projectGuid);

                                lSheet.LinkedElementId = reader.GetString(reader.GetOrdinal("LinkedSheet_Element_Id"));
                                lSheet.IsSource = reader.GetBoolean(reader.GetOrdinal("LinkedSheet_IsSource"));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return lSheet;
        }

        public SheetParameterValue GetSheetParameterValue(Guid paramId, Guid sheetId)
        {
            SheetParameterValue paramValue = null;
            try
            {
                if (null != connection && null != command)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        cmd.CommandText = "SELECT * FROM SheetParameterValues WHERE ParameterValue_Parameter_Id= '" + paramId.ToString() + "' AND ParameterValue_Sheet_Id = '" + sheetId.ToString() + "'";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                paramValue = new SheetParameterValue();
                                paramValue.ParameterValueId = reader.GetGuid(reader.GetOrdinal("ParameterValue_Id"));
                                paramValue.SheetId = reader.GetGuid(reader.GetOrdinal("ParameterValue_Sheet_Id"));
                                Guid parameterId = reader.GetGuid(reader.GetOrdinal("ParameterValue_Parameter_Id"));
                                paramValue.Parameter = new SheetParameter() { ParameterId = parameterId };
                                paramValue.ParameterValue = reader.GetString(reader.GetOrdinal("ParameterValue_Parameter_Value"));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return paramValue;
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
                connection = null;
                command = null;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
