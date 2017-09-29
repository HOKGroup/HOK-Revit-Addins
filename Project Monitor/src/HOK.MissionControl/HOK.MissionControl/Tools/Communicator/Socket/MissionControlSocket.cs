using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.SocketIoClientDotNet.Client;

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
            //socket.Connect();

            socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT, () =>
            {
                Log.AppendLog(LogMessageType.INFO, "Connected to Mission Control socket.");
            });

            socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_DISCONNECT, () =>
            {
                Log.AppendLog(LogMessageType.INFO, "Connected to Mission Control socket.");
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

                    var ids = new HashSet<string>();
                    foreach (var i in data)
                    {
                        ids.Add((string)i.Value); // this is under assumption that MongoDB would not store not unique ids
                    }

                    Messenger.Default.Send( new TaskDeletedMessage { DeletedIds = ids });
                }
            });

            socket.On("task_added", body =>
            {
                if (body == null)
                {
                    Log.AppendLog(LogMessageType.ERROR, "task_deleted: Data was null!");
                }
                else
                {
                    var data = JObject.FromObject(body);
                    var familyStats = data["body"].ToObject<FamilyStat>();
                    var familyId = data["familyId"].ToObject<int>();
                    if (familyStats == null || familyId == -1) return;

                    Messenger.Default.Send( new TaskAddedMessage { FamilyStat = familyStats, FamilyId = familyId});
                }
            });

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
