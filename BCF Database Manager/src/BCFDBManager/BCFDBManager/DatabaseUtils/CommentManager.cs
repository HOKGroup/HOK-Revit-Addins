using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BCFDBManager.DatabaseUtils
{
    public static class CommentManager
    {
        public static bool AddComment(Markup markup, Comment commentToAdd, string dbFile)
        {
            bool added = false;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        string topicGuid = markup.Topic.Guid;

                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.Append("INSERT INTO Comment (Guid, VerbalStatus, Status, Date, Author, Comment, ModifiedDate, ModifiedAuthor, Topic_Guid) VALUES ");
                        strBuilder.Append("('" + commentToAdd.Guid + "', '" + commentToAdd.VerbalStatus + "', '" + commentToAdd.Status + "', @date, '" + commentToAdd.Author + "', '" + commentToAdd.Comment1 + "', @modifiedDate, '" + commentToAdd.ModifiedAuthor + "', '" + topicGuid + "')");
                        cmd.Parameters.Add("@date", DbType.DateTime);
                        cmd.Parameters["@date"].Value = commentToAdd.Date;
                        cmd.Parameters.Add("@modifiedDate", DbType.DateTime);
                        if (commentToAdd.ModifiedDateSpecified) { cmd.Parameters["@modifiedDate"].Value = commentToAdd.ModifiedDate; }

                        cmd.CommandText = strBuilder.ToString();
                        int insertedComment = cmd.ExecuteNonQuery();
                        
                        strBuilder = new StringBuilder();
                        strBuilder.Append("INSERT INTO Viewpoint (Guid, Comment_Guid) VALUES ");
                        strBuilder.Append("('" + commentToAdd.Viewpoint.Guid + "', '" + commentToAdd.Guid + "')");
                        cmd.CommandText = strBuilder.ToString();
                        int insertedViewPoint = cmd.ExecuteNonQuery();

                        if (insertedComment > 0 && insertedViewPoint > 0)
                        {
                            added = true;
                        }
                        
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add a comment item.\n"+ex.Message, "Add Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return added;
        }

        public static bool EditComment(Comment commentToEdit, string dbFile)
        {
            bool edited = false;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.Append("UPDATE Comment SET VerbalStatus = '" + commentToEdit.VerbalStatus + "', Status = '" + commentToEdit.Status + "', Date = @date, ");
                        strBuilder.Append("Author = '" + commentToEdit.Author + "', Comment = '" + commentToEdit.Comment1 + "', ModifiedDate = @modifiedDate, ModifiedAuthor = '"+commentToEdit.ModifiedAuthor+"' ");
                        strBuilder.Append("WHERE Guid = '" + commentToEdit.Guid + "'");
                        
                        cmd.Parameters.Add("@date", DbType.DateTime);
                        cmd.Parameters["@date"].Value = commentToEdit.Date;
                        cmd.Parameters.Add("@modifiedDate", DbType.DateTime);
                        if (commentToEdit.ModifiedDateSpecified) { cmd.Parameters["@modifiedDate"].Value = commentToEdit.ModifiedDate; }

                        cmd.CommandText = strBuilder.ToString();
                        int updatedComment = cmd.ExecuteNonQuery();
                        if (updatedComment > 0)
                        {
                            edited = true;
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to edit a comment item.\n" + ex.Message, "Edit Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return edited;
        }

        public static bool DeleteComment(Comment commentToDelete, string dbFile)
        {
            bool deleted = false;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(connection))
                    {
                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.Append("DELETE FROM Comment WHERE Guid = '"+commentToDelete.Guid+"'");

                        cmd.CommandText = strBuilder.ToString();
                        int deletedComment = cmd.ExecuteNonQuery();

                        strBuilder = new StringBuilder();
                        strBuilder.Append("DELETE FROM Viewpoint WHERE Comment_Guid = '" + commentToDelete.Guid + "'");
                        cmd.CommandText = strBuilder.ToString();
                        int deletedViewPoint = cmd.ExecuteNonQuery();

                        if (deletedComment > 0 && deletedViewPoint > 0)
                        {
                            deleted = true;
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete a comment item.\n"+ex.Message, "Delete Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deleted;
        }
    }
}
