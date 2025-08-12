using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MklinlUi.Core;
using System.IO;

namespace MklinlUi.WebUI.Pages;

public sealed class IndexModel(
    SymlinkManager manager,
    IDeveloperModeService developerModeService,
    ILogger<IndexModel> logger) : PageModel
{
    [BindProperty] public string LinkType { get; set; } = "File";

    [BindProperty] public List<IFormFile> SourceFiles { get; set; } = [];

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

            try
            {
                var result = await manager.CreateSymlinkAsync(DestinationPath, SourcePath);
                Success = result.Success;
                Message = result.Success ? "Symlink created successfully." : result.ErrorMessage;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating symlink from {Source} to {Destination}", SourcePath, DestinationPath);
                Success = false;
                Message = "An unexpected error occurred while creating the symlink.";
            }
            return Page();
        }

        var sourceFiles = new List<string>();
        foreach (var formFile in SourceFiles)
        {
            var name = Path.GetFileName(formFile.FileName);
            if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Success = false;
                Message = "One or more file names are invalid.";
                return Page();
            }

            if (!System.IO.File.Exists(formFile.FileName))
            {
                Success = false;
                Message = $"Source file not found: {formFile.FileName}.";
                return Page();
            }

            sourceFiles.Add(name);
        }

        if (sourceFiles.Count == 0 || string.IsNullOrWhiteSpace(DestinationFolder))
        {
            Success = false;
            Message = "Select at least one source file and a destination folder.";
            return Page();
        }

        IReadOnlyList<SymlinkResult> results;
        try
        {
            results = await manager.CreateFileSymlinksAsync(sourceFiles, DestinationFolder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating file symlinks in {DestinationFolder}", DestinationFolder);
            Success = false;
            Message = "An unexpected error occurred while creating file symlinks.";
            return Page();
        }

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