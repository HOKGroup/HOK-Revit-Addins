using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ARUP.IssueTracker.Windows
{
    //This is an abstraction for attached components within various (versions) authoring tools
    public abstract class IComponentController
    {
        public AuthoringTool client;

        // Revit stuff
        public abstract void selectElements(List<string> elementIds);
    }
}
