# BuildMark System Design

## Overview

BuildMark is a .NET command-line tool that generates markdown build notes from
GitHub repository metadata. It queries GitHub's GraphQL API to retrieve commits,
issues, pull requests, and version tags, then formats the results as a structured
markdown report suitable for embedding in release documentation.

## System Architecture

BuildMark is composed of four subsystems and a top-level entry point:

| Component            | Kind      | Responsibility                                           |
|----------------------|-----------|----------------------------------------------------------|
| `Program`            | Unit      | Entry point; dispatches to handlers based on CLI flags   |
| `Cli`                | Subsystem | Command-line argument parsing and output channel control |
| `RepoConnectors`     | Subsystem | Repository metadata retrieval via the GitHub GraphQL API |
| `SelfTest`           | Subsystem | Built-in self-validation test framework                  |
| `Utilities`          | Subsystem | Shared path combination helpers                          |

## External Interfaces

| Interface       | Direction | Protocol / Format                                   |
|-----------------|-----------|-----------------------------------------------------|
| Command line    | Input     | POSIX-style flags parsed by `Context`               |
| GitHub GraphQL  | Output    | HTTPS POST to `https://api.github.com/graphql`      |
| Markdown report | Output    | File written to `--report` path, UTF-8 markdown     |
| Log file        | Output    | Optional file written to `--log` path, plain text   |
| Test results    | Output    | TRX or JUnit XML written to `--results` path        |
| Exit code       | Output    | 0 = success, 1 = error                              |

## Data Flow

```text
[Command Line Args]
        │
        ▼
  Context (Cli)          ← parses flags, opens log file
        │
        ▼
  Program.Run()
        ├─ --version  →  writes version to stdout
        ├─ --help     →  writes usage to stdout
        ├─ --validate →  Validation (SelfTest) → writes results to --results file
        └─ (default)  →  ProcessBuildNotes()
                              │
                              ▼
                   RepoConnectorFactory
                              │
                              ▼
                   GitHubRepoConnector   ←─── GitHub GraphQL API
                              │
                              ▼
                   BuildInformation.ToMarkdown()
                              │
                              ▼
                   [Markdown Report File]
```

## System-Wide Design Constraints

- **Target framework**: .NET 8 and .NET 9
- **Platform support**: Windows, Linux, macOS
- **Packaging**: Published as a .NET global tool (`dotnet tool install`)
- **Authentication**: GitHub token supplied via `GH_TOKEN` environment variable,
  `GITHUB_TOKEN` environment variable, or `gh auth token` CLI fallback
- **No GUI**: All interaction is through the command line; no interactive prompts
- **Self-contained**: No external configuration files required for normal operation

## Integration Patterns

### GitHub GraphQL Client

`GitHubRepoConnector` uses `GitHubGraphQLClient` to issue paginated GraphQL
queries over HTTPS. Authentication is via `Authorization: bearer <token>` header.
The connector retrieves:

- All tags (for identifying version boundaries)
- Commits in the requested range
- Issues referenced by commits
- Pull requests in the requested range
- Releases (for known-issues data)

### Self-Validation

The `--validate` flag invokes `Validation.Run`, which exercises core tool
functionality using a `MockRepoConnector` and writes a standard TRX or JUnit XML
results file. This allows operators to verify the tool works correctly in their
environment without requiring a live GitHub connection.

### Report Generation

`BuildInformation.ToMarkdown` converts the in-memory build data model into a
markdown string. The heading depth is configurable via `--report-depth`, allowing
the report to be embedded at any level in a larger document.
