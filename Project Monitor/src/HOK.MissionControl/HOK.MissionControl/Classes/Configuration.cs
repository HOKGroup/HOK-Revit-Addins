using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Classes
{
    public class Configuration 
    {
        private string idVal = "";
        private string nameVal = "";
        private List<RvtFile> filesVal = new List<RvtFile>();
        private List<ProjectUpdater> updatersVal = new List<ProjectUpdater>();

        public string _id { get { return idVal; } set { idVal = value; } }
        public string name { get { return nameVal; } set { nameVal = value; } }
        public List<RvtFile> files { get { return filesVal; } set { filesVal = value; } }
        public List<ProjectUpdater> updaters { get { return updatersVal; } set { updatersVal = value; } }
    }
}
