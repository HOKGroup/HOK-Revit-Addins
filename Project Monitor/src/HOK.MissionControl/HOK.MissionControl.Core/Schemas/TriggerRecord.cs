using System;

namespace HOK.MissionControl.Core.Schemas
{
    public class TriggerRecord
    {
        public string configId { get; set; } = "";
        public string centralPath { get; set; } = "";
        public string updaterId { get; set; } = "";
        public string categoryName { get; set; } = "";
        public string elementUniqueId { get; set; } = "";
        public DateTime edited { get; set; } = DateTime.Now;
        public string editedBy { get; set; } = "";
    }
}
