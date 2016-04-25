using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.KeynoteEditor.Utils
{
    public class KeynoteConfiguration
    {
        private string projectId = "";
        private string keynoteSetId = "";

        public string ProjectId { get { return projectId; } set { projectId = value; } }
        public string KeynoteSetId { get { return keynoteSetId; } set { keynoteSetId = value; } }

        public KeynoteConfiguration()
        {
        }
    }
}
