using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Access.Dao;

namespace RevitDBManager.Classes
{
    public class SqlHelper
    {
        public SqlHelper()
        {

        }

        public static string InsertIntoTable(string tableName, Dictionary<string, string> valuePair)
        {
            int i = 0;
            string queryString = "";
            StringBuilder strFields = new StringBuilder();
            StringBuilder strValues = new StringBuilder();

            foreach (string fieldKey in valuePair.Keys)
            {
                if (i == valuePair.Count - 1)
                {
                    strFields.Append("[" + fieldKey + "]");
                    string value = checkString(valuePair[fieldKey]);
                    strValues.Append("'" + value + "'");
                }
                else
                {
                    strFields.Append("[" + fieldKey + "],");
                    string value = checkString(valuePair[fieldKey]);
                    strValues.Append("'" + value + "',");
                }
                i++;
            }

            queryString = "INSERT INTO  [" + tableName + "] (" + strFields.ToString() + ") VALUES ( " + strValues.ToString() + ")";
            return queryString;
        }

        public static string DeleteTable(string tableName)
        {
            string query = "DELETE FROM [" + tableName+"]";
            return query;
        }

        //obsolete
        public string UpdateValue(string tableName, Dictionary<string, string> valuePair, string keyField, string keyValue)
        {
            string queryString = "";
            int i = 0;
            StringBuilder strSets = new StringBuilder();
            
            foreach (string fieldKey in valuePair.Keys)
            {
                if (i == valuePair.Count - 1)
                {
                    string value=checkString(valuePair[fieldKey]);
                    strSets.Append("["+fieldKey + "] = '" + value+"'");
                }
                else
                {
                    string value = checkString(valuePair[fieldKey]);
                    strSets.Append( "["+fieldKey + "] = '" + value + "', ");
                }
                i++;
            }
            keyValue = checkString(keyValue);
            queryString = "UPDATE [" + tableName + "] SET "+strSets.ToString() + " WHERE [" + keyField + "] = '" + keyValue + "'";
            return queryString;
        }

        private static string checkString(string input)
        {
            string result = input;
            if ((input != null) && (input.Contains("'")))
            {
                result = String.Format("{0}", input.Replace("'", "''"));
            }
            if (input == null || input == string.Empty)
            {
                result = "NULL";
            }
            return result;
        }


    }
}
