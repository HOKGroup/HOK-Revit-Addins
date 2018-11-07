using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using HOK.Core.Utilities;
using Safe.FMEServer.API;

namespace HOK.FileOnpeningMonitor
{
    public static class FMEServerUtil
    {
        private static string userId = "";
        private static string password = "";
        private static string host = "";
        private static int port;
        private static string clientId = "";

        private static IFMEServerSession serverSession;
        private static IFMERepositoryManager repositoryMgr;

        public static bool RunFMEWorkspace(CentralFileInfo info, string repository, string workspace, out Dictionary<string, string> properties)
        {
            var result = false;
            properties = new Dictionary<string, string>();
            try
            {
                if (null == serverSession)
                {
                    serverSession = ConnectToFMEServer();
                }

                if (null != serverSession)
                {
                    var transformationMgr = serverSession.GetTransformationManager();

                    var request = serverSession.CreateTransformationRequest("SERVER_CONSOLE_CLIENT", repository, workspace);
                    request.SetPublishedParameter("Office_pp", info.UserLocation);
                    request.SetPublishedParameter("Username_pp", info.UserName);
                    request.SetPublishedParameter("FileName_pp", info.DocCentralPath);

                    var transformationResult = serverSession.CreateTransformationResult();
                    var jobId = transformationMgr.SubmitJob(request);
                    Thread.Sleep(500);

                    if (transformationMgr.GetJobResult(jobId, transformationResult))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to run FME workspace from the FME Server.\n" + ex.Message, "Run FME Workspace", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private static IFMEServerSession ConnectToFMEServer()
        {
            IFMEServerSession fmeServerSession;
            try
            {
                var settingsString = Resources.StreamEmbeddedResource("HOK.Core.Resources.Settings.json");
                var settings = Json.Deserialize<Settings>(settingsString);

                userId = settings.FileOnOpeningFmeUserId;
                password = settings.FileOnOpeningFmePassword;
                host = settings.FileOnOpeningFmeHost;
                port = settings.FileOnOpeningFmePort;
                clientId = settings.FileOnOpeningFmeClientId;
                fmeServerSession = FMEServer.CreateServerSession();

                var connectInfo = fmeServerSession.CreateServerConnectionInfo(host, port, userId, password);
                fmeServerSession.Init(connectInfo, null);

                repositoryMgr = fmeServerSession.GetRepositoryManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect to FME Server.\n" + ex.Message, "Connect to FME Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                fmeServerSession = null;
            }
            return fmeServerSession;
        }
    }
}
