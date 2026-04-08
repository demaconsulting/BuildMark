# BuildMark System Design

## Overview

BuildMark is a .NET command-line tool that generates markdown build notes from
GitHub repository metadata. It queries GitHub's GraphQL API to retrieve commits,
issues, pull requests, and version tags, then formats the results as a structured
markdown report suitable for embedding in release documentation.

## System Architecture

BuildMark is composed of six subsystems and a top-level entry point:

- `Program` (Unit) — entry point; dispatches to handlers based on CLI flags
- `Cli` (Subsystem) — command-line argument parsing and output channel control
- `BuildNotes` (Subsystem) — output data model shared by all connectors and `Program`
- `Configuration` (Subsystem) — parses the `.buildmark.yaml` configuration file
- `RepoConnectors` (Subsystem) — repository metadata retrieval, including item-controls
  parsing and concrete connectors
- `SelfTest` (Subsystem) — built-in self-validation test framework
- `Utilities` (Subsystem) — shared path combination and process execution helpers

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
        ├─ --lint     →  BuildMarkConfigReader (Configuration)
        │                  reads .buildmark.yaml, reports issues, exits
        └─ (default)  →  ProcessBuildNotes()
                              │
                              ├──────────────────────────────────────────────┐
                              ▼                                              │
               BuildMarkConfigReader (Configuration)                        │
                    reads .buildmark.yaml (optional)                        │
                    returns ConfigurationLoadResult                          │
                    reports any issues to Context                            │
                              │                                              │
                              ▼                                              │
                   RepoConnectorFactory ◄────────────────────────────────────┘
                    (uses ConnectorConfig)
                              │
                              ▼
                   GitHubRepoConnector   ←─── GitHub GraphQL API
                              │                (fetches body of issues and PRs)
                              │  ← applies SectionConfig / RuleConfig via ItemRouter
                              ▼
                   ItemControlsParser (ItemControls)
                              │  ← applied per-issue and per-PR
                              │  ← overrides visibility, type, affected-versions
                              ▼
                   BuildNotes.BuildInformation.ToMarkdown()
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
- **Configuration linting**: Malformed configuration file issues are reported to the
  user via `ConfigurationLoadResult.ReportTo`; the `--lint` flag validates the
  configuration file and exits without performing a build

## Integration Patterns

### Configuration File

`BuildMarkConfigReader.ReadAsync(path)` looks for a `.buildmark.yaml` file at the
supplied path (normally the repository root). The method always returns a
`ConfigurationLoadResult`:

- If the file is absent, `Config` is `null` and `Issues` is empty; the tool
  proceeds with default behavior.
- If the file is present but contains YAML errors or invalid values, `Config` may
  be `null` and `Issues` contains one or more `ConfigurationIssue` records, each
  carrying a `FilePath`, `Line`, `Severity` (`Error` or `Warning`), and
  `Description`. `ReportTo(context)` writes each issue to the log and sets the
  exit code to 1 when any issue is an error.
- If the file is valid, `Config` is a fully populated `BuildMarkConfig` and
  `Issues` is empty.

`Program` calls `result.ReportTo(context)` immediately after reading the
configuration. The `--lint` flag causes `Program` to stop after this step,
allowing operators to validate the configuration file without running a build.

When a valid `BuildMarkConfig` is available, its properties are consumed as
follows:

- `BuildMarkConfig.Connector` — optional `ConnectorConfig` carrying the connector
  `Type` (`"github"`, `"azure-devops"`, or `"github+azure-devops"`), a `GitHub`
  property holding a `GitHubConnectorConfig` (when `Type` includes `"github"`),
  and an `AzureDevOps` placeholder. The `GitHubConnectorConfig` is passed to
  `GitHubRepoConnector` and may supply `Owner`, `Repo`, and `BaseUrl` overrides.
  The full `ConnectorConfig` is also passed to `RepoConnectorFactory` to select
  the appropriate connector implementation.
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
