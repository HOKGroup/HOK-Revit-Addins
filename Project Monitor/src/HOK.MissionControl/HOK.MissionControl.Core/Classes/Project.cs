using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Core.Classes
{
    public class Project
    {
        private string idVal = "";
        private string numberVal = "";
        private string nameVal = "";
        private string officeVal = "";
        private ProjectAddress addressVal = new ProjectAddress();
        private List<string> configurationsVal = new List<string>();


        public string _id { get { return idVal; } set { idVal = value; } }
        public string number { get { return numberVal; } set { numberVal = value; } }
        public string name { get { return nameVal; } set { nameVal = value; } }
        public string office { get { return officeVal; } set { officeVal = value; } }
        public ProjectAddress address { get { return addressVal; } set { addressVal = value; } }
        public List<string> configurations { get { return configurationsVal; } set { configurationsVal = value; } }

    }
}
