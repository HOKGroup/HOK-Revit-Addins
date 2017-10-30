using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json.Linq;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.SocketIoClientDotNet.Client;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator.Socket
{
    public class MissionControlSocket
    {
        //public const string BaseUrlLocal = "http://hok-184vs/";
        public const string BaseUrlLocal = "http://localhost:8080";

        public void Main()
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
                Log.AppendLog(LogMessageType.INFO, "Disconnected fromo Mission Control socket.");
            });

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

                    Messenger.Default.Send( new FamilyTaskDeletedMessage { DeletedIds = ids });
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
                    var familyStats = data["body"].ToObject<FamilyStat>();
                    var familyName = data["familyName"].ToObject<string>();
                    if (familyStats == null || string.IsNullOrEmpty(familyName)) return;

                    Messenger.Default.Send( new FamilyTaskAddedMessage { FamilyStat = familyStats, FamilyName = familyName});
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
                    var familyStats = data["body"].ToObject<FamilyStat>();
                    var familyName = data["familyName"].ToObject<string>();
                    var oldTaskId = data["oldTaskId"].ToObject<string>();
                    if (familyStats == null || string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(oldTaskId)) return;

                    Messenger.Default.Send(new FamilyTaskUpdatedMessage { FamilyStat = familyStats, FamilyName = familyName, OldTaskId = oldTaskId});
                }
            });

            socket.On("sheetTask_updated", body =>
            {
                if (body == null)
                {
                    Log.AppendLog(LogMessageType.ERROR, "sheetTask_updated: Data was null!");
                }
                else
                {
                    var data = JObject.FromObject(body);
                    var sheetsData = data["body"].ToObject<SheetsData>();
                    var identifier = data["identifier"].ToObject<string>();
                    if (sheetsData == null || string.IsNullOrEmpty(identifier)) return;

                    Messenger.Default.Send(new SheetsTaskUpdateMessage { Task = sheetsData.sheetsChanges.FirstOrDefault(x => string.Equals(x.identifier, identifier, StringComparison.Ordinal))});
                }
            });

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
