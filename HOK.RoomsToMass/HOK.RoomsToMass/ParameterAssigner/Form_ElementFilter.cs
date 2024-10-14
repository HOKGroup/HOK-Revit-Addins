using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public partial class Form_ElementFilter : System.Windows.Forms.Form
    {
        private Document m_doc;
        private bool paramFilterActivated = false;
        private List<MassProperties> integratedMassList = new List<MassProperties>(); //mass from host projects
        private Dictionary<Category, bool> elementCategories = new Dictionary<Category, bool>();
        private Dictionary<long/*categoryId*/, Dictionary<long, ParameterProperties>> parameterMaps = new Dictionary<long, Dictionary<long, ParameterProperties>>();
        private Dictionary<long, ParameterProperties> selectedParameters = new Dictionary<long, ParameterProperties>();
        private Dictionary<long/*phaseId*/, Phase> phases = new Dictionary<long, Phase>();
        private Dictionary<int/*worksetId*/, Workset> worksets = new Dictionary<int, Workset>();

        public List<MassProperties> IntegratedMassDictionary { get { return integratedMassList; } set { integratedMassList = value; } }
        public Dictionary<Category, bool> ElementCategories { get { return elementCategories; } set { elementCategories = value; } }
        public Dictionary<long, Dictionary<long, ParameterProperties>> ParameterMaps { get { return parameterMaps; } set { parameterMaps = value; } }

        public Form_ElementFilter(Document document, List<MassProperties> integratedMass, Dictionary<Category, bool> categories, Dictionary<long, Dictionary<long, ParameterProperties>> paramMaps)
        {
            m_doc = document;
            integratedMassList = integratedMass;
            elementCategories = categories;
            parameterMaps = paramMaps;

            InitializeComponent();
            DisplayCategory();
            
            splitContainer1.Panel2Collapsed = true;
            CollectPhaseAndWorkset();
            this.Size = new Size(310, 500);
            this.MaximumSize = new Size(310, 500);
            radioButtonNone.Checked = true;
        }

        private void DisplayCategory()
        {
            try
            {
                foreach (Category category in elementCategories.Keys)
                {
                    ListViewItem item = new ListViewItem(category.Name);
                    item.Name = category.Name;
                    item.Tag = category;
                    item.Checked = elementCategories[category];
                    
                    listViewCategory.Items.Add(item);
                }
                listViewCategory.Sort();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void CollectPhaseAndWorkset()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Element> elements = collector.OfCategory(BuiltInCategory.OST_Phases).ToElements().ToList();
                foreach (Element element in elements)
                {
                    Phase ph = element as Phase;
                    if (null != ph)
                    {
                        if (!phases.ContainsKey(GetElementIdValue(ph.Id)))
                        {
                            phases.Add(GetElementIdValue(ph.Id), ph);
                        }
                    }
                }

                FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(m_doc).OfKind(WorksetKind.UserWorkset);
                foreach (Workset ws in worksetCollector)
                {
                    worksets.Add(ws.Id.IntegerValue, ws);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect information about phase and worksets.\n" + ex.Message, "Form_ElementFilter:CollectPhaseAndWorkset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayParameter()
        {
            try
            {
                cmbParamName1.DataSource = null;
                cmbParamName2.DataSource = null;

                cmbParamName1.Items.Clear();
                cmbOperation1.Items.Clear();
                cmbValue1.Items.Clear();
                cmbParamName2.Items.Clear();
                cmbOperation2.Items.Clear();
                cmbValue2.Items.Clear();

                selectedParameters = new Dictionary<long, ParameterProperties>();
                if (listViewCategory.CheckedItems.Count > 0)
                {
                    ListViewItem firstItem = listViewCategory.CheckedItems[0];
                    if (null != firstItem.Tag)
                    {
                        Category category = firstItem.Tag as Category;
                        if (parameterMaps.ContainsKey(GetElementIdValue(category.Id)))
                        {
                            selectedParameters = parameterMaps[GetElementIdValue(category.Id)];
                        }
                    }

                    if (listViewCategory.CheckedItems.Count > 1)
                    {
                        for (int i = 1; i < listViewCategory.CheckedItems.Count; i++)
                        {
                            ListViewItem item = listViewCategory.CheckedItems[i];
                            if (null != item.Tag)
                            {
                                Category category = item.Tag as Category;
                                if (parameterMaps.ContainsKey(GetElementIdValue(category.Id)))
                                {
                                    Dictionary<long, ParameterProperties> paramDictionary = new Dictionary<long, ParameterProperties>();
                                    paramDictionary = parameterMaps[GetElementIdValue(category.Id)];

                                    var commonDictionary = selectedParameters.Keys.Intersect(paramDictionary.Keys).ToDictionary(t => t, t => selectedParameters[t]);
                                    selectedParameters = commonDictionary.ToDictionary(x => x.Key, x => x.Value);

                                    //to fill out union of parameter values
                                    List<long> paramIds = selectedParameters.Keys.ToList();
                                    foreach (int paramId in paramIds)
                                    {
                                        ParameterProperties pp = selectedParameters[paramId];
                                        if (paramDictionary.ContainsKey(paramId))
                                        {
                                            ParameterProperties tempPP = paramDictionary[paramId];
                                            switch (pp.ParamStorageType)
                                            {
                                                case StorageType.ElementId:
                                                    pp.ElementIDValues = pp.ElementIDValues.Union(tempPP.ElementIDValues).ToList();
                                                    break;
                                                case StorageType.Double:
                                                    pp.DoubleValues = pp.DoubleValues.Union(tempPP.DoubleValues).ToList();
                                                    break;
                                                case StorageType.Integer:
                                                    pp.IntValues = pp.IntValues.Union(tempPP.IntValues).ToList();
                                                    break;
                                                case StorageType.String:
                                                    pp.StringValues = pp.StringValues.Union(tempPP.StringValues).ToList();
                                                    break;
                                            }
                                        }
                                        selectedParameters.Remove(paramId);
                                        selectedParameters.Add(pp.ParamId, pp);
                                    }
                                }
                            }
                        }
                    }

                    if (selectedParameters.Count > 0)
                    {
                        BindingList<ParameterProperties> paramProperties1 = new BindingList<ParameterProperties>();
                        BindingList<ParameterProperties> paramProperties2 = new BindingList<ParameterProperties>();

                        var sortedDictionary = (from entry in selectedParameters orderby entry.Value.ParamName ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                        foreach (int paramId in sortedDictionary.Keys)
                        {
                            ParameterProperties pp = sortedDictionary[paramId];
                            paramProperties1.Add(pp);
                            paramProperties2.Add(pp);
                        }

                        cmbParamName1.DataSource = paramProperties1;
                        cmbParamName1.ValueMember = "ParamId";
                        cmbParamName1.DisplayMember = "ParamName";

                        cmbParamName2.DataSource = paramProperties2;
                        cmbParamName2.ValueMember = "ParamId";
                        cmbParamName2.DisplayMember = "ParamName";

                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonFilter_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewCategory.CheckedItems.Count > 0)
                {
                    List<ElementFilter> elementCatFilters = new List<ElementFilter>();
                    elementCategories.Clear();
                    foreach (ListViewItem item in listViewCategory.Items)
                    {
                        if (null != item.Tag)
                        {
                            Category category = (Category)item.Tag;
                            if (item.Checked)
                            {
                                ElementCategoryFilter catFilter = new ElementCategoryFilter(category.Id);
                                elementCatFilters.Add(catFilter);
                                elementCategories.Add(category, true);
                            }
                            else
                            {
                                elementCategories.Add(category, false);
                            }
                        }
                    }
                    
                    LogicalOrFilter orFilter = new LogicalOrFilter(elementCatFilters);

                    for (int i = integratedMassList.Count - 1; i > -1; i--)
                    {
                        MassProperties mp = integratedMassList[i];
                        List<ElementId> elementIds = new List<ElementId>();
                        foreach (Element elem in mp.ElementContainer)
                        {
                            elementIds.Add(elem.Id);
                        }

                        if (elementIds.Count > 0)
                        {
                            List<Element> filteredElement=new List<Element>();
                            FilteredElementCollector collector = new FilteredElementCollector(m_doc, elementIds);

                            if (paramFilterActivated)
                            {
                                if (ValidateParamFilter())
                                {
                                    string paramValue = "";
                                    if (textBoxValue1.Visible) { paramValue = textBoxValue1.Text; }
                                    else if(!checkBoxEmpty1.Checked) { paramValue = cmbValue1.SelectedItem.ToString(); }
                                    ElementFilter paramFilter1 = CreateParamFilter((int)cmbParamName1.SelectedValue, cmbOperation1.SelectedItem.ToString(), paramValue, checkBoxEmpty1.Checked);
                                    
                                    List<ElementId> elementIdList = collector.WherePasses(orFilter).ToElementIds().ToList();
                                    collector = new FilteredElementCollector(m_doc, elementIdList);
                                    if (radioButtonAnd.Checked)
                                    {
                                        if (textBoxValue2.Visible) { paramValue = textBoxValue2.Text; }
                                        else if(!checkBoxEmpty2.Checked) { paramValue = cmbValue2.SelectedItem.ToString(); }
                                        ElementFilter paramFilter2 = CreateParamFilter((int)cmbParamName2.SelectedValue, cmbOperation2.SelectedItem.ToString(), paramValue, checkBoxEmpty2.Checked);
                                        if (null != paramFilter1 && null != paramFilter2)
                                        {
                                            LogicalAndFilter andFilter = new LogicalAndFilter(paramFilter1, paramFilter2);
                                            filteredElement = collector.WherePasses(andFilter).ToElements().ToList();
                                        }
                                    }
                                    else if (radioButtonOr.Checked)
                                    {
                                        if (textBoxValue2.Visible) { paramValue = textBoxValue2.Text; }
                                        else if(!checkBoxEmpty2.Checked) { paramValue = cmbValue2.SelectedItem.ToString(); }
                                        ElementFilter paramFilter2 = CreateParamFilter((int)cmbParamName2.SelectedValue, cmbOperation2.SelectedItem.ToString(), paramValue, checkBoxEmpty2.Checked);
                                        if (null!=paramFilter1 && null != paramFilter2)
                                        {
                                            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(paramFilter1, paramFilter2);
                                            filteredElement = collector.WherePasses(logicalOrFilter).ToElements().ToList();
                                        }
                                    }
                                    else if (radioButtonNone.Checked)
                                    {
                                        if (null != paramFilter1)
                                        {
                                            filteredElement = collector.WhereElementIsNotElementType().WherePasses(paramFilter1).ToElements().ToList();
                                        }
                                    }
                                    mp.FilteredContainer = filteredElement;
                                    integratedMassList.RemoveAt(i);
                                    integratedMassList.Add(mp);
                                    this.DialogResult = DialogResult.OK;
                                }
                            }
                            else
                            {
                                filteredElement = collector.WherePasses(orFilter).ToElements().ToList();
                                mp.FilteredContainer = filteredElement;
                                integratedMassList.RemoveAt(i);
                                integratedMassList.Add(mp);
                                this.DialogResult = DialogResult.OK;
                            }

                            
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void buttonAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewCategory.Items)
            {
                item.Checked = true;
            }
        }

        private void buttonNone_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewCategory.Items)
            {
                item.Checked = false;
            }
        }

        private void buttonInvert_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewCategory.Items)
            {
                if (item.Checked)
                {
                    item.Checked = false;
                }
                else
                {
                    item.Checked = true;
                }
            }
        }

        private void buttonAdvance_Click(object sender, EventArgs e)
        {
            try
            {
                if (paramFilterActivated)
                {
                    paramFilterActivated = false;
                    splitContainer1.Panel2Collapsed = true;
                    buttonAdvance.Text = ">> Advance";
                    this.Size = new Size(310, 500);
                    this.MaximumSize = new Size(310, 500);
                    this.MinimumSize = new Size(310, 500);
                }
                else
                {
                    paramFilterActivated = true;
                    splitContainer1.Panel2Collapsed = false;
                    buttonAdvance.Text = "<< Default";
                    this.Size = new Size(600, 500);
                    this.MaximumSize = new Size(600, 500);
                    this.MinimumSize = new Size(600, 500);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void listViewCategory_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            DisplayParameter();
        }

        private void cmbParamName1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cmbOperation1.Items.Clear();
                cmbOperation1.ResetText();
                cmbValue1.Items.Clear();
                cmbValue1.ResetText();

                if (cmbParamName1.SelectedIndex > -1)
                {
                    int paramId = (int)cmbParamName1.SelectedValue;
                    if (selectedParameters.ContainsKey(paramId))
                    {
                        ParameterProperties pp = selectedParameters[paramId];
                        switch (pp.ParamStorageType)
                        {
                            case StorageType.ElementId:
                                cmbOperation1.Items.Add("equals");
                                cmbOperation1.Items.Add("does not equal");

                                if (pp.ParamName == "Phase Created" || pp.ParamName == "Phase Demolished")
                                {
                                    FindPhase(pp, true);
                                }
                                break;
                            case StorageType.Double:
                                cmbOperation1.Items.Add("equals");
                                cmbOperation1.Items.Add("does not equal");
                                cmbOperation1.Items.Add("is greater than");
                                cmbOperation1.Items.Add("is greater than or equal to");
                                cmbOperation1.Items.Add("is less than");
                                cmbOperation1.Items.Add("is less than or equal to");

                                foreach (double dbl in pp.DoubleValues)
                                {
                                    cmbValue1.Items.Add(dbl);
                                }
                                break;
                            case StorageType.Integer:
                                cmbOperation1.Items.Add("equals");
                                cmbOperation1.Items.Add("does not equal");
                                cmbOperation1.Items.Add("is greater than");
                                cmbOperation1.Items.Add("is greater than or equal to");
                                cmbOperation1.Items.Add("is less than");
                                cmbOperation1.Items.Add("is less than or equal to");

                                if (pp.ParamName == "Workset")
                                {
                                    FindWorkset(pp, true);
                                }
                                else
                                {
                                    foreach (int intVal in pp.IntValues)
                                    {
                                        cmbValue1.Items.Add(intVal);
                                    }
                                }

                                break;
                            case StorageType.String:
                                cmbOperation1.Items.Add("equals");
                                cmbOperation1.Items.Add("does not equal");
                                cmbOperation1.Items.Add("is greater than");
                                cmbOperation1.Items.Add("is greater than or equal to");
                                cmbOperation1.Items.Add("is less than");
                                cmbOperation1.Items.Add("is less than or equal to");
                                cmbOperation1.Items.Add("contains");
                                cmbOperation1.Items.Add("does not contain");
                                cmbOperation1.Items.Add("begins with");
                                cmbOperation1.Items.Add("does not begin with");
                                cmbOperation1.Items.Add("ends with");
                                cmbOperation1.Items.Add("does not end with");

                                foreach (string strVal in pp.StringValues)
                                {
                                    cmbValue1.Items.Add(strVal);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void cmbParamName2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cmbOperation2.Items.Clear();
                cmbOperation2.ResetText();
                cmbValue2.Items.Clear();
                cmbValue2.ResetText();

                if (null != cmbParamName2.SelectedValue)
                {
                    int paramId = (int)cmbParamName2.SelectedValue;
                    if (selectedParameters.ContainsKey(paramId))
                    {
                        ParameterProperties pp = selectedParameters[paramId];
                        switch (pp.ParamStorageType)
                        {
                            case StorageType.ElementId:
                                cmbOperation2.Items.Add("equals");
                                cmbOperation2.Items.Add("does not equal");
                                cmbOperation2.Items.Add("is greater than");
                                cmbOperation2.Items.Add("is greater than or equal to");
                                cmbOperation2.Items.Add("is less than");
                                cmbOperation2.Items.Add("is less than or equal to");
                                cmbOperation2.Items.Add("contains");
                                cmbOperation2.Items.Add("does not contain");
                                cmbOperation2.Items.Add("begins with");
                                cmbOperation2.Items.Add("does not begin with");
                                cmbOperation2.Items.Add("ends with");
                                cmbOperation2.Items.Add("does not end with");

                                if (pp.ParamName == "Phase Created" || pp.ParamName == "Phase Demolished")
                                {
                                    FindPhase(pp, false);
                                }
                                break;
                            case StorageType.Double:
                                cmbOperation2.Items.Add("equals");
                                cmbOperation2.Items.Add("does not equal");
                                cmbOperation2.Items.Add("is greater than");
                                cmbOperation2.Items.Add("is greater than or equal to");
                                cmbOperation2.Items.Add("is less than");
                                cmbOperation2.Items.Add("is less than or equal to");

                                foreach (double dbl in pp.DoubleValues)
                                {
                                    cmbValue2.Items.Add(dbl);
                                }
                                break;
                            case StorageType.Integer:
                                cmbOperation2.Items.Add("equals");
                                cmbOperation2.Items.Add("does not equal");
                                cmbOperation2.Items.Add("is greater than");
                                cmbOperation2.Items.Add("is greater than or equal to");
                                cmbOperation2.Items.Add("is less than");
                                cmbOperation2.Items.Add("is less than or equal to");

                                if (pp.ParamName == "Workset")
                                {
                                    FindWorkset(pp, false);
                                }
                                else
                                {
                                    foreach (int intVal in pp.IntValues)
                                    {
                                        cmbValue2.Items.Add(intVal);
                                    }
                                }

                                break;
                            case StorageType.String:
                                cmbOperation2.Items.Add("equals");
                                cmbOperation2.Items.Add("does not equal");
                                cmbOperation2.Items.Add("is greater than");
                                cmbOperation2.Items.Add("is greater than or equal to");
                                cmbOperation2.Items.Add("is less than");
                                cmbOperation2.Items.Add("is less than or equal to");
                                cmbOperation2.Items.Add("contains");
                                cmbOperation2.Items.Add("does not contain");
                                cmbOperation2.Items.Add("begins with");
                                cmbOperation2.Items.Add("does not begin with");
                                cmbOperation2.Items.Add("ends with");
                                cmbOperation2.Items.Add("does not end with");

                                foreach (string strVal in pp.StringValues)
                                {
                                    cmbValue2.Items.Add(strVal);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void FindWorkset(ParameterProperties pp, bool isFirstParam)
        {
            try
            {
                foreach (int worksetId in pp.IntValues)
                {
                    if (worksets.ContainsKey(worksetId))
                    {
                        Workset workset = worksets[worksetId];
                        if (isFirstParam) { cmbValue1.Items.Add(workset.Name); }
                        else { cmbValue2.Items.Add(workset.Name); }
                    }
                }
            }
            catch(Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void FindPhase(ParameterProperties pp, bool isFirstParam)
        {
            try
            {
                foreach (ElementId eId in pp.ElementIDValues)
                {
                    if (phases.ContainsKey(GetElementIdValue(eId)))
                    {
                        Phase phase = phases[GetElementIdValue(eId)];
                        if (isFirstParam) { cmbValue1.Items.Add(phase.Name); }
                        else { cmbValue2.Items.Add(phase.Name); }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void cmbOperation1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string[] textboxString = new string[] { "contains", "does not contain", "begins with", "does not begin with", "ends with", "does not end with" };
                if (textboxString.Contains(cmbOperation1.SelectedItem.ToString()))
                {
                    cmbValue1.Visible = false;
                    textBoxValue1.Visible = true;
                }
                else
                {
                    cmbValue1.Visible = true;
                    textBoxValue1.Visible = false;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void cmbOperation2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string[] textboxString = new string[] { "contains", "does not contain", "begins with", "does not begin with", "ends with", "does not end with" };
                if (textboxString.Contains(cmbOperation2.SelectedItem.ToString()))
                {
                    cmbValue2.Visible = false;
                    textBoxValue2.Visible = true;
                }
                else
                {
                    cmbValue2.Visible = true;
                    textBoxValue2.Visible = false;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private ElementFilter CreateParamFilter(int paramId, string strOperator, string paramValue, bool emptyValue)
        {
            try
            {
                ParameterProperties pp = selectedParameters[paramId];
                ParameterValueProvider pvp = new ParameterValueProvider(NewElementId(pp.ParamId));

                switch (pp.ParamStorageType)
                {
                    case StorageType.ElementId:
                        if (pp.ParamName == "Phase Created" || pp.ParamName == "Phase Demolished")
                        {
                            FilterNumericRuleEvaluator fnre1 = new FilterNumericEquals();

                            var phaseId = from phase in phases where phase.Value.Name == paramValue select phase.Key;
                            long pId = phaseId.First();
                            ElementId ruleValId = NewElementId(pId);

                            FilterRule filterRule1 = new FilterElementIdRule(pvp, fnre1, ruleValId);

                            if (strOperator == "does not equal")
                            {
                                return new ElementParameterFilter(filterRule1, true);
                            }
                            else
                            {
                                return new ElementParameterFilter(filterRule1);
                            }
                        }
                        return null;
 
                    case StorageType.Double:
                        if (emptyValue)
                        {
                            FilterNumericRuleEvaluator fnre = new FilterNumericGreater();
                            FilterDoubleRule doubleRule = new FilterDoubleRule(pvp, fnre, 0, double.Epsilon);
                            return new ElementParameterFilter(doubleRule, true);
                        }

                        FilterNumericRuleEvaluator fnre2 = FindFilterNumericRuleEvaluator(strOperator);
                        double dblValue = 0;
                        if (!string.IsNullOrEmpty(paramValue))
                        {
                            double.TryParse(paramValue, out dblValue);
                        }
                       
                        FilterRule filterRule2 = new FilterDoubleRule(pvp, fnre2, dblValue, double.Epsilon);

                        if (strOperator == "does not equal")
                        {
                            return new ElementParameterFilter(filterRule2, true);
                        }
                        else
                        {
                            return new ElementParameterFilter(filterRule2);
                        }
                    
                    case StorageType.Integer:
                        if (emptyValue)
                        {
                            FilterNumericRuleEvaluator fnre = new FilterNumericGreater();
                            FilterIntegerRule integerRule = new FilterIntegerRule(pvp, fnre, 0);
                            return new ElementParameterFilter(integerRule, true);
                        }
                        FilterNumericRuleEvaluator fnre3 = FindFilterNumericRuleEvaluator(strOperator);

                        int intValue = 0;
                        if (pp.ParamName == "Workset")
                        {
                            var worksetId = from ws in worksets where ws.Value.Name == paramValue select ws.Key;
                            intValue = worksetId.First();
                        }
                        else if (!string.IsNullOrEmpty(paramValue))
                        {
                            int.TryParse(paramValue, out intValue);
                        }

                        FilterRule filterRule3 = new FilterIntegerRule(pvp, fnre3, intValue);
   
                        if (strOperator == "does not equal")
                        {
                            return new ElementParameterFilter(filterRule3, true);
                        }
                        else
                        {
                            return new ElementParameterFilter(filterRule3);
                        }
                  
                    case StorageType.String:
                        if (emptyValue)
                        {
                            FilterStringRuleEvaluator fsre = new FilterStringGreater();
#if REVIT2022_OR_GREATER
                            FilterStringRule stringRule = new FilterStringRule(pvp, fsre, "");
#else
                            FilterStringRule stringRule = new FilterStringRule(pvp, fsre, "", false);
#endif
                            return new ElementParameterFilter(stringRule, true);

                        }

                        FilterStringRuleEvaluator fnre4 = FindFilterStringRuleEvaluator(strOperator);
                        string strValue = paramValue;
#if REVIT2022_OR_GREATER
                        FilterStringRule filterRule4 = new FilterStringRule(pvp, fnre4, strValue);
#else
                        FilterStringRule filterRule4 = new FilterStringRule(pvp, fnre4, strValue, false);
#endif

                        if (strOperator.Contains("does not"))
                        {
                            return new ElementParameterFilter(filterRule4, true);
                        }
                        else
                        {
                            return new ElementParameterFilter(filterRule4, false);
                        }
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create parameter filters.\n" + ex.Message, "CreateParamFilter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return null;
        }

        private FilterNumericRuleEvaluator FindFilterNumericRuleEvaluator(string strOperator)
        {
            switch(strOperator)
            {
                case "equals":
                    return new FilterNumericEquals();
                case "does not equal":
                    return new FilterNumericEquals();
                case "is greater than":
                    return new FilterNumericGreater();
                case "is greater than or equal to":
                    return new FilterNumericGreaterOrEqual();
                case "is less than":
                    return new FilterNumericLess();
                case "is less than or equal to":
                    return new FilterNumericLessOrEqual();
                default:
                    return new FilterNumericEquals();
            }
        }

        private FilterStringRuleEvaluator FindFilterStringRuleEvaluator(string strOperator)
        {
            switch (strOperator)
            {
                case "equals":
                   return new FilterStringEquals();
                case "does not equal":
                    return new FilterStringEquals();
                case "is greater than":
                    return new FilterStringGreater();
                case "is greater than or equal to":
                    return new FilterStringGreaterOrEqual();
                case "is less than":
                    return new FilterStringLess();
                case "is less than or equal to":
                    return new FilterStringLessOrEqual();
                case "contains":
                    return new FilterStringContains();
                case "does not contain":
                    return new FilterStringContains();
                case "begins with":
                    return new FilterStringBeginsWith();
                case "does not begin with":
                    return new FilterStringBeginsWith();
                case "ends with":
                    return new FilterStringEndsWith();
                case "does not end with":
                    return new FilterStringEndsWith();
                default:
                    return new FilterStringEquals();
            }
        }

        private bool ValidateParamFilter()
        {
            try
            {
                if (cmbParamName1.SelectedIndex > 0)
                {
                    if (cmbOperation1.SelectedIndex < 0)
                    {
                        MessageBox.Show("Select an operation type for Parameter 1.", "Select an Operation Type", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }

                    if (!checkBoxEmpty1.Checked && cmbValue1.SelectedIndex < 0 && string.IsNullOrEmpty(textBoxValue1.Text))
                    {
                        MessageBox.Show("Enter a parameter value for Parameter 1", "Enter a Value", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Seletect a parameter name from the drop-down list: Parameter 1", "Select a Parameter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (radioButtonAnd.Checked || radioButtonAnd.Checked)
                {
                    if (cmbParamName2.SelectedIndex > 0)
                    {
                        if (cmbOperation2.SelectedIndex < 0)
                        {
                            MessageBox.Show("Select an operation type for Parameter 2.", "Select an Operation Type", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }

                        if (!checkBoxEmpty2.Checked && cmbValue2.SelectedIndex < 0 && string.IsNullOrEmpty(textBoxValue2.Text))
                        {
                            MessageBox.Show("Enter a parameter value for Parameter 2", "Enter a Value", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Seletect a parameter name from the drop-down list: Parameter 2", "Select a Parameter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to validate the input of parameter filter.\n" + ex.Message, "ValidateParamFilter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return true;
        }

        private void radioButtonNone_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonNone.Checked)
            {
                cmbParamName2.ResetText();
                cmbParamName2.Enabled = false;
                cmbOperation2.ResetText();
                cmbOperation2.Enabled = false;
                cmbValue2.ResetText();
                cmbValue2.Enabled = false;
                textBoxValue2.Text = "";
                textBoxValue2.Enabled = false;
                checkBoxEmpty2.Enabled = false;
            }
            else
            {
                cmbParamName2.Enabled = true;
                cmbOperation2.Enabled = true;
                cmbValue2.Enabled = true;
                textBoxValue2.Enabled = true;
                checkBoxEmpty2.Enabled = true;
            }
        }


    }
}
