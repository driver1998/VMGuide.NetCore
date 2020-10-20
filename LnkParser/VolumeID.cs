using System;

namespace LnkParser
{
    public class VolumeID {
        public UInt32 DriveType { get; }
        public UInt32 DriveSerialNumber { get; }
        public string VolumeLabel { get; }

        internal VolumeID(byte[] bytes, int start) {
            DriveType = BitConverter.ToUInt32(bytes, start + 4);
            DriveSerialNumber = BitConverter.ToUInt32(bytes, start + 8);

            var labelOffset = BitConverter.ToUInt32(bytes, start + 12);
            var hasUnicode = (labelOffset == 0x14);

            if (hasUnicode) {
                labelOffset = BitConverter.ToUInt32(bytes, start + 16);
                VolumeLabel = Utils.GetNullTerminatedUnicodeString(bytes, start + (int)labelOffset);
            } else {
                VolumeLabel = Utils.GetNullTerminatedString(bytes, start + (int)labelOffset);
            }
        }
    }
}