# MklinkUI

MklinkUI is a small web-based utility that reports whether Windows Developer Mode is enabled and creates file or directory symbolic links.

## Solution structure
The solution (`MklinkUi.sln`) is composed of several projects, each with a distinct responsibility:

- `src/MklinkUi.Core` – cross-platform abstractions and the `SymlinkManager` coordinator.
- `src/MklinkUi.Windows` – Windows-only services that read the registry and invoke the Win32 `CreateSymbolicLink` API. The project requires the Windows Desktop SDK and is built only on Windows.
- `src/MklinkUi.Fakes` – stub implementations used for development and tests on non-Windows hosts.
- `src/MklinkUi.WebUI` – ASP.NET Core front end that loads either `MklinkUi.Windows.dll` or `MklinkUi.Fakes.dll` at runtime.
- `tests/MklinkUi.Tests` – xUnit tests using FluentAssertions and Moq.
- `tests/MklinkUi.Windows.Tests` – Windows-only tests for the real symlink service.

## Platform-specific service registration
`ServiceRegistration.AddPlatformServices` checks the current OS and loads `MklinkUi.Windows.dll` or `MklinkUi.Fakes.dll` from the application directory using reflection. If neither assembly is found, basic default services are used that rely on the cross-platform `File.CreateSymbolicLink` API and assume Developer Mode is enabled.

## Building
### Non-Windows development
Build the fake services and then the web app:

```bash
dotnet build src/MklinkUi.Fakes
dotnet build src/MklinkUi.WebUI
```

### Windows production
Build the real Windows services and then the web app:

```bash
dotnet build src/MklinkUi.Windows
dotnet build src/MklinkUi.WebUI
```

The WebUI project copies any of the above DLLs that exist into its output folder. At runtime it loads `MklinkUi.Windows.dll` when present; otherwise it uses the fake or default implementations.

## Build and run with Visual Studio 2022
1. Open `MklinkUi.sln` in **Visual Studio 2022**.
2. Set **MklinkUi.WebUI** as the startup project.
3. Press **F5** to build and launch the web app.

When running on Windows and `MklinkUi.Windows.dll` is present, its services are used automatically. On other platforms the app runs with the fake or built-in default services and does not require the Windows Desktop SDK.

### Command line
Alternatively, use the .NET CLI:

```bash
dotnet restore
dotnet run --project src/MklinkUi.WebUI
```

Run the unit tests:

```bash
dotnet test
```

## Publishing
Publish the app for Windows:

```bash
dotnet publish src/MklinkUi.WebUI -c Release -r win-x64 --self-contained false
```
The published files are in `src/MklinkUi.WebUI/bin/Release/net8.0/publish`.

## Limitations and known issues
- The web UI is minimal and lacks comprehensive error handling.
- On non-Windows platforms, the developer mode check always reports enabled.
- Creating symbolic links may require elevated privileges or Windows Developer Mode.

## Web interface

The dark-themed web interface centers its main card on screen for common desktop resolutions. When creating file links you can paste or list multiple source files and select a destination folder. Each file is linked into the folder using its original file name.

## Ports

By default the app binds to HTTP port **5280** (and HTTPS **5281** when a certificate is configured). Override with the `ASPNETCORE_URLS` environment variable or `Server:Port` in `appsettings.json`.

