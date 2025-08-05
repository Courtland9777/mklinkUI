namespace MklinkUI.Core.Services;

public interface IRegistry
{
    object? GetValue(string keyName, string valueName, object? defaultValue);
}
