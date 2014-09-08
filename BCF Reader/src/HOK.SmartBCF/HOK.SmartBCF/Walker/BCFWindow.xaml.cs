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
        private SortableObservableCollection<LinkedBcfFileInfo> linkedBCFs = new SortableObservableCollection<LinkedBcfFileInfo>();

        public Dictionary<string, LinkedBcfFileInfo> BCFFileDictionary { get { return bcfFileDictionary; } set { bcfFileDictionary = value; } }

        public BCFWindow(Dictionary<string, LinkedBcfFileInfo> fileHistory)
        {
            bcfFileDictionary = fileHistory;
            foreach (LinkedBcfFileInfo info in bcfFileDictionary.Values)
            {
                linkedBCFs.Add(info);
            }
            linkedBCFs.Sort();

            InitializeComponent();
            dataGridBCFs.DataContext = linkedBCFs;
        }

        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportBCFWindow importWindow = new ImportBCFWindow();
                if (importWindow.ShowDialog() == true)
                {
                    LinkedBcfFileInfo fileInfo = importWindow.BCFFileInfo;
                    linkedBCFs.Add(fileInfo);
                    linkedBCFs.Sort();
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
                    foreach (object selItem in dataGridBCFs.SelectedItems)
                    {
                        LinkedBcfFileInfo fileInfo = (LinkedBcfFileInfo)selItem;
                        linkedBCFs.Remove(fileInfo);
                    }
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
                if (linkedBCFs.Count > 0)
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
