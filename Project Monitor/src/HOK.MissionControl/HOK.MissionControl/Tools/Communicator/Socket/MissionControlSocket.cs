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
                        var data = JObject.FromObject(body);
                        var sheetsData = data["body"].ToObject<SheetData>();
                        var sheetId = data["sheetId"].ToObject<string>();
                        var taskId = data["taskId"].ToObject<string>();
                        if (sheetsData == null || string.IsNullOrEmpty(sheetId) || string.IsNullOrEmpty(taskId)) return;

                        var sheet = sheetsData.sheets.FirstOrDefault(x => string.Equals(x.Id, sheetId, StringComparison.Ordinal));
                        var task = sheet?.tasks.FirstOrDefault(x => string.Equals(x.Id, taskId, StringComparison.Ordinal));
                        if (task == null 
                            || task.assignedTo != Environment.UserName.ToLower() 
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
                        var sheetId = data["sheetId"].ToObject<string>();
                        var taskId = data["taskId"].ToObject<string>();
                        if (sheetsData == null || string.IsNullOrEmpty(sheetId) || string.IsNullOrEmpty(taskId)) return;

                        var sheet = sheetsData.sheets.FirstOrDefault(x => string.Equals(sheetId, x.Id, StringComparison.Ordinal));
                        var task = sheet?.tasks.FirstOrDefault(x => string.Equals(x.Id, taskId, StringComparison.Ordinal));
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
                        var sheetId = data["sheetId"].ToObject<string>();
                        var deletedIds = data["deletedIds"].ToObject<List<string>>();
                        var path = data["centralPath"].ToObject<string>();
                        if (string.IsNullOrEmpty(sheetId) || !deletedIds.Any() || string.IsNullOrEmpty(path)) return;
                        if (!string.Equals(path.ToLower(), centralPath.ToLower(), StringComparison.Ordinal)) return;

                        Messenger.Default.Send(new SheetsTaskDeletedMessage { SheetId = sheetId, DeletedIds = deletedIds });
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
                        if (sheetsData == null || !sheetsIds.Any()) return;
                        if (!string.Equals(sheetsData.centralPath.ToLower(), centralPath.ToLower(), StringComparison.Ordinal)) return;

                        Messenger.Default.Send(new SheetTaskSheetsCreatedMessage { Sheets = sheetsData.sheets.Where(x => sheetsIds.Contains(x.Id)).ToList() });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_sheetsAdded: Data was null!");
                    }
                });

                socket.On("sheetTask_sheetDeleted", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var sheetId = data["sheetId"].ToObject<string>();
                        var deletedIds = data["deletedIds"].ToObject<List<string>>();
                        var path = data["centralPath"].ToObject<string>();
                        if (string.IsNullOrEmpty(sheetId) || string.IsNullOrEmpty(path) || !deletedIds.Any()) return;
                        if (!string.Equals(path.ToLower(), centralPath.ToLower(), StringComparison.Ordinal)) return;

                        Messenger.Default.Send(new SheetTaskSheetDeletedMessage { SheetId = sheetId, DeletedIds = deletedIds });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_sheetDeleted: Data was null!");
                    }
                });

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
