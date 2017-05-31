using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Classes;
using HOK.MissionControl.Core.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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
            Title = "DTM Tool v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataContext = this;
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
                var settingsUpdated = false;
                var centralPath = reportingInfo.CentralPath;
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(centralPath))
                {
                    var config = AppCommand.Instance.ConfigDictionary[centralPath];
                    var updaterFound = from updater in config.updaters where string.Equals(updater.updaterId.ToLower(),
                            reportingInfo.UpdaterId.ToLower(), StringComparison.Ordinal)
                        select updater;
                    if (updaterFound.Any())
                    {
                        var pUpdater = updaterFound.First();
                        var updaterIndex = config.updaters.IndexOf(pUpdater);

                        var triggerFound = from trigger in pUpdater.CategoryTriggers where trigger.categoryName == reportingInfo.CategoryName select trigger;
                        if (triggerFound.Any())
                        {
                            var cTrigger = triggerFound.First();
                            var triggerIndex = pUpdater.CategoryTriggers.IndexOf(cTrigger);

                            config.updaters[updaterIndex].CategoryTriggers[triggerIndex].isEnabled = false;

                            AppCommand.Instance.ConfigDictionary.Remove(centralPath);
                            AppCommand.Instance.ConfigDictionary.Add(centralPath, config);
                            
                            //refresh category trigger
                            AppCommand.Instance.DtmUpdaterInstance.Unregister(currentDoc);
                            AppCommand.Instance.DtmUpdaterInstance.Register(currentDoc, config.updaters[updaterIndex]);

                            settingsUpdated = true;
                        }
                    }
                }

                if (settingsUpdated)
                {
                    //database updated
                    var record = new TriggerRecord
                    {
                        configId = reportingInfo.ConfigId,
                        centralPath = reportingInfo.CentralPath,
                        updaterId = reportingInfo.UpdaterId,
                        categoryName = reportingInfo.CategoryName,
                        elementUniqueId = reportingInfo.ReportingUniqueId,
                        edited = DateTime.Now,
                        editedBy = Environment.UserName
                    };
                    
                    ServerUtil.PostTriggerRecords(record);

                    DialogResult = false;
                }
                Close();
                
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("DTMWindow-buttonEdit_Click:" + ex.Message);
            }
        }

        private void buttonIgnore_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}
