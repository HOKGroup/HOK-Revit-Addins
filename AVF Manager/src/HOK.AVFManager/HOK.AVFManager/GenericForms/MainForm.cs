using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using HOK.AVFManager.GenericClasses;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Analysis;
using System.IO;

namespace HOK.AVFManager.GenericForms
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private Document doc;
        private AnalysisCategory selectedAnalysis;
        private InputCollector inputCollector;
        private DisplayStyleManager displayStyleManager;
        private ListViewItem currentAnalysis;
        private SettingProperties curSettingProperties=new SettingProperties();
        private Dictionary<string/*resultTypeName*/, SettingProperties> userSettings = new Dictionary<string, SettingProperties>();
        private bool isPickFace = false;
        private bool isPickRef = false;
        private ToolTip toolTip = new ToolTip();
        private bool foundStyles = false;

        public SettingProperties CurrentSettingProperites { get { return curSettingProperties; } set { curSettingProperties = value; } }
        public AnalysisCategory CurrentAnalysisCategory { get { return selectedAnalysis; } set { selectedAnalysis = value; } }
        public bool IsPickFace { get { return isPickFace; } set { isPickFace = value; } }
        public bool IsPickRef { get { return isPickRef; } set { isPickRef = value; } }
        public bool FoundStyles { get { return foundStyles; } set { foundStyles = value; } }

        #region Constructor
        public MainForm(Document document, AnalysisCategory analysisCategory)
        {
            doc = document;
            selectedAnalysis = analysisCategory;

            InitializeComponent();
            this.Text = "Settings - Analysis Visualization Framework v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            tabControlCategory.SelectedIndex = (int)analysisCategory;
            labelModel.Text = tabControlCategory.SelectedTab.Text;

            inputCollector = new InputCollector(doc);
            inputCollector.SetComponents(listViewResults, comboBoxCategory, comboBoxParameter,labelSelElements, bttnPickElements, bttnRefElements,
                radioButtonTop, radioButtonSide, radioButtonBottom, radioButtonAll, labelDescription);
            userSettings = inputCollector.SettingDictionary;

            displayStyleManager = new DisplayStyleManager(doc);
            displayStyleManager.SetComponents(comboBoxStyles, labelStyleType, comboBoxUnit);
            foundStyles=displayStyleManager.DisplayDefaultSettings();
        }
        //When Picking Elements
        public MainForm(Document document, AnalysisCategory analysisCategory, SettingProperties settingProperties)
        {
            doc = document;
            selectedAnalysis = analysisCategory;
            curSettingProperties = settingProperties;

            InitializeComponent();
            tabControlCategory.SelectedIndex = (int)analysisCategory;
            labelModel.Text = tabControlCategory.SelectedTab.Text;

            inputCollector = new InputCollector(doc);
            inputCollector.SetComponents(listViewResults, comboBoxCategory, comboBoxParameter, labelSelElements, bttnPickElements, bttnRefElements,
                radioButtonTop, radioButtonSide, radioButtonBottom, radioButtonAll, labelDescription);
            inputCollector.SetCurrentSettings(curSettingProperties);
            userSettings = inputCollector.SettingDictionary;
            inputCollector.DisplayUserSettings(curSettingProperties);

            displayStyleManager = new DisplayStyleManager(doc,curSettingProperties);
            displayStyleManager.SetComponents(comboBoxStyles, labelStyleType, comboBoxUnit);
            displayStyleManager.DisplayDefaultSettings();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            pictureBoxCube.Image = imageListCube.Images[0];
            if (null != curSettingProperties)
            {
                switch (curSettingProperties.DisplayFace)
                {
                    case DisplayingFaces.Top:
                        pictureBoxCube.Image = imageListCube.Images[1]; break;
                    case DisplayingFaces.Side:
                        pictureBoxCube.Image = imageListCube.Images[2]; break;
                    case DisplayingFaces.Bottom:
                        pictureBoxCube.Image = imageListCube.Images[3]; break;
                    case DisplayingFaces.All:
                        pictureBoxCube.Image = imageListCube.Images[4]; break;
                    default:
                        pictureBoxCube.Image = imageListCube.Images[0]; break;
                }
            }

            foreach (ListViewItem item in listViewUP.Items)
            {
                if (item.Text == "Mass Analysis") { item.Tag = ResultType.MassAnalysis; }
                if (item.Text == "FAR Calculator") { item.Tag = ResultType.FARCalculator; }
                if (item.Text == "Topography") { item.Tag = ResultType.Topography; }
                if (item.Text == "Building Network") { item.Tag = ResultType.BuildingNetwork; }
                if (item.Text == "Customized Analysis") { item.Tag = ResultType.CustomizedAnalysis; }
            }
            foreach (ListViewItem item in listViewAR.Items)
            {
                if (item.Text == "Facade Analysis") { item.Tag = ResultType.FacadeAnalysis; }
                if (item.Text == "Heat Map") { item.Tag = ResultType.HeatMap; }
                if (item.Text == "Customized Analysis") { item.Tag = ResultType.CustomizedAnalysis; }
            }
            foreach (ListViewItem item in listViewID.Items)
            {
                if (item.Text == "Radiance Analysis") { item.Tag = ResultType.RadianceAnalysis; }
                if (item.Text == "Field of View") { item.Tag = ResultType.FieldOfView; }
                if (item.Text == "Customized Analysis") { item.Tag = ResultType.CustomizedAnalysis; }
            }

            string selectedTabPage = tabControlCategory.TabPages[tabControlCategory.SelectedIndex].Name;
            for (int i = tabControlCategory.TabPages.Count - 1; i > -1; i--)
            {
                if (tabControlCategory.TabPages[i].Name == selectedTabPage) { continue; }
                else
                {
                    tabControlCategory.TabPages.RemoveAt(i);
                }
            }
            DisplayExisitingResults();
            textBoxTitle.Text = curSettingProperties.LegendTitle;
            textBoxDescription.Text = curSettingProperties.LegendDescription;
        }
        #endregion 

        private void DisplayExisitingResults()
        {
            listViewResults.Items.Clear();
            SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
            if (sfm != null)
            {
                IList<int> regIndices = sfm.GetRegisteredResults();
                foreach (int i in regIndices)
                {
                    AnalysisResultSchema result = sfm.GetResultSchema(i);
                    ListViewItem item = new ListViewItem(result.Name);
                    item.Name = result.Name;
                    item.Tag = result;
                    item.Checked = result.IsVisible;
                    listViewResults.Items.Add(item);
                }
            }
        }

        #region RadioButton (Top, Side, Bottom, All)
        private void radioButtonTop_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonTop.Checked)
            {
                pictureBoxCube.Image = imageListCube.Images[1];
            }
            else
            {
                pictureBoxCube.Image = imageListCube.Images[0];
            }
        }

        private void radioButtonSide_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSide.Checked)
            {
                pictureBoxCube.Image = imageListCube.Images[2];
            }
            else
            {
                pictureBoxCube.Image = imageListCube.Images[0];
            }
        }

        private void radioButtonBottom_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBottom.Checked)
            {
                pictureBoxCube.Image = imageListCube.Images[3];
            }
            else
            {
                pictureBoxCube.Image = imageListCube.Images[0];
            }
        }

        private void radioButtonAll_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAll.Checked)
            {
                pictureBoxCube.Image = imageListCube.Images[4];
            }
            else
            {
                pictureBoxCube.Image = imageListCube.Images[0];
            }
        }
        #endregion

        #region ListView ToolBox
        private void listViewUP_SelectedIndexChanged(object sender, EventArgs e)
        {
            bttnImportData.Enabled = false;
            if (listViewUP.SelectedItems.Count>0)
            {
                SaveUserSettings();
                currentAnalysis = listViewUP.SelectedItems[0];
                if (userSettings.ContainsKey(currentAnalysis.Text)) 
                { 
                    curSettingProperties = userSettings[currentAnalysis.Text];
                    curSettingProperties.AnalysisCategory = AnalysisCategory.UrbanPlanning;
                    if (null != currentAnalysis.Tag) { curSettingProperties.ResultType = (ResultType)currentAnalysis.Tag; }

                    inputCollector.DisplayUserSettings(curSettingProperties);

                    displayStyleManager.CurrentSetting = curSettingProperties;
                    displayStyleManager.DisplayUserSettings();
                    DisplayResultName();
                }

                for (int i = 0; i < listViewUP.Items.Count; i++)
                {
                    ListViewItem item = listViewUP.Items[i];
                    if (item.Text == currentAnalysis.Text) { item.BackColor = System.Drawing.Color.DodgerBlue; item.ForeColor = System.Drawing.Color.White; }
                    else { item.BackColor = System.Drawing.Color.White; item.ForeColor = System.Drawing.Color.Black; }
                }
            }
        }

        private void listViewAR_SelectedIndexChanged(object sender, EventArgs e)
        {
            bttnImportData.Enabled = false;
            if (listViewAR.SelectedItems.Count > 0)
            {
                SaveUserSettings();
                currentAnalysis = listViewAR.SelectedItems[0];
                if (userSettings.ContainsKey(currentAnalysis.Text))
                {
                    curSettingProperties = userSettings[currentAnalysis.Text];
                    curSettingProperties.AnalysisCategory = AnalysisCategory.Architecture;
                    if (null != currentAnalysis.Tag) { curSettingProperties.ResultType = (ResultType)currentAnalysis.Tag; }

                    inputCollector.DisplayUserSettings(curSettingProperties);

                    displayStyleManager.CurrentSetting = curSettingProperties;
                    displayStyleManager.DisplayUserSettings();
                    DisplayResultName();
                }

                for (int i = 0; i < listViewAR.Items.Count; i++)
                {
                    ListViewItem item = listViewAR.Items[i];
                    if (item.Text == currentAnalysis.Text) { item.BackColor = System.Drawing.Color.DodgerBlue; item.ForeColor = System.Drawing.Color.White; }
                    else { item.BackColor = System.Drawing.Color.White; item.ForeColor = System.Drawing.Color.Black; }
                }
            }
        }

        private void listViewID_SelectedIndexChanged(object sender, EventArgs e)
        {
            bttnImportData.Enabled = false;
            if (listViewID.SelectedItems.Count > 0)
            {
                SaveUserSettings();
                currentAnalysis = listViewID.SelectedItems[0];
                if (userSettings.ContainsKey(currentAnalysis.Text))
                {
                    if (currentAnalysis.Text == "Radiance Analysis") { bttnImportData.Enabled = true; }
                    curSettingProperties = userSettings[currentAnalysis.Text];
                    curSettingProperties.AnalysisCategory = AnalysisCategory.InteriorDesign;
                    if (null != currentAnalysis.Tag) { curSettingProperties.ResultType = (ResultType)currentAnalysis.Tag; }

                    inputCollector.DisplayUserSettings(curSettingProperties);

                    displayStyleManager.CurrentSetting = curSettingProperties;
                    displayStyleManager.DisplayUserSettings();
                    DisplayResultName();
                }

                for (int i = 0; i < listViewID.Items.Count; i++)
                {
                    ListViewItem item = listViewID.Items[i];
                    if (item.Text == currentAnalysis.Text) { item.BackColor = System.Drawing.Color.DodgerBlue; item.ForeColor = System.Drawing.Color.White; }
                    else { item.BackColor = System.Drawing.Color.White; item.ForeColor = System.Drawing.Color.Black; }
                }
            }
        }

        private void SaveUserSettings()
        {
            if (null != curSettingProperties.ResultName && null != textBoxTitle.Text)
            {
                if (comboBoxCategory.SelectedIndex > -1) { curSettingProperties.CategoryName = comboBoxCategory.SelectedItem.ToString(); }
                if (comboBoxParameter.SelectedIndex > -1) { curSettingProperties.ParameterName = comboBoxParameter.SelectedItem.ToString(); }
                if (radioButtonTop.Checked) { curSettingProperties.DisplayFace = DisplayingFaces.Top; }
                if (radioButtonSide.Checked) { curSettingProperties.DisplayFace = DisplayingFaces.Side; }
                if (radioButtonBottom.Checked) { curSettingProperties.DisplayFace = DisplayingFaces.Bottom; }
                if (radioButtonAll.Checked) { curSettingProperties.DisplayFace = DisplayingFaces.All; }
                if (comboBoxStyles.SelectedIndex > -1) { curSettingProperties.DisplayStyle = comboBoxStyles.SelectedItem.ToString(); }
                if (comboBoxUnit.SelectedIndex > -1) { curSettingProperties.Units = comboBoxUnit.SelectedItem.ToString(); }
                curSettingProperties.LegendTitle = textBoxTitle.Text;
                if (textBoxDescription.Text != string.Empty) { curSettingProperties.LegendDescription = textBoxDescription.Text; }
                if (userSettings.ContainsKey(curSettingProperties.ResultName)) { userSettings.Remove(curSettingProperties.ResultName); }
                userSettings.Add(curSettingProperties.ResultName, curSettingProperties);

                if (null != curSettingProperties.Configurations) { CurrentSettingProperites.NumberOfMeasurement = curSettingProperties.Configurations.Count; }
                else if (string.Empty != curSettingProperties.ParameterName)
                {
                    Dictionary<string, string> configurations = new Dictionary<string, string>();
                    configurations.Add(curSettingProperties.ParameterName, " ");
                    curSettingProperties.Configurations = configurations;
                    CurrentSettingProperites.NumberOfMeasurement = curSettingProperties.Configurations.Count; 
                }
            }
        }

        private void DisplayResultName()
        {
            string resultName = curSettingProperties.ResultName;
            if (listViewResults.Items.ContainsKey(resultName))
            {
                int index = 0;
                foreach (ListViewItem item in listViewResults.Items)
                {
                    if (item.Text.Contains(resultName))
                    {
                        string[] strArr = item.Text.Split('-');
                        int suffix = 0;
                        if (strArr.Length > 1)
                        {
                            if (int.TryParse(strArr[strArr.Length - 1], out suffix))
                            {
                                if (suffix > index) { index = suffix; }
                            }
                        }
                    }
                }
                index += 1;
                curSettingProperties.LegendTitle = resultName + " - " + index;
            }
            else
            {
                curSettingProperties.LegendTitle = resultName;
            }
            textBoxTitle.Text = curSettingProperties.LegendTitle;
            textBoxDescription.Text = curSettingProperties.ResultName;
            curSettingProperties.LegendDescription = textBoxDescription.Text;
        }
        #endregion 

        #region Display Style Settings
        private void comboBoxStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxStyles.Items.Count > 0 && null != comboBoxStyles.SelectedItem)
            {
                curSettingProperties.DisplayStyle = comboBoxStyles.SelectedItem.ToString();
            }
        }

        private void comboBoxUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxUnit.Items.Count > 0 && null != comboBoxUnit.SelectedItem)
            {
                curSettingProperties.Units = comboBoxUnit.SelectedItem.ToString();
            }
        }

        private void SetStyleAndUnit()
        {
            if (comboBoxStyles.Items.Count > 0 && null != comboBoxStyles.SelectedItem)
            {
                curSettingProperties.DisplayStyle = comboBoxStyles.SelectedItem.ToString();
            }
            if (comboBoxUnit.Items.Count > 0 && null != comboBoxUnit.SelectedItem)
            {
                curSettingProperties.Units = comboBoxUnit.SelectedItem.ToString();
            }
        }
        #endregion

        private void bttnPickFaces_Click(object sender, EventArgs e)
        {
            //curSettingProperties = inputCollector.CurrentSetting;
            curSettingProperties.DisplayFace = DisplayingFaces.Custom;
            isPickFace = true;
            SaveUserSettings();
            if (curSettingProperties.SelectedElements.Count > 0)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void bttnPickElements_Click(object sender, EventArgs e)
        {
            //curSettingProperties = inputCollector.CurrentSetting;
            isPickFace = false;
            SaveUserSettings();
            if (curSettingProperties.SelectedElements.Count > 0)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        public void PickFaces(string categoryName)
        {
            try
            {
                UIDocument uidoc = new UIDocument(doc);
                IList<Reference> refList = uidoc.Selection.PickObjects(ObjectType.Face, "Select multiple faces in the category of " + categoryName);
                
                Dictionary<ElementId, Dictionary<Reference, Face>> pickedFaces = new Dictionary<ElementId, Dictionary<Reference, Face>>();
                int faceCount = 0;
                foreach (Reference reference in refList)
                {
                    ElementId elementId = reference.ElementId;
                    Face face = doc.GetElement(reference).GetGeometryObjectFromReference(reference) as Face;

                    if (null != elementId && null != face)
                    {
                        if (pickedFaces.ContainsKey(elementId))
                        {
                            pickedFaces[elementId].Add(reference, face);
                            faceCount++;
                        }
                        else
                        {
                            pickedFaces.Add(elementId, new Dictionary<Reference, Face>());
                            pickedFaces[elementId].Add(reference, face);
                            faceCount++;
                        }
                    }
                }

                curSettingProperties.IsPicked = true;
                curSettingProperties.SelectedFaces = pickedFaces;
                labelSelFaces.Text = faceCount + " Faces Selected";
                inputCollector.SetCurrentSettings(curSettingProperties);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Pick Faces in the category of " + categoryName + "\n" + ex.Message, "MainForm:PickFaces", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void PickElements(string categoryName)
        {
            try
            {
                UIDocument uidoc = new UIDocument(doc);
                IList<Reference> refList = uidoc.Selection.PickObjects(ObjectType.Element, "Select multiple elements in the category of " + categoryName);

                List<Element> pickedElements = new List<Element>();
                foreach (Reference reference in refList)
                {
                    Element element = doc.GetElement(reference.ElementId);
                    if (null != element && element.Category.Name == categoryName)
                    {
                        pickedElements.Add(element);
                    }
                }
                curSettingProperties.IsPicked = true;
                curSettingProperties.SelectedElements = pickedElements;
                labelSelElements.Text = pickedElements.Count.ToString() + " Elements Selected";
                inputCollector.SetCurrentSettings(curSettingProperties);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Pick Elements in the category of " + categoryName + "\n" + ex.Message, "MainForm:PickElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void PickReference(string categoryName)
        {
            try
            {
                UIDocument uidoc = new UIDocument(doc);
                IList<Reference> refList = uidoc.Selection.PickObjects(ObjectType.Element, "Select multiple elements in the category of " + categoryName);

                List<Element> pickedElements = new List<Element>();
                foreach (Reference reference in refList)
                {
                    Element element = doc.GetElement(reference.ElementId);
                    if (null != element && element.Category.Name == categoryName)
                    {
                        pickedElements.Add(element);
                    }
                }
                curSettingProperties.IsPicked = true;
                curSettingProperties.ReferenceElements = pickedElements;
                inputCollector.SetCurrentSettings(curSettingProperties);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Pick Elements in the category of " + categoryName + "\n" + ex.Message, "MainForm:PickElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnRefElements_Click(object sender, EventArgs e)
        {
            isPickFace = false;
            isPickRef = true;
            SaveUserSettings();
            this.DialogResult = DialogResult.OK;
        }

        private void bttnDefaults_Click(object sender, EventArgs e)
        {
            if (null != currentAnalysis)
            {
                curSettingProperties = inputCollector.DisplayDefaultSettings(currentAnalysis.Text);
                displayStyleManager.CurrentSetting = curSettingProperties;
                displayStyleManager.DisplayUserSettings();
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnRemove_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Would you like to remove all Analysis Results of the current view?", "Remove All Analysis Results", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
                sfm.Clear();
            }
        }

        //Start Analyzing
        private void bttnAnalysis_Click(object sender, EventArgs e)
        {
            SaveUserSettings();
            SetStyleAndUnit();
            bool valid = ValidAnalysisSetting();
            if (valid && listViewResults.Items.ContainsKey(curSettingProperties.LegendTitle))
            {
                DialogResult dr = MessageBox.Show("[" + curSettingProperties.LegendTitle + "] already exists.\n Do you want to overwrite it?", "Exisiting Analysis Result", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Run Analysis");
                        try
                        {
                            HideExistingAnalysis();
                            progressBarAnalysis.Visible = true;
                            toolStripStatusLabel.Visible = true;
                            toolStripStatusLabel.Text = "Calculating .. ";
                            AnalysisDataManager dataManager = new AnalysisDataManager(doc, curSettingProperties, progressBarAnalysis);
                            if (dataManager.CreateSpatialField()) { this.Close(); }
                            else { toolStripStatusLabel.Text = "Ready"; }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to run analysis.\n" + ex.Message, "AVF Manager - Run Analysis", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                        }
                    }
                }
            }
            else if(valid)
            {
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Run Analysis");
                    try
                    {
                        HideExistingAnalysis();
                        AnalysisDataManager dataManager = new AnalysisDataManager(doc, curSettingProperties, progressBarAnalysis);
                        progressBarAnalysis.Visible = true;
                        toolStripStatusLabel.Visible = true;
                        toolStripStatusLabel.Text = "Calculating .. ";
                        if (dataManager.CreateSpatialField()) { this.Close(); }
                        else { toolStripStatusLabel.Text = "Ready"; }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to run analysis.\n" + ex.Message, "AVF Manager - Run Analysis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private bool ValidAnalysisSetting()
        {
            bool result = false;
            if (null == curSettingProperties.ResultName) { MessageBox.Show("Please select an analysis type in the toolbox.", "Invalid Analysis Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            else if (string.Empty == curSettingProperties.LegendTitle||null==curSettingProperties.LegendTitle) { MessageBox.Show("Please set a name for the title of legend.", "Invalid Analysis Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            else if (string.Empty == curSettingProperties.LegendDescription||null==curSettingProperties.LegendDescription) { MessageBox.Show("Please enter description for the description of legend.", "Invalid Analysis Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            else if (curSettingProperties.SelectedElements.Count < 1) { MessageBox.Show("There is no element selected for the analysis.", "Invalid Analysis Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            else { result = true; }
            return result;
        }

        private void HideExistingAnalysis()
        {
            SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
            if (sfm != null)
            {
                IList<int> regIndices = sfm.GetRegisteredResults();
                foreach (int i in regIndices)
                {
                    AnalysisResultSchema result = sfm.GetResultSchema(i);
                    result.IsVisible = false;
                    sfm.SetResultSchema(i, result);
                }
            }
        }

        private void bttnAdvanced_Click(object sender, EventArgs e)
        {
            if (null != curSettingProperties.ResultName)
            {
                List<string> parameters = new List<string>();
                foreach (object item in comboBoxParameter.Items)
                {
                    parameters.Add(item.ToString());
                }
                curSettingProperties.ParameterList = parameters;

                AdvancedSettingsForm advancedSettings = new AdvancedSettingsForm(curSettingProperties);
                if (DialogResult.OK == advancedSettings.ShowDialog())
                {
                    curSettingProperties = advancedSettings.CurrentSettings;
                    advancedSettings.Close();
                }
            }
        }

        private void bttnImportData_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Path.GetDirectoryName(doc.PathName);
                openFileDialog.Filter = "dat files (*.dat)|*.dat|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    curSettingProperties.ReferenceDataFile = openFileDialog.FileName;
                    toolTip.SetToolTip(this.bttnImportData, curSettingProperties.ReferenceDataFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import reference data.\n" + ex.Message, "MainForm:bttnImportData_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
