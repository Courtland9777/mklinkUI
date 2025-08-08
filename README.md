# MklinkUI

MklinkUI is a small web-based utility that reports whether Windows Developer Mode is enabled and creates file or directory symbolic links.

## Solution structure
The solution (`MklinlUi.sln`) is composed of several projects, each with a distinct responsibility:

- `src/MklinlUi.Core` – cross-platform abstractions and the `SymlinkManager` coordinator.
- `src/MklinlUi.Windows` – Windows-only services that read the registry and invoke the Win32 `CreateSymbolicLink` API. The project is only built on Windows hosts so the rest of the solution can compile elsewhere.
- `src/MklinlUi.Fakes` – test double implementations used by the unit tests.
- `src/MklinlUi.WebUI` – ASP.NET Core front end that registers Windows services when built on Windows and otherwise uses in-project defaults.
- `tests/MklinlUi.Tests` – xUnit tests using FluentAssertions and Moq.

## Platform-specific service registration
`ServiceRegistration.AddPlatformServices` uses dependency injection to register platform services. When the app is built on Windows, the real implementations from `MklinlUi.Windows` are added. On other operating systems the WebUI falls back to built-in default services that rely on the cross-platform `File.CreateSymbolicLink` API and assume Developer Mode is enabled.

## Building
Build the WebUI on any platform:

```bash
dotnet build src/MklinlUi.WebUI
```

On Windows the build also compiles `MklinlUi.Windows` to provide the real services. On non-Windows hosts the project is skipped and the default implementations are used.

## Build and run with Visual Studio 2022
1. Open `MklinlUi.sln` in **Visual Studio 2022**.
2. Set **MklinlUi.WebUI** as the startup project.
3. Press **F5** to build and launch the web app.

When running on Windows, `MklinlUi.Windows` is referenced and its services are registered automatically. On other platforms the app compiles and runs without the Windows Desktop SDK by using the fallback services.

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

