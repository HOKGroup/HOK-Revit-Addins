using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string configFile = "";
        private batch batchConfig = new batch();
        private AutorunSettings settings = new AutorunSettings();

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Solibri Batch Configuration v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private bool ReadBatchConfig(string fileName, out batch batchFromFile)
        {
            bool validated = false;
            batchFromFile = new batch();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(batch));
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    XmlReader reader = XmlReader.Create(fs);

                    if (serializer.CanDeserialize(reader))
                    {
                        batchFromFile = (batch)serializer.Deserialize(reader);
                        validated = true;
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read batch configuration file.\n"+ex.Message, "Read Batch Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return validated;
        }

        private bool DisplayBatchConfig(batch batchSetting)
        {
            bool displayed = false;
            try
            {
                textBoxConfig.Text = configFile;

                List<model> ifcFiles = batchSetting.target.openmodel.OrderBy(o => o.file).ToList();
                dataGridIFC.ItemsSource = null;
                dataGridIFC.ItemsSource = ifcFiles;

                List<model> rulesetFiles = batchSetting.target.openruleset.OrderBy(o => o.file).ToList();
                dataGridRuleset.ItemsSource = null;
                dataGridRuleset.ItemsSource = rulesetFiles;

                textBoxBCF.Text = "";
                if (batchSetting.target.bcfreport.Count > 0)
                {
                    textBoxBCF.Text = batchSetting.target.bcfreport.First().file;
                }

                textBoxSolibri.Text = "";
                if (batchSetting.target.savemodel.Count > 0)
                {
                    textBoxSolibri.Text = batchSetting.target.savemodel.First().file;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display batch settings.\n" + ex.Message, "Display Batch Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return displayed;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    bool fileAdded = false;
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    string xmlFile = "";
                    List<string> ifcFiles = new List<string>();
                    List<string> rulesetFiles = new List<string>();
                    foreach (string file in files)
                    {
                        if (System.IO.Path.GetExtension(file).Contains("xml"))
                        {
                            xmlFile = file; break;
                        }
                        if (System.IO.Path.GetExtension(file).Contains("ifc"))
                        {
                            ifcFiles.Add(file);
                        }
                        if (System.IO.Path.GetExtension(file).Contains("cset"))
                        {
                            rulesetFiles.Add(file);
                        }
                    }
                    if (!string.IsNullOrEmpty(xmlFile))
                    {
                        batch batchFromFile = new batch();
                        if (ReadBatchConfig(xmlFile, out batchFromFile))
                        {
                            batchConfig = batchFromFile;
                            configFile = xmlFile;
                            fileAdded = true;
                        }
                    }
                    if (ifcFiles.Count > 0)
                    {
                        foreach (string fileItem in ifcFiles)
                        {
                            var itemFound = from ifcFile in batchConfig.target.openmodel where ifcFile.file == fileItem select ifcFile;
                            if (File.Exists(fileItem) && itemFound.Count() == 0)
                            {
                                model modelItem = new model();
                                modelItem.file = fileItem;

                                batchConfig.target.openmodel.Add(modelItem);
                                fileAdded = true;
                            }
                        }
                    }
                    if (rulesetFiles.Count > 0)
                    {
                        foreach (string fileItem in rulesetFiles)
                        {
                            var itemFound = from rule in batchConfig.target.openruleset where rule.file == fileItem select rule;
                            if (File.Exists(fileItem) && itemFound.Count() == 0)
                            {
                                FileInfo fi = new FileInfo(fileItem);
                                model modelItem = new model();
                                modelItem.file = fileItem;

                                batchConfig.target.openruleset.Add(modelItem);
                                fileAdded = true;
                            }
                        }
                    }
                    if (fileAdded)
                    {
                        DisplayBatchConfig(batchConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to drop files.\n"+ex.Message, "Drop Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "xml files (*.xml)|*.xml";
                openFileDialog.RestoreDirectory = false;
                if (settings.RunRemote && Directory.Exists(settings.RemoteDirectory)) { openFileDialog.InitialDirectory = settings.RemoteDirectory; }
                openFileDialog.Multiselect = false;
                openFileDialog.Title = "Open Solibri Batch Configuration File";

                if (openFileDialog.ShowDialog() == true)
                {
                    string xmlFile = openFileDialog.FileName;
                    batch batchFromFile = new batch();
                    if (ReadBatchConfig(xmlFile, out batchFromFile))
                    {
                        batchConfig = batchFromFile;
                        configFile = xmlFile;
                        DisplayBatchConfig(batchConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open the configuration file.\n"+ex.Message, "Open Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsValidateConfig(batchConfig))
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "xml files (*.xml)|*.xml";
                    saveFileDialog.RestoreDirectory = false;
                    if (settings.RunRemote && Directory.Exists(settings.RemoteDirectory)) { saveFileDialog.InitialDirectory = settings.RemoteDirectory; }
                    saveFileDialog.Title = "Save Solibri Batch Configuration File";
                    if (!string.IsNullOrEmpty(configFile))
                    {
                        saveFileDialog.FileName = System.IO.Path.GetFileName(configFile);
                    }

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string xmlFile = saveFileDialog.FileName;
                        if (settings.RunRemote)
                        {
                            if (System.IO.Path.GetDirectoryName(xmlFile) != settings.RemoteDirectory)
                            {
                                MessageBox.Show("Please save the configuration file in the assigned directory in the remote machine.", "Invalid File Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                        }
                        XmlSerializer serializer = new XmlSerializer(typeof(batch));
                        using (FileStream fs = new FileStream(xmlFile, FileMode.Create))
                        {
                            XmlWriter writer = XmlWriter.Create(fs);
                            serializer.Serialize(writer, batchConfig);
                            fs.Close();
                        }
                        if (File.Exists(xmlFile))
                        {
                            configFile = xmlFile;
                            textBoxConfig.Text = configFile;
                            bool batchCreated = CreateBatchFile(xmlFile);
                            if (batchCreated)
                            {
                                MessageBoxResult msgResult = MessageBox.Show("Batch configuration files are successfully saved in " + xmlFile+"\nWould you like to close this application?", "Batch Configuration Saved", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (msgResult == MessageBoxResult.Yes)
                                {
                                    this.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the configuration file.\n" + ex.Message, "Save Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool IsValidateConfig(batch batchSetting)
        {
            bool validated = true;
            try
            {
                if (batchSetting.target.openmodel.Count == 0)
                {
                    MessageBox.Show("Please select IFC files to be opened in Solibri.", "IFC Files Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                if (batchConfig.target.savemodel.Count == 0)
                {
                    MessageBox.Show("Please assign a file path for the Solibri file to be saved.", "Solibri File Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                if (batchSetting.target.openruleset.Count > 0 && batchSetting.target.bcfreport.Count > 0)
                {
                    //auto check
                    if (batchConfig.target.check.Count == 0)
                    {
                        batchConfig.target.check.Add(new object());
                    }
                    if (batchConfig.target.autocomment.Count == 0)
                    {
                        batchConfig.target.autocomment.Add(new autocomment());
                    }
                    if (batchConfig.target.createpresentation.Count == 0)
                    {
                        batchConfig.target.createpresentation.Add(new object());
                    }
                }

                if (batchConfig.target.closemodel.Count == 0)
                {
                    batchConfig.target.closemodel.Add(new object());
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to validate the configuration setting.\n" + ex.Message, "Validate Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return validated;
        }

        private bool CreateBatchFile(string xmlFile)
        {
            bool created = false;
            try
            {
                string batchFile = System.IO.Path.GetDirectoryName(xmlFile) + "\\" + System.IO.Path.GetFileName(xmlFile).Replace(".xml", ".bat");
                using (StreamWriter writer = File.CreateText(batchFile))
                {
                    writer.WriteLineAsync("echo %TIME%");
                    writer.WriteLineAsync("\""+settings.ExeFile+"\" \"" + xmlFile + "\"");
                    writer.WriteLineAsync("echo %TIME%");
                    writer.WriteLineAsync("pause");
                    writer.Close();
                }
                if (File.Exists(batchFile))
                {
                    created = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a batch file.\n"+ex.Message, "Create a Batch File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonAddIFC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "IFC files (*.ifc)|*.ifc";
                openFileDialog.RestoreDirectory = false;
                openFileDialog.Title = "Open IFC Files";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        var itemFound = from ifcFile in batchConfig.target.openmodel where ifcFile.file == fileName select ifcFile;
                        if (File.Exists(fileName) && itemFound.Count() == 0)
                        {
                            model modelItem = new model();
                            modelItem.file = fileName;
                            batchConfig.target.openmodel.Add(modelItem);
                        }
                    }

                    List<model> ifcFiles = batchConfig.target.openmodel.OrderBy(o => o.file).ToList();
                    dataGridIFC.ItemsSource = null;
                    dataGridIFC.ItemsSource = ifcFiles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add IFC files into the list.\n" + ex.Message, "Add IFC Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDeleteIFC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridIFC.SelectedItem)
                {
                    foreach (model model in dataGridIFC.SelectedItems)
                    {
                        batchConfig.target.openmodel.Remove(model);
                    }

                    List<model> ifcFiles = batchConfig.target.openmodel.OrderBy(o => o.file).ToList();
                    dataGridIFC.ItemsSource = null;
                    dataGridIFC.ItemsSource = ifcFiles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete IFC files from the list.\n" + ex.Message, "Delete IFC Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Ruleset Files (*.cset)|*.cset";
                openFileDialog.RestoreDirectory = false;
                openFileDialog.Title = "Open Ruleset Files";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        var itemFound = from ruleset in batchConfig.target.openruleset where ruleset.file == fileName select ruleset;
                        if (File.Exists(fileName) && itemFound.Count() == 0)
                        {
                            model modelItem = new model();
                            modelItem.file = fileName;
                            batchConfig.target.openruleset.Add(modelItem);
                        }
                    }

                    List<model> rulesetFiles = batchConfig.target.openruleset.OrderBy(o => o.file).ToList();
                    dataGridRuleset.ItemsSource = null;
                    dataGridRuleset.ItemsSource = rulesetFiles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add Rulesets files into the list.\n" + ex.Message, "Add Rulesets Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDeleteRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridRuleset.SelectedItem)
                {
                    foreach (model model in dataGridRuleset.SelectedItems)
                    {
                        batchConfig.target.openruleset.Remove(model);
                    }

                    List<model> rulesetFiles = batchConfig.target.openruleset.OrderBy(o => o.file).ToList();
                    dataGridRuleset.ItemsSource = null;
                    dataGridRuleset.ItemsSource = rulesetFiles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete Rulesets files.\n" + ex.Message, "Dlete Rulesets Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonSaveBCF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "BCFZIP files (*.bcfzip)|*.bcfzip";
                saveFileDialog.RestoreDirectory = false;
                saveFileDialog.Title = "Save BCFZIP File";
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    bcf bcfItem = new bcf();
                    bcfItem.file = saveFileDialog.FileName;
                    bcfItem.version = "2";
                    batchConfig.target.bcfreport = new List<bcf>();
                    batchConfig.target.bcfreport.Add(bcfItem);

                    textBoxBCF.Text = bcfItem.file;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save BCF Files.\n" + ex.Message, "Save BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonSaveSolibri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Solibri Model Checker Files (*.smc)|*.smc";
                saveFileDialog.RestoreDirectory = false;
                saveFileDialog.Title = "Save Solibri File";

                if (saveFileDialog.ShowDialog() == true)
                {
                    model modelItem = new model();
                    modelItem.file = saveFileDialog.FileName;

                    batchConfig.target.savemodel = new List<model>();
                    batchConfig.target.savemodel.Add(modelItem);

                    textBoxSolibri.Text = modelItem.file;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Solibri Files.\n" + ex.Message, "Save Solibri", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingWindow settingWindow = new SettingWindow(settings);
                settingWindow.Owner = this;
                if (settingWindow.ShowDialog() == true)
                {
                    settings = settingWindow.Settings;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to oepn settings window.\n" + ex.Message, "Open Settings Window", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                configFile = "";
                batchConfig = new batch();
                settings = new AutorunSettings();
                DisplayBatchConfig(batchConfig);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set default properties.\n" + ex.Message, "Set Default", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

      
    }
}
