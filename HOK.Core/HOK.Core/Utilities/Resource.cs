using System.IO;
using System.Reflection;

namespace HOK.Core.Utilities
{
    public static class Resources
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string StreamEmbeddedResource(string fileName, Assembly assembly = null)
        {
            if(assembly == null) assembly = Assembly.GetExecutingAssembly();

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
