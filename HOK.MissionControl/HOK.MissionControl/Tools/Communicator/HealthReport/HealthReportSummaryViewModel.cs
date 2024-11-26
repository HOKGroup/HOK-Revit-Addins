using System.Windows.Controls;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class HealthReportSummaryViewModel : ObservableRecipient
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
            set { _fillColor = value; OnPropertyChanged(nameof(FillColor)); }
        }

        private string _toolName;
        public string ToolName
        {
            get { return _toolName; }
            set { _toolName = value; OnPropertyChanged(nameof(ToolName)); }
        }

        private bool _showButton;
        public bool ShowButton
        {
            get { return _showButton; }
            set { _showButton = value; OnPropertyChanged(nameof(ShowButton)); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        private string _count;
        public string Count
        {
            get { return _count; }
            set { _count = value; OnPropertyChanged(nameof(Count)); }
        }

        private string _score;
        public string Score
        {
            get { return _score; }
            set { _score = value; OnPropertyChanged(nameof(Score)); }
        }
    }
}
