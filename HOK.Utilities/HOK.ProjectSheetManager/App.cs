#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
#endregion

namespace HOK.ProjectSheetManager
{
    class App : ExternalApplication
    {
        public override void OnStartup()
        {
        }

        public override void OnShutdown()
        {
        }
    }
}
