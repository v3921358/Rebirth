using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Constants
    {
        public static readonly Random Rand = new Random();

        public const ushort Version = 95;

        public static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");

            return byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

        public static string GetString(byte[] buffer)
        {
            return BitConverter.ToString(buffer).Replace("-", " ");
        }

        public static byte[] GetBytes(string hexString)
        {
            string newString = hexString
                .Replace(" ", "")
                .Replace("-", "");

            // if odd number of characters, discard last character
            if (newString.Length % 2 != 0)
            {
                newString = newString.Substring(0, newString.Length - 1);
            }

            int byteLength = newString.Length / 2;
            byte[] bytes = new byte[byteLength];
            string hex;
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new Char[] { newString[j], newString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }
            return bytes;
        }
    }
}
