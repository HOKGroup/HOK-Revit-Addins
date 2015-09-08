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

namespace HOK.CameraDuplicator
{
    /// <summary>
    /// Interaction logic for ViewConfigurationWindow.xaml
    /// </summary>
    public partial class ViewConfigurationWindow : Window
    {
        private ModelInfo sModelInfo = null;
        private ModelInfo rModelInfo = null;
        private ViewConfiguration viewConfig = new ViewConfiguration();

        private Dictionary<int, ItemInfo> sourceItems = new Dictionary<int, ItemInfo>();
        private Dictionary<int, ItemInfo> recipientItems = new Dictionary<int, ItemInfo>();

        private MapType currentType = MapType.None;

        public ViewConfiguration ViewConfig { get { return viewConfig; } set { viewConfig = value; } }

        public ViewConfigurationWindow(ModelInfo source, ModelInfo recipient, ViewConfiguration vc)
        {
            sModelInfo = source;
            rModelInfo = recipient;
            viewConfig = vc;

            InitializeComponent();
            labelSource.Content = sModelInfo.ModelName;
            labelTarget.Content = rModelInfo.ModelName;

            checkBoxWorkset.IsChecked = vc.ApplyWorksetVisibility;
            GetItems();
            //remove non-existing mapping items
            for (int i = viewConfig.MapItems.Count - 1; i > -1; i--)
            {
                MapItemInfo mapItemInfo = viewConfig.MapItems[i];
                if (!sourceItems.ContainsKey(mapItemInfo.SourceItemId) || !recipientItems.ContainsKey(mapItemInfo.RecipientItemId))
                {
                    viewConfig.MapItems.RemoveAt(i);
                }
            }

            radioButtonLevel.IsChecked = true;
        }

        private void GetItems()
        {
            try
            {
                GetLevels();
                GetPhases();
                GetTemplates();
                GetScopeBoxes();
                GetWorksets();
                GetAreaSchemes();

                var currentMaps = from map in viewConfig.MapItems where map.SourceModelId == sModelInfo.ModelId && map.RecipientModelId == rModelInfo.ModelId select map;
                foreach (MapItemInfo mapInfo in currentMaps)
                {
                    int sItemId = mapInfo.SourceItemId;
                    int rItemId = mapInfo.RecipientItemId;

                    if (sourceItems.ContainsKey(sItemId) && recipientItems.ContainsKey(rItemId))
                    {
                        ItemInfo sItem = sourceItems[sItemId];
                        if (!sItem.Mapped)
                        {
                            sItem.Mapped = true;
                            sourceItems.Remove(sItemId);
                            sourceItems.Add(sItemId, sItem);
                        }

                        ItemInfo rItem = recipientItems[rItemId];
                        if (!rItem.Mapped)
                        {
                            rItem.Mapped = true;
                            recipientItems.Remove(rItemId);
                            recipientItems.Add(rItemId, rItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get items.\n" + ex.Message, "Get Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetLevels()
        {
            try
            {
                FilteredElementCollector sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                List<Element> sLevels = sCollector.OfCategory(BuiltInCategory.OST_Levels).ToElements().ToList();
                foreach (Element elem in sLevels)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.Level);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                FilteredElementCollector rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                List<Element> rLevels = rCollector.OfCategory(BuiltInCategory.OST_Levels).ToElements().ToList();
                foreach (Element elem in rLevels)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.Level);
                    if (!recipientItems.ContainsKey(iInfo.ItemId))
                    {
                        recipientItems.Add(iInfo.ItemId, iInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get level items.\n" + ex.Message, "Get Levels", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetPhases()
        {
            try
            {
                FilteredElementCollector sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                List<Element> sPhases = sCollector.OfCategory(BuiltInCategory.OST_Phases).ToElements().ToList();
                foreach (Element elem in sPhases)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.Phase);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                FilteredElementCollector rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                List<Element> rPhases = rCollector.OfCategory(BuiltInCategory.OST_Phases).ToElements().ToList();
                foreach (Element elem in rPhases)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.Phase);
                    if (!recipientItems.ContainsKey(iInfo.ItemId))
                    {
                        recipientItems.Add(iInfo.ItemId, iInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get phase items.\n" + ex.Message, "Get Phases", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetTemplates()
        {
            try
            {
                List<Type> types = new List<Type>();
                types.Add(typeof(View3D));
                types.Add(typeof(ViewPlan));
                ElementMulticlassFilter multiClassFilter = new ElementMulticlassFilter(types);

                FilteredElementCollector sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                List<View> sViews = sCollector.WherePasses(multiClassFilter).ToElements().Cast<View>().ToList();
                var sTemplates = from view in sViews where view.IsTemplate select view;
                foreach (View view in sTemplates)
                {
                    ItemInfo iInfo = new ItemInfo(view, MapType.ViewTemplate);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                FilteredElementCollector rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                List<View> rViews = sCollector.WherePasses(multiClassFilter).ToElements().Cast<View>().ToList();
                var rTemplates = from view in rViews where view.IsTemplate select view;
                foreach (Element elem in rTemplates)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.ViewTemplate);
                    if (!recipientItems.ContainsKey(iInfo.ItemId))
                    {
                        recipientItems.Add(iInfo.ItemId, iInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get view templates.\n" + ex.Message, "Get View Templates", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetWorksets()
        {
            try
            {
                if (sModelInfo.ModelDoc.IsWorkshared)
                {
                    FilteredWorksetCollector collector = new FilteredWorksetCollector(sModelInfo.ModelDoc);
                    WorksetKindFilter wsFilter = new WorksetKindFilter(WorksetKind.UserWorkset);
                    List<Workset> worksets = collector.WherePasses(wsFilter).ToWorksets().ToList();
                    foreach (Workset ws in worksets)
                    {
                        ItemInfo iInfo = new ItemInfo(ws, MapType.Workset);
                        if(!sourceItems.ContainsKey(iInfo.ItemId))
                        {
                            sourceItems.Add(iInfo.ItemId, iInfo);
                        }
                    }
                }

                if (rModelInfo.ModelDoc.IsWorkshared)
                {
                    FilteredWorksetCollector collector = new FilteredWorksetCollector(rModelInfo.ModelDoc);
                    WorksetKindFilter wsFilter = new WorksetKindFilter(WorksetKind.UserWorkset);
                    List<Workset> worksets = collector.WherePasses(wsFilter).ToWorksets().ToList();
                    foreach (Workset ws in worksets)
                    {
                        ItemInfo iInfo = new ItemInfo(ws, MapType.Workset);
                        if (!recipientItems.ContainsKey(iInfo.ItemId))
                        {
                            recipientItems.Add(iInfo.ItemId, iInfo);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get worksets.\n"+ex.Message, "Get Worksets", MessageBoxButton.OK, MessageBoxImage.Warning); 
            }
        }

        private void GetScopeBoxes()
        {
            try
            {
                FilteredElementCollector sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                List<Element> sScopeBoxes= sCollector.OfCategory(BuiltInCategory.OST_VolumeOfInterest).ToElements().ToList();
                foreach (Element elem in sScopeBoxes)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.ScopeBox);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                FilteredElementCollector rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                List<Element> rScopeBoxes = rCollector.OfCategory(BuiltInCategory.OST_VolumeOfInterest).ToElements().ToList();
                foreach (Element elem in rScopeBoxes)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.ScopeBox);
                    if (!recipientItems.ContainsKey(iInfo.ItemId))
                    {
                        recipientItems.Add(iInfo.ItemId, iInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get scope boxes.\n" + ex.Message, "Get Scope Boxes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetAreaSchemes()
        {
            try
            {
                FilteredElementCollector sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                List<Element> sSchemes = sCollector.OfCategory(BuiltInCategory.OST_AreaSchemes).ToElements().ToList();
                foreach (Element elem in sSchemes)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.AreaScheme);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                FilteredElementCollector rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                List<Element> rSchemes = rCollector.OfCategory(BuiltInCategory.OST_AreaSchemes).ToElements().ToList();
                foreach (Element elem in rSchemes)
                {
                    ItemInfo iInfo = new ItemInfo(elem, MapType.AreaScheme);
                    if (!recipientItems.ContainsKey(iInfo.ItemId))
                    {
                        recipientItems.Add(iInfo.ItemId, iInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get area schemes.\n" + ex.Message, "Get Area Schemes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayItems()
        {
            try
            {
                var sItems = from item in sourceItems.Values where !item.Mapped && item.MapTypeEnum == currentType select item;
                List<ItemInfo> sItemList = sItems.OrderBy(o => o.ItemName).ToList();
                dataGridSource.ItemsSource = null;
                dataGridSource.ItemsSource = sItemList;

                var rItems = from item in recipientItems.Values where !item.Mapped  && item.MapTypeEnum == currentType select item;
                List<ItemInfo> rItemList = rItems.OrderBy(o => o.ItemName).ToList();
                dataGridRecipient.ItemsSource = null;
                dataGridRecipient.ItemsSource = rItemList;
                
                var currentMap = from map in viewConfig.MapItems where map.SourceModelId == sModelInfo.ModelId && map.RecipientModelId == rModelInfo.ModelId && map.MapItemType == currentType select map;
                dataGridMap.ItemsSource = null;
                dataGridMap.ItemsSource = currentMap.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display items.\n"+ex.Message, "Display Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridSource.SelectedItem && null != dataGridRecipient.SelectedItem)
                {
                    ItemInfo sItemInfo = (ItemInfo)dataGridSource.SelectedItem;
                    ItemInfo rItemInfo = (ItemInfo)dataGridRecipient.SelectedItem;

                    if (rItemInfo.Enabled)
                    {
                        MapItemInfo mapItemInfo = new MapItemInfo();
                        mapItemInfo.MapItemType = currentType;
                        mapItemInfo.SourceModelId = sModelInfo.ModelId;
                        mapItemInfo.SourceItemId = sItemInfo.ItemId;
                        mapItemInfo.SourceItemName = sItemInfo.ItemName;
                        mapItemInfo.RecipientModelId = rModelInfo.ModelId;
                        mapItemInfo.RecipientItemId = rItemInfo.ItemId;
                        mapItemInfo.RecipientItemName = rItemInfo.ItemName;
                        viewConfig.MapItems.Add(mapItemInfo);

                        if (sourceItems.ContainsKey(sItemInfo.ItemId) && recipientItems.ContainsKey(rItemInfo.ItemId))
                        {
                            sItemInfo.Mapped = true;
                            sourceItems.Remove(sItemInfo.ItemId);
                            sourceItems.Add(sItemInfo.ItemId, sItemInfo);

                            rItemInfo.Mapped = true;
                            recipientItems.Remove(rItemInfo.ItemId);
                            recipientItems.Add(rItemInfo.ItemId, rItemInfo);
                        }

                        DisplayItems();
                    }
                    else
                    {
                        MessageBox.Show("The type of recipient item is different from the source item.\nPlease select a valid item.", "Type Mismatched", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a source item and a recipient item to create a link.", "Missing Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add a link.\n"+ex.Message, "Add Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridMap.SelectedItem)
                {
                    MapItemInfo mapItemInfo = (MapItemInfo)dataGridMap.SelectedItem;
                    bool removed = viewConfig.MapItems.Remove(mapItemInfo);

                    int sItemId = mapItemInfo.SourceItemId;
                    int rItemId = mapItemInfo.RecipientItemId;
                    if (sourceItems.ContainsKey(sItemId) && recipientItems.ContainsKey(rItemId))
                    {
                        ItemInfo sItemInfo = sourceItems[sItemId];
                        sItemInfo.Mapped = false;
                        sourceItems.Remove(sItemId);
                        sourceItems.Add(sItemId, sItemInfo);

                        ItemInfo rItemInfo = recipientItems[rItemId];
                        rItemInfo.Mapped = false;
                        recipientItems.Remove(rItemId);
                        recipientItems.Add(rItemId, rItemInfo);
                    }

                    DisplayItems();
                }
                else
                {
                    MessageBox.Show("Please select a mapping item from the list.", "Missing Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
     
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove a link.\n"+ex.Message, "Remove Link", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void radioButtonLevel_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButtonLevel.IsChecked)
            {
                currentType = MapType.Level;
                DisplayItems();
            }
        }

        private void radioButtonViewTemplate_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButtonViewTemplate.IsChecked)
            {
                currentType = MapType.ViewTemplate;
                DisplayItems();
            }
        }

        private void radioButtonScopeBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButtonScopeBox.IsChecked)
            {
                currentType = MapType.ScopeBox;
                DisplayItems();
            }
        }

        private void radioButtonWorkset_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButtonWorkset.IsChecked)
            {
                currentType = MapType.Workset;
                DisplayItems();
            }
        }

        private void radioButtonPhase_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButtonPhase.IsChecked)
            {
                currentType = MapType.Phase;
                DisplayItems();
            }
        }

        private void radioButtonAreaScheme_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButtonAreaScheme.IsChecked)
            {
                currentType = MapType.AreaScheme;
                DisplayItems();
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            bool savedStorage = ViewConfigDataStorageUtil.StoreViewConfiguration(rModelInfo.ModelDoc, viewConfig);
            if (savedStorage)
            {
                this.DialogResult = true;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dataGridSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridSource.SelectedItem && currentType == MapType.ViewTemplate)
                {
                    ItemInfo sourceItem = (ItemInfo)dataGridSource.SelectedItem;
                    View selectedTemplate = sourceItem.Item as View;
                    if (null != selectedTemplate)
                    {
                        ViewType selectedViewType = selectedTemplate.ViewType;

                        List<ItemInfo> rItemList = new List<ItemInfo>();
                        var rItems = from item in recipientItems.Values where !item.Mapped && item.MapTypeEnum == currentType select item;
                        foreach (ItemInfo itemInfo in rItems)
                        {
                            View viewItem = itemInfo.Item as View;
                            if (null != viewItem)
                            {
                                if (viewItem.ViewType == selectedViewType)
                                {
                                    itemInfo.Enabled = true;
                                }
                                else
                                {
                                    itemInfo.Enabled = false;
                                }
                                rItemList.Add(itemInfo);
                            }
                        }

                        rItemList = rItemList.OrderBy(o => o.ItemName).ToList();
                        dataGridRecipient.ItemsSource = null;
                        dataGridRecipient.ItemsSource = rItemList;
                    } 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a source item.\n"+ex.Message, "Select a Source Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void checkBoxWorkset_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)checkBoxWorkset.IsChecked)
            {
                viewConfig.ApplyWorksetVisibility = true;
            }
        }

        private void checkBoxWorkset_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)checkBoxWorkset.IsChecked)
            {
                viewConfig.ApplyWorksetVisibility = false;
            }
        }

    }

    public class ItemInfo
    {
        private Element item = null;
        private Workset worksetItem = null;
        private string itemName = "";
        private int itemId = -1;
        private ElementId itemElementId = ElementId.InvalidElementId;
        private WorksetId worksetItemId = WorksetId.InvalidWorksetId;
        private MapType mapTypeEnum = MapType.None;
        private bool mapped = false;
        private bool enabled = true;

        public Element Item { get { return item; } set { item = value; } }
        public Workset WorksetItem { get { return worksetItem; } set { worksetItem = value; } }
        public string ItemName { get { return itemName; } set { itemName = value; } }
        public int ItemId { get { return itemId; } set { itemId = value; } }
        public ElementId ItemElementId { get { return itemElementId; } set { itemElementId = value; } }
        public WorksetId WorksetItemId { get { return worksetItemId; } set { worksetItemId = value; } }
        public MapType MapTypeEnum { get { return mapTypeEnum; } set { mapTypeEnum = value; } }
        public bool Mapped { get { return mapped; } set { mapped = value; } }
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        public ItemInfo() { }

        public ItemInfo(Element elem, MapType mType)
        {
            item = elem;
            itemName = elem.Name;
            itemElementId = elem.Id;
            itemId = itemElementId.IntegerValue;
            mapTypeEnum = mType;
        }

        public ItemInfo(Workset ws, MapType mType)
        {
            worksetItem = ws;
            itemName = worksetItem.Name;
            worksetItemId = worksetItem.Id;
            itemId = worksetItemId.IntegerValue;
            mapTypeEnum = mType;
        }
    }
}
