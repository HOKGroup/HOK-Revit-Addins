using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.Utilities.ViewDepth
{
    public static class ViewDepthDataStorageUtil
    {
        public static Guid schemaId = new Guid("E96F577F-62ED-4B61-9A15-94425330D3A6");
        public static Schema m_schema = Schema.Lookup(schemaId);

        private static string s_ViewId = "ViewId";
        private static string s_ViewOverriden = "ViewOverriden";

        public static Dictionary<ElementId, bool> GetOverridenViews(Document doc)
        {
            Dictionary<ElementId, bool> overridenViews = new Dictionary<ElementId, bool>();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }
                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetViewsStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(m_schema);
                            ElementId viewId = entity.Get<ElementId>(m_schema.GetField(s_ViewId));
                            bool viewOverriden = entity.Get<bool>(m_schema.GetField(s_ViewOverriden));
                            if (!overridenViews.ContainsKey(viewId))
                            {
                                overridenViews.Add(viewId, viewOverriden);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get information about overriden views.\n"+ex.Message, "Get Overriden Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return overridenViews;
        }

        public static bool GetOverridenViews(Document doc, ElementId viewId)
        {
            bool viewOverriden = false;
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }
                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetViewsStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(m_schema);
                            ElementId vId = entity.Get<ElementId>(m_schema.GetField(s_ViewId));
                            if (vId.IntegerValue == viewId.IntegerValue)
                            {
                                viewOverriden = entity.Get<bool>(m_schema.GetField(s_ViewOverriden));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get information about overriden views.\n" + ex.Message, "Get Overriden Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewOverriden;
        }

        public static bool UpdateDataStorage(Document doc, ElementId viewId, bool viewOverriden)
        {
            bool result = false;
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetViewsStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Update values in the data storage.");
                            try
                            {
                                bool storageFound = false;
                                for (int i = 0; i < savedStorage.Count; i++)
                                {
                                    DataStorage storage = savedStorage[i];
                                    Entity entity = storage.GetEntity(m_schema);
                                    ElementId vId = entity.Get<ElementId>(m_schema.GetField(s_ViewId));
                                    if (vId.IntegerValue == viewId.IntegerValue)
                                    {
                                        entity.Set<bool>(s_ViewOverriden, viewOverriden);
                                        storage.SetEntity(entity);
                                        storageFound = true;
                                        break;
                                    }
                                }
                                if (!storageFound)
                                {
                                    DataStorage storage = DataStorage.Create(doc);

                                    Entity entity = new Entity(schemaId);
                                    entity.Set<ElementId>(s_ViewId, viewId);
                                    entity.Set<bool>(s_ViewOverriden, viewOverriden);
                                    storage.SetEntity(entity);
                                }

                                trans.Commit();
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set values in the data storage.\n" + ex.Message, "Update Data Stroage - View Depth", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                    else
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Update values in the data storage.");
                            try
                            {
                                DataStorage storage = DataStorage.Create(doc);

                                Entity entity = new Entity(schemaId);
                                entity.Set<ElementId>(s_ViewId, viewId);
                                entity.Set<bool>(s_ViewOverriden, viewOverriden);
                                storage.SetEntity(entity);

                                trans.Commit();
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set values in the data storage.\n" + ex.Message, "Update Data Stroage - View Depth", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update data storage.\n"+ex.Message, "Update Data Storage - View Depth", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("ViewDepth");
                schemaBuilder.AddSimpleField(s_ViewId, typeof(ElementId));
                schemaBuilder.AddSimpleField(s_ViewOverriden, typeof(bool));
                schema = schemaBuilder.Finish();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetViewsStorage(Document document, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }
}
