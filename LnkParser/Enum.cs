namespace LnkParser
{
    public enum LinkFlag
    {
        HasTargetIdList = 1,
        HasLinkInfo     = 1 << 1,
        HasName         = 1 << 2,
        HasRelativePath = 1 << 3,
        HasWorkingDir   = 1 << 4,
        HasArguments    = 1 << 5,
        HasIconLocation = 1 << 6,
        IsUnicode       = 1 << 7,
        ForceNoLinkInfo = 1 << 8
    }

    public enum LinkInfoFlag
    {
        VolumeIDAndLocalBasePath = 1,
        CommonNetworkRelativeLinkAndPathSuffix = 1 << 1
    }

    public enum CommonNetworkRelativeLinkFlag
    {
        ValidDevice  = 1,
        ValidNetType = 1 << 1
    }
}