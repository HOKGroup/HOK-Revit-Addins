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
using System.Windows.Shapes;

namespace HOK.SheetDataEditor
{
    public enum AddItemType
    {
        Views, Revisions, None
    }
    /// <summary>
    /// Interaction logic for ItemWindow.xaml
    /// </summary>
    public partial class ItemWindow : Window
    {
        private AddItemType itemType = AddItemType.None;
        private RevitSheetData sheetData = null;
        private Guid selectedSheetId = Guid.Empty;

        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }

        public ItemWindow(AddItemType type, RevitSheetData data, Guid sheetId)
        {
            itemType = type;
            sheetData = data;
            selectedSheetId = sheetId;
                 
            InitializeComponent();
            if (itemType == AddItemType.Views)
            {
                this.Title = "Add View Items";
            }
            else if (itemType == AddItemType.Revisions)
            {
                this.Title = "Add Revision Items";
            }
            DisplayItems();
        }

        private void DisplayItems()
        {
            try
            {
                 DataGridCheckBoxColumn checkBoxColumn = new DataGridCheckBoxColumn();
                 checkBoxColumn.Binding = new Binding("IsSelected");
                 dataGridItem.Columns.Add(checkBoxColumn);

                if (itemType == AddItemType.Views)
                {
                    DataGridTextColumn typeColumn = new DataGridTextColumn();
                    typeColumn.Header = "View Type";
                    typeColumn.Width = new DataGridLength(0.5, DataGridLengthUnitType.Star);
                    typeColumn.Binding = new Binding("ViewTypeName");
                    typeColumn.IsReadOnly = true;
                    dataGridItem.Columns.Add(typeColumn);

                    DataGridTextColumn nameColumn = new DataGridTextColumn();
                    nameColumn.Header = "View Name";
                    nameColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    nameColumn.Binding = new Binding("Name");
                    nameColumn.IsReadOnly = true;
                    dataGridItem.Columns.Add(nameColumn);

                    var views = from view in sheetData.Views.Values where null == view.Sheet select view;
                    if (views.Count() > 0)
                    {
                        List<RevitView> revitViews = views.OrderBy(o => o.Name).ToList();
                        dataGridItem.ItemsSource = revitViews;
                    }
                }
                else if (itemType == AddItemType.Revisions)
                {
                    DataGridTextColumn descriptionColumn = new DataGridTextColumn();
                    descriptionColumn.Header = "Description";
                    descriptionColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    descriptionColumn.Binding = new Binding("Description");
                    descriptionColumn.IsReadOnly = true;
                    dataGridItem.Columns.Add(descriptionColumn);

                    List<Guid> selectedIds = new List<Guid>();
                    var selectedRevisionIds = from selectedRevision in sheetData.RevisionMatrix.Values where selectedRevision.SheetId == selectedSheetId select selectedRevision.RevisionId;
                    if (selectedRevisionIds.Count() > 0)
                    {
                        selectedIds = selectedRevisionIds.ToList();
                    }

                    var revisionItems = from revision in sheetData.Revisions.Values where !selectedIds.Contains(revision.Id) select revision;
                    if (revisionItems.Count() > 0)
                    {
                        dataGridItem.ItemsSource = revisionItems.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display items to be added.\n"+ex.Message, "Display Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemType == AddItemType.Views)
                {
                    List<RevitView> views = (List<RevitView>)dataGridItem.ItemsSource;
                    List<RevitView> updatedViews = new List<RevitView>();
                    foreach (RevitView view in views)
                    {
                        RevitView selectedView = view;
                        selectedView.IsSelected = true;
                        updatedViews.Add(selectedView);
                    }

                    dataGridItem.ItemsSource = null;
                    dataGridItem.ItemsSource = updatedViews;
                }
                else if (itemType == AddItemType.Revisions)
                {
                    List<RevitRevision> revisions = (List<RevitRevision>)dataGridItem.ItemsSource;
                    List<RevitRevision> updatedRevisions = new List<RevitRevision>();
                    foreach (RevitRevision revision in revisions)
                    {
                        RevitRevision selectedRevision = revision;
                        selectedRevision.IsSelected = true;
                        updatedRevisions.Add(selectedRevision);
                    }

                    dataGridItem.ItemsSource = null;
                    dataGridItem.ItemsSource = updatedRevisions;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select all items.\n"+ex.Message, "Select All Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemType == AddItemType.Views)
                {
                    List<RevitView> views = (List<RevitView>)dataGridItem.ItemsSource;
                    List<RevitView> updatedViews = new List<RevitView>();
                    foreach (RevitView view in views)
                    {
                        RevitView selectedView = view;
                        selectedView.IsSelected = false;
                        updatedViews.Add(selectedView);
                    }

                    dataGridItem.ItemsSource = null;
                    dataGridItem.ItemsSource = updatedViews;
                }
                else if (itemType == AddItemType.Revisions)
                {
                    List<RevitRevision> revisions = (List<RevitRevision>)dataGridItem.ItemsSource;
                    List<RevitRevision> updatedRevisions = new List<RevitRevision>();
                    foreach (RevitRevision revision in revisions)
                    {
                        RevitRevision selectedRevision = revision;
                        selectedRevision.IsSelected = false;
                        updatedRevisions.Add(selectedRevision);
                    }

                    dataGridItem.ItemsSource = null;
                    dataGridItem.ItemsSource = updatedRevisions;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unselect all items.\n"+ex.Message, "Unselect All Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemType == AddItemType.Views)
                {
                    List<RevitView> views = (List<RevitView>)dataGridItem.ItemsSource;
                    var selectedViewIds = from view in views where view.IsSelected select view.Id;
                    if (selectedViewIds.Count() > 0)
                    {
                        RevitSheet SheetItem = sheetData.Sheets[selectedSheetId];
                        List<Guid> viewToAdd = selectedViewIds.ToList();
                        foreach (Guid viewId in viewToAdd)
                        {
                            if (sheetData.Views.ContainsKey(viewId))
                            {
                                RevitView viewItem = sheetData.Views[viewId];
                                viewItem.IsSelected = false;
                                viewItem.SetSheet(SheetItem);
                                sheetData.Views.Remove(viewId);
                                sheetData.Views.Add(viewId, viewItem);
                                bool added = DatabaseUtil.UpdateViewOnSheet(selectedSheetId, viewId, true);
                            }
                        }
                    }
                }
                else if (itemType == AddItemType.Revisions)
                {
                    List<RevitRevision> revisions = (List<RevitRevision>)dataGridItem.ItemsSource;
                    var selectedRevisionIds = from revision in revisions where revision.IsSelected select revision.Id;
                    if (selectedRevisionIds.Count() > 0)
                    {
                        List<Guid> revisionToAdd = selectedRevisionIds.ToList();
                        foreach (Guid revisionId in revisionToAdd)
                        {
                            //update database
                            RevisionOnSheet revisionOnSheet = new RevisionOnSheet(Guid.NewGuid(), selectedSheetId, revisionId);
                            bool added = DatabaseUtil.InsertRevisionOnSheet(revisionOnSheet);
                            if (added)
                            {
                                sheetData.RevisionMatrix.Add(revisionOnSheet.MapId, revisionOnSheet);
                            }
                        }
                    }
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add items from the list.\n"+ex.Message, "Add Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
