using HOK.ElementWatcher.Classes;
using HOK.ElementWatcher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HOK.ElementWatcher.Settings
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private DTMConfigurations configuration = new DTMConfigurations();
        private int selectedUpdaterIndex = -1; //updater index
        private int selectedTriggerIndex = -1;

        //commands
        private RelayCommand updaterSwitchCommand;
        private RelayCommand applyCommand;
        private RelayCommand acceptCommand;
        private RelayCommand ignoreCommand;

        public DTMConfigurations Configuration { get { return configuration; } set { configuration = value; NotifyPropertyChanged("Configuration"); } }
        public int SelectedUpdaterIndex { get { return selectedUpdaterIndex; } set { selectedUpdaterIndex = value; NotifyPropertyChanged("SelectedUpdaterIndex"); } }
        public int SelectedTriggerIndex { get { return selectedTriggerIndex; } set { selectedTriggerIndex = value; NotifyPropertyChanged("SelectedTriggerIndex"); } }

        public ICommand UpdaterSwitchCommand { get { return updaterSwitchCommand; } }
        public ICommand ApplyCommand { get { return applyCommand; } }
        public ICommand AcceptCommand { get { return acceptCommand; } }
        public ICommand IgnoreCommand { get { return ignoreCommand; } }

        public AdminViewModel(DTMConfigurations config)
        {
            configuration = config;
            updaterSwitchCommand = new RelayCommand(param => this.UpdaterSwitchExecuted(param));
            applyCommand = new RelayCommand(param => this.ApplyExecuted(param));
            acceptCommand = new RelayCommand(param => this.AcceptExecuted(param));
            ignoreCommand = new RelayCommand(param => this.IgnoreExecuted(param));
        }

        public void UpdaterSwitchExecuted(object param)
        {
            try
            {
                if (selectedUpdaterIndex > -1)
                {
                    bool isUpdaterOn = configuration.ProjectUpdaters[selectedUpdaterIndex].IsUpdaterOn;
                    this.Configuration.ProjectUpdaters[selectedUpdaterIndex].IsUpdaterOn = (isUpdaterOn) ? false : true;
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
                //update database
                string content = "";
                string errMsg = "";
                HttpStatusCode status = ServerUtil.UpdateConfiguration(out content, out errMsg, configuration);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void AcceptExecuted(object param)
        {
            try
            {
                CategoryTrigger selectedTrigger = this.Configuration.ProjectUpdaters[selectedUpdaterIndex].CategoryTriggers[selectedTriggerIndex];
                this.Configuration.ProjectUpdaters[selectedUpdaterIndex].CategoryTriggers[selectedTriggerIndex].IsEnabled = false;
                this.Configuration.ProjectUpdaters[selectedUpdaterIndex].CategoryTriggers[selectedTriggerIndex].Requests.Clear();
                string content = "";
                string errMsg = "";
                HttpStatusCode status = ServerUtil.DeleteItem(out content, out errMsg, ControllerType.requestqueues, "triggerid/" + selectedTrigger._id.ToString());
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void IgnoreExecuted(object param)
        {
            try
            {
                CategoryTrigger selectedTrigger = this.Configuration.ProjectUpdaters[selectedUpdaterIndex].CategoryTriggers[selectedTriggerIndex];
                this.Configuration.ProjectUpdaters[selectedUpdaterIndex].CategoryTriggers[selectedTriggerIndex].Requests.Clear();
                string content = "";
                string errMsg = "";
                HttpStatusCode status = ServerUtil.DeleteItem(out content, out errMsg, ControllerType.requestqueues, "triggerid/" + selectedTrigger._id.ToString());
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
