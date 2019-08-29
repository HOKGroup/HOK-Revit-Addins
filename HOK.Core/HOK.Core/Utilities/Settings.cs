﻿// ReSharper disable UnusedMember.Global

namespace HOK.Core.Utilities
{
    public class Settings
    {
        public string FeedbackToken { get; set; }
        public string FeedbackPath { get; set; }
        public string ModelReportingServiceEndpoint { get; set; }
        public string FileOnOpeningFmeUserId { get; set; }
        public string FileOnOpeningFmePassword { get; set; }
        public string FileOnOpeningFmeHost { get; set; }
        public int FileOnOpeningFmePort { get; set; }
        public string FileOnOpeningFmeClientId { get; set; }
        public string HttpAddress { get; set; }
        public string HttpAddressDebug { get; set; }
    }
}