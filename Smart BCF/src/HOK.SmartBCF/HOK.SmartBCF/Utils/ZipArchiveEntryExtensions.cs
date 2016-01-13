using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.Utils
{
    public static class ZipArchiveEntryExtensions
    {
        public static string ExtractGuidFolderName(this ZipArchiveEntry entry)
        {
            string guidStr = "";
            try
            {
                if (entry.FullName.Length > 36)
                {
                    Guid guid = Guid.Empty;
                    if (entry.FullName.Contains("/"))
                    {
                        if (Guid.TryParse(entry.FullName.Substring(entry.FullName.LastIndexOf('/') - 36, 36), out guid))
                        {
                            guidStr = guid.ToString();
                        }
                    }
                    else if (entry.FullName.Contains("\\"))
                    {
                        if (Guid.TryParse(entry.FullName.Substring(entry.FullName.LastIndexOf('\\') - 36, 36), out guid))
                        {
                            guidStr = guid.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return guidStr;
        }
    }
}
