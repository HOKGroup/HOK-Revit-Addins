using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Core.Classes
{
    public class TriggerRecord
    {
        private string idVal;
        private string configIdVal = "";
        private string centralPathVal = "";
        private string updaterIdVal = "";
        private string categoryNameVal = "";
        private string elementUniqueIdVal = "";
        private DateTime editedVal = DateTime.Now;
        private string editedByVal = "";


        //public string _id { get { return idVal; } set { idVal = value;  } }
        public string configId { get { return configIdVal; } set { configIdVal = value; } }
        public string centralPath { get { return centralPathVal; } set { centralPathVal = value; } }
        public string updaterId { get { return updaterIdVal; } set { updaterIdVal = value; } }
        public string categoryName { get { return categoryNameVal; } set { categoryNameVal = value; } }
        public string elementUniqueId { get { return elementUniqueIdVal; } set { elementUniqueIdVal = value; } }
        public DateTime edited { get { return editedVal; } set { editedVal = value; } }
        public string editedBy { get { return editedByVal; } set { editedByVal = value; } }

    }
}
