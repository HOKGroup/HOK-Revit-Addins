using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using HOK.SheetManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace HOK.SheetManager.Windows.Editor
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
        private RevitSheetData rvtSheetData = null;
        private AddItemType itemType = AddItemType.None;
        private RevitSheet selectedSheet = null;

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }

        public ItemWindow(AddItemType type, RevitSheet sheet)
        {
            itemType = type;
            selectedSheet = sheet;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtSheetData = this.DataContext as RevitSheetData;

                if (itemType == AddItemType.Views)
                {
                    this.Title = "Add View Items";

                    DataGridTextColumn typeColumn = new DataGridTextColumn();
                    typeColumn.Header = "View Type";
                    typeColumn.Width = new DataGridLength(0.5, DataGridLengthUnitType.Star);
                    typeColumn.Binding = new Binding("ViewType.Name");
                    typeColumn.IsReadOnly = true;
                    dataGridItem.Columns.Add(typeColumn);

                    DataGridTextColumn nameColumn = new DataGridTextColumn();
                    nameColumn.Header = "View Name";
                    nameColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    nameColumn.Binding = new Binding("Name");
                    nameColumn.IsReadOnly = true;
                    dataGridItem.Columns.Add(nameColumn);

                    var views = from view in rvtSheetData.Views where view.Sheet.Id == Guid.Empty select view;
                    if (views.Count() > 0)
                    {
                        List<RevitView> revitViews = views.OrderBy(o => o.Name).ToList();
                        for (int i = 0; i < revitViews.Count; i++)
                        {
                            revitViews[i].LinkStatus.IsSelected = false;
                        }

                        dataGridItem.ItemsSource = new ObservableCollection<object>(revitViews);
                    }
                }
                else if (itemType == AddItemType.Revisions)
                {
                    this.Title = "Add Revision Items";

                    DataGridTextColumn descriptionColumn = new DataGridTextColumn();
                    descriptionColumn.Header = "Description";
                    descriptionColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    descriptionColumn.Binding = new Binding("Description");
                    descriptionColumn.IsReadOnly = true;
                    dataGridItem.Columns.Add(descriptionColumn);

                    for (int i = 0; i < rvtSheetData.Revisions.Count; i++)
                    {
                        rvtSheetData.Revisions[i].LinkStatus.IsSelected = false;
                    }

                    var revisions = from revision in rvtSheetData.Revisions where !selectedSheet.SheetRevisions[revision.Id].Include select revision;
                    if (revisions.Count() > 0)
                    {
                        dataGridItem.ItemsSource = new ObservableCollection<object>(revisions);
                    }
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<object> itemSource = dataGridItem.ItemsSource as ObservableCollection<object>;
                
                for (int i = 0; i < itemSource.Count; i++)
                {
                    switch (itemType)
                    {
                        case AddItemType.Revisions:
                            (itemSource[i] as RevitRevision).LinkStatus.IsSelected = true;
                            break;
                        case AddItemType.Views:
                            (itemSource[i] as RevitView).LinkStatus.IsSelected = true;
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<object> itemSource = dataGridItem.ItemsSource as ObservableCollection<object>;
                for (int i = 0; i < itemSource.Count; i++)
                {
                    switch (itemType)
                    {
                        case AddItemType.Revisions:
                            (itemSource[i] as RevitRevision).LinkStatus.IsSelected = false;
                            break;
                        case AddItemType.Views:
                            (itemSource[i] as RevitView).LinkStatus.IsSelected = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<object> itemSource = dataGridItem.ItemsSource as ObservableCollection<object>;
                if (itemType == AddItemType.Views)
                {
                    List<RevitView> views = itemSource.Cast<RevitView>().ToList();
                    var selectedViews = from view in views where view.LinkStatus.IsSelected select view;
                    if (selectedViews.Count() > 0)
                    {
                        foreach (RevitView view in selectedViews)
                        {
                            int index = rvtSheetData.Views.IndexOf(view);
                            this.RvtSheetData.Views[index].Sheet = selectedSheet;
                            bool dbUpdated = SheetDataWriter.UpdateViewOnSheet(selectedSheet.Id, view.Id);
                        }
                    }
                    this.DialogResult = true;
                }
                else if (itemType == AddItemType.Revisions)
                {
                    List<RevitRevision> revisions = itemSource.Cast<RevitRevision>().ToList();
                    var selectedRevisions = from revision in revisions where revision.LinkStatus.IsSelected select revision;
                    if (selectedRevisions.Count() > 0)
                    {
                        int sheetIndex = rvtSheetData.Sheets.IndexOf(selectedSheet);
                        foreach (RevitRevision revision in selectedRevisions)
                        {
                            this.RvtSheetData.Sheets[sheetIndex].SheetRevisions[revision.Id].Include = true;
                            bool dbUpdated = SheetDataWriter.ChangeRevisionOnSheet(rvtSheetData.Sheets[sheetIndex].SheetRevisions[revision.Id], CommandType.UPDATE);
                        }
                    }
                    this.DialogResult = true;
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        
    }
}
