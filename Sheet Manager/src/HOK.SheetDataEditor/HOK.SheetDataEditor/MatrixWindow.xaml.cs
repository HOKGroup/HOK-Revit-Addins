using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HOK.SheetDataEditor
{
    /// <summary>
    /// Interaction logic for MatrixWindow.xaml
    /// </summary>
    public partial class MatrixWindow : Window
    {
        private RevitSheetData sheetData = null;
        private bool editMode = false;

        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }

        public MatrixWindow(RevitSheetData data)
        {
            sheetData = data;
            InitializeComponent();
            CollectRevisions();
        }
        
        private void CollectRevisions()
        {
            try
            {
                var schedules = new ObservableCollection<RevisionSchedule>();

                if (sheetData.Sheets.Count >0 && sheetData.Revisions.Count > 0 )
                {
                    List<Guid> revisionIds = sheetData.Revisions.Keys.ToList();

                    List<RevitSheet> sheets = sheetData.Sheets.Values.OrderBy(o => o.Number).ToList();
                    foreach (RevitSheet sheet in sheets)
                    {
                        RevisionSchedule schedule = new RevisionSchedule();
                        schedule.SheetObj = sheet;
                        schedule.SheetInfo = sheet.Number + " - " + sheet.Name;

                        Dictionary<Guid, bool> revisions = new Dictionary<Guid, bool>();
                        foreach (Guid revisionId in revisionIds)
                        {
                            bool selected = false;
                            var maps = from map in sheetData.RevisionMatrix.Values where map.SheetId == sheet.Id && map.RevisionId == revisionId select map;
                            if (maps.Count() > 0)
                            {
                                selected = true;
                            }

                            if (!revisions.ContainsKey(revisionId))
                            {
                                revisions.Add(revisionId, selected);
                            }
                        }
                        schedule.Revisions = revisions;
                        schedules.Add(schedule);
                    }
                    dataGridMatrix.ItemsSource = schedules;
                }

                foreach (RevitRevision revision in sheetData.Revisions.Values)
                {
                    DataGridTemplateColumn column = new DataGridTemplateColumn();
                    column.Header = revision.Description; 

                    DataTemplate dataTemplate = new DataTemplate();
                    FrameworkElementFactory elementFactory = new FrameworkElementFactory(typeof(CheckBox));
                    elementFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    elementFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                    elementFactory.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(OnChecked));
                    elementFactory.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(OnUnchecked));
                    elementFactory.SetBinding(ToggleButton.IsCheckedProperty, new Binding("Revisions[" + revision.Id + "]") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    dataTemplate.VisualTree = elementFactory;
                    column.CellTemplate = dataTemplate;

                    dataGridMatrix.Columns.Add(column);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect revisions.\n"+ex.Message, "Collect Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (editMode)
                {
                    if (null != sender)
                    {
                        CheckBox checkBox = sender as CheckBox;
                        Guid sheetId = Guid.Empty;
                        Guid revisionId = Guid.Empty;
                        bool found = GetCheckedIds(checkBox, out sheetId, out revisionId);
                        if (found)
                        {
                            Guid mapId = Guid.NewGuid();
                            RevisionOnSheet revisionOnSheet = new RevisionOnSheet(mapId, sheetId, revisionId);

                            bool added = DatabaseUtil.InsertRevisionOnSheet(revisionOnSheet);
                            if (added)
                            {
                                if(!sheetData.RevisionMatrix.ContainsKey(revisionOnSheet.MapId))
                                {
                                    sheetData.RevisionMatrix.Add(revisionOnSheet.MapId, revisionOnSheet);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to fire chekcbox event.\n"+ex.Message, "Checkbox Checked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }

        private void OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (editMode)
                {
                    if (null != sender)
                    {
                        CheckBox checkBox = sender as CheckBox;
                        Guid sheetId = Guid.Empty;
                        Guid revisionId = Guid.Empty;
                        bool found = GetCheckedIds(checkBox, out sheetId, out revisionId);
                        if (found)
                        {
                            bool deleted = DatabaseUtil.DeleteRevisionOnSheet(sheetId, revisionId);
                            if (deleted)
                            {
                                var deletedMap = from map in sheetData.RevisionMatrix.Values where map.SheetId == sheetId && map.RevisionId == revisionId select map.MapId;
                                if (deletedMap.Count() > 0)
                                {
                                    List<Guid> deletedMapIds = deletedMap.ToList();
                                    foreach (Guid mapId in deletedMapIds)
                                    {
                                        sheetData.RevisionMatrix.Remove(mapId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to fire checkbox event.\n"+ex.Message, "Checkbox Unchecked", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool GetCheckedIds(CheckBox checkBox, out Guid sheetId, out Guid revisionId)
        {
            bool found = false;
            revisionId = Guid.Empty;
            sheetId = Guid.Empty;
            try
            {
                Binding binding = BindingOperations.GetBinding(checkBox, ToggleButton.IsCheckedProperty);
                string sortMemberPath = binding.Path.Path;
                var mg = Regex.Match(sortMemberPath, @"\[(.*?)\]");
                if (mg.Success)
                {
                    string value = mg.Groups[1].Value;
                    revisionId = new Guid(value);
                }

                var parent = VisualTreeHelper.GetParent(checkBox);
                while (parent != null && parent.GetType() != typeof(DataGridRow))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                DataGridRow datarow = parent as DataGridRow;
                if (null != datarow)
                {
                    RevisionSchedule schedule = datarow.Item as RevisionSchedule;
                    if (null != schedule)
                    {
                        sheetId = schedule.SheetObj.Id;
                    }
                }

                if (sheetId != Guid.Empty && revisionId != Guid.Empty)
                {
                    found = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to checked Ids.\n"+ex.Message, "Get Checked Ids", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return found;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            editMode = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = true;
        }

    }


    
    public class RevisionSchedule : INotifyPropertyChanged
    {
        private RevitSheet sheetObj = null;
        private string sheetInfo = "";
        private Dictionary<Guid, bool> revisions = new Dictionary<Guid, bool>();

        public RevitSheet SheetObj { get { return sheetObj; } set { sheetObj = value; } }
        public string SheetInfo { get { return sheetInfo; } set { sheetInfo = value; } }
        public Dictionary<Guid, bool> Revisions { get { return revisions; } set { revisions = value; } }

        public RevisionSchedule()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
