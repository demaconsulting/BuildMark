# BuildMark System Design

## Overview

BuildMark is a .NET command-line tool that generates markdown build notes from
GitHub repository metadata. It queries GitHub's GraphQL API to retrieve commits,
issues, pull requests, and version tags, then formats the results as a structured
markdown report suitable for embedding in release documentation.

## System Architecture

BuildMark is composed of five subsystems and a top-level entry point:

| Component            | Kind      | Responsibility                                           |
|----------------------|-----------|----------------------------------------------------------|
| `Program`            | Unit      | Entry point; dispatches to handlers based on CLI flags   |
| `Cli`                | Subsystem | Command-line argument parsing and output channel control |
| `Configuration`      | Subsystem | Parses the `.buildmark.yaml` configuration file          |
| `RepoConnectors`     | Subsystem | Repository metadata retrieval via the GitHub GraphQL API |
| `SelfTest`           | Subsystem | Built-in self-validation test framework                  |
| `Utilities`          | Subsystem | Shared path combination helpers                          |
| `ItemControls`       | Subsystem | Parsing of buildmark blocks and version interval sets    |

## External Interfaces

| Interface            | Direction | Protocol / Format                                        |
|----------------------|-----------|----------------------------------------------------------|
| Command line         | Input     | POSIX-style flags parsed by `Context`                    |
| `.buildmark.yaml`    | Input     | YAML file read from the repository root                  |
| GitHub GraphQL       | Output    | HTTPS POST to `https://api.github.com/graphql`           |
| Markdown report      | Output    | File written to `--report` path, UTF-8 markdown          |
| Log file             | Output    | Optional file written to `--log` path, plain text        |
| Test results         | Output    | TRX or JUnit XML written to `--results` path             |
| Exit code            | Output    | 0 = success, 1 = error                                   |

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
                              ├──────────────────────────────────────────────┐
                              ▼                                              │
               BuildMarkConfigReader (Configuration)                        │
                    reads .buildmark.yaml (optional)                        │
                    returns BuildMarkConfig                                  │
                              │                                              │
                              ▼                                              │
                   RepoConnectorFactory ◄────────────────────────────────────┘
                    (uses ConnectorConfig)
                              │
                              ▼
                   GitHubRepoConnector   ←─── GitHub GraphQL API
                              │                (fetches body of issues and PRs)
                              │  ← applies SectionConfig / RuleConfig
                              ▼
                   ItemControlsParser (ItemControls)
                              │  ← applied per-issue and per-PR
                              │  ← overrides visibility, type, affected-versions
                              ▼
                   BuildInformation.ToMarkdown()
                              │
                              ▼
                   [Markdown Report File]
```

## System-Wide Design Constraints

- **Target framework**: .NET 8, .NET 9, and .NET 10
- **Platform support**: Windows, Linux, macOS
- **Packaging**: Published as a .NET global tool (`dotnet tool install`)
- **Authentication**: GitHub token supplied via `GH_TOKEN` environment variable,
  `GITHUB_TOKEN` environment variable, or `gh auth token` CLI fallback
- **No GUI**: All interaction is through the command line; no interactive prompts
- **Self-contained**: The tool operates without any configuration file; an optional
  `.buildmark.yaml` file in the repository root enables connector selection and
  item routing customization

## Integration Patterns

### Configuration File

`BuildMarkConfigReader.ReadAsync(path)` looks for a `.buildmark.yaml` file at the
supplied path (normally the repository root). If the file is absent the method
returns `null` and the tool proceeds with default behavior. When the file is
present it is deserialized into a `BuildMarkConfig` object, which is consumed by
`Program` during startup:

- `BuildMarkConfig.Connector` — optional `ConnectorConfig` carrying the connector
  `Type` (`"github"`, `"azure-devops"`, or `"github+azure-devops"`) and any
  connector-specific settings. Passed to `RepoConnectorFactory` to select the
  appropriate connector implementation.
- `BuildMarkConfig.Sections` — ordered list of `SectionConfig` objects (each with
  an `Id` and `Title`) that define the report sections. Passed to the active
  connector for output structuring.
- `BuildMarkConfig.Rules` — list of `RuleConfig` objects that map item attributes
  (labels, work-item types) to report sections. Passed to the active connector for
  item routing.

### GitHub GraphQL Client

`GitHubRepoConnector` uses `GitHubGraphQLClient` to issue paginated GraphQL
queries over HTTPS. Authentication is via `Authorization: bearer <token>` header.
The connector retrieves:

- All tags (for identifying version boundaries)
- Commits in the requested range
- Issues referenced by commits (including description body)
- Pull requests in the requested range (including description body)
- Releases (for known-issues data)

The `body` field of issues and pull requests is fetched so that `ItemControlsParser`
can extract embedded `buildmark` blocks.

### Item Controls

When `GitHubRepoConnector` processes each issue or pull request, it passes the
description body to `ItemControlsParser.Parse`. If a `buildmark` code block is
present (including when hidden inside an HTML comment), the parser returns an
`ItemControlsInfo` record that carries optional overrides for `Visibility`, `Type`,
and `AffectedVersions`.

The connector applies these overrides as follows:

- `visibility: internal` — the item is excluded from all report sections
- `visibility: public` — the item is included regardless of its label-derived type
- `type: bug` or `type: feature` — overrides the label-derived type classification
- `affected-versions` — stored on the `ItemInfo` record for downstream use

When no `buildmark` block is present, the existing label-based rules apply
unchanged.

### Self-Validation

The `--validate` flag invokes `Validation.Run`, which exercises core tool
functionality using a `MockRepoConnector` and writes a standard TRX or JUnit XML
results file. This allows operators to verify the tool works correctly in their
environment without requiring a live GitHub connection.

### Report Generation

`BuildInformation.ToMarkdown` converts the in-memory build data model into a
markdown string. The heading depth is configurable via `--report-depth`, allowing
the report to be embedded at any level in a larger document.
