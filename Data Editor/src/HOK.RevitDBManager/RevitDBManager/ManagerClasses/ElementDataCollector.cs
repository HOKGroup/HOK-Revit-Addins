using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using System.Windows.Forms;
using RevitDBManager.GenericForms;

namespace RevitDBManager.Classes
{
    public class ElementDataCollector
    {
        private Document doc;
        private Dictionary<string/*category*/, Dictionary<int/*typeID*/, ElementTypeProperties>> elementDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>(); //entire project elements
        private Dictionary<string/*category*/, Dictionary<int/*typeID*/, TypeProperties>> typeDictionary = new Dictionary<string, Dictionary<int, TypeProperties>>(); //selected Family Symbols
        private Dictionary<string/*category*/, Dictionary<int/*instanceID*/, InstanceProperties>> instanceDictionary = new Dictionary<string, Dictionary<int, InstanceProperties>>();//selected Family Instances
        private Dictionary<string, Dictionary<int, ElementTypeProperties>> sysTypeDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>(); //selected system element types
        private Dictionary<string, Dictionary<int, ElementProperties>> sysInstDictionary = new Dictionary<string, Dictionary<int, ElementProperties>>();//selected system elements
        private Dictionary<int, ViewTypeProperties> viewTypeDictionary = new Dictionary<int, ViewTypeProperties>();//selected view family types
        private Dictionary<int, ViewProperties> viewInstDictionary = new Dictionary<int, ViewProperties>();//selected views

        private Dictionary<int/*paramID*/, ParamProperties> paramDictionary = new Dictionary<int, ParamProperties>();
        private Dictionary<string/*category*/, List<Family>/*families*/> selectedFamilies = new Dictionary<string, List<Family>>();
        private Dictionary<int/*roomID*/, RoomProperties> roomDictionary = new Dictionary<int, RoomProperties>();
        private Dictionary<int/*spaceID*/, SpaceProperties> spaceDictionary = new Dictionary<int, SpaceProperties>();
        private Dictionary<int/*areaID*/, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();

        private Dictionary<string/*ViewFamily*/, Dictionary<int/*typeID*/, ViewFamilyType>> viewFamilyTypes = new Dictionary<string, Dictionary<int, ViewFamilyType>>();//for tree view display

        private List<FamilySymbol> selectedSymbols = new List<FamilySymbol>();
        private string[] excludeViewFamily = new string[] { "Invalid", "Schedule", "Walkthrough", "ImageView", "CostReport", "LoadsReport", "PressureLossReport", "PanelSchedule", "GraphicalColumnSchedule", "StructuralPlan" };
        private ToolStripProgressBar progressBar;

        public ToolStripProgressBar ProgressBar { get { return progressBar; } set { progressBar = value; } }
        public TempStorage tempStorage = new TempStorage();

        public ElementDataCollector(Document document)
        {
            doc = document;

            CollectFamilySymbols1();
            CollectSystemFamilies(); //system families
            CollectRoomData();
            CollectSpaceData();
            CollectAreaData();
            CollectViewTypeData();
        }

        private void CollectFamilySymbols1()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<ElementFilter> filterList = new List<ElementFilter>();
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Ceilings));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_CurtainWallPanels));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Doors));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Floors));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Furniture));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_FurnitureSystems));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_GenericModel));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Mass));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_SpecialityEquipment));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Stairs));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Windows));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Walls));

                LogicalOrFilter orFilter = new LogicalOrFilter(filterList);
                List<Element> famSymbols = collector.OfClass(typeof(FamilySymbol)).WherePasses(orFilter).ToList<Element>();

                if (famSymbols.Count > 0)
                {
                    foreach (Element element in famSymbols)
                    {
                        ElementType elementType = element as ElementType;
                        if (null != elementType)
                        {
                            ElementTypeProperties etp = new ElementTypeProperties(elementType);
                            string categoryName = etp.CategoryName;
                            if (elementDictionary.ContainsKey(categoryName))
                            {
                                elementDictionary[categoryName].Add(etp.TypeID, etp);
                            }
                            else
                            {
                                elementDictionary.Add(categoryName, new Dictionary<int, ElementTypeProperties>());
                                elementDictionary[categoryName].Add(etp.TypeID, etp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect FamilySymbols: \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectFamilySymbols2()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<ElementFilter> filterList = new List<ElementFilter>();
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_DetailComponents));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Fluids));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_ModelText));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_GenericAnnotation));
                LogicalOrFilter orFilter = new LogicalOrFilter(filterList);
                ICollection<ElementId> excludes = collector.OfClass(typeof(ElementType)).WherePasses(orFilter).ToElementIds();

                ExclusionFilter excFilter = new ExclusionFilter(excludes);

                collector = new FilteredElementCollector(doc);
                collector = collector.OfClass(typeof(FamilySymbol)).WherePasses(excFilter);
                var query = from element in collector
                            where !element.Category.Name.Contains("Tag") && element.Parameters.Size > 0
                            orderby element.Category.Name
                            select element;

                List<Element> famSymbols = query.ToList<Element>();

                if (famSymbols.Count > 0)
                {
                    foreach (Element element in famSymbols)
                    {
                        ElementType elementType = element as ElementType;
                        if (null != elementType)
                        {
                            ElementTypeProperties etp = new ElementTypeProperties(elementType);
                            string categoryName = etp.CategoryName;
                            if (elementDictionary.ContainsKey(categoryName))
                            {
                                elementDictionary[categoryName].Add(etp.TypeID, etp);
                            }
                            else
                            {
                                elementDictionary.Add(categoryName, new Dictionary<int, ElementTypeProperties>());
                                elementDictionary[categoryName].Add(etp.TypeID, etp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect FamilySymbols: \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectSystemFamilies()
        {
            string typeName = "";
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<ElementFilter> filterList = new List<ElementFilter>();
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Ceilings));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Floors));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_GenericModel));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_StairsRailing));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Ramps));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Roofs));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Stairs));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation));
                filterList.Add(new ElementCategoryFilter(BuiltInCategory.OST_Walls));
               
                LogicalOrFilter orFilter = new LogicalOrFilter(filterList);
                List<Element> elementTypes = collector.OfClass(typeof(ElementType)).WherePasses(orFilter).ToList<Element>();

                if (elementTypes.Count > 0)
                {
                    foreach (Element element in elementTypes)
                    {
                        ElementType elementType = element as ElementType;
                        typeName = elementType.Name;

                        if (null != elementType)
                        {
                            ElementTypeProperties etp = new ElementTypeProperties(elementType);
                            string categoryName = etp.CategoryName;

                            if (elementDictionary.ContainsKey(categoryName))
                            {
                                if (!elementDictionary[categoryName].ContainsKey(etp.TypeID))
                                {
                                    elementDictionary[categoryName].Add(etp.TypeID, etp);
                                }
                            }
                            else
                            {
                                elementDictionary.Add(categoryName, new Dictionary<int, ElementTypeProperties>());
                                elementDictionary[categoryName].Add(etp.TypeID, etp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect SystemFamilies: "+typeName+"\n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectRoomData()
        {
            try
            {
                ElementCategoryFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> roomElements = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().ToList();

                roomDictionary = new Dictionary<int, RoomProperties>();
                //string[] roomFields = new string[] { "Name", "Number", "Level", "Area", "Base Offset", "Limit Offset", "Unbounded Height", "Upper Limit", "Volume" };
                foreach (Element element in roomElements)
                {
                    Room aroom = element as Room;
                    if (aroom != null)
                    {
                        RoomProperties rp = new RoomProperties(aroom, doc);
                        AddParameterInfo(rp.RoomParameters);
                        roomDictionary.Add(rp.RoomID, rp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect Room data: \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectSpaceData()
        {
            try
            {
                ElementCategoryFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_MEPSpaces);
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> spaceElements = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().ToList();

                spaceDictionary = new Dictionary<int, SpaceProperties>();
                foreach (Element element in spaceElements)
                {
                    Space space = element as Space;
                    if (null != space)
                    {
                        SpaceProperties sp = new SpaceProperties(space, doc);
                        AddParameterInfo(sp.SpaceParameters);
                        spaceDictionary.Add(sp.SpaceID, sp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect Space data: \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectAreaData()
        {
            try
            {
                ElementCategoryFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Areas);
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> areaElements = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().ToList();

                areaDictionary = new Dictionary<int, AreaProperties>();
                foreach (Element element in areaElements)
                {
                    Area area = element as Area;
                    if (null != area)
                    {
                        AreaProperties ap = new AreaProperties(area, doc);
                        AddParameterInfo(ap.AreaParameters);
                        areaDictionary.Add(ap.AreaID, ap);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect area data: \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectViewTypeData()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<ViewFamilyType> viewTypeElements = collector.OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().ToList();

                foreach (ViewFamilyType viewType in viewTypeElements)
                {
                    string viewFamily = viewType.ViewFamily.ToString();
                    if (excludeViewFamily.Contains(viewFamily)) { continue; }

                    if (viewFamilyTypes.ContainsKey(viewFamily))
                    {
                        viewFamilyTypes[viewFamily].Add(viewType.Id.IntegerValue, viewType);
                    }
                    else
                    {
                        viewFamilyTypes.Add(viewFamily, new Dictionary<int, ViewFamilyType>());
                        viewFamilyTypes[viewFamily].Add(viewType.Id.IntegerValue, viewType);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect area data: \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void CollectSelectedElementsData(List<ElementType> selTypes)
        {
            try
            {
                typeDictionary = new Dictionary<string, Dictionary<int, TypeProperties>>();
                instanceDictionary = new Dictionary<string, Dictionary<int, InstanceProperties>>();
                sysTypeDictionary = new Dictionary<string, Dictionary<int, ElementTypeProperties>>();
                sysInstDictionary = new Dictionary<string, Dictionary<int, ElementProperties>>();
                viewTypeDictionary = new Dictionary<int, ViewTypeProperties>();
                viewInstDictionary = new Dictionary<int, ViewProperties>();

                if (selTypes.Count > 0)
                {
                    foreach (ElementType atype in selTypes)
                    {
                        progressBar.PerformStep();
                        FamilySymbol famSymbol = atype as FamilySymbol;
                        ViewFamilyType viewFamilyType = atype as ViewFamilyType;
                        if (null != famSymbol)
                        {
                            TypeProperties tp = new TypeProperties(famSymbol);
                            AddParameterInfo(tp.TypeParameters);

                            if (typeDictionary.ContainsKey(tp.CategoryName))
                            {
                                typeDictionary[tp.CategoryName].Add(tp.TypeID, tp);
                            }
                            else
                            {
                                typeDictionary.Add(tp.CategoryName, new Dictionary<int, TypeProperties>());
                                typeDictionary[tp.CategoryName].Add(tp.TypeID, tp);
                            }
                            StoreDataOfFamilySymbol(famSymbol);
                        }
                        else if (null != viewFamilyType)
                        {
                            ViewTypeProperties vtp = new ViewTypeProperties(viewFamilyType);
                            AddParameterInfo(vtp.ViewTypeParameters);

                            if (!viewTypeDictionary.ContainsKey(vtp.ViewTypeID))
                            {
                                viewTypeDictionary.Add(vtp.ViewTypeID, vtp);
                                StoreDataOfViewType(viewFamilyType);
                            }
                        }
                        else //system family
                        {
                            ElementTypeProperties etp = new ElementTypeProperties(atype);
                            AddParameterInfo(etp.ElementTypeParameters);

                            if (sysTypeDictionary.ContainsKey(etp.CategoryName))
                            {
                                sysTypeDictionary[etp.CategoryName].Add(etp.TypeID, etp);
                            }
                            else
                            {
                                sysTypeDictionary.Add(etp.CategoryName, new Dictionary<int, ElementTypeProperties>());
                                sysTypeDictionary[etp.CategoryName].Add(etp.TypeID, etp);
                            }
                            StoreDataOfElementType(atype);
                        }
                    }
                }

                CollectFamilyNames();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect selected Elements Data: \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StoreDataOfFamilySymbol(FamilySymbol familySymbol)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                FamilyInstanceFilter instFilter = new FamilyInstanceFilter(doc, familySymbol.Id);
                List<Element> instances = collector.WherePasses(instFilter).ToList<Element>();

                if (instances.Count > 0)
                {
                    foreach (Element instance in instances)
                    {
                        FamilyInstance fi = (FamilyInstance)instance;
                        InstanceProperties ip = new InstanceProperties(fi);
                        AddParameterInfo(ip.InstParameters);

                        if (instanceDictionary.ContainsKey(ip.CategoryName))
                        {
                            instanceDictionary[ip.CategoryName].Add(ip.InstanceID, ip);
                        }
                        else
                        {
                            instanceDictionary.Add(ip.CategoryName, new Dictionary<int, InstanceProperties>());
                            instanceDictionary[ip.CategoryName].Add(ip.InstanceID, ip);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect selected Element Types: "+familySymbol.Name+" \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StoreDataOfElementType(ElementType elementType)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ElementCategoryFilter catFilter = new ElementCategoryFilter(elementType.Category.Id);
                collector = collector.WherePasses(catFilter).WhereElementIsNotElementType();

                var query = from element in collector
                            where element.GetTypeId() == elementType.Id
                            select element;

                List<Element> elements = query.ToList<Element>();

                if (elements.Count > 0)
                {
                    foreach (Element elem in elements)
                    {
                        ElementProperties ep = new ElementProperties(elem);
                        AddParameterInfo(ep.ElementParameters);

                        if (sysInstDictionary.ContainsKey(ep.CategoryName))
                        {
                            sysInstDictionary[ep.CategoryName].Add(ep.ElemntID,ep);
                        }
                        else
                        {
                            sysInstDictionary.Add(ep.CategoryName, new Dictionary<int, ElementProperties>());
                            sysInstDictionary[ep.CategoryName].Add(ep.ElemntID, ep);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect selected Element Types: " + elementType.Name + " \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StoreDataOfViewType(ViewFamilyType viewType)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ElementCategoryFilter viewfilter = new ElementCategoryFilter(BuiltInCategory.OST_Views);
                ElementCategoryFilter sheetfilter = new ElementCategoryFilter(BuiltInCategory.OST_Sheets);
                LogicalOrFilter orfilter = new LogicalOrFilter(viewfilter, sheetfilter);
                collector = collector.WherePasses(orfilter).WhereElementIsNotElementType();

                var query = from element in collector
                            where element.GetTypeId() == viewType.Id
                            select element;

                List<Element> elements = query.ToList<Element>();

                if (elements.Count > 0)
                {
                    foreach (Element elem in elements)
                    {
                        Autodesk.Revit.DB.View view = elem as Autodesk.Revit.DB.View;
                        ViewProperties vp = new ViewProperties(view, doc);
                        AddParameterInfo(vp.ViewParameters);

                        if (!viewInstDictionary.ContainsKey(vp.ViewID))
                        {
                            viewInstDictionary.Add(vp.ViewID, vp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect selected view Types: " + viewType.Name + " \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectFamilyNames()
        {
            try
            {
                selectedFamilies = new Dictionary<string, List<Family>>();

                List<int> familyIds = new List<int>();
                foreach (string category in typeDictionary.Keys)
                {
                    foreach (int typeId in typeDictionary[category].Keys)
                    {
                        TypeProperties tp = typeDictionary[category][typeId];
                        Family family = tp.FamilySymbolObject.Family;
                        if (familyIds.Contains(family.Id.IntegerValue)) { continue; }
                        familyIds.Add(family.Id.IntegerValue);

                        if (selectedFamilies.ContainsKey(category))
                        {
                            selectedFamilies[category].Add(family);
                        }
                        else
                        {
                            selectedFamilies.Add(category, new List<Family>());
                            selectedFamilies[category].Add(family);
                        }
                    }
                }
                foreach (string category in sysTypeDictionary.Keys)
                {
                    if (!selectedFamilies.ContainsKey(category))
                    {
                        selectedFamilies.Add(category, new List<Family>());
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect family names. \n" + ex.Message, "ElementDataCollector Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddParameterInfo(Dictionary<int,ParamProperties> paramProperties)
        {
            foreach (int pId in paramProperties.Keys)
            {
                if (!paramDictionary.ContainsKey(pId))
                {
                    paramDictionary.Add(pId,paramProperties[pId]);
                }
            }
        }

        public Dictionary<string, Dictionary<int, ElementTypeProperties>> GetElementDictionary()
        {
            return elementDictionary;
        }

        public Dictionary<string,Dictionary<int, TypeProperties>> GetTypeDictionary()
        {
            return typeDictionary;
        }

        public Dictionary<string,Dictionary<int, InstanceProperties>> GetInstanceDictionary()
        {
            return instanceDictionary;
        }

        public Dictionary<string, Dictionary<int, ElementTypeProperties>> GetSysTypeDictionary()
        {
            return sysTypeDictionary;
        }

        public Dictionary<string, Dictionary<int, ElementProperties>> GetSysInstDictionary()
        {
            return sysInstDictionary;
        }

        public Dictionary<int, RoomProperties> GetRoomDictionary()
        {
            return roomDictionary;
        }

        public Dictionary<int, SpaceProperties> GetSpaceDictionary()
        {
            return spaceDictionary;
        }

        public Dictionary<int, AreaProperties> GetAreaDictionary()
        {
            return areaDictionary;
        }

        public Dictionary<string, Dictionary<int, ViewFamilyType>> GetViewFamilyTypes()
        {
            return viewFamilyTypes;
        }

        public Dictionary<int, ViewTypeProperties> GetViewTypeDictionary()
        {
            return viewTypeDictionary;
        }

        public Dictionary<int, ViewProperties> GetViewInstDictionary()
        {
            return viewInstDictionary;
        }

        public Dictionary<int, ParamProperties> GetParameterInfo()
        {
            return paramDictionary;
        }

        public Dictionary<string, List<Family>> GetSelectedFamilies()
        {
            return selectedFamilies;
        }

    }
}
