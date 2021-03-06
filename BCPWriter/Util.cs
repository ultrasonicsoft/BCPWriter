﻿namespace BCPWriter
{
    using System;
    using System.Text;

    /// <summary>
    /// Internal utility class.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Encode text using OEM code page, see <a href="http://en.wikipedia.org/wiki/Windows_code_page">Windows code page</a>.
        /// </summary>
        /// <remarks>
        /// When we use SQLChar and SQLVarChar unicode is not used and text is encoded using OEM code page.
        /// </remarks>
        /// <param name="text">Text to encode.</param>
        /// <returns>Text encoded using OEM code page.</returns>
        public static byte[] EncodeToOEMCodePage(string text)
        {
            Encoding enc = Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            return enc.GetBytes(text);
        }

        /// <summary>
        /// Converts a byte[] to hexadecimal.
        /// </summary>
        /// <remarks>
        /// See <a href="http://stackoverflow.com/questions/623104/c-byte-to-hex-string">byte[] to hex string</a>
        /// </remarks>
        /// <param name="data">Data to convert.</param>
        /// <returns>String containing hexadecimal.</returns>
        public static string ToHexString(byte[] data)
        {
            /*StringBuilder hex = new StringBuilder();
            foreach (byte b in data)
            {
                hex.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:x2}", b);
            }
            return hex.ToString();*/

            return BitConverter.ToString(data).Replace("-", string.Empty);
        }

        /// <summary>
        /// Converts a string to a byte[].
        /// </summary>
        /// <param name="text">Text to convert.</param>
        public static byte[] StringToByteArray(string text)
        {
            return Encoding.Default.GetBytes(text);
        }

        /// <summary>
        /// See <a href="http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa-in-c">How do you convert Byte Array to Hexadecimal String, and vice versa, in C#?</a>
        /// </summary>
        public static byte[] HexToByteArray(string hex)
        {
            int nbChars = hex.Length;
            byte[] bytes = new byte[nbChars / 2];
            for (int i = 0; i < nbChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
