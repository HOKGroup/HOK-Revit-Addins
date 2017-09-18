using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.MoveBackup
{
    [Name(nameof(Properties.Resources.MoveBackup_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.MoveBackup_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.MoveBackup_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.MoveBackup_PanelName), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.MoveBackup_Namespace), typeof(Properties.Resources))]
    [AdditionalButtonNames(nameof(Properties.Resources.MoveBackup_AdditionalButtonNames), typeof(Properties.Resources))]
    public class AppCommand : IExternalApplication
    {
        public static readonly string[] extensions = {
            ".rvt",
            ".rfa",
            ".rte"
        };

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentSaved += OnDocumentSaved;
            application.ControlledApplication.DocumentSavedAs += OnDocumentSavedAs;

            return Result.Succeeded;
        }

        private static void OnDocumentSavedAs(object sender, DocumentSavedAsEventArgs e)
        {
            if (e.Status == RevitAPIEventStatus.Failed || e.Status == RevitAPIEventStatus.Cancelled) return;
            if (e.Document == null) return;

            MoveBackups(e.Document);
        }

        private static void OnDocumentSaved(object sender, DocumentSavedEventArgs e)
        {
            if (e.Status == RevitAPIEventStatus.Failed || e.Status == RevitAPIEventStatus.Cancelled) return;
            if (e.Document == null) return;

            MoveBackups(e.Document);
        }

        /// <summary>
        /// Initiates the Move Backups method on another thread.
        /// </summary>
        /// <param name="doc">Revit document.</param>
        private static void MoveBackups(Document doc)
        {
            var fileExtension = Path.GetExtension(doc.PathName);
            if (!extensions.Contains(fileExtension)) return;

            new Thread(
                    new MoveBackupModel
                        {
                            RevitFilePath = doc.PathName
                        }
                        .MoveBackupFiles)
                {
                    Priority = ThreadPriority.BelowNormal
                }
                .Start();
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
