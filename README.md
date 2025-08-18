# MklinkUI

MklinkUI is a small web-based utility that creates file or directory symbolic links.

The application runs without containerization and does not expose a health check endpoint.

## Solution structure
The solution (`MklinkUi.sln`) is composed of several projects, each with a distinct responsibility:

- `src/MklinkUi.Core` – cross-platform abstractions, shared helpers such as `PathHelpers`, and the `SymlinkManager` coordinator.
- `src/MklinkUi.Windows` – Windows-only services that read the registry and invoke the Win32 `CreateSymbolicLink` API. The project requires the Windows Desktop SDK and is built only on Windows.
- `src/MklinkUi.Fakes` – stub implementations used for development and tests on non-Windows hosts.
- `src/MklinkUi.WebUI` – ASP.NET Core front end that loads either `MklinkUi.Windows.dll` or `MklinkUi.Fakes.dll` at runtime.
- `tests/MklinkUi.Tests` – xUnit tests using FluentAssertions and Moq.
- `tests/MklinkUi.Windows.Tests` – Windows-only tests for the real symlink service.

## Platform-specific service registration
`ServiceRegistration.AddPlatformServices` checks the current OS and loads `MklinkUi.Windows.dll` or `MklinkUi.Fakes.dll` from the application directory using reflection. If neither assembly is found, basic default services are used that rely on the cross-platform `File.CreateSymbolicLink` API.

Outside of the Development environment the application verifies it is running on Windows and exits with a `PlatformNotSupportedException` on other operating systems.

## Configuration

Configuration is provided by `appsettings.json`, optional environment-specific JSON files (e.g. `appsettings.Development.json`), and environment variables. Environment variables with the `MKLINKUI__` prefix override JSON values. Example:

```bash
MKLINKUI__SERVER__DEFAULTHTTPPORT=5285
ASPNETCORE_URLS=http://localhost:5280
```

The default configuration includes:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://localhost:5280" },
      "Https": { "Url": "https://localhost:5281" }
    }
  },
  "Server": {
    "PreferredPortRange": "5280-5299",
    "DefaultHttpPort": 5280,
    "DefaultHttpsPort": 5281
  },
  "Symlink": {
    "CollisionPolicy": "Skip",
    "BatchMax": 100
  },
  "Paths": {
    "LogDirectory": ""
  },
  "UI": {
    "MaxCardWidth": 800,
    "EnableDragDrop": true
  }
}
```

Developer mode behaviour is now determined by the standard `DOTNET_ENVIRONMENT` / `ASPNETCORE_ENVIRONMENT` variables; the app treats the `Development` environment as developer mode.

## Logging and diagnostics

MklinkUI uses Serilog for structured logging. Logs are written to `%AppData%/MklinkUi/logs` on Windows or `~/.mklinkui/logs` on other systems with daily rolling files. Each log entry is enriched with machine, process and thread identifiers, application version, environment name and a per-request correlation identifier.

The correlation ID flows through requests via the `X-Correlation-ID` header and is included in any error responses. Include this value when reporting bugs.

| Code | Meaning |
| --- | --- |
| E_INVALID_PATH | Provided path was not absolute |
| E_DEV_MODE_REQUIRED | Developer mode or elevation is required |
| E_UNEXPECTED | Unexpected server error |

Logging levels are controlled by `Serilog:MinimumLevel` in `appsettings.json` and can be overridden in environment-specific files or via environment variables such as `Serilog__MinimumLevel__Default`.

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

### Testing & Coverage

Collect coverage reports for both test projects:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat="lcov,cobertura"
```

The Windows-specific tests in `MklinkUi.Windows.Tests` are automatically skipped on non-Windows hosts.

## Publishing
Publish the app for Windows:

```bash
dotnet publish src/MklinkUi.WebUI -c Release -r win-x64 --self-contained false
```
The published files are in `src/MklinkUi.WebUI/bin/Release/net8.0/publish`.

## Limitations and known issues
- The web UI is minimal and lacks comprehensive error handling.
- Creating symbolic links may require elevated privileges or Windows Developer Mode.
- Browser file pickers cannot expose absolute file paths, so only file names are captured when selecting files.

## Web interface

The dark-themed web interface centers its main card on screen for common desktop resolutions. Two link modes are available:

- **File → File** – select a single source file and a destination folder. A link with the same file name is created inside the destination folder.
- **Folder → Folder** – select one or more source folders and a destination folder. Each selected folder is linked into the destination folder using its original name.

All paths must be provided as absolute paths; relative paths are rejected by the UI and services.

## Ports

By default the app binds to HTTP port **5280** and HTTPS **5281**. These defaults come from the `Server` section of `appsettings.json` and can be overridden via `ASPNETCORE_URLS` or prefixed environment variables such as `MKLINKUI__SERVER__DEFAULTHTTPPORT`.


## Continuous integration
A GitHub Actions workflow runs on every push and pull request to build the application on Linux and Windows and execute the full test suite.
