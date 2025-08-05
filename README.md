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

Logs are written to `%AppData%/MklinkUI/app.log`.

## Usage
The application displays the current Developer Mode status. Symlink creation and installer features will be added in future iterations.
