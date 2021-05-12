using System;
using System.Collections.Generic;
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
using Autodesk.Revit.UI;

namespace HOK.RoomsToMass.ToMass
{
    public enum ParameterUpdateType
    {
        FromMassToHost, FromHostToMass, None
    }
    /// <summary>
    /// Interaction logic for MassParameterWindow.xaml
    /// </summary>
    public partial class MassParameterWindow : Window
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private MassConfiguration massConfig = null;
        private List<ParameterInfo> hostParameters = new List<ParameterInfo>();
        private List<ParameterInfo> massParameters = new List<ParameterInfo>();
        private List<ParameterMapInfo> mapInfoList = new List<ParameterMapInfo>();

        public MassConfiguration MassConfig { get { return massConfig; } set { massConfig = value; } }
        
        public MassParameterWindow(UIApplication uiapp, MassConfiguration configuration)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            massConfig = configuration;
            mapInfoList = massConfig.MassParameters;
            GetParameterInformation();

            InitializeComponent();
            labelHostCategoryName.Content = configuration.HostCategory;
            labelMassCategoryName.Content = configuration.MassCategory;
            DisplayParameterMapInfo();
            DisplayParameterInfo();

            if (massConfig.UpdateType == ParameterUpdateType.FromHostToMass)
            {
                radioButtonHostToMass.IsChecked = true;
            }
            else if (massConfig.UpdateType == ParameterUpdateType.FromMassToHost)
            {
                radioButtonMassToHost.IsChecked = true;
            }

        }

        private void GetParameterInformation()
        {
            try
            {
                Categories categories = m_doc.Settings.Categories;
                hostParameters = new List<ParameterInfo>();
                if (!string.IsNullOrEmpty(massConfig.HostCategory))
                {
                    Category hostCategory = categories.get_Item(massConfig.HostCategory);
                    BuiltInCategory bltCategory = (BuiltInCategory)hostCategory.Id.IntegerValue;
                    List<ElementId> categoryIds = new List<ElementId>();
                    categoryIds.Add(hostCategory.Id);
                    List<ElementId> parameterIds = ParameterFilterUtilities.GetFilterableParametersInCommon(m_doc, categoryIds).ToList();

                    //get sample element 
                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<Element> elements = collector.OfCategory(bltCategory).WhereElementIsNotElementType().ToElements().ToList();
                    if (elements.Count > 0)
                    {
                        Element sampleElement = elements.First();
                        foreach (Parameter param in sampleElement.Parameters)
                        {
                            if (param.Definition.Name.Contains("Extensions.")) { continue; }
                            if(parameterIds.Contains(param.Id))
                            {
                                ParameterInfo paramInfo = new ParameterInfo(param);
                                hostParameters.Add(paramInfo);
                            }
                        }
                    }
                }
                hostParameters = hostParameters.OrderBy(o => o.ParameterName).ToList();

                massParameters = new List<ParameterInfo>();
                if (!string.IsNullOrEmpty(massConfig.MassCategory))
                {
                    Category massCategory = categories.get_Item(massConfig.MassCategory);
                    BuiltInCategory bltCategory = (BuiltInCategory)massCategory.Id.IntegerValue;
                    List<ElementId> categoryIds = new List<ElementId>();
                    categoryIds.Add(massCategory.Id);
                    List<ElementId> parameterIds = ParameterFilterUtilities.GetFilterableParametersInCommon(m_doc, categoryIds).ToList();
                    //get sample element 
                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<Element> elements = collector.OfCategory(bltCategory).WhereElementIsNotElementType().ToElements().ToList();
                    if (elements.Count > 0)
                    {
                        Element sampleElement = elements.First();
                        foreach (Parameter param in sampleElement.Parameters)
                        {
                            if (param.Definition.Name.Contains("Extensions.")) { continue; }
                            if (parameterIds.Contains(param.Id))
                            {
                                ParameterInfo paramInfo = new ParameterInfo(param);
                                massParameters.Add(paramInfo);
                            }
                        }
                    }
                    else
                    {
                        if (parameterIds.Contains(new ElementId((int)BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)))
                        {
                            ParameterInfo commentsParamInfo = new ParameterInfo();
                            commentsParamInfo.ParameterName = "Comments";
                            commentsParamInfo.ParamStorageType = StorageType.String;
#if RELEASE2022
                            commentsParamInfo.ParamType = SpecTypeId.String.Text;
#else
                            commentsParamInfo.ParamType = ParameterType.Text;
#endif
                            massParameters.Add(commentsParamInfo);
                        }


                        if (parameterIds.Contains(new ElementId((int)BuiltInParameter.ALL_MODEL_MARK)))
                        {
                            ParameterInfo markParamInfo = new ParameterInfo();
                            markParamInfo.ParameterName = "Mark";
                            markParamInfo.ParamStorageType = StorageType.String;
#if RELEASE2022
                            markParamInfo.ParamType = SpecTypeId.String.Text;
#else
                            markParamInfo.ParamType = ParameterType.Text;
#endif
                            massParameters.Add(markParamInfo);
                        }


                        DefinitionBindingMapIterator bindingMapIterator = m_doc.ParameterBindings.ForwardIterator();

                        while (bindingMapIterator.MoveNext())
                        {
                            InstanceBinding binding = bindingMapIterator.Current as InstanceBinding;
                            if (null != binding)
                            {
                                if (binding.Categories.Contains(massCategory))
                                {
                                    Definition paramDefinition = bindingMapIterator.Key;
                                    if (paramDefinition.Name.Contains("Extensions.")) { continue; }
                                    ParameterInfo paramInfo = new ParameterInfo();
                                    paramInfo.ParameterName = paramDefinition.Name;
#if RELEASE2022
                                    paramInfo.ParamType = paramDefinition.GetDataType();
#else
                                    paramInfo.ParamType = paramDefinition.ParameterType;
#endif
                                    massParameters.Add(paramInfo);
                                }
                            }
                        }
                    }

                }
                massParameters = massParameters.OrderBy(o => o.ParameterName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get parameter information.\n"+ex.Message, "Get Parameter Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayParameterInfo()
        {
            try
            {

                List<ParameterInfo> hostParamList = new List<ParameterInfo>();
                hostParamList.AddRange(hostParameters);
                List<ParameterInfo> massParamList = new List<ParameterInfo>();
                massParamList.AddRange(massParameters);

                foreach (ParameterMapInfo mapInfo in mapInfoList)
                {
                    int hostIndex = hostParamList.FindIndex(o => o.ParameterName == mapInfo.HostParamInfo.ParameterName);
                    if (hostIndex > -1)
                    {
                        hostParamList.RemoveAt(hostIndex);
                    }

                    int massIndex = massParamList.FindIndex(o => o.ParameterName == mapInfo.MassParamInfo.ParameterName);
                    if (massIndex > -1)
                    {
                        massParamList.RemoveAt(massIndex);
                    }
                }

                dataGridHost.ItemsSource = null;
                dataGridHost.ItemsSource = hostParamList;

                dataGridMass.ItemsSource = null;
                dataGridMass.ItemsSource = massParamList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display parameter information.\n"+ex.Message, "Display Parameter Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayParameterMapInfo()
        {
            try
            {
                List<ParameterMapInfo> copiedList = mapInfoList;
                dataGridMap.ItemsSource = null;
                dataGridMap.ItemsSource = copiedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display parameter mapping information.\n"+ex.Message, "Display Parameter Mapping Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridHost.SelectedItem)
                {
                    ParameterInfo hostInfo = (ParameterInfo)dataGridHost.SelectedItem;
                    List<ParameterInfo> massParamList = (List<ParameterInfo>)dataGridMass.ItemsSource;
                    List<ParameterInfo> updatedList = new List<ParameterInfo>();
                    foreach (ParameterInfo pi in massParamList)
                    {
                        pi.Enabled = (pi.ParamType == hostInfo.ParamType) ? true : false;
                        updatedList.Add(pi);
                    }

                    dataGridMass.ItemsSource = null;
                    dataGridMass.ItemsSource = updatedList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh datagrid.\n" + ex.Message, "Data Grid Selection Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridHost.SelectedItem && null != dataGridMass.SelectedItem )
                {
                    ParameterInfo hostParam = (ParameterInfo)dataGridHost.SelectedItem;
                    ParameterInfo massParam = (ParameterInfo)dataGridMass.SelectedItem;

                    if (hostParam.ParamType == massParam.ParamType)
                    {
                        ParameterMapInfo mapInfo = new ParameterMapInfo(hostParam, massParam);
                        mapInfoList.Add(mapInfo);

                        DisplayParameterMapInfo();
                        DisplayParameterInfo();
                    }
                    else
                    {
                        MessageBox.Show("Parameter type does not match.\nPlease select a mass parameter of which type is same as the host parameter.", "Parameter Type Mismatch", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add link.\n"+ex.Message, "Add Link", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridMap.SelectedItem)
                {
                    ParameterMapInfo mapInfo = (ParameterMapInfo)dataGridMap.SelectedItem;
                    int mapIndex = mapInfoList.FindIndex(o => o.HostParamInfo.ParameterName == mapInfo.HostParamInfo.ParameterName 
                        && o.MassParamInfo.ParameterName == mapInfo.MassParamInfo.ParameterName);
                    if (mapIndex > -1)
                    {
                        mapInfoList.RemoveAt(mapIndex);
                    }
                    DisplayParameterMapInfo();
                    DisplayParameterInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove link.\n"+ex.Message, "Remove Link", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)radioButtonHostToMass.IsChecked)
                {
                    massConfig.UpdateType = ParameterUpdateType.FromHostToMass;
                }
                else if ((bool)radioButtonMassToHost.IsChecked)
                {
                    massConfig.UpdateType = ParameterUpdateType.FromMassToHost;
                }

                massConfig.MassParameters = mapInfoList;

                bool stored = MassConfigDataStorageUtil.StoreMassConfiguration(m_doc, massConfig);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set the configuration.\n"+ex.Message, "Mass Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }

    public class ParameterMapInfo
    {
        private string hostParamName = "";
        private ParameterInfo hostParamInfo = null;
        private string massParamName = "";
        private ParameterInfo massParamInfo = null;

        public string HostParamName { get { return hostParamName; } set { hostParamName = value; } }
        public ParameterInfo HostParamInfo { get { return hostParamInfo; } set { hostParamInfo = value; } }
        public string MassParamName { get { return massParamName; } set { massParamName = value; } }
        public ParameterInfo MassParamInfo { get { return massParamInfo; } set { massParamInfo = value; } }

        public ParameterMapInfo(ParameterInfo host, ParameterInfo mass)
        {
            hostParamInfo = host;
            hostParamName = hostParamInfo.ParameterName;
            massParamInfo = mass;
            massParamName = massParamInfo.ParameterName;
        }
    }

    public class ParameterInfo
    {
        private string parameterName = "";
#if RELEASE2022
        private ForgeTypeId paramType = null;
#else
        private ParameterType paramType = ParameterType.Invalid;
#endif
        private StorageType paramStorageType = StorageType.None;
        private bool enabled = true;

        public string ParameterName { get { return parameterName; } set { parameterName = value; } }
#if RELEASE2022
        public ForgeTypeId ParamType { get { return paramType; } set { paramType = value; } }
#else
        public ParameterType ParamType { get { return paramType; } set { paramType = value; } }
#endif
        public StorageType ParamStorageType { get { return paramStorageType; } set { paramStorageType = value; } }
        public bool Enabled { get { return enabled; } set { enabled = value; } }


        public ParameterInfo()
        {
        }

        public ParameterInfo(Parameter param)
        {
            parameterName = param.Definition.Name;
#if RELEASE2022
            paramType = param.Definition.GetDataType();
#else
            paramType = param.Definition.ParameterType;
#endif
            paramStorageType = param.StorageType;
        }
    }

}
