# SelfTest

## Verification Approach

The SelfTest subsystem is verified with dedicated subsystem tests in `SelfTestTests.cs`. Tests
invoke `Validation.Run` through a `Context` constructed with controlled argument arrays, then
inspect created results files and log output. `MockRepoConnector` is used so that no real
repository access is needed.

## Dependencies

| Mock / Stub | Reason |
| --- | --- |
| `MockRepoConnector` | Provides a real connector factory that does not require external network access. |

## Test Scenarios

### SelfTest_Validation_WithTrxFile_WritesResults

**Scenario**: `Validation.Run` is called with `--validate --results <tmp>.trx --silent`; the
results file path ends in `.trx`.

**Expected**: A `.trx` file is created; its content contains `TestRun` and
`BuildMark Self-Validation`.

**Requirement coverage**: `BuildMark-SelfTest-Qualification`.

### SelfTest_Validation_WithXmlFile_WritesResults

**Scenario**: `Validation.Run` is called with `--validate --results <tmp>.xml --silent`; the
results file path ends in `.xml`.

**Expected**: An `.xml` file is created; its content contains `testsuites` and
`BuildMark Self-Validation`.

**Requirement coverage**: `BuildMark-SelfTest-Qualification`.

### SelfTest_ResultsOutput_WithTrxFile_CreatesFile

**Scenario**: `Validation.Run` is called with `--validate --results output.trx --silent`.

**Expected**: The `.trx` results file is created and has a non-zero file size.

**Requirement coverage**: `BuildMark-SelfTest-ResultsOutput`.

### SelfTest_ResultsOutput_WithXmlFile_CreatesFile

**Scenario**: `Validation.Run` is called with `--validate --results output.xml --silent`.

**Expected**: The `.xml` results file is created and has a non-zero file size.

**Requirement coverage**: `BuildMark-SelfTest-ResultsOutput`.

### SelfTest_Qualification_WithoutResultsFile_Succeeds

**Scenario**: `Validation.Run` is called with `--validate --log <tmp>/validation.log --silent`; no
`--results` argument is supplied.

**Expected**: Validation completes without error; the log file is created; its content contains
`BuildMark Self-validation` and `Total Tests:`.

**Requirement coverage**: `BuildMark-SelfTest-Qualification`.

## Requirements Coverage

- **`BuildMark-SelfTest-Qualification`**:
  - SelfTest_Validation_WithTrxFile_WritesResults
  - SelfTest_Validation_WithXmlFile_WritesResults
  - SelfTest_Qualification_WithoutResultsFile_Succeeds
- **`BuildMark-SelfTest-ResultsOutput`**:
  - SelfTest_ResultsOutput_WithTrxFile_CreatesFile
  - SelfTest_ResultsOutput_WithXmlFile_CreatesFile
