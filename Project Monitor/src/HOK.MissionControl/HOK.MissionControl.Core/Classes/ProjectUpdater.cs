using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Core.Classes
{
    public class ProjectUpdater
    {
        private string idVal = "";
        private string updaterIdVal = "";
        private string updaterNameVal = "";
        private string descriptionVal = "";
        private string addInIdVal = "";
        private string addInNameVal = "";
        private bool isUpdaterOnVal = false;
        private List<CategoryTrigger> categoryTriggersVal = new List<CategoryTrigger>();

        public string _id { get { return idVal; } set { idVal = value; } }
        public string updaterId { get { return updaterIdVal; } set { updaterIdVal = value; } }
        public string updaterName { get { return updaterNameVal; } set { updaterNameVal = value; } }
        public string description { get { return descriptionVal; } set { descriptionVal = value; } }
        public string addInId { get { return addInIdVal; } set { addInIdVal = value; } }
        public string addInName { get { return addInNameVal; } set { addInNameVal = value; } }
        public bool isUpdaterOn { get { return isUpdaterOnVal; } set { isUpdaterOnVal = value; } }
        public List<CategoryTrigger> CategoryTriggers { get { return categoryTriggersVal; } set { categoryTriggersVal = value; } }

    }
}
