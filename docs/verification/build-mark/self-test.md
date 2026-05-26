## SelfTest

### Verification Approach

The SelfTest subsystem is verified with dedicated subsystem tests in `SelfTestTests.cs`. Tests
invoke `Validation.Run` through a `Context` constructed with controlled argument arrays, then
inspect created results files and log output. `MockRepoConnector` is used as the connector factory
so that no real repository access is needed. Unit-level tests for `Validation` are in
`ValidationTests.cs`.

### Test Environment

Tests create temporary directories and results files (`.trx`, `.xml`) through `TemporaryDirectory`,
which creates a unique `tmp-*` subdirectory under the current working directory. Write access to
the current working directory is required. No network access or external API calls are made;
`MockRepoConnector` provides all repository data.

### Acceptance Criteria

- All tests in `SelfTestTests.cs` pass with zero failures.
- All `BuildMark-SelfTest-*` requirements have at least one test in the subsystem test file.

### Test Scenarios

**SelfTest_Validation_WithTrxFile_WritesResults**: Verifies that running the SelfTest subsystem
with `--validate --results <tmp>.trx --silent` creates a `.trx` file whose content contains
`TestRun` and `BuildMark Self-Validation`, confirming TRX output at subsystem level.
This scenario is tested by `SelfTest_Validation_WithTrxFile_WritesResults`.

**SelfTest_Validation_WithXmlFile_WritesResults**: Verifies that running with
`--validate --results <tmp>.xml --silent` creates an `.xml` file whose content contains
`testsuites` and `BuildMark Self-Validation`, confirming JUnit XML output at subsystem level.
This scenario is tested by `SelfTest_Validation_WithXmlFile_WritesResults`.

**SelfTest_ResultsOutput_WithTrxFile_CreatesFile**: Verifies that the results file is created
with non-zero size when a `.trx` path is specified, confirming file I/O at subsystem level.
This scenario is tested by `SelfTest_ResultsOutput_WithTrxFile_CreatesFile`.

**SelfTest_ResultsOutput_WithXmlFile_CreatesFile**: Verifies that the results file is created
with non-zero size when an `.xml` path is specified.
This scenario is tested by `SelfTest_ResultsOutput_WithXmlFile_CreatesFile`.

**SelfTest_Qualification_WithoutResultsFile_Succeeds**: Verifies that when no `--results` argument
is supplied, validation completes without error, the log file is created, and its content contains
`BuildMark Self-validation` and `Total Tests:`.
This scenario is tested by `SelfTest_Qualification_WithoutResultsFile_Succeeds`.
