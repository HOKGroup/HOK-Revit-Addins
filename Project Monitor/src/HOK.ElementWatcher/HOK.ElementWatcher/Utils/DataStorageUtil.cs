
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using HOK.ElementWatcher.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementWatcher.Utils
{
    public static class DataStorageUtil
    {
        public static Guid schemaId = new Guid("F3227CAF-DBD0-4C10-841E-9BB65D577B6F");
        public static Schema m_schema = Schema.Lookup(schemaId);

        private static string s_ProjectFileId = "ProjectFileId";

        public static Guid GetProjectFileId(Document doc)
        {
            Guid projectFileId = Guid.Empty;
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }
                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetDTMStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        DataStorage storage = savedStorage.First();
                        Entity entity = storage.GetEntity(m_schema);
                        projectFileId = entity.Get<Guid>(m_schema.GetField(s_ProjectFileId));
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return projectFileId;
        }

        public static bool StoreProjectFileId(Document doc, Guid projectFileId)
        {
            bool saved = false;
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }
                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetDTMStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Store Project Setup");
                            try
                            {
                                foreach (DataStorage storage in savedStorage)
                                {
                                    doc.Delete(storage.Id);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                                trans.RollBack();
                            }
                        }
                    }

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Store Project Setup");
                        try
                        {
                            DataStorage dataStorage = DataStorage.Create(doc);
                            Entity entity = new Entity(schemaId);
                            entity.Set<Guid>(s_ProjectFileId, projectFileId);
                            dataStorage.SetEntity(entity);

                            trans.Commit();
                            saved = true;
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                            trans.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return saved;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("DTMTool");
                schemaBuilder.AddSimpleField(s_ProjectFileId, typeof(Guid));
                schema = schemaBuilder.Finish();

            }
            catch (Exception ex)
            {
                string message = "Cannot Create Schema: " + ex.Message;
            }
            return schema;
        }

        private static IList<DataStorage> GetDTMStorage(Document document, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }
}
