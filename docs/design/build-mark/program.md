# Program

## Overview

`Program` is the top-level unit of BuildMark. It owns the application entry point,
creates the `Context` object from command-line arguments, and dispatches execution
to the appropriate handler based on the parsed flags.

The unit contains two public methods: `Main`, which is called by the .NET runtime,
and `Run`, which accepts an existing `Context` and performs the main execution logic.
`Run` is exposed separately so that integration tests can inject a pre-configured
`Context` without repeating argument parsing.

## Data Model

`Program` exposes a single static property:

| Property  | Type     | Description                                    |
|-----------|----------|------------------------------------------------|
| `Version` | `string` | The tool version read from assembly attributes |

The version is resolved at startup by inspecting `AssemblyInformationalVersionAttribute`
first, then falling back to `AssemblyVersionAttribute`, and finally defaulting to
`"0.0.0"` if neither attribute is present.

## Methods

### `Main(string[] args) → int`

The application entry point. It creates a `Context` from the supplied command-line
arguments and delegates to `Run`. Any `ArgumentException` or
`InvalidOperationException` is caught, written to the error output via
`Context.WriteError`, and results in an exit code of 1. Any other exception is
re-thrown after logging so the runtime can report an unhandled exception.

### `Run(Context context) → void`

Executes the main program logic against an already-constructed `Context`. The
method applies the following priority order:

1. If `context.Version` is set, print the version string and return.
2. If `context.Help` is set, print the usage message and return.
3. If `context.Validate` is set, delegate to `Validation.Run(context)` and return.
4. Otherwise, call `ProcessBuildNotes(context)` to generate the build report.

The exit code is managed through `context.ExitCode` rather than as a return value.

### `ProcessBuildNotes(Context context)`

Resolves the build version, creates a repository connector via
`context.ConnectorFactory` (or the default `RepoConnectorFactory`), fetches
`BuildInformation`, writes a summary to the console, and optionally writes the
markdown report to `context.ReportFile`.

## Interactions

| Unit / Subsystem       | Role                                                              |
|------------------------|-------------------------------------------------------------------|
| `Context`              | Provides parsed flags, arguments, and output methods              |
| `Validation`           | Executes self-validation when `--validate` flag is set            |
| `RepoConnectorFactory` | Creates the default `IRepoConnector` implementation               |
| `BuildInformation`     | Returned by the connector; converted to markdown via `ToMarkdown` |
