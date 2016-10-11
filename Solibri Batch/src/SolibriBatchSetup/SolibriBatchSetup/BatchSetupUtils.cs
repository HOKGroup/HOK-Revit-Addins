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
using System.ComponentModel;

namespace SolibriBatchSetup
{
    public static class BatchSetupUtils
    {
        private static string[] modelExtensions = new string[] { ".ifc", ".zip", ".dwg", ".ifczip" };

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
                ObservableCollection<GenericElement> elements = batch.Target.Elements;
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
                            else if (modelExtensions.Contains(openModel.FileExtension))
                            {
                                InputModel modeltoOpen = new InputModel(openModel);
                                unit.Models.Add(modeltoOpen);
                            }
                            break;
                        case "UpdateModel":
                            UpdateModel updateModel = element as UpdateModel;
                            InputModel modeltoUpdate = new InputModel(updateModel);
                            unit.Models.Add(modeltoUpdate);
                            break;
                        case "OpenRuleset":
                            OpenRuleset ruleset = element as OpenRuleset;
                            unit.Rulesets.Add(ruleset);
                            break;
                        case "OpenClassification":
                            OpenClassification classification = element as OpenClassification;
                            unit.Classifications.Add(classification);
                            break;
                        case "Check":
                            Check check = element as Check;
                            check.IsSpecified = true;
                            unit.CheckTask = check;
                            break;
                        case "AutoComment":
                            AutoComment comment = element as AutoComment;
                            comment.IsSpecified = true;
                            unit.CommentTask = comment;
                            break;
                        case "WriterReport":
                            unit.CheckingReport = element as WriterReport;
                            break;
                        case "CreatePresentation":
                            CreatePresentation createP = element as CreatePresentation;
                            createP.IsSelected = true;
                            unit.PresentationCreate = createP;
                            break;
                        case "UpdatePresentation":
                            UpdatePresentation updateP = element as UpdatePresentation;
                            updateP.IsSelected = true;
                            unit.PresentationUpdate = updateP;
                            break;
                        case "GeneralReport":
                            unit.PresentationReport = element as GeneralReport;
                            break;
                        case "BCFReport":
                            unit.BCFReport = element as BCFReport;
                            break;
                        case "CoordinationReport":
                            unit.CoordReport = element as CoordinationReport;
                            break;
                        case "SaveModel":
                            unit.SaveSolibri = element as SaveModel;
                            unit.TaskName = System.IO.Path.GetFileNameWithoutExtension(unit.SaveSolibri.File);
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

        public static ObservableCollection<GenericElement> ConvertToGenericElements(ObservableCollection<ProcessUnit> processUnits)
        {
            ObservableCollection<GenericElement> elements = new ObservableCollection<GenericElement>();
            try
            {
                foreach (ProcessUnit unit in processUnits)
                {
                    //open models
                    if (!string.IsNullOrEmpty(unit.OpenSolibri.File))
                    {
                        elements.Add(unit.OpenSolibri);
                    }
                    if (unit.Models.Count > 0)
                    {
                        foreach (InputModel model in unit.Models)
                        {
                            if (model.IsUpdate)
                            {
                                UpdateModel updateModel = new UpdateModel(model);
                                elements.Add(updateModel);
                            }
                            else
                            {
                                OpenModel openModel = new OpenModel(model);
                                elements.Add(openModel);
                            }
                        }
                    }

                    if (unit.Classifications.Count > 0)
                    {
                        foreach (OpenClassification classification in unit.Classifications)
                        {
                            elements.Add(classification);
                        }
                    }

                    if (unit.Rulesets.Count > 0)
                    {
                        foreach (OpenRuleset rule in unit.Rulesets)
                        {
                            elements.Add(rule);
                        }
                    }

                    if (unit.CheckEnabled)
                    {
                        elements.Add(unit.CheckTask);

                        if (!string.IsNullOrEmpty(unit.CheckingReport.PdfFile) || !string.IsNullOrEmpty(unit.CheckingReport.RtfFile))
                        {
                            elements.Add(unit.CheckingReport);
                        }

                        if (unit.PresentationCreate.IsSelected)
                        {
                            elements.Add(unit.CommentTask);
                            elements.Add(unit.PresentationCreate);
                        }

                        if (unit.PresentationUpdate.IsSelected)
                        {
                            elements.Add(unit.CommentTask);
                            elements.Add(unit.PresentationUpdate);
                        }

                        if (!string.IsNullOrEmpty(unit.PresentationReport.PdfFile) || !string.IsNullOrEmpty(unit.PresentationReport.RtfFile))
                        {
                            elements.Add(unit.PresentationReport);
                        }

                        if (!string.IsNullOrEmpty(unit.BCFReport.File))
                        {
                            elements.Add(unit.BCFReport);
                        }

                        if (!string.IsNullOrEmpty(unit.CoordReport.File))
                        {
                            elements.Add(unit.CoordReport);
                        }
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

        private static void DetermineCheckEnabled(ProcessUnit unit, out bool checkEnabled, out bool commentEnabled)
        {
            checkEnabled = false;
            commentEnabled = false;
            try
            {
                if (unit.PresentationCreate.IsSelected || unit.PresentationUpdate.IsSelected)
                {
                    checkEnabled = true;
                    commentEnabled = true;
                    return;
                }

                if (!string.IsNullOrEmpty(unit.CheckingReport.PdfFile) || !string.IsNullOrEmpty(unit.CheckingReport.RtfFile))
                {
                    checkEnabled = true;
                }
                if (!string.IsNullOrEmpty(unit.PresentationReport.PdfFile)|| !string.IsNullOrEmpty(unit.PresentationReport.RtfFile) || !string.IsNullOrEmpty(unit.BCFReport.File) || !string.IsNullOrEmpty(unit.CoordReport.File))
                {
                    checkEnabled = true;
                    commentEnabled = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static bool SaveBatchConfig(Batch batch)
        {
            bool saved = false;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Batch));
                using (FileStream fs = new FileStream(batch.FilePath, FileMode.Create))
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
                if (File.Exists(batch.FilePath))
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

    

}
