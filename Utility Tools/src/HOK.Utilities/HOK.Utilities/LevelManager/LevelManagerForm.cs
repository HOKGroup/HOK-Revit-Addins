using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;



namespace HOK.Utilities.LevelManager
{
    public partial class LevelManagerForm : System.Windows.Forms.Form
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private Dictionary<string, Level> levels = new Dictionary<string, Level>();
        private Dictionary<string, Level> selectedLevels = new Dictionary<string, Level>();
        private Dictionary<int/*elementId*/, Element> modelGroups = new Dictionary<int, Element>();
        private XYZ moveDirection = null;
        
        private bool maintain = false;
        private ElementSet selectedElement =  new ElementSet();
        private Dictionary<string/*catName*/, List<Element>> selectedElements = new Dictionary<string, List<Element>>();
        private bool logCreated = false;

        public LevelManagerForm(UIApplication application)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            CollectLevels();

            Selection selection = m_app.ActiveUIDocument.Selection;
#if RELEASE2015
            IList<ElementId> selectedIds = selection.GetElementIds().ToList();
            foreach (ElementId eId in selectedIds)
            {
                Element element = m_doc.GetElement(eId);
                if (null != element)
                {
                    selectedElement.Insert(element);
                }
            }
#else
            selectedElement = selection.Elements;
#endif

            if (selectedElement.IsEmpty)
            {
                radioButtonSelected.Enabled = false;
            }
            else
            {
                labelSelElements.Text = selectedElement.Size.ToString() + " Elements Selected";
                CollectSelectedLevels();
                radioButtonSelected.Checked = true;
            }
#if RELEASE2013
            checkBoxRoom.Enabled = false;
#endif
        }

        public void CollectLevels()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<Level> levelList = collector.OfClass(typeof(Level)).WhereElementIsNotElementType().ToElements().Cast<Level>().ToList();
                if (levelList.Count > 0)
                {
                    foreach (Level level in levelList)
                    {
                        if (!levels.ContainsKey(level.Name))
                        {
                            levels.Add(level.Name, level);
                            comboBoxFrom.Items.Add(level.Name);
                            comboBoxTo.Items.Add(level.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not collect level elements.\n"+ex.Message, "CollectLevels", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void CollectSelectedLevels()
        {
            try
            {
                foreach (Element element in selectedElement)
                {
#if RELEASE2013
                    if (null != element.Level)
                    {
                        if (!selectedLevels.ContainsKey(element.Level.Name))
                        {
                            selectedLevels.Add(element.Level.Name, element.Level);
                        }
                    }
#else
                    if (ElementId.InvalidElementId != element.LevelId)
                    {
                        Level eLevel = m_doc.GetElement(element.LevelId) as Level;
                        if (!selectedLevels.ContainsKey(eLevel.Name))
                        {
                            selectedLevels.Add(eLevel.Name, eLevel);
                        }
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not collect levels from selected elements.\n" + ex.Message, "Collect Levels from Selected Elements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                toolStripProgressBar.Visible = true;
                maintain = checkBoxMaintain.Checked;

                if (RehostLevel())
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not rehost elements to another level.\n" + ex.Message, "Rehost Level : All Elements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool RehostLevel()
        {
            bool result=false;
            try
            {
                Level levelFrom = null; 
                Level levelTo = null;
                if (comboBoxFrom.SelectedIndex > -1)
                {
                    string levelName = comboBoxFrom.SelectedItem.ToString();
                    if (levels.ContainsKey(levelName))
                    {
                        levelFrom = levels[levelName];
                    }
                }
                else if(radioButtonAll.Checked||radioButtonCategory.Checked)
                {
                    MessageBox.Show("Please select a level migrating from.", "Level Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (comboBoxTo.SelectedIndex > -1)
                {
                    string levelName = comboBoxTo.SelectedItem.ToString();
                    if (levels.ContainsKey(levelName))
                    {
                        levelTo = levels[levelName];
                    }
                }
                else
                {
                    MessageBox.Show("Please select a level migrating to.", "Level Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                logCreated = LogFileManager.CreateLogFile(m_doc);
                LogFileManager.ClearLogFile();
                LogFileManager.AppendLog(DateTime.Now.ToString() + ": Started Rehosting Elements to a new level, " + levelTo.Name);


                if (radioButtonAll.Checked)
                {
                    moveDirection = new XYZ(0, 0, levelFrom.Elevation - levelTo.Elevation);
                    result = RehostAllElement(levelFrom, levelTo);
                }
                else if (radioButtonSelected.Checked)
                {
                    result = RehostSelected(selectedElement, levelTo);
                }
                else if (radioButtonCategory.Checked && selectedElements.Count > 0)
                {
                    moveDirection = new XYZ(0, 0, levelFrom.Elevation - levelTo.Elevation);
                    result = RehostCategories(levelFrom, levelTo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not rehost levels.\n"+ex.Message, "Rehost Level", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private bool RehostAllElement(Level levelFrom, Level levelTo)
        {
            bool result = false;
            try
            {
                Transaction trans = new Transaction(m_doc);
                trans.Start("Rehost All");

                ElementLevelFilter levelFilter = new ElementLevelFilter(levelFrom.Id);

                ParameterValueProvider pvp = new ParameterValueProvider(new ElementId((int)BuiltInParameter.RBS_START_LEVEL_PARAM));
                FilterNumericRuleEvaluator fnrv = new FilterNumericEquals();
                ElementId rulevalId = levelFrom.Id;
                FilterRule paramFr = new FilterElementIdRule(pvp, fnrv, rulevalId);
                ElementParameterFilter epf = new ElementParameterFilter(paramFr);

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<ElementId> elementIds = collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().WherePasses(levelFilter).ToElementIds().ToList();

                List<Element> systemFamilies = new List<Element>();
                List<Autodesk.Revit.DB.Architecture.Room> rooms = new List<Autodesk.Revit.DB.Architecture.Room>();
                List<Element> familyInstances = new List<Element>();
#if RELEASE2013
#else
                RoomManager roomManager = new RoomManager(m_app);
                if (checkBoxRoom.Checked)
                {
                    FilteredElementCollector roomCollector = new FilteredElementCollector(m_doc);
                    rooms = roomCollector.OfCategory(BuiltInCategory.OST_Rooms).WherePasses(levelFilter).WhereElementIsNotElementType().ToElements().Cast<Room>().ToList();
                    
                    roomManager.RoomElements = rooms;
                    roomManager.CollectRooms();
                }
#endif
                if (elementIds.Count > 0)
                {
                    collector = new FilteredElementCollector(m_doc);
                    LogicalOrFilter orFilter = new LogicalOrFilter(epf, levelFilter);
                    ExclusionFilter exclusionFilter = new ExclusionFilter(elementIds);
                    LogicalAndFilter andFilter = new LogicalAndFilter(orFilter, exclusionFilter);
                    systemFamilies = collector.WherePasses(andFilter).WhereElementIsNotElementType().ToElements().ToList();

                    toolStripProgressBar.Maximum = systemFamilies.Count;
                    toolStripStatusLabel.Text = "Migrating System Families . . ";
                    Thread.Sleep(500);

                    RehostSystemFamily(levelTo, systemFamilies);
                    collector = new FilteredElementCollector(m_doc);
                    familyInstances = collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().WherePasses(levelFilter).ToElements().ToList();

                    toolStripProgressBar.Value = 0;
                    toolStripProgressBar.Maximum = familyInstances.Count;
                    toolStripStatusLabel.Text = "Migrating Family Instances . . ";
                    Thread.Sleep(500);

                    RehostFamilyInstance(levelTo, familyInstances);
                }
                else
                {
                    collector = new FilteredElementCollector(m_doc);
                    LogicalOrFilter orFilter = new LogicalOrFilter(epf, levelFilter);
                    systemFamilies = collector.WherePasses(orFilter).WhereElementIsNotElementType().ToElements().ToList();

                    toolStripProgressBar.Maximum = systemFamilies.Count;
                    toolStripStatusLabel.Text = "Migrating System Families . . ";
                    Thread.Sleep(500);

                    RehostSystemFamily(levelTo, systemFamilies);
                }
#if RELEASE2013
#else
                if (checkBoxRoom.Checked)
                {
                    if (roomManager.CopyRooms(levelTo))
                    {
                        roomManager.DeleteRooms();
                    }
                }
#endif
                trans.Commit();
                
                //Determine whether delete the level or not
                if (checkBoxDelete.Checked)
                {
                    FilteredElementCollector elementCollector = new FilteredElementCollector(m_doc);
                    LogicalOrFilter orFilter = new LogicalOrFilter(epf, levelFilter);
                    IList<Element> elementList = elementCollector.WherePasses(orFilter).WhereElementIsNotElementType().ToElements().ToList();
                    if (elementList.Count > 0)
                    {
                        trans = new Transaction(m_doc);
                        try
                        {
                            trans.Start("Make Selection");
#if RELEASE2015
                            List<ElementId> elementsToShow = new List<ElementId>();
                            foreach (Element element in elementList)
                            {
                                elementsToShow.Add(element.Id);
                            }
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.SetElementIds(elementsToShow);
#else
                            SelElementSet selElementSet = SelElementSet.Create();

                            List<ElementId> elementsToShow = new List<ElementId>();
                            foreach (Element element in elementList)
                            {
                                selElementSet.Add(element);
                                elementsToShow.Add(element.Id);
                            }
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.Elements = selElementSet;
#endif
                            trans.Commit();
                            MessageBox.Show("Selected elements cannot be hosted to a new level.\nPlease identify the problems.", "Failure Message: Rehosting Elements", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            result = true;
                        }
                        catch
                        {
                            trans.RollBack();
                            toolStripProgressBar.Visible = false;
                            toolStripStatusLabel.Text = "Ready . .";
                            result = false;
                        }
                    }
                    else
                    {
                        trans = new Transaction(m_doc);
                        try
                        {
                            trans.Start("Delete Element");

                            List<ViewPlan> associatedViews = new List<ViewPlan>();
                            FilteredElementCollector viewCollector = new FilteredElementCollector(m_doc);
                            List<ViewPlan> planViews = viewCollector.OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().Cast<ViewPlan>().ToList();
                            var query = from element in planViews where null!=element.GenLevel select element;
                            List<ViewPlan> views = query.ToList<ViewPlan>();
                            
                            foreach (ViewPlan viewPlan in views)
                            {
                                if (viewPlan.GenLevel.Id == levelFrom.Id)
                                {
                                    associatedViews.Add(viewPlan);
                                }
                            }

                            if (associatedViews.Count > 0)
                            {
                                StringBuilder strBuilder = new StringBuilder();
                                strBuilder.AppendLine("Elements have been migrated to "+levelTo.Name+".\nIf you want to retain annotation from any of the following views, click Cancel so you can make copies before deleting "+levelFrom.Name+" and associated views. Proceed?");
                                strBuilder.AppendLine("");

                                foreach (ViewPlan viewplan in associatedViews)
                                {
                                    strBuilder.AppendLine("view "+viewplan.Name+" will be deleted");
                                }

                                DialogResult dr = MessageBox.Show(strBuilder.ToString(),"Warning Message : Plan Views", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                if (dr == DialogResult.OK)
                                {
                                    m_doc.Delete(levelFrom.Id);
                                }
                            }
                            else
                            {
                                m_doc.Delete(levelFrom.Id);
                            }
                            
                            trans.Commit();
                            MessageBox.Show("All elements hosted by the old level were successfully migrated to the new level.\nThe deprecated level was removed.", "Successfully Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LogFileManager.AppendLog(DateTime.Now.ToString() + ": All elements hosted by the old level were successfully migrated to the new level.\nThe deprecated level was removed.");
                            if (logCreated) { LogFileManager.WriteLogFile(); }
                            result = true;
                        }
                        catch
                        {
                            trans.RollBack();
                            toolStripProgressBar.Visible = false;
                            toolStripStatusLabel.Text = "Ready . .";
                            result = false;
                        }
                    }
                }
                else if (modelGroups.Count > 0)
                {
                    trans = new Transaction(m_doc);
                    trans.Start("Make Selection");

                    try
                    {
                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.AppendLine("Following model groups are hosted to a new level.\nAll members of the groups should be migrated manually.");
                        strBuilder.AppendLine("");
#if RELEASE2015
                        List<ElementId> elementsToShow = new List<ElementId>();
                        foreach (int eId in modelGroups.Keys)
                        {
                            Element modelGroup = modelGroups[eId];
                            elementsToShow.Add(modelGroup.Id);
                            strBuilder.AppendLine("Element Id: " + eId + "\tModel Group: " + modelGroup.Name);
                        }

                        LogFileManager.AppendLog(strBuilder.ToString());
                        if (logCreated) { LogFileManager.WriteLogFile(); }

                        MessageBoxForm msgForm = new MessageBoxForm("Model Groups", strBuilder.ToString(), LogFileManager.logFullName, true);
                        DialogResult dr = msgForm.ShowDialog();
                        if (dr == DialogResult.OK)
                        {

                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.SetElementIds(elementsToShow);
                        }
#else
                        SelElementSet selElementSet = SelElementSet.Create();
                        List<ElementId> elementsToShow = new List<ElementId>();
                        foreach (int eId in modelGroups.Keys)
                        {
                            Element modelGroup = modelGroups[eId];
                            selElementSet.Add(modelGroup);
                            elementsToShow.Add(modelGroup.Id);
                            strBuilder.AppendLine("Element Id: " + eId + "\tModel Group: " + modelGroup.Name);
                        }

                        LogFileManager.AppendLog(strBuilder.ToString());
                        if (logCreated) { LogFileManager.WriteLogFile(); }

                        MessageBoxForm msgForm = new MessageBoxForm("Model Groups", strBuilder.ToString(), LogFileManager.logFullName, true);
                        DialogResult dr = msgForm.ShowDialog();
                        if (dr == DialogResult.OK)
                        {

                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.Elements = selElementSet;

                        }
#endif

                        trans.Commit();
                    }
                    catch
                    {
                        trans.RollBack();
                    }
                    result = true;
                }
                else
                {
                    MessageBox.Show("All elements hosted by the old level were successfully migrated to the new level.", "Successfully Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogFileManager.AppendLog(DateTime.Now.ToString() + ": All elements hosted by the old level were successfully migrated to the new level.\nThe deprecated level was removed.");
                    if (logCreated) { LogFileManager.WriteLogFile(); }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not rehost the selected elements to another level.\n" + ex.Message, "Rehost Level : SelectedElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private bool RehostSelected(ElementSet elementSet, Level levelTo)
        {
            bool result = false;
            try
            {
                Transaction trans = new Transaction(m_doc);
                trans.Start("Rehost Selected");

                List<ElementId> selElementIds = new List<ElementId>();
                List<Element> elements = new List<Element>();
                foreach (Element element in elementSet)
                {
                    selElementIds.Add(element.Id);
                    elements.Add(element);
                }

                FilteredElementCollector collector = new FilteredElementCollector(m_doc, selElementIds);
                IList<ElementId> elementIds = collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().ToElementIds().ToList();

                List<Element> systemFamilies = new List<Element>();
                List<Room> rooms = new List<Room>();
                List<Element> familyInstances = new List<Element>();

#if RELEASE2013
#else
                RoomManager roomManager = new RoomManager(m_app);
                if (checkBoxRoom.Checked)
                {
                    FilteredElementCollector roomCollector = new FilteredElementCollector(m_doc,selElementIds);
                    rooms = roomCollector.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements().Cast<Room>().ToList();

                    roomManager.RoomElements = rooms;
                    roomManager.CollectRooms();
                }
#endif
                if (elementIds.Count > 0)
                {
                    collector = new FilteredElementCollector(m_doc, selElementIds);
                    ExclusionFilter exclusionFilter = new ExclusionFilter(elementIds);
                    systemFamilies = collector.WherePasses(exclusionFilter).WhereElementIsNotElementType().ToElements().ToList();

                    toolStripProgressBar.Maximum = systemFamilies.Count;
                    toolStripStatusLabel.Text = "Migrating System Families . . ";

                    RehostSystemFamily(levelTo, systemFamilies);

                    collector = new FilteredElementCollector(m_doc, selElementIds);
                    familyInstances = collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().ToElements().ToList();

                    toolStripProgressBar.Value = 0;
                    toolStripProgressBar.Maximum = familyInstances.Count;
                    toolStripStatusLabel.Text = "Migrating Family Instances . . ";
                    Thread.Sleep(500);

                    RehostFamilyInstance(levelTo, familyInstances);
                }
                else
                {
                    systemFamilies = elements;

                    toolStripProgressBar.Maximum = systemFamilies.Count;
                    toolStripStatusLabel.Text = "Migrating System Families . . ";
                    Thread.Sleep(500);

                    RehostSystemFamily(levelTo, systemFamilies);
                }
#if RELEASE2013
#else
                if (checkBoxRoom.Checked)
                {
                    if (roomManager.CopyRooms(levelTo))
                    {
                        roomManager.DeleteRooms();
                    }
                }
#endif
                trans.Commit();

                //Determine whether delete the level or not
                if (checkBoxDelete.Checked)
                {
                    List<Element> elementsToDelete = new List<Element>();
                    List<ElementId> levelIds = new List<ElementId>();

                    foreach (string levelName in selectedLevels.Keys)
                    {
                        Level levelFrom = selectedLevels[levelName];
                        levelIds.Add(levelFrom.Id);

                        ElementLevelFilter levelFilter = new ElementLevelFilter(levelFrom.Id);

                        ParameterValueProvider pvp = new ParameterValueProvider(new ElementId((int)BuiltInParameter.RBS_START_LEVEL_PARAM));
                        FilterNumericRuleEvaluator fnrv = new FilterNumericEquals();
                        ElementId rulevalId = levelFrom.Id;
                        FilterRule paramFr = new FilterElementIdRule(pvp, fnrv, rulevalId);
                        ElementParameterFilter epf = new ElementParameterFilter(paramFr);

                        FilteredElementCollector elementCollector = new FilteredElementCollector(m_doc);
                        LogicalOrFilter orFilter = new LogicalOrFilter(epf, levelFilter);
                        IList<Element> elementList = elementCollector.WherePasses(orFilter).WhereElementIsNotElementType().ToElements().ToList();
                        elementsToDelete.AddRange(elementList);
                    }

                    if (elementsToDelete.Count > 0)
                    {
                        trans = new Transaction(m_doc);
                        try
                        {
                            trans.Start("Make Selection");
#if RELEASE2015
                            List<ElementId> elementsToShow = new List<ElementId>();
                            foreach (Element element in elementsToDelete)
                            {
                                elementsToShow.Add(element.Id);
                            }
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.SetElementIds(elementsToShow);
#else
                            SelElementSet selElementSet = SelElementSet.Create();
                            List<ElementId> elementsToShow = new List<ElementId>();
                            foreach (Element element in elementsToDelete)
                            {
                                selElementSet.Add(element);
                                elementsToShow.Add(element.Id);
                            }
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.Elements = selElementSet;
#endif


                            trans.Commit();
                            MessageBox.Show("Selected elements cannot be hosted to a new level.\nPlease identify the problems.", "Failure Message: Rehosting Elements", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            result = true;
                        }
                        catch
                        {
                            trans.RollBack();
                            toolStripProgressBar.Visible = false;
                            toolStripStatusLabel.Text = "Ready . .";
                            result = false;
                        }
                    }
                    else
                    {
                        trans = new Transaction(m_doc);
                        try
                        {
                            trans.Start("Delete Element");
                            List<ViewPlan> associatedViews = new List<ViewPlan>();
                            string levelNames = "";
                            foreach (string levelName in selectedLevels.Keys)
                            {
                                levelNames += levelName + ", ";
                                Level levelFrom = selectedLevels[levelName];
                                FilteredElementCollector viewCollector = new FilteredElementCollector(m_doc);
                                List<ViewPlan> planViews = viewCollector.OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().Cast<ViewPlan>().ToList();
                                var query = from element in planViews where null!=element.GenLevel select element;
                                IList<ViewPlan> views = query.ToList<ViewPlan>();
                                foreach (ViewPlan viewPlan in views)
                                {
                                    if (viewPlan.GenLevel.Id == levelFrom.Id)
                                    {
                                        associatedViews.Add(viewPlan);
                                    }
                                }
                            }

                            if (associatedViews.Count > 0)
                            {
                                StringBuilder strBuilder = new StringBuilder();
                                strBuilder.AppendLine("Elements have been migrated to " + levelTo.Name + ".\nIf you want to retain annotation from any of the following views, click Cancel so you can make copies before deleting "+levelNames+" and associated views. Proceed?");
                                strBuilder.AppendLine("");

                                foreach (ViewPlan viewplan in associatedViews)
                                {
                                    strBuilder.AppendLine("view " + viewplan.Name + " will be deleted");
                                }

                                DialogResult dr = MessageBox.Show(strBuilder.ToString(), "Warning Message : Plan Views", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                if (dr == DialogResult.OK)
                                {
                                    m_doc.Delete(levelIds);
                                }
                            }
                            else
                            {
                                m_doc.Delete(levelIds);
                            }

                            trans.Commit();
                            MessageBox.Show("All elements hosted by the old level were successfully migrated to the new level.\nThe deprecated level was removed.", "Successfully Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LogFileManager.AppendLog(DateTime.Now.ToString() + ": All elements hosted by the old level were successfully migrated to the new level.\nThe deprecated level was removed.");
                            if (logCreated) { LogFileManager.WriteLogFile(); }
                            result = true;
                        }
                        catch
                        {
                            trans.RollBack();
                            toolStripProgressBar.Visible = false;
                            toolStripStatusLabel.Text = "Ready . .";
                            result = false;
                        }
                    }
                }
                else if (modelGroups.Count > 0)
                {
                    trans = new Transaction(m_doc);
                    try
                    {
                        trans.Start("Make Selection");

                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.AppendLine("Following model groups are hosted to a new level.\nAll members of the groups should be migrated manually.");
                        strBuilder.AppendLine("");

#if RELEASE2015
                        List<ElementId> elementsToShow = new List<ElementId>();
                        foreach (int eId in modelGroups.Keys)
                        {
                            Element modelGroup = modelGroups[eId];
                            elementsToShow.Add(modelGroup.Id);
                            strBuilder.AppendLine("Element Id: " + eId + "\tModel Group: " + modelGroup.Name);
                        }

                        LogFileManager.AppendLog(strBuilder.ToString());
                        if (logCreated) { LogFileManager.WriteLogFile(); }

                        MessageBoxForm msgForm = new MessageBoxForm("Model Groups", strBuilder.ToString(), LogFileManager.logFullName, true);
                        DialogResult dr = msgForm.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.SetElementIds(elementsToShow);
                        }
#else
                        SelElementSet selElementSet = SelElementSet.Create();
                        List<ElementId> elementsToShow = new List<ElementId>();
                        foreach (int eId in modelGroups.Keys)
                        {
                            Element modelGroup = modelGroups[eId];
                            selElementSet.Add(modelGroup);
                            elementsToShow.Add(modelGroup.Id);
                            strBuilder.AppendLine("Element Id: " + eId + "\tModel Group: " + modelGroup.Name);
                        }

                        LogFileManager.AppendLog(strBuilder.ToString());
                        if (logCreated) { LogFileManager.WriteLogFile(); }

                        MessageBoxForm msgForm = new MessageBoxForm("Model Groups", strBuilder.ToString(), LogFileManager.logFullName, true);
                        DialogResult dr = msgForm.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.Elements = selElementSet;
                        }
#endif

                        trans.Commit();
                    }
                    catch
                    {
                        trans.RollBack();
                    }
                    result = true;
                }
                else
                {
                    MessageBox.Show("All elements hosted by the old level were successfully migrated to the new level.", "Successfully Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogFileManager.AppendLog(DateTime.Now.ToString() + ": All elements hosted by the old level were successfully migrated to the new level.");
                    if (logCreated) { LogFileManager.WriteLogFile(); }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not rehost the selected elements to another level.\n"+ex.Message, "Rehost Level : SelectedElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("[Error] Rehost Level with Selected Elements: " + ex.Message);
                if (logCreated) { LogFileManager.WriteLogFile(); }
            }
            return result;
        }

        private bool RehostCategories(Level levelFrom, Level levelTo)
        {
            bool result = false;
            try
            {
                Transaction trans = new Transaction(m_doc);
                trans.Start("Rehost Selected Categories");

                List<ElementId> selElementIds = new List<ElementId>();
                foreach (string categoryName in selectedElements.Keys)
                {
                    if (categoryName == "Rooms") { continue; }

                    List<Element> elements = selectedElements[categoryName];
                    foreach (Element element in elements)
                    {
                        selElementIds.Add(element.Id);
                    }
                }

                ElementLevelFilter levelFilter = new ElementLevelFilter(levelFrom.Id);

                ParameterValueProvider pvp = new ParameterValueProvider(new ElementId((int)BuiltInParameter.RBS_START_LEVEL_PARAM));
                FilterNumericRuleEvaluator fnrv = new FilterNumericEquals();
                ElementId rulevalId = levelFrom.Id;
                FilterRule paramFr = new FilterElementIdRule(pvp, fnrv, rulevalId);
                ElementParameterFilter epf = new ElementParameterFilter(paramFr);

                FilteredElementCollector collector = new FilteredElementCollector(m_doc, selElementIds);
                IList<ElementId> elementIds = collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().WherePasses(levelFilter).ToElementIds().ToList();

                List<Element> systemFamilies = new List<Element>();
                List<Room> rooms = new List<Room>();
                List<Element> familyInstances = new List<Element>();
#if RELEASE2013
#else
                RoomManager roomManager = new RoomManager(m_app);
                if (checkBoxRoom.Checked)
                {
                    if (selectedElements.ContainsKey("Rooms"))
                    {
                        List<ElementId> roomIds = new List<ElementId>();
                        foreach (Element room in selectedElements["Rooms"])
                        {
                            roomIds.Add(room.Id);
                        }

                        FilteredElementCollector roomCollector = new FilteredElementCollector(m_doc, roomIds);
                        rooms = roomCollector.WherePasses(levelFilter).WhereElementIsNotElementType().ToElements().Cast<Room>().ToList();
                        roomManager.RoomElements = rooms;
                        roomManager.CollectRooms();
                    }
                }
#endif
                if (elementIds.Count > 0) //when familiy instances exist
                {

                    collector = new FilteredElementCollector(m_doc, selElementIds);
                    LogicalOrFilter orFilter = new LogicalOrFilter(epf, levelFilter);
                    ExclusionFilter exclusionFilter = new ExclusionFilter(elementIds);
                    LogicalAndFilter andFilter = new LogicalAndFilter(orFilter, exclusionFilter);
                    systemFamilies = collector.WherePasses(andFilter).WhereElementIsNotElementType().ToElements().ToList();

                    toolStripProgressBar.Maximum = systemFamilies.Count;
                    toolStripStatusLabel.Text = "Migrating System Families . . ";
                    Thread.Sleep(500);

                    RehostSystemFamily(levelTo, systemFamilies);

                    collector = new FilteredElementCollector(m_doc, selElementIds);
                    familyInstances = collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().WherePasses(levelFilter).ToElements().ToList();


                    toolStripProgressBar.Value = 0;
                    toolStripProgressBar.Maximum = familyInstances.Count;
                    toolStripStatusLabel.Text = "Migrating Family Instances . . ";
                    Thread.Sleep(500);

                    RehostFamilyInstance(levelTo, familyInstances);
                }
                else
                {
                    collector = new FilteredElementCollector(m_doc, selElementIds);
                    LogicalOrFilter orFilter = new LogicalOrFilter(epf, levelFilter);
                    systemFamilies = collector.WherePasses(orFilter).WhereElementIsNotElementType().ToElements().ToList();

                    toolStripProgressBar.Maximum = systemFamilies.Count;
                    toolStripStatusLabel.Text = "Migrating System Families . . ";
                    Thread.Sleep(500);

                    RehostSystemFamily(levelTo, systemFamilies);
                }
#if RELEASE2013
#else
                if (checkBoxRoom.Checked)
                {
                    if (roomManager.CopyRooms(levelTo))
                    {
                        roomManager.DeleteRooms();
                    }
                }
#endif
                trans.Commit();

                //Determine whether delete the level or not
                if (checkBoxDelete.Checked)
                {
                    FilteredElementCollector elementCollector = new FilteredElementCollector(m_doc, selElementIds);
                    LogicalOrFilter orFilter = new LogicalOrFilter(epf, levelFilter);
                    IList<Element> elementList = elementCollector.WherePasses(orFilter).WhereElementIsNotElementType().ToElements().ToList();
                    if (elementList.Count > 0)
                    {
                        trans = new Transaction(m_doc);
                        try
                        {
                            trans.Start("Make Selection");

#if RELEASE2015
                            List<ElementId> elementsToShow = new List<ElementId>();
                            foreach (Element element in elementList)
                            {
                                elementsToShow.Add(element.Id);
                            }
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.SetElementIds(elementsToShow);
#else
                            SelElementSet selElementSet = SelElementSet.Create();
                            List<ElementId> elementsToShow = new List<ElementId>();
                            foreach (Element element in elementList)
                            {
                                selElementSet.Add(element);
                                elementsToShow.Add(element.Id);
                            }
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.Elements = selElementSet;
#endif

                            MessageBox.Show("Selected elements cannot be hosted to a new level.\nPlease identify the problems.", "Failure Message: Rehosting Elements", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            result = true;
                        }
                        catch
                        {
                            trans.RollBack();
                            toolStripProgressBar.Visible = false;
                            toolStripStatusLabel.Text = "Ready . .";
                            result = false;
                        }
                    }
                    else
                    {
                        trans = new Transaction(m_doc);
                        try
                        {
                            trans.Start("Delete Element");
                            List<ViewPlan> associatedViews = new List<ViewPlan>();
                            FilteredElementCollector viewCollector = new FilteredElementCollector(m_doc);
                            List<ViewPlan> planViews = viewCollector.OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().Cast<ViewPlan>().ToList();
                            var query = from element in planViews where null!=element.GenLevel select element;
                            IList<ViewPlan> views = query.ToList<ViewPlan>();

                            foreach (ViewPlan viewPlan in views)
                            {
                                if (viewPlan.GenLevel.Id == levelFrom.Id)
                                {
                                    associatedViews.Add(viewPlan);
                                }
                            }

                            if (associatedViews.Count > 0)
                            {
                                StringBuilder strBuilder = new StringBuilder();
                                strBuilder.AppendLine("Elements have been migrated to " + levelTo.Name + ".\nIf you want to retain annotation from any of the following views, click Cancel so you can make copies before deleting " + levelFrom.Name + " and associated views. Proceed?");
                                strBuilder.AppendLine("");

                                foreach (ViewPlan viewplan in associatedViews)
                                {
                                    strBuilder.AppendLine("view " + viewplan.Name + " will be deleted");
                                }

                                DialogResult dr = MessageBox.Show(strBuilder.ToString(), "Warning Message : Plan Views", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                if (dr == DialogResult.OK)
                                {
                                    m_doc.Delete(levelFrom.Id);
                                }
                            }
                            else
                            {
                                m_doc.Delete(levelFrom.Id);
                            }
                            trans.Commit();
                            MessageBox.Show("All elements hosted by the old level were successfully migrated to the new level.\nThe deprecated level was removed.", "Successfully Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LogFileManager.AppendLog(DateTime.Now.ToString() + ": All elements hosted by the old level were successfully migrated to the new level.\nThe deprecated level was removed.");
                            if (logCreated) { LogFileManager.WriteLogFile(); }
                            result = true;
                        }
                        catch
                        {
                            trans.RollBack();
                            toolStripProgressBar.Visible = false;
                            toolStripStatusLabel.Text = "Ready . .";
                            result = false;
                        }
                    }
                }
                else if (modelGroups.Count > 0)
                {
                    trans = new Transaction(m_doc);
                    try
                    {
                        trans.Start("Make Selection");
                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.AppendLine("Following model groups are hosted to a new level.\nAll members of the groups should be migrated manually.");
                        strBuilder.AppendLine("");

#if RELEASE2015
                        List<ElementId> elementsToShow = new List<ElementId>();
                        foreach (int eId in modelGroups.Keys)
                        {
                            Element modelGroup = modelGroups[eId];
                            elementsToShow.Add(modelGroup.Id);
                            strBuilder.AppendLine("Element Id: " + eId + "\tModel Group: " + modelGroup.Name);
                        }

                        LogFileManager.AppendLog(strBuilder.ToString());
                        if (logCreated) { LogFileManager.WriteLogFile(); }

                        MessageBoxForm msgForm = new MessageBoxForm("Model Groups", strBuilder.ToString(), LogFileManager.logFullName, true);
                        DialogResult dr = msgForm.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.SetElementIds(elementsToShow);
                        }
#else
                         SelElementSet selElementSet = SelElementSet.Create();
                        List<ElementId> elementsToShow = new List<ElementId>();
                        foreach (int eId in modelGroups.Keys)
                        {
                            Element modelGroup = modelGroups[eId];
                            selElementSet.Add(modelGroup);
                            elementsToShow.Add(modelGroup.Id);
                            strBuilder.AppendLine("Element Id: " + eId + "\tModel Group: " + modelGroup.Name);
                        }

                        LogFileManager.AppendLog(strBuilder.ToString());
                        if (logCreated) { LogFileManager.WriteLogFile(); }

                        MessageBoxForm msgForm = new MessageBoxForm("Model Groups", strBuilder.ToString(), LogFileManager.logFullName, true);
                        DialogResult dr = msgForm.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            m_app.ActiveUIDocument.ShowElements(elementsToShow);
                            m_app.ActiveUIDocument.Selection.Elements = selElementSet;
                        }
#endif

                        trans.Commit();
                    }
                    catch
                    {
                        trans.RollBack();
                    }
                    result = true;
                }
                else
                {
                    MessageBox.Show("All elements hosted by the old level were successfully migrated to the new level.", "Successfully Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogFileManager.AppendLog(DateTime.Now.ToString() + ": All elements hosted by the old level were successfully migrated to the new level.");
                    if (logCreated) { LogFileManager.WriteLogFile(); }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not rehost the selected elements to another level.\n" + ex.Message, "Rehost Level : SelectedElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private void RehostFamilyInstance(Level toLevel, List<Element> elementList)
        {
            try
            {
                foreach (Element element in elementList)
                {
                    toolStripProgressBar.PerformStep();
                    FamilyInstance fi = element as FamilyInstance;
                    if (null != fi)
                    {
#if RELEASE2013
                        if (null != fi.SuperComponent || null != fi.Group)
                        {
                            continue; //sub component should be ignored.
                        }
#else
                         if (null != fi.SuperComponent || fi.GroupId!=ElementId.InvalidElementId)
                        {
                            continue; //sub component should be ignored.
                        }
#endif
                       
                    }

                    Parameter levelparam = element.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                    Parameter baseLevelParam = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);

                    bool moved = false;
                    if (null != levelparam && !levelparam.IsReadOnly)
                    {
                        ElementId eLevelId = levelparam.AsElementId();
                        levelparam.Set(toLevel.Id);
                        if (maintain)
                        {
                            if (radioButtonAll.Checked || radioButtonCategory.Checked)
                            {
                                moved = element.Location.Move(moveDirection);
                            }
                            else if(radioButtonSelected.Checked)
                            {
                                if (ElementId.InvalidElementId != eLevelId)
                                {
                                    Level eLevel = m_doc.GetElement(eLevelId) as Level;
                                    XYZ direction = new XYZ(0, 0, eLevel.Elevation - toLevel.Elevation);
                                    moved = element.Location.Move(direction);
                                }
                            }
                        }
                    }
                    else if (null != baseLevelParam && !baseLevelParam.IsReadOnly)
                    {
                        LevelManager.MoveColumn(m_doc, element, toLevel, maintain);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not rehost levels.\n" + ex.Message, "Rehost Family Instance to Level", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RehostSystemFamily(Level toLevel, List<Element> elementList)
        {
            try
            {
                try
                {
                    foreach (Element element in elementList)
                    {
#if RELEASE2013
                        if (element.Group != null) { continue; } //if parent group exists, this should be ignored
#else
                        if (element.GroupId != ElementId.InvalidElementId) { continue; } //if parent group exists, this should be ignored
#endif

                        toolStripProgressBar.PerformStep();
                        if (null != element.Category)
                        {
                            switch (element.Category.Name)
                            {
                                case "Walls":
                                    LevelManager.MoveWalls(m_doc, element, toLevel, maintain);
                                    break;
                                case"Shaft Openings":
                                    LevelManager.MoveWalls(m_doc, element, toLevel, maintain);
                                    break;
                                case "Stairs":
                                    LevelManager.MoveStairs(m_doc, element, toLevel, maintain);
                                    break;
                                case "Floors":
                                    LevelManager.MoveFloors(m_doc, element, toLevel, maintain);
                                    break;
                                case "Ceilings":
                                    LevelManager.MoveCeiling(m_doc, element, toLevel, maintain);
                                    break;
                                case "Roofs":
                                    LevelManager.MoveRoof(m_doc, element, toLevel, maintain);
                                    break;
                                case "Railings":
                                    //LevelManager.MoveRailings(m_doc, element, toLevel, maintain);
                                    break;
                                case "Pads":
                                    LevelManager.MovePads(m_doc, element, toLevel, maintain);
                                    break;
                                case "Model Groups":
                                    bool writable = LevelManager.MoveGroups(m_doc, element, toLevel, maintain);
                                    if (writable)
                                    {
                                        if (!modelGroups.ContainsKey(element.Id.IntegerValue))
                                        {
                                            modelGroups.Add(element.Id.IntegerValue, element);//for warning message
                                        }
                                    }
                                    break;
                                default://Cable Tray, Conduit, Duct, Flex Duct, Pipe
                                    LevelManager.MoveMEP(m_doc, element, toLevel, maintain);
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not rehost levels.\n" + ex.Message, "Rehost System Family to Level", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonCategory_Click(object sender, EventArgs e)
        {
            try
            {
                CategoryForm categoryForm = new CategoryForm(m_app);
                if (DialogResult.OK == categoryForm.ShowDialog())
                {
                    selectedElements = categoryForm.SelectedElements;
                    labelCategory.Text = selectedElements.Count.ToString() + " Categories Selected";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select categories.\n"+ex.Message, "Select Categories", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void radioButtonCategory_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCategory.Checked)
            {
                buttonCategory.Enabled = true;
            }
            else
            {
                buttonCategory.Enabled = false;
            }
        }

        private void radioButtonSelected_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSelected.Checked)
            {
                comboBoxFrom.Enabled = false;
                checkBoxDelete.Checked = false;
                checkBoxDelete.Enabled = false;
            }
            else
            {
                comboBoxFrom.Enabled = true;
                checkBoxDelete.Enabled = true;
            }
        }

        
    }
}
