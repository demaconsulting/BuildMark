# Agent Quick Reference

Project-specific guidance for agents working on BuildMark - a .NET CLI tool for generating markdown build notes.

## Available Specialized Agents

- **Requirements Agent** - Develops requirements and ensures test coverage linkage
- **Technical Writer** - Creates accurate documentation following regulatory best practices
- **Software Developer** - Writes production code and self-validation tests in literate style
- **Test Developer** - Creates unit and integration tests following AAA pattern
- **Code Quality Agent** - Enforces linting, static analysis, and security standards
- **Repo Consistency Agent** - Ensures BuildMark remains consistent with TemplateDotNetTool template patterns

## Tech Stack

- C# 12, .NET 8.0/9.0/10.0, MSTest, dotnet CLI, NuGet

## Key Files

- **`requirements.yaml`** - All requirements with test linkage (enforced via `dotnet reqstream --enforce`)
- **`.editorconfig`** - Code style (file-scoped namespaces, 4-space indent, UTF-8+BOM, LF endings)
- **`.cspell.json`, `.markdownlint.json`, `.yamllint.yaml`** - Linting configs

## Requirements

- Link ALL requirements to tests (prefer `BuildMark_*` self-validation tests)
- Enforced in CI: `dotnet reqstream --requirements requirements.yaml --tests "test-results/**/*.trx" --enforce`
- When adding features: add requirement + link to test

## Testing

- **Test Naming**: `BuildMark_MethodUnderTest_Scenario` for self-validation tests
- **Self-Validation**: All tests run via `--validate` flag and can output TRX/JUnit format
- **Test Framework**: Uses DemaConsulting.TestResults library for test result generation
- **MSTest v4**: Use `Assert.HasCount()`, `Assert.IsEmpty()`, `Assert.DoesNotContain()` (not old APIs)
- **Console Tests**: Always save/restore `Console.Out` in try/finally

## Code Style

- **XML Docs**: On ALL members (public/internal/private) with spaces after `///` in summaries
- **Errors**: `ArgumentException` for parsing, `InvalidOperationException` for runtime issues
- **Namespace**: File-scoped namespaces only
- **Using Statements**: Top of file only (no nested using declarations except for IDisposable)
- **String Formatting**: Use interpolated strings ($"") for clarity
- **No code duplication**: Extract to properties/methods

## Project Structure

- **Context.cs**: Handles command-line argument parsing, logging, and output
- **Program.cs**: Main entry point with version/help/validation routing
- **Validation.cs**: Self-validation tests with TRX/JUnit output support

## Standard Command-Line Arguments

All DEMA Consulting tools should support:

- `-v`, `--version` - Display version information
- `-?`, `-h`, `--help` - Display help message
- `--silent` - Suppress console output
- `--validate` - Run self-validation
- `--results <file>` - Write validation results to file (TRX or JUnit format)
- `--log <file>` - Write output to log file

## Build and Test

```bash
# Build the project
dotnet build --configuration Release

# Run self-validation
dotnet run --project src/DemaConsulting.BuildMark \
  --configuration Release --framework net10.0 --no-build -- --validate

# Use convenience scripts
./build.sh    # Linux/macOS
build.bat     # Windows
```

## Documentation

- **User Guide**: `docs/guide/guide.md`
- **Requirements**: `requirements.yaml` -> auto-generated docs
- **Build Notes**: Auto-generated via BuildMark
- **Code Quality**: Auto-generated via CodeQL and SonarMark
- **Trace Matrix**: Auto-generated via ReqStream

## CI/CD

- **Quality Checks**: Markdown lint, spell check, YAML lint
- **Build**: Multi-platform (Windows/Linux)
- **CodeQL**: Security scanning
- **Integration Tests**: .NET 8/9/10 on Windows/Linux
- **Documentation**: Auto-generated via Pandoc + Weasyprint

## Common Tasks

```bash
# Format code
dotnet format

# Run all linters
./lint.sh     # Linux/macOS
lint.bat      # Windows

# Pack as NuGet tool
dotnet pack --configuration Release
```
