# AGENTS

Guidelines for contributors and automated agents working on **MklinkUI**.

## Project Overview
- **Solution:** `MklinkUi.sln`
- **Projects:**
  - `src/MklinkUi.Core` – cross-platform abstractions and `SymlinkManager`.
  - `src/MklinkUi.Windows` – Windows-only services using the Windows Desktop SDK.
  - `src/MklinkUi.Fakes` – stub services for non-Windows development and tests.
  - `src/MklinkUi.WebUI` – ASP.NET Core front end.
  - `tests/MklinkUi.Tests` – xUnit tests using FluentAssertions and Moq.

## Contribution Workflow
1. **Environment**
   - Requires .NET 8 SDK.
   - Keep Windows-specific code isolated in `MklinkUi.Windows` and rely on interfaces from `MklinkUi.Core`.

2. **Before Committing**
   - Restore packages:  
     `dotnet restore`
   - Build relevant projects:
     - Non‑Windows development:  
       `dotnet build src/MklinkUi.Fakes`  
       `dotnet build src/MklinkUi.WebUI`
     - Windows development:  
       `dotnet build src/MklinkUi.Windows`  
       `dotnet build src/MklinkUi.WebUI`
   - Run unit tests:  
     `dotnet test`
   - (Optional) Apply formatting:  
     `dotnet format`

3. **Testing & Code Quality**
   - Add or update tests in `tests/MklinkUi.Tests` for all functional changes.
   - Follow existing coding patterns; use dependency injection and keep functions small with XML documentation where helpful.

4. **Documentation**
   - Update `README.md` or other docs when behavior or build steps change.
   - When modifying service discovery or platform-specific logic, ensure `ServiceRegistration.AddPlatformServices` handles new assemblies.

## Pull Requests
- Provide a concise summary and rationale.
- Note any new build or run steps.
- Reference relevant issues when applicable.
- Confirm in the PR description that you executed the commands above.

