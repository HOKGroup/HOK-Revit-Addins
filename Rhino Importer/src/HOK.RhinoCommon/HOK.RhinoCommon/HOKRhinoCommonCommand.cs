using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;

namespace HOK.RhinoCommon
{
    [System.Runtime.InteropServices.Guid("d4b2756c-de33-4dcf-9f77-f62d2c61cded")]
    public class HOKRhinoCommonCommand : Command
    {
        public HOKRhinoCommonCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static HOKRhinoCommonCommand Instance
        {
            get;
            private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "HOKRhinoCommonCommand"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Usually commands in export plug-ins are used to modify settings and behavior.
            // The export work itself is performed by the HOKRhinoCommonPlugIn class.

            return Result.Success;
        }
    }
}
