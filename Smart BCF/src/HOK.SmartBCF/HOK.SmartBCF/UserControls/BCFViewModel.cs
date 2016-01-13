using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.Utils;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using HOK.SmartBCF.Windows;
using System.IO;
using HOK.SmartBCF.BCFDBWriter;

namespace HOK.SmartBCF.UserControls
{
    public class BCFViewModel : INotifyPropertyChanged
    {
        private bool addInMode = false;
        private string databaseFile = "";
        private ObservableCollection<BCFZIP> bcfFiles = new ObservableCollection<BCFZIP>();
        private int primaryFileIndex = 0;
        private int selectedIndex = 0;

        private bool databaseOpened = false;
        private bool componentEnabled = false;
        private string statusText = "";

        //Commands
        private RelayCommand createDBCommand;
        private RelayCommand openDBCommand;
        private RelayCommand addBCFCommand;
        private RelayCommand convertBCFCommand;
        private RelayCommand settingCommand;

        public bool AddInMode { get { return addInMode; } set { addInMode = value; NotifyPropertyChanged("AddInMode"); } }
        public string DatabaseFile { get { return databaseFile; } set { databaseFile = value; NotifyPropertyChanged("DatabaseFile"); } }
        public ObservableCollection<BCFZIP> BCFFiles { get { return bcfFiles; } set { bcfFiles = value; NotifyPropertyChanged("BCFFiles"); } }
        public int PrimaryFileIndex { get { return primaryFileIndex; } set { primaryFileIndex = value; NotifyPropertyChanged("PrimaryFileIndex"); } }
        public int SelectedIndex { get { return selectedIndex; } set { selectedIndex = value; NotifyPropertyChanged("SelectedIndex"); } }

        public bool DatabaseOpened { get { return databaseOpened; } set { databaseOpened = value; NotifyPropertyChanged("DatabaseOpened"); } }
        public bool ComponentEnabled { get { return componentEnabled; } set { componentEnabled = value; NotifyPropertyChanged("ComponentEnabled"); } }
        public string StatusText { get { return statusText; } set { statusText = value; NotifyPropertyChanged("StatusText"); } }

        public ICommand CreateDBCommand { get { return createDBCommand; }}
        public ICommand OpenDBCommand { get { return openDBCommand; } }
        public ICommand AddBCFCommand { get { return addBCFCommand; } }
        public ICommand ConvertBCFCommand { get { return convertBCFCommand; } }
        public ICommand SettingCommand { get { return settingCommand; } }

        public BCFViewModel(bool addIn)
        {
            addInMode = addIn;

            createDBCommand = new RelayCommand(param => this.CreateDBExecuted(param));
            openDBCommand = new RelayCommand(param => this.OpenDBExecuted(param));
            addBCFCommand = new RelayCommand(param => this.AddBCFExecuted(param));
            convertBCFCommand = new RelayCommand(param => this.ConvertBCFExecuted(param));
            settingCommand = new RelayCommand(param => this.SettingExecuted(param));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void CreateDBExecuted(object param)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Select a BCFZip File to Create a Database File";
                openDialog.DefaultExt = ".bcfzip";
                openDialog.Filter = "BCF (.bcfzip)|*.bcfzip";
                if ((bool)openDialog.ShowDialog())
                {
                    string bcfPath = openDialog.FileName;
                    SaveDatabase(bcfPath);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void OpenDBExecuted(object param)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Select a database to be connected";
                openDialog.DefaultExt = ".sqlite";
                openDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)openDialog.ShowDialog())
                {
                    bool openedDB = OpenDatabase(openDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void AddBCFExecuted(object param)
        {
            try
            {
                if (string.IsNullOrEmpty(databaseFile))
                {
                    MessageBox.Show("Please connect to a database file before adding BCF files.", "Empty Database", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Select a BCFZip File to Create a Database File";
                openDialog.DefaultExt = ".bcfzip";
                openDialog.Filter = "BCF (.bcfzip)|*.bcfzip";
                if ((bool)openDialog.ShowDialog())
                {
                    string bcfPath = openDialog.FileName;
                    bool added = AddBCF(bcfPath);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void ConvertBCFExecuted(object param)
        {
            try
            {
                if (bcfFiles.Count > 0)
                {

                    //select primary file info
                    BCFZIP combinedBCF = CombineBCF(bcfFiles, 0);
                    if (null != combinedBCF)
                    {

                        SaveFileDialog saveDialog = new SaveFileDialog();
                        saveDialog.Title = "Save BCF";
                        saveDialog.DefaultExt = ".bcfzip";
                        saveDialog.Filter = "BCF (.bcfzip)|*.bcfzip";
                        saveDialog.OverwritePrompt = true;
                        if ((bool)saveDialog.ShowDialog())
                        {
                            string bcfPath = saveDialog.FileName;
                            bool saved = BCFWriter.BCFWriter.Write(bcfPath, combinedBCF);
                            if (saved)
                            {
                                MessageBox.Show(bcfPath + "\n has been successfully saved!!", "BCF Saved", MessageBoxButton.OK, MessageBoxImage.Information);
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

        public void SettingExecuted(object param)
        {
            try
            {
                if (bcfFiles.Count > 0)
                {
                    SettingsWindow settingWindow = new SettingsWindow(this);
                    if (settingWindow.ShowDialog() == true)
                    {
                        if (settingWindow.ColorChanged)
                        {
                            Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void Refresh()
        {
            try
            {
                int selectedBCF = selectedIndex;
                int selectedMarkup = bcfFiles[selectedIndex].SelectedMarkup;
                bcfFiles.Clear();

                Dictionary<string, BCFZIP> dictionary = BCFDBReader.BCFDBReader.ReadDatabase(databaseFile);
                foreach (BCFZIP bcf in dictionary.Values)
                {
                    bcfFiles.Add(bcf);
                }
                this.SelectedIndex = selectedBCF;
                this.BCFFiles[selectedBCF].SelectedMarkup = selectedMarkup;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public bool OpenDatabase(string dbPath)
        {
            bool openDB = false;
            try
            {
                this.DatabaseFile = dbPath;
                this.BCFFiles.Clear();

                Dictionary<string, BCFZIP> bcfDictionary = BCFDBReader.BCFDBReader.ReadDatabase(databaseFile);
                if (bcfDictionary.Count > 0)
                {
                    bool connected = BCFDBWriter.BCFDBWriter.ConnectDatabase(databaseFile);
                    foreach (BCFZIP bcf in bcfDictionary.Values)
                    {
                        this.BCFFiles.Add(bcf);
                    }
                    this.SelectedIndex = 0;
                }

                ProgressManager.progressBar.Visibility = System.Windows.Visibility.Hidden;

                this.StatusText = dbPath;
                this.DatabaseOpened = true;
                if (addInMode) { this.ComponentEnabled = true; }
                openDB = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open database.\n" + ex.Message, "Open Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return openDB;
        }

        public bool SaveDatabase(string bcfPath)
        {
            bool savedDB = false;
            try
            {
                this.BCFFiles.Clear();

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "Specify a Database File Location";
                saveDialog.DefaultExt = ".sqlite";
                saveDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                saveDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(bcfPath);
                saveDialog.OverwritePrompt = false;
                if ((bool)saveDialog.ShowDialog())
                {
                    this.DatabaseFile = saveDialog.FileName;
                    if (File.Exists(databaseFile))
                    {
                        MessageBoxResult msgResult = MessageBox.Show("\"" + databaseFile + "\" already exists.\nDo you want to replace it?",
                            "File Exists", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgResult == MessageBoxResult.Yes)
                        {
                            File.Delete(databaseFile);
                        }
                        else if (msgResult == MessageBoxResult.No)
                        {
                            return false;
                        }
                    }

                    BCFZIP bcfzip = BCFReader.BCFReader.Read(bcfPath);
                    bcfzip.IsPrimary = true;
                    if (bcfzip.Markups.Count > 0)
                    {
                        bool connected = BCFDBWriter.BCFDBWriter.ConnectDatabase(databaseFile);
                        if (connected)
                        {
                            bool created = BCFDBWriter.BCFDBWriter.CreateTables();
                            bool written = BCFDBWriter.BCFDBWriter.WriteDatabase(bcfzip, ConflictMode.IGNORE);

                            if (created && written)
                            {
                                //MessageBox.Show("The database file has been successfully created!!\n" + dbFile, "Database Created", MessageBoxButton.OK, MessageBoxImage.Information);
                                this.BCFFiles.Add(bcfzip);
                                this.DatabaseOpened = true;
                                if (addInMode) { this.ComponentEnabled = true; }
                                this.SelectedIndex = bcfFiles.Count - 1;
                            }
                            else
                            {
                                MessageBox.Show("The datbase file has not been successfully created.\nPlease check the log file.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Information);
                                savedDB = false;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("An invalid BCFZip file has been selected.\n Please select another BCFZip file to create a database file.", "Invalid BCFZip", MessageBoxButton.OK, MessageBoxImage.Information);
                        savedDB = false;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save BCF data into the database.\n" + ex.Message, "Import BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return savedDB;
        }

        public bool AddBCF(string bcfPath)
        {
            bool added = false;
            try
            {
                TaskDialogOption selectedOption = TaskDialogOption.NONE;
                var bcfExisting = from bcf in bcfFiles where bcf.ZipFilePath == bcfPath select bcf;
                if (bcfExisting.Count() > 0)
                {
                    TaskDialogWindow dialogWindow = new TaskDialogWindow("\"" + bcfPath + "\" already exists in the database.\nDo you want to replace it?");
                    if ((bool)dialogWindow.ShowDialog())
                    {
                        selectedOption = dialogWindow.SelectedOption;
                        switch (selectedOption)
                        {
                            case TaskDialogOption.REPLACE:
                                BCFZIP existingBCF = bcfExisting.First();
                                bool deleted = BCFDBWriter.BCFDBWriter.DeleteBCF(existingBCF);
                                if (deleted)
                                {
                                    this.BCFFiles.Remove(existingBCF);
                                }
                                break;
                            case TaskDialogOption.MERGE:
                                break;
                            case TaskDialogOption.IGNORE:
                                return false;
                            case TaskDialogOption.NONE:
                                return false;
                        }
                    }
                }

                BCFZIP bcfzip = BCFReader.BCFReader.Read(bcfPath);
                if (bcfzip.Markups.Count > 0)
                {
                    ConflictMode mode = ConflictMode.IGNORE;
                    if (selectedOption == TaskDialogOption.MERGE)
                    {
                        int index = bcfFiles.IndexOf(bcfExisting.First());
                        if (index > -1)
                        {
                            BCFZIP mergedBCF = BCFDBWriter.BCFDBWriter.MergeDatabase(bcfzip, bcfFiles[index]);
                            if (null != mergedBCF)
                            {
                                this.BCFFiles[index] = mergedBCF;
                            }
                        }
                    }
                    else
                    {
                        bool written = BCFDBWriter.BCFDBWriter.WriteDatabase(bcfzip, mode);
                        if (written)
                        {
                            bcfzip.IsPrimary = false;
                            this.BCFFiles.Add(bcfzip);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add BCF into the connected database.\n" + ex.Message, "Add BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return added;
        }

        private BCFZIP CombineBCF(ObservableCollection<BCFZIP> bcfFiles, int primaryIndex)
        {
            BCFZIP combinedBCF = null;
            try
            {
                //Set Primary BCF 
                BCFZIP primaryBCF = bcfFiles[primaryIndex];
                for (int i = 0; i < bcfFiles.Count; i++)
                {
                    if (i == primaryIndex) { continue; }
                    BCFZIP bcf = bcfFiles[i];
                    foreach (Markup markup in bcf.Markups)
                    {
                        primaryBCF.Markups.Add(markup);
                    }
                }
                primaryBCF.Markups = new ObservableCollection<Markup>(primaryBCF.Markups.OrderBy(o => o.Topic.Index));
                for (int i = 0; i < primaryBCF.Markups.Count; i++)
                {
                    primaryBCF.Markups[i].Topic.Index = i;
                }
                combinedBCF = primaryBCF;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to combine multiple bcf files.\n" + ex.Message, "Combine BCF Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return combinedBCF;
        }

    }
}
