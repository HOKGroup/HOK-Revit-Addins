using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Classes
{
    public class CategoryTrigger 
    {
        private string idVal = "";
        private string categoryNameVal = "";
        private string descriptionVal = "";
        private bool isEnabledVal = false;
        private bool lockedVal = false;
        private string modifiedByVal = "";
        private DateTime modifiedVal = DateTime.Now;

        private string _id { get { return idVal; } set { idVal = value; } }
        public string categoryName { get { return categoryNameVal; } set { categoryNameVal = value; } }
        public string description { get { return descriptionVal; } set { descriptionVal = value; } }
        public bool isEnabled { get { return isEnabledVal; } set { isEnabledVal = value; } }
        public bool locked { get { return lockedVal; } set { lockedVal = value; } }
        public string modifiedBy { get { return modifiedByVal; } set { modifiedByVal = value; } }
        public DateTime modified { get { return modifiedVal; } set { modifiedVal = value; } }

        public CategoryTrigger()
        {

        }

    }
}
