using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.ViewAnalysis
{
    public static class DataStorageUtil
    {
        public static Guid analysisSchemaId = new Guid("C08965D2-5750-4CE1-8F69-979D0DF0360D");
        public static Schema analysisSchema = Schema.Lookup(analysisSchemaId);

        private static string s_dataFileName = "DataFileName";
        private static string s_overwriteData = "OverwriteData";
        private static string s_dataResolution = "DataResolution";


        public static AnalysisSettings ReadAnalysisSettings(Document doc)
        {
            AnalysisSettings settings = new AnalysisSettings();
            try
            {
                if (null == analysisSchema)
                {
                    analysisSchema = CreateAnalysisSchema();
                }

                if (null != analysisSchema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, analysisSchema);
                    if (savedStorage.Count > 0)
                    {
                        DataStorage storage = savedStorage.First();
                        Entity entity = storage.GetEntity(analysisSchema);
                        string dataFileName = entity.Get<string>(analysisSchema.GetField(s_dataFileName));
                        bool overwrite = entity.Get<bool>(analysisSchema.GetField(s_overwriteData));
                        string dataResolution = entity.Get<string>(analysisSchema.GetField(s_dataResolution));

                        settings.DataFileName = dataFileName;
                        settings.OverwriteData = overwrite;
                        settings.Resolution = (DataResolution)Enum.Parse(typeof(DataResolution), dataResolution);
                        switch (settings.Resolution)
                        {
                            case DataResolution.High:
                                settings.Interval = 0.5;
                                break;
                            case DataResolution.Medium:
                                settings.Interval = 1;
                                break;
                            case DataResolution.Low:
                                settings.Interval = 2;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read settings for View Analysis.\n" + ex.Message, "Read Settings for View Analysis", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return settings;
        }

        public static bool UpdateAnalysisSettings(Document doc, AnalysisSettings settings)
        {
            bool saved = false;
            try
            {
                if (null == analysisSchema)
                {
                    analysisSchema = CreateAnalysisSchema();
                }

                if (null != analysisSchema)
                {
                    IList<DataStorage> savedStorage = GetDataStorage(doc, analysisSchema);
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
                            Entity entity = new Entity(analysisSchemaId);
                            entity.Set<string>(s_dataFileName, settings.DataFileName);
                            entity.Set<bool>(s_overwriteData, settings.OverwriteData);
                            entity.Set<string>(s_dataResolution, settings.Resolution.ToString());
                            storage.SetEntity(entity);
                            trans.Commit();

                            saved = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to add new data storage.\n" + ex.Message, "Update Data Storage", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update settings for View Analysis.\n" + ex.Message, "Update Settings for View Analysis", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
            
        }
      

        public static Schema CreateAnalysisSchema()
        {
            Schema schema = null;
            try
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(analysisSchemaId);
                schemaBuilder.SetSchemaName("ViewAnalysisSchema");
                schemaBuilder.AddSimpleField(s_dataFileName, typeof(string));
                schemaBuilder.AddSimpleField(s_overwriteData, typeof(bool));
                schemaBuilder.AddSimpleField(s_dataResolution, typeof(string));
                schema = schemaBuilder.Finish();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create BCF Color schema.\n" + ex.Message, "Create Schema", MessageBoxButton.OK, MessageBoxImage.Warning);
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
}
