using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using HOK.ColorSchemeEditor.BCFUtils;
using System.Collections.ObjectModel;
using Autodesk.Revit.DB;
using System.IO;
using WinForm = System.Windows.Forms;
using HOK.ColorSchemeEditor.WPFClasses;

namespace HOK.ColorSchemeEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UIApplication m_app;
        private Document m_doc;

        private BCFUtil bcfUtil;
        private BCFZIP bcfZip = new BCFZIP();
        private ColorSchemeInfo colorSchemeInfo = new ColorSchemeInfo();
        private ColorScheme selectedColorScheme = new ColorScheme();
        private Random random = new Random();
        private bool userInput = false;
        private ColorEditorSettings colorEditorSettings=new ColorEditorSettings();

        private const String m_noneParam = "(none)";
        private List<CategoryInfo> categoryInfoList = new List<CategoryInfo>();
        private List<ViewInfo> viewInfoList = new List<ViewInfo>();
        private Dictionary<int, ElementProperties> filteredElements = new Dictionary<int, ElementProperties>();
        private SortableObservableCollection<ListViewModel> sortableSchemeViews = new SortableObservableCollection<ListViewModel>();
       
        public MainWindow(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            bcfUtil = new BCFUtil(uiapp);
            InitializeComponent();
            this.Title = "Color Scheme Editor v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            DisplayViewLists();

            dataGridSchemes.DataContext = sortableSchemeViews;
            colorEditorSettings = DataStorageUtil.ReadDataStorage(m_doc);
            if (!string.IsNullOrEmpty(colorEditorSettings.BCFPath))
            {
                OpenBCF(colorEditorSettings.BCFPath);
            }
        }

        #region UI events
        private void OpenBCF(string bcfPath)
        {
            try
            {
                textBoxFilePath.Text = bcfPath;

                bcfZip = new BCFZIP();
                colorSchemeInfo = new ColorSchemeInfo();
                selectedColorScheme = new ColorScheme();
                filteredElements = new Dictionary<int, ElementProperties>();

                sortableSchemeViews.Clear();
                comboBoxColor.ItemsSource = null;
                dataGridDefinition.ItemsSource = null;

                bcfZip = bcfUtil.ReadBCF(bcfPath);

                DisplayBCF(bcfZip);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open BCF.\n"+ex.Message, "Open BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //edit categories and parameter filters
        private void buttonCategories_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedColorScheme)
                {
                    CategoryWindow categoryWindow = new CategoryWindow(m_app, selectedColorScheme);
                    categoryWindow.Owner = this;
                    if (categoryWindow.ShowDialog() == true)
                    {
                        List<Category> selectedCategories = new List<Category>();
                        selectedCategories = categoryWindow.UserSelectedList;
                        List<Element> selectedElements = new List<Element>();
                        selectedElements = categoryWindow.SelectedElements;
                        List<FilterRule> selectedFilterRules = new List<FilterRule>();
                        selectedFilterRules = categoryWindow.SelectedRules;
                        bool includeLinks = categoryWindow.IncludeLinks;
                        categoryWindow.Close();

                        ICollection<ElementId> filterCatIds = new List<ElementId>();
                        List<string> catNames = new List<string>();
                        foreach (Category category in selectedCategories)
                        {
                            filterCatIds.Add(category.Id);
                            catNames.Add(category.Name);
                        }

                        selectedColorScheme.Categories = catNames;
                        selectedColorScheme.FilteredElements = selectedElements;
                        selectedColorScheme.FilterRules = selectedFilterRules;
                        selectedColorScheme.IncludeLinks = includeLinks;

                        SelectParameter(selectedColorScheme);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select categories.\n" + ex.Message, "Select Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //create a new color scheme
        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorScheme colorScheme = new ColorScheme();
                var names = from scheme in colorSchemeInfo.ColorSchemes select scheme.SchemeName;
                List<string> schemeNames = new List<string>();
                schemeNames = names.ToList();
                NameWindow nameWindow = new NameWindow("New Color Scheme", "Color Scheme", "", schemeNames);
                nameWindow.Owner = this;
                if (nameWindow.ShowDialog() == true)
                {
                    colorScheme.SchemeName = nameWindow.NameText;
                    colorScheme.SchemeId = Guid.NewGuid().ToString();
                    nameWindow.Close();

                    CategoryWindow categoryWindow = new CategoryWindow(m_app);
                    categoryWindow.Owner = this;
                    if (categoryWindow.ShowDialog() == true)
                    {
                        List<Category> selectedCategories = new List<Category>();
                        selectedCategories = categoryWindow.UserSelectedList;
                        List<Element> selectedElements = new List<Element>();
                        selectedElements = categoryWindow.SelectedElements;
                        List<FilterRule> selectedFilterRules = new List<FilterRule>();
                        selectedFilterRules = categoryWindow.SelectedRules;
                        bool includeLinks = categoryWindow.IncludeLinks;

                        categoryWindow.Close();

                        ICollection<ElementId> filterCatIds = new List<ElementId>();
                        List<string> catNames = new List<string>();
                        foreach (Category category in selectedCategories)
                        {
                            filterCatIds.Add(category.Id);
                            catNames.Add(category.Name);
                        }

                        colorScheme.IncludeLinks = includeLinks;
                        colorScheme.Categories = catNames;
                        colorScheme.FilteredElements = selectedElements;
                        colorScheme.FilterRules = selectedFilterRules;
                        colorScheme.DefinitionBy = DefinitionType.ByValue;
                        
                        ViewInfo viewInfo=comboBoxView.SelectedItem as ViewInfo;
                        if (null != viewInfo) 
                        {
                            colorScheme.SelectedViewInfo = viewInfo;
                            colorScheme.ViewName = viewInfo.ViewName;  
                        }

                        colorSchemeInfo.ColorSchemes.Add(colorScheme);
                        ListViewModel viewModel = new ListViewModel(colorScheme);
                        sortableSchemeViews.Add(viewModel);
                        sortableSchemeViews.Sort();

                        dataGridSchemes.SelectedItem = viewModel;

                        buttonSaveBCF.IsEnabled = true;
                        buttonSaveAsBCF.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a new color scheme.\n" + ex.Message, "Create a New Color Scheme", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //make a copy of a color scheme
        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedColorScheme)
                {
                    ColorScheme copiedScheme = new ColorScheme(selectedColorScheme);
                    var names = from scheme in colorSchemeInfo.ColorSchemes select scheme.SchemeName;
                    List<string> schemeNames = new List<string>();
                    schemeNames = names.ToList();
                    NameWindow nameWindow = new NameWindow("Copy Color Scheme", "Color Scheme", selectedColorScheme.SchemeName + "_Copy", schemeNames);
                    nameWindow.Owner = this;
                    if (nameWindow.ShowDialog() == true)
                    {
                        copiedScheme.SchemeName = nameWindow.NameText;
                        copiedScheme.SchemeId = Guid.NewGuid().ToString();
                        nameWindow.Close();

                        colorSchemeInfo.ColorSchemes.Add(copiedScheme);
                        ListViewModel viewModel = new ListViewModel(copiedScheme);
                        sortableSchemeViews.Add(viewModel);
                        sortableSchemeViews.Sort();
                       
                        dataGridSchemes.SelectedItem = viewModel;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy the slected color scheme.\n" + ex.Message, "Copy a Color Scheme", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //rename a color scheme
        private void buttonRename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedColorScheme)
                {
                    string oldName = selectedColorScheme.SchemeName;

                    var names = from scheme in colorSchemeInfo.ColorSchemes select scheme.SchemeName;
                    List<string> schemeNames = new List<string>();
                    schemeNames = names.ToList();

                    NameWindow nameWindow = new NameWindow("Rename Color Scheme", "Color Scheme", selectedColorScheme.SchemeName, schemeNames);
                    nameWindow.Owner = this;
                    if (nameWindow.ShowDialog() == true)
                    {
                        string newName = nameWindow.NameText;
                        selectedColorScheme.SchemeName = newName;
                        nameWindow.Close();

                        for (int i = 0; i < colorSchemeInfo.ColorSchemes.Count; i++)
                        {
                            ColorScheme scheme = colorSchemeInfo.ColorSchemes[i];
                            if (scheme.SchemeId == selectedColorScheme.SchemeId)
                            {
                                colorSchemeInfo.ColorSchemes.RemoveAt(i);
                            }
                        }
                        colorSchemeInfo.ColorSchemes.Add(selectedColorScheme);
                    }

                    int selectedIndex = 0;
                    for (int i = 0; i < sortableSchemeViews.Count; i++)
                    {
                        ListViewModel listViewModel = sortableSchemeViews[i];
                        ColorScheme scheme = listViewModel.ItemContent;
                        if (scheme.SchemeId == selectedColorScheme.SchemeId)
                        {
                            listViewModel = new ListViewModel(selectedColorScheme);
                            sortableSchemeViews[i] = listViewModel;
                            selectedIndex = i;
                            break;
                        }
                    }
                    dataGridSchemes.SelectedItem = sortableSchemeViews[selectedIndex];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to rename the selected color scheme.\n" + ex.Message, "Rename Color Scheme", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        //delete a color scheme
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedColorScheme && null!=dataGridSchemes.SelectedItem)
                {
                    sortableSchemeViews.Remove((ListViewModel)dataGridSchemes.SelectedItem);

                    for (int i = 0; i < colorSchemeInfo.ColorSchemes.Count; i++)
                    {
                        ColorScheme scheme = colorSchemeInfo.ColorSchemes[i];
                        if (scheme.SchemeId == selectedColorScheme.SchemeId)
                        {
                            colorSchemeInfo.ColorSchemes.RemoveAt(i); break;
                        }
                    }
           
                    comboBoxColor.ItemsSource = null;
                    dataGridDefinition.ItemsSource = null;

                    if (colorSchemeInfo.ColorSchemes.Count > 0)
                    {
                        dataGridSchemes.SelectedItem = sortableSchemeViews[0];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete the selected color scheme.\n" + ex.Message, "Delete Color Scheme", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //add definitions
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedColorScheme.DefinitionBy == DefinitionType.ByValue)
                {
                    var definitions = from definition in selectedColorScheme.ColorDefinitions select definition.ParameterValue;
                    List<string> definitionNames = new List<string>();
                    definitionNames = definitions.ToList();

                    NameWindow nameWindow = new NameWindow("New Color Scheme Entry", "Color Scheme Entry", "", definitionNames, selectedColorScheme.SelectedParamInfo.ParamStorageType);
                    nameWindow.Owner = this;
                    if (nameWindow.ShowDialog() == true)
                    {
                        string name = nameWindow.NameText;
                        nameWindow.Close();
                  
                        var foundNames = from colorDef in selectedColorScheme.ColorDefinitions where colorDef.ParameterValue == name select colorDef;
                        if (foundNames.Count() > 0)
                        {
                            MessageBox.Show(name + " already exist in the color scheme.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            ColorDefinition definition = new ColorDefinition();
                            definition.UserDefined = true;
                            definition.ParameterValue = name;
                            definition.InUse = "No";

                            byte[] colorBytes = new byte[3];
                            colorBytes[0] = (byte)random.Next(256);
                            colorBytes[1] = (byte)random.Next(256);
                            colorBytes[2] = (byte)random.Next(256);
                            definition.Color = colorBytes;

                            System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(definition.Color[0], definition.Color[1], definition.Color[2]);
                            definition.BackgroundColor = new SolidColorBrush(windowColor);
                            definition.RGB = definition.Color[0].ToString() + " - " + definition.Color[1].ToString() + " - " + definition.Color[2].ToString();

                            selectedColorScheme.CustomColorDefinitions.Add(definition);
                            selectedColorScheme.ColorDefinitions.Add(definition);
                            DisplayDefinitions(selectedColorScheme);
                        }
                    }
                }
                else if (selectedColorScheme.DefinitionBy == DefinitionType.ByRange)
                {
                    RangeWindow rangeWindow = new RangeWindow();
                    rangeWindow.Owner = this;
                    if (rangeWindow.ShowDialog() == true)
                    {
                        double minValue = rangeWindow.MinValue;
                        double maxValue = rangeWindow.MaxValue;

                        rangeWindow.Close();

                        List<double> rangeValues = new List<double>();
                        if (minValue >= double.MinValue) { rangeValues.Add(minValue); }
                        if (maxValue <= double.MaxValue) { rangeValues.Add(maxValue); }

                        foreach (ColorDefinition definition in selectedColorScheme.ColorDefinitions)
                        {
                            if ((minValue > definition.MinimumValue) || (maxValue < definition.MinimumValue)) { rangeValues.Add(definition.MinimumValue); }
                            if ((minValue > definition.MaximumValue) || (maxValue < definition.MaximumValue)) { rangeValues.Add(definition.MaximumValue); }
                        }

                        rangeValues = rangeValues.Distinct().ToList();
                        rangeValues = rangeValues.OrderBy(o => o).ToList();

                        List<ColorDefinition> colorDefinitions = GetColorDefinitionByRange(selectedColorScheme, rangeValues);
                        selectedColorScheme.ColorDefinitions.Clear();
                        selectedColorScheme.ColorDefinitions = colorDefinitions;
                        DisplayDefinitions(selectedColorScheme);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add a new color scheme entry.\n" + ex.Message, "Add a Color Scheme Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //remove definitions
        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridDefinition.SelectedItem)
                {
                    if (selectedColorScheme.DefinitionBy == DefinitionType.ByValue)
                    {
                        ColorDefinition definition = dataGridDefinition.SelectedItem as ColorDefinition;
                        if (definition.InUse == "No")
                        {
                            for (int j = selectedColorScheme.ColorDefinitions.Count-1; j>-1; j--)
                            {
                                if (selectedColorScheme.ColorDefinitions[j].ParameterValue == definition.ParameterValue)
                                {
                                    selectedColorScheme.ColorDefinitions.RemoveAt(j); break;
                                }
                            }

                            if (definition.UserDefined)
                            {
                                for (int i = selectedColorScheme.CustomColorDefinitions.Count - 1; i > -1; i--)
                                {
                                    if (selectedColorScheme.CustomColorDefinitions[i].ParameterValue == definition.ParameterValue)
                                    {
                                        selectedColorScheme.CustomColorDefinitions.RemoveAt(i); break;
                                    }
                                }
                            }
                        }
                    }
                    else if (selectedColorScheme.DefinitionBy == DefinitionType.ByRange)
                    {
                        List<double> rangeValues = new List<double>();
                        foreach (ColorDefinition definition in selectedColorScheme.ColorDefinitions)
                        {
                            rangeValues.Add(definition.MinimumValue);
                            rangeValues.Add(definition.MaximumValue);
                        }
                        rangeValues = rangeValues.Distinct().ToList();

                        ColorDefinition selectedDefinition = dataGridDefinition.SelectedItem as ColorDefinition;
                        double minimumValue = selectedDefinition.MinimumValue;
                        double maximumValue = selectedDefinition.MaximumValue;

                        if (rangeValues.Contains(maximumValue)) { rangeValues.Remove(maximumValue); }

                        rangeValues = rangeValues.OrderBy(o => o).ToList();

                        List<ColorDefinition> colordefinitions = GetColorDefinitionByRange(selectedColorScheme, rangeValues);
                        selectedColorScheme.ColorDefinitions.Clear();
                        selectedColorScheme.ColorDefinitions = colordefinitions;
                        
                    }

                    DisplayDefinitions(selectedColorScheme);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove a color shcme entry.\n" + ex.Message, "Remove a Color Scheme Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //change color of the color definition
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //color dialog
            WinForm.ColorDialog colorDialog = new WinForm.ColorDialog();
            if (WinForm.DialogResult.OK == colorDialog.ShowDialog())
            {
                System.Drawing.Color color = colorDialog.Color;
                colorDialog.Dispose();

                ColorDefinition selectedDefinition = (sender as Button).DataContext as ColorDefinition;
                ColorDefinition colorDefinition = new ColorDefinition(selectedDefinition);
                colorDefinition.Color[0] = color.R;
                colorDefinition.Color[1] = color.G;
                colorDefinition.Color[2] = color.B;

                System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(colorDefinition.Color[0], colorDefinition.Color[1], colorDefinition.Color[2]);
                colorDefinition.BackgroundColor = new SolidColorBrush(windowColor);
                colorDefinition.RGB = colorDefinition.Color[0].ToString() + " - " + colorDefinition.Color[1].ToString() + " - " + colorDefinition.Color[2].ToString();


                for (int i = 0; i < selectedColorScheme.ColorDefinitions.Count; i++)
                {
                    ColorDefinition definition = selectedColorScheme.ColorDefinitions[i];
                    if (definition.ParameterValue == colorDefinition.ParameterValue)
                    {
                        selectedColorScheme.ColorDefinitions[i] = colorDefinition; break;
                    }
                }

                DisplayDefinitions(selectedColorScheme);
                //update color scheme
            }
        }

        //import an exisitng color scheme
        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.FileName = @"B:\";
                openDialog.DefaultExt = ".bcfzip";
                openDialog.Filter = "bcf zip (*.bcfzip)|*.bcfzip";
                openDialog.InitialDirectory = FindProjectDirectory();
                openDialog.RestoreDirectory = true;
                if (openDialog.ShowDialog() == true)
                {
                    string bcfPath = openDialog.FileName;
                    if (bcfPath != textBoxFilePath.Text)
                    {
                        BCFZIP importedBCF = bcfUtil.ReadBCF(bcfPath);
                        ColorSchemeInfo importedSchemeInfo = new ColorSchemeInfo();
                        if (null != importedBCF.BCFComponents)
                        {
                            foreach (BCFComponent bcfComponent in importedBCF.BCFComponents)
                            {
                                if (null != bcfComponent.ColorSchemeInfo)
                                {
                                    importedSchemeInfo = bcfComponent.ColorSchemeInfo;
                                    List<ColorScheme> colorSchemes = new List<ColorScheme>();
                                    foreach (ColorScheme scheme in importedSchemeInfo.ColorSchemes)
                                    {
                                        //get filtered elements
                                        ColorScheme updatedScheme = GetFilteredElements(scheme);
                                        colorSchemes.Add(updatedScheme);
                                    }
                                    importedSchemeInfo.ColorSchemes = colorSchemes;
                                }
                            }
                        }

                        if (importedSchemeInfo.ColorSchemes.Count > 0)
                        {
                            SchemeWindow schemeWindow = new SchemeWindow(importedSchemeInfo);
                            schemeWindow.Owner = this;
                            if (schemeWindow.ShowDialog() == true)
                            {
                                List<ColorScheme> colorSchemeList = schemeWindow.SelectedColorSchemes;
                                foreach (ColorScheme scheme in colorSchemeList)
                                {
                                    var schemeFound = from schemeItem in colorSchemeInfo.ColorSchemes where schemeItem.SchemeName == scheme.SchemeName select schemeItem;
                                    if (schemeFound.Count() > 0)
                                    {
                                        ColorScheme importedScheme = new ColorScheme(scheme);
                                        importedScheme.SchemeName = scheme.SchemeName + " - Imported";
                                        colorSchemeInfo.ColorSchemes.Add(importedScheme);
                                    }
                                    else
                                    {
                                        colorSchemeInfo.ColorSchemes.Add(scheme);
                                    }
                                }
                            }
                        }

                        sortableSchemeViews.Clear();
                        if (colorSchemeInfo.ColorSchemes.Count > 0)
                        {
                            foreach (ColorScheme scheme in colorSchemeInfo.ColorSchemes)
                            {
                                ListViewModel viewModel = new ListViewModel(scheme);
                                sortableSchemeViews.Add(viewModel);
                            }
                            sortableSchemeViews.Sort();
                            dataGridSchemes.SelectedItem = sortableSchemeViews[0];
                        }
                    }
                    else
                    {
                        MessageBox.Show("The name of file is same as the already opened file.\nPlease open another bcf file.", "File Cannot be Imported", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import color schemes.\n" + ex.Message, "Import Color Schemes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //a color shceme is selected
        private void dataGridSchemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                userInput = false;
                if (null != dataGridSchemes.SelectedItem)
                {

                    if (!string.IsNullOrEmpty(selectedColorScheme.SchemeId))
                    {
                        SaveChanges(selectedColorScheme);
                    }

                    ListViewModel selectedItem = dataGridSchemes.SelectedItem as ListViewModel;
                    if (null != selectedItem)
                    {
                        selectedColorScheme = selectedItem.ItemContent as ColorScheme;
                    }
                    SelectParameter(selectedColorScheme);
                    SelectView(selectedColorScheme);

                }
                userInput = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a color scheme.\n"+ex.Message, "Color Scheme", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
      
        private void comboBoxView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (userInput)
                {
                    ViewInfo viewInfo = comboBoxView.SelectedItem as ViewInfo;
                    if (null != viewInfo)
                    {
                        selectedColorScheme.SelectedViewInfo = viewInfo;
                        selectedColorScheme.ViewName = viewInfo.ViewName;
                    }

                    for (int i = 0; i < colorSchemeInfo.ColorSchemes.Count; i++)
                    {
                        ColorScheme scheme = colorSchemeInfo.ColorSchemes[i];
                        if (scheme.SchemeName == selectedColorScheme.SchemeName)
                        {
                            colorSchemeInfo.ColorSchemes.RemoveAt(i);
                        }
                    }
                    colorSchemeInfo.ColorSchemes.Add(selectedColorScheme);

                    int selectedIndex = 0;
                    for (int i = 0; i < sortableSchemeViews.Count; i++)
                    {
                        if (sortableSchemeViews[i].ItemContent.SchemeId == selectedColorScheme.SchemeId)
                        {
                            ListViewModel listViewModel = new ListViewModel(selectedColorScheme);
                            sortableSchemeViews[i] = listViewModel;
                            selectedIndex = i;
                            break;
                        }
                    }
                    dataGridSchemes.SelectedItem = sortableSchemeViews[selectedIndex];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the view.\n" + ex.Message, "View Selection Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //a parameter value for color is selected
        private void comboBoxColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ParameterInfo paramInfo = comboBoxColor.SelectedItem as ParameterInfo;
                if (null != paramInfo)
                {
                    StorageType storageType = paramInfo.ParamStorageType;

                    if (storageType == StorageType.Double || storageType == StorageType.Integer)
                    {
                        radioButtonByRange.IsEnabled = true;
                        checkBoxSplit.IsEnabled = true;
                        integerUpDown.IsEnabled = true;
                    }
                    else if (storageType == StorageType.ElementId || storageType == StorageType.String)
                    {
                        selectedColorScheme.DefinitionBy = DefinitionType.ByValue;
                        radioButtonByRange.IsEnabled = false;
                        checkBoxSplit.IsEnabled = false;
                        integerUpDown.IsEnabled = false;
                    }

                    switch (selectedColorScheme.DefinitionBy)
                    {
                        case DefinitionType.ByValue:
                            radioButtonByValue.IsChecked = true;
                            radioButtonByRange.IsChecked = false;
                            break;
                        case DefinitionType.ByRange:
                            radioButtonByValue.IsChecked = false;
                            radioButtonByRange.IsChecked = true;
                            break;
                    }
                    checkBoxSplit.IsChecked = selectedColorScheme.PresetRange;
                    integerUpDown.IsEnabled = selectedColorScheme.PresetRange;
                    integerUpDown.Value = selectedColorScheme.NumberOfRange;
                    selectedColorScheme = UpdateColorDefinitions(selectedColorScheme, paramInfo);
                    DisplayDefinitions(selectedColorScheme);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply the selection of the parameter.\n" + ex.Message, "Combobox Color By", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //a definition is selected
        private void dataGridDefinition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (selectedColorScheme.DefinitionBy == DefinitionType.ByValue)
                {
                    buttonDelete.IsEnabled = false;
                    foreach (ColorDefinition definition in dataGridDefinition.SelectedItems)
                    {
                        if (definition.InUse == "NO")
                        {
                            buttonDelete.IsEnabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the selection of the row.\n" + ex.Message, "Row Selection Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //color by range
        private void radioButtonByRange_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (userInput)
                {
                    if ((bool)radioButtonByRange.IsChecked)
                    {
                        ParameterInfo paramInfo = comboBoxColor.SelectedItem as ParameterInfo;
                        if (null != paramInfo)
                        {
                            selectedColorScheme.DefinitionBy = DefinitionType.ByRange;
                            selectedColorScheme = UpdateColorDefinitions(selectedColorScheme, paramInfo);
                            DisplayDefinitions(selectedColorScheme);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display definition by range.\n" + ex.Message, "Display Definitions By Range", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //color by value
        private void radioButtonByValue_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (userInput)
                {
                    if ((bool)radioButtonByValue.IsChecked)
                    {
                        ParameterInfo paramInfo = comboBoxColor.SelectedItem as ParameterInfo;
                        if (null != paramInfo)
                        {
                            selectedColorScheme.DefinitionBy = DefinitionType.ByValue;
                            selectedColorScheme = UpdateColorDefinitions(selectedColorScheme, paramInfo);
                            DisplayDefinitions(selectedColorScheme);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display definition by values.\n" + ex.Message, "Display Definitions By Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //number of ranges value changed
        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (userInput && selectedColorScheme.PresetRange)
            {
                ColorScheme updatedScheme = new ColorScheme(selectedColorScheme);
                updatedScheme.NumberOfRange = Convert.ToInt16(integerUpDown.Value);

                ParameterInfo paramInfo = comboBoxColor.SelectedItem as ParameterInfo;
                if (null != paramInfo)
                {
                    selectedColorScheme = UpdateColorDefinitions(updatedScheme, paramInfo);
                    DisplayDefinitions(selectedColorScheme);
                }
            }
        }

        //auto split enabled
        private void checkBoxSplit_Checked(object sender, RoutedEventArgs e)
        {
            if (userInput)
            {
                if ((bool)checkBoxSplit.IsChecked)
                {
                    integerUpDown.IsEnabled = true;

                    ColorScheme updatedScheme = new ColorScheme(selectedColorScheme);
                    updatedScheme.PresetRange = true;
                    updatedScheme.NumberOfRange = Convert.ToInt16(integerUpDown.Value);

                    ParameterInfo paramInfo = comboBoxColor.SelectedItem as ParameterInfo;
                    if (null != paramInfo)
                    {
                        selectedColorScheme = UpdateColorDefinitions(updatedScheme, paramInfo);
                        DisplayDefinitions(selectedColorScheme);
                    }
                }
                else
                {
                    integerUpDown.IsEnabled = false;
                }
            }
        }

        //auto split disabled
        private void checkBoxSplit_Unchecked(object sender, RoutedEventArgs e)
        {
            if (userInput)
            {
                if (checkBoxSplit.IsChecked == false)
                {
                    integerUpDown.IsEnabled = false;

                    ColorScheme updatedScheme = new ColorScheme(selectedColorScheme);
                    updatedScheme.PresetRange = false;
                    updatedScheme.NumberOfRange = Convert.ToInt16(integerUpDown.Value);

                    selectedColorScheme = updatedScheme;
                }
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonNewBCF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (colorSchemeInfo.ColorSchemes.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Would you like to save BCF before creating a new file?", "Save BCF", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (SaveToBCF())
                        {
                            MessageBox.Show("The current settings are successfully saved as BCF.\n" + bcfZip.FileName, "BCF Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                
                comboBoxView.SelectedIndex = 0;
                textBoxFilePath.Text = "";

                bcfZip = new BCFZIP();
                colorSchemeInfo = new ColorSchemeInfo();
                selectedColorScheme = new ColorScheme();
                filteredElements = new Dictionary<int, ElementProperties>();

                sortableSchemeViews.Clear();
                comboBoxColor.ItemsSource = null;
                dataGridDefinition.ItemsSource = null;

                buttonSaveBCF.IsEnabled = false;
                buttonSaveAsBCF.IsEnabled = false;
                buttonIsolate.IsEnabled = false;
                buttonClear.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create new bcf.\n"+ex.Message, "Create New BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }

        private void buttonOpenBCF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = @"B:\";
                openFileDialog.DefaultExt = ".bcfzip";
                openFileDialog.Filter = "bcf zip (*.bcfzip)|*.bcfzip";
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == true)
                {
                    string bcfPath = openFileDialog.FileName;
                    colorEditorSettings.BCFPath = bcfPath;
                    OpenBCF(bcfPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open the BCF zip file.\n" + ex.Message, "Open BCFZIP", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonSaveBCF_Click(object sender, RoutedEventArgs e)
        {
            if (SaveToBCF())
            {
                MessageBox.Show("The current settings are successfully saved as BCF.\n" + bcfZip.FileName, "BCF Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void buttonSaveAsBCF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (colorSchemeInfo.ColorSchemes.Count > 0)
                {
                    SaveChanges(selectedColorScheme);

                    string filePath = "";
                    BCFComponent bcfComponent = null;
                    //create new file.
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    string defPath = FindProjectDirectory();
                    if (!string.IsNullOrEmpty(defPath))
                    {
                        string directoryPath = System.IO.Path.GetDirectoryName(defPath);
                        saveFileDialog.InitialDirectory = directoryPath;
                        saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(defPath);
                    }
                    saveFileDialog.Title = "Save BCF As";
                    saveFileDialog.DefaultExt = ".bcfzip";
                    saveFileDialog.Filter = "bcf zip (*.bcfzip)|*.bcfzip";
                    saveFileDialog.RestoreDirectory = true;
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        filePath = saveFileDialog.FileName;
                        bcfZip.FileName = filePath;
                        textBoxFilePath.Text = filePath;

                        bcfComponent = bcfUtil.CreateBCF(filePath);
                        bcfComponent.ColorSchemeInfo = colorSchemeInfo;
                        Dictionary<int, ElementProperties> elementDictionary = GetElementDictionary(selectedColorScheme);
                        if (elementDictionary.Count > 0)
                        {
                            bcfComponent.VisualizationInfo = bcfUtil.GetVisInfo(elementDictionary, System.IO.Path.Combine(bcfComponent.DirectoryPath, "snapshot.png"));
                        }

                        if (bcfUtil.WriteBCF(bcfComponent))
                        {
                            bool result = bcfUtil.ChangeToBcfzip(filePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save as.\n"+ex.Message, "Save As", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (checkBoxSave.IsChecked == true)
            {
                if (SaveToBCF())
                {
                    MessageBox.Show("The current settings are successfully saved as BCF.\n" + bcfZip.FileName, "BCF Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            if (!string.IsNullOrEmpty(bcfZip.FileName))
            {
                colorEditorSettings.BCFPath = bcfZip.FileName;
            }
            else
            {
                colorEditorSettings.BCFPath = "";
            }
            DataStorageUtil.UpdateDataStorage(m_doc, colorEditorSettings);
        }

        //apply colors and save BCF
        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveChanges(selectedColorScheme);
                List<ElementId> coloredElements = ApplyColors(selectedColorScheme);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply colors.\n" + ex.Message, "Apply Colors", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //update all color schems and save BCF
        private void buttonUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveChanges(selectedColorScheme);

                List<ElementId> coloredElements = new List<ElementId>();
                foreach (ColorScheme colorScheme in colorSchemeInfo.ColorSchemes)
                {
                    coloredElements.AddRange(ApplyColors(colorScheme));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply colors.\n" + ex.Message, "Update All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //isolate elements overriden colors
        private void buttonIsolate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != selectedColorScheme.SelectedViewInfo)
                {
                    ViewInfo selectedViewInfo = selectedColorScheme.SelectedViewInfo;
                    View selectedView = selectedViewInfo.ViewObj;

                    if (selectedView.IsInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate))
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Reset View");
                            try
                            {
                                selectedView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                                trans.Commit();
                            }
                            catch { trans.RollBack(); }
                        }
                    }
                    else
                    {
                        SaveChanges(selectedColorScheme);

                        List<ElementId> coloredElements = ApplyColors(selectedColorScheme);

                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Isolate");
                            try
                            {
                                selectedView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                                selectedView.IsolateElementsTemporary(coloredElements);
                                trans.Commit();
                            }
                            catch { trans.RollBack(); }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply colors and isolate the view.\n" + ex.Message, "Isolate Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string schemeId = selectedColorScheme.SchemeId;
                if (!string.IsNullOrEmpty(schemeId))
                {
                    if (colorEditorSettings.ColoredElements.ContainsKey(schemeId))
                    {
                        IList<ElementId> elementIds = colorEditorSettings.ColoredElements[schemeId];

                        ViewInfo selectedViewInfo = selectedColorScheme.SelectedViewInfo;
                        View selectedView = selectedViewInfo.ViewObj;

                        OverrideGraphicSettings settings = new OverrideGraphicSettings();
                        settings.SetProjectionFillColor(Autodesk.Revit.DB.Color.InvalidColorValue);
                        settings.SetCutFillColor(Autodesk.Revit.DB.Color.InvalidColorValue);
                        settings.SetProjectionFillPatternId(ElementId.InvalidElementId);
                        settings.SetCutFillPatternId(ElementId.InvalidElementId);

                        bool result = false;
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Clear Override Colors");
                            try
                            {
                                if (null != selectedView)
                                {
                                    foreach (ElementId eId in elementIds)
                                    {
                                        selectedView.SetElementOverrides(eId, settings);
                                    }
                                }
                                trans.Commit();
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Failed to clear override colors.\n" + ex.Message, "Clear Override Colors", MessageBoxButton.OK, MessageBoxImage.Warning);
                                trans.RollBack();
                                result = false;
                            }
                        }
                        if (result)
                        {
                            colorEditorSettings.ColoredElements.Remove(schemeId);
                            DataStorageUtil.UpdateDataStorage(m_doc, colorEditorSettings);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to clear override colors.\n"+ex.Message, "Clear Override Colors", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        private void DisplayViewLists()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<View> views = collector.OfClass(typeof(View)).ToElements().Cast<View>().ToList();

                var collectedViews = from view in views where view.CanEnableTemporaryViewPropertiesMode() == true select view;
                foreach (View view in collectedViews)
                {
                    ViewInfo viewInfo = new ViewInfo(view);
                    viewInfoList.Add(viewInfo);
                }

                viewInfoList = viewInfoList.OrderBy(o => o.ViewName).ToList();
                comboBoxView.ItemsSource = viewInfoList;
                comboBoxView.DisplayMemberPath = "ViewName";
                

                for (int i = 0; i < comboBoxView.Items.Count; i++)
                {
                    ViewInfo viewInfo = comboBoxView.Items[i] as ViewInfo;
                    if (viewInfo.ViewName == m_doc.ActiveView.Name)
                    {
                        comboBoxView.SelectedIndex = i;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the list of views.\n" + ex.Message, "Display View Lists", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayBCF(BCFZIP bcfZipFile)
        {
            try
            {
                //assume that only one bcf guid folder contains colorscheme.xml
                if (null != bcfZipFile.BCFComponents)
                {
                    foreach (BCFComponent bcfComponent in bcfZipFile.BCFComponents)
                    {
                        if (null != bcfComponent.ColorSchemeInfo)
                        {
                            colorSchemeInfo = bcfComponent.ColorSchemeInfo;
                            List<ColorScheme> colorSchemes = new List<ColorScheme>();
                            foreach (ColorScheme scheme in colorSchemeInfo.ColorSchemes)
                            {
                                //get filtered elements
                                ColorScheme updatedScheme = GetFilteredElements(scheme);
                                var views = from view in viewInfoList where view.ViewName == scheme.ViewName select view;
                                if (views.Count() > 0)
                                {
                                    ViewInfo viewInfo = views.First();
                                    updatedScheme.SelectedViewInfo = viewInfo;
                                }
                                foreach (ColorDefinition definition in scheme.ColorDefinitions)
                                {
                                    if (definition.UserDefined)
                                    {
                                        updatedScheme.CustomColorDefinitions.Add(definition);
                                    }
                                }
                                colorSchemes.Add(updatedScheme);
                            }
                            colorSchemeInfo.ColorSchemes = colorSchemes;
                        }
                    }
                }

                if (colorSchemeInfo.ColorSchemes.Count > 0)
                {
                    sortableSchemeViews.Clear();
                    foreach (ColorScheme scheme in colorSchemeInfo.ColorSchemes)
                    {
                        ListViewModel viewModel = new ListViewModel(scheme);
                        sortableSchemeViews.Add(viewModel);
                    }
                    sortableSchemeViews.Sort();
                    dataGridSchemes.SelectedItem = sortableSchemeViews[0];
                    
                    buttonSaveBCF.IsEnabled = true;
                    buttonSaveAsBCF.IsEnabled = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display BCF Zip.\n"+ex.Message, "Display BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayDefinitions(ColorScheme scheme)
        {
            try
            {
                List<ColorDefinition> colorDefinitionList = new List<ColorDefinition>();
                foreach (ColorDefinition definition in scheme.ColorDefinitions)
                {
                    if (!string.IsNullOrEmpty(definition.ParameterValue))
                    {
                        colorDefinitionList.Add(definition);
                    }
                }

                dataGridDefinition.ItemsSource = null;
                dataGridDefinition.ItemsSource = colorDefinitionList;
                dataGridDefinition.CanUserAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display definitions.\n" + ex.Message, "Display Definitions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectParameter(ColorScheme colorScheme)
        {
            try
            {
                Categories categories = m_doc.Settings.Categories;
                ICollection<ElementId> filterCatIds = new List<ElementId>();
                List<Category> categoryList = new List<Category>();
                foreach (string catName in colorScheme.Categories)
                {
                    Category category = categories.get_Item(catName);
                    if (null != category)
                    {
                        filterCatIds.Add(category.Id);
                        categoryList.Add(category);
                    }
                }

                ICollection<ElementId> supportedParams = ParameterFilterUtilities.GetFilterableParametersInCommon(m_doc, filterCatIds);
                List<string> projectParamNames = RevitUtil.FindProjectParameters(m_doc, categoryList);
                if (colorScheme.FilteredElements.Count > 0)
                {
                    Element element = colorScheme.FilteredElements.First();
                    ResetParameterComboBox(comboBoxColor, element, supportedParams, projectParamNames);
                }

                if (!string.IsNullOrEmpty(colorScheme.ParameterName))
                {
                    bool found = false;
                    for (int i = 0; i < comboBoxColor.Items.Count; i++)
                    {
                        ParameterInfo paramInfo = comboBoxColor.Items[i] as ParameterInfo;
                        if (paramInfo.Name == colorScheme.ParameterName)
                        {
                            comboBoxColor.SelectedIndex = i; found = true; break;
                        }
                    }
                    if (!found) { comboBoxColor.SelectedIndex = 0; }
                }
                else
                {
                    comboBoxColor.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set the parameter group.\n" + ex.Message, "Select Parameter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectView(ColorScheme colorScheme)
        {
            try
            {
                if (null != colorScheme.SelectedViewInfo)
                {
                    comboBoxView.SelectedItem = colorScheme.SelectedViewInfo;
                    //enable or disable the clear color and isolate button
                    buttonIsolate.IsEnabled = false;
                    buttonClear.IsEnabled = false;

                    View activeView = m_doc.ActiveView;
                    if (null != activeView)
                    {
                        if (activeView.Id.IntegerValue == colorScheme.SelectedViewInfo.ViewId.IntegerValue)
                        {
                            buttonIsolate.IsEnabled = true;
                            buttonClear.IsEnabled = true;
                            buttonIsolate.ToolTip = "Apply override colors and isolate the active view.";
                            buttonClear.ToolTip = "Clear override colors.";
                        }
                        else
                        {
                            buttonIsolate.ToolTip = "Please make sure the selected view is opened in the background as the active view.";
                            buttonClear.ToolTip = "Please make sure the selected view is opened in the background as the active view.";
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(colorScheme.ViewName + " cannot be selected.\n" + ex.Message, "Select View", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveChanges(ColorScheme colorscheme)
        {
            try
            {
                if (!string.IsNullOrEmpty(colorscheme.SchemeId))
                {
                    for (int i = 0; i < colorSchemeInfo.ColorSchemes.Count; i++)
                    {
                        if (colorSchemeInfo.ColorSchemes[i].SchemeId == colorscheme.SchemeId)
                        {
                            colorSchemeInfo.ColorSchemes.RemoveAt(i); break;
                        }
                    }
                    colorSchemeInfo.ColorSchemes.Add(colorscheme);

                    for (int i = 0; i < sortableSchemeViews.Count; i++)
                    {
                        if (sortableSchemeViews[i].ItemContent.SchemeId == colorscheme.SchemeId)
                        {
                            ListViewModel viewModel = new ListViewModel(colorscheme);
                            sortableSchemeViews[i] = viewModel; break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save changes on a color scheme.\n" + ex.Message, "Save Changes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ResetParameterComboBox(System.Windows.Controls.ComboBox comboBox, Element element, ICollection<ElementId> paramSet, List<string> projectParamNames)
        {
            try
            {
                List<string> paramNames = projectParamNames;
                foreach (ElementId paramId in paramSet)
                {
                    if (paramId.IntegerValue < 0)
                    {
                        BuiltInParameter bltParameter = (BuiltInParameter)paramId.IntegerValue;
                        string paramName = LabelUtils.GetLabelFor(bltParameter);
                        paramNames.Add(paramName);
                    }
                }

                paramNames = paramNames.Distinct().ToList();

                List<ParameterInfo> paramInfoList = ResetParameterInfo(element, paramNames);
                comboBox.ItemsSource = null;
                comboBox.ItemsSource = paramInfoList;
                comboBox.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to reset parameter combobox.\n" + ex.Message, "Reset Parameter ComboBox", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<ParameterInfo> ResetParameterInfo(Element element, List<string> paramNames)
        {
            List<ParameterInfo> resetInfo = new List<ParameterInfo>();
            try
            {
                Dictionary<string, ParameterInfo> paramInfo = AddBasicParameterInfo();

                //Insert Level associated parameter
                foreach (Parameter parameter in element.Parameters)
                {
                    string parameterName = parameter.Definition.Name;
                    if (parameterName.Contains("Level") && parameter.StorageType == StorageType.ElementId)
                    {
                        if (!paramNames.Contains(parameterName))
                        {
                            ParameterInfo pi = new ParameterInfo(parameter);
                            pi.IsInstance = true;
                            if (!paramInfo.ContainsKey(parameterName))
                            {
                                paramInfo.Add(parameterName, pi);
                            }
                        }
                    }
                }

                foreach (string paramName in paramNames)
                {
                    if (paramName.Contains("Extensions.")) { continue; }
#if RELEASE2014
                    Parameter param = element.get_Parameter(paramName);
#else
                    Parameter param = element.LookupParameter(paramName);
#endif

                    if (null != param)
                    {
                        ParameterInfo pi = new ParameterInfo(param);
                        pi.IsInstance = true;
                        if (!paramInfo.ContainsKey(paramName))
                        {
                            paramInfo.Add(paramName, pi);
                        }
                    }
                    else
                    {
                        ElementId typeId = element.GetTypeId();
                        ElementType eType = m_doc.GetElement(typeId) as ElementType;
                        if (null != eType)
                        {
#if RELEASE2014
                            param = eType.get_Parameter(paramName);
#else
                            param = eType.LookupParameter(paramName);
#endif

                            if (null != param)
                            {
                                ParameterInfo pi = new ParameterInfo(param);
                                pi.IsInstance = false;
                                if (!paramInfo.ContainsKey(paramName))
                                {
                                    paramInfo.Add(paramName, pi);
                                }
                            }
                        }
                    }
                }

                resetInfo = paramInfo.Values.ToList();
                resetInfo = resetInfo.OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to reset parameter information.\n" + ex.Message, "Reset Parameter Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return resetInfo;
        }

        private Dictionary<string, ParameterInfo> AddBasicParameterInfo()
        {
            Dictionary<string, ParameterInfo> paramInfo = new Dictionary<string, ParameterInfo>();
            try
            {
                BuiltInParameter[] basicParameters = new BuiltInParameter[] { BuiltInParameter.ALL_MODEL_FAMILY_NAME, BuiltInParameter.ALL_MODEL_TYPE_NAME };
                foreach (BuiltInParameter bltParam in basicParameters)
                {
                    ParameterInfo pi = new ParameterInfo(m_doc, bltParam);
                    pi.IsInstance = false;
                    paramInfo.Add(pi.Name, pi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add basic parameter information.\n"+ex.Message, "Add Basic Parameter Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return paramInfo;
        }

        private ColorScheme GetFilteredElements(ColorScheme colorScheme)
        {
            ColorScheme updatedScheme = new ColorScheme(colorScheme);
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<ElementFilter> categoryFilters = new List<ElementFilter>();
                Categories categories = m_doc.Settings.Categories;
                foreach (string catName in updatedScheme.Categories)
                {
                    Category category = categories.get_Item(catName);
                    categoryFilters.Add(new ElementCategoryFilter(category.Id));
                }

                LogicalOrFilter orFilter = new LogicalOrFilter(categoryFilters);
                List<ElementId> elementIds = collector.WherePasses(orFilter).WhereElementIsNotElementType().ToElementIds().ToList();
                List<Element> filteredElements = collector.WherePasses(orFilter).WhereElementIsNotElementType().ToElements().ToList();

                if (elementIds.Count > 0 && colorScheme.FilterRules.Count > 0)
                {
                    collector = new FilteredElementCollector(m_doc, elementIds);
                    List<Autodesk.Revit.DB.FilterRule> filterRules = RevitUtil.ConverToRevitFilterRule(colorScheme.FilterRules);
                    ElementParameterFilter paramFilter = new ElementParameterFilter(filterRules);
                    filteredElements = collector.WherePasses(paramFilter).ToElements().ToList();
                }
                updatedScheme.FilteredElements = filteredElements;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get filtered elements.\n"+ex.Message, "Get Filtered Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedScheme;
        }

        private ColorScheme UpdateColorDefinitions(ColorScheme colorScheme, ParameterInfo paramInfo)
        {
            ColorScheme updatedScheme = new ColorScheme(colorScheme);
            try
            {
                updatedScheme.SelectedParamInfo = paramInfo;
                updatedScheme.ParameterName = paramInfo.Name;
                
                Dictionary<string, ParameterValueInfo> valueDictionary = new Dictionary<string, ParameterValueInfo>();
                if (updatedScheme.FilteredElements.Count > 0)
                {
                    foreach (Element element in updatedScheme.FilteredElements)
                    {
                        ParameterValueInfo valueInfo = new ParameterValueInfo(m_doc, element, paramInfo);
                        if (!string.IsNullOrEmpty(valueInfo.ValueAsString))
                        {
                            if (!valueDictionary.ContainsKey(valueInfo.ValueAsString))
                            {
                                valueDictionary.Add(valueInfo.ValueAsString, valueInfo);
                            }
                        }
                    }
                }
                
                Dictionary<string/*paramValue*/, ColorDefinition> colorDictionary = new Dictionary<string, ColorDefinition>();
                if (updatedScheme.DefinitionBy == DefinitionType.ByValue)
                {
                    foreach (ParameterValueInfo valueInfo in valueDictionary.Values)
                    {
                        ColorDefinition colorDefinition = new ColorDefinition();
                        colorDefinition.InUse = "Yes";
                        colorDefinition.UserDefined = false;
                        colorDefinition.ParameterValue = valueInfo.ValueAsString;

                        colorDefinition = SetColorValues(updatedScheme, colorDefinition);
                        colorDictionary.Add(colorDefinition.ParameterValue, colorDefinition);
                    }

                    foreach (ColorDefinition definition in updatedScheme.CustomColorDefinitions)
                    {
                        if (colorDictionary.ContainsKey(definition.ParameterValue))
                        {
                            ColorDefinition colorDefinition = colorDictionary[definition.ParameterValue];
                            colorDefinition.Color = definition.Color;
                            colorDefinition.BackgroundColor = definition.BackgroundColor;
                            colorDefinition.RGB = definition.RGB;

                            colorDictionary.Remove(definition.ParameterValue);
                            colorDictionary.Add(colorDefinition.ParameterValue, colorDefinition);
                        }
                        else
                        {
                            //if a custom definition doesn't match with the parameter storage type, the custom definition will not be included.
                            if (paramInfo.ParamStorageType == StorageType.Double)
                            {
                                double dblValue = 0;
                                if (!double.TryParse(definition.ParameterValue, out dblValue)) { continue; }
                            }
                            else if (paramInfo.ParamStorageType == StorageType.Integer)
                            {
                                int intValue = 0;
                                if (!int.TryParse(definition.ParameterValue, out intValue)) { continue; }
                            }
                            System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(definition.Color[0], definition.Color[1], definition.Color[2]);
                            definition.BackgroundColor = new SolidColorBrush(windowColor);
                            definition.RGB = definition.Color[0].ToString() + " - " + definition.Color[1].ToString() + " - " + definition.Color[2].ToString();
                            colorDictionary.Add(definition.ParameterValue, definition);
                        }
                    }

                    List<ColorDefinition> colorDefinitions = colorDictionary.Values.ToList();
                    //sorting
                    if (paramInfo.ParamStorageType == StorageType.Double)
                    {
                        double dblValue = 0;
                        colorDefinitions = colorDefinitions.OrderBy(o => double.TryParse(o.ParameterValue, out dblValue)).ToList();
                    }
                    else if (paramInfo.ParamStorageType == StorageType.Integer)
                    {
                        int intValue = 0;
                        colorDefinitions = colorDefinitions.OrderBy(o => int.TryParse(o.ParameterValue, out intValue)).ToList();
                    }
                    else
                    {
                        colorDefinitions = colorDefinitions.OrderBy(o => o.ParameterValue).ToList();
                    }

                    updatedScheme.ColorDefinitions.Clear();
                    updatedScheme.ColorDefinitions = colorDefinitions;
                }
                else if (updatedScheme.DefinitionBy == DefinitionType.ByRange)
                {

                    List<double> rangeValues = GetRagneOfValues(paramInfo.ParamStorageType, valueDictionary, updatedScheme);

                    List<ColorDefinition> colordefintions=GetColorDefinitionByRange(updatedScheme, rangeValues);
                    updatedScheme.ColorDefinitions.Clear();
                    updatedScheme.ColorDefinitions = colordefintions;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(colorScheme.SchemeName + " Failed to validate the colorscheme.\n"+ex.Message, "Validate Color Scheme", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedScheme;
        }

        private List<double> GetRagneOfValues(StorageType storageType, Dictionary<string, ParameterValueInfo> valueInfoDictionary, ColorScheme updatedScheme)
        {
            List<double> rangeValues = new List<double>();
            double minVal = 0;
            double maxVal = 0;
            try
            {
                if (valueInfoDictionary.Count > 0)
                {
                    rangeValues.Add(double.MinValue);
                    rangeValues.Add(double.MaxValue);

                    if (storageType == StorageType.Double)
                    {
                        minVal = valueInfoDictionary.First().Value.DblValue;
                        maxVal = valueInfoDictionary.First().Value.DblValue;

                        foreach (ParameterValueInfo valueInfo in valueInfoDictionary.Values)
                        {
                            double value = valueInfo.DblValue;
                            if (minVal > value) { minVal = value; }
                            if (maxVal < value) { maxVal = value; }
                        }
                    }
                    else if (storageType == StorageType.Integer)
                    {
                        minVal = valueInfoDictionary.First().Value.IntValue;
                        maxVal = valueInfoDictionary.First().Value.IntValue;

                        foreach (ParameterValueInfo valueInfo in valueInfoDictionary.Values)
                        {
                            int value = valueInfo.IntValue;
                            if (minVal > value) { minVal = value; }
                            if (maxVal < value) { maxVal = value; }
                        }
                    }

                    rangeValues.Add(minVal);
                    rangeValues.Add(maxVal);

                    if (updatedScheme.PresetRange)
                    {
                        if (updatedScheme.NumberOfRange > 3)
                        {
                            int splitBy = updatedScheme.NumberOfRange - 2;
                            int increment = (int)((maxVal - minVal) / splitBy);
                            for (int i = 1; i < splitBy; i++)
                            {
                                double newValue = minVal + (i * increment);
                                rangeValues.Add(newValue);
                            }
                        }
                    }
                    else
                    {
                        foreach (ColorDefinition definition in updatedScheme.ColorDefinitions)
                        {
                            rangeValues.Add(definition.MinimumValue);
                            rangeValues.Add(definition.MaximumValue);
                        }
                    }

                    rangeValues = rangeValues.Distinct().ToList();
                    rangeValues = rangeValues.OrderBy(o => o).ToList();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the range of values.\n"+ex.Message, "Get Range of Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return rangeValues;
        }

        private ColorDefinition FindColorByParamValue(string paramValue, ColorScheme scheme)
        {
            ColorDefinition definition = null;
            try
            {
                var definitions = from adef in scheme.ColorDefinitions where adef.ParameterValue == paramValue select adef;
                if (definitions.Count() > 0)
                {
                    definition = definitions.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find a color definition by a parameter value.\n" + ex.Message, "Find Color by Parameter Value", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return definition;
        }

        private ColorDefinition SetColorValues(ColorScheme colorScheme, ColorDefinition definition)
        {
            ColorDefinition colorDefinition = new ColorDefinition(definition);
            try
            {
                ColorDefinition foundDefinition = FindColorByParamValue(colorDefinition.ParameterValue, colorScheme);
                if (null != foundDefinition)
                {
                    colorDefinition.Color = foundDefinition.Color;
                }
                else
                {
                    byte[] colorBytes = new byte[3];
                    colorBytes[0] = (byte)random.Next(256);
                    colorBytes[1] = (byte)random.Next(256);
                    colorBytes[2] = (byte)random.Next(256);
                    colorDefinition.Color = colorBytes;
                }

                System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(colorDefinition.Color[0], colorDefinition.Color[1], colorDefinition.Color[2]);
                colorDefinition.BackgroundColor = new SolidColorBrush(windowColor);
                colorDefinition.RGB = colorDefinition.Color[0].ToString() + " - " + colorDefinition.Color[1].ToString() + " - " + colorDefinition.Color[2].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set color values.\n"+ex.Message, "Set Color Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return colorDefinition;
        }

        private List<ColorDefinition> GetColorDefinitionByRange(ColorScheme colorscheme, List<double> rangeValues)
        {
            List<ColorDefinition> colorDefinitions = new List<ColorDefinition>();
            try
            {
                for (int i = 0; i < rangeValues.Count - 1; i++)
                {
                    double startVal = rangeValues[i];
                    double endVal = rangeValues[i + 1];

                    ColorDefinition definition = new ColorDefinition();
                    
                    definition.InUse = "Yes";
                    definition.UserDefined = false;
                    definition.MinimumValue = startVal;
                    definition.MaximumValue = endVal;

                    if (startVal <= double.MinValue)
                    {
                        definition.ParameterValue = "value <= " + (int)endVal;
                    }
                    else if (endVal >= double.MaxValue)
                    {
                        definition.ParameterValue = (int)startVal + " < value";
                    }
                    else
                    {
                        definition.ParameterValue = (int)startVal + " <= value < " + (int)endVal;
                    }

                    definition = SetColorValues(colorscheme, definition);
                    colorDefinitions.Add(definition);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the list of color definition by range values.\n"+ex.Message, "Get Color Definition By Range", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return colorDefinitions;
        }

        /*
        private Dictionary<int, ElementProperties> GetElementDictionary(ColorScheme colorScheme)
        {
            Dictionary<int, ElementProperties> elementDictionary = new Dictionary<int, ElementProperties>();
            try
            {
                if (null != colorScheme)
                {
                    if (null != colorScheme.SelectedParamInfo)
                    {
                        List<Element> elements = colorScheme.FilteredElements;
                        ParameterInfo paramInfo = colorScheme.SelectedParamInfo;

                        foreach (Element element in elements)
                        {
                            ParameterValueInfo valueInfo = new ParameterValueInfo(m_doc, element, paramInfo);

                            List<ColorDefinition> foundDefinitions = new List<ColorDefinition>();
                            if (colorScheme.DefinitionBy == DefinitionType.ByValue)
                            {
                                var foundDef = from definition in colorScheme.ColorDefinitions where definition.ParameterValue == valueInfo.ValueAsString select definition;
                                foundDefinitions = foundDef.ToList();
                            }
                            else if (colorScheme.DefinitionBy == DefinitionType.ByRange)
                            {
                                if (paramInfo.ParamStorageType == StorageType.Integer)
                                {
                                    int paramValue = valueInfo.IntValue;
                                    var foundDef = from definition in colorScheme.ColorDefinitions where (definition.MinimumValue <= paramValue) && (definition.MaximumValue >= paramValue) select definition;
                                    foundDefinitions = foundDef.ToList();
                                }
                                else if (paramInfo.ParamStorageType == StorageType.Double)
                                {
                                    double paramValue = valueInfo.DblValue;
                                    var foundDef = from definition in colorScheme.ColorDefinitions where (definition.MinimumValue <= paramValue) && (definition.MaximumValue >= paramValue) select definition;
                                    foundDefinitions = foundDef.ToList();
                                }
                            }

                            if (foundDefinitions.Count() > 0)
                            {
                                ColorDefinition colorDefinition = foundDefinitions.First();
                                ElementProperties ep = new ElementProperties(element, colorDefinition);
                                if (!elementDictionary.ContainsKey(ep.ElementIdInt))
                                {
                                    elementDictionary.Add(ep.ElementIdInt, ep);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get filtered elements.\n"+ex.Message, "Get Filtered Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return elementDictionary;
        }
        */
        private Dictionary<int, ElementProperties> GetElementDictionary(ColorScheme colorScheme)
        {
            Dictionary<int, ElementProperties> elementDictionary = new Dictionary<int, ElementProperties>();
            try
            {
                if (null != colorScheme)
                {
                    if (!string.IsNullOrEmpty(colorScheme.ParameterName))
                    {
                        List<Element> elements = colorScheme.FilteredElements;
                        string paramName = colorScheme.ParameterName;
                        ParameterInfo paramInfo = colorScheme.SelectedParamInfo;

                        foreach (Element element in elements)
                        {
                            ParameterValueInfo valueInfo = new ParameterValueInfo(m_doc, element, paramInfo);

                            List<ColorDefinition> foundDefinitions = new List<ColorDefinition>();
                            if (colorScheme.DefinitionBy == DefinitionType.ByValue)
                            {
                                var foundDef = from definition in colorScheme.ColorDefinitions where definition.ParameterValue == valueInfo.ValueAsString select definition;
                                foundDefinitions = foundDef.ToList();
                            }
                            else if (colorScheme.DefinitionBy == DefinitionType.ByRange)
                            {
                                if (valueInfo.ParamStorageType == StorageType.Integer)
                                {
                                    int paramValue = valueInfo.IntValue;
                                    var foundDef = from definition in colorScheme.ColorDefinitions where (definition.MinimumValue <= paramValue) && (definition.MaximumValue >= paramValue) select definition;
                                    foundDefinitions = foundDef.ToList();
                                }
                                else if (valueInfo.ParamStorageType == StorageType.Double)
                                {
                                    double paramValue = valueInfo.DblValue;
                                    var foundDef = from definition in colorScheme.ColorDefinitions where (definition.MinimumValue <= paramValue) && (definition.MaximumValue >= paramValue) select definition;
                                    foundDefinitions = foundDef.ToList();
                                }
                            }

                            if (foundDefinitions.Count() > 0)
                            {
                                ColorDefinition colorDefinition = foundDefinitions.First();
                                ElementProperties ep = new ElementProperties(element, colorDefinition);
                                if (!elementDictionary.ContainsKey(ep.ElementIdInt))
                                {
                                    elementDictionary.Add(ep.ElementIdInt, ep);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get filtered elements.\n" + ex.Message, "Get Filtered Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return elementDictionary;
        }

        private OverrideGraphicSettings GetOverrideGraphicSettings(ColorDefinition definition)
        {
            OverrideGraphicSettings settings = new OverrideGraphicSettings();
            try
            {
                byte[] color = definition.Color;
                Autodesk.Revit.DB.Color fillColor = new Autodesk.Revit.DB.Color(color[0], color[1], color[2]);
                using (Transaction trans = new Transaction(m_doc))
                {
                    try
                    {
                        trans.Start("Set Colors of Override Graphics Settings");
                        settings.SetProjectionFillColor(fillColor);
                        settings.SetCutFillColor(fillColor);
                        FillPatternElement fillPatternElement = FillPatternElement.GetFillPatternElementByName(m_doc, FillPatternTarget.Drafting, "Solid fill");
                        if (null != fillPatternElement)
                        {
                            settings.SetProjectionFillPatternId(fillPatternElement.Id);
                            settings.SetCutFillPatternId(fillPatternElement.Id);
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.RollBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set override graphics settings.\n" + ex.Message, "Get Override Graphics Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return settings;
        }

        private bool SaveToBCF()
        {
            bool result = false;
            try
            {
                if (colorSchemeInfo.ColorSchemes.Count > 0)
                {
                    SaveChanges(selectedColorScheme);

                    string filePath = "";
                    BCFComponent bcfComponent = null;
                    if (!string.IsNullOrEmpty(bcfZip.FileName))
                    {
                        filePath = bcfZip.FileName;
                        //save existing file.
                        foreach (BCFComponent component in bcfZip.BCFComponents)
                        {
                            if (null != component.ColorSchemeInfo)
                            {
                                bcfComponent = component;
                            }
                        }
                        if (null != bcfComponent)
                        {
                            bcfComponent.ColorSchemeInfo = colorSchemeInfo;
                            Dictionary<int, ElementProperties> elementDictionary = GetElementDictionary(selectedColorScheme);
                            if (elementDictionary.Count > 0)
                            {
                                bcfComponent.VisualizationInfo = bcfUtil.GetVisInfo(elementDictionary, System.IO.Path.Combine(bcfComponent.DirectoryPath, "snapshot.png"));
                            }

                            if (bcfUtil.WriteBCF(bcfComponent))
                            {
                                result = bcfUtil.ChangeToBcfzip(filePath);
                            }
                        }
                    }
                    else
                    {
                        //create new file.
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        string defPath = FindProjectDirectory();
                        if (!string.IsNullOrEmpty(defPath))
                        {
                            string directoryPath = System.IO.Path.GetDirectoryName(defPath);
                            saveFileDialog.InitialDirectory = directoryPath;
                            saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(defPath);
                        }
                        saveFileDialog.DefaultExt = ".bcfzip";
                        saveFileDialog.Filter = "bcf zip (*.bcfzip)|*.bcfzip";
                        saveFileDialog.RestoreDirectory = true;
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            filePath = saveFileDialog.FileName;
                            bcfZip.FileName = filePath;
                            textBoxFilePath.Text = filePath;

                            bcfComponent = bcfUtil.CreateBCF(filePath);
                            bcfComponent.ColorSchemeInfo = colorSchemeInfo;
                            Dictionary<int, ElementProperties> elementDictionary = GetElementDictionary(selectedColorScheme);
                            if (elementDictionary.Count > 0)
                            {
                                bcfComponent.VisualizationInfo = bcfUtil.GetVisInfo(elementDictionary, System.IO.Path.Combine(bcfComponent.DirectoryPath, "snapshot.png"));
                            }

                            if (bcfUtil.WriteBCF(bcfComponent))
                            {
                                result = bcfUtil.ChangeToBcfzip(filePath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the current BCF settings.\n" + ex.Message, "Save BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private string FindProjectDirectory()
        {
            string bcfPath = "";
            try
            {
                string masterFilePath = "";
                if (m_doc.IsWorkshared)
                {
                    ModelPath modelPath = m_doc.GetWorksharingCentralModelPath();
                    masterFilePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if (string.IsNullOrEmpty(masterFilePath))
                    {
                        masterFilePath = m_doc.PathName;
                    }
                }
                else
                {
                    masterFilePath = m_doc.PathName;
                }

                if (!string.IsNullOrEmpty(masterFilePath))
                {
                    bcfPath = masterFilePath.Replace(".rvt", ".bcfzip");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find the Revit project directory.\n" + ex.Message, "Find Project Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfPath;
        }

        private List<ElementId> ApplyColors(ColorScheme colorScheme)
        {
            List<ElementId> coloredElements = new List<ElementId>();
            try
            {
                Dictionary<int, ElementProperties> filteredElements = GetElementDictionary(colorScheme);
                ViewInfo selectedViewInfo = colorScheme.SelectedViewInfo;
                View selectedView = selectedViewInfo.ViewObj;

                progressBar.Visibility = System.Windows.Visibility.Visible;
                statusLable.Visibility = System.Windows.Visibility.Visible;
                statusLable.Text = "Applying Override Colors for " + colorScheme.SchemeName + "...";

                progressBar.Minimum = 0;
                progressBar.Maximum = filteredElements.Count;
                progressBar.Value = 0;

                double value = 0;
                UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

                bool colored = true;
                StringBuilder strBuilder = new StringBuilder();

                using (TransactionGroup group = new TransactionGroup(m_doc))
                {
                    group.Start("Overrides by Elements");
                    using (Transaction trans = new Transaction(m_doc))
                    {
                        foreach (ColorDefinition colorDefinition in colorScheme.ColorDefinitions)
                        {
                            OverrideGraphicSettings settings = GetOverrideGraphicSettings(colorDefinition);

                            List<ElementProperties> elements = new List<ElementProperties>();
                            var foundElements = from element in filteredElements.Values where element.Definition.ParameterValue == colorDefinition.ParameterValue select element;
                            elements = foundElements.ToList();

                            foreach (ElementProperties ep in elements)
                            {
                                value += 1;
                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                                trans.Start("Set Element Overrides: " + ep.ElementId.IntegerValue);
                                try
                                {
                                    selectedView.SetElementOverrides(ep.ElementId, settings);
                                    trans.Commit();
                                    coloredElements.Add(ep.ElementId);
                                }
                                catch
                                {
                                    colored = false;
                                    strBuilder.AppendLine(ep.ElementId.IntegerValue + " " + ep.RevitElement.Name);
                                    trans.RollBack();
                                }
                            }
                        }
                    }
                    group.Assimilate();
                }

                if (!colored)
                {
                    MessageBox.Show("Following elements cannot be ovveridden with assigned colors.\n\n" + strBuilder.ToString(), "Override Colors - Skipped Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (coloredElements.Count > 0)
                {
                    IDictionary<string, IList<ElementId>> dictionary = colorEditorSettings.ColoredElements;
                    string key = colorScheme.SchemeId;
                    if (dictionary.ContainsKey(key))
                    {
                        dictionary.Remove(key);
                    }
                    dictionary.Add(key, coloredElements);
                    
                    colorEditorSettings.ColoredElements = dictionary;
                    DataStorageUtil.UpdateDataStorage(m_doc, colorEditorSettings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply override colors by color schemes\n"+ex.Message, "Apply Colors", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            statusLable.Text = "Ready";
            progressBar.Visibility = System.Windows.Visibility.Hidden;
            return coloredElements;
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

    }
}
