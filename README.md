# MklinkUI

MklinkUI is a small utility for Windows 11 that creates symbolic links and displays whether Developer Mode is enabled.

## Projects
- `MklinkUI.App`: WPF application.
- `MklinkUI.Core`: Core logic and services.
- `MklinkUI.Tests`: xUnit tests using FluentAssertions and Moq.
- `installer`: Placeholder WiX installer project.

## Setup
1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/).
2. Restore and build the solution:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Run tests:
   ```bash
   dotnet test src/MklinkUI.Tests/MklinkUI.Tests.csproj
   ```

Logs are written to `%AppData%/MklinkUI/app.log`. User settings are stored in `%AppData%/MklinkUI/settings.json`.

## Usage
The application displays the current Developer Mode status. You can create file or directory symbolic links, browse paths, or drag files directly onto the input boxes.

### UI Features
- **Theme toggle** – switch between light, dark, or match the system. Changes apply immediately and persist between sessions.
- **Tray icon** – minimizing or closing hides the window to the system tray. The tray menu provides *Open* and *Exit* actions, and an option allows starting minimized.
- **Drag and drop** – drop files or folders onto the source or destination boxes to populate the paths.
- **Browse buttons** – open standard Windows dialogs to select source files/folders and destination targets.

Symlink creation and installer features will be added in future iterations.
