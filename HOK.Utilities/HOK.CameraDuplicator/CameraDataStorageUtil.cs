using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using HOK.Core.Utilities;

namespace HOK.CameraDuplicator
{
    public static class ViewConfigDataStorageUtil
    {
        private static Guid schemaId = new Guid("0EC8C23E-A71F-4DD8-9834-FB284CB4D969");
        private static Guid subSchemaId = new Guid("15BCCA65-4C98-4E12-9408-395AF0D8A91E");
        private static Schema m_schema = null;
        private static Schema subSchema = null;

        private static string s_WorksetVisibility = "WorksetVisibility"; //apply workset visibility
        private static string s_MapItems = "MapItems"; //sub entity name
        private static string s_SourceModelId = "SourceModelId";
        private static string s_RecipientModelId = "RecipientModelId";
        private static string s_MapItemType = "MapItemType";
        private static string s_SourceItemId = "SourceItemId";
        private static string s_RecipientItemId = "RecipientItemId";
        private static string s_SourceItemName = "SourceItemName";
        private static string s_RecipientItemName = "RecipientItemName";

        public static ViewConfiguration GetViewConfiguration(Document doc)
        {
            var viewConfig = new ViewConfiguration();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    var savedStorage = GetViewConfigurationStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        var storage = savedStorage.First();
                        var entity = storage.GetEntity(m_schema);
                        viewConfig.ApplyWorksetVisibility = entity.Get<bool>(m_schema.GetField(s_WorksetVisibility));

                        var mapItems = new List<MapItemInfo>();
                        var subEntities = entity.Get<IList<Entity>>(m_schema.GetField(s_MapItems));
                        foreach (var subE in subEntities)
                        {
                            var mapItem = new MapItemInfo();
                            mapItem.SourceModelId = subE.Get<string>(subSchema.GetField(s_SourceModelId));
                            mapItem.RecipientModelId = subE.Get<string>(subSchema.GetField(s_RecipientModelId));
                            mapItem.MapItemType = (MapType)Enum.Parse(typeof(MapType), subE.Get<string>(subSchema.GetField(s_MapItemType)));
                            mapItem.SourceItemId = subE.Get<int>(subSchema.GetField(s_SourceItemId));
                            mapItem.RecipientItemId = subE.Get<int>(subSchema.GetField(s_RecipientItemId));
                            mapItem.SourceItemName = subE.Get<string>(subSchema.GetField(s_SourceItemName));
                            mapItem.RecipientItemName = subE.Get<string>(subSchema.GetField(s_RecipientItemName));
                            
                            mapItems.Add(mapItem);
                        }
                        viewConfig.MapItems = mapItems;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get view configuration.\n" + ex.Message, "Get View Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewConfig;
        }

        public static bool StoreViewConfiguration(Document doc, ViewConfiguration vc)
        {
            var stored = false;
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    var savedStorage = GetViewConfigurationStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (var trans = new Transaction(doc))
                        {
                            trans.Start("Delete Storage");
                            try
                            {
                                foreach (var storage in savedStorage)
                                {
                                    doc.Delete(storage.Id);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to delete data storage for mapping items.\n" + ex.Message, "Delete Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }

                    using (var trans = new Transaction(doc))
                    {
                        trans.Start("Save Storage");
                        try
                        {
                            var storage = DataStorage.Create(doc);
                            var entity = new Entity(schemaId);
                            entity.Set<bool>(s_WorksetVisibility, vc.ApplyWorksetVisibility);

                            var subEntities = new List<Entity>();
                            foreach (var mapItemInfo in vc.MapItems)
                            {
                                var subEntity = new Entity(subSchemaId);
                                subEntity.Set<string>(s_SourceModelId, mapItemInfo.SourceModelId);
                                subEntity.Set<string>(s_RecipientModelId, mapItemInfo.RecipientModelId);
                                subEntity.Set<string>(s_MapItemType, mapItemInfo.MapItemType.ToString());
                                subEntity.Set<long>(s_SourceItemId, mapItemInfo.SourceItemId);
                                subEntity.Set<long>(s_RecipientItemId, mapItemInfo.RecipientItemId);
                                subEntity.Set<string>(s_SourceItemName, mapItemInfo.SourceItemName);
                                subEntity.Set<string>(s_RecipientItemName, mapItemInfo.RecipientItemName);
                                subEntities.Add(subEntity);
                            }

                            entity.Set<IList<Entity>>(s_MapItems, subEntities);
                            storage.SetEntity(entity);
                            trans.Commit();
                            stored = true;
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to save mapping items.\n" + ex.Message, "Store Mapping Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to store model Id.\n" + ex.Message, "Store Model Id", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return stored;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                var subBuilder = new SchemaBuilder(subSchemaId);
                subBuilder.SetSchemaName("MapItemInfoList");
                subBuilder.AddSimpleField(s_SourceModelId, typeof(string));
                subBuilder.AddSimpleField(s_RecipientModelId, typeof(string));
                subBuilder.AddSimpleField(s_MapItemType, typeof(string));
                subBuilder.AddSimpleField(s_SourceItemId, typeof(int));
                subBuilder.AddSimpleField(s_RecipientItemId, typeof(int));
                subBuilder.AddSimpleField(s_SourceItemName, typeof(string));
                subBuilder.AddSimpleField(s_RecipientItemName, typeof(string));
                subSchema = subBuilder.Finish();

                var sBuilder = new SchemaBuilder(schemaId);
                sBuilder.SetSchemaName("ViewConfiguration");
                sBuilder.AddSimpleField(s_WorksetVisibility, typeof(bool));
                var fBuilder = sBuilder.AddArrayField(s_MapItems, typeof(Entity));
                fBuilder.SetSubSchemaGUID(subSchemaId);

                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetViewConfigurationStorage(Document doc, Schema schema)
        {
            var collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }

    public static class ModelDataStorageUtil
    {
        private static Guid schemaId = new Guid("1D58D1AD-8730-4205-95F0-39FF4BC38E97");
        private static Schema Schema = Schema.Lookup(schemaId);

        private static string s_ModelId = "ModelId";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string GetModelId(Document doc)
        {
            var modelId = "";
            try
            {
                if (Schema == null)
                {
                    Schema = CreateSchema();
                }
                else
                {
                    var savedStorage = new FilteredElementCollector(doc)
                        .OfClass(typeof(DataStorage))
                        .Cast<DataStorage>()
                        .FirstOrDefault(x => x.GetEntity(Schema) != null && x.GetEntity(Schema).IsValid());
                    if (savedStorage != null)
                    {
                        var entity = savedStorage.GetEntity(Schema);
                        modelId = entity.Get<string>(Schema.GetField(s_ModelId));
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);

                modelId = Guid.NewGuid().ToString();
                StoreModelId(doc, modelId);
            }

            return modelId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        public static bool StoreModelId(Document doc, string modelId)
        {
            var stored = false;
            try
            {
                if (Schema == null)
                {
                    Schema = CreateSchema();
                }
                else
                {
                    var savedStorage = new FilteredElementCollector(doc)
                        .OfClass(typeof(DataStorage))
                        .Cast<DataStorage>()
                        .Where(x => x.GetEntity(Schema) != null && x.GetEntity(Schema).IsValid())
                        .ToList();
                    if (savedStorage.Any())
                    {
                        using (var trans = new Transaction(doc))
                        {
                            trans.Start("Delete Storage");
                            try
                            {
                                foreach (var storage in savedStorage)
                                {
                                    doc.Delete(storage.Id);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                                trans.RollBack();
                            }
                        }
                    }

                    using (var trans = new Transaction(doc))
                    {
                        trans.Start("Save Storage");
                        try
                        {
                            var storage = DataStorage.Create(doc);
                            var entity = new Entity(schemaId);
                            entity.Set(s_ModelId, modelId);
                            storage.SetEntity(entity);

                            trans.Commit();
                            stored = true;
                        }
                        catch (Exception ex)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return stored;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                var sBuilder = new SchemaBuilder(schemaId);
                sBuilder.SetSchemaName("ModelUniqueId");
                sBuilder.AddSimpleField(s_ModelId, typeof(string));
                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            return schema;
        }
    }
}
