# Contributing to BuildMark

Thank you for your interest in contributing to BuildMark! We welcome contributions from the community and appreciate
your help in making this project better.

## Code of Conduct

This project adheres to a [Code of Conduct][code-of-conduct]. By participating, you are expected to uphold this code.
Please report unacceptable behavior through GitHub.

## How to Contribute

### Reporting Bugs

If you find a bug, please create an issue on GitHub with the following information:

- **Description**: Clear description of the bug
- **Steps to Reproduce**: Detailed steps to reproduce the issue
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **Environment**: Operating system, .NET version, BuildMark version
- **Logs**: Any relevant error messages or logs

### Suggesting Features

We welcome feature suggestions! Please create an issue on GitHub with:

- **Feature Description**: Clear description of the proposed feature
- **Use Case**: Why this feature would be useful
- **Proposed Solution**: Your ideas on how to implement it (optional)
- **Alternatives**: Any alternative solutions you've considered (optional)

### Submitting Pull Requests

We follow a standard GitHub workflow for contributions:

1. **Fork** the repository
2. **Clone** your fork locally
3. **Create a branch** for your changes (`git checkout -b feature/my-feature`)
4. **Make your changes** following our coding standards
5. **Test your changes** thoroughly
6. **Commit your changes** with clear commit messages
7. **Push** to your fork
8. **Create a Pull Request** to the main repository

## Development Setup

### Prerequisites

- [.NET SDK][dotnet-download] 10.0 and 11.0 (both are required: the 11.0 SDK builds the
  `net11.0` target and the .NET 10 runtime runs the `net10.0` tests)
- Git
- A code editor (Visual Studio, VS Code, or Rider recommended)

### Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/demaconsulting/BuildMark.git
   cd BuildMark
   ```

2. Restore dependencies:

   ```bash
   dotnet tool restore
   dotnet restore
   ```

3. Build the project:

   ```bash
   dotnet build --configuration Release
   ```

4. Run tests:

   ```bash
   dotnet test --configuration Release
   ```

## Coding Standards

### General Guidelines

- Follow the [C# Coding Conventions][csharp-conventions]
- Use clear, descriptive names for variables, methods, and classes
- Write XML documentation comments for all public, internal, and private members
- Keep methods focused and single-purpose
- Write tests for new functionality

### Code Style

This project enforces code style through `.editorconfig`. Key requirements:

- **Indentation**: 4 spaces for C#, 2 spaces for YAML/JSON/XML
- **Line Endings**: LF (Unix-style)
- **Encoding**: UTF-8
- **Namespaces**: Use file-scoped namespace declarations
- **Braces**: Required for all control statements
- **Naming Conventions**:
  - Interfaces: `IInterfaceName`
  - Classes/Structs/Enums: `PascalCase`
  - Methods/Properties: `PascalCase`
  - Parameters/Local Variables: `camelCase`

### XML Documentation

All members require XML documentation with proper indentation:

```csharp
/// <summary>
///     Brief description of what this does.
/// </summary>
/// <param name="parameter">Description of the parameter.</param>
/// <returns>Description of the return value.</returns>
public int ExampleMethod(string parameter)
{
    // Implementation
}
```

Note the spaces after `///` for proper indentation in summary blocks.

## Testing Guidelines

### Test Framework

We use MSTest v4 for unit and integration tests.

### Test Naming Convention

Use the pattern: `ClassName_MethodUnderTest_Scenario_ExpectedBehavior`

Examples:

- `Program_Main_NoArguments_ReturnsSuccess`
- `Context_Create_WithInvalidFlag_ThrowsArgumentException`
- `BuildInformation_ToMarkdown_ValidData_ReturnsMarkdown`

### Writing Tests

- Write tests that are clear and focused
- Use modern MSTest v4 assertions:
  - `Assert.HasCount(expectedCount, collection)`
  - `Assert.IsEmpty(collection)`
  - `Assert.DoesNotContain(item, collection)`
- Always clean up resources (use `try/finally` for console redirection)

### Mocking and Testing Patterns

When testing classes that depend on external services (like GitHub GraphQL API):

1. **Use virtual methods for dependency creation**: Classes expose internal virtual methods
   (e.g., `CreateGraphQLClient`) that can be overridden in derived test classes
2. **Mock HTTP responses**: Use `MockGitHubGraphQLHttpMessageHandler` to simulate GitHub API responses
3. **Helper methods**: Use built-in helper methods for common responses

Example:

```csharp
// Create a mock HTTP handler using helper methods
using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
    .AddCommitsResponse("commit123")
    .AddReleasesResponse("v1.0.0")
    .AddEmptyPullRequestsResponse();

// Create HttpClient and inject into GitHubGraphQLClient
using var mockHttpClient = new HttpClient(mockHandler);
using var client = new GitHubGraphQLClient(mockHttpClient);

// Now the client will use mock responses instead of real API calls
var commits = await client.GetCommitsAsync("owner", "repo", "branch");
```

See `GitHubRepoConnectorTestabilityTests.cs` for complete examples.

### Running Tests

```bash
# Run all tests
dotnet test --configuration Release

# Run specific test
dotnet test --filter "FullyQualifiedName~YourTestName"

# Run with coverage
dotnet test --collect "XPlat Code Coverage"
```

### Self-Validation Tests

```bash
# Run self-validation tests
dotnet run --project src/DemaConsulting.BuildMark \
  --configuration Release --framework net10.0 --no-build -- --validate
```

## Documentation

### Markdown Guidelines

All markdown files must follow these rules (enforced by LintMark):

- Maximum line length: 120 characters
- Use ATX-style headers (`# Header`)
- Lists must be surrounded by blank lines
- Use reference-style links: `[text][ref]` with `[ref]: url` at document end
- **Exception**: `README.md` uses absolute URLs (it's included in the NuGet package)
- **Exception**: `.github/agents/*.md` files use inline links so URLs are visible in agent context

### Spell Checking

All files are spell-checked using TextLint. Add project-specific terms to `.textlint.yaml`:

```yaml
words:
  - buildmark
```

**Never** add a misspelled word to the `.textlint.yaml` word list to silence a spell-checking failure.
Doing so defeats the purpose of spell-checking and reduces the quality of the repository.

- If TextLint flags a word that is **misspelled**, fix the spelling in the source file.
- If TextLint flags a word that is a **genuine technical term** (tool name, project identifier, etc.) and is spelled
  correctly, raise a **proposal** (e.g. comment in a pull request) explaining why the word should be added. The
  proposal must be reviewed and approved before the word is added to the list.

### Help Cards

The built-in help (`buildmark help`) is served from Markdown help cards in `src/DemaConsulting.BuildMark/Help/`,
which the DemaConsulting.HelpCards package embeds in the tool assembly. `Cli/HelpCommand.cs` is the only code that
uses the package.

- **One file per topic.** The path below `Help/`, without `.md`, is the card id. `_source.md` names the source
  (`name:`) and sets `schema: 2`.
- **Front matter.** Every card has a `title`, a `summary` (at most 80 characters for a topic, 100 otherwise), and
  exactly one purpose tag: `tutorial`, `reference`, or `troubleshooting`. Add `aliases` for topic names that users
  commonly guess, such as `examples`, so `help <name>` finds the card.
- **New topics.** Add the id of every new top-level card to `HelpCommand.RootCardIds`, the one list of topics; a
  missing or extra topic fails the tests.
- **Examples.** Give every task that runs offline a runnable `command` example, one command per example: no pipes,
  redirection, or `&&`. Wrap `content` in single quotes and use double quotes inside the command, which bash,
  PowerShell, and cmd read alike. State the result when it matters with `exit:` (default 0) and `expect:`, a short
  fragment that must appear somewhere in the console output; `expect` cannot check a file that the command writes.
  Keep each `description` to about 50 characters. A command that needs a Git clone, network access, or a token
  goes in a fenced `bash` block or a `snippet` example, not in a `command` example.
- **Input files.** Show complete files only for formats that users author, such as `.buildmark.yaml`, and describe
  formats that the tool produces (build notes, TRX, JUnit) in prose. A card cannot save a file whose name starts
  with a dot, so `.buildmark.yaml` appears as a fenced `yaml` block in the card body. When a card's examples read
  it, keep a copy in `test/DemaConsulting.BuildMark.Tests/TestData/HelpExamples/<card id>.buildmark.yaml`; the
  example-runner test copies it into `invocation.WorkingDirectory` as `.buildmark.yaml` before running the tool. A
  copy of a sample shown in a card must equal the sample.
- **Width.** A printed card may not be wider than 120 characters. Links print as full commands (`[[#name]]` as
  about 50 characters) and `{{version}}` of a development build as about 46, so keep body lines near 75 characters
  with at most one link each.
- **Text.** Cards are linted and spell-checked like any Markdown file: no body line starting with `#`, no wide
  tables, and every fenced block names its language.

Run `pwsh ./build.ps1 -Changed` after changing a card. The `HelpCommand` unit tests validate every card, with
warnings treated as errors, and check its printed width; an integration test runs every command example against
the built tool and compares the result with the card.

## Quality Checks

Before submitting a pull request, ensure all quality checks pass.

While developing, the scripts can limit themselves to what your change affects (compared with `origin/main`,
including uncommitted files). Each script requires a scope and prints its usage with `-Help`; run the full
`-All` checks below before submitting:

```pwsh
pwsh ./build.ps1 -Changed    # build and test only the test projects the changes affect
pwsh ./build.ps1 -Project Tests   # build and test the named project (short names work; -Help lists them)
pwsh ./fix.ps1 -Changed      # auto-fix only the changed file types
pwsh ./lint.ps1 -Changed     # run only the lint checks whose inputs changed
```

### 1. Build and Test

```bash
# Build the project
dotnet build --configuration Release

# Run unit tests
dotnet test --configuration Release
```

All tests must pass with zero warnings. The script equivalent is `pwsh ./build.ps1 -All`.

### 2. Linting

```pwsh
# After making changes: applies dotnet format, markdown, and YAML fixes silently
pwsh ./fix.ps1 -All

# Before submitting a pull request: all linters must pass
pwsh ./lint.ps1 -All
```

### 3. Code Coverage

Maintain or improve code coverage. Use the `--collect "XPlat Code Coverage"` option when running tests.

## Commit Messages

Write clear, concise commit messages:

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit first line to 72 characters
- Reference issues and pull requests when applicable

Examples:

- `Add support for custom report headers`
- `Fix crash when results file path is invalid`
- `Update documentation for --report-depth option`
- `Refactor argument parsing for better testability`

## Pull Request Process

1. **Update Documentation**: Update relevant documentation for your changes
2. **Add Tests**: Include tests that cover your changes
3. **Run Quality Checks**: Ensure all linters, tests, and builds pass
4. **Submit PR**: Create a pull request with a clear description
5. **Code Review**: Address feedback from maintainers
6. **Merge**: Once approved, a maintainer will merge your PR

### Pull Request Template

When creating a pull request, include:

- **Description**: What changes does this PR introduce?
- **Motivation**: Why are these changes needed?
- **Related Issues**: Link to any related issues
- **Testing**: How have you tested these changes?
- **Checklist**:
  - [ ] Tests added/updated
  - [ ] Documentation updated
  - [ ] All tests pass
  - [ ] Code follows style guidelines
  - [ ] No new warnings introduced

## Requirements Management

BuildMark uses [DemaConsulting.ReqStream][reqstream] for requirements traceability:

- All requirements are defined in `requirements.yaml`
- Each requirement must be linked to at least one test case
- Run `dotnet reqstream` to generate the requirements documentation
- Use the `--enforce` flag in CI to ensure all requirements are covered

When adding new features:

1. Add the requirement to `requirements.yaml`
2. Write tests that validate the requirement
3. Link the tests to the requirement in `requirements.yaml`

## Release Process

Releases follow this process:

1. Create a release issue to track the release
2. Update version references as needed
3. Create a pull request with the release changes
4. Merge the pull request after review
5. Create a GitHub release with the new version tag - CI/CD will automatically build and publish the NuGet package

## Getting Help

If you need help or have questions:

- **GitHub Issues**: For bug reports and feature requests - [open an issue][issues]
- **GitHub Discussions**: For general questions and discussions - [start a discussion][discussions]
- **Pull Request Comments**: For questions about specific code changes
- **Security Vulnerabilities**: Please review the [Security Policy][security] before reporting

## License

By contributing to BuildMark, you agree that your contributions will be licensed under the MIT License.

Thank you for contributing to BuildMark!

[code-of-conduct]: https://github.com/demaconsulting/BuildMark/blob/main/CODE_OF_CONDUCT.md
[dotnet-download]: https://dotnet.microsoft.com/download
[csharp-conventions]: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
[reqstream]: https://github.com/demaconsulting/ReqStream
[discussions]: https://github.com/demaconsulting/BuildMark/discussions
[issues]: https://github.com/demaconsulting/BuildMark/issues
[security]: https://github.com/demaconsulting/BuildMark/blob/main/SECURITY.md
