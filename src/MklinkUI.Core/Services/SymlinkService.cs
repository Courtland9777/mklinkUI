namespace MklinkUi.Windows;

public class SymlinkService : ISymlinkService
{
    // Parameterless constructor required for dynamic loading
    public SymlinkService() { }

    public bool CreateSymlink(string source, string target)
    {
        // Placeholder implementation; real implementation will invoke OS specific APIs
        return false;
    }
}
