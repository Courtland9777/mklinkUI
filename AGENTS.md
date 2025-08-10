# AGENTS

Guidelines for contributors and automated agents working on **MklinkUI**.

## Project Overview
- **Solution:** `MklinlUi.sln`
- **Projects:**
  - `src/MklinlUi.Core` – cross-platform abstractions and `SymlinkManager`.
  - `src/MklinlUi.Windows` – Windows-only services using the Windows Desktop SDK.
  - `src/MklinlUi.Fakes` – stub services for non-Windows development and tests.
  - `src/MklinlUi.WebUI` – ASP.NET Core front end.
  - `tests/MklinlUi.Tests` – xUnit tests using FluentAssertions and Moq.

## Contribution Workflow
1. **Environment**
   - Requires .NET 8 SDK.
   - Keep Windows-specific code isolated in `MklinlUi.Windows` and rely on interfaces from `MklinlUi.Core`.

2. **Before Committing**
   - Restore packages:  
     `dotnet restore`
   - Build relevant projects:
     - Non‑Windows development:  
       `dotnet build src/MklinlUi.Fakes`  
       `dotnet build src/MklinlUi.WebUI`
     - Windows development:  
       `dotnet build src/MklinlUi.Windows`  
       `dotnet build src/MklinlUi.WebUI`
   - Run unit tests:  
     `dotnet test`
   - (Optional) Apply formatting:  
     `dotnet format`

3. **Testing & Code Quality**
   - Add or update tests in `tests/MklinlUi.Tests` for all functional changes.
   - Follow existing coding patterns; use dependency injection and keep functions small with XML documentation where helpful.

4. **Documentation**
   - Update `README.md` or other docs when behavior or build steps change.
   - When modifying service discovery or platform-specific logic, ensure `ServiceRegistration.AddPlatformServices` handles new assemblies.

## Pull Requests
- Provide a concise summary and rationale.
- Note any new build or run steps.
- Reference relevant issues when applicable.
- Confirm in the PR description that you executed the commands above.

