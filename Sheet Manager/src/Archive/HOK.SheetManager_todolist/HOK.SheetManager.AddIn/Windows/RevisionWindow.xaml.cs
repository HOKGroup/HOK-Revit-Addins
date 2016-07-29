using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.SheetManager.AddIn.Windows
{
    /// <summary>
    /// Interaction logic for RevisionWindow.xaml
    /// </summary>
    public partial class RevisionWindow : Window
    {
        private Guid linkedProjectId = Guid.Empty;

        private RevitSheetData rvtSheetData = null;
       
        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; } }

        public RevisionWindow(Guid projectId)
        {
            linkedProjectId = projectId;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtSheetData = this.DataContext as RevitSheetData;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
         
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < rvtSheetData.Revisions.Count; i++)
                {
                    this.RvtSheetData.Revisions[i].LinkStatus.IsSelected = true;
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
                for (int i = 0; i < rvtSheetData.Revisions.Count; i++)
                {
                    this.RvtSheetData.Revisions[i].LinkStatus.IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

       
        
    }
}
