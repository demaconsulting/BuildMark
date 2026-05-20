# BuildMark

## Verification Strategy

BuildMark is verified at two levels:

**System-level (integration)** testing is provided by `IntegrationTests.cs`, which runs the
BuildMark executable end-to-end via `Runner.Run()` (`dotnet <dll>`). These tests invoke the
full compiled binary, exercising the complete pipeline from CLI argument parsing through build
notes generation, and validate exit codes and console output without any in-process mocking.

**Unit-level** testing is provided by `ProgramTests.cs`, which calls `Program.Run()` directly
with a controlled `Context` object. These tests validate individual flags and error conditions
with fast, isolated, in-process invocations.

The `RepoConnectorsTests.cs` file exercises the full data pipeline, from connector factory
creation through `GetBuildInformationAsync`, using mock data to cover GitHub, Azure DevOps, and
Mock connector paths.

Self-test (`--validate`) is covered by `BuildMark_ValidateFlag_RunsSelfValidation` in
`IntegrationTests.cs` and the self-test suite in `ValidationTests.cs`. The CI pipeline
additionally runs the full build notes generation chain with live GitHub metadata to confirm
end-to-end operation.

## Dependencies

| Mock / Stub              | Reason                                                        |
| ------------------------ | ------------------------------------------------------------- |
| `MockRepoConnector`      | Provides deterministic repository data without live API calls |
| `MockHttpMessageHandler` | Used by GraphQL/REST client unit tests                        |
| Context output capture   | Replaces `Console.Out` with `StringWriter` for assertion      |

## Test Environment

Tests run via `dotnet test` on the CI matrix (Windows, Ubuntu, macOS) against .NET 8, 9,
and 10. No external services are required for unit and integration tests; all HTTP
communication is intercepted by `MockHttpMessageHandler`. A live GitHub Actions environment
is used for the end-to-end CI validation of the report generation pipeline.

## Acceptance Criteria

The system-level test run passes when: all automated tests in `ProgramTests.cs` and
`RepoConnectorsTests.cs` complete with zero failures; the CI pipeline executes the
end-to-end build notes generation step without error; and there are no unresolved
anomalies of Error severity.

## Test Scenarios (System-Level)

### BuildMark_VersionFlag_OutputsVersion

**Scenario**: BuildMark executable is invoked with the `--version` flag via `Runner.Run()`.

**Expected**: Version string is written to output; exit code is 0.

**Requirement coverage**: `BuildMark-Command-Version`

### BuildMark_HelpFlag_OutputsUsageInformation

**Scenario**: BuildMark executable is invoked with the `--help` flag via `Runner.Run()`.

**Expected**: Usage information including available options is written to output; exit code is 0.

**Requirement coverage**: `BuildMark-Command-Help`

### BuildMark_SilentFlag_SuppressesOutput

**Scenario**: BuildMark executable is invoked with `--silent --help` flags via `Runner.Run()`.

**Expected**: No banner is written to output; exit code is 0.

**Requirement coverage**: `BuildMark-Command-Silent`

### BuildMark_InvalidArgument_ShowsError

**Scenario**: BuildMark executable is invoked with an unrecognized argument via `Runner.Run()`.

**Expected**: An error message containing "Unsupported argument" is written to output; exit code is 1.

**Requirement coverage**: `BuildMark-Command-ExitCode`

### BuildMark_ValidateFlag_RunsSelfValidation

**Scenario**: BuildMark executable is invoked with the `--validate` flag via `Runner.Run()`.

**Expected**: Self-validation output is written; exit code is 0.

**Requirement coverage**: `BuildMark-Validation-SelfValidation`

### BuildMark_LogParameter_IsAccepted

**Scenario**: BuildMark executable is invoked with `--log test.log --help` via `Runner.Run()`.

**Expected**: Exit code is 0; no "Unsupported argument" error in output.

**Requirement coverage**: `BuildMark-Command-Log`

### BuildMark_ReportParameter_IsAccepted

**Scenario**: BuildMark executable is invoked with `--report output.md --help` via `Runner.Run()`.

**Expected**: Exit code is 0; no "Unsupported argument" error in output.

**Requirement coverage**: `BuildMark-Report-Markdown`

### BuildMark_DepthParameter_IsAccepted

**Scenario**: BuildMark executable is invoked with `--depth 2 --help` via `Runner.Run()`.

**Expected**: Exit code is 0; no "Unsupported argument" error in output.

**Requirement coverage**: `BuildMark-Command-Depth`

### BuildMark_BuildVersionParameter_IsAccepted

**Scenario**: BuildMark executable is invoked with `--build-version 1.0.0 --help` via `Runner.Run()`.

**Expected**: Exit code is 0; no "Unsupported argument" error in output.

**Requirement coverage**: `BuildMark-Command-BuildVersion`

### BuildMark_ResultsParameter_IsAccepted

**Scenario**: BuildMark executable is invoked with `--results results.trx --help` via `Runner.Run()`.

**Expected**: Exit code is 0; no "Unsupported argument" error in output.

**Requirement coverage**: `BuildMark-Command-Results`

### BuildMark_LintFlag_IsAccepted

**Scenario**: BuildMark executable is invoked with the `--lint` flag via `Runner.Run()`.

**Expected**: Exit code is 0; no "Unsupported argument" error in output.

**Requirement coverage**: `BuildMark-Config-Lint`

### Program_Version_ReturnsValidVersion

**Scenario**: `Program.Version` property is accessed.

**Expected**: Returns a non-null, non-empty version string in semver format.

**Requirement coverage**: `BuildMark-Command-Version`

### Program_Run_VersionFlag_OutputsVersionToConsole

**Scenario**: `Program.Run` is called with a context having `Version = true`.

**Expected**: Version string is written to context output; exit code is 0.

**Requirement coverage**: `BuildMark-Command-Version`

### Program_Run_HelpFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with a context having `Help = true`.

**Expected**: Help text is written to context output; exit code is 0.

**Requirement coverage**: `BuildMark-Command-Help`

### Program_Run_QuestionMarkFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with `?` flag.

**Expected**: Help text is written to context output; exit code is 0.

**Requirement coverage**: `BuildMark-Command-Help`

### Program_Run_LongHelpFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with `--help` flag.

**Expected**: Help text is written to context output; exit code is 0.

**Requirement coverage**: `BuildMark-Command-Help`

### Program_Run_ValidateFlag_OutputsValidationMessage

**Scenario**: `Program.Run` is called with `Validate = true`.

**Expected**: Validation output is written; self-test completes; exit code is 0.

**Requirement coverage**: `BuildMark-Validation-SelfValidation`

### Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues

**Scenario**: `Program.Run` is called with report and include-known-issues flags set.

**Expected**: Build notes report is generated including known issues section; exit code is 0.

**Requirement coverage**: `BuildMark-Report-Markdown`

### Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero

**Scenario**: `Program.Run` is called with `Lint = true` but no configuration file present.

**Expected**: Exit code remains 0 (lint with no config is not an error).

**Requirement coverage**: `BuildMark-Config-Lint`

### Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode

**Scenario**: `Program.Run` is called with an invalid build version string.

**Expected**: Error message is written to stderr; exit code is 1.

**Requirement coverage**: `BuildMark-Program-ErrorHandling-InvalidBuildVersion`

### Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode

**Scenario**: `Program.Run` is called but the connector factory throws `InvalidOperationException`.

**Expected**: Error message is written to stderr; exit code is 1.

**Requirement coverage**: `BuildMark-Program-ErrorHandling-ConnectorFailure`

## Requirements Coverage

- **BuildMark-Command-Version**: BuildMark_VersionFlag_OutputsVersion,
  Program_Version_ReturnsValidVersion, Program_Run_VersionFlag_OutputsVersionToConsole
- **BuildMark-Command-Help**: BuildMark_HelpFlag_OutputsUsageInformation,
  Program_Run_HelpFlag_OutputsHelpMessage,
  Program_Run_QuestionMarkFlag_OutputsHelpMessage, Program_Run_LongHelpFlag_OutputsHelpMessage
- **BuildMark-Validation-SelfValidation**: BuildMark_ValidateFlag_RunsSelfValidation,
  Program_Run_ValidateFlag_OutputsValidationMessage
- **BuildMark-Report-Markdown**: BuildMark_ReportParameter_IsAccepted,
  Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues
- **BuildMark-Config-Lint**: BuildMark_LintFlag_IsAccepted,
  Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero
- **BuildMark-Command-ExitCode**: BuildMark_InvalidArgument_ShowsError
- **BuildMark-Program-ErrorHandling-InvalidBuildVersion**:
  Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode
- **BuildMark-Program-ErrorHandling-ConnectorFailure**:
  Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode
