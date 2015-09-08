using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.ElementMover
{
    public static class MoverDataStorageUtil
    {
        private static Guid elementSchemaId = new Guid("8545E731-06FC-40C6-A7CB-76D2B56493F1");
        private static Guid familySchemaId = new Guid("9A6B4A7F-C060-49EC-863E-F6D9D25FC444");
        private static Schema elementSchema = null;
        private static Schema familySchema = null;

        private static string s_SourceLinkInstanceId = "SourceLinkInstanceId";

        //Linked Element Info
        private static string s_LinkElementType = "LinkElementType";
        private static string s_SourceElementId = "SourceElementId";
        private static string s_SourceUniqueId = "SourceUniqueId";
        private static string s_LinkedElementId = "LinkedElementId";
        private static string s_LinkedUniqueId = "LinkedUniqueId";
        
        //Linked Family Info
        private static string s_SourceSymbolId = "SourceSymbolId";
        private static string s_LinkedSymbolId = "LinkedSymbolId";

        public static LinkedElementInfo GetLinkedElementInfo(Element linkedElement)
        {
            LinkedElementInfo linkInfo = null;
            try
            {
                if (null == elementSchema)
                {
                    elementSchema = CreateElementSchema();
                }

                if (null != elementSchema)
                {
                    Entity entity = linkedElement.GetEntity(elementSchema);
                    if (entity.IsValid())
                    {
                        linkInfo = new LinkedElementInfo();
                        linkInfo.LinkElementType = (LinkType)Enum.Parse(typeof(LinkType), entity.Get<string>(elementSchema.GetField(s_LinkElementType)));
                        linkInfo.SourceElementId = entity.Get<ElementId>(elementSchema.GetField(s_SourceElementId));
                        linkInfo.SourceUniqueId = entity.Get<string>(elementSchema.GetField(s_SourceUniqueId));
                        linkInfo.LinkedElementId = entity.Get<ElementId>(elementSchema.GetField(s_LinkedElementId));
                        linkInfo.LinkedUniqueId = entity.Get<string>(elementSchema.GetField(s_LinkedUniqueId));
                        linkInfo.SourceLinkInstanceId = entity.Get<ElementId>(elementSchema.GetField(s_SourceLinkInstanceId));
                    }
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the information from the linked element.\n" + ex.Message, "Get Linked Element Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkInfo;
        }

        public static bool UpdateLinkedElementInfo(LinkedElementInfo linkInfo, Element linkedElement)
        {
            bool updated = false;
            try
            {
                if (null == elementSchema)
                {
                    elementSchema = CreateElementSchema();
                }

                if (null != elementSchema)
                {
                    Entity entity = linkedElement.GetEntity(elementSchema);
                    if (entity.IsValid())
                    {
                        linkedElement.DeleteEntity(elementSchema);
                    }
                    entity = new Entity(elementSchema);
                    entity.Set<string>(elementSchema.GetField(s_LinkElementType), linkInfo.LinkElementType.ToString());
                    entity.Set<ElementId>(elementSchema.GetField(s_SourceElementId), linkInfo.SourceElementId);
                    entity.Set<string>(elementSchema.GetField(s_SourceUniqueId), linkInfo.SourceUniqueId);
                    entity.Set<ElementId>(elementSchema.GetField(s_LinkedElementId), linkInfo.LinkedElementId);
                    entity.Set<string>(elementSchema.GetField(s_LinkedUniqueId), linkInfo.LinkedUniqueId);
                    entity.Set<ElementId>(elementSchema.GetField(s_SourceLinkInstanceId), linkInfo.SourceLinkInstanceId);
                    linkedElement.SetEntity(entity);
                    updated = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the information of the linked element.\n" + ex.Message, "Update Linked Element Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        public static bool RemoveLinkedElementInfo(Element linkedElement)
        {
            bool removed = false;
            try
            {
                Entity entity = linkedElement.GetEntity(elementSchema);
                if (entity.IsValid())
                {
                    linkedElement.DeleteEntity(elementSchema);
                    removed = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove the linked element info from data storage.\n" + ex.Message, "Remove Linked Element Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return removed;
        }

        public static LinkedFamilyInfo GetLinkedFamilyInfo(ElementType linkedType)
        {
            LinkedFamilyInfo familyInfo = null;
            try
            {
                if (null == familySchema)
                {
                    familySchema = CreateFamilySchema();
                }

                if (null != familySchema)
                {
                    Entity entity = linkedType.GetEntity(familySchema);
                    if (entity.IsValid())
                    {
                        familyInfo = new LinkedFamilyInfo();
                        familyInfo.SourceLinkInstanceId = entity.Get<ElementId>(familySchema.GetField(s_SourceLinkInstanceId));
                        familyInfo.SourceTypeId = entity.Get<ElementId>(familySchema.GetField(s_SourceSymbolId));
                        familyInfo.TargetTypeId = entity.Get<ElementId>(familySchema.GetField(s_LinkedSymbolId));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the information from the linked family type.\n" + ex.Message, "Get Linked Family Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return familyInfo;
        }

        public static bool UpdateLinkedFamilyInfo(LinkedFamilyInfo familyInfo, ElementType linkedType)
        {
            bool updated = false;
            try
            {
                if (null == familySchema)
                {
                    familySchema = CreateFamilySchema();
                }

                if (null != familySchema)
                {
                    Entity entity = linkedType.GetEntity(familySchema);
                    if (entity.IsValid())
                    {
                        linkedType.DeleteEntity(familySchema);
                    }
                    entity = new Entity(familySchema);
                    entity.Set<ElementId>(familySchema.GetField(s_SourceLinkInstanceId), familyInfo.SourceLinkInstanceId);
                    entity.Set<ElementId>(familySchema.GetField(s_SourceSymbolId), familyInfo.SourceTypeId);
                    entity.Set<ElementId>(familySchema.GetField(s_LinkedSymbolId), familyInfo.TargetTypeId);

                    linkedType.SetEntity(entity);
                    updated = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the information of the linked family.\n" + ex.Message, "Update Linked Family Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        public static bool RemoveLinkedFamilyInfo(ElementType linkedType)
        {
            bool removed = false;
            try
            {
                Entity entity = linkedType.GetEntity(familySchema);
                if (entity.IsValid())
                {
                    linkedType.DeleteEntity(familySchema);
                    removed = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove the linked family info from data storage.\n" + ex.Message, "Remove Linked Family Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return removed;
        }

        private static Schema CreateElementSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder sBuilder = new SchemaBuilder(elementSchemaId);
                sBuilder.SetSchemaName("LinkedElementInfo");
                sBuilder.AddSimpleField(s_LinkElementType, typeof(string));
                sBuilder.AddSimpleField(s_SourceElementId, typeof(ElementId));
                sBuilder.AddSimpleField(s_SourceUniqueId, typeof(string));

                sBuilder.AddSimpleField(s_LinkedElementId, typeof(ElementId));
                sBuilder.AddSimpleField(s_LinkedUniqueId, typeof(string));
                sBuilder.AddSimpleField(s_SourceLinkInstanceId, typeof(ElementId));
               
                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create element schema.\n" + ex.Message, "Create Element Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static Schema CreateFamilySchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder sBuilder = new SchemaBuilder(familySchemaId);
                sBuilder.SetSchemaName("LinkedFamilyInfo");
                sBuilder.AddSimpleField(s_SourceLinkInstanceId, typeof(ElementId));
                sBuilder.AddSimpleField(s_SourceSymbolId, typeof(ElementId));
                sBuilder.AddSimpleField(s_LinkedSymbolId, typeof(ElementId));

                schema = sBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create family schema.\n" + ex.Message, "Create Family Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }
    }
}
