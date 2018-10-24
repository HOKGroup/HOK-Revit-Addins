using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
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
        BCF_Responsibility,
        BCF_Topic
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ParameterUtil
    {
        private static string sharedParameterTxt = "";
        private static readonly Definition ProjectIdDefinition = null;
        private static readonly Definition ActionDefinition = null;
        private static readonly Definition AuthorDefinition = null;
        private static readonly Definition CommentDefinition = null;
        private static readonly Definition DateDefinition = null;
        private static readonly Definition NameDefinition = null;
        private static readonly Definition ResponsibilityDefinition = null;
        private static readonly Definition TopicDefinition = null;

        /// <summary>
        /// Sets the Shared Parameter Definition file from a file path.
        /// </summary>
        /// <param name="uiapp">UI Application.</param>
        /// <param name="filePath">File Path to Shared Parameters File.</param>
        /// <returns>Definition File or null if failed.</returns>
        public static DefinitionFile SetDefinitionFile(UIApplication uiapp, string filePath)
        {
            DefinitionFile definitionFile = null;
            try
            {
                var doc = uiapp.ActiveUIDocument.Document;
                if (File.Exists(filePath))
                {
                    using (var trans = new Transaction(doc))
                    {
                        trans.Start("Open Parameter Definition");
                        try
                        {
                            uiapp.Application.SharedParametersFilename = filePath;
                            definitionFile = uiapp.Application.OpenSharedParameterFile();
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                            trans.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to set a definition file.\n" + ex.Message, "Set Definition File",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return definitionFile;
        }

        public static bool CreateBCFParameters(UIApplication uiapp, List<BuiltInCategory> bltCategories)
        {
            var created = false;
            try
            {
                var doc = uiapp.ActiveUIDocument.Document;

                if (string.IsNullOrEmpty(sharedParameterTxt))
                {
                    var currentAssembly = Assembly.GetExecutingAssembly().Location;
                    sharedParameterTxt = Path.GetDirectoryName(currentAssembly) + "/Resources/Addins Shared Parameters.txt";
                }

                var originalDefinitionFile = uiapp.Application.SharedParametersFilename;
                var definitionFile = SetDefinitionFile(uiapp, sharedParameterTxt);

                
             
                //insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_ColorSchemeId, pInfoCategory);
                if (bltCategories.Count > 0)
                {
                    var insertedParam = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_Action, bltCategories);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiapp"></param>
        /// <returns></returns>
        public static string GetBCFProjectId(UIApplication uiapp)
        {
            var bcfProjectId = "";
            try
            {
                var doc = uiapp.ActiveUIDocument.Document;
                var pInfo = doc.ProjectInformation;
                if (null != pInfo)
                {
#if RELEASE2015 || RELEASE2016
                    var param = pInfo.LookupParameter(BCFParameters.BCF_ProjectId.ToString());
#elif RELEASE2013||RELEASE2014
                    Parameter param = pInfo.get_Parameter(BCFParameters.BCF_ProjectId.ToString());
#endif
                    if (param != null)
                    {
                        bcfProjectId = param.AsString();
                    }
                    else
                    {
                        // TODO: This is a little sketchy! 
                        // TODO: Why not just stream a file to location on user drive? That way I don't have to distribute it with the DLLs.
                        // (Konrad) Shared Parameters File should be included with the DLL distribution.
                        if (string.IsNullOrEmpty(sharedParameterTxt))
                        {
                            var currentAssembly = Assembly.GetExecutingAssembly().Location;
                            sharedParameterTxt = Path.GetDirectoryName(currentAssembly) + "/Resources/Addins Shared Parameters.txt";
                        }

                        var originalDefinitionFile = uiapp.Application.SharedParametersFilename;
                        var definitionFile = SetDefinitionFile(uiapp, sharedParameterTxt);
                        if (definitionFile != null)
                        {
                            //create BCF_ProjectId parameter
                            var pInfoCategory = new List<BuiltInCategory>
                            {
                                BuiltInCategory.OST_ProjectInformation
                            };
                            var unused = InsertBinding(uiapp, definitionFile, BCFParameters.BCF_ProjectId, pInfoCategory);

                            SetDefinitionFile(uiapp, originalDefinitionFile);
                        }
                       
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return bcfProjectId;
        }

        public static bool SetBCFProjectId(Document doc, string bcfProjectId)
        {
            var inserted = false;
            try
            {
                var pInfo = doc.ProjectInformation;
                if (null != pInfo)
                {
#if RELEASE2015 || RELEASE2016
                    var param = pInfo.LookupParameter(BCFParameters.BCF_ProjectId.ToString());
#elif RELEASE2013||RELEASE2014
                    Parameter param = pInfo.get_Parameter(BCFParameters.BCF_ProjectId.ToString());
#endif
                    if (null != param)
                    {
                        using (var trans = new Transaction(doc))
                        {
                            try 
                            {
                                trans.Start("Set Parameter");
                                inserted = param.Set(bcfProjectId);
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
                var message = ex.Message;
            }
            return inserted;
        }

        public static string GetBCFColorSchemeId(Document doc)
        {
            var colorSchemeId = "";
            try
            {
                var pInfo = doc.ProjectInformation;
                if (null != pInfo)
                {
#if RELEASE2015 || RELEASE2016
                    var param = pInfo.LookupParameter(BCFParameters.BCF_ColorSchemeId.ToString());
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
                var message = ex.Message;
            }
            return colorSchemeId;
        }

        public static bool SetColorSchemeId(Document doc, string colorSchemeId)
        {
            var inserted = false;
            try
            {
                var pInfo = doc.ProjectInformation;
                if (null != pInfo)
                {
#if RELEASE2015 || RELEASE2016
                    var param = pInfo.LookupParameter(BCFParameters.BCF_ColorSchemeId.ToString());
#elif RELEASE2013||RELEASE2014
                    Parameter param = pInfo.get_Parameter(BCFParameters.BCF_ColorSchemeId.ToString());
#endif
                    if (null != param)
                    {
                        using (var trans = new Transaction(doc))
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
                var message = ex.Message;
            }
            return inserted;
        }

        /// <summary>
        /// Generates new Parameter Bindings in the model for BCF Parameters.
        /// </summary>
        /// <param name="uiapp">UI Application.</param>
        /// <param name="definitionFile">Shared Parameter Definition File.</param>
        /// <param name="bcfParam">BCF Parameter to be added.</param>
        /// <param name="bltCategories">Categories to bind the Parameter to.</param>
        /// <returns>True if successfull, otherwise false.</returns>
        private static bool InsertBinding(UIApplication uiapp, DefinitionFile definitionFile, BCFParameters bcfParam, IEnumerable<BuiltInCategory> bltCategories)
        {
            var inserted = false;
            try
            {
                var doc = uiapp.ActiveUIDocument.Document;
                using (var trans = new Transaction(doc))
                {
                    trans.Start("Insert Binding");
                    try
                    {
                        var catSet = uiapp.Application.Create.NewCategorySet();
                        foreach (var bltCat in bltCategories)
                        {
                            var category = doc.Settings.Categories.get_Item(bltCat);
                            if (category != null) catSet.Insert(category);
                        }

                        // (Jinsol) See if the project parameter already exists.
                        var definitionFound = FindExistingDefinition(doc, bcfParam);
                        if (definitionFound != null)
                        {
                            var elemBinding = (ElementBinding)doc.ParameterBindings.get_Item(definitionFound);
                            if (elemBinding != null)
                            {
                                var reinsert = false;
                                foreach (Category cat in catSet)
                                {
                                    if (!elemBinding.Categories.Contains(cat)) reinsert = true;
                                }

                                if (!reinsert) return false;

                                var binding = uiapp.Application.Create.NewInstanceBinding(catSet);
                                inserted = doc.ParameterBindings.ReInsert(definitionFound, binding);
                                trans.Commit();
                                return inserted;
                            }
                        }

                        Definition bcfDefinition = null;
                        foreach (var group in definitionFile.Groups)
                        {
                            if (group.Name != "HOK BCF") continue;

                            foreach (var definition in group.Definitions)
                            {
                                if (definition.Name != bcfParam.ToString()) continue;

                                bcfDefinition = definition;
                                break;
                            }
                            break;
                        }

                        if (null != bcfDefinition)
                        {
                            var binding = uiapp.Application.Create.NewInstanceBinding(catSet);
                            inserted = doc.ParameterBindings.Insert(bcfDefinition, binding);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                        trans.RollBack();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show(bcfParam + " Failed to insert binding." + ex.Message, "Insert Binding",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return inserted;
        }

        /// <summary>
        /// Checks if Definition already exists in the model.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="bcfParam">BCF Parameter</param>
        /// <returns>Parameter Definition for given BCF Paramter or null.</returns>
        private static Definition FindExistingDefinition(Document doc, BCFParameters bcfParam)
        {
            Definition definitionFound = null;
            try
            {
                switch (bcfParam)
                {
                    case BCFParameters.BCF_ProjectId:
                        definitionFound = ProjectIdDefinition;
                        break;
                    case BCFParameters.BCF_Action:
                        definitionFound = ActionDefinition;
                        break;
                    case BCFParameters.BCF_Author:
                        definitionFound = AuthorDefinition;
                        break;
                    case BCFParameters.BCF_Comment:
                        definitionFound = CommentDefinition;
                        break;
                    case BCFParameters.BCF_Date:
                        definitionFound = DateDefinition;
                        break;
                    case BCFParameters.BCF_Name:
                        definitionFound = NameDefinition;
                        break;
                    case BCFParameters.BCF_Responsibility:
                        definitionFound = ResponsibilityDefinition;
                        break;
                    case BCFParameters.BCF_Topic:
                        definitionFound = TopicDefinition;
                        break;
                    case BCFParameters.BCF_ColorSchemeId:
                        break;
                    case BCFParameters.BCF_IssueNumber:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(bcfParam), bcfParam, null);
                }

                if (definitionFound == null)
                {
                    var iter = doc.ParameterBindings.ForwardIterator();
                    while (iter.MoveNext())
                    {
                        var definition = iter.Key;
                        if (definition.Name == bcfParam.ToString())
                        {
                            definitionFound = definition;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show(bcfParam + ": Failed to find existing definition.\n" + ex.Message,
                    "Find Exisiting Definition", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return definitionFound;
        }

        public static bool UpdateBCFParameter(Document doc, ElementProperties ep, BCFParameters bcfParam, string value)
        {
            var result = false;
            try
            {
                var element = doc.GetElement(new ElementId(ep.ElementId));
                if (null != element)
                {
#if RELEASE2015 || RELEASE2016
                    var param = element.LookupParameter(bcfParam.ToString());
                    
#elif RELEASE2013||RELEASE2014
                    Parameter param = element.get_Parameter(bcfParam.ToString());
#endif
                    if (null != param)
                    {
                        using (var trans = new Transaction(doc))
                        {
                            trans.Start("Update Parameter");
                            try
                            {
                                result = param.Set(value);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                var message = ex.Message;
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
            var result = false;
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
