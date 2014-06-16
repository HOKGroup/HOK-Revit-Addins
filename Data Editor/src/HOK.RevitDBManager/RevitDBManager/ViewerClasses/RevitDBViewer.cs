using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Access = Microsoft.Office.Interop.Access;
using Microsoft.Office.Interop.Access.Dao;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using RevitDBManager.ViewerClasses;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitDBManager.GenericForms;
using RevitDBManager.ViewerForms;
using System.Runtime.InteropServices;
using RevitDBManager.HelperClasses;

namespace RevitDBManager.Classes
{
    public class RevitDBViewer
    {
        private string dbPath = "";
        private Autodesk.Revit.DB.Document doc;
        private Access.Application oAccess;
        private Database daoDB;
        private object mMissing = System.Reflection.Missing.Value;
        private DataSet dataSet;
        private ExternalReference externalReference;
        private ReadFromRevit readRevit;
        private form_ProgressBar progressForm;
        private UnitConverter unitConverter;
        private bool isRoomSelected=false;
        private bool isSpaceSelected=false;
        private bool isAreaSelected = false;
        private bool isSynchronzed = false; //to resuce the taking time to open the viewer, it will provide synchronization options to users.
        private bool warningMode = false;
  
        private Dictionary<string/*tableName*/, List<string> /*nonvisibleParamName*/> nonVisibleFields = new Dictionary<string, List<string>>();
        private Dictionary<string/*tableName*/, List<string> /*noneditableParamName*/> nonEditableFields = new Dictionary<string, List<string>>();
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, LockType>> lockTypeFields = new Dictionary<string, Dictionary<string/*fieldName*/, LockType>>();
        private Dictionary<string/*tableName*/, List<string>/*fieldNames*/> internalFiedls = new Dictionary<string, List<string>>();
        private Dictionary<string/*paramId*/, string/*paramName*/> paramIdMap = new Dictionary<string, string>();
        private Dictionary<string/*paramId*/, FieldProperties> fieldDictionary = new Dictionary<string, FieldProperties>();
        private Dictionary<string/*categoryName*/, bool/*excludeInstance*/> excludeInstanceSettings = new Dictionary<string, bool>();
        private Dictionary<string/*tablename*/, Dictionary<string/*paramName*/, string/*expression*/>> calculatedExpression = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, int/*decimalPlaces*/>> decimalPlaces = new Dictionary<string, Dictionary<string, int>>();

        private Dictionary<string/*categoryName*/, Dictionary<int/*typeId*/, ElementTypeProperties>> elementTypeDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>();
        private Dictionary<string/*categoryName*/, Dictionary<int/*instId*/, InstanceProperties>> instanceDictionary = new Dictionary<string, Dictionary<int, InstanceProperties>>();
        private Dictionary<string/*categoryName*/, Dictionary<int/*instId*/, ElementProperties>> sysInstDictionary = new Dictionary<string, Dictionary<int, ElementProperties>>();
        private Dictionary<int/*roomId*/, RoomProperties> roomDictionary = new Dictionary<int,RoomProperties>();
        private Dictionary<int/*spaceId*/,SpaceProperties> spaceDictionary = new Dictionary<int, SpaceProperties>();
        private Dictionary<int/*areaId*/, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();
        private Dictionary<int/*viewId*/, ViewProperties> viewDictionary = new Dictionary<int, ViewProperties>();
        
        private string[] defNonEditable = new string[] { "InstanceID", "TypeID", "FamilyID", "FamilyName", "FamilyType","ID","ViewTypeName", "ViewFamilyName"};
        private string[] internalField = new string[] { "FacingFlipped", "FromRoom", "HandFlipped", "Host", "IsSlantedColumn", "Mirrored", "Room", "ToRoom" };
        private List<string> warningTables = new List<string>();
        private Dictionary<string, List<int>> warningRows = new Dictionary<string, List<int>>();
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, string/*DUT*/>> dutDictionary = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, string/*suffix*/>> suffixDictionary = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string/*tableName*/, Dictionary<string/*paramId*/, string/*columnName*/>> doubleFields = new Dictionary<string, Dictionary<string, string>>();
        

        #region Properties
        public Database DaoDB { get { return daoDB; } set { daoDB = value; } }
        public DataSet DataSet { get { return dataSet; } set { dataSet = value; } }
        public Dictionary<string/*tableName*/, List<string>/*fieldNames*/> InternalFields { get { return internalFiedls; } set { internalFiedls = value; } }
        public Dictionary<string, Dictionary<int, ElementTypeProperties>> ElementTypeDictionary { get { return elementTypeDictionary; } set { elementTypeDictionary = value; } }
        public Dictionary<string, Dictionary<int, InstanceProperties>> InstanceDictionary { get { return instanceDictionary; } set { instanceDictionary = value; } }
        public Dictionary<string, Dictionary<int, ElementProperties>> SysInstDictionary { get { return sysInstDictionary; } set { sysInstDictionary = value; } }
        public Dictionary<int, RoomProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }
        public Dictionary<int, SpaceProperties> SpaceDictionary { get { return spaceDictionary; } set { spaceDictionary = value; } }
        public Dictionary<int, AreaProperties> AreaDictionary { get { return areaDictionary; } set { areaDictionary = value; } }
        public Dictionary<int, ViewProperties> ViewDictionary { get { return viewDictionary; } set { viewDictionary = value; } }
        public bool IsRoomSelected { get { return isRoomSelected; } set { isRoomSelected = value; } }
        public bool IsSpaceSelected { get { return isSpaceSelected; } set { isSpaceSelected = value; } }
        public bool IsAreaSelected { get { return isAreaSelected; } set { isAreaSelected = value; } }
        public Dictionary<string, List<string>> NonVisibleFields { get { return nonVisibleFields; } set { nonVisibleFields = value; } }
        public Dictionary<string, Dictionary<string, LockType>> LockTypeFields { get { return lockTypeFields; } set { lockTypeFields = value; } }
        public Dictionary<string, string> ParamIdMap { get { return paramIdMap; } set { paramIdMap = value; } }
        public Dictionary<string, bool> ExcludeInstanceSettings { get { return excludeInstanceSettings; } set { excludeInstanceSettings = value; } }
        public Dictionary<string, Dictionary<string, string>> CalculatedExpression { get { return calculatedExpression; } set { calculatedExpression = value; } }
        public Dictionary<string, Dictionary<string, int>> DecimalPlaces { get { return decimalPlaces; } set { decimalPlaces = value; } }
        public bool WarningMode { get { return warningMode; } set { warningMode = value; } }
        public List<string> WarningTables { get { return warningTables; } set { warningTables = value; } }
        public Dictionary<string, List<int>> WarningRows { get { return warningRows; } set { warningRows = value; } }
        public Dictionary<string, Dictionary<string, string>> DUTDictionary { get { return dutDictionary; } set { dutDictionary = value; } }
        public Dictionary<string, Dictionary<string, string>> DoubleFields { get { return doubleFields; } set { doubleFields = value; } }
        #endregion

        public RevitDBViewer(UIApplication m_app, string filePath, bool isViewer, bool isWarningMode)
        {
            try
            {
                dbPath = filePath;
                warningMode = isWarningMode;
                doc = m_app.ActiveUIDocument.Document;
                oAccess = new Access.Application();
                oAccess.OpenCurrentDatabase(dbPath);
                daoDB = oAccess.CurrentDb();
                oAccess.Visible = false;

                dataSet = new DataSet();

                ReadParamData();
                ReadExcludeInst();
                ReadInternalDBSettings();

                externalReference = new ExternalReference(daoDB);

                if (isViewer) //Viewer or Synchronize 
                {
                    if (warningMode == false) //during synchronizing, when users select a fixing mode for controlling parameters.
                    {
                        TaskDialog taskDialog = new TaskDialog("Synchronization Option");
                        taskDialog.MainInstruction = "Choose a Synchronization Option.";
                        taskDialog.MainContent = "Synchronize to display the most up-to-date data. \nSkip the process if the database is recently synchronized.";
                        taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Synchronize");
                        taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Skip this process");

                        TaskDialogResult tResult = taskDialog.Show();
                        if (TaskDialogResult.CommandLink1 == tResult)
                        {
                            isSynchronzed = true; 
                        }
                        else if (TaskDialogResult.CommandLink2==tResult)
                        {
                            isSynchronzed = false;
                        }

                        if (isSynchronzed)
                        {
                            readRevit = new ReadFromRevit(doc, daoDB);
                            readRevit.IsSpaceSelected = isSpaceSelected;
                            readRevit.IsAreaSelected = isAreaSelected;
                            readRevit.NonVisibleFields = nonVisibleFields;
                            readRevit.LockTypeFields = lockTypeFields;
                            readRevit.ParamIDMap = paramIdMap;
                            readRevit.ExcludeInstSettings = excludeInstanceSettings;
                            readRevit.ExternalFields = externalReference.ExternalFields;
                            readRevit.SourceTables = externalReference.SourceTables;
                            //if (warningMode == false) { readRevit.SetFixMode = true; }// if the viewer is not fixing mode, it will ignore the mismatched data.
                           
                            readRevit.CollectRevitElementsData();
                            readRevit.SaveRevitData(); //Save Revit Data into Database (ReadOnly, RevitOnly, LockAll)
                            warningMode = readRevit.FixMode;
                            warningTables = readRevit.WarningTables;

                            elementTypeDictionary = readRevit.ElementTypeDictionary;
                            instanceDictionary = readRevit.InstanceDictionary;
                            sysInstDictionary = readRevit.SysInstDictionary;
                            roomDictionary = readRevit.RoomDictionary;
                            spaceDictionary = readRevit.SpaceDictionary;
                            areaDictionary = readRevit.AreaDictionary;
                            viewDictionary = readRevit.ViewDictionary;

                            //1. save Revit Data into Dictionary if Visibility is true
                            //2. save Revit Data into Database if the lockType is "isEditable"
                            //3. create Ref_Database field if the locktype is "isDBOnly" (Obsolete)
                            //4. store dictionary data into Revit and Ref_database field 
                            //5. store database data into Database field.
                        }
                    }

                    progressForm = new form_ProgressBar();
                    progressForm.Text = "Opening Data Editor...";
                    progressForm.MaxValue = SetMaxValue();
                    progressForm.LabelText = "Preparing Data Tables for the Viewer...";
                    progressForm.Show();

                    CreateDataTables();

                    unitConverter = new UnitConverter();
                    unitConverter.DUTDictionary = dutDictionary;
                    unitConverter.RevitDataSet = dataSet;
                    unitConverter.CollectParamSuffix();
                    suffixDictionary = unitConverter.SuffixDictionary;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect data from the database."+ex.Message, "Error: RevitDBViewer Initialization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //Read and save parameters setting first
        private void ReadParamData()
        {
            SaveParamInfo();
            SaveParamSettings("Rvt_Type_Parameters"); //to store data of nonVisible, nonEditable, LockType
            SaveParamSettings("Rvt_Inst_Parameters");
        }

        private void ReadExcludeInst()
        {
            try
            {
                excludeInstanceSettings = new Dictionary<string, bool>();
                Recordset recordset = daoDB.OpenRecordset( "UI_ExcludeInstance");

                string categoryName = "";
                bool exclude = false;

                while (!recordset.EOF)
                {
                    categoryName = recordset.Fields["CategoryName"].Value;
                    exclude = Convert.ToBoolean(recordset.Fields["ExcludeInstance"].Value);
                    excludeInstanceSettings.Add(categoryName, exclude);
                    recordset.MoveNext();
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save parameters information. \n" + ex.Message, "DB Viewer Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //to create a map between parameter ids and names
        private void SaveParamInfo()
        {
            try
            {
                fieldDictionary=new Dictionary<string,FieldProperties>();
                Recordset recordset = daoDB.OpenRecordset("Rvt_ParameterInfo");

                string paramName = "";
                string paramId = "";
                string paramFormat = "";
                string paramType = "";
                string displayUnitType = "";

                while (!recordset.EOF)
                {
                    paramId = recordset.Fields["ParamID"].Value;
                    paramName = recordset.Fields["ParamName"].Value;
                    paramFormat = recordset.Fields["ParamFormat"].Value;
                    paramType = recordset.Fields["ParamType"].Value;
                    displayUnitType = recordset.Fields["DisplayUnitType"].Value;

                    FieldProperties fp = new FieldProperties(paramName, paramId, paramFormat);
                    fp.ParamType = paramType;
                    fp.DisplayUnitType = displayUnitType;

                    if (!paramIdMap.ContainsKey(paramId))
                    {
                        paramIdMap.Add(paramId, paramName);
                        fieldDictionary.Add(paramId, fp);
                    }
                    recordset.MoveNext();
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save parameters information. \n" + ex.Message, "DB Viewer Error:",MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveParamSettings(string paramTable)
        {
            try
            {
                Recordset recordset = daoDB.OpenRecordset(paramTable);
                string prefix = "";
                string categoryName = "";
                string tableName = "";
                string paramId = "";
                string paramName="";

                while (!recordset.EOF)
                {
                    categoryName = recordset.Fields["CategoryName"].Value;
                 
                    if (categoryName.Contains("Rooms")) { prefix = "Rvt_"; isRoomSelected = true; } //Rvt_Rooms, Rvt_Spaces
                    else if (categoryName.Contains("Spaces")) { prefix = "Rvt_"; isSpaceSelected = true; }
                    else if (categoryName.Contains("Areas")) { prefix = "Rvt_"; isAreaSelected = true; }
                    else { prefix = paramTable.Substring(4, 5); }
                    tableName = prefix + categoryName;

                    paramName=recordset.Fields["ParamName"].Value;
                    paramId = recordset.Fields["ParamID"].Value;

                    bool visible = Convert.ToBoolean(recordset.Fields["IsVisible"].Value);
                    if (visible)
                    {
                        bool editable = Convert.ToBoolean(recordset.Fields["IsEditable"].Value);
                        if (!editable)
                        {
                            if (!nonEditableFields.ContainsKey(tableName)) { nonEditableFields.Add(tableName, new List<string>()); nonEditableFields[tableName].Add(paramName); }
                            else if (!nonEditableFields[tableName].Contains(paramName)) { nonEditableFields[tableName].Add(paramName); }
                        }

                        if (!lockTypeFields.ContainsKey(tableName)) { lockTypeFields.Add(tableName, new Dictionary<string, LockType>()); }
                        if (!lockTypeFields[tableName].ContainsKey(paramName))
                        {
                            bool isReadOnly = Convert.ToBoolean(recordset.Fields["IsReadOnly"].Value);
                            bool isLockAll = Convert.ToBoolean(recordset.Fields["IsLockAll"].Value);
                            bool isEditable = Convert.ToBoolean(recordset.Fields["IsEditable"].Value);

                            if (isReadOnly) { lockTypeFields[tableName].Add(paramName, LockType.ReadOnly); }
                            else if (isLockAll) { lockTypeFields[tableName].Add(paramName, LockType.LockAll); }
                            else if (isEditable) { lockTypeFields[tableName].Add(paramName, LockType.Editable); }
                            else { lockTypeFields[tableName].Add(paramName, LockType.None); }
                        }

                        //DisplayUnitType dictionary
                        if (!dutDictionary.ContainsKey(tableName)) { dutDictionary.Add(tableName, new Dictionary<string, string>()); }
                        if (!dutDictionary[tableName].ContainsKey(paramName))
                        {
                            if (fieldDictionary.ContainsKey(paramId))
                            {
                                FieldProperties fp = fieldDictionary[paramId];
                                if (fp.DisplayUnitType != "NULL") { dutDictionary[tableName].Add(paramName, fp.DisplayUnitType); } //excluding null
                            }
                        }

                    }
                    else
                    {
                        if (!nonVisibleFields.ContainsKey(tableName)) { nonVisibleFields.Add(tableName, new List<string>()); nonVisibleFields[tableName].Add(paramName); }
                        else if (!nonVisibleFields[tableName].Contains(paramName)) { nonVisibleFields[tableName].Add(paramName); }
                    }
                    recordset.MoveNext();
                }
                recordset.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save parameters settings. \n" + ex.Message, "DB Viewer Error:");
            }
        }

        private void ReadInternalDBSettings()
        {
            string tableName = "UI_RevitInternalDB";
            try
            {
                Recordset recordset;
                recordset = daoDB.OpenRecordset(tableName);
                if (recordset.RecordCount > 0)
                {
                    while (!recordset.EOF)
                    {
                        string table = recordset.Fields["TableName"].Value;
                        string objectName = recordset.Fields["ObjectName"].Value;
                        string fieldName = recordset.Fields["FieldName"].Value;

                        if (internalFiedls.ContainsKey(table))
                        {
                            List<string> fieldNames = new List<string>();
                            fieldNames = internalFiedls[table];
                            fieldNames.Add(objectName + ":" + fieldName);
                            internalFiedls.Remove(table);
                            internalFiedls.Add(table, fieldNames);
                        }
                        else
                        {
                            List<string> fieldNames = new List<string>();
                            fieldNames.Add(objectName + ":" + fieldName);
                            internalFiedls.Add(table, fieldNames);
                        }
                        recordset.MoveNext();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Read Data from Table: " + tableName + "\n" + ex.Message);
            }
        }

        private int SetMaxValue()
        {
            int max = 0;
            foreach (TableDef table in daoDB.TableDefs)
            {
                string name=table.Name;
                if (name.Contains("Parameters")) { continue; }
                else if (name.Contains("Inst_") || name.Contains("Type_") || name.Contains("Rooms") || name.Contains("Spaces")||name.Contains("Areas"))
                {
                    Recordset recordset = daoDB.OpenRecordset(name);
                    max += recordset.RecordCount;
                    recordset.Close();
                }
            }
            return max;
        }

        private void CreateDataTables()
        {
            DataTable dataTable = new DataTable();

            TableDefs tables = daoDB.TableDefs;
            foreach (TableDef table in tables)
            {
                if (table.Name.Contains("Parameters"))
                {
                    continue;
                }
                else if (table.Name.Contains("Inst_"))
                {
                    string categoryName = table.Name.Substring(5);
                    if (excludeInstanceSettings.ContainsKey(categoryName)) { if (excludeInstanceSettings[categoryName]) { continue; } }
                    dataTable = ReadDataFromTable(table);
                    dataSet.Tables.Add(dataTable);
                }
                else if (table.Name.Contains("Type_"))
                {
                    dataTable = ReadDataFromTable(table);
                    dataSet.Tables.Add(dataTable);
                }
                else if (table.Name.Contains("Rooms") || table.Name.Contains("Spaces")||table.Name.Contains("Areas"))
                {
                    dataTable = ReadDataFromTable(table);
                    dataSet.Tables.Add(dataTable);
                    
                }
            }
            progressForm.CurValue = progressForm.MaxValue;
        }

        private DataTable ReadDataFromTable(TableDef table)
        {
            string tableName = table.Name;
            Dictionary<string/*colName*/, int/*indexColumn*/> indexMap = new Dictionary<string, int>();// to avoid errors occured by parameters names: Height, height

            List<string> nonVisibles = new List<string>();
            List<string> nonEditables = new List<string>();
            if (nonVisibleFields.ContainsKey(tableName))  {  nonVisibles = nonVisibleFields[tableName];  }
            if (nonEditableFields.ContainsKey(tableName))  {  nonEditables = nonEditableFields[tableName];  }
            if (!doubleFields.ContainsKey(tableName)) { doubleFields.Add(tableName, new Dictionary<string, string>()); }

            try
            {
                Recordset recordset = daoDB.OpenTable(tableName);
                DataTable dataTable = new DataTable(tableName);
                dataTable.CaseSensitive = true; 
                DataColumn column;
                DataRow row;

                string primaryKey = "";
                foreach (Index index in table.Indexes)
                {
                    if (index.Primary)
                    {
                        foreach (Field field in index.Fields)
                        {
                            primaryKey = field.Name; break;
                        }
                    }
                }

                string fieldName="";
                string paramName="";

                int i = 0;
                foreach (Field field in table.Fields)
                {
                    fieldName = field.Name;
                    if (fieldName.Contains("FamilyID")) { continue; }
                    if (tableName.Contains("Inst_") && fieldName.Contains("TypeID")) { continue; }

                    if(paramIdMap.ContainsKey(fieldName)) { paramName = paramIdMap[fieldName]; }
                    else { paramName = fieldName;}

                    if (fieldName.Contains("Dbl_"))
                    {
                        string paramId = fieldName.Replace("Dbl_", "");
                        doubleFields[tableName].Add(paramId, fieldName);
                        continue;
                    }

                    //Visibility Setting
                    if (nonVisibles.Contains(paramName)) { continue; }
                    if (indexMap.ContainsKey(paramName)) { continue; }

                    column = new DataColumn();
                    column.DataType = System.Type.GetType(GetDataType(fieldName));
                    column.ColumnName = paramName;
                    
                    //Key setting
                    if (primaryKey == fieldName) { column.Unique = true; }
                    else { column.Unique = false; }

                    //Readonly Setting
                    Field2 field2 = field as Field2;
                    if (field2.Expression != string.Empty) 
                    { 
                        column.ReadOnly = true;
                        UpdateFomulatedLockType(table.Name, paramName, LockType.Calculated);
                        UpdateDecimalPlaces(table.Name, field2);
                    }
                    else if (nonEditables.Contains(paramName) || defNonEditable.Contains(paramName)) { column.ReadOnly = true; }
                    else { column.ReadOnly = false; }

                    dataTable.Columns.Add(column);
                    indexMap.Add(column.ColumnName, i);
                    i++;

                }
                
                if (primaryKey.Length > 0)
                {
                    DataColumn[] PrimaryKeyColumns = new DataColumn[1];
                    PrimaryKeyColumns[0] = dataTable.Columns[primaryKey];
                    dataTable.PrimaryKey = PrimaryKeyColumns;
                }

                while (!recordset.EOF)
                {
                    progressForm.PerformStep();
                    row = dataTable.NewRow();

                    foreach (Field field in recordset.Fields)
                    {
                        string colName = field.Name;
                        if (paramIdMap.ContainsKey(field.Name)) { colName = paramIdMap[field.Name]; }
                        if (indexMap.ContainsKey(colName))
                        {
                            int index = indexMap[colName];
                            if (dataTable.Columns[index].DataType == typeof(System.Boolean))
                            {
                                if (Convert.IsDBNull(field.Value)) { row[index] = false; continue; }
                                else if ((string)field.Value == "1") { row[index] = true; }
                                else { row[index] = false; }
                            }
                            else if (field.Type == 7)//double field
                            {
                                if (Convert.IsDBNull(field.Value)) { row[index] = 0; }
                                double val = Convert.ToDouble(field.Value);
                                int decimalPlace = 2;
                                if (decimalPlaces.ContainsKey(table.Name))
                                {
                                    if (decimalPlaces[table.Name].ContainsKey(field.Name))
                                    {
                                        decimalPlace = decimalPlaces[table.Name][field.Name];
                                    }
                                }
                                row[index] = Math.Round(val, decimalPlace);
                            }
                            else if (!Convert.IsDBNull(field.Value))
                            {
                                row[index] = field.Value; //overwrite value when there's any duplicate names of parameters with different paramIds
                            }
                        }
                    }
                    dataTable.Rows.Add(row);
                    recordset.MoveNext();
                }
                recordset.Close();
                
                dataTable = AddInternalData(dataTable);
                
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Read Data from Table: "+tableName+"\n" + ex.Message); 
                return null;
            }
        }

        private void UpdateDecimalPlaces(string tableName, Field2 field)
        {
            foreach (Property property in field.Properties)
            {
                if (property.Name == "DecimalPlaces")
                {
                    int place = Convert.ToInt32(property.Value);
                    if (decimalPlaces.ContainsKey(tableName))
                    {
                        if (decimalPlaces[tableName].ContainsKey(field.Name))
                        {
                            decimalPlaces[tableName][field.Name] = place;
                        }
                        else
                        {
                            decimalPlaces[tableName].Add(field.Name, place);
                        }
                    }
                    else
                    {
                        decimalPlaces.Add(tableName, new Dictionary<string, int>());
                        decimalPlaces[tableName].Add(field.Name, place);
                    }
                }
            }
        }

        private string GetDataType(string fieldName)
        {
            string dataType = "System.String"; //default
            try
            {
                if (fieldDictionary.ContainsKey(fieldName))
                {
                    string paramFormat = fieldDictionary[fieldName].ParamFormat;
                    string paramType = fieldDictionary[fieldName].ParamType;
                    if (paramFormat == "Double")
                    {
                        //dataType = "System.Double";
                        dataType = "System.String"; 
                    }
                    else if (paramFormat == "Integer")
                    {
                        if (paramType == "YesNo")
                        {
                            dataType = "System.Boolean";
                        }
                        else
                        {
                            dataType = "System.Int32";
                        }
                    }
                }
                else if (fieldName.Contains("Dbl_"))
                {
                    dataType = "System.Double";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get data type for datacolumn.\n"+ex.Message, "RevitDBViewer Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dataType;
        }

        private string GetParameterName(string paramId)
        {
            string paramName = "";
            try
            {
                int id = 0;

                if (int.TryParse(paramId, out id))
                {
                    string paramTable = "Rvt_ParameterInfo";
                    string strSql = "SELECT ParamName FROM " + paramTable + " WHERE ParamID ='" + paramId + "'";
                    Recordset recordset;
                    recordset = daoDB.OpenRecordset(strSql);

                    if (recordset.RecordCount > 0)
                    {
                        paramName = recordset.Fields["ParamName"].Value;
                    }
                    recordset.Close();
                    return paramName;
                }
                else
                {
                    return paramId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter name from parameter id:"+paramId+" \n" + ex.Message, "RevitDBViewer Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return paramName;
            }
        }

        private string GetParameterId(string tableName, string paramName)
        {
            string paramId = "";
            try
            {
                string prefix = tableName.Split('_')[0];
                string categoryName = tableName.Split('_')[1];

                if (categoryName.Contains("Rooms") || categoryName.Contains("Spaces") ||categoryName.Contains("Areas")) { prefix = "Type"; }
                string paramTable = "Rvt_" + prefix + "_Parameters";
                string strSql = "SELECT ParamID FROM " + paramTable + " WHERE CategoryName='"+categoryName+"' AND ParamName ='" + paramName + "'";
                Recordset recordset = daoDB.OpenRecordset(strSql);
                if (recordset.RecordCount > 0)
                {
                    paramId=recordset.Fields["ParamID"].Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter Id from parameter name. \n"+ex.Message, "RevitDBViewer Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return paramId;
        }

        private DataTable AddInternalData(DataTable dTable)
        {
            DataTable dataTable = dTable;
            try
            {
                if (internalFiedls.ContainsKey(dataTable.TableName))
                {
                    List<string> fieldNames = new List<string>();
                    fieldNames = internalFiedls[dataTable.TableName];

                    foreach (string fieldName in fieldNames)
                    {
                        DataColumn column = new DataColumn();
                        column.DataType = System.Type.GetType("System.String");
                        column.ColumnName = fieldName;

                        dataTable.Columns.Add(column);
                    }

                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (string fieldName in fieldNames)
                        {
                            string[] strArray = fieldName.Split(':');
                            string objectName = strArray[0];
                            string field = strArray[1];
                            string Id = "";
                            if (objectName.Contains("Instance")) //Family Instaces: "FacingFlipped", "HandFlipped", "IsSlantedColumn", "Mirrored"
                            {
                                Id = row["InstanceID"].ToString();
                            }
                            else
                            {
                                Id = row[objectName].ToString();
                            }
                            if (Id != string.Empty)
                            {
                                row[fieldName] = FindValues(dataTable.TableName, objectName, field, Id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add internal data: " + dTable.TableName + "\n" + ex.Message, "RevitDBViewer AddInternalData Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        private DataTable DeleteInternalData(DataTable dTable)
        {
            DataTable dataTable = dTable;

            for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
            {
                DataColumn column = dataTable.Columns[i];
                if (column.ColumnName.Contains(":"))
                {
                    dataTable.Columns.RemoveAt(i);
                }
            }
            return dataTable;
        }

        //when a new field is created, reload updated database into the viewer
        public DataTable ReloadDataTable(DataTable dTable)
        {
            DataTable dataTable = dTable;
            try
            {
                Recordset recordset = daoDB.OpenRecordset(dataTable.TableName);
                if (recordset.RecordCount > 0)
                {
                    Dictionary<string/*paramName*/, string/*fieldName*/> fieldPairs = new Dictionary<string, string>();
                    
                    foreach (Field field in recordset.Fields)
                    {
                        Field2 field2 = field as Field2;
                        if (field2.Expression != string.Empty)
                        {
                            string paramName = field.Name;
                            if (paramIdMap.ContainsKey(field.Name)) { paramName = paramIdMap[field.Name]; }
                            fieldPairs.Add(paramName, field.Name);
                            UpdateDecimalPlaces(dataTable.TableName, field2);
                            UpdateFomulatedLockType(dataTable.TableName, paramName, LockType.Calculated);
                        }
                    }

                    foreach (string columnName in fieldPairs.Keys)
                    {
                        dataTable.Columns[columnName].ReadOnly = false;
                    }

                    int rowIndex = 0;
                    while (!recordset.EOF)
                    {
                        foreach (string columnName in fieldPairs.Keys)
                        {
                            string fieldName = fieldPairs[columnName];
                            Field field = recordset.Fields[fieldName];

                            if (field.Type == 7) //short enum for Double
                            {
                                if (Convert.IsDBNull(field.Value))
                                {
                                    dataTable.Rows[rowIndex][columnName] = 0;
                                }
                                else
                                {
                                    double val = Convert.ToDouble(field.Value);
                                    int decimalPlace = 2;
                                    if (decimalPlaces.ContainsKey(dataTable.TableName))
                                    {
                                        if (decimalPlaces[dataTable.TableName].ContainsKey(field.Name))
                                        {
                                            decimalPlace = decimalPlaces[dataTable.TableName][field.Name];
                                        }
                                    }
                                    dataTable.Rows[rowIndex][columnName] = Math.Round(val, decimalPlace);
                                }
                            }
                            else
                            {
                                dataTable.Rows[rowIndex][columnName] = field.Value;
                            }
                        }
                        rowIndex++;
                        recordset.MoveNext();
                    }

                    foreach (string columnName in fieldPairs.Keys)
                    {
                        dataTable.Columns[columnName].ReadOnly = true;
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to reload dataTable: " + dTable.TableName + "\n" + ex.Message, "RevitDBViewer Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dataTable;
        }

        public void AddToolTipExpression(string tableName, DataGridView dataGridview)
        {
            Recordset recordset = daoDB.OpenRecordset(tableName);
            if (recordset.RecordCount > 0)
            {
                foreach (Field field in recordset.Fields)
                {
                    string paramName = field.Name;
                    if (paramIdMap.ContainsKey(field.Name)) { paramName = paramIdMap[field.Name]; }

                    Field2 field2 = field as Field2;
                    string expression = field2.Expression;
                    string tooltipText = expression;
                    if (expression != string.Empty)
                    {
                        string[] splitStr = expression.Split(new char[] { '[', ']' });

                        for (int i = 0; i < splitStr.Length; i++)
                        {
                            if(splitStr[i].Contains("Dbl_"))
                            {
                                string paramId = splitStr[i].Replace("Dbl_", "");
                                if (paramIdMap.ContainsKey(paramId))
                                {
                                    string name = paramIdMap[paramId];
                                    tooltipText = tooltipText.Replace(splitStr[i], name);
                                }
                            }
                            
                        }
                        if (dataGridview.Columns.Contains(paramName))
                        {
                            dataGridview.Columns[paramName].ToolTipText = tooltipText;
                        }
                    }
                    else if(dataGridview.Columns.Contains(paramName))
                    {
                        dataGridview.Columns[paramName].ToolTipText = paramName;
                    }
                }
            }
            recordset.Close();
        }

        public string FindValues(string tableName, string objectName, string fieldName, string id)
        {
            string value = "";
            string strSql = "";
            try
            {
                switch (objectName)
                {
                    case "Family Instance":
                        strSql = "SELECT [" + fieldName + "] FROM [" + tableName + "] WHERE InstanceID='" + id + "'";
                        break;
                    case "Host":
                        break;
                    default: //Room
                        strSql = "SELECT * FROM Rvt_Rooms WHERE ID='" + id + "'";
                        break;
                }

                Recordset recordset;
                recordset = daoDB.OpenRecordset(strSql);

                if (recordset.RecordCount > 0)
                {
                    if (objectName == "Family Instance")
                    {
                        value = recordset.Fields[fieldName].Value;
                    }
                    else
                    {
                        foreach (Field field in recordset.Fields)
                        {
                            string paramName = "";
                            if (paramIdMap.ContainsKey(field.Name)) { paramName = paramIdMap[field.Name]; }
                            if (paramName == fieldName)
                            {
                                value = recordset.Fields[field.Name].Value; break;
                            }
                        }
                    }
                }

                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find value from the query: \n" + strSql + "\n" + ex.Message, "RevitDBViewer FindValues Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return value;
        }

        //to add or delete internal data
        public void UpdateDataTable()
        {
            try
            {
                for (int i = dataSet.Tables.Count - 1; i >= 0; i--)
                {
                    DataTable dataTable = dataSet.Tables[i];
                    if (dataSet.Tables.CanRemove(dataTable) && internalFiedls.ContainsKey(dataTable.TableName))
                    {
                        DataTable newTable = new DataTable();
                        newTable = DeleteInternalData(dataTable); //delete previously defined DataColumn with internal data
                        newTable = AddInternalData(newTable); //Add internal Data

                        dataSet.Tables.RemoveAt(i);
                        dataSet.Tables.Add(newTable);
                    }
                }

                UpdateInternalDBSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update datatables\n" + ex.Message, "RevitDBViewer UpdateDataTable Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateInternalDBSettings()
        {
            try
            {
                string query = SqlHelper.DeleteTable("UI_RevitInternalDB");
                daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);

                foreach (string tableName in internalFiedls.Keys)
                {
                    Dictionary<string/*field*/, string/*value*/> valuePair = new Dictionary<string, string>();

                    List<string> fieldNames = new List<string>();
                    fieldNames = internalFiedls[tableName];

                    foreach (string item in fieldNames)
                    {
                        string[] strArray = item.Split(':');

                        valuePair = new Dictionary<string, string>();
                        valuePair.Add("TableName", tableName);
                        valuePair.Add("ObjectName", strArray[0]);
                        valuePair.Add("FieldName", strArray[1]);

                        query = SqlHelper.InsertIntoTable("UI_RevitInternalDB", valuePair);
                        daoDB.Execute(query, RecordsetOptionEnum.dbFailOnError);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update internal Revit properties.\n" + ex.Message, "RevitDBViewer UpdateInternalDBSettings Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool UpdateCell(string tableName, string fieldName, string value, string keyField, string keyValue, double dblValue)
        {
            string paramId = GetParameterId(tableName, fieldName);
            if (value.Contains("'")) { value = value.Replace("'", "''"); }
           
            string queryStr = "UPDATE [" + tableName + "] SET [" + paramId + "] = '" + value + "' WHERE [" + keyField + "] = '" + keyValue + "'";
            try
            {
                if (string.Empty != paramId)
                {
                    daoDB.Execute(queryStr, RecordsetOptionEnum.dbFailOnError);
                    if (doubleFields.ContainsKey(tableName))
                    {
                        if (doubleFields[tableName].ContainsKey(paramId))
                        {
                            string dblField = doubleFields[tableName][paramId];
                            queryStr = "UPDATE [" + tableName + "] SET [" + dblField + "] = '" + dblValue + "' WHERE [" + keyField + "] = '" + keyValue + "'";
                            daoDB.Execute(queryStr, RecordsetOptionEnum.dbFailOnError);
                        }
                    }

                }
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot update with the query. \n"+queryStr+"\n"+ex.Message, "RevitDBViewer UpdateCell Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void GrayTextReadOnly(DataTable dataTable, DataGridView gridView)
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                gridView.Columns[column.ColumnName].ReadOnly = column.ReadOnly;
                if (column.ReadOnly)
                {
                    gridView.Columns[column.ColumnName].DefaultCellStyle.ForeColor = System.Drawing.Color.Gray;
                }
                else
                {
                    gridView.Columns[column.ColumnName].DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                }
            }
        }

        public void AddDoubleTag(string dataTable, DataGridView gridView)
        {
            unitConverter.AddDoubleValueTag(dataTable, gridView);
        }

        public void HideInternalObject(DataGridView gridView)
        {
            foreach (DataGridViewColumn col in gridView.Columns)
            {
                if (internalField.Contains(col.HeaderText))
                {
                    col.Visible = false;
                }
            }
        }

        public void HideInternalData(DataGridView gridView)
        {
            foreach (DataGridViewColumn col in gridView.Columns)
            {
                if (col.HeaderText.Contains(":") && !col.HeaderText.Contains("Ref"))
                {
                    string[] internalPrefix = col.HeaderText.Split(':');
                    if (internalField.Contains(internalPrefix[0]) || internalField.Contains(internalPrefix[1])) { col.Visible = false; }
                }
            }
        }

        public void UnhideInternalData(DataGridView gridView)
        {
            foreach (DataGridViewColumn col in gridView.Columns)
            {
                if (col.HeaderText.Contains(":") && !col.HeaderText.Contains("Ref"))
                {
                    col.Visible = true;
                    col.ReadOnly = true;
                    col.DefaultCellStyle.ForeColor = System.Drawing.Color.Gray;
                }
            }
        }

        //order columns in the daatagridview equivalent to a corresponding datatable
        public void OrderColumns(DataTable dTable, DataGridView gridView)
        {
            for (int i = 0; i < dTable.Columns.Count; i++)
            {
                string fieldName = dTable.Columns[i].ColumnName;
                gridView.Columns[fieldName].DisplayIndex = i;
            }
        }

        public void CreateCheckBoxColumns(DataTable dTable, DataGridView gridView)
        {
            List<string> YesNoColumns = new List<string>(); //column names of yesno paramtype
            foreach (DataColumn column in dTable.Columns)
            {
                if (column.DataType == typeof(System.Int32))
                {
                    string paramId = GetParameterId(dTable.TableName, column.ColumnName);
                    if (fieldDictionary.ContainsKey(paramId))
                    {
                        FieldProperties fp = fieldDictionary[paramId];
                        if (fp.ParamType == "YesNo")
                        {
                            YesNoColumns.Add(column.ColumnName);
                        }
                    }
                }
            }

            foreach (DataGridViewColumn col in gridView.Columns)
            {
                if (YesNoColumns.Contains(col.Name))
                {
                    col.CellTemplate = new DataGridViewCheckBoxCell();
                    col.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        //when field contains expressions, lock type will be changed into 'calculated'
        public void UpdateFomulatedLockType(string tableName, string fieldName, LockType lockType)
        {
            if (lockTypeFields.ContainsKey(tableName))
            {
                if (lockTypeFields[tableName].ContainsKey(fieldName))
                {
                    lockTypeFields[tableName].Remove(fieldName);
                }
                lockTypeFields[tableName].Add(fieldName, lockType);
            }
            else
            {
                Dictionary<string, LockType> lockTypes = new Dictionary<string, LockType>();
                lockTypes.Add(fieldName, lockType);
                lockTypeFields.Add(tableName, lockTypes);
            }
        }

        public bool ConnectExternalReference(DataTable dataTable, DataGridView gridView)
        {
            return externalReference.CreateComboBoxList(dataTable, gridView);
        }

        public void DisplayExternalData(DataTable dataTable, DataGridView gridView)
        {
            externalReference.DisplayExternalData(dataTable, gridView);
        }

        public void HideExtColumns(DataTable dataTable, DataGridView mainGridView)
        {
            externalReference.HideParameters(dataTable, mainGridView);
        }

        public void ShowExtColumns(DataTable dataTable, DataGridView mainGridView)
        {
            externalReference.ShowParameters(dataTable, mainGridView);
        }

        public void UpdateExternalData(DataTable dataTable, DataGridView gridViewExt, int id, int rowIndex)
        {
            externalReference.UpdateExternalData(dataTable, gridViewExt, id, rowIndex);
        }

        public void ColorWarningCells(DataTable dataTable, DataGridView gridViewMain,DataGridView gridViewExt )
        {
            externalReference.WarningRows = warningRows;
            externalReference.ColorWarningCells(dataTable, gridViewMain, gridViewExt);
            warningRows = externalReference.WarningRows;
        }

        public void DrawHeaderCell(DataGridViewCellPaintingEventArgs e, string tableName, DataGridView gridView, ImageList imageList)
        {
            string colName = gridView.Columns[e.ColumnIndex].HeaderText;
            Image headerImg=null;
            if (lockTypeFields.ContainsKey(tableName))
            {
                if (lockTypeFields[tableName].ContainsKey(colName))
                {
                    LockType lockType = lockTypeFields[tableName][colName];
                    switch (lockType)
                    {
                        case LockType.ReadOnly:
                            headerImg = imageList.Images[4];
                            break;
                        case LockType.LockAll:
                            headerImg = imageList.Images[1];
                            break;
                        case LockType.Editable:
                            headerImg = imageList.Images[2];
                            break;
                        case LockType.Calculated:
                            headerImg = imageList.Images[5];
                            break;
                        case LockType.None:
                            headerImg = null;
                            break;
                    }

                    if (headerImg != null)
                    {
                        e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                        e.Graphics.DrawImage(headerImg, e.CellBounds.Right - 25, e.CellBounds.Top + 3);
                        e.Handled = true;
                    }
                }
            }
            if (colName.Contains(":")) 
            {
                //Internal fields
                string[] internalPrefix = colName.Split(':');
                if (internalField.Contains(internalPrefix[0])||internalField.Contains(internalPrefix[1])) { headerImg = imageList.Images[7]; }
              
                //external fields
                if (colName.Contains("Ext")) { headerImg = imageList.Images[5]; }

                if (headerImg != null)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                    e.Graphics.DrawImage(headerImg, e.CellBounds.Right - 25, e.CellBounds.Top + 3);
                    e.Handled = true;
                }
            }
            
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
    }
}
