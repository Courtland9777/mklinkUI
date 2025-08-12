using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MklinkUi.Core;

namespace MklinkUi.WebUI.Pages;

public sealed record SymlinkResultView(string Source, string Link, bool Success, string? ErrorMessage);

public sealed class IndexModel(
    SymlinkManager manager,
    IDeveloperModeService developerModeService,
    ILogger<IndexModel> logger) : PageModel
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

    public List<SymlinkResultView> Results { get; } = [];

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
                Results.Add(new SymlinkResultView(SourcePath, DestinationPath, false,
                    "Source and destination paths are required."));
                return Page();
            }

            try
            {
                var result = await manager.CreateSymlinkAsync(DestinationPath, SourcePath);
                Results.Add(new SymlinkResultView(SourcePath, DestinationPath, result.Success,
                    result.Success ? "OK" : result.ErrorMessage));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating symlink from {Source} to {Destination}", SourcePath,
                    DestinationPath);
                Results.Add(new SymlinkResultView(SourcePath, DestinationPath, false,
                    "An unexpected error occurred while creating the symlink."));
            }

            return Page();
        }

        var sourceFiles = SourceFilePaths
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (sourceFiles.Count == 0 || string.IsNullOrWhiteSpace(DestinationFolder))
        {
            Results.Add(new SymlinkResultView(string.Empty, DestinationFolder, false,
                "Select at least one source file and a destination folder."));
            return Page();
        }

        foreach (var path in sourceFiles)
        {
            var name = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Results.Add(new SymlinkResultView(path,
                    Path.Combine(DestinationFolder, name ?? string.Empty), false,
                    "One or more file names are invalid."));
                return Page();
            }

            if (!System.IO.File.Exists(path))
            {
                Results.Add(new SymlinkResultView(path,
                    Path.Combine(DestinationFolder, name), false,
                    $"Source file not found: {path}."));
                return Page();
            }
        }

        IReadOnlyList<SymlinkResult> results;
        try
        {
            results = await manager.CreateFileSymlinksAsync(sourceFiles, DestinationFolder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating file symlinks in {DestinationFolder}", DestinationFolder);
            Results.Add(new SymlinkResultView(string.Empty, DestinationFolder, false,
                "An unexpected error occurred while creating file symlinks."));
            return Page();
        }

        if (results.Count != sourceFiles.Count)
        {
            logger.LogError("Symlink result count {ResultCount} does not match source file count {SourceCount}",
                results.Count, sourceFiles.Count);
            Results.Add(new SymlinkResultView(string.Empty, DestinationFolder, false,
                "An unexpected error occurred while creating file symlinks."));
            return Page();
        }

        for (var i = 0; i < sourceFiles.Count; i++)
        {
            var source = sourceFiles[i];
            var link = Path.Combine(DestinationFolder, Path.GetFileName(source));
            var r = results[i];
            Results.Add(new SymlinkResultView(source, link, r.Success,
                r.Success ? "OK" : r.ErrorMessage));
        }

        return Page();
    }
}