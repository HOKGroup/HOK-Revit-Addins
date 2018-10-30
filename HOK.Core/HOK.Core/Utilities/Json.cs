using System;
using Newtonsoft.Json;

namespace HOK.Core.Utilities
{
    public static class Json
    {
        public static JsonSerializerSettings Settings { get; set; }

        /// <summary>
        /// Static constructor, used to initialise settings.
        /// </summary>
        static Json()
        {
            Settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                CheckAdditionalContent = true,
                Formatting = Formatting.Indented
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json) where T : new()
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(json, Settings);
                return obj;
            }
            catch (Exception)
            {
                throw new Exception("Failed to deserialize json string into a valid object.");
            }
        }
    }
}
