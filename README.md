# MklinkUI

MklinkUI is a small web-based utility that reports whether Windows Developer Mode is enabled and creates file or directory symbolic links.

## Project Layout
- `src/MklinlUi.Core` – core abstractions and the `SymlinkManager` coordinator.
- `src/MklinlUi.Windows` – Windows-only services that read the registry and invoke the `CreateSymbolicLink` Win32 API.
- `src/MklinlUi.Fakes` – fallback services used on non-Windows platforms.
- `src/MklinlUi.WebUI` – ASP.NET Core front end that loads platform services at runtime.
- `tests/MklinlUi.Tests` – xUnit tests using FluentAssertions and Moq.

## Running
Restore dependencies and run the web app:
```bash
dotnet restore
dotnet run --project src/MklinlUi.WebUI
```

## Publishing
Publish the app for Windows:
```bash
dotnet publish src/MklinlUi.WebUI -c Release -r win-x64 --self-contained false
```
The published files are in `src/MklinlUi.WebUI/bin/Release/net8.0/publish`.

## Platform-specific behavior
`ServiceRegistration.AddPlatformServices` loads `MklinlUi.Windows.dll` only when running on Windows. That assembly checks Developer Mode via the registry and creates links with P/Invoke. On other platforms the app falls back to the no-op implementations in `MklinlUi.Fakes.dll` so the site and tests can run without Windows-specific features.
