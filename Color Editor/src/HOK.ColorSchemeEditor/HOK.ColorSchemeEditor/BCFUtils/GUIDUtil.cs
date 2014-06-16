using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using Autodesk.Revit.DB;

namespace HOK.ColorSchemeEditor.BCFUtils
{
    public static class GUIDUtil
    {
        private static readonly char[] base64Chars = new char[]
        { '0','1','2','3','4','5','6','7','8','9'
        , 'A','B','C','D','E','F','G','H','I','J'
        , 'K','L','M','N','O','P','Q','R','S','T'
        , 'U','V','W','X','Y','Z','a','b','c','d'
        , 'e','f','g','h','i','j','k','l','m','n'
        , 'o','p','q','r','s','t','u','v','w','x'
        , 'y','z','_','$' };

        public static string CreateGUID(Element element)
        {
            string ifcGUID = "";
            try
            {
                BuiltInParameter builtInParameter = (element is ElementType) ? BuiltInParameter.IFC_TYPE_GUID : BuiltInParameter.IFC_GUID;
                ifcGUID = CreateGUIDBase(element, builtInParameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create GUID from an element.\n" + ex.Message, "Create GUID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return ifcGUID;
        }

        static private string CreateGUIDBase(Element element, BuiltInParameter parameterName)
        {

            string ifcGUID = null;
            try
            {
                Parameter param = element.get_Parameter(parameterName);
                if (null != param)
                {
                    ifcGUID = param.AsString();
                }

                if (string.IsNullOrEmpty(ifcGUID))
                {
                    Guid guid = ConvertToGUID(element);
                    ifcGUID = ConvertToIfcGuid(guid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create GUID based on element ID.\n" + ex.Message, "CreateGUIDBase", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return ifcGUID;
        }

        //Get the UniqueID in encoded on .NET GUID format
        public static Guid ConvertToGUID(this Autodesk.Revit.DB.Element element)
        {
            Guid guid = new Guid();
            try
            {
                string a = element.UniqueId;
                Guid episodeId = new Guid(a.Substring(0, 36));
                int elementId = int.Parse(a.Substring(37), NumberStyles.AllowHexSpecifier);
                int last_32_bits = int.Parse(a.Substring(28, 8), NumberStyles.AllowHexSpecifier);
                int xor = last_32_bits ^ elementId;
                a = a.Substring(0, 28) + xor.ToString("x8");
                guid = new Guid(a);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to convert uniqueId to GUID.\n" + ex.Message, "ConvertToGUID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return guid;
        }

        //Conversion of a GUID to a IfcGUID
        public static string ConvertToIfcGuid(Guid guid)
        {
            uint[] num = new uint[6];
            char[] str = new char[22];
            int i, n;
            byte[] b = guid.ToByteArray();

            // Creation of six 32 Bit integers from the components of the GUID structure
            num[0] = (uint)(BitConverter.ToUInt32(b, 0) / 16777216);
            num[1] = (uint)(BitConverter.ToUInt32(b, 0) % 16777216);
            num[2] = (uint)(BitConverter.ToUInt16(b, 4) * 256 + BitConverter.ToInt16(b, 6) / 256);
            num[3] = (uint)((BitConverter.ToUInt16(b, 6) % 256) * 65536 + b[8] * 256 + b[9]);
            num[4] = (uint)(b[10] * 65536 + b[11] * 256 + b[12]);
            num[5] = (uint)(b[13] * 65536 + b[14] * 256 + b[15]);

            // Conversion of the numbers into a system using a base of 64
            n = 2;
            int pos = 0;
            for (i = 0; i < 6; i++)
            {
                cv_to_64(num[i], ref str, pos, n);
                pos += n; n = 4;
            }
            return new String(str);
        }

        //Conversion of a IfcGUID to a GUID
        public static Guid FromIfcGUID(string guid)
        {
            Debug.Assert(guid.Length == 22, "Input string must not be longer that 22 chars");
            uint[] num = new uint[6];
            char[] str = guid.ToCharArray();
            int n = 2, pos = 0, i;
            for (i = 0; i < 6; i++)
            {
                num[i] = cv_from_64(str, pos, n);
                pos += n; n = 4;
            }

            int a = (int)((num[0] * 16777216 + num[1]));
            short b = (short)(num[2] / 256);
            short c = (short)((num[2] % 256) * 256 + num[3] / 65536);
            byte[] d = new byte[8];
            d[0] = Convert.ToByte((num[3] / 256) % 256);
            d[1] = Convert.ToByte(num[3] % 256);
            d[2] = Convert.ToByte(num[4] / 65536);
            d[3] = Convert.ToByte((num[4] / 256) % 256);
            d[4] = Convert.ToByte(num[4] % 256);
            d[5] = Convert.ToByte(num[5] / 65536);
            d[6] = Convert.ToByte((num[5] / 256) % 256);
            d[7] = Convert.ToByte(num[5] % 256);

            return new Guid(a, b, c, d);
        }

        //Conversion of an integer into characters
        static void cv_to_64(uint number, ref char[] result, int start, int len)
        {
            uint act;
            int iDigit, nDigits;

            Debug.Assert(len <= 4);
            act = number;
            nDigits = len;

            for (iDigit = 0; iDigit < nDigits; iDigit++)
            {
                result[start + len - iDigit - 1] = base64Chars[(int)(act % 64)];
                act /= 64;
            }
            Debug.Assert(act == 0, "Logic failed, act was not null: " + act.ToString());
            return;
        }

        //the number from the characters
        static uint cv_from_64(char[] str, int start, int len)
        {
            int i, j, index;
            uint res = 0;
            Debug.Assert(len <= 4);

            for (i = 0; i < len; i++)
            {
                index = -1;
                for (j = 0; j < 64; j++)
                {
                    if (base64Chars[j] == str[start + i])
                    {
                        index = j;
                        break;
                    }
                }
                Debug.Assert(index >= 0);
                res = res * 64 + ((uint)index);
            }
            return res;
        }

        static public bool IsValidIFCGUID(string guid)
        {
            if (guid == null)
                return false;

            if (guid.Length != 22)
                return false;

            foreach (char guidChar in guid)
            {
                if ((guidChar >= '0' && guidChar <= '9') ||
                    (guidChar >= 'A' && guidChar <= 'Z') ||
                    (guidChar >= 'a' && guidChar <= 'z') ||
                    (guidChar == '_' || guidChar == '$'))
                    continue;

                return false;
            }

            return true;
        }
    }
}
