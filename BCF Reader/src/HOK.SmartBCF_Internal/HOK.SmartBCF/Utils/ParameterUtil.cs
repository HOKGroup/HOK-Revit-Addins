using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SmartBCF.GoogleUtils;

namespace HOK.SmartBCF.Utils
{
    public enum BCFParameters
    {
        BCF_Action,
        BCF_Author,
        BCF_Comment,
        BCF_ColorSchemeId,
        BCF_Date,
        BCF_IssueNumber,
        BCF_Name,
        BCF_ProjectId,
        BCF_ProjectName,
        BCF_ProjectLink,
        BCF_Responsibility,
        BCF_Topic
    }

    public static class ParameterUtil
    {
        private static string sharedParameterTxt = "";

        private static Definition ProjectIdDefinition = null;
        private static Definition ProjectNameDefinition = null;
        private static Definition ProjectLinkDefinition = null;
        private static Definition ActionDefinition = null;
        private static Definition AuthorDefinition = null;
        private static Definition CommentDefinition = null;
        private static Definition DateDefinition = null;
        private static Definition NameDefinition = null;
        private static Definition ResponsibilityDefinition = null;
        private static Definition TopicDefinition = null;

        public static DefinitionFile SetDefinitionFile(UIApplication uiapp, string definitionPath)
        {
            DefinitionFile definitionFile = null;
            try
            {
                Document doc = uiapp.ActiveUIDocument.Document;
                if (File.Exists(definitionPath))
                {
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Open Parameter Definition");
                        try
                        {
                            uiapp.Application.SharedParametersFilename = definitionPath;
                            definitionFile = uiapp.Application.OpenSharedParameterFile();
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                            trans.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set a definition file.\n"+ex.Message, "Set Definition File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return definitionFile;
        }

        public static bool CreateBCFParameters(UIApplication uiapp, List<BuiltInCategory> bltCategories)
        {
            bool created = false;
            try
            {
                Document doc = uiapp.ActiveUIDocument.Document;

                if (string.IsNullOrEmpty(sharedParameterTxt))
                {
                    string currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    sharedParameterTxt = Path.GetDirectoryName(currentAssembly) + "/Resources/HOK BCF Parameters.txt";
                }

                string originalDefinitionFile = uiapp.Application.SharedParametersFilename;
                DefinitionFile definitionFile = SetDefinitionFile(uiapp, sharedParameterTxt);

                //create BCF_ProjectId parameter
                List<BuiltInCategory> pInfoCategory = new List<BuiltInCategory>();
                pInfoCategory.Add(BuiltInCategory.OST_ProjectInformation);
                bool insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_ProjectName, pInfoCategory);
                insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_ProjectLink, pInfoCategory);
                //bool insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_ProjectId, pInfoCategory);
                //insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_ColorSchemeId, pInfoCategory);
                if (bltCategories.Count > 0)
                {
                    insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_Action, bltCategories);
                    insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_Author, bltCategories);
                    insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_Comment, bltCategories);
                    insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_Date, bltCategories);
                    insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_Name, bltCategories);
                    insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_Responsibility, bltCategories);
                    insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_Topic, bltCategories);
                }

                SetDefinitionFile(uiapp, originalDefinitionFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create BCF parameters.\n"+ex.Message, "Create BCF Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }
        public static string GetBCFProjectInfo(Document doc, string paramName)
        {
            string paramValue = "";
            try
            {
                ProjectInfo pInfo = doc.ProjectInformation;
                if (null != pInfo)
                {
#if RELEASE2015
                    Parameter param = pInfo.LookupParameter(paramName);
#elif RELEASE2013||RELEASE2014
                    Parameter param = pInfo.get_Parameter(paramName);
#endif
                    if (null != param)
                    {
                        paramValue = param.AsString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get BCF project name from the current document.\n"+ex.Message, "Get BCF Project Name", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return paramValue;
        }

        public static bool SetBCFProjectInfo(Document doc, string paramName, string paramValue)
        {
            bool inserted = false;
            try
            {
                ProjectInfo pInfo = doc.ProjectInformation;
                if (null != pInfo)
                {
#if RELEASE2015
                    Parameter param = pInfo.LookupParameter(paramName);
#elif RELEASE2013||RELEASE2014
                    Parameter param = pInfo.get_Parameter(paramName);
#endif
                    if (null != param)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            try 
                            {
                                trans.Start("Set Parameter");
                                inserted = param.Set(paramValue);
                                trans.Commit();
                            }
                            catch { trans.RollBack();  }
                        }
                    }
                }
                inserted = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        public static string GetBCFColorSchemeId(Document doc)
        {
            string colorSchemeId = "";
            try
            {
                ProjectInfo pInfo = doc.ProjectInformation;
                if (null != pInfo)
                {
#if RELEASE2015
                    Parameter param = pInfo.LookupParameter(BCFParameters.BCF_ColorSchemeId.ToString());
#elif RELEASE2013||RELEASE2014
                    Parameter param = pInfo.get_Parameter(BCFParameters.BCF_ColorSchemeId.ToString());
#endif
                    if (null != param)
                    {
                        colorSchemeId = param.AsString();
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return colorSchemeId;
        }

        public static bool SetColorSchemeId(Document doc, string colorSchemeId)
        {
            bool inserted = false;
            try
            {
                ProjectInfo pInfo = doc.ProjectInformation;
                if (null != pInfo)
                {
#if RELEASE2015
                    Parameter param = pInfo.LookupParameter(BCFParameters.BCF_ColorSchemeId.ToString());
#elif RELEASE2013||RELEASE2014
                    Parameter param = pInfo.get_Parameter(BCFParameters.BCF_ColorSchemeId.ToString());
#endif
                    if (null != param)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            try
                            {
                                trans.Start("Set Parameter");
                                inserted = param.Set(colorSchemeId);
                                trans.Commit();
                            }
                            catch { trans.RollBack(); }
                        }
                    }
                }
                inserted = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        private static bool InsertBinding(UIApplication uiapp, DefinitionFile definitionFile, BCFParameters bcfParam, List<BuiltInCategory> bltCategories)
        {
            bool inserted = false;
            try
            {
                Document doc = uiapp.ActiveUIDocument.Document;
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Insert Binding");
                    try
                    {
                        DefinitionBindingMapIterator iter = doc.ParameterBindings.ForwardIterator();
                        CategorySet catSet = uiapp.Application.Create.NewCategorySet();
                        foreach (BuiltInCategory bltCat in bltCategories)
                        {
                            Category category = doc.Settings.Categories.get_Item(bltCat);
                            if (null != category)
                            {
                                catSet.Insert(category);
                            }
                        }

                        //see if the project parameter already exists
                        Definition definitionFound = FindExistingDefinition(doc, bcfParam);
                        if (null != definitionFound)
                        {
                            ElementBinding elemBinding = (ElementBinding)doc.ParameterBindings.get_Item(definitionFound);
                            if (null != elemBinding)
                            {
                                bool reinsert = false;
                                foreach (Category cat in catSet)
                                {
                                    if (!elemBinding.Categories.Contains(cat))
                                    {
                                        reinsert = true;
                                    }
                                }

                                if (reinsert)
                                {
                                    InstanceBinding binding = uiapp.Application.Create.NewInstanceBinding(catSet);
                                    inserted = doc.ParameterBindings.ReInsert(definitionFound, binding);
                                    trans.Commit();
                                }
                                return inserted;
                            }
                        }
                        

                        Definition bcfDefinition = null;
                        foreach (DefinitionGroup group in definitionFile.Groups)
                        {
                            if (group.Name == "HOK BCF")
                            {
                                foreach (Definition definition in group.Definitions)
                                {
                                    if (definition.Name == bcfParam.ToString())
                                    {
                                        bcfDefinition = definition;
                                        break;
                                    }
                                }
                                break;
                            }
                        }

                        if (null != bcfDefinition)
                        {
                            InstanceBinding binding = uiapp.Application.Create.NewInstanceBinding(catSet);
                            inserted = doc.ParameterBindings.Insert(bcfDefinition, binding);
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        trans.RollBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(bcfParam.ToString() + " Failed to insert binding." + ex.Message, "Insert Binding", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        private static Definition FindExistingDefinition(Document doc, BCFParameters bcfParam)
        {
            Definition definitionFound = null;
            try
            {
                DefinitionBindingMapIterator iter = doc.ParameterBindings.ForwardIterator();
                while (iter.MoveNext())
                {
                    Definition definition = iter.Key;
                    ElementBinding elemBinding = (ElementBinding)iter.Current;
                    if (definition.Name == bcfParam.ToString())
                    {
                        definitionFound = definition;
                    }
                }
                if (null != definitionFound)
                {
                    switch (bcfParam)
                    {
                        case BCFParameters.BCF_ProjectId:
                            ProjectIdDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_ProjectName:
                            ProjectNameDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_ProjectLink:
                            ProjectLinkDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_Action:
                            ActionDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_Author:
                            AuthorDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_Comment:
                            CommentDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_Date:
                            DateDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_Name:
                            NameDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_Responsibility:
                            ResponsibilityDefinition = definitionFound;
                            break;
                        case BCFParameters.BCF_Topic:
                            TopicDefinition = definitionFound;
                            break;
                    }
                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(bcfParam.ToString() + ": Failed to find existing definition.\n"+ex.Message, "Find Exisiting Definition", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return definitionFound;
        }

        public static bool UpdateBCFParameter(Document doc, ElementProperties ep, BCFParameters bcfParam, string value)
        {
            bool result = false;
            try
            {
                Element element = doc.GetElement(new ElementId(ep.ElementId));
                if (null != element)
                {
#if RELEASE2015
                    Parameter param = element.LookupParameter(bcfParam.ToString());
                    
#elif RELEASE2013||RELEASE2014
                    Parameter param = element.get_Parameter(bcfParam.ToString());
#endif
                    if (null != param)
                    {
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Update Parameter");
                            try
                            {
                                result = param.Set(value);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                                trans.RollBack();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(bcfParam.ToString() + " Failed to update BCF parameter.\n" + ex.Message, "Update BCF Parameter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public static bool UpdateBCFParameters(Document doc, ElementProperties ep, IssueEntry issue, Comment comment)
        {
            bool result = false;
            try
            {
                foreach (BCFParameters param in Enum.GetValues(typeof(BCFParameters)))
                {
                    switch (param)
                    {
                        case BCFParameters.BCF_Action:
                            result = UpdateBCFParameter(doc, ep, BCFParameters.BCF_Action, ep.Action);
                            break;
                        case BCFParameters.BCF_Author:
                            if (null != comment)
                            {
                                result = UpdateBCFParameter(doc, ep, BCFParameters.BCF_Author, comment.Author);
                            }
                            break;
                        case BCFParameters.BCF_Comment:
                            if (null != comment)
                            {
                                result = UpdateBCFParameter(doc, ep, BCFParameters.BCF_Comment, comment.Comment1);
                            }
                            break;
                        case BCFParameters.BCF_Date:
                            if (null != comment)
                            {
                                result = UpdateBCFParameter(doc, ep, BCFParameters.BCF_Date, comment.Date.ToString());
                            }
                            break;
                        case BCFParameters.BCF_IssueNumber:
                            break;
                        case BCFParameters.BCF_Name:
                            result = UpdateBCFParameter(doc, ep, BCFParameters.BCF_Name, issue.BCFName);
                            break;
                        case BCFParameters.BCF_Responsibility:
                            result = UpdateBCFParameter(doc, ep, BCFParameters.BCF_Responsibility, ep.ResponsibleParty);
                            break;
                        case BCFParameters.BCF_Topic:
                            result = UpdateBCFParameter(doc, ep, BCFParameters.BCF_Topic, issue.IssueTopic);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update BCF Parameter." + ex.Message, "Update BCF Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }
    }
}
