using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MklinlUi.Core;

namespace MklinlUi.WebUI.Pages;

public class IndexModel : PageModel
{
    private readonly SymlinkManager _manager;
    private readonly IDeveloperModeService _developerModeService;

    public IndexModel(SymlinkManager manager, IDeveloperModeService developerModeService)
    {
        _manager = manager;
        _developerModeService = developerModeService;
    }

    [BindProperty]
    public string SourcePath { get; set; } = string.Empty;

    [BindProperty]
    public string DestinationPath { get; set; } = string.Empty;

    [BindProperty]
    public string LinkType { get; set; } = "File";

    public bool DeveloperModeEnabled { get; private set; }

    public string? Message { get; private set; }
    public bool? Success { get; private set; }

    public async Task OnGetAsync()
    {
        DeveloperModeEnabled = await _developerModeService.IsEnabledAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        DeveloperModeEnabled = await _developerModeService.IsEnabledAsync();
        var result = await _manager.CreateSymlinkAsync(DestinationPath, SourcePath);
        Success = result.Success;
        Message = result.Success ? "Symlink created successfully." : result.ErrorMessage;
        return Page();
    }
}
