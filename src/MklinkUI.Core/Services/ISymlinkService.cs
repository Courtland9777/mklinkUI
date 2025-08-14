namespace MklinkUi.Windows;

public interface ISymlinkService
{
    bool CreateSymlink(string source, string target);
}
