using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;

using System.Data.OleDb;
using System.Data;

using System.Windows.Forms;
using Access = Microsoft.Office.Interop.Access;
using Microsoft.Office.Interop.Access.Dao;
using Autodesk.Revit.DB;
using System.IO;
using System.Diagnostics;
using RevitDBManager.ManagerForms;



namespace RevitDBManager.Classes
{
    public class RevitDBCreator
    {
        private UIApplication uiapp;
        private string inputDir = "";
        private Dictionary<string/*category*/,Dictionary<int/*typeID*/, TypeProperties>> typeDictionary = new Dictionary<string,Dictionary<int, TypeProperties>>();
        private Dictionary<string/*category*/, Dictionary<int/*instanceID*/, InstanceProperties>> instanceDictionary = new Dictionary<string, Dictionary<int, InstanceProperties>>();
        private Dictionary<string/*category*/, Dictionary<int/*typeID*/, ElementTypeProperties>> sysTypeDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>();
        private Dictionary<string/*category*/, Dictionary<int/*elementID*/, ElementProperties>> sysInstDictionary = new Dictionary<string, Dictionary<int, ElementProperties>>();
        private Dictionary<int/*roomID*/, RoomProperties> roomDictionary = new Dictionary<int, RoomProperties>();
        private Dictionary<int/*spaceID*/,SpaceProperties> spaceDictionary=new Dictionary<int,SpaceProperties>();
        private Dictionary<int/*areaID*/, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();
        private Dictionary<int, ViewTypeProperties> viewTypeDictionary = new Dictionary<int, ViewTypeProperties>();//selected view family types
        private Dictionary<int, ViewProperties> viewInstDictionary = new Dictionary<int, ViewProperties>();//selected views
        private Dictionary<int/*paramID*/, ParamProperties> paramInfoDictionary = new Dictionary<int, ParamProperties>();
        private Dictionary<string/*categoryName*/, bool/*excludeInstance*/> excludeInstanceSettings = new Dictionary<string, bool>();

        private Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> instParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
        private Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> typeParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
        //view family-defined parameters
        private Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> viewTypeParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> viewInstParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();

        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> instCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<int/*category or family ID*/, SyncProperties> syncDictionary = new Dictionary<int, SyncProperties>();
        private Dictionary<string/*paramName*/, LinkedParameter> linkedParameters = new Dictionary<string, LinkedParameter>();

        private string[] internalField = new string[] { "FacingFlipped", "FromRoom", "HandFlipped", "Host", "IsSlantedColumn", "Mirrored", "Room", "ToRoom" };
        private string[] systemField = new string[] { "InstanceID", "TypeID", "FamilyID", "FamilyName", "FamilyType" };
        private string[] viewField = new string[] { "TypeID", "ViewTypeName", "ViewFamilyName" };
        private Access.Application oAccess;
        private Database daoDB;
        private object mMissing = System.Reflection.Missing.Value;
        private TableDef tblDef;
        private Field field;
        private Relation relation;
        private bool isSpaceSelected = false;
        private bool isAreaSelected = false;
        private bool isRoomSelected = false;
        private ToolStripProgressBar progressBar;

        #region Public Variables
        public Dictionary<string, Dictionary<int, TypeProperties>> TypeDictionary { get { return typeDictionary; } set { typeDictionary = value; } }
        public Dictionary<string, Dictionary<int, InstanceProperties>> InstanceDictionary { get { return instanceDictionary; } set { instanceDictionary = value; } }
        public Dictionary<string, Dictionary<int, ElementTypeProperties>> SysTypeDictionary { get { return sysTypeDictionary; } set { sysTypeDictionary = value; } }
        public Dictionary<string, Dictionary<int, ElementProperties>> SysInstDictionary { get { return sysInstDictionary; } set { sysInstDictionary = value; } }
        public Dictionary<int, RoomProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }
        public Dictionary<int, SpaceProperties> SpaceDictionary { get { return spaceDictionary; } set { spaceDictionary = value; } }
        public Dictionary<int, AreaProperties> AreaDictionary { get { return areaDictionary; } set { areaDictionary = value; } }
        public Dictionary<int, ParamProperties> ParamInfoDictionary { get { return paramInfoDictionary; } set { paramInfoDictionary = value; } }
        public Dictionary<int, ViewTypeProperties> ViewTypeDictionary { get { return viewTypeDictionary; } set { viewTypeDictionary = value; } }
        public Dictionary<int, ViewProperties> ViewInstDictionary { get { return viewInstDictionary; } set { viewInstDictionary = value; } }
        public Dictionary<int, Dictionary<int, ParamProperties>> InstanceParamSettings { get { return instParamSettings; } set { instParamSettings = value; } }
        public Dictionary<int, Dictionary<int, ParamProperties>> TypeParamSettings { get { return typeParamSettings; } set { typeParamSettings = value; } }
        public Dictionary<string, Dictionary<int, ParamProperties>> ViewInstanceParamSettings { get { return viewInstParamSettigns; } set { viewInstParamSettigns = value; } }
        public Dictionary<string, Dictionary<int, ParamProperties>> ViewTypeParamSettings { get { return viewTypeParamSettigns; } set { viewTypeParamSettigns = value; } }
        public Dictionary<string, Dictionary<int, ParamProperties>> InstCategoryParamSettings { get { return instCatParamSettings; } set { instCatParamSettings = value; } }
        public Dictionary<string, Dictionary<int, ParamProperties>> TypeCategoryParamSettings { get { return typeCatParamSettings; } set { typeCatParamSettings = value; } }
        public Dictionary<int, SyncProperties> SyncDictionary { get { return syncDictionary; } set { syncDictionary = value; } }
        public Dictionary<string, LinkedParameter> LinkedParameters { get { return linkedParameters; } set { linkedParameters = value; } }
        public Dictionary<string, bool> ExcludeInstSettings { get { return excludeInstanceSettings; } set { excludeInstanceSettings = value; } }
        public ToolStripProgressBar ProgressBar { get { return progressBar; } set { progressBar = value; } }
        #endregion

        public RevitDBCreator(UIApplication uiapplication, string input, bool isEditMode)
        {
            try
            {
                uiapp = uiapplication;
                inputDir = input;
                
                oAccess = new Access.Application();
                if (isEditMode) { oAccess.OpenCurrentDatabase(inputDir,false); }
                else { oAccess.NewCurrentDatabase(inputDir); }
                oAccess.Visible = false;
                daoDB = oAccess.CurrentDb();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot start RevitDBCreator: \n" + ex.Message);
                CloseDatabase();
            }
        }

        public bool CreateTables()
        {
            bool result = true;
            try
            {
                if (instCatParamSettings.ContainsKey("Spaces") || typeCatParamSettings.ContainsKey("Spaces")) { isSpaceSelected = true; }
                if (instCatParamSettings.ContainsKey("Areas") || typeCatParamSettings.ContainsKey("Areas")) { isAreaSelected = true; }

                foreach (string category in typeDictionary.Keys)
                {
                    if (!CreateInstanceTable(category)) { result = false; return false; }
                    if (!CreateTypeTable(category)) { result = false; return false; }
                    //if (!CreateRelationship(category)) { result = false; return false; }
                }

                foreach (string category in sysTypeDictionary.Keys)
                {
                    if (!CreateSysInstanceTable(category)) { result = false; return false; }
                    if (!CreateSysTypeTable(category)) { result = false; return false; }
                    //if (!CreateRelationship(category)) { result = false; return false; }
                }

                if (viewTypeDictionary.Count > 0)
                {
                    if (!CreateViewInstanceTable()) { result = false; return false; }
                    if (!CreateViewTypeTable()) { result = false; return false; }
                    //if (!CreateRelationship("Views")) { result = false; return false; }
                }
                
                if (!CreateRoomTable()) { result = false; return false; }
                if (isSpaceSelected) { if (!CreateSpaceTable()) { result = false; return false; } }
                if (isAreaSelected) { if (!CreateAreaTable()) { result = false; return false; } }
                if (!CreateParameterInfoTable()) { result = false; return false; }
                if (!CreateParamSettingTable("Rvt_Inst_Parameters", instCatParamSettings, instParamSettings, viewInstParamSettigns)) { result = false; return false; }
                if (!CreateParamSettingTable("Rvt_Type_Parameters", typeCatParamSettings, typeParamSettings, viewTypeParamSettigns)) { result = false; return false; }
                if (!CreateFamilyInfoTable()) { result = false; return false; }
                if (!CreateRevitInternalTable()) { result = false; return false; }
                if (!CreateAutoSyncTable()) { result = false; return false; }
                if (!CreateExternalRefTable()) { result = false; return false; }
                if (!CreateExcludeInstTable()) { result = false; return false; }

                progressBar.Value = progressBar.Maximum; 
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create tables: \n" + ex.Message);
                result = false;
            }

            return result;
        }

        public bool UpdateTables()
        {
            bool result = true;
            try
            {
                if (instCatParamSettings.ContainsKey("Rooms") || typeCatParamSettings.ContainsKey("Rooms")) { isRoomSelected = true; }
                if (instCatParamSettings.ContainsKey("Spaces") || typeCatParamSettings.ContainsKey("Spaces")) { isSpaceSelected = true; }
                if (instCatParamSettings.ContainsKey("Areas") || typeCatParamSettings.ContainsKey("Areas")) { isAreaSelected = true; }

                if (!DeleteTables()) { result = false; } //when categories no longer exist
                if (!DeleteAllContents()) { result = false; }
                List<string> addedCategories = new List<string>();
                addedCategories = AddTables(); //create tables and write data in them

                if (!WriteParamData("Rvt_ParameterInfo")) { result = false; }
                if (!WriteCategoryParamSetting("Rvt_Inst_Parameters", instCatParamSettings)) { result = false; }
                if (!WriteFamilyParamSetting("Rvt_Inst_Parameters", instParamSettings, viewInstParamSettigns)) { result = false; }

                if (!WriteCategoryParamSetting("Rvt_Type_Parameters", typeCatParamSettings)) { result = false; }
                if (!WriteFamilyParamSetting("Rvt_Type_Parameters", typeParamSettings, viewTypeParamSettigns)) { result = false; }

                if (!WriteFamilyData("Rvt_FamilyInfo")) { result = false; }
                if (!WriteAutoSyncData("UI_AutoSyncOptions")) { result = false; }
                if (!WriteReferenceData("UI_ExternalReference")) { result = false; }
                if (!WriteExcludeInstData("UI_ExcludeInstance")) { result = false; }

                foreach (string category in typeDictionary.Keys)
                {
                    if (addedCategories.Contains(category)) { continue; }

                    string tableName = "Type_" + category;
                    if (!UpdateFields(tableName, category, "Rvt_Type_Parameters")) { result = false; }
                    if (!WriteTypeData(tableName, category)) { result = false; }

                    tableName = "Inst_" + category;
                    if (!UpdateFields(tableName, category, "Rvt_Inst_Parameters")) { result = false; }
                    if (!WriteInstanceData(tableName, category)) { result = false; }
                }

                foreach (string category in sysTypeDictionary.Keys)
                {
                    if (addedCategories.Contains(category)) { continue; }
                    string tableName = "Type_" + category;
                    if (!UpdateFields(tableName, category, "Rvt_Type_Parameters")) { result = false; }
                    if (!WriteSysTypeData(tableName, category)) { result = false; }

                    tableName = "Inst_" + category;
                    if (!UpdateFields(tableName, category, "Rvt_Inst_Parameters")) { result = false; }
                    if (!WriteSysInstanceData(tableName, category)) { result = false; }
                }

                if (viewTypeDictionary.Count>0 && !addedCategories.Contains("Views"))
                {
                    string tableName = "Type_Views";
                    if (!UpdateFields(tableName, "Views", "Rvt_Type_Parameters")) { result = false; }
                    if (!WriteViewTypeData(tableName)) { result = false; }

                    tableName = "Inst_Views";
                    if (!UpdateFields(tableName, "Views", "Rvt_Inst_Parameters")) { result = false; }
                    if (!WriteViewInstData(tableName)) { result = false; }
                }

                if (isRoomSelected) { if (!UpdateFields("Rvt_Rooms", "Rooms", "Rvt_Type_Parameters")) { result = false; } }
                if (!WriteRoomData("Rvt_Rooms")) { result = false; }

                if (isSpaceSelected && !addedCategories.Contains("Spaces"))
                {
                    if (!UpdateFields("Rvt_Spaces", "Spaces", "Rvt_Type_Parameters")) { result = false; }
                    if (!WriteSpaceData("Rvt_Spaces")) { result = false; }
                }

                if (isAreaSelected && !addedCategories.Contains("Areas"))
                {
                    if (!UpdateFields("Rvt_Areas", "Areas", "Rvt_Type_Parameters")) { result = false; }
                    if (!WriteAreaData("Rvt_Areas")) { result = false; }
                }

                progressBar.Value = progressBar.Maximum;
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update tables: \n" + ex.Message);
                CloseDatabase();
                return false;
            }
        }

        private bool CreateInstanceTable(string categoryName)
        {
            bool result = false;
            string tableName = "Inst_" + categoryName;
            try
            {
                Dictionary<int, InstanceProperties> instances = new Dictionary<int, InstanceProperties>();
                if (instanceDictionary.ContainsKey(categoryName)) { instances = instanceDictionary[categoryName]; }

                tblDef = daoDB.CreateTableDef("Inst_" + categoryName, mMissing, mMissing, mMissing);

                //"InstanceID", "TypeID", "FamilyID", "FamilyName", "FamilyType"
                foreach (string fieldName in systemField)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    tblDef.Fields.Append(field);
                }

                //Different kinds of families have different sets of parameters. All existing parameters should be appeared in one table altogether.
                string familyName = null;
                List<int> paramIds = new List<int>();

                foreach (int instId in instances.Keys)
                {
                    InstanceProperties ip = instances[instId];
                    if (string.Compare(ip.FamilyName, familyName) != 0)
                    {
                        var sortedDict = (from entry in ip.InstParameters
                                          orderby entry.Value.ParamName ascending
                                          select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = sortedDict;

                        foreach (int pId in ppDic.Keys)
                        {
                            if (!paramIds.Contains(pId))
                            {
                                field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                                field.AllowZeroLength = true;
                                tblDef.Fields.Append(field);
                                paramIds.Add(pId);
                            }
                        }
                    }
                    familyName = ip.FamilyName;
                }

                foreach (string fieldName in internalField)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("InstanceID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                daoDB.TableDefs.Append(tblDef);

                result = WriteInstanceData(tableName, categoryName);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create instance table: " + tableName + "\n "+ ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateTypeTable(string categoryName)
        {
            bool result = false;
            string tableName = "Type_" + categoryName;
            try
            {
                Dictionary<int, TypeProperties> types = new Dictionary<int, TypeProperties>();
                if (typeDictionary.ContainsKey(categoryName)) { types = typeDictionary[categoryName]; }
                
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] defFields = new string[] { "TypeID", "FamilyID", "FamilyName", "FamilyType" };
                foreach (string fieldName in defFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    tblDef.Fields.Append(field);
                }

                //Different kinds of families have different sets of parameters. All existing parameters should be appeared in one table altogether.
                string familyName = null;
                List<int> paramIds = new List<int>();
                foreach (int typeId in types.Keys)
                {
                    TypeProperties tp = types[typeId];
                    if (string.Compare(tp.FamilyName, familyName) != 0)
                    {
                        var sortedDict = (from entry in tp.TypeParameters
                                          orderby entry.Value.ParamName ascending
                                          select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = sortedDict;
                        foreach (int pId in ppDic.Keys)
                        {
                            if (!paramIds.Contains(pId))
                            {
                                field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                                field.AllowZeroLength = true;
                                tblDef.Fields.Append(field);
                                paramIds.Add(pId);
                            }
                        }
                    }
                    familyName = tp.FamilyName;
                }

                //parameter fields 
                //Access can only export 10 characters worth of field name. To differentiate those field names, field names are set as their parameter Ids.


                Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("TypeID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);
                
                daoDB.TableDefs.Append(tblDef);

                if (WriteTypeData(tableName, categoryName)) { result = true; }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to create type table: " + tableName + "\n"+ ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateSysInstanceTable(string categoryName)
        {
            bool result = false;
            string tableName = "Inst_" + categoryName;
            try
            {
                TableDef tableDef = null;
                foreach (TableDef table in daoDB.TableDefs)
                {
                    if (table.Name == tableName)
                    {
                        tableDef = table;
                    }
                }

                Dictionary<int, ElementProperties> instances = new Dictionary<int, ElementProperties>();
                if (sysInstDictionary.ContainsKey(categoryName)) { instances = sysInstDictionary[categoryName]; }

                List<string> exisitngFields = new List<string>();

                if (tableDef != null) // when both system family and component family exist in the same category.
                {
                    tblDef = tableDef;
                    //daoDB.Relations.Delete("TypeOfInstance_" + categoryName);
                    foreach (Field field in tblDef.Fields)
                    {
                        exisitngFields.Add(field.Name);
                    }
                }
                else
                {
                    tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                    string[] defFields = new string[] { "InstanceID", "TypeID", "FamilyType" };
                    foreach (string fieldName in defFields)
                    {
                        field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                        tblDef.Fields.Append(field);
                    }
                    Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                    tblIndex.Primary = true;
                    tblIndex.Name = "PrimaryKey";
                    tblIndex.Required = true;
                    tblIndex.IgnoreNulls = false;
                    field = tblIndex.CreateField("InstanceID", mMissing, mMissing);
                    ((IndexFields)tblIndex.Fields).Append(field);
                    tblDef.Indexes.Append(tblIndex);

                    daoDB.TableDefs.Append(tblDef);
                }

                //Different kinds of types may have different sets of parameters. All existing parameters should be appeared in one table altogether.
                int typeId = 0;
                List<int> paramIds = new List<int>();
                foreach (int instId in instances.Keys)
                {
                    ElementProperties ep = instances[instId];
                    if (typeId.CompareTo(ep.TypeID) != 0)
                    {
                        var sortedDict = (from entry in ep.ElementParameters
                                          orderby entry.Value.ParamName ascending
                                          select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                        foreach (int pId in sortedDict.Keys)
                        {
                            if (!paramIds.Contains(pId) && !exisitngFields.Contains(pId.ToString()))
                            {
                                field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                                field.AllowZeroLength = true;
                                tblDef.Fields.Append(field);
                                paramIds.Add(pId);
                            }
                        }
                    }
                    typeId = ep.TypeID;
                }
                result = WriteSysInstanceData(tableName, categoryName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create system family instance table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateSysTypeTable(string categoryName)
        {
            bool result = false;
            string tableName = "Type_" + categoryName;
            try
            {
                TableDef tableDef = null;
                foreach (TableDef table in daoDB.TableDefs)
                {
                    if (table.Name == tableName)
                    {
                        tableDef = table;
                    }
                }

                Dictionary<int, ElementTypeProperties> types = new Dictionary<int, ElementTypeProperties>();
                if (sysTypeDictionary.ContainsKey(categoryName)) { types = sysTypeDictionary[categoryName]; }

                List<string> existingFields = new List<string>();
                if (tableDef != null)
                {
                    tblDef = tableDef;
                    foreach (Field field in tblDef.Fields)
                    {
                        existingFields.Add(field.Name);
                    }
                }
                else if (sysTypeDictionary.ContainsKey(categoryName))
                {
                    tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                    string[] defFields = new string[] { "TypeID", "FamilyType" };
                    foreach (string fieldName in defFields)
                    {
                        field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                        tblDef.Fields.Append(field);
                    }

                    Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                    tblIndex.Primary = true;
                    tblIndex.Name = "PrimaryKey";
                    tblIndex.Required = true;
                    tblIndex.IgnoreNulls = false;
                    field = tblIndex.CreateField("TypeID", mMissing, mMissing);
                    ((IndexFields)tblIndex.Fields).Append(field);
                    tblDef.Indexes.Append(tblIndex);

                    daoDB.TableDefs.Append(tblDef);
                }

                //Different kinds of types may contain different sets of parameters. All existing parameters should be appeared in one table altogether.
                List<int> paramIds = new List<int>();
                foreach (int typeId in types.Keys)
                {
                    ElementTypeProperties tp = types[typeId];
                    var sortedDict = (from entry in tp.ElementTypeParameters
                                      orderby entry.Value.ParamName ascending
                                      select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                    foreach (int pId in sortedDict.Keys)
                    {
                        if (!paramIds.Contains(pId) && !existingFields.Contains(pId.ToString()))
                        {
                            field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                            field.AllowZeroLength = true;
                            tblDef.Fields.Append(field);
                            paramIds.Add(pId);
                        }
                    }
                }

                if (WriteSysTypeData(tableName, categoryName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create system family type table: " + tableName +  "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateRelationship(string categoryName)
        {
            bool result = false;
            try
            {
                relation = daoDB.CreateRelation("TypeOfInstance_" + categoryName, "Type_" + categoryName, "Inst_" + categoryName, RelationAttributeEnum.dbRelationUpdateCascade);
                relation.Fields.Append(relation.CreateField("TypeID"));
                relation.Fields["TypeID"].ForeignName = "TypeID";
                daoDB.Relations.Append(relation);
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to append relationship of tables: " + categoryName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateRoomTable()
        {
            bool result = false;
            string tableName = "Rvt_Rooms";
            
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] roomFields = new string[] { "ID" };
                foreach (string fieldName in roomFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("ID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                //Extract parameters from a room object in the dictionary
                Dictionary<int, ParamProperties> pramProperties = new Dictionary<int, ParamProperties>();
                if (roomDictionary.Count > 0)
                {
                    pramProperties = roomDictionary[roomDictionary.Keys.First()].RoomParameters;

                    var sortedDict = (from entry in pramProperties
                                      orderby entry.Value.ParamName ascending
                                      select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                    foreach (int pId in sortedDict.Keys)
                    {
                        field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                        field.AllowZeroLength = true;
                        tblDef.Fields.Append(field);
                    }
                }
                daoDB.TableDefs.Append(tblDef);

                if (roomDictionary.Count > 0)
                {
                    result = WriteRoomData(tableName);
                }
                else { result = true; }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to create room table: " + tableName +"\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateSpaceTable()
        {
            bool result = false;
            string tableName = "Rvt_Spaces";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] spaceFields = new string[] { "ID" };
                foreach (string fieldName in spaceFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                //Extract parameters from a room object in the dictionary
                Dictionary<int, ParamProperties> pramProperties = new Dictionary<int, ParamProperties>();
                pramProperties = spaceDictionary[spaceDictionary.Keys.First()].SpaceParameters;
                var sortedDict = (from entry in pramProperties
                                  orderby entry.Value.ParamName ascending
                                  select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                foreach (int pId in sortedDict.Keys)
                {
                    field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("ID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                daoDB.TableDefs.Append(tblDef);

                if (WriteSpaceData(tableName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create space table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateAreaTable()
        {
            bool result = false;
            string tableName = "Rvt_Areas";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] areaFields = new string[] { "ID" };
                foreach (string fieldName in areaFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                //Extract parameters from a room object in the dictionary
                Dictionary<int, ParamProperties> pramProperties = new Dictionary<int, ParamProperties>();
                pramProperties = areaDictionary[areaDictionary.Keys.First()].AreaParameters;
                var sortedDict = (from entry in pramProperties
                                  orderby entry.Value.ParamName ascending
                                  select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                foreach (int pId in sortedDict.Keys)
                {
                    field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("ID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                daoDB.TableDefs.Append(tblDef);

                if (WriteAreaData(tableName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create area table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateViewTypeTable()
        {
            bool result = false;
            string tableName = "Type_Views";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                foreach (string fieldName in viewField)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("TypeID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                daoDB.TableDefs.Append(tblDef);

                //Different kinds of types may contain different sets of parameters. All existing parameters should be appeared in one table altogether.
                List<int> paramIds = new List<int>();
                foreach (int typeId in viewTypeDictionary.Keys)
                {
                    ViewTypeProperties vtp = viewTypeDictionary[typeId];

                    var sortedDict = (from entry in vtp.ViewTypeParameters
                                      orderby entry.Value.ParamName ascending
                                      select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                    foreach (int pId in sortedDict.Keys)
                    {
                        if (!paramIds.Contains(pId))
                        {
                            field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                            field.AllowZeroLength = true;
                            tblDef.Fields.Append(field);
                            paramIds.Add(pId);
                        }
                    }
                }
                if (WriteViewTypeData(tableName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create view type table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateViewInstanceTable()
        {
            bool result = false;
            string tableName = "Inst_Views";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] defFields = new string[] { "InstanceID", "TypeID", "ViewTypeName" };
                foreach (string fieldName in defFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("InstanceID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                daoDB.TableDefs.Append(tblDef);

                //Different kinds of types may contain different sets of parameters. All existing parameters should be appeared in one table altogether.
                List<int> paramIds = new List<int>();
                foreach (int instId in viewInstDictionary.Keys)
                {
                    ViewProperties vp = viewInstDictionary[instId];
                    var sortedDict = (from entry in vp.ViewParameters
                                      orderby entry.Value.ParamName ascending
                                      select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //order by parameter names

                    foreach (int pId in sortedDict.Keys)
                    {
                        if (!paramIds.Contains(pId))
                        {
                            field = tblDef.CreateField(pId.ToString(), DataTypeEnum.dbText);
                            field.AllowZeroLength = true;
                            tblDef.Fields.Append(field);
                            paramIds.Add(pId);
                        }
                    }
                }
                if (WriteViewInstData(tableName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create view inst table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateParameterInfoTable()
        {
            bool result = false;
            string tableName = "Rvt_ParameterInfo";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] paramFields = new string[] { "ParamID", "ParamName", "ParamFormat", "IsReadOnly", "IsShared", "ParamType", "DisplayUnitType" };

                foreach (string fieldName in paramFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("primaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("ParamID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                daoDB.TableDefs.Append(tblDef);

                if (WriteParamData(tableName)) { result = true; }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to create ParameterInfo table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateParamSettingTable(string tableName,Dictionary<string,Dictionary<int,ParamProperties>> paramCatSettings,
            Dictionary<int,Dictionary<int,ParamProperties>> paramSettings, Dictionary<string,Dictionary<int,ParamProperties>> viewParamSettings)
        {
            bool result = false;
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] settingFields = new string[] { "KeyID", "CategoryName", "FamilyID", "FamilyName", "ParamID", "ParamName", "ParamFormat", "IsVisible", 
                "IsReadOnly", "IsLockAll", "IsEditable","IsProject" };

                foreach (string fieldName in settingFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("PrimaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("KeyID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                daoDB.TableDefs.Append(tblDef);

                WriteCategoryParamSetting(tableName, paramCatSettings);
                if (WriteFamilyParamSetting(tableName, paramSettings, viewParamSettings)) { result = true; }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to create ParamSetting table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateFamilyInfoTable()
        {
            bool result = false;
            string tableName = "Rvt_FamilyInfo";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] familyFields = new string[] { "TypeID", "TypeName", "FamilyID", "FamilyName", "CategoryName", };

                foreach (string fieldName in familyFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    tblDef.Fields.Append(field);
                }

                Index tblIndex = tblDef.CreateIndex("primaryKey");
                tblIndex.Primary = true;
                tblIndex.Name = "PrimaryKey";
                tblIndex.Required = true;
                tblIndex.IgnoreNulls = false;
                field = tblIndex.CreateField("TypeID", mMissing, mMissing);
                ((IndexFields)tblIndex.Fields).Append(field);
                tblDef.Indexes.Append(tblIndex);

                daoDB.TableDefs.Append(tblDef);

                if (WriteFamilyData(tableName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create FamilyInfo table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateRevitInternalTable()
        {
            bool result = false;
            string tableName = "UI_RevitInternalDB";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] internalDBFields = new string[] { "TableName", "ObjectName", "FieldName", };
                foreach (string fieldName in internalDBFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    tblDef.Fields.Append(field);
                }

                daoDB.TableDefs.Append(tblDef);
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create RevitInternalDB table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateAutoSyncTable()
        {
            bool result = false;
            string tableName = "UI_AutoSyncOptions";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] syncFields = new string[] { "SyncType", "CategoryID", "CategoryName", "FamilyID", "FamilyName", };
                foreach (string fieldName in syncFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    tblDef.Fields.Append(field);
                }

                daoDB.TableDefs.Append(tblDef);
                if (WriteAutoSyncData(tableName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create AutoSync table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateExternalRefTable()
        {
            bool result = false;
            string tableName = "UI_ExternalReference";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] syncFields = new string[] { "CategoryName", "ControlParam", "ControlField", "TableName", "DBPath"};
                foreach (string fieldName in syncFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                for (int i = 1; i < 6; i++)
                {
                    string updateParam = "UpdateParam" + i;
                    field = tblDef.CreateField(updateParam, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);

                    string updateField = "UpdateField" + i;
                    field = tblDef.CreateField(updateField, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                daoDB.TableDefs.Append(tblDef);
                if (WriteReferenceData(tableName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create AutoSync table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool CreateExcludeInstTable()
        {
            bool result = false;
            string tableName = "UI_ExcludeInstance";
            try
            {
                tblDef = daoDB.CreateTableDef(tableName, mMissing, mMissing, mMissing);

                string[] excludeInstFields = new string[] { "CategoryName", "ExcludeInstance"};
                foreach (string fieldName in excludeInstFields)
                {
                    field = tblDef.CreateField(fieldName, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    tblDef.Fields.Append(field);
                }

                daoDB.TableDefs.Append(tblDef);
                if (WriteExcludeInstData(tableName)) { result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a table: " + tableName + "\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private bool WriteInstanceData(string tableName, string category)
        {
            if (excludeInstanceSettings.ContainsKey(category))
            {
                if (excludeInstanceSettings[category]) { return true; }
            }
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                Dictionary<int, InstanceProperties> instances = new Dictionary<int, InstanceProperties>();
                if (instanceDictionary.ContainsKey(category)) { instances = instanceDictionary[category]; }

                foreach (int instId in instances.Keys)
                {
                    InstanceProperties ip = instances[instId];

                    valuePair = new Dictionary<string, string>();
                    //system fields
                    valuePair.Add("InstanceID", ip.InstanceID.ToString());
                    valuePair.Add("TypeID", ip.TypeID.ToString());
                    valuePair.Add("FamilyID", ip.FamilyID.ToString());
                    valuePair.Add("FamilyName", ip.FamilyName);
                    valuePair.Add("FamilyType", ip.FamilyTypeName);

                    //parameter fields
                    Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                    ppDic = ip.InstParameters;
                    foreach (int pId in ppDic.Keys)
                    {
                        if (!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = ppDic[pId];
                        if (internalField.Contains(pp.ParamName)) { continue; } //to avoid redundant definition
                        if ( !calFields[pp.ParamID.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                        
                    }

                    //internal fields
                    valuePair.Add("FacingFlipped", ip.FacingFlipped.ToString());
                    if (null != ip.FromRoom) { valuePair.Add("FromRoom", ip.FromRoom.Id.ToString()); }
                    valuePair.Add("HandFlipped", ip.HandFlipped.ToString());
                    if (null != ip.Host) { valuePair.Add("Host", ip.Host.Id.IntegerValue.ToString()); }
                    valuePair.Add("IsSlantedColumn", ip.IsSlantedColumn.ToString());
                    valuePair.Add("Mirrored", ip.Mirrored.ToString());
                    if (null != ip.Room) { valuePair.Add("Room", ip.Room.Id.IntegerValue.ToString()); }
                    if (null != ip.ToRoom) { valuePair.Add("ToRoom", ip.ToRoom.Id.IntegerValue.ToString()); }

                    string query=SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    progressBar.PerformStep();
                }
                
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write instance data in " + tableName + ":\n" + ex.Message, "RevitDBCreator Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool WriteTypeData(string tableName, string category)
        {
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();
                Dictionary<int, TypeProperties> types = new Dictionary<int, TypeProperties>();
                if (typeDictionary.ContainsKey(category)) { types = typeDictionary[category]; }

                foreach (int typeId in types.Keys)
                {
                    TypeProperties tp = types[typeId];
                    valuePair = new Dictionary<string, string>();
                    //system fields
                    valuePair.Add("TypeID", tp.TypeID.ToString());
                    valuePair.Add("FamilyID", tp.FamilyID.ToString());
                    valuePair.Add("FamilyName", tp.FamilyName);
                    valuePair.Add("FamilyType", tp.FamilyTypeName);
                    Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                    ppDic = tp.TypeParameters;
                    //parameter fields
                    foreach (int pId in ppDic.Keys)
                    {
                        if (!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = ppDic[pId];
                        if (!calFields[pId.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    progressBar.PerformStep();
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write type data in "+tableName+":\n" + ex.Message,"RevitDBCreator Error", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool WriteSysTypeData(string tableName, string category)
        {
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();
                Dictionary<int, ElementTypeProperties> types = new Dictionary<int, ElementTypeProperties>();
                if (sysTypeDictionary.ContainsKey(category)) { types = sysTypeDictionary[category]; }

                foreach (int typeId in types.Keys)
                {
                    ElementTypeProperties etp = types[typeId];
                    valuePair = new Dictionary<string, string>();
                    //system fields
                    valuePair.Add("TypeID", etp.TypeID.ToString());
                    valuePair.Add("FamilyType", etp.ElementTypeName);
                    
                    //parameter fields
                    foreach (int pId in etp.ElementTypeParameters.Keys)
                    {
                        if(!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = etp.ElementTypeParameters[pId];
                        if (!calFields[pp.ParamID.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    progressBar.PerformStep();
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write system type data in " + tableName + ":\n" + ex.Message, "RevitDBCreator Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool WriteSysInstanceData(string tableName, string category)
        {
            if (excludeInstanceSettings.ContainsKey(category))
            {
                if (excludeInstanceSettings[category]) { return true; }
            }
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                Dictionary<int, ElementProperties> instances = new Dictionary<int, ElementProperties>();
                if (sysInstDictionary.ContainsKey(category)) { instances = sysInstDictionary[category]; }

                foreach (int instId in instances.Keys)
                {
                    ElementProperties ep = instances[instId];

                    valuePair = new Dictionary<string, string>();
                    
                    //system fields
                    valuePair.Add("InstanceID", ep.ElemntID.ToString());
                    valuePair.Add("TypeID", ep.TypeID.ToString());
                    valuePair.Add("FamilyType", ep.TypeName);

                    //parameter fields
                    foreach (int pId in ep.ElementParameters.Keys)
                    {
                        if (!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = ep.ElementParameters[pId];
                        if (!calFields[pp.ParamID.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    progressBar.PerformStep();
                }
                
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write system instance data in " + tableName + ":\n" + ex.Message, "RevitDBCreator Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool WriteRoomData(string tableName)
        {
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);

            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (int roomId in roomDictionary.Keys)
                {
                    RoomProperties rp = roomDictionary[roomId];

                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("ID", rp.RoomID.ToString());

                    Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                    ppDic = rp.RoomParameters;
                    foreach (int pId in ppDic.Keys)
                    {
                        if (!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = ppDic[pId];
                        if (pp.ParamName == "Level") { valuePair.Add(pp.ParamID.ToString(), rp.Level); }
                        else if (pp.ParamName == "Upper Limit") { valuePair.Add(pp.ParamID.ToString(), rp.UpperLimit); }
                        else if (!calFields[pp.ParamID.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }
                progressBar.PerformStep();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write room data: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteSpaceData(string tableName)
        {
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (int spaceId in spaceDictionary.Keys)
                {
                    SpaceProperties sp = spaceDictionary[spaceId];

                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("ID", sp.SpaceID.ToString());

                    Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                    ppDic = sp.SpaceParameters;
                    foreach (int pId in ppDic.Keys)
                    {
                        if (!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = ppDic[pId];
                        if (!calFields[pp.ParamID.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }
                progressBar.PerformStep();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write space data: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteAreaData(string tableName)
        {
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (int areaId in areaDictionary.Keys)
                {
                    AreaProperties ap = areaDictionary[areaId];

                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("ID", ap.AreaID.ToString());

                    Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                    ppDic = ap.AreaParameters;
                    foreach (int pId in ppDic.Keys)
                    {
                        if (!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = ppDic[pId];
                        if (!calFields[pp.ParamID.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }
                progressBar.PerformStep();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write area data: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteViewTypeData(string tableName)
        {
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();
                foreach (int typeId in viewTypeDictionary.Keys)
                {
                    ViewTypeProperties vtp = viewTypeDictionary[typeId];
                    valuePair = new Dictionary<string, string>();
                    //system fields
                    valuePair.Add("TypeID", vtp.ViewTypeID.ToString());
                    valuePair.Add("ViewTypeName", vtp.ViewTypeName);
                    valuePair.Add("ViewFamilyName", vtp.ViewFamilyName);
                    Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                    ppDic = vtp.ViewTypeParameters;
                    //parameter fields
                    foreach (int pId in ppDic.Keys)
                    {
                        if (!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = ppDic[pId];
                        if (!calFields[pp.ParamID.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    progressBar.PerformStep();
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write view type data in " + tableName + ":\n" + ex.Message, "RevitDBCreator Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool WriteViewInstData(string tableName)
        {
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();
                foreach (int instId in viewInstDictionary.Keys)
                {
                    ViewProperties vp=viewInstDictionary[instId];
                    valuePair = new Dictionary<string, string>();
                    //system fields
                    valuePair.Add("InstanceID", vp.ViewID.ToString());
                    valuePair.Add("TypeID", vp.TypeID.ToString());
                    valuePair.Add("ViewTypeName", vp.ViewTypeName);
                    Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                    ppDic = vp.ViewParameters;
                    //parameter fields
                    foreach (int pId in ppDic.Keys)
                    {
                        if (!calFields.ContainsKey(pId.ToString())) { continue; }
                        ParamProperties pp = ppDic[pId];
                        if (!calFields[pp.ParamID.ToString()] && !valuePair.ContainsKey(pp.ParamID.ToString()))
                        {
                            valuePair.Add(pp.ParamID.ToString(), pp.ParamValue);
                        }
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    progressBar.PerformStep();
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write view inst data in " + tableName + ":\n" + ex.Message, "RevitDBCreator Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool WriteParamData(string tableName)
        {
            bool result = false;
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (int paramID in ParamInfoDictionary.Keys)
                {
                    ParamProperties pp = ParamInfoDictionary[paramID];

                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("ParamID", pp.ParamID.ToString());
                    valuePair.Add("ParamName", pp.ParamName);
                    valuePair.Add("ParamFormat", pp.ParamFormat);
                    valuePair.Add("IsReadOnly", pp.IsReadOnly.ToString());
                    valuePair.Add("IsShared", pp.IsShared.ToString());
                    valuePair.Add("ParamType", pp.ParamType);
                    valuePair.Add("DisplayUnitType", pp.DisplayUnitType);

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }
                result = true;
                progressBar.PerformStep();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write Parameter Information: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteFamilyData(string tableName)
        {
            bool result = false;
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (string category in typeDictionary.Keys)
                {
                    foreach (int typeId in typeDictionary[category].Keys)
                    {
                        TypeProperties tp = typeDictionary[category][typeId];
                        valuePair = new Dictionary<string, string>();
                        valuePair.Add("TypeID", tp.TypeID.ToString());
                        valuePair.Add("TypeName", tp.FamilyTypeName);
                        valuePair.Add("FamilyID", tp.FamilyID.ToString());
                        valuePair.Add("FamilyName", tp.FamilyName);
                        valuePair.Add("CategoryName", tp.CategoryName);

                        string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                        daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    }
                }

                foreach (string category in sysTypeDictionary.Keys)
                {
                    foreach (int typeId in sysTypeDictionary[category].Keys)
                    {
                        ElementTypeProperties etp = sysTypeDictionary[category][typeId];
                        valuePair = new Dictionary<string, string>();
                        valuePair.Add("TypeID", etp.TypeID.ToString());
                        valuePair.Add("TypeName", etp.ElementTypeName);
                        valuePair.Add("CategoryName", etp.CategoryName);

                        string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                        daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    }
                }

                foreach (int typeId in viewTypeDictionary.Keys)
                {
                    ViewTypeProperties vtp = viewTypeDictionary[typeId];
                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("TypeID", vtp.ViewTypeID.ToString());
                    valuePair.Add("TypeName", vtp.ViewTypeName);
                    valuePair.Add("FamilyName", vtp.ViewFamilyName);
                    valuePair.Add("CategoryName", vtp.CategoryName);

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }

                progressBar.PerformStep();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write Family Information: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteFamilyParamSetting(string tableName,Dictionary<int,Dictionary<int,ParamProperties>> familyParamSettings
            , Dictionary<string,Dictionary<int,ParamProperties>> viewParamSettings)
        {
            bool result = false;
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (int familyId in familyParamSettings.Keys)
                {
                    Dictionary<int, ParamProperties> paramDictionary = new Dictionary<int, ParamProperties>();
                    paramDictionary = familyParamSettings[familyId];

                    foreach (int paramId in paramDictionary.Keys)
                    {
                        ParamProperties pp = paramDictionary[paramId];

                        valuePair = new Dictionary<string, string>();
                        valuePair.Add("KeyID", pp.ParamID.ToString() + ";" + familyId.ToString() + ";" + "family");
                        valuePair.Add("CategoryName", pp.CategoryName);
                        valuePair.Add("FamilyID", familyId.ToString());
                        valuePair.Add("FamilyName", pp.FamilyName);
                        valuePair.Add("ParamID", paramId.ToString());
                        valuePair.Add("ParamName", pp.ParamName);
                        valuePair.Add("ParamFormat", pp.ParamFormat);
                        valuePair.Add("IsVisible", pp.IsVisible.ToString());
                        valuePair.Add("IsReadOnly", pp.IsReadOnly.ToString());
                        valuePair.Add("IsLockAll", pp.IsLockAll.ToString());
                        valuePair.Add("IsEditable", pp.IsEditable.ToString());
                        valuePair.Add("IsProject", pp.IsProject.ToString());

                        string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                        daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                        progressBar.PerformStep();
                    }
                }

                foreach (string familyName in viewParamSettings.Keys)
                {
                    Dictionary<int, ParamProperties> paramDictionary = new Dictionary<int, ParamProperties>();
                    paramDictionary = viewParamSettings[familyName];

                    foreach (int paramId in paramDictionary.Keys)
                    {
                        ParamProperties pp = paramDictionary[paramId];

                        valuePair = new Dictionary<string, string>();
                        valuePair.Add("KeyID", pp.ParamID.ToString() + ";" + familyName + ";" + "family");
                        valuePair.Add("CategoryName", "Views");
                        valuePair.Add("FamilyName", familyName);
                        valuePair.Add("ParamID", paramId.ToString());
                        valuePair.Add("ParamName", pp.ParamName);
                        valuePair.Add("ParamFormat", pp.ParamFormat);
                        valuePair.Add("IsVisible", pp.IsVisible.ToString());
                        valuePair.Add("IsReadOnly", pp.IsReadOnly.ToString());
                        valuePair.Add("IsLockAll", pp.IsLockAll.ToString());
                        valuePair.Add("IsEditable", pp.IsEditable.ToString());
                        valuePair.Add("IsProject", pp.IsProject.ToString());

                        string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                        daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                        progressBar.PerformStep();
                    }
                }
                
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write Parameter Setting data: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteCategoryParamSetting(string tableName, Dictionary<string,Dictionary<int,ParamProperties>> categoryParamSettings)
        {
            bool result = false;
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (string category in categoryParamSettings.Keys)
                {
                    Dictionary<int, ParamProperties> paramDictionary = new Dictionary<int, ParamProperties>();
                    paramDictionary = categoryParamSettings[category];

                    foreach (int paramId in paramDictionary.Keys)
                    {
                        ParamProperties pp = paramDictionary[paramId];

                        valuePair = new Dictionary<string, string>();
                        valuePair.Add("KeyID", pp.ParamID.ToString() + ";" + category + ";" + "category");
                        valuePair.Add("CategoryName", category);
                        valuePair.Add("ParamID", paramId.ToString());
                        valuePair.Add("ParamName", pp.ParamName);
                        valuePair.Add("ParamFormat", pp.ParamFormat);
                        valuePair.Add("IsVisible", pp.IsVisible.ToString());
                        valuePair.Add("IsReadOnly", pp.IsReadOnly.ToString());
                        valuePair.Add("IsLockAll", pp.IsLockAll.ToString());
                        valuePair.Add("IsEditable", pp.IsEditable.ToString());
                        valuePair.Add("IsProject", pp.IsProject.ToString());

                        string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                        daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                        progressBar.PerformStep();
                    }
                }
                
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write Parameter Setting data: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteAutoSyncData(string tableName)
        {
            bool result = false;
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (int syncID in syncDictionary.Keys)
                {
                    SyncProperties sp = syncDictionary[syncID];

                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("SyncType", sp.SyncType.ToString());
                    valuePair.Add("CategoryID", sp.CategoryID.ToString());
                    valuePair.Add("CategoryName", sp.CategoryName);
                    valuePair.Add("FamilyID", sp.FamilyID.ToString());
                    valuePair.Add("FamilyName", sp.FamilyName);

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }
                progressBar.PerformStep();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write type data: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteReferenceData(string tableName)
        {
            bool result = false;
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (string category in linkedParameters.Keys)
                {
                    LinkedParameter lp = linkedParameters[category];

                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("CategoryName", lp.CategoryName);
                    valuePair.Add("ControlParam", lp.ControlParameter);
                    valuePair.Add("ControlField", lp.ControlField);
                    valuePair.Add("TableName", lp.TableName);
                    valuePair.Add("DBPath", lp.DBPath);

                    int i=1;
                    foreach (string updateParam in lp.UpdateParameterField.Keys)
                    {
                        string paramfield = "UpdateParam" + i;
                        valuePair.Add(paramfield, updateParam);
                        string fieldName = "UpdateField" + i;
                        valuePair.Add(fieldName, lp.UpdateParameterField[updateParam]);
                        i++;
                    }

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }
                progressBar.PerformStep();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write type data: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool WriteExcludeInstData(string tableName)
        {
            bool result = false;
            try
            {
                Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                foreach (string categoryName in excludeInstanceSettings.Keys)
                {
                    valuePair = new Dictionary<string, string>();
                    valuePair.Add("CategoryName", categoryName);
                    valuePair.Add("ExcludeInstance", excludeInstanceSettings[categoryName].ToString());

                    string query = SqlHelper.InsertIntoTable(tableName, valuePair);
                    daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                }
                progressBar.PerformStep();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write type data: \n" + ex.Message);
                result = false;
            }
            return result;
        }

        private bool DeleteTables()
        {
            bool result = false;
            try
            {
                List<string> delCategories = new List<string>();
                foreach (TableDef table in daoDB.TableDefs)
                {
                    string tableName = table.Name;
                    if (tableName.Contains("Inst_") && !tableName.Contains("Rvt_"))
                    {
                        string[] categoryName = tableName.Split('_');
                        if (categoryName[1].Contains("Views"))
                        {
                            if (viewTypeDictionary.Count < 1)
                            {
                                delCategories.Add("Views");
                            }
                        }
                        else if (!typeDictionary.ContainsKey(categoryName[1]) && !sysTypeDictionary.ContainsKey(categoryName[1]))
                        {
                            delCategories.Add(categoryName[1]);
                        }
                    }
                    else if (tableName.Contains("Spaces")||tableName.Contains("Areas"))
                    {
                        string[] categoryName = tableName.Split('_');
                        if (!typeCatParamSettings.ContainsKey(categoryName[1]) && !instCatParamSettings.ContainsKey(categoryName[1]))
                        {
                            delCategories.Add(categoryName[1]);
                        }
                    }
                }

                if (delCategories.Count > 0)
                {
                    foreach (string delCat in delCategories)
                    {
                        if (delCat.Contains("Spaces")||delCat.Contains("Areas")) { daoDB.TableDefs.Delete("Rvt_" + delCat); }
                        else
                        {
                            //daoDB.Relations.Delete("TypeOfInstance_" + delCat);
                            daoDB.TableDefs.Delete("Inst_" + delCat);
                            daoDB.TableDefs.Delete("Type_" + delCat);
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete tables:\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        private List<string> AddTables()
        {
            List<string> addCategories = new List<string>();
            try
            {
                List<string> exisitngCategories = new List<string>();

                foreach (TableDef table in daoDB.TableDefs)
                {
                    if (table.Name.Contains("Inst_") && !table.Name.Contains("Rvt_"))
                    {
                        string[] categoryName = table.Name.Split('_');
                        exisitngCategories.Add(categoryName[1]);
                    }
                    if (table.Name.Contains("Rooms") || table.Name.Contains("Spaces")||table.Name.Contains("Areas"))
                    {
                        string[] categoryName = table.Name.Split('_');
                        exisitngCategories.Add(categoryName[1]);
                    }
                }

                foreach (string category in typeDictionary.Keys)
                {
                    if (!exisitngCategories.Contains(category))
                    {
                        addCategories.Add(category);
                        CreateTypeTable(category);
                        CreateInstanceTable(category);
                        //CreateRelationship(category);
                    }
                }

                foreach (string category in sysTypeDictionary.Keys)
                {
                    if (!exisitngCategories.Contains(category))
                    {
                        addCategories.Add(category);
                        CreateSysTypeTable(category);
                        CreateSysInstanceTable(category);
                        //CreateRelationship(category);
                    }
                }

                if (viewTypeDictionary.Count > 0)
                {
                    if (!exisitngCategories.Contains("Views"))
                    {
                        addCategories.Add("Views");
                        CreateViewTypeTable();
                        CreateViewInstanceTable();
                        //CreateRelationship("Views");
                    }
                }
                
                if (isSpaceSelected)
                {
                    if (!exisitngCategories.Contains("Spaces"))
                    {
                        addCategories.Add("Spaces");
                        CreateSpaceTable();
                    }
                }

                if (isAreaSelected)
                {
                    if (!exisitngCategories.Contains("Areas"))
                    {
                        addCategories.Add("Areas");
                        CreateAreaTable();
                    }
                }

                return addCategories;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add tables:\n" + ex.Message, "RevitDBCreator Error:");
                return addCategories;
            }
        }

        private bool DeleteAllContents()
        {
            bool result = false;
            try
            {
                List<string> exisitngCategories = new List<string>();
                List<string> rvtTables = new List<string>();

                foreach (TableDef table in daoDB.TableDefs)
                {
                    if (table.Name.Contains("Inst_") && !table.Name.Contains("Rvt_"))
                    {
                        string[] categoryName = table.Name.Split('_');
                        exisitngCategories.Add(categoryName[1]);
                    }
                    else if (table.Name.Contains("Rvt_") || table.Name.Contains("AutoSync") || table.Name.Contains("External") || table.Name.Contains("ExcludeInstance"))
                    {
                        rvtTables.Add(table.Name);
                    }
                }

                string queryStr = "";
                string tableName = "";
                //delete all data from instance and type tables
                foreach (string category in exisitngCategories)
                {
                    tableName = "Inst_" + category;
                    queryStr = SqlHelper.DeleteTable(tableName);
                    daoDB.Execute(queryStr, RecordsetOptionEnum.dbFailOnError);

                    tableName = "Type_" + category;
                    queryStr = SqlHelper.DeleteTable(tableName);
                    daoDB.Execute(queryStr, RecordsetOptionEnum.dbFailOnError);
                }

                //delete all data from Rvt tables
                foreach (string table in rvtTables)
                {
                    queryStr = SqlHelper.DeleteTable(table);
                    daoDB.Execute(queryStr, RecordsetOptionEnum.dbFailOnError);
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete all contents from tables:\n" + ex.Message, "RevitDBCreator Error:");
                result = false;
            }
            return result;
        }

        //When deleted or added new families, the database requires deleting non-exisitng fields or adding new filelds.
        private bool UpdateFields(string tableName, string catName, string paramTable)
        {
            bool result = false;
            Dictionary<string, bool> calFields = new Dictionary<string, bool>();
            calFields = FindCalculatedFields(tableName);
            try
            {
                TableDef table = null;
                List<string> newFields = new List<string>();
                List<string> oldFields = new List<string>();

                foreach (TableDef tableDef in daoDB.TableDefs)
                {
                    if (tableDef.Name == tableName)
                    {
                        table = tableDef;
                    }
                }
                if (null != table)
                {
                    string strSql = "SELECT * FROM " + paramTable + " WHERE CategoryName ='" + catName + "'";
                    Recordset recordset = daoDB.OpenRecordset(strSql, RecordsetTypeEnum.dbOpenDynaset);
                    if (recordset.RecordCount > 0)
                    {
                        while (!recordset.EOF)
                        {
                            string paramId = recordset.Fields["ParamID"].Value;
                            if (!newFields.Contains(paramId) && !internalField.Contains(paramId))
                            {
                                newFields.Add(paramId); //from parameter settings
                            }
                            recordset.MoveNext();
                        }
                    }
                    recordset.Close();

                    foreach (Field field in table.Fields)
                    {
                        string fieldName = field.Name;
                        if (internalField.Contains(fieldName) || systemField.Contains(fieldName)||fieldName=="ID"||viewField.Contains(fieldName)) { continue; }

                        if (newFields.Contains(fieldName))
                        {
                            newFields.Remove(fieldName);//when the field already exists in the table definition
                        }
                        else
                        {
                            if (calFields.ContainsKey(fieldName)) { if (calFields[fieldName]) { continue; } } //shouldn't be removed
                            if (fieldName.Contains("Dbl_")) { continue; }
                            oldFields.Add(fieldName); //fields that doesn't belongs to current selection of fields
                        }
                    }

                    if (newFields.Count > 0)
                    {
                        foreach (string fieldName in newFields)
                        {
                            Field newField = table.CreateField(fieldName, DataTypeEnum.dbText);
                            newField.AllowZeroLength = true;
                            table.Fields.Append(newField);
                        }
                    }

                    if (oldFields.Count > 0)
                    {
                        foreach (string fieldName in oldFields)
                        {
                            table.Fields.Delete(fieldName);
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update fields: "+tableName+" \n" + ex.Message);
                result = false;
            }
            return result;
        }

        public void CloseDatabase()
        {
            if (null != oAccess.CurrentDb())
            {
                oAccess.CloseCurrentDatabase();
                oAccess.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oAccess);
                GC.Collect();

                foreach (Process pr in Process.GetProcessesByName("MSACCESS"))
                {
                    pr.Kill();
                }
            }
        }

        public void DeleteDatabase()
        {
            File.Delete(inputDir);
        }

        private Dictionary<string, bool> FindCalculatedFields(string tableName)
        {
            Dictionary<string, bool> calculatedFields = new Dictionary<string,bool>();
            Recordset recordset = daoDB.OpenTable(tableName);

            foreach (Field field in recordset.Fields)
            {
                Field2 field2 = field as Field2;
                if (field2.Expression != string.Empty)
                {
                    calculatedFields.Add(field2.Name, true);
                }
                else
                {
                    calculatedFields.Add(field2.Name, false);
                }
            }
            recordset.Close();
            return calculatedFields;
        }

    }


}
