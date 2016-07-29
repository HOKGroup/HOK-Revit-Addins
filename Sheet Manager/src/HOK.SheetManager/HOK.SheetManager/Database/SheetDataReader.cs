using HOK.SheetManager.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Database
{
    public static class SheetDataReader
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

        public static RevitSheetData ReadSheetDatabase(string dbFile, RevitSheetData sheetData)
        {
            try
            {
                string connectionStr = GetConnectionString(dbFile);
                using (connection = new SQLiteConnection(connectionStr, true))
                {
                    connection.Open();
                    sheetData.FilePath = dbFile;

                    ReadDiscipline(ref sheetData);
                    ReadLinkedProjects(ref sheetData);
                    ReadRevisions(ref sheetData);
                    ReadSheetParameters(ref sheetData);
                    ReadRevitSheets(ref sheetData);
                    ReadViewTypes(ref sheetData);
                    ReadViews(ref sheetData);
                    ReadReplaceItems(ref sheetData);

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return sheetData;
        }

        private static void ReadDiscipline(ref RevitSheetData sheetData)
        {
            try
            {
                sheetData.Disciplines.Clear();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Discipline";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Discipline discipline = new Discipline();
                            discipline.Id = reader.GetGuid(reader.GetOrdinal("Discipline_Id"));
                            discipline.Name = reader.GetString(reader.GetOrdinal("Discipline_Name"));
                            sheetData.Disciplines.Add(discipline);
                        }
                    }
                }
                sheetData.Disciplines = new ObservableCollection<Discipline>(sheetData.Disciplines.OrderBy(o => o.Name));
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static void ReadLinkedProjects(ref RevitSheetData sheetData)
        {
            try
            {
                sheetData.LinkedProjects.Clear();
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

                            sheetData.LinkedProjects.Add(project);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static void ReadSheetParameters(ref RevitSheetData sheetData)
        {
            try
            {
                sheetData.SheetParameters.Clear();
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
                            sheetData.SheetParameters.Add(parameter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static void ReadRevitSheets(ref RevitSheetData sheetData)
        {
            try
            {
                ObservableCollection<LinkedSheet> linkedSheets = new ObservableCollection<LinkedSheet>();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM LinkedSheets";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LinkedSheet lSheet = new LinkedSheet();
                            lSheet.Id = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Id"));
                            lSheet.SheetId = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Sheet_Id"));
                            Guid projectId = reader.GetGuid(reader.GetOrdinal("LinkedSheet_Project_Id"));
                            var projectFound = from project in sheetData.LinkedProjects where project.Id == projectId select project;
                            if (projectFound.Count() > 0)
                            {
                                lSheet.LinkProject = projectFound.First();
                            }
                            
                            lSheet.LinkedElementId = reader.GetString(reader.GetOrdinal("LinkedSheet_Element_Id"));
                            lSheet.IsSource = reader.GetBoolean(reader.GetOrdinal("LinkedSheet_IsSource"));

                            linkedSheets.Add(lSheet);
                        }
                    }
                }

                ObservableCollection<SheetParameterValue> sheetParameterValues = new ObservableCollection<SheetParameterValue>();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM SheetParameterValues";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SheetParameterValue paramValue = new SheetParameterValue();
                            paramValue.ParameterValueId = reader.GetGuid(reader.GetOrdinal("ParameterValue_Id"));
                            paramValue.SheetId = reader.GetGuid(reader.GetOrdinal("ParameterValue_Sheet_Id"));
                            Guid parameterId = reader.GetGuid(reader.GetOrdinal("ParameterValue_Parameter_Id"));
                            var paramFound = from param in sheetData.SheetParameters where param.ParameterId == parameterId select param;
                            if (paramFound.Count() > 0)
                            {
                                paramValue.Parameter = paramFound.First();
                            }
                            paramValue.ParameterValue = reader.GetString(reader.GetOrdinal("ParameterValue_Parameter_Value"));
                            sheetParameterValues.Add(paramValue);
                        }
                    }
                }

                ObservableCollection<RevisionOnSheet> revisionOnSheets = new ObservableCollection<RevisionOnSheet>();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM RevisionsOnSheet";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RevisionOnSheet ros = new RevisionOnSheet();
                            ros.MapId = reader.GetGuid(reader.GetOrdinal("RevisionsOnSheet_Id"));
                            ros.SheetId = reader.GetGuid(reader.GetOrdinal("RevisionsOnSheet_Sheet_Id"));

                            Guid revisionId = reader.GetGuid(reader.GetOrdinal("RevisionsOnSheet_Revision_Id"));
                            var revisionFound = from rev in sheetData.Revisions where rev.Id == revisionId select rev;
                            if (revisionFound.Count() > 0)
                            {
                                ros.RvtRevision = revisionFound.First();
                            }
                            ros.Include = reader.GetBoolean(reader.GetOrdinal("RevisionsOnSheet_Include"));

                            revisionOnSheets.Add(ros);
                        }
                    }
                }
                

                sheetData.Sheets.Clear();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Sheets";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RevitSheet sheet = new RevitSheet();
                            sheet.Id = reader.GetGuid(reader.GetOrdinal("Sheet_Id"));
                            sheet.Number = reader.GetString(reader.GetOrdinal("Sheet_Number"));
                            sheet.Name = reader.GetString(reader.GetOrdinal("Sheet_Name"));
                  
                            Guid disciplineId = reader.GetGuid(reader.GetOrdinal("Sheet_Discipline_Id"));
                            var disciplineFound = from discipline in sheetData.Disciplines where discipline.Id == disciplineId select discipline;
                            if (disciplineFound.Count() > 0)
                            {
                                sheet.DisciplineObj = disciplineFound.First();
                            }

                            var linkedSheetFound = from linkedSheet in linkedSheets where linkedSheet.SheetId == sheet.Id select linkedSheet;
                            if (linkedSheetFound.Count() > 0)
                            {
                                sheet.LinkedSheets = new ObservableCollection<LinkedSheet>(linkedSheetFound.ToList());
                            }

                            foreach (SheetParameter param in sheetData.SheetParameters)
                            {
                                var parameterValueFound = from paramValue in sheetParameterValues where paramValue.SheetId == sheet.Id && paramValue.Parameter.ParameterId == param.ParameterId select paramValue;
                                if (parameterValueFound.Count() > 0)
                                {
                                    SheetParameterValue value = parameterValueFound.First();
                                    sheet.SheetParameters.Add(param.ParameterId, value);
                                }
                            }

                            var revisionOnSheetFound = from ros in revisionOnSheets where ros.SheetId == sheet.Id select ros;
                            if (revisionOnSheetFound.Count() > 0)
                            {
                                foreach (RevisionOnSheet ros in revisionOnSheetFound)
                                {
                                    if (!sheet.SheetRevisions.ContainsKey(ros.RvtRevision.Id))
                                    {
                                        sheet.SheetRevisions.Add(ros.RvtRevision.Id, ros);
                                    }
                                }
                            }

                            sheetData.Sheets.Add(sheet);
                        }
                    }
                }
                sheetData.Sheets = new ObservableCollection<RevitSheet>(sheetData.Sheets.OrderBy(o => o.Number));
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static void ReadRevisions(ref RevitSheetData sheetData)
        {
            try
            {
                ObservableCollection<LinkedRevision> linkedRevisions = new ObservableCollection<LinkedRevision>();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM LinkedRevisions";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LinkedRevision lRevision = new LinkedRevision();
                            lRevision.Id = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Id"));
                            lRevision.RevisionId = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Revision_Id"));
                            lRevision.Sequence = reader.GetInt32(reader.GetOrdinal("LinkedRevision_Sequence"));
                            lRevision.Number = reader.GetString(reader.GetOrdinal("LinkedRevision_Number"));
                            lRevision.NumberType = (NumberType)Enum.Parse(typeof(NumberType), reader.GetString(reader.GetOrdinal("LinkedRevision_NumberType")));

                            Guid projectId = reader.GetGuid(reader.GetOrdinal("LinkedRevision_Project_Id"));
                            var projectFound = from project in sheetData.LinkedProjects where project.Id == projectId select project;
                            if (projectFound.Count() > 0)
                            {
                                LinkedProject project = projectFound.First();
                                lRevision.LinkProject = project;
                            }

                            lRevision.LinkedElementId = reader.GetString(reader.GetOrdinal("LinkedRevision_Element_Id"));
                            lRevision.IsSource = reader.GetBoolean(reader.GetOrdinal("LinkedRevision_IsSource"));

                            linkedRevisions.Add(lRevision);
                        }
                    }
                }

                ObservableCollection<RevisionDocument> revisionDocuments = new ObservableCollection<RevisionDocument>();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM RevisionDocuments";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RevisionDocument document = new RevisionDocument();
                            document.Id = reader.GetGuid(reader.GetOrdinal("Document_Id"));
                            document.Title = reader.GetString(reader.GetOrdinal("Document_Title"));
                            document.Path = reader.GetString(reader.GetOrdinal("Document_Path"));
                            if (reader["Document_Contents"] != System.DBNull.Value)
                            {
                                document.Contents = (byte[])reader["Document_Contents"];
                            }

                            revisionDocuments.Add(document);
                        }
                    }
                }

                sheetData.Revisions.Clear();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Revisions";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RevitRevision revision = new RevitRevision();
                            revision.Id = reader.GetGuid(reader.GetOrdinal("Revision_Id"));
                            revision.Description = reader.GetString(reader.GetOrdinal("Revision_Description"));
                            revision.IssuedBy = reader.GetString(reader.GetOrdinal("Revision_IssuedBy"));
                            revision.IssuedTo = reader.GetString(reader.GetOrdinal("Revision_IssuedTo"));
                            revision.Date = reader.GetString(reader.GetOrdinal("Revision_Date"));

                            Guid documentId = reader.GetGuid(reader.GetOrdinal("Revision_Document_Id"));
                            var documentFound = from document in revisionDocuments where document.Id == documentId select document;
                            if (documentFound.Count() > 0)
                            {
                                revision.Document = documentFound.First();
                            }

                            var linkedRevisionFound = from link in linkedRevisions where link.RevisionId == revision.Id select link;
                            if (linkedRevisionFound.Count() > 0)
                            {
                                revision.LinkedRevisions = new ObservableCollection<LinkedRevision>(linkedRevisionFound.ToList());
                            }
                            sheetData.Revisions.Add(revision);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static void ReadViewTypes(ref RevitSheetData sheetData)
        {
            try
            {
                sheetData.ViewTypes.Clear();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM ViewTypes";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RevitViewType viewType = new RevitViewType();
                            viewType.Id = reader.GetGuid(reader.GetOrdinal("ViewType_Id"));
                            viewType.Name = reader.GetString(reader.GetOrdinal("ViewType_Name"));
                            viewType.ViewType = (ViewTypeEnum)Enum.Parse(typeof(ViewTypeEnum), reader.GetString(reader.GetOrdinal("ViewType_Enum")));
                            sheetData.ViewTypes.Add(viewType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static void ReadViews(ref RevitSheetData sheetData)
        {
            try
            {
                sheetData.Views.Clear();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM Views";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RevitView view = new RevitView();
                            view.Id = reader.GetGuid(reader.GetOrdinal("View_Id"));
                            view.Name = reader.GetString(reader.GetOrdinal("View_Name"));
                            Guid sheetId = reader.GetGuid(reader.GetOrdinal("View_Sheet_Id"));
                            if (sheetId != Guid.Empty)
                            {
                                var sheetFound = from sheet in sheetData.Sheets where sheet.Id == sheetId select sheet;
                                if (sheetFound.Count() > 0)
                                {
                                    view.Sheet = sheetFound.First();
                                }
                            }

                            Guid viewTypeId = reader.GetGuid(reader.GetOrdinal("View_ViewType_Id"));
                            if (viewTypeId != Guid.Empty)
                            {
                                var viewTypeFound = from viewType in sheetData.ViewTypes where viewType.Id == viewTypeId select viewType;
                                if (viewTypeFound.Count() > 0)
                                {
                                    view.ViewType = viewTypeFound.First();
                                }
                            }

                            view.LocationU = reader.GetDouble(reader.GetOrdinal("View_LocationX"));
                            view.LocationV = reader.GetDouble(reader.GetOrdinal("View_LocationY"));

                            sheetData.Views.Add(view);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void ReadReplaceItems(ref RevitSheetData sheetData)
        {
            try
            {
                sheetData.ItemMaps.Clear();
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "SELECT * FROM ReplaceItems";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RevitItemMapper item = new RevitItemMapper();
                            item.ItemId = reader.GetGuid(reader.GetOrdinal("ReplaceItem_Id"));
                            item.ItemType = (MappingType)Enum.Parse(typeof(MappingType), reader.GetString(reader.GetOrdinal("ReplaceItem_Type")));
                            item.ParameterName = reader.GetString(reader.GetOrdinal("ReplaceItem_Parameter"));
                            item.SourceValue = reader.GetString(reader.GetOrdinal("ReplaceItem_Source_Value"));
                            item.TargetValue = reader.GetString(reader.GetOrdinal("ReplaceItem_Target_Value"));
                            sheetData.ItemMaps.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }
}
