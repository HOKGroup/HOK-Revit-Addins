using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;

namespace HOK.RoomUpdater
{
    /// <summary>
    /// Interaction logic for RoomUpdaterWindow.xaml
    /// </summary>
    public partial class RoomUpdaterWindow : Window
    {
        private UIApplication m_app;
        private Document m_doc;
        private Dictionary<string/*categoryName*/, Dictionary<string/*paramName*/, ParameterProperties>> projectParameters = new Dictionary<string,Dictionary<string,ParameterProperties>>();
        private List<ParameterMapProperties> parameterMapList = new List<ParameterMapProperties>();
        private StringBuilder errorMessages = new StringBuilder();
        private bool groupExist = false;

        public RoomUpdaterWindow(UIApplication app)
        {
            m_app = app;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            this.Title = "Room Updater v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            projectParameters = GetProjectParameters();
            DisplayCategories();
            parameterMapList = RoomUpdaterDataStorageUtil.GetParameterMaps(m_doc);
            if (parameterMapList.Count > 0)
            {
                dataGridParamMap.ItemsSource = parameterMapList;
            }
        }

        private void DisplayCategories()
        {
            try
            {
                Categories categories = m_doc.Settings.Categories;
                List<CategoryProperties> spaceCategoryList = new List<CategoryProperties>();
                BuiltInCategory bltRoom= BuiltInCategory.OST_Rooms;
                Category roomCat = categories.get_Item(bltRoom);
                if (null != roomCat)
                {
                    CategoryProperties cp = new CategoryProperties(roomCat);
                    cp.ParamDictionary = GetParameterProperties(roomCat);
                    if (cp.ParamDictionary.Count > 0)
                    {
                        spaceCategoryList.Add(cp);
                    }
                    
                }

                BuiltInCategory bltSpace = BuiltInCategory.OST_MEPSpaces;
                Category spaceCat = categories.get_Item(bltSpace);
                if (null != spaceCat)
                {
                    CategoryProperties cp = new CategoryProperties(spaceCat);
                    cp.ParamDictionary = GetParameterProperties(spaceCat);
                    if (cp.ParamDictionary.Count > 0)
                    {
                        spaceCategoryList.Add(cp);
                    }
                }

                comboBoxSpace.ItemsSource = spaceCategoryList;
                comboBoxSpace.DisplayMemberPath = "CategoryName";
                comboBoxSpace.SelectedIndex = 0;

                List<CategoryProperties> elementCategoryList = new List<CategoryProperties>();
                List<ElementId> filterCatIds = ParameterFilterUtilities.GetAllFilterableCategories().ToList();
                foreach (ElementId id in filterCatIds)
                {
                    try
                    {
                        BuiltInCategory bltCat = (BuiltInCategory)id.IntegerValue;
                        Category category = categories.get_Item(bltCat);
                        if (null != category)
                        {
                            if (null != category.Parent) { continue; } //skip subcategories
                            if (bltCat == BuiltInCategory.OST_Rooms || bltCat == BuiltInCategory.OST_MEPSpaces) { continue; }

                            if (category.HasMaterialQuantities)
                            {
#if RELEASE2015
                                if (category.CategoryType == CategoryType.Model)
                                {
                                    CategoryProperties cp = new CategoryProperties(category);
                                    cp.ParamDictionary = GetParameterProperties(category);
                                    if (cp.ParamDictionary.Count > 0)
                                    {
                                        elementCategoryList.Add(cp);
                                    }
                                }
#else
                                CategoryProperties cp = new CategoryProperties(category);
                                cp.ParamDictionary = GetParameterProperties(category);
                                if (cp.ParamDictionary.Count > 0)
                                {
                                    elementCategoryList.Add(cp);
                                }
                                
#endif
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }

                elementCategoryList = elementCategoryList.OrderBy(o => o.CategoryName).ToList();
                comboBoxRevit.ItemsSource = elementCategoryList;
                comboBoxRevit.DisplayMemberPath = "CategoryName";
                comboBoxRevit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display categories.\n"+ex.Message, "Display Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Dictionary<string, ParameterProperties> GetParameterProperties(Category category)
        {
            Dictionary<string, ParameterProperties> paramDictionary = new Dictionary<string, ParameterProperties>();
            try
            {
                Element firstElement = null;
                ElementType firstTypeElement = null;
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Element> elements = collector.OfCategoryId(category.Id).WhereElementIsNotElementType().ToElements().ToList();
               
                if (elements.Count > 0)
                {
                    firstElement = elements.First();
                    firstTypeElement = m_doc.GetElement(firstElement.GetTypeId()) as ElementType;
                }
                else
                {
                    return paramDictionary;
                }
                
                List<ElementId> categoryIds=new List<ElementId>();
                categoryIds.Add(category.Id);

                //add builtIn Parameter
                ICollection<ElementId> supportedParams = ParameterFilterUtilities.GetFilterableParametersInCommon(m_doc, categoryIds);
                foreach (ElementId eId in supportedParams)
                {
                    if (eId.IntegerValue < 0)
                    {
                        BuiltInParameter bltParam = (BuiltInParameter)eId.IntegerValue;
                        string paramName = LabelUtils.GetLabelFor(bltParam);

                        if (paramName.Contains("Extensions.")) { continue; }

                        ParameterProperties pp = null;
#if RELEASE2013 ||RELEASE2014
                        Parameter param = firstElement.get_Parameter(paramName);
#elif RELEASE2015
                        Parameter param = firstElement.LookupParameter(paramName);
#endif
                        
                        if (null != param)
                        {
                            pp = new ParameterProperties(param);
                            pp.IsInstance = true;
                            pp.BltParameter = bltParam;
                            if (!paramDictionary.ContainsKey(pp.ParameterName)) { paramDictionary.Add(pp.ParameterName, pp); }
                        }
                        else if (null != firstTypeElement)
                        {
#if RELEASE2013 ||RELEASE2014
                            param = firstTypeElement.get_Parameter(paramName);
#elif RELEASE2015
                            param = firstTypeElement.LookupParameter(paramName);
#endif

                            if (null != param)
                            {
                                pp = new ParameterProperties(param);
                                pp.IsInstance = false;
                                pp.BltParameter = bltParam;
                                if (!paramDictionary.ContainsKey(pp.ParameterName)) { paramDictionary.Add(pp.ParameterName, pp); }
                            }
                        }                      
                    }
                }
                //add project parameter
                if (projectParameters.ContainsKey(category.Name))
                {
                    Dictionary<string, ParameterProperties> dictionary = projectParameters[category.Name];
                    foreach (string paramName in dictionary.Keys)
                    {
                        if (paramName.Contains("Extensions.")) { continue; }

                        ParameterProperties property = dictionary[paramName];
                        if (property.IsInstance && null != firstTypeElement)
                        {
#if RELEASE2013||RELEASE2014
                            Parameter param = firstElement.get_Parameter(property.ParameterName);
#elif RELEASE2015
                             Parameter param = firstElement.LookupParameter(property.ParameterName);
#endif

                            if (null != param)
                            {
                                ParameterProperties pp = new ParameterProperties(param);
                                pp.IsInstance = true;
                                
                                if (!paramDictionary.ContainsKey(pp.ParameterName)) { paramDictionary.Add(pp.ParameterName, pp); }
                            }
                        }
                        else if (null != firstTypeElement)
                        {
#if RELEASE2013||RELEASE2014
                            Parameter param = firstTypeElement.get_Parameter(property.ParameterName);
#elif RELEASE2015
                            Parameter param = firstTypeElement.LookupParameter(property.ParameterName);
#endif

                            if (null != param)
                            {
                                ParameterProperties pp = new ParameterProperties(param);
                                pp.IsInstance = false;
                                if (!paramDictionary.ContainsKey(pp.ParameterName)) { paramDictionary.Add(pp.ParameterName, pp); }
                            }
                        }
                    }
                }

                //add family parameter
                //sorting
                collector=new FilteredElementCollector(m_doc);
                ICollection<ElementId> familyInstanceIds = collector.OfCategoryId(category.Id).OfClass(typeof(FamilyInstance)).ToElementIds();
                collector=new FilteredElementCollector(m_doc);
                List<FamilySymbol> familySymbols = collector.OfCategoryId(category.Id).OfClass(typeof(FamilySymbol)).ToElements().Cast<FamilySymbol>().ToList();
                List<string> familyNames = new List<string>();

                if (familyInstanceIds.Count > 0)
                {
                    foreach (FamilySymbol symbol in familySymbols)
                    {
                        string familyName = symbol.Family.Name;
                        if (!string.IsNullOrEmpty(familyName))
                        {
                            if (!familyNames.Contains(familyName))
                            {
                                FamilyInstanceFilter instanceFilter = new FamilyInstanceFilter(m_doc, symbol.Id);
                                collector = new FilteredElementCollector(m_doc, familyInstanceIds);
                                ICollection<Element> familyInstances = collector.WherePasses(instanceFilter).ToElements();
                                if (familyInstances.Count > 0)
                                {
                                    FamilyInstance firstFamilyInstance = familyInstances.First() as FamilyInstance;
                                    foreach (Parameter param in firstFamilyInstance.Parameters)
                                    {
                                        if (param.Id.IntegerValue > 0)
                                        {
                                            ParameterProperties pp = new ParameterProperties(param);
                                            pp.IsInstance = true;
                                            if (!paramDictionary.ContainsKey(pp.ParameterName) && !pp.ParameterName.Contains("Extensions.")) { paramDictionary.Add(pp.ParameterName, pp); }
                                        }
                                    }

                                    FamilySymbol firstFamilySymbol = firstFamilyInstance.Symbol;
                                    foreach (Parameter param in firstFamilySymbol.Parameters)
                                    {
                                        if (param.Id.IntegerValue > 0)
                                        {
                                            ParameterProperties pp = new ParameterProperties(param);
                                            pp.IsInstance = false;
                                            if (!paramDictionary.ContainsKey(pp.ParameterName) && !pp.ParameterName.Contains("Extensions.")) { paramDictionary.Add(pp.ParameterName, pp); }
                                        }
                                    }
                                }
                                familyNames.Add(familyName);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter properties.\n"+ex.Message, "Get Parameter Properties", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return paramDictionary;
        }

        private Dictionary<string, Dictionary<string, ParameterProperties>> GetProjectParameters()
        {
            Dictionary<string, Dictionary<string, ParameterProperties>> paramDictionary = new Dictionary<string, Dictionary<string, ParameterProperties>>();
            try
            {
                BindingMap bindingMap = m_doc.ParameterBindings;
                DefinitionBindingMapIterator iterator = bindingMap.ForwardIterator();
                while (iterator.MoveNext())
                {
                    Definition definition = iterator.Key as Definition;
                    string paramName = definition.Name;
                    ElementBinding binding = iterator.Current as ElementBinding;

                    ParameterProperties pp = new ParameterProperties();
                    pp.ParameterName = paramName;
                    if (binding is InstanceBinding)
                    {
                        pp.IsInstance = true;
                    }
                    else if (binding is TypeBinding)
                    {
                        pp.IsInstance = false;
                    }
                    foreach (Category category in binding.Categories)
                    {
                        if (!string.IsNullOrEmpty(category.Name))
                        {
                            if (!paramDictionary.ContainsKey(category.Name))
                            {
                                Dictionary<string, ParameterProperties> dictionary = new Dictionary<string, ParameterProperties>();
                                dictionary.Add(pp.ParameterName, pp);
                                paramDictionary.Add(category.Name, dictionary);
                            }
                            else
                            {
                                if (!paramDictionary[category.Name].ContainsKey(pp.ParameterName))
                                {
                                    paramDictionary[category.Name].Add(pp.ParameterName, pp);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get project parameters\n"+ex.Message, "Get Project Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return paramDictionary;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void comboBoxSpace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxSpace.SelectedItem)
                {
                    CategoryProperties cp = (CategoryProperties)comboBoxSpace.SelectedItem;
                    List<ParameterProperties> paramList = cp.ParamDictionary.Values.ToList();
                    paramList = paramList.OrderBy(o => o.ParameterName).ToList();
                    dataGridSpatial.ItemsSource = null;
                    dataGridSpatial.ItemsSource = paramList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select spatial category.\n"+ex.Message, "Spatial Category Selection Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxRevit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxRevit.SelectedItem)
                {
                    CategoryProperties cp = (CategoryProperties)comboBoxRevit.SelectedItem;
                    var parameters = from param in cp.ParamDictionary.Values where param.IsReadOnly == false select param;
                    if (parameters.Count() > 0)
                    {
                        List<ParameterProperties> paramList = parameters.OrderBy(o => o.ParameterName).ToList();
                        dataGridRevit.ItemsSource = null;
                        dataGridRevit.ItemsSource = paramList;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select Revit Category.\n"+ex.Message, "Revit Category Selection Changed.", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridSpatial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridSpatial.SelectedItem)
                {
                    ParameterProperties selectedItem = (ParameterProperties)dataGridSpatial.SelectedItem;
                    StorageType storageType = selectedItem.ParamType;
                    List<ParameterProperties> paramList = dataGridRevit.ItemsSource as List<ParameterProperties>;
                    List<ParameterProperties> parameterList = new List<ParameterProperties>();
                    if (null != paramList)
                    {
                        foreach (ParameterProperties pp in paramList)
                        {
                            ParameterProperties property = new ParameterProperties(pp);
                            if (property.ParamType == storageType)
                            {
                                property.SetTextColor(true);
                            }
                            else
                            {
                                property.SetTextColor(false);
                            }
                            parameterList.Add(property);
                        }
                    }

                    dataGridRevit.ItemsSource = null;
                    dataGridRevit.ItemsSource = parameterList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a parameter in spatial elements.\n"+ex.Message, "Spatial Parameters Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridRevit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply selection.\n"+ex.Message, "Revit Parameters Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridSpatial.SelectedItem && null != dataGridRevit.SelectedItem)
                {
                    ParameterProperties spp = (ParameterProperties)dataGridSpatial.SelectedItem;
                    ParameterProperties rpp = (ParameterProperties)dataGridRevit.SelectedItem;
                    if (rpp.IsSelectable)
                    {
                        CategoryProperties scp=(CategoryProperties)comboBoxSpace.SelectedItem;
                        CategoryProperties rcp=(CategoryProperties)comboBoxRevit.SelectedItem;

                        ParameterMapProperties pmp = new ParameterMapProperties(spp, rpp, scp, rcp);

                        var properties = from property in parameterMapList
                                         where property.SpatialCatName == scp.CategoryName && property.RevitCatName == rcp.CategoryName &&
                                             property.SpatialParamName == spp.ParameterName && property.RevitParamName == rpp.ParameterName
                                         select property;
                        if (properties.Count() == 0)
                        {
                            parameterMapList.Add(pmp);

                            dataGridParamMap.ItemsSource = null;
                            dataGridParamMap.ItemsSource = parameterMapList;
                        }
                        else
                        {
                            MessageBox.Show("The selected parameter map already exist in the selsction.\nPlease select different categories and paraemters.", "Existing Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The storage types do not match between spatial and Revit parameters.\nPlease select a valid parameter.", "Storage Type Mismatched", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a pair of parameter from a spatial category and a Revit category.", "Parameter Not Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add links.\n"+ex.Message, "Add Parameter Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridParamMap.SelectedItem)
                {
                    ParameterMapProperties pmp = (ParameterMapProperties)dataGridParamMap.SelectedItem;
                    dataGridParamMap.ItemsSource = null;
                    for (int i = 0; i < parameterMapList.Count; i++)
                    {
                        ParameterMapProperties properties = parameterMapList[i];
                        if (properties.SpatialCatName == pmp.SpatialCatName && properties.RevitCatName == pmp.RevitCatName && properties.SpatialParamName == pmp.SpatialParamName && properties.RevitParamName == pmp.RevitParamName)
                        {
                            parameterMapList.RemoveAt(i);
                            break;
                        }
                    }
                    dataGridParamMap.ItemsSource = parameterMapList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove a parameter map.\n"+ex.Message, "Remove Parameter Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (parameterMapList.Count > 0)
                {
                    errorMessages = new StringBuilder();
                    statusLable.Text = "Writing parameters values...";

                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

                    using (TransactionGroup group = new TransactionGroup(m_doc))
                    {
                        group.Start("Write Revit Elements Parameters");
                        foreach (ParameterMapProperties pmp in parameterMapList)
                        {
                            List<SpatialElementProperties> spatialElementsList = FindSpatialElements(pmp, (bool)checkBoxLink.IsChecked);
                            progressBar.Minimum = 0;
                            progressBar.Maximum = spatialElementsList.Count;
                            progressBar.Value = 0;

                            double value = 0;
                            foreach (SpatialElementProperties sep in spatialElementsList)
                            {
                                value++;

                                List<Element> revitElements = FindRevitElements(pmp, sep);
                                if (revitElements.Count > 0)
                                {
                                    bool written = WriteParameter(pmp, sep, revitElements);
                                }
                                
                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                            }
                        }
                        if (groupExist)
                        {
                            MessageBox.Show("Elements in groups were skipped and prameter values were not written.", "Groups Exist", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        
                        group.Assimilate();
                    }

                    bool saved = RoomUpdaterDataStorageUtil.StoreParameterMaps(m_doc, parameterMapList);

                    statusLable.Text = "Completed.";
                    progressBar.Visibility = System.Windows.Visibility.Hidden;

                    if(!string.IsNullOrEmpty(errorMessages.ToString()))
                    {
                        RoomUpdaterErrorWindow errorWindow = new RoomUpdaterErrorWindow(errorMessages.ToString());
                        if(errorWindow.ShowDialog() == true)
                        {
                            errorMessages = new StringBuilder();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply parameters maps.\n"+ex.Message, "Apply Parameter Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
                statusLable.Text = "";
            }
        }

        private List<SpatialElementProperties> FindSpatialElements(ParameterMapProperties pmp, bool includeLinks)
        {
            List<SpatialElementProperties> sepList = new List<SpatialElementProperties>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<SpatialElement> spatialElements = collector.OfCategoryId(pmp.SpatialCategory.CategoryObj.Id).WhereElementIsNotElementType().Cast<SpatialElement>().ToList();
                foreach (SpatialElement se in spatialElements)
                {
                    SpatialElementProperties sep = new SpatialElementProperties(se);
                    sep.IsLinked = false;
                    sepList.Add(sep);
                }

                if (includeLinks)
                {
                    FilteredElementCollector linkCollector = new FilteredElementCollector(m_doc);
                    List<RevitLinkInstance> linkInstances = linkCollector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();
                    foreach (RevitLinkInstance instance in linkInstances)
                    {
                        LinkedInstanceProperties lip = new LinkedInstanceProperties(instance);
                        if (null != lip.LinkedDocument && null!=lip.DocumentTitle )
                        {
                            FilteredElementCollector spaceCollector = new FilteredElementCollector(lip.LinkedDocument);
                            List<SpatialElement> elements = spaceCollector.OfCategoryId(pmp.SpatialCategory.CategoryObj.Id).WhereElementIsNotElementType().Cast<SpatialElement>().ToList();
                            if (elements.Count > 0)
                            {
                                foreach (SpatialElement se in elements)
                                {
                                    SpatialElementProperties sep = new SpatialElementProperties(se);
                                    sep.IsLinked = true;
                                    sep.LinkProperties = lip;
                                    if (sep.SpaceArea > 0)
                                    {
                                        sepList.Add(sep);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessages.AppendLine("Cannot find spatial elements from "+pmp.SpatialCategory.CategoryName);
                errorMessages.AppendLine(ex.Message);
            }
            return sepList;
        }

        private List<Element> FindRevitElements(ParameterMapProperties pmp, SpatialElementProperties sep)
        {
            List<Element> revitElements = new List<Element>();
            try
            {
                BoundingBoxXYZ boundingBox = null;
                Room room = null;
                Space space = null;
                
                if (sep.CategoryId == (int)BuiltInCategory.OST_Rooms)
                {
                    room = sep.ElementObj as Room;
                    boundingBox = room.get_BoundingBox(null);
                }
                else if (sep.CategoryId == (int)BuiltInCategory.OST_MEPSpaces)
                {
                    space = sep.ElementObj as Space;
                    boundingBox = space.get_BoundingBox(null);
                }

                if (null != boundingBox)
                {
                    XYZ minXYZ = boundingBox.Min;
                    XYZ maxXYZ = boundingBox.Max;

                    if (sep.IsLinked && null != sep.LinkProperties)
                    {
                        minXYZ = sep.LinkProperties.TransformValue.OfPoint(minXYZ);
                        maxXYZ = sep.LinkProperties.TransformValue.OfPoint(maxXYZ);
                    }
                    //bounding box quick filter
                    Outline outline = new Outline(minXYZ, maxXYZ);
                    BoundingBoxIntersectsFilter intersectFilter = new BoundingBoxIntersectsFilter(outline);
                    BoundingBoxIsInsideFilter insideFilter = new BoundingBoxIsInsideFilter(outline);
                    LogicalOrFilter orFilter = new LogicalOrFilter(intersectFilter, insideFilter);

                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<Element> elements = collector.OfCategoryId(pmp.RevitCategory.CategoryObj.Id).WherePasses(orFilter).ToElements().ToList();

                    if (checkBoxIntersect.IsChecked == true)
                    {
                        revitElements.AddRange(elements);
                    }
                    else
                    {
                        //slow solid filter
                        if (elements.Count > 0)
                        {
                            foreach (Element element in elements)
                            {
                                LocationPoint locationPt = element.Location as LocationPoint;
                                if (null != locationPt)
                                {
                                    XYZ point = locationPt.Point;
                                    if (sep.IsLinked && null != sep.LinkProperties)
                                    {
                                        point = sep.LinkProperties.TransformValue.Inverse.OfPoint(point);
                                    }

                                    if (null != room)
                                    {
                                        if (room.IsPointInRoom(point))
                                        {
                                            revitElements.Add(element);
                                        }
                                    }
                                    else if (null != space)
                                    {
                                        if (space.IsPointInSpace(point))
                                        {
                                            revitElements.Add(element);
                                        }
                                    }
                                }

                                LocationCurve locationCurve = element.Location as LocationCurve;
                                if (null != locationCurve)
                                {
                                    Curve curve = locationCurve.Curve;
#if RELEASE2013
                                XYZ firstPt = curve.get_EndPoint(0);
                                XYZ secondPt = curve.get_EndPoint(1);
#elif RELEASE2014||RELEASE2015
                                    XYZ firstPt = curve.GetEndPoint(0);
                                    XYZ secondPt = curve.GetEndPoint(1);
#endif

                                    if (sep.IsLinked && null != sep.LinkProperties)
                                    {
                                        firstPt = sep.LinkProperties.TransformValue.Inverse.OfPoint(firstPt);
                                        secondPt = sep.LinkProperties.TransformValue.Inverse.OfPoint(secondPt);
                                    }

                                    if (null != room)
                                    {
                                        if (room.IsPointInRoom(firstPt) || room.IsPointInRoom(secondPt))
                                        {
                                            revitElements.Add(element);
                                        }
                                    }
                                    else if (null != space)
                                    {
                                        if (space.IsPointInSpace(firstPt) || space.IsPointInSpace(secondPt))
                                        {
                                            revitElements.Add(element);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessages.AppendLine("Cannot find model elements from - " +sep.ElementObj.Number+" : "+ sep.ElementObj.Name );
                errorMessages.AppendLine(ex.Message);
            }
            return revitElements;
        }

        private bool WriteParameter(ParameterMapProperties pmp, SpatialElementProperties sep, List<Element> revitElements)
        {
            bool result = false;
            try
            {
                SpatialElement spatialElement = sep.ElementObj;
                if (null != spatialElement)
                {
#if RELEASE2013 || RELEASE2014
                    Parameter sParam = spatialElement.get_Parameter(pmp.SpatialParamName);
#elif RELEASE2015
                    Parameter sParam = spatialElement.LookupParameter(pmp.SpatialParamName);
#endif

                    if (null != sParam)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Write Parameters");
                            try
                            {
                                foreach (Element re in revitElements)
                                {
                                    
#if RELEASE2013
                                    if (null!=re.Group) { groupExist = true; continue; } //elements in group
#elif RELEASE2014 || RELEASE2015
                                    if (re.GroupId != ElementId.InvalidElementId) { groupExist = true; continue; } //elements in group
#endif
                                    

#if RELEASE2013 || RELEASE2014
                                    Parameter rParam = re.get_Parameter(pmp.RevitParamName);
#elif RELEASE2015
                                    Parameter rParam = re.LookupParameter(pmp.RevitParamName);
#endif


                                    if (null == rParam)
                                    {
                                        ElementType elementType = m_doc.GetElement(re.GetTypeId()) as ElementType;
                                        if (null != elementType)
                                        {
#if RELEASE2013 || RELEASE2014
                                            rParam = elementType.get_Parameter(pmp.RevitParamName);

#elif RELEASE2015
                                            rParam = elementType.LookupParameter(pmp.RevitParamName);
#endif

                                        }
                                    }
                                    
                                    if (null != rParam)
                                    {
                                        if (rParam.IsReadOnly) { continue; }
                                        switch (rParam.StorageType)
                                        {
                                            case StorageType.Double:
                                                try { rParam.Set(sParam.AsDouble()); }
                                                catch { }
                                                break;
                                            case StorageType.ElementId:
                                                try { rParam.Set(sParam.AsElementId()); }
                                                catch { }
                                                break;
                                            case StorageType.Integer:
                                                try { rParam.Set(sParam.AsInteger()); }
                                                catch { }
                                                break;
                                            case StorageType.String:
                                                try { rParam.Set(sParam.AsString()); }
                                                catch { }
                                                break;

                                        }
                                    }
                                }

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                                trans.RollBack();
                            }
                        }
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessages.AppendLine("Cannot write parameter values of family instances inside - " + sep.ElementObj.Number + " : " + sep.ElementObj.Name);
                errorMessages.AppendLine(ex.Message);
            }
            return result;
        }
    }

    public class CategoryProperties
    {
        private Category m_cat = null;
        private string categoryName = "";
        private int categoryId = -1;
        private Dictionary<string/*paramName*/, ParameterProperties> paramDictionary = new Dictionary<string, ParameterProperties>();

        public Category CategoryObj { get { return m_cat; } set { m_cat = value; } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; } }
        public int CategoryId { get { return categoryId; } set { categoryId = value; } }
        public Dictionary<string/*paramName*/, ParameterProperties> ParamDictionary { get { return paramDictionary; } set { paramDictionary = value; } }

        public CategoryProperties(Category cat)
        {
            m_cat = cat;
            categoryName = cat.Name;
            categoryId = cat.Id.IntegerValue;
        }
    }

    public class ParameterProperties
    {
        private Parameter m_param = null;
        private string paramName = "";
        private StorageType paramType = StorageType.None;
        private int paramId = -1;
        private BuiltInParameter bltParameter = BuiltInParameter.INVALID;
        private bool isInstance = true;
        private bool isReadOnly = false;
        private bool isSelectable = true;
        private Brush textColor = new SolidColorBrush(Colors.Black);

        public Parameter ParameterObj { get { return m_param; } set { m_param = value; } }
        public string ParameterName { get { return paramName; } set { paramName = value; } }
        public StorageType ParamType { get { return paramType; } set { paramType = value; } }
        public int ParamId { get { return paramId; } set { paramId = value; } }
        public BuiltInParameter BltParameter { get { return bltParameter; } set { bltParameter = value; } }
        public bool IsInstance { get { return isInstance; } set { isInstance = value; } }
        public bool IsReadOnly { get { return isReadOnly; } set { isReadOnly = value; } }
        public bool IsSelectable { get { return isSelectable; } set { isSelectable = value; } }
        public Brush TextColor { get { return textColor; } set { textColor = value; } }


        public ParameterProperties()
        {
        }

        public ParameterProperties(string name)
        {
            paramName = name;
        }

        public ParameterProperties(ParameterProperties pp)
        {
            this.ParameterObj = pp.ParameterObj;
            this.ParameterName = pp.ParameterName;
            this.ParamType = pp.ParamType;
            this.ParamId = pp.ParamId;
            this.BltParameter = pp.BltParameter;
            this.IsInstance = pp.IsInstance;
            this.IsReadOnly = pp.IsReadOnly;
            this.IsSelectable = pp.IsSelectable;
            this.TextColor = pp.TextColor;
        }

        public ParameterProperties(Parameter param)
        {
            m_param = param;
            paramName = m_param.Definition.Name;
            paramType = m_param.StorageType;
            paramId = m_param.Id.IntegerValue;
            isReadOnly = param.IsReadOnly;
        }

        public void SetTextColor(bool selectable)
        {
            isSelectable = selectable;
            if (selectable)
            {
                textColor = new SolidColorBrush(Colors.Black);
            }
            else
            {
                textColor = new SolidColorBrush(Colors.Gray);
            }
        }
    }

    public class ParameterMapProperties
    {
        private ParameterProperties spatialParameter = null;
        private ParameterProperties revitParameter = null;
        private CategoryProperties spatialCategory = null;
        private CategoryProperties revitCategory = null;

        private string spatialParamName = "";
        private string revitParamName = "";
        private string spatialCatName = "";
        private string revitCatName = "";

        public ParameterProperties SpatialParameter { get { return spatialParameter; } set { spatialParameter = value; } }
        public ParameterProperties RevitParameter { get { return revitParameter; } set { revitParameter = value; } }
        public CategoryProperties SpatialCategory { get { return spatialCategory; } set { spatialCategory = value; } }
        public CategoryProperties RevitCategory { get { return revitCategory; } set { revitCategory = value; } }

        public string SpatialParamName { get { return spatialParamName; } set { spatialParamName = value; } }
        public string RevitParamName { get { return revitParamName; } set { revitParamName = value; } }
        public string SpatialCatName { get { return spatialCatName; } set { spatialCatName = value; } }
        public string RevitCatName { get { return revitCatName; } set { revitCatName = value; } }

        public ParameterMapProperties(ParameterProperties spp, ParameterProperties rpp, CategoryProperties scp, CategoryProperties rcp)
        {
            spatialParameter = spp;
            spatialParamName = spp.ParameterName;

            revitParameter = rpp;
            revitParamName = rpp.ParameterName;

            spatialCategory = scp;
            spatialCatName = scp.CategoryName;

            revitCategory = rcp;
            revitCatName = rcp.CategoryName;
        }
    }

    public class SpatialElementProperties
    {
        private SpatialElement elementObj = null;
        private string elementName = "";
        private int categoryId = -1;
        private bool isLinked = false;
        private double spaceArea = 0;
        private LinkedInstanceProperties linkProperties = null;

        public SpatialElement ElementObj { get { return elementObj; } set { elementObj = value; } }
        public string ElementName { get { return elementName; } set { elementName = value; } }
        public int CategoryId { get { return categoryId; } set { categoryId = value; } }
        public bool IsLinked { get { return isLinked; } set { isLinked = value; } }
        public double SpaceArea { get { return spaceArea; } set { spaceArea = value; } }
        public LinkedInstanceProperties LinkProperties { get { return linkProperties; } set { linkProperties = value; } }

        public SpatialElementProperties(SpatialElement element)
        {
         
            elementObj = element;
            if (!string.IsNullOrEmpty(element.Name))
            {
                elementName = element.Name;
            }
            if (null != element.Category)
            {
                categoryId = element.Category.Id.IntegerValue;
            }

            if (null != element.Area)
            {
                spaceArea = element.Area;
            }
        }
    }

    public class LinkedInstanceProperties
    {
        private RevitLinkInstance m_instance = null;
        private int instanceId = -1;
        private Document linkedDocument = null;
        private string documentTitle = "";
        private Autodesk.Revit.DB.Transform transformValue = null;

        public RevitLinkInstance Instance { get { return m_instance; } set { m_instance = value; } }
        public int InstanceId { get { return instanceId; } set { instanceId = value; } }
        public Document LinkedDocument { get { return linkedDocument; } set { linkedDocument = value; } }
        public string DocumentTitle { get { return documentTitle; } set { documentTitle = value; } }
        public Autodesk.Revit.DB.Transform TransformValue { get { return transformValue; } set { transformValue = value; } }

        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            m_instance = instance;
            instanceId = instance.Id.IntegerValue;
#if RELEASE2013
            linkedDocument = instance.Document;
#elif RELEASE2014 || RELEASE2015
            linkedDocument = instance.GetLinkDocument();
#endif
            if (null != linkedDocument)
            {
                if (!string.IsNullOrEmpty(linkedDocument.Title))
                {
                    documentTitle = linkedDocument.Title;
                }
            }

            if (null != instance.GetTotalTransform())
            {
                transformValue = instance.GetTotalTransform();
            }
        }
    }

}
