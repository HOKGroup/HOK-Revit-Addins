using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.MissionControl.LinksManager.ImagesTab;
using HOK.MissionControl.LinksManager.StylesTab;
using HOK.MissionControl.LinksManager.ImportsTab;
using HOK.Core.WpfUtilities;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.LinksManager
{
    public class LinksManagerViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public ObservableCollection<TabItem> TabItems { get; set; }
        public RelayCommand<Window> WindowLoaded { get; }

        public LinksManagerViewModel(Document doc)
        {
            Title = "Mission Control - Links Manager v." + Assembly.GetExecutingAssembly().GetName().Version;
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem{Content = new ImagesView {DataContext = new ImagesViewModel(new ImagesModel(doc))}, Header = "Images"},
                new TabItem{Content = new StylesView {DataContext = new StylesViewModel(new StylesModel(doc))}, Header = "Styles"},
                new TabItem{Content = new ImportsView {DataContext = new ImportsViewModel(new ImportsModel(doc))}, Header = "Imported DWGs"}
            };
        }

        /// <summary>
        /// Registers status bar and status label with the Progress Manager.
        /// </summary>
        /// <param name="win"></param>
        private static void OnWindowLoaded(Window win)
        {
            StatusBarManager.ProgressBar = ((LinksManagerView)win).progressBar;
            StatusBarManager.StatusLabel = ((LinksManagerView)win).statusLabel;
        }
    }
}
