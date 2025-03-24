#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;
using JetBrains.Annotations;
using System.Runtime.Versioning;
#endregion

namespace HOK.ColorBasedIssueFinder
{
#if REVIT2025_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Command : ExternalCommand
    {
        public override void Execute()
        {
            if (WindowController.Focus<IssueFinder>())
                return;
            var title = "Color Based Issue Finder";
            var view = new IssueFinder(this.UiApplication);
            view.Title = title;

            WindowController.Show(view, this.UiApplication.MainWindowHandle);
        }
    }
}
