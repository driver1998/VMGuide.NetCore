using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LnkParser
{
    internal static class Utils
    {
        
        public static string GetNullTerminatedString(byte[] bytes, int start)
        {
            int len = 0, i = start;
            int c = bytes[i];

            while (c != 0) {
                len++;
                i++;
                c = bytes[i];
            }

            return GetSystemEncoding().GetString(bytes, start, len);
        }

        public static string GetNullTerminatedUnicodeString(byte[] bytes, int start)
        {           
            int len = 0, i = start;
            int c = bytes[i] | (bytes[i+1] << 8);

            while (c != 0) {
                len += 2;
                i += 2;
                c = bytes[i] | (bytes[i+1] << 8);
            }

            return Encoding.Unicode.GetString(bytes, start, len);
        }

        public static Encoding GetSystemEncoding() {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Encoding.UTF8;

            Encoding encoding;
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                encoding = Encoding.GetEncoding(GetACP());
            }
            catch (ArgumentException)
            {
                encoding = Encoding.UTF8;
            }
            return encoding;
        }

        [DllImport("kernel32.dll")]
        private static extern Int32 GetACP();

        public static DateTime GetFileTime(byte[] bytes, int start)
            => new DateTime(1601, 1, 1).AddTicks(BitConverter.ToInt64(bytes, start));
    }
}