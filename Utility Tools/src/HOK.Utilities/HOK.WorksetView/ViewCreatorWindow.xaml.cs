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
using System.IO;
using Newtonsoft.Json;
using System.Drawing;

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
        private List<string> viewByList = new List<string>() { "Design Options","Phase","RVT Links", "Workset","From File"};
        private List<ItemInfo> sourceItems = new List<ItemInfo>();
        private ObservableCollection<ItemInfo> selectedItems = new ObservableCollection<ItemInfo>();
        private ObservableCollection<DataGrid> selectedItemsGrid = new ObservableCollection<DataGrid>();
        private ObservableCollection<Level> levelItems = new ObservableCollection<Level>();
        private JsonFileParameters Data= null;
        private List<View> TemplateViews= new List<View>();
        public List<List<Category>> CategoryList2D = new List<List<Category>>();
        public List<List<Category>> CategoryList3D = new List<List<Category>>();
        public List<TemplateChoosedDataGrid> templateChoosedDataGrids = new List<TemplateChoosedDataGrid>();

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        public ObservableCollection<string> Templates { get; private set; }
        public ViewCreatorWindow(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            InitializeComponent();
            
            var tr = System.Reflection.Assembly.GetExecutingAssembly().GetName();
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

                //All Views
                collector = new FilteredElementCollector(m_doc);
                IList<View> Views = collector.OfClass(typeof(View)).ToElements().Cast<View>().ToList();
                foreach (View V in Views)
                {
                    if (V.IsTemplate)
                    {
                        TemplateViews.Add(V);
                    }
                    else
                    {
                        ItemInfo info = new ItemInfo(V, ViewBy.Category);
                        sourceItems.Add(info);
                    }

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
                        case "From File":
                            radioButton3D.IsChecked = true;
                            selectedType = ViewBy.Category;
                            break;
                    }

                    listBoxItems.ItemsSource = null;
                    List<ItemInfo> sources = new List<ItemInfo>();
                    if (viewByList[index] == "From File")
                    {
                        Categories DocCategories = m_doc.Settings.Categories;
                        List<Category> Categories = new List<Category>();
                        foreach(Category category in DocCategories)
                        {
                            Categories.Add(category);
                        }
                        if (Data != null)
                        {
                            foreach(var Da in Data.Data.Views)
                            {
                                ItemInfo item = new ItemInfo(Da.Viewname,ViewBy.Category);
                                item.ItemName = Da.Viewname;
                                item.IsSelected = false;
                                List<Category> TwoDCats = new List<Category>();
                                List<Category> ThreeDCats = new List<Category>();
                                if (Da.Visible2Dcateogries.Count > 0)
                                {
                                    foreach(var JsonTwoDCat in Da.Visible2Dcateogries)
                                    {
                                        var TwoDCat = Categories.Where(m => m.Name == JsonTwoDCat).FirstOrDefault();
                                        
                                        if(TwoDCat != null)
                                        {
                                            TwoDCats.Add(TwoDCat);
                                        }
                                    }
                                }
                                if (Da.Visible3Dcateogries.Count > 0)
                                {
                                    foreach (var JsonThreeDCat in Da.Visible3Dcateogries)
                                    {
                                        var ThreeDCat = Categories.Where(m => m.Name == JsonThreeDCat).FirstOrDefault();
                                        if (ThreeDCat != null)
                                        {
                                            ThreeDCats.Add(ThreeDCat);
                                        }

                                    }
                                }
                                CategoryList2D.Add(TwoDCats);
                                CategoryList3D.Add(ThreeDCats);
                                sources.Add(item);
                            }
                            
                           /* var items = (from item in sourceItems where item.ItemType == selectedType select item).ToList();
                            foreach (var item in items)
                            {
                                var ViewName = (item.ItemObj as View).Name;
                                var DataNames=Data.data.views.Select(m=>m.viewname).ToList();
                                if (DataNames.Contains(ViewName))
                                {
                                    sources.Add(item);
                                }
                            }*/

                        }
                    }
                    else
                    {
                        sources = (from item in sourceItems where item.ItemType == selectedType select item).ToList();
                    }

                    if (sources.Count() > 0)
                    {   
                        List<DataGrid> dataGrids = new List<DataGrid>();
                        foreach (var item in sources)
                        {
                            DataGrid dataGrid = new DataGrid();
                            dataGrid.CheckBox = false;
                            dataGrid.ItemName=item.ItemName;
                            dataGrids.Add(dataGrid);
                        }
                        selectedItems = new ObservableCollection<ItemInfo>(sources);
                        selectedItemsGrid = new ObservableCollection<DataGrid>(dataGrids);
                        listBoxItems.ItemsSource = selectedItemsGrid;
                        ObservableCollection<string> templates= new ObservableCollection<string>();
                        templates.Add("None");
                        foreach(var template in TemplateViews)
                        {
                            templates.Add(template.Name);
                        }

                        Templates =templates;
                        if (listBoxItems.Columns.Count <= 1)
                        {
                            listBoxItems.Columns[0].Visibility = System.Windows.Visibility.Hidden;
                            listBoxItems.Columns[0].Header = " ";
                        }
                        else
                        {
                            listBoxItems.Columns[0].Header = "Templates";
                            listBoxItems.Columns[0].Visibility = System.Windows.Visibility.Visible;
                            listBoxItems.Columns[0].DisplayIndex = 2;
                            listBoxItems.Columns[1].Header = " ";
                            listBoxItems.Columns[2].Header = "Item Name";
                            listBoxItems.Columns[0].Width = 400 - 205;
                            listBoxItems.Columns[1].Width = 20;
                            listBoxItems.Columns[2].Width = 140;
                        }


                    }
                    else
                    {
                        listBoxItems.Columns[0].Visibility = System.Windows.Visibility.Hidden;
                        listBoxItems.Columns[0].Header = " ";
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
                    if (comboBoxViewBy.SelectedItem.ToString()=="From File")
                    {
                        ChooseFile.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        ChooseFile.Visibility = System.Windows.Visibility.Hidden;
                    }
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
                var checkedItemsGrids = from item in selectedItemsGrid where item.CheckBox select item;
                List<int> CheckItemsCount= new List<int>();
                int CheckedCOunt= 0;
                foreach(var CheckedItem in selectedItemsGrid)
                {
                    if (CheckedItem.CheckBox)
                    {
                        CheckItemsCount.Add(CheckedCOunt);
                    }
                    CheckedCOunt++;
                }
                var checkedItems=new List<ItemInfo>();
                foreach(var checkedItem in checkedItemsGrids)
                {
                    var Item = selectedItems.Where(m => m.ItemName == checkedItem.ItemName).FirstOrDefault();
                    checkedItems.Add(Item);
                }
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
                    int count = 0;
                    foreach(ItemInfo item in checkedItems)
                    {
                        List<Category> CatTwoD = new List<Category>();
                        List<Category> CatThreeD = new List<Category>();
                        if (CategoryList2D.Count == 0)
                        {
                            Category cat = null;
                            CatTwoD.Add(cat);
                        }
                        if (CategoryList3D.Count == 0)
                        {
                            Category cat = null;
                            CatThreeD.Add(cat);
                        }
                        CategoryList2D.Add(CatTwoD);
                        CategoryList3D.Add(CatThreeD);
                    }
                    foreach (ItemInfo item in checkedItems)
                    {
                        string TemplateChoosed = templateChoosedDataGrids.Where(m => m.RowNumber == CheckItemsCount[count]).Select(m => m.SelectedValue).FirstOrDefault();
                        List<View> TemplateView=TemplateViews.Where(m=>m.Name == TemplateChoosed).ToList();
                        if (create3dView)
                        {
                            View3D viewCreated = ViewCreator.Create3DView(m_doc, item, view3dFamilyType, overwrite,CategoryList2D[count],CategoryList3D[count],TemplateView);
                            if (null != viewCreated)
                            {
                                createdViews.Add(viewCreated);
                            }
                        }
                        else
                        {
                            if(null!= comboBoxLevel.SelectedItem)
                            {
                                ViewPlan viewCreated = ViewCreator.CreateFloorPlan(m_doc, item, viewPlanFamilyType, (Level)comboBoxLevel.SelectedItem, overwrite, CategoryList2D[count], CategoryList3D[count], TemplateView);
                                if (null != viewCreated)
                                {
                                    createdViews.Add(viewCreated);
                                }
                            }
                        }
                        value++;
                        count++;
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

        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.ShowDialog();
            string JsonLocation = openFileDialog.FileName;
            if(JsonLocation != null)
            {
                string path = Environment.ExpandEnvironmentVariables(JsonLocation);
                string jsonFromFileUpdated;
                using (var reader = new StreamReader(path))
                {
                    jsonFromFileUpdated = reader.ReadToEnd();
                }
                Data= JsonConvert.DeserializeObject<JsonFileParameters>(jsonFromFileUpdated);
                if (Data != null)
                {
                    DisplaySourceItems();
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string SelectedValue= (sender as System.Windows.Controls.ComboBox).SelectedValue.ToString();
            var RowNumber = (((sender as System.Windows.Controls.ComboBox).BindingGroup.Owner) as DataGridRow).AlternationIndex;
            var CheckTemplateChoosed = templateChoosedDataGrids.Where(m => m.RowNumber == RowNumber).FirstOrDefault();
            if(CheckTemplateChoosed != null)
            {
                CheckTemplateChoosed.SelectedValue = SelectedValue;
            }
            else
            {
                templateChoosedDataGrids.Add(new TemplateChoosedDataGrid
                {
                    RowNumber = RowNumber,
                    SelectedValue = SelectedValue
                });
            } 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (listBoxItems.Columns.Count <= 1)
            {
                listBoxItems.Columns[0].Visibility=System.Windows.Visibility.Hidden;
                listBoxItems.Columns[0].Header = " ";
            }
            else
            {
                listBoxItems.Columns[0].Header = "Templates";
                listBoxItems.Columns[0].Visibility = System.Windows.Visibility.Visible;
                listBoxItems.Columns[0].DisplayIndex = 2;
                listBoxItems.Columns[1].Header = " ";
                listBoxItems.Columns[2].Header = "Item Name";
                listBoxItems.Columns[0].Width = 400 - 205;
                listBoxItems.Columns[1].Width = 20;
                listBoxItems.Columns[2].Width = 140;
            }

        }
    }

    public enum ViewBy
    {
        DesignOption, Phase, Workset, Link,Category, None
    }

    public class ItemInfo : INotifyPropertyChanged
    {
        private object itemObj = null;
        private ViewBy itemType = ViewBy.None;
        private string itemName = "";
        private int itemId = -1;
        private bool isSelected = false;

        public object ItemObj { get { return itemObj; } set { itemObj = value; NotifyPropertyChanged("ItemObj"); } }
        public ViewBy ItemType { get { return itemType; } set { itemType = value; NotifyPropertyChanged("ItemType"); } }
        public string ItemName { get { return itemName; } set { itemName = value; NotifyPropertyChanged("ItemName"); } }
        public int ItemId { get { return itemId; } set { itemId = value; NotifyPropertyChanged("ItemId"); } }
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
                        itemId = phase.Id.IntegerValue;
                    }
                    break;
                case ViewBy.DesignOption:
                    DesignOption designOption = obj as DesignOption;
                    if (null != designOption)
                    {
                        itemName = designOption.Name;
                        itemId = designOption.Id.IntegerValue;
                    }
                    break;
                case ViewBy.Link:
                    RevitLinkType linktype = obj as RevitLinkType;
                    if (null != linktype)
                    {
                        itemName = linktype.Name;
                        itemId = linktype.Id.IntegerValue;
                    }
                    break;
                case ViewBy.Category:
                    View View = obj as View;
                    if (null != View)
                    {
                        itemName = View.Name;
                        itemId = View.Id.IntegerValue;
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

    public class DataGrid
    {
        public bool CheckBox { get; set; }
        public string ItemName { get; set; }

    }
    public class TemplateChoosedDataGrid
    {
        public string SelectedValue { get; set; }
        public int RowNumber { get; set; }
    }

}
