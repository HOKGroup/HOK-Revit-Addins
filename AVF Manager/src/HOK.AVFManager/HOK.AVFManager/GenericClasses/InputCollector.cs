using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using HOK.AVFManager.GenericForms;
using System.Xml;
using System.IO;

namespace HOK.AVFManager.GenericClasses
{
    public class InputCollector
    {
        private Document doc;

        #region WindowsForms Components
        private ListView listViewResult;
        private ComboBox comboBoxCategory;
        private ComboBox comboBoxParameter;
        private Label labelSelElements;
        private Label labelDescription;
        private Button bttnPickElement;
        private Button bttnSetReference;
        private RadioButton radioButtonTop;
        private RadioButton radioButtonSide;
        private RadioButton radioButtonBottom;
        private RadioButton radioButtonAll;
        #endregion

        private Dictionary<string/*resultName*/, SettingProperties> defaultSettings = new Dictionary<string, SettingProperties>();
        private Dictionary<string, BuiltInCategory> categoryDictionary = new Dictionary<string, BuiltInCategory>();
        private SettingProperties currentSetting;

        public SettingProperties CurrentSetting { get { return currentSetting; } set { currentSetting = value; } }
        public Dictionary<string, SettingProperties> SettingDictionary { get { return defaultSettings; } set { defaultSettings = value; } }

        public InputCollector(Document document)
        {
            doc = document;
            currentSetting = new SettingProperties();
            AddCategoryDictionary();
            ReadDefaultXML();
        }

        private void AddCategoryDictionary()
        {
            categoryDictionary.Add("Areas", BuiltInCategory.OST_Areas);
            categoryDictionary.Add("Ceilings", BuiltInCategory.OST_Ceilings);
            categoryDictionary.Add("Curtain Panels", BuiltInCategory.OST_CurtainWallPanels);
            categoryDictionary.Add("Curtain Systems", BuiltInCategory.OST_Curtain_Systems);
            categoryDictionary.Add("Floors", BuiltInCategory.OST_Floors);
            categoryDictionary.Add("Generic Models", BuiltInCategory.OST_GenericModel);
            categoryDictionary.Add("Mass", BuiltInCategory.OST_Mass);
            categoryDictionary.Add("Rooms", BuiltInCategory.OST_Rooms);
            categoryDictionary.Add("Spaces", BuiltInCategory.OST_MEPSpaces);
            categoryDictionary.Add("Topography", BuiltInCategory.OST_TopographySurface);
            categoryDictionary.Add("Walls", BuiltInCategory.OST_Walls);
            categoryDictionary.Add("Windows", BuiltInCategory.OST_Windows);
        }

        public void SetComponents(ListView lvResult, ComboBox cbCategory, ComboBox cbParameter,Label labelElements, Button bttnPick, Button bttnSet,
            RadioButton rbTop, RadioButton rbSide, RadioButton rbBottom, RadioButton rbAll, Label labelRefDescription)
        {
            listViewResult = lvResult;
            comboBoxCategory = cbCategory;
            comboBoxParameter = cbParameter;
            labelSelElements = labelElements;
            bttnPickElement = bttnPick;
            bttnSetReference = bttnSet;
            radioButtonTop = rbTop;
            radioButtonSide = rbSide;
            radioButtonBottom = rbBottom;
            radioButtonAll = rbAll;
            labelDescription = labelRefDescription;

            comboBoxCategory.SelectedIndexChanged += new EventHandler(comboBoxCategory_SelectedIndexChanged);
            bttnPickElement.Click += new EventHandler(bttnPickElement_Click);
            bttnSetReference.Click += new EventHandler(bttnSetReference_Click);
        }

        private void ReadDefaultXML()
        {
            try
            {
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                XmlTextReader reader = new XmlTextReader(Path.GetDirectoryName(currentAssembly) + "/Resources/DefaultSettings.xml");
                SettingProperties settingProperties = new SettingProperties();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "AnalysisTool")
                    {
                        settingProperties = new SettingProperties();
                        reader.Read();
                        while (reader.Name != "AnalysisTool")
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.Name)
                                {
                                    case "Name":
                                        settingProperties.ResultName = reader.ReadElementContentAsString();
                                        break;
                                    case "DefaultCategory":
                                        settingProperties.CategoryName = reader.ReadElementContentAsString();
                                        break;
                                    case "Categories":
                                        string strCategories = reader.ReadElementContentAsString();
                                        string[] categoryArray = strCategories.Split(',');
                                        settingProperties.CategoryOptions = categoryArray;
                                        break;
                                    case "Parameter":
                                        settingProperties.ParameterName = reader.ReadElementContentAsString();
                                        break;
                                    case "DisplayingFace":
                                        string displayingFace = reader.ReadElementContentAsString();
                                        switch (displayingFace)
                                        {
                                            case "Top":
                                                settingProperties.DisplayFace = DisplayingFaces.Top; break;
                                            case "Side":
                                                settingProperties.DisplayFace = DisplayingFaces.Side; break;
                                            case "Bottom":
                                                settingProperties.DisplayFace = DisplayingFaces.Bottom; break;
                                            case "All":
                                                settingProperties.DisplayFace = DisplayingFaces.All; break;
                                            default:
                                                settingProperties.DisplayFace = DisplayingFaces.Custom; break;
                                        }
                                        break;
                                    case "SetReference":
                                        settingProperties.SetReference = reader.ReadElementContentAsBoolean();
                                        break;
                                    case "RefCategory":
                                        settingProperties.RefCategory = reader.ReadElementContentAsString();
                                        break;
                                    case "RefDescription":
                                        settingProperties.RefDescription = reader.ReadElementContentAsString();
                                        break;
                                    case "Units":
                                        settingProperties.Units = reader.ReadElementContentAsString();
                                        break;
                                }
                            }
                            reader.Read();
                        }
                        if (!defaultSettings.ContainsKey(settingProperties.ResultName)) { defaultSettings.Add(settingProperties.ResultName, settingProperties); }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read DefaultSettings.xml \n" + ex.Message, "InputCollector:ReadDefaultXML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }

        public SettingProperties DisplayDefaultSettings(string analysisName)
        {
            if (defaultSettings.ContainsKey(analysisName))
            {
                currentSetting = defaultSettings[analysisName];
                DisplayUserSettings(currentSetting);
            }
            return currentSetting;
        }

        public void DisplayUserSettings(SettingProperties settings)
        {
            try
            {
                currentSetting = settings;
                List<Element> selectedElements = new List<Element>();
                if (null != currentSetting.SelectedElements)
                {
                    selectedElements = currentSetting.SelectedElements;
                }

                comboBoxCategory.Items.Clear();
                comboBoxParameter.Items.Clear();
                comboBoxCategory.Text = "";
                comboBoxParameter.Text = "";

                radioButtonTop.Checked = false;
                radioButtonSide.Checked = false;
                radioButtonBottom.Checked = false;
                radioButtonAll.Checked = false;
                bttnPickElement.Enabled = false;
                bttnSetReference.Enabled = false;
                labelSelElements.Text = "";

                foreach (string category in settings.CategoryOptions)
                {
                    comboBoxCategory.Items.Add(category);
                }
                for (int i = 0; i < comboBoxCategory.Items.Count; i++)
                {
                    if (comboBoxCategory.Items[i].ToString() == settings.CategoryName)
                    {
                        comboBoxCategory.SelectedIndex = i;
                    }
                }

                if (selectedElements.Count > 0)
                {
                    labelSelElements.Text = selectedElements.Count + " Elements Selected";
                    currentSetting.SelectedElements = selectedElements;
                }

                bttnSetReference.Enabled = settings.SetReference;
                labelDescription.Text = settings.RefDescription;

                switch (settings.DisplayFace)
                {
                    case DisplayingFaces.Top:
                        radioButtonTop.Checked = true;
                        break;
                    case DisplayingFaces.Side:
                        radioButtonSide.Checked = true;
                        break;
                    case DisplayingFaces.Bottom:
                        radioButtonBottom.Checked = true;
                        break;
                    case DisplayingFaces.All:
                        radioButtonAll.Checked = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display users' settings.\n" + ex.Message, "InputCollector:DisplayUserSettings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetCurrentSettings(SettingProperties settings)
        {
            if (defaultSettings.ContainsKey(settings.ResultName))
            {
                defaultSettings.Remove(settings.ResultName);
            }
            defaultSettings.Add(settings.ResultName, settings);
            currentSetting = settings;
        }

        private void comboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (null != comboBoxCategory.SelectedItem)
            {
                DisplayParametersList();
            }
        }

        private void DisplayParametersList()
        {
            try
            {
                string selCategory = comboBoxCategory.SelectedItem.ToString();
                currentSetting.CategoryName = selCategory;
                comboBoxParameter.Items.Clear();
                comboBoxParameter.Text = "";
                bttnPickElement.Enabled = false;

                if (categoryDictionary.ContainsKey(selCategory))
                {
                    BuiltInCategory bitCategory = categoryDictionary[selCategory];

                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    ElementCategoryFilter catFilter = new ElementCategoryFilter(bitCategory);
                    List<Element> elements = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().ToList();
                    List<string> parameterList = new List<string>();

                    string elemName = "";
                    foreach (Element element in elements)
                    {
                        if (null != element.Name)
                        {
                            if (string.Compare(element.Name, elemName) == 0) { continue; }
                        }
                        
                        foreach (Parameter parameter in element.Parameters)
                        {
                            if (!parameterList.Contains(parameter.Definition.Name))
                            {
                                if (parameter.StorageType == StorageType.Double)
                                {
                                    parameterList.Add(parameter.Definition.Name);
                                    comboBoxParameter.Items.Add(parameter.Definition.Name);
                                }
                            }
                        }

                        if (element.Category.Name == "Areas" || element.Category.Name == "Rooms") { break; }
                        
                        elemName = element.Name;
                    }
                    currentSetting.ParameterList = parameterList;

                    for (int i = 0; i < comboBoxParameter.Items.Count; i++)
                    {
                        if (comboBoxParameter.Items[i].ToString() == currentSetting.ParameterName)
                        {
                            comboBoxParameter.SelectedIndex = i;
                        }
                    }
                    if (comboBoxParameter.SelectedIndex < 0)
                    {
                        comboBoxParameter.SelectedIndex = 0;
                    }

                    labelSelElements.Text = elements.Count + " Elements Selected";
                    currentSetting.SelectedElements = elements;
                    if (elements.Count > 0)
                    {
                        bttnPickElement.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display parameter lists.\n" + ex.Message, "InputCollector : DisplayParametersList", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnPickElement_Click(object sender, EventArgs e)
        {
        }

        private void bttnSetReference_Click(object sender, EventArgs e)
        {
        }
    }
}
