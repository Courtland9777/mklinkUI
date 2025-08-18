using System.ComponentModel.DataAnnotations;

namespace MklinkUi.WebUI;

public sealed class ServerOptions
{
    [RegularExpression("^\\d+-\\d+$")]
    public required string PreferredPortRange { get; set; }

    [Range(1, 65535)]
    public required int DefaultHttpPort { get; set; }

    [Range(1, 65535)]
    public required int DefaultHttpsPort { get; set; }
}
