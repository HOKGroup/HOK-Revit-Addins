using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ARUP.IssueTracker.Classes
{
    /// <summary>
    /// used for encryption and decryption
    /// </summary>
    public static class DataProtector
    {
        private const string EntropyValue = "secret";

        /// <summary>
        /// Encrypts a string using the DPAPI.
        /// </summary>
        /// <param name="stringToEncrypt">The string to encrypt.</param>
        /// <returns>The encrypted data.</returns>
        public static string EncryptData(string stringToEncrypt)
        {
            if (string.IsNullOrWhiteSpace(stringToEncrypt))
                return string.Empty;
            byte[] encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(stringToEncrypt), Encoding.Unicode.GetBytes(EntropyValue), DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts a string using the DPAPI.
        /// </summary>
        /// <param name="stringToDecrypt">The string to decrypt.</param>
        /// <returns>The decrypted data.</returns>
        public static string DecryptData(string stringToDecrypt)
        {
            if (!string.IsNullOrWhiteSpace(stringToDecrypt) && IsBase64String(stringToDecrypt))
            {
                try
                {
                    byte[] decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(stringToDecrypt), Encoding.Unicode.GetBytes(EntropyValue), DataProtectionScope.LocalMachine);
                    return Encoding.Unicode.GetString(decryptedData);
                }
                catch
                {
                    
                }
            }
            return string.Empty;
        }
        public static bool IsBase64String(this string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }
    }
}


