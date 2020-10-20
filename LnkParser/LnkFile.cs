using System;
using System.IO;
using System.Text;
namespace LnkParser
{
    public class LnkFile
    {
        public string Path { get; }
        public string TargetLocation
        { 
            get
            {
                if (LinkInfo is null) return "";

                if ((LinkInfo.LinkInfoFlags & (UInt32)LinkInfoFlag.VolumeIDAndLocalBasePath) != 0) {
                    return System.IO.Path.Combine(
                        LinkInfo.LocalBasePath, LinkInfo.CommonPathSuffix);
                }

                if ((LinkInfo.LinkInfoFlags & (UInt32)LinkInfoFlag.CommonNetworkRelativeLinkAndPathSuffix) != 0 ) {
                    if (LinkInfo.LocalBasePath == "")
                        return System.IO.Path.Combine(
                            LinkInfo.CommonNetworkRelativeLink.NetName, LinkInfo.CommonPathSuffix);
                    else
                        return System.IO.Path.Combine(
                            LinkInfo.LocalBasePath, LinkInfo.CommonPathSuffix);
                }

                return "";
            }
        }
        public ShellLinkHeader ShellLinkHeader { get; }
        public LinkInfo LinkInfo { get; }
        
        public string Name { get; }
        public string RelativePath { get; }
        public string WorkingDirectory { get; }
        public string Arguments { get; }
        public string IconLocation { get; }

        public LnkFile(string path)
        {
            Path = path;            
            var bytes = File.ReadAllBytes(Path);
            int offset = 0;

            // ShellLinkHeader
            ShellLinkHeader = new ShellLinkHeader(bytes, offset);
            offset += 76;

            // TODO: Shell LinkID Items
            if ((ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.HasTargetIdList) != 0) {
                var idListSize = BitConverter.ToUInt16(bytes, offset);
                offset += 2 + idListSize;
            }

            // LinkInfo
            var hasLinkInfo     = (ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.HasLinkInfo) != 0;
            var forceNoLinkInfo = (ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.ForceNoLinkInfo) != 0;
            if (hasLinkInfo) {
                var linkInfoSize = BitConverter.ToUInt32(bytes, offset);
                if (!forceNoLinkInfo) LinkInfo = new LinkInfo(bytes, offset);
                offset += (int)linkInfoSize;
            }

            var isUnicode = (ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.IsUnicode) != 0;
            
            // StringData: Name
            if ((ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.HasName) != 0) {
                Name = GetStringData(bytes, ref offset, isUnicode);
            }
            
            // StringData: RelativePath
            if ((ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.HasRelativePath) != 0) {
                RelativePath = GetStringData(bytes, ref offset, isUnicode);
            }
            
            // StringData: WorkingDirectory
            if ((ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.HasWorkingDir) != 0) {
                WorkingDirectory = GetStringData(bytes, ref offset, isUnicode);
            }
            
            // StringData: Arguments
            if ((ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.HasArguments) != 0) {
                Arguments = GetStringData(bytes, ref offset, isUnicode);
            }

            // StringData: IconLocation
            if ((ShellLinkHeader.LinkFlags & (UInt32)LinkFlag.HasIconLocation) != 0) {
                IconLocation = GetStringData(bytes, ref offset, isUnicode);
            }
        }

        private string GetStringData(byte[] bytes, ref int offset, bool isUnicode)
        {
            var len = BitConverter.ToUInt16(bytes, offset);
            var size = isUnicode? len*2 : len;
            offset += 2;

            string str;
            if (isUnicode)
                str = Encoding.Unicode.GetString(bytes, offset, size);
            else
                str = Utils.GetSystemEncoding().GetString(bytes, offset, size);

            offset += size;
            return str;
        }
    }
}
