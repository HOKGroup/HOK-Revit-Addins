using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace RevitDBManager.Classes
{
    public class ParameterSettings
    {
        private DataGridView paramGridView;
        private Document doc;
        private ImageList lockImages;
        private Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
        private Dictionary<string, Dictionary<int, ParamProperties>> sharedParameters=new Dictionary<string, Dictionary<int,ParamProperties>>();
        private Dictionary<int/*index*/, ProjectParameter> proParamDictionary = new Dictionary<int, ProjectParameter>();
        private Dictionary<string/*category*/, Dictionary<string/*param*/, ProjectParameter>> catProParamDictionary = new Dictionary<string, Dictionary<string, ProjectParameter>>();
        private bool isEditMode;

        #region Properties
        public DataGridView DataGridView { get { return paramGridView; } set { paramGridView = value; } }
        public ImageList LockImages { get { return lockImages; } set { lockImages = value; } }
        public Dictionary<string, Dictionary<int, ParamProperties>> SharedParameters { get { return sharedParameters; } set { sharedParameters = value; } }
        public Dictionary<int, ProjectParameter> ProjectParameters { get { return proParamDictionary; } set { proParamDictionary = value; } }
        public Dictionary<string, Dictionary<string, ProjectParameter>> CatProjectParam { get { return catProParamDictionary; } set { catProParamDictionary = value; } }
        #endregion

        public ParameterSettings(Document document, bool isEdit)
        {
            doc = document;
            isEditMode = isEdit; //to apply default values when creating a new database
            CollectProjectParam();
        }

        public Dictionary<int, ParamProperties> SaveParameterSettings(Dictionary<int, ParamProperties> parameterDictionary, bool isInstance)
        {
            Dictionary<int, ParamProperties> paramDictionary = new Dictionary<int, ParamProperties>();
            paramDictionary = parameterDictionary;
            try
            {
                for (int i = 0; i < paramGridView.RowCount; i++)
                {
                    string strHeader = paramGridView.Rows[i].Cells[3].Value.ToString();
                    if (strHeader == "header") { continue; }
                    int paramId = int.Parse(strHeader);

                    ParamProperties pp = paramDictionary[paramId];
                    pp.IsInstance = isInstance;
                    pp.IsVisible = (bool)paramGridView.Rows[i].Cells[0].Tag;

                    switch ((LockType)paramGridView.Rows[i].Cells[2].Tag)
                    {
                        case LockType.LockAll:
                            pp.IsLockAll = true;
                            pp.IsEditable = false;
                            break;
                        case LockType.Editable:
                            pp.IsEditable = true;
                            pp.IsLockAll = false;
                            break;
                        case LockType.ReadOnly:
                            pp.IsEditable = false;
                            pp.IsLockAll = false;
                            break;
                    }
                    paramDictionary.Remove(paramId);
                    paramDictionary.Add(pp.ParamID, pp);

                    if (pp.IsShared && sharedParameters[pp.CategoryName].ContainsKey(pp.ParamID))
                    {
                        sharedParameters[pp.CategoryName].Remove(pp.ParamID);
                        sharedParameters[pp.CategoryName].Add(pp.ParamID, pp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save parameter settings.\n"+ex.Message, "ParameterSettings Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            

            return paramDictionary;
        }

        public void DisplayParameters(Dictionary<int, ParamProperties> paramProperties)
        {
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.BackColor = System.Drawing.Color.LightGray;
            headerStyle.ForeColor = System.Drawing.Color.White;

            try
            {
                int rowNum = 0;
                var properties = from property in paramProperties orderby property.Value.ParamGroup select property.Value;
                string paramGroup = null;

                foreach (ParamProperties pp in properties)
                {
                    if (string.Compare(paramGroup, pp.ParamGroup) == 0)
                    {
                        DisplayRow(pp, rowNum);

                        rowNum++;
                    }
                    else //when a new row for parameter group is required
                    {

                        string paramGroupStr = "";
                        if (pp.ParamGroup == "INVALID") { paramGroupStr = "OTHERS"; }
                        else if (pp.ParamGroup.Contains("PG_")) { paramGroupStr = pp.ParamGroup.Substring(3).Replace("_"," "); }

                        paramGridView.Rows.Add();
                        paramGridView.Rows[rowNum].Cells[0] = new DataGridViewTextBoxCell();
                        paramGridView.Rows[rowNum].Cells[0].Style = headerStyle;
                        paramGridView.Rows[rowNum].Cells[0].Tag = false;
                        paramGridView.Rows[rowNum].Cells[1] = new DataGridViewTextBoxCell();
                        paramGridView.Rows[rowNum].Cells[1].Style = headerStyle;
                        paramGridView.Rows[rowNum].Cells[1].Value = paramGroupStr; //to remove "PG_" 
                        paramGridView.Rows[rowNum].Cells[2] = new DataGridViewTextBoxCell();
                        paramGridView.Rows[rowNum].Cells[2].Style = headerStyle;
                        paramGridView.Rows[rowNum].Cells[3].Value = "header";
                        rowNum++;

                        DisplayRow(pp, rowNum);
                        rowNum++;
                    }
                    paramGroup = pp.ParamGroup;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Failed to display parameters in the data grid view: \n" + ex.Message,"ParameterSettings Error:");
            }
        }

        private void DisplayRow(ParamProperties property, int rowNum)
        {
            DataGridViewCellStyle readonlyStyle = new DataGridViewCellStyle();
            readonlyStyle.BackColor = System.Drawing.Color.White;
            readonlyStyle.ForeColor = System.Drawing.Color.Gray;

            paramGridView.Rows.Add();
            paramGridView.Rows[rowNum].Cells[0].Tag = property.IsVisible; //parameter isVisible
            paramGridView.Rows[rowNum].Cells[1].Value = property.ParamName; //parameter ParamName

            if (property.IsShared && sharedParameters[property.CategoryName].ContainsKey(property.ParamID))
            {
                ParamProperties pp = sharedParameters[property.CategoryName][property.ParamID];
                property.IsLockAll = pp.IsLockAll;
                property.IsEditable = pp.IsEditable;
                paramGridView.Rows[rowNum].Cells[0].Tag = pp.IsVisible;
            }

            if (property.IsReadOnly)
            {
                paramGridView.Rows[rowNum].Cells[1].Style = readonlyStyle;
                paramGridView.Rows[rowNum].Cells[2].Value = lockImages.Images[(int)LockType.ReadOnly]; //read only
                paramGridView.Rows[rowNum].Cells[2].Tag = LockType.ReadOnly;
            }
            else if (property.IsLockAll) 
            {
                paramGridView.Rows[rowNum].Cells[2].Value = lockImages.Images[(int)LockType.LockAll]; //lock all
                paramGridView.Rows[rowNum].Cells[2].Tag = LockType.LockAll;
            }
            else if (property.IsEditable) 
            {
                paramGridView.Rows[rowNum].Cells[2].Value = lockImages.Images[(int)LockType.Editable]; //Revit lock
                paramGridView.Rows[rowNum].Cells[2].Tag = LockType.Editable;
            } 
            else 
            {
                paramGridView.Rows[rowNum].Cells[1].Style = readonlyStyle;
                paramGridView.Rows[rowNum].Cells[2].Value = lockImages.Images[(int)LockType.ReadOnly]; //read only
                paramGridView.Rows[rowNum].Cells[2].Tag = LockType.ReadOnly;
            }

            paramGridView.Rows[rowNum].Cells[3].Value = property.ParamID;
            paramGridView.Rows[rowNum].Cells[3].Tag = property;
        }

        private void CollectProjectParam()
        {
            try
            {
                proParamDictionary = new Dictionary<int, ProjectParameter>();
                BindingMap bindingMap = doc.ParameterBindings;
                DefinitionBindingMapIterator iterator = bindingMap.ForwardIterator();

                int index = 0;
                while (iterator.MoveNext())
                {
                    ElementBinding elementBinding = iterator.Current as ElementBinding;
                    CategorySet categories = elementBinding.Categories;
                    Definition definition = iterator.Key;
                    if (CheckValidParameter(categories))
                    {
                        ProjectParameter pp = new ProjectParameter(definition, categories);
                        pp.ParameterName = pp.Definition.Name;
                        string type = elementBinding.GetType().ToString();
                        if (type.Contains("InstanceBinding")) { pp.IsInstance = true; }
                        else { pp.IsInstance = false; }
                        proParamDictionary.Add(index, pp);
                        index++;
                    }
                }
                
                foreach (int i in proParamDictionary.Keys)
                {
                    ProjectParameter pp = proParamDictionary[i];
                   
                    foreach (Category category in pp.Categories)
                    {
                        if (!category.AllowsBoundParameters) { continue; }
                        string catName = category.Name;
                        if (catProParamDictionary.ContainsKey(catName))
                        {
                            if (!catProParamDictionary[catName].ContainsKey(pp.ParameterName))
                            {
                                catProParamDictionary[catName].Add(pp.ParameterName, pp);
                            }
                        }
                        else
                        {
                            catProParamDictionary.Add(catName, new Dictionary<string, ProjectParameter>());
                            catProParamDictionary[catName].Add(pp.ParameterName, pp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed collect project parameters: \n"+ex.Message, "ParameterSettings Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CheckValidParameter(CategorySet categories)
        { 
            //all categories should allow to bind parameters
            bool result = true;
            foreach (Category category in categories)
            {
                if (!category.AllowsBoundParameters) { result = false; }
            }
            return result;
        }
    }

    public class ProjectParameter
    {
        private Definition definition=null;
        private CategorySet categories=null;
        private string paramName = "";

        public ProjectParameter(Definition def, CategorySet cat)
        {
            definition = def;
            categories = cat;
        }

        public ProjectParameter(string param)
        {
            paramName = param;
        }
        public Definition Definition { get { return definition; } set { definition = value; } }
        public string ParameterName { get { return paramName; } set { paramName = value; } }
        public string ParameterGroup { get { try { return definition.ParameterGroup.ToString(); } catch { return "null"; } } }
        public string ParameterType { get { try { return definition.ParameterType.ToString(); } catch { return "null"; } } }
        public bool IsInstance { get; set; }
        public CategorySet Categories { get { return categories; } set { categories = value; } }
        public string StrCategories
        {
            get
            {
                try 
                {
                    string strCat = "";
                    foreach (Category category in categories)
                    {
                        strCat += category.Name + ";";
                    }
                    return strCat;
                }
                catch { return "null"; }
            }

        }
    }

    public class LinkedParameter
    {
        private string paramName = "";
        private string fieldName = "";

        public LinkedParameter(string param, string field)
        {
            paramName = param;
            fieldName = field;
        }


        public string ControlParameter{ get { return paramName; } set { paramName = value; } }
        public string ControlField { get { return fieldName; } set { fieldName = value; } }
        public Dictionary<string/*parameter*/, string/*field*/> UpdateParameterField { get; set; }
        public string TableName { get; set; }
        public string DBPath { get; set; }
        public string CategoryName { get; set; }
    }

    public class TempStorage
    {
        private Dictionary<string, Dictionary<int, TypeProperties>> typeDictionary = new Dictionary<string, Dictionary<int, TypeProperties>>();
        private Dictionary<string, Dictionary<int, InstanceProperties>> instanceDictionary = new Dictionary<string, Dictionary<int, InstanceProperties>>();
        private Dictionary<string, Dictionary<int, ElementTypeProperties>> sysTypeDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>();
        private Dictionary<string, Dictionary<int, ElementProperties>> sysInstDictionary = new Dictionary<string, Dictionary<int, ElementProperties>>();
        private Dictionary<int, ViewTypeProperties> viewTypeDictionary = new Dictionary<int, ViewTypeProperties>();
        private Dictionary<int, ViewProperties> viewInstDictionary = new Dictionary<int, ViewProperties>();

        private Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> typeParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
        private Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> instParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> instCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> viewTypeParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> viewInstParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();

        public Dictionary<string, Dictionary<int, TypeProperties>> TypeDictionary { get { return typeDictionary; } set { typeDictionary = value; } }
        public Dictionary<string, Dictionary<int, InstanceProperties>> InstanceDictionary { get { return instanceDictionary; } set { instanceDictionary = value; } }
        public Dictionary<string, Dictionary<int, ElementTypeProperties>> SysTypeDictionary { get { return sysTypeDictionary; } set { sysTypeDictionary = value; } }
        public Dictionary<string, Dictionary<int, ElementProperties>> SysInstDictionary { get { return sysInstDictionary; } set { sysInstDictionary = value; } }
        public Dictionary<int, ViewTypeProperties> ViewTypeDictionary { get { return viewTypeDictionary; } set { viewTypeDictionary = value; } }
        public Dictionary<int, ViewProperties> ViewInstDictionary { get { return viewInstDictionary; } set { viewInstDictionary = value; } }

        public Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> TypeParamSettings { get { return typeParamSettings; } set { typeParamSettings = value; } }
        public Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> InstParamSettings { get { return instParamSettings; } set { instParamSettings = value; } }
        public Dictionary<string/*category*/, Dictionary<int, ParamProperties>> TypeCatParamSettings { get { return typeCatParamSettings; } set { typeCatParamSettings = value; } }
        public Dictionary<string/*category*/, Dictionary<int, ParamProperties>> InstCatParamSettings { get { return instCatParamSettings; } set { instCatParamSettings = value; } }
        public Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> ViewTypeParamSettigns { get { return viewTypeParamSettigns; } set { viewTypeParamSettigns = value; } }
        public Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> ViewInstParamSettigns { get { return viewInstParamSettigns; } set { viewInstParamSettigns = value; } }
    }

}
