## Program

### Overview

`Program` is the top-level unit of BuildMark. It owns the application entry point,
creates the `Context` object from command-line arguments, and dispatches execution
to the appropriate handler based on the parsed flags.

The unit exposes a private entry point `Main`, which is called by the .NET runtime,
and a public method `Run`, which accepts an existing `Context` and performs the main
execution logic. `Run` is exposed separately so that integration tests can inject a
pre-configured `Context` without repeating argument parsing.

### Data Model

`Program` exposes a single static property:

| Property  | Type     | Description                                    |
|-----------|----------|------------------------------------------------|
| `Version` | `string` | The tool version read from assembly attributes |

The version is resolved at startup by inspecting `AssemblyInformationalVersionAttribute`
first, then falling back to `assembly.GetName().Version`, and finally defaulting to
`"0.0.0"` if neither attribute is present.

### Methods

#### `Main(string[] args) → int`

The application entry point (declared `private static`). It creates a `Context` from
the supplied command-line arguments and delegates to `Run`. Any `ArgumentException`
or `InvalidOperationException` is caught, written directly to `Console.Error` (no
`Context` object exists yet at this level), and results in an exit code of 1. Any
other exception is re-thrown after logging so the runtime can report an unhandled
exception.

#### `Run(Context context) → void`

Executes the main program logic against an already-constructed `Context`. The
method applies the following priority order:

1. If `context.Version` is set, print the version string and return.
2. Print the application banner (version and copyright).
3. If `context.Help` is set, print the usage message and return.
4. If `context.Validate` is set, delegate to `Validation.Run(context)` and return.
5. If `context.Lint` is set, call `LoadConfiguration()`, call
   `result.ReportTo(context)`, and return.
6. Otherwise, call `ProcessBuildNotes(context)` to generate the build report.

The exit code is managed through `context.ExitCode` rather than as a return value.

#### `ProcessBuildNotes(Context context)`

Calls `BuildMarkConfigReader.ReadAsync` to load the optional `.buildmark.yaml`
file, then calls `result.ReportTo(context)` to surface any configuration issues.
If errors occurred, the method returns early. Otherwise:

1. **Effective configuration**: derives `effectiveConfig` as `loadResult.Config ??
   BuildMarkConfig.CreateDefault()`. When no `.buildmark.yaml` file is present (i.e.,
   `loadResult.Config` is `null`), `BuildMarkConfig.CreateDefault()` supplies built-in
   section and rule definitions so the tool functions without any configuration file.
2. **Effective option resolution**: derives `effectiveReportFile` from `context.ReportFile` if set,
   or from `effectiveConfig.Report?.File` as fallback; derives `effectiveReportDepth` from
   `context.Depth` if set, or `effectiveConfig.Report?.Depth`, defaulting to 1; derives
   `effectiveIncludeKnownIssues` from `context.IncludeKnownIssues` OR
   `effectiveConfig.Report?.IncludeKnownIssues`.
3. **ConnectorFactory injection**: if `context.ConnectorFactory` is non-null it is invoked directly
   (test injection path); otherwise `RepoConnectorFactory.Create(effectiveConfig.Connector)` is used.
4. **Configuration step**: when the production factory path is used and the connector implements
   `RepoConnectorBase`, calls `configurableConnector.Configure(effectiveConfig.Rules,
   effectiveConfig.Sections)`.
5. Parses `context.BuildVersion` using `VersionTag.Create`; on `ArgumentException`,
   writes an error and returns early.
6. Calls `connector.GetBuildInformationAsync(buildVersion)` synchronously; on
   `InvalidOperationException`, writes an error and returns early.
7. Writes a build summary to the context output.
8. If `effectiveReportFile` is non-null, renders the markdown and writes it to that path.
   Any file-system exception during write is caught, reported via `context.WriteError`, and
   execution continues - the method does not propagate the exception. This graceful-degradation
   choice ensures that a report-write failure does not obscure the build summary already written
   to the console and allows the exit code to reflect only semantic errors rather than I/O
   failures outside the tool's control.

#### `PrintBanner(Context context)`

Writes the application name, version, and copyright notice to the context output.
Called unconditionally after the version-flag check in `Run`.

#### `PrintHelp(Context context)`

Writes the full usage message (command syntax, options list) to the context output.
Called when any of the `-?`, `-h`, or `--help` flags is set.

#### `LoadConfiguration() → ConfigurationLoadResult`

Synchronously invokes `BuildMarkConfigReader.ReadAsync(Environment.CurrentDirectory)`.
Called from both the lint branch of `Run` and from `ProcessBuildNotes` to keep the
async-to-sync bridging in one place.

### Interactions

| Unit / Subsystem         | Role                                                                            |
|--------------------------|---------------------------------------------------------------------------------|
| `Context`                | Provides parsed flags, arguments, and output methods                            |
| `Validation`             | Executes self-validation when `--validate` flag is set                          |
| `BuildMarkConfigReader`  | Called in `Run` (for `--lint`) and `ProcessBuildNotes` to read `.buildmark.yaml`|
| `ConfigurationLoadResult`| Returned by `BuildMarkConfigReader`; `ReportTo(context)` called immediately     |
| `RepoConnectorFactory`   | Creates the connector via `Create(result.Config?.Connector)`                    |
| `BuildInformation`       | Returned by the connector; converted to markdown via `ToMarkdown`               |
