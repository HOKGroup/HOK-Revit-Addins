using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MachineControlTest
{
    class Program
    {
        const string url = "http://localhost:80";

        static void Main(string[] args)
        {
            string filePath = @"\\group\hok\+PUBLIC PARKING\NY\Jinsol Kim\MissionControl\DoorMonitorTest.rvt";
            string encodedUri = System.Uri.EscapeUriString(filePath);
            string encodedUri2 = System.Uri.EscapeDataString(filePath);
            string decdedUri2 = System.Uri.UnescapeDataString(encodedUri2);

            var options = new IO.Options()
            {
                IgnoreServerCertificateValidation = true,
                AutoConnect = true,
                ForceNew = true
            };

            Socket socket = IO.Socket(url, options);

            socket.On(Socket.EVENT_CONNECT, () =>
            {
                Console.WriteLine("Connected");
            });

            socket.On("update_configuration", (data) =>
            {
                JObject data2 = JObject.FromObject(data);

                Console.WriteLine(data2);
            });

            socket.On("add_configuration", (data) =>
            {
                JObject data2 = JObject.FromObject(data);

                Console.WriteLine(data2);
            });

            while (true) { Thread.Sleep(100); }
        }
    }
}
