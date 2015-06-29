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
using Google.Apis.Drive.v2.Data;
using HOK.SmartBCF.GoogleUtils;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for OnlineBCFWindow.xaml
    /// </summary>
    public partial class OnlineBCFWindow : Window
    {
        private string projectId = "";
        private FolderHolders googleFolders = null;

        private List<OnlineBCFInfo> onlineBCFs = new List<OnlineBCFInfo>();
       
        public List<OnlineBCFInfo> OnlineBCFs { get { return onlineBCFs; } set { onlineBCFs = value; } }
       
        public OnlineBCFWindow(FolderHolders folders)
        {
            googleFolders = folders;
            projectId = googleFolders.RootId;
            InitializeComponent();
            labelProjectId.Content = projectId;
            DisplaySheetList();
        }

        private void DisplaySheetList()
        {
            try
            {
                if (null!=googleFolders && !string.IsNullOrEmpty(projectId))
                {
                    onlineBCFs = FileManager.GetOnlineBCFs(googleFolders);
                    onlineBCFs = onlineBCFs.OrderBy(o => o.SheetTitle).ToList();
                    dataGridFiles.ItemsSource = null;
                    dataGridFiles.ItemsSource = onlineBCFs;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display lists of BCF Google Spreadsheet.\n"+ex.Message, "Display BCF Lists", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
   

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = true;
        }

        private delegate void UpdateLableDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateProgressDelegate(System.Windows.DependencyProperty dp, Object value);

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

    public class OnlineBCFInfo
    {
        private string bcfProjectId = "";
        private string markupSheetId = "";
        private string viewpointSheetId = "";
        private string sheetTitle = "";
        private string sheetOwner = "";
        private string sheetModified = "";
        private File markupFile = null;
        private File viewpointFile = null;
        private List<File> imageFiles = new List<File>();
        private File archiveFile = null;
        private bool isSelected = false;

        public string BCFProjectId { get { return bcfProjectId; } set { bcfProjectId = value; } }
        public string MarkupSheetId { get { return markupSheetId; } set { markupSheetId = value; } }
        public string ViewpointSheetId { get { return viewpointSheetId; } set { viewpointSheetId = value; } }
        public string SheetTitle { get { return sheetTitle; } set { sheetTitle = value; } }
        public string SheetOwner { get { return sheetOwner; } set { sheetOwner = value; } }
        public string SheetModified { get { return sheetModified; } set { sheetModified = value; } }
        public File MarkupFile { get { return markupFile; } set { markupFile = value; } }
        public File ViewpointFile { get { return viewpointFile; } set { viewpointFile = value; } }
        public List<File> ImageFiles { get { return imageFiles; } set { imageFiles = value; } }
        public File ArchiveFile { get { return archiveFile; } set { archiveFile = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public OnlineBCFInfo(string rootId, File markupSheet, File viewpointSheet)
        {
            bcfProjectId = rootId;
            markupFile = markupSheet;
            markupSheetId = markupSheet.Id;
            viewpointFile = viewpointSheet;
            viewpointSheetId = viewpointSheet.Id;

            sheetTitle = markupSheet.Title;
            foreach (string owner in markupSheet.OwnerNames)
            {
                sheetOwner = owner; break;
            }
            sheetModified = markupSheet.ModifiedDate.ToString();
        }
    }
}
