using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.Core.Utilities
{
    public static class Json
    {
        public static JsonSerializerOptions Options { get; set; }

        /// <summary>
        /// Static constructor, used to initialise settings.
        /// </summary>
        static Json()
        {

            Options = new JsonSerializerOptions
            {
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Serialize<T>(T data)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                };
                options.Converters.Add(new JsonStringEnumConverter());
                var jsonString = JsonSerializer.Serialize<T>(data, options);
                return jsonString;
            }
            catch (Exception)
            {
                throw new Exception("Failed to serialize object into a valid json string");
            }
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
                var options = new JsonSerializerOptions
                {
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                };
                options.Converters.Add(new JsonStringEnumConverter());
                var obj = JsonSerializer.Deserialize<T>(json, options);
                return obj;
            }
            catch (Exception)
            {
                throw new Exception("Failed to deserialize json string into a valid object.");
            }
        }
    }
}
