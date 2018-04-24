using System;
using System.Linq;
using System.Threading.Tasks;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Core.Utils
{
    public static class AddinUtilities
    {
        /// <summary>
        /// Retrieves addins collection from MongoDB and posts new usage log.
        /// </summary>
        /// <typeparam name="T">AddinData type.</typeparam>
        /// <param name="addinLog">AddinData object.</param>
        [Obsolete]
        public static void PublishAddinLog<T>(T addinLog) where T: new()
        {
            try
            {
                //TODO: Fix this
                //var collections = ServerUtilities.FindAll(new AddinData(), "addins");
                //var c = collections.FirstOrDefault();
                //var collectionId = "";
                //if (c != null)
                //{
                //    collectionId = c.Id;
                //}
                //if (collections.Count == 0)
                //{
                //    ServerUtilities.Post(new AddinData(), "addins", out AddinData addinData);
                //}
                
                //ServerUtilities.Post<AddinData>(addinLog, "addins/" + addinData.Id + "/addlog");
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        /// <summary>
        /// Publishes Addin Log to MongoDB in asynch fashion.
        /// </summary>
        /// <param name="addinLog">AddinLog data to be published.</param>
        /// <param name="callback">Callback method. Will be called when asynch task is complete.</param>
        /// <returns></returns>
        public static async Task PublishAddinLog(AddinLog addinLog, Action<AddinData> callback)
        {
            try
            {
                var collections = await ServerUtilities.FindAll<AddinData>("addins");
                var c = collections.FirstOrDefault();
                var collectionId = "";
                if (c != null)
                {
                    collectionId = c.Id;
                }
                if (collections.Count == 0)
                {
                    var result = await ServerUtilities.PostAsync<AddinData>(new AddinData(), "addins");
                    if (!string.IsNullOrEmpty(result.Id)) collectionId = result.Id;
                }

                var response = await ServerUtilities.PostAsync<AddinData>(addinLog, "addins/" + collectionId + "/addlog");
                callback(response);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
