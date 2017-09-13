using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using HOK.AddInManager.Classes;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;

namespace HOK.AddInManager.UserControls
{
    public class AddInViewModel : INotifyPropertyChanged
    {
        private Addins addins;
        private bool userChanged = true;

        public Addins AddinsObj
        {
            get { return addins; }
            set { addins = value; NotifyPropertyChanged("AddinsObj"); }
        }

        private ObservableCollection<AddinInfo> selectedAddins = new ObservableCollection<AddinInfo>();
        public ObservableCollection<AddinInfo> SelectedAddins
        {
            get { return selectedAddins; }
            set { selectedAddins = value; NotifyPropertyChanged("SelectedAddins"); }
        }

        private readonly RelayCommand openUrlCommand;
        public ICommand OpenUrlCommand
        {
            get { return openUrlCommand; }
        }

        private readonly RelayCommand loadTypeCommand;
        public ICommand LoadTypeCommand
        {
            get { return loadTypeCommand; }
        }

        public AddInViewModel(Addins addinsObj)
        {
            addins = addinsObj;
            openUrlCommand = new RelayCommand(OpenUrlExecuted);
            loadTypeCommand = new RelayCommand(LoadTypeExecuted);
        }

        public void OpenUrlExecuted(object param)
        {
            try
            {
                if (null != param)
                {
                    System.Diagnostics.Process.Start(param.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public void LoadTypeExecuted(object param)
        {
            try
            {
                if (param == null || !userChanged) return;

                var parameters = param as DataGridParameters;
                if (parameters == null || parameters.SelectedCollection.Count <= 1) return;

                userChanged = false;
                foreach (var obj in parameters.SelectedCollection)
                {
                    var info = obj as AddinInfo;
                    if (info == null) continue;

                    var addinFound = addins.AddinCollection
                        .Where(x => x.ToolName == info.ToolName)
                        .ToList();
                    if (!addinFound.Any()) continue;

                    var addinIndex = addins.AddinCollection.IndexOf(addinFound.First());
                    if (addinIndex > -1)
                    {
                        addins.AddinCollection[addinIndex].ToolLoadType = parameters.SelectedLoadType;
                    }
                }
                userChanged = true;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }

    public class DataGridParameters
    {
        public LoadType SelectedLoadType { get; set; }
        public ObservableCollection<object> SelectedCollection { get; set; }
    }
}
