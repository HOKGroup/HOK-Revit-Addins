using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ColorBasedIssueFinder.IssueFinderLib
{
    public class Results
    {
        public string DocName { get; set; }
        public List<ErrorRect> ErrorRects { get; set; }
    }
}
