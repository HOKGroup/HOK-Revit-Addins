using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.Analysis;


namespace HOK.AVFManager.GenericClasses
{
    public class DisplayStyleManager
    {
        private Document doc;
        private SettingProperties currentSetting=new SettingProperties();

        private ComboBox comboBoxStyle;
        private Label labelStyleType;
        private ComboBox comboBoxUnits;

        private Dictionary<string/*analysisName*/, string/*analysisType*/> styleDictionary = new Dictionary<string, string>();

        public SettingProperties CurrentSetting { get { return currentSetting; } set { currentSetting = value; } }

        public DisplayStyleManager(Document document)
        {
            doc = document;
        }

        public DisplayStyleManager(Document document, SettingProperties settingProperties)
        {
            doc = document;
            currentSetting = settingProperties;
        }

        public void SetComponents(ComboBox cbStyle, Label lbType, ComboBox cbUnits)
        {
            comboBoxStyle = cbStyle;
            labelStyleType = lbType;
            comboBoxUnits = cbUnits;

            comboBoxStyle.SelectedIndexChanged += new EventHandler(comboBoxStyle_SelectedIndexChanged);
        }

        public bool DisplayDefaultSettings()
        {
            bool result = false;
            try
            {
                comboBoxStyle.Items.Clear();
                comboBoxUnits.Items.Clear();

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> displayStyles = collector.OfClass(typeof(AnalysisDisplayStyle)).ToElements().ToList();

                if (displayStyles.Count > 0)
                {
                    foreach (Element element in displayStyles)
                    {
                        AnalysisDisplayStyle style = element as AnalysisDisplayStyle;

                        AnalysisDisplayColoredSurfaceSettings coloredSurface;
                        try { coloredSurface = style.GetColoredSurfaceSettings(); }
                        catch { coloredSurface = null; }

                        AnalysisDisplayMarkersAndTextSettings markersText;
                        try { markersText = style.GetMarkersAndTextSettings(); }
                        catch { markersText = null; }

                        if (null != coloredSurface) { styleDictionary.Add(style.Name, "Colored Surface"); }
                        else if (null != markersText) { styleDictionary.Add(style.Name, "Markers And Text"); }

                        comboBoxStyle.Items.Add(style.Name);
                    }

                    if (comboBoxStyle.Items.Count > 0)
                    {
                        if (null != currentSetting.DisplayStyle)
                        {
                            for (int i = 0; i < comboBoxStyle.Items.Count; i++)
                            {
                                if (comboBoxStyle.Items[i].ToString() == currentSetting.DisplayStyle)
                                {
                                    comboBoxStyle.SelectedIndex = i;
                                }
                            }
                        }
                        else
                        {
                            comboBoxStyle.SelectedIndex = 0;
                        }
                    }

                    comboBoxUnits.Items.Add("none");
                    comboBoxUnits.Items.Add("feet");
                    comboBoxUnits.Items.Add("inches");
                    comboBoxUnits.Items.Add("meters");
                    comboBoxUnits.Items.Add("square feet");
                    comboBoxUnits.Items.Add("square meters");
                    comboBoxUnits.Items.Add("cubic feet");
                    comboBoxUnits.Items.Add("cubic meters");

                    if (null != currentSetting.Units)
                    {
                        for (int i = 0; i < comboBoxUnits.Items.Count; i++)
                        {
                            if (comboBoxUnits.Items[i].ToString() == currentSetting.Units)
                            {
                                comboBoxUnits.SelectedIndex = i;
                            }
                        }
                    }
                    else
                    {
                        comboBoxUnits.SelectedIndex = 0;
                    }
                    result = true;
                }
                else
                {
                    MessageBox.Show("An analysis display style is required for the HOK AVF tool to work.\nGo to Additional Settings in the Manage Tab and select Analysis Display Style.  Then re-run the HOK AVF Tool.", 
                        "Missing Display Style", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    result = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display default settings.\n" + ex.Message, "DisplayStyleManager:DisplayDefaultSettings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        public void DisplayUserSettings()
        {
            try
            {
                if (comboBoxStyle.Items.Count > 0)
                {
                    if (null != currentSetting.DisplayStyle)
                    {
                        for (int i = 0; i < comboBoxStyle.Items.Count; i++)
                        {
                            if (comboBoxStyle.Items[i].ToString() == currentSetting.DisplayStyle)
                            {
                                comboBoxStyle.SelectedIndex = i;
                            }
                        }
                    }
                    else
                    {
                        comboBoxStyle.SelectedIndex = 0;
                    }
                }
                else
                {
                }

                if (comboBoxUnits.Items.Count > 0)
                {
                    if (null != currentSetting.Units)
                    {
                        for (int i = 0; i < comboBoxUnits.Items.Count; i++)
                        {
                            if (comboBoxUnits.Items[i].ToString() == currentSetting.Units)
                            {
                                comboBoxUnits.SelectedIndex = i;
                            }
                        }
                    }
                    else
                    {
                        comboBoxUnits.SelectedIndex = 0;
                    }
                }
                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display users' settings.\n" + ex.Message, "DisplayStyleManager:DisplayUserSettings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void comboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(null!=comboBoxStyle.SelectedItem)
            {
                string selectedStyle=comboBoxStyle.SelectedItem.ToString();
                if (styleDictionary.ContainsKey(selectedStyle)) { labelStyleType.Text = styleDictionary[selectedStyle]; }
            }
        }
    }
}
