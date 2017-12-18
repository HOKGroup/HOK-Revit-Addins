using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json.Linq;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.SocketIoClientDotNet.Client;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator.Socket
{
    public class MissionControlSocket
    {
        //public const string BaseUrlLocal = "http://hok-184vs/";
        public const string BaseUrlLocal = "http://localhost:8080";

        public void Main(Document doc)
        {
            var centralPath = BasicFileInfo.Extract(doc.PathName).CentralPath;

            try
            {
                var options = new IO.Options
                {
                    IgnoreServerCertificateValidation = true,
                    AutoConnect = true,
                    ForceNew = true,
                    Upgrade = false,
                };

                options.Transports = ImmutableList.Create<string>(WebSocket.NAME);

                var socket = IO.Socket(BaseUrlLocal, options);

                socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT, () =>
                {
                    Log.AppendLog(LogMessageType.INFO, "Connected to Mission Control socket.");
                });

                socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_DISCONNECT, () =>
                {
                    Log.AppendLog(LogMessageType.INFO, "Disconnected from Mission Control socket.");
                });

                socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT_TIMEOUT, () =>
                {
                    Log.AppendLog(LogMessageType.INFO, "Mission Control socket connection timed out.");
                });

                #region Family Tasks

                socket.On("task_deleted", body =>
                {
                    if (body == null)
                    {
                        Log.AppendLog(LogMessageType.ERROR, "task_deleted: Data was null!");
                    }
                    else
                    {
                        var data = JObject.FromObject(body);
                        if (data == null) return;

                        var ids = new List<string>();
                        foreach (var i in data)
                        {
                            ids.Add((string)i.Value);
                        }

                        Messenger.Default.Send(new FamilyTaskDeletedMessage { DeletedIds = ids });
                    }
                });

                socket.On("familyTask_added", body =>
                {
                    if (body == null)
                    {
                        Log.AppendLog(LogMessageType.ERROR, "familyTask_added: Data was null!");
                    }
                    else
                    {
                        var data = JObject.FromObject(body);
                        var familyStats = data["body"].ToObject<FamilyData>();
                        var familyName = data["familyName"].ToObject<string>();
                        if (familyStats == null || string.IsNullOrEmpty(familyName)) return;

                        Messenger.Default.Send(new FamilyTaskAddedMessage { FamilyStat = familyStats, FamilyName = familyName });
                    }
                });

                socket.On("familyTask_updated", body =>
                {
                    if (body == null)
                    {
                        Log.AppendLog(LogMessageType.ERROR, "familyTask_updated: Data was null!");
                    }
                    else
                    {
                        var data = JObject.FromObject(body);
                        var familyStats = data["body"].ToObject<FamilyData>();
                        var familyName = data["familyName"].ToObject<string>();
                        var oldTaskId = data["oldTaskId"].ToObject<string>();
                        if (familyStats == null || string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(oldTaskId)) return;

                        Messenger.Default.Send(new FamilyTaskUpdatedMessage { FamilyStat = familyStats, FamilyName = familyName, OldTaskId = oldTaskId });
                    }
                });

                #endregion

                #region Sheet Tasks

                socket.On("sheetTask_added", body =>
                {
                    if (body != null)
                    {
                        //TODO: What happens when trying to mark newly created sheet for deletion?
                        //TODO: Identifier will be empty.
                        //TODO: It might be a good idea in general to always pass Sheet _id and Task _id 
                        //TODO: Whether we are dealing with new sheet or edits, those are always true.
                        var data = JObject.FromObject(body);
                        var sheetsData = data["body"].ToObject<SheetData>();
                        var identifier = data["identifier"].ToObject<string>();
                        if (sheetsData == null || string.IsNullOrEmpty(identifier)) return;

                        var sheet = sheetsData.sheets.FirstOrDefault(x => string.Equals(x.identifier, identifier, StringComparison.Ordinal));
                        var task = sheet?.tasks.LastOrDefault();
                        if (task == null || task.assignedTo != Environment.UserName.ToLower() 
                        || !string.Equals(task.centralPath.ToLower(), centralPath.ToLower(), StringComparison.Ordinal)) return;

                        Messenger.Default.Send(new SheetsTaskAddedMessage { Sheet = sheet, Task = task });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_added: Data was null!");
                    }
                });

                socket.On("sheetTask_updated", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var sheetsData = data["body"].ToObject<SheetData>();
                        var identifier = data["identifier"].ToObject<string>();
                        var _id = data["_id"].ToObject<string>();
                        if (sheetsData == null || string.IsNullOrEmpty(_id)) return;

                        SheetItem sheet;
                        if (string.IsNullOrEmpty(identifier))
                        {
                            // (Konrad) We are possibly updating a "new sheet" that has no identifier.
                            // For this case we can use sheetId property that was passed along.
                            var sheetId = data["sheetId"].ToObject<string>();
                            if (string.IsNullOrEmpty(sheetId)) return;

                            sheet = sheetsData.sheets.FirstOrDefault(x => string.Equals(sheetId, x.Id, StringComparison.Ordinal));
                        }
                        else
                        {
                            sheet = sheetsData.sheets.FirstOrDefault(x => string.Equals(x.identifier, identifier, StringComparison.Ordinal));
                        }
                        
                        var task = sheet?.tasks.FirstOrDefault(x => x.Id == _id);
                        if (task == null || !string.Equals(task.centralPath.ToLower(), centralPath.ToLower(), StringComparison.Ordinal)) return;

                        Messenger.Default.Send(new SheetsTaskUpdatedMessage { Sheet = sheet, Task = task });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_updated: Data was null!");
                    }
                });

                socket.On("sheetTask_deleted", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var identifier = data["identifier"].ToObject<string>();
                        var deleted = data["deleted"].ToObject<List<string>>();
                        if (string.IsNullOrEmpty(identifier) || !deleted.Any()) return;

                        Messenger.Default.Send(new SheetsTaskDeletedMessage { Identifier = identifier, Deleted = deleted });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_deleted: Data was null!");
                    }
                });

                socket.On("sheetTask_sheetsAdded", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var sheetsData = data["body"].ToObject<SheetData>();
                        var sheetsIds = data["sheetIds"].ToObject<List<string>>();
                        if (sheetsData == null || !sheetsIds.Any() || !string.Equals(sheetsData.centralPath.ToLower(), centralPath.ToLower(), StringComparison.Ordinal)) return;

                        Messenger.Default.Send(new SheetTaskSheetsCreatedMessage { Sheets = sheetsData.sheets.Where(x => sheetsIds.Contains(x.Id)).ToList() });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_sheetsAdded: Data was null!");
                    }
                });



                //socket.On("sheetTask_approved", body =>
                //{
                //    if (body == null)
                //    {
                //        Log.AppendLog(LogMessageType.ERROR, "sheetTask_approved: Data was null!");
                //    }
                //    else
                //    {
                //        var data = JObject.FromObject(body);
                //        var sheetsData = data["body"].ToObject<SheetData>();
                //        var identifier = data["identifier"].ToObject<string>();
                //        if (sheetsData == null || string.IsNullOrEmpty(identifier)) return;

                //        Messenger.Default.Send(new SheetsTaskApprovedMessage { Identifier = identifier, Sheet = sheetsData.sheets.FirstOrDefault(x => string.Equals(x.identifier, identifier, StringComparison.Ordinal)) });
                //    }
                //});

                #endregion

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
