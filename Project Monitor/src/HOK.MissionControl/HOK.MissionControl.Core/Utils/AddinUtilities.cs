using System;
using System.Linq;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Core.Utils
{
    public static class AddinUtilities
    {
        public static void PublishAddinLog<T>(T addinLog) where T: new()
        {
            try
            {
                var collections = ServerUtilities.FindAll(new AddinData(), "addins");
                var c = collections.FirstOrDefault();
                var collectionId = "";
                if (c != null)
                {
                    collectionId = c.Id;
                }
                if (collections.Count == 0)
                {
                    collectionId = ServerUtilities.PostDataScheme(new AddinData(), "addins").Id;
                }

                ServerUtilities.PostToMongoDB(addinLog, "addins", collectionId, "addlog");
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
            
        }
    }
}
