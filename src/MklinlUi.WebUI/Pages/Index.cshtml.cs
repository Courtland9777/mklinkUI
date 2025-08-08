using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MklinlUi.Core;

namespace MklinlUi.WebUI.Pages;

public sealed class IndexModel(SymlinkManager manager, IDeveloperModeService developerModeService) : PageModel
{

    [BindProperty]
    public string LinkType { get; set; } = "File";

    [BindProperty, Required]
    public string SourcePath { get; set; } = string.Empty;

    [BindProperty, Required]
    public string DestinationPath { get; set; } = string.Empty;

    public bool DeveloperModeEnabled { get; private set; }

    public string? Message { get; private set; }
    public bool? Success { get; private set; }

    public async Task OnGetAsync()
    {
        DeveloperModeEnabled = await developerModeService.IsEnabledAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Success = false;
            Message = "Source and destination paths are required.";
            return Page();
        }

        DeveloperModeEnabled = await developerModeService.IsEnabledAsync();
        var result = await manager.CreateSymlinkAsync(DestinationPath, SourcePath);
        Success = result.Success;
        Message = result.Success ? "Symlink created successfully." : result.ErrorMessage;
        return Page();
    }
}
