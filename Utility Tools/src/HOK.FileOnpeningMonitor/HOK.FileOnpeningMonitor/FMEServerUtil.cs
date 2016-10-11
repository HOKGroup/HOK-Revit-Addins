using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Safe.FMEServer.API;

namespace HOK.FileOnpeningMonitor
{
    public static class FMEServerUtil
    {
        private static string userId = "";
        private static string password = "";
        private static string host = "";
        private static int port = 0;
        private static string clientId = "";

        private static IFMEServerSession serverSession = null;
        private static IFMERepositoryManager repositoryMgr = null;

        public static bool RunFMEWorkspace(CentralFileInfo info, string repository, string workspace, out Dictionary<string, string> properties)
        {
            bool result = false;
            properties = new Dictionary<string, string>();
            try
            {
                if (null == serverSession)
                {
                    serverSession = ConnectToFMEServer();
                }

                if (null != serverSession)
                {
                    IFMETransformationManager transformationMgr = serverSession.GetTransformationManager();

                    IFMETransformationRequest request = serverSession.CreateTransformationRequest("SERVER_CONSOLE_CLIENT", repository, workspace);
                    request.SetPublishedParameter("Office_pp", info.UserLocation);
                    request.SetPublishedParameter("Username_pp", info.UserName);
                    request.SetPublishedParameter("FileName_pp", info.DocCentralPath);

                    IFMETransformationResult transformationResult = serverSession.CreateTransformationResult();
                    long jobId = transformationMgr.SubmitJob(request);
                    Thread.Sleep(500);

                    if (transformationMgr.GetJobResult(jobId, transformationResult))
                    {
                        result = true;
                    }


                    //run Synchronously
                    //transformationResult = transformationMgr.TransactJob(request);

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
            IFMEServerSession fmeServerSession = null;
            try
            {
                userId = "revit";
                password = "revit";
                //host = "hok-119vs";
                host = "fme.hok.com";
                port = 7071;
                clientId = "app_revitnotification";

                fmeServerSession = FMEServer.CreateServerSession();

                IFMEServerConnectionInfo connectInfo = fmeServerSession.CreateServerConnectionInfo(host, port, userId, password);
                IDictionary<string, string> directives = new Dictionary<string, string>();
                directives.Add("CLIENT_ID", clientId);
                fmeServerSession.Init(connectInfo, directives);

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
