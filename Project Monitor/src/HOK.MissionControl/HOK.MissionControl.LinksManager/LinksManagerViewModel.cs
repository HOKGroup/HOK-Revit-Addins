using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using HOK.MissionControl.LinksManager.ImagesTab;
using HOK.MissionControl.LinksManager.StylesTab;
using HOK.MissionControl.LinksManager.ImportsTab;

namespace HOK.MissionControl.LinksManager
{
    public class LinksManagerViewModel : ViewModelBase
    {
        public LinksManagerModel Model { get; set; }
        public string Title { get; set; }
        public ObservableCollection<TabItem> TabItems { get; set; }

        public LinksManagerViewModel(LinksManagerModel model)
        {
            Model = model;
            Title = "Mission Control - Links Manager v." + Assembly.GetExecutingAssembly().GetName().Version;

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem{Content = new ImagesView {DataContext = new ImagesViewModel(new ImagesModel(Model._doc))}, Header = "Images"},
                new TabItem{Content = new StylesView {DataContext = new StylesViewModel(new StylesModel(Model._doc))}, Header = "Styles"},
                new TabItem{Content = new ImportsView {DataContext = new ImportsViewModel(new ImportsModel(Model._doc))}, Header = "Imported DWGs"}
            };
        }
    }
}
