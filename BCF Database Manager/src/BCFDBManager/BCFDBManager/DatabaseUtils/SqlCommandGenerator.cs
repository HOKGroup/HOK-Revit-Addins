using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BCFDBManager.DatabaseUtils
{
    public enum SQLiteDataTypeEnum
    {
        NONE = 0, INTEGER=1, TEXT=2, BLOB=3, REAL=4, NUMERIC=5, BOOLEAN = 6, DATETIME = 7
    }

    public static class SqlCommandGenerator
    {
        public static string CreateTableFromDataTable(DataTable dt)
        {
            string sqlStatement = "";
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.Append("CREATE TABLE IF NOT EXISTS " + dt.TableName + "(");
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    DataColumn dc = dt.Columns[i];
                    string colName = dc.ColumnName;
                    //for test
                    if (colName.Contains("Index")) { continue; }

                    SQLiteDataTypeEnum sqliteDataType = ConverToSQLiteDataType(dc.DataType.Name);
                    bool unique = dc.Unique;
                   
                    strBuilder.Append(colName + " " + sqliteDataType.ToString());
                    if (unique)
                    {
                        //strBuilder.Append(" PRIMARY KEY");
                    }

                    if (i != dt.Columns.Count - 1)
                    {
                        strBuilder.Append(", ");
                    }
                }
                strBuilder.Append(")");

                if (dt.Columns.Count > 0 && strBuilder.Length > 0)
                {
                    sqlStatement = strBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to wrtie a sql statement to create table from the data table.\n" + ex.Message, "Create Table from Data Table", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sqlStatement;
        }

        public static string CreateTableQuery(TableProperties tp)
        {
            string sql = "";
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.Append("CREATE TABLE IF NOT EXISTS " + tp.TableName + "(");

                foreach (string fieldName in tp.Fields.Keys)
                {
                    FieldProperties fp = tp.Fields[fieldName];
                    strBuilder.Append(fieldName + " " + fp.DataType.ToString());

                    if (fp.IsPrimaryKey)
                    {
                        strBuilder.Append(" PRIMARY KEY");
                    }

                    if (null != fp.ForeignKey)
                    {
                        strBuilder.Append(" REFERENCES " + fp.ForeignKey.ReferenceTableName + "(" + fp.ForeignKey.ReferenceKeyName + ") ON DELETE CASCADE");
                    }

                    strBuilder.Append(",");
                }

                strBuilder.Replace(',', ')', strBuilder.Length - 1, 1);
                sql = strBuilder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write a slq statement for creating tables.\n" + ex.Message, "Create Table Query", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sql;
        }

        private static SQLiteDataTypeEnum ConverToSQLiteDataType(string dataTypeName)
        {
            SQLiteDataTypeEnum sqliteType = SQLiteDataTypeEnum.TEXT;
            switch (dataTypeName)
            {
                case "Boolean":
                    sqliteType = SQLiteDataTypeEnum.BOOLEAN;
                    break;
                case "Byte":
                    sqliteType = SQLiteDataTypeEnum.INTEGER;
                    break;
                case "Char":
                    sqliteType = SQLiteDataTypeEnum.TEXT;
                    break;
                case "DateTime":
                    sqliteType = SQLiteDataTypeEnum.DATETIME;
                    break;
                case "Decimal":
                    sqliteType = SQLiteDataTypeEnum.NUMERIC;
                    break;
                case "Double":
                    sqliteType = SQLiteDataTypeEnum.REAL;
                    break;
                case "Guid":
                    sqliteType = SQLiteDataTypeEnum.TEXT;
                    break;
                case "Int16":
                    sqliteType = SQLiteDataTypeEnum.INTEGER;
                    break;
                case "Int32":
                    sqliteType = SQLiteDataTypeEnum.INTEGER;
                    break;
                case "Int64":
                    sqliteType = SQLiteDataTypeEnum.INTEGER;
                    break;
                case "SByte":
                    sqliteType = SQLiteDataTypeEnum.INTEGER;
                    break;
                case "Single":
                    sqliteType = SQLiteDataTypeEnum.REAL;
                    break;
                case "String":
                    sqliteType = SQLiteDataTypeEnum.TEXT;
                    break;
                case "TimeSpan":
                    sqliteType = SQLiteDataTypeEnum.NUMERIC;
                    break;
                case "UInt16":
                    sqliteType = SQLiteDataTypeEnum.INTEGER;
                    break;
                case "UInt32":
                    sqliteType = SQLiteDataTypeEnum.INTEGER;
                    break;
                case "UInt64":
                    sqliteType = SQLiteDataTypeEnum.INTEGER;
                    break;
                case "Byte[]":
                    sqliteType = SQLiteDataTypeEnum.BLOB;
                    break;
                case "Uri":
                    sqliteType = SQLiteDataTypeEnum.TEXT;
                    break;
                  
            }
            return sqliteType;
        }
    }

}
