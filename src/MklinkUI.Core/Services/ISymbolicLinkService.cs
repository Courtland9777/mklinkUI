namespace MklinkUI.Core.Services;

public interface ISymbolicLinkService
{
    SymbolicLinkResult CreateSymbolicLink(string sourcePath, string destinationPath, bool isDirectory);
}

