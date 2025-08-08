# MklinkUI

MklinkUI is a small web-based utility that reports whether Windows Developer Mode is enabled and creates file or directory symbolic links.

## Solution structure
The solution (`MklinlUi.sln`) is composed of several projects, each with a distinct responsibility:

- `src/MklinlUi.Core` – cross-platform abstractions and the `SymlinkManager` coordinator.
- `src/MklinlUi.Windows` – Windows-only services that read the registry and invoke the Win32 `CreateSymbolicLink` API.
- `src/MklinlUi.Fakes` – fallback services used on non-Windows platforms and in tests.
- `src/MklinlUi.WebUI` – ASP.NET Core front end that loads platform services at runtime.
- `tests/MklinlUi.Tests` – xUnit tests using FluentAssertions and Moq.

## Dynamic platform detection and assembly loading
`ServiceRegistration.AddPlatformServices` inspects the current OS at runtime. On Windows, it loads `MklinlUi.Windows.dll`; on other platforms, it loads `MklinlUi.Fakes.dll`. The assembly is loaded via `Assembly.LoadFrom`, and reflection is used to locate concrete implementations of `IDeveloperModeService` and `ISymlinkService`. If loading fails, default implementations are used so the app can still run.

## Build and run with Visual Studio 2022
1. Open `MklinlUi.sln` in **Visual Studio 2022**.
2. Set **MklinlUi.WebUI** as the startup project.
3. Press **F5** to build and launch the web app.

When running on Windows, `MklinlUi.Windows` is loaded automatically. On other platforms the app falls back to the fake services.

### Command line
Alternatively, use the .NET CLI:

```bash
dotnet restore
dotnet run --project src/MklinlUi.WebUI
```

Run the unit tests:

```bash
dotnet test
```

## Publishing
Publish the app for Windows:

```bash
dotnet publish src/MklinlUi.WebUI -c Release -r win-x64 --self-contained false
```
The published files are in `src/MklinlUi.WebUI/bin/Release/net8.0/publish`.

## Limitations and known issues
- The web UI is minimal and lacks comprehensive error handling.
- On non-Windows platforms, the developer mode check always reports enabled.
- Creating symbolic links may require elevated privileges or Windows Developer Mode.

