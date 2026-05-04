# Validation

## Verification Approach

`Validation` is verified with dedicated unit tests in `ValidationTests.cs`. Tests construct a
`Context` with controlled arguments, call `Validation.Run`, and assert on the created results
files, console error output, and exit code. `MockRepoConnector` is provided as the connector
factory; no further mocking is required.

## Dependencies

| Mock / Stub         | Reason                                                                |
| ------------------- | --------------------------------------------------------------------- |
| `MockRepoConnector` | Supplies connector factory so self-check runs without network access. |

## Test Scenarios

### Validation_Run_WithTrxResultsFile_WritesTrxFile

**Scenario**: `Validation.Run` is called with `--validate --results <tmp>/results.trx`; console
output is captured.

**Expected**: `results.trx` is created; its content contains `TestRun` and
`BuildMark Self-Validation`.

**Requirement coverage**: `BuildMark-Validation-Run`, `BuildMark-Validation-TrxOutput`.

### Validation_Run_WithXmlResultsFile_WritesJUnitFile

**Scenario**: `Validation.Run` is called with `--validate --results <tmp>/results.xml`; console
output is captured.

**Expected**: `results.xml` is created; its content contains `testsuites` and
`BuildMark Self-Validation`.

**Requirement coverage**: `BuildMark-Validation-Run`, `BuildMark-Validation-JUnitOutput`.

### Validation_Run_WithUnsupportedResultsFileExtension_ShowsError

**Scenario**: `Validation.Run` is called with `--validate --results <tmp>/results.json`
(unsupported extension); console error is captured.

**Expected**: Console error contains `"Unsupported results file format"`; exit code is 1.

**Requirement coverage**: `BuildMark-Validation-TrxOutput`, `BuildMark-Validation-JUnitOutput`.

### Validation_Run_WithInvalidResultsFilePath_ShowsError

**Scenario**: `Validation.Run` is called with
`--validate --results /invalid_path_that_does_not_exist/results.trx`; console error is captured.

**Expected**: Console error contains `"Failed to write results file"`; exit code is 1.

**Requirement coverage**: `BuildMark-Validation-TrxOutput`.

### Validation_Run_WithoutResultsFile_CompletesSuccessfully

**Scenario**: `Validation.Run` is called with `--validate --silent`; no `--results` argument is
supplied.

**Expected**: Validation completes without error; exit code is 0.

**Requirement coverage**: `BuildMark-Validation-Run`.

## Requirements Coverage

- **`BuildMark-Validation-Run`**:
  - Validation_Run_WithTrxResultsFile_WritesTrxFile
  - Validation_Run_WithXmlResultsFile_WritesJUnitFile
  - Validation_Run_WithoutResultsFile_CompletesSuccessfully
- **`BuildMark-Validation-TrxOutput`**:
  - Validation_Run_WithTrxResultsFile_WritesTrxFile
  - Validation_Run_WithUnsupportedResultsFileExtension_ShowsError
  - Validation_Run_WithInvalidResultsFilePath_ShowsError
- **`BuildMark-Validation-JUnitOutput`**:
  - Validation_Run_WithXmlResultsFile_WritesJUnitFile
  - Validation_Run_WithUnsupportedResultsFileExtension_ShowsError
