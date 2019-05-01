
namespace HOK.MissionControl.Tools.Communicator.Messaging
{
    public enum DataType
    {
        Sheets,
        Families
    }

    /// <summary>
    /// Message that gets called when Families or Sheets data was downloaded.
    /// It's used to populate the Communicator UI.
    /// </summary>
    public class CommunicatorDataDownloaded
    {
        public DataType Type { get; set; }
        public string CentralPath { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CentralPathObtained
    {
        public string CentralPath { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DocumentClosed
    {
        public bool CloseWindow { get; set; }
    }
}
