# BuildMark

## Verification Approach

BuildMark is verified at the system level through a set of integration and end-to-end
tests that exercise the full pipeline from CLI invocation to build notes generation.
The `ProgramTests.cs` file exercises the entry point with all supported flags and
validates both exit codes and console output. The `RepoConnectorsTests.cs` file
exercises the full data pipeline, from connector factory creation through
`GetBuildInformationAsync`, using mock data to cover GitHub, Azure DevOps, and Mock
connector paths.

Self-test (`--validate`) is covered by `Program_Run_ValidateFlag_OutputsValidationMessage`
and the self-test suite in `ValidationTests.cs`. The CI pipeline additionally runs
the full build notes generation chain with live GitHub metadata to confirm end-to-end
operation.

## Dependencies

| Mock / Stub              | Reason                                                        |
| ------------------------ | ------------------------------------------------------------- |
| `MockRepoConnector`      | Provides deterministic repository data without live API calls |
| `MockHttpMessageHandler` | Used by GraphQL/REST client unit tests                        |
| Context output capture   | Replaces `Console.Out` with `StringWriter` for assertion      |

## Test Scenarios (System-Level)

### Program_Version_ReturnsValidVersion

**Scenario**: `Program.Version` property is accessed.

**Expected**: Returns a non-null, non-empty version string in semver format.

**Requirement coverage**: `BuildMark-Program-Version`

### Program_Run_VersionFlag_OutputsVersionToConsole

**Scenario**: `Program.Run` is called with a context having `Version = true`.

**Expected**: Version string is written to context output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Version`

### Program_Run_HelpFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with a context having `Help = true`.

**Expected**: Help text is written to context output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Help`

### Program_Run_QuestionMarkFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with `?` flag.

**Expected**: Help text is written to context output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Help`

### Program_Run_LongHelpFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with `--help` flag.

**Expected**: Help text is written to context output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Help`

### Program_Run_ValidateFlag_OutputsValidationMessage

**Scenario**: `Program.Run` is called with `Validate = true`.

**Expected**: Validation output is written; self-test completes; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Validate`

### Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues

**Scenario**: `Program.Run` is called with report and include-known-issues flags set.

**Expected**: Build notes report is generated including known issues section; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Report`

### Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero

**Scenario**: `Program.Run` is called with `Lint = true` but no configuration file present.

**Expected**: Exit code remains 0 (lint with no config is not an error).

**Requirement coverage**: `BuildMark-Program-Lint`

### Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode

**Scenario**: `Program.Run` is called with an invalid build version string.

**Expected**: Error message is written to stderr; exit code is 1.

**Requirement coverage**: `BuildMark-Program-ErrorHandling`

### Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode

**Scenario**: `Program.Run` is called but the connector factory throws `InvalidOperationException`.

**Expected**: Error message is written to stderr; exit code is 1.

**Requirement coverage**: `BuildMark-Program-ErrorHandling`

## Requirements Coverage

- **BuildMark-Program-Version**: Program_Version_ReturnsValidVersion,
  Program_Run_VersionFlag_OutputsVersionToConsole
- **BuildMark-Program-Help**: Program_Run_HelpFlag_OutputsHelpMessage,
  Program_Run_QuestionMarkFlag_OutputsHelpMessage, Program_Run_LongHelpFlag_OutputsHelpMessage
- **BuildMark-Program-Validate**: Program_Run_ValidateFlag_OutputsValidationMessage
- **BuildMark-Program-Report**: Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues
- **BuildMark-Program-Lint**: Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero
- **BuildMark-Program-ErrorHandling**: Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode,
  Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode
