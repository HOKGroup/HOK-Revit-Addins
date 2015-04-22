using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.DoorMonitor
{
    public static class ProjectSetupDataStroageUtil
    {
        public static Guid schemaId = new Guid("534060A0-6A4A-4C08-8B7D-35755CF7E8CD");
        public static Schema m_schema = Schema.Lookup(schemaId);

        private static string s_IsMonitorOn = "IsMonitorOn";
        private static string s_IsStateCA = "IsStateCA";
        private static string s_ProjectState = "ProjectState";

        public static MonitorProjectSetup GetProjectSetup(Document doc)
        {
            MonitorProjectSetup projectSetup = new MonitorProjectSetup();
            try
            {
                if (null == m_schema)
                {
                    m_schema = CreateSchema();
                }
                if (null != m_schema)
                {
                    IList<DataStorage> savedStorage = GetProjectSetupStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        DataStorage storage = savedStorage.First();
                        Entity entity = storage.GetEntity(m_schema);

                        projectSetup.IsMonitorOn = entity.Get<bool>(m_schema.GetField(s_IsMonitorOn));
                        projectSetup.ProjectState = entity.Get<string>(m_schema.GetField(s_ProjectState));
                        projectSetup.IsStateCA = entity.Get<bool>(m_schema.GetField(s_IsStateCA));
                    }
                }
            }
            catch (Exception ex)
            {
                string message ="Cannot get Project Setup Information:"+ ex.Message;
            }
            return projectSetup;
        }

        public static bool StoreProjectSetup(Document doc, MonitorProjectSetup projectSetup)
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
                    IList<DataStorage> savedStorage = GetProjectSetupStorage(doc, m_schema);
                    if (savedStorage.Count > 0)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Store Project Setup");
                            try
                            {
                                foreach (DataStorage storage in savedStorage)
                                {
                                    doc.Delete(storage.Id);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                                trans.RollBack();
                            }
                        }
                    }

                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Store Project Setup");
                        try
                        {
                            DataStorage dataStorage = DataStorage.Create(doc);
                            Entity entity = new Entity(schemaId);
                            entity.Set<bool>(s_IsMonitorOn, projectSetup.IsMonitorOn);
                            entity.Set<string>(s_ProjectState, projectSetup.ProjectState);
                            entity.Set<bool>(s_IsStateCA, projectSetup.IsStateCA);
                            dataStorage.SetEntity(entity);
                            trans.Commit();
                            saved = true;
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                            trans.RollBack();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                string message = "Cannot store project setup." + ex.Message;
            }
            return saved;
        }

        private static Schema CreateSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                schemaBuilder.SetSchemaName("DoorMonitor");
                schemaBuilder.AddSimpleField(s_IsMonitorOn, typeof(bool));
                schemaBuilder.AddSimpleField(s_ProjectState, typeof(string));
                schemaBuilder.AddSimpleField(s_IsStateCA, typeof(bool));
                schema = schemaBuilder.Finish();

            }
            catch (Exception ex)
            {
                string message = "Cannot Create Schema: " + ex.Message;
            }
            return schema;
        }

        private static IList<DataStorage> GetProjectSetupStorage(Document document, Schema schema)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(DataStorage));
            Func<DataStorage, bool> hasTargetData = ds => (ds.GetEntity(schema) != null && ds.GetEntity(schema).IsValid());

            return collector.Cast<DataStorage>().Where<DataStorage>(hasTargetData).ToList<DataStorage>();
        }

    }

    public class MonitorProjectSetup
    {
        private bool isMonitorOn = false;
        private bool isCA = false;
        private string projectState = "";

        public bool IsMonitorOn { get { return isMonitorOn; } set { isMonitorOn = value; } }
        public bool IsStateCA { get { return isCA; } set { isCA = value; } }
        public string ProjectState { get { return projectState; } set { projectState = value; } }

        public MonitorProjectSetup()
        {

        }
    }
}
