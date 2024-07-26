using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator.Messaging;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.Communicator.Socket
{
    public class MissionControlSocket
    {
        private ClientWebSocket _client;
        private Uri _serverUri;
        private const int ReceiveChunkSize = 1024;
        private readonly Thread _t;
        public string BaseUrlLocal = ServerUtilities.RestApiBaseUrl;

        public MissionControlSocket(Document doc)
        {
            _t = new Thread(async () =>
            {
                try {
                    _serverUri = new Uri(BaseUrlLocal);
                    _client = new ClientWebSocket();
                    await _client.ConnectAsync(new Uri(""), CancellationToken.None);
                    await ReceiveMessagesAsync();

                }
                catch (ThreadInterruptedException)
                {
                    _client.Abort();
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

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[ReceiveChunkSize];
            while (_client.State == WebSocketState.Open)
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    Console.WriteLine("Closed connection.");
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(message);
                }
            }
        }

        private void HandleMessage(string message)
        {
            // Assuming the message is a JSON string
            var json = JObject.Parse(message);
            var eventType = json["event"]?.ToString();
            switch (eventType)
            {
                case "taskDeleted":
                    HandleTaskDeleted(json);
                    break;
                case "sheetCreated":
                    HandleSheetCreated(json);
                    break;
                default:
                    Console.WriteLine($"Unhandled event type: {eventType}");
                    break;
            }
        }

        private void HandleTaskDeleted(JObject json)
        {
            var taskId = json["data"]?["taskId"]?.ToString();
            Console.WriteLine($"Task deleted: {taskId}");
        }

        private void HandleSheetCreated(JObject json)
        {
            var sheetId = json["data"]?["sheetId"]?.ToString();
            Console.WriteLine($"Sheet created: {sheetId}");
        }

        public void Start()
        {
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
