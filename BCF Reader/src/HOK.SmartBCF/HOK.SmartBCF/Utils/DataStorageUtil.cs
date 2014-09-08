using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.SmartBCF.Utils
{
    public static class DataStorageUtil
    {
        public static Guid schemaId = new Guid("7DB00972-B525-4C37-AC97-BCF1E52C1E49");
        public static Guid subSchemaId = new Guid("FA29AC48-D6F7-4764-8281-2078CAB05FFF");
        public static Schema m_Schema = Schema.Lookup(schemaId);

        private static string s_linkedBCFName = "LinkedBCFName"; //Name of the linked BCF
        private static string s_linkedBCFFileId = "LinkedBCFFileId"; //Google spreadsheet Id
        private static string s_sharedLinkId = "SahredLinkId";
        private static string s_sharedLinkAddress = "SharedLinkAddress";

        public static Dictionary<string/*fileId*/, LinkedBcfFileInfo> ReadDataStorage(Document doc)
        {
            Dictionary<string, LinkedBcfFileInfo> linkedBcfFiles = new Dictionary<string, LinkedBcfFileInfo>();
            try
            {
                if (null == m_Schema)
                {
                    m_Schema = CreateSchema();
                }

                if (null != m_Schema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, m_Schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(m_Schema);
                            string fileName = entity.Get<string>(m_Schema.GetField(s_linkedBCFName));
                            string fileId = entity.Get<string>(m_Schema.GetField(s_linkedBCFFileId));
                            string folderId = entity.Get<string>(m_Schema.GetField(s_sharedLinkId));
                            string folderAddress = entity.Get<string>(m_Schema.GetField(s_sharedLinkAddress));

                            LinkedBcfFileInfo fileInfo = new LinkedBcfFileInfo(fileName, fileId, folderAddress, folderId);
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
                MessageBox.Show("Failed to read data storage.\n" + ex.Message, "Read Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkedBcfFiles;
        }

        public static bool UpdateDataStorage(Document doc, Dictionary<string, LinkedBcfFileInfo> linkedBcfFiles)
        {
            bool result = false;
            try
            {
                if (null == m_Schema)
                {
                    m_Schema = CreateSchema();
                }

                if (null != m_Schema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, m_Schema);
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
                                Entity entity = new Entity(schemaId);
                                entity.Set<string>(s_linkedBCFName, info.BCFName);
                                entity.Set<string>(s_linkedBCFFileId, info.BCFFileId);
                                entity.Set<string>(s_sharedLinkId, info.SharedLinkId);
                                entity.Set<string>(s_sharedLinkAddress, info.SharedLinkAddress);
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

        public static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("LinkedBCFs");
                schemaBuilder.AddSimpleField(s_linkedBCFName, typeof(string));
                schemaBuilder.AddSimpleField(s_linkedBCFFileId, typeof(string));
                schemaBuilder.AddSimpleField(s_sharedLinkId, typeof(string));
                schemaBuilder.AddSimpleField(s_sharedLinkAddress, typeof(string));
                schema = schemaBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema for smartBCF.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        public string BCFName { get { return bcfName; } set { bcfName = value; } }
        public string BCFFileId { get { return bcfFileId; } set { bcfFileId = value; } }
        public string SharedLinkId { get { return sharedLinkId; } set { sharedLinkId = value; } }
        public string SharedLinkAddress { get { return sharedLinkAddress; } set { sharedLinkAddress = value; } }

        public LinkedBcfFileInfo(string name, string fileId, string folderAddress, string folderId)
        {
            bcfName = name;
            bcfFileId = fileId;
            sharedLinkAddress = folderAddress;
            sharedLinkId = folderId;
        }

        public LinkedBcfFileInfo() { }
    }
}
