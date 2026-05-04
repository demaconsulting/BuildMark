# ConfigurationIssue

## Verification Approach

`ConfigurationIssue` is a data class with no dedicated test class. It is verified
indirectly through `ProgramTests.cs` via the `ConfigurationLoadResult` pipeline.
When configuration parsing produces issues, they are stored as `ConfigurationIssue`
instances and reported to the context. No dedicated test exercises this path in
isolation; it is covered by integration paths where configuration errors surface.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios (Integration)

### Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero

**Scenario**: `ConfigurationLoadResult` contains no `ConfigurationIssue` entries
when no config file is present; `ReportTo` is a no-op.

**Expected**: No issues reported; exit code is 0.

**Requirement coverage**: `BuildMark-Configuration-ConfigurationIssue`

## Requirements Coverage

- **BuildMark-Configuration-ConfigurationIssue**:
  Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero
