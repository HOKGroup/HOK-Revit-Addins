using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitDBManager.Classes;
using System.IO;
using System.Windows.Media.Imaging;
using RevitDBManager.ManagerForms;
using System.Diagnostics;
using RevitDBManager.GenericForms;

namespace RevitDBManager.Forms
{
    public partial class form_Editor : System.Windows.Forms.Form
    {
        #region variables
        private Autodesk.Revit.UI.UIApplication application;
        private UIDocument uidoc;
        private Document doc;
        private Dictionary<string, Dictionary<int, ElementTypeProperties>> elementDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>();//project elements
        private Dictionary<string/*ViewFamily*/, Dictionary<int/*typeID*/, ViewFamilyType>> viewFamilyTypes = new Dictionary<string, Dictionary<int, ViewFamilyType>>();

        private Dictionary<string,Dictionary<int, TypeProperties>> typeDictionary = new Dictionary<string, Dictionary<int, TypeProperties>>(); //selected family symbol elements
        private Dictionary<string, Dictionary<int, InstanceProperties>> instanceDictionary = new Dictionary<string, Dictionary<int, InstanceProperties>>(); //selected family instance elements
        private Dictionary<string, Dictionary<int, ElementTypeProperties>> sysTypeDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>(); //selected system element types
        private Dictionary<string, Dictionary<int, ElementProperties>> sysInstDictionary = new Dictionary<string, Dictionary<int, ElementProperties>>();//selected system elements
        private Dictionary<int, ViewTypeProperties> viewTypeDictionary = new Dictionary<int, ViewTypeProperties>();//selected view family types
        private Dictionary<int, ViewProperties> viewInstDictionary = new Dictionary<int, ViewProperties>();//selected views

        private Dictionary<int, RoomProperties> roomDictionary = new Dictionary<int, RoomProperties>();
        private Dictionary<int, SpaceProperties> spaceDictionary = new Dictionary<int, SpaceProperties>();
        private Dictionary<int, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();

        private Dictionary<Category, List<FamilySymbol>> categoryDictionary = new Dictionary<Category, List<FamilySymbol>>();
        private Dictionary<string/*category*/, List<Family>/*familyObjects*/> selectedFamilies = new Dictionary<string, List<Family>>();

        private Dictionary<int/*paramID*/, ParamProperties> paramInfoDictionary = new Dictionary<int, ParamProperties>();
        private Dictionary<int/*category or family Id*/, SyncProperties> syncDictionary = new Dictionary<int, SyncProperties>();
        private Dictionary<string/*categoryName*/,Dictionary<int/*paramID*/, ParamProperties>> sharedParamDcitionary = new Dictionary<string,Dictionary<int, ParamProperties>>();
        private Dictionary<int/*index*/, ProjectParameter> proParamDictionary = new Dictionary<int, ProjectParameter>();
        private Dictionary<string/*category*/, Dictionary<string/*paramName*/, ProjectParameter>> catProParamDictionary = new Dictionary<string, Dictionary<string, ProjectParameter>>();
        private Dictionary<string/*category*/, LinkedParameter> linkedParameters = new Dictionary<string, LinkedParameter>();
        private Dictionary<string/*categoryName*/, bool/*excludeInstance*/> excludeInstanceSettings = new Dictionary<string, bool>();

        private ElementDataCollector dataCollector;
        private ThumbnailCreator thumbnailCreator;
        private RevitDBEditor editor;
        private RevitDBCreator dbCreator;
        private ParameterSettings paramSettings;
        private List<ElementId/*symbolID*/> symbolsToExclude = new List<ElementId>();
        private string dbLocation = "";
        private int selectedFamilyID=0;
        private string selectedCategory = "";
        private string selectedFamily = "";
        private bool isCategory = false;
        private bool isView = false;
        private bool isEditMode = false;
        private string extDBPath = "";
        private bool isRoomSelected = false;
        private bool isSpaceSelected = false;
        private bool isAreaSelected = false;
        private bool refreshed = false;
        private bool selectionChanged = true; //to prevent from refreshing parameters setting in parameter tab
        private bool firstEditMode = true;
        private TempStorage tempStorage = new TempStorage(); // to store parameter settings temporary
        private DefaultSettings defaultSettings;

        //family-defined parameters
        private Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> typeParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
        private Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> instParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
        
        //category-defined parameters
        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> instCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();

        //view family-defined parameters
        private Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> viewTypeParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> viewInstParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
        #endregion

        #region properties
        public List<ElementId/*symbolID*/> SymbolsToExclude { get { return symbolsToExclude; } set { symbolsToExclude = value; } }
        public Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> TypeParamSettings { get { return typeParamSettings; } set { typeParamSettings = value; } }
        public Dictionary<int/*familyId*/, Dictionary<int/*paramId*/, ParamProperties>> InstParamSettings { get { return instParamSettings; } set { instParamSettings = value; } }
        public Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> ViewTypeParamSettings { get { return viewTypeParamSettigns; } set { viewTypeParamSettigns = value; } }
        public Dictionary<string/*ViewFamily*/, Dictionary<int/*paramId*/, ParamProperties>> ViewInstParamSettings { get { return viewInstParamSettigns; } set { viewInstParamSettigns = value; } }
        public Dictionary<string/*category*/, Dictionary<int, ParamProperties>> TypeCatParamSettings { get { return typeCatParamSettings; } set { typeCatParamSettings = value; } }
        public Dictionary<string/*category*/, Dictionary<int, ParamProperties>> InstCatParamSettings { get { return instCatParamSettings; } set { instCatParamSettings = value; } }
        public Dictionary<int/*paramID*/, ParamProperties> ParamInfoDictionary { get { return paramInfoDictionary; } set { paramInfoDictionary = value; } }
        #endregion

        public form_Editor(UIApplication app, bool isEdit,string dbFile)
        {
            try
            {
                application = app;
                uidoc = app.ActiveUIDocument;
                doc = uidoc.Document;
                isEditMode = isEdit;
                dbLocation = dbFile;

                InitializeComponent();

                defaultSettings = new DefaultSettings(application);

                //It will retreive Revit Elements and collect them into dictionary
                dataCollector = new ElementDataCollector(doc);
                elementDictionary = dataCollector.GetElementDictionary();
                roomDictionary = dataCollector.GetRoomDictionary();
                spaceDictionary = dataCollector.GetSpaceDictionary();
                areaDictionary = dataCollector.GetAreaDictionary();
                viewFamilyTypes = dataCollector.GetViewFamilyTypes();

                thumbnailCreator = new ThumbnailCreator(doc, imgListThumbnail,elementDictionary);
                imgListThumbnail = thumbnailCreator.GetImageList(); //Image List Key: index, name (symbol.id.integervalue)

                paramSettings = new ParameterSettings(doc, isEditMode);
                paramSettings.DataGridView = dataGridParameter;
                paramSettings.LockImages = imageListLock;
                proParamDictionary = paramSettings.ProjectParameters;
                catProParamDictionary = paramSettings.CatProjectParam;

                if (isEditMode)
                {
                    editor = new RevitDBEditor(application, dbLocation);
                    excludeInstanceSettings = editor.ExcludeInstSettings;
                    symbolsToExclude = editor.SymbolsToExclude;
                    editor.DisplaySelection(listViewSelection, imgListThumbnail);
                    syncDictionary = editor.SyncDictionary;
                    linkedParameters = editor.LinkedParameter;
                    editor.DisplayRefData(dataGridExt);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialization failed.\n"+ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void form_Editor_Load(object sender, EventArgs e)
        {
            LoadForm();
        }

        private void LoadForm()
        {
            try
            {
                progressBar.Visible = false;
                statusLabel.Text = "Ready";

                if (!isEditMode)
                {
                    saveFileDialogDB.Filter = "Access Database Files (*.mdb;*.accdb)|*.mdb;*.accdb";
                    saveFileDialogDB.FileName = Path.GetFileNameWithoutExtension(GetDefaultDbPath()) + ".accdb";
                    saveFileDialogDB.InitialDirectory = Path.GetDirectoryName(GetDefaultDbPath());
                    saveFileDialogDB.RestoreDirectory = true;

                    if (saveFileDialogDB.ShowDialog() == DialogResult.OK)
                    {
                        dbLocation = saveFileDialogDB.FileName;
                    }
                    else
                    {
                        this.Close();
                    }
                }
                txtBoxDbPath.Text = dbLocation;
                listViewCategory.Items.Clear();

                foreach (string category in elementDictionary.Keys)
                {
                    ListViewItem categoryItem = new ListViewItem(category);
                    int firstkey = elementDictionary[category].Keys.First();
                    categoryItem.Name = elementDictionary[category][firstkey].CategoryID.ToString();

                    if (syncDictionary.ContainsKey(int.Parse(categoryItem.Name)))
                    {
                        categoryItem.Text += " (Auto)";
                        categoryItem.ForeColor = System.Drawing.Color.Gray;
                    }
                    listViewCategory.Items.Add(categoryItem);
                }

                var source = new AutoCompleteStringCollection();
                source.AddRange(elementDictionary.Keys.ToArray());
                txtBoxCategory.AutoCompleteCustomSource = source;

                //Room, Space and Area
                if (roomDictionary.Count > 0) { listViewCategory.Items.Add("Rooms"); }
                if (spaceDictionary.Count > 0) { listViewCategory.Items.Add("Spaces"); }
                if (areaDictionary.Count > 0) { listViewCategory.Items.Add("Areas"); }
                if (viewFamilyTypes.Count > 0)
                {
                    ListViewItem categoryItem = new ListViewItem("Views");
                    Category category = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Views);
                    categoryItem.Name = category.Id.ToString();
                    if (syncDictionary.ContainsKey(category.Id.IntegerValue))
                    {
                        categoryItem.Text += " (Auto)";
                        categoryItem.ForeColor = System.Drawing.Color.Gray;
                    }
                    listViewCategory.Items.Add(categoryItem);
                }
                listViewSelection.ListViewItemSorter = new ListViewItemComparer(2); //sort by category names
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the Editor form.\n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                SaveParameterSettings(selectedCategory, selectedFamilyID,selectedFamily, isCategory,isView , radioBttnInst.Checked);
                if (excludeInstanceSettings.ContainsKey(selectedCategory)) { excludeInstanceSettings[selectedCategory] = checkBoxExcludeInst.Checked; }

                int numElements = SetMaxValue();
                statusLabel.Text = "Writing Revit Data in Database...( "+numElements+" elements found)";
                progressBar.Maximum = 5 + numElements;//10 tables + number of types +number of instances
                progressBar.Value = 1;
                progressBar.Visible = true;
                
                dbCreator = new RevitDBCreator(application, dbLocation, isEditMode);
                dbCreator.TypeDictionary = typeDictionary;
                dbCreator.InstanceDictionary = instanceDictionary;
                dbCreator.SysTypeDictionary = sysTypeDictionary;
                dbCreator.SysInstDictionary = sysInstDictionary;
                dbCreator.RoomDictionary = roomDictionary;
                dbCreator.SpaceDictionary = spaceDictionary;
                dbCreator.AreaDictionary = areaDictionary;
                dbCreator.ViewTypeDictionary = viewTypeDictionary;
                dbCreator.ViewInstDictionary = viewInstDictionary;
                dbCreator.ParamInfoDictionary = paramInfoDictionary;
                dbCreator.InstanceParamSettings = instParamSettings;
                dbCreator.TypeParamSettings = typeParamSettings;
                dbCreator.ViewInstanceParamSettings = viewInstParamSettigns;
                dbCreator.ViewTypeParamSettings = viewTypeParamSettigns;
                dbCreator.InstCategoryParamSettings = instCatParamSettings;
                dbCreator.TypeCategoryParamSettings = typeCatParamSettings;
                dbCreator.SyncDictionary = syncDictionary;
                dbCreator.LinkedParameters = linkedParameters;
                dbCreator.ExcludeInstSettings = excludeInstanceSettings;
                dbCreator.ProgressBar = progressBar;

                bool createdDB = false;
                string message = "";
                if (isEditMode)
                {
                    createdDB = dbCreator.UpdateTables();
                    statusLabel.Text = "Updated";
                    message = "updated";
                }
                else
                {
                    createdDB = dbCreator.CreateTables();
                    statusLabel.Text = "Created";
                    message = "created";
                }
                DialogResult dr;
                if (createdDB)
                {
                    dbCreator.CloseDatabase();

                    dr = MessageBox.Show("The database was successfully " + message + ".\n" + dbLocation, "Database File " + message, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (dr == DialogResult.OK) { this.Dispose(); this.Close(); }

                    defaultSettings.DefaultDBFile = dbLocation;
                    string comments="";
                    if (defaultSettings.DBInfoDictionary.ContainsKey(dbLocation)) { comments = defaultSettings.DBInfoDictionary[dbLocation].Comments; }

                    DBFileInfo dbFileInfo = new DBFileInfo();
                    dbFileInfo.isDefault = true;
                    dbFileInfo.DateModified = DateTime.Now.ToString();
                    dbFileInfo.ModifiedBy = Environment.UserName;
                    dbFileInfo.FilePath = dbLocation;
                    dbFileInfo.FileName = Path.GetFileName(dbLocation);
                    dbFileInfo.Comments = comments;

                    defaultSettings.SetDefualtFile(dbFileInfo);
                    defaultSettings.WriteINI();
                }
                else
                {
                    dbCreator.CloseDatabase();
                    dbCreator.DeleteDatabase();

                    dr = MessageBox.Show("The database file cannot be " + message + ".\n Please contact to the develompent team.", "RevitDBCreator Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (dr == DialogResult.OK) { this.Dispose(); this.Close(); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start creating database.\n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dbCreator.CloseDatabase();
            }

        }

        private int SetMaxValue()
        {
            int count = 0;
            foreach (string category in typeDictionary.Keys)
            {
                count += typeDictionary[category].Count;
            }
            foreach (string category in sysTypeDictionary.Keys)
            {
                count += sysTypeDictionary[category].Count;
            }
            foreach (string category in instanceDictionary.Keys)
            {
                count += instanceDictionary[category].Count;
            }
            foreach (string category in sysInstDictionary.Keys)
            {
                count += sysInstDictionary[category].Count;
            }
            foreach (int familyId in instParamSettings.Keys)
            {
                count += instParamSettings[familyId].Count;
            }
            foreach (string familyName in viewInstParamSettigns.Keys)
            {
                count += viewInstParamSettigns[familyName].Count;
            }
            foreach (int familyId in typeParamSettings.Keys)
            {
                count += typeParamSettings[familyId].Count;
            }
            foreach (string familyName in viewTypeParamSettigns.Keys)
            {
                count += viewTypeParamSettigns[familyName].Count;
            }
            foreach (string category in instCatParamSettings.Keys)
            {
                count += instCatParamSettings[category].Count;
            }
            foreach (string category in typeCatParamSettings.Keys)
            {
                count += typeCatParamSettings[category].Count;
            }
            count += roomDictionary.Count;
            count += spaceDictionary.Count;
            count += areaDictionary.Count;
            count += viewTypeDictionary.Count;
            count += viewInstDictionary.Count;

            return count;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = tabControl1.SelectedIndex;

            switch (index)
            {
                case 0:
                    bttnChange.Enabled = true;
                    bttnExisting.Enabled = true;
                    bttnNext.Enabled = true;
                    bttnBack.Enabled = false;
                    bttnCreate.Enabled = false;
                    refreshed = false;
                    if (!selectionChanged) { SaveParameterSettings(selectedCategory, selectedFamilyID, selectedFamily, isCategory, isView, radioBttnInst.Checked); }
                    selectionChanged = true;
                    break;
                case 1:
                    bttnChange.Enabled = false;
                    bttnExisting.Enabled = false;
                    bttnNext.Enabled = true;
                    bttnBack.Enabled = true;
                    bttnCreate.Enabled = false;
                    refreshed = true;
                    if (selectionChanged) { DisplayParameterTab(); selectionChanged = false; }
                    break;
                case 2:
                    bttnChange.Enabled = false;
                    bttnExisting.Enabled = false;
                    if (!selectionChanged) { SaveParameterSettings(selectedCategory, selectedFamilyID, selectedFamily, isCategory, isView, radioBttnInst.Checked); }
                    bttnNext.Enabled = false;
                    bttnBack.Enabled = true;
                    if (refreshed) { bttnCreate.Enabled = true; }
                    break;
            }
        }

        #region Category Tab
        
        private void listViewCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTreeView();
        }

        private void UpdateTreeView()
        {
            treeViewFamily.Nodes.Clear();

            try
            {
                ListViewItem selCategory = new ListViewItem();
                
                foreach (ListViewItem item in listViewCategory.SelectedItems)
                {
                    selCategory = item;
                }

                string categoryName = selCategory.Text;
                if (elementDictionary.ContainsKey(categoryName))
                {
                    foreach (int typeId in elementDictionary[categoryName].Keys)
                    {
                        ElementTypeProperties etp = elementDictionary[categoryName][typeId];
                        //family symbol
                        if (etp.IsFamilySymbol)
                        {
                            string familyName = etp.FamilyName;
                            var sync = from syncProperty in syncDictionary
                                       where syncProperty.Value.FamilyID == etp.FamilyID
                                       select syncProperty.Value;

                            string nodeText = familyName;
                            if (!treeViewFamily.Nodes.ContainsKey(etp.FamilyID.ToString()))
                            {
                                TreeNode familyNode = new TreeNode();
                                familyNode.Name = etp.FamilyID.ToString();
                                if (sync.Count() > 0) { nodeText += " (Auto)"; familyNode.ForeColor = System.Drawing.Color.Gray; } //if the family was set to auto sync
                                else { familyNode.ForeColor = System.Drawing.Color.Black; }
                                familyNode.Text = nodeText;

                                treeViewFamily.Nodes.Add(familyNode);
                            }

                            if (symbolsToExclude.Contains(etp.FamilySymbolObject.Id)) { continue; }
                            TreeNode symbolNode = new TreeNode();
                            symbolNode.Name = etp.TypeID.ToString();
                            symbolNode.Text = etp.ElementTypeName;
                            symbolNode.Tag = etp.FamilySymbolObject;

                            treeViewFamily.Nodes[etp.FamilyID.ToString()].Nodes.Add(symbolNode);
                        }
                        else //system family
                        {
                            string familyName = "";
                            if (etp.CategoryName == "Walls")
                            {
                                WallType wallType = etp.ElementTypeObject as WallType;
                                familyName = "System Family: " + wallType.Kind.ToString();
                            }
                            else
                            {
                                familyName = "System Family: " + etp.CategoryName;
                            }
                            if (!treeViewFamily.Nodes.ContainsKey(familyName))
                            {
                                TreeNode familyNode = new TreeNode();
                                familyNode.Name = familyName;
                                familyNode.Text = familyName;
                                treeViewFamily.Nodes.Add(familyNode);
                            }
                            if (symbolsToExclude.Contains(etp.ElementTypeObject.Id)) { continue; }
                            TreeNode symbolNode = new TreeNode();
                            symbolNode.Name = etp.TypeID.ToString();
                            symbolNode.Text = etp.ElementTypeName;
                            symbolNode.Tag = etp.ElementTypeObject;

                            treeViewFamily.Nodes[familyName].Nodes.Add(symbolNode);
                        }
                    }
                }
                else if (categoryName.Contains("Rooms") || categoryName.Contains("Spaces")||categoryName.Contains("Areas"))
                {
                    TreeNode categoryNode = new TreeNode();
                    categoryNode.Name = selCategory.Text;
                    categoryNode.Text = selCategory.Text;
                    if (listViewSelection.Items.ContainsKey(selCategory.Text)) { categoryNode.ForeColor = System.Drawing.Color.Gray; } //if the category already exist in the selection
                    treeViewFamily.Nodes.Add(categoryNode);
                }
                else if (categoryName.Contains("Views"))
                {
                    foreach (string viewfamily in viewFamilyTypes.Keys)
                    {
                        string familyName = "System Family: " + viewfamily;
                        if (!treeViewFamily.Nodes.ContainsKey(familyName))
                        {
                            TreeNode familyNode = new TreeNode();
                            familyNode.Name = familyName;
                            familyNode.Text = familyName;
                            treeViewFamily.Nodes.Add(familyNode);
                        }

                        foreach (int typeId in viewFamilyTypes[viewfamily].Keys)
                        {
                            if(!listViewSelection.Items.ContainsKey(typeId.ToString()))
                            {
                                ViewFamilyType viewType = viewFamilyTypes[viewfamily][typeId];
                                TreeNode typeNode = new TreeNode();
                                typeNode.Name = viewType.Id.ToString();
                                typeNode.Text = viewType.Name;
                                typeNode.Tag = viewType;

                                treeViewFamily.Nodes[familyName].Nodes.Add(typeNode);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to generate the family TreeView: \n" + ex.Message);
            }
        }

        private string GetDefaultDbPath()
        {
            string dbPath="";
            string masterFilePath = "";

            if (doc.IsWorkshared&& null!=doc.GetWorksharingCentralModelPath())
            {
                masterFilePath = doc.GetWorksharingCentralModelPath().ToString();
            }
            else
            {
                masterFilePath = doc.PathName;
            }

            dbPath = Path.GetDirectoryName(masterFilePath) + @"\" + Path.GetFileNameWithoutExtension(masterFilePath) + ".accdb";
           
            return dbPath;
        }

        private bool AddItemInSelection(ElementType elementType)
        {
            bool result = false;
            try
            {
                if (!listViewSelection.Items.ContainsKey(elementType.Id.IntegerValue.ToString()))
                {
                    ListViewItem item = new ListViewItem(elementType.Name);
                    item.Name = elementType.Id.IntegerValue.ToString();
                    item.Tag = elementType;
                    item.ImageKey = elementType.Id.IntegerValue.ToString();

                    string familyName = "";
                    int familyId;
                    string categoryName = "";
                    FamilySymbol familySymbol = elementType as FamilySymbol;
                    ViewFamilyType viewFamilyType = elementType as ViewFamilyType;
                    if (null != familySymbol) { familyName = familySymbol.Family.Name; familyId = familySymbol.Family.Id.IntegerValue; categoryName = familySymbol.Category.Name; }
                    else if (null != viewFamilyType) { familyName = viewFamilyType.ViewFamily.ToString(); familyId = -1; categoryName = "Views"; }
                    else { familyName = "System Family"; familyId = -1; categoryName = elementType.Category.Name; }

                    item.SubItems.Add(familyName);
                    item.SubItems.Add(categoryName);
                    listViewSelection.Items.Add(item);
                    symbolsToExclude.Add(elementType.Id);
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add item in the selection: "+elementType.Name+"\n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private bool AddItemInSelection(string categoryName)
        {
            bool result = false;
            try
            {
                if (!listViewSelection.Items.ContainsKey(categoryName))
                {
                    ListViewItem item = new ListViewItem(categoryName);
                    item.Name = categoryName;
                    if (categoryName == "Rooms") { item.ImageIndex = 0; }
                    if (categoryName == "Spaces") { item.ImageIndex = 1; }
                    if (categoryName == "Areas") { item.ImageIndex = 2; }
                    item.SubItems.Add("");
                    item.SubItems.Add(categoryName);
                    listViewSelection.Items.Add(item);
                }
                
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add item in the selection: "+categoryName+"\n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        #endregion

        #region Category Tab Component Events

        private void bttnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode familyNode in treeViewFamily.Nodes)
            {
                familyNode.Checked = true;
                foreach (TreeNode typeNode in familyNode.Nodes)
                {
                    typeNode.Checked = true;
                }
            }
        }

        private void bttnCheckNone_Click(object sender, EventArgs e)
        {
            foreach (TreeNode familyNode in treeViewFamily.Nodes)
            {
                familyNode.Checked = false;
                foreach (TreeNode typeNode in familyNode.Nodes)
                {
                    typeNode.Checked = false;
                }
            }
        }

        private void treeViewFamily_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    this.CheckAllChildNodes(e.Node, e.Node.Checked);
                }
            }
        }

        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.Checked = nodeChecked;
                if (node.Nodes.Count > 0)
                {
                    // If the current node has child nodes, call the CheckAllChildsNodes method recursively.
                    this.CheckAllChildNodes(node, nodeChecked);
                }
            }
        }
        
        private void bttnChange_Click(object sender, EventArgs e)
        {
            saveFileDialogDB.Filter = "Access Database Files (*.mdb;*.accdb)|*.mdb;*.accdb";
            saveFileDialogDB.FileName = txtBoxDbPath.Text;
            saveFileDialogDB.InitialDirectory = Path.GetDirectoryName(txtBoxDbPath.Text);

            if (saveFileDialogDB.ShowDialog() == DialogResult.OK)
            {
                if (defaultSettings.DBInfoDictionary.ContainsKey(saveFileDialogDB.FileName))
                {
                    MessageBox.Show("The entered file already exists. Please create with a different name.", "Duplicate Files", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (saveFileDialogDB.FileName != dbLocation)
                {
                    isEditMode = false;
                    dbLocation = saveFileDialogDB.FileName;
                    txtBoxDbPath.Text = dbLocation;
                    this.Text = "Create New Revit Data Collection";
                    listViewSelection.Items.Clear();
                    dataGridExt.Rows.Clear();

                    excludeInstanceSettings = new Dictionary<string, bool>();
                    symbolsToExclude = new List<ElementId>();
                    syncDictionary = new Dictionary<int, SyncProperties>();
                    linkedParameters = new Dictionary<string, LinkedParameter>();
                }
            }
        }

        private void bttnAdd_Click(object sender, EventArgs e)
        {
            bool result = false;
            foreach (TreeNode familyNode in treeViewFamily.Nodes)
            {
                if (familyNode.Nodes.Count > 0)
                {
                    foreach (TreeNode typeNode in familyNode.Nodes)
                    {
                        if (null != typeNode.Tag && typeNode.Checked)
                        {
                            ElementType elementType = typeNode.Tag as ElementType;
                            if (null != elementType) { result = AddItemInSelection(elementType); }
                        }
                    }
                }
                else if(familyNode.Name.Contains("Rooms")||familyNode.Name.Contains("Spaces")||familyNode.Name.Contains("Areas"))
                {
                    if (familyNode.Checked)
                    {
                        result=AddItemInSelection(familyNode.Name);
                    }
                }
            }
            if (result) { UpdateTreeView(); }
        }

        private void bttnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                bool result = false;
                StringBuilder strBuilder = new StringBuilder();
                Category viewCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Views);

                foreach (ListViewItem listItem in listViewSelection.SelectedItems)
                {
                    if (null != listItem.Tag)
                    {
                        ElementType elementType = (ElementType)listItem.Tag;
                        ViewFamilyType viewType=listItem.Tag as ViewFamilyType;
                       
                        if (null!=elementType.Category)
                        {
                            if (syncDictionary.ContainsKey(elementType.Category.Id.IntegerValue))
                            {
                                strBuilder.AppendLine(elementType.Category.Name + ": " + elementType.Name);
                                continue;
                            }
                        }
                        else if (null != viewType)
                        {
                            if (syncDictionary.ContainsKey(viewCategory.Id.IntegerValue))
                            {
                                strBuilder.AppendLine("Views: " + viewType.Name);
                                continue;
                            }
                        }

                        FamilySymbol symbol = elementType as FamilySymbol;
                        if (null != symbol)
                        {
                            if (syncDictionary.ContainsKey(symbol.Family.Id.IntegerValue))
                            {
                                strBuilder.AppendLine(symbol.Family.Name+": "+symbol.Name);
                                continue;
                            }
                        }
                        symbolsToExclude.Remove(elementType.Id);
                    }
                    listViewSelection.Items.Remove(listItem);
                }
                if (strBuilder.Length > 0)
                {
                    MessageBox.Show("The following types are under Auto Synchronization option.\n Please cancel Auto-Sync option.\n"+strBuilder.ToString(), "Auto-Synchronization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                result = true;
                if (result) { UpdateTreeView(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove items from the selection: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnIconView_Click(object sender, EventArgs e)
        {
            listViewSelection.View = System.Windows.Forms.View.List;
        }

        private void bttnDetailView_Click(object sender, EventArgs e)
        {
            listViewSelection.View = System.Windows.Forms.View.Details;
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnNext_Click(object sender, EventArgs e)
        {
            int index = tabControl1.SelectedIndex;
            if (index+1 < tabControl1.TabCount)
            {
                tabControl1.SelectedIndex = index+1;
                bttnBack.Enabled = true;
                if (index+1 == tabControl1.TabCount - 1)
                {
                    bttnNext.Enabled = false;
                    bttnCreate.Enabled = true;
                }
                else
                {
                    bttnNext.Enabled = true;
                    bttnCreate.Enabled = false;
                }
            }
            
        }

        private void bttnBack_Click(object sender, EventArgs e)
        {
            int index = tabControl1.SelectedIndex;
            if (index - 1 >=0)
            {
                tabControl1.SelectedIndex = index - 1;
                bttnNext.Enabled = true;
                if (index -1 == 0)
                {
                    bttnBack.Enabled = false;
                    bttnCreate.Enabled = false;
                }
            }
        }
        
        #endregion

        #region Parameter Tab
        private void DisplayParameterTab()
        {
            try
            {
                List<ElementType> selectedTypes = new List<ElementType>();

                foreach (ListViewItem listItem in listViewSelection.Items)
                {
                    if (null != listItem.Tag)
                    {
                        ElementType elementType = (ElementType)listItem.Tag;
                        selectedTypes.Add(elementType);
                    }
                }

                if (listViewSelection.Items.ContainsKey("Rooms")) { isRoomSelected = true; } else { isRoomSelected = false; }
                if (listViewSelection.Items.ContainsKey("Spaces")) { isSpaceSelected = true; } else { isSpaceSelected = false; }
                if (listViewSelection.Items.ContainsKey("Areas")) { isAreaSelected = true; } else { isAreaSelected = false; }

                statusLabel.Text = "Collecting data from selected types...";
                progressBar.Maximum = selectedTypes.Count;
                progressBar.Visible = true;
                
                dataCollector.ProgressBar = progressBar;
                dataCollector.CollectSelectedElementsData(selectedTypes);

                typeDictionary = new Dictionary<string, Dictionary<int, TypeProperties>>();
                instanceDictionary = new Dictionary<string, Dictionary<int, InstanceProperties>>();
                sysTypeDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>();
                sysInstDictionary = new Dictionary<string, Dictionary<int, ElementProperties>>();
                roomDictionary = new Dictionary<int, RoomProperties>();
                spaceDictionary = new Dictionary<int, SpaceProperties>();
                areaDictionary = new Dictionary<int, AreaProperties>();
                viewTypeDictionary = new Dictionary<int, ViewTypeProperties>();
                viewInstDictionary = new Dictionary<int, ViewProperties>();
                selectedFamilies = new Dictionary<string, List<Family>>();
                paramInfoDictionary = new Dictionary<int, ParamProperties>();

                typeDictionary = dataCollector.GetTypeDictionary();
                instanceDictionary = dataCollector.GetInstanceDictionary();
                sysTypeDictionary = dataCollector.GetSysTypeDictionary();
                sysInstDictionary = dataCollector.GetSysInstDictionary();
                roomDictionary = dataCollector.GetRoomDictionary();
                spaceDictionary = dataCollector.GetSpaceDictionary();
                areaDictionary = dataCollector.GetAreaDictionary();
                viewTypeDictionary = dataCollector.GetViewTypeDictionary();
                viewInstDictionary = dataCollector.GetViewInstDictionary();
                selectedFamilies = dataCollector.GetSelectedFamilies();
                paramInfoDictionary = dataCollector.GetParameterInfo();

                if (isSpaceSelected)
                {
                    selectedFamilies.Add("Spaces", new List<Family>());
                }
                if (isAreaSelected)
                {
                    selectedFamilies.Add("Areas", new List<Family>());
                }
                if (isRoomSelected)
                {
                    selectedFamilies.Add("Rooms", new List<Family>());
                }

                DisplayFamilyGridView(selectedFamilies);
                selectedCategory = dataGridFamily.Rows[0].Cells[0].Value.ToString();
                selectedFamilyID = 0;
                selectedFamily = "";
                isCategory = true;
                isView = false;
                dataGridParameter.Rows.Clear();
                DisplayParamGridView(selectedCategory, selectedFamilyID,selectedFamily, isCategory,isView);
                if (excludeInstanceSettings.ContainsKey(selectedCategory)) { checkBoxExcludeInst.Checked = excludeInstanceSettings[selectedCategory]; }
                if (selectedCategory.Contains("Areas") || selectedCategory.Contains("Rooms") || selectedCategory.Contains("Spaces"))
                {
                    checkBoxExcludeInst.Checked = false; checkBoxExcludeInst.Enabled = false;
                }
                else { checkBoxExcludeInst.Enabled = true; }
                progressBar.Visible = false; statusLabel.Text = "";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the parameter tab: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayFamilyGridView(Dictionary<string, List<Family>> selFamilies)
        {
            try
            {
                typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
                instCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
                viewTypeParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
                viewInstParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
                sharedParamDcitionary = new Dictionary<string, Dictionary<int, ParamProperties>>();

                CollectTypeParamSettings();
                CollectSysTypeParamSettings();
                CollectInstParamSettings();
                CollectSysInstParamSettings();
                CollectViewTypeParamSettings();
                CollectViewInstParamSettings();

                if (isRoomSelected) { CollectRoomParamSettings(); }
                if (isSpaceSelected) { CollectSpaceParamSettings(); }
                if (isAreaSelected) { CollectAreaParamSettings(); }

                DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
                headerStyle.BackColor = System.Drawing.Color.LightGray;
                headerStyle.ForeColor = System.Drawing.Color.White;

                dataGridFamily.Rows.Clear();

                int rowNum = 0;
                var categoryNames = selFamilies.Keys.ToList();
                categoryNames.Sort();

                var oldCategories = excludeInstanceSettings.Keys.ToList();
                foreach (string oldCategory in oldCategories)
                {
                    if (!categoryNames.Contains(oldCategory))
                    {
                        excludeInstanceSettings.Remove(oldCategory);
                    }
                }

                foreach (string category in categoryNames)
                {
                    dataGridFamily.Rows.Add();
                    dataGridFamily.Rows[rowNum].Cells[0].Value = category;
                    dataGridFamily.Rows[rowNum].Cells[1].Value = category;
                    dataGridFamily.Rows[rowNum].Cells[0].Tag = "category";
                    dataGridFamily.Rows[rowNum].Cells[0].Style = headerStyle;
                    rowNum++;

                    if (!excludeInstanceSettings.ContainsKey(category)) { excludeInstanceSettings.Add(category, false); }

                    var sortedFamilies = from families in selFamilies[category]
                                         orderby families.Name ascending
                                         select families;

                    foreach (Family afamily in sortedFamilies)
                    {
                        int familyId = afamily.Id.IntegerValue;
                        bool existType = false; bool existInst = false;
                        if (typeParamSettings.ContainsKey(familyId)) { if (typeParamSettings[familyId].Count > 0 ) { existType = true; } }
                        if (instParamSettings.ContainsKey(familyId)) { if (instParamSettings[familyId].Count > 0 ) { existInst = true; } }
                        
                        if (existType || existInst)
                        {
                            dataGridFamily.Rows.Add();
                            dataGridFamily.Rows[rowNum].Cells[0].Value = afamily.Name;
                            dataGridFamily.Rows[rowNum].Cells[1].Value = category;
                            dataGridFamily.Rows[rowNum].Cells[0].Tag = familyId;
                            rowNum++;  
                        }                  
                    }
                }
                if (viewTypeParamSettigns.Count>0)
                {
                    dataGridFamily.Rows.Add();
                    dataGridFamily.Rows[rowNum].Cells[0].Value = "Views";
                    dataGridFamily.Rows[rowNum].Cells[1].Value = "Views";
                    dataGridFamily.Rows[rowNum].Cells[0].Tag = "category";
                    dataGridFamily.Rows[rowNum].Cells[0].Style = headerStyle;
                    rowNum++;

                    if (!excludeInstanceSettings.ContainsKey("Views")) { excludeInstanceSettings.Add("Views", false); }

                    foreach (string viewFamily in viewTypeParamSettigns.Keys)
                    {
                        dataGridFamily.Rows.Add();
                        dataGridFamily.Rows[rowNum].Cells[0].Value = viewFamily;
                        dataGridFamily.Rows[rowNum].Cells[1].Value = "Views";
                        dataGridFamily.Rows[rowNum].Cells[0].Tag = viewFamily;
                        rowNum++;
                    }
                }

                if (isEditMode && firstEditMode)
                {
                    editor.TypeParamSettings = typeParamSettings;
                    editor.InstParamSettings = instParamSettings;
                    editor.ViewTypeParamSettings = viewTypeParamSettigns;
                    editor.ViewInstParamSettings = viewInstParamSettigns;
                    editor.TypeCatParamSettings = typeCatParamSettings;
                    editor.InstCatParamSettings = instCatParamSettings;

                    editor.CollectParamSettings(); //to display pre-set settings of all parameters
                    typeParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
                    typeParamSettings = editor.TypeParamSettings;
                    instParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
                    instParamSettings = editor.InstParamSettings;
                    viewTypeParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
                    viewTypeParamSettigns = editor.ViewTypeParamSettings;
                    viewInstParamSettigns = new Dictionary<string, Dictionary<int, ParamProperties>>();
                    viewInstParamSettigns = editor.ViewInstParamSettings;
                    typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
                    typeCatParamSettings = editor.TypeCatParamSettings;
                    instCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
                    instCatParamSettings = editor.InstCatParamSettings;

                    firstEditMode = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the family lists: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectTypeParamSettings()
        {
            try
            {
                typeParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
                typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();

                // category >> family >> type structure of dictionary will be utilized for category and family naming list.
                Dictionary<string/*category*/, Dictionary<int/*family*/, Dictionary<int, TypeProperties>>> familyDictionary
                    = new Dictionary<string, Dictionary<int, Dictionary<int, TypeProperties>>>();

                foreach (string category in typeDictionary.Keys)
                {
                    if (!sharedParamDcitionary.ContainsKey(category)) { sharedParamDcitionary.Add(category, new Dictionary<int, ParamProperties>()); }
                    familyDictionary.Add(category, new Dictionary<int, Dictionary<int, TypeProperties>>());

                    foreach (int typeId in typeDictionary[category].Keys)
                    {
                        TypeProperties tp = typeDictionary[category][typeId];
                        int familyId = tp.FamilyID;

                        if (familyDictionary[category].ContainsKey(familyId))
                        {
                            familyDictionary[category][familyId].Add(typeId, tp);
                        }
                        else
                        {
                            familyDictionary[category].Add(familyId, new Dictionary<int, TypeProperties>());
                            familyDictionary[category][familyId].Add(typeId, tp);
                        }
                    }
                }

                foreach (string category in familyDictionary.Keys)
                {
                    Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                    if(catProParamDictionary.ContainsKey(category)) { projectParams = catProParamDictionary[category]; }

                    typeCatParamSettings.Add(category, new Dictionary<int, ParamProperties>());

                    foreach (int familyId in familyDictionary[category].Keys)
                    {
                        int firstKey = familyDictionary[category][familyId].Keys.First();
                        TypeProperties tp = familyDictionary[category][familyId][firstKey];
                        Dictionary<int, ParamProperties> typeParameters = new Dictionary<int, ParamProperties>();
                        typeParameters = tp.TypeParameters;

                        if (tempStorage.TypeParamSettings.ContainsKey(familyId))
                        {
                            typeParamSettings.Add(familyId, tempStorage.TypeParamSettings[familyId]);
                            continue;
                        }

                        typeParamSettings.Add(familyId, new Dictionary<int, ParamProperties>());

                        foreach (int pId in typeParameters.Keys)
                        {
                            ParamProperties pp = typeParameters[pId];
                            pp.IsInstance = false;
                            pp.IsVisible = false; //default visibility setting as false
                            pp.IsEditable = true;
                            pp.IsProject = false;
                            pp.CategoryName = category;

                            //default lock type is "IsEditable"
                            if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true; }

                            //BuiltInParameters will be binded in category
                            if (pp.ParamID < 0 || projectParams.ContainsKey(pp.ParamName))
                            {
                                //project Parameters will be visible 
                                if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true;  }

                                if (!typeCatParamSettings[category].ContainsKey(pp.ParamID))
                                {
                                    typeCatParamSettings[category].Add(pp.ParamID, pp);
                                }
                            }
                            else
                            {
                                //shared parameters should be collect to have equivalent values between different families.
                                if (pp.IsShared && !sharedParamDcitionary[category].ContainsKey(pp.ParamID))
                                {
                                    sharedParamDcitionary[category].Add(pp.ParamID, pp);
                                }
                                typeParamSettings[familyId].Add(pp.ParamID, pp);
                            }
                        }
                    }

                    if (tempStorage.TypeCatParamSettings.ContainsKey(category))
                    {
                        typeCatParamSettings.Remove(category);
                        typeCatParamSettings.Add(category, tempStorage.TypeCatParamSettings[category]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from type parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }

        private void CollectSysTypeParamSettings()
        {
            try
            {
                foreach (string category in sysTypeDictionary.Keys)
                {
                    if (!sharedParamDcitionary.ContainsKey(category)) { sharedParamDcitionary.Add(category, new Dictionary<int, ParamProperties>()); }
                    Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                    if (catProParamDictionary.ContainsKey(category)) { projectParams = catProParamDictionary[category]; }

                    if (tempStorage.TypeCatParamSettings.ContainsKey(category))
                    {
                        if (typeCatParamSettings.ContainsKey(category)) { typeCatParamSettings.Remove(category); }
                        typeCatParamSettings.Add(category, tempStorage.TypeCatParamSettings[category]);
                        continue;
                    }

                    if (!typeCatParamSettings.ContainsKey(category))
                    {
                        typeCatParamSettings.Add(category, new Dictionary<int, ParamProperties>());
                    }

                    Dictionary<int, ParamProperties> typeParam = new Dictionary<int, ParamProperties>();
                    foreach (int typeId in sysTypeDictionary[category].Keys)
                    {
                        ElementTypeProperties etp = sysTypeDictionary[category][typeId];
                        foreach (int pId in etp.ElementTypeParameters.Keys)
                        {
                            if (!typeParam.ContainsKey(pId))
                            {
                                typeParam.Add(pId, etp.ElementTypeParameters[pId]);
                            }
                        }
                    }

                    foreach (int paramId in typeParam.Keys)
                    {
                        ParamProperties pp = typeParam[paramId];
                        pp.IsVisible = false; //default visibility setting as false
                        pp.IsEditable = true;
                        pp.IsInstance = false;
                        pp.IsProject = false;
                        pp.CategoryName = category;

                        //default lock type is "IsEditable"
                        if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true; }
                        if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true;  }

                        if (!typeCatParamSettings[category].ContainsKey(pp.ParamID))
                        {
                            typeCatParamSettings[category].Add(pp.ParamID, pp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from system type parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectInstParamSettings()
        {
            try
            {
                instParamSettings = new Dictionary<int, Dictionary<int, ParamProperties>>();
                instCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();

                // category >> family >> type structure of dictionary will be utilized for category and family naming list.
                Dictionary<string/*category*/, Dictionary<int/*family*/, Dictionary<int, InstanceProperties>>> familyDictionary
                    = new Dictionary<string, Dictionary<int, Dictionary<int, InstanceProperties>>>();

                foreach (string category in instanceDictionary.Keys)
                {
                    familyDictionary.Add(category, new Dictionary<int, Dictionary<int, InstanceProperties>>());

                    foreach (int instanceId in instanceDictionary[category].Keys)
                    {
                        InstanceProperties ip = instanceDictionary[category][instanceId];
                        int familyId = ip.FamilyID;

                        if (familyDictionary[category].ContainsKey(familyId)) { continue; }

                        familyDictionary[category].Add(familyId, new Dictionary<int, InstanceProperties>());
                        familyDictionary[category][familyId].Add(instanceId, ip);
                    }
                }

                foreach (string category in familyDictionary.Keys)
                {
                    Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                    if (catProParamDictionary.ContainsKey(category)) { projectParams = catProParamDictionary[category]; }

                    instCatParamSettings.Add(category, new Dictionary<int, ParamProperties>());

                    foreach (int familyId in familyDictionary[category].Keys)
                    {
                        int firstKey = familyDictionary[category][familyId].Keys.First();
                        InstanceProperties ip = familyDictionary[category][familyId][firstKey];
                        Dictionary<int, ParamProperties> instParameters = new Dictionary<int, ParamProperties>();
                        instParameters = ip.InstParameters;

                        if (tempStorage.InstParamSettings.ContainsKey(familyId))
                        {
                            instParamSettings.Add(familyId, tempStorage.InstParamSettings[familyId]);
                            continue;
                        }

                        instParamSettings.Add(familyId, new Dictionary<int, ParamProperties>());

                        foreach (int pId in instParameters.Keys)
                        {
                            ParamProperties pp = instParameters[pId];
                            pp.IsInstance = true;
                            pp.IsVisible = false; //default visibility setting as false
                            pp.IsEditable = true;
                            pp.IsProject = false;
                            pp.CategoryName = category;

                            //default lock type is "IsEditable"
                            if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true; }

                            //BuiltInParameters will be binded in category
                            if (pp.ParamID < 0 || projectParams.ContainsKey(pp.ParamName))
                            {
                                //project Parameters will be visible 
                                if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true; }

                                if (!instCatParamSettings[category].ContainsKey(pp.ParamID))
                                {
                                    instCatParamSettings[category].Add(pp.ParamID, pp);
                                }
                            }
                            else
                            {
                                //shared parameters should be collect to have equivalent values between different families.
                                if (pp.IsShared && !sharedParamDcitionary[category].ContainsKey(pp.ParamID))
                                {
                                    sharedParamDcitionary[category].Add(pp.ParamID, pp);
                                }
                                instParamSettings[familyId].Add(pp.ParamID, pp);
                            }
                        }
                    }

                    if (tempStorage.InstCatParamSettings.ContainsKey(category))
                    {
                        instCatParamSettings.Remove(category);
                        instCatParamSettings.Add(category, tempStorage.InstCatParamSettings[category]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from insatance parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }

        private void CollectSysInstParamSettings()
        {
            try
            {
                foreach (string category in sysInstDictionary.Keys)
                {
                    Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                    if (catProParamDictionary.ContainsKey(category)) { projectParams = catProParamDictionary[category]; }

                    if (tempStorage.InstCatParamSettings.ContainsKey(category))
                    {
                        if (instCatParamSettings.ContainsKey(category)) { instCatParamSettings.Remove(category); }
                        InstCatParamSettings.Add(category, tempStorage.InstCatParamSettings[category]);
                        continue;
                    }

                    if (!instCatParamSettings.ContainsKey(category))
                    {
                        instCatParamSettings.Add(category, new Dictionary<int, ParamProperties>());
                    }

                    Dictionary<int, ParamProperties> instParam = new Dictionary<int, ParamProperties>();
                    int typeId = 0;
                    foreach (int elemId in sysInstDictionary[category].Keys)
                    {
                        ElementProperties ep = sysInstDictionary[category][elemId];
                        if (typeId.CompareTo(ep.TypeID) == 0) { continue; }
                     
                        foreach (int pId in ep.ElementParameters.Keys)
                        {
                            if (!instParam.ContainsKey(pId))
                            {
                                instParam.Add(pId, ep.ElementParameters[pId]);
                            }
                        }
                        typeId = ep.TypeID;
                    }

                    foreach (int paramId in instParam.Keys)
                    {
                        ParamProperties pp = instParam[paramId];
                        pp.IsVisible = false; //default visibility setting as false
                        pp.IsEditable = true;
                        pp.IsInstance = true;
                        pp.IsProject = false;
                        pp.CategoryName = category;

                        //default lock type is "IsEditable"
                        if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true; }
                        if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true;  }

                        if (!instCatParamSettings[category].ContainsKey(pp.ParamID))
                        {
                            instCatParamSettings[category].Add(pp.ParamID, pp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from system instance parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectRoomParamSettings()
        {
            try
            {
                if (roomDictionary.Count > 0 )
                {
                    if (!sharedParamDcitionary.ContainsKey("Rooms")) { sharedParamDcitionary.Add("Rooms", new Dictionary<int, ParamProperties>()); }

                    if (tempStorage.TypeCatParamSettings.ContainsKey("Rooms"))
                    {
                        typeCatParamSettings.Add("Rooms", tempStorage.TypeCatParamSettings["Rooms"]);
                        instCatParamSettings.Add("Rooms", new Dictionary<int, ParamProperties>());
                    }
                    else
                    {
                        RoomProperties rp = roomDictionary[roomDictionary.Keys.First()];
                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = rp.RoomParameters;
                        Dictionary<int, ParamProperties> instParam = new Dictionary<int, ParamProperties>();
                        Dictionary<int, ParamProperties> typeParam = new Dictionary<int, ParamProperties>();
                        Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                        if (catProParamDictionary.ContainsKey("Rooms")) { projectParams = catProParamDictionary["Rooms"]; }

                        foreach (int pId in ppDic.Keys)
                        {
                            ParamProperties pp = ppDic[pId];
                            pp.IsVisible = false; //default visibility setting as false
                            pp.IsEditable = true;
                            pp.IsProject = false;
                            pp.CategoryName = "Rooms";

                            //default lock type is "IsEditable"
                            if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true;  }
                            if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true;  }

                            if (pp.IsInstance)
                            {
                                pp.IsInstance = true;
                                instParam.Add(pp.ParamID, pp);
                            }
                            else
                            {
                                pp.IsInstance = false;
                                typeParam.Add(pp.ParamID, pp);
                            }
                        }
                        if (instCatParamSettings.ContainsKey("Rooms")) { instCatParamSettings.Remove("Rooms"); }
                        if (typeCatParamSettings.ContainsKey("Rooms")) { typeCatParamSettings.Remove("Rooms"); }

                        instCatParamSettings.Add("Rooms", instParam);
                        typeCatParamSettings.Add("Rooms", typeParam);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from room parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void CollectSpaceParamSettings()
        {
            try
            {
                if (spaceDictionary.Count > 0)
                {
                    if (!sharedParamDcitionary.ContainsKey("Spaces")) { sharedParamDcitionary.Add("Spaces", new Dictionary<int, ParamProperties>()); }

                    if (tempStorage.TypeCatParamSettings.ContainsKey("Spaces"))
                    {
                        typeCatParamSettings.Add("Spaces", tempStorage.TypeCatParamSettings["Spaces"]);
                        instCatParamSettings.Add("Spaces", new Dictionary<int, ParamProperties>());
                    }
                    else
                    {
                        SpaceProperties sp = spaceDictionary[spaceDictionary.Keys.First()];
                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = sp.SpaceParameters;
                        Dictionary<int, ParamProperties> instParam = new Dictionary<int, ParamProperties>();
                        Dictionary<int, ParamProperties> typeParam = new Dictionary<int, ParamProperties>();
                        Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                        if (catProParamDictionary.ContainsKey("Spaces")) { projectParams = catProParamDictionary["Spaces"]; }

                        foreach (int pId in ppDic.Keys)
                        {
                            ParamProperties pp = ppDic[pId];
                            pp.IsVisible = false; //default visibility setting as false
                            pp.IsEditable = true;
                            pp.IsProject = false;
                            pp.CategoryName = "Spaces";

                            //default lock type is "IsEditable"
                            if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true; }
                            if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true;  }

                            if (pp.IsInstance)
                            {
                                pp.IsInstance = true;
                                instParam.Add(pp.ParamID, pp);
                            }
                            else
                            {
                                pp.IsInstance = false;
                                typeParam.Add(pp.ParamID, pp);
                            }
                        }
                        if (instCatParamSettings.ContainsKey("Spaces")) { instCatParamSettings.Remove("Spaces"); }
                        if (typeCatParamSettings.ContainsKey("Spaces")) { typeCatParamSettings.Remove("Spaces"); }

                        instCatParamSettings.Add("Spaces", instParam);
                        typeCatParamSettings.Add("Spaces", typeParam);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from space parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectAreaParamSettings()
        {
            try
            {
                if (areaDictionary.Count > 0 )
                {
                    if (!sharedParamDcitionary.ContainsKey("Areas")) { sharedParamDcitionary.Add("Areas", new Dictionary<int, ParamProperties>()); }

                    if (tempStorage.TypeCatParamSettings.ContainsKey("Areas"))
                    {
                        typeCatParamSettings.Add("Areas", tempStorage.TypeCatParamSettings["Areas"]);
                        instCatParamSettings.Add("Areas", new Dictionary<int, ParamProperties>());
                    }
                    else
                    {
                        AreaProperties ap = areaDictionary[areaDictionary.Keys.First()];
                        Dictionary<int, ParamProperties> ppDic = new Dictionary<int, ParamProperties>();
                        ppDic = ap.AreaParameters;
                        Dictionary<int, ParamProperties> instParam = new Dictionary<int, ParamProperties>();
                        Dictionary<int, ParamProperties> typeParam = new Dictionary<int, ParamProperties>();
                        Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                        if (catProParamDictionary.ContainsKey("Areas")) { projectParams = catProParamDictionary["Areas"]; }

                        foreach (int pId in ppDic.Keys)
                        {
                            ParamProperties pp = ppDic[pId];
                            pp.IsVisible = false; //default visibility setting as false
                            pp.IsEditable = true;
                            pp.IsProject = false;
                            pp.CategoryName = "Areas";

                            //default lock type is "IsEditable"
                            if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true; }
                            if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true;  }

                            if (pp.IsInstance)
                            {
                                pp.IsInstance = true;
                                instParam.Add(pp.ParamID, pp);
                            }
                            else
                            {
                                pp.IsInstance = false;
                                typeParam.Add(pp.ParamID, pp);
                            }
                        }
                        if (instCatParamSettings.ContainsKey("Areas")) { instCatParamSettings.Remove("Areas"); }
                        if (typeCatParamSettings.ContainsKey("Areas")) { typeCatParamSettings.Remove("Areas"); }

                        instCatParamSettings.Add("Areas", instParam);
                        typeCatParamSettings.Add("Areas", typeParam);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from area parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectViewTypeParamSettings()
        {
            try
            {
                if (!sharedParamDcitionary.ContainsKey("Views")) { sharedParamDcitionary.Add("Views", new Dictionary<int, ParamProperties>()); }
                Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                if (catProParamDictionary.ContainsKey("Views")) { projectParams = catProParamDictionary["Views"]; }

                Dictionary<string/*familyName*/, Dictionary<int, ViewTypeProperties>> viewFamilyDictionary = new Dictionary<string, Dictionary<int, ViewTypeProperties>>();
                foreach (int typeId in viewTypeDictionary.Keys)
                {
                    ViewTypeProperties vtp=viewTypeDictionary[typeId];
                    string viewFamily = viewTypeDictionary[typeId].ViewFamilyName;
                    if (!viewFamilyDictionary.ContainsKey(viewFamily))
                    {
                        viewFamilyDictionary.Add(viewFamily, new Dictionary<int, ViewTypeProperties>());
                    }
                    viewFamilyDictionary[viewFamily].Add(vtp.ViewTypeID, vtp);
                }

                foreach (string viewfamily in viewFamilyDictionary.Keys)
                {
                    if (tempStorage.ViewTypeParamSettigns.ContainsKey(viewfamily))
                    {
                        viewTypeParamSettigns.Add(viewfamily, tempStorage.ViewTypeParamSettigns[viewfamily]);
                        continue;
                    }

                    int firstkey = viewFamilyDictionary[viewfamily].Keys.First();
                    ViewTypeProperties vtp = viewFamilyDictionary[viewfamily][firstkey];
                    Dictionary<int, ParamProperties> typeParameters = new Dictionary<int, ParamProperties>();
                    typeParameters = vtp.ViewTypeParameters;

                    viewTypeParamSettigns.Add(viewfamily, new Dictionary<int, ParamProperties>());

                    foreach (int pId in typeParameters.Keys)
                    {
                        ParamProperties pp = typeParameters[pId];
                        pp.IsInstance = false;
                        pp.IsVisible = false; //default visibility setting as false
                        pp.IsEditable = true;
                        pp.IsProject = false;
                        pp.CategoryName = "Views";

                        //default lock type is "IsEditable"
                        if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true; }

                        //project Parameters will be visible 
                        if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true;  }

                        viewTypeParamSettigns[viewfamily].Add(pp.ParamID, pp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from view type parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectViewInstParamSettings()
        {
            try
            {
                if (!sharedParamDcitionary.ContainsKey("Views")) { sharedParamDcitionary.Add("Views", new Dictionary<int, ParamProperties>()); }
                Dictionary<string, ProjectParameter> projectParams = new Dictionary<string, ProjectParameter>();
                if (catProParamDictionary.ContainsKey("Views")) { projectParams = catProParamDictionary["Views"]; }

                Dictionary<string/*familyName*/, Dictionary<int/*typeID*/, Dictionary<int, ViewProperties>>> viewFamilyDictionary = new Dictionary<string, Dictionary<int,  Dictionary<int, ViewProperties>>>();
                foreach (int instId in viewInstDictionary.Keys)
                {
                    ViewProperties vp = viewInstDictionary[instId];
                    ViewTypeProperties vtp = viewTypeDictionary[vp.TypeID];
                    string viewFamily = vtp.ViewFamilyName;
                    if (!viewFamilyDictionary.ContainsKey(viewFamily))
                    {
                        viewFamilyDictionary.Add(viewFamily, new Dictionary<int, Dictionary<int, ViewProperties>>());
                        viewFamilyDictionary[viewFamily].Add(vtp.ViewTypeID, new Dictionary<int, ViewProperties>());
                        viewFamilyDictionary[viewFamily][vtp.ViewTypeID].Add(vp.ViewID, vp);
                    }
                    else if (!viewFamilyDictionary[viewFamily].ContainsKey(vtp.ViewTypeID))
                    {
                        viewFamilyDictionary[viewFamily].Add(vtp.ViewTypeID, new Dictionary<int, ViewProperties>());
                        viewFamilyDictionary[viewFamily][vtp.ViewTypeID].Add(vp.ViewID, vp);
                    }
                    else
                    {
                        viewFamilyDictionary[viewFamily][vtp.ViewTypeID].Add(vp.ViewID, vp);
                    }
                }

                foreach (string viewfamily in viewFamilyDictionary.Keys)
                {
                    if (tempStorage.ViewInstParamSettigns.ContainsKey(viewfamily))
                    {
                        viewInstParamSettigns.Add(viewfamily, tempStorage.ViewInstParamSettigns[viewfamily]);
                        continue;
                    }

                    Dictionary<int, ParamProperties> instParameters = new Dictionary<int, ParamProperties>();

                    foreach (int typeId in viewFamilyDictionary[viewfamily].Keys)
                    {
                        int firstkey = viewFamilyDictionary[viewfamily][typeId].Keys.First();
                        ViewProperties vp = viewFamilyDictionary[viewfamily][typeId][firstkey];

                        foreach (int pId in vp.ViewParameters.Keys)
                        {
                            if (instParameters.ContainsKey(pId)) { continue; }

                            ParamProperties pp = vp.ViewParameters[pId];
                            pp.IsInstance = false;
                            pp.IsVisible = false; //default visibility setting as false
                            pp.IsEditable = true;
                            pp.IsProject = false;
                            pp.CategoryName = "Views";

                            //default lock type is "IsEditable"
                            if (!pp.IsReadOnly) { pp.IsLockAll = false; pp.IsEditable = true; }

                            //project Parameters will be visible 
                            if (projectParams.ContainsKey(pp.ParamName)) { pp.IsProject = true;  }

                            instParameters.Add(pp.ParamID, pp);
                        }
                    }
                    viewInstParamSettigns.Add(viewfamily, instParameters);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter settings from view instance parameters: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayParamGridView(string categoryName, int familyId, string familyName, bool isCategorySelected, bool isViewSelected)
        {
            try
            {
                dataGridParameter.Rows.Clear();
                paramSettings.SharedParameters = sharedParamDcitionary;

                if (radioBttnType.Checked)
                {
                    Dictionary<int, ParamProperties> typeParameters = new Dictionary<int, ParamProperties>();
                    if (isCategorySelected && typeCatParamSettings.ContainsKey(categoryName)) { typeParameters = typeCatParamSettings[categoryName]; }
                    else if(typeParamSettings.ContainsKey(familyId)) { typeParameters = typeParamSettings[familyId]; }
                    else if (isViewSelected && viewTypeParamSettigns.ContainsKey(familyName)) { typeParameters = viewTypeParamSettigns[familyName]; }
                    paramSettings.DisplayParameters(typeParameters);
                }

                if (radioBttnInst.Checked)
                {
                    Dictionary<int, ParamProperties> instanceParameters = new Dictionary<int, ParamProperties>();
                    if (isCategorySelected && instCatParamSettings.ContainsKey(categoryName)) { instanceParameters = instCatParamSettings[categoryName]; }
                    else if(instParamSettings.ContainsKey(familyId)){ instanceParameters = instParamSettings[familyId]; }
                    else if (isViewSelected && viewInstParamSettigns.ContainsKey(familyName)) { instanceParameters = viewInstParamSettigns[familyName]; }
                    paramSettings.DisplayParameters(instanceParameters);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the parameters lists: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveParameterSettings(string categoryName, int familyId, string familyName, bool isCategorySelected, bool isViewSelected, bool isInstance)
        {
            try
            {
                if (isInstance)
                {
                    Dictionary<int, ParamProperties> instanceParameters = new Dictionary<int, ParamProperties>();
                    if (isCategorySelected && instCatParamSettings.ContainsKey(categoryName)) { instanceParameters = instCatParamSettings[categoryName]; }
                    else if(instParamSettings.ContainsKey(familyId)) { instanceParameters = instParamSettings[familyId]; }
                    else if (isViewSelected && viewInstParamSettigns.ContainsKey(familyName)) { instanceParameters = viewInstParamSettigns[familyName]; }

                    if (instanceParameters.Count > 0)
                    {
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        paramSettings.DataGridView = dataGridParameter;
                        paramProperties = paramSettings.SaveParameterSettings(instanceParameters, true);

                        if (isCategorySelected && instCatParamSettings.ContainsKey(categoryName)) { instCatParamSettings.Remove(categoryName); instCatParamSettings.Add(categoryName, paramProperties); tempStorage.InstCatParamSettings = instCatParamSettings; }
                        else if (isViewSelected && viewInstParamSettigns.ContainsKey(familyName)) { viewInstParamSettigns.Remove(familyName); viewInstParamSettigns.Add(familyName, paramProperties); tempStorage.ViewInstParamSettigns = viewInstParamSettigns; }
                        else if (instParamSettings.ContainsKey(familyId)) { instParamSettings.Remove(familyId); instParamSettings.Add(familyId, paramProperties); tempStorage.InstParamSettings = instParamSettings; }
                    }
                }
                else
                {
                    Dictionary<int, ParamProperties> typeParameters = new Dictionary<int, ParamProperties>();
                    if (isCategorySelected && typeCatParamSettings.ContainsKey(categoryName)) { typeParameters = typeCatParamSettings[categoryName]; }
                    else if(typeParamSettings.ContainsKey(familyId)) { typeParameters = typeParamSettings[familyId]; }
                    else if (isViewSelected && viewTypeParamSettigns.ContainsKey(familyName)) { typeParameters = viewTypeParamSettigns[familyName]; }
                    if (typeParameters.Count > 0)
                    {
                        Dictionary<int, ParamProperties> paramProperties = new Dictionary<int, ParamProperties>();
                        paramSettings.DataGridView = dataGridParameter;
                        paramProperties = paramSettings.SaveParameterSettings(typeParameters, false);

                        if (isCategorySelected && typeCatParamSettings.ContainsKey(categoryName)) { typeCatParamSettings.Remove(categoryName); typeCatParamSettings.Add(categoryName, paramProperties); tempStorage.TypeCatParamSettings = typeCatParamSettings; }
                        else if (isViewSelected && viewTypeParamSettigns.ContainsKey(familyName)) { viewTypeParamSettigns.Remove(familyName); viewTypeParamSettigns.Add(familyName, paramProperties); tempStorage.ViewTypeParamSettigns = viewTypeParamSettigns; }
                        else if(typeParamSettings.ContainsKey(familyId)) { typeParamSettings.Remove(familyId); typeParamSettings.Add(familyId, paramProperties); tempStorage.TypeParamSettings = typeParamSettings; }
                    }
                }
                sharedParamDcitionary = new Dictionary<string, Dictionary<int, ParamProperties>>();
                sharedParamDcitionary = paramSettings.SharedParameters;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save parameter settings: "+categoryName+" \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Parameter Tab Component Event
        private void dataGridFamily_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SaveParameterSettings(selectedCategory, selectedFamilyID, selectedFamily, isCategory, isView, radioBttnInst.Checked);
            if (excludeInstanceSettings.ContainsKey(selectedCategory)) { excludeInstanceSettings[selectedCategory] = checkBoxExcludeInst.Checked; }

            DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)dataGridFamily.Rows[e.RowIndex].Cells[0];
            string value = cell.Value.ToString();
            DataGridViewTextBoxCell categoryCell = (DataGridViewTextBoxCell)dataGridFamily.Rows[e.RowIndex].Cells[1];
            string categoryName = categoryCell.Value.ToString();
            int familyId=0;

            if (null != cell.Tag)
            {
                string strTag = cell.Tag.ToString();
                if (strTag.Contains("category"))
                {
                    selectedCategory = categoryName;
                    selectedFamilyID = 0;
                    selectedFamily = "";
                    isCategory = true;
                    isView = false;
                }
                else
                {
                    if (int.TryParse(strTag, out familyId)) //Tag: family Id
                    {
                        selectedCategory = categoryName;
                        selectedFamilyID = familyId;
                        selectedFamily = "";
                        isCategory = false;
                        isView = false;
                    }
                    else //Tag: view family Name 
                    {
                        selectedCategory = categoryName;
                        selectedFamilyID = familyId;
                        selectedFamily = strTag;
                        isCategory = false;
                        isView = true;
                    }
                }
                
                DisplayParamGridView(selectedCategory, selectedFamilyID, selectedFamily, isCategory,isView);
                if (excludeInstanceSettings.ContainsKey(selectedCategory)) { checkBoxExcludeInst.Checked = excludeInstanceSettings[selectedCategory]; }
                if (selectedCategory.Contains("Areas") || selectedCategory.Contains("Rooms") || selectedCategory.Contains("Spaces"))
                {
                    checkBoxExcludeInst.Checked = false; checkBoxExcludeInst.Enabled = false;
                }
                else { checkBoxExcludeInst.Enabled = true; }
            }
        }

        private void radioBttnType_CheckedChanged(object sender, EventArgs e)
        {
            SaveParameterSettings(selectedCategory, selectedFamilyID, selectedFamily, isCategory, isView, !radioBttnInst.Checked);
            dataGridParameter.Rows.Clear();
            DisplayParamGridView(selectedCategory, selectedFamilyID, selectedFamily, isCategory, isView);
        }

        private void checkBoxExcludeInst_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxExcludeInst.Checked)
            {
                radioBttnInst.Checked = false;
                radioBttnType.Checked = true;
                radioBttnInst.Enabled = false;
            }
            else
            {
                radioBttnInst.Checked = true;
                radioBttnType.Checked = false;
                radioBttnInst.Enabled = true;
            }
        }

        //To draw view icon in the dataGridParameter 
        private void dataGridParameter_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0 && e.RowIndex > 0)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                    DataGridViewCell cell = dataGridParameter.Rows[e.RowIndex].Cells[0];

                    bool isVisible = false;
                    if (cell.Tag != null) { isVisible = (bool)cell.Tag; }

                    if (isVisible)
                    {
                        string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                        Icon icon = new Icon(Path.GetDirectoryName(currentAssembly) + "/Resources/eye.ico");
                        e.Graphics.DrawIcon(icon, e.CellBounds.Left + 25, e.CellBounds.Top + 3);
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurs while painting DataGridViewButtonCell \n"+ex.Message, "DataGridView Error:");
            }
            
        }

        private void dataGridParameter_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dataGridParameter.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        private void dataGridParameter_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex > 0)
            {
                DataGridViewCell cell = dataGridParameter.Rows[e.RowIndex].Cells[0];
                if (cell.Tag != null)
                {
                    cell.Tag = !(bool)cell.Tag;
                }
            } 
        }

        private void dataGridParameter_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && dataGridParameter.Rows[e.RowIndex].Cells[3].Value.ToString() != "header")
            {
                dataGridParameter.Rows[e.RowIndex].Selected = true;
            }
        }

        private void dataGridParameter_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0 && e.ColumnIndex > 0)
            {
                DataGridViewCell cell = dataGridParameter.Rows[e.RowIndex].Cells[3];
                if (cell.Tag != null)
                {
                    lblName.Text = ""; lblGroup.Text = ""; lblType.Text = ""; lblDUT.Text = ""; lblParamType.Text = ""; lblFormat.Text = "";
                    ParamProperties pp = (ParamProperties)cell.Tag;
                    lblName.Text = pp.ParamName;
                    lblGroup.Text = pp.ParamGroup.Substring(3);
                    lblType.Text = pp.ParamType;
                    if (pp.DisplayUnitType.Contains("DUT")) { lblDUT.Text = pp.DisplayUnitType.Substring(4); }
                    if (pp.ParamID < 0) { lblParamType.Text = "Built-in Parameter"; }
                    else if (pp.IsShared) { lblParamType.Text = "Shared Parameter"; }
                    else { lblParamType.Text = "Family Parameter"; }
                    lblFormat.Text = pp.ParamFormat;
                }

            }
        }
        #endregion

        #region Lock Type
        private void lockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridParameter.SelectedRows)
            {
                int rowNum = row.Index;
                if ((dataGridParameter.Rows[rowNum].Cells[3].Value.ToString() != "header") && (LockType.ReadOnly != (LockType)dataGridParameter.Rows[rowNum].Cells[2].Tag))
                {
                    dataGridParameter.Rows[rowNum].Cells[2].Value = imageListLock.Images[(int)LockType.LockAll]; //lock all
                    dataGridParameter.Rows[rowNum].Cells[2].Tag = LockType.LockAll;
                }
            }       
        }

        private void revitLockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridParameter.SelectedRows)
            {
                int rowNum = row.Index;
                if ((dataGridParameter.Rows[rowNum].Cells[3].Value.ToString() != "header") && (LockType.ReadOnly != (LockType)dataGridParameter.Rows[rowNum].Cells[2].Tag))
                {
                    dataGridParameter.Rows[rowNum].Cells[2].Value = imageListLock.Images[(int)LockType.Editable]; //revit lock
                    dataGridParameter.Rows[rowNum].Cells[2].Tag = LockType.Editable;
                }
            }
        }


        private void bttnLock_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridParameter.SelectedRows)
            {
                int rowNum = row.Index;
                if ((dataGridParameter.Rows[rowNum].Cells[3].Value.ToString() != "header") && (LockType.ReadOnly != (LockType)dataGridParameter.Rows[rowNum].Cells[2].Tag))
                {
                    dataGridParameter.Rows[rowNum].Cells[2].Value = imageListLock.Images[(int)LockType.LockAll]; //lock all
                    dataGridParameter.Rows[rowNum].Cells[2].Tag = LockType.LockAll;
                }
            }
        }

        private void bttnRevit_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridParameter.SelectedRows)
            {
                int rowNum = row.Index;
                if ((dataGridParameter.Rows[rowNum].Cells[3].Value.ToString() != "header") && (LockType.ReadOnly != (LockType)dataGridParameter.Rows[rowNum].Cells[2].Tag))
                {
                    dataGridParameter.Rows[rowNum].Cells[2].Value = imageListLock.Images[(int)LockType.Editable]; //editable
                    dataGridParameter.Rows[rowNum].Cells[2].Tag = LockType.Editable;
                }
            }
        }

        #endregion

        #region Auto Synchronization

        private void bttnSyncCat_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (ListViewItem item in listViewCategory.SelectedItems)
                {
                    if (!item.Text.Contains("(Auto)"))
                    {
                        int id = 0;
                        bool result = int.TryParse(item.Name, out id);
                        if (result)
                        {
                            SyncProperties sp = new SyncProperties();
                            sp.SyncType = SyncType.Category;
                            sp.CategoryID = id;
                            sp.CategoryName = item.Text;
                            syncDictionary.Add(sp.CategoryID, sp);

                            AutoAddToSelection(item);
                            item.Text += " (Auto)";
                            item.ForeColor = System.Drawing.Color.Gray;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set Auto-Sync Option: \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AutoAddToSelection(ListViewItem catItem)
        {
            try
            {
                string categoryName = catItem.Text;
                if (elementDictionary.ContainsKey(categoryName))
                {
                    foreach (int typeId in elementDictionary[categoryName].Keys)
                    {
                        ElementTypeProperties etp = elementDictionary[categoryName][typeId];
                        List<int> familyIds = new List<int>();

                        if (etp.IsFamilySymbol)
                        {
                            if (!syncDictionary.ContainsKey(etp.FamilyID) && !familyIds.Contains(etp.FamilyID))
                            {
                                familyIds.Add(etp.FamilyID);
                                SyncProperties sp = new SyncProperties();
                                sp.SyncType = SyncType.Family;
                                sp.FamilyID = etp.FamilyID;
                                sp.FamilyName = etp.FamilyName;

                                syncDictionary.Add(sp.FamilyID, sp);
                            }
                        }
                        if (!listViewSelection.Items.ContainsKey(etp.TypeID.ToString()))
                        {
                            AddItemInSelection(etp.ElementTypeObject);
                        }
                    }
                }
                else if (categoryName == "Views")
                {
                    Category category = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);

                    SyncProperties sp = new SyncProperties();
                    sp.SyncType = SyncType.Category;
                    sp.CategoryID = category.Id.IntegerValue;
                    sp.CategoryName = category.Name;
                    syncDictionary.Add(sp.CategoryID, sp);

                    foreach (string viewFamily in viewFamilyTypes.Keys)
                    {
                        foreach (int viewTypeId in viewFamilyTypes[viewFamily].Keys)
                        {
                            ViewFamilyType vft = viewFamilyTypes[viewFamily][viewTypeId];
                            AddItemInSelection(vft);
                        }
                    }

                }
                UpdateTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add items in the selection from Auto-Sync option: "+catItem.Text+"\n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCancelCat_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewCategory.SelectedItems)
            {
                if (item.Text.Contains("(Auto)"))
                {
                    string itemText = item.Text.Replace(" (Auto)", "");
                    item.Text = itemText;
                    item.ForeColor = System.Drawing.Color.Black;
                    int id=int.Parse(item.Name);
                    syncDictionary.Remove(id);

                    CancelAutoSync(item);
                }
            }
        }

        private void CancelAutoSync(ListViewItem catItem)
        {
            try
            {
                string categoryName = catItem.Text;
                if (elementDictionary.ContainsKey(categoryName))
                {
                    foreach (int typeId in elementDictionary[categoryName].Keys)
                    {
                        ElementTypeProperties etp = elementDictionary[categoryName][typeId];
                        if (etp.IsFamilySymbol)
                        {
                            if (syncDictionary.ContainsKey(etp.FamilyID))
                            {
                                syncDictionary.Remove(etp.FamilyID);
                            }
                        }

                        if (listViewSelection.Items.ContainsKey(etp.TypeID.ToString()))
                        {
                            ListViewItem listItem = listViewSelection.Items[etp.TypeID.ToString()];
                            listViewSelection.Items.Remove(listItem);
                            symbolsToExclude.Remove(etp.ElementTypeObject.Id);
                        }
                    }
                }
                else if (categoryName == "Views")
                {
                    Category category = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);
                    syncDictionary.Remove(category.Id.IntegerValue);

                    foreach (string viewFamily in viewFamilyTypes.Keys)
                    {
                        foreach (int viewTypeId in viewFamilyTypes[viewFamily].Keys)
                        {
                            ViewFamilyType vft = viewFamilyTypes[viewFamily][viewTypeId];

                            if (listViewSelection.Items.ContainsKey(vft.Id.IntegerValue.ToString()))
                            {
                                ListViewItem listItem = listViewSelection.Items[vft.Id.IntegerValue.ToString()];
                                listViewSelection.Items.Remove(listItem);
                                symbolsToExclude.Remove(vft.Id);
                            }
                        }
                    }
                }
                UpdateTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to cancel the Auto-Sync option: "+catItem.Text+" \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //Only FamilySymbol will be allowed to do Type Sync but ElementType from System families
        private void bttnSyncType_Click(object sender, EventArgs e)
        {
            TreeNode familyNode = treeViewFamily.SelectedNode;

            if (null != familyNode && !familyNode.Text.Contains("(Auto)"))
            {
                int id = 0;
                bool result = int.TryParse(familyNode.Name, out id);

                if (result)
                {
                    SyncProperties sp = new SyncProperties();
                    sp.SyncType = SyncType.Family;
                    sp.FamilyID = id;
                    sp.FamilyName = familyNode.Text;
                    syncDictionary.Add(sp.FamilyID, sp);

                    AutoAddToSelection(familyNode);
                    familyNode.Text += " (Auto)";
                    familyNode.ForeColor = System.Drawing.Color.Gray;
                }
            }
            else
            {
                MessageBox.Show("Please select an available family name for Auto-Sync", "Auto-Sync Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AutoAddToSelection(TreeNode node)
        {
            foreach (TreeNode typeNode in node.Nodes)
            {
                if (null != typeNode.Tag)
                {
                    ElementType elementType = (ElementType)typeNode.Tag;
                    AddItemInSelection(elementType);
                }
            }
            UpdateTreeView();
        }

        private void bttnCancelFamily_Click(object sender, EventArgs e)
        {
            TreeNode familyNode = treeViewFamily.SelectedNode;
            if (null!=familyNode && familyNode.Text.Contains("(Auto)"))
            {
                string nodeText = familyNode.Text.Replace(" (Auto)", "");
                familyNode.Text = nodeText;
                familyNode.ForeColor = System.Drawing.Color.Black;

                int id = int.Parse(familyNode.Name);
                syncDictionary.Remove(id);

                CancelAutoSync(familyNode);
            }
        }

        private void CancelAutoSync(TreeNode familyNode)
        {
            try
            {
                string selNodeID = familyNode.Name;

                for (int i = listViewSelection.Items.Count - 1; i >= 0; i--)
                {
                    ListViewItem listItem = listViewSelection.Items[i];
                    string familyID = listItem.SubItems[1].Name;
                    string catID = listItem.SubItems[2].Name;

                    if (selNodeID == familyID)
                    {
                        if (syncDictionary.ContainsKey(int.Parse(catID)))
                        {
                            ListViewItem catItem = listViewCategory.Items[catID];
                            string[] strArray = catItem.Text.Split(' ');
                            catItem.Text = strArray[0];
                            catItem.ForeColor = System.Drawing.Color.Black;
                            syncDictionary.Remove(int.Parse(catID));
                        }

                        ElementType elementType = (ElementType)listItem.Tag;
                        if (null != elementType) { symbolsToExclude.Remove(elementType.Id); }
                        listViewSelection.Items.Remove(listItem);
                    }
                }
                UpdateTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to cancel the Auto-Sync option: "+familyNode.Text+" \n" + ex.Message, "Revit Data Editor Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region reference database

        private void bttnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter="Access Database Files (*.mdb;*.accdb)|*.mdb;*.accdb";
            openFileDialog.InitialDirectory = Path.GetDirectoryName(GetDefaultDbPath());
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                extDBPath = openFileDialog.FileName;
                lblExtDB.Text = "External Database: "+extDBPath;
            }
        }

        private void bttnEditParam_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridExt.SelectedRows[0];
            int index=row.Index;

            if (null != row.Tag)
            {
                LinkedParameter lp = (LinkedParameter)row.Tag;
                if (File.Exists(lp.DBPath))
                {
                    form_ExternalDB externalDBForm = new form_ExternalDB(lp.CategoryName, lp.DBPath, dataGridExt);
                    externalDBForm.TypeCatParamSettings = typeCatParamSettings;
                    externalDBForm.IsEdit = true;
                    externalDBForm.RowIndex = index;
                    externalDBForm.LinkedParameter = lp;

                    if (DialogResult.OK == externalDBForm.ShowDialog())
                    {
                        lp = externalDBForm.LinkedParameter;
                        if (linkedParameters.ContainsKey(lp.CategoryName)) { linkedParameters.Remove(lp.CategoryName); }
                        linkedParameters.Add(lp.CategoryName, lp);

                        typeCatParamSettings = externalDBForm.TypeCatParamSettings;
                        externalDBForm.CloseDatabase();
                        externalDBForm.Close();
                        DisplayParamGridView(selectedCategory, selectedFamilyID, selectedFamily, isCategory, isView);
                    }
                }
                else
                {
                    MessageBox.Show("the External Database does not exist. \n" + lp.DBPath, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void bttnDelParam_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridExt.SelectedRows[0];
            dataGridExt.Rows.Remove(row);
            string categoryName = row.Cells[0].Value.ToString();
            if (linkedParameters.ContainsKey(categoryName)) { linkedParameters.Remove(categoryName); }
        }

        private void bttnAddRelationship_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(extDBPath))
                {
                    form_CategorySelection categoryForm = new form_CategorySelection();
                    List<string> categoryList = new List<string>();
                    foreach (string category in selectedFamilies.Keys)
                    {
                        if (linkedParameters.ContainsKey(category)) { continue; }
                        categoryList.Add(category);
                    }
                    categoryForm.CategoryList = categoryList;

                    if (categoryForm.ShowDialog() == DialogResult.OK)
                    {
                        string selectedCategory = categoryForm.SelectedCategory;
                        categoryForm.Close();

                        form_ExternalDB externalDBForm = new form_ExternalDB(selectedCategory, extDBPath, dataGridExt);
                        externalDBForm.TypeCatParamSettings = typeCatParamSettings;
                        externalDBForm.IsEdit = false;

                        if (externalDBForm.ShowDialog() == DialogResult.OK)
                        {
                            LinkedParameter lp = externalDBForm.LinkedParameter;
                            if (linkedParameters.ContainsKey(lp.CategoryName)) { linkedParameters.Remove(lp.CategoryName); }
                            linkedParameters.Add(lp.CategoryName, lp);

                            typeCatParamSettings = externalDBForm.TypeCatParamSettings; //updating parameters setting set to be equivalent to controlling parameters'
                            externalDBForm.CloseDatabase();
                            externalDBForm.Close();
                            DisplayParamGridView(selectedCategory, selectedFamilyID, selectedFamily, isCategory, isView);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("the External Database does not exist.\n Please select an exisitng database file.\n" + extDBPath, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add a parameter-field relationship. \n" + ex.Message, "Reference Link Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        private void form_Editor_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (Process pr in Process.GetProcesses())
            {
                if (pr.ProcessName == "MSACCESS")
                {
                    pr.Kill();
                }
            }
        }

        private void txtBoxSelection_Enter(object sender, EventArgs e)
        {
            var source = new AutoCompleteStringCollection();
            foreach (ListViewItem typeItem in listViewSelection.Items)
            {
                source.Add(typeItem.Text);
            }
            txtBoxSelection.AutoCompleteCustomSource = source;
        }

        private void bttnSelectionFilt_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSelection.Items)
            {
                if (item.Text.Contains(txtBoxSelection.Text))
                {
                    item.Selected = true;
                }
            }
        }

        private void bttnExisting_Click(object sender, EventArgs e)
        {
            try
            {
                form_FileSelection fileSelectionForm = new form_FileSelection(application);
                if (fileSelectionForm.ShowDialog() == DialogResult.OK)
                {
                    if (fileSelectionForm.DefualtFilePath != dbLocation)
                    {
                        dbLocation = fileSelectionForm.DefualtFilePath;
                        isEditMode = true;
                        firstEditMode = true;

                        editor = new RevitDBEditor(application, dbLocation);
                        excludeInstanceSettings = new Dictionary<string, bool>();
                        excludeInstanceSettings = editor.ExcludeInstSettings;
                        symbolsToExclude = new List<ElementId>();
                        symbolsToExclude = editor.SymbolsToExclude;
                        editor.DisplaySelection(listViewSelection, imgListThumbnail);
                        syncDictionary = new Dictionary<int, SyncProperties>();
                        syncDictionary = editor.SyncDictionary;
                        linkedParameters = new Dictionary<string, LinkedParameter>();
                        linkedParameters = editor.LinkedParameter;
                        editor.DisplayRefData(dataGridExt);

                        LoadForm();
                        txtBoxDbPath.Text = dbLocation;
                        this.Text = "Edit Revit Database: " + dbLocation;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change data source.\n" + ex.Message, "Error:Data Source", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string htmPath = @"V:\RVT-Data\HOK Program\Documentation\Data Editor_Instruction.pdf";
            System.Diagnostics.Process.Start(htmPath);
        }

        private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

      

    }
}
