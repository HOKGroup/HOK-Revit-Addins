using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.Utilities.RoomElevation
{
    public static class ElevationCreatorDataStorageUtil
    {
        public static Guid schemaId = new Guid("259C0340-CE24-4CFF-BAA3-F2DE59225EFC");
        public static Schema m_schema = Schema.Lookup(schemaId);

        private static string s_RoomNumber = "RoomNumber";
        private static string s_RoomName = "RoomName";
        private static string s_RoomId = "RoomId";
        private static string s_IsLinked = "IsLinked";
        private static string s_RvtInstanceId = "RvtInstanceId";
        private static string s_elevationMarkId = "ElevationMarkId";
        private static string s_ElevationIds = "ElevationIds";

        public static Dictionary<int, RoomElevationProperties> GetRoomElevationProperties(Document doc, Dictionary<int, LinkedInstanceProperties> linkedDocs)
        {
            Dictionary<int, RoomElevationProperties> pDictionary = new Dictionary<int, RoomElevationProperties>();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }
                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetRoomElevationStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        foreach (DataStorage storage in savedStorage)
                        {
                            Entity entity = storage.GetEntity(m_schema);
                            string roomNumber = entity.Get<string>(m_schema.GetField(s_RoomNumber));
                            string roomName = entity.Get<string>(m_schema.GetField(s_RoomName));
                            ElementId roomId = entity.Get<ElementId>(m_schema.GetField(s_RoomId));
                            bool isLinked = entity.Get<bool>(m_schema.GetField(s_IsLinked));
                            ElementId rvtInstanceId = entity.Get<ElementId>(m_schema.GetField(s_RvtInstanceId));
                            ElementId elevationMarkId = entity.Get<ElementId>(m_schema.GetField(s_elevationMarkId));
                            IList<ElementId> elevationIds = entity.Get<IList<ElementId>>(m_schema.GetField(s_ElevationIds));

                            if (roomId != ElementId.InvalidElementId)
                            {
                                Room room = null;
                                RoomElevationProperties rep = null;
                                if (isLinked && linkedDocs.ContainsKey(rvtInstanceId.IntegerValue))
                                {
                                    LinkedInstanceProperties lip = linkedDocs[rvtInstanceId.IntegerValue];
                                    room = lip.LinkedDocument.GetElement(roomId) as Room;
                                    if (null != room)
                                    {
                                        rep = new RoomElevationProperties(room, rvtInstanceId.IntegerValue);
                                    }
                                }
                                else
                                {
                                    room = doc.GetElement(roomId) as Room;
                                    if (null != room)
                                    {
                                        rep = new RoomElevationProperties(room);
                                    }
                                }

                                if (null != rep)
                                {
                                    rep.ElevationMarkId = elevationMarkId.IntegerValue;
                                    foreach (ElementId eId in elevationIds)
                                    {
                                        ViewSection viewSection = doc.GetElement(eId) as ViewSection;
                                        if (null != viewSection)
                                        {
                                            ElevationViewProperties evp = new ElevationViewProperties(viewSection);
                                            if (!rep.ElevationViews.ContainsKey(evp.ViewId))
                                            {
                                                rep.ElevationViews.Add(evp.ViewId, evp);
                                            }
                                        }
                                    }
                                    if (!pDictionary.ContainsKey(rep.RoomId))
                                    {
                                        pDictionary.Add(rep.RoomId, rep);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get room elevation properties from data storage.\n" + ex.Message, "Elevation Creator : GetRoomElevationProperties", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return pDictionary;
        }

        public static bool StoreRoomElevationProperties(Document doc, Dictionary<int,RoomElevationProperties> dictionary)
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
                    
                    IList<DataStorage> savedStorage = GetRoomElevationStorage(doc, m_schema);
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
                                MessageBox.Show("Failed to delete data storage.\n" + ex.Message, "Elevation Creator : Update Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Add New Storage");
                        try
                        {
                            foreach (RoomElevationProperties rep in dictionary.Values)
                            {
                                DataStorage storage = DataStorage.Create(doc);
                                Entity entity = new Entity(schemaId);
                                entity.Set<string>(s_RoomNumber, rep.RoomNumber);
                                entity.Set<string>(s_RoomName, rep.RoomName);
                                entity.Set<ElementId>(s_RoomId, new ElementId(rep.RoomId));
                                entity.Set<bool>(s_IsLinked,rep.IsLinked);
                                entity.Set<ElementId>(s_RvtInstanceId, new ElementId(rep.RvtLinkId));
                                entity.Set<ElementId>(s_elevationMarkId, new ElementId(rep.ElevationMarkId));

                                IList<ElementId> elevationIds = new List<ElementId>();
                                foreach (int eId in rep.ElevationViews.Keys)
                                {
                                    elevationIds.Add(new ElementId(eId));
                                }
                                entity.Set(s_ElevationIds, elevationIds);
                                storage.SetEntity(entity);
                            }

                            trans.Commit();
                            saved = true;
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to add data storage.\n" + ex.Message, "Elevation Creator : Update Data Stroage", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save room elevation properties in data storage.\n" + ex.Message, "Elevation Creator : StoreRoomeElevationProperties", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        public static bool StoreRoomElevationProperties(Document doc, RoomElevationProperties rep)
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
                    IList<DataStorage> savedStorage = GetRoomElevationStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Delete Data Storage");
                            try
                            {
                                DataStorage storageToDelete = null;
                                foreach (DataStorage ds in savedStorage)
                                {
                                    Entity entity = ds.GetEntity(m_schema);
                                    ElementId roomId = entity.Get<ElementId>(m_schema.GetField(s_RoomId));
                                    if (rep.RoomId == roomId.IntegerValue)
                                    {
                                        storageToDelete = ds;
                                         break;
                                    }
                                }
                                if (null != storageToDelete)
                                {
                                    doc.Delete(storageToDelete.Id);
                                }
                                
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to delete data storage.\n" + ex.Message, "Elevation Creator : Update Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Add New Storage");
                        try
                        {
                            DataStorage storage = DataStorage.Create(doc);
                            Entity entity = new Entity(schemaId);
                            entity.Set<string>(s_RoomNumber, rep.RoomNumber);
                            entity.Set<string>(s_RoomName, rep.RoomName);
                            entity.Set<ElementId>(s_RoomId, new ElementId(rep.RoomId));
                            entity.Set<bool>(s_IsLinked, rep.IsLinked);
                            entity.Set<ElementId>(s_RvtInstanceId, new ElementId(rep.RvtLinkId));
                            entity.Set<ElementId>(s_elevationMarkId, new ElementId(rep.ElevationMarkId));

                            IList<ElementId> elevationIds = new List<ElementId>();
                            foreach (int eId in rep.ElevationViews.Keys)
                            {
                                elevationIds.Add(new ElementId(eId));
                            }
                            entity.Set<IList<ElementId>>(s_ElevationIds, elevationIds);
                            storage.SetEntity(entity);

                            trans.Commit();
                            saved = true;
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to add data storage.\n" + ex.Message, "Elevation Creator : Update Data Stroage", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save room elevation properties in data storage.\n" + ex.Message, "Elevation Creator : StoreRoomeElevationProperties", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("RoomElevationCreator");
                schemaBuilder.AddSimpleField(s_RoomNumber, typeof(string));
                schemaBuilder.AddSimpleField(s_RoomName, typeof(string));
                schemaBuilder.AddSimpleField(s_IsLinked, typeof(bool));
                schemaBuilder.AddSimpleField(s_RoomId, typeof(ElementId));
                schemaBuilder.AddSimpleField(s_RvtInstanceId, typeof(ElementId));
                schemaBuilder.AddSimpleField(s_elevationMarkId, typeof(ElementId));
                schemaBuilder.AddArrayField(s_ElevationIds, typeof(ElementId));
                schema = schemaBuilder.Finish();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetRoomElevationStorage(Document document, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }

    public class RoomElevationProperties
    {
        private Room m_room = null;
        private string roomName = "";
        private string roomNumber = "";
        private int roomId = -1;
        private bool isLinked = false;
        private Document document = null;
        private string documentTitle = "";
        private int rvtLinkId = -1;
        private int elevationMarkId = -1;
        private Dictionary<int, ElevationViewProperties> elevationViews = new Dictionary<int, ElevationViewProperties>();

        public Room RoomObj { get { return m_room; } set { m_room = value; } }
        public string RoomName { get { return roomName; } set { roomName = value; } }
        public string RoomNumber { get { return roomNumber; } set { roomNumber = value; } }
        public int RoomId { get { return roomId; } set { roomId = value; } }
        public bool IsLinked { get { return isLinked; } set { isLinked = value; } }
        public string DocumentTitle { get { return documentTitle; } set { documentTitle = value; } }
        public int RvtLinkId { get { return rvtLinkId; } set { rvtLinkId = value; } }
        public int ElevationMarkId { get { return elevationMarkId; } set { elevationMarkId = value; } }
        public Dictionary<int, ElevationViewProperties> ElevationViews { get { return elevationViews; } set { elevationViews = value; } }

        public RoomElevationProperties(Room room)
        {
            m_room = room;
            roomId = m_room.Id.IntegerValue;

            roomName = m_room.Name;
            roomNumber = m_room.Number;


            Parameter parameter = m_room.get_Parameter(BuiltInParameter.ROOM_NAME);
            if (null != parameter)
            {
                roomName = parameter.AsString();
            }
            parameter = m_room.get_Parameter(BuiltInParameter.ROOM_NUMBER);
            if (null != parameter)
            {
                roomNumber = parameter.AsString();
            }

            document = m_room.Document;
#if RELEASE2014 || RELEASE2015
            isLinked = document.IsLinked;
#endif
            documentTitle = document.Title;

        }

        public RoomElevationProperties(Room room, int rvtInstanceId)
        {
            m_room = room;
            roomId = m_room.Id.IntegerValue;
            
            roomName = m_room.Name;
            roomNumber = m_room.Number;
            
            Parameter parameter = m_room.get_Parameter(BuiltInParameter.ROOM_NAME);
            if (null != parameter)
            {
                roomName = parameter.AsString();
            }
            parameter = m_room.get_Parameter(BuiltInParameter.ROOM_NUMBER);
            if (null != parameter)
            {
                roomNumber = parameter.AsString();
            }

            document = m_room.Document;
#if RELEASE2014 || RELEASE2015
            isLinked = document.IsLinked;
#endif
            documentTitle = document.Title;
            rvtLinkId = rvtInstanceId;
        }

        public RoomElevationProperties(RoomElevationProperties rep)
        {
            m_room = rep.RoomObj;
            roomName = rep.RoomName;
            roomNumber = rep.roomNumber;
            roomId = rep.RoomId;
            isLinked = rep.IsLinked;
            documentTitle = rep.DocumentTitle;
            rvtLinkId = rep.RvtLinkId;
            elevationMarkId = rep.ElevationMarkId;
            elevationViews = rep.ElevationViews;
        }

    }

    public class ElevationViewProperties
    {
        private ViewSection m_view = null;
        private string viewName = "";
        private int viewId = -1;
        private int viewIndex = -1;
        private bool createdByWall = false;
        private int wallId = -1;

        public ViewSection ViewObj { get { return m_view; } set { m_view = value; } }
        public string ViewName { get { return viewName; } set { viewName = value; } }
        public int ViewId { get { return viewId; } set { viewId = value; } }
        public int ViewIndex { get { return viewIndex; } set { viewIndex = value; } }
        public bool CreatedByWall { get { return createdByWall; } set { createdByWall = value; } }
        public int WallId { get { return wallId; } set { wallId = value; } }

        public ElevationViewProperties(ViewSection viewSection)
        {
            m_view = viewSection;
            viewName = m_view.ViewName;
            viewId = m_view.Id.IntegerValue;
        }

        public ElevationViewProperties(ElevationViewProperties evp)
        {
            m_view = evp.ViewObj;
            viewName = evp.ViewName;
            viewId = evp.ViewId;
            viewIndex = evp.ViewIndex;
            createdByWall = evp.CreatedByWall;
            wallId = evp.WallId;
        }
    }

    public static class ElevationCreatorSettingStorageUtil
    {
        public static Guid schemaId = new Guid("FCAAE2B3-121D-4465-BD7E-E8AAE3E853BD");
        public static Schema m_schema = Schema.Lookup(schemaId);

        private static string s_IsLinkedRoom = "IsLinkedRoom";
        private static string s_IsLinkedWall = "IsLinkedWall";
        private static string s_ViewTemplateId = "ViewTemplateId";
        private static string s_SacleByTemplate = "SacleByTemplate";
        private static string s_CustomRatio = "CustomRatio";
        private static string s_SpaceAround = "SpaceAround";
        private static string s_AEnabled = "AEnabled";
        private static string s_BEnabled = "BEnabled";
        private static string s_CEnabled = "CEnabled";
        private static string s_DEnabled = "DEnabled";
        private static string s_PrefixSelected = "PrefixSelected";
        private static string s_PrefixText = "PrefixText";
        private static string s_IntermediateSelected = "IntermediateSelected";
        private static string s_IntermediateText = "IntermediateText";
        private static string s_ElevationSelected = "ElevationSelected";
        private static string s_ABCDSelected = "ABCDSelected";
        private static string s_SuffixSelected = "SuffixSelected";
        private static string s_SuffixText = "SuffixText";


        public static ElevationCreatorSettings  GetElevationCreatorSettings(Document doc)
        {
            ElevationCreatorSettings settings = new ElevationCreatorSettings();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }
                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetElevationCreatorSettings(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        DataStorage storage = savedStorage.First();
                        Entity entity = storage.GetEntity(m_schema);

                        settings.IsLinkedRoom  = entity.Get<bool>(m_schema.GetField(s_IsLinkedRoom));
                        settings.IsLInkedWall = entity.Get<bool>(m_schema.GetField(s_IsLinkedWall));
                        settings.ViewTemplateId = entity.Get<int>(m_schema.GetField(s_ViewTemplateId));
                        settings.ScaleByTemplate = entity.Get<bool>(m_schema.GetField(s_SacleByTemplate));
                        settings.CustomScale = entity.Get<int>(m_schema.GetField(s_CustomRatio));
                        settings.SpaceAround = entity.Get<int>(m_schema.GetField(s_SpaceAround));
                        settings.AIsSelected = entity.Get<bool>(m_schema.GetField(s_AEnabled));
                        settings.BIsSelected = entity.Get<bool>(m_schema.GetField(s_BEnabled));
                        settings.CIsSelected = entity.Get<bool>(m_schema.GetField(s_CEnabled));
                        settings.DIsSelected = entity.Get<bool>(m_schema.GetField(s_DEnabled));
                        settings.PrefixSelected = entity.Get<bool>(m_schema.GetField(s_PrefixSelected));
                        settings.PrefixText = entity.Get<string>(m_schema.GetField(s_PrefixText));
                        settings.IntermediateSelected = entity.Get<bool>(m_schema.GetField(s_IntermediateSelected));
                        settings.IntermediateText = entity.Get<string>(m_schema.GetField(s_IntermediateText));
                        settings.ElevationSelected = entity.Get<bool>(m_schema.GetField(s_ElevationSelected));
                        settings.ABCDSelected = entity.Get<bool>(m_schema.GetField(s_ABCDSelected));
                        settings.SuffixSelected = entity.Get<bool>(m_schema.GetField(s_SuffixSelected));
                        settings.SuffixText = entity.Get<string>(m_schema.GetField(s_SuffixText));

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get elevation creator settings from data storage.\n" + ex.Message, "Elevation Creator : GetElevationCreatorSettings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return settings;
        }


        public static bool StoreElevationCreatorSettings(Document doc, ElevationCreatorSettings settings)
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
                    IList<DataStorage> savedStorage = GetElevationCreatorSettings(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Delete Data Storage");
                            try
                            {
                                DataStorage storageToDelete = savedStorage.First();
                                doc.Delete(storageToDelete.Id);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to delete data storage of elevation creator settings.\n" + ex.Message, "Elevation Creator : Update Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Add New Storage");
                        try
                        {
                            DataStorage storage = DataStorage.Create(doc);
                            Entity entity = new Entity(schemaId);
                            entity.Set<bool>(s_IsLinkedRoom, settings.IsLinkedRoom);
                            entity.Set<bool>(s_IsLinkedWall, settings.IsLInkedWall);
                            entity.Set<int>(s_ViewTemplateId, settings.ViewTemplateId);
                            entity.Set<bool>(s_SacleByTemplate, settings.ScaleByTemplate);
                            entity.Set<int>(s_CustomRatio, settings.CustomScale);
                            entity.Set<int>(s_SpaceAround, settings.SpaceAround);
                            entity.Set<bool>(s_AEnabled, settings.AIsSelected);
                            entity.Set<bool>(s_BEnabled, settings.BIsSelected);
                            entity.Set<bool>(s_CEnabled, settings.CIsSelected);
                            entity.Set<bool>(s_DEnabled, settings.DIsSelected);
                            entity.Set<bool>(s_PrefixSelected, settings.PrefixSelected);
                            entity.Set<string>(s_PrefixText, settings.PrefixText);
                            entity.Set<bool>(s_IntermediateSelected, settings.IntermediateSelected);
                            entity.Set<string>(s_IntermediateText, settings.IntermediateText);
                            entity.Set<bool>(s_ElevationSelected, settings.ElevationSelected);
                            entity.Set<bool>(s_ABCDSelected, settings.ABCDSelected);
                            entity.Set<bool>(s_SuffixSelected, settings.SuffixSelected);
                            entity.Set<string>(s_SuffixText, settings.SuffixText);

                            storage.SetEntity(entity);
                            trans.Commit();
                            saved = true;
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to add data storage of elevation creator settings.\n" + ex.Message, "Elevation Creator : Update Data Stroage", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save elevation creator settings in data storage.\n" + ex.Message, "Elevation Creator : StoreElevationCreatorSettings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("RoomElevationCreatorSettings");
                schemaBuilder.AddSimpleField(s_IsLinkedRoom, typeof(bool));
                schemaBuilder.AddSimpleField(s_IsLinkedWall, typeof(bool));
                schemaBuilder.AddSimpleField(s_ViewTemplateId, typeof(int));
                schemaBuilder.AddSimpleField(s_SacleByTemplate, typeof(bool));
                schemaBuilder.AddSimpleField(s_CustomRatio, typeof(int));
                schemaBuilder.AddSimpleField(s_SpaceAround, typeof(int));
                schemaBuilder.AddSimpleField(s_AEnabled, typeof(bool));
                schemaBuilder.AddSimpleField(s_BEnabled, typeof(bool));
                schemaBuilder.AddSimpleField(s_CEnabled, typeof(bool));
                schemaBuilder.AddSimpleField(s_DEnabled, typeof(bool));
                schemaBuilder.AddSimpleField(s_PrefixSelected, typeof(bool));
                schemaBuilder.AddSimpleField(s_PrefixText, typeof(string));
                schemaBuilder.AddSimpleField(s_IntermediateSelected, typeof(bool));
                schemaBuilder.AddSimpleField(s_IntermediateText, typeof(string));
                schemaBuilder.AddSimpleField(s_ElevationSelected, typeof(bool));
                schemaBuilder.AddSimpleField(s_ABCDSelected, typeof(bool));
                schemaBuilder.AddSimpleField(s_SuffixSelected, typeof(bool));
                schemaBuilder.AddSimpleField(s_SuffixText, typeof(string));
                schema = schemaBuilder.Finish();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return schema;
        }

        private static IList<DataStorage> GetElevationCreatorSettings(Document document, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }
    }

    public class ElevationCreatorSettings
    {
        private bool isLinkedRoom = false;
        private bool isLinkedWall = false;
        private int viewTemplateId = -1;
        private bool scaleByTemplate = true;
        private int customScale = 96;
        private int spaceAround = 10;
        private bool a_isSelected = true;
        private bool b_isSelected = true;
        private bool c_isSelected = true;
        private bool d_isSelected = true;
        private bool prefixSelected = true;
        private string prefixText = "Room";
        private bool intermediateSelected = true;
        private string intermediateText = "Number";
        private bool elevationSelected = true;
        private bool abcdSelected = true;
        private bool suffixSelected = false;
        private string suffixText = "";
        //transient data 
        private ViewFamilyType elevationFamilyType = null;
        private ViewPlan activeViewPlan = null;

        public bool IsLinkedRoom { get { return isLinkedRoom; } set { isLinkedRoom = value; } }
        public bool IsLInkedWall { get { return isLinkedWall; } set { isLinkedWall = value; } }
        public int ViewTemplateId { get { return viewTemplateId; } set { viewTemplateId = value; } }
        public bool ScaleByTemplate { get { return scaleByTemplate; } set { scaleByTemplate = value; } }
        public int CustomScale { get { return customScale; } set { customScale = value; } }
        public int SpaceAround { get { return spaceAround; } set { spaceAround = value; } }
        public bool AIsSelected { get { return a_isSelected; } set { a_isSelected = value; } }
        public bool BIsSelected { get { return b_isSelected; } set { b_isSelected = value; } }
        public bool CIsSelected { get { return c_isSelected; } set { c_isSelected = value; } }
        public bool DIsSelected { get { return d_isSelected; } set { d_isSelected = value; } }
        public bool PrefixSelected { get { return prefixSelected; } set { prefixSelected = value; } }
        public string PrefixText { get { return prefixText; } set { prefixText = value; } }
        public bool IntermediateSelected { get { return intermediateSelected; } set { intermediateSelected = value; } }
        public string IntermediateText { get { return intermediateText; } set { intermediateText = value; } }
        public bool ElevationSelected { get { return elevationSelected; } set { elevationSelected = value; } }
        public bool ABCDSelected { get { return abcdSelected; } set { abcdSelected = value; } }
        public bool SuffixSelected { get { return suffixSelected; } set { suffixSelected = value; } }
        public string SuffixText { get { return suffixText; } set { suffixText = value; } }
        public ViewFamilyType ElevationFamilyType { get { return elevationFamilyType; } set { elevationFamilyType = value; } }
        public ViewPlan ActiveViewPlan { get { return activeViewPlan; } set { activeViewPlan = value; } }
        
        public ElevationCreatorSettings()
        {
        }

        public ElevationCreatorSettings(ElevationCreatorSettings settings)
        {
            isLinkedRoom = settings.IsLinkedRoom;
            isLinkedWall = settings.IsLInkedWall;
            viewTemplateId = settings.ViewTemplateId;
            scaleByTemplate = settings.ScaleByTemplate;
            customScale = settings.CustomScale;
            spaceAround = settings.SpaceAround;
            a_isSelected = settings.AIsSelected;
            b_isSelected = settings.BIsSelected;
            c_isSelected = settings.CIsSelected;
            d_isSelected = settings.DIsSelected;
            prefixSelected = settings.PrefixSelected;
            prefixText = settings.PrefixText;
            intermediateSelected = settings.IntermediateSelected;
            elevationSelected = settings.ElevationSelected;
            abcdSelected = settings.ABCDSelected;
            suffixSelected = settings.SuffixSelected;
            suffixText = settings.SuffixText;
            elevationFamilyType = settings.ElevationFamilyType;
            activeViewPlan = settings.ActiveViewPlan;

        }

    }
}
