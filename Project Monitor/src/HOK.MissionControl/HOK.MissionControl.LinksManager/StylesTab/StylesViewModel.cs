using GalaSoft.MvvmLight;

namespace HOK.MissionControl.LinksManager.StylesTab
{
    public class StylesViewModel : ViewModelBase
    {
        public StylesModel Model { get; set; }

        public StylesViewModel(StylesModel model)
        {
            Model = model;
        }
    }
}
