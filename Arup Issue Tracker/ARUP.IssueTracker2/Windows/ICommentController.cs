using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ARUP.IssueTracker.Windows
{
    public enum AuthoringTool 
    {
        Revit, Navisworks, None
    }

    //This is an abstraction for adding a comment within various (versions) authoring tools
    public abstract class ICommentController
    {
        public AuthoringTool client;

        // Revit stuff
        public abstract Tuple<string, string> getSnapshotAndViewpoint(int elemCheck);
        public abstract void comboVisuals_SelectionChanged(object sender, SelectionChangedEventArgs e, AddComment addCommentWindow);
        public string[] visuals = { "FlatColors", "HLR", "Realistic", "RealisticWithEdges", "Rendering", "Shading", "ShadingWithEdges", "Wireframe" };
    }
}
