using System;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator
{
    public enum RequestId
    {
        None = 0,
        EditFamily = 1,
        OpenView = 2,
        UpdateSheet = 3,
        CreateSheet = 4
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
        public SheetTask SheetTask { get; set; }

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
                    case RequestId.CreateSheet:
                    {
                        CreateSheet(app);
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
        /// Submits request to create new sheet.
        /// </summary>
        /// <param name="app"></param>
        private void CreateSheet(UIApplication app)
        {
            IsUpdatingSheet = true;

            app.Application.FailuresProcessing += FailureProcessing;
            var doc = app.ActiveUIDocument.Document;

            using (var trans = new Transaction(doc, "CreateSheet"))
            {
                trans.Start();

                try
                {
                    ViewSheet sheet;
                    if (SheetTask.isPlaceholder)
                    {
                        sheet = ViewSheet.CreatePlaceholder(doc);
                    }
                    else
                    {
                        // TODO: This should be exposed to user in Mission Control.
                        var titleblock = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).FirstOrDefault(x => x.Category.Name == "Title Blocks");
                        if (titleblock == null)
                        {
                            IsUpdatingSheet = false;
                            Messenger.Default.Send(new SheetTaskCompletedMessage { Completed = false, Message = "Could not find a valid TitleBlock." });
                            return;
                        }
                        sheet = ViewSheet.Create(doc, titleblock.Id);
                    }

                    sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.Set(SheetTask.number);
                    sheet.get_Parameter(BuiltInParameter.SHEET_NAME)?.Set(SheetTask.name);

                    // (Konrad) We can set this here and pick up in the UI before sending off to MongoDB.
                    var newSheetItem = new SheetItem(sheet, SheetTask.centralPath)
                    {
                        tasks = SheetItem.tasks,
                        collectionId = SheetItem.collectionId,
                        Id = SheetItem.Id,
                        isNewSheet = true // this was overriden to false by default constructor
                    };
                    SheetItem = newSheetItem;

                    trans.Commit();
                    IsUpdatingSheet = false;
                }
                catch (Exception e)
                {
                    trans.RollBack();
                    IsUpdatingSheet = false;

                    Log.AppendLog(LogMessageType.EXCEPTION, "Failed to create sheet.");
                    Messenger.Default.Send(new SheetTaskCompletedMessage { Completed = false, Message = e.Message });
                }
            }

            // (Konrad) We don't want Revit to keep triggering this even when we are not processing updates.
            app.Application.FailuresProcessing -= FailureProcessing;
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
                    var action = "update";
                    try
                    {
                        if (SheetTask.isDeleted)
                        {
                            action = "delete";
                            doc.Delete(view.Id);
                        }
                        else
                        {
                            view.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.Set(SheetTask.number);
                            view.get_Parameter(BuiltInParameter.SHEET_NAME)?.Set(SheetTask.name);
                        }

                        trans.Commit();
                        IsUpdatingSheet = false;
                    }
                    catch (Exception e)
                    {
                        trans.RollBack();
                        IsUpdatingSheet = false;

                        Log.AppendLog(LogMessageType.EXCEPTION, "Failed to " + action + " sheet.");
                        Messenger.Default.Send(new SheetTaskCompletedMessage { Completed = false, Message = e.Message});
                    }
                }
            }

            // (Konrad) We don't want Revit to keep triggering this even when we are not processing updates.
            app.Application.FailuresProcessing -= FailureProcessing;
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
                if (IsUpdatingSheet) Messenger.Default.Send(new SheetTaskCompletedMessage { Completed = true, Message = "Success!" });
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
    }
}
