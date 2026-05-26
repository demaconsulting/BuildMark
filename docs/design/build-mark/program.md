## Program

### Purpose

`Program` is the top-level unit of BuildMark. It owns the application entry point, creates
the `Context` object from command-line arguments, and dispatches execution to the appropriate
handler based on the parsed flags.

The unit exposes a private entry point `Main`, which is called by the .NET runtime, and a
public method `Run`, which accepts an existing `Context` and performs the main execution
logic. `Run` is exposed separately so that integration tests can inject a pre-configured
`Context` without repeating argument parsing.

### Data Model

**Version**: `string` — The tool version string, resolved from
`AssemblyInformationalVersionAttribute` (preferred; includes pre-release labels and build
metadata), then `assembly.GetName().Version`, and finally `"0.0.0"` when neither attribute
is present.

### Key Methods

**Main(string[] args) → int**: Application entry point, declared `private static`.

- *Parameters*: `string[] args` — command-line arguments from the runtime.
- *Returns*: `int` — exit code; 0 for success, 1 for failure.
- *Preconditions*: Called by the .NET runtime on startup.
- *Postconditions*: Returns an exit code and terminates the process.

Creates a `Context` from the supplied arguments and delegates to `Run`. `ArgumentException`
and `InvalidOperationException` are caught, written to `Console.Error`, and yield exit code

1. Any other exception is re-thrown after logging to generate runtime event logs.

**Run(Context context) → void**: Executes program logic against an already-constructed
`Context`.

- *Parameters*: `Context context` — pre-constructed context with parsed flags and output
  methods.
- *Returns*: void; side effects include writing to `context` and setting `context.ExitCode`.
- *Preconditions*: `context` is non-null and fully initialized.
- *Postconditions*: One execution path has completed; `context.ExitCode` reflects the outcome.

Applies a priority dispatch order: (1) if `context.Version` is set, print the version string
and return; (2) print the application banner; (3) if `context.Help` is set, print the usage
message and return; (4) if `context.Validate` is set, delegate to `Validation.Run(context)`
and return; (5) if `context.Lint` is set, call `LoadConfiguration()`, call
`result.ReportTo(context)`, and return; (6) otherwise, call `ProcessBuildNotes(context)`.

**ProcessBuildNotes(Context context)**: Loads configuration and generates the build report.

- *Parameters*: `Context context` — context for reading flags and writing output.
- *Returns*: void.
- *Preconditions*: `context` is non-null; must be called after banner has been printed.
- *Postconditions*: Report file written (if `--report` is set); exit code set on any error.

Steps: (1) calls `LoadConfiguration()` and surfaces issues via `result.ReportTo(context)`;
returns early on errors; (2) derives `effectiveConfig` as
`loadResult.Config ?? BuildMarkConfig.CreateDefault()`; (3) resolves effective report options
by preferring CLI arguments over configuration values; (4) creates the connector via
`context.ConnectorFactory` (test injection path) or `RepoConnectorFactory.Create`, then calls
`configurableConnector.Configure(rules, sections)` when using the production factory;
(5) parses `context.BuildVersion` via `VersionTag.Create`, returning early on
`ArgumentException`; (6) calls `connector.GetBuildInformationAsync` synchronously, returning
early on `InvalidOperationException`; (7) writes a build summary to `context`; (8) if
`effectiveReportFile` is set, renders markdown and writes to file — file-system exceptions are
caught and reported via `context.WriteError` without propagating.

**PrintBanner(Context context)**: Writes the application name, version, and copyright notice
to `context`.

- *Parameters*: `Context context` — output target.
- *Returns*: void.
- *Preconditions*: None.
- *Postconditions*: Banner lines written to context output.

Called unconditionally after the version-flag check in `Run`.

**PrintHelp(Context context)**: Writes the full usage message to `context`.

- *Parameters*: `Context context` — output target.
- *Returns*: void.
- *Preconditions*: None.
- *Postconditions*: Usage message written to context output.

Called when any of the `-?`, `-h`, or `--help` flags is set.

**LoadConfiguration() → ConfigurationLoadResult**: Synchronously loads the optional
repository configuration.

- *Parameters*: None.
- *Returns*: `ConfigurationLoadResult` — config (may be `null` if absent) and any issues.
- *Preconditions*: `Environment.CurrentDirectory` is accessible.
- *Postconditions*: Returns a fully populated `ConfigurationLoadResult`.

Synchronously invokes `BuildMarkConfigReader.ReadAsync(Environment.CurrentDirectory)` via
`GetAwaiter().GetResult()`. Called from both the lint branch of `Run` and from
`ProcessBuildNotes` to keep the async-to-sync bridging in one place.

### Error Handling

`Main` catches `ArgumentException` and `InvalidOperationException` from `Context`
construction and writes them directly to `Console.Error`, returning exit code 1. Any other
exception is re-thrown. `ProcessBuildNotes` catches file-system exceptions during report
writing and reports them via `context.WriteError` without propagating;
`InvalidOperationException` from `connector.GetBuildInformationAsync` is caught, reported,
and causes early return. The graceful-degradation choice for file-system exceptions ensures
that a report-write failure does not obscure the build summary already written to the console
and allows the exit code to reflect only semantic errors.

### Dependencies

- **Context** — provides parsed CLI flags, output methods, and exit code tracking
  (Cli subsystem)
- **Validation** — executes self-validation when `--validate` flag is set (SelfTest
  subsystem)
- **BuildMarkConfigReader** — reads and parses the optional `.buildmark.yaml` file
  (Configuration subsystem)
- **ConfigurationLoadResult** — holds configuration and parse issues; `ReportTo(context)`
  surfaces issues to the user
- **BuildMarkConfig** — carries effective configuration; `CreateDefault()` supplies
  built-in defaults when no config file is present
- **RepoConnectorFactory** — creates the connector from configuration (RepoConnectors
  subsystem)
- **RepoConnectorBase** — used for optional routing configuration when no factory is
  injected
- **BuildInformation** — returned by the connector; `ToMarkdown` renders the report
  (BuildNotes subsystem)
- **VersionTag** — parses the `--build-version` argument (Version subsystem)

### Callers

N/A — entry point, called by the .NET runtime. `Run` is also invoked directly by integration
tests via `Context.Create(args, connectorFactory)`.
