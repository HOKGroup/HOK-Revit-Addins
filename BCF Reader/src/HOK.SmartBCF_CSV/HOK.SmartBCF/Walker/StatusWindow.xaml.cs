using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Google.Apis.Drive.v2.Data;
using HOK.SmartBCF.GoogleUtils;
using WinForm = System.Windows.Forms;

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window
    {
        private ColorSchemeInfo schemeInfo = new ColorSchemeInfo();
        private string colorSheetId = "";
        private ColorScheme actionScheme = null;
        private ColorScheme responsibleScheme = null;
        private List<ColorDefinition> actionDefinitions = new List<ColorDefinition>();
        private List<ColorDefinition> responsibleDefinitions = new List<ColorDefinition>();
        private Random random = new Random();

        public ColorSchemeInfo SchemeInfo { get { return schemeInfo; } set { schemeInfo = value; } }
        public List<ColorDefinition> ActionDefinitions { get { return actionDefinitions; } set { actionDefinitions = value; } }
        public List<ColorDefinition> ResponsibleDefinitions { get { return responsibleDefinitions; } set { responsibleDefinitions = value; } }
       

        public StatusWindow(ColorSchemeInfo colorSchemeInfo, string sheetId)
        {
            schemeInfo = colorSchemeInfo;
            colorSheetId = sheetId;
            GetColorDefinitionList(out actionDefinitions, out responsibleDefinitions);

            InitializeComponent();
            dataGridAction.ItemsSource = null;
            dataGridAction.ItemsSource = actionDefinitions;

            dataGridResponsibility.ItemsSource = null;
            dataGridResponsibility.ItemsSource = responsibleDefinitions;
        }


        private void GetColorDefinitionList(out List<ColorDefinition> actionList, out List<ColorDefinition> responsibleList)
        {
            actionList = new List<ColorDefinition>();
            responsibleList = new List<ColorDefinition>();
            try
            {
                foreach (ColorScheme scheme in schemeInfo.ColorSchemes)
                {
                    if (scheme.SchemeName == "BCF Action")
                    {
                        actionScheme = scheme;
                        foreach (ColorDefinition definition in scheme.ColorDefinitions)
                        {
                            actionList.Add(definition);
                        }
                    }
                    else if (scheme.SchemeName == "BCF Responsibility")
                    {
                        responsibleScheme = scheme;
                        foreach (ColorDefinition definition in scheme.ColorDefinitions)
                        {
                            responsibleList.Add(definition);
                        }
                    }
                }

                actionList = actionList.OrderBy(o => o.ParameterValue).ToList();
                responsibleList = responsibleList.OrderBy(o => o.ParameterValue).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get color definition list.\n"+ex.Message, "Get Color Definition List", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            schemeInfo = GetColorSchemeInfo(actionDefinitions, responsibleDefinitions);
            this.DialogResult = true;
        }

        private ColorSchemeInfo GetColorSchemeInfo(List<ColorDefinition> actionList, List<ColorDefinition> responsibleList)
        {
            ColorSchemeInfo colorSchemeInfo = schemeInfo;
            try
            {
                for (int i = 0; i < colorSchemeInfo.ColorSchemes.Count; i++)
                {
                    if (colorSchemeInfo.ColorSchemes[i].SchemeName == "BCF Action")
                    {
                        colorSchemeInfo.ColorSchemes[i].ColorDefinitions = actionList;
                    }
                    else if (colorSchemeInfo.ColorSchemes[i].SchemeName == "BCF Responsibility")
                    {
                        colorSchemeInfo.ColorSchemes[i].ColorDefinitions = responsibleList;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to consolidate into color scheme info fromm color definition lists.\n"+ex.Message, "Get Color Scheme Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return colorSchemeInfo;
        }

        private void buttonActionColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

                    dataGridAction.ItemsSource = null;
                    for (int i = 0; i < actionDefinitions.Count; i++)
                    {
                        if (actionDefinitions[i].ParameterValue == colorDefinition.ParameterValue)
                        {
                            actionDefinitions[i] = colorDefinition; break;
                        }
                    }
                    dataGridAction.ItemsSource = actionDefinitions;

                    bool updated = BCFParser.UpdateColorSheet(actionScheme, selectedDefinition, colorDefinition, ModifyItem.Edit, colorSheetId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set color.\n" + ex.Message, "Set Color", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonResponsibleColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

                    dataGridResponsibility.ItemsSource = null;
                    for (int i = 0; i < responsibleDefinitions.Count; i++)
                    {
                        if (responsibleDefinitions[i].ParameterValue == colorDefinition.ParameterValue)
                        {
                            responsibleDefinitions[i] = colorDefinition; break;
                        }
                    }
                    dataGridResponsibility.ItemsSource = responsibleDefinitions;

                    bool updated = BCFParser.UpdateColorSheet(responsibleScheme, selectedDefinition, colorDefinition, ModifyItem.Edit, colorSheetId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set color.\n" + ex.Message, "Set Color", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorDefinition colorDefinition = new ColorDefinition();
                byte[] colorBytes = new byte[3];
                colorBytes[0] = (byte)random.Next(256);
                colorBytes[1] = (byte)random.Next(256);
                colorBytes[2] = (byte)random.Next(256);
                colorDefinition.Color = colorBytes;

                System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(colorDefinition.Color[0], colorDefinition.Color[1], colorDefinition.Color[2]);
                colorDefinition.BackgroundColor = new SolidColorBrush(windowColor);

                var names = from name in actionDefinitions select name.ParameterValue;
                
                StatusItemWindow itemWindow = new StatusItemWindow(colorDefinition, ColorSource.Action, NewOrEdit.New);
                if (names.Count() > 0)
                {
                    itemWindow.DefinitionNames = names.ToList();
                }
                
                if (itemWindow.ShowDialog() == true)
                {
                    ColorDefinition newDefinition = itemWindow.SelColorDefinition;
                    itemWindow.Close();

                    dataGridAction.ItemsSource = null;
                    actionDefinitions.Add(newDefinition);
                    actionDefinitions = actionDefinitions.OrderBy(o => o.ParameterValue).ToList();
                    dataGridAction.ItemsSource = actionDefinitions;

                    bool updated = BCFParser.UpdateColorSheet(actionScheme, null, newDefinition, ModifyItem.Add, colorSheetId);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add Action item.\n"+ex.Message, "Add Action Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonEditAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridAction.SelectedItem)
                {
                    ColorDefinition colorDefinition = (ColorDefinition)dataGridAction.SelectedItem;
                    ColorDefinition oldDefinition = new ColorDefinition(colorDefinition);
                    string oldName = colorDefinition.ParameterValue;

                    var names = from name in actionDefinitions select name.ParameterValue;

                    StatusItemWindow itemWindow = new StatusItemWindow(colorDefinition, ColorSource.Action, NewOrEdit.Edit);
                    if (names.Count() > 0)
                    {
                        itemWindow.DefinitionNames = names.ToList();
                    }

                    if (itemWindow.ShowDialog() == true)
                    {
                        ColorDefinition newDefinition = itemWindow.SelColorDefinition;
                        itemWindow.Close();

                        dataGridAction.ItemsSource = null;
                        actionDefinitions = actionDefinitions.OrderBy(o => o.ParameterValue).ToList();
                        dataGridAction.ItemsSource = actionDefinitions;

                        bool updated = BCFParser.UpdateColorSheet(actionScheme, oldDefinition, newDefinition, ModifyItem.Edit, colorSheetId);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to edit Action item.\n" + ex.Message, "Edit Action Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDeleteAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridAction.SelectedItem)
                {
                    ColorDefinition colorDefinition = (ColorDefinition)dataGridAction.SelectedItem;
                    string oldName = colorDefinition.ParameterValue;

                    dataGridAction.ItemsSource = null;
                    for (int i = 0; i < actionDefinitions.Count; i++)
                    {
                        if (actionDefinitions[i].ParameterValue == oldName)
                        {
                            actionDefinitions.RemoveAt(i);
                        }
                    }
                    dataGridAction.ItemsSource = actionDefinitions;

                    bool updated = BCFParser.UpdateColorSheet(actionScheme, colorDefinition, null, ModifyItem.Delete, colorSheetId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete Action item.\n" + ex.Message, "Delete Action Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddRes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorDefinition colorDefinition = new ColorDefinition();
                byte[] colorBytes = new byte[3];
                colorBytes[0] = (byte)random.Next(256);
                colorBytes[1] = (byte)random.Next(256);
                colorBytes[2] = (byte)random.Next(256);
                colorDefinition.Color = colorBytes;

                System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(colorDefinition.Color[0], colorDefinition.Color[1], colorDefinition.Color[2]);
                colorDefinition.BackgroundColor = new SolidColorBrush(windowColor);

                var names = from name in responsibleDefinitions select name.ParameterValue;

                StatusItemWindow itemWindow = new StatusItemWindow(colorDefinition, ColorSource.Responsibility, NewOrEdit.New);
                if (names.Count() > 0)
                {
                    itemWindow.DefinitionNames = names.ToList();
                }

                if (itemWindow.ShowDialog() == true)
                {
                    ColorDefinition newDefinition = itemWindow.SelColorDefinition;
                    itemWindow.Close();

                    dataGridResponsibility.ItemsSource = null;
                    responsibleDefinitions.Add(newDefinition);
                    responsibleDefinitions = responsibleDefinitions.OrderBy(o => o.ParameterValue).ToList();
                    dataGridResponsibility.ItemsSource = responsibleDefinitions;

                    bool updated = BCFParser.UpdateColorSheet(responsibleScheme, null, newDefinition, ModifyItem.Add, colorSheetId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add Responsibility item.\n" + ex.Message, "Add Responsibility Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonEditRes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridResponsibility.SelectedItem)
                {
                    ColorDefinition colorDefinition = (ColorDefinition)dataGridResponsibility.SelectedItem;
                    ColorDefinition oldDefinition = new ColorDefinition(colorDefinition);
                    string oldName = colorDefinition.ParameterValue;

                    var names = from name in responsibleDefinitions select name.ParameterValue;

                    StatusItemWindow itemWindow = new StatusItemWindow(colorDefinition, ColorSource.Responsibility, NewOrEdit.Edit);
                    if (names.Count() > 0)
                    {
                        itemWindow.DefinitionNames = names.ToList();
                    }

                    if (itemWindow.ShowDialog() == true)
                    {
                        ColorDefinition newDefinition = itemWindow.SelColorDefinition;
                        itemWindow.Close();

                        dataGridResponsibility.ItemsSource = null;
                        responsibleDefinitions = responsibleDefinitions.OrderBy(o => o.ParameterValue).ToList();
                        dataGridResponsibility.ItemsSource = responsibleDefinitions;

                        bool updated = BCFParser.UpdateColorSheet(responsibleScheme, oldDefinition, newDefinition, ModifyItem.Edit, colorSheetId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to edit Responsibility item.\n" + ex.Message, "Edit Responsibility Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDeleteRes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridResponsibility.SelectedItem)
                {
                    ColorDefinition colorDefinition = (ColorDefinition)dataGridResponsibility.SelectedItem;
                    string oldName = colorDefinition.ParameterValue;

                    dataGridResponsibility.ItemsSource = null;
                    for (int i = 0; i < responsibleDefinitions.Count; i++)
                    {
                        if (responsibleDefinitions[i].ParameterValue == oldName)
                        {
                            responsibleDefinitions.RemoveAt(i);
                        }
                    }
                    dataGridResponsibility.ItemsSource = responsibleDefinitions;

                    bool updated = BCFParser.UpdateColorSheet(responsibleScheme, colorDefinition, null, ModifyItem.Delete, colorSheetId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete Responsibility item.\n" + ex.Message, "Delete Responsibility Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private delegate void UpdateLableDelegate(System.Windows.DependencyProperty dp, Object value);

        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            ProjectWindow projectWindow = new ProjectWindow();
            if (projectWindow.ShowDialog() == true)
            {
             
                string labelText = "Importing Color Schemes...";
                UpdateLableDelegate updateLabelDelegate = new UpdateLableDelegate(statusLable.SetValue);
                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, labelText });

                string projectId = projectWindow.ProjectId;
                File colorSheet = FileManager.FindSubItemByFolderId("Color Schemes", projectId);
                ColorSchemeInfo colorInfo = BCFParser.ReadColorSchemes(colorSheet.Id, true);

                List<ColorDefinition> actionColors = new List<ColorDefinition>();
                List<ColorDefinition> responsibilityColors = new List<ColorDefinition>();

                foreach (ColorScheme colorScheme in colorInfo.ColorSchemes)
                {
                    switch (colorScheme.SchemeName)
                    {
                        case "BCF Action":
                            actionColors = colorScheme.ColorDefinitions;
                            break;
                        case "BCF Responsibility":
                            responsibilityColors = colorScheme.ColorDefinitions;
                            break;
                    }
                }

                actionDefinitions = actionColors.OrderBy(o => o.ParameterValue).ToList();
                responsibleDefinitions = responsibilityColors.OrderBy(o => o.ParameterValue).ToList();

                dataGridAction.ItemsSource = null;
                dataGridAction.ItemsSource = actionDefinitions;

                dataGridResponsibility.ItemsSource = null;
                dataGridResponsibility.ItemsSource = responsibleDefinitions;

                bool writeColor = BCFParser.WriteColorSheet(colorInfo, colorSheetId);
                labelText = "Ready";
                Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, labelText });
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
    }
}
