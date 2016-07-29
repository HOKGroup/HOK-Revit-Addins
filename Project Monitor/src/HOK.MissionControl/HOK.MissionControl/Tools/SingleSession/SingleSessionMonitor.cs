using HOK.MissionControl.Classes;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Tools.SingleSession
{
    public static class SingleSessionMonitor
    {
        private static Guid updaterGuid = new Guid("90391154-67BB-452E-A1A7-A07A98B94F86");
        private static string singleFile = "";
        private static List<string> openedDocuments = new List<string>();
        private static bool singleSessionActivated = false;

        public static Guid UpdaterGuid { get { return updaterGuid; } set { updaterGuid = value; } }
        public static string SingleFile { get { return singleFile; } set { singleFile = value; } }
        public static List<string> OpenedDocuments { get { return openedDocuments; } set { openedDocuments = value; } }
        public static bool SingleSessionActivated { get { return singleSessionActivated; } set { singleSessionActivated = value; } }

        public static bool CancelOpening(string centralFile, Configuration config)
        {
            bool cancel = false;
            try
            {
                if (singleSessionActivated && openedDocuments.Count > 0) { return true; }

                var updaterFound = from updater in config.updaters where updater.updaterId.ToLower() == updaterGuid.ToString().ToLower() select updater;
                if (updaterFound.Count() > 0)
                {
                    ProjectUpdater ssUpdater = updaterFound.First();
                    if (ssUpdater.isUpdaterOn)
                    {
                        if (openedDocuments.Count > 0)
                        {
                            cancel = true;
                        }
                        else
                        {
                            //first opening single file that will activate the single session
                            singleFile = centralFile;
                            singleSessionActivated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("CancelOpening:" + ex.Message);
            }
            return cancel;
        }

        public static void CloseFile(string centralFile)
        {
            try
            {
                if (openedDocuments.Contains(centralFile))
                {
                    openedDocuments.Remove(centralFile);
                }
                if (singleFile == centralFile)
                {
                    singleFile = "";
                    singleSessionActivated = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("CloseFile:" + ex.Message);
            }
        }

    }
}
