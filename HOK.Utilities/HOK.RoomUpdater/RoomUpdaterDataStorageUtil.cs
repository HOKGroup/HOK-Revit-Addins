using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.RoomUpdater
{
    public static class RoomUpdaterDataStorageUtil
    {
        public static Guid schemaId = new Guid("2898B467-0F45-4AA3-ADAD-1CA1173801AA");
        public static Schema m_schema = Schema.Lookup(schemaId);

        private static string s_SpatialCategory = "SpatialCategory";
        private static string s_SpatialParameter = "SpatialParameter";
        private static string s_RevitCategory = "RevitCategory";
        private static string s_RevitParameter = "RevitParameter";

        public static List<ParameterMapProperties> GetParameterMaps(Document doc)
        {
            List<ParameterMapProperties> parameterMaps = new List<ParameterMapProperties>();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }
                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetParameterMapStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(m_schema);
                            ParameterMapProperties pmp = GetParameterMapProperties(doc, entity);
                            if (null != pmp)
                            {
                                parameterMaps.Add(pmp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter maps from data storage.\n"+ex.Message, "Get Parameter Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return parameterMaps;
        }

        private static ParameterMapProperties GetParameterMapProperties(Document doc, Entity entity)
        {
            ParameterMapProperties pmp = null;
            try
            {
                string spatialCategoryName = entity.Get<string>(m_schema.GetField(s_SpatialCategory));
                string spatialParameterName = entity.Get<string>(m_schema.GetField(s_SpatialParameter));
                string revitCategoryName = entity.Get<string>(m_schema.GetField(s_RevitCategory));
                string revitParameterName = entity.Get<string>(m_schema.GetField(s_RevitParameter));

                Category spatialCategory = doc.Settings.Categories.get_Item(spatialCategoryName);
                Category revitCategory = doc.Settings.Categories.get_Item(revitCategoryName);
                if (null != spatialCategory && null != revitCategory)
                {
                    CategoryProperties scp = new CategoryProperties(spatialCategory);
                    CategoryProperties rcp = new CategoryProperties(revitCategory);

                    ParameterProperties spp = new ParameterProperties(spatialParameterName);
                    ParameterProperties rpp = new ParameterProperties(revitParameterName);

                    pmp = new ParameterMapProperties(spp, rpp, scp, rcp);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter map properties.\n"+ex.Message, "Get Parameter Map Properties", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return pmp;
        }

        public static bool StoreParameterMaps(Document doc, List<ParameterMapProperties> pmpList)
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
                    IList<DataStorage> savedStorage = GetParameterMapStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Delete Data Storage");
                            try
                            {
                                foreach (DataStorage ds in savedStorage)
                                {
                                    doc.Delete(ds.Id);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to delete data storage.\n" + ex.Message, "Update Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Add New Storage");
                        try
                        {
                            foreach (ParameterMapProperties pmp in pmpList)
                            {
                                DataStorage storage = DataStorage.Create(doc);
                                Entity entity = new Entity(schemaId);
                                entity.Set<string>(s_SpatialCategory, pmp.SpatialCatName);
                                entity.Set<string>(s_RevitCategory, pmp.RevitCatName);
                                entity.Set<string>(s_SpatialParameter, pmp.SpatialParamName);
                                entity.Set<string>(s_RevitParameter, pmp.RevitParamName);
                                
                                storage.SetEntity(entity);
                            }
                            trans.Commit();
                            saved = true;
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to add data storage.\n" + ex.Message, "Update Data Stroage", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save parameter maps in data storage.\n"+ex.Message, "Store Parameter Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("RoomUpdater");
                schemaBuilder.AddSimpleField(s_SpatialCategory, typeof(string));
                schemaBuilder.AddSimpleField(s_SpatialParameter, typeof(string));
                schemaBuilder.AddSimpleField(s_RevitCategory, typeof(string));
                schemaBuilder.AddSimpleField(s_RevitParameter, typeof(string));
                schema = schemaBuilder.Finish();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n"+ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetParameterMapStorage(Document document, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }
}
