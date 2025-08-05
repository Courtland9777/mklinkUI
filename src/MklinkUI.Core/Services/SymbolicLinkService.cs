using System.ComponentModel;
using Serilog;

namespace MklinkUI.Core.Services;

public class SymbolicLinkService : ISymbolicLinkService
{
    private readonly ISymbolicLink _symbolicLink;

    public SymbolicLinkService(ISymbolicLink symbolicLink)
    {
        _symbolicLink = symbolicLink;
    }

    public SymbolicLinkResult CreateSymbolicLink(string sourcePath, string destinationPath, bool isDirectory)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
        {
            const string message = "Source and destination paths must be provided.";
            Log.Error(message);
            return new SymbolicLinkResult(false, message);
        }

        try
        {
            var (success, error) = _symbolicLink.CreateSymbolicLink(destinationPath, sourcePath, isDirectory);
            if (success)
            {
                Log.Information("Created {Type} symbolic link: {Dest} -> {Source}", isDirectory ? "directory" : "file", destinationPath, sourcePath);
                return new SymbolicLinkResult(true, null);
            }

            var errorMessage = new Win32Exception(error).Message;
            Log.Error("Failed to create symbolic link {Dest} -> {Source}: {ErrorMessage}", destinationPath, sourcePath, errorMessage);
            return new SymbolicLinkResult(false, errorMessage);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception while creating symbolic link {Dest} -> {Source}", destinationPath, sourcePath);
            return new SymbolicLinkResult(false, ex.Message);
        }
    }
}

