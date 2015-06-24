using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.CameraDuplicator
{
    public static class CameraDataStorageUtil
    {
        private static Guid schemaId = new Guid("D39F4422-82B2-478A-A658-16C68B66C822");
        private static Schema m_Schema = Schema.Lookup(schemaId);

        private static string s_SourceModelId = "SourceModelId";
        private static string s_SourceViewId = "SourceViewId";
        private static string s_RecipientModelId = "RecipientModelId";
        private static string s_RecipientViewId = "RecipientViewId";

        public static List<CameraViewMap> GetCameraViewMap(Document doc)
        {
            List<CameraViewMap> viewMapList = new List<CameraViewMap>();
            try
            {
                if (null == m_Schema)
                {
                    m_Schema = CreateSchema();
                }

                if (null != m_Schema)
                {
                    IList<DataStorage> savedStorage = GetCameraViewMapStorage(doc, m_Schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage ds in savedStorage)
                        {
                            Entity entity = ds.GetEntity(m_Schema);
                            string sModelId = entity.Get<string>(m_Schema.GetField(s_SourceModelId));
                            int sViewId = entity.Get<int>(m_Schema.GetField(s_SourceViewId));
                            string rModelId = entity.Get<string>(m_Schema.GetField(s_RecipientModelId));
                            int rViewId = entity.Get<int>(m_Schema.GetField(s_RecipientViewId));

                            CameraViewMap map = new CameraViewMap();
                            map.SourceModelId = sModelId;
                            map.SourceViewId = sViewId;
                            map.RecipientModelId = rModelId;
                            map.RecipientViewId = rViewId;

                            viewMapList.Add(map);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get camera view map.\n" + ex.Message, "Get Camera View Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewMapList;
        }

        public static bool StoreCameraViewMap(Document doc, List<CameraViewMap> viewMapList)
        {
            bool stored = false;
            if (null == m_Schema)
            {
                m_Schema = CreateSchema();
            }

            if (null != m_Schema)
            {
                IList<DataStorage> savedStorage = GetCameraViewMapStorage(doc, m_Schema);
                if (savedStorage.Count > 0)
                {
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Delete Storage");
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
                            trans.RollBack();
                            MessageBox.Show("Failed to delete data storage for model Id.\n" + ex.Message, "Delete Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Save Storage");
                    try
                    {
                        foreach (CameraViewMap vMap in viewMapList)
                        {
                            DataStorage storage = DataStorage.Create(doc);
                            Entity entity = new Entity(schemaId);
                            entity.Set<string>(s_SourceModelId, vMap.SourceModelId);
                            entity.Set<int>(s_SourceViewId, vMap.SourceViewId);
                            entity.Set<string>(s_RecipientModelId, vMap.RecipientModelId);
                            entity.Set<int>(s_RecipientViewId, vMap.RecipientViewId);
                            storage.SetEntity(entity);
                        }
                        trans.Commit();
                        stored = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to save model Id.\n" + ex.Message, "Store Model Id", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            return stored;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder sBuilder = new SchemaBuilder(schemaId);
                sBuilder.SetSchemaName("CameraViewInfoMap");
                sBuilder.AddSimpleField(s_SourceModelId, typeof(string));
                sBuilder.AddSimpleField(s_SourceViewId, typeof(int));
                sBuilder.AddSimpleField(s_RecipientModelId, typeof(string));
                sBuilder.AddSimpleField(s_RecipientViewId, typeof(int));
                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetCameraViewMapStorage(Document doc, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }

    public static class ModelDataStorageUtil
    {
        private static Guid schemaId = new Guid("1D58D1AD-8730-4205-95F0-39FF4BC38E97");
        private static Schema m_schema = Schema.Lookup(schemaId);

        private static string s_ModelId = "ModelId";

        public static string GetModelId(Document doc)
        {
            string modelId = "";
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetModelIdStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        DataStorage storage = savedStorage.First();
                        Entity entity = storage.GetEntity(m_schema);
                        modelId = entity.Get<string>(m_schema.GetField(s_ModelId));
                    }
                    else
                    {
                        modelId = Guid.NewGuid().ToString();
                        StoreModelId(doc, modelId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get model Id.\n" + ex.Message, "Get Model Id", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return modelId;
        }

        public static bool StoreModelId(Document doc, string modelId)
        {
            bool stored = false;
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetModelIdStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Delete Storage");
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
                                trans.RollBack();
                                MessageBox.Show("Failed to delete data storage for model Id.\n" + ex.Message, "Delete Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Save Storage");
                        try
                        {
                            DataStorage storage = DataStorage.Create(doc);
                            Entity entity = new Entity(schemaId);
                            entity.Set<string>(s_ModelId, modelId);
                            storage.SetEntity(entity);
                            trans.Commit();
                            stored = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to save model Id.\n"+ex.Message, "Store Model Id", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to store model Id.\n"+ex.Message, "Store Model Id", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return stored;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder sBuilder = new SchemaBuilder(schemaId);
                sBuilder.SetSchemaName("ModelUniqueId");
                sBuilder.AddSimpleField(s_ModelId, typeof(string));
                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetModelIdStorage(Document doc, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }
}
