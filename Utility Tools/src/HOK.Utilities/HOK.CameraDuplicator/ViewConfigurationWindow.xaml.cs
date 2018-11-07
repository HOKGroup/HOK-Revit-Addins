using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;

namespace HOK.CameraDuplicator
{
    /// <summary>
    /// Interaction logic for ViewConfigurationWindow.xaml
    /// </summary>
    public partial class ViewConfigurationWindow
    {
        private ModelInfo sModelInfo;
        private ModelInfo rModelInfo;

        private Dictionary<int, ItemInfo> sourceItems = new Dictionary<int, ItemInfo>();
        private Dictionary<int, ItemInfo> recipientItems = new Dictionary<int, ItemInfo>();

        private MapType currentType = MapType.None;

        public ViewConfiguration ViewConfig { get; set; } = new ViewConfiguration();

        public ViewConfigurationWindow(ModelInfo source, ModelInfo recipient, ViewConfiguration vc)
        {
            sModelInfo = source;
            rModelInfo = recipient;
            ViewConfig = vc;

            InitializeComponent();
            labelSource.Content = sModelInfo.ModelName;
            labelTarget.Content = rModelInfo.ModelName;

            checkBoxWorkset.IsChecked = vc.ApplyWorksetVisibility;
            GetItems();
            //remove non-existing mapping items
            for (var i = ViewConfig.MapItems.Count - 1; i > -1; i--)
            {
                var mapItemInfo = ViewConfig.MapItems[i];
                if (!sourceItems.ContainsKey(mapItemInfo.SourceItemId) || !recipientItems.ContainsKey(mapItemInfo.RecipientItemId))
                {
                    ViewConfig.MapItems.RemoveAt(i);
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

                var currentMaps = from map in ViewConfig.MapItems where map.SourceModelId == sModelInfo.ModelId && map.RecipientModelId == rModelInfo.ModelId select map;
                foreach (var mapInfo in currentMaps)
                {
                    var sItemId = mapInfo.SourceItemId;
                    var rItemId = mapInfo.RecipientItemId;

                    if (sourceItems.ContainsKey(sItemId) && recipientItems.ContainsKey(rItemId))
                    {
                        var sItem = sourceItems[sItemId];
                        if (!sItem.Mapped)
                        {
                            sItem.Mapped = true;
                            sourceItems.Remove(sItemId);
                            sourceItems.Add(sItemId, sItem);
                        }

                        var rItem = recipientItems[rItemId];
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
                var sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                var sLevels = sCollector.OfCategory(BuiltInCategory.OST_Levels).ToElements().ToList();
                foreach (var elem in sLevels)
                {
                    var iInfo = new ItemInfo(elem, MapType.Level);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                var rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                var rLevels = rCollector.OfCategory(BuiltInCategory.OST_Levels).ToElements().ToList();
                foreach (var elem in rLevels)
                {
                    var iInfo = new ItemInfo(elem, MapType.Level);
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
                var sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                var sPhases = sCollector.OfCategory(BuiltInCategory.OST_Phases).ToElements().ToList();
                foreach (var elem in sPhases)
                {
                    var iInfo = new ItemInfo(elem, MapType.Phase);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                var rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                var rPhases = rCollector.OfCategory(BuiltInCategory.OST_Phases).ToElements().ToList();
                foreach (var elem in rPhases)
                {
                    var iInfo = new ItemInfo(elem, MapType.Phase);
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
                var types = new List<Type>();
                types.Add(typeof(View3D));
                types.Add(typeof(ViewPlan));
                var multiClassFilter = new ElementMulticlassFilter(types);

                var sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                var sViews = sCollector.WherePasses(multiClassFilter).ToElements().Cast<View>().ToList();
                var sTemplates = from view in sViews where view.IsTemplate select view;
                foreach (var view in sTemplates)
                {
                    var iInfo = new ItemInfo(view, MapType.ViewTemplate);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                var rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                var rViews = sCollector.WherePasses(multiClassFilter).ToElements().Cast<View>().ToList();
                var rTemplates = from view in rViews where view.IsTemplate select view;
                foreach (Element elem in rTemplates)
                {
                    var iInfo = new ItemInfo(elem, MapType.ViewTemplate);
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
                    var collector = new FilteredWorksetCollector(sModelInfo.ModelDoc);
                    var wsFilter = new WorksetKindFilter(WorksetKind.UserWorkset);
                    var worksets = collector.WherePasses(wsFilter).ToWorksets().ToList();
                    foreach (var ws in worksets)
                    {
                        var iInfo = new ItemInfo(ws, MapType.Workset);
                        if(!sourceItems.ContainsKey(iInfo.ItemId))
                        {
                            sourceItems.Add(iInfo.ItemId, iInfo);
                        }
                    }
                }

                if (rModelInfo.ModelDoc.IsWorkshared)
                {
                    var collector = new FilteredWorksetCollector(rModelInfo.ModelDoc);
                    var wsFilter = new WorksetKindFilter(WorksetKind.UserWorkset);
                    var worksets = collector.WherePasses(wsFilter).ToWorksets().ToList();
                    foreach (var ws in worksets)
                    {
                        var iInfo = new ItemInfo(ws, MapType.Workset);
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
                var sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                var sScopeBoxes= sCollector.OfCategory(BuiltInCategory.OST_VolumeOfInterest).ToElements().ToList();
                foreach (var elem in sScopeBoxes)
                {
                    var iInfo = new ItemInfo(elem, MapType.ScopeBox);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                var rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                var rScopeBoxes = rCollector.OfCategory(BuiltInCategory.OST_VolumeOfInterest).ToElements().ToList();
                foreach (var elem in rScopeBoxes)
                {
                    var iInfo = new ItemInfo(elem, MapType.ScopeBox);
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
                var sCollector = new FilteredElementCollector(sModelInfo.ModelDoc);
                var sSchemes = sCollector.OfCategory(BuiltInCategory.OST_AreaSchemes).ToElements().ToList();
                foreach (var elem in sSchemes)
                {
                    var iInfo = new ItemInfo(elem, MapType.AreaScheme);
                    if (!sourceItems.ContainsKey(iInfo.ItemId))
                    {
                        sourceItems.Add(iInfo.ItemId, iInfo);
                    }
                }

                var rCollector = new FilteredElementCollector(rModelInfo.ModelDoc);
                var rSchemes = rCollector.OfCategory(BuiltInCategory.OST_AreaSchemes).ToElements().ToList();
                foreach (var elem in rSchemes)
                {
                    var iInfo = new ItemInfo(elem, MapType.AreaScheme);
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
                var sItemList = sItems.OrderBy(o => o.ItemName).ToList();
                dataGridSource.ItemsSource = null;
                dataGridSource.ItemsSource = sItemList;

                var rItems = from item in recipientItems.Values where !item.Mapped  && item.MapTypeEnum == currentType select item;
                var rItemList = rItems.OrderBy(o => o.ItemName).ToList();
                dataGridRecipient.ItemsSource = null;
                dataGridRecipient.ItemsSource = rItemList;
                
                var currentMap = from map in ViewConfig.MapItems where map.SourceModelId == sModelInfo.ModelId && map.RecipientModelId == rModelInfo.ModelId && map.MapItemType == currentType select map;
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
                    var sItemInfo = (ItemInfo)dataGridSource.SelectedItem;
                    var rItemInfo = (ItemInfo)dataGridRecipient.SelectedItem;

                    if (rItemInfo.Enabled)
                    {
                        var mapItemInfo = new MapItemInfo();
                        mapItemInfo.MapItemType = currentType;
                        mapItemInfo.SourceModelId = sModelInfo.ModelId;
                        mapItemInfo.SourceItemId = sItemInfo.ItemId;
                        mapItemInfo.SourceItemName = sItemInfo.ItemName;
                        mapItemInfo.RecipientModelId = rModelInfo.ModelId;
                        mapItemInfo.RecipientItemId = rItemInfo.ItemId;
                        mapItemInfo.RecipientItemName = rItemInfo.ItemName;
                        ViewConfig.MapItems.Add(mapItemInfo);

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
                    var mapItemInfo = (MapItemInfo)dataGridMap.SelectedItem;
                    var removed = ViewConfig.MapItems.Remove(mapItemInfo);

                    var sItemId = mapItemInfo.SourceItemId;
                    var rItemId = mapItemInfo.RecipientItemId;
                    if (sourceItems.ContainsKey(sItemId) && recipientItems.ContainsKey(rItemId))
                    {
                        var sItemInfo = sourceItems[sItemId];
                        sItemInfo.Mapped = false;
                        sourceItems.Remove(sItemId);
                        sourceItems.Add(sItemId, sItemInfo);

                        var rItemInfo = recipientItems[rItemId];
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
            var savedStorage = ViewConfigDataStorageUtil.StoreViewConfiguration(rModelInfo.ModelDoc, ViewConfig);
            if (savedStorage)
            {
                DialogResult = true;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void dataGridSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridSource.SelectedItem && currentType == MapType.ViewTemplate)
                {
                    var sourceItem = (ItemInfo)dataGridSource.SelectedItem;
                    var selectedTemplate = sourceItem.Item as View;
                    if (null != selectedTemplate)
                    {
                        var selectedViewType = selectedTemplate.ViewType;

                        var rItemList = new List<ItemInfo>();
                        var rItems = from item in recipientItems.Values where !item.Mapped && item.MapTypeEnum == currentType select item;
                        foreach (var itemInfo in rItems)
                        {
                            var viewItem = itemInfo.Item as View;
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
                ViewConfig.ApplyWorksetVisibility = true;
            }
        }

        private void checkBoxWorkset_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)checkBoxWorkset.IsChecked)
            {
                ViewConfig.ApplyWorksetVisibility = false;
            }
        }
    }

    public class ItemInfo
    {
        public Element Item { get; set; }
        public Workset WorksetItem { get; set; }
        public string ItemName { get; set; } = "";
        public int ItemId { get; set; } = -1;
        public ElementId ItemElementId { get; set; } = ElementId.InvalidElementId;
        public WorksetId WorksetItemId { get; set; } = WorksetId.InvalidWorksetId;
        public MapType MapTypeEnum { get; set; } = MapType.None;
        public bool Mapped { get; set; }
        public bool Enabled { get; set; } = true;

        public ItemInfo() { }

        public ItemInfo(Element elem, MapType mType)
        {
            Item = elem;
            ItemName = elem.Name;
            ItemElementId = elem.Id;
            ItemId = ItemElementId.IntegerValue;
            MapTypeEnum = mType;
        }

        public ItemInfo(Workset ws, MapType mType)
        {
            WorksetItem = ws;
            ItemName = WorksetItem.Name;
            WorksetItemId = WorksetItem.Id;
            ItemId = WorksetItemId.IntegerValue;
            MapTypeEnum = mType;
        }
    }
}
