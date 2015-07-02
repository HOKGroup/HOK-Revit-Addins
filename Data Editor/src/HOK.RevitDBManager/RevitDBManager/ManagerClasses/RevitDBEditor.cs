using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RevitDBManager.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Access = Microsoft.Office.Interop.Access;
using Microsoft.Office.Interop.Access.Dao;
using System.Diagnostics;
using System.Windows.Forms;
using RevitDBManager.ManagerForms;

namespace RevitDBManager.Classes
{
    public class RevitDBEditor
    {
        private UIApplication m_app;
        private Autodesk.Revit.DB.Document doc;
        private string dbPath = "";
        private Access.Application oAccess;
        private Database daoDB;
        private object mMissing = System.Reflection.Missing.Value;
        private bool isRoomSelected = false;
        private bool isSpaceSelected = false;
        private bool isAreaSelected = false;

        //family-defined parameters
        private Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> typeParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
        private Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> instParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
        //category-defined parameters
        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> instCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();

        private Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> viewTypeParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> viewInstParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();

        private Dictionary<string, bool> excludeInstSettings = new Dictionary<string, bool>();

        //Revit Data
        private Dictionary<int/*paramID*/, ParamProperties> paramInfoDictionary = new Dictionary<int, ParamProperties>();
        private Dictionary<int/*category or family ID*/, SyncProperties> syncDictionary = new Dictionary<int, SyncProperties>();
        private List<ElementId/*symbolId*/> symbolsToExclude = new List<ElementId>();
        private Dictionary<string, LinkedParameter> linkedParameters = new Dictionary<string, LinkedParameter>();
        private string[] excludeViewFamily = new string[] { "Invalid", "Schedule", "Walkthrough", "ImageView", "CostReport", "LoadsReport", "PressureLossReport", "PanelSchedule", "GraphicalColumnSchedule", "StructuralPlan" };

        #region properties
        public List<ElementId/*symbolID*/> SymbolsToExclude { get { return symbolsToExclude; } set { symbolsToExclude = value; } }
        public Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> TypeParamSettings { get { return typeParamSettings; } set { typeParamSettings = value; } }
        public Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> InstParamSettings { get { return instParamSettings; } set { instParamSettings = value; } }
        public Dictionary<string/*familyName*/, Dictionary<int/*paramId*/, ParamProperties>> ViewTypeParamSettings { get { return viewTypeParamSettigns; } set { viewTypeParamSettigns = value; } }
        public Dictionary<string/*familyName*/, Dictionary<int/*paramId*/, ParamProperties>> ViewInstParamSettings { get { return viewInstParamSettigns; } set { viewInstParamSettigns = value; } }
        public Dictionary<string/*category*/, Dictionary<int, ParamProperties>> TypeCatParamSettings { get { return typeCatParamSettings; } set { typeCatParamSettings = value; } }
        public Dictionary<string/*category*/, Dictionary<int, ParamProperties>> InstCatParamSettings { get { return instCatParamSettings; } set { instCatParamSettings = value; } }
        public Dictionary<string, bool> ExcludeInstSettings { get { return excludeInstSettings; } set { excludeInstSettings = value; } }
        public Dictionary<int/*paramID*/, ParamProperties> ParamInfoDictionary { get { return paramInfoDictionary; } set { paramInfoDictionary = value; } }
        public Dictionary<int/*category or family ID*/, SyncProperties> SyncDictionary { get { return syncDictionary; } set { syncDictionary = value; } }
        public Dictionary<string, LinkedParameter> LinkedParameter { get { return linkedParameters; } set { linkedParameters = value; } }
        public bool IsRoomSelected { get { return isRoomSelected; } set { isRoomSelected = value; } }
        public bool IsSpaceSelected { get { return isSpaceSelected; } set { isSpaceSelected = value; } }
        public bool IsAreaSelected { get { return isAreaSelected; } set { isAreaSelected = value; } }
        #endregion

        public RevitDBEditor(UIApplication uiapp, string dbfile)
        {
            try
            {
                m_app = uiapp;
                doc = m_app.ActiveUIDocument.Document;
                dbPath = dbfile;

                oAccess = new Access.Application();
                oAccess.OpenCurrentDatabase(dbPath,false);
                daoDB = oAccess.CurrentDb();
                oAccess.Visible = false;

                ReadExcludeInst();
                ReadFamilyData(); //Reading FamilyInfo table to collect pre-selected typeID for symboltoExclude
                ReadSyncData(); //Reading Auto-Sync settings
                CheckNewTypes(); //Check if there are new families added or removed for synchronization
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start RevidDBEditor: \n" + ex.Message);
                CloseDatabase();
            }
        }

        private void ReadExcludeInst()
        {
            try
            {
                string tableName = "UI_ExcludeInstance";
                Recordset recordset;
                recordset = daoDB.OpenTable(tableName);

                if (recordset.RecordCount > 0)
                {
                    while (!recordset.EOF)
                    {
                        string categoryName=recordset.Fields["CategoryName"].Value;
                        bool exclude=Convert.ToBoolean(recordset.Fields["ExcludeInstance"].Value);

                        excludeInstSettings.Add(categoryName, exclude);
                        recordset.MoveNext();
                    }
                }
                recordset.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Read Data from UI_ExcludeInstance table: \n" + ex.Message);
                CloseDatabase();
            }
        }

        private void ReadFamilyData()
        {
            try
            {
                string tableName="Rvt_FamilyInfo";
                Recordset recordset;
                recordset = daoDB.OpenTable(tableName);

                if (recordset.RecordCount > 0)
                {
                    while (!recordset.EOF)
                    {
                        int typeId = int.Parse(recordset.Fields["TypeID"].Value);
                        ElementId elementID = new ElementId(typeId);
                        if (null != doc.GetElement(elementID))
                        {
                            symbolsToExclude.Add(elementID);
                        }
                        recordset.MoveNext();
                    }
                }
                recordset.Close();

                string sql = "SELECT * FROM Rvt_Type_Parameters WHERE CategoryName ='Rooms'";
                recordset = daoDB.OpenRecordset(sql, RecordsetTypeEnum.dbOpenDynaset); 
                if (recordset.RecordCount > 0) { isRoomSelected = true; } // Rooms data already existed 
                recordset.Close();

                sql = "SELECT * FROM Rvt_Type_Parameters WHERE CategoryName ='Spaces'";
                recordset = daoDB.OpenRecordset(sql, RecordsetTypeEnum.dbOpenDynaset); 
                if (recordset.RecordCount > 0) { isSpaceSelected = true; } //Spaces data already existed
                recordset.Close();

                sql = "SELECT * FROM Rvt_Type_Parameters WHERE CategoryName ='Areas'";
                recordset = daoDB.OpenRecordset(sql, RecordsetTypeEnum.dbOpenDynaset);
                if (recordset.RecordCount > 0) { isAreaSelected = true; } //area data already existed
                recordset.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Read Data from FamilyInfo table: \n" + ex.Message);
                CloseDatabase();
            }
        }

        public void ReadSyncData()
        {
            try
            {
                Recordset recordset;
                recordset = daoDB.OpenTable("UI_AutoSyncOptions");

                if (recordset.RecordCount > 0)
                {
                    while (!recordset.EOF)
                    {
                        SyncProperties sp = new SyncProperties();
                        string syncType = recordset.Fields["SyncType"].Value;

                        switch (syncType)
                        {
                            case "Category":
                                sp.SyncType = SyncType.Category;
                                sp.CategoryID = int.Parse(recordset.Fields["CategoryID"].Value);
                                sp.CategoryName = recordset.Fields["CategoryName"].Value;
                                syncDictionary.Add(sp.CategoryID, sp);
                                break;
                            case "Family":
                                sp.SyncType = SyncType.Family;
                                sp.FamilyID = int.Parse(recordset.Fields["FamilyID"].Value);
                                sp.FamilyName = recordset.Fields["FamilyName"].Value;
                                syncDictionary.Add(sp.FamilyID, sp);
                                break;
                        }
                        recordset.MoveNext();
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Read Data from AutoSyncOptions table: \n" + ex.Message);
                CloseDatabase();
            }
        }

        private void CheckNewTypes()
        {
            Dictionary<int, SyncProperties> newSyncDictionary = new Dictionary<int, SyncProperties>();
            List<int> deletedSync = new List<int>();

            foreach (int id in syncDictionary.Keys)
            {
                SyncProperties sp = syncDictionary[id];
                switch (sp.SyncType)
                {
                    case SyncType.Category:
                        UpdateCategory(sp.CategoryID, newSyncDictionary, deletedSync);
                        break;
                    case SyncType.Family:
                        UpdateFamily(sp.FamilyID, deletedSync);
                        break;
                }
            }
            //because dictionary collection cannot be modified during the foreach loop
            foreach (int id in newSyncDictionary.Keys)
            {
                SyncProperties sp = newSyncDictionary[id];
                syncDictionary.Add(sp.FamilyID, sp); //when adding new families
            }

            //when exisiting deleted families
            foreach (int id in deletedSync)
            {
                syncDictionary.Remove(id);
            }
        }

        private void UpdateCategory(int id, Dictionary<int, SyncProperties> newSync, List<int> deleted)
        {
            try
            {
                BuiltInCategory enumCategory = (BuiltInCategory)id;
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> elementTypes = collector.OfCategory(enumCategory).OfClass(typeof(ElementType)).ToElements().ToList();

                if (elementTypes.Count > 0)
                {
                    foreach (Element element in elementTypes)
                    {
                        ElementType etype = (ElementType)element;
                        if (!symbolsToExclude.Contains(etype.Id))
                        {
                            symbolsToExclude.Add(etype.Id);

                            FamilySymbol symbol = etype as FamilySymbol;
                            if (null == symbol) { continue; }
                            if (!syncDictionary.ContainsKey(symbol.Family.Id.IntegerValue)&&!newSync.ContainsKey(symbol.Family.Id.IntegerValue))
                            {
                                SyncProperties sp = new SyncProperties();
                                sp.SyncType = SyncType.Family;
                                sp.FamilyID = symbol.Family.Id.IntegerValue;
                                sp.FamilyName = symbol.Family.Name;
                                newSync.Add(sp.FamilyID, sp);
                            }
                        }
                    }
                }
                else if (enumCategory == BuiltInCategory.OST_Views||enumCategory==BuiltInCategory.OST_Sheets)
                {
                    collector = new FilteredElementCollector(doc);
                    List<ViewFamilyType> viewTypeElements = collector.OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().ToList();
                    foreach (ViewFamilyType viewType in viewTypeElements)
                    {
                        string viewFamily = viewType.ViewFamily.ToString();
                        if (excludeViewFamily.Contains(viewFamily)) { continue; }
                        if (!symbolsToExclude.Contains(viewType.Id))
                        {
                            symbolsToExclude.Add(viewType.Id);
                        }
                    }
                }
                else
                {
                    deleted.Add(id);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to upadte categoires for auto-synchronization: \n" + ex.Message);
            }
        }

        private void UpdateFamily(int id, List<int> deleted)
        {
            try
            {
                ElementId famId = new ElementId(id);
                Element element = doc.GetElement(famId);
                Family family = element as Family;

                if (null != family)
                {
#if RELEASE2013||RELEASE2014
                    foreach (FamilySymbol symbol in family.Symbols)
                    {
                        if (!symbolsToExclude.Contains(symbol.Id))
                        {
                            symbolsToExclude.Add(symbol.Id);
                        }
                    }
#elif RELEASE2015 || RELEASE2016
                    foreach (ElementId symbolId in family.GetFamilySymbolIds())
                    {
                        if (!symbolsToExclude.Contains(symbolId))
                        {
                            symbolsToExclude.Add(symbolId);
                        }
                    }
#endif
                }
                else
                {
                    deleted.Add(id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to upadte families for auto-synchronization: \n" + ex.Message);
            }

        }

        private void ReadReferenceData()
        {
            try
            {
                string tableName = "UI_ExternalReference";
                Recordset recordset;
                recordset = daoDB.OpenTable(tableName);

                while (!recordset.EOF)
                {
                    string controlParam = recordset.Fields["ControlParam"].Value;
                    string controlField = recordset.Fields["ControlField"].Value;
                    string categoryName = recordset.Fields["CategoryName"].Value;

                    if (!linkedParameters.ContainsKey(categoryName))
                    {
                        LinkedParameter lp = new LinkedParameter(controlParam, controlField);
                        lp.CategoryName = categoryName;
                        lp.TableName = recordset.Fields["TableName"].Value;
                        lp.DBPath = recordset.Fields["DBPath"].Value;

                        Dictionary<string, string> updateParams = new Dictionary<string, string>();
                        for (int i = 1; i < 6; i++)
                        {
                            var updateParam = recordset.Fields["UpdateParam" + i].Value;
                            var updateField = recordset.Fields["UpdateField" + i].Value;
                            if (!Convert.IsDBNull(updateParam) && !updateParams.ContainsKey(updateParam)) { updateParams.Add(updateParam, updateField); }
                        }
                        lp.UpdateParameterField = updateParams;

                        linkedParameters.Add(lp.CategoryName, lp);
                    }
                    recordset.MoveNext();
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Read Data from UI_ExternalReference table: \n" + ex.Message);
                CloseDatabase();
            }
        }

        #region form_CreateDB

        public void DisplaySelection(ListView listViewSelection, ImageList imgListThumbnail)
        {
            try
            {
                listViewSelection.Items.Clear();

                foreach (ElementId eid in symbolsToExclude)
                {
                    ElementType elementType = doc.GetElement(eid) as ElementType;

                    ListViewItem listItem = new ListViewItem(elementType.Name);
                    listItem.Name = elementType.Id.IntegerValue.ToString();
                    listItem.Tag = elementType;
                    listItem.ImageKey = elementType.Id.IntegerValue.ToString();

                    string familyName = "";
                    int familyId;
                    string categoryName = "";
                    FamilySymbol familySymbol = elementType as FamilySymbol;
                    ViewFamilyType viewFamilyType = elementType as ViewFamilyType;
                    if (null != familySymbol) { familyName = familySymbol.Family.Name; familyId = familySymbol.Family.Id.IntegerValue; categoryName = familySymbol.Category.Name; }
                    else if (null != viewFamilyType) { familyName = viewFamilyType.ViewFamily.ToString(); familyId = -1; categoryName = "Views"; }
                    else { familyName = "System Family"; familyId = -1; categoryName = elementType.Category.Name; }

                    listItem.SubItems.Add(familyName);
                    listItem.SubItems.Add(categoryName);
                    listViewSelection.Items.Add(listItem);
                }

                if (isRoomSelected)
                {
                    ListViewItem item = new ListViewItem("Rooms");
                    item.Name = "Rooms";
                    item.ImageIndex = 0;
                    item.SubItems.Add("");
                    item.SubItems.Add("Rooms");
                    listViewSelection.Items.Add(item);
                }
                if (isSpaceSelected)
                {
                    ListViewItem item = new ListViewItem("Spaces");
                    item.Name = "Spaces";
                    item.ImageIndex = 1;
                    item.SubItems.Add("");
                    item.SubItems.Add("Spaces");
                    listViewSelection.Items.Add(item);
                }
                if (isAreaSelected)
                {
                    ListViewItem item = new ListViewItem("Areas");
                    item.Name = "Areas";
                    item.ImageIndex = 2;
                    item.SubItems.Add("");
                    item.SubItems.Add("Areas");
                    listViewSelection.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display listview for the selection: \n" + ex.Message);
            } 
        }

        //when calling DisplayFamilyGridView() method in the form_CreateDB
        public void CollectParamSettings()
        {
            Dictionary<int, Dictionary<int, ParamProperties>> familyParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
            familyParamSettings = CollectFamilyParamSettings(typeParamSettings, "Rvt_Type_Parameters");
            typeParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>(familyParamSettings);

            familyParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
            familyParamSettings = CollectFamilyParamSettings(instParamSettings, "Rvt_Inst_Parameters");
            instParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>(familyParamSettings);

            Dictionary<string, Dictionary<int, ParamProperties>> viewParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
            viewParamSettings = CollectFamilyParamSettings(viewTypeParamSettigns, "Rvt_Type_Parameters");
            viewTypeParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>(viewParamSettings);

            viewParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
            viewParamSettings = CollectFamilyParamSettings(viewInstParamSettigns, "Rvt_Inst_Parameters");
            viewInstParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>(viewParamSettings);

            Dictionary<string, Dictionary<int, ParamProperties>> categoryParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
            categoryParamSettings = CollectCategoryParamSettings(typeCatParamSettings, "Rvt_Type_Parameters");
            typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>(categoryParamSettings);

            categoryParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
            categoryParamSettings = CollectCategoryParamSettings(instCatParamSettings, "Rvt_Inst_Parameters");
            instCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>(categoryParamSettings);

        }

        private Dictionary<int, Dictionary<int, ParamProperties>> CollectFamilyParamSettings(Dictionary<int, Dictionary<int, ParamProperties>> familyParamSettings, string tableName)
        {
            Dictionary<int, Dictionary<int, ParamProperties>> paramSettings = new Dictionary<int, Dictionary<int, ParamProperties>>(familyParamSettings);
            List<int> familyIds = new List<int>(paramSettings.Keys.ToList());
            try
            {
                Recordset recordset;
                string strSql = "";
                foreach (int familyId in familyIds)
                {
                    Dictionary<int, ParamProperties> paramDictionary = new Dictionary<int, ParamProperties>();
                    paramDictionary = paramSettings[familyId];

                    strSql = "SELECT * FROM [" + tableName + "] WHERE FamilyID ='" + familyId.ToString() + "'";
                    recordset = daoDB.OpenRecordset(strSql, RecordsetTypeEnum.dbOpenDynaset);
                    if (recordset.RecordCount > 0)
                    {
                        while (!recordset.EOF)
                        {
                            ParamProperties pp = new ParamProperties();
                            int paramId = int.Parse(recordset.Fields["ParamID"].Value);
                            if (paramDictionary.ContainsKey(paramId))
                            {
                                pp = paramDictionary[paramId];
                                pp.IsVisible = Convert.ToBoolean(recordset.Fields["IsVisible"].Value);
                                pp.IsLockAll = Convert.ToBoolean(recordset.Fields["IsLockAll"].Value);
                                pp.IsEditable = Convert.ToBoolean(recordset.Fields["IsEditable"].Value);

                                paramDictionary.Remove(paramId);
                                paramDictionary.Add(paramId, pp);
                            }
                            recordset.MoveNext();
                        }
                        paramSettings.Remove(familyId);
                        paramSettings.Add(familyId, paramDictionary);
                    }
                    recordset.Close();
                }

                return paramSettings;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect family parameter settings: \n"+tableName+"\n"+ ex.Message,"RevitDBEditor Error:",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return null;
            }
        }

        private Dictionary<string, Dictionary<int, ParamProperties>> CollectFamilyParamSettings(Dictionary<string, Dictionary<int, ParamProperties>> viewParamSettings, string tableName)
        {
            Dictionary<string, Dictionary<int, ParamProperties>> paramSettings = new Dictionary<string, Dictionary<int, ParamProperties>>(viewParamSettings);
            List<string> familyNames = new List<string>(paramSettings.Keys.ToList());
            try
            {
                Recordset recordset;
                string strSql = "";
                foreach (string familyName in familyNames)
                {
                    Dictionary<int, ParamProperties> paramDictionary = new Dictionary<int, ParamProperties>();
                    paramDictionary = paramSettings[familyName];

                    strSql = "SELECT * FROM [" + tableName + "] WHERE FamilyName ='" + familyName + "'";
                    recordset = daoDB.OpenRecordset(strSql, RecordsetTypeEnum.dbOpenDynaset);
                    if (recordset.RecordCount > 0)
                    {
                        while (!recordset.EOF)
                        {
                            ParamProperties pp = new ParamProperties();
                            int paramId = int.Parse(recordset.Fields["ParamID"].Value);
                            if (paramDictionary.ContainsKey(paramId))
                            {
                                pp = paramDictionary[paramId];
                                pp.IsVisible = Convert.ToBoolean(recordset.Fields["IsVisible"].Value);
                                pp.IsLockAll = Convert.ToBoolean(recordset.Fields["IsLockAll"].Value);
                                pp.IsEditable = Convert.ToBoolean(recordset.Fields["IsEditable"].Value);

                                paramDictionary.Remove(paramId);
                                paramDictionary.Add(paramId, pp);
                            }
                            recordset.MoveNext();
                        }
                        paramSettings.Remove(familyName);
                        paramSettings.Add(familyName, paramDictionary);
                    }
                    recordset.Close();
                }

                return paramSettings;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect view parameter settings: \n" + tableName + "\n" + ex.Message, "RevitDBEditor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private Dictionary<string, Dictionary<int, ParamProperties>> CollectCategoryParamSettings(Dictionary<string, Dictionary<int, ParamProperties>> categoryParamSettings, string tableName)
        {
            Dictionary<string, Dictionary<int, ParamProperties>> paramSettings = new Dictionary<string, Dictionary<int, ParamProperties>>(categoryParamSettings);
            List<string> categoryNames = new List<string>(paramSettings.Keys.ToList());
        
            try
            {

                Recordset recordset;
                string strSql = "";
                foreach (string categoryName in categoryNames)
                {
                    Dictionary<int, ParamProperties> paramDictionary = new Dictionary<int, ParamProperties>();
                    paramDictionary = paramSettings[categoryName];

                    strSql = "SELECT * FROM [" + tableName + "] WHERE CategoryName ='" + categoryName + "'";
                    recordset = daoDB.OpenRecordset(strSql, RecordsetTypeEnum.dbOpenDynaset);
                    if (recordset.RecordCount > 0)
                    {
                        while (!recordset.EOF)
                        {
                            ParamProperties pp = new ParamProperties();
                            int paramId = int.Parse(recordset.Fields["ParamID"].Value);
                            if (paramDictionary.ContainsKey(paramId))
                            {
                                pp = paramDictionary[paramId];
                                pp.IsVisible = Convert.ToBoolean(recordset.Fields["IsVisible"].Value);
                                pp.IsLockAll = Convert.ToBoolean(recordset.Fields["IsLockAll"].Value);
                                pp.IsEditable = Convert.ToBoolean(recordset.Fields["IsEditable"].Value);

                                paramDictionary.Remove(paramId);
                                paramDictionary.Add(paramId, pp);
                            }
                            recordset.MoveNext();
                        }
                        paramSettings.Remove(categoryName);
                        paramSettings.Add(categoryName, paramDictionary);
                    }
                    recordset.Close();
                }

                return paramSettings;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect category parameter settings: \n" + ex.Message);
                return null;
            }
            
        }

        public void DisplayRefData(DataGridView dataGridExt)
        {
            ReadReferenceData();
            dataGridExt.Rows.Clear();
            foreach (string categoryName in linkedParameters.Keys)
            {
                LinkedParameter lp = linkedParameters[categoryName];

                if (null == lp.ControlField || null == lp.ControlParameter || null == lp.TableName || null == lp.DBPath) { continue; }
                else if ("NULL" == lp.ControlField || "NULL" == lp.ControlParameter || "NULL" == lp.TableName || "NULL" == lp.DBPath) { continue; }
                else
                {
                    string strUpdateParams = "";
                    foreach (string updateParam in lp.UpdateParameterField.Keys)
                    {
                        strUpdateParams += updateParam + ", ";
                    }
                    int rowIndex=dataGridExt.Rows.Add(lp.CategoryName, lp.ControlParameter, strUpdateParams, lp.DBPath);
                    dataGridExt.Rows[rowIndex].Tag = lp;
                }
            }
        }

        #endregion


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
