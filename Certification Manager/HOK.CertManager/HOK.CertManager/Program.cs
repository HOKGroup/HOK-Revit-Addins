using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HOK.CertManager
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                X509Certificate2 certificate = new X509Certificate2(@"hokaddin.pfx", "hokaddin", X509KeyStorageFlags.DefaultKeySet);
                X509Store store = new X509Store(StoreName.TrustedPublisher, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                store.Close();

                X509Store rootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                rootStore.Open(OpenFlags.ReadWrite);
                rootStore.Add(certificate);
                rootStore.Close();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
