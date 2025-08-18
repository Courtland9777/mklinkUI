using System.ComponentModel.DataAnnotations;

namespace MklinkUi.WebUI;

public sealed class ServerOptions
{
    [RegularExpression("^\\d+-\\d+$")]
    public string PreferredPortRange { get; set; } = "5280-5299";

    [Range(1, 65535)]
    public int DefaultHttpPort { get; set; } = 5280;

    [Range(1, 65535)]
    public int DefaultHttpsPort { get; set; } = 5281;
}
