### Context

#### Verification Approach

`Context` is verified with unit tests in `ContextTests.cs`. Because `Context` depends only on .NET
base class library types (`Console`, `StreamWriter`, `Path`), no mocking or test doubles are
required. Tests call `Context.Create` with controlled argument arrays, inspect the resulting
properties and exit codes, and verify output written to captured streams.

#### Test Environment

N/A - standard test environment. `ContextTests.cs` runs within the standard `dotnet test` host;
no external dependencies are required beyond temporary log files created by log-related tests.

#### Acceptance Criteria

- All tests in `ContextTests.cs` pass with zero failures.
- Flag parsing, argument parsing, output routing, silent mode, log file, exit code, and error
  handling paths are all covered.

#### Test Scenarios

**Context_Create_EmptyArguments_CreatesValidContext**: Verifies that `Context.Create` with an empty
argument array initializes successfully with all boolean flags false, `ResultsFile` null, `Depth`
null, and `ExitCode` 0.
This scenario is tested by `Context_Create_EmptyArguments_CreatesValidContext`.

**Context_Create_ShortVersionFlag_SetsVersionProperty**: Verifies that `["-v"]` sets the `Version`
property to true. This scenario is tested by
`Context_Create_ShortVersionFlag_SetsVersionProperty`.

**Context_Create_LongVersionFlag_SetsVersionProperty**: Verifies that `["--version"]` sets the
`Version` property to true. This scenario is tested by
`Context_Create_LongVersionFlag_SetsVersionProperty`.

**Context_Create_QuestionMarkHelpFlag_SetsHelpProperty**: Verifies that `["-?"]` sets the `Help`
property to true. This scenario is tested by
`Context_Create_QuestionMarkHelpFlag_SetsHelpProperty`.

**Context_Create_ShortHelpFlag_SetsHelpProperty**: Verifies that `["-h"]` sets the `Help` property
to true. This scenario is tested by `Context_Create_ShortHelpFlag_SetsHelpProperty`.

**Context_Create_LongHelpFlag_SetsHelpProperty**: Verifies that `["--help"]` sets the `Help`
property to true. This scenario is tested by `Context_Create_LongHelpFlag_SetsHelpProperty`.

**Context_Create_SilentFlag_SetsSilentProperty**: Verifies that `["--silent"]` sets the `Silent`
property to true. This scenario is tested by `Context_Create_SilentFlag_SetsSilentProperty`.

**Context_Create_ValidateFlag_SetsValidateProperty**: Verifies that `["--validate"]` sets the
`Validate` property to true.
This scenario is tested by `Context_Create_ValidateFlag_SetsValidateProperty`.

**Context_Create_LintFlag_SetsLintProperty**: Verifies that `["--lint"]` sets the `Lint` property
to true. This scenario is tested by `Context_Create_LintFlag_SetsLintProperty`.

**Context_Create_BuildVersionArgument_SetsBuildVersionProperty**: Verifies that
`["--build-version", "1.2.3"]` sets `BuildVersion` to `"1.2.3"`.
This scenario is tested by `Context_Create_BuildVersionArgument_SetsBuildVersionProperty`.

**Context_Create_ReportArgument_SetsReportFileProperty**: Verifies that `["--report", "output.md"]`
sets `ReportFile` to `"output.md"`.
This scenario is tested by `Context_Create_ReportArgument_SetsReportFileProperty`.

**Context_Create_DepthArgument_SetsDepthProperty**: Verifies that `["--depth", "3"]` sets `Depth`
to 3. This scenario is tested by `Context_Create_DepthArgument_SetsDepthProperty`.

**Context_Create_LegacyReportDepthArgument_SetsDepthProperty**: Verifies that the legacy alias
`["--report-depth", "3"]` sets `Depth` to 3 identically to `--depth`.
This scenario is tested by `Context_Create_LegacyReportDepthArgument_SetsDepthProperty`.

**Context_Create_IncludeKnownIssuesFlag_SetsIncludeKnownIssuesProperty**: Verifies that
`["--include-known-issues"]` sets `IncludeKnownIssues` to true.
This scenario is tested by
`Context_Create_IncludeKnownIssuesFlag_SetsIncludeKnownIssuesProperty`.

**Context_Create_ResultsArgument_SetsResultsFileProperty**: Verifies that
`["--results", "output.trx"]` sets `ResultsFile` to `"output.trx"`.
This scenario is tested by `Context_Create_ResultsArgument_SetsResultsFileProperty`.

**Context_Create_ResultArgument_SetsResultsFileProperty**: Verifies that the `--result` alias sets
`ResultsFile` identically to `--results`.
This scenario is tested by `Context_Create_ResultArgument_SetsResultsFileProperty`.

**Context_Create_LogArgument_CreatesLogFile**: Verifies that `["--log", "<tmp>.log"]` creates the
log file and routes a subsequent `WriteLine` call to it.
This scenario is tested by `Context_Create_LogArgument_CreatesLogFile`.

**Context_Create_MultipleArguments_SetsAllPropertiesCorrectly**: Verifies that all flags and
arguments in a combined invocation are parsed correctly in a single context creation.
This scenario is tested by `Context_Create_MultipleArguments_SetsAllPropertiesCorrectly`.

**Context_Create_UnsupportedArgument_ThrowsArgumentException**: Verifies that `["--unsupported"]`
throws an `ArgumentException` containing "Unsupported argument".
This scenario is tested by `Context_Create_UnsupportedArgument_ThrowsArgumentException`.

**Context_Create_BuildVersionWithoutValue_ThrowsArgumentException**: Verifies that
`["--build-version"]` without a value throws an `ArgumentException`.
This scenario is tested by `Context_Create_BuildVersionWithoutValue_ThrowsArgumentException`.

**Context_Create_ReportWithoutValue_ThrowsArgumentException**: Verifies that `["--report"]`
without a value throws an `ArgumentException`.
This scenario is tested by `Context_Create_ReportWithoutValue_ThrowsArgumentException`.

**Context_Create_DepthWithoutValue_ThrowsArgumentException**: Verifies that `["--depth"]` without
a value throws an `ArgumentException`.
This scenario is tested by `Context_Create_DepthWithoutValue_ThrowsArgumentException`.

**Context_Create_DepthWithNonIntegerValue_ThrowsArgumentException**: Verifies that
`["--depth", "abc"]` throws an `ArgumentException`.
This scenario is tested by `Context_Create_DepthWithNonIntegerValue_ThrowsArgumentException`.

**Context_Create_DepthWithZeroValue_ThrowsArgumentException**: Verifies that `["--depth", "0"]`
(below the minimum of 1) throws an `ArgumentException`.
This scenario is tested by `Context_Create_DepthWithZeroValue_ThrowsArgumentException`.

**Context_Create_DepthWithNegativeValue_ThrowsArgumentException**: Verifies that
`["--depth", "-1"]` (negative value) throws an `ArgumentException`.
This scenario is tested by `Context_Create_DepthWithNegativeValue_ThrowsArgumentException`.

**Context_Create_DepthExceedingMaximum_ThrowsArgumentOutOfRangeException**: Verifies that
`["--depth", "7"]` (above the maximum of 6) throws an `ArgumentOutOfRangeException`.
This scenario is tested by
`Context_Create_DepthExceedingMaximum_ThrowsArgumentOutOfRangeException`.

**Context_Create_ResultsWithoutValue_ThrowsArgumentException**: Verifies that `["--results"]`
without a value throws an `ArgumentException`.
This scenario is tested by `Context_Create_ResultsWithoutValue_ThrowsArgumentException`.

**Context_Create_ResultWithoutValue_ThrowsArgumentException**: Verifies that `["--result"]`
without a value throws an `ArgumentException`.
This scenario is tested by `Context_Create_ResultWithoutValue_ThrowsArgumentException`.

**Context_Create_LogWithoutValue_ThrowsArgumentException**: Verifies that `["--log"]` without a
value throws an `ArgumentException`.
This scenario is tested by `Context_Create_LogWithoutValue_ThrowsArgumentException`.

**Context_Create_InvalidLogFilePath_ThrowsInvalidOperationException**: Verifies that supplying an
invalid log file path throws an `InvalidOperationException`.
This scenario is tested by `Context_Create_InvalidLogFilePath_ThrowsInvalidOperationException`.

**Context_WriteLine_NotSilent_WritesToConsole**: Verifies that a non-silent context writes to
standard output when `WriteLine` is called.
This scenario is tested by `Context_WriteLine_NotSilent_WritesToConsole`.

**Context_WriteLine_Silent_DoesNotWriteToConsole**: Verifies that a silent context does not write
to standard output when `WriteLine` is called.
This scenario is tested by `Context_WriteLine_Silent_DoesNotWriteToConsole`.

**Context_WriteLine_WithLogFile_WritesToLogFile**: Verifies that a context with a log file routes
`WriteLine` output to the log file.
This scenario is tested by `Context_WriteLine_WithLogFile_WritesToLogFile`.

**Context_WriteError_NotSilent_WritesToConsole**: Verifies that a non-silent context writes to
standard error when `WriteError` is called.
This scenario is tested by `Context_WriteError_NotSilent_WritesToConsole`.

**Context_WriteError_Silent_DoesNotWriteToConsole**: Verifies that a silent context does not write
to standard error when `WriteError` is called.
This scenario is tested by `Context_WriteError_Silent_DoesNotWriteToConsole`.

**Context_WriteError_WithLogFile_WritesToLogFile**: Verifies that a context with a log file routes
`WriteError` output to the log file.
This scenario is tested by `Context_WriteError_WithLogFile_WritesToLogFile`.

**Context_WriteError_SetsExitCodeToOne**: Verifies that calling `WriteError` on a context sets
`ExitCode` to 1. This scenario is tested by `Context_WriteError_SetsExitCodeToOne`.

**Context_ExitCode_NoErrors_RemainsZero**: Verifies that a context with no errors written retains
`ExitCode` of 0. This scenario is tested by `Context_ExitCode_NoErrors_RemainsZero`.

**Context_Dispose_ClosesLogFileProperly**: Verifies that disposing a context with an open log file
closes the file without error.
This scenario is tested by `Context_Dispose_ClosesLogFileProperly`.
