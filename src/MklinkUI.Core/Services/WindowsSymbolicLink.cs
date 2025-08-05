using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MklinkUI.Core.Services;

/// <summary>
/// Windows-specific symbolic link implementation using kernel32 APIs.
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsSymbolicLink : ISymbolicLink
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLinkFlag dwFlags);

    public (bool Success, int ErrorCode) CreateSymbolicLink(string linkPath, string targetPath, bool isDirectory)
    {
        var flags = isDirectory ? SymbolicLinkFlag.Directory : SymbolicLinkFlag.File;
        var result = CreateSymbolicLink(linkPath, targetPath, flags);
        var error = result ? 0 : Marshal.GetLastWin32Error();
        return (result, error);
    }

    private enum SymbolicLinkFlag : uint
    {
        File = 0,
        Directory = 1
    }
}

