using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class HealthReportSummaryViewModel : ViewModelBase
    {
        public HealthReportSummaryModel Model { get; set; }
        public RelayCommand<Button> LaunchTool { get; set; }
        public RelayCommand LaunchWebsite { get; set; }

        public HealthReportSummaryViewModel()
        {
            Model = new HealthReportSummaryModel();
            LaunchTool = new RelayCommand<Button>(OnLaunchTool);
            LaunchWebsite = new RelayCommand(OnLaunchWebsite);
        }

        private void OnLaunchWebsite()
        {
            Model.LaunchWebsite();
        }

        private void OnLaunchTool(Button tb)
        {
            if (tb == null) return;
            var text = (string)tb.Content;
            Model.LaunchCommand(text);
        }

        private System.Windows.Media.Color _fillColor;
        public System.Windows.Media.Color FillColor {
            get { return _fillColor; }
            set { _fillColor = value; RaisePropertyChanged(() => FillColor); }
        }

        private string _toolName;
        public string ToolName
        {
            get { return _toolName; }
            set { _toolName = value; RaisePropertyChanged(() => ToolName); }
        }

        private bool _showButton;
        public bool ShowButton
        {
            get { return _showButton; }
            set { _showButton = value; RaisePropertyChanged(() => ShowButton); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(() => Title); }
        }

        private string _count;
        public string Count
        {
            get { return _count; }
            set { _count = value; RaisePropertyChanged(() => Count); }
        }

        private string _score;
        public string Score
        {
            get { return _score; }
            set { _score = value; RaisePropertyChanged(() => Score); }
        }
    }
}
