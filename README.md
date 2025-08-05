# MklinkUI

MklinkUI is a small utility for Windows 11 that creates symbolic links and displays whether Developer Mode is enabled.

## Projects
 - `MklinkUI.App`: WPF application for creating file and directory symbolic links with a tray icon.
 - `MklinkUI.Core`: Core logic and services.
 - `MklinkUI.Tests`: xUnit tests using FluentAssertions and Moq.
 - `installer`: WiX installer project for packaging the app.

## Prerequisites
- Windows 11
- [.NET 8 SDK](https://dotnet.microsoft.com/)
- Administrative rights or Developer Mode enabled

## Setup
1. Restore and build the solution:
   ```powershell
   dotnet restore
   dotnet build
   ```
2. Run tests:
   ```powershell
   dotnet test src/MklinkUI.Tests/MklinkUI.Tests.csproj
   ```
3. Build the installer (requires [WiX Toolset 4](https://wixtoolset.org/)):
   ```powershell
   dotnet build installer/MklinkUI.Installer.wixproj
   wix build installer/MklinkUI.Installer.wixproj -o MklinkUI.msi
   ```

## Logs and Settings
Logs are written to `%AppData%/MklinkUI/app.log`.
User settings are stored in `%AppData%/MklinkUI/settings.json`.

## Usage
The application displays the current Developer Mode status and can create file or directory symbolic links. Minimize or close the window to hide it in the system tray; the tray icon menu lets you reopen the app, exit, or start minimized. Browse paths or drag files directly onto the input boxes, then click **Create** to generate a link.

### UI Features
- **Theme toggle** – switch between light, dark, or match the system. Changes apply immediately and persist between sessions.
- **Tray icon** – minimizing or closing hides the window to the system tray. The tray menu provides *Open* and *Exit* actions, and an option allows starting minimized.
- **Drag and drop** – drop files or folders onto the source or destination boxes to populate the paths.
- **Browse buttons** – open standard Windows dialogs to select source files/folders and destination targets.
