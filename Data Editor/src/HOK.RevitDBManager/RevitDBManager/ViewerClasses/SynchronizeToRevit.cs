using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Access.Dao;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using System.Data;
using RevitDBManager.GenericForms;

namespace RevitDBManager.Classes
{
    public class SynchronizeToRevit
    {
        private UIApplication m_app;
        private Autodesk.Revit.DB.Document doc;
        private Database daoDB;
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, LockType>> lockTypeFields = new Dictionary<string, Dictionary<string/*fieldName*/, LockType>>();
        private Dictionary<string/*paramId*/, string/*paramName*/> paramIdMap = new Dictionary<string, string>();
        private Dictionary<string, bool> excludeInstSettings = new Dictionary<string, bool>();
        private ToolStripProgressBar progressBar;

        private Dictionary<string/*tableName*/, Dictionary<string/*keyId*/, List<string>/*colNames*/>> changedValues = new Dictionary<string, Dictionary<string, List<string>>>();
        private Dictionary<string/*fieldName*/, int/*decimalPlaces*/> decimalPlaces = new Dictionary<string/*fieldName*/, int/*decimalPlaces*/>();

        private string[] systemField = new string[] { "InstanceID", "TypeID", "FamilyID", "FamilyName", "FamilyType","ID"};
        private string[] internalField = new string[] { "FacingFlipped", "FromRoom", "HandFlipped", "Host", "IsSlantedColumn", "Mirrored", "Room", "ToRoom" };

        public Dictionary<string, Dictionary<string, LockType>> LockTypeFields { get { return lockTypeFields; } set { lockTypeFields = value; } }
        public Dictionary<string, string> ParamIdMap { get { return paramIdMap; } set { paramIdMap = value; } }
        public Dictionary<string, bool> ExcludeInstSettings { get { return excludeInstSettings; } set { excludeInstSettings = value; } }
        public ToolStripProgressBar ProgressBar { get { return progressBar; } set { progressBar = value; } }
        public Dictionary<string, Dictionary<string, List<string>>> ChangedValues { get { return changedValues; } set { changedValues = value; } }

        public SynchronizeToRevit(UIApplication application,Database database)
        {
            m_app = application;
            doc=m_app.ActiveUIDocument.Document;
            daoDB = database;
        }

        public bool SyncTableToRevit(string tableName)
        {
            bool result = false;
            try
            {
                List<string> validFields = new List<string>();
                validFields = GetValidFields(tableName);

                string idField = "";
                if (tableName.Contains("Inst_")) { idField = "InstanceID"; }
                else if (tableName.Contains("Type_")) { idField = "TypeID"; }
                else if (tableName.Contains("Rvt_")) { idField = "ID"; }

                Recordset recordset = daoDB.OpenTable(tableName);

                progressBar.Value = 1;
                progressBar.Maximum = recordset.RecordCount;

                FindDecimalPlaces(recordset.Fields);
               
                List<string> changedFields = new List<string>();
                List<string> calculatedFields = new List<string>();

                if (lockTypeFields.ContainsKey(tableName))
                {
                    var calFields = from calfield in lockTypeFields[tableName]
                                    where calfield.Value == LockType.Calculated
                                    select calfield.Key;
                    calculatedFields.AddRange(calFields);
                }

                while (!recordset.EOF)
                {
                    string keyId = recordset.Fields[idField].Value;
                    int id = int.Parse(keyId);
                    ElementId elementID = new ElementId(id);
                    Element element = doc.GetElement(elementID);

                    if (null != element)
                    {
                        changedFields = new List<string>();
                        changedFields.AddRange(calculatedFields);

                        if (changedValues[tableName].ContainsKey(keyId))
                        {
                            changedFields.AddRange(changedValues[tableName][keyId]);
                        }

                        if (changedFields.Count > 0)
                        {
                            Fields fields = recordset.Fields;
                            AddValueToRevit(element, fields, validFields, changedFields);
                        }
                    }
                    progressBar.PerformStep();
                    recordset.MoveNext();
                }
                recordset.Close();
                progressBar.Visible = false;
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Read Data from table:"+ tableName+"\n" + ex.Message,"SynchronizeToRevit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                result = false;
            }
            return result;
        }

        private void FindDecimalPlaces(Fields fields)
        {
            decimalPlaces = new Dictionary<string/*fieldName*/, int/*decimalPlaces*/>();
            foreach (Field field in fields)
            {
                if (field.Type == 7)
                {
                    foreach (Property property in field.Properties)
                    {
                        if (property.Name == "DecimalPlaces")
                        {
                            int place = Convert.ToInt32(property.Value);
                            if (!decimalPlaces.ContainsKey(field.Name))
                            {
                                decimalPlaces.Add(field.Name, place);
                            }
                        }
                    }
                }
            }
        }

        //Valid Fields for Sync: Editable, Calculated
        private List<string> GetValidFields(string tableName)
        {
            List<string> validFields = new List<string>(); //paramIds

            if (lockTypeFields.ContainsKey(tableName))
            {
                foreach (string paramName in lockTypeFields[tableName].Keys)
                {
                    LockType lockType = lockTypeFields[tableName][paramName];
                    if (lockType == LockType.Editable || lockType == LockType.Calculated)
                    {
                        string paramId = GetParameterId(tableName, paramName);
                        if (!validFields.Contains(paramId))
                        {
                            validFields.Add(paramId);
                        }
                    }
                }
            }

            return validFields;
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
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter Id from parameter name. \n" + ex.Message, "RevitDBViewer Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return paramId;
        }

        //synch only if the field is visible in the viewer and the lock type should be one among RevitOnly, DBOnly
        private void AddValueToRevit(Element element, Fields fields, List<string> validFields, List<string> changedFields)
        {
            string paramName = "";
            try
            {
                foreach (string paramId in validFields)
                {
                    bool isDouble = false;
                    string strValue = "";
                    double dblValue = 0;

                    if (paramIdMap.ContainsKey(paramId)) { paramName = paramIdMap[paramId]; }
                    else { continue; }

                    if (!changedFields.Contains(paramName)) { continue; }

                    Field field = fields[paramId];
                    if (Convert.IsDBNull(field.Value)) { continue; }

                    if (field.Type == 7) 
                    { 
                        isDouble = true;
                        dblValue = field.Value; 
                        if(decimalPlaces.ContainsKey(field.Name)) { dblValue=Math.Round(dblValue, decimalPlaces[field.Name]); }
                    }
                    else { isDouble = false; strValue = field.Value; }
                    
                    if (strValue == "NULL") { continue; }

#if RELEASE2013||RELEASE2014
                     Autodesk.Revit.DB.Parameter parameter = element.get_Parameter(paramName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017 || RELEASE2018
                    Autodesk.Revit.DB.Parameter parameter = element.LookupParameter(paramName);
#endif

                    if (null != parameter && !parameter.IsReadOnly)
                    {
                        switch (parameter.StorageType)
                        {
                            case StorageType.Double:
                                if (isDouble)
                                {
                                    parameter.Set(dblValue);
                                }
                                else
                                {
                                    parameter.SetValueString(strValue);
                                }
                                //double dblVal = double.Parse(value);
                                //parameter.Set(dblVal);
                                break;
                            case StorageType.Integer:
                                int intVal = int.Parse(strValue);
                                parameter.Set(intVal);
                                break;
                            case StorageType.ElementId:
                                //ElementId eId = new ElementId(int.Parse(value));
                                //if (null != eId) { parameter.Set(eId); }
                                break;
                            case StorageType.String:
                                parameter.Set(strValue);
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Add Value into a FamilyInstance (Parameter Name: " + paramName + ")" + ex.Message, "SynchronizeToRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void AddValueToRevit(Element element, Fields fields, List<string> validFields)
        {
            string paramName = "";
            try
            {
                foreach (string paramId in validFields)
                {
                    bool isDouble = false;
                    string strValue = "";
                    double dblValue = 0;

                    if (paramIdMap.ContainsKey(paramId)) { paramName = paramIdMap[paramId]; }
                    else { continue; }

                    Field field = fields[paramId];
                    if (Convert.IsDBNull(field.Value)) { continue; }

                    if (field.Type == 7)
                    {
                        isDouble = true;
                        dblValue = field.Value;
                        if (decimalPlaces.ContainsKey(field.Name)) { dblValue = Math.Round(dblValue, decimalPlaces[field.Name]); }
                    }
                    else { isDouble = false; strValue = field.Value; }

                    if (strValue == "NULL" || strValue == string.Empty) { continue; }

#if RELEASE2013 ||RELEASE2014
                    Autodesk.Revit.DB.Parameter parameter = element.get_Parameter(paramName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017 || RELEASE2018
                    Autodesk.Revit.DB.Parameter parameter = element.LookupParameter(paramName);
#endif
                    
                    if (null != parameter && !parameter.IsReadOnly)
                    {
                        switch (parameter.StorageType)
                        {
                            case StorageType.Double:
                                if (isDouble) { parameter.Set(dblValue); }
                                else { parameter.SetValueString(strValue); }
                                //double dblVal = 0;
                                //double.TryParse(value, out dblVal);
                                //parameter.Set(dblVal);
                                break;
                            case StorageType.Integer:
                                int intVal = 0;
                                int.TryParse(strValue, out intVal);
                                parameter.Set(intVal);
                                break;
                            case StorageType.ElementId:
                                //ElementId eId = new ElementId(int.Parse(value));
                                //if (null != eId) { parameter.Set(eId); }
                                break;
                            case StorageType.String:
                                parameter.Set(strValue);
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Add Value into a FamilyInstance (Parameter Name: " + paramName + ")" + ex.Message, "SynchronizeToRevit Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void SyncRevitProject()
        {
            foreach (TableDef tableDef in daoDB.TableDefs)
            {
                string tableName = tableDef.Name;
                if (tableName.Contains("Inst_") || tableName.Contains("Type_"))
                {
                    if (tableName.Contains("Parameters")) { continue; }
                    SyncTableToRevit(tableName);
                }
            }
        }

    }
}
