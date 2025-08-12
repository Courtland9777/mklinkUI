using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MklinlUi.Core;
using System.IO;

namespace MklinlUi.WebUI.Pages;

public sealed class IndexModel(SymlinkManager manager, IDeveloperModeService developerModeService) : PageModel
{
    [BindProperty] public string LinkType { get; set; } = "File";

    /// <summary>
    ///     Full paths for files to link, provided as newline-separated values.
    /// </summary>
    [BindProperty]
    public string SourceFilePaths { get; set; } = string.Empty;

    [BindProperty] public string DestinationFolder { get; set; } = string.Empty;

    [BindProperty] public string SourcePath { get; set; } = string.Empty;

    [BindProperty] public string DestinationPath { get; set; } = string.Empty;

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

        var sourceFiles = SourceFilePaths
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (sourceFiles.Count == 0 || string.IsNullOrWhiteSpace(DestinationFolder))
        {
            Success = false;
            Message = "Select at least one source file and a destination folder.";
            return Page();
        }

        foreach (var path in sourceFiles)
        {
            var name = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Success = false;
                Message = "One or more file names are invalid.";
                return Page();
            }

            if (!System.IO.File.Exists(path))
            {
                Success = false;
                Message = $"Source file not found: {path}.";
                return Page();
            }
        }

        var results = await manager.CreateFileSymlinksAsync(sourceFiles, DestinationFolder);
        Success = results.All(r => r.Success);
        Message = string.Join("\n", sourceFiles.Select((s, i) =>
        {
            var link = Path.Combine(DestinationFolder, Path.GetFileName(s));
            var r = results[Math.Min(i, results.Count - 1)];
            return $"{s} -> {link}: {(r.Success ? "OK" : r.ErrorMessage)}";
        }));
        return Page();
    }
}