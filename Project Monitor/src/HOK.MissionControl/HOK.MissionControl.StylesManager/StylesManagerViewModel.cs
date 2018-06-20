#region References

using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.MissionControl.StylesManager.DimensionsTab;
using HOK.MissionControl.StylesManager.Tabs;

#endregion

namespace HOK.MissionControl.StylesManager
{
    public class StylesManagerViewModel : ViewModelBase
    {
        public RelayCommand<Window> WindowLoaded { get; set; }
        public RelayCommand<Window> ControlClosing { get; set; }
        public string Title { get; set; }
        public ObservableCollection<TabItem> TabItems { get; set; }

        public StylesManagerViewModel(Document doc)
        {
            Title = "Mission Control - Styles Manager v." + Assembly.GetExecutingAssembly().GetName().Version;
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            ControlClosing = new RelayCommand<Window>(OnControlClosing);

            var dims = new TabItem
            {
                Content = new DimensionsView
                {
                    DataContext = new DimensionsViewModel(new DimensionsModel(doc))
                },
                Header = "Dimension Types"
            };
            var dimOverrides = new TabItem
            {
                Content = new DimensionOverridesView
                {
                    DataContext = new DimensionOverridesViewModel(new DimensionOverridesModel(doc))
                },
                Header = "Dimension Overrides"
            };
            var textStyles = new TabItem
            {
                Content = new TextView
                {
                    DataContext = new TextViewModel(new TextModel(doc))
                },
                Header = "Text Styles"
            };

            TabItems = new ObservableCollection<TabItem>
            {
                dims,
                dimOverrides,
                textStyles
            };
        }

        private static void OnControlClosing(Window win)
        {
            ((StylesManagerView) win).DataContext = null;
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
