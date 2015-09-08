using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using BCFDBManager.BCFUtils;
using BCFDBManager.DatabaseUtils;
using Microsoft.Win32;

namespace BCFDBManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string dbFile = "";
        private Dictionary<string/*fileId*/, BCFZIP> bcfDictionary = new Dictionary<string, BCFZIP>();
        private BCFZIP selectedFile = null;
        private Markup selectedMarkup = null;
        private Comment selectedComment = null;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "BCF Database Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void DisplayUI()
        {
            try
            {
                List<BCFZIP> bcfZips = bcfDictionary.Values.OrderBy(o => o.ZipFileName).ToList();
                comboBoxFile.ItemsSource = null;
                comboBoxFile.ItemsSource = bcfZips;
                comboBoxFile.DisplayMemberPath = "ZipFileName";
                comboBoxFile.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Display BCF Information.\n" + ex.Message, "Display UI", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxFile.SelectedItem)
                {
                    selectedFile = (BCFZIP)comboBoxFile.SelectedItem;
                    if (null != selectedFile)
                    {
                        Dictionary<string, BCFComponent> components = selectedFile.BCFComponents;
                        if (components.Count > 0)
                        {
                            var topics = from component in components.Values select component.MarkupInfo.Topic;
                            if (topics.Count() > 0)
                            {
                                List<Topic> topicList = topics.OrderBy(o => o.Index).ToList();
                                comboBoxIssue.ItemsSource = null;
                                comboBoxIssue.ItemsSource = topicList;
                                comboBoxIssue.DisplayMemberPath = "Title";
                                comboBoxIssue.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a BCF File Item.\n"+ex.Message, "BCF File Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxIssue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != comboBoxIssue.SelectedItem && null!= selectedFile)
                {
                    Topic selectedTopic = (Topic)comboBoxIssue.SelectedItem;
                    string guid = selectedTopic.Guid;

                    List<Comment> comments = new List<Comment>();
                    if (selectedFile.BCFComponents.ContainsKey(guid))
                    {
                        BCFComponent bcfComponent = selectedFile.BCFComponents[guid];
                        if (null != bcfComponent.MarkupInfo)
                        {
                            selectedMarkup = bcfComponent.MarkupInfo;
                            if (null != selectedMarkup)
                            {
                                comments = selectedMarkup.Comment;
                                comments = comments.OrderBy(o => o.Comment1).ToList();
                            }
                        }
                    }

                    dataGridComments.ItemsSource = null;
                    dataGridComments.ItemsSource = comments;
                    dataGridComments.SelectedIndex = 0;
                    //RefershSnapshotImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select an issue item.\n"+ex.Message, "Issue Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool RefershSnapshotImage()
        {
            bool refreshed = false;
            try
            {
                if (null != dataGridComments.SelectedItem && null != selectedFile && null != selectedMarkup)
                {
                    selectedComment = (Comment)dataGridComments.SelectedItem;
                    string viewPointId = selectedComment.Viewpoint.Guid;
                    ViewPoint selectedViewpoint = null;
                    if (!string.IsNullOrEmpty(viewPointId))
                    {
                        var viewpoints = from vp in selectedMarkup.Viewpoints where vp.Guid == viewPointId select vp;
                        if (viewpoints.Count() > 0)
                        {
                            selectedViewpoint = viewpoints.First();
                        }
                    }
                    else if (selectedMarkup.Viewpoints.Count > 0)
                    {
                        selectedViewpoint = selectedMarkup.Viewpoints.First();
                    }

                    if (null != selectedViewpoint)
                    {
                        if (null != selectedViewpoint.SnapshotImage)
                        {
                            using (MemoryStream stream = new MemoryStream(selectedViewpoint.SnapshotImage))
                            {
                                imageSnapshot.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                                refreshed = true;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh the snapshot image.\n"+ex.Message, "Refresh Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return refreshed;
        }

        private void dataGridComments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                bool refreshed = RefershSnapshotImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a comment item.\n" + ex.Message, "Comment Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxFile.SelectedItem && null != comboBoxIssue.SelectedItem)
                {
                    Comment comment = new Comment();
                    comment.Guid = Guid.NewGuid().ToString();
                    CommentViewpoint viewPoint = new CommentViewpoint();
                    viewPoint.Guid = selectedMarkup.Viewpoints.First().Guid;
                    comment.Viewpoint = viewPoint;

                    CommentWindow commentWindow = new CommentWindow(selectedMarkup, comment, CommentMode.ADD);
                    if (commentWindow.ShowDialog() == true)
                    {
                        selectedMarkup = commentWindow.SelectedMarkup;
                        selectedComment = commentWindow.SelectedComment;
                       
                        bool added = CommentManager.AddComment(selectedMarkup, selectedComment, dbFile);

                        string commentGuid = selectedComment.Guid;
                        int fileIndex = comboBoxFile.SelectedIndex;
                        int issueIndex = comboBoxIssue.SelectedIndex;

                        string issueId = selectedMarkup.Topic.Guid;
                        if (selectedFile.BCFComponents.ContainsKey(issueId))
                        {
                            BCFComponent bcfComponent = selectedFile.BCFComponents[issueId];
                            bcfComponent.MarkupInfo = selectedMarkup;

                            selectedFile.BCFComponents.Remove(issueId);
                            selectedFile.BCFComponents.Add(issueId, bcfComponent);

                            bcfDictionary.Remove(selectedFile.FileId);
                            bcfDictionary.Add(selectedFile.FileId, selectedFile);

                            DisplayUI();
                            comboBoxFile.SelectedIndex = fileIndex;
                            comboBoxIssue.SelectedIndex = issueIndex;

                            List<Comment> comments = (List<Comment>)dataGridComments.ItemsSource;
                            int commentIndex = comments.FindIndex(o => o.Guid == commentGuid);
                            dataGridComments.SelectedIndex = commentIndex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add an comment item.\n"+ex.Message, "Add Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonEditComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxFile.SelectedItem && null != comboBoxIssue.SelectedItem && null != dataGridComments.SelectedItem)
                {
                    CommentWindow commentWindow = new CommentWindow(selectedMarkup, selectedComment, CommentMode.EDIT);
                    if (commentWindow.ShowDialog() == true)
                    {
                        selectedMarkup = commentWindow.SelectedMarkup;
                        selectedComment = commentWindow.SelectedComment;
                        bool updated = CommentManager.EditComment(selectedComment, dbFile);

                        string commentGuid = selectedComment.Guid;
                        int fileIndex = comboBoxFile.SelectedIndex;
                        int issueIndex = comboBoxIssue.SelectedIndex;

                        string issueId = selectedMarkup.Topic.Guid;
                        if (selectedFile.BCFComponents.ContainsKey(issueId))
                        {
                            BCFComponent bcfComponent = selectedFile.BCFComponents[issueId];
                            bcfComponent.MarkupInfo = selectedMarkup;

                            selectedFile.BCFComponents.Remove(issueId);
                            selectedFile.BCFComponents.Add(issueId, bcfComponent);

                            bcfDictionary.Remove(selectedFile.FileId);
                            bcfDictionary.Add(selectedFile.FileId, selectedFile);

                            DisplayUI();
                            comboBoxFile.SelectedIndex = fileIndex;
                            comboBoxIssue.SelectedIndex = issueIndex;

                            List<Comment> comments = (List<Comment>)dataGridComments.ItemsSource;
                            int commentIndex = comments.FindIndex(o => o.Guid == commentGuid);
                            dataGridComments.SelectedIndex = commentIndex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to edit a comment item.\n" + ex.Message, "Edit Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDeleteComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxFile.SelectedItem && null != comboBoxIssue.SelectedItem && null != dataGridComments.SelectedItem)
                {
                    bool deleted = CommentManager.DeleteComment(selectedComment, dbFile);
                    int commentIndex = selectedMarkup.Comment.FindIndex(o => o.Guid == selectedComment.Guid);
                    if (commentIndex > -1)
                    {
                        selectedMarkup.Comment.RemoveAt(commentIndex);
                    }

                    int fileIndex = comboBoxFile.SelectedIndex;
                    int issueIndex = comboBoxIssue.SelectedIndex;

                    string issueId = selectedMarkup.Topic.Guid;
                    if (selectedFile.BCFComponents.ContainsKey(issueId))
                    {
                        BCFComponent bcfComponent = selectedFile.BCFComponents[issueId];
                        bcfComponent.MarkupInfo = selectedMarkup;

                        selectedFile.BCFComponents.Remove(issueId);
                        selectedFile.BCFComponents.Add(issueId, bcfComponent);

                        bcfDictionary.Remove(selectedFile.FileId);
                        bcfDictionary.Add(selectedFile.FileId, selectedFile);

                        DisplayUI();
                        comboBoxFile.SelectedIndex = fileIndex;
                        comboBoxIssue.SelectedIndex = issueIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete an comment item.\n" + ex.Message, "Delete Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonNewDB_Click(object sender, RoutedEventArgs e)
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
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Title = "Specify a Database File Location";
                    saveDialog.DefaultExt = ".sqlite";
                    saveDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                    saveDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(bcfPath);
                    if ((bool)saveDialog.ShowDialog())
                    {
                        dbFile = saveDialog.FileName;

                        progressBar.Visibility = System.Windows.Visibility.Visible;

                        BCFFileManager.progressBar = progressBar;
                        BCFFileManager.statusLabel = statusLable;

                        BCFZIP bcfzip = BCFFileManager.ReadBCF(bcfPath);
                        bcfzip = BCFFileManager.MapBinaryData(bcfzip);

                        if (bcfzip.BCFComponents.Count > 0)
                        {
                            DBManager.progressBar = progressBar;
                            DBManager.statusLabel = statusLable;

                            bool created = DBManager.CreateTables(dbFile, bcfzip);
                            bool written = DBManager.WriteDatabase(dbFile, bcfzip, ConflictMode.IGNORE);

                            if (created && written)
                            {
                                textBoxDB.Text = dbFile;

                                MessageBox.Show("The database file has been successfully created!!\n" + dbFile, "Database Created", MessageBoxButton.OK, MessageBoxImage.Information);
                                bcfDictionary = new Dictionary<string, BCFZIP>();
                                bcfDictionary.Add(bcfzip.FileId, bcfzip);
                                DisplayUI();
                            }
                            else
                            {
                                MessageBox.Show("The datbase file has not been successfully created.\nPlease check the log file.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("An invalid BCFZip file has been selected.\n Please select another BCFZip file to create a database file.", "Invalid BCFZip", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        progressBar.Visibility = System.Windows.Visibility.Hidden;
                        statusLable.Text = "Ready";
                        buttonAddBCF.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import BCF.\n" + ex.Message, "Import BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonConnectDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Select a database to be connected";
                openDialog.DefaultExt = ".sqlite";
                openDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)openDialog.ShowDialog())
                {
                    progressBar.Visibility = System.Windows.Visibility.Visible;

                    dbFile = openDialog.FileName;
                    textBoxDB.Text = dbFile;

                    DBManager.progressBar = progressBar;
                    DBManager.statusLabel = statusLable;
                    bcfDictionary = DBManager.ReadDatabase(dbFile, false);
                    DisplayUI();

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                    buttonAddBCF.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect Database.\n" + ex.Message, "Connect Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddBCF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(dbFile))
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

                    progressBar.Visibility = System.Windows.Visibility.Visible;

                    BCFFileManager.progressBar = progressBar;
                    BCFFileManager.statusLabel = statusLable;

                    BCFZIP bcfzip = BCFFileManager.ReadBCF(bcfPath);
                    bcfzip = BCFFileManager.MapBinaryData(bcfzip);

                    if (bcfzip.BCFComponents.Count > 0)
                    {
                        ConflictMode mode = ConflictMode.IGNORE;
                        if (CheckDuplicateTopics(bcfzip))
                        {
                            MessageBoxResult mr = MessageBox.Show("Duplicate topics have been found.\nWould you like to replace the topic item?\n\nYes - Replace\tNo - Ignore", "Duplicates Found", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                            if (mr == MessageBoxResult.Yes)
                            {
                                mode = ConflictMode.REPLACE;
                            }
                            else if (mr == MessageBoxResult.Cancel)
                            {
                                progressBar.Visibility = System.Windows.Visibility.Hidden;
                                statusLable.Text = "Ready";
                                return;
                            }
                        }

                        DBManager.progressBar = progressBar;
                        DBManager.statusLabel = statusLable;

                        bool written = DBManager.WriteDatabase(dbFile, bcfzip, mode);

                        if (written)
                        {
                            textBoxDB.Text = dbFile;

                            MessageBox.Show("The information of BCF has been successfully added to the database!!\n" + dbFile, "BCF Added", MessageBoxButton.OK, MessageBoxImage.Information);
                            if (!bcfDictionary.ContainsKey(bcfzip.FileId))
                            {
                                bcfDictionary.Add(bcfzip.FileId, bcfzip);
                                DisplayUI();
                            }
                        }
                        else
                        {
                            MessageBox.Show("The datbase file has not been successfully created.\nPlease check the log file.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add a BCF file into the current database.\n"+ex.Message, "Add BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool CheckDuplicateTopics(BCFZIP bcfZip)
        {
            bool duplicate = false;
            try
            {
                List<BCFZIP> zipList = bcfDictionary.Values.ToList();
                foreach (string topicId in bcfZip.BCFComponents.Keys)
                {
                    var topicFound = from zip in zipList where zip.BCFComponents.ContainsKey(topicId) select zip;
                    if (topicFound.Count() > 0)
                    {
                        duplicate = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check duplicates of topics.\n"+ex.Message, "Check Duplicates", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return duplicate;
        }


        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        

    }
}
