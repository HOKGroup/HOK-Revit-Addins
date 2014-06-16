using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB;
using System.Windows;

namespace HOK.ColorSchemeEditor.BCFUtils
{
    public static class DataStorageUtil
    {
        public static Guid schemaId = new Guid("D31E3E96-F0FF-4527-90CE-5FB64D642C2F");
        public static Guid subSchemaId = new Guid("1B0C2456-D4EE-4FA5-927C-130F963A54D4");
        public static Schema m_schema = Schema.Lookup(schemaId);

        //field Name
        private static string s_BCFPath = "BCFPath";
        private static string s_ElementIds = "ElementIds";
        private static string s_ColoredElementIds = "ColoredElementIds";

        public static ColorEditorSettings ReadDataStorage(Document doc)
        {
            ColorEditorSettings settings = new ColorEditorSettings();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetSettingStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        Entity savedSetting = savedStorage.First().GetEntity(m_schema);
                        settings.BCFPath = savedSetting.Get<string>(m_schema.GetField(s_BCFPath));
                        var mapField = savedSetting.Get<IDictionary<string, Entity>>(m_schema.GetField(s_ColoredElementIds));
                        if (null != mapField)
                        {
                            IDictionary<string, IList<ElementId>> dictionary = new Dictionary<string, IList<ElementId>>();
                            foreach (string schemeId in mapField.Keys)
                            {
                                Entity entity = mapField[schemeId];
                                IList<ElementId> elementIds = entity.Get<IList<ElementId>>(s_ElementIds);
                                if (null!=elementIds && !dictionary.ContainsKey(schemeId))
                                {
                                    dictionary.Add(schemeId, elementIds);
                                }
                            }
                            settings.ColoredElements = dictionary;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read data storage for the Color Editor.\n"+ex.Message, "Read Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return settings;
        }

        public static Schema CreateSchema()
        {
            Schema schema = null;
            Schema subSchema = null;
            try
            {
                SchemaBuilder subSchemaBuilder = new SchemaBuilder(subSchemaId);
                subSchemaBuilder.SetSchemaName("ElementIds");
                subSchemaBuilder.AddArrayField(s_ElementIds, typeof(ElementId));
                subSchema = subSchemaBuilder.Finish();

                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("ColorEditorSetting");
                schemaBuilder.AddSimpleField(s_BCFPath, typeof(string));
                var mapField = schemaBuilder.AddMapField(s_ColoredElementIds, typeof(string), typeof(Entity));
                mapField.SetSubSchemaGUID(subSchemaId);
                schema = schemaBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema for Color Scheme Editor.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        public static void UpdateDataStorage(Document doc, ColorEditorSettings settings)
        {
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetSettingStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Update values in the data storage.");
                            try
                            {
                                DataStorage storage = savedStorage.First();
                                Entity entity = storage.GetEntity(m_schema);
                                entity.Set<string>(s_BCFPath, settings.BCFPath);
                                IDictionary<string, Entity> mapOfEntities = new Dictionary<string, Entity>();
                                foreach (string schemeId in settings.ColoredElements.Keys)
                                {
                                    IList<ElementId> elementIds = settings.ColoredElements[schemeId];
                                    Entity subEntity = new Entity(subSchemaId);
                                    subEntity.Set<IList<ElementId>>(s_ElementIds, elementIds);
                                    mapOfEntities.Add(schemeId, subEntity);
                                }
                                entity.Set<IDictionary<string, Entity>>(s_ColoredElementIds, mapOfEntities);
                                storage.SetEntity(entity);

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set values in the data storage.\n" + ex.Message, "Update Data Stroage", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                    else //create a new data storage
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Update values in the data storage.");
                            try
                            {
                                DataStorage storage = DataStorage.Create(doc);

                                Entity entity = new Entity(schemaId);
                                entity.Set<string>(s_BCFPath, settings.BCFPath);
                                IDictionary<string, Entity> mapOfEntities = new Dictionary<string, Entity>();
                                foreach (string schemeId in settings.ColoredElements.Keys)
                                {
                                    IList<ElementId> elementIds = settings.ColoredElements[schemeId];
                                    Entity subEntity = new Entity(subSchemaId);
                                    subEntity.Set<IList<ElementId>>(s_ElementIds, elementIds);
                                    mapOfEntities.Add(schemeId, subEntity);
                                }
                                entity.Set<IDictionary<string, Entity>>(s_ColoredElementIds, mapOfEntities);
                                storage.SetEntity(entity);

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set values in the data storage.\n" + ex.Message, "Update Data Stroage", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the extensible storage.\n" + ex.Message, "Update Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void UpdatePath(Document doc, ColorEditorSettings settings)
        {
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetSettingStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Update BCF Path in the data storage.");
                            try
                            {
                                DataStorage storage = savedStorage.First();
                                Entity entity = storage.GetEntity(m_schema);
                                if (!string.IsNullOrEmpty(settings.BCFPath))
                                {
                                    entity.Set<string>(s_BCFPath, settings.BCFPath);
                                    storage.DeleteEntity(m_schema);
                                    storage.SetEntity(entity);
                                }

                                trans.Commit();
                            }
                            catch(Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set BCF path in the data storage.\n"+ex.Message, "Update Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the bcf path in the extensible storage.\n"+ex.Message, "Update BCF Path", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void UpdateElementIds(Document doc, ColorEditorSettings settings)
        {
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetSettingStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Update ElementIds in the data storage.");
                            try
                            {
                                DataStorage storage = savedStorage.First();
                                Entity entity = storage.GetEntity(m_schema);
                                if (settings.ColoredElements.Count>0)
                                {
                                    IDictionary<string, Entity> mapOfEntities = new Dictionary<string, Entity>();
                                    foreach (string schemeId in settings.ColoredElements.Keys)
                                    {
                                        IList<ElementId> elementIds = settings.ColoredElements[schemeId];
                                        Entity subEntity = new Entity(subSchemaId);
                                        subEntity.Set<IList<ElementId>>(s_ElementIds, elementIds);
                                        mapOfEntities.Add(schemeId, subEntity);
                                    }
                                    entity.Set<IDictionary<string, Entity>>(s_ColoredElementIds, mapOfEntities);
                                    storage.SetEntity(entity);
                                }
                                
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set element Ids in the data storage.\n" + ex.Message, "Update Element Ids", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update element Ids in the extensible storage.\n" + ex.Message, "Update Element Ids", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static IList<DataStorage> GetSettingStorage(Document document, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }

    public class ColorEditorSettings
    {
        private string bcfPath = "";
        private IDictionary<string/*schemeId*/, IList<ElementId>> coloredElements = new Dictionary<string/*schemeId*/, IList<ElementId>>();

        public string BCFPath { get { return bcfPath; } set { bcfPath = value; } } //storing the opened bcfzip
        public IDictionary<string/*schemeId*/, IList<ElementId>> ColoredElements { get { return coloredElements; } set { coloredElements = value; } }

        public ColorEditorSettings(string path, IDictionary<string/*schemeId*/, IList<ElementId>> ids)
        {
            bcfPath = path;
            coloredElements = ids;
        }

        public ColorEditorSettings()
        {
        }
    }

}
