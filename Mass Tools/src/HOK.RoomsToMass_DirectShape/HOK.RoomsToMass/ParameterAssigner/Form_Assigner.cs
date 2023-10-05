using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HOK.RoomsToMass.ToMass;
using System.IO;
using HOK.Core.Utilities;
using static HOK.Core.Utilities.ElementIdExtension;


namespace HOK.RoomsToMass.ParameterAssigner
{
    public partial class Form_Assigner : System.Windows.Forms.Form
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;

        private Dictionary<string/*worksetName*/, int/*worksetId*/ > worksetDictionary = new Dictionary<string, int>();
        private List<MassProperties> integratedMassList = new List<MassProperties>(); //mass from host projects and linked files
        private Dictionary<long/*instanceId*/, LinkedInstanceProperties> linkedMassDictionary = new Dictionary<long, LinkedInstanceProperties>(); //mass from linked models
        private Dictionary<long/*elementId*/, ElementProperties> elementDictionary = new Dictionary<long, ElementProperties>();
        private Dictionary<long/*elementId*/, ElementProperties> intersectingElements = new Dictionary<long, ElementProperties>();
        private Dictionary<long/*elementId*/, ElementProperties> unassignedElements = new Dictionary<long, ElementProperties>();
        private Dictionary<long/*categoryId*/, Dictionary<long, ParameterProperties>> parameterMaps = new Dictionary<long, Dictionary<long, ParameterProperties>>();
        private List<string> massParameters = new List<string>();
        private Dictionary<string/*massParam*/, string/*elemParam*/> selectedParameters = new Dictionary<string, string>();
        //private Dictionary<int/*elementId*/, int/*massId*/> elementMassMap = new Dictionary<int, int>();
        private Dictionary<Category, bool> elementCategories = new Dictionary<Category, bool>();
        private List<long/*categoryId*/> selectedCategoryIds = new List<long>();
        private StringBuilder failureMessage = new StringBuilder();
        private MassSource selectedSource;
        private List<string> splitCategories = new List<string>();
        private string[] categoryNames = new string[] { "Columns", "Conduits", "Ducts", "Floors", "Pipes", "Roofs", "Structural Columns", "Structural Framing", "Walls" };
        private SplitINIDataManager splitDataManager;
        private bool followHost = true;
        private double ratio = 0.7;

        public Dictionary<string, int> WorksetDictionary { get { return worksetDictionary; } set { worksetDictionary = value; } }
        public List<MassProperties> IntegratedMassList { get { return integratedMassList; } set { integratedMassList = value; } }
        public Dictionary<long, LinkedInstanceProperties> LinkedMassDictionary { get { return linkedMassDictionary; } set { linkedMassDictionary = value; } }
        public Dictionary<long, ElementProperties> ElementDictionary { get { return elementDictionary; } set { elementDictionary = value; } }
        public Dictionary<Category, bool> ElementCategories { get { return elementCategories; } set { elementCategories = value; } }
        public List<string> MassParameters { get { return massParameters; } set { massParameters = value; } }
        public MassSource SelectedSourceType { get { return selectedSource; } set { selectedSource = value; } }
        public Dictionary<long, Dictionary<long, ParameterProperties>> ParameterMaps { get { return parameterMaps; } set { parameterMaps = value; } }

        public Form_Assigner(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();

            toolStripProgressBar.Visible = false;

            splitDataManager = new SplitINIDataManager(m_app);
            if (splitDataManager.SplitDictionary.Count > 0)
            {
                //bttnJoin.Enabled = true;
            }
            this.Text = "Mass Commands - v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Form_Assigner_Load(object sender, EventArgs e)
        {
            foreach (var paramName in massParameters)
            {
                selectedParameters.Add(paramName, paramName);
            }

            DefineRadioButton(selectedSource);
            DisplayMassList(selectedSource);
        }

        private void DefineRadioButton(MassSource massSourceType)
        {
            radioButtonAll.Enabled = false;
            radioButtonHost.Enabled = false;
            radioButtonLink.Enabled = false;

            switch (massSourceType)
            {
                case MassSource.SelectedMass:
                    radioButtonHost.Enabled = true;
                    radioButtonHost.Checked = true;
                    break;
                case MassSource.DisplayAll:
                    radioButtonAll.Enabled = true;
                    radioButtonHost.Enabled = true;
                    radioButtonLink.Enabled = true;
                    radioButtonAll.Checked = true;
                    break;
                case MassSource.OnlyHost:
                    radioButtonHost.Enabled = true;
                    radioButtonHost.Checked = true;
                    break;
                case MassSource.OnlyLink:
                    radioButtonLink.Enabled = true;
                    radioButtonLink.Checked = true;
                    break;
            }

        }

        private void DisplayMassList(MassSource massSourceType)
        {
            try
            {
                dataGridViewParameter.Rows.Clear();
                dataGridViewParameter.Columns.Clear();
                foreach (var paramName in massParameters)
                {

                    var colIndex = dataGridViewParameter.Columns.Add(paramName, paramName);
                    if (selectedParameters.ContainsKey(paramName))
                    {
                        dataGridViewParameter.Columns[paramName].HeaderText = selectedParameters[paramName];
                    }
                    else
                    {
                        dataGridViewParameter.Columns[paramName].Visible = false;
                    }
                }

                dataGridViewMass.Rows.Clear();
                var index = 0;
                //mass instances from linked files
                foreach (var mp in integratedMassList)
                {
                    if (mp.IsHost && massSourceType == MassSource.OnlyLink) { continue; }
                    if (!mp.IsHost && massSourceType == MassSource.OnlyHost) { continue; }

                    index = dataGridViewMass.Rows.Add();
                    dataGridViewMass.Rows[index].Cells[0].Value = false;//Selection
                    dataGridViewMass.Rows[index].Cells[1].Value = mp.MassId;//MassId
                    dataGridViewMass.Rows[index].Cells[2].Value = mp.MassName;//Name
                    if (!mp.IsHost && null != mp.LInkedFileName)
                    {
                        dataGridViewMass.Rows[index].Cells[3].Value = mp.LInkedFileName;//Linked File
                    }
                    else
                    {
                        dataGridViewMass.Rows[index].Cells[3].Value = "";//Linked File
                    }

                    if (checkBoxFilter.Checked && null != mp.FilteredContainer)
                    {
                        dataGridViewMass.Rows[index].Cells[4].Value = mp.FilteredContainer.Count + " Elements";//View
                        dataGridViewMass.Rows[index].Cells[4].Tag = mp.FilteredContainer;
                    }
                    else
                    {
                        dataGridViewMass.Rows[index].Cells[4].Value = mp.ElementCount.ToString() + " Elements";//View
                        dataGridViewMass.Rows[index].Cells[4].Tag = mp.ElementContainer;
                    }

                    dataGridViewMass.Rows[index].Tag = mp;

                    DisplayParamList(mp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the information about mass elements.\n" + ex.Message, "Form_Assigner:DisplayMassList", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayParamList(MassProperties mp)
        {
            try
            {
                var index = dataGridViewParameter.Rows.Add();
                dataGridViewParameter.Rows[index].Tag = mp;
                foreach (DataGridViewColumn column in dataGridViewParameter.Columns)
                {
                    dataGridViewParameter.Rows[index].Cells[column.Name].Value = "";
                }

                foreach (var param in mp.MassParameters)
                {
                    if (dataGridViewParameter.Columns.Contains(param.Definition.Name))
                    {
                        switch (param.StorageType)
                        {
                            case StorageType.String:
                                dataGridViewParameter.Rows[index].Cells[param.Definition.Name].Value = param.AsString();
                                break;
                            case StorageType.Double:
                                dataGridViewParameter.Rows[index].Cells[param.Definition.Name].Value = param.AsDouble();
                                break;
                            case StorageType.Integer:
                                dataGridViewParameter.Rows[index].Cells[param.Definition.Name].Value = param.AsInteger();
                                break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the information about mass parameters.\n" + ex.Message, "Form_Assigner:DisplayParamList", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridViewMass_Scroll(object sender, ScrollEventArgs e)
        {
            if (dataGridViewMass.RowCount == dataGridViewParameter.RowCount)
            {
                dataGridViewParameter.FirstDisplayedScrollingRowIndex = dataGridViewMass.FirstDisplayedScrollingRowIndex;
            }
        }

        private void dataGridViewParameter_Scroll(object sender, ScrollEventArgs e)
        {
            if (dataGridViewParameter.RowCount == dataGridViewMass.RowCount)
            {
                dataGridViewMass.FirstDisplayedScrollingRowIndex = dataGridViewParameter.FirstDisplayedScrollingRowIndex;
            }
        }

        private void bttnCheckAll_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridViewMass.Rows)
                {
                    row.Cells[0].Value = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all.\n" + ex.Message, "Form_Assigner:bttnCheckAll_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCheckNone_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridViewMass.Rows)
                {
                    row.Cells[0].Value = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck all.\n" + ex.Message, "Form_Assigner:bttnCheckNone_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void radioButtonAll_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAll.Checked)
            {
                selectedSource = MassSource.DisplayAll;
                DisplayMassList(selectedSource);
            }
        }

        private void radioButtonLink_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonLink.Checked)
            {
                selectedSource = MassSource.OnlyLink;
                DisplayMassList(selectedSource);
            }
        }

        private void radioButtonHost_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonHost.Checked)
            {
                selectedSource = MassSource.OnlyHost;
                DisplayMassList(selectedSource);
            }
        }

        private void bttnParameter_Click(object sender, EventArgs e)
        {
            try
            {
                var parametersForm = new Form_Parameters(massParameters, selectedParameters);
                if (DialogResult.OK == parametersForm.ShowDialog())
                {
                    selectedParameters = new Dictionary<string, string>();
                    selectedParameters = parametersForm.SelectedParam;
                    parametersForm.Close();
                }

                for (var i = 0; i < dataGridViewParameter.Columns.Count; i++)
                {
                    var column = dataGridViewParameter.Columns[i];
                    if (selectedParameters.ContainsKey(column.Name))
                    {
                        column.HeaderText = selectedParameters[column.Name];
                        column.Visible = true;
                    }
                    else
                    {
                        column.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the information about mass parameters.\n" + ex.Message, "Form_Assigner:bttnParameter_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnAssign_Click(object sender, EventArgs e)
        {
            try
            {
                var missingWorksets = new List<string>();
                var massList = CountElements();
                var existingParameters = new List<string>();
                var emptyOnly = checkBoxEmptyParam.Checked;
                var modelGroupExist = false;

                if (massList.Count > 0)
                {
                    if (CheckOverlappingToAssign(massList))
                    {
                        foreach (var mp in massList)
                        {
                            var elementList = new List<Element>();
                            if (checkBoxFilter.Checked)
                            {
                                elementList = mp.FilteredContainer;
                            }
                            else
                            {
                                elementList = mp.ElementContainer;
                            }

                            foreach (var element in elementList)
                            {
                                toolStripProgressBar.PerformStep();
                                //if (ElementId.InvalidElementId!=element.GroupId) {  modelGroupExist = true; continue;  }
                                if (intersectingElements.ContainsKey(GetElementIdValue(element.Id)))
                                {
                                    var ep = intersectingElements[GetElementIdValue(element.Id)];
                                    if (ep.CategoryName == "Model Groups") { modelGroupExist = true; continue; }
                                    if (ep.SelectedMassId != mp.MassId) { continue; } //if an element already assigned to another mass to propagate parameters
                                }

                                using (var trans = new Transaction(m_doc))
                                {
                                    var failureHandlingOptions = trans.GetFailureHandlingOptions();
                                    var failureHandler = new FailureHandler();
                                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                                    failureHandlingOptions.SetClearAfterRollback(true);
                                    trans.SetFailureHandlingOptions(failureHandlingOptions);

                                    trans.Start("Set Parameter");
                                    //set parameters
                                    foreach (var param in mp.MassParameters)
                                    {
                                        if (!selectedParameters.ContainsKey(param.Definition.Name)) { continue; }

                                        var paramName = selectedParameters[param.Definition.Name];
                                        var elemParam = element.LookupParameter(paramName);

                                        if (null != elemParam)
                                        {
                                            if (!existingParameters.Contains(param.Definition.Name))
                                            {
                                                existingParameters.Add(param.Definition.Name); //to determine the existence of parameters.
                                            }

                                            switch (elemParam.StorageType)
                                            {
                                                case StorageType.Integer:
                                                    try
                                                    {
                                                        if (emptyOnly) { if (elemParam.AsInteger() != 0) { break; } }
                                                        if (paramName == "Workset")
                                                        {
                                                            var worksetName = param.AsString();
                                                            if (worksetDictionary.ContainsKey(worksetName))
                                                            {
                                                                elemParam.Set(worksetDictionary[worksetName]);
                                                            }
                                                            else if (!missingWorksets.Contains(worksetName))
                                                            {
                                                                missingWorksets.Add(worksetName);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            elemParam.Set(param.AsInteger());
                                                        }
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    break;

                                                case StorageType.Double:
                                                    try
                                                    {
                                                        if (emptyOnly) { if (elemParam.AsDouble() != 0) { break; } }
                                                        elemParam.Set(param.AsDouble());
                                                    }
                                                    catch { }
                                                    break;
                                                case StorageType.String:
                                                    try
                                                    {
                                                        if (emptyOnly) { if (elemParam.AsString() != "") { break; } }
                                                        var value = param.AsString();
                                                        elemParam.Set(value);
                                                    }
                                                    catch { }
                                                    break;
                                            }
                                        }
                                    }
                                    trans.Commit();
                                    if (failureHandler.FailureMessageInfoList.Count > 0)
                                    {
                                        failureMessage.AppendLine("[" + element.Name + ", " + element.Id + "] :" + failureHandler.FailureMessageInfoList[0].ErrorMessage);
                                    }
                                }
                            }

                        }

                        if (missingWorksets.Count > 0)
                        {
                            var strBuilder = new StringBuilder();
                            strBuilder.AppendLine("The following worksets were not found in the host project.\n");
                            foreach (var worksetName in missingWorksets)
                            {
                                strBuilder.AppendLine(worksetName);
                            }
                            MessageBox.Show(strBuilder.ToString(), "Worksets Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        if (selectedParameters.Count != existingParameters.Count)
                        {
                            var strBuilder = new StringBuilder();
                            strBuilder.AppendLine("The following selected parameters cannot be found in any of elements.");
                            foreach (var massParam in selectedParameters.Keys)
                            {
                                if (!existingParameters.Contains(massParam))
                                {
                                    strBuilder.AppendLine("Parameter Name: " + selectedParameters[massParam]);//element Parameter name
                                }
                            }
                            MessageBox.Show(strBuilder.ToString(), "Parameters Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        toolStripStatusLabel.Text = "Completed.";
                        toolStripProgressBar.Visible = false;

                        if (modelGroupExist)
                        {
                            MessageBox.Show("Elements in Model Groups were skipped for the assignment of parameters.\nYou may require ungroup them before running this command.", "Model Groups Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        if (failureMessage.Length > 0)
                        {
                            //logCreated = LogFileManager.CreateLogFile(m_doc, MassTool.MassCommands);
                            //LogFileManager.ClearLogFile();

                            var strBuilder = new StringBuilder();
                            strBuilder.AppendLine(DateTime.Now + ": Started Assigning Parameter Values");
                            strBuilder.AppendLine("Parameter values cannot be set on following elements\n");
                            Log.AppendLog(LogMessageType.WARNING, strBuilder.ToString() + failureMessage);
                            //if (logCreated) { LogFileManager.WriteLogFile(); }

                            var msgForm = new MessageBoxForm("Assigning Parameters", strBuilder.ToString() + failureMessage, LogFileManager.logFullName, true, true);
                            msgForm.ShowDialog();

                            //MessageBox.Show("Parameter values cannot be set on following elements:\n\n"+failureMessage.ToString(), "Failure Message : Assigning Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (existingParameters.Count > 0)
                        {
                            if (unassignedElements.Count > 0)
                            {
                                //logCreated = LogFileManager.CreateLogFile(m_doc, MassTool.MassCommands);
                                //LogFileManager.ClearLogFile();
                                var strBuilder = new StringBuilder();
                                strBuilder.AppendLine(DateTime.Now + ": Unassigned Elements");
                                strBuilder.AppendLine("Following elements are skipped and not assigned with any of mass parameters.\n");
                                foreach (var elementId in unassignedElements.Keys)
                                {
                                    var ep = unassignedElements[elementId];
                                    strBuilder.AppendLine("Element Id: " + ep.ElementId + " Element Name: " + ep.ElementName);
                                }
                                Log.AppendLog(LogMessageType.WARNING, strBuilder.ToString());
                                //if (logCreated) { LogFileManager.WriteLogFile(); }

                                var msgForm = new MessageBoxForm("Unassigned Elements", strBuilder.ToString(), LogFileManager.logFullName, true, true);
                                msgForm.ShowDialog();

                            }
                            else
                            {
                                MessageBox.Show("Parameter values were successfully propagated.", "Completion Message : Assigning Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        this.DialogResult = DialogResult.OK;
                    }
                }
                else
                {
                    MessageBox.Show("Please select at least one mass to assign parameters of elements.", "No Mass Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to transfer mass parameters to element parameters.\n" + ex.Message, "Form_Assigner:bttnAssign_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<Element> FindEmptyParameter(List<Element> selectedElements)
        {
            var filteredElements = new List<Element>();
            try
            {
                var parameterNames = selectedParameters.Values.ToList();
                var categoryNames = new List<string>();
                var paramDictionary = new Dictionary<long, Parameter>();

                foreach (var element in selectedElements)
                {
                    if (null != element.Category)
                    {
                        if (categoryNames.Contains(element.Category.Name)) { continue; }
                        foreach (var paramName in parameterNames)
                        {
                            var param = element.LookupParameter(paramName);

                            if (null != param)
                            {
                                if (paramDictionary.ContainsKey(GetElementIdValue(param.Id))) { continue; }
                                else
                                {
                                    paramDictionary.Add(GetElementIdValue(param.Id), param);
                                }
                            }
                        }
                        categoryNames.Add(element.Category.Name);
                    }
                }

                var elementParamFilters = new List<ElementFilter>();
                foreach (var paramId in paramDictionary.Keys)
                {
                    var param = paramDictionary[paramId];
                    var provider = new ParameterValueProvider(param.Id);
                    ElementParameterFilter filter;
                    switch (param.StorageType)
                    {
                        case StorageType.Double:
                            var doubleRule = new FilterDoubleRule(provider, new FilterNumericEquals(), 0, 0);
                            filter = new ElementParameterFilter(doubleRule);
                            elementParamFilters.Add(filter);
                            break;
                        case StorageType.Integer:
                            var integerRule = new FilterIntegerRule(provider, new FilterNumericEquals(), 0);
                            filter = new ElementParameterFilter(integerRule);
                            elementParamFilters.Add(filter);
                            break;
                        case StorageType.String:
#if RELEASE2022 || RELEASE2023 || RELEASE2024
                            var stringRule = new FilterStringRule(provider, new FilterStringEquals(), "");
#else
                            var stringRule = new FilterStringRule(provider, new FilterStringEquals(), "", false);
#endif
                            filter = new ElementParameterFilter(stringRule);
                            elementParamFilters.Add(filter);
                            break;
                    }
                }
                var orFilter = new LogicalOrFilter(elementParamFilters);

                var selectedElementIds = new List<ElementId>();
                foreach (var elem in selectedElements)
                {
                    selectedElementIds.Add(elem.Id);
                }

                var elementCollector = new FilteredElementCollector(m_doc, selectedElementIds);
                filteredElements = elementCollector.WherePasses(orFilter).ToElements().ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find empty parameters.\n" + ex.Message, "Form_Assigner:FindEmptyParameter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return filteredElements;
        }

        private void bttnSplit_Click(object sender, EventArgs e)
        {
            try
            {
                var massList = new List<MassProperties>();
                foreach (DataGridViewRow row in dataGridViewMass.Rows)
                {
                    var selected = Convert.ToBoolean(row.Cells[0].Value);
                    if (selected && null != row.Tag)
                    {
                        var mp = row.Tag as MassProperties;
                        massList.Add(mp);
                    }
                }

                if (massList.Count > 0)
                {
                    if (CheckOverlappingToSplit(massList))
                    {
                        toolStripStatusLabel.Text = "Spliting Elements . .";
                        toolStripProgressBar.Maximum = intersectingElements.Count;
                        toolStripProgressBar.Visible = true;
                        toolStripProgressBar.ProgressBar.Refresh();

                        if (intersectingElements.Count > 0)
                        {
                            var keys = intersectingElements.Keys.ToList();
                            var elementSpliter = new ElementSpliter(m_doc);
                            var errorString = new StringBuilder();

                            foreach (var elementId in keys)
                            {
                                toolStripProgressBar.ProgressBar.PerformStep();

                                var ep = intersectingElements[elementId];
                                if (ep.SelectedMassId == 0) { continue; }
                                if (!splitCategories.Contains(ep.CategoryName)) { continue; }
                                if (ep.OpverappingMaps.Count > 0)
                                {
                                    ep = elementSpliter.SplitElement(ep);
                                    if (ep.SplitSucceed)
                                    {
                                        intersectingElements.Remove(elementId);
                                        intersectingElements.Add(elementId, ep);
                                        splitDataManager.AppendINI(ep);
                                        elementSpliter.DeleteOriginalElement(ep);
                                    }
                                    else
                                    {
                                        errorString.AppendLine("[" + ep.ElementId + "] " + ep.ElementName);
                                    }
                                }
                                else
                                {
                                    errorString.AppendLine("[" + ep.ElementId + "] " + ep.ElementName);
                                }
                            }

                            splitDataManager.WriteINI();

                            toolStripStatusLabel.Text = "Completed.";
                            toolStripProgressBar.Visible = false;

                            if (errorString.Length > 0)
                            {
                                //logCreated = LogFileManager.CreateLogFile(m_doc, MassTool.MassCommands);
                                //LogFileManager.ClearLogFile();
                                //LogFileManager.AppendLog(DateTime.Now.ToString() + ": Started Spliting Elements");
                                //LogFileManager.AppendLog(errorString.ToString());
                                //if (logCreated) { LogFileManager.WriteLogFile(); }

                                var msgForm = new MessageBoxForm("Spliting Elements", "This tool was not able to split following elements: \n\n" + errorString.ToString(), LogFileManager.logFullName, true, true);
                                var dr = msgForm.ShowDialog();

                                //MessageBox.Show("This tool was not able to split following elements: \n\n" + errorString.ToString(), "Failure Message : Split Elements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                MessageBox.Show("Intersecting elements are successfully split by designated mass elements.", "Complete Split of Elements", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            this.DialogResult = DialogResult.OK;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select at least one mass to split elements. ", "No Mass Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to split elements.\n" + ex.Message, "Form_Assigner:bttnSplit_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<MassProperties> CountElements()
        {
            var massList = new List<MassProperties>();
            try
            {
                var elementCount = 0;
                foreach (DataGridViewRow row in dataGridViewMass.Rows)
                {
                    var selected = Convert.ToBoolean(row.Cells[0].Value);
                    var paramToTransfer = new List<Parameter>();
                    if (selected && null != row.Tag)
                    {
                        var mp = row.Tag as MassProperties;
                        if (mp.MassParameters.Count > 0)
                        {
                            foreach (var param in mp.MassParameters)
                            {
                                if (selectedParameters.ContainsKey(param.Definition.Name))
                                {
                                    paramToTransfer.Add(param);
                                }
                            }
                        }
                        if (paramToTransfer.Count > 0)
                        {
                            if (checkBoxFilter.Checked) { elementCount += mp.FilteredContainer.Count; }
                            else { elementCount += mp.ElementContainer.Count; }
                            massList.Add(mp);
                        }
                    }
                }

                if (elementCount > 0)
                {
                    toolStripStatusLabel.Text = "Writing Parameters . .";
                    toolStripProgressBar.Maximum = elementCount;
                    toolStripProgressBar.Visible = true;
                }
                return massList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to count the number of elements in the mass objects.\n" + ex.Message, "Form_Assigner:CountElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return massList;
            }
        }

        private bool CheckOverlappingToSplit(List<MassProperties> selectedMass)
        {
            var result = false;
            try
            {
                intersectingElements = new Dictionary<long, ElementProperties>();
                var intersectingCategories = new List<string>();
                var massIds = new List<long>();//massIds that will be displayed in the overapping form
                foreach (var elementId in elementDictionary.Keys)
                {
                    var ep = elementDictionary[elementId];
                    if (!ep.LinkedElement) { continue; }//elements in host model won't be split
                    if (!categoryNames.Contains(ep.CategoryName)) { continue; }
                    if (checkBoxFilter.Checked)
                    {
                        if (!selectedCategoryIds.Contains(ep.CategoryId)) { continue; }
                    }
                    if (null != ep.MassContainers)
                    {
                        if (ep.MassContainers.Count > 0)
                        {
                            foreach (var mp in selectedMass)
                            {
                                if (ep.MassContainers.ContainsKey(mp.MassId))
                                {
                                    intersectingElements.Add(ep.ElementId, ep);
                                    if (!intersectingCategories.Contains(ep.CategoryName)) { intersectingCategories.Add(ep.CategoryName); }

                                    foreach (var massId in ep.MassContainers.Keys)
                                    {
                                        if (!massIds.Contains(massId))
                                        {
                                            massIds.Add(massId);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (intersectingElements.Count > 0)
                {
                    var massdictionary = new Dictionary<long, MassProperties>();
                    foreach (DataGridViewRow row in dataGridViewMass.Rows)
                    {
                        if (null != row.Tag)
                        {
                            var mp = row.Tag as MassProperties;
                            if (!massdictionary.ContainsKey(mp.MassId))
                            {
                                massdictionary.Add(mp.MassId, mp);
                            }
                        }
                    }

                    var overlapForm = new Form_OverlapMass(m_app, true, intersectingElements, massdictionary, massIds, intersectingCategories);
                    if (ratio != 0)
                    {
                        overlapForm.SetDeterminant(ratio);
                        overlapForm.SetFollowByHost(followHost);
                    }

                    if (overlapForm.ShowDialog() == DialogResult.OK)
                    {
                        intersectingElements = new Dictionary<long, ElementProperties>();
                        unassignedElements = new Dictionary<long, ElementProperties>();
                        intersectingElements = overlapForm.IntersectingElements;
                        unassignedElements = overlapForm.UnassignedElements;
                        splitCategories = overlapForm.CategoriesToSplit;
                        overlapForm.Close();
                        if (intersectingElements.Count > 0)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        toolStripStatusLabel.Text = "Ready";
                        toolStripProgressBar.Visible = false;
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find overlapping elements.\n" + ex.Message, "Form_Assigner:CheckOverlapping", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return result;
            }
        }

        private bool CheckOverlappingToAssign(List<MassProperties> selectedMass)
        {
            var result = false;
            try
            {
                intersectingElements = new Dictionary<long, ElementProperties>();
                var intersectingCategories = new List<string>();
                var massIds = new List<long>();//massIds that will be displayed in the overapping form
                foreach (var elementId in elementDictionary.Keys)
                {
                    var ep = elementDictionary[elementId];
                    if (ep.LinkedElement) { continue; } //parameters in linked models cannot be assigned
                    if (!categoryNames.Contains(ep.CategoryName)) { continue; }
                    if (checkBoxFilter.Checked)
                    {
                        if (!selectedCategoryIds.Contains(ep.CategoryId)) { continue; }
                    }
                    if (null != ep.MassContainers)
                    {
                        if (ep.MassContainers.Count > 1)
                        {
                            foreach (var mp in selectedMass)
                            {
                                if (ep.MassContainers.ContainsKey(mp.MassId))
                                {
                                    intersectingElements.Add(ep.ElementId, ep);
                                    if (!intersectingCategories.Contains(ep.CategoryName)) { intersectingCategories.Add(ep.CategoryName); }

                                    foreach (var massId in ep.MassContainers.Keys)
                                    {
                                        if (!massIds.Contains(massId))
                                        {
                                            massIds.Add(massId);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (intersectingElements.Count > 0)
                {
                    var massdictionary = new Dictionary<long, MassProperties>();
                    foreach (DataGridViewRow row in dataGridViewMass.Rows)
                    {
                        if (null != row.Tag)
                        {
                            var mp = row.Tag as MassProperties;
                            if (!massdictionary.ContainsKey(mp.MassId))
                            {
                                massdictionary.Add(mp.MassId, mp);
                            }
                        }
                    }

                    var overlapForm = new Form_OverlapMass(m_app, false, intersectingElements, massdictionary, massIds, intersectingCategories);
                    if (ratio != 0)
                    {
                        overlapForm.SetDeterminant(ratio);
                        overlapForm.SetFollowByHost(followHost);
                    }

                    if (overlapForm.ShowDialog() == DialogResult.OK)
                    {
                        intersectingElements = new Dictionary<long, ElementProperties>();
                        unassignedElements = new Dictionary<long, ElementProperties>();
                        intersectingElements = overlapForm.IntersectingElements;
                        unassignedElements = overlapForm.UnassignedElements;
                        splitCategories = overlapForm.CategoriesToSplit;
                        overlapForm.Close();
                        result = true;
                    }
                    else
                    {
                        toolStripStatusLabel.Text = "Ready";
                        toolStripProgressBar.Visible = false;
                        result = false;
                    }
                }
                else
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find overlapping elements.\n" + ex.Message, "Form_Assigner:CheckOverlapping", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return result;
            }
        }

        private void dataGridViewMass_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex > -1 && e.ColumnIndex == dataGridViewMass.Columns["ColumnView"].Index)
                {
                    if (null != dataGridViewMass.Rows[e.RowIndex].Tag)
                    {
                        var mp = dataGridViewMass.Rows[e.RowIndex].Tag as MassProperties;
                        var uidoc = m_app.ActiveUIDocument;
                        Selection selection = uidoc.Selection;
                        List<Element> elementList = new List<Element>();
                        if (checkBoxFilter.Checked) { elementList = mp.FilteredContainer; }
                        else { elementList = mp.ElementContainer; }
                        List<ElementId> elementIds = new List<ElementId>();

                        foreach (Element element in elementList)
                        {
                            if (element.Document.IsLinked) { continue; }
                            elementIds.Add(element.Id);
                        }
                        uidoc.ShowElements(elementIds);
                        selection.SetElementIds(elementIds);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to show preview of elements.\n" + ex.Message, "Form_Assigner:dataGridViewMass_CellClick", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dataGridViewMass_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewMass.RowCount == dataGridViewParameter.RowCount)
            {
                for (var index = 0; index < dataGridViewMass.Rows.Count; index++)
                {
                    if (dataGridViewMass.Rows[index].Selected)
                    {
                        dataGridViewParameter.Rows[index].Selected = true;
                        break;
                    }
                }
            }
        }

        private void dataGridViewParameter_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewMass.RowCount == dataGridViewParameter.RowCount)
            {
                for (var index = 0; index < dataGridViewParameter.Rows.Count; index++)
                {
                    if (dataGridViewParameter.Rows[index].Selected)
                    {
                        dataGridViewMass.Rows[index].Selected = true;
                        break;
                    }
                }
            }
        }

        private void bttnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                var filterForm = new Form_ElementFilter(m_doc, integratedMassList, elementCategories, parameterMaps);
                if (DialogResult.OK == filterForm.ShowDialog())
                {
                    integratedMassList = filterForm.IntegratedMassDictionary;
                    elementCategories = filterForm.ElementCategories;
                    filterForm.Close();
                    checkBoxFilter.Checked = true;

                    DisplayMassList(selectedSource);

                    selectedCategoryIds = new List<long>();
                    foreach (var category in elementCategories.Keys)
                    {
                        if (elementCategories[category])
                        {
                            selectedCategoryIds.Add(GetElementIdValue(category.Id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize the element filter.\n" + ex.Message, "Form_Assigner:bttnFilter_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void checkBoxFilter_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                DisplayMassList(selectedSource);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The Checkbox for Enable Filter doesn't work correctly.\n" + ex.Message, "Form_Assigner:checkBoxFilter_CheckedChanged", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonSetting_Click(object sender, EventArgs e)
        {
            try
            {
                var formSettings = new Form_Settings(ratio, followHost);
                if (DialogResult.OK == formSettings.ShowDialog())
                {
                    ratio = formSettings.Ratio;
                    followHost = formSettings.FollowHost;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply settings.\n" + ex.Message, "Form_Assigner:buttonSetting_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var htmPath = @"V:\RVT-Data\HOK Program\Documentation\Mass Tool_Instruction.pdf";
                System.Diagnostics.Process.Start(htmPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open the help link.\n" + ex.Message, "Form_Assigner:linkHelp_LinkClicked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

    }
}
