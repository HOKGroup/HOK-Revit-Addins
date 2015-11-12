using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using SolibriBatchSetup.Schema;

namespace SolibriBatchSetup
{
    public static class BatchSetupUtils
    {
        public static bool ReadBatchConfig(string fileName, out Batch batchFromFile)
        {
            bool validated = false;
            batchFromFile = new Batch(fileName);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Batch));
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    XmlReader reader = XmlReader.Create(fs);

                    if (serializer.CanDeserialize(reader))
                    {
                        batchFromFile = (Batch)serializer.Deserialize(reader);
                        batchFromFile.FilePath = fileName;
                        validated = true;
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read batch configuration file.\n" + ex.Message, "Read Batch Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return validated;
        }

        public static ObservableCollection<ProcessUnit> ExtractProcessUnits(Batch batch)
        {
            ObservableCollection<ProcessUnit> processUnits = new ObservableCollection<ProcessUnit>();
            try
            {
                ProcessUnit unit = new ProcessUnit();
                List<GenericElement> elements = batch.Target.Elements;
                for (int i = 0; i < elements.Count; i++)
                {
                    GenericElement element = elements[i];
                    string elementType = element.GetType().Name;
                    switch (elementType)
                    {
                        case "OpenModel":
                            OpenModel openModel = element as OpenModel;
                            if (openModel.FileExtension == ".smc")
                            {
                                unit.OpenSolibri = openModel;
                            }
                            else if (openModel.FileExtension == ".ifc")
                            {
                                unit.IfcFiles.Add(openModel);
                            }
                            break;
                        case "BCFReport":
                            unit.BCFReport = element as BCFReport;
                            break;
                        case "SaveModel":
                            unit.SaveSolibri = element as SaveModel;
                            if (unit.IfcFiles.Count > 0)
                            {
                                unit.OpenSolibri = new OpenModel(unit.SaveSolibri.File);
                            }
                            break;
                        case "CloseModel":
                            processUnits.Add(unit);
                            unit = new ProcessUnit();
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to extract process units.\n" + ex.Message, "Extract Process Unit", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return processUnits;
        }

        public static ObservableCollection<OpenRuleset> ExtractRulesets(Batch batch)
        {
            ObservableCollection<OpenRuleset> rulesets = new ObservableCollection<OpenRuleset>();
            try
            {
                var elements = from element in batch.Target.Elements where element.GetType().Name == "OpenRuleset" select element;
                List<OpenRuleset> elementList = elements.Cast<OpenRuleset>().ToList();
                List<string> fileNames = new List<string>();
                foreach (OpenRuleset ruleset in elementList)
                {
                    if (!fileNames.Contains(ruleset.File))
                    {
                        fileNames.Add(ruleset.File);
                        rulesets.Add(ruleset);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to extract rulesets.\n" + ex.Message, "Extract Rulesets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return rulesets;
        }

        public static List<GenericElement> ConvertToGenericElements(ObservableCollection<ProcessUnit> processUnits, ObservableCollection<OpenRuleset> rulesets)
        {
            List<GenericElement> elements = new List<GenericElement>();
            try
            {
                bool runCheck = (rulesets.Count > 0) ? true : false;
                foreach (ProcessUnit unit in processUnits)
                {
                    //open models
                    if (unit.IfcFiles.Count > 0)
                    {
                        elements.AddRange(unit.IfcFiles);
                    }
                    else
                    {
                        elements.Add(unit.OpenSolibri);
                    }

                    if (rulesets.Count > 0)
                    {
                        elements.AddRange(rulesets);
                        elements.Add(new Check());
                        elements.Add(new AutoComment());
                        elements.Add(new CreatePresentation());
                        elements.Add(unit.BCFReport);
                    }

                    elements.Add(unit.SaveSolibri);
                    elements.Add(new CloseModel());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to convert to generic elements.\n" + ex.Message, "Convert To Generic Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return elements;
        }

        public static bool SaveBatchConfig(string fileName, Batch batch)
        {
            bool saved = false;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Batch));
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "\t",
                        NewLineChars = Environment.NewLine,
                        NewLineHandling = NewLineHandling.Replace,
                        Encoding = new UTF8Encoding(false)
                    };

                    XmlWriter writer = XmlWriter.Create(fs, settings);
                    serializer.Serialize(writer, batch);
                    fs.Close();
                }
                if (File.Exists(fileName))
                {
                    saved = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save batch configuration.\n" + ex.Message, "Save Batch Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        public static bool CreateBatchFile(string xmlFileName, AutorunSettings settings)
        {
            bool created = false;
            try
            {
                string batchFile = System.IO.Path.GetDirectoryName(xmlFileName) + "\\" + System.IO.Path.GetFileName(xmlFileName).Replace(".xml", ".bat");
                using (StreamWriter writer = File.CreateText(batchFile))
                {
                    writer.WriteLineAsync("echo %TIME%");
                    writer.WriteLineAsync("\"" + settings.SolibriSetup.ExeFile + "\" \"" + xmlFileName + "\"");
                    writer.WriteLineAsync("echo %TIME%");
                    //writer.WriteLineAsync("pause");
                    writer.Close();
                }
                if (File.Exists(batchFile))
                {
                    created = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create batch file.\n" + ex.Message, "Save Batch File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }
    }

    public class ProcessUnit
    {
        private Guid unitId = Guid.Empty;
        private OpenModel openSolibri = new OpenModel();
        private List<OpenModel> ifcFiles = new List<OpenModel>();
        private BCFReport bcfReport = new BCFReport();
        private SaveModel saveSolibri = new SaveModel();

        public Guid UnitId { get { return unitId; } set { unitId = value; } }
        public OpenModel OpenSolibri { get { return openSolibri; } set { openSolibri = value; } }
        public List<OpenModel> IfcFiles { get { return ifcFiles; } set { ifcFiles = value; } }
        public BCFReport BCFReport { get { return bcfReport; } set { bcfReport = value; } }
        public SaveModel SaveSolibri { get { return saveSolibri; } set { saveSolibri = value; } }
        

        public ProcessUnit()
        {
        }
    }

}
