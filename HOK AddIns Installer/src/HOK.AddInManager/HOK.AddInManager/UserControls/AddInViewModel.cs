using System;
using System.Collections.ObjectModel;
using System.Linq;
using HOK.AddInManager.Classes;
using HOK.Core.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace HOK.AddInManager.UserControls
{
    public class AddInViewModel : ViewModelBase
    {
        private Addins addins;
        private bool userChanged = true;
        public string Title { get; set; }
        public RelayCommand Help { get; set; }
        public RelayCommand<object> LoadTypeCommand { get; set; }
        public RelayCommand<object> OpenUrlCommand { get; set; }

        public AddInViewModel(Addins addinsObj)
        {
            addins = addinsObj;
            Title = "HOK Addin Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Help = new RelayCommand(OnHelp);
            OpenUrlCommand = new RelayCommand<object>(OpenUrlExecuted);
            LoadTypeCommand = new RelayCommand<object>(LoadTypeExecuted);
        }

        private static void OnHelp()
        {
            try
            {
                var ttt = AppCommand.thisApp.addinManagerToolTip;
                if (!string.IsNullOrEmpty(ttt.HelpUrl))
                {
                    System.Diagnostics.Process.Start(ttt.HelpUrl);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
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

        public Addins AddinsObj
        {
            get { return addins; }
            set { addins = value; RaisePropertyChanged(() => AddinsObj); }
        }

        private ObservableCollection<AddinInfo> selectedAddins = new ObservableCollection<AddinInfo>();
        public ObservableCollection<AddinInfo> SelectedAddins
        {
            get { return selectedAddins; }
            set { selectedAddins = value; RaisePropertyChanged(() => SelectedAddins); }
        }
    }

    public class DataGridParameters
    {
        public LoadType SelectedLoadType { get; set; }
        public ObservableCollection<object> SelectedCollection { get; set; }
    }
}
