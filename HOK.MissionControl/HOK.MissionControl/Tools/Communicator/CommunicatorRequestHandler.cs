#region References
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Interop;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using CommunityToolkit.Mvvm.Messaging;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator.Messaging;
using HOK.MissionControl.Utils;
using HOK.MissionControl.Utils.StatusReporter;
using static HOK.Core.Utilities.ElementIdExtension;
#endregion

namespace HOK.MissionControl.Tools.Communicator
{
    public class CommunicatorRequestHandler : IExternalEventHandler
    {
        public Status Status { get; set; }
        public string Message { get; set; }
        public FamilyItem FamilyItem { get; set; }
        public FamilyTask FamilyTask { get; set; }
        public SheetItem SheetItem { get; set; }
        public SheetTask SheetTask { get; set; }
        public static bool IsUpdatingSheet { get; set; }
        public CommunicatorRequest Request { get; set; } = new CommunicatorRequest();
        public static string CentralPath { get; set; }

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
                    case RequestId.SubmitFamily:
                    {
                        SubmitFamily(app);
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
                    case RequestId.ReportStatus:
                    {
                        ReportStatus();
                        break;
                    }
                    case RequestId.Disable:
                    {
                        DisableCommunicator(app);
                        break;
                    }
                    case RequestId.GetCentralPath:
                    {
                        GetCentralPath(app);
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
        /// 
        /// </summary>
        /// <param name="app"></param>
        private static void GetCentralPath(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            CentralPath = FileInfoUtil.GetCentralFilePath(doc);

            WeakReferenceMessenger.Default.Send(new CentralPathObtained
            {
                CentralPath = CentralPath
            });
        }

        /// <summary>
        /// Disables Mission Control Communicator docable panel and button.
        /// </summary>
        private static void DisableCommunicator(UIApplication app)
        {
            var dpid = new DockablePaneId(new Guid(Properties.Resources.CommunicatorGuid));
            var dp = app.GetDockablePane(dpid);
            if (dp == null) return;

            dp.Hide();
            AppCommand.Instance.CommunicatorButton.Enabled = false;
        }

        /// <summary>
        /// We are calling this method likely from another thread. You can't open a new Window and attach it to 
        /// Revit window as it's owner from another thread that is not STAT. 
        /// </summary>
        private void ReportStatus()
        {
            var viewModel = new StatusReporterViewModel(Status, Message);
            var view = new StatusReporterView
            {
                DataContext = viewModel
            };
            var unused = new WindowInteropHelper(view)
            {
                Owner = Process.GetCurrentProcess().MainWindowHandle
            };
            view.Show();
        }

        /// <summary>
        /// Submits request to create new sheet.
        /// </summary>
        /// <param name="app"></param>
        private void CreateSheet(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            CentralPath = FileInfoUtil.GetCentralFilePath(doc);

            IsUpdatingSheet = true;
            app.Application.FailuresProcessing += FailureProcessing;
            
            using (var trans = new Transaction(doc, "CreateSheet"))
            {
                trans.Start();

                try
                {
                    ViewSheet sheet;
                    if (SheetTask.IsPlaceholder)
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
                            WeakReferenceMessenger.Default.Send(new SheetTaskCompletedMessage
                            {
                                Completed = false,
                                Message = "Could not find a valid TitleBlock.",
                                CentralPath = CentralPath
                            });
                            return;
                        }
                        sheet = ViewSheet.Create(doc, titleblock.Id);
                    }

                    sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.Set(SheetTask.Number);
                    sheet.get_Parameter(BuiltInParameter.SHEET_NAME)?.Set(SheetTask.Name);

                    // (Konrad) We can set this here and pick up in the UI before sending off to MongoDB.
                    var newSheetItem = new SheetItem(sheet, CentralPath)
                    {
                        Tasks = SheetItem.Tasks,
                        CollectionId = SheetItem.CollectionId,
                        Id = SheetItem.Id,
                        IsNewSheet = true // this was overriden to false by default constructor
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
                    WeakReferenceMessenger.Default.Send(new SheetTaskCompletedMessage
                    {
                        Completed = false,
                        Message = e.Message,
                        CentralPath = CentralPath
                    });
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
            var doc = app.ActiveUIDocument.Document;
            CentralPath = FileInfoUtil.GetCentralFilePath(doc);

            IsUpdatingSheet = true;
            app.Application.FailuresProcessing += FailureProcessing;
            
            var view = doc.GetElement(SheetItem.UniqueId) as ViewSheet;
            if (view != null)
            {
                if (WorksharingUtils.GetCheckoutStatus(doc, view.Id) == CheckoutStatus.OwnedByOtherUser)
                {
                    IsUpdatingSheet = false;
                    WeakReferenceMessenger.Default.Send(new SheetTaskCompletedMessage
                    {
                        Completed = false,
                        Message = "Element owned by another user. Try reloading latest.",
                        CentralPath = CentralPath
                    });
                    return;
                }
                using (var trans = new Transaction(doc, "UpdateSheet"))
                {
                    trans.Start();
                    var action = "update";
                    try
                    {
                        if (SheetTask.IsDeleted)
                        {
                            action = "delete";
                            doc.Delete(view.Id);
                        }
                        else
                        {
                            view.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.Set(SheetTask.Number);
                            view.get_Parameter(BuiltInParameter.SHEET_NAME)?.Set(SheetTask.Name);
                        }

                        trans.Commit();
                        IsUpdatingSheet = false;
                    }
                    catch (Exception e)
                    {
                        trans.RollBack();
                        IsUpdatingSheet = false;

                        Log.AppendLog(LogMessageType.EXCEPTION, "Failed to " + action + " sheet.");
                        WeakReferenceMessenger.Default.Send(new SheetTaskCompletedMessage
                        {
                            Completed = false,
                            Message = e.Message,
                            CentralPath = CentralPath
                        });
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
            var view = doc.GetElement(SheetItem.UniqueId) as ViewSheet;
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

            var family = new FilteredElementCollector(doc).OfClass(typeof(Family)).FirstOrDefault(x => GetElementIdValue(x.Id) == FamilyItem.ElementId);
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
        /// If Family is marked as Completed it will post the data to MC.
        /// </summary>
        /// <param name="app"></param>
        public void SubmitFamily(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            if (doc == null || doc.IsFamilyDocument) return;

            var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            var familyStatsId = MissionControlSetup.FamilyData[centralPath].Id;
            if (string.IsNullOrEmpty(familyStatsId)) return;

            FamilyTask.CompletedOn = DateTime.UtcNow;
            FamilyTask.CompletedBy = Environment.UserName.ToLower();

            if (!ServerUtilities.Post(FamilyTask,
                "families/" + familyStatsId + "/family/" + FamilyItem.Name + "/updatetask/" + FamilyTask.Id, out FamilyData unused))
            {
                Log.AppendLog(LogMessageType.ERROR, "Failed to submit Family Task Completed update.");
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
                if (IsUpdatingSheet) WeakReferenceMessenger.Default.Send(new SheetTaskCompletedMessage
                {
                    Completed = true,
                    Message = "Success!",
                    CentralPath = CentralPath
                });
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
                if (IsUpdatingSheet) WeakReferenceMessenger.Default.Send(new SheetTaskCompletedMessage
                {
                    Completed = false,
                    Message = "Could not commit transaction. Try reloading latest.",
                    CentralPath = CentralPath
                });
            }
        }
    }

    public class CommunicatorRequest
    {
        private int _request = (int)RequestId.None;

        public RequestId Take()
        {
            return (RequestId)Interlocked.Exchange(ref _request, (int)RequestId.None);
        }

        public void Make(RequestId request)
        {
            Interlocked.Exchange(ref _request, (int)request);
        }
    }

    public enum RequestId
    {
        None,
        EditFamily,
        SubmitFamily,
        OpenView,
        UpdateSheet,
        CreateSheet,
        ReportStatus,
        Disable,
        GetCentralPath
    }

    public enum Status
    {
        Success,
        Error,
        Info
    }
}
