# PathHelpers

## Verification Approach

`PathHelpers` is a pure utility class with no dedicated test class. It is verified
indirectly through CLI and program tests that exercise path-related flag handling
(`--log`, `--report`, `--results`) and through the overall build pipeline where
paths are resolved during document generation.

No direct unit tests exist for `PathHelpers` because the class provides straightforward
path combination logic with no branching that requires isolated testing. Its behavior
is validated through the integration tests that consume it.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

## Test Scenarios (Integration)

### Cli_LogFlag_CreatesLogFile

**Scenario**: `Context` is created with a log file path; `PathHelpers` is used to
resolve the file path.

**Expected**: Log file is created at the resolved path.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

### Cli_ReportFlags_SetProperties

**Scenario**: `Context` is created with `--report` flag; path is processed by the
context/utilities layer.

**Expected**: `ReportFile` property contains the resolved path.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

## Requirements Coverage

- **BuildMark-Utilities-PathHelpers**: Cli_LogFlag_CreatesLogFile,
  Cli_ReportFlags_SetProperties
