## Program

### Verification Approach

`Program` unit tests are in `ProgramTests.cs`. Each test constructs a `Context` object with
controlled arguments and output capture, calls `Program.Run`, then asserts on the context output
and exit code. The connector factory is injected via a context override to avoid live API calls
where needed. No framework mocking library is used; a local `ThrowingConnector` stub simulates
connector failure conditions.

### Test Environment

N/A - standard test environment. `ProgramTests.cs` runs within the standard `dotnet test` host;
no external services, live network, or file system side effects are required beyond a temporary
directory used by report-generation tests.

### Acceptance Criteria

- All tests in `ProgramTests.cs` pass with zero failures.
- Exit code assertions cover both success (0) and error (1) paths.

### Test Scenarios

**Program_Version_ReturnsValidVersion**: Verifies that `Program.Version` returns a non-null,
non-empty version string in semver format, confirming version metadata is embedded in the assembly.
This scenario is tested by `Program_Version_ReturnsValidVersion`.

**Program_Run_VersionFlag_OutputsVersionToConsole**: Verifies that calling `Program.Run` with a
context having `Version = true` writes the version string to standard output and exits with code 0.
This scenario is tested by `Program_Run_VersionFlag_OutputsVersionToConsole`.

**Program_Run_HelpFlag_OutputsHelpMessage**: Verifies that calling `Program.Run` with `-h` writes
usage text including "Usage: buildmark" and "Options:" to standard output and exits with code 0.
This scenario is tested by `Program_Run_HelpFlag_OutputsHelpMessage`.

**Program_Run_QuestionMarkFlag_OutputsHelpMessage**: Verifies that the `-?` short flag produces the
same help output as `-h`. This scenario is tested by
`Program_Run_QuestionMarkFlag_OutputsHelpMessage`.

**Program_Run_LongHelpFlag_OutputsHelpMessage**: Verifies that the `--help` long flag produces
standard help output and exits with code 0. This scenario is tested by
`Program_Run_LongHelpFlag_OutputsHelpMessage`.

**Program_Run_ValidateFlag_OutputsValidationMessage**: Verifies that calling `Program.Run` with
`Validate = true` writes output containing "Self-validation" and exits with code 0.
This scenario is tested by `Program_Run_ValidateFlag_OutputsValidationMessage`.

**Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues**: Verifies that
`Program.Run` with report and `IncludeKnownIssues = true` flags creates the report file and exits
with code 0. This scenario is tested by
`Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues`.

**Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero**: Verifies that running with
`Lint = true` when no configuration file is present leaves the exit code at 0, confirming lint
mode is not an error when no config exists.
This scenario is tested by `Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero`.

**Program_Run_WithSilentFlag_SuppressesOutput**: Verifies that `--silent --help` suppresses the
banner text from console output while still exiting with code 0.
This scenario is tested by `Program_Run_WithSilentFlag_SuppressesOutput`.

**Program_Run_WithLogFlag_WritesToLogFile**: Verifies that `--log <file> --help` creates the log
file and writes non-empty output to it, confirming log routing through `Context`.
This scenario is tested by `Program_Run_WithLogFlag_WritesToLogFile`.

**Program_Run_WithResultsFlag_WritesResultsFile**: Verifies that `--validate --results <file>`
creates the results file and exits with code 0.
This scenario is tested by `Program_Run_WithResultsFlag_WritesResultsFile`.

**Program_Run_WithBuildVersionFlag_AcceptsBuildVersion**: Verifies that `--build-version 3.2.1`
produces a report containing the version string "3.2.1" and exits with code 0.
This scenario is tested by `Program_Run_WithBuildVersionFlag_AcceptsBuildVersion`.

**Program_Run_WithDepthFlag_SetsHeadingDepth**: Verifies that `--depth 3` causes the generated
report to use level-three headings (`###`) for the title section.
This scenario is tested by `Program_Run_WithDepthFlag_SetsHeadingDepth`.

**Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode**: Verifies that an invalid build
version string causes an error message on stderr and exits with code 1.
This scenario is tested by `Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode`.

**Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode**: Verifies
that when the connector factory throws `InvalidOperationException`, an error message is written
to stderr and the exit code is 1. This scenario is tested by
`Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode`.
