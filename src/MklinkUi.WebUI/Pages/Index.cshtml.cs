using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using MklinkUi.Core;

namespace MklinkUi.WebUI.Pages;

public sealed record SymlinkResultView(string Source, string Link, bool Success, string? ErrorMessage);

public sealed class IndexModel(
    SymlinkManager manager,
    IHostEnvironment environment,
    ILogger<IndexModel> logger) : PageModel
{
    private static readonly char[] NewLineSeparators = ['\r', '\n'];
    [BindProperty] public string LinkType { get; set; } = "File";

    [BindProperty]
    public string SourceFile { get; set; } = string.Empty;

    [BindProperty]
    public string SourceFolders { get; set; } = string.Empty;

    [BindProperty]
    public string DestinationFolder { get; set; } = string.Empty;

    public bool DeveloperModeEnabled { get; private set; }

    public List<SymlinkResultView> Results { get; } = [];

    public Task OnGetAsync()
    {
        DeveloperModeEnabled = environment.IsDevelopment();
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        DeveloperModeEnabled = environment.IsDevelopment();

        if (LinkType == "File")
        {
            if (string.IsNullOrWhiteSpace(SourceFile) || string.IsNullOrWhiteSpace(DestinationFolder))
            {
                Results.Add(new SymlinkResultView(SourceFile, DestinationFolder, false,
                    "Select a source file and a destination folder."));
                return Page();
            }

            if (!PathHelpers.AreFullyQualified(SourceFile, DestinationFolder))
            {
                Results.Add(new SymlinkResultView(SourceFile, DestinationFolder, false,
                    "Paths must be absolute."));
                return Page();
            }

            if (!System.IO.File.Exists(SourceFile))
            {
                Results.Add(new SymlinkResultView(SourceFile, DestinationFolder, false,
                    $"Source file not found: {SourceFile}."));
                return Page();
            }

            try
            {
                var result = await manager.CreateFileLinkAsync(SourceFile, DestinationFolder);
                var link = Path.Combine(DestinationFolder, Path.GetFileName(SourceFile));
                Results.Add(new SymlinkResultView(SourceFile, link, result.Success,
                    result.Success ? "OK" : result.ErrorMessage));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating file link for {Source}", SourceFile);
                Results.Add(new SymlinkResultView(SourceFile, DestinationFolder, false,
                    "An unexpected error occurred while creating the symlink."));
            }

            return Page();
        }

        var folders = SourceFolders
            .Split(NewLineSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (folders.Count == 0)
        {
            return Page();
        }

        if (string.IsNullOrWhiteSpace(DestinationFolder))
        {
            Results.Add(new SymlinkResultView(string.Empty, DestinationFolder, false,
                "Select at least one source folder and a destination folder."));
            return Page();
        }

        if (folders.Any(f => !PathHelpers.IsFullyQualified(f)) || !PathHelpers.IsFullyQualified(DestinationFolder))
        {
            Results.Add(new SymlinkResultView(string.Empty, DestinationFolder, false,
                "Paths must be absolute."));
            return Page();
        }

        IReadOnlyList<SymlinkResult> results;
        try
        {
            results = await manager.CreateDirectoryLinksAsync(folders, DestinationFolder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating directory links in {DestinationFolder}", DestinationFolder);
            Results.Add(new SymlinkResultView(string.Empty, DestinationFolder, false,
                "An unexpected error occurred while creating the symlinks."));
            return Page();
        }

        if (results.Count != folders.Count)
        {
            logger.LogError("Symlink result count {ResultCount} does not match source count {SourceCount}",
                results.Count, folders.Count);
            Results.Add(new SymlinkResultView(string.Empty, DestinationFolder, false,
                "An unexpected error occurred while creating the symlinks."));
            return Page();
        }

        for (var i = 0; i < folders.Count; i++)
        {
            var source = folders[i];
            var link = Path.Combine(DestinationFolder, Path.GetFileName(source));
            var r = results[i];
            Results.Add(new SymlinkResultView(source, link, r.Success,
                r.Success ? "OK" : r.ErrorMessage));
        }

        return Page();
    }
}