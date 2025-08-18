using System.ComponentModel.DataAnnotations;

namespace MklinkUi.WebUI;

public sealed class UiOptions
{
    [Range(100, 5000)]
    public int MaxCardWidth { get; set; } = 800;

    public bool EnableDragDrop { get; set; } = true;
}
