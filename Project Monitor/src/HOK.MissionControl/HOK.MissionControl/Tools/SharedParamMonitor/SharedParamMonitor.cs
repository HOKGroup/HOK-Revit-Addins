using System;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Tools.SharedParamMonitor
{
    public class SharedParamMonitor
    {
        public static void VerifySharedParamPath()
        {
            try
            {

            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
