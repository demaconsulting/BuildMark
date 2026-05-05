## Cli

### Verification Approach

The Cli subsystem is verified through `CliTests.cs`, which exercises the `Context`
class directly by constructing instances with various argument combinations and
asserting on the resulting property values. Each test targets a specific flag or
argument combination and validates correct parsing behavior, including error conditions
and output behavior.

### Dependencies

| Mock / Stub          | Reason                                                             |
| -------------------- | ------------------------------------------------------------------ |
| `StringWriter`       | Captures context output for assertion without console side effects |
| In-process arguments | Passed directly to `Context` constructor instead of `args[]`       |

### Test Scenarios

#### Cli_Context_EmptyArguments_CreatesValidContext

**Scenario**: A `Context` is created with no arguments.

**Expected**: Context is created without error; all flags default to false.

**Requirement coverage**: `BuildMark-Cli-Context`

#### Cli_VersionFlag_SetsProperty

**Scenario**: Context is created with `--version` argument.

**Expected**: `Version` property is `true`.

**Requirement coverage**: `BuildMark-Program-Version`

#### Cli_HelpFlag_SetsProperty

**Scenario**: Context is created with `--help` argument.

**Expected**: `Help` property is `true`.

**Requirement coverage**: `BuildMark-Program-Help`

#### Cli_SilentFlag_SetsProperty

**Scenario**: Context is created with `--silent` argument.

**Expected**: `Silent` property is `true`.

**Requirement coverage**: `BuildMark-Program-Silent`

#### Cli_SilentFlag_SuppressesConsoleOutput

**Scenario**: Context is created with `--silent` argument and a write is performed.

**Expected**: Output is suppressed; nothing is written to console.

**Requirement coverage**: `BuildMark-Program-Silent`

#### Cli_BuildVersionFlag_SetsProperty

**Scenario**: Context is created with `--build-version 1.2.3` argument.

**Expected**: `BuildVersion` property equals `"1.2.3"`.

**Requirement coverage**: `BuildMark-Program-BuildVersion`

#### Cli_ReportFlags_SetProperties

**Scenario**: Context is created with `["--report", "output.md", "--depth", "3",
"--include-known-issues"]`.

**Expected**: `ReportFile` equals `"output.md"`; `Depth` equals 3; `IncludeKnownIssues` is true.

**Requirement coverage**: `BuildMark-Program-Report`, `BuildMark-Program-Depth`

#### Cli_LogFlag_CreatesLogFile

**Scenario**: Context is created with `--log path.log` argument.

**Expected**: `Log` property is set; log file is created at the specified path.

**Requirement coverage**: `BuildMark-Program-Log`

#### Cli_ValidateFlag_SetsProperty

**Scenario**: Context is created with `--validate` argument.

**Expected**: `Validate` property is `true`.

**Requirement coverage**: `BuildMark-Program-Validate`

#### Cli_ResultsFlag_SetsProperty

**Scenario**: Context is created with `["--results", "results.trx"]`.

**Expected**: `ResultsFile` property equals `"results.trx"`.

**Requirement coverage**: `BuildMark-Program-Results`

#### Cli_ResultFlag_SetsProperty

**Scenario**: Context is created with `["--result", "results.trx"]` (alias).

**Expected**: `ResultsFile` property equals `"results.trx"`.

**Requirement coverage**: `BuildMark-Program-Results`

#### Cli_ErrorOutput_WritesToStderr

**Scenario**: `context.WriteError` is called with an error message.

**Expected**: The message `"Subsystem error test"` appears in the standard error stream.

**Requirement coverage**: `BuildMark-Program-ErrorHandling`

#### Cli_InvalidArgument_ThrowsException

**Scenario**: Context is created with `["--unsupported"]`.

**Expected**: `ArgumentException` is thrown with a message containing
`"Unsupported argument '--unsupported'"`.

**Requirement coverage**: `BuildMark-Cli-Context`

#### Cli_MissingArgumentValue_ThrowsException

**Scenario**: Context is created with `["--build-version"]` (value missing).

**Expected**: `ArgumentException` is thrown with a message containing
`"--build-version requires a version argument"`.

**Requirement coverage**: `BuildMark-Cli-Context`

#### Cli_ExitCode_DefaultsToZero

**Scenario**: Context is created; `ExitCode` property is read without any errors.

**Expected**: `ExitCode` is 0.

**Requirement coverage**: `BuildMark-Cli-Context`

#### Cli_WriteError_SetsExitCodeToOne

**Scenario**: `context.WriteError` is called.

**Expected**: `ExitCode` is 1 after the call.

**Requirement coverage**: `BuildMark-Program-ErrorHandling`

#### Cli_VersionShortFlag_SetsProperty

**Scenario**: Context is created with `-v` short argument.

**Expected**: `Version` property is `true`.

**Requirement coverage**: `BuildMark-Program-Version`

#### Cli_HelpShortFlags_SetProperty

**Scenario**: Context is created with `-h` or `-?` short arguments.

**Expected**: `Help` property is `true`.

**Requirement coverage**: `BuildMark-Program-Help`

#### Cli_LintFlag_SetsProperty

**Scenario**: Context is created with `--lint` argument.

**Expected**: `Lint` property is `true`.

**Requirement coverage**: `BuildMark-Program-Lint`

### Requirements Coverage

- **BuildMark-Cli-Context**: Cli_Context_EmptyArguments_CreatesValidContext,
  Cli_InvalidArgument_ThrowsException, Cli_MissingArgumentValue_ThrowsException,
  Cli_ExitCode_DefaultsToZero
- **BuildMark-Program-Version**: Cli_VersionFlag_SetsProperty, Cli_VersionShortFlag_SetsProperty
- **BuildMark-Program-Help**: Cli_HelpFlag_SetsProperty, Cli_HelpShortFlags_SetProperty
- **BuildMark-Program-Silent**: Cli_SilentFlag_SetsProperty,
  Cli_SilentFlag_SuppressesConsoleOutput
- **BuildMark-Program-BuildVersion**: Cli_BuildVersionFlag_SetsProperty
- **BuildMark-Program-Report**: Cli_ReportFlags_SetProperties
- **BuildMark-Program-Depth**: Cli_ReportFlags_SetProperties
- **BuildMark-Program-Log**: Cli_LogFlag_CreatesLogFile
- **BuildMark-Program-Validate**: Cli_ValidateFlag_SetsProperty
- **BuildMark-Program-Results**: Cli_ResultsFlag_SetsProperty, Cli_ResultFlag_SetsProperty
- **BuildMark-Program-Lint**: Cli_LintFlag_SetsProperty
- **BuildMark-Program-ErrorHandling**: Cli_ErrorOutput_WritesToStderr,
  Cli_WriteError_SetsExitCodeToOne
