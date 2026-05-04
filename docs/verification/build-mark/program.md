# Program

## Verification Approach

`Program` unit tests are in `ProgramTests.cs`. Each test constructs a `Context` object
with controlled arguments and output capture, calls `Program.Run`, then asserts on the
context output and exit code. The connector factory is injected via a context override
to avoid live API calls where needed.

## Dependencies

| Mock / Stub            | Reason                                                   |
| ---------------------- | -------------------------------------------------------- |
| `Context`              | Constructed with controlled arguments and output capture |
| Connector factory mock | Injected to avoid live API calls                         |

## Test Scenarios

### Program_Version_ReturnsValidVersion

**Scenario**: `Program.Version` is accessed directly.

**Expected**: Returns a non-null semver string.

**Requirement coverage**: `BuildMark-Program-Version`

### Program_Run_VersionFlag_OutputsVersionToConsole

**Scenario**: `Program.Run` is called with `Version = true` in context.

**Expected**: Version string appears in output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Version`

### Program_Run_HelpFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with `Help = true` in context.

**Expected**: Help text appears in output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Help`

### Program_Run_QuestionMarkFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with `?` argument.

**Expected**: Help text appears in output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Help`

### Program_Run_LongHelpFlag_OutputsHelpMessage

**Scenario**: `Program.Run` is called with `--help` argument.

**Expected**: Help text appears in output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Help`

### Program_Run_ValidateFlag_OutputsValidationMessage

**Scenario**: `Program.Run` is called with `Validate = true`.

**Expected**: Validation output appears; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Validate`

### Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues

**Scenario**: `Program.Run` is called with report flags and `IncludeKnownIssues = true`.

**Expected**: Report is generated including known issues; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Report`

### Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero

**Scenario**: `Program.Run` is called with `Lint = true` but no config present.

**Expected**: Exit code is 0.

**Requirement coverage**: `BuildMark-Program-Lint`

### Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode

**Scenario**: `Program.Run` is called with an invalid build version.

**Expected**: Error is written to stderr; exit code is 1.

**Requirement coverage**: `BuildMark-Program-ErrorHandling`

### Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode

**Scenario**: `Program.Run` is called but connector factory throws `InvalidOperationException`.

**Expected**: Error is written to stderr; exit code is 1.

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
