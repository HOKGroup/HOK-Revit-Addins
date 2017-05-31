using System;

namespace HOK.MissionControl.Core.Classes
{
    public class CategoryTrigger
    {
        public string categoryName { get; set; } = "";
        public string description { get; set; } = "";
        public bool isEnabled { get; set; } = false;
        public bool locked { get; set; } = false;
        public string modifiedBy { get; set; } = "";
        public DateTime modified { get; set; } = DateTime.Now;
    }
}
