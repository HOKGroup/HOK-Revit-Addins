using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.MissionControl.StylesManager.DimensionsTab;

namespace HOK.MissionControl.StylesManager
{
    public class StylesManagerViewModel : ViewModelBase
    {
        public RelayCommand<Window> WindowLoaded { get; set; }
        public string Title { get; set; }
        public ObservableCollection<TabItem> TabItems { get; set; }

        public StylesManagerViewModel(Document doc)
        {
            Title = "Mission Control - Styles Manager v." + Assembly.GetExecutingAssembly().GetName().Version;
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem{Content = new DimensionsView {DataContext = new DimensionsViewModel(new DimensionsModel(doc))}, Header = "Dimensions"}
            };
        }

        /// <summary>
        /// Registers status bar and status label with the Progress Manager.
        /// </summary>
        /// <param name="win"></param>
        private static void OnWindowLoaded(Window win)
        {
            HOK.Core.WpfUtilities.StatusBarManager.ProgressBar = ((StylesManagerView)win).progressBar;
            HOK.Core.WpfUtilities.StatusBarManager.StatusLabel = ((StylesManagerView)win).statusLabel;
        }
    }
}
