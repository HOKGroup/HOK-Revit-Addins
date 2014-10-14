using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.SmartBCF.Utils
{
    public static class DataStorageUtil
    {
        public static Guid bcfSchemaId = new Guid("7DB00972-B525-4C37-AC97-BCF1E52C1E49");
        public static Guid colorSchemaId = new Guid("83E4C48C-83B5-4696-86D7-653C2BBF277D");
        public static Guid categorySchemaId = new Guid("5106EC42-8A60-4508-BD8D-87C8562FD40C");
        public static Schema bcf_Schema = Schema.Lookup(bcfSchemaId);
        public static Schema color_Schema = Schema.Lookup(colorSchemaId);
        public static Schema category_Schema = Schema.Lookup(categorySchemaId);

        //Linked BCF File Info
        private static string s_linkedBCFName = "LinkedBCFName"; //Name of the linked BCF
        private static string s_linkedBCFFileId = "LinkedBCFFileId"; //Google spreadsheet Id
        private static string s_sharedLinkId = "SahredLinkId"; //bcf project Id
        private static string s_sharedLinkAddress = "SharedLinkAddress"; //full address of the shared folder link
        private static string s_sharedFolderTitle = "SharedFolderTitle"; //shared folder name

        //BCF Color Schemes
        private static string s_colorSchemeId = "ColorSchemeId";
        private static string s_schemeName = "SchemeName";
        private static string s_parameterName = "ParameterName";
        private static string s_parameterValue = "ParameterValue";
        private static string s_colorR = "ColorR";
        private static string s_colorG = "ColorG";
        private static string s_colorB = "ColorB";

        //BCF Categories
        private static string s_categoryNames = "CategoryNames";

        public static Dictionary<string/*fileId*/, LinkedBcfFileInfo> ReadLinkedBCFFileInfo(Document doc)
        {
            Dictionary<string, LinkedBcfFileInfo> linkedBcfFiles = new Dictionary<string, LinkedBcfFileInfo>();
            try
            {
                if (null == bcf_Schema)
                {
                    bcf_Schema = CreateBCFSchema();
                }

                if (null != bcf_Schema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, bcf_Schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(bcf_Schema);
                            string fileName = entity.Get<string>(bcf_Schema.GetField(s_linkedBCFName));
                            string fileId = entity.Get<string>(bcf_Schema.GetField(s_linkedBCFFileId));
                            string folderId = entity.Get<string>(bcf_Schema.GetField(s_sharedLinkId));
                            string folderAddress = entity.Get<string>(bcf_Schema.GetField(s_sharedLinkAddress));
                            string folderTitle = entity.Get<string>(bcf_Schema.GetField(s_sharedFolderTitle));

                            LinkedBcfFileInfo fileInfo = new LinkedBcfFileInfo(fileName, fileId, folderAddress, folderId, folderTitle);
                            if (!linkedBcfFiles.ContainsKey(fileId))
                            {
                                linkedBcfFiles.Add(fileId, fileInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read linked BCF Files Info from data storage.\n" + ex.Message, "Read Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedBcfFiles;
        }

        public static ColorSchemeInfo ReadColorSchemeInfo(Document doc)
        {
            ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
            try
            {
                List<string> categoryNames = ReadBCFCategories(doc);

                if (null == color_Schema)
                {
                    color_Schema = CreateColorSchema();
                }

                if (null != color_Schema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, color_Schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(color_Schema);
                            string colorSchemeId = entity.Get<string>(color_Schema.GetField(s_colorSchemeId));
                            string colorSchemeName = entity.Get<string>(color_Schema.GetField(s_schemeName));
                            string parameterName = entity.Get<string>(color_Schema.GetField(s_parameterName));
                            string parameterValue = entity.Get<string>(color_Schema.GetField(s_parameterValue));
                            string colorR = entity.Get<string>(color_Schema.GetField(s_colorR));
                            string colorG = entity.Get<string>(color_Schema.GetField(s_colorG));
                            string colorB = entity.Get<string>(color_Schema.GetField(s_colorB));
                            byte[] colorBytes = new byte[3];
                            colorBytes[0] = byte.Parse(colorR);
                            colorBytes[1] = byte.Parse(colorG);
                            colorBytes[2] = byte.Parse(colorB);

                            var schemes = from scheme in schemeInfo.ColorSchemes where scheme.SchemeId == colorSchemeId select scheme;
                            if (schemes.Count() > 0)
                            {
                                for (int i = 0; i < schemeInfo.ColorSchemes.Count; i++)
                                {
                                    if (schemeInfo.ColorSchemes[i].SchemeId == colorSchemeId)
                                    {
                                        ColorDefinition cd = new ColorDefinition();
                                        cd.ParameterValue = parameterValue;
                                        cd.Color = colorBytes;

                                        System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
                                        cd.BackgroundColor = new SolidColorBrush(windowColor);

                                        schemeInfo.ColorSchemes[i].ColorDefinitions.Add(cd);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                ColorScheme scheme = new ColorScheme();
                                scheme.SchemeId = colorSchemeId;
                                scheme.SchemeName = colorSchemeName;
                                scheme.Categories = categoryNames;
                                scheme.ParameterName = parameterName;
                                scheme.DefinitionBy = DefinitionType.ByValue;

                                ColorDefinition cd = new ColorDefinition();
                                cd.ParameterValue = parameterValue;
                                cd.Color = colorBytes;

                                System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(cd.Color[0], cd.Color[1], cd.Color[2]);
                                cd.BackgroundColor = new SolidColorBrush(windowColor);

                                scheme.ColorDefinitions.Add(cd);

                                schemeInfo.ColorSchemes.Add(scheme);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read color scheme info from data storage.\n"+ex.Message, "Read Color Scheme Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schemeInfo;
        }

        public static List<string> ReadBCFCategories(Document doc)
        {
            List<string> categoryNames = new List<string>();
            try
            {
                if (null == category_Schema)
                {
                    category_Schema = CreateCategorySchema();
                }

                if (null != category_Schema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, category_Schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(category_Schema);
                            IList<string> bcfCategoryNames = entity.Get<IList<string>>(category_Schema.GetField(s_categoryNames));
                            categoryNames = bcfCategoryNames.ToList();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read BCF category names from data storate.\n"+ex.Message, "Read BCF Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return categoryNames;
        }

        public static bool UpdateLinkedBCFFileInfo(Document doc, Dictionary<string, LinkedBcfFileInfo> linkedBcfFiles)
        {
            bool result = false;
            try
            {
                if (null == bcf_Schema)
                {
                    bcf_Schema = CreateBCFSchema();
                }

                if (null != bcf_Schema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, bcf_Schema);
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
                            foreach (LinkedBcfFileInfo info in linkedBcfFiles.Values)
                            {
                                DataStorage storage = DataStorage.Create(doc);
                                Entity entity = new Entity(bcfSchemaId);
                                entity.Set<string>(s_linkedBCFName, info.BCFName);
                                entity.Set<string>(s_linkedBCFFileId, info.BCFFileId);
                                entity.Set<string>(s_sharedLinkId, info.SharedLinkId);
                                entity.Set<string>(s_sharedLinkAddress, info.SharedLinkAddress);
                                entity.Set<string>(s_sharedFolderTitle, info.SharedFolderName);
                                storage.SetEntity(entity);
                            }

                            trans.Commit();
                            result = true;
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
                MessageBox.Show("Failed to update BCF file history.\n" + ex.Message, "Update BCF File History", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static bool UpdateColorSchemeInfo(Document doc, ColorSchemeInfo schemeInfo)
        {
            bool result = false;
            try
            {
                if (null == color_Schema)
                {
                    color_Schema = CreateColorSchema();
                }

                if (null != color_Schema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, color_Schema);
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
                            foreach (ColorScheme colorScheme in schemeInfo.ColorSchemes)
                            {
                                foreach (ColorDefinition colorDefinition in colorScheme.ColorDefinitions)
                                {
                                    DataStorage storage = DataStorage.Create(doc);
                                    Entity entity = new Entity(colorSchemaId);
                                    entity.Set<string>(s_colorSchemeId, colorScheme.SchemeId);
                                    entity.Set<string>(s_schemeName, colorScheme.SchemeName);
                                    entity.Set<string>(s_parameterName, colorScheme.ParameterName);
                                    entity.Set<string>(s_parameterValue, colorDefinition.ParameterValue);
                                    entity.Set<string>(s_colorR, colorDefinition.Color[0].ToString());
                                    entity.Set<string>(s_colorG, colorDefinition.Color[1].ToString());
                                    entity.Set<string>(s_colorB, colorDefinition.Color[2].ToString());
                                    storage.SetEntity(entity);
                                }
                            }

                            trans.Commit();
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to add Color Schemes to data storage.\n" + ex.Message, "Update Data Stroage", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update Color Schemes Info to data storage.\n" + ex.Message, "Update Color Schemes Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static bool UpdateCategoryNames(Document doc, List<string> categoryNames)
        {
            bool result = false;
            try
            {
                if (null == category_Schema)
                {
                    category_Schema = CreateCategorySchema();
                }

                if (null != category_Schema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, category_Schema);
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
                            DataStorage storage = DataStorage.Create(doc);
                            Entity entity = new Entity(categorySchemaId);
                            entity.Set<IList<string>>(s_categoryNames, categoryNames);
                            storage.SetEntity(entity);

                            trans.Commit();
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to add category names to data storage.\n" + ex.Message, "Update Data Stroage", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update category names to data storage.\n" + ex.Message, "Update BCF Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static Schema CreateBCFSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(bcfSchemaId);
                schemaBuilder.SetSchemaName("LinkedBCFs");
                schemaBuilder.AddSimpleField(s_linkedBCFName, typeof(string));
                schemaBuilder.AddSimpleField(s_linkedBCFFileId, typeof(string));
                schemaBuilder.AddSimpleField(s_sharedLinkId, typeof(string));
                schemaBuilder.AddSimpleField(s_sharedLinkAddress, typeof(string));
                schemaBuilder.AddSimpleField(s_sharedFolderTitle, typeof(string));
                schema = schemaBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create BCF schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        public static Schema CreateColorSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(colorSchemaId);
                schemaBuilder.SetSchemaName("BCFColorSchemes");
                schemaBuilder.AddSimpleField(s_colorSchemeId, typeof(string));
                schemaBuilder.AddSimpleField(s_schemeName, typeof(string));
                schemaBuilder.AddSimpleField(s_parameterName, typeof(string));
                schemaBuilder.AddSimpleField(s_parameterValue, typeof(string));
                schemaBuilder.AddSimpleField(s_colorR, typeof(string));
                schemaBuilder.AddSimpleField(s_colorG, typeof(string));
                schemaBuilder.AddSimpleField(s_colorB, typeof(string));
                schema = schemaBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create BCF Color schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        public static Schema CreateCategorySchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(categorySchemaId);
                schemaBuilder.SetSchemaName("BCFCategoryNames");
                schemaBuilder.AddArrayField(s_categoryNames, typeof(string));
                schema = schemaBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create BCF categories schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetDataStorage(Document doc, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }

    public class LinkedBcfFileInfo
    {
        private string bcfName = "";
        private string bcfFileId = "";
        private string sharedLinkId = "";
        private string sharedLinkAddress = "";
        private string sharedFolderName = "";

        public string BCFName { get { return bcfName; } set { bcfName = value; } }
        public string BCFFileId { get { return bcfFileId; } set { bcfFileId = value; } }
        public string SharedLinkId { get { return sharedLinkId; } set { sharedLinkId = value; } }
        public string SharedLinkAddress { get { return sharedLinkAddress; } set { sharedLinkAddress = value; } }
        public string SharedFolderName { get { return sharedFolderName; } set { sharedFolderName = value; } }

        public LinkedBcfFileInfo(string name, string fileId, string folderAddress, string folderId, string folderName)
        {
            bcfName = name;
            bcfFileId = fileId;
            sharedLinkAddress = folderAddress;
            sharedLinkId = folderId;
            sharedFolderName = folderName;
        }

        public LinkedBcfFileInfo() { }
    }
}
