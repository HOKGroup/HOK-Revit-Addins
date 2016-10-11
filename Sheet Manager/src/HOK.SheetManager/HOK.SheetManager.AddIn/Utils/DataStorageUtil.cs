using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using HOK.SheetManager.AddIn.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.SheetManager.AddIn.Utils
{
    public static class DataStorageUtil
    {
        private static Guid configSchemaId = new Guid("89E0F4B4-E3C8-47AD-8174-00CB4E35FD1F");
        private static Schema configSchema = null;

        //config fields
        private static string s_ModelId = "ModelId";
        private static string s_CentralPath = "CentralPath";
        private static string s_TitleBlcokId = "TitleBlcokId";
        private static string s_IsPlaceHolder = "IsPlaceHolder";
        private static string s_DatabaseFile = "DatabaseFile";
        private static string s_AutoUpdate = "AutoUpdate";

        public static SheetManagerConfiguration GetConfiguration(Document doc)
        {
            SheetManagerConfiguration config = new SheetManagerConfiguration();
            try
            {
                if (null == configSchema)
                {
                    configSchema = CreateConfigSchema();
                }

                if (null != configSchema)
                {
                    DataStorage savedStorage = GetStorage(doc, configSchema);
                    if (null != savedStorage)
                    {
                        Entity entity = savedStorage.GetEntity(configSchema);
                        config.ModelId = entity.Get<Guid>(configSchema.GetField(s_ModelId));
                        config.CentralPath = entity.Get<string>(configSchema.GetField(s_CentralPath));
                        config.TitleblockId = entity.Get<ElementId>(configSchema.GetField(s_TitleBlcokId));
                        config.IsPlaceholder = entity.Get<bool>(configSchema.GetField(s_IsPlaceHolder));
                        config.DatabaseFile = entity.Get<string>(configSchema.GetField(s_DatabaseFile));
                        config.AutoUpdate = entity.Get<bool>(configSchema.GetField(s_AutoUpdate));
                    }
                    else
                    {
                        config.ModelId = Guid.NewGuid();
                        config.CentralPath = GetCentralFilePath(doc);

                        //default title block
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<ElementId> elementIds = collector.OfCategory(BuiltInCategory.OST_TitleBlocks).WhereElementIsElementType().ToElementIds().ToList();
                        if (elementIds.Count > 0)
                        {
                            config.TitleblockId = elementIds.First();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the information of sheet settings.\n" + ex.Message, "Get Sheet Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return config;
        }

        private static Schema CreateConfigSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder sBuilder = new SchemaBuilder(configSchemaId);
                sBuilder.SetSchemaName("SheetManagerConfiguration");
                sBuilder.AddSimpleField(s_ModelId, typeof(Guid));
                sBuilder.AddSimpleField(s_CentralPath, typeof(string));
                sBuilder.AddSimpleField(s_DatabaseFile, typeof(string));
                sBuilder.AddSimpleField(s_TitleBlcokId, typeof(ElementId));
                sBuilder.AddSimpleField(s_IsPlaceHolder, typeof(bool));
                sBuilder.AddSimpleField(s_AutoUpdate, typeof(bool));

                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a schema for the configuration.\n" + ex.Message, "Create Configuration Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static DataStorage GetStorage(Document doc, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var dataStorages = collector.OfClass(typeof(DataStorage));

            foreach (DataStorage storage in dataStorages)
            {
                Entity entity = storage.GetEntity(schema);
                if (entity.IsValid())
                {
                    return storage;
                }
            }
            return null;
        }

        public static string GetCentralFilePath(Document doc)
        {
            string centralPath = doc.PathName;
            try
            {
                if (doc.IsWorkshared)
                {
                    if (doc.IsDetached) { return centralPath; }
                    else
                    {
                        ModelPath centralModelPath = doc.GetWorksharingCentralModelPath();
                        if (null != centralModelPath)
                        {
                            string userVisibleCentralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(centralModelPath);
                            if (!string.IsNullOrEmpty(userVisibleCentralPath))
                            {
                                centralPath = userVisibleCentralPath;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return centralPath;
        }

        public static bool StoreConfiguration(Document doc, SheetManagerConfiguration config)
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
                    DataStorage savedStorage = GetStorage(doc, configSchema);
                    if (null == savedStorage)
                    {
                        savedStorage = DataStorage.Create(doc);
                    }
                    if (null!=savedStorage)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Store Storage");
                            try
                            {
                                Entity entity = new Entity(configSchemaId);
                                entity.Set<Guid>(s_ModelId, config.ModelId);
                                entity.Set<string>(s_CentralPath, config.CentralPath);
                                entity.Set<ElementId>(s_TitleBlcokId, config.TitleblockId);
                                entity.Set<bool>(s_IsPlaceHolder, config.IsPlaceholder);
                                entity.Set<string>(s_DatabaseFile, config.DatabaseFile);
                                entity.Set<bool>(s_AutoUpdate, config.AutoUpdate);
                                savedStorage.SetEntity(entity);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to sotre configuration.\n" + ex.Message, "Store Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return stored;
        }
    }
}
