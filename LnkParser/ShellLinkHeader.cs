using System;
using System.IO;

namespace LnkParser
{
    public class ShellLinkHeader
    {
        public UInt32 LinkFlags { get; }
        public UInt32 FileAttributes { get; }
        public DateTime CreationTime { get; }
        public DateTime AccessTime { get; }
        public DateTime WriteTime { get; }
        public UInt32 FileSize { get; }
        public Int32 IconIndex { get; }
        public UInt32 ShowCommand { get; }
        public UInt16 HotKey { get; }

        private readonly Guid LinkCLSID = Guid.Parse("00021401-0000-0000-c000-000000000046");

        internal ShellLinkHeader(byte[] bytes, int start)
        {
            // Check HeaderSize and LinkCLSID
            var headerSize = BitConverter.ToUInt32(bytes, start);
            var idBytes = new byte[16];
            Buffer.BlockCopy(bytes, start + 4, idBytes, 0, 16);
            var id = new Guid(idBytes);
            if (headerSize != 76 || id != LinkCLSID) throw new InvalidDataException();

            LinkFlags = BitConverter.ToUInt32(bytes, start + 20);
            FileAttributes = BitConverter.ToUInt32(bytes, start + 24);
            CreationTime = Utils.GetFileTime(bytes, start + 28);
            AccessTime = Utils.GetFileTime(bytes, start + 36);
            WriteTime = Utils.GetFileTime(bytes, start + 44);
            FileSize = BitConverter.ToUInt32(bytes, start + 52);
            IconIndex = BitConverter.ToInt32(bytes, start + 56);
            ShowCommand = BitConverter.ToUInt32(bytes, start + 60);
            HotKey = BitConverter.ToUInt16(bytes, start + 64);
        }
    }
}