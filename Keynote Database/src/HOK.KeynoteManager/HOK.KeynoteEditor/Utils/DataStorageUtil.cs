using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.KeynoteEditor.Utils
{
    public static class DataStorageUtil
    {
        private static Guid configSchemaId = new Guid("699B0BBA-B1BF-444B-BB7D-54E285B8C5B4");
        private static Schema configSchema = null;

        //config fields
        private static string s_ProjectId = "ProjectId";
        private static string s_KeynoteSetId = "KeynoteSetId";

        public static KeynoteConfiguration GetConfiguration(Document doc)
        {
            KeynoteConfiguration config = new KeynoteConfiguration();
            try
            {
                if (null == configSchema)
                {
                    configSchema = CreateConfigSchema();
                }

                if (null != configSchema)
                {
                    IList<DataStorage> savedStorage = GetStorage(doc, configSchema);
                    if (savedStorage.Count > 0)
                    {
                        DataStorage storage = savedStorage.First();
                        Entity entity = storage.GetEntity(configSchema);
                        config.ProjectId = entity.Get<string>(configSchema.GetField(s_ProjectId));
                        config.KeynoteSetId = entity.Get<string>(configSchema.GetField(s_KeynoteSetId));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get configuration of Keynote Editor.\n" + ex.Message, "Get Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return config;
        }

        public static bool StoreConfiguration(Document doc, KeynoteConfiguration config)
        {
            bool stored = false;
            try
            {
                if (null == configSchema)
                {
                    configSchema = CreateConfigSchema();
                }

                if (null != configSchema)
                {
                    IList<DataStorage> savedStorage = GetStorage(doc, configSchema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Delete Storage");
                            try
                            {
                                var storageIds = from storage in savedStorage select storage.Id;
                                if (storageIds.Count() > 0)
                                {
                                    foreach (ElementId storageId in storageIds)
                                    {
                                        doc.Delete(storageId);
                                    }
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Store Storage");
                        try
                        {
                            DataStorage dStorage = DataStorage.Create(doc);
                            Entity entity = new Entity(configSchemaId);
                            entity.Set<string>(s_ProjectId, config.ProjectId);
                            entity.Set<string>(s_KeynoteSetId, config.KeynoteSetId);
                            dStorage.SetEntity(entity);
                           
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            string message = ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to store configuration.\n" + ex.Message, "Store Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return stored;
        }

        private static Schema CreateConfigSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder sBuilder = new SchemaBuilder(configSchemaId);
                sBuilder.SetSchemaName("KeynoteEditorConfiguration");
                sBuilder.AddSimpleField(s_ProjectId, typeof(string));
                sBuilder.AddSimpleField(s_KeynoteSetId, typeof(string));

                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a schema for the configuration./n" + ex.Message, "Create Configuration Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetStorage(Document doc, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }
}
