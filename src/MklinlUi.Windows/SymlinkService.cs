using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using MklinlUi.Core;

namespace MklinlUi.Windows;

/// <summary>
/// Windows implementation of <see cref="ISymlinkService"/>.
/// </summary>
public class SymlinkService : ISymlinkService
{
    public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath, CancellationToken cancellationToken = default)
    {
        var flags = Directory.Exists(targetPath) ? SymbolicLinkFlag.Directory : SymbolicLinkFlag.File;
        if (CreateSymbolicLink(linkPath, targetPath, flags))
        {
            return Task.FromResult(new SymlinkResult(true));
        }

        var error = Marshal.GetLastWin32Error();
        var message = new Win32Exception(error).Message;
        return Task.FromResult(new SymlinkResult(false, message));
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLinkFlag dwFlags);

    private enum SymbolicLinkFlag : uint
    {
        File = 0,
        Directory = 1
    }
}
