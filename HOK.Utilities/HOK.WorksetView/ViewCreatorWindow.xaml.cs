using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.ObjectModel;
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
using System.ComponentModel;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.WorksetView
{
    /// <summary>
    /// Interaction logic for ViewCreatorWindow.xaml
    /// </summary>
    public partial class ViewCreatorWindow : Window
    {
        private UIApplication m_app;
        private Document m_doc;
  
        private ViewFamilyType view3dFamilyType = null;
        private ViewFamilyType viewPlanFamilyType = null;
        private List<string> viewByList = new List<string>() { "Design Options","Phase","RVT Links", "Workset"};
        private List<ItemInfo> sourceItems = new List<ItemInfo>();
        private ObservableCollection<ItemInfo> selectedItems = new ObservableCollection<ItemInfo>();
        private ObservableCollection<Level> levelItems = new ObservableCollection<Level>();

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public ViewCreatorWindow(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            InitializeComponent();

            this.Title = "View Creator v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            GetViewFamilyType();

            CollectLevels();
            CollectSourceItems();

            comboBoxViewBy.ItemsSource = null;
            comboBoxViewBy.ItemsSource = viewByList;
            comboBoxViewBy.SelectedIndex = 0;
        }

        private void GetViewFamilyType()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_app.ActiveUIDocument.Document);
                List<ViewFamilyType> elements = collector.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().ToList();
                var types = from type in elements where type.ViewFamily == ViewFamily.ThreeDimensional select type;
                view3dFamilyType = types.First();

                types = from type in elements where type.ViewFamily == ViewFamily.FloorPlan select type;
                viewPlanFamilyType = types.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get view family type.\n" + ex.Message, "Get 3D View Family Type", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CollectLevels()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Level> levels = collector.OfClass(typeof(Level)).WhereElementIsNotElementType().ToElements().Cast<Level>().ToList();
                levels = levels.OrderBy(o => o.Name).ToList();
                levelItems = new ObservableCollection<Level>(levels);

                comboBoxLevel.ItemsSource = null;
                comboBoxLevel.ItemsSource = levelItems;
                comboBoxLevel.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect levels.\n" + ex.Message, "Collect Levels", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CollectSourceItems()
        {
            try
            {
                sourceItems = new List<ItemInfo>();

                //workset
                FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(m_doc);
                IList<Workset> worksets = worksetCollector.ToWorksets();
                var userWorksets = from ws in worksets where ws.Kind == WorksetKind.UserWorkset select ws;
                if (userWorksets.Count() > 0)
                {
                    foreach (Workset workset in userWorksets)
                    {
                        ItemInfo info = new ItemInfo(workset, ViewBy.Workset);
                        sourceItems.Add(info);
                    }
                }
                
                //phase
                PhaseArray phases = m_doc.Phases;
                if (phases.Size > 0)
                {
                    foreach (Phase phase in phases)
                    {
                        ItemInfo info = new ItemInfo(phase, ViewBy.Phase);
                        sourceItems.Add(info);
                    }
                }

                //design option
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<DesignOption> options = collector.OfClass(typeof(DesignOption)).ToElements().Cast<DesignOption>().ToList();
                foreach (DesignOption dOption in options)
                {
                    ItemInfo info = new ItemInfo(dOption, ViewBy.DesignOption);
                    sourceItems.Add(info);
                }

                //linked models
                collector = new FilteredElementCollector(m_doc);
                IList<RevitLinkType> types = collector.OfClass(typeof(RevitLinkType)).ToElements().Cast<RevitLinkType>().ToList();
                foreach (RevitLinkType linkType in types)
                {
                    ItemInfo info = new ItemInfo(linkType, ViewBy.Link);
                    sourceItems.Add(info);
                }

                sourceItems = sourceItems.OrderBy(o => o.ItemName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect source items.\n"+ex.Message, "Collect Source Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplaySourceItems()
        {
            try
            {
                int index = comboBoxViewBy.SelectedIndex;
                if (index>-1)
                {
                    string selectedViewBy = viewByList[index];
                    ViewBy selectedType = ViewBy.None;
                    switch (selectedViewBy)
                    {
                        case "Workset":
                            selectedType = ViewBy.Workset;
                            break;
                        case "Phase":
                            selectedType = ViewBy.Phase;
                            break;
                        case "Design Options":
                            selectedType = ViewBy.DesignOption;
                            break;
                        case "RVT Links":
                            radioButton3D.IsChecked = true;
                            selectedType = ViewBy.Link;
                            break;
                    }

                    listBoxItems.ItemsSource = null;
                    var sources = from item in sourceItems where item.ItemType == selectedType select item;
                    if (sources.Count() > 0)
                    {
                        selectedItems = new ObservableCollection<ItemInfo>(sources);
                        listBoxItems.ItemsSource = selectedItems;
                    }
                }
              
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display source items\n" + ex.Message, "Display Source Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxViewBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxViewBy.SelectedItem)
                {
                    DisplaySourceItems();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select source Type.\n" + ex.Message, "Select View By", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    selectedItems[i].IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all items.\n" + ex.Message, "Check all items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    selectedItems[i].IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck all items.\n" + ex.Message, "Uncheck all items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkedItems = from item in selectedItems where item.IsSelected select item;
                if (checkedItems.Count() > 0)
                {
                    List<View> createdViews = new List<View>();
                    bool create3dView = (bool)radioButton3D.IsChecked;
                    bool overwrite = (bool)checkBoxOverwrite.IsChecked;
                    statusLable.Text = "Creating Views..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

                    progressBar.Minimum = 0;
                    progressBar.Maximum = checkedItems.Count();
                    progressBar.Value = 0;

                    double value = 0;

                    foreach (ItemInfo item in checkedItems)
                    {
                        
                        if (create3dView)
                        {
                            View3D viewCreated = ViewCreator.Create3DView(m_doc, item, view3dFamilyType, overwrite);
                            if (null != viewCreated)
                            {
                                createdViews.Add(viewCreated);
                            }
                        }
                        else
                        {
                            if(null!= comboBoxLevel.SelectedItem)
                            {
                                ViewPlan viewCreated = ViewCreator.CreateFloorPlan(m_doc, item, viewPlanFamilyType, (Level)comboBoxLevel.SelectedItem, overwrite);
                                if (null != viewCreated)
                                {
                                    createdViews.Add(viewCreated);
                                }
                            }
                        }
                        value++;
                        Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                    }

                    if (createdViews.Count > 0)
                    {
                        MessageBox.Show(createdViews.Count + " views are created.", "Views Created", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    statusLable.Text = "Ready";
                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create views.\n" + ex.Message, "Create Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public enum ViewBy
    {
        DesignOption, Phase, Workset, Link, None
    }

    public class ItemInfo : INotifyPropertyChanged
    {
        private object itemObj = null;
        private ViewBy itemType = ViewBy.None;
        private string itemName = "";
        private long itemId = -1;
        private bool isSelected = false;

        public object ItemObj { get { return itemObj; } set { itemObj = value; NotifyPropertyChanged("ItemObj"); } }
        public ViewBy ItemType { get { return itemType; } set { itemType = value; NotifyPropertyChanged("ItemType"); } }
        public string ItemName { get { return itemName; } set { itemName = value; NotifyPropertyChanged("ItemName"); } }
        public long ItemId { get { return itemId; } set { itemId = value; NotifyPropertyChanged("ItemId"); } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public ItemInfo(object obj, ViewBy type)
        {
            itemObj = obj;
            itemType = type;
            switch (type)
            {
                case ViewBy.Workset:
                    Workset workset = obj as Workset;
                    if (null != workset)
                    {
                        itemName = workset.Name;
                        itemId = workset.Id.IntegerValue;
                    }
                    break;
                case ViewBy.Phase:
                    Phase phase = obj as Phase;
                    if (null != phase)
                    {
                        itemName = phase.Name;
                        itemId = GetElementIdValue(phase.Id);
                    }
                    break;
                case ViewBy.DesignOption:
                    DesignOption designOption = obj as DesignOption;
                    if (null != designOption)
                    {
                        itemName = designOption.Name;
                        itemId = GetElementIdValue(designOption.Id);
                    }
                    break;
                case ViewBy.Link:
                    RevitLinkType linktype = obj as RevitLinkType;
                    if (null != linktype)
                    {
                        itemName = linktype.Name;
                        itemId = GetElementIdValue(linktype.Id);
                    }
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
    }
}
