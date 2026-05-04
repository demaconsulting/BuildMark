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

**Requirement coverage**: `BuildMark-Program-ErrorHandling-InvalidBuildVersion`

**Scenario**: `Program.Run` is called but connector factory throws `InvalidOperationException`.

**Expected**: Error is written to stderr; exit code is 1.

**Requirement coverage**: `BuildMark-Program-ErrorHandling-ConnectorFailure`

### Program_Run_WithSilentFlag_SuppressesOutput

**Scenario**: `Program.Run` is called with `--silent` and `--help` flags in context.

**Expected**: Banner text is suppressed from console output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Silent`

### Program_Run_WithLogFlag_WritesToLogFile

**Scenario**: `Program.Run` is called with `--log <file>` flag pointing to a temporary file.

**Expected**: Log file is created and contains non-empty output; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Log`

### Program_Run_WithResultsFlag_WritesResultsFile

**Scenario**: `Program.Run` is called with `--validate` and `--results <file>` flags.

**Expected**: Results file is created; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Results`

### Program_Run_WithBuildVersionFlag_AcceptsBuildVersion

**Scenario**: `Program.Run` is called with `--build-version 3.2.1` and a mock connector.

**Expected**: Report is generated containing the specified version string; exit code is 0.

**Requirement coverage**: `BuildMark-Program-BuildVersion`

### Program_Run_WithDepthFlag_SetsHeadingDepth

**Scenario**: `Program.Run` is called with `--depth 3` and a mock connector.

**Expected**: Report uses level-three headings (`###`) for the title; exit code is 0.

**Requirement coverage**: `BuildMark-Program-Depth`

## Requirements Coverage

- **BuildMark-Program-Version**: Program_Version_ReturnsValidVersion,
  Program_Run_VersionFlag_OutputsVersionToConsole
- **BuildMark-Program-Help**: Program_Run_HelpFlag_OutputsHelpMessage,
  Program_Run_QuestionMarkFlag_OutputsHelpMessage, Program_Run_LongHelpFlag_OutputsHelpMessage
- **BuildMark-Program-Validate**: Program_Run_ValidateFlag_OutputsValidationMessage
- **BuildMark-Program-Report**: Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues
- **BuildMark-Program-Lint**: Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero
- **BuildMark-Program-Silent**: Program_Run_WithSilentFlag_SuppressesOutput
- **BuildMark-Program-Log**: Program_Run_WithLogFlag_WritesToLogFile
- **BuildMark-Program-Results**: Program_Run_WithResultsFlag_WritesResultsFile
- **BuildMark-Program-BuildVersion**: Program_Run_WithBuildVersionFlag_AcceptsBuildVersion
- **BuildMark-Program-Depth**: Program_Run_WithDepthFlag_SetsHeadingDepth
- **BuildMark-Program-ErrorHandling-InvalidBuildVersion**: Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode
- **BuildMark-Program-ErrorHandling-ConnectorFailure**: Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode
