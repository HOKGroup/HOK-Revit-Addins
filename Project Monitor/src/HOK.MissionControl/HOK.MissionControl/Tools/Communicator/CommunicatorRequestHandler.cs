using System;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using GalaSoft.MvvmLight.Messaging;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator
{
    public enum RequestId
    {
        None = 0,
        EditFamily = 1,
        OpenView = 2,
        UpdateSheet = 3
    }

    public class CommunicatorRequest
    {
        private int _request = (int) RequestId.None;

        public RequestId Take()
        {
            return (RequestId) Interlocked.Exchange(ref _request, (int) RequestId.None);
        }

        public void Make(RequestId request)
        {
            Interlocked.Exchange(ref _request, (int) request);
        }
    }

    public class CommunicatorRequestHandler : IExternalEventHandler
    {
        public static bool IsUpdatingSheet { get; set; }
        public FamilyItem FamilyItem { get; set; }
        public SheetItem SheetItem { get; set; }
        public SheetItem SheetTask { get; set; }

        public CommunicatorRequest Request { get; } = new CommunicatorRequest();

        public string GetName()
        {
            return "Task External Event";
        }

        public void Execute(UIApplication app)
        {
            try
            {
                switch (Request.Take())
                {
                    case RequestId.None:
                    {
                        return;
                    }
                    case RequestId.EditFamily:
                    {
                        EditFamily(app);
                        break;
                    }
                    case RequestId.OpenView:
                    {
                        OpenView(app);
                        break;
                    }
                    case RequestId.UpdateSheet:
                    {
                        UpdateSheet(app);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Submits edits to a sheet.
        /// </summary>
        /// <param name="app"></param>
        private void UpdateSheet(UIApplication app)
        {
            IsUpdatingSheet = true;

            app.Application.FailuresProcessing += FailureProcessing;
            var doc = app.ActiveUIDocument.Document;
            var view = doc.GetElement(SheetItem.uniqueId) as ViewSheet;
            if (view != null)
            {
                if (WorksharingUtils.GetCheckoutStatus(doc, view.Id) == CheckoutStatus.OwnedByOtherUser)
                {
                    IsUpdatingSheet = false;
                    Messenger.Default.Send(new SheetTaskCompletedMessage { Completed = false, Message = "Element owned by another user. Try reloading latest." });
                    return;
                }
                using (var trans = new Transaction(doc, "UpdateSheet"))
                {
                    trans.Start();

                    try
                    {
                        view.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.Set(SheetTask.number);
                        view.get_Parameter(BuiltInParameter.SHEET_NAME)?.Set(SheetTask.name);

                        trans.Commit();
                        IsUpdatingSheet = false;
                    }
                    catch (Exception e)
                    {
                        trans.RollBack();
                        IsUpdatingSheet = false;

                        Log.AppendLog(LogMessageType.EXCEPTION, "Failed to update sheet.");
                        Messenger.Default.Send(new SheetTaskCompletedMessage { Completed = false, Message = e.Message});
                    }
                }
            }
        }

        /// <summary>
        /// Error handler if we cannot commit the transaction for some reason. It will return proper message to UI.
        /// </summary>
        private static void FailureProcessing(object sender, FailuresProcessingEventArgs args)
        {
            var fa = args.GetFailuresAccessor();
            var fmas = fa.GetFailureMessages();
            var count = 0;

            if (fmas.Count == 0)
            {
                args.SetProcessingResult(FailureProcessingResult.Continue);
                if(IsUpdatingSheet) Messenger.Default.Send(new SheetTaskCompletedMessage { Completed = true, Message = "Success!" });
                return;
            }

            foreach (var fma in fmas)
            {
                if (fma.GetSeverity() == FailureSeverity.Warning)
                {
                    fa.DeleteWarning(fma);
                }
                else
                {
                    // (Konrad) Error is more than just a warning. Let's roll back.
                    args.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                    count++;
                }
            }

            if (count > 0)
            {
                if (IsUpdatingSheet) Messenger.Default.Send(new SheetTaskCompletedMessage { Completed = false, Message = "Could not commit transaction. Try reloading latest." });
            }
        }

        /// <summary>
        /// Opens selected View Sheet.
        /// </summary>
        /// <param name="app">UI App.</param>
        private void OpenView(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var view = doc.GetElement(SheetItem.uniqueId) as ViewSheet;
            if (view != null)
            {
                app.ActiveUIDocument.ActiveView = view;
            }
        }

        /// <summary>
        /// Method to open family for editing.
        /// </summary>
        /// <param name="app">Current UI Application.</param>
        public void EditFamily(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            if (doc == null || doc.IsFamilyDocument) return;

            var family = new FilteredElementCollector(doc).OfClass(typeof(Family)).FirstOrDefault(x => x.Id.IntegerValue == FamilyItem.elementId);
            if (family == null) return;

            var famDoc = doc.EditFamily((Family)family);
            string filePath;
            var storedPath = famDoc.PathName;
            if (File.Exists(storedPath))
            {
                filePath = storedPath;
            }
            else
            {
                var myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                filePath = Path.Combine(myDocPath, famDoc.Title);
                if(File.Exists(filePath)) File.Delete(filePath);
                famDoc.SaveAs(filePath);
            }

            famDoc.Close(false);
            app.OpenAndActivateDocument(
                ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath),
                new OpenOptions(),
                false);
        }
    }
}
