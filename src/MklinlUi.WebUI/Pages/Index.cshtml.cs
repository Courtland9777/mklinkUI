using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MklinlUi.Core;

namespace MklinlUi.WebUI.Pages;

public sealed class IndexModel(SymlinkManager manager, IDeveloperModeService developerModeService) : PageModel
{

    [BindProperty]
    public string LinkType { get; set; } = "File";

    [BindProperty]
    public string SourceFilesInput { get; set; } = string.Empty;

    [BindProperty]
    public string DestinationFolder { get; set; } = string.Empty;

    [BindProperty]
    public string SourcePath { get; set; } = string.Empty;

    [BindProperty]
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
        DeveloperModeEnabled = await developerModeService.IsEnabledAsync();

        if (LinkType == "Folder")
        {
            if (string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
            {
                Success = false;
                Message = "Source and destination paths are required.";
                return Page();
            }
            var result = await manager.CreateSymlinkAsync(DestinationPath, SourcePath);
            Success = result.Success;
            Message = result.Success ? "Symlink created successfully." : result.ErrorMessage;
            return Page();
        }
        else
        {
            var SourceFiles = SourceFilesInput
                .Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            if (SourceFiles.Count == 0 || string.IsNullOrWhiteSpace(DestinationFolder))
            {
                Success = false;
                Message = "Select at least one source file and a destination folder.";
                return Page();
            }

            var results = await manager.CreateFileSymlinksAsync(SourceFiles, DestinationFolder);
            Success = results.All(r => r.Success);
            Message = string.Join("\n", SourceFiles.Select((s, i) =>
            {
                var link = Path.Combine(DestinationFolder, Path.GetFileName(s));
                var r = results[Math.Min(i, results.Count - 1)];
                return $"{s} -> {link}: {(r.Success ? "OK" : r.ErrorMessage)}";
            }));
            return Page();
        }
    }
}
