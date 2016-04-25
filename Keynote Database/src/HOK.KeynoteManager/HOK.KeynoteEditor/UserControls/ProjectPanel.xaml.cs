using HOK.Keynote.ClassModels;
using HOK.Keynote.REST;
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

namespace HOK.KeynoteEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ProjectPanel.xaml
    /// </summary>
    public partial class ProjectPanel : Window
    {
        private ObservableCollection<KeynoteProjectInfo> projectList = new ObservableCollection<KeynoteProjectInfo>();
        private ObservableCollection<KeynoteSetInfo> keynoteSetList = new ObservableCollection<KeynoteSetInfo>();
        private KeynoteProjectInfo projectInfo = new KeynoteProjectInfo();
        private KeynoteSetInfo setInfo = new KeynoteSetInfo();
        private string setName = "";
        private List<KeynoteInfo> keynoteList = new List<KeynoteInfo>();

        public ObservableCollection<KeynoteProjectInfo> ProjectList { get { return projectList; } set { projectList = value; } }
        public ObservableCollection<KeynoteSetInfo> KeynoteSetList { get { return keynoteSetList; } set { keynoteSetList = value; } }
        public KeynoteProjectInfo ProjectInfo { get { return projectInfo; } set { projectInfo = value; } }
        public KeynoteSetInfo SetInfo { get { return setInfo; } set { setInfo = value; } }
        public string SetName { get { return setName; } set { setName = value; } }
        public List<KeynoteInfo> KeynoteList { get { return keynoteList; } set { keynoteList = value; } }

        public ProjectPanel()
        {
            InitializeComponent();
            this.DataContext = this;

            GetProjectList();
        }

        private void GetProjectList()
        {
            try
            {
                List<KeynoteProjectInfo> projectFound = ServerUtil.GetProjects("projects");
                if (projectFound.Count > 0)
                {
                    projectList = new ObservableCollection<KeynoteProjectInfo>(projectFound.ToList());
                }

                List<KeynoteSetInfo> setInfoFound = ServerUtil.GetKeynoteSets("keynotesets");
                if (setInfoFound.Count > 0)
                {
                    keynoteSetList = new ObservableCollection<KeynoteSetInfo>(setInfoFound.ToList());
                }

                comboBoxNumber.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxNumber.SelectedItem)
                {
                    projectInfo = comboBoxNumber.SelectedItem as KeynoteProjectInfo;
                    if (null != projectInfo)
                    {
                        var setFound = from set in keynoteSetList where set._id == projectInfo.keynoteSet_id select set;
                        if (setFound.Count() > 0)
                        {
                            setInfo = setFound.First();
                            if(!string.IsNullOrEmpty(setInfo._id))
                            {
                                keynoteList = ServerUtil.GetKeynotes("keynotes/setid/" + setInfo._id);
                            }
                        }
                    }
                    setName = textBoxKeynote.Text;

                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

       
        
    }
}
