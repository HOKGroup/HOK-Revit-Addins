using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementFlatter.Util
{
    public static class LogManager
    {
        private static string logPath = "";
        private static List<LogMessageInfo> messageList = new List<LogMessageInfo>();

        public static string LogPath { get { return logPath; } set { logPath = value; } }
        public static List<LogMessageInfo> MessageList { get { return messageList; } set { messageList = value; } }

        public static void SetLogPath(Document doc)
        {
            try
            {
                if (doc.IsWorkshared)
                {
                    ModelPath centralModelPath = doc.GetWorksharingCentralModelPath();
                    if (null != centralModelPath)
                    {
                        string userVisiblePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(centralModelPath);
                        logPath = userVisiblePath.Replace(".rvt", ".log");
                    }
                }
                else
                {
                    logPath = doc.PathName.Replace(".rvt", ".log");
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void AppendLog(LogMessageType messageType, string message)
        {
            LogMessageInfo messageInfo = new LogMessageInfo() 
            { 
                MessageType = messageType ,
                Message = message
            };
            messageList.Add(messageInfo);
        }

        public static void AppendLog(LogMessageInfo messageInfo)
        {
            messageList.Add(messageInfo);
        }

        public static void ClearLog()
        {
            messageList.Clear();
        }

        public static bool WriteLog()
        {
            bool result = false;
            try
            {
                string[] logToAppend = null;
                if (File.Exists(logPath))
                {
                    logToAppend = File.ReadAllLines(logPath);
                }
                TimeSpan timeSpan = messageList[messageList.Count - 1].MessageTime.Subtract(messageList[0].MessageTime);
                messageList.Reverse();
                List<string> logMessages = new List<string>();
                logMessages.Add("=======================================================================================================================");
                logMessages.Add("Total Time: " + timeSpan.ToString());
                for (int i = 0; i < messageList.Count; i++)
                {
                    logMessages.Add(ConvertToString(messageList[i]));
                }
                
                if (null != logToAppend)
                {
                    logMessages.AddRange(logToAppend.ToList());
                }

                File.WriteAllLines(logPath, logMessages.ToArray());
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private static string ConvertToString(LogMessageInfo info)
        {
            string messageStr = "";
            try
            {
                messageStr = info.MessageTime.ToString() + "\t" + info.MessageType.ToString() + ": " + info.Message;
                if (info.RelatedElementIds.Count > 0)
                {
                    messageStr += "\tElement Ids: ";
                    foreach (ElementId elementId in info.RelatedElementIds)
                    {
                        messageStr += elementId.IntegerValue.ToString()+", ";
                    }
                }
               
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return messageStr;
        }
    }

    public class LogMessageInfo
    {
        private LogMessageType messageType = LogMessageType.NONE;
        private string message = "";
        private DateTime messageTime = DateTime.Now;
        private List<ElementId> elementIds = new List<ElementId>();

        public LogMessageType MessageType { get { return messageType; } set { messageType = value; } }
        public string Message { get { return message; } set { message = value; } }
        public DateTime MessageTime { get { return messageTime; } set { messageTime = value; } }
        public List<ElementId> RelatedElementIds { get { return elementIds; } set { elementIds = value; } }

        public LogMessageInfo()
        {
        }

        public LogMessageInfo(LogMessageType msgType, string msg, DateTime time, List<ElementId> eIds)
        {
            messageType = msgType;
            message = msg;
            messageTime = time;
            elementIds = eIds;
        }
    }

    public enum LogMessageType
    {
        NONE, INFO, WARNING, EXCEPTION, ERROR
    }
}
