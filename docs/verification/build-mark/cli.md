## Cli

### Verification Approach

The Cli subsystem is verified through `CliTests.cs` (subsystem-level) and `ContextTests.cs`
(unit-level). `CliTests.cs` exercises the `Context` class directly by constructing instances with
various argument combinations and asserting on the resulting property values. Each test targets a
specific flag or argument combination and validates correct parsing behavior, including error
conditions and output behavior. `StringWriter` captures context output for assertion without console
side effects, and arguments are passed directly to the `Context` constructor rather than through
`args[]`.

### Test Environment

N/A - standard test environment. Both `CliTests.cs` and `ContextTests.cs` run within the standard
`dotnet test` host; no external services, live network, or file system side effects beyond an
in-process `StringWriter` and temporary log files are required.

### Acceptance Criteria

- All tests in `CliTests.cs` pass with zero failures.
- All `BuildMark-Cli-*` and referenced `BuildMark-Program-*` requirements have at least one
  test in the subsystem test file.

### Test Scenarios

**Cli_Context_EmptyArguments_CreatesValidContext**: Verifies that a `Context` created with no
arguments initializes successfully with all flags defaulting to false.
This scenario is tested by `Cli_Context_EmptyArguments_CreatesValidContext`.

**Cli_VersionFlag_SetsProperty**: Verifies that constructing a context with `--version` sets the
`Version` property to true. This scenario is tested by `Cli_VersionFlag_SetsProperty`.

**Cli_HelpFlag_SetsProperty**: Verifies that constructing a context with `--help` sets the `Help`
property to true. This scenario is tested by `Cli_HelpFlag_SetsProperty`.

**Cli_SilentFlag_SetsProperty**: Verifies that constructing a context with `--silent` sets the
`Silent` property to true. This scenario is tested by `Cli_SilentFlag_SetsProperty`.

**Cli_SilentFlag_SuppressesConsoleOutput**: Verifies that a context created with `--silent` does
not write to the console when a write is performed, confirming output suppression.
This scenario is tested by `Cli_SilentFlag_SuppressesConsoleOutput`.

**Cli_BuildVersionFlag_SetsProperty**: Verifies that `--build-version 1.2.3` sets the
`BuildVersion` property to `"1.2.3"`. This scenario is tested by
`Cli_BuildVersionFlag_SetsProperty`.

**Cli_ReportFlags_SetProperties**: Verifies that `--report output.md --depth 3
--include-known-issues` sets `ReportFile` to `"output.md"`, `Depth` to 3, and `IncludeKnownIssues`
to true. This scenario is tested by `Cli_ReportFlags_SetProperties`.

**Cli_LogFlag_CreatesLogFile**: Verifies that `--log path.log` sets the `Log` property and creates
the log file at the specified path.
This scenario is tested by `Cli_LogFlag_CreatesLogFile`.

**Cli_ValidateFlag_SetsProperty**: Verifies that `--validate` sets the `Validate` property to
true. This scenario is tested by `Cli_ValidateFlag_SetsProperty`.

**Cli_ResultsFlag_SetsProperty**: Verifies that `--results results.trx` sets `ResultsFile` to
`"results.trx"`. This scenario is tested by `Cli_ResultsFlag_SetsProperty`.

**Cli_ResultFlag_SetsProperty**: Verifies that the `--result` alias sets `ResultsFile` to
`"results.trx"` identically to `--results`.
This scenario is tested by `Cli_ResultFlag_SetsProperty`.

**Cli_ErrorOutput_WritesToStderr**: Verifies that `context.WriteError` writes the error message
to the standard error stream. This scenario is tested by `Cli_ErrorOutput_WritesToStderr`.

**Cli_InvalidArgument_ThrowsException**: Verifies that an unrecognized argument causes an
`ArgumentException` containing "Unsupported argument '--unsupported'".
This scenario is tested by `Cli_InvalidArgument_ThrowsException`.

**Cli_MissingArgumentValue_ThrowsException**: Verifies that `--build-version` without a following
value causes an `ArgumentException` containing the missing-value message.
This scenario is tested by `Cli_MissingArgumentValue_ThrowsException`.

**Cli_ExitCode_DefaultsToZero**: Verifies that a freshly created context has `ExitCode` of 0
before any errors are written.
This scenario is tested by `Cli_ExitCode_DefaultsToZero`.

**Cli_WriteError_SetsExitCodeToOne**: Verifies that calling `context.WriteError` sets `ExitCode`
to 1. This scenario is tested by `Cli_WriteError_SetsExitCodeToOne`.

**Cli_VersionShortFlag_SetsProperty**: Verifies that the `-v` short argument sets the `Version`
property to true. This scenario is tested by `Cli_VersionShortFlag_SetsProperty`.

**Cli_HelpShortFlags_SetProperty**: Verifies that `-h` and `-?` short arguments set the `Help`
property to true. This scenario is tested by `Cli_HelpShortFlags_SetProperty`.

**Cli_LintFlag_SetsProperty**: Verifies that `--lint` sets the `Lint` property to true.
This scenario is tested by `Cli_LintFlag_SetsProperty`.
