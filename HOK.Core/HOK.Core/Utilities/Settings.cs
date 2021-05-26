// ReSharper disable UnusedMember.Global

namespace HOK.Core.Utilities
{
    public class Settings
    {
        public string FeedbackToken { get; set; }
        public string FeedbackPath { get; set; }
        public string ClarityUserId { get; set; }
        public string ClarityToken { get; set; }
        public string ClarityMachine { get; set; }
        public string[] ClarityServers { get; set; }
        public string ModelReportingServiceEndpoint { get; set; }
        public string CitrixDesktopConnectorKey { get; set; }
        public string CitrixDesktopConnectorValue { get; set; }
        public string FileOnOpeningFmeUserId { get; set; }
        public string FileOnOpeningFmePassword { get; set; }
        public string FileOnOpeningFmeApiToken { get; set; }
        public string FileOnOpeningFmeHost { get; set; }
        public int FileOnOpeningFmePort { get; set; }
        public string FileOnOpeningFmeClientId { get; set; }
        public string HttpAddress { get; set; }
        public string HttpAddressDebug { get; set; }
    }
}