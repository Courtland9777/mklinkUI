namespace MklinkUI.Core.Services;

public interface ISymbolicLink
{
    (bool Success, int ErrorCode) CreateSymbolicLink(string linkPath, string targetPath, bool isDirectory);
}

