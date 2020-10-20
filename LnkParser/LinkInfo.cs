using System;

namespace LnkParser
{
    public class LinkInfo        
    {
        public UInt32 LinkInfoFlags { get; }
        public VolumeID VolumeID { get; }
        public CommonNetworkRelativeLink CommonNetworkRelativeLink { get; }
        public string LocalBasePath { get; }
        public string CommonPathSuffix { get; }

        internal LinkInfo(byte[] bytes, int start)
        {
            var headerSize = BitConverter.ToUInt32(bytes, start + 4);
            var hasUnicode = (headerSize >= 0x24);

            LocalBasePath = "";
            CommonPathSuffix = "";
            LinkInfoFlags = BitConverter.ToUInt32(bytes, start + 8);
            
            if ((LinkInfoFlags & (UInt32)LinkInfoFlag.VolumeIDAndLocalBasePath) != 0) {

                // LocalBasePath
                if (hasUnicode) {
                    var offset = BitConverter.ToUInt32(bytes, start + 28);
                    LocalBasePath = Utils.GetNullTerminatedUnicodeString(bytes, start + (int)offset);
                } else {
                    var offset = BitConverter.ToUInt32(bytes, start + 16);
                    LocalBasePath = Utils.GetNullTerminatedString(bytes, start + (int)offset);
                }

                // VolumeID
                var volumeIdOffset = BitConverter.ToUInt32(bytes, start + 12);
                var volumeIdSize   = BitConverter.ToUInt32(bytes, start + (int)volumeIdOffset);
                VolumeID = new VolumeID(bytes, start + (int)volumeIdOffset);
            }

            if ((LinkInfoFlags & (UInt32)LinkInfoFlag.CommonNetworkRelativeLinkAndPathSuffix) != 0) {
                
                // CommonPathSuffix
                if (hasUnicode) {
                    var offset = BitConverter.ToUInt32(bytes, start + 32);
                    CommonPathSuffix = Utils.GetNullTerminatedUnicodeString(bytes, start + (int)offset);
                } else {
                    var offset = BitConverter.ToUInt32(bytes, start + 24);
                    CommonPathSuffix = Utils.GetNullTerminatedString(bytes, start + (int)offset);
                }

                // CommonNetworkRelativeLink
                var linkOffset = BitConverter.ToUInt32(bytes, start + 20);
                var linkSize   = BitConverter.ToUInt32(bytes, start + (int)linkOffset);
                CommonNetworkRelativeLink = new CommonNetworkRelativeLink(bytes, start + (int)linkOffset);
            }
        }
    }
}