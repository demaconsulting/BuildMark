# ConfigurationLoadResult

## Verification Approach

`ConfigurationLoadResult` is a data class with no dedicated test class. It is verified
indirectly through `ProgramTests.cs`, where the result of `BuildMarkConfigReader.ReadAsync`
is used to report issues and extract the active configuration. Tests that exercise
the lint path confirm that `ConfigurationLoadResult` is handled correctly when no
issues are present.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios (Integration)

### Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero

**Scenario**: `ConfigurationLoadResult` is returned from `BuildMarkConfigReader` with
no issues when no configuration file is present.

**Expected**: `result.ReportTo(context)` completes without errors; exit code is 0.

**Requirement coverage**: `BuildMark-Configuration-ConfigurationLoadResult`

## Requirements Coverage

- **BuildMark-Configuration-ConfigurationLoadResult**:
  Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero
