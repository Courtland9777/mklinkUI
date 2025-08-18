using System.ComponentModel.DataAnnotations;

namespace MklinkUi.Core;

public enum CollisionPolicy
{
    Skip,
    Overwrite,
    Rename
}

public sealed class SymlinkOptions
{
    public CollisionPolicy CollisionPolicy { get; set; } = CollisionPolicy.Skip;

    [Range(1, int.MaxValue)]
    public int BatchMax { get; set; } = 100;
}
