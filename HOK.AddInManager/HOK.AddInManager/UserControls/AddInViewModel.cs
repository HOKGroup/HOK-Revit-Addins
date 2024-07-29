#region References

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using HOK.AddInManager.Classes;
using HOK.Core.Utilities;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using HOK.Core.WpfUtilities;
using HOK.Feedback;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

#endregion

namespace HOK.AddInManager.UserControls
{
    public class AddInViewModel : ObservableRecipient
    {
        #region Properties

        private bool _userChanged = true;
        public string Title { get; set; }
        public RelayCommand Help { get; set; }
        public RelayCommand SubmitComment { get; set; }
        public RelayCommand<object> LoadTypeCommand { get; set; }
        public RelayCommand<object> OpenUrlCommand { get; set; }
        public RelayCommand<Window> WindowLoaded { get; set; }
        public RelayCommand<Window> Cancel { get; set; }
        public RelayCommand<Window> Ok { get; set; }

        private Addins _addins;
        public Addins AddinsObj
        {
            get { return _addins; }
            set { _addins = value; OnPropertyChanged(nameof(AddinsObj)); }
        }

        private ObservableCollection<AddinInfo> _selectedAddins = new ObservableCollection<AddinInfo>();
        public ObservableCollection<AddinInfo> SelectedAddins
        {
            get { return _selectedAddins; }
            set { _selectedAddins = value; OnPropertyChanged(nameof(SelectedAddins)); }
        }

        #endregion

        public AddInViewModel(Addins addinsObj)
        {
            _addins = addinsObj;
            Title = "HOK Addin Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Help = new RelayCommand(OnHelp);
            SubmitComment = new RelayCommand(OnSubmitComment);
            OpenUrlCommand = new RelayCommand<object>(OpenUrlExecuted);
            LoadTypeCommand = new RelayCommand<object>(LoadTypeExecuted);
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            Cancel = new RelayCommand<Window>(OnCancel);
            Ok = new RelayCommand<Window>(OnOk);
        }

        private static void OnWindowLoaded(Window win)
        {
            StatusBarManager.StatusLabel = ((MainWindow)win).statusLabel;
        }

        private static void OnCancel(Window win)
        {
            win.Close();
        }

        private static void OnOk(Window win)
        {
            win.DialogResult = true;
            win.Close();
        }

        private void OnSubmitComment()
        {
            var model = new FeedbackModel();
            var viewModel = new FeedbackViewModel(model, Title);
            var view = new FeedbackView
            {
                DataContext = viewModel
            };

            var unused = new WindowInteropHelper(view)
            {
                Owner = Process.GetCurrentProcess().MainWindowHandle
            };

            view.ShowDialog();
        }

        private static void OnHelp()
        {
            try
            {
                var ttt = AppCommand.thisApp.addinManagerToolTip;
                if (!string.IsNullOrEmpty(ttt.HelpUrl))
                {
                    Process.Start(ttt.HelpUrl);
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
                    Process.Start(param.ToString());
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
                if (param == null || !_userChanged) return;

                var parameters = param as DataGridParameters;
                if (parameters == null || parameters.SelectedCollection.Count <= 1) return;

                _userChanged = false;
                foreach (var obj in parameters.SelectedCollection)
                {
                    var info = obj as AddinInfo;
                    if (info == null) continue;

                    var addinFound = _addins.AddinCollection
                        .Where(x => x.ToolName == info.ToolName)
                        .ToList();
                    if (!addinFound.Any()) continue;

                    var addinIndex = _addins.AddinCollection.IndexOf(addinFound.First());
                    if (addinIndex > -1)
                    {
                        _addins.AddinCollection[addinIndex].ToolLoadType = parameters.SelectedLoadType;
                    }
                }
                _userChanged = true;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }

    public class DataGridParameters
    {
        public LoadType SelectedLoadType { get; set; }
        public ObservableCollection<object> SelectedCollection { get; set; }
    }
}
