using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Classes
{
    public class RvtFile 
    {
        private string idVal = "";
        private string centralPathVal = "";

        public string _id { get { return idVal; } set { idVal = value; } }
        public string centralPath { get { return centralPathVal; } set { centralPathVal = value; } }

    }
}
