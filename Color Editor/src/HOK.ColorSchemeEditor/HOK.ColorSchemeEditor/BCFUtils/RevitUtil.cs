using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Windows;

namespace HOK.ColorSchemeEditor.BCFUtils
{
    public static class RevitUtil
    {
        private static Dictionary<string, List<string>> projectParameterInfo = null;

        public static string GetValidCategoryName(string catName)
        {
            string validName = catName;
            if (validName.Contains('\\')) { validName = validName.Replace('\\', '_'); }
            if (validName.Contains(':')) { validName = validName.Replace(':', '_'); }
            if (validName.Contains('{')) { validName = validName.Replace('{', '_'); }
            if (validName.Contains('}')) { validName = validName.Replace('}', '_'); }
            if (validName.Contains('[')) { validName = validName.Replace('[', '_'); }
            if (validName.Contains(']')) { validName = validName.Replace(']', '_'); }
            if (validName.Contains('|')) { validName = validName.Replace('|', '_'); }
            if (validName.Contains(';')) { validName = validName.Replace(';', '_'); }
            if (validName.Contains('<')) { validName = validName.Replace('<', '_'); }
            if (validName.Contains('>')) { validName = validName.Replace('>', '_'); }
            if (validName.Contains('?')) { validName = validName.Replace('?', '_'); }
            if (validName.Contains('\'')) { validName = validName.Replace('\'', '_'); }
            if (validName.Contains('~')) { validName = validName.Replace('~', '_'); }
            return validName;
        }

        public static System.Drawing.Color ConvertToSystemColor(Autodesk.Revit.DB.Color revitColor)
        {
            System.Drawing.Color systemColor = System.Drawing.Color.FromArgb(revitColor.Red, revitColor.Green, revitColor.Blue);
            return systemColor;
        }

        public static Autodesk.Revit.DB.Color ConvertToRevitColor(System.Drawing.Color systemColor)
        {
            Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(systemColor.R, systemColor.G, systemColor.B);
            return revitColor;
        }

        public static string GetParamValueAsString(Document doc, Element element, ParameterInfo paramInfo)
        {
            string paramValue = "";
            try
            {
                Parameter parameter = null;
                if (paramInfo.IsInstance) //instance parameter
                {
                    if (paramInfo.BltParameter != BuiltInParameter.INVALID)
                    {
                        parameter = element.get_Parameter(paramInfo.BltParameter);
                    }
                    else
                    {
#if RELEASE2014
                        parameter = element.get_Parameter(paramInfo.Name);
#else
                        parameter = element.LookupParameter(paramInfo.Name);
#endif
                    }
                }
                else //type parameter
                {
                    ElementId elementId = element.GetTypeId();
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
                        switch (parameter.StorageType)
                        {
                            case StorageType.Double:
                                paramValue = Math.Round(parameter.AsDouble(), 2).ToString();
                                break;
                            case StorageType.Integer:
                                paramValue = parameter.AsInteger().ToString();
                                break;
                            case StorageType.String:
                                paramValue = parameter.AsString();
                                break;
                            case StorageType.ElementId:
                                ElementId eId = parameter.AsElementId();
                                Element e = doc.GetElement(eId);
                                if (null != e)
                                {
                                    paramValue = e.Name;
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(paramInfo.Name + " Failed to get the parameter value as string.\n" + ex.Message, "Get Parameter Value as String", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return paramValue;
        }

        public static string GetParamValueAsString(Document doc, Element element, string paramName)
        {
            string paramValue = "";
            try
            {
#if RELEASE2014
                Parameter parameter = parameter = element.get_Parameter(paramName);
#else
                Parameter parameter = parameter = element.LookupParameter(paramName);
#endif
                if (null != parameter)
                {
                    if (parameter.HasValue)
                    {
                        switch (parameter.StorageType)
                        {
                            case StorageType.Double:
                                paramValue = ((int)parameter.AsDouble()).ToString();
                                break;
                            case StorageType.Integer:
                                paramValue = parameter.AsInteger().ToString();
                                break;
                            case StorageType.String:
                                paramValue = parameter.AsString();
                                break;
                            case StorageType.ElementId:
                                ElementId eId = parameter.AsElementId();
                                Element e = doc.GetElement(eId);
                                if (null != e)
                                {
                                    paramValue = e.Name;
                                }
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(paramName + " Failed to get the parameter value as string.\n" + ex.Message, "Get Parameter Value as String", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return paramValue;
        }

        public static Autodesk.Revit.DB.FilterRule GetDoubleRule(ElementId paramId, CriteriaName selectedCriteria, double ruleValue)
        {
            Autodesk.Revit.DB.FilterRule rule = null;
            try
            {
                switch (selectedCriteria)
                {
                    case CriteriaName.equals:
                        rule = ParameterFilterRuleFactory.CreateEqualsRule(paramId, ruleValue, double.Epsilon);
                        break;
                    case CriteriaName.isgreaterthan:
                        rule = ParameterFilterRuleFactory.CreateGreaterRule(paramId, ruleValue, double.Epsilon);
                        break;
                    case CriteriaName.isgreaterthanorequalto:
                        rule = ParameterFilterRuleFactory.CreateGreaterOrEqualRule(paramId, ruleValue, double.Epsilon);
                        break;
                    case CriteriaName.islessthan:
                        rule = ParameterFilterRuleFactory.CreateLessRule(paramId, ruleValue, double.Epsilon);
                        break;
                    case CriteriaName.islessthanorequalto:
                        rule = ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, ruleValue, double.Epsilon);
                        break;
                    case CriteriaName.doesnotequal:
                        rule = ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, ruleValue, double.Epsilon);
                        break;
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to get double filter rule.\n" + ex.Message, "Get Double Filter Rule", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return rule;
        }

        public static Autodesk.Revit.DB.FilterRule GetIntegerRule(ElementId paramId, CriteriaName selectedCriteria, int ruleValue)
        {
            Autodesk.Revit.DB.FilterRule rule = null;
            try
            {
                switch (selectedCriteria)
                {
                    case CriteriaName.equals:
                        rule = ParameterFilterRuleFactory.CreateEqualsRule(paramId, ruleValue);
                        break;
                    case CriteriaName.isgreaterthan:
                        rule = ParameterFilterRuleFactory.CreateGreaterRule(paramId, ruleValue);
                        break;
                    case CriteriaName.isgreaterthanorequalto:
                        rule = ParameterFilterRuleFactory.CreateGreaterOrEqualRule(paramId, ruleValue);
                        break;
                    case CriteriaName.islessthan:
                        rule = ParameterFilterRuleFactory.CreateLessRule(paramId, ruleValue);
                        break;
                    case CriteriaName.islessthanorequalto:
                        rule = ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, ruleValue);
                        break;
                    case CriteriaName.doesnotequal:
                        rule = ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, ruleValue);
                        break;
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to get integer filter rule.\n" + ex.Message, "Get Integer Filter Rule", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return rule;
        }

        public static Autodesk.Revit.DB.FilterRule GetStringRule(ElementId paramId, CriteriaName selectedCriteria, string ruleValue)
        {
            Autodesk.Revit.DB.FilterRule rule = null;
            try
            {
                switch (selectedCriteria)
                {
                    case CriteriaName.beginswith:
                        rule = ParameterFilterRuleFactory.CreateBeginsWithRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.contains:
                        rule = ParameterFilterRuleFactory.CreateContainsRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.endswith:
                        rule = ParameterFilterRuleFactory.CreateEndsWithRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.equals:
                        rule = ParameterFilterRuleFactory.CreateEqualsRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.isgreaterthan:
                        rule = ParameterFilterRuleFactory.CreateGreaterRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.isgreaterthanorequalto:
                        rule = ParameterFilterRuleFactory.CreateGreaterOrEqualRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.islessthan:
                        rule = ParameterFilterRuleFactory.CreateLessRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.islessthanorequalto:
                        rule = ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.doesnotbeginwith:
                        rule = ParameterFilterRuleFactory.CreateNotBeginsWithRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.doesnotcontain:
                        rule = ParameterFilterRuleFactory.CreateNotContainsRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.doesnotendwith:
                        rule = ParameterFilterRuleFactory.CreateNotEndsWithRule(paramId, ruleValue, false);
                        break;
                    case CriteriaName.doesnotequal:
                        rule = ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, ruleValue, false);
                        break;
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to get string filter rule.\n" + ex.Message, "Get String Filter Rule", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return rule;
        }

        public static Autodesk.Revit.DB.FilterRule GetElementIdRule(ElementId paramId, CriteriaName selectedCriteria, ElementId ruleValue)
        {
            Autodesk.Revit.DB.FilterRule rule = null;
            try
            {
                switch (selectedCriteria)
                {
                    case CriteriaName.equals:
                        rule = ParameterFilterRuleFactory.CreateEqualsRule(paramId, ruleValue);
                        break;
                    case CriteriaName.isgreaterthan:
                        rule = ParameterFilterRuleFactory.CreateGreaterRule(paramId, ruleValue);
                        break;
                    case CriteriaName.isgreaterthanorequalto:
                        rule = ParameterFilterRuleFactory.CreateGreaterOrEqualRule(paramId, ruleValue);
                        break;
                    case CriteriaName.islessthan:
                        rule = ParameterFilterRuleFactory.CreateLessRule(paramId, ruleValue);
                        break;
                    case CriteriaName.islessthanorequalto:
                        rule = ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, ruleValue);
                        break;
                    case CriteriaName.doesnotequal:
                        rule = ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, ruleValue);
                        break;
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to get integer filter rule.\n" + ex.Message, "Get Integer Filter Rule", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return rule;
        }

        public static List<Autodesk.Revit.DB.FilterRule> ConverToRevitFilterRule(List<FilterRule> filterRules)
        {
            List<Autodesk.Revit.DB.FilterRule> revitFilterRules = new List<Autodesk.Revit.DB.FilterRule>();
            try
            {
                foreach (FilterRule rule in filterRules)
                {
                    ElementId paramId = new ElementId(rule.ParameterId);
                    Autodesk.Revit.DB.FilterRule revitFilterRule = null;
                    switch (rule.ParameterStorageType)
                    {
                        case ParameterStorageType.Double:
                            double dblValue = 0;
                            if (double.TryParse(rule.RuleValue, out dblValue))
                            {
                                revitFilterRule = GetDoubleRule(paramId, rule.CriteriaName, dblValue);
                            }
                            break;
                        case ParameterStorageType.Integer:
                            int intValue = 0;
                            if (int.TryParse(rule.RuleValue, out intValue))
                            {
                                revitFilterRule = GetIntegerRule(paramId, rule.CriteriaName, intValue);
                            }
                            break;
                        case ParameterStorageType.String:
                            revitFilterRule = GetStringRule(paramId, rule.CriteriaName, rule.RuleValue);
                            break;
                        case ParameterStorageType.ElementId:
                            int eId = 0;
                            if (int.TryParse(rule.RuleValue, out eId))
                            {
                                ElementId elementId = new ElementId(eId);
                                revitFilterRule = GetElementIdRule(paramId, rule.CriteriaName, elementId);
                            }
                            break;
                    }

                    if (null != revitFilterRule)
                    {
                        revitFilterRules.Add(revitFilterRule);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to get Revit filter rules.\n" + ex.Message, "Get Revit Filter Rules", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return revitFilterRules;
        }

        public static Dictionary<string/*paramName*/, List<string>/*categoryNames*/> GetProjectParameterInfo(Document doc)
        {
            Dictionary<string, List<string>> projectParameters = new Dictionary<string, List<string>>();
            try
            {
                BindingMap bindingMap = doc.ParameterBindings;
                DefinitionBindingMapIterator iterator = bindingMap.ForwardIterator();

                while (iterator.MoveNext())
                {
                    Definition definition = iterator.Key as Definition;
                    string paramName = definition.Name;
                    ElementBinding elementBinding = iterator.Current as ElementBinding;
                    List<string> categoryNames = new List<string>();
                    if (null != elementBinding)
                    {
                        foreach (Category category in elementBinding.Categories)
                        {
                            if (!string.IsNullOrEmpty(category.Name))
                            {
                                categoryNames.Add(category.Name);
                            }
                        }
                    }

                    if (!projectParameters.ContainsKey(paramName))
                    {
                        projectParameters.Add(paramName, categoryNames);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get project parameter information.\n"+ex.Message, "Get Project Parameter Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return projectParameters;
        }

        public static List<string> FindProjectParameters(Document doc, List<Category> categoryList)
        {
            List<string> paramNames = new List<string>();
            try
            {
                if (null == projectParameterInfo)
                {
                    projectParameterInfo = GetProjectParameterInfo(doc);
                }

                if (null != projectParameterInfo)
                {
                    Dictionary<string, List<string>> collectedParamInfo = projectParameterInfo;
                    foreach (Category category in categoryList)
                    {
                        string categoryName = category.Name;
                        var parameters = from paramInfo in collectedParamInfo where paramInfo.Value.Contains(categoryName) select paramInfo;
                        collectedParamInfo = parameters.ToDictionary(p => p.Key, p => p.Value);
                    }

                    if (collectedParamInfo.Count > 0)
                    {
                        paramNames = collectedParamInfo.Keys.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get project parameters.\n" + ex.Message, "Get Project Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return paramNames;
        }

    }
}
