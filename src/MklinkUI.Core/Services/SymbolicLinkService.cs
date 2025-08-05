using System.ComponentModel;
using System.IO;
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
            return new SymbolicLinkResult(false, message, 0);
        }

        if (!Path.IsPathFullyQualified(sourcePath) || !Path.IsPathFullyQualified(destinationPath))
        {
            const string message = "Paths must be absolute.";
            Log.Error(message);
            return new SymbolicLinkResult(false, message, 0);
        }

        if (isDirectory)
        {
            if (!Directory.Exists(sourcePath))
            {
                var message = $"Source directory '{sourcePath}' does not exist.";
                Log.Error(message);
                return new SymbolicLinkResult(false, message, 0);
            }
            if (Directory.Exists(destinationPath) || File.Exists(destinationPath))
            {
                var message = $"Destination '{destinationPath}' already exists.";
                Log.Error(message);
                return new SymbolicLinkResult(false, message, 0);
            }
        }
        else
        {
            if (!File.Exists(sourcePath))
            {
                var message = $"Source file '{sourcePath}' does not exist.";
                Log.Error(message);
                return new SymbolicLinkResult(false, message, 0);
            }
            if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
            {
                var message = $"Destination '{destinationPath}' already exists.";
                Log.Error(message);
                return new SymbolicLinkResult(false, message, 0);
            }
        }

        try
        {
            var (success, error) = _symbolicLink.CreateSymbolicLink(destinationPath, sourcePath, isDirectory);
            if (success)
            {
                Log.Information("Created {Type} symbolic link: {Dest} -> {Source}", isDirectory ? "directory" : "file", destinationPath, sourcePath);
                return new SymbolicLinkResult(true, null, 0);
            }

            var errorMessage = new Win32Exception(error).Message;
            Log.Error("Failed to create symbolic link {Dest} -> {Source}: {ErrorMessage}", destinationPath, sourcePath, errorMessage);
            return new SymbolicLinkResult(false, errorMessage, error);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception while creating symbolic link {Dest} -> {Source}", destinationPath, sourcePath);
            return new SymbolicLinkResult(false, ex.Message, 0);
        }
    }
}

