### Context Verification

This document describes the unit-level verification design for the `Context` unit. It defines the
test scenarios, dependency usage, and requirement coverage for `Cli/Context.cs`.

#### Verification Approach

`Context` is verified with unit tests defined in `ContextTests.cs`. Because `Context` depends only
on .NET base class library types (`Console`, `StreamWriter`, `Path`), no mocking or test doubles
are required. Tests call `Context.Create` with controlled argument arrays, inspect the resulting
properties and exit codes, and verify output written to captured streams.

#### Dependencies

`Context` has no dependencies on other tool units. All dependencies are real .NET BCL types;
no mocking is needed at this level.

#### Test Scenarios

##### Context_Create_EmptyArguments_CreatesValidContext

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: All boolean flags are false; `ResultsFile` is null; `Depth` is null;
exit code is 0.

**Requirement coverage**: `BuildMark-Context-DefaultConstruction`.

##### Context_Create_ShortVersionFlag_SetsVersionProperty

**Scenario**: `Context.Create` is called with `["-v"]`.

**Expected**: `Version` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_LongVersionFlag_SetsVersionProperty

**Scenario**: `Context.Create` is called with `["--version"]`.

**Expected**: `Version` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_QuestionMarkHelpFlag_SetsHelpProperty

**Scenario**: `Context.Create` is called with `["-?"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_ShortHelpFlag_SetsHelpProperty

**Scenario**: `Context.Create` is called with `["-h"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_LongHelpFlag_SetsHelpProperty

**Scenario**: `Context.Create` is called with `["--help"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_SilentFlag_SetsSilentProperty

**Scenario**: `Context.Create` is called with `["--silent"]`.

**Expected**: `Silent` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_ValidateFlag_SetsValidateProperty

**Scenario**: `Context.Create` is called with `["--validate"]`.

**Expected**: `Validate` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_LintFlag_SetsLintProperty

**Scenario**: `Context.Create` is called with `["--lint"]`.

**Expected**: `Lint` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_BuildVersionArgument_SetsBuildVersionProperty

**Scenario**: `Context.Create` is called with `["--build-version", "1.2.3"]`.

**Expected**: `BuildVersion` property equals `"1.2.3"`.

**Requirement coverage**: `BuildMark-Context-ArgumentParsing`.

##### Context_Create_ReportArgument_SetsReportFileProperty

**Scenario**: `Context.Create` is called with `["--report", "output.md"]`.

**Expected**: `ReportFile` property equals `"output.md"`.

**Requirement coverage**: `BuildMark-Context-ArgumentParsing`.

##### Context_Create_DepthArgument_SetsDepthProperty

**Scenario**: `Context.Create` is called with `["--depth", "3"]`.

**Expected**: `Depth` property equals 3.

**Requirement coverage**: `BuildMark-Context-ArgumentParsing`.

##### Context_Create_LegacyReportDepthArgument_SetsDepthProperty

**Scenario**: `Context.Create` is called with `["--report-depth", "3"]` (legacy alias for
`--depth`).

**Expected**: `Depth` property equals 3.

**Requirement coverage**: `BuildMark-Context-ArgumentParsing`.

##### Context_Create_IncludeKnownIssuesFlag_SetsIncludeKnownIssuesProperty

**Scenario**: `Context.Create` is called with `["--include-known-issues"]`.

**Expected**: `IncludeKnownIssues` property is true.

**Requirement coverage**: `BuildMark-Context-FlagParsing`.

##### Context_Create_ResultsArgument_SetsResultsFileProperty

**Scenario**: `Context.Create` is called with `["--results", "output.trx"]`.

**Expected**: `ResultsFile` property equals `"output.trx"`.

**Requirement coverage**: `BuildMark-Context-ArgumentParsing`.

##### Context_Create_ResultArgument_SetsResultsFileProperty

**Scenario**: `Context.Create` is called with `["--result", "output.trx"]` (alias).

**Expected**: `ResultsFile` property equals `"output.trx"`.

**Requirement coverage**: `BuildMark-Context-ArgumentParsing`.

##### Context_Create_LogArgument_CreatesLogFile

**Scenario**: `Context.Create` is called with `["--log", "<tmp>.log"]`; `WriteLine` is then called
with a test message.

**Expected**: The log file is created; the test message is written to it.

**Requirement coverage**: `BuildMark-Context-LogFile`.

##### Context_Create_MultipleArguments_SetsAllPropertiesCorrectly

**Scenario**: `Context.Create` is called with `["--silent", "--validate", "--lint",
"--build-version", "1.2.3", "--report", "report.md", "--depth", "2",
"--include-known-issues", "--results", "results.trx"]`.

**Expected**: `Silent`, `Validate`, `Lint`, and `IncludeKnownIssues` are true; `BuildVersion`
equals `"1.2.3"`; `ReportFile` equals `"report.md"`; `Depth` equals 2; `ResultsFile` equals
`"results.trx"`; no exception is thrown.

**Requirement coverage**: `BuildMark-Context-FlagParsing`, `BuildMark-Context-ArgumentParsing`.

##### Context_Create_UnsupportedArgument_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--unsupported"]`.

**Expected**: An `ArgumentException` is thrown containing the text "Unsupported argument".

**Boundary / error path**: Unknown argument rejection.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_BuildVersionWithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--build-version"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_ReportWithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--report"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_DepthWithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_DepthWithNonIntegerValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "abc"]`.

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_DepthWithZeroValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "0"]` (below minimum of 1).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_DepthWithNegativeValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "-1"]` (negative value).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_DepthExceedingMaximum_ThrowsArgumentOutOfRangeException

**Scenario**: `Context.Create` is called with `["--depth", "7"]` (above the maximum of 6).

**Expected**: An `ArgumentOutOfRangeException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_ResultsWithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--results"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_ResultWithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--result"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_LogWithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--log"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_Create_InvalidLogFilePath_ThrowsInvalidOperationException

**Scenario**: `Context.Create` is called with `["--log", "<invalid-path>"]`.

**Expected**: An `InvalidOperationException` is thrown when the log file cannot be created.

**Requirement coverage**: `BuildMark-Context-ErrorHandling`.

##### Context_WriteLine_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` is created and `WriteLine` is called with a test message.

**Expected**: The test message appears on standard output.

**Requirement coverage**: `BuildMark-Context-Output`.

##### Context_WriteLine_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` (created with `["--silent"]`) calls `WriteLine`.

**Expected**: Standard output receives nothing.

**Requirement coverage**: `BuildMark-Context-SilentMode`.

##### Context_WriteLine_WithLogFile_WritesToLogFile

**Scenario**: A `Context` created with a log file calls `WriteLine` with a test message.

**Expected**: The test message appears in the log file.

**Requirement coverage**: `BuildMark-Context-LogFile`.

##### Context_WriteError_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` calls `WriteError` with a test message.

**Expected**: The test message appears on standard error.

**Requirement coverage**: `BuildMark-Context-Output`.

##### Context_WriteError_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` calls `WriteError`.

**Expected**: Standard error receives nothing.

**Requirement coverage**: `BuildMark-Context-SilentMode`.

##### Context_WriteError_WithLogFile_WritesToLogFile

**Scenario**: A `Context` created with a log file calls `WriteError` with a test message.

**Expected**: The test message appears in the log file.

**Requirement coverage**: `BuildMark-Context-LogFile`.

##### Context_WriteError_SetsExitCodeToOne

**Scenario**: A `Context` calls `WriteError`.

**Expected**: `ExitCode` is 1 after the call.

**Requirement coverage**: `BuildMark-Context-ExitCode`.

##### Context_ExitCode_NoErrors_RemainsZero

**Scenario**: A `Context` is created and no errors are written.

**Expected**: `ExitCode` remains 0.

**Requirement coverage**: `BuildMark-Context-ExitCode`.

##### Context_Dispose_ClosesLogFileProperly

**Scenario**: A `Context` with a log file is disposed.

**Expected**: The log file is properly closed without error.

**Requirement coverage**: `BuildMark-Context-LogFile`.

#### Requirements Coverage

- **`BuildMark-Context-DefaultConstruction`**:
  - Context_Create_EmptyArguments_CreatesValidContext
- **`BuildMark-Context-FlagParsing`**:
  - Context_Create_ShortVersionFlag_SetsVersionProperty
  - Context_Create_LongVersionFlag_SetsVersionProperty
  - Context_Create_QuestionMarkHelpFlag_SetsHelpProperty
  - Context_Create_ShortHelpFlag_SetsHelpProperty
  - Context_Create_LongHelpFlag_SetsHelpProperty
  - Context_Create_SilentFlag_SetsSilentProperty
  - Context_Create_ValidateFlag_SetsValidateProperty
  - Context_Create_LintFlag_SetsLintProperty
  - Context_Create_IncludeKnownIssuesFlag_SetsIncludeKnownIssuesProperty
  - Context_Create_MultipleArguments_SetsAllPropertiesCorrectly
- **`BuildMark-Context-ArgumentParsing`**:
  - Context_Create_BuildVersionArgument_SetsBuildVersionProperty
  - Context_Create_ReportArgument_SetsReportFileProperty
  - Context_Create_DepthArgument_SetsDepthProperty
  - Context_Create_LegacyReportDepthArgument_SetsDepthProperty
  - Context_Create_ResultsArgument_SetsResultsFileProperty
  - Context_Create_ResultArgument_SetsResultsFileProperty
  - Context_Create_MultipleArguments_SetsAllPropertiesCorrectly
- **`BuildMark-Context-LogFile`**:
  - Context_Create_LogArgument_CreatesLogFile
  - Context_WriteLine_WithLogFile_WritesToLogFile
  - Context_WriteError_WithLogFile_WritesToLogFile
  - Context_Dispose_ClosesLogFileProperly
- **`BuildMark-Context-ErrorHandling`**:
  - Context_Create_UnsupportedArgument_ThrowsArgumentException
  - Context_Create_BuildVersionWithoutValue_ThrowsArgumentException
  - Context_Create_ReportWithoutValue_ThrowsArgumentException
  - Context_Create_DepthWithoutValue_ThrowsArgumentException
  - Context_Create_DepthWithNonIntegerValue_ThrowsArgumentException
  - Context_Create_DepthWithZeroValue_ThrowsArgumentException
  - Context_Create_DepthWithNegativeValue_ThrowsArgumentException
  - Context_Create_DepthExceedingMaximum_ThrowsArgumentOutOfRangeException
  - Context_Create_ResultsWithoutValue_ThrowsArgumentException
  - Context_Create_ResultWithoutValue_ThrowsArgumentException
  - Context_Create_LogWithoutValue_ThrowsArgumentException
  - Context_Create_InvalidLogFilePath_ThrowsInvalidOperationException
- **`BuildMark-Context-Output`**:
  - Context_WriteLine_NotSilent_WritesToConsole
  - Context_WriteError_NotSilent_WritesToConsole
- **`BuildMark-Context-SilentMode`**:
  - Context_WriteLine_Silent_DoesNotWriteToConsole
  - Context_WriteError_Silent_DoesNotWriteToConsole
- **`BuildMark-Context-ExitCode`**:
  - Context_WriteError_SetsExitCodeToOne
  - Context_ExitCode_NoErrors_RemainsZero
