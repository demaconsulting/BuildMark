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

- [.NET SDK][dotnet-download] 8.0, 9.0, or 10.0
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
- **Encoding**: UTF-8 with BOM
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
  - `Assert.HasCount(collection, expectedCount)`
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

## Documentation

### Markdown Guidelines

All markdown files must follow these rules (enforced by markdownlint):

- Maximum line length: 120 characters
- Use ATX-style headers (`# Header`)
- Lists must be surrounded by blank lines
- Use reference-style links: `[text][ref]` with `[ref]: url` at document end
- **Exception**: `README.md` uses absolute URLs (it's included in the NuGet package)

### Spell Checking

All files are spell-checked using cspell. Add project-specific terms to `.cspell.json`:

```json
{
  "words": [
    "myterm"
  ]
}
```

## Quality Checks

Before submitting a pull request, ensure all quality checks pass:

### 1. Build and Test

```bash
dotnet build --configuration Release
dotnet test --configuration Release
```

All tests must pass with zero warnings.

### 2. Linting

Run markdown linting:

```bash
npx markdownlint-cli2 "**/*.md" "#node_modules"
```

Run YAML linting:

```bash
yamllint .
```

Run spell checking:

```bash
npx cspell "**/*.{md,cs,yaml,json}" --no-progress
```

### 3. Code Coverage

Maintain code coverage above 80%:

```bash
dotnet test --collect "XPlat Code Coverage"
```

## Pull Request Process

1. **Update Documentation**: Ensure all documentation is updated to reflect your changes
2. **Add Tests**: Include tests for new functionality
3. **Update Changelog**: Add a brief description of your changes
4. **Link Issues**: Reference any related issues in your PR description
5. **Request Review**: Request review from maintainers
6. **Address Feedback**: Respond to review comments promptly

## Getting Help

If you need help or have questions:

- **GitHub Issues**: For bug reports and feature requests
- **GitHub Discussions**: For general questions and discussions
- **Pull Request Comments**: For questions about specific code changes

Thank you for contributing to BuildMark!

[code-of-conduct]: https://github.com/demaconsulting/BuildMark/blob/main/CODE_OF_CONDUCT.md
[dotnet-download]: https://dotnet.microsoft.com/download
[csharp-conventions]: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
