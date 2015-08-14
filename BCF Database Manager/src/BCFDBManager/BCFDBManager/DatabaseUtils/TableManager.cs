using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCFDBManager.DatabaseUtils
{
    public enum TableNames //index is the sequnce of the tables to be created for the implementation of foreign keys
    {
        //project
        ProjectExtension = 0, Project = 1,

        //version
        Version = 2,

        //markup
        Topic = 3, Labels = 4, HeaderFile =5, BimSnippet =6, DocumentReferences = 7, RelatedTopics = 8, Comment = 9, Viewpoint = 10, Viewpoints = 11, 

        //visinfo
        Bitmaps = 12, Point = 13, Direction = 14, ClippingPlane = 15, Components = 16, Lines = 17, OrthogonalCamera = 18, PerspectiveCamera = 19,

        //customInfo
        BCFFileInfo = 20, FileTopics = 21
    }
   
    public static class TableManager
    {
        private static Dictionary<string, TableProperties> tablesInfo = new Dictionary<string, TableProperties>();

        public static Dictionary<string, TableProperties> TablesInfo { get { return tablesInfo; } set { tablesInfo = value; } }
        
        public static bool GetTablesInfo()
        {
            bool result = false;
            try
            {
                bool ptCollected = GetProjectTablesInfo();
                bool vtCollected = GetVersionTablesInfo();
                bool mtCollected = GetMarkupTablesInfo();
                bool vitCollected = GetVisualizationInfoTablesInfo();
                bool bitCollected = GetBCFFileInfoTablesInfo();

                if (ptCollected && vtCollected && bitCollected && bitCollected && mtCollected && vitCollected)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private static bool GetProjectTablesInfo()
        {
            bool result = false;
            try
            {
                #region Project
                string tableName = TableNames.Project.ToString();
                TableProperties tp = new TableProperties(tableName, false);

                FieldProperties fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //guid
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ProjectId", SQLiteDataTypeEnum.TEXT, false, true); //guid
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ProjectName", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ProjectExtension_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("ProjectExtension", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region ProjectExtension
                tableName = TableNames.ProjectExtension.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //guid
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ExtensionSchema", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion
                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private static bool GetVersionTablesInfo()
        {
            bool result = false;
            try
            {
                #region Version
                string tableName = TableNames.Version.ToString();
                TableProperties tp = new TableProperties(tableName, false);

                FieldProperties fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("VersionId", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("DetailedVersion", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion
                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private static bool GetBCFFileInfoTablesInfo()
        {
            bool result = false;
            try
            {
                #region BCFFileInfo
                string tableName = TableNames.BCFFileInfo.ToString();
                TableProperties tp = new TableProperties(tableName, true);

                FieldProperties fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("FileName", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("FilePath", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("UploadedBy", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("UploadedDate", SQLiteDataTypeEnum.DATETIME, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CreationDate", SQLiteDataTypeEnum.DATETIME, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Project_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Project", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Version_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Version", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region FileTopics
                tableName = TableNames.FileTopics.ToString();
                tp = new TableProperties(tableName, true);

                fp = new FieldProperties("Topic_Guid", SQLiteDataTypeEnum.TEXT, false, true);
                fp.ForeignKey = new ForeignKeyInfo("Topic", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("File_Guid", SQLiteDataTypeEnum.TEXT, false, true);
                fp.ForeignKey = new ForeignKeyInfo("BCFFileInfo", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion
                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private static bool GetMarkupTablesInfo()
        {
            bool result = false;
            try
            {
                //  Topic = 3, HeaderFile = 4, BimSnippet = 5, DocumentReferences = 6, RelatedTopics = 7, Comment = 8, Viewpoint = 9, Viewpoints = 10
                #region HeaderFile
                string tableName = TableNames.HeaderFile.ToString();
                TableProperties tp = new TableProperties(tableName, false);

                FieldProperties fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("IfcProject", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("IfcSpatialStructureElement", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("isExternal", SQLiteDataTypeEnum.BOOLEAN, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("FileName", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Date", SQLiteDataTypeEnum.DATETIME, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Reference", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Topic_Guid", SQLiteDataTypeEnum.TEXT, false, false); //custom field
                fp.ForeignKey = new ForeignKeyInfo("Topic", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); } 

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region Topic
                tableName = TableNames.Topic.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("TopicType", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("TopicStatus", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Title", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ReferenceLink", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Description", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Priority", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("TopicIndex", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CreationDate", SQLiteDataTypeEnum.DATETIME, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CreationAuthor", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ModifiedDate", SQLiteDataTypeEnum.DATETIME, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ModifiedAuthor", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("AssignedTo", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region Labels
                tableName = TableNames.Labels.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Label", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Topic_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Topic", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region BimSnippet
                tableName = TableNames.BimSnippet.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("SnippetType", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("isExternal", SQLiteDataTypeEnum.BOOLEAN, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Reference", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ReferenceSchema", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("FileContent", SQLiteDataTypeEnum.BLOB, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Topic_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Topic", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region DocumentReferences
                tableName = TableNames.DocumentReferences.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); 
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("isExternal", SQLiteDataTypeEnum.BOOLEAN, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ReferenceDocument", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("FileContent", SQLiteDataTypeEnum.BLOB, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Description", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Topic_Guid", SQLiteDataTypeEnum.TEXT, false, false); //custom field
                fp.ForeignKey = new ForeignKeyInfo("Topic", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region RelatedTopics
                tableName = TableNames.RelatedTopics.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); 
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Topic_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Topic", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region Comment
                tableName = TableNames.Comment.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("VerbalStatus", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Status", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Date", SQLiteDataTypeEnum.DATETIME, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Author", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Comment", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ModifiedDate", SQLiteDataTypeEnum.DATETIME, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ModifiedAuthor", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Topic_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Topic", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region Viewpoints
                tableName = TableNames.Viewpoints.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Viewpoint", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Snapshot", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Snapshot_Image", SQLiteDataTypeEnum.BLOB, false, false); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Topic_Guid", SQLiteDataTypeEnum.TEXT, false, false); //custom field
                fp.ForeignKey = new ForeignKeyInfo("Topic", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region Viewpoint (CommentViewPoint)
                tableName = TableNames.Viewpoint.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Comment_Guid", SQLiteDataTypeEnum.TEXT, false, false); //custom field
                fp.ForeignKey = new ForeignKeyInfo("Comment", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private static bool GetVisualizationInfoTablesInfo()
        {
            bool result = false;
            try
            {
                //Bitmaps = 11, Point = 12, Direction = 13, ClippingPlane = 14, Components = 15, Lines = 16, OrthogonalCamera = 17, PerspectiveCamera = 18
                #region Bitmaps (VisualizationInfoBitmaps)
                string tableName = TableNames.Bitmaps.ToString();
                TableProperties tp = new TableProperties(tableName, false);

                FieldProperties fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); } //custom field: concatenated file name

                fp = new FieldProperties("Bitmap", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Bitmap_Image", SQLiteDataTypeEnum.BLOB, false, false); //custom field image source
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Reference", SQLiteDataTypeEnum.TEXT, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Location", SQLiteDataTypeEnum.TEXT, false, true);
                fp.ForeignKey = new ForeignKeyInfo("Point", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Normal", SQLiteDataTypeEnum.TEXT, false, true);
                fp.ForeignKey = new ForeignKeyInfo("Direction", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Up", SQLiteDataTypeEnum.TEXT, false, true);
                fp.ForeignKey = new ForeignKeyInfo("Direction", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Height", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Viewpoints_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Viewpoints", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region Point
                tableName = TableNames.Point.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("X", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Y", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Z", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region Direction
                tableName = TableNames.Direction.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("X", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Y", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Z", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region ClippingPlane
                tableName = TableNames.ClippingPlane.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Location", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Point", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Direction", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Direction", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Viewpoints_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Viewpoints", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region Components
                tableName = TableNames.Components.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custiom guid
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("IfcGuid", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Selected", SQLiteDataTypeEnum.BOOLEAN, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Visible", SQLiteDataTypeEnum.BOOLEAN, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Color", SQLiteDataTypeEnum.BLOB, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("OriginatingSystem", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("AuthoringToolId", SQLiteDataTypeEnum.TEXT, false, false);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Viewpoints_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Viewpoints", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion
               
                #region Lines
                tableName = TableNames.Lines.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("StartPoint", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Point", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("EndPoint", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Point", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Viewpoints_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Viewpoints", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region OrthogonalCamera
                tableName = TableNames.OrthogonalCamera.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CameraViewPoint", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Point", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CameraDirection", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Direction", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CameraUpVector", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Direction", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("ViewToWorldScale", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Viewpoints_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Viewpoints", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                #region PerspectiveCamera
                tableName = TableNames.PerspectiveCamera.ToString();
                tp = new TableProperties(tableName, false);

                fp = new FieldProperties("Guid", SQLiteDataTypeEnum.TEXT, true, true); //custom field
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CameraViewPoint", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Point", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CameraDirection", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Direction", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("CameraUpVector", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Direction", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("FieldOfView", SQLiteDataTypeEnum.REAL, false, true);
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                fp = new FieldProperties("Viewpoints_Guid", SQLiteDataTypeEnum.TEXT, false, false);
                fp.ForeignKey = new ForeignKeyInfo("Viewpoints", "Guid");
                if (!tp.Fields.ContainsKey(fp.FieldName)) { tp.Fields.Add(fp.FieldName, fp); }

                if (!tablesInfo.ContainsKey(tp.TableName))
                {
                    tablesInfo.Add(tp.TableName, tp);
                }
                #endregion

                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }
    }

    public class TableProperties
    {
        private string tableName = "";
        private bool isCustomTable = false;
        private Dictionary<string /*fieldName*/, FieldProperties> fields = new Dictionary<string, FieldProperties>();

        public string TableName { get { return tableName; } set { tableName = value; } }
        public bool IsCustomTable { get { return isCustomTable; } set { isCustomTable = value; } }
        public Dictionary<string, FieldProperties> Fields { get { return fields; } set { fields = value; } }

        public TableProperties()
        {
        }

        public TableProperties(string name, bool isCustom)
        {
            tableName = name;
            isCustomTable = isCustom;
        }
    }

    public class FieldProperties
    {
        private string fieldName = "";
        private SQLiteDataTypeEnum dataType = SQLiteDataTypeEnum.NONE;
        private bool isPrimaryKey = false;
        private bool isRequired = false;
        private ForeignKeyInfo foreignKey = null;

        public string FieldName { get { return fieldName; } set { fieldName = value; } }
        public SQLiteDataTypeEnum DataType { get { return dataType; } set { dataType = value; } }
        public bool IsPrimaryKey { get { return isPrimaryKey; } set { isPrimaryKey = value; } }
        public bool IsRequired { get { return isRequired; } set { isRequired = value; } }
        public ForeignKeyInfo ForeignKey { get { return foreignKey; } set { foreignKey = value; } }

        public FieldProperties()
        {
        }

        public FieldProperties(string name, SQLiteDataTypeEnum dataTypeEnum, bool primary, bool required)
        {
            fieldName = name;
            dataType = dataTypeEnum;
            isPrimaryKey = primary;
            isRequired = required;
        }
    }

    public class ForeignKeyInfo
    {
        private string tableName = "";
        private string keyName = "";

        public string ReferenceTableName { get { return tableName; } set { tableName = value; } }
        public string ReferenceKeyName { get { return keyName; } set { keyName = value; } }

        public ForeignKeyInfo()
        {
        }

        public ForeignKeyInfo(string table, string key)
        {
            tableName = table;
            keyName = key;
        }
    }
}
