using HOK.AddInManager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HOK.AddInManager.Utils
{
    public static class SettingUtil
    {
        public static void ReadConfig(string xmlFile, ref Addins addins)
        {
            try
            {
                if (File.Exists(xmlFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Addins));
                    FileStream fs = new FileStream(xmlFile, FileMode.Open);
                    XmlReader reader = XmlReader.Create(fs);
                    if (serializer.CanDeserialize(reader))
                    {
                        Addins settingAddins = (Addins)serializer.Deserialize(reader);
                        for (int i = 0; i < addins.AddinCollection.Count; i++)
                        {
                            AddinInfo info = addins.AddinCollection[i];
                            var settingFound = from settingAddin in settingAddins.AddinCollection where settingAddin.ToolName == info.ToolName select settingAddin;
                            if (settingFound.Count() > 0)
                            {
                                AddinInfo settingInfo = settingFound.First();
                                addins.AddinCollection[i].ToolLoadType = settingInfo.ToolLoadType;
                            }
                        }
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void WriteConfig(string xmlFile, Addins addins)
        {
            try
            {
                using (FileStream fs = new FileStream(xmlFile, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Addins));
                    serializer.Serialize(fs, addins);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
