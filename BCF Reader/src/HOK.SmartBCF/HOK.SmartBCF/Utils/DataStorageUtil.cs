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
using HOK.SmartBCF.GoogleUtils;

namespace HOK.SmartBCF.Utils
{
    public static class DataStorageUtil
    {
        public static Guid bcfSchemaId = new Guid("06060EA4-5B9D-4234-94E7-4A8D6ED45620");
        public static Schema bcf_Schema = Schema.Lookup(bcfSchemaId);

        //Linked BCF File Info
        private static string s_linkedBCFName = "LinkedBCFName"; //Name of the linked BCF
        private static string s_linkedBCFMarkupId = "LinkedBCFMarkupId"; //Google spreadsheet Id
        private static string s_linkedBCFViewpointId = "LinkedBCFViewpointId"; //Google spreadsheet Id
        private static string s_sharedLinkId = "SahredLinkId"; //bcf project Id
        private static string s_sharedLinkAddress = "SharedLinkAddress"; //full address of the shared folder link
        private static string s_sharedFolderTitle = "SharedFolderTitle"; //shared folder name

        public static Dictionary<string/*markupId*/, LinkedBcfFileInfo> ReadLinkedBCFFileInfo( Document doc, string bcfProjectFolderId)
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
                            string markupId = entity.Get<string>(bcf_Schema.GetField(s_linkedBCFMarkupId));
                            string viewpointId = entity.Get<string>(bcf_Schema.GetField(s_linkedBCFViewpointId));
                            string folderId = entity.Get<string>(bcf_Schema.GetField(s_sharedLinkId));
                            string folderAddress = entity.Get<string>(bcf_Schema.GetField(s_sharedLinkAddress));
                            string folderTitle = entity.Get<string>(bcf_Schema.GetField(s_sharedFolderTitle));

                            if (bcfProjectFolderId == folderId)
                            {
                               
                                LinkedBcfFileInfo fileInfo = new LinkedBcfFileInfo(fileName, markupId, viewpointId, folderAddress, folderTitle, folderId);
                                if (!linkedBcfFiles.ContainsKey(markupId))
                                {
                                    linkedBcfFiles.Add(markupId, fileInfo);
                                }
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
                                entity.Set<string>(s_linkedBCFMarkupId, info.MarkupFileId);
                                entity.Set<string>(s_linkedBCFViewpointId, info.ViewpointFileId);
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

        public static Schema CreateBCFSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(bcfSchemaId);
                schemaBuilder.SetSchemaName("LinkedBCFs");
                schemaBuilder.AddSimpleField(s_linkedBCFName, typeof(string));
                schemaBuilder.AddSimpleField(s_linkedBCFMarkupId, typeof(string));
                schemaBuilder.AddSimpleField(s_linkedBCFViewpointId, typeof(string));
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
        private string markupFileId = "";
        private string viewpointFileId = "";
        private string sharedLinkId = "";
        private string sharedLinkAddress = "";
        private string sharedFolderName = "";

        public string BCFName { get { return bcfName; } set { bcfName = value; } }
        public string MarkupFileId { get { return markupFileId; } set { markupFileId = value; } }
        public string ViewpointFileId { get { return viewpointFileId; } set { viewpointFileId = value; } }
        public string SharedLinkId { get { return sharedLinkId; } set { sharedLinkId = value; } }
        public string SharedLinkAddress { get { return sharedLinkAddress; } set { sharedLinkAddress = value; } }
        public string SharedFolderName { get { return sharedFolderName; } set { sharedFolderName = value; } }

        public LinkedBcfFileInfo(string name, string markupId, string viewpointId, string folderAddress, string folderName, string folderId)
        {
            bcfName = name;
            markupFileId = markupId;
            viewpointFileId = viewpointId;
            sharedLinkAddress = folderAddress;
            sharedFolderName = folderName;
            sharedLinkId = folderId;
        }

        public LinkedBcfFileInfo() { }
    }
}
