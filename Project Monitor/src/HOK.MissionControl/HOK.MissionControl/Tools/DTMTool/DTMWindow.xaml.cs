using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Classes;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace HOK.MissionControl.Tools.DTMTool
{
    /// <summary>
    /// Interaction logic for DTMWindow.xaml
    /// </summary>
    public partial class DTMWindow : Window
    {
        private Document currentDoc = null;
        private ReportingElementInfo reportingInfo = new ReportingElementInfo();
        private string descriptionText = "Grid elements are locked.";

        public string DescriptionText { get { return descriptionText; } set { descriptionText = value; } }

        public DTMWindow(Document doc, ReportingElementInfo info)
        {
            currentDoc = doc;
            reportingInfo = info;
            if (!string.IsNullOrEmpty(info.Description))
            {
                descriptionText = info.Description;
            }
            else
            {
                descriptionText = reportingInfo.CategoryName + " are locked.";
            }
            
            InitializeComponent();
            this.Title = "DTM Tool v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("DTMWindow-Window_Loaded:" + ex.Message);
            }
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://youtu.be/qyUnSuYYs18");
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("DTMWindow-buttonPlay_Click:" + ex.Message);
            }
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //update category trigger settings
                bool settingsUpdated = false;
                string centralPath = reportingInfo.CentralPath;
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(centralPath))
                {
                    Configuration config = AppCommand.Instance.ConfigDictionary[centralPath];
                    var updaterFound = from updater in config.updaters where updater.updaterId.ToLower() == reportingInfo.UpdaterId.ToLower() select updater;
                    if (updaterFound.Count() > 0)
                    {
                        ProjectUpdater pUpdater = updaterFound.First();
                        int updaterIndex = config.updaters.IndexOf(pUpdater);

                        var triggerFound = from trigger in pUpdater.CategoryTriggers where trigger.categoryName == reportingInfo.CategoryName select trigger;
                        if (triggerFound.Count() > 0)
                        {
                            CategoryTrigger cTrigger = triggerFound.First();
                            int triggerIndex = pUpdater.CategoryTriggers.IndexOf(cTrigger);

                            config.updaters[updaterIndex].CategoryTriggers[triggerIndex].isEnabled = false;

                            AppCommand.Instance.ConfigDictionary.Remove(centralPath);
                            AppCommand.Instance.ConfigDictionary.Add(centralPath, config);
                            
                            //refresh category trigger
                            AppCommand.Instance.DTMUpdaterInstance.Unregister(currentDoc);
                            AppCommand.Instance.DTMUpdaterInstance.Register(currentDoc, config.updaters[updaterIndex]);

                            settingsUpdated = true;
                        }
                    }
                }

                if (settingsUpdated)
                {
                    //database updated
                    TriggerRecord record = new TriggerRecord()
                    {
                        configId = reportingInfo.ConfigId,
                        centralPath = reportingInfo.CentralPath,
                        updaterId = reportingInfo.UpdaterId,
                        categoryName = reportingInfo.CategoryName,
                        elementUniqueId = reportingInfo.ReportingUniqueId,
                        edited = DateTime.Now,
                        editedBy = Environment.UserName
                    };

                    string content;
                    string errMessage;
                    
                    HttpStatusCode status = ServerUtil.PostTriggerRecords(out content, out errMessage, record);

                    this.DialogResult = false;
                }
                this.Close();
                
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("DTMWindow-buttonEdit_Click:" + ex.Message);
            }
        }

        private void buttonIgnore_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}
