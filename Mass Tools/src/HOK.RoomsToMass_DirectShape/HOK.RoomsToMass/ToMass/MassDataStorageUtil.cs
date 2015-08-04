using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.RoomsToMass.ToMass
{
    public static class MassDataStorageUtil
    {
        private static Guid schemaId = new Guid("42BFD1BA-CE29-4866-B111-14FA0EC89928");
        private static Schema m_schema = null;

        private static string s_SourceCategory = "SourceCategory";
        private static string s_LinkedSourceId = "LinkedSourceId"; //unique Id
        private static string s_SourceCentroid = "SourceCentroid";
        private static string s_MassHeight = "MassHeight";

        public static bool GetLinkedHostInfo(Element massElement, out string hostCategory, out string hostUniqueId, out XYZ centroid, out double userHeight)
        {
            bool found = false;
            hostCategory = "";
            hostUniqueId = "";
            centroid = null;
            userHeight = 0;
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    Entity entity = massElement.GetEntity(m_schema);
                    if (entity.IsValid())
                    {
                        hostCategory = entity.Get<string>(m_schema.GetField(s_SourceCategory));
                        hostUniqueId = entity.Get<string>(m_schema.GetField(s_LinkedSourceId));
                        centroid = entity.Get<XYZ>(m_schema.GetField(s_SourceCentroid), DisplayUnitType.DUT_DECIMAL_FEET);
                        userHeight = entity.Get<double>(m_schema.GetField(s_MassHeight), DisplayUnitType.DUT_DECIMAL_FEET);
                        if (!string.IsNullOrEmpty(hostCategory) && !string.IsNullOrEmpty(hostUniqueId) && null != centroid && userHeight > 0)
                        {
                            found = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get a linked mass information.\n" + ex.Message, "Get Linked Masses", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return found;
        }

        public static bool SetLinkedHostInfo(Element massElement, string hostCategory, string hostUniqueId, XYZ centroid, double userHeight)
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
                    Entity entity = massElement.GetEntity(m_schema);
                    if (entity.IsValid())
                    {
                        massElement.DeleteEntity(m_schema);
                    }
                    entity = new Entity(m_schema);
                    entity.Set<string>(m_schema.GetField(s_SourceCategory), hostCategory);
                    entity.Set<string>(m_schema.GetField(s_LinkedSourceId), hostUniqueId);
                    entity.Set<XYZ>(m_schema.GetField(s_SourceCentroid), centroid, DisplayUnitType.DUT_DECIMAL_FEET);
                    entity.Set<double>(m_schema.GetField(s_MassHeight), userHeight, DisplayUnitType.DUT_DECIMAL_FEET);
                    massElement.SetEntity(entity);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set linked mass info.\n"+ex.Message, "Set Linked Masses", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder sBuilder = new SchemaBuilder(schemaId);
                sBuilder.SetSchemaName("LinkedSourceInfo");
                sBuilder.AddSimpleField(s_SourceCategory, typeof(string));
                sBuilder.AddSimpleField(s_LinkedSourceId, typeof(string));
                FieldBuilder fBuilder = sBuilder.AddSimpleField(s_SourceCentroid, typeof(XYZ));
                fBuilder.SetUnitType(UnitType.UT_Length);
                fBuilder = sBuilder.AddSimpleField(s_MassHeight, typeof(double));
                fBuilder.SetUnitType(UnitType.UT_Length);
                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n" + ex.Message, "Create Mass Data Storage Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }
    }

    public static class MassConfigDataStorageUtil
    {
        private static Guid schemaId = new Guid("DE42E2F9-5273-4394-9280-BC0910669112");
        private static Guid subSchemaId = new Guid("13F92759-261C-4883-840A-F1182465474A");
        private static Schema m_schema = null;
        private static Schema subSchema = null;

        private static string s_HostCategory = "HostCategory";
        private static string s_MassCategory = "MassCategory";
        private static string s_ParameterUpdateType = "ParameterUpdateType";
        private static string s_SetDefaultHeight = "SetDefaultHeight";
        private static string s_UserHeight = "UserHeight";
        private static string s_ParameterMaps = "MassParameters"; //sub entity name

        private static string s_HostParameterName = "HostParameterName";
        private static string s_MassParameterName = "MassParameterName";

        public static MassConfiguration GetMassConfiguration(Document doc, SourceType massSource)
        {
            MassConfiguration massConfig = new MassConfiguration();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }

                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetMassConfigurationStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(m_schema);
                            string hostCategory = entity.Get<string>(m_schema.GetField(s_HostCategory));
                            if (hostCategory == massSource.ToString())
                            {
                                massConfig.MassSourceType = massSource;
                                massConfig.HostCategory = hostCategory;
                                massConfig.MassCategory = entity.Get<string>(m_schema.GetField(s_MassCategory));
                                massConfig.UpdateType = (ParameterUpdateType)Enum.Parse(typeof(ParameterUpdateType), entity.Get<string>(m_schema.GetField(s_ParameterUpdateType)));
                                massConfig.UserHeight = entity.Get<double>(m_schema.GetField(s_UserHeight), DisplayUnitType.DUT_DECIMAL_FEET);
                                massConfig.SetDefaultHeight = entity.Get<bool>(m_schema.GetField(s_SetDefaultHeight));

                                List<ParameterMapInfo> massParameters = new List<ParameterMapInfo>();
                                IList<Entity> subEntities = entity.Get<IList<Entity>>(m_schema.GetField(s_ParameterMaps));
                                foreach (Entity subE in subEntities)
                                {
                                    string hostParamName = subE.Get<string>(subSchema.GetField(s_HostParameterName));
                                    ParameterInfo hostParamInfo = GetParameterInfo(doc, massConfig.HostCategory, hostParamName);

                                    string massParamName = subE.Get<string>(subSchema.GetField(s_MassParameterName));
                                    ParameterInfo massParamInfo = GetParameterInfo(doc, massConfig.MassCategory, massParamName);

                                    if (null != hostParamInfo && null != massParamInfo)
                                    {
                                        ParameterMapInfo mapInfo = new ParameterMapInfo(hostParamInfo, massParamInfo);
                                        massParameters.Add(mapInfo);
                                    }
                                }
                                massConfig.MassParameters = massParameters;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get mass configuration.\n" + ex.Message, "Get Mass Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return massConfig;
        }

        public static bool StoreMassConfiguration(Document doc, MassConfiguration mc)
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
                    IList<DataStorage> savedStorage = GetMassConfigurationStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Delete Storage");
                            try
                            {
                                foreach (DataStorage storage in savedStorage)
                                {
                                    Entity entity = storage.GetEntity(m_schema);
                                    string hostCategory = entity.Get<string>(m_schema.GetField(s_HostCategory));
                                    if (hostCategory == mc.HostCategory)
                                    {
                                        doc.Delete(storage.Id);
                                        break;
                                    }
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

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Save Storage");
                        try
                        {
                            DataStorage storage = DataStorage.Create(doc);
                            Entity entity = new Entity(schemaId);
                            entity.Set<string>(s_HostCategory, mc.HostCategory);
                            entity.Set<string>(s_MassCategory, mc.MassCategory);
                            entity.Set<string>(s_ParameterUpdateType, mc.UpdateType.ToString());
                            entity.Set<bool>(s_SetDefaultHeight, mc.SetDefaultHeight);
                            entity.Set<double>(s_UserHeight, mc.UserHeight, DisplayUnitType.DUT_DECIMAL_FEET);

                            List<Entity> subEntities = new List<Entity>();
                            foreach (ParameterMapInfo paramMapInfo in mc.MassParameters)
                            {
                                Entity subEntity = new Entity(subSchemaId);
                                subEntity.Set<string>(s_HostParameterName, paramMapInfo.HostParamInfo.ParameterName);
                                subEntity.Set<string>(s_MassParameterName, paramMapInfo.MassParamInfo.ParameterName);
                                subEntities.Add(subEntity);
                            }

                            entity.Set<IList<Entity>>(s_ParameterMaps, subEntities);
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
                MessageBox.Show("Failed to store mass configuration.\n" + ex.Message, "Store Mass Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return stored;
        }


        private static ParameterInfo GetParameterInfo(Document doc, string categoryName, string parameterName)
        {
            ParameterInfo paramInfo = null;
            try
            {
                Categories categories = doc.Settings.Categories;
                Category category = categories.get_Item(categoryName);
                ElementCategoryFilter catFilter = new ElementCategoryFilter(category.Id);

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> elements = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().ToList();
                if (elements.Count > 0)
                {
                    Element element = elements.First();
                    Parameter param = element.LookupParameter(parameterName);
                    if (null != param)
                    {
                        paramInfo = new ParameterInfo(param);
                        return paramInfo;
                    }
                }
                else
                {
                    List<ElementId> categoryIds = new List<ElementId>();
                    categoryIds.Add(category.Id);
                    List<ElementId> parameterIds = ParameterFilterUtilities.GetFilterableParametersInCommon(doc, categoryIds).ToList();

                    if (parameterName == "Comments")
                    {
                        paramInfo = new ParameterInfo();
                        paramInfo.ParameterName = "Comments";
                        paramInfo.ParamStorageType = StorageType.String;
                        paramInfo.ParamType = ParameterType.Text;
                        return paramInfo;
                    }


                    if (parameterName == "Mark")
                    {
                        paramInfo = new ParameterInfo();
                        paramInfo.ParameterName = "Mark";
                        paramInfo.ParamStorageType = StorageType.String;
                        paramInfo.ParamType = ParameterType.Text;
                        return paramInfo;
                    }


                    DefinitionBindingMapIterator bindingMapIterator = doc.ParameterBindings.ForwardIterator();
                    while (bindingMapIterator.MoveNext())
                    {
                        InstanceBinding binding = bindingMapIterator.Current as InstanceBinding;
                        if (null != binding)
                        {
                            if (binding.Categories.Contains(category))
                            {
                                Definition paramDefinition = bindingMapIterator.Key;
                                if (paramDefinition.Name == parameterName)
                                {
                                    paramInfo = new ParameterInfo();
                                    paramInfo.ParameterName = paramDefinition.Name;
                                    paramInfo.ParamType = paramDefinition.ParameterType;
                                    return paramInfo;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter information.\n" + ex.Message, "Get Parameter Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return paramInfo;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder subBuilder = new SchemaBuilder(subSchemaId);
                subBuilder.SetSchemaName("MassParameterMaps");
                subBuilder.AddSimpleField(s_HostParameterName, typeof(string));
                subBuilder.AddSimpleField(s_MassParameterName, typeof(string));
                subSchema = subBuilder.Finish();

                SchemaBuilder sBuilder = new SchemaBuilder(schemaId);
                sBuilder.SetSchemaName("MassConfiguration");
                sBuilder.AddSimpleField(s_HostCategory, typeof(string));
                sBuilder.AddSimpleField(s_MassCategory, typeof(string));
                sBuilder.AddSimpleField(s_ParameterUpdateType, typeof(string));
                sBuilder.AddSimpleField(s_SetDefaultHeight, typeof(bool));
                FieldBuilder fBuilder = sBuilder.AddSimpleField(s_UserHeight, typeof(double));
                fBuilder.SetUnitType(UnitType.UT_Length);

                fBuilder = sBuilder.AddArrayField(s_ParameterMaps, typeof(Entity));
                fBuilder.SetSubSchemaGUID(subSchemaId);

                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetMassConfigurationStorage(Document doc, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }

    public class MassConfiguration
    {
        private SourceType massSourceType = SourceType.None;
        private string hostCategory = "";
        private string massCategory = "";
        private ParameterUpdateType updateType = ParameterUpdateType.None;
        private double userHeight = 10;
        private bool setDefaultHeight = false;
        private List<ParameterMapInfo> massParameters = new List<ParameterMapInfo>();

        public SourceType MassSourceType { get { return massSourceType; } set { massSourceType = value; } }
        public string HostCategory { get { return hostCategory; } set { hostCategory = value; } }
        public string MassCategory { get { return massCategory; } set { massCategory = value; } }
        public ParameterUpdateType UpdateType { get { return updateType; } set { updateType = value; } }
        public double UserHeight { get { return userHeight; } set { userHeight = value; } }
        public bool SetDefaultHeight { get { return setDefaultHeight; } set { setDefaultHeight = value; } }
        public List<ParameterMapInfo> MassParameters { get { return massParameters; } set { massParameters = value; } }

        public MassConfiguration()
        {
        }
    }

}
