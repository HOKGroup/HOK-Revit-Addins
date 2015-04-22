using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.ModelManager.GoogleDocs
{
    public static class DataStorageUtil
    {
        public static Guid schemaId = new Guid("7B25AEC3-12AF-48A7-9D78-2E3FBD1321AC");
        public static Schema m_schema = Schema.Lookup(schemaId);

        private static string s_RevitDocId = "RevitDocumentId"; //Document Id to identify 
        private static string s_GoogleSheetId = "GoogleSheetId";

        public static ProjectReplicatorSettings ReadSettings(Document doc)
        {
            ProjectReplicatorSettings settings = new ProjectReplicatorSettings();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorages = GetDataStorage(doc, m_schema);
                   
                    if (savedStorages.Count > 0)
                    {
                        DataStorage savedStorage = savedStorages.First();
                        Entity entity = savedStorage.GetEntity(m_schema);
                        settings.RevitDocumentId = entity.Get<string>(m_schema.GetField(s_RevitDocId));
                        settings.GoogleSheetId = entity.Get<string>(m_schema.GetField(s_GoogleSheetId));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read google sheet Id.\n"+ex.Message, "Read Google Sheet Id", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return settings;
        }

        public static bool UpdateSettings(Document doc, ProjectReplicatorSettings settings)
        {
            bool updated = false;
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> storages = GetDataStorage(doc, m_schema);
                    if (storages.Count>0)
                    {
                        DataStorage savedStorage = storages.First();
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Delete DataStorage");
                            try
                            {
                                doc.Delete(savedStorage.Id);
                                trans.Commit();
                            }
                            catch
                            {
                                trans.RollBack();
                            }
                        }
                    }

                    //create new data storage
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Create Data Storage");
                        try
                        {
                            DataStorage storage = DataStorage.Create(doc);

                            Entity entity = new Entity(schemaId);
                            entity.Set<string>(s_RevitDocId, settings.RevitDocumentId);
                            entity.Set<string>(s_GoogleSheetId, settings.GoogleSheetId);
                            storage.SetEntity(entity);

                            trans.Commit();
                            updated = true;
                        }
                        catch(Exception ex)
                        {
                            string message = ex.Message;
                            trans.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update File Id.\n"+ex.Message, "Update Google Sheet Id", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        public static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("ProjectReplicator");
                schemaBuilder.AddSimpleField(s_GoogleSheetId, typeof(string));
                schemaBuilder.AddSimpleField(s_RevitDocId, typeof(string));
                schema = schemaBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n"+ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetDataStorage(Document document, Schema schema)
        {
            
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }

    public class ProjectReplicatorSettings
    {
        private string googleSheetId = "";
        private string revitDocumentId = "";

        public string GoogleSheetId { get { return googleSheetId; } set { googleSheetId = value; } }
        public string RevitDocumentId { get { return revitDocumentId; } set { revitDocumentId = value; } }

        public ProjectReplicatorSettings()
        {
        }
    }
}
