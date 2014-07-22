using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public enum MassSource { SelectedMass = 0, DisplayAll, OnlyHost, OnlyLink }

    public partial class Form_LinkedFiles : System.Windows.Forms.Form
    {
        private UIApplication m_app;
        private Document m_doc;
        private List<Element> selectedMass = new List<Element>();
        private Dictionary<string/*worksetName*/, int/*worksetId*/ > worksetDictionary = new Dictionary<string, int>();
        private List<MassProperties> integratedMassList = new List<MassProperties>(); //mass from host projects
        private Dictionary<int/*instanceId*/, LinkedInstanceProperties> linkedMassDictionary = new Dictionary<int, LinkedInstanceProperties>(); //mass from linked models
        private Dictionary<int/*elementId*/, ElementProperties> elementDictionary = new Dictionary<int, ElementProperties>(); //all intersecting elements
        private Dictionary<Category, bool> elementCategories = new Dictionary<Category, bool>();
        private List<string> massParameters = new List<string>();
        private MassSource selectedSourceType;
        private Form_ProgressBar progressForm = new Form_ProgressBar();
        private bool progressFormOpend = false;
        private List<int> categoryIds = new List<int>();
        private Dictionary<int/*categoryId*/, Dictionary<int, ParameterProperties>> parameterMaps = new Dictionary<int, Dictionary<int, ParameterProperties>>();
        private List<Document> linkedDocuments = new List<Document>();

        public Dictionary<string, int> WorksetDictionary { get { return worksetDictionary; } set { worksetDictionary = value; } }
        public List<MassProperties> IntegratedMassList { get { return integratedMassList; } set { integratedMassList = value; } }
        public Dictionary<int, LinkedInstanceProperties> LinkedMassDictionary { get { return linkedMassDictionary; } set { linkedMassDictionary = value; } }
        public Dictionary<int, ElementProperties> ElementDictionary { get { return elementDictionary; } set { elementDictionary = value; } }
        public Dictionary<Category, bool> ElementCategories { get { return elementCategories; } set { elementCategories = value; } }
        public List<string> MassParameters { get { return massParameters; } set { massParameters = value; } }
        public MassSource SelectedSourceType { get { return selectedSourceType; } set { selectedSourceType = value; } }
        public Dictionary<int, Dictionary<int, ParameterProperties>> ParameterMaps { get { return parameterMaps; } set { parameterMaps = value; } }
        public List<Document> LinkedDocuments { get { return linkedDocuments; } set { linkedDocuments = value; } }

        public Form_LinkedFiles(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            selectedSourceType = MassSource.OnlyHost;
            CollectLinkedDocuments();
            CollectWorksets();
            DisplayRvtLinkTypes();
            FindSelectedMass();
        }

        private void CollectWorksets()
        {
            try
            {
                FilteredWorksetCollector worksets = new FilteredWorksetCollector(m_doc).OfKind(WorksetKind.UserWorkset);
                foreach (Workset workset in worksets)
                {
                    if (!worksetDictionary.ContainsKey(workset.Name))
                    {
                        worksetDictionary.Add(workset.Name, workset.Id.IntegerValue);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect workset from the host project.\n" + ex.Message, "LinkedModelManager:CollectWorksets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("CollectWorksets", ex.Message);
            }
        }

        private void CollectLinkedDocuments()
        {
            try
            {
                foreach (Document document in m_app.Application.Documents)
                {
#if RELEASE2014 ||RELEASE2015
                    if (document.IsLinked)
                    {
                        linkedDocuments.Add(document);
                    }
#elif RELEASE2013
                    linkedDocuments.Add(document);
#endif
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect linked documents.\n" + ex.Message, "LinkedModelManager:CollectLinkedDocuments", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("CollectLinkedDocuments", ex.Message);
            }
        }

        private void DisplayRvtLinkTypes()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType();
                List<RevitLinkInstance> revitLinkInstances = collector.ToElements().Cast<RevitLinkInstance>().ToList();

                Dictionary<int/*typeId*/, RevitLinkType> linkTypes = new Dictionary<int, RevitLinkType>();
                foreach (RevitLinkInstance instance in revitLinkInstances)
                {
                    LinkedInstanceProperties lip = new LinkedInstanceProperties(instance);
                    ElementId typeId = instance.GetTypeId();
                    RevitLinkType linkType = m_doc.GetElement(typeId) as RevitLinkType;
                    lip.FileName = linkType.Name;
                    lip.TypeId = linkType.Id.IntegerValue;

                    if (!linkTypes.ContainsKey(linkType.Id.IntegerValue))
                    {
                        linkTypes.Add(linkType.Id.IntegerValue, linkType);
                    }

                    Parameter nameParam = instance.get_Parameter(BuiltInParameter.RVT_LINK_INSTANCE_NAME);
                    if (null != nameParam)
                    {
                        lip.InstanceName = nameParam.AsString();
                    }

                    foreach (Document document in m_app.Application.Documents)
                    {
                        if (lip.FileName.Contains(document.Title))
                        {
                            lip.LinkedDocument = document;
                            FilteredElementCollector linkedCollector = new FilteredElementCollector(document);
                            List<Element> massElements = linkedCollector.OfCategory(BuiltInCategory.OST_Mass).WhereElementIsNotElementType().ToElements().ToList();
                            lip.MassElements = massElements;
                            linkedMassDictionary.Add(lip.InstanceId, lip);
                            break;
                        }
                    }
                }

                foreach (int typeId in linkTypes.Keys)
                {
                    RevitLinkType linkType = linkTypes[typeId];
                    TreeNode typeNode = new TreeNode(linkType.Name);
                    typeNode.Name = typeId.ToString();
                    typeNode.Tag = linkType.Id;

                    foreach (int instanceId in linkedMassDictionary.Keys)
                    {
                        LinkedInstanceProperties lip = linkedMassDictionary[instanceId];
                        if (lip.TypeId == typeId)
                        {
                            TreeNode instanceNode = new TreeNode(lip.FileName+" : "+lip.InstanceName);
                            instanceNode.Name = lip.InstanceId.ToString();
                            instanceNode.Tag = lip.InstanceId;

                            foreach (Element massElement in lip.MassElements)
                            {
                                TreeNode massNode = new TreeNode(massElement.Name);
                                massNode.Name = massElement.Id.ToString();
                                massNode.Tag = massElement.Id;

                                instanceNode.Nodes.Add(massNode);
                            }
                            typeNode.Nodes.Add(instanceNode); 
                        }
                    }
                    treeViewLinkedFile.Nodes.Add(typeNode);
                }
                treeViewLinkedFile.Sort();

                if (treeViewLinkedFile.Nodes.Count > 0)
                {
                    radioButtonLink.Enabled = true;
                    radioButtonAll.Enabled = true;
                    radioButtonAll.Checked = true;

                    foreach (TreeNode typeNode in treeViewLinkedFile.Nodes)
                    {
                        typeNode.Checked = true;
                        foreach (TreeNode instanceNode in typeNode.Nodes)
                        {
                            instanceNode.Checked = true;
                            foreach (TreeNode massNode in instanceNode.Nodes)
                            {
                                massNode.Checked = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display RevitLinkTypes.\n" + ex.Message, "Form_LinkedFiles:DisplayRvtLinkTypes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("DisplayRvtLinkTypes", ex.Message);
            }
        }

        private void FindSelectedMass()
        {
            try
            {
                UIDocument uidoc = m_app.ActiveUIDocument;
                Selection selection = uidoc.Selection;

                foreach (ElementId elementId in selection.GetElementIds())
                {
                    Element element = m_doc.GetElement(elementId);
                    if (null != element)
                    {
                        if (null != element.Category)
                        {
                            if (element.Category.Name == "Mass")
                            {
                                selectedMass.Add(element);
                            }
                        }
                    }
                }

                if (selectedMass.Count > 0)
                {
                    radioButtonSelectedMass.Enabled = true;
                    radioButtonSelectedMass.Checked = true;
                    labelSelectedMass.Text = selectedMass.Count.ToString() + " mass selected.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find selected mass elements.\n" + ex.Message, "Form_LinkedFiles:FindSelectedMass", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("FindSelectedMass", ex.Message);
            }
        }

        private void bttnApply_Click(object sender, EventArgs e)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Element> massElements = collector.OfCategory(BuiltInCategory.OST_Mass).WhereElementIsNotElementType().ToElements().ToList();

                switch (selectedSourceType)
                {
                    case MassSource.SelectedMass:
                        CollectHostMass(selectedMass);
                        break;
                    case MassSource.DisplayAll:
                        CollectHostMass(massElements);
                        CollectLinkedMass();
                        break;
                    case MassSource.OnlyHost:
                        CollectHostMass(massElements);
                        break;
                    case MassSource.OnlyLink:
                        CollectLinkedMass();
                        break;
                }

                FindOverlaps();

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect RevitLinkTypes.\n" + ex.Message, "Form_LinkedFiles:bttnApply_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("bttnApply_Click", ex.Message);
            }
        }

        private void CollectHostMass(List<Element> massElements)
        {
            try
            {
                progressForm = new Form_ProgressBar();
                if (massElements.Count > 0)
                {
                    progressForm.Text = "Finding Elements";
                    progressForm.LabelText = "Finding elements intersecting with 3D mass in the host project . . .";
                    progressForm.LabelCount = massElements.Count.ToString() + " mass found";
                    progressForm.MaxValue = massElements.Count;
                    progressForm.Show();
                    progressFormOpend = true;
                    progressForm.Refresh();
                }

                foreach (Element element in massElements)
                {
                    progressForm.PerformStep();
                    FamilyInstance fi = element as FamilyInstance;
                    if (null != fi)
                    {
                        MassProperties mp = new MassProperties(fi);
                        mp.WorksetId = fi.WorksetId.IntegerValue; //from host project

                        Solid unionSolid = null;
                        Options opt = m_app.Application.Create.NewGeometryOptions();
                        opt.ComputeReferences = true;
                        opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;
                        GeometryElement geomElement = fi.get_Geometry(new Options(opt));
                        foreach (GeometryObject obj in geomElement)
                        {
                            Solid solid = obj as Solid;
                            if (null != solid)
                            {
                                if (solid.Volume > 0)
                                {
                                    if (unionSolid == null) { unionSolid = solid; }
                                    else
                                    {
                                        unionSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, unionSolid, BooleanOperationsType.Union);
                                    }
                                }
                            }
                        }
                        mp.MassSolid = unionSolid;

                        if (null != unionSolid)
                        {
                            if (unionSolid.Volume > 0)
                            {
                                mp.ElementContainer = FindElementsInMass(m_doc, mp.MassSolid, mp.MassId, false);
                                /*
                                foreach (Document linkedDoc in linkedDocuments)
                                {
                                    List<Element> filteredElements = FindElementsInMass(linkedDoc, mp.MassSolid, mp.MassId, true);
                                    mp.ElementContainer.AddRange(filteredElements);
                                }
                                 */
                                mp.ElementCount = mp.ElementContainer.Count;
                            }
                        }

                        if (mp.MassParameters.Count > 0)
                        {
                            foreach (Parameter param in mp.MassParameters)
                            {
                                if (!massParameters.Contains(param.Definition.Name))
                                {
                                    massParameters.Add(param.Definition.Name);
                                }
                            }
                        }

                        mp.IsHost = true;
                        integratedMassList.Add(mp);
                    }
                }

                if (progressFormOpend)
                {
                    progressForm.Close();
                    progressFormOpend = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect mass elements from the host project.\n" + ex.Message, "LinkedModelManager:CollectHostMass", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("CollectHostMass", ex.Message);
            }
        }

        private void CollectLinkedMass()
        {
            try
            {
                List<int> selectedMass = new List<int>();
                foreach (TreeNode typeNode in treeViewLinkedFile.Nodes)
                {
                    foreach (TreeNode instanceNode in typeNode.Nodes)
                    {
                        int instanceId = int.Parse(instanceNode.Name);
                        bool massChecked = false;
                        foreach (TreeNode massNode in instanceNode.Nodes)
                        {
                            if (massNode.Checked)
                            {
                                selectedMass.Add(int.Parse(massNode.Name));
                                massChecked = true;
                            }
                        }
                        if (!massChecked) { linkedMassDictionary.Remove(instanceId); }
                    }
                }

                if (selectedMass.Count > 0)
                {
                    progressForm = new Form_ProgressBar();
                    progressForm.Text = "Finding Elements";
                    progressForm.LabelText = "Finding elements intersecting with 3D mass in linked files . . .";
                    progressForm.LabelCount = selectedMass.Count.ToString() + " mass found";
                    progressForm.MaxValue = selectedMass.Count;
                    progressForm.Show();
                    progressFormOpend = true;
                    progressForm.Refresh();

                    List<int> instanceIds = linkedMassDictionary.Keys.ToList();
                    foreach (int instanceId in instanceIds)
                    {
                        LinkedInstanceProperties lip = linkedMassDictionary[instanceId];
                        Dictionary<int, MassProperties> massDictionary = new Dictionary<int, MassProperties>();
                        foreach (Element massElement in lip.MassElements)
                        {
                            if (!selectedMass.Contains(massElement.Id.IntegerValue)) { continue; }

                            progressForm.PerformStep();
                            FamilyInstance fi = massElement as FamilyInstance;
                            if (null != fi)
                            {
                                MassProperties mp = new MassProperties(fi);

                                Solid unionSolid = null;
                                Options opt = m_app.Application.Create.NewGeometryOptions();
                                opt.ComputeReferences = true;
                                opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;

                                GeometryElement geomElement = fi.get_Geometry(new Options(opt));

                                if (null != lip.TransformValue) { geomElement = geomElement.GetTransformed(lip.TransformValue); }
                                foreach (GeometryObject obj in geomElement)
                                {
                                    Solid solid = obj as Solid;
                                    if (null != solid)
                                    {
                                        if (solid.Volume > 0)
                                        {
                                            if (unionSolid == null) { unionSolid = solid; }
                                            else
                                            {
                                                unionSolid = BooleanOperationsUtils.ExecuteBooleanOperation(unionSolid, solid, BooleanOperationsType.Union);
                                            }
                                        }
                                    }
                                }
                                mp.MassSolid = unionSolid;
                                if (null != unionSolid)
                                {
                                    if (unionSolid.Volume > 0)
                                    {
                                        mp.ElementContainer = FindElementsInMass(m_doc, mp.MassSolid, mp.MassId, false);
                                                                           
                                        foreach (Document linkedDoc in linkedDocuments)
                                        {
                                            List<Element> filteredElements=FindElementsInMass(linkedDoc, mp.MassSolid, mp.MassId, true);
                                            mp.ElementContainer.AddRange(filteredElements);
                                        }
                                        mp.ElementCount = mp.ElementContainer.Count;
                                    }
                                }

                                if (mp.MassParameters.Count > 0)
                                {
                                    foreach (Parameter param in mp.MassParameters)
                                    {
                                        if (!massParameters.Contains(param.Definition.Name))
                                        {
                                            massParameters.Add(param.Definition.Name);
                                        }
                                    }
                                }
                                mp.IsHost = false;
                                mp.LInkedFileName = lip.FileName;

                                if (!massDictionary.ContainsKey(mp.MassId))
                                {
                                    massDictionary.Add(mp.MassId, mp);
                                }
                                integratedMassList.Add(mp);
                            }
                        }
                        lip.MassContainers = massDictionary;
                        linkedMassDictionary.Remove(instanceId);
                        linkedMassDictionary.Add(instanceId, lip);
                    }

                    progressForm.Close();
                    progressFormOpend = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect mass elements from the linked files.\n" + ex.Message, "LinkedModelManager:CollectLinkedMass", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("CollectLinkedMass", ex.Message);
            }
        }

        private List<Element> FindElementsInMass(Document doc, Solid massSolid, int massId, bool linkedElment)
        {
            List<Element> elementList = new List<Element>();
            try
            {
                FilteredElementCollector elementCollector = new FilteredElementCollector(doc);
                elementCollector.WherePasses(new ElementIntersectsSolidFilter(massSolid)).WhereElementIsNotElementType();
                elementList = elementCollector.ToElements().ToList();

                foreach (Element element in elementList)
                {
                    int elementId = element.Id.IntegerValue;
                    if (!elementDictionary.ContainsKey(elementId))
                    {
                        ElementProperties ep = new ElementProperties(element);
                        ep.Doc = doc;
                        ep.LinkedElement = linkedElment;
                        CollectParameterValues(element);

                        FamilyInstance fi = element as FamilyInstance;
                        if (null != fi)
                        {
                            if (null != fi.Host) { ep.HostElementId = fi.Host.Id.IntegerValue; }
                        }

                        Dictionary<int, Solid> massContainers = new Dictionary<int, Solid>();
                        massContainers.Add(massId, massSolid);
                        ep.MassContainers = massContainers;
                        elementDictionary.Add(ep.ElementId, ep);

                        if (!categoryIds.Contains(element.Category.Id.IntegerValue) && null != element.Category.Name)
                        {
                            elementCategories.Add(element.Category, false);
                            categoryIds.Add(element.Category.Id.IntegerValue);
                        }
                    }
                    else
                    {
                        ElementProperties ep = elementDictionary[elementId];
                        if (!ep.MassContainers.ContainsKey(massId))
                        {
                            ep.MassContainers.Add(massId, massSolid);
                            elementDictionary.Remove(elementId);
                            elementDictionary.Add(elementId, ep);
                        }
                    }
                }

                return elementList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find mass elements in mass.\n" + ex.Message, "LinkedModelManager:FindElementsInMass", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("FindElementsInMass", ex.Message);
                return elementList;
            }
        }

        private void CollectParameterValues(Element element)
        {
            try
            {
                if (null != element.Category)
                {
                    Category category = element.Category;
                    Dictionary<int, ParameterProperties> paramDictionary = new Dictionary<int, ParameterProperties>();
                    if (parameterMaps.ContainsKey(category.Id.IntegerValue))
                    {
                        paramDictionary = parameterMaps[category.Id.IntegerValue];
                        foreach (Parameter param in element.Parameters)
                        {
                            if (param.StorageType == StorageType.ElementId)
                            {
                                if (param.Id.IntegerValue != (int)BuiltInParameter.PHASE_CREATED && param.Id.IntegerValue != (int)BuiltInParameter.PHASE_DEMOLISHED) { continue; }
                            }

                            if (paramDictionary.ContainsKey(param.Id.IntegerValue))
                            {
                                paramDictionary[param.Id.IntegerValue].AddValue(param);
                            }
                            else
                            {
                                ParameterProperties pp = new ParameterProperties(param);
                                pp.AddValue(param);
                                paramDictionary.Add(pp.ParamId, pp);
                            }
                        }
                        parameterMaps.Remove(category.Id.IntegerValue);
                    }
                    else
                    {
                        foreach (Parameter param in element.Parameters)
                        {
                            if (param.StorageType == StorageType.ElementId)
                            {
                                if (param.Id.IntegerValue != (int)BuiltInParameter.PHASE_CREATED && param.Id.IntegerValue != (int)BuiltInParameter.PHASE_DEMOLISHED) { continue; }
                            }

                            if (param.Definition.Name.Contains("Extensions.")) { continue; }
                            //ParameterProperties pp = new ParameterProperties();
                            ParameterProperties pp = new ParameterProperties(param);
                            pp.AddValue(param);
                            paramDictionary.Add(pp.ParamId, pp);
                        }
                    }
                    parameterMaps.Add(category.Id.IntegerValue, paramDictionary);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameter information.\n"+ex.Message, "Collect Parameter Values", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //find elements which intersect more than two mass elements. 
        private void FindOverlaps()
        {
            try
            {
                if (elementDictionary.Count > 0)
                {
                    progressForm = new Form_ProgressBar();
                    progressForm.Text = "Finding Overlapping Conditions";
                    progressForm.LabelText = "Finding elements intersecting with more than two mass elements...";
                    progressForm.LabelCount = elementDictionary.Count.ToString() + " elements found";
                    progressForm.MaxValue = elementDictionary.Count;
                    progressForm.Show();
                    progressFormOpend = true;
                    progressForm.Refresh();
                }

                List<int> elementIds = elementDictionary.Keys.ToList();
                foreach (int elementId in elementIds)
                {
                    progressForm.PerformStep();
                    ElementProperties ep = elementDictionary[elementId];
                    if (ep.MassContainers.Count > 1)
                    {
                        Solid unionSolid = null;
                        Options opt = m_app.Application.Create.NewGeometryOptions();
                        opt.ComputeReferences = true;
                        opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;

                        GeometryElement geomElement = ep.ElementObj.get_Geometry(new Options(opt));
                        if (ep.ElementObj is FamilyInstance)
                        {
                            geomElement = geomElement.GetTransformed(Transform.Identity);
                        }
                        
                        foreach (GeometryObject obj in geomElement)
                        {
                            Solid solid = obj as Solid;
                            if (null != solid)
                            {
                                if (solid.Volume > 0)
                                {
                                    if (unionSolid == null) { unionSolid = solid; }
                                    else
                                    {
                                        try { unionSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, unionSolid, BooleanOperationsType.Union); }
                                        catch { }
                                    }
                                }
                            }
                        }
                        ep.ElementSolid = unionSolid;

                        Dictionary<int, double> overlapMap = new Dictionary<int, double>();
                        if (unionSolid != null)
                        {
                            if (unionSolid.Volume > 0)
                            {
                                foreach (int massId in ep.MassContainers.Keys)
                                {
                                    Solid massSolid = ep.MassContainers[massId];
                                    Solid intersectSolid = null;
                                    try { intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(massSolid, unionSolid, BooleanOperationsType.Intersect); }
                                    catch { intersectSolid = null; }
                                    if (null != intersectSolid)
                                    {
                                        if (intersectSolid.Volume > 0)
                                        {
                                            double ratio = intersectSolid.Volume / unionSolid.Volume;
                                            if (!overlapMap.ContainsKey(massId))
                                            {
                                                overlapMap.Add(massId, ratio);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        ep.OpverappingMaps = overlapMap;
                        elementDictionary.Remove(ep.ElementId);
                        elementDictionary.Add(ep.ElementId, ep);
                    }
                }

                if (progressFormOpend)
                {
                    progressForm.Close();
                    progressFormOpend = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find overlapping conditions.\n" + ex.Message, "LinkedModelManager:FindOverlaps", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("FindOverlaps", ex.Message);
            }
        }

        #region eventHandler
        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void radioButtonSelectedMass_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSelectedMass.Checked)
            {
                selectedSourceType = MassSource.SelectedMass;
            }
        }

        private void radioButtonAll_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAll.Checked)
            {
                selectedSourceType = MassSource.DisplayAll;
            }
        }

        private void radioButtonHost_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonHost.Checked)
            {
                selectedSourceType = MassSource.OnlyHost;
            }
        }

        private void radioButtonLink_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonLink.Checked)
            {
                selectedSourceType = MassSource.OnlyLink;
            }
        }

        private void treeViewLinkedFile_AfterCheck(object sender, TreeViewEventArgs e)
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

        private void bttnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode typeNode in treeViewLinkedFile.Nodes)
            {
                typeNode.Checked = true;
                foreach (TreeNode instanceNode in typeNode.Nodes)
                {
                    instanceNode.Checked = true;
                    foreach (TreeNode massNode in instanceNode.Nodes)
                    {
                        massNode.Checked = true;
                    }
                }
            }
        }

        private void bttnCheckNone_Click(object sender, EventArgs e)
        {
            foreach (TreeNode typeNode in treeViewLinkedFile.Nodes)
            {
                typeNode.Checked = false;
                foreach (TreeNode instanceNode in typeNode.Nodes)
                {
                    instanceNode.Checked = false;
                    foreach (TreeNode massNode in instanceNode.Nodes)
                    {
                        massNode.Checked = false;
                    }
                }
            }
        }
        #endregion
    }
}
