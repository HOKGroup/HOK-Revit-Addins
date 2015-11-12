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
using System.Windows.Shapes;

namespace HOK.SheetDataEditor
{
    /// <summary>
    /// Interaction logic for DisciplineWindow.xaml
    /// </summary>
    public partial class DisciplineWindow : Window
    {
        private RevitSheetData sheetData = null;
        private Dictionary<Guid, Discipline> itemDictionary = new Dictionary<Guid, Discipline>();
        private ObservableCollection<object> disciplineItems = new ObservableCollection<object>();

        public RevitSheetData SheetData { get { return sheetData; } set { sheetData = value; } }

        public DisciplineWindow(RevitSheetData revitSheetData)
        {
            sheetData = revitSheetData;
            itemDictionary = sheetData.Disciplines.ToDictionary(o => o.Key, o => o.Value);
            InitializeComponent();

            DisplayDisciplineItems();
        }

        private void DisplayDisciplineItems()
        {
            try
            {
                dataGridDisciplines.ItemsSource = null;
                List<Discipline> disciplineList = itemDictionary.Values.OrderBy(o => o.Name).ToList();
                foreach (Discipline discipline in disciplineList)
                {
                    disciplineItems.Add(discipline);
                }
                bool dummyAdded = false;
                if (disciplineItems.Count == 0)
                {
                    disciplineItems.Add(new Discipline());
                    dummyAdded = true;
                }
                dataGridDisciplines.ItemsSource = disciplineItems;
                if (dummyAdded)
                {
                    disciplineItems.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void dataGridDisciplines_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Delete)
                {
                    if (dataGridDisciplines.SelectedItems.Count > 0)
                    {
                        Discipline selectedDisciline = dataGridDisciplines.SelectedItems[0] as Discipline;
                        if (null != selectedDisciline)
                        {
                            if (selectedDisciline.Id == Guid.Empty) { e.Handled = true; }//default guid;

                            var sheetsFound = from sheet in sheetData.Sheets.Values where sheet.DisciplineObj.Id == selectedDisciline.Id select sheet;
                            int sheetsExisting = sheetsFound.Count();
                            if (sheetsExisting > 0)
                            {
                                MessageBoxResult msgResult = MessageBox.Show("The selected discipline has been assigned " + sheetsExisting + " sheet items.\nAre you sure you want to delete the discipline?", "Delete Discipline", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (msgResult == MessageBoxResult.No)
                                {
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool CleanEmptyItems()
        {
            bool result = false;
            try
            {
                disciplineItems = (ObservableCollection<object>)dataGridDisciplines.ItemsSource;
                itemDictionary = new Dictionary<Guid, Discipline>();
                foreach (Discipline discipline in disciplineItems)
                {
                    Discipline newItem = discipline;
                    if (string.IsNullOrEmpty(newItem.Name) || newItem.Name=="Undefined") { continue; }
                    if (newItem.Id == Guid.Empty)
                    {
                        newItem.Id = Guid.NewGuid();
                    }
                    if (!itemDictionary.ContainsKey(newItem.Id))
                    {
                        itemDictionary.Add(newItem.Id, newItem);
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CleanEmptyItems();

                Dictionary<Guid, Discipline> existingItems = sheetData.Disciplines;
                foreach (Guid itemId in itemDictionary.Keys)
                {
                    Discipline disciplineItem = itemDictionary[itemId];
                    if (existingItems.ContainsKey(itemId))
                    {
                        //update
                        bool updated = DatabaseUtil.ChangeDisciplineItem(disciplineItem, CommandType.UPDATE);
                        existingItems.Remove(itemId);
                    }
                    else
                    {
                        //insert new item
                        bool inserted = DatabaseUtil.ChangeDisciplineItem(disciplineItem, CommandType.INSERT);
                    }
                }

                foreach (Discipline disciplineItem in existingItems.Values)
                {
                    if (disciplineItem.Id == Guid.Empty) { continue; }
                    //remove deleted items
                    bool deleted = DatabaseUtil.ChangeDisciplineItem(disciplineItem, CommandType.DELETE);
                    var sheetIds = from sheet in sheetData.Sheets.Values where sheet.DisciplineObj.Id == disciplineItem.Id select sheet.Id;
                    if (sheetIds.Count() > 0)
                    {
                        List<Guid> sheetIdFound = sheetIds.ToList();
                        foreach (Guid guid in sheetIdFound)
                        {
                            RevitSheet rSheet = sheetData.Sheets[guid];
                            rSheet.DisciplineObj = new Discipline();
                            sheetData.Sheets.Remove(guid);
                            sheetData.Sheets.Add(guid, rSheet);
                        }
                    }
                }

                if(!itemDictionary.ContainsKey(Guid.Empty))
                {
                    itemDictionary.Add(Guid.Empty, new Discipline());
                }
                sheetData.Disciplines = itemDictionary;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

       
    }
}
