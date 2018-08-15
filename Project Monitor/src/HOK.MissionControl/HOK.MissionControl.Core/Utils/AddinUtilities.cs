using System;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Core.Utils
{
    public static class AddinUtilities
    {
        /// <summary>
        /// Publishes Addin Log to MongoDB in asynch fashion.
        /// </summary>
        /// <param name="addinLog">AddinLog data to be published.</param>
        /// <returns></returns>
        public static async void PublishAddinLog(AddinLog addinLog)
        {
            try
            {
                var unused = await ServerUtilities.PostAsync<AddinLog>(addinLog, "addins");
                Log.AppendLog(LogMessageType.INFO, "Addin info was published. " + unused.Id);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
