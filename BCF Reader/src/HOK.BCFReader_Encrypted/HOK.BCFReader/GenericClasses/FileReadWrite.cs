using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace HOK.BCFReader.GenericClasses
{
    class FileReadWrite
    {
        // Key for TripleDES encryption
        public static byte[] key = { 21, 10, 64, 10, 100, 40, 200, 4,
                    21, 54, 65, 246, 5, 62, 1, 54,
                    54, 6, 8, 9, 65, 4, 65, 9};

        private static byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0 };

        public static string ReadFile(string FilePath)
        {
            FileInfo fi = new FileInfo(FilePath);
            if (fi.Exists == false)
                return string.Empty;

            FileStream fin = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            TripleDES tdes = new TripleDESCryptoServiceProvider();
            CryptoStream cs = new CryptoStream(fin, tdes.CreateDecryptor(key, iv), CryptoStreamMode.Read);

            StringBuilder SB = new StringBuilder();
            int ch;
            for (int i = 0; i < fin.Length; i++)
            {
                ch = cs.ReadByte();
                if (ch == 0)
                    break;
                SB.Append(Convert.ToChar(ch));
            }

            cs.Close();
            fin.Close();
            return SB.ToString();
        }

        public static void WriteFile(string FilePath, string Data)
        {
            FileStream fout = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write);
            TripleDES tdes = new TripleDESCryptoServiceProvider();
            CryptoStream cs = new CryptoStream(fout, tdes.CreateEncryptor(key, iv), CryptoStreamMode.Write);

            byte[] d = Encoding.ASCII.GetBytes(Data);
            cs.Write(d, 0, d.Length);
            cs.WriteByte(0);

            cs.Close();
            fout.Close();
        }
    }
}
