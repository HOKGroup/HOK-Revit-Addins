using HOK.AddInManager.Classes;
using HOK.AddInManager.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HOK.AddInManager.UserControls
{
    public class AddInViewModel:INotifyPropertyChanged
    {
        private Addins addins = new Addins();
        private ObservableCollection<AddinInfo> selectedAddins = new ObservableCollection<AddinInfo>();

        private RelayCommand openUrlCommand;
        private RelayCommand loadTypeCommand;
        private bool userChanged = true;

        public Addins AddinsObj { get { return addins; } set { addins = value; NotifyPropertyChanged("AddinsObj"); } }
        public ObservableCollection<AddinInfo> SelectedAddins { get { return selectedAddins; } set { selectedAddins = value; NotifyPropertyChanged("SelectedAddins"); } }

        public ICommand OpenUrlCommand { get { return openUrlCommand; } }
        public ICommand LoadTypeCommand { get { return loadTypeCommand; } }

        public AddInViewModel(Addins addinsObj)
        {
            addins = addinsObj;
            openUrlCommand = new RelayCommand(param => this.OpenUrlExecuted(param));
            loadTypeCommand = new RelayCommand(param => this.LoadTypeExecuted(param));
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
                string message = ex.Message;
            }
        }

        public void LoadTypeExecuted(object param)
        {
            try
            {
                if (null != param && userChanged)
                {
                    DataGridParameters parameters = param as DataGridParameters;
                    if (null != parameters)
                    {  
                        if (parameters.SelectedCollection.Count > 1)
                        {
                            userChanged = false;
                            foreach (object obj in parameters.SelectedCollection)
                            {
                                AddinInfo info = obj as AddinInfo;
                                if (null != info)
                                {
                                    var addinFound = from addin in addins.AddinCollection where addin.ToolName == info.ToolName select addin;
                                    if (addinFound.Count() > 0)
                                    {
                                        int addinIndex = addins.AddinCollection.IndexOf(addinFound.First());
                                        if (addinIndex > -1)
                                        {
                                            addins.AddinCollection[addinIndex].ToolLoadType = parameters.SelectedLoadType;
                                        }
                                    }
                                }
                            }
                            userChanged = true;
                        }                     
                    }
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

    public class DataGridParameters
    {
        public LoadType SelectedLoadType { get; set; }
        public ObservableCollection<object> SelectedCollection { get; set; }
    }
}
