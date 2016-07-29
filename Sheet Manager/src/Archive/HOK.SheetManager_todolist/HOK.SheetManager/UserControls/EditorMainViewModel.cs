using HOK.SheetManager.UserControls.WorkSpace;
using HOK.SheetManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace HOK.SheetManager.UserControls
{
    public class EditorMainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<EditorCommandInfo> commands = new ObservableCollection<EditorCommandInfo>();
        private EditorCommandInfo selectedCommand = null;

        public ObservableCollection<EditorCommandInfo> Commands { get { return commands; } set { commands = value; NotifyPropertyChanged("Commands"); } }
        public EditorCommandInfo SelectedCommand { get { return selectedCommand; } set { selectedCommand = value; NotifyPropertyChanged("SelectedCommand"); } }

        public EditorMainViewModel()
        {
            GetCommandInfo();
        }

        private void GetCommandInfo()
        {
            try
            {
                EditorCommandInfo commandInfo = new EditorCommandInfo() { CommandName = "PROJECTS", ContentPanel = new ProjectPanel() { DataContext = new ProjectViewModel() } };
                commands.Add(commandInfo);

                commandInfo = new EditorCommandInfo() { CommandName = "SHEETS", ContentPanel = new ProjectPanel() { DataContext = new ProjectViewModel() } };
                commands.Add(commandInfo);

                commandInfo = new EditorCommandInfo() { CommandName = "REVISIONS", ContentPanel = new ProjectPanel() { DataContext = new ProjectViewModel() } };
                commands.Add(commandInfo);

                commandInfo = new EditorCommandInfo() { CommandName = "REVISION ON SHEETS", ContentPanel = new ProjectPanel() { DataContext = new ProjectViewModel() } };
                commands.Add(commandInfo);

                commandInfo = new EditorCommandInfo() { CommandName = "VIEWS", ContentPanel = new ProjectPanel() { DataContext = new ProjectViewModel() } };
                commands.Add(commandInfo);

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

    public class EditorCommandInfo : INotifyPropertyChanged
    {
        private string commandName = "";
        private bool isSelected = false;
        private UserControl contentPanel = null;

        public string CommandName { get { return commandName; } set { commandName = value; NotifyPropertyChanged("CommandName"); } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }
        public UserControl ContentPanel { get { return contentPanel; } set { contentPanel = value; NotifyPropertyChanged("ContentPanel"); } }

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
