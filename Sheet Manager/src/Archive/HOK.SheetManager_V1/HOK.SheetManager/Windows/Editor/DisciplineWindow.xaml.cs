using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
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
    /// <summary>
    /// Interaction logic for DisciplineWindow.xaml
    /// </summary>
    public partial class DisciplineWindow : Window
    {
        private RevitSheetData rvtSheetData = null;

        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }

        public DisciplineWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rvtSheetData = this.DataContext as RevitSheetData;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Discipline discipline = new Discipline(Guid.NewGuid(), "New Discipline");
                this.RvtSheetData.Disciplines.Add(discipline);
                bool databaseUpdated = SheetDataWriter.ChangeDisciplineItem(discipline, CommandType.INSERT);
            }
            catch (Exception ex)
            {
                string messag = ex.Message;
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridDisciplines.SelectedItem)
                {
                    MessageBoxResult msgResult = MessageBox.Show("Sheet items under the selected discipline will be set to [Undefined].\nWould you like to continue?", "Sheet Items Found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (msgResult == MessageBoxResult.Yes)
                    {
                        Discipline selectedDiscipline = dataGridDisciplines.SelectedItem as Discipline;
                        var undefinedDiscipline = from discipline in rvtSheetData.Disciplines where discipline.Id == Guid.Empty select discipline;
                        if (undefinedDiscipline.Count() > 0)
                        {
                            Discipline undefined = undefinedDiscipline.First() as Discipline;
                            var sheetsToChange = from sheet in rvtSheetData.Sheets where sheet.DisciplineObj.Id == selectedDiscipline.Id select sheet;
                            if (sheetsToChange.Count() > 0)
                            {
                                foreach (RevitSheet sheet in sheetsToChange)
                                {
                                    int index = rvtSheetData.Sheets.IndexOf(sheet);
                                    this.RvtSheetData.Sheets[index].DisciplineObj = undefined;
                                }
                            }
                        }

                        this.RvtSheetData.Disciplines.Remove(selectedDiscipline);
                        bool databaseUpdated = SheetDataWriter.ChangeDisciplineItem(selectedDiscipline, CommandType.DELETE);
                    }
                }
            }
            catch (Exception ex)
            {
                string messag = ex.Message;
            }
        }

        private void dataGridDisciplines_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                //discipline name changed

                DataGridRow row = e.Row;
                TextBox textBox = e.EditingElement as TextBox;
                if (null != row && null != textBox)
                {
                    Discipline discipline = row.Item as Discipline;
                    discipline.Name = textBox.Text;
                    bool databaseUpdated = SheetDataWriter.ChangeDisciplineItem(discipline, CommandType.UPDATE);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
           
        }

    }
}
