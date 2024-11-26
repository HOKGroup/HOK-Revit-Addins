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
using HOK.Core.Utilities;
using static HOK.Core.Utilities.ElementIdExtension;

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
        private Dictionary<long/*instanceId*/, LinkedInstanceProperties> linkedMassDictionary = new Dictionary<long, LinkedInstanceProperties>(); //mass from linked models
        private Dictionary<long/*elementId*/, ElementProperties> elementDictionary = new Dictionary<long, ElementProperties>(); //all intersecting elements
        private Dictionary<Category, bool> elementCategories = new Dictionary<Category, bool>();
        private List<string> massParameters = new List<string>();
        private MassSource selectedSourceType;
        private Form_ProgressBar progressForm = new Form_ProgressBar();
        private bool progressFormOpend = false;
        private List<long> categoryIds = new List<long>();
        private Dictionary<long/*categoryId*/, Dictionary<long, ParameterProperties>> parameterMaps = new Dictionary<long, Dictionary<long, ParameterProperties>>();

        public Dictionary<string, int> WorksetDictionary { get { return worksetDictionary; } set { worksetDictionary = value; } }
        public List<MassProperties> IntegratedMassList { get { return integratedMassList; } set { integratedMassList = value; } }
        public Dictionary<long, LinkedInstanceProperties> LinkedMassDictionary { get { return linkedMassDictionary; } set { linkedMassDictionary = value; } }
        public Dictionary<long, ElementProperties> ElementDictionary { get { return elementDictionary; } set { elementDictionary = value; } }
        public Dictionary<Category, bool> ElementCategories { get { return elementCategories; } set { elementCategories = value; } }
        public List<string> MassParameters { get { return massParameters; } set { massParameters = value; } }
        public MassSource SelectedSourceType { get { return selectedSourceType; } set { selectedSourceType = value; } }
        public Dictionary<long, Dictionary<long, ParameterProperties>> ParameterMaps { get { return parameterMaps; } set { parameterMaps = value; } }
        

        public Form_LinkedFiles(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            selectedSourceType = MassSource.OnlyHost;
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        

        private void DisplayRvtLinkTypes()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType();
                List<RevitLinkInstance> revitLinkInstances = collector.ToElements().Cast<RevitLinkInstance>().ToList();

                Dictionary<long/*typeId*/, RevitLinkType> linkTypes = new Dictionary<long, RevitLinkType>();
                foreach (RevitLinkInstance instance in revitLinkInstances)
                {
                    LinkedInstanceProperties lip = new LinkedInstanceProperties(instance);
                    ElementId typeId = instance.GetTypeId();
                   
                    RevitLinkType linkType = m_doc.GetElement(typeId) as RevitLinkType;
                    lip.FileName = linkType.Name;
                    lip.TypeId = GetElementIdValue(linkType.Id);

                    if (!linkTypes.ContainsKey(GetElementIdValue(linkType.Id)))
                    {
                        linkTypes.Add(GetElementIdValue(linkType.Id), linkType);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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

                    MassProperties mp = new MassProperties(element);
                    mp.WorksetId = element.WorksetId.IntegerValue; //from host project

                    Solid unionSolid = null;
                    Options opt = m_app.Application.Create.NewGeometryOptions();
                    opt.ComputeReferences = true;
                    opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;
                    GeometryElement geomElement = element.get_Geometry(new Options(opt));
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
                            List<Element> linkedElements = FindElementsInLInkedFiles(element, geomElement);
                            mp.ElementContainer.AddRange(linkedElements);
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

                if (progressFormOpend)
                {
                    progressForm.Close();
                    progressFormOpend = false;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void CollectLinkedMass()
        {
            try
            {
                List<long> selectedMass = new List<long>();
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

                    List<long> instanceIds = linkedMassDictionary.Keys.ToList();
                    foreach (long instanceId in instanceIds)
                    {
                        LinkedInstanceProperties lip = linkedMassDictionary[instanceId];
                        Dictionary<long, MassProperties> massDictionary = new Dictionary<long, MassProperties>();
                        foreach (Element massElement in lip.MassElements)
                        {
                            if (!selectedMass.Contains(GetElementIdValue(massElement.Id))) { continue; }

                            progressForm.PerformStep();
                            MassProperties mp = new MassProperties(massElement);
                            Solid unionSolid = null;
                            Options opt = m_app.Application.Create.NewGeometryOptions();
                            opt.ComputeReferences = true;
                            opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;

                            GeometryElement geomElement = massElement.get_Geometry(new Options(opt));

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
                                    List<Element> linkedElements = FindElementsInLInkedFiles(massElement, geomElement);
                                    mp.ElementContainer.AddRange(linkedElements);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private List<Element> FindElementsInMass(Document doc, Solid massSolid, long massId, bool linkedElement)
        {
            List<Element> elementList = new List<Element>();
            try
            {
                FilteredElementCollector elementCollector = new FilteredElementCollector(doc);
                ElementCategoryFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_RvtLinks,true);
                elementCollector.WherePasses(catFilter).WherePasses(new ElementIntersectsSolidFilter(massSolid)).WhereElementIsNotElementType();
                elementList = elementCollector.ToElements().ToList();

                foreach (Element element in elementList)
                {
                    long elementId = GetElementIdValue(element.Id);
                    if (!elementDictionary.ContainsKey(elementId))
                    {
                        ElementProperties ep = new ElementProperties(element);
                        ep.LinkedElement = linkedElement;
                        CollectParameterValues(element);

                        FamilyInstance fi = element as FamilyInstance;
                        if (null != fi)
                        {
                            if (null != fi.Host) { ep.HostElementId = GetElementIdValue(fi.Host.Id); }
                        }

                        Dictionary<long, Solid> massContainers = new Dictionary<long, Solid>();
                        massContainers.Add(massId, massSolid);
                        ep.MassContainers = massContainers;
                        elementDictionary.Add(ep.ElementId, ep);

                        if (!categoryIds.Contains(GetElementIdValue(element.Category.Id)) && null != element.Category.Name)
                        {
                            elementCategories.Add(element.Category, false);
                            categoryIds.Add(GetElementIdValue(element.Category.Id));
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return elementList;
            }
        }

        private List<Element> FindElementsInLInkedFiles(Element massInstance, GeometryElement geomElement)
        {
            List<Element> foundElements = new List<Element>();
            try
            {
                Options opt = m_app.Application.Create.NewGeometryOptions();
                opt.ComputeReferences = true;
                opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType();
                List<RevitLinkInstance> revitLinkInstances = collector.ToElements().Cast<RevitLinkInstance>().ToList();

                foreach (RevitLinkInstance linkInstance in revitLinkInstances)
                {
                    ElementId typeId = linkInstance.GetTypeId();
                    RevitLinkType linkType = m_doc.GetElement(typeId) as RevitLinkType;
                    string linkDocName = linkType.Name;

                    Document linkedDoc = null;
                    foreach (Document document in m_app.Application.Documents)
                    {
                        if (linkDocName.Contains(document.Title))
                        {
                            linkedDoc = document; break;
                        }
                    }

                    Transform transformValue = linkInstance.GetTotalTransform();

                    if (null != linkedDoc && null != transformValue)
                    {
                        GeometryElement transformedElement = geomElement.GetTransformed(transformValue.Inverse);
                        Solid originalSolid = GetSolid(geomElement);
                        Solid transformedSolid = GetSolid(transformedElement);

                        if (null != transformedSolid)
                        {
                            FilteredElementCollector elementCollector = new FilteredElementCollector(linkedDoc);
                            elementCollector.WherePasses(new ElementIntersectsSolidFilter(transformedSolid)).WhereElementIsNotElementType();
                            List<Element> elementList = elementCollector.ToElements().ToList();

                            foreach (Element element in elementList)
                            {
                                long elementId = GetElementIdValue(element.Id);
                                if (!elementDictionary.ContainsKey(elementId))
                                {
                                    ElementProperties ep = new ElementProperties(element);
                                    ep.Doc = linkedDoc;
                                    ep.TransformValue = transformValue;
                                    ep.LinkedElement = true;

                                    CollectParameterValues(element);

                                    FamilyInstance fi = element as FamilyInstance;
                                    if (null != fi)
                                    {
                                        if (null != fi.Host) { ep.HostElementId = GetElementIdValue(fi.Host.Id); }
                                    }

                                    Dictionary<long, Solid> massContainers = new Dictionary<long, Solid>();
                                    massContainers.Add(GetElementIdValue(massInstance.Id), originalSolid); //adjusted to the host coordinate system
                                    ep.MassContainers = massContainers;
                                    elementDictionary.Add(ep.ElementId, ep);

                                    if (!categoryIds.Contains(GetElementIdValue(element.Category.Id)) && null != element.Category.Name)
                                    {
                                        elementCategories.Add(element.Category, false);
                                        categoryIds.Add(GetElementIdValue(element.Category.Id));
                                    }
                                }
                                else
                                {
                                    ElementProperties ep = elementDictionary[elementId];
                                    if (!ep.MassContainers.ContainsKey(GetElementIdValue(massInstance.Id)))
                                    {
                                        ep.MassContainers.Add(GetElementIdValue(massInstance.Id), originalSolid);
                                        elementDictionary.Remove(elementId);
                                        elementDictionary.Add(elementId, ep);
                                    }
                                }
                            }
                            foundElements.AddRange(elementList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find elements in mass.\n"+ex.Message, "FindElementsInMass", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return foundElements;
        }

        private Solid GetSolid(GeometryElement geoElement)
        {
            Solid unionSolid = null;
            try
            {
                Options opt = m_app.Application.Create.NewGeometryOptions();
                opt.ComputeReferences = true;
                opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;

                foreach (GeometryObject obj in geoElement)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get solid.\n"+ex.Message, "Get Solid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return unionSolid;
        }

        private void CollectParameterValues(Element element)
        {
            try
            {
                if (null != element.Category)
                {
                    Category category = element.Category;
                    Dictionary<long, ParameterProperties> paramDictionary = new Dictionary<long, ParameterProperties>();
                    if (parameterMaps.ContainsKey(GetElementIdValue(category.Id)))
                    {
                        paramDictionary = parameterMaps[GetElementIdValue(category.Id)];
                        foreach (Parameter param in element.Parameters)
                        {
                            if (param.StorageType == StorageType.ElementId)
                            {
                                if (GetElementIdValue(param.Id) != (int)BuiltInParameter.PHASE_CREATED && GetElementIdValue(param.Id) != (int)BuiltInParameter.PHASE_DEMOLISHED) { continue; }
                            }

                            if (paramDictionary.ContainsKey(GetElementIdValue(param.Id)))
                            {
                                //paramDictionary[GetElementIdValue(param.Id)].AddValue(param);
                                paramDictionary.Remove(GetElementIdValue(param.Id));
                            }
                            ParameterProperties pp = new ParameterProperties(param);
                            pp.AddValue(param);
                            paramDictionary.Add(pp.ParamId, pp);
                        }
                        parameterMaps.Remove(GetElementIdValue(category.Id));
                    }
                    else
                    {
                        foreach (Parameter param in element.Parameters)
                        {
                            if (param.StorageType == StorageType.ElementId)
                            {
                                if (GetElementIdValue(param.Id) != (int)BuiltInParameter.PHASE_CREATED && GetElementIdValue(param.Id) != (int)BuiltInParameter.PHASE_DEMOLISHED) { continue; }
                            }

                            if (param.Definition.Name.Contains("Extensions.")) { continue; }
                            if (!paramDictionary.ContainsKey(GetElementIdValue(param.Id)))
                            {
                                ParameterProperties pp = new ParameterProperties(param);
                                pp.AddValue(param);
                                paramDictionary.Add(pp.ParamId, pp);
                            }
                        }
                    }
                    parameterMaps.Add(GetElementIdValue(category.Id), paramDictionary);
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

                List<long> elementIds = elementDictionary.Keys.ToList();
                foreach (long elementId in elementIds)
                {
                    progressForm.PerformStep();
                    ElementProperties ep = elementDictionary[elementId];
                    if (ep.MassContainers.Count > 0)
                    {
                        Solid unionSolid = null;
                        Options opt = m_app.Application.Create.NewGeometryOptions();
                        opt.ComputeReferences = true;
                        opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;

                        GeometryElement geomElement = ep.ElementObj.get_Geometry(new Options(opt));
                        geomElement = geomElement.GetTransformed(ep.TransformValue);
                      
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

                        Dictionary<long, double> overlapMap = new Dictionary<long, double>();
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
