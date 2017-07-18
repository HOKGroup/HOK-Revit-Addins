using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RevitDBManager.Classes;
using Autodesk.Revit.DB;
using Microsoft.Office.Interop.Access.Dao;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using RevitDBManager.GenericForms;
using System.Data;
using RevitDBManager.HelperClasses;
using Autodesk.Revit.UI;

namespace RevitDBManager.ViewerClasses
{
    class ReadFromRevit
    {
        private Autodesk.Revit.DB.Document doc;
        private Database daoDB;
        private form_ProgressBar progressForm;
        private UnitConverter unitConverter;
        private Dictionary<string/*tableName*/, List<string> /*nonvisibleField*/> nonVisibleFields = new Dictionary<string, List<string>>();
        private Dictionary<string/*tableName*/, Dictionary<string/*fieldName*/, LockType>> lockTypeFields = new Dictionary<string, Dictionary<string/*fieldName*/, LockType>>();

        private Dictionary<string/*categoryName*/, Dictionary<int/*typeId*/, ElementTypeProperties>> elementTypes = new Dictionary<string, Dictionary<int, ElementTypeProperties>>();
        private Dictionary<int/*viewTypeID*/, ViewTypeProperties> viewTypes = new Dictionary<int, ViewTypeProperties>();

        private Dictionary<string/*categoryName*/, Dictionary<int/*instanceId*/, InstanceProperties>> familyInstances = new Dictionary<string, Dictionary<int, InstanceProperties>>();
        private Dictionary<string/*categoryName*/, Dictionary<int/*elementId*/, ElementProperties>> sysElements = new Dictionary<string, Dictionary<int, ElementProperties>>();
        private Dictionary<string/*paramId*/, string/*paramName*/> paramIdMap = new Dictionary<string, string>();
        
        private Dictionary<int/*roomId*/, RoomProperties> rooms = new Dictionary<int, RoomProperties>();
        private Dictionary<int/*spaceId*/, SpaceProperties> spaces = new Dictionary<int, SpaceProperties>();
        private Dictionary<int/*areaId*/, AreaProperties> areas = new Dictionary<int, AreaProperties>();
        private Dictionary<int/*viewId*/, ViewProperties> views = new Dictionary<int, ViewProperties>();
        private Dictionary<string/*categoryName*/, bool/*excludeInstance*/> excludeInstanceSettings = new Dictionary<string, bool>();

        private bool isSpaceSelected = false;
        private bool isAreaSelected = false;
        private string progressbarText = "";
        private bool setFixMode = false;
        private bool fixMode = false; //prompt to see viewer to fix controlling values when the external database doesn't include a value user typed

        private Dictionary<string/*tableName*/, LinkedParameter> externalFields = new Dictionary<string, LinkedParameter>();
        private Dictionary<string/*tableName*/, DataTable> sourceTables = new Dictionary<string, DataTable>();//datatables from a linked external database
        private List<string/*tableName*/> warningTables = new List<string>(); //to navigate in the data viewer to this table
        private static Dictionary<string/*paramId*/, string/*suffix*/> suffixDictionary = new Dictionary<string, string>();

        public Dictionary<string, Dictionary<int, ElementTypeProperties>> ElementTypeDictionary { get { return elementTypes; } set { elementTypes = value; } }
        public Dictionary<string, Dictionary<int, InstanceProperties>> InstanceDictionary { get { return familyInstances; } set { familyInstances = value; } }
        public Dictionary<string, Dictionary<int, ElementProperties>> SysInstDictionary { get { return sysElements; } set { sysElements = value; } }
        public Dictionary<int, RoomProperties> RoomDictionary { get { return rooms; } set { rooms = value; } }
        public Dictionary<int, SpaceProperties> SpaceDictionary { get { return spaces; } set { spaces = value; } }
        public Dictionary<int, AreaProperties> AreaDictionary { get { return areas; } set { areas = value; } }
        public Dictionary<int, ViewProperties> ViewDictionary { get { return views; } set { views = value; } }
        public Dictionary<string/*tableName*/, List<string> /*nonvisibleField*/> NonVisibleFields { get { return nonVisibleFields; } set { nonVisibleFields = value; } }
        public Dictionary<string/*tableName*/, Dictionary<string/*fieldName*/, LockType>> LockTypeFields { get { return lockTypeFields; } set { lockTypeFields = value; } }
        public Dictionary<string, string> ParamIDMap { get { return paramIdMap; } set { paramIdMap = value; } }
        public Dictionary<string, bool> ExcludeInstSettings { get { return excludeInstanceSettings; } set { excludeInstanceSettings = value; } }
        public bool IsSpaceSelected { get { return isSpaceSelected; } set { isSpaceSelected = value; } }
        public bool IsAreaSelected { get { return isAreaSelected; } set { isAreaSelected = value; } }
        public string ProgressbarText { get { return progressbarText; } set { progressbarText = value; } }
        public Dictionary<string, LinkedParameter> ExternalFields { get { return externalFields; } set { externalFields = value; } }
        public Dictionary<string, DataTable> SourceTables { get { return sourceTables; } set { sourceTables = value; } }
        public bool FixMode { get { return fixMode; } set { fixMode = value; } }
        public bool SetFixMode { get { return setFixMode; } set { setFixMode = value; } }
        public List<string> WarningTables { get { return warningTables; } set { warningTables = value; } }

        public ReadFromRevit(Autodesk.Revit.DB.Document document, Database database)
        {
            doc = document;
            daoDB = database;
            progressbarText = "Opening Data Editor...";
            CheckElementTypes();

            unitConverter = new UnitConverter();
        }

        //find whether added element types or deleted types exist. 
        private void CheckElementTypes()
        {
            ReadFromFamilyInfo();
            ReadFromAutoSync();
            WriteToFamilyInfo();
        }

        private void ReadFromFamilyInfo()
        {
            try
            {
                Recordset recordset = daoDB.OpenTable("Rvt_FamilyInfo");
                while (!recordset.EOF)
                {
                    int typeId = int.Parse(recordset.Fields["TypeID"].Value);
                    string categoryName = recordset.Fields["CategoryName"].Value;
                    ElementId elementId = new ElementId(typeId);
                    Element element = doc.GetElement(elementId);
                    if (null != element)
                    {
                        ElementType elementType = element as ElementType;
                        ViewFamilyType viewFamilyType = element as ViewFamilyType;
                        if (null != viewFamilyType)
                        {
                            ViewTypeProperties vtp = new ViewTypeProperties(viewFamilyType);
                            if (!viewTypes.ContainsKey(vtp.ViewTypeID))
                            {
                                viewTypes.Add(vtp.ViewTypeID, vtp);
                            }
                        }
                        else if (null != elementType)
                        {
                            ElementTypeProperties etp = new ElementTypeProperties(elementType);
                            if (elementTypes.ContainsKey(categoryName))
                            {
                                elementTypes[categoryName].Add(etp.TypeID, etp);
                            }
                            else
                            {
                                elementTypes.Add(categoryName, new Dictionary<int, ElementTypeProperties>());
                                elementTypes[categoryName].Add(etp.TypeID, etp);
                            }
                        }
                    }
                    recordset.MoveNext();
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read element types from FamilyInfo table: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReadFromAutoSync()
        {
            try
            {
                List<ElementId> elementTypeIds = new List<ElementId>();
 
                Recordset recordset = daoDB.OpenTable("UI_AutoSyncOptions");
                while (!recordset.EOF)
                {
                    string syncType = recordset.Fields["SyncType"].Value;
                    if (syncType == "Category")
                    {
                        int id = int.Parse(recordset.Fields["CategoryID"].Value);
                        BuiltInCategory enumCategory = (BuiltInCategory)id;
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<ElementId> typeIds = collector.OfCategory(enumCategory).OfClass(typeof(ElementType)).ToElementIds().ToList();
                        elementTypeIds.AddRange(typeIds);

                        if (enumCategory == BuiltInCategory.OST_Views || enumCategory == BuiltInCategory.OST_Sheets)
                        {
                            collector = new FilteredElementCollector(doc);
                            List<ViewFamilyType> viewTypeElements = collector.OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().ToList();

                            foreach (ViewFamilyType viewType in viewTypeElements)
                            {
                                string viewFamily = viewType.ViewFamily.ToString();
                                if(ViewTypeProperties.excludeViewFamily.Contains(viewFamily)) {continue;}
                                elementTypeIds.Add(viewType.Id);
                            }
                        }
                    }
                    else if (syncType == "Family")
                    {
                        int id = int.Parse(recordset.Fields["FamilyID"].Value);
                        ElementId familyId = new ElementId(id);
                        FamilySymbolFilter symbolFilter = new FamilySymbolFilter(familyId);
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<ElementId> symbolIds = collector.WherePasses(symbolFilter).ToElementIds().ToList();

                        foreach (ElementId eId in symbolIds)
                        {
                            if (!elementTypeIds.Contains(eId))
                            {
                                elementTypeIds.Add(eId);
                            }
                        }
                    }
                    recordset.MoveNext();
                }

                foreach (ElementId elementId in elementTypeIds)
                {
                    Element element = doc.GetElement(elementId);
                    ElementType elementType = element as ElementType;
                    ViewFamilyType viewType = element as ViewFamilyType;
                    if(null!=viewType)
                    {
                        ViewTypeProperties vtp = new ViewTypeProperties(viewType);
                        if (!viewTypes.ContainsKey(vtp.ViewTypeID))
                        {
                            viewTypes.Add(vtp.ViewTypeID, vtp);
                        }
                    }
                    else if (null != elementType)
                    {
                        string catName = elementType.Category.Name;
                        if (elementTypes.ContainsKey(catName))
                        {
                            if (elementTypes[catName].ContainsKey(elementId.IntegerValue)) { continue; }

                            ElementTypeProperties etp = new ElementTypeProperties(elementType);
                            elementTypes[catName].Add(etp.TypeID, etp);
                        }
                        else
                        {
                            ElementTypeProperties etp = new ElementTypeProperties(elementType);
                            elementTypes.Add(catName,new Dictionary<int, ElementTypeProperties>());
                            elementTypes[catName].Add(etp.TypeID, etp);
                        }
                    }
                    
                }

                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read element types from UI_AutoSyncOptions table: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void WriteToFamilyInfo()
        {
            try
            {
                string tableName = "Rvt_FamilyInfo";
                string queryStr = "DELETE FROM [" + tableName + "]";
                daoDB.Execute(queryStr, RecordsetOptionEnum.dbFailOnError);

                Dictionary<string, string> valuePair = new Dictionary<string, string>();

                foreach (string category in elementTypes.Keys)
                {
                    foreach (int typeId in elementTypes[category].Keys)
                    {
                        ElementTypeProperties etp = elementTypes[category][typeId];

                        valuePair = new Dictionary<string, string>();
                        valuePair.Add("TypeID", etp.TypeID.ToString());
                        valuePair.Add("TypeName", etp.ElementTypeName);
                        valuePair.Add("FamilyID", etp.FamilyID.ToString());
                        valuePair.Add("FamilyName", etp.FamilyName);
                        valuePair.Add("CategoryName", etp.CategoryName);

                        string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                        daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    }
                }
                foreach (int typeId in viewTypes.Keys)
                {
                    ViewTypeProperties vtp = viewTypes[typeId];

                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("TypeID", vtp.ViewTypeID.ToString());
                    valuePair.Add("TypeName", vtp.ViewTypeName);
                    valuePair.Add("FamilyName", vtp.ViewFamilyName);
                    valuePair.Add("CategoryName", vtp.CategoryName);

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write to FamilyInfo table: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CollectRevitElementsData()
        {
            progressForm = new form_ProgressBar();
            progressForm.Text = progressbarText;
            progressForm.MaxValue = SetMaxValue1();
            progressForm.LabelText = "Collecting Information from the Revit Project...";
            progressForm.Show();
            progressForm.Refresh();

            CollectElements();
            if (viewTypes.Count > 0) { CollectViews(); }
            CollectRooms();
            if (isSpaceSelected) { CollectSpaces(); }
            if (isAreaSelected) { CollectAreas(); }

            progressForm.Close();
        }

        private void CollectElements()
        {
            try
            {
                foreach (string categoryName in elementTypes.Keys)
                {
                    if (excludeInstanceSettings.ContainsKey(categoryName)) { if (excludeInstanceSettings[categoryName]) { continue; } }
                    foreach (int typeId in elementTypes[categoryName].Keys)
                    {
                        ElementTypeProperties etp = elementTypes[categoryName][typeId];
                        if (etp.IsFamilySymbol)
                        {
                            CollectFamilyInstances(etp);
                        }
                        else
                        {
                            CollectSystemFamilies(etp);
                        }
                        progressForm.PerformStep();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect elements: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CollectFamilyInstances(ElementTypeProperties etp)
        {
            try
            {
                ElementId eId = new ElementId(etp.TypeID);
                FamilyInstanceFilter filter = new FamilyInstanceFilter(doc, eId);
                FilteredElementCollector collecter = new FilteredElementCollector(doc);
                List<Element> elements = collecter.WherePasses(filter).ToElements().ToList();

                foreach (Element elem in elements)
                {
                    FamilyInstance instance = (FamilyInstance)elem;
                    InstanceProperties ip = new InstanceProperties(instance);

                    if (!familyInstances.ContainsKey(ip.CategoryName))
                    {
                        familyInstances.Add(ip.CategoryName, new Dictionary<int, InstanceProperties>());
                    }
                    
                    familyInstances[ip.CategoryName].Add(ip.InstanceID, ip);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect family instances: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CollectSystemFamilies(ElementTypeProperties etp)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector = collector.OfCategory(etp.BuiltInCategory).WhereElementIsNotElementType();
                var query = from element in collector
                            where element.GetTypeId().IntegerValue == etp.TypeID
                            select element;

                List<Element> sysInstances = query.ToList<Element>();

                foreach (Element elem in sysInstances)
                {
                    ElementProperties ep = new ElementProperties(elem);
                    if (!sysElements.ContainsKey(ep.CategoryName))
                    {
                        sysElements.Add(ep.CategoryName, new Dictionary<int, ElementProperties>());
                    }
                    sysElements[ep.CategoryName].Add(ep.ElemntID, ep);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect system families: \n"+ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CollectViews()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ElementCategoryFilter viewfilter = new ElementCategoryFilter(BuiltInCategory.OST_Views);
                ElementCategoryFilter sheetfilter = new ElementCategoryFilter(BuiltInCategory.OST_Sheets);
                LogicalOrFilter orfilter = new LogicalOrFilter(viewfilter, sheetfilter);
                collector = collector.WherePasses(orfilter).WhereElementIsNotElementType();

                var query = from element in collector
                            where viewTypes.Keys.Contains(element.GetTypeId().IntegerValue)
                            select element;

                List<Element> elements = query.ToList<Element>();

                foreach (Element elem in elements)
                {
                    Autodesk.Revit.DB.View view = elem as Autodesk.Revit.DB.View;
                    ViewProperties vp = new ViewProperties(view, doc);
                    if(!views.ContainsKey(vp.ViewID))
                    {
                        views.Add(vp.ViewID,vp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect system families: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CollectRooms()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> roomElements = collector.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements().ToList();
                foreach (Element element in roomElements)
                {
                    Room aroom = element as Room;
                    if (null != aroom)
                    {
                        RoomProperties rp = new RoomProperties(aroom, doc);
                        rooms.Add(rp.RoomID, rp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect rooms: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CollectSpaces()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> spaceElements = collector.OfCategory(BuiltInCategory.OST_MEPSpaces).WhereElementIsNotElementType().ToElements().ToList();
                foreach (Element element in spaceElements)
                {
                    Space space = element as Space;
                    if (null != space)
                    {
                        SpaceProperties sp = new SpaceProperties(space, doc);
                        spaces.Add(sp.SpaceID, sp);
                    }
                }
                progressForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect spaces: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CollectAreas()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> areaElements = collector.OfCategory(BuiltInCategory.OST_Areas).WhereElementIsNotElementType().ToElements().ToList();
                foreach (Element element in areaElements)
                {
                    Area area = element as Area;
                    if (null != area)
                    {
                        AreaProperties ap = new AreaProperties(area, doc);
                        areas.Add(ap.AreaID, ap);
                    }
                }
                progressForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect areas: \n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int SetMaxValue1()
        {
            int count = 0;
            foreach (string categoryName in elementTypes.Keys)
            {
                count += elementTypes[categoryName].Keys.Count;
            }
            return count;
        }

        private int SetMaxValue2()
        {
            int count = 0;
            foreach (string category in elementTypes.Keys)
            {
                count += elementTypes[category].Count;
            }
            foreach (string category in familyInstances.Keys)
            {
                count += familyInstances[category].Count;
            }
            foreach (string category in sysElements.Keys)
            {
                count += sysElements[category].Count;
            }
            count += rooms.Count;
            count += viewTypes.Count;
            count += views.Count;
            if (isSpaceSelected) { count += spaces.Count; }
            if (isAreaSelected) { count += areas.Count; }
                
            return count;
        }

        public void SaveRevitData()
        {
            progressForm = new form_ProgressBar();
            progressForm.Text = progressbarText;
            progressForm.MaxValue = SetMaxValue2();
            progressForm.LabelText = "Writing Revit Data in the Database...";
            progressForm.Show();
            progressForm.Refresh();

            foreach (TableDef table in daoDB.TableDefs)
            {
                if (table.Name.Contains("Type_") && !table.Name.Contains("Parameters"))
                {
                    string categoryName = table.Name.Substring(5);
                    if (elementTypes.ContainsKey(categoryName))
                    {
                        Dictionary<int, ElementTypeProperties> dictionary = new Dictionary<int, ElementTypeProperties>();
                        dictionary = elementTypes[categoryName];
                        SaveRevitDataToDatabase(table.Name, dictionary, "TypeID");
                    }
                    else if (categoryName == "Views")
                    {
                        Dictionary<int, ViewTypeProperties> dictionary = new Dictionary<int, ViewTypeProperties>();
                        dictionary = viewTypes;
                        SaveRevitDataToDatabase(table.Name, dictionary, "TypeID");
                    }
                }
                else if (table.Name.Contains("Rooms"))
                {
                    Dictionary<int, RoomProperties> dictionary = new Dictionary<int, RoomProperties>();
                    dictionary = rooms;
                    SaveRevitDataDictionary(table.Name, dictionary, "ID");
                }
                else if (table.Name.Contains("Spaces") && isSpaceSelected)
                {
                    Dictionary<int, SpaceProperties> dictionary = new Dictionary<int, SpaceProperties>();
                    dictionary = spaces;
                    SaveRevitDataDictionary(table.Name, dictionary, "ID");
                }
                else if (table.Name.Contains("Areas") && isAreaSelected)
                {
                    Dictionary<int, AreaProperties> dictionary = new Dictionary<int, AreaProperties>();
                    dictionary = areas;
                    SaveRevitDataDictionary(table.Name, dictionary, "ID");
                }
            }

            foreach (TableDef table in daoDB.TableDefs)
            {
                if (table.Name.Contains("Inst_") && !table.Name.Contains("Parameters"))
                {
                    string categoryName = table.Name.Substring(5);
                    if (excludeInstanceSettings.ContainsKey(categoryName)) { if (excludeInstanceSettings[categoryName]) { continue; } }

                    if (categoryName == "Views")
                    {
                        Dictionary<int, ViewProperties> dictionary = new Dictionary<int, ViewProperties>();
                        dictionary = views;
                        SaveRevitDataToDatabase(table.Name, dictionary, "InstanceID");
                    }
                    if (familyInstances.ContainsKey(categoryName)) 
                    {
                        Dictionary<int, InstanceProperties> dictionary = new Dictionary<int, InstanceProperties>();
                        dictionary = familyInstances[categoryName];
                        SaveRevitDataToDatabase(table.Name, dictionary, "InstanceID");
                    }
                    if (sysElements.ContainsKey(categoryName))
                    {
                        Dictionary<int, ElementProperties> dictionary = new Dictionary<int, ElementProperties>();
                        dictionary = sysElements[categoryName];
                        SaveRevitDataToDatabase(table.Name, dictionary, "InstanceID");
                    }
                }
            }

            progressForm.Close();
        }

        private double GetDoubleValue(string paramId, string asStringVal)
        {
            double dblVal = 0;
            string suffix = "";
            if (suffixDictionary.ContainsKey(paramId))
            {
                suffix = suffixDictionary[paramId];
            }
            else
            {
                suffix = unitConverter.GetSuffix(asStringVal);
                suffixDictionary.Add(paramId, suffix);
            }

            dblVal = unitConverter.GetDoubleValue(asStringVal, suffix);
            
            return dblVal;
        }

        //Element Type Data
        private void  SaveRevitDataToDatabase(string tableName, Dictionary<int, ElementTypeProperties> dictionary, string idField)
        {
            try
            {
                List<string> fieldNames = new List<string>();
                List<string> doubleFields = new List<string>();

                Recordset recordset = daoDB.OpenTable(tableName);
                foreach (Field field in recordset.Fields)
                {
                    if (field.Name.Contains("Dbl_")) { doubleFields.Add(field.Name); }
                    else { fieldNames.Add(field.Name); }
                }

                List<string> paramIds = new List<string>();
                paramIds = GetValidFields(tableName, fieldNames);//excluded non-visible and database controlled parameters: RevitOnly, LockAll, ReadOnly

                List<int> newIds = new List<int>(); 
                newIds = dictionary.Keys.ToList();
                while (!recordset.EOF)
                {
                    int id = int.Parse(recordset.Fields[idField].Value);

                    if (recordset.Updatable && paramIds.Count > 0)
                    {
                        recordset.Edit();

                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        
                        //update Revit value according to controlling parameter value
                        if (dictionary.ContainsKey(id)) 
                        {
                            paramProperties = UpdateParamToRevit(tableName, dictionary[id].ElementTypeParameters, paramIds, id, recordset.Fields);   
                        }
                        else { recordset.Delete(); recordset.MoveNext(); continue; } //delete data from database when the element no longer exists in the Revit project.

                        foreach (string paramId in paramIds)
                        {
                            int intParamId = 0;
                            int.TryParse(paramId, out intParamId);
                            if (paramProperties.ContainsKey(intParamId))
                            {
                                ParamProperties pp = paramProperties[intParamId];
                                Field field = recordset.Fields[paramId];
                                if (IsCalculatedField(field)) { continue; }
                                if (field.DataUpdatable) { field.Value = pp.ParamValue; }
                            }
                        }

                        foreach (string dblFieldName in doubleFields)
                        {
                            string paramId = dblFieldName.Replace("Dbl_", "");
                            if (paramIds.Contains(paramId))
                            {
                                Field field = recordset.Fields[paramId];
                                string strValue = Convert.ToString(field.Value);
                                Field dblField = recordset.Fields[dblFieldName];
                                if (dblField.DataUpdatable) { dblField.Value = GetDoubleValue(paramId, strValue); }
                            }
                        }

                        recordset.Update();
                    }

                    progressForm.PerformStep();
                    newIds.Remove(id);
                    recordset.MoveNext();
                }

                if (newIds.Count > 0) //record that doesn't exist in database since new added to Revit Project
                {
                    foreach (int typeId in newIds)
                    {
                        progressForm.PerformStep();

                        ElementTypeProperties etp = dictionary[typeId];
                        recordset.AddNew();
                        recordset.Fields["TypeID"].Value = etp.TypeID.ToString();
                        recordset.Fields["FamilyType"].Value = etp.ElementTypeName;
                        if (etp.IsFamilySymbol)
                        {
                            recordset.Fields["FamilyID"].Value = etp.FamilyID.ToString();
                            recordset.Fields["FamilyName"].Value = etp.FamilyName;
                        }

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = etp.ElementTypeParameters;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!fieldNames.Contains(pId.ToString())) { continue; }
                            Field field = recordset.Fields[pId.ToString()];
                            if (IsCalculatedField(field)) { continue; }
                            if (field.DataUpdatable) { field.Value = ppDic[pId].ParamValue; }
                        }
                        recordset.Update();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Revit data to database: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Family Instance Data
        private void SaveRevitDataToDatabase(string tableName, Dictionary<int, InstanceProperties> dictionary, string idField)
        {
            try
            {
                List<string> fieldNames = new List<string>();
                List<string> doubleFields = new List<string>();

                Recordset recordset = daoDB.OpenTable(tableName);
                foreach (Field field in recordset.Fields)
                {
                    if (field.Name.Contains("Dbl_")) { doubleFields.Add(field.Name); }
                    else { fieldNames.Add(field.Name); }
                }

                List<string> paramIds = new List<string>();
                paramIds = GetValidFields(tableName, fieldNames);

                List<int> newIds = new List<int>();
                newIds = dictionary.Keys.ToList();

                while (!recordset.EOF)
                {
                    int id = int.Parse(recordset.Fields[idField].Value);
                    if (recordset.Updatable && paramIds.Count > 0)
                    {
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        if (dictionary.ContainsKey(id)) { paramProperties = dictionary[id].InstParameters; }
                        else { recordset.Delete(); recordset.MoveNext(); continue; } //delete data from database when the element no longer exists in the Revit project.
                        
                        recordset.Edit();
                        foreach (string paramId in paramIds)
                        {
                            int intParamId = 0;
                            int.TryParse(paramId, out intParamId);
                            if (paramProperties.ContainsKey(intParamId))
                            {
                                Field field = recordset.Fields[paramId];
                                if (IsCalculatedField(field)) { continue; }
                                if (field.DataUpdatable) { field.Value = paramProperties[intParamId].ParamValue; }
                            }
                        }

                        foreach (string dblFieldName in doubleFields) //get double values from AsString fields
                        {
                            string paramId = dblFieldName.Replace("Dbl_", "");
                            if (paramIds.Contains(paramId))
                            {
                                Field field = recordset.Fields[paramId];
                                string strValue = Convert.ToString(field.Value);
                                Field dblField = recordset.Fields[dblFieldName];
                                if (dblField.DataUpdatable) { dblField.Value = GetDoubleValue(paramId, strValue); }
                            }
                        }

                        recordset.Update();
                    }
                    progressForm.PerformStep();
                    newIds.Remove(id);
                    recordset.MoveNext();
                }

                if (newIds.Count > 0) //record that doesn't exist in database since newly added to Revit Project
                {
                    foreach (int instId in newIds)
                    {
                        progressForm.PerformStep();

                        InstanceProperties ip = dictionary[instId];
                        recordset.AddNew();
                        recordset.Fields["InstanceID"].Value = ip.InstanceID.ToString();
                        recordset.Fields["TypeID"].Value = ip.TypeID.ToString();
                        recordset.Fields["FamilyID"].Value = ip.FamilyID.ToString() ;
                        recordset.Fields["FamilyName"].Value = ip.FamilyName;
                        recordset.Fields["FamilyType"].Value = ip.FamilyTypeName;

                        recordset.Fields["FacingFlipped"].Value = ip.FacingFlipped.ToString();
                        recordset.Fields["HandFlipped"].Value = ip.HandFlipped.ToString();
                        recordset.Fields["IsSlantedColumn"].Value = ip.IsSlantedColumn.ToString();
                        recordset.Fields["Mirrored"].Value = ip.Mirrored.ToString();
                        if (null != ip.FromRoom) { recordset.Fields["FromRoom"].Value = ip.FromRoom.Id.ToString(); }
                        if (null != ip.Host) { recordset.Fields["Host"].Value = ip.Host.Id.ToString(); }
                        if (null != ip.Room) { recordset.Fields["Room"].Value = ip.Room.Id.ToString(); }
                        if (null != ip.ToRoom) { recordset.Fields["ToRoom"].Value = ip.ToRoom.Id.ToString(); }

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = ip.InstParameters; ;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!fieldNames.Contains(pId.ToString())) { continue; }
                            Field field = recordset.Fields[pId.ToString()];
                            if (IsCalculatedField(field)) { continue; }
                            if (field.DataUpdatable) { field.Value = ppDic[pId].ParamValue; }
                        }
                        recordset.Update();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Revit data to database: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //System Family Instance Data
        private void SaveRevitDataToDatabase(string tableName, Dictionary<int, ElementProperties> dictionary, string idField)
        {
            try
            {
                List<string> fieldNames = new List<string>();
                List<string> doubleFields = new List<string>();

                Recordset recordset = daoDB.OpenTable(tableName);
                foreach (Field field in recordset.Fields)
                {
                    if (field.Name.Contains("Dbl_")) { doubleFields.Add(field.Name); }
                    else { fieldNames.Add(field.Name); }
                }

                List<string> paramIds = new List<string>();
                paramIds = GetValidFields(tableName,fieldNames);

                List<int> newIds = new List<int>();
                newIds = dictionary.Keys.ToList();

                while (!recordset.EOF)
                {
                    int id = int.Parse(recordset.Fields[idField].Value);
                    if (recordset.Updatable && paramIds.Count > 0)
                    {
                        
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        if (dictionary.ContainsKey(id)) { paramProperties = dictionary[id].ElementParameters; }
                        else { recordset.Delete(); recordset.MoveNext(); continue; } //delete data from database when the element no longer exists in the Revit project.
                        
                        recordset.Edit();
                        foreach (string paramId in paramIds)
                        {
                            int intParamId = 0;
                            int.TryParse(paramId, out intParamId);
                            if (paramProperties.ContainsKey(intParamId))
                            {
                                Field field = recordset.Fields[paramId];
                                if (IsCalculatedField(field)) { continue; }
                                if (field.DataUpdatable) { field.Value = paramProperties[intParamId].ParamValue; }
                            }
                        }

                        foreach (string dblFieldName in doubleFields)
                        {
                            string paramId = dblFieldName.Replace("Dbl_", "");
                            if (paramIds.Contains(paramId))
                            {
                                Field field = recordset.Fields[paramId];
                                string strValue = Convert.ToString(field.Value);
                                Field dblField = recordset.Fields[dblFieldName];
                                if (dblField.DataUpdatable) { dblField.Value = GetDoubleValue(paramId, strValue); }
                            }
                        }
                        recordset.Update();
                    }
                    progressForm.PerformStep();
                    newIds.Remove(id);
                    recordset.MoveNext();
                }

                if (newIds.Count > 0) //record that doesn't exist in database since newly added to Revit Project
                {
                    foreach (int instId in newIds)
                    {
                        progressForm.PerformStep();

                        ElementProperties ep = dictionary[instId];
                        recordset.AddNew();
                        recordset.Fields["InstanceID"].Value = ep.ElemntID.ToString();
                        recordset.Fields["TypeID"].Value = ep.TypeID.ToString();
                        recordset.Fields["FamilyType"].Value = ep.TypeName;

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = ep.ElementParameters;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!fieldNames.Contains(pId.ToString())) { continue; }
                            Field field = recordset.Fields[pId.ToString()];
                            if (IsCalculatedField(field)) { continue; }
                            if (field.DataUpdatable) { field.Value = ppDic[pId].ParamValue; }
                        }
                        recordset.Update();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Revit data to database: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //View Type Data
        private void SaveRevitDataToDatabase(string tableName, Dictionary<int, ViewTypeProperties> dictionary, string idField)
        {
            try
            {
                List<string> fieldNames = new List<string>();
                List<string> doubleFields = new List<string>();

                Recordset recordset = daoDB.OpenTable(tableName);
                foreach (Field field in recordset.Fields)
                {
                    if (field.Name.Contains("Dbl_")) { doubleFields.Add(field.Name); }
                    else { fieldNames.Add(field.Name); }
                }

                List<string> paramIds = new List<string>();
                paramIds = GetValidFields(tableName, fieldNames);

                List<int> newIds = new List<int>();
                newIds = dictionary.Keys.ToList();

                while (!recordset.EOF)
                {
                    int id = int.Parse(recordset.Fields[idField].Value);
                    if (recordset.Updatable && paramIds.Count > 0)
                    {
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        if (dictionary.ContainsKey(id)) { paramProperties = dictionary[id].ViewTypeParameters; }
                        else { recordset.Delete(); recordset.MoveNext(); continue; } //delete data from database when the element no longer exists in the Revit project.
                        
                        recordset.Edit();
                        foreach (string paramId in paramIds)
                        {
                            int intParamId = 0;
                            int.TryParse(paramId, out intParamId);
                            if (paramProperties.ContainsKey(intParamId))
                            {
                                Field field = recordset.Fields[paramId];
                                if (IsCalculatedField(field)) { continue; }
                                if (field.DataUpdatable) { field.Value = paramProperties[intParamId].ParamValue; }
                            }
                        }

                        foreach (string dblFieldName in doubleFields)
                        {
                            string paramId = dblFieldName.Replace("Dbl_", "");
                            if (paramIds.Contains(paramId))
                            {
                                Field field = recordset.Fields[paramId];
                                string strValue = Convert.ToString(field.Value);
                                Field dblField = recordset.Fields[dblFieldName];
                                if (dblField.DataUpdatable) { dblField.Value = GetDoubleValue(paramId, strValue); }
                            }
                        }

                        recordset.Update();
                    }
                    progressForm.PerformStep();
                    newIds.Remove(id);
                    recordset.MoveNext();
                }

                if (newIds.Count > 0) //record that doesn't exist in database since newly added to Revit Project
                {
                    foreach (int typeId in newIds)
                    {
                        progressForm.PerformStep();

                        ViewTypeProperties vtp = dictionary[typeId];
                        recordset.AddNew();
                        recordset.Fields["TypeID"].Value = vtp.ViewTypeID.ToString();
                        recordset.Fields["ViewTypeName"].Value = vtp.ViewTypeName;
                        recordset.Fields["ViewFamilyName"].Value = vtp.ViewFamilyName;

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = vtp.ViewTypeParameters;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!fieldNames.Contains(pId.ToString())) { continue; }
                            Field field = recordset.Fields[pId.ToString()];
                            if (IsCalculatedField(field)) { continue; }
                            if (field.DataUpdatable) { field.Value = ppDic[pId].ParamValue; }
                        }
                        recordset.Update();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Revit data to database: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //View Instance Data
        private void SaveRevitDataToDatabase(string tableName, Dictionary<int, ViewProperties> dictionary, string idField)
        {
            try
            {
                List<string> fieldNames = new List<string>();
                List<string> doubleFields = new List<string>();

                Recordset recordset = daoDB.OpenTable(tableName);
                foreach (Field field in recordset.Fields)
                {
                    if (field.Name.Contains("Dbl_")) { doubleFields.Add(field.Name); }
                    else { fieldNames.Add(field.Name); }
                }

                List<string> paramIds = new List<string>();
                paramIds = GetValidFields(tableName, fieldNames);

                List<int> newIds = new List<int>();
                newIds = dictionary.Keys.ToList();

                while (!recordset.EOF)
                {
                    int id = int.Parse(recordset.Fields[idField].Value);
                    if (recordset.Updatable && paramIds.Count > 0)
                    {
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        if (dictionary.ContainsKey(id)) { paramProperties = dictionary[id].ViewParameters; }
                        else { recordset.Delete(); recordset.MoveNext(); continue; } //delete data from database when the element no longer exists in the Revit project.
                        
                        recordset.Edit();
                        foreach (string paramId in paramIds)
                        {
                            int intParamId = 0;
                            int.TryParse(paramId, out intParamId);
                            if (paramProperties.ContainsKey(intParamId))
                            {
                                Field field = recordset.Fields[paramId];
                                if (IsCalculatedField(field)) { continue; }
                                if (field.DataUpdatable) { field.Value = paramProperties[intParamId].ParamValue; }
                            }
                        }

                        foreach (string dblFieldName in doubleFields)
                        {
                            string paramId = dblFieldName.Replace("Dbl_", "");
                            if (paramIds.Contains(paramId))
                            {
                                Field field = recordset.Fields[paramId];
                                string strValue = Convert.ToString(field.Value);
                                Field dblField = recordset.Fields[dblFieldName];
                                if (dblField.DataUpdatable) { dblField.Value = GetDoubleValue(paramId, strValue); }
                            }
                        }
                        recordset.Update();
                    }
                    progressForm.PerformStep();
                    newIds.Remove(id);
                    recordset.MoveNext();
                }

                if (newIds.Count > 0) //record that doesn't exist in database since newly added to Revit Project
                {
                    foreach (int instId in newIds)
                    {
                        progressForm.PerformStep();

                        ViewProperties vp = dictionary[instId];
                        recordset.AddNew();
                        recordset.Fields["InstanceID"].Value = vp.ViewID.ToString();
                        recordset.Fields["TypeID"].Value = vp.TypeID.ToString();
                        recordset.Fields["ViewTypeName"].Value = vp.ViewTypeName;

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = vp.ViewParameters;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!fieldNames.Contains(pId.ToString())) { continue; }
                            Field field = recordset.Fields[pId.ToString()];
                            if (IsCalculatedField(field)) { continue; }
                            if (field.DataUpdatable) { field.Value = ppDic[pId].ParamValue; }
                        }
                        recordset.Update();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Revit data to database: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Rooms
        private void SaveRevitDataDictionary(string tableName, Dictionary<int, RoomProperties> dictionary, string idField)
        {
            try
            {
                List<string> fieldNames = new List<string>();
                List<string> doubleFields = new List<string>();

                Recordset recordset = daoDB.OpenTable(tableName);
                foreach (Field field in recordset.Fields)
                {
                    if (field.Name.Contains("Dbl_")) { doubleFields.Add(field.Name); }
                    else { fieldNames.Add(field.Name); }
                }

                List<string> paramIds = new List<string>();
                paramIds = GetValidFields(tableName,fieldNames);

                List<int> newIds = new List<int>();
                newIds = dictionary.Keys.ToList();
                while (!recordset.EOF)
                {
                    int id = int.Parse(recordset.Fields[idField].Value);
                    if (recordset.Updatable && paramIds.Count > 0)
                    {
                        recordset.Edit();
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        if (dictionary.ContainsKey(id)) { paramProperties = UpdateParamToRevit(tableName, dictionary[id].RoomParameters, paramIds, id, recordset.Fields); }
                        else { recordset.Delete(); recordset.MoveNext(); continue; } //delete data from database when the element no longer exists in the Revit project.

                        foreach (string paramId in paramIds)
                        {
                            int intParamId = 0;
                            int.TryParse(paramId, out intParamId);
                            if (paramProperties.ContainsKey(intParamId))
                            {
                                ParamProperties pp = paramProperties[intParamId];
                                Field field = recordset.Fields[paramId];
                                if (IsCalculatedField(field)) { continue; }
                                if (field.DataUpdatable) 
                                {
                                    if (pp.ParamName == "Level") { field.Value = dictionary[id].Level; }
                                    else if (pp.ParamName == "Upper Limit") { field.Value = dictionary[id].UpperLimit; }
                                    else { field.Value = paramProperties[intParamId].ParamValue; }
                                }
                            }
                        }

                        foreach (string dblFieldName in doubleFields)
                        {
                            string paramId = dblFieldName.Replace("Dbl_", "");
                            if (paramIds.Contains(paramId))
                            {
                                Field field = recordset.Fields[paramId];
                                string strValue = Convert.ToString(field.Value);
                                Field dblField = recordset.Fields[dblFieldName];
                                if (dblField.DataUpdatable) { dblField.Value = GetDoubleValue(paramId, strValue); }
                            }
                        }

                        recordset.Update();
                    }

                    progressForm.PerformStep();
                    newIds.Remove(id);
                    recordset.MoveNext();
                }

                if (newIds.Count > 0) //record that doesn't exist in database since newly added to Revit Project
                {
                    foreach (int roomId in newIds)
                    {
                        progressForm.PerformStep();

                        RoomProperties rp = dictionary[roomId];
                        recordset.AddNew();
                        recordset.Fields["ID"].Value = rp.RoomID.ToString();

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = rp.RoomParameters;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!fieldNames.Contains(pId.ToString())) { continue; }
                            Field field = recordset.Fields[pId.ToString()];
                            if (IsCalculatedField(field)) { continue; }
                            if (field.DataUpdatable) { field.Value = ppDic[pId].ParamValue; }
                        }
                        recordset.Update();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Revit data to database: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Spaces
        private void SaveRevitDataDictionary(string tableName, Dictionary<int, SpaceProperties> dictionary, string idField)
        {
            try
            {
                List<string> fieldNames = new List<string>();
                List<string> doubleFields = new List<string>();

                Recordset recordset = daoDB.OpenTable(tableName);
                foreach (Field field in recordset.Fields)
                {
                    if (field.Name.Contains("Dbl_")) { doubleFields.Add(field.Name); }
                    else { fieldNames.Add(field.Name); }
                }

                List<string> paramIds = new List<string>();
                paramIds = GetValidFields(tableName, fieldNames);

                List<int> newIds = new List<int>();
                newIds = dictionary.Keys.ToList();
                while (!recordset.EOF)
                {

                    int id = int.Parse(recordset.Fields[idField].Value);

                    if (recordset.Updatable && paramIds.Count > 0)
                    {
                        recordset.Edit();
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        if (dictionary.ContainsKey(id)) { paramProperties = UpdateParamToRevit(tableName, dictionary[id].SpaceParameters, paramIds, id, recordset.Fields); }
                        else { recordset.Delete(); recordset.MoveNext(); continue; } //delete data from database when the element no longer exists in the Revit project.

                        foreach (string paramId in paramIds)
                        {
                            int intParamId = 0;
                            int.TryParse(paramId, out intParamId);
                            if (paramProperties.ContainsKey(intParamId))
                            {
                                Field field = recordset.Fields[paramId];
                                if (IsCalculatedField(field)) { continue; }
                                if (field.DataUpdatable) { field.Value = paramProperties[intParamId].ParamValue; }
                            }
                        }

                        foreach (string dblFieldName in doubleFields)
                        {
                            string paramId = dblFieldName.Replace("Dbl_", "");
                            if (paramIds.Contains(paramId))
                            {
                                Field field = recordset.Fields[paramId];
                                string strValue = Convert.ToString(field.Value);
                                Field dblField = recordset.Fields[dblFieldName];
                                if (dblField.DataUpdatable) { dblField.Value = GetDoubleValue(paramId, strValue); }
                            }
                        }

                        recordset.Update();
                    }
                    progressForm.PerformStep();
                    newIds.Remove(id);
                    recordset.MoveNext();
                }

                if (newIds.Count > 0) //record that doesn't exist in database since newly added to Revit Project
                {
                    foreach (int spaceId in newIds)
                    {
                        progressForm.PerformStep();

                        SpaceProperties sp = dictionary[spaceId];
                        recordset.AddNew();
                        recordset.Fields["ID"].Value = sp.SpaceID.ToString();

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = sp.SpaceParameters;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!fieldNames.Contains(pId.ToString())) { continue; }
                            Field field = recordset.Fields[pId.ToString()];
                            if (IsCalculatedField(field)) { continue; }
                            if (field.DataUpdatable) { field.Value = ppDic[pId].ParamValue; }
                        }
                        recordset.Update();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Revit data to database: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Areas
        private void SaveRevitDataDictionary(string tableName, Dictionary<int, AreaProperties> dictionary, string idField)
        {
            try
            {
                List<string> fieldNames = new List<string>();
                List<string> doubleFields = new List<string>();

                Recordset recordset = daoDB.OpenTable(tableName);
                foreach (Field field in recordset.Fields)
                {
                    if (field.Name.Contains("Dbl_")) { doubleFields.Add(field.Name); }
                    else { fieldNames.Add(field.Name); }
                }

                List<string> paramIds = new List<string>();
                paramIds = GetValidFields(tableName, fieldNames);

                List<int> newIds = new List<int>();
                newIds = dictionary.Keys.ToList();
                while (!recordset.EOF)
                {

                    int id = int.Parse(recordset.Fields[idField].Value);

                    if (recordset.Updatable && paramIds.Count > 0)
                    {
                        recordset.Edit();
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        if (dictionary.ContainsKey(id)) { paramProperties = UpdateParamToRevit(tableName, dictionary[id].AreaParameters, paramIds, id, recordset.Fields); }
                        else { recordset.Delete(); recordset.MoveNext(); continue; } //delete data from database when the element no longer exists in the Revit project.

                        foreach (string paramId in paramIds)
                        {
                            int intParamId = 0;
                            int.TryParse(paramId, out intParamId);
                            if (paramProperties.ContainsKey(intParamId))
                            {
                                Field field = recordset.Fields[paramId];
                                if (IsCalculatedField(field)) { continue; }
                                if (field.DataUpdatable) { field.Value = paramProperties[intParamId].ParamValue; }
                            }
                        }

                        foreach (string dblFieldName in doubleFields)
                        {
                            string paramId = dblFieldName.Replace("Dbl_", "");
                            if (paramIds.Contains(paramId))
                            {
                                Field field = recordset.Fields[paramId];
                                string strValue = Convert.ToString(field.Value);
                                Field dblField = recordset.Fields[dblFieldName];
                                if (dblField.DataUpdatable) { dblField.Value = GetDoubleValue(paramId, strValue); }
                            }
                        }
                        recordset.Update();
                    }
                    progressForm.PerformStep();
                    newIds.Remove(id);
                    recordset.MoveNext();
                }

                if (newIds.Count > 0) //record that doesn't exist in database since newly added to Revit Project
                {
                    foreach (int areaId in newIds)
                    {
                        progressForm.PerformStep();

                        AreaProperties ap = dictionary[areaId];
                        recordset.AddNew();
                        recordset.Fields["ID"].Value = ap.AreaID.ToString();

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = ap.AreaParameters;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!fieldNames.Contains(pId.ToString())) { continue; }
                            Field field = recordset.Fields[pId.ToString()];
                            if (IsCalculatedField(field)) { continue; }
                            if (field.DataUpdatable) { field.Value = ppDic[pId].ParamValue; }
                        }
                        recordset.Update();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Revit data to database: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Dictionary<int, ParamProperties> UpdateParamToRevit (string tableName, Dictionary<int, ParamProperties> properties, List<string> paramIds/*RevitOnly*/, int eId/*elementId*/, Fields fields)
        {
            Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
            paramProperties = properties;


            if (externalFields.ContainsKey(tableName) && sourceTables.ContainsKey(tableName))
            {
                ElementId elementID = new ElementId(eId);
                Element element = doc.GetElement(elementID);

                if (null != element)
                {
                    LinkedParameter lp = externalFields[tableName];
                    DataTable sourceTable = sourceTables[tableName];

                    var controlProperty = from property in paramProperties.Values
                                          where property.ParamName == lp.ControlParameter
                                          select property;
                    ParamProperties cp = controlProperty.First();

                    //null value in controlling fiedld will be ignored
                    if (!paramIds.Contains(cp.ParamID.ToString()) || cp.ParamValue == string.Empty || cp.ParamValue == null) { return paramProperties; }

                    var sourceRow = from row in sourceTable.AsEnumerable()
                                    where row.Field<string>(lp.ControlField) == cp.ParamValue
                                    select row;

                    if (sourceRow.Count() > 0)
                    {
                        DataRow dataRow = sourceRow.First(); //datarow that contains control parameter value

                        foreach (string updateParam in lp.UpdateParameterField.Keys)
                        {
                            var updateProperty = from property in paramProperties.Values
                                                 where property.ParamName == updateParam
                                                 select property;
                            ParamProperties up = updateProperty.First();

                            string updateField = lp.UpdateParameterField[updateParam];

                            if (up.ParamValue != dataRow[updateField].ToString())
                            {
                                up.ParamValue = dataRow[updateField].ToString();
                                paramProperties[up.ParamID] = up;
                                //Overwrite value of updating parameters in the updating field of the Database
                                Field field = fields[up.ParamID.ToString()];
                                if (field.DataUpdatable) { field.Value = up.ParamValue; }

                                //Overwrite Revit value of updating parameters, when the param setting is RevitControlled. 
                                if (paramIds.Contains(up.ParamID.ToString())) 
                                {
#if RELEASE2013 ||RELEASE2014
                                    Autodesk.Revit.DB.Parameter parameter = element.get_Parameter(updateParam);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017 || RELEASE2018
                                    Autodesk.Revit.DB.Parameter parameter = element.LookupParameter(updateParam);
#endif
                                    
                                    if (null != parameter && !parameter.IsReadOnly && sourceTable.Columns.Contains(updateField))
                                    {
                                        switch (parameter.StorageType)
                                        {
                                            case StorageType.Double:
                                                double dblVal = parameter.AsDouble();
                                                double dblDBVal = Convert.ToDouble(dataRow[updateField]);
                                                if (dblVal != dblDBVal) { parameter.Set(dblDBVal); }
                                                break;
                                            case StorageType.Integer:
                                                int intVal = parameter.AsInteger();
                                                int intDBVal = Convert.ToInt32(dataRow[updateField]);
                                                if (intVal != intDBVal) { parameter.Set(intDBVal); }
                                                break;
                                            case StorageType.String:
                                                string strVal = parameter.AsString();
                                                string strDBVal = Convert.ToString(dataRow[updateField]);
                                                if (strVal != strDBVal) { parameter.Set(strDBVal); }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else//when the controlling parameter value entered by users doesn't exist in the source table 
                    {
                        if (setFixMode == false)
                        {
                            TaskDialog taskDialog = new TaskDialog("Refernec Database");
                            taskDialog.MainInstruction="Controlling Parameters are Mismatched.";
                            taskDialog.MainContent = "The modified Revit values are not matched to referencing fields.\n Would you like to open the Data Editor in Fixing Mode?";
                            taskDialog.AllowCancellation = true;
                            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Open in Fixing Mode");
                            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Ignore Mismatched Data");

                            TaskDialogResult tResult = taskDialog.Show();
                            if (TaskDialogResult.CommandLink1 == tResult)
                            {
                                fixMode = true;
                            }
                            else if (TaskDialogResult.CommandLink2 == tResult)
                            {
                                fixMode = false;
                            }
                            
                            setFixMode = true;
                        }
                        if (!warningTables.Contains(tableName)) { warningTables.Add(tableName); }
                    }
                }
                
            }

            return paramProperties;
        }

        private List<string> GetValidFields(string tableName, List<string> fieldNames)
        {
            List<string> paramIds = new List<string>();
            try
            {
                List<string> nonVisibleParams = new List<string>();
                if (nonVisibleFields.ContainsKey(tableName))
                {
                    nonVisibleParams = nonVisibleFields[tableName]; //non-visible parameter will be skip 
                }

                if (lockTypeFields.ContainsKey(tableName))
                {
                    foreach (string paramName in lockTypeFields[tableName].Keys)
                    {
                        if (nonVisibleParams.Contains(paramName)) { continue; }
                        LockType lockType = lockTypeFields[tableName][paramName];
                        if (lockType == LockType.Editable || lockType == LockType.ReadOnly || lockType == LockType.LockAll) //Excluding Database Controlled Parameters
                        {
                            string paramId = GetParameterId(tableName, paramName);
                            if (fieldNames.Contains(paramId))
                            {
                                paramIds.Add(paramId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get valid fields: " + tableName + "\n" + ex.Message, "ReadFromRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return paramIds;
        }

        private string GetParameterId(string tableName, string paramName)
        {
            string paramId = "";
            try
            {
                string prefix = tableName.Split('_')[0];
                string categoryName = tableName.Split('_')[1];

                if (categoryName.Contains("Rooms") || categoryName.Contains("Spaces")||categoryName.Contains("Areas")) { prefix = "Type"; }
                string paramTable = "Rvt_" + prefix + "_Parameters";
                string strSql = "SELECT ParamID FROM " + paramTable + " WHERE CategoryName='" + categoryName + "' AND ParamName ='" + paramName + "'";
                Recordset recordset = daoDB.OpenRecordset(strSql);
                if (recordset.RecordCount > 0)
                {
                    paramId = recordset.Fields["ParamID"].Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter Id from parameter name. \n" + ex.Message, "RevitDBViewer Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return paramId;
        }

        private bool IsCalculatedField(Field field)
        {
            bool calculated = false;
            Field2 field2 = field as Field2;
            if (field2.Expression != string.Empty)
            {
                calculated = true;
            }
            return calculated;
        }
    }
}
