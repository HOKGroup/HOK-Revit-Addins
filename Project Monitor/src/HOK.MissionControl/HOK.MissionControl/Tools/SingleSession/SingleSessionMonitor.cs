using System;
using System.Collections.Generic;
using System.Linq;
using HOK.Core;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.SingleSession
{
    public static class SingleSessionMonitor
    {
        public static Guid UpdaterGuid { get; set; } = new Guid("90391154-67BB-452E-A1A7-A07A98B94F86");
        public static string SingleFile { get; set; } = "";
        public static List<string> OpenedDocuments { get; set; } = new List<string>();
        public static bool SingleSessionActivated { get; set; }

        /// <summary>
        /// Checks if there is a Single Session updater turned on for this session and if document opening should be cancelled.
        /// </summary>
        /// <param name="centralFile">Path to the central file.</param>
        /// <param name="config">Configuration for the current Revit model.</param>
        /// <returns>True if opening of file should be cancelled.</returns>
        public static bool CancelOpening(string centralFile, Configuration config)
        {
            var cancel = false;
            try
            {
                if (SingleSessionActivated && OpenedDocuments.Count > 0) { return true; }

                var updaterFound = config.updaters
                    .Where(x => string.Equals(x.updaterId.ToLower(), UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    .ToList();
                if (updaterFound.Any())
                {
                    var ssUpdater = updaterFound.First();
                    if (ssUpdater.isUpdaterOn)
                    {
                        if (OpenedDocuments.Count > 0)
                        {
                            cancel = true;
                        }
                        else
                        {
                            //first opening single file that will activate the single session
                            SingleFile = centralFile;
                            SingleSessionActivated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return cancel;
        }

        /// <summary>
        /// Closes file that user attempted to open.
        /// </summary>
        /// <param name="centralFile">Path to central file.</param>
        public static void CloseFile(string centralFile)
        {
            try
            {
                if (OpenedDocuments.Contains(centralFile))
                {
                    OpenedDocuments.Remove(centralFile);
                }
                if (SingleFile != centralFile) return;
                SingleFile = "";
                SingleSessionActivated = false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }
    }
}
