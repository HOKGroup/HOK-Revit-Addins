
namespace HOK.MissionControl.Tools.Communicator.Messaging
{
    public enum SummaryType
    {
        Views,
        Worksets,
        Families,
        Styles,
        Links,
        Models,
        Groups
    }

    public class HealthReportSummaryAdded
    {
        public SummaryType Type { get; set; }
        public object Data { get; set; }
    }
}
