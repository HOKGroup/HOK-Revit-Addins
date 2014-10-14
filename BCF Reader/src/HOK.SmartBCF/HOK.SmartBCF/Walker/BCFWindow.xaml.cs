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
using HOK.SmartBCF.GoogleUtils;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for BCFWindow.xaml
    /// </summary>
    public partial class BCFWindow : Window
    {
        private Dictionary<string/*fileId*/, LinkedBcfFileInfo> bcfFileDictionary = new Dictionary<string, LinkedBcfFileInfo>();
        private List<LinkedBcfFileInfo> linkedBCFs = new List<LinkedBcfFileInfo>();
        private string bcfProjectId = "";
        private string bcfColorSchemeId = "";
        private string sharedLink = "";
        private FolderHolders googleFolders = null;

        public Dictionary<string, LinkedBcfFileInfo> BCFFileDictionary { get { return bcfFileDictionary; } set { bcfFileDictionary = value; } }
        public string BCFProjectId { get { return bcfProjectId; } set { bcfProjectId = value; } }
        public string BCFColorSchemeId { get { return bcfColorSchemeId; } set { bcfColorSchemeId = value; } }
        public FolderHolders GoogleFolders { get { return googleFolders; } set { googleFolders = value; } }

        public BCFWindow(Dictionary<string, LinkedBcfFileInfo> fileHistory, FolderHolders folders)
        {
            bcfFileDictionary = fileHistory;
            googleFolders = folders;
            if (null != googleFolders)
            {
                bcfProjectId = googleFolders.RootId;
                bcfColorSchemeId = googleFolders.ColorSheet.Id;
            }

            foreach (LinkedBcfFileInfo info in bcfFileDictionary.Values)
            {
                linkedBCFs.Add(info);
            }
            linkedBCFs = linkedBCFs.OrderBy(o => o.BCFName).ToList();

            InitializeComponent();
            if (!string.IsNullOrEmpty(bcfProjectId))
            {
                textBoxId.Text = bcfProjectId;
                sharedLink = FileManager.GetSharedLinkAddress(bcfProjectId);
            }
            dataGridBCFs.ItemsSource = null;
            dataGridBCFs.ItemsSource = linkedBCFs;
        }

        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBoxId.Text))
                {
                    if (textBoxId.Text != bcfProjectId)
                    {
                        bcfProjectId = textBoxId.Text;
                        sharedLink = FileManager.GetSharedLinkAddress(bcfProjectId);
                        googleFolders = FileManager.CreateDefaultFolders(textBoxId.Text);
                    }
                }
                ImportBCFWindow importWindow = new ImportBCFWindow(googleFolders);
                if (importWindow.ShowDialog() == true)
                {
                    if (importWindow.RememberProjectId)
                    {
                        bcfProjectId = importWindow.BCFProjectId;
                        bcfColorSchemeId = importWindow.BCFColorSchemeId;
                        googleFolders = importWindow.GoogleFolders;
                        if (!string.IsNullOrEmpty(bcfProjectId))
                        {
                            textBoxId.Text = bcfProjectId;
                        }
                    }
                    LinkedBcfFileInfo fileInfo = importWindow.BCFFileInfo;
                    
                    dataGridBCFs.ItemsSource = null;
                    linkedBCFs.Add(fileInfo);
                    linkedBCFs = linkedBCFs.OrderBy(o => o.BCFName).ToList();
                    dataGridBCFs.ItemsSource = linkedBCFs;
                    importWindow.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import BCF.\n" + ex.Message, "Import BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
       
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bcfProjectId = textBoxId.Text;
                sharedLink = FileManager.GetSharedLinkAddress(bcfProjectId);
                if (!string.IsNullOrEmpty(bcfProjectId))
                {
                    if (null == googleFolders)
                    {
                        googleFolders = FileManager.CreateDefaultFolders(bcfProjectId);
                    }

                    if (null != googleFolders)
                    {
                        //add online BCF
                        OnlineBCFWindow onlineBCFWindow = new OnlineBCFWindow(googleFolders);
                        if (onlineBCFWindow.ShowDialog() == true)
                        {
                            List<OnlineBCFInfo> onlineBCFs = onlineBCFWindow.OnlineBCFs;

                            foreach (OnlineBCFInfo info in onlineBCFs)
                            {
                                if (info.IsSelected)
                                {
                                    LinkedBcfFileInfo linkedBCF = new LinkedBcfFileInfo(info.SheetTitle, info.SpreadsheetId, sharedLink, bcfProjectId, googleFolders.RootTitle);
                                    dataGridBCFs.ItemsSource = null;
                                    linkedBCFs.Add(linkedBCF);
                                    linkedBCFs = linkedBCFs.OrderBy(o => o.BCFName).ToList();
                                    dataGridBCFs.ItemsSource = linkedBCFs;
                                }
                            }
                            onlineBCFWindow.Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid BCF project Id.", "Invalid BCF Project Id", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add an online BCF./n"+ex.Message, "Add Online BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridBCFs.SelectedItems.Count > 0)
                {
                    int index = dataGridBCFs.SelectedIndex;
                    dataGridBCFs.ItemsSource = null;
                    linkedBCFs.RemoveAt(index);
                    dataGridBCFs.ItemsSource = linkedBCFs;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove linked BCF./n"+ex.Message, "Remove Linked BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bcfFileDictionary = new Dictionary<string, LinkedBcfFileInfo>();
                foreach (LinkedBcfFileInfo fileInfo in linkedBCFs)
                {
                    if (!bcfFileDictionary.ContainsKey(fileInfo.BCFFileId))
                    {
                        bcfFileDictionary.Add(fileInfo.BCFFileId, fileInfo);
                    }
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the information of linked BCFs.\n"+ex.Message, "Save Linked BCFs", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }

    public class LinkedBCF
    {
        private string bcfTitle = "";
        private string sharedLink = "";
        private FileHolders fileHolders = null;

        public string BCFTitle { get { return bcfTitle; } set { bcfTitle = value; } }
        public string SharedLink { get { return sharedLink; } set { sharedLink = value; } }
        public FileHolders FilesInfo { get { return fileHolders; } set { fileHolders = value; } }
        
        public LinkedBCF()
        {

        }
    }

    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public void Sort()
        {
            Sort(Comparer<T>.Default);
        }

        public void Sort(IComparer<T> comparer)
        {
            int i, j;
            T index;

            for (i = 1; i < Count; i++)
            {
                index = this[i];
                j = i;

                while ((j > 0) && (comparer.Compare(this[j - 1], index) == 1))
                {
                    this[j] = this[j - 1];
                    j = j - 1;
                }

                this[j] = index;
            }
        }
    }
}
