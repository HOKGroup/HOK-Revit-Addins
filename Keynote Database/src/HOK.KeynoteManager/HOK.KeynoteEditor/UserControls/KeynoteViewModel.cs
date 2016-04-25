using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Keynote.ClassModels;
using HOK.Keynote.REST;
using HOK.KeynoteEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HOK.KeynoteEditor.UserControls
{
    public class KeynoteViewModel :INotifyPropertyChanged
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private KeynoteConfiguration config = new KeynoteConfiguration();
        private KeynoteProjectInfo projectInfo = new KeynoteProjectInfo();
        private KeynoteSetInfo setInfo = new KeynoteSetInfo();
        private List<KeynoteInfo> keynoteList = new List<KeynoteInfo>();
        private ObservableCollection<TreeViewModel> keynoteTreeView = new ObservableCollection<TreeViewModel>();
        private string statusText = "Ready";

        private RelayCommand importCommand;
        private RelayCommand addItemCommand;
        private RelayCommand addChildItemCommand;
        private RelayCommand deleteItemCommand;
        private RelayCommand applyCommand;

        public KeynoteConfiguration Config { get { return config; } set { config = value; NotifyPropertyChanged("Config"); } }
        public KeynoteProjectInfo ProjectInfo { get { return projectInfo; } set { projectInfo = value; NotifyPropertyChanged("ProjectInfo"); } }
        public KeynoteSetInfo SetInfo { get { return setInfo; } set { setInfo = value; NotifyPropertyChanged("SetInfo"); } }
        public List<KeynoteInfo> KeynoteList { get { return keynoteList; } set { keynoteList = value; NotifyPropertyChanged("KeynoteList"); } }
        public ObservableCollection<TreeViewModel> KeynoteTreeView { get { return keynoteTreeView; } set { keynoteTreeView = value; NotifyPropertyChanged("KeynoteTreeView"); } }
        
        public string StatusText { get { return statusText; } set { statusText = value; NotifyPropertyChanged("StatusText"); } }
        public static bool UserChanged = false;
        public static Dictionary<string, KeynoteInfo> KeynoteToAdd = new Dictionary<string, KeynoteInfo>();
        public static Dictionary<string, KeynoteInfo> KeynoteToUpdate = new Dictionary<string, KeynoteInfo>();
        public static List<string> KeynoteToDelete = new List<string>();
        

        public ICommand ImportCommand { get { return importCommand; } }
        public ICommand AddItemCommand { get { return addItemCommand; } }
        public ICommand AddChildItemCommand { get { return addChildItemCommand; } }
        public ICommand DeleteItemCommand { get { return deleteItemCommand; } }
        public ICommand ApplyCommand { get { return applyCommand; } }

        public KeynoteViewModel(UIApplication uiapp, KeynoteConfiguration configuration)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            config = configuration;

            RegisterCommand();
            GetKeynoteInfo();

            KeynoteToAdd.Clear();
            KeynoteToUpdate.Clear();
            KeynoteToDelete.Clear();
        }

        private void RegisterCommand()
        {
            try
            {
                importCommand = new RelayCommand(param => this.ImportExecuted(param));
                addItemCommand = new RelayCommand(param => this.AddItemExecuted(param));
                addChildItemCommand = new RelayCommand(param => this.AddChildItemExecuted(param));
                deleteItemCommand = new RelayCommand(param => this.DeleteItemExecuted(param));
                applyCommand = new RelayCommand(param => this.ApplyExecuted(param));
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void GetKeynoteInfo()
        {
            try
            {
                List<KeynoteProjectInfo> projectFound = ServerUtil.GetProjects("projects/" + config.ProjectId);
                if (projectFound.Count > 0)
                {
                    projectInfo = projectFound.First();
                }

                List<KeynoteSetInfo> setFound = ServerUtil.GetKeynoteSets("keynotesets/" + config.KeynoteSetId);
                if (setFound.Count > 0)
                {
                    setInfo = setFound.First();
                }

                keynoteList = ServerUtil.GetKeynotes("keynotes/setid/" + config.KeynoteSetId);
                if (keynoteList.Count > 0)
                {
                    keynoteTreeView = TreeViewModelUtil.SetTree(keynoteList);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get keynote information.\n" + ex.Message, "Get Keynote Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void ImportExecuted(object param)
        {
            try
            {
                ProjectPanel projectPanel = new ProjectPanel();
                if ((bool)projectPanel.ShowDialog())
                {

                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void AddItemExecuted(object param)
        {
            try
            {
                UserChanged = true;
                this.StatusText = "In Progress..";

                string keyText = "00";
                if (keynoteTreeView.Count > 0)
                {
                    TreeViewModel lastItem = keynoteTreeView[keynoteTreeView.Count - 1];
                    string lastItemKeyText = lastItem.KeynoteItem.key;
                    keyText = IncreaseKeyText(lastItemKeyText);
                }

                KeynoteInfo keynote = new KeynoteInfo(Guid.NewGuid().ToString(), keyText, "", "New Keynote", setInfo._id);
                TreeViewModel tvm = new TreeViewModel(keynote);
                this.KeynoteTreeView.Add(tvm);

                if (!KeynoteToAdd.ContainsKey(keynote._id))
                {
                    KeynoteToAdd.Add(keynote._id, keynote);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private string IncreaseKeyText(string keyText)
        {
            string nextKey = "";
            try
            {
                if (!string.IsNullOrEmpty(keyText))
                {
                    string[] splited = keyText.Split('.');
                    if (splited.Length > 0)
                    {
                        string suffixStr = new string(splited[splited.Length-1].ToString().Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());
                        if (!string.IsNullOrEmpty(suffixStr))
                        {
                            int keyNumber = 0;
                            if (int.TryParse(suffixStr, out keyNumber))
                            {
                                nextKey = keyText.Remove(keyText.Length - suffixStr.Length);
                                nextKey = nextKey + (keyNumber + 1).ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return nextKey;
        }

        public void AddChildItemExecuted(object param)
        {
            try
            {
                UserChanged = true;
                this.StatusText = "In Progress..";
 
                TreeViewModel treeNodeToAdd = null;

                for (int i = 0; i < keynoteTreeView.Count; i++)
                {
                    TreeViewModel selectedTree = keynoteTreeView[i];
                    if (selectedTree.IsSelected)
                    {
                        TreeViewModel childNode = GetChildToAdd(selectedTree);
                        if (null != childNode)
                        {
                            treeNodeToAdd = childNode;
                            this.KeynoteTreeView[i].ChildrenItems.Add(childNode);
                        }
                        break;
                    }
                    else if (selectedTree.ChildrenItems.Count > 0)
                    {
                        for (int j = 0; j < selectedTree.ChildrenItems.Count; j++)
                        {
                            TreeViewModel childTree = selectedTree.ChildrenItems[j];
                            if (childTree.IsSelected)
                            {
                                TreeViewModel grandChildNode = GetChildToAdd(childTree);
                                if (null != grandChildNode)
                                {
                                    treeNodeToAdd = grandChildNode;
                                    this.KeynoteTreeView[i].ChildrenItems[j].ChildrenItems.Add(grandChildNode);
                                }
                                break;
                            }
                        }
                    }
                }

                if (null != treeNodeToAdd)
                {
                    KeynoteInfo keynote = treeNodeToAdd.KeynoteItem;
                    if (!KeynoteToAdd.ContainsKey(keynote._id))
                    {
                        KeynoteToAdd.Add(keynote._id, keynote);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private TreeViewModel GetChildToAdd(TreeViewModel selectedTree)
        {
            TreeViewModel childNode = null;
            try
            {
                KeynoteInfo selectedKeynote = selectedTree.KeynoteItem;

                string keyText = selectedKeynote.key + ".00";
                if (selectedTree.ChildrenItems.Count > 0)
                {
                    TreeViewModel lastChild = selectedTree.ChildrenItems[selectedTree.ChildrenItems.Count - 1];
                    string lastItemKeyText = lastChild.KeynoteItem.key;
                    keyText = IncreaseKeyText(lastItemKeyText);
                }

                KeynoteInfo keynote = new KeynoteInfo(Guid.NewGuid().ToString(), keyText, selectedKeynote.key, "New Keynote", setInfo._id);
                childNode = new TreeViewModel(keynote);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return childNode;
        }

        public void DeleteItemExecuted(object param)
        {
            try
            {
                UserChanged = true;
                this.StatusText = "In Progress..";
                
                for (int i = 0; i < keynoteTreeView.Count; i++)
                {
                    TreeViewModel selectedTree = keynoteTreeView[i];
                    if (selectedTree.IsSelected)
                    {
                        FindChildKeynotes(selectedTree);
                        this.KeynoteTreeView.Remove(selectedTree);
                        break;
                    }
                    else if (selectedTree.ChildrenItems.Count > 0)
                    {
                        for (int j = 0; j < selectedTree.ChildrenItems.Count; j++)
                        {
                            TreeViewModel childTree = selectedTree.ChildrenItems[j];
                            if (childTree.IsSelected)
                            {
                                FindChildKeynotes(childTree);
                                this.KeynoteTreeView[i].ChildrenItems.Remove(childTree);
                                break;
                            }
                            else if (childTree.ChildrenItems.Count > 0)
                            {
                                for (int k = 0; k < childTree.ChildrenItems.Count; k++)
                                {
                                    TreeViewModel grandChildTree = selectedTree.ChildrenItems[j].ChildrenItems[k];
                                    if (grandChildTree.IsSelected)
                                    {
                                        FindChildKeynotes(grandChildTree);
                                        this.KeynoteTreeView[i].ChildrenItems[j].ChildrenItems.Remove(grandChildTree);
                                        break;
                                    }
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

        private void FindChildKeynotes(TreeViewModel treeView)
        {
            try
            {
                KeynoteInfo keynote = treeView.KeynoteItem;
                if (!KeynoteToDelete.Contains(keynote._id))
                {
                    KeynoteToDelete.Add(keynote._id);
                }
                for (int i = 0; i < treeView.ChildrenItems.Count; i++)
                {
                    TreeViewModel childTree = treeView.ChildrenItems[i];
                    FindChildKeynotes(childTree);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void ApplyExecuted(object param)
        {
            try
            {
                string content = "";
                string errMsg = "";

                HttpStatusCode deleted = HttpStatusCode.Unused;
                HttpStatusCode updated = HttpStatusCode.Unused;
                HttpStatusCode added = HttpStatusCode.Unused;
                //delete 
                var existingKeynoteIds = from keynote in keynoteList where KeynoteToDelete.Contains(keynote._id) select keynote._id;
                if(existingKeynoteIds.Count()> 0)
                {
                    foreach (string keynoteId in existingKeynoteIds)
                    {
                        deleted = ServerUtil.DeleteKeynote(out content, out errMsg, keynoteId);
                    }
                }    
                else
                {
                    deleted = HttpStatusCode.OK;
                }

                var keynoteToUpdateIds = from keynoteId in KeynoteToUpdate.Keys where !KeynoteToDelete.Contains(keynoteId) select keynoteId;
                if (keynoteToUpdateIds.Count() > 0)
                {
                    foreach (string keynoteId in keynoteToUpdateIds)
                    {
                        updated = ServerUtil.UpdateKeynote(out content, out errMsg, KeynoteToUpdate[keynoteId]);
                    }
                }
                else
                {
                    updated = HttpStatusCode.Accepted;
                }

                var pureAddedList = from keynote in KeynoteToAdd.Values where !KeynoteToDelete.Contains(keynote._id) && !KeynoteToUpdate.ContainsKey(keynote._id) select keynote;
                if (pureAddedList.Count() > 0)
                {
                    added = ServerUtil.PostKeynote(out content, out errMsg, pureAddedList.ToList());
                }
                else
                {
                    added = HttpStatusCode.OK;
                }

                if (deleted == HttpStatusCode.OK && updated == HttpStatusCode.Accepted && added == HttpStatusCode.OK)
                {
                    UserChanged = false;
                    this.StatusText = "Ready";
                    KeynoteToDelete.Clear();
                    KeynoteToUpdate.Clear();
                    KeynoteToAdd.Clear();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

    }
}
