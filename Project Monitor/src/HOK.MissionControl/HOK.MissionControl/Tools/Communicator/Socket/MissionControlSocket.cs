#region References

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
using HOK.MissionControl.Utils;

#endregion

namespace HOK.MissionControl.Tools.Communicator.Socket
{
    public class MissionControlSocket
    {
        private volatile bool _alive;
        private readonly Thread _t;

        private Quobject.SocketIoClientDotNet.Client.Socket _socket;
        public const string BaseUrlLocal = "http://hok-184vs/";
        //public const string BaseUrlLocal = "http://localhost:8080";

        public MissionControlSocket(Document doc)
        {
            _alive = false;
            _t = new Thread(() =>
            {
                try
                {
                    var centralPath = FileInfoUtil.GetCentralFilePath(doc);

                    var options = new IO.Options
                    {
                        IgnoreServerCertificateValidation = true,
                        AutoConnect = true,
                        ForceNew = true,
                        Upgrade = false
                    };

                    options.Transports = ImmutableList.Create<string>(WebSocket.NAME);

                    _socket = IO.Socket(BaseUrlLocal, options);

                    _socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT,
                        () =>
                        {
                            _socket.Emit("room", centralPath.ToLower());
                            Log.AppendLog(LogMessageType.INFO, "Connected to Mission Control socket.");
                            Log.AppendLog(LogMessageType.INFO, "Joined Room: " + centralPath.ToLower());
                        });

                    _socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_DISCONNECT,
                        () => { Log.AppendLog(LogMessageType.INFO, "Disconnected from Mission Control socket."); });

                    _socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT_TIMEOUT,
                        () => { Log.AppendLog(LogMessageType.INFO, "Mission Control socket connection timed out."); });

                    #region Family Tasks

                    _socket.On("task_deleted", body =>
                    {
                        if (body != null)
                        {
                            var data = JObject.FromObject(body);
                            var deletedIds = data.Property("deletedIds") != null
                                ? data["deletedIds"].ToObject<List<string>>()
                                : new List<string>();
                            var familyName = data.Property("familyName") != null
                                ? data["familyName"].ToObject<string>()
                                : string.Empty;
                            if (!deletedIds.Any() || string.IsNullOrEmpty(familyName)) return;

                            Messenger.Default.Send(new FamilyTaskDeletedMessage
                            {
                                FamilyName = familyName,
                                DeletedIds = deletedIds
                            });
                        }
                        else
                        {
                            Log.AppendLog(LogMessageType.ERROR, "task_deleted: Data was null!");
                        }
                    });

                    _socket.On("familyTask_added", body =>
                    {
                        if (body != null)
                        {
                            var data = JObject.FromObject(body);
                            var familyName = data.Property("familyName") != null
                                ? data["familyName"].ToObject<string>()
                                : string.Empty;
                            var task = data.Property("task") != null ? data["task"].ToObject<FamilyTask>() : null;
                            if (string.IsNullOrEmpty(familyName) || task == null) return;

                            // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                            // We can use "collectionId" to identify the model since that is tied to centralPath.
                            if (!string.Equals(task.AssignedTo, Environment.UserName, StringComparison.CurrentCultureIgnoreCase)) return;

                            Messenger.Default.Send(new FamilyTaskAddedMessage {FamilyName = familyName, Task = task});
                        }
                        else
                        {
                            Log.AppendLog(LogMessageType.ERROR, "familyTask_added: Data was null!");
                        }
                    });

                    _socket.On("familyTask_updated", body =>
                    {
                        if (body != null)
                        {
                            var data = JObject.FromObject(body);
                            var familyName = data.Property("familyName") != null
                                ? data["familyName"].ToObject<string>()
                                : string.Empty;
                            var task = data.Property("task") != null ? data["task"].ToObject<FamilyTask>() : null;
                            if (task == null || string.IsNullOrEmpty(familyName)) return;

                            Messenger.Default.Send(new FamilyTaskUpdatedMessage {FamilyName = familyName, Task = task});
                        }
                        else
                        {
                            Log.AppendLog(LogMessageType.ERROR, "familyTask_updated: Data was null!");
                        }
                    });

                    #endregion

                    #region Sheet Tasks

                    _socket.On("sheetTask_added", body =>
                    {
                        if (body != null)
                        {
                            var data = JObject.FromObject(body);
                            var sheetsData = data.Property("body") != null ? data["body"].ToObject<SheetData>() : null;
                            var sheetId = data.Property("sheetId") != null
                                ? data["sheetId"].ToObject<string>()
                                : string.Empty;
                            var taskId = data.Property("taskId") != null
                                ? data["taskId"].ToObject<string>()
                                : string.Empty;
                            if (sheetsData == null || string.IsNullOrEmpty(sheetId) || string.IsNullOrEmpty(taskId)) return;

                            var sheet = sheetsData.Sheets.FirstOrDefault(x =>
                                string.Equals(x.Id, sheetId, StringComparison.Ordinal));
                            var task = sheet?.Tasks.FirstOrDefault(x =>
                                string.Equals(x.Id, taskId, StringComparison.Ordinal));

                            // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                            // We can use "collectionId" to identify the model since that is tied to centralPath.
                            if (task == null || !string.Equals(task.AssignedTo, Environment.UserName, StringComparison.OrdinalIgnoreCase)) return;

                            Messenger.Default.Send(new SheetsTaskAddedMessage
                            {
                                Sheet = sheet,
                                Task = task,
                                CentralPath = centralPath
                            });
                        }
                        else
                        {
                            Log.AppendLog(LogMessageType.ERROR, "sheetTask_added: Data was null!");
                        }
                    });

                    _socket.On("sheetTask_updated", body =>
                    {
                        if (body != null)
                        {
                            var data = JObject.FromObject(body);
                            var sheetsData = data.Property("body") != null ? data["body"].ToObject<SheetData>() : null;
                            var sheetId = data.Property("sheetId") != null
                                ? data["sheetId"].ToObject<string>()
                                : string.Empty;
                            var taskId = data.Property("taskId") != null
                                ? data["taskId"].ToObject<string>()
                                : string.Empty;
                            if (sheetsData == null || string.IsNullOrEmpty(sheetId) || string.IsNullOrEmpty(taskId)) return;


                            var sheet = sheetsData.Sheets.FirstOrDefault(x =>
                                string.Equals(sheetId, x.Id, StringComparison.Ordinal));
                            var task = sheet?.Tasks.FirstOrDefault(x =>
                                string.Equals(x.Id, taskId, StringComparison.Ordinal));

                            if (task == null) return;

                            Messenger.Default.Send(new SheetsTaskUpdatedMessage
                            {
                                Sheet = sheet,
                                Task = task,
                                CentralPath = centralPath
                            });
                        }
                        else
                        {
                            Log.AppendLog(LogMessageType.ERROR, "sheetTask_updated: Data was null!");
                        }
                    });

                    _socket.On("sheetTask_deleted", body =>
                    {
                        if (body != null)
                        {
                            var data = JObject.FromObject(body);
                            var sheetId = data.Property("sheetId") != null
                                ? data["sheetId"].ToObject<string>()
                                : string.Empty;
                            var deletedIds = data.Property("deletedIds") != null
                                ? data["deletedIds"].ToObject<List<string>>()
                                : new List<string>();
                            if (string.IsNullOrEmpty(sheetId) || !deletedIds.Any()) return;

                            Messenger.Default.Send(new SheetsTaskDeletedMessage
                            {
                                SheetId = sheetId,
                                DeletedIds = deletedIds,
                                CentralPath = centralPath
                            });
                        }
                        else
                        {
                            Log.AppendLog(LogMessageType.ERROR, "sheetTask_deleted: Data was null!");
                        }
                    });

                    _socket.On("sheetTask_sheetsAdded", body =>
                    {
                        if (body != null)
                        {
                            var data = JObject.FromObject(body);
                            var sheetsData = data.Property("body") != null ? data["body"].ToObject<SheetData>() : null;
                            var sheetsIds = data.Property("sheetIds") != null
                                ? data["sheetIds"].ToObject<List<string>>()
                                : new List<string>();
                            if (sheetsData == null || !sheetsIds.Any()) return;

                            Messenger.Default.Send(new SheetTaskSheetsCreatedMessage
                            {
                                Sheets = sheetsData.Sheets.Where(x => sheetsIds.Contains(x.Id)).ToList(),
                                CentralPath = centralPath
                            });
                        }
                        else
                        {
                            Log.AppendLog(LogMessageType.ERROR, "sheetTask_sheetsAdded: Data was null!");
                        }
                    });

                    _socket.On("sheetTask_sheetDeleted", body =>
                    {
                        if (body != null)
                        {
                            var data = JObject.FromObject(body);
                            var sheetId = data.Property("sheetId") != null
                                ? data["sheetId"].ToObject<string>()
                                : string.Empty;
                            var deletedIds = data.Property("deletedIds") != null
                                ? data["deletedIds"].ToObject<List<string>>()
                                : new List<string>();
                            if (string.IsNullOrEmpty(sheetId) || !deletedIds.Any()) return;

                            Messenger.Default.Send(new SheetTaskSheetDeletedMessage
                            {
                                SheetId = sheetId,
                                DeletedIds = deletedIds,
                                CentralPath = centralPath
                            });
                        }
                        else
                        {
                            Log.AppendLog(LogMessageType.ERROR, "sheetTask_sheetDeleted: Data was null!");
                        }
                    });

                    #endregion

                    while (_alive)
                    {
                        Thread.Sleep(100);
                    }

                }
                catch (ThreadInterruptedException)
                {
                    _socket.Disconnect();
                    Log.AppendLog(LogMessageType.ERROR, "Socket was canceled. Most likely because Document was closed.");
                }
                catch (Exception e)
                {
                    Log.AppendLog(LogMessageType.ERROR, e.Message);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
        }

        public void Start()
        {
            _alive = true;
            _t.Start();
        }

        public void Kill(int timeout = 0)
        {
            // (Konrad) This will immediately throw an exception on the thread
            // We can catch it and close the Socket.
            _t.Interrupt();
        }
    }
}
