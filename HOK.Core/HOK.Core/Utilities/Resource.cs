using System.IO;
using System.Reflection;

namespace HOK.Core.Utilities
{
    public static class Resources
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string StreamEmbeddedResource(Assembly assembly, string fileName)
        {
            using (var stream = assembly.GetManifestResourceStream(fileName))
            {
                if (stream == null) return string.Empty;

                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    return result;
                }
            }
        }
    }
}
