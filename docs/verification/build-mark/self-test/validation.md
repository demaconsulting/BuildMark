### Validation

#### Verification Approach

`Validation` is verified with dedicated unit tests in `ValidationTests.cs`. Tests construct a
`Context` with controlled arguments, call `Validation.Run`, and assert on the created results
files, console error output, and exit code. `MockRepoConnector` is provided as the connector
factory so the self-check runs without network access; no further mocking is required.

#### Test Environment

N/A - standard test environment. `ValidationTests.cs` runs within the standard `dotnet test` host.
Tests create temporary result files using `TemporaryDirectory`; write access to the current working
directory is required. No network access or external API calls are made.

#### Acceptance Criteria

- All tests in `ValidationTests.cs` pass with zero failures.
- TRX output, JUnit XML output, unsupported-format error, invalid-path error, and
  no-results-file paths are all covered.

#### Test Scenarios

**Validation_Run_WithTrxResultsFile_WritesTrxFile**: Verifies that `Validation.Run` with
`--validate --results <tmp>/results.trx` creates a `.trx` file whose content contains `TestRun`
and `BuildMark Self-Validation`, confirming TRX results output.
This scenario is tested by `Validation_Run_WithTrxResultsFile_WritesTrxFile`.

**Validation_Run_WithXmlResultsFile_WritesJUnitFile**: Verifies that `Validation.Run` with
`--validate --results <tmp>/results.xml` creates an `.xml` file whose content contains
`testsuites` and `BuildMark Self-Validation`, confirming JUnit XML results output.
This scenario is tested by `Validation_Run_WithXmlResultsFile_WritesJUnitFile`.

**Validation_Run_WithUnsupportedResultsFileExtension_ShowsError**: Verifies that supplying a
`.json` results file path causes an error message containing "Unsupported results file format"
on stderr and sets the exit code to 1.
This scenario is tested by `Validation_Run_WithUnsupportedResultsFileExtension_ShowsError`.

**Validation_Run_WithInvalidResultsFilePath_ShowsError**: Verifies that supplying an
inaccessible results file path causes an error message containing "Failed to write results file"
on stderr and sets the exit code to 1.
This scenario is tested by `Validation_Run_WithInvalidResultsFilePath_ShowsError`.

**Validation_Run_WithoutResultsFile_CompletesSuccessfully**: Verifies that when no `--results`
argument is supplied, validation completes without error and the exit code is 0.
This scenario is tested by `Validation_Run_WithoutResultsFile_CompletesSuccessfully`.
