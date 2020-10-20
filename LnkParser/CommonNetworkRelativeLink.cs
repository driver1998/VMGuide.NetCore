using System;

namespace LnkParser
{
    public class CommonNetworkRelativeLink
    {
        public UInt32 CommonNetworkRelativeLinkFlags { get; }
        public UInt32 NetworkProviderType { get; }
        public string NetName { get; }
        public string DeviceName { get; }

        internal CommonNetworkRelativeLink(byte[] bytes, int start)
        {
            CommonNetworkRelativeLinkFlags = BitConverter.ToUInt32(bytes, start + 4);

            var netNameOffset = BitConverter.ToUInt32(bytes, start + 8);
            var hasUnicode = (netNameOffset > 0x14);
            
            if (hasUnicode) {
                netNameOffset = BitConverter.ToUInt32(bytes, start + 20);
                NetName = Utils.GetNullTerminatedUnicodeString(bytes, start + (int)netNameOffset);
            } else {
                NetName = Utils.GetNullTerminatedString(bytes, start + (int)netNameOffset);
            }
            
            if ((CommonNetworkRelativeLinkFlags & (int)CommonNetworkRelativeLinkFlag.ValidDevice) != 0) {
                if (hasUnicode) {
                    var devNameOffset = BitConverter.ToUInt32(bytes, start + 24);
                    DeviceName = Utils.GetNullTerminatedUnicodeString(bytes, start + (int)devNameOffset);
                } else {
                    var devNameOffset = BitConverter.ToUInt32(bytes, start + 12);
                    DeviceName = Utils.GetNullTerminatedString(bytes, start + (int)devNameOffset);
                }
            } else {
                DeviceName = "";
            }

            if ((CommonNetworkRelativeLinkFlags & (int)CommonNetworkRelativeLinkFlag.ValidNetType) != 0)
                NetworkProviderType = BitConverter.ToUInt32(bytes, start + 16);
        }
    }
}