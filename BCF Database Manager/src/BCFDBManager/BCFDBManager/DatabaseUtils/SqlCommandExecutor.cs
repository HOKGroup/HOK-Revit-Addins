using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BCFDBManager.DatabaseUtils
{
    public static class SqlCommandExecutor
    {
        public static bool CreateDatabase(string fileName, bool overwrite)
        {
            bool created = false;
            try
            {
                if (File.Exists(fileName) && overwrite)
                {
                    File.Delete(fileName);
                }

                SQLiteConnection.CreateFile(fileName);
                if (File.Exists(fileName))
                {
                    created = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a database file.\n"+ex.Message, "Create Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        public static bool CreateDatabaseTables(string[] schemaFiles)
        {
            bool created = false;
            try
            {
                foreach (string schemaFile in schemaFiles)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create database tables.\n" + ex.Message, "Create Database Tables", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }
    }
}
