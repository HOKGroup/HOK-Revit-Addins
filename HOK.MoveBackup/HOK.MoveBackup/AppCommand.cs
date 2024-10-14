using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;

namespace HOK.MoveBackup
{
    public class AppCommand : ExternalApplication
    {
        public static readonly string[] extensions = {
            ".rvt",
            ".rfa",
            ".rte"
        };

        public override void OnStartup()
        {
            Application.ControlledApplication.DocumentSaved += OnDocumentSaved;
            Application.ControlledApplication.DocumentSavedAs += OnDocumentSavedAs;
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

            new Thread(new MoveBackupModel {RevitFilePath = doc.PathName}.MoveBackupFiles)
            {
                Priority = ThreadPriority.BelowNormal
            }.Start();
        }
    }
}
