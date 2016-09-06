using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Autodesk.Revit.DB;
using System.Collections.ObjectModel;
using Autodesk.Revit.UI;
using HOK.ColorSchemeEditor.BCFUtils;
using HOK.ColorSchemeEditor.WPFClasses;

namespace HOK.ColorSchemeEditor
{


    /// <summary>
    /// Interaction logic for CategoryWindow.xaml
    /// </summary>
    public partial class CategoryWindow : Window
    {
        private UIApplication m_app;
        private Document m_doc;
        private ColorScheme selectedColorScheme = null;
        private List<string> selectedCategories = new List<string>();
        private List<CategoryInfo> categoryInfoList=new List<CategoryInfo>();
        private List<CriterionInfo> criteriaInfoList = new List<CriterionInfo>();
        private List<Category> userSelectedList = new List<Category>();
        private bool filterEnabled = false;
        private List<Element> selectedElements = new List<Element>();
        private List<FilterRule> selectedRules = new List<FilterRule>();
        private Dictionary<int, ParameterInfo> paramInfoDictionary = new Dictionary<int, ParameterInfo>();
        private Dictionary<string/*doc title*/, Document> linkedFiles = new Dictionary<string, Document>();
        private bool includeLinks = false;

        public ColorScheme SelectedColorScheme { get { return selectedColorScheme; } set { selectedColorScheme = value; } }
        public List<CategoryInfo> CategoryInfoList { get { return categoryInfoList; } set { categoryInfoList = value; } }
        public List<Category> UserSelectedList { get { return userSelectedList; } set { userSelectedList = value; } }
        public List<Element> SelectedElements { get { return selectedElements; } set { selectedElements = value; } }
        public List<FilterRule> SelectedRules { get { return selectedRules; } set { selectedRules = value; } }
        public Dictionary<int, ParameterInfo> ParamInfoDictionary { get { return paramInfoDictionary; } set { paramInfoDictionary = value; } }
        public bool IncludeLinks { get { return includeLinks; } set { includeLinks = value; } }
       
        public CategoryWindow(UIApplication uiapp)
        {
            try
            {
                m_app = uiapp;
                m_doc = m_app.ActiveUIDocument.Document;

                GetLinkedFiles();

                Categories categories = m_doc.Settings.Categories;
                
                List<ElementId> filterCatIds = ParameterFilterUtilities.GetAllFilterableCategories().ToList();
                foreach (ElementId id in filterCatIds)
                {
                    try
                    {
                        BuiltInCategory bltCat = (BuiltInCategory)id.IntegerValue;
                        Category category = categories.get_Item(bltCat);
                        if (null != category)
                        {
                            if (null != category.Parent) { continue; } //skip subcategories
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                            if (category.CategoryType != CategoryType.Model) { continue; }
#endif
                            CategoryInfo catInfo = new CategoryInfo(category, filterCatIds);
                            categoryInfoList.Add(catInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }
                
                categoryInfoList = categoryInfoList.OrderBy(o => o.Name).ToList();

                InitializeComponent();

                this.MinWidth = 335;
                this.Width = 335;
                this.MaxWidth = 335;
                treeViewCategory.ItemsSource = null;
                treeViewCategory.ItemsSource = TreeViewModel.SetTreeView(categoryInfoList, new List<string>());

                Array enumArray=Enum.GetValues(typeof(CriteriaName));
                foreach (CriteriaName criteria in enumArray)
                {
                    CriterionInfo criInfo = new CriterionInfo(criteria);
                    criteriaInfoList.Add(criInfo);
                }
                criteriaInfoList = criteriaInfoList.OrderBy(o => o.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to display Category Window.\n"+ex.Message, "Category Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public CategoryWindow(UIApplication uiapp, ColorScheme colorScheme)
        {
            try
            {
                m_app = uiapp;
                m_doc = m_app.ActiveUIDocument.Document;

                GetLinkedFiles();

                selectedColorScheme = colorScheme;
                selectedElements = colorScheme.FilteredElements;
                selectedRules = colorScheme.FilterRules;
                includeLinks = colorScheme.IncludeLinks;

                Categories categories = m_doc.Settings.Categories;

                List<ElementId> filterCatIds = ParameterFilterUtilities.GetAllFilterableCategories().ToList();
                foreach (ElementId id in filterCatIds)
                {
                    try
                    {
                        BuiltInCategory bltCat = (BuiltInCategory)id.IntegerValue;
                        Category category = categories.get_Item(bltCat);
                        if (null != category)
                        {
                            if (null != category.Parent) { continue; } //skip subcategories
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                            if (category.CategoryType != CategoryType.Model) { continue; }
#endif

                            CategoryInfo catInfo = new CategoryInfo(category, filterCatIds);
                            categoryInfoList.Add(catInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }

                categoryInfoList = categoryInfoList.OrderBy(o => o.Name).ToList();

                InitializeComponent();

                treeViewCategory.ItemsSource = null;
                treeViewCategory.ItemsSource = TreeViewModel.SetTreeView(categoryInfoList, selectedColorScheme.Categories);

                Array enumArray = Enum.GetValues(typeof(CriteriaName));
                foreach (CriteriaName criteria in enumArray)
                {
                    CriterionInfo criInfo = new CriterionInfo(criteria);
                    criteriaInfoList.Add(criInfo);
                }
                criteriaInfoList = criteriaInfoList.OrderBy(o => o.DisplayName).ToList();
                checkBoxLink.IsChecked = includeLinks;

                ClearUIComponents();
                DisplayParameters();

                if (selectedRules.Count > 0)
                {
                    filterEnabled = true;
                    this.MinWidth = 660;
                    this.Width = 660;
                    this.MaxWidth = 660;
                    buttonParameter.Content = "Disable Filters";
                    DisplayFilterRule(selectedRules);
                }
                else
                {
                    filterEnabled = false;
                    this.MinWidth = 335;
                    this.Width = 335;
                    this.MaxWidth = 335;
                    buttonParameter.Content = "Enable Filters";
                }

               
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to display Category Window.\n" + ex.Message, "Category Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetLinkedFiles()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<RevitLinkInstance> linkInstances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements().Cast<RevitLinkInstance>().ToList();

                foreach (RevitLinkInstance instance in linkInstances)
                {
                    ElementId typeId = instance.GetTypeId();
                    RevitLinkType linkType = m_doc.GetElement(typeId) as RevitLinkType;
                    string linkTypeName = linkType.Name;

                    foreach (Document document in m_app.Application.Documents)
                    {
                        if (!string.IsNullOrEmpty(document.Title))
                        {
                            if (linkTypeName.Contains(document.Title))
                            {
                                if (!linkedFiles.ContainsKey(document.Title))
                                {
                                    linkedFiles.Add(document.Title, document);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get linked files.\n"+ex.Message, "Get Linked Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayFilterRule(List<FilterRule> filterRules)
        {
            try
            {
                filterEnabled = true;
                this.MinWidth = 660;
                this.Width = 660;
                this.MaxWidth = 660;
                buttonParameter.Content = "Disable Filters";

                switch (filterRules.Count)
                {
                    case 1:
                        FilterRule rule1 = filterRules[0];
                        SelectFilterRule(rule1, comboBoxFilterBy1, comboBoxCriteria1, comboBoxValue1);
                        break;
                    case 2:
                        FilterRule rule2 = filterRules[1];
                        SelectFilterRule(rule2, comboBoxFilterBy2, comboBoxCriteria2, comboBoxValue2);
                        break;
                    case 3:
                        FilterRule rule3 = filterRules[2];
                        SelectFilterRule(rule3, comboBoxFilterBy3, comboBoxCriteria3, comboBoxValue3);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to display filter rules.\n" + ex.Message, "Display Filter Rules", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectFilterRule(FilterRule rule, System.Windows.Controls.ComboBox cmbFilter, System.Windows.Controls.ComboBox cmbCriteria, System.Windows.Controls.ComboBox cmbValue)
        {
            try
            {
                for (int i = 0; i < cmbFilter.Items.Count; i++)
                {
                    ParameterInfo paramInfo = cmbFilter.Items[i] as ParameterInfo;
                    if (paramInfo.ParamId.IntegerValue == rule.ParameterId)
                    {
                        cmbFilter.SelectedIndex = i;
                    }
                }

                for (int i = 0; i < cmbCriteria.Items.Count; i++)
                {
                    CriterionInfo criInfo = cmbCriteria.Items[i] as CriterionInfo;
                    if (criInfo.CriterionName == rule.CriteriaName)
                    {
                        cmbCriteria.SelectedIndex = i;
                    }
                }

                if (rule.ParameterStorageType == ParameterStorageType.ElementId)
                {
                    for (int i = 0; i < cmbValue.Items.Count; i++)
                    {
                        ParameterValueInfo valueInfo = cmbValue.Items[i] as ParameterValueInfo;
                        if (valueInfo.IdValue.IntegerValue.ToString() == rule.RuleValue)
                        {
                            cmbValue.SelectedIndex = i;
                        }
                    }
                }
                else if (rule.ParameterStorageType == ParameterStorageType.Double)
                {
                    for (int i = 0; i < cmbValue.Items.Count; i++)
                    {
                        ParameterValueInfo valueInfo = cmbValue.Items[i] as ParameterValueInfo;
                        if(valueInfo.DblValue.ToString()==rule.RuleValue)
                        {
                            cmbValue.SelectedIndex = i;
                        }
                    }
                }
                else
                {
                    cmbValue.Text = rule.RuleValue;
                }
                
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to select filter rules.\n" + ex.Message, "Select Filter Rule", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<Element> GetFilteredElements(out List<FilterRule> rules)
        {
            rules = new List<FilterRule>();
            List<Element> filteredElements = new List<Element>();
            try
            {
                if (null != comboBoxFilterBy1.SelectedItem && null != comboBoxCriteria1.SelectedItem && !string.IsNullOrEmpty(comboBoxValue1.Text))
                {
                    var eIds = from element in selectedElements select element.Id;
                    List<ElementId> elementIds = new List<ElementId>();
                    elementIds = eIds.ToList();

                    FilteredElementCollector elementCollector = new FilteredElementCollector(m_doc, elementIds);
                    
                    List<Autodesk.Revit.DB.FilterRule> filterRules=new List<Autodesk.Revit.DB.FilterRule>();
                    FilterRule rule1 = new FilterRule();
                    Autodesk.Revit.DB.FilterRule filterRule1 = GetFilterRule(comboBoxFilterBy1, comboBoxCriteria1, comboBoxValue1, out rule1);
                    if (null != filterRule1)
                    {
                        filterRules.Add(filterRule1);
                        rules.Add(rule1);
                    }

                    FilterRule rule2 = new FilterRule();
                    if (null != comboBoxFilterBy2.SelectedItem && null != comboBoxCriteria2.SelectedItem && !string.IsNullOrEmpty(comboBoxValue2.Text))
                    {
                        Autodesk.Revit.DB.FilterRule filterRule2 = GetFilterRule(comboBoxFilterBy2, comboBoxCriteria2, comboBoxValue2, out rule2);
                        if (null != filterRule2)
                        {
                            filterRules.Add(filterRule2);
                            rules.Add(rule2);
                        }
                    }

                    FilterRule rule3 = new FilterRule();
                    if (null != comboBoxFilterBy3.SelectedItem && null != comboBoxCriteria3.SelectedItem && !string.IsNullOrEmpty(comboBoxValue3.Text))
                    {
                        Autodesk.Revit.DB.FilterRule filterRule3 = GetFilterRule(comboBoxFilterBy3, comboBoxCriteria3, comboBoxValue3, out rule3);
                        if (null != filterRule3)
                        {
                            filterRules.Add(filterRule3);
                            rules.Add(rule3);
                        }
                    }

                    ElementParameterFilter paramFilter=new ElementParameterFilter(filterRules);
                    filteredElements = elementCollector.WherePasses(paramFilter).ToElements().ToList();
                }
                else
                {
                    filteredElements = selectedElements;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to get filtered elements.\n" + ex.Message, "Get Filtered Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return filteredElements;
        }

        private Autodesk.Revit.DB.FilterRule GetFilterRule(System.Windows.Controls.ComboBox paramComboBox, System.Windows.Controls.ComboBox criteriaComboBox, System.Windows.Controls.ComboBox valueComboBox, out FilterRule rule)
        {
            rule = new FilterRule();
            Autodesk.Revit.DB.FilterRule filterRule = null;
            try
            {
                if (null != paramComboBox.SelectedItem && null != criteriaComboBox.SelectedItem && !string.IsNullOrEmpty(valueComboBox.Text))
                {
                    ParameterInfo paramInfo = paramComboBox.SelectedItem as ParameterInfo;
                    ElementId paramId = paramInfo.ParamId;
                    CriterionInfo criterionInfo = criteriaComboBox.SelectedItem as CriterionInfo;

                    string strValue = valueComboBox.Text;
                    rule.RuleValue = strValue;
                    ParameterValueInfo valueInfo = null;
                    if (null != valueComboBox.SelectedItem)
                    {
                        valueInfo = valueComboBox.SelectedItem as ParameterValueInfo;
                    }

                    rule.ParameterId = paramInfo.ParamId.IntegerValue;
                    rule.CriteriaName = criterionInfo.CriterionName; 

                    switch (paramInfo.ParamStorageType)
                    {
                        case StorageType.Double:
                            rule.ParameterStorageType = ParameterStorageType.Double;
                            double dblValue = 0;
                            double.TryParse(rule.RuleValue, out dblValue);
                            if (null != valueInfo)
                            {
                                if (rule.RuleValue == valueInfo.StrValue)
                                {
                                    dblValue = valueInfo.DblValue;
                                    rule.RuleValue = valueInfo.DblValue.ToString();
                                }
                            }

                            filterRule = RevitUtil.GetDoubleRule(paramId, criterionInfo.CriterionName, dblValue);
                            break;
                        case StorageType.Integer:
                            rule.ParameterStorageType = ParameterStorageType.Integer;
                            int intValue = 0;
                            if (int.TryParse(strValue, out intValue))
                            {
                                filterRule = RevitUtil.GetIntegerRule(paramId, criterionInfo.CriterionName, intValue);
                            }
                            break;
                        case StorageType.String:
                            rule.ParameterStorageType = ParameterStorageType.String;
                            filterRule = RevitUtil.GetStringRule(paramId, criterionInfo.CriterionName, strValue);
                            break;
                        case StorageType.ElementId:
                            rule.ParameterStorageType = ParameterStorageType.ElementId;
                            rule.RuleValue = valueInfo.IdValue.IntegerValue.ToString();
                            filterRule = RevitUtil.GetElementIdRule(paramId, criterionInfo.CriterionName, valueInfo.IdValue);
                            break;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to get filter rule.\n" + ex.Message, "Get Filter Rule", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return filterRule;
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TreeViewModel> treeviewModels = treeViewCategory.ItemsSource as List<TreeViewModel>;
                foreach (TreeViewModel node in treeviewModels)
                {
                    if (null != node._parent) { continue; }
                    node.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to check all categories.\n"+ex.Message, "Check All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TreeViewModel> treeviewModels = treeViewCategory.ItemsSource as List<TreeViewModel>;
                foreach (TreeViewModel node in treeviewModels)
                {
                    if (null != node._parent) { continue; }
                    node.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to uncheck all categories.\n"+ex.Message, "Uncheck All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxHide_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)checkBoxHide.IsChecked)
                {
                    List<TreeViewModel> categoryView = treeViewCategory.ItemsSource as List<TreeViewModel>;
                    var selectedItems = from node in categoryView where node.IsChecked == true select node;
                    List<TreeViewModel> tempView = selectedItems.ToList();

                    treeViewCategory.ItemsSource = null;
                    treeViewCategory.ItemsSource = tempView;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to hide un-checked categories.\n"+ex.Message, "Hide Un-checked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxHide_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (checkBoxHide.IsChecked == false)
                {
                    List<TreeViewModel> currentView = treeViewCategory.ItemsSource as List<TreeViewModel>;
                    List<string> selectedCategories = new List<string>();
                    foreach (TreeViewModel model in currentView)
                    {
                        if (model.IsChecked==true)
                        {
                            selectedCategories.Add(model.Name);
                        }
                    }

                    List<TreeViewModel> treeViewCategories = TreeViewModel.SetTreeView(categoryInfoList, selectedCategories);
                   
                    treeViewCategory.ItemsSource = null;
                    treeViewCategory.ItemsSource = treeViewCategories;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to hide un-checked categories.\n" + ex.Message, "Hide Un-checked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonParameter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (filterEnabled)
                {
                    //Collapse
                    filterEnabled = false;
                    this.MinWidth = 335;
                    this.Width = 335;
                    this.MaxWidth = 335;
                    buttonParameter.Content = "Enable Filters";
                    selectedRules.Clear();
                    ClearUIComponents();
                }
                else
                {
                    //Expand
                    filterEnabled = true;
                    this.MinWidth = 660;
                    this.Width = 660;
                    this.MaxWidth = 660;
                    buttonParameter.Content = "Disable Filters";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to collapse the panel for parameter filters.\n"+ex.Message, "Collapse Filter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayParameters()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<ElementFilter> categoryFilters = new List<ElementFilter>();

                ICollection<ElementId> filterCatIds = new List<ElementId>();
                List<Category> categoryList = new List<Category>();
                List<TreeViewModel> treeviewModels = treeViewCategory.ItemsSource as List<TreeViewModel>;
                var selectedItems = from node in treeviewModels where node.IsChecked == true select node;
                if (selectedItems.Count() > 0)
                {
                    foreach (TreeViewModel model in selectedItems)
                    {
                        if (null != model.Tag)
                        {
                            CategoryInfo catInfo = model.Tag as CategoryInfo;
                            filterCatIds.Add(catInfo.CategoryId);
                            categoryList.Add(catInfo.CategoryObj);
                            categoryFilters.Add(new ElementCategoryFilter(catInfo.CategoryId));
                        }
                    }

                    LogicalOrFilter orFilter = new LogicalOrFilter(categoryFilters);
                    selectedElements = collector.WherePasses(orFilter).WhereElementIsNotElementType().ToElements().ToList();

                    if (includeLinks)
                    {
                        foreach (Document doc in linkedFiles.Values)
                        {
                            collector = new FilteredElementCollector(doc);
                            List<Element> elements = collector.WherePasses(orFilter).WhereElementIsNotElementType().ToElements().ToList();
                            if (elements.Count > 0)
                            {
                                selectedElements.AddRange(elements);
                            }
                        }
                    }

                    if (selectedElements.Count > 0)
                    {
                        paramInfoDictionary = new Dictionary<int, ParameterInfo>(); //accumulated dictionary
                        paramInfoDictionary = RevitUtil.GetParameterInfoList(m_doc, categoryList[0]);
                        if (categoryList.Count > 1)
                        {
                            List<int> intersectedIds = paramInfoDictionary.Keys.ToList(); //intersected id list
                            for (int i = 1; i < categoryList.Count; i++)
                            {
                                Dictionary<int, ParameterInfo> dictionary = RevitUtil.GetParameterInfoList(m_doc, categoryList[i]);
                                List<int> paramIds = dictionary.Keys.ToList();
                                foreach (int paramId in paramIds)
                                {
                                    if (!paramInfoDictionary.ContainsKey(paramId))
                                    {
                                        paramInfoDictionary.Add(paramId, dictionary[paramId]);
                                    }
                                }
                                intersectedIds = intersectedIds.Intersect(paramIds).ToList();
                            }

                            if (intersectedIds.Count > 0 && paramInfoDictionary.Count > 0)
                            {
                                List<int> paramIds = paramInfoDictionary.Keys.ToList();
                                foreach (int paramId in paramIds)
                                {
                                    if (!intersectedIds.Contains(paramId))
                                    {
                                        paramInfoDictionary.Remove(paramId);
                                    }
                                }
                            }
                        }
                        /*
                        Element sampleElement = selectedElements.First();
                        ICollection<ElementId> supportedParams = ParameterFilterUtilities.GetFilterableParametersInCommon(m_doc, filterCatIds);
                        List<string> projectParamNames = RevitUtil.FindProjectParameters(m_doc, categoryList);
                        */

                        ResetParameterComboBox(comboBoxFilterBy1, paramInfoDictionary);
                        ResetParameterComboBox(comboBoxFilterBy2, paramInfoDictionary);
                        ResetParameterComboBox(comboBoxFilterBy3, paramInfoDictionary);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("No element exists under the selected categories.\nPlease select other categories.", "Select Categories", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }  
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to display parameters.\n" + ex.Message, "Display Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearUIComponents()
        {
            try
            {
                comboBoxFilterBy1.ItemsSource = null;
                comboBoxFilterBy2.ItemsSource = null;
                comboBoxFilterBy3.ItemsSource = null;
                comboBoxCriteria1.ItemsSource = null;
                comboBoxCriteria2.ItemsSource = null;
                comboBoxCriteria3.ItemsSource = null;
                comboBoxValue1.ItemsSource = null;
                comboBoxValue2.ItemsSource = null;
                comboBoxValue3.ItemsSource = null;
                comboBoxValue1.Text = "";
                comboBoxValue2.Text = "";
                comboBoxValue3.Text = "";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to clear UI components.\n" + ex.Message, "Clear UI Components", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ResetParameterComboBox(System.Windows.Controls.ComboBox comboBox, Dictionary<int, ParameterInfo> paramInfoDictionary)
        {
            try
            {
                List<ParameterInfo> paramInfoList = paramInfoDictionary.Values.ToList();
                paramInfoList = paramInfoList.OrderBy(o => o.Name).ToList();

                comboBox.ItemsSource = null;
                comboBox.ItemsSource = paramInfoList;
                comboBox.DisplayMemberPath = "Name";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to reset parameter combobox.\n"+ex.Message, "Reset Parameter Combobox", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxFilterBy1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ParameterInfo paramInfo = comboBoxFilterBy1.SelectedItem as ParameterInfo;
                if (null != paramInfo)
                {
                    comboBoxCriteria1.ItemsSource = null;
                    if (paramInfo.ParamStorageType == StorageType.String)
                    {
                        comboBoxCriteria1.ItemsSource = criteriaInfoList;
                    }
                    else
                    {
                        var criteria = from criterion in criteriaInfoList where criterion.OnlyForString == false select criterion;
                        if (criteria.Count() > 0)
                        {
                            List<CriterionInfo> criteriaList = new List<CriterionInfo>();
                            criteriaList = criteria.ToList();
                            comboBoxCriteria1.ItemsSource = criteriaList;
                        }
                    }
                    comboBoxCriteria1.DisplayMemberPath = "DisplayName";

                    Dictionary<string, ParameterValueInfo> valueDictionary = new Dictionary<string, ParameterValueInfo>();
                    foreach (Element element in selectedElements)
                    {
                        ParameterValueInfo valueInfo = new ParameterValueInfo(m_doc, element, paramInfo);
                        if (!string.IsNullOrEmpty(valueInfo.ValueAsString))
                        {
                            if (!valueDictionary.ContainsKey(valueInfo.ValueAsString))
                            {
                                valueDictionary.Add(valueInfo.ValueAsString, valueInfo);
                            }
                        }
                    }
                    if (paramInfo.ParamStorageType == StorageType.ElementId)
                    {
                        comboBoxValue1.IsEditable = false;
                    }
                    else
                    {
                        comboBoxValue1.IsEditable = true;
                    }

                    comboBoxValue1.ItemsSource = null;
                    comboBoxValue1.ItemsSource = valueDictionary.Values.ToList();
                    comboBoxValue1.DisplayMemberPath = "ValueAsString";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to apply the selection on comboBoxFilterBy1.\n" + ex.Message, "Combobox FilterBy", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxFilterBy2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ParameterInfo paramInfo = comboBoxFilterBy2.SelectedItem as ParameterInfo;
                if (null != paramInfo)
                {
                    comboBoxCriteria2.ItemsSource = null;
                    if (paramInfo.ParamStorageType == StorageType.String)
                    {
                        comboBoxCriteria2.ItemsSource = criteriaInfoList;
                    }
                    else
                    {
                        var criteria = from criterion in criteriaInfoList where criterion.OnlyForString == false select criterion;
                        comboBoxCriteria2.ItemsSource = criteria;
                    }
                    comboBoxCriteria2.DisplayMemberPath = "DisplayName";

                    Dictionary<string, ParameterValueInfo> valueDictionary = new Dictionary<string, ParameterValueInfo>();
                    foreach (Element element in selectedElements)
                    {
                        ParameterValueInfo valueInfo = new ParameterValueInfo(m_doc, element, paramInfo);
                        if (!string.IsNullOrEmpty(valueInfo.ValueAsString))
                        {
                            if (!valueDictionary.ContainsKey(valueInfo.ValueAsString))
                            {
                                valueDictionary.Add(valueInfo.ValueAsString, valueInfo);
                            }
                        }
                    }

                    if (paramInfo.ParamStorageType == StorageType.ElementId)
                    {
                        comboBoxValue2.IsEditable = false;
                    }
                    else
                    {
                        comboBoxValue2.IsEditable = true;
                    }

                    comboBoxValue2.ItemsSource = null;
                    comboBoxValue2.ItemsSource = valueDictionary.Values.ToList();
                    comboBoxValue2.DisplayMemberPath = "ValueAsString";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to apply the selection on comboBoxFilterBy2.\n" + ex.Message, "Combobox FilterBy", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxFilterBy3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ParameterInfo paramInfo = comboBoxFilterBy3.SelectedItem as ParameterInfo;
                if (null != paramInfo)
                {
                    comboBoxCriteria3.ItemsSource = null;
                    if (paramInfo.ParamStorageType == StorageType.String)
                    {
                        comboBoxCriteria3.ItemsSource = criteriaInfoList;
                    }
                    else
                    {
                        var criteria = from criterion in criteriaInfoList where criterion.OnlyForString == false select criterion;
                        comboBoxCriteria3.ItemsSource = criteria;
                    }
                    comboBoxCriteria3.DisplayMemberPath = "DisplayName";

                    Dictionary<string, ParameterValueInfo> valueDictionary = new Dictionary<string, ParameterValueInfo>();
                    foreach (Element element in selectedElements)
                    {
                        ParameterValueInfo valueInfo = new ParameterValueInfo(m_doc, element, paramInfo);
                        if (!string.IsNullOrEmpty(valueInfo.ValueAsString))
                        {
                            if (!valueDictionary.ContainsKey(valueInfo.ValueAsString))
                            {
                                valueDictionary.Add(valueInfo.ValueAsString, valueInfo);
                            }
                        }
                    }

                    if (paramInfo.ParamStorageType == StorageType.ElementId)
                    {
                        comboBoxValue3.IsEditable = false;
                    }
                    else
                    {
                        comboBoxValue3.IsEditable = true;
                    }

                    comboBoxValue3.ItemsSource = null;
                    comboBoxValue3.ItemsSource = valueDictionary.Values.ToList();
                    comboBoxValue3.DisplayMemberPath = "ValueAsString";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to apply the selection on comboBoxFilterBy2.\n" + ex.Message, "Combobox FilterBy", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TreeViewModel> treeView = treeViewCategory.ItemsSource as List<TreeViewModel>;
                var selectedItems = from node in treeView where node.IsChecked == true select node;
                if (selectedItems.Count()>0)
                {
                    userSelectedList = new List<Category>();
                    foreach (TreeViewModel item in selectedItems)
                    {
                        if (null != item.Tag)
                        {
                            CategoryInfo catInfo = item.Tag as CategoryInfo;
                            userSelectedList.Add(catInfo.CategoryObj);
                        }
                    }

                    if (selectedElements.Count > 0)
                    {
                        if (filterEnabled)
                        {
                            selectedElements = GetFilteredElements(out selectedRules);
                        }

                        if (selectedElements.Count > 0)
                        {
                            this.DialogResult = true;
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("No Revit elements exist under the selected criteria.\nPlease select other categories or parameter to proceed.", "Empty Elements Set", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select at least one category.\n", "Empty Category Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to collect the selected categories.\n" + ex.Message, "Category Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearUIComponents();
                DisplayParameters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply the tree view selection.\n" + ex.Message, "Tree View Category", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearUIComponents();
                DisplayParameters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply the tree view selection.\n" + ex.Message, "Tree View Category", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void treeViewCategory_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            string enteredKey = key.ToString();
            for (int i = 0; i < treeViewCategory.Items.Count; i++)
            {
                TreeViewModel model = treeViewCategory.Items[i] as TreeViewModel;
                if (model.Name.StartsWith(enteredKey.ToLower()) || model.Name.StartsWith(enteredKey.ToUpper()))
                {
                    
                }
            }
        }

        private void checkBoxLink_Checked(object sender, RoutedEventArgs e)
        {
            includeLinks = true;
            ClearUIComponents();
            DisplayParameters();
        }

        private void checkBoxLink_Unchecked(object sender, RoutedEventArgs e)
        {
            includeLinks = false;
            ClearUIComponents();
            DisplayParameters();
        }

    }

    public class CategoryInfo
    {
        private Category categoryObj = null;
        private BuiltInCategory bltCategory = BuiltInCategory.INVALID;
        private string name = "";
        private ElementId categoryId = ElementId.InvalidElementId;
        private List<ElementId> filterableIds = new List<ElementId>();
        private List<CategoryInfo> subCategories = new List<CategoryInfo>();

        public Category CategoryObj { get { return categoryObj; } set { categoryObj = value; } }
        public BuiltInCategory BltInCategory { get { return bltCategory; } set { bltCategory = value; } }
        public string Name { get { return name; } set { name = value; } }
        public ElementId CategoryId { get { return categoryId; } set { categoryId = value; } }
        public List<CategoryInfo> SubCategories { get { return subCategories; } set { subCategories = value; } }

        public CategoryInfo(Category category)
        {
            categoryObj = category;
            name = category.Name;
            categoryId = category.Id;
            bltCategory = (BuiltInCategory)categoryId.IntegerValue;
        }

        public CategoryInfo(Category category, List<ElementId> categoryIds)
        {
            categoryObj = category;
            filterableIds = categoryIds;
            name = category.Name;
            categoryId = category.Id;
            bltCategory = (BuiltInCategory)categoryId.IntegerValue;

            subCategories = SetSubCategories(categoryObj);
        }

        private List<CategoryInfo> SetSubCategories(Category category)
        {
            List<CategoryInfo> subCatInfoList = new List<CategoryInfo>();
            try
            {
                if (category.SubCategories.Size > 0)
                {
                    foreach (Category subCategory in category.SubCategories)
                    {
                        if (filterableIds.Contains(subCategory.Id))
                        {
                            CategoryInfo catInfo = new CategoryInfo(subCategory);
                            subCatInfoList.Add(catInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return subCatInfoList;
        }
    }

    public class ParameterInfo
    {
        private Parameter parameterObj = null;
        private BuiltInParameter bltParameter = BuiltInParameter.INVALID;
        private string name = "";
        private ElementId paramId = ElementId.InvalidElementId;
        private StorageType storageType = StorageType.None;
        private bool isInstance = true;

        public Parameter ParameterObj { get { return parameterObj; } set { parameterObj = value; } }
        public BuiltInParameter BltParameter { get { return bltParameter; } set { bltParameter = value; } }
        public string Name { get { return name; } set { name = value; } }
        public ElementId ParamId { get { return paramId; } set { paramId = value; } }
        public StorageType ParamStorageType { get { return storageType; } set { storageType = value; } }
        public bool IsInstance { get { return isInstance; } set { isInstance = value; } }

        public ParameterInfo()
        {
        }

        public ParameterInfo(Document doc, ElementId eId)
        {
            paramId = eId;
            if (paramId.IntegerValue < 0)
            {
                bltParameter = (BuiltInParameter)paramId.IntegerValue;
                storageType = doc.get_TypeOfStorage(bltParameter);
            } 
            name = LabelUtils.GetLabelFor(bltParameter);
            
        }

        public ParameterInfo(Document doc, BuiltInParameter bltParam)
        {
            ElementId eId = new ElementId((int)bltParam);
            paramId = eId;
            bltParameter = bltParam;
            storageType = doc.get_TypeOfStorage(bltParam);
            name = LabelUtils.GetLabelFor(bltParam);
        }

        public ParameterInfo(string paramName)
        {
            name = paramName;
        }
        
        public ParameterInfo(Parameter param)
        {
            name = param.Definition.Name;
            parameterObj = param;
            paramId = param.Id;
            storageType = param.StorageType;

            if (param.Id.IntegerValue < 0)
            {
                bltParameter = (BuiltInParameter)param.Id.IntegerValue;
            }
        }

        public ParameterInfo(Parameter param, bool instanceParameter)
        {
            name = param.Definition.Name;
            parameterObj = param;
            paramId = param.Id;
            storageType = param.StorageType;

            if (param.Id.IntegerValue < 0)
            {
                bltParameter = (BuiltInParameter)param.Id.IntegerValue;
            }
            isInstance = instanceParameter;
        }
    }

    public class ParameterValueInfo
    {
        private Document doc=null;
        private Element revitElement = null;
        private ParameterInfo paramInfo = null;
        private string paramName = "";
        private bool isInstance = true;
        private double dblValue = 0;
        private int intValue = 0;
        private string strValue = "";
        private ElementId idValue = ElementId.InvalidElementId;
        private string valueAsString = "<empty value>";
        private StorageType storageType = StorageType.None;
        private bool isEmpty = true;

        public Element RevitElement { get { return revitElement; } set { revitElement = value; } }
        public bool IsInstance { get { return isInstance; } set { isInstance = value; } }
        public ParameterInfo ParamInfo { get { return paramInfo; } set { paramInfo = value; } }
        public double DblValue { get { return dblValue; } set { dblValue = value; } }
        public int IntValue { get { return intValue; } set { intValue = value; } }
        public string StrValue { get { return strValue; } set { strValue = value; } }
        public ElementId IdValue { get { return idValue; } set { idValue = value; } }
        public string ValueAsString { get { return valueAsString; } set { valueAsString = value; } }
        public StorageType ParamStorageType { get { return storageType; } set { storageType = value; } }
        public bool IsEmpty { get { return isEmpty; } set { isEmpty = value; } }
        
        public ParameterValueInfo(Document document, Element element, ParameterInfo parameterInfo)
        {
            doc = document;
            revitElement = element;
            paramInfo = parameterInfo;
            GetParameterValue();
        }
        
        public ParameterValueInfo(Document document, Element element, string parameterName)
        {
            doc = document;
            revitElement = element;
            paramName = parameterName;
            GetParameterValueByName();
        }

        private void GetParameterValue()
        {
            try
            {
                Parameter parameter = null;
                if (paramInfo.IsInstance) //instance parameter
                {
                    if (paramInfo.BltParameter != BuiltInParameter.INVALID)
                    {
                        parameter = revitElement.get_Parameter(paramInfo.BltParameter);
                    }
                    else
                    {
#if RELEASE2014
                        parameter = revitElement.get_Parameter(paramInfo.Name);
#else
                        parameter = revitElement.LookupParameter(paramInfo.Name);
#endif
                    }
                }
                else //type parameter
                {
                    ElementId elementId = revitElement.GetTypeId();
                    ElementType elementType = doc.GetElement(elementId) as ElementType;
                    if (paramInfo.BltParameter != BuiltInParameter.INVALID)
                    {
                        parameter = elementType.get_Parameter(paramInfo.BltParameter);
                    }
                    else
                    {
#if RELEASE2014
                        parameter = elementType.get_Parameter(paramInfo.Name);
#else
                        parameter = elementType.LookupParameter(paramInfo.Name);
#endif

                    }
                }
                if (null != parameter)
                {
                    if (parameter.HasValue)
                    {
                        isEmpty = false;
                        storageType = parameter.StorageType;
                        switch (parameter.StorageType)
                        {
                            case StorageType.Double:
                                dblValue = parameter.AsDouble();
                                if (!string.IsNullOrEmpty(parameter.AsValueString()))
                                {
                                    valueAsString = parameter.AsValueString();
                                }
                                else
                                {
                                    valueAsString = Math.Round(dblValue, 2).ToString();
                                }
                                break;
                            case StorageType.Integer:
                                intValue = parameter.AsInteger();
                                if (!string.IsNullOrEmpty(parameter.AsValueString()))
                                {
                                    valueAsString = parameter.AsValueString();
                                }
                                else
                                {
                                    valueAsString = intValue.ToString();
                                }
                                break;
                            case StorageType.String:
                                strValue = parameter.AsString();
                                valueAsString = strValue;
                                break;
                            case StorageType.ElementId:
                                idValue = parameter.AsElementId();
                                Element element = doc.GetElement(idValue);
                                if (null != element)
                                {
                                    valueAsString = element.Name;
                                }
                                break;
                        }
                    }
                    else
                    {
                        isEmpty = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void GetParameterValueByName()
        {
            try
            {
                Parameter parameter = null;
#if RELEASE2014
                parameter = revitElement.get_Parameter(paramName);
#else
                parameter = revitElement.LookupParameter(paramName);
#endif
                if (null != parameter) //instance parameter
                {
                    isInstance = true;
                }
                else //type parameter
                {
                    ElementId elementId = revitElement.GetTypeId();
                    ElementType elementType = doc.GetElement(elementId) as ElementType;
#if RELEASE2014
                    parameter = elementType.get_Parameter(paramName);
#else
                    parameter = elementType.LookupParameter(paramName);
#endif
                    if (null != parameter)
                    {
                        isInstance = false;
                    }
                }

                if (null != parameter)
                {
                    if (parameter.HasValue)
                    {
                        isEmpty = false;
                        storageType = parameter.StorageType;
                        switch (storageType)
                        {
                            case StorageType.Double:
                                dblValue = parameter.AsDouble();
                                if (!string.IsNullOrEmpty(parameter.AsValueString()))
                                {
                                    valueAsString = parameter.AsValueString();
                                }
                                else
                                {
                                    valueAsString = Math.Round(dblValue, 2).ToString();
                                }
                                break;
                            case StorageType.Integer:
                                intValue = parameter.AsInteger();
                                if (!string.IsNullOrEmpty(parameter.AsValueString()))
                                {
                                    valueAsString = parameter.AsValueString();
                                }
                                else
                                {
                                    valueAsString = intValue.ToString();
                                }
                                break;
                            case StorageType.String:
                                strValue = parameter.AsString();
                                valueAsString = strValue;
                                break;
                            case StorageType.ElementId:
                                idValue = parameter.AsElementId();
                                Element element = doc.GetElement(idValue);
                                if (null != element)
                                {
                                    valueAsString = element.Name;
                                }
                                break;
                        }
                    }
                    else
                    {
                        isEmpty = true;
                    }
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
        }
    }

    public class CriterionInfo
    {
        private CriteriaName criterionName = CriteriaName.na;
        private string displayName = "n/a";
        private bool onlyForString = false;

        public CriteriaName CriterionName { get { return criterionName; } set { criterionName = value; } }
        public string DisplayName { get { return displayName; } set { displayName = value; } }
        public bool OnlyForString { get { return onlyForString; } set { onlyForString = value; } }

        public CriterionInfo(CriteriaName criterion)
        {
            criterionName = criterion;
            displayName = GetDisplayName(out onlyForString);
        }

        private string GetDisplayName(out bool stringOnly)
        {
            stringOnly = false;
            string text = "";
            try
            {
                switch (criterionName)
                {
                    case CriteriaName.beginswith:
                        stringOnly = true;
                        text = "begins with";
                        break;
                    case CriteriaName.contains:
                        stringOnly = true;
                        text = "contains";
                        break;
                    case CriteriaName.endswith:
                        stringOnly = true;
                        text = "ends with"; 
                        break;
                    case CriteriaName.equals:
                        stringOnly = false;
                        text = "equals";
                        break;
                    case CriteriaName.isgreaterthan:
                        stringOnly = false;
                        text = "is greater than";
                        break;
                    case CriteriaName.isgreaterthanorequalto:
                        stringOnly = false;
                        text = "is greater than or equal to";
                        break;
                    case CriteriaName.islessthan:
                        stringOnly = false;
                        text = "is less than";
                        break;
                    case CriteriaName.islessthanorequalto:
                        stringOnly = false;
                        text = "is less than or equal to";
                        break;
                    case CriteriaName.doesnotbeginwith:
                        stringOnly = true;
                        text = "does not begin with";
                        break;
                    case CriteriaName.doesnotcontain:
                        stringOnly = true;
                        text = "does not contain";
                        break;
                    case CriteriaName.doesnotendwith:
                        stringOnly = true;
                        text = "does not end with";
                        break;
                    case CriteriaName.doesnotequal:
                        stringOnly = false;
                        text = "does not equal";
                        break;
                    case CriteriaName.na:
                        stringOnly = false;
                        text = "n/a";
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(criterionName.ToString()+" Failed to display the name of criterion.\n"+ex.Message, "Get Display Name", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return text;
        }
    }

    public class ViewInfo
    {
        private View viewObj = null;
        private ElementId viewId = ElementId.InvalidElementId;
        private string viewName = "";
        private ViewType viewTypeEnum;
        private string viewTypeName = "";

        public View ViewObj { get { return viewObj; } set { viewObj = value; } }
        public ElementId ViewId { get { return viewId; } set { viewId = value; } }
        public string ViewName { get { return viewName; } set { viewName = value; } }
        public ViewType ViewTypeEnum { get { return viewTypeEnum; } set { viewTypeEnum = value; } }
        public string ViewTypeName { get { return viewTypeName; } set { viewTypeName = value; } }

        public ViewInfo(View view)
        {
            viewObj = view;
            viewId = view.Id;
            viewName = view.Name;
            viewTypeEnum = view.ViewType;
            viewTypeName = viewTypeEnum.ToString();
        }

        public ViewInfo() { }
    }
}
