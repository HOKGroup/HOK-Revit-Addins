using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using HOK.AddInManager.Classes;
using HOK.Core.Utilities;

namespace HOK.AddInManager.Utils
{
    public static class SettingUtil
    {
        /// <summary>
        /// Reads XML Configuration File.
        /// </summary>
        /// <param name="xmlFile">File Path to XML file.</param>
        /// <param name="addins">List of Addins to be deserialized.</param>
        public static void ReadConfig(string xmlFile, ref Addins addins)
        {
            try
            {
                if (!File.Exists(xmlFile)) return;

                var serializer = new XmlSerializer(typeof(Addins));
                var fs = new FileStream(xmlFile, FileMode.Open);
                var reader = XmlReader.Create(fs);
                if (serializer.CanDeserialize(reader))
                {
                    var settingAddins = (Addins)serializer.Deserialize(reader);
                    for (var i = 0; i < addins.AddinCollection.Count; i++)
                    {
                        var info = addins.AddinCollection[i];
                        var settingFound = settingAddins.AddinCollection
                            .Where(x => x.ToolName == info.ToolName)
                            .ToList();
                        if (!settingFound.Any()) continue;

                        var settingInfo = settingFound.First();
                        addins.AddinCollection[i].ToolLoadType = settingInfo.ToolLoadType;
                    }
                }
                fs.Close();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Writes XML file containing Plugin information.
        /// </summary>
        /// <param name="xmlFile">File Path to XML file.</param>
        /// <param name="addins">List of Addins to be serialized.</param>
        public static void WriteConfig(string xmlFile, Addins addins)
        {
            try
            {
                using (var fs = new FileStream(xmlFile, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(Addins));
                    serializer.Serialize(fs, addins);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
